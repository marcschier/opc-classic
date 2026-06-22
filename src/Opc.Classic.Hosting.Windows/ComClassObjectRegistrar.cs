// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hosting.Windows;

/// <summary>
/// Registers a managed OPC server EXE with the Windows COM Service Control Manager
/// (SCM) by exposing a minimal <c>IClassFactory</c> via <c>ole32!CoRegisterClassObject</c>.
/// </summary>
/// <remarks>
/// <para>
/// COM SCM activates an out-of-process server by reading <c>HKCR\CLSID\{x}\LocalServer32</c>,
/// launching the EXE with the <c>-Embedding</c> command-line switch, and expecting the
/// EXE to call <c>CoRegisterClassObject</c> within a short window (typically a few seconds).
/// If the EXE does not register a class object SCM will report <c>CO_E_SERVER_EXEC_FAILURE</c>
/// to the client.
/// </para>
/// <para>
/// Each <see cref="RegisterClassObject(Guid, Func{Guid, IntPtr}?, bool)"/> call allocates a
/// fresh <c>IClassFactory</c> instance whose <c>CreateInstance</c> delegates to the
/// supplied <see cref="Func{T, TResult}"/>; the callback receives the requested IID
/// and returns a CCW pointer (or <see cref="IntPtr.Zero"/> for
/// <see cref="E_NOINTERFACE"/>). The previous parameterless overload remains for
/// callers that only need to satisfy SCM's "class object registered" expectation
/// without actually dispatching activations (used by the earlier smoke harnesses).
/// </para>
/// <para>
/// All COM interop here is written in raw <c>unsafe</c> code that is compatible with
/// NativeAOT trimming (<c>IsAotCompatible=true</c>). No reflection-based
/// <c>[ComVisible]</c> or runtime CCW marshalling is used; the vtable is built explicitly
/// from <c>[UnmanagedCallersOnly]</c> static methods.
/// </para>
/// <para>
/// CCW instances are never freed (leak-at-process-exit) because Windows COM expects
/// the IUnknown pointer it received via <c>CoRegisterClassObject</c> to remain valid
/// for the lifetime of the registration; this matches the canonical CCW pattern and
/// avoids use-after-free races on Release.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class ComClassObjectRegistrar
{
    private const int S_OK = 0;
    private static readonly int E_NOINTERFACE = global::Opc.Classic.OpcResultId.NoInterface.Code;
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private const int CLSCTX_LOCAL_SERVER = 0x4;
    private const int REGCLS_MULTIPLEUSE = 1;
    private const int REGCLS_SUSPENDED = 4;
    private const uint COINIT_APARTMENTTHREADED = 0x2;
    private const uint COINIT_MULTITHREADED = 0x0;

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid IID_IClassFactory = Guid.Parse("00000001-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, FactoryEntry> s_factories = new();

    /// <summary>
    /// Initializes the calling thread's COM apartment as STA. Idempotent: subsequent
    /// calls after the first successful init are no-ops at the OS level.
    /// </summary>
    /// <remarks>
    /// STA threads require a Win32 message pump to dispatch incoming COM calls.
    /// If the caller's main thread doesn't run a <c>GetMessage</c>/<c>DispatchMessage</c>
    /// loop (e.g. when using <c>Host.RunAsync</c> from
    /// Microsoft.Extensions.Hosting which only runs the .NET hosted-service loop),
    /// incoming activation requests from SCM will queue forever and the client
    /// observes <c>CO_E_SERVER_EXEC_FAILURE</c> after the SCM timeout. For most
    /// OPC server scenarios <see cref="InitializeMultithreaded" /> is the better
    /// choice — MTA dispatches COM calls on a pool thread without requiring a
    /// message pump.
    /// </remarks>
    public static void InitializeApartmentThreaded()
    {
        int hr = CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);
        // S_FALSE (1) means "apartment already initialized"; both are acceptable.
        if (hr < 0)
        {
            throw new InvalidOperationException(
                $"CoInitializeEx failed with HRESULT 0x{hr:X8}.");
        }
    }

    /// <summary>
    /// Initializes the calling thread's COM apartment as MTA. Idempotent
    /// at the OS level. MTA is the preferred apartment for non-UI COM
    /// servers because incoming activation requests from SCM dispatch on
    /// a pool thread without requiring a Win32 <c>GetMessage</c> loop.
    /// </summary>
    public static void InitializeMultithreaded()
    {
        int hr = CoInitializeEx(IntPtr.Zero, COINIT_MULTITHREADED);
        if (hr < 0)
        {
            throw new InvalidOperationException(
                $"CoInitializeEx(MULTITHREADED) failed with HRESULT 0x{hr:X8}.");
        }
    }

    /// <summary>
    /// Tears down the COM apartment initialized by <see cref="InitializeApartmentThreaded" />.
    /// </summary>
    public static void Uninitialize() => CoUninitialize();

    /// <summary>
    /// Registers a stub <c>IClassFactory</c> whose <c>CreateInstance</c> always
    /// returns <see cref="E_NOINTERFACE"/>. Useful when only SCM's "class object
    /// registered" expectation needs to be satisfied (smoke / registration-plumbing
    /// validation).
    /// </summary>
    public static uint RegisterClassObject(Guid clsid, bool suspended = true) =>
        RegisterClassObject(clsid, createInstanceCallback: null, suspended);

    /// <summary>
    /// Registers a class factory whose <c>CreateInstance</c> dispatches to
    /// <paramref name="createInstanceCallback"/>. The callback receives the
    /// client-requested IID and must return a CCW <see cref="IntPtr"/> with
    /// ref count = 1 (the caller's reference) or <see cref="IntPtr.Zero"/>
    /// for <see cref="E_NOINTERFACE"/>.
    /// </summary>
    /// <param name="clsid">The CLSID the class factory should be advertised for.</param>
    /// <param name="createInstanceCallback">
    /// Per-call factory invoked at activation time. <see langword="null"/> falls
    /// back to <see cref="E_NOINTERFACE"/>.
    /// </param>
    /// <param name="suspended">
    /// When <see langword="true" /> (recommended for multi-class servers), register with
    /// <c>REGCLS_SUSPENDED</c> so SCM does not dispatch activations until
    /// <see cref="ResumeClassObjects" /> is called.
    /// </param>
    public static uint RegisterClassObject(
        Guid clsid,
        Func<Guid, IntPtr>? createInstanceCallback,
        bool suspended = true)
    {
        IntPtr factoryInstance = AllocateFactoryInstance();
        s_factories[factoryInstance] = new FactoryEntry(clsid, createInstanceCallback);

        int regcls = REGCLS_MULTIPLEUSE | (suspended ? REGCLS_SUSPENDED : 0);
        int hr = CoRegisterClassObject(
            in clsid,
            factoryInstance,
            CLSCTX_LOCAL_SERVER,
            regcls,
            out uint cookie);

        if (hr < 0)
        {
            throw new InvalidOperationException(
                $"CoRegisterClassObject failed for CLSID {clsid:B} with HRESULT 0x{hr:X8}.");
        }

        return cookie;
    }

    /// <summary>
    /// Resumes activation dispatch after all class objects have been registered with
    /// <c>REGCLS_SUSPENDED</c>.
    /// </summary>
    public static void ResumeClassObjects()
    {
        int hr = CoResumeClassObjects();
        if (hr < 0)
        {
            throw new InvalidOperationException(
                $"CoResumeClassObjects failed with HRESULT 0x{hr:X8}.");
        }
    }

    /// <summary>
    /// Revokes a previously registered class object so SCM stops routing activations
    /// to this process.
    /// </summary>
    public static void RevokeClassObject(uint cookie)
    {
        int hr = CoRevokeClassObject(cookie);
        if (hr < 0)
        {
            throw new InvalidOperationException(
                $"CoRevokeClassObject failed with HRESULT 0x{hr:X8}.");
        }
    }

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized native struct with explicit byte count.")]
    private static IntPtr AllocateFactoryInstance()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)&CreateInstance;
        vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&LockServer;

        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        return (IntPtr)instance;
    }

    [UnmanagedCallersOnly]
    private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
    {
        if (ppv == null)
        {
            return E_INVALIDARG;
        }

        if (riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_INVALIDARG;
        }

        Guid iid = *riid;
        if (iid == IID_IUnknown || iid == IID_IClassFactory)
        {
            *ppv = pThis;
            return S_OK;
        }

        *ppv = IntPtr.Zero;
        return E_NOINTERFACE;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        if (!s_factories.TryGetValue(pThis, out FactoryEntry? entry))
        {
            return 1;
        }
        return (uint)Interlocked.Increment(ref entry.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_factories.TryGetValue(pThis, out FactoryEntry? entry))
        {
            return 0;
        }

        long next = Interlocked.Decrement(ref entry.RefCount);
        // Static factory instances are leaked at process exit (matches the
        // canonical CCW pattern). SCM holds references over the registration
        // lifetime; freeing here would race with QueryInterface calls in flight.
        return next < 0 ? 0 : (uint)next;
    }

    [UnmanagedCallersOnly]
    private static int CreateInstance(IntPtr pThis, IntPtr pUnkOuter, Guid* riid, IntPtr* ppv)
    {
        _ = pUnkOuter;
        if (ppv == null)
        {
            return E_INVALIDARG;
        }
        *ppv = IntPtr.Zero;

        if (riid == null)
        {
            return E_INVALIDARG;
        }

        if (!s_factories.TryGetValue(pThis, out FactoryEntry? entry)
            || entry.CreateInstanceCallback is null)
        {
            return E_NOINTERFACE;
        }

        IntPtr ccw;
        try
        {
            ccw = entry.CreateInstanceCallback(*riid);
        }
#pragma warning disable CA1031 // Crossing the unmanaged COM boundary; any managed exception here would escape into ole32 and crash the process.
        catch (Exception)
#pragma warning restore CA1031
        {
            return E_NOINTERFACE;
        }

        if (ccw == IntPtr.Zero)
        {
            return E_NOINTERFACE;
        }

        *ppv = ccw;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int LockServer(IntPtr pThis, int fLock)
    {
        _ = pThis;
        _ = fLock;
        return S_OK;
    }

    // ----- ole32.dll P/Invoke -----

    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int CoInitializeEx(IntPtr reserved, uint coInit);

    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void CoUninitialize();

    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int CoRegisterClassObject(
        in Guid rclsid,
        IntPtr pUnk,
        int clsContext,
        int flags,
        out uint cookie);

    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int CoRevokeClassObject(uint cookie);

    [DllImport("ole32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int CoResumeClassObjects();

    private sealed class FactoryEntry
    {
        public FactoryEntry(Guid clsid, Func<Guid, IntPtr>? createInstanceCallback)
        {
            Clsid = clsid;
            CreateInstanceCallback = createInstanceCallback;
        }

        public Guid Clsid { get; }
        public Func<Guid, IntPtr>? CreateInstanceCallback { get; }

        public long RefCount;
    }
}
