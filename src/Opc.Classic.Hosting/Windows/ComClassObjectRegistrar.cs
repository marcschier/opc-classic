//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

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
/// The class factory exposed by this type implements a STUB form of <c>IClassFactory</c>:
/// <c>CreateInstance</c> always returns <c>E_NOINTERFACE</c>. The intent for <c>ctt-2</c>
/// is to satisfy SCM's "class object registered" expectation so that the EXE is launched
/// and the registration plumbing can be smoke-tested, while leaving the actual
/// <c>IOPCServer</c> dispatch wire-up to a follow-up todo (the managed DCOM listener is
/// currently scaffold-grade).
/// </para>
/// <para>
/// All COM interop here is written in raw <c>unsafe</c> code that is compatible with
/// NativeAOT trimming (<c>IsAotCompatible=true</c>). No reflection-based
/// <c>[ComVisible]</c> or runtime CCW marshalling is used; the vtable is built explicitly
/// from <c>[UnmanagedCallersOnly]</c> static methods.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class ComClassObjectRegistrar
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private const int CLSCTX_LOCAL_SERVER = 0x4;
    private const int REGCLS_MULTIPLEUSE = 1;
    private const int REGCLS_SUSPENDED = 4;
    private const uint COINIT_APARTMENTTHREADED = 0x2;

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid IID_IClassFactory = Guid.Parse("00000001-0000-0000-C000-000000000046");

    // Static factory instance allocated in unmanaged memory so its address is stable
    // across managed GC cycles. SCM holds the IUnknown* across the process lifetime.
    private static IntPtr s_factoryInstance;
    private static long s_refCount;

    /// <summary>
    /// Initializes the calling thread's COM apartment as STA. Idempotent: subsequent
    /// calls after the first successful init are no-ops at the OS level.
    /// </summary>
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
    /// Tears down the COM apartment initialized by <see cref="InitializeApartmentThreaded" />.
    /// </summary>
    public static void Uninitialize() => CoUninitialize();

    /// <summary>
    /// Registers the IUnknown-stub <c>IClassFactory</c> with the given CLSID against
    /// the Windows COM SCM. Returns the registration cookie to pass to
    /// <see cref="RevokeClassObject(uint)" /> on shutdown.
    /// </summary>
    /// <param name="clsid">The CLSID the class factory should be advertised for.</param>
    /// <param name="suspended">
    /// When <see langword="true" /> (recommended for multi-class servers), register with
    /// <c>REGCLS_SUSPENDED</c> so SCM does not dispatch activations until
    /// <see cref="ResumeClassObjects" /> is called.
    /// </param>
    public static uint RegisterClassObject(Guid clsid, bool suspended = true)
    {
        EnsureFactoryAllocated();

        int regcls = REGCLS_MULTIPLEUSE | (suspended ? REGCLS_SUSPENDED : 0);
        int hr = CoRegisterClassObject(
            in clsid,
            s_factoryInstance,
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
    /// <see cref="REGCLS_SUSPENDED" />.
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
    private static void EnsureFactoryAllocated()
    {
        if (s_factoryInstance != IntPtr.Zero)
        {
            return;
        }

        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)&CreateInstance;
        vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&LockServer;

        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;

        IntPtr previous = Interlocked.CompareExchange(ref s_factoryInstance, (IntPtr)instance, IntPtr.Zero);
        if (previous != IntPtr.Zero)
        {
            // Lost the race; another thread already allocated. Free our copy.
            NativeMemory.Free(vtable);
            NativeMemory.Free(instance);
        }
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
            Interlocked.Increment(ref s_refCount);
            return S_OK;
        }

        *ppv = IntPtr.Zero;
        return E_NOINTERFACE;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        _ = pThis;
        long next = Interlocked.Increment(ref s_refCount);
        return (uint)next;
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        _ = pThis;
        long next = Interlocked.Decrement(ref s_refCount);
        // Static singleton: never actually free, even at ref count 0. SCM revokes
        // via CoRevokeClassObject which calls Release; we keep the instance alive
        // for re-registration scenarios.
        return next < 0 ? 0 : (uint)next;
    }

    [UnmanagedCallersOnly]
    private static int CreateInstance(IntPtr pThis, IntPtr pUnkOuter, Guid* riid, IntPtr* ppv)
    {
        _ = pThis;
        _ = pUnkOuter;
        _ = riid;
        if (ppv != null)
        {
            *ppv = IntPtr.Zero;
        }
        // STUB: IOPCServer dispatch is not yet wired up. SCM accepts the activation
        // (the class object IS registered) but the client immediately sees
        // E_NOINTERFACE. Replaced by a real factory in a follow-up todo.
        return E_NOINTERFACE;
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
}
