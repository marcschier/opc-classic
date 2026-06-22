// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hosting.Windows.Tests;

[SupportedOSPlatform("windows")]
internal static unsafe class ShutdownConnectionPointCcwTestHelpers
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_POINTER = unchecked((int)0x80004003);
    private const int SinkVtableSlotCount = 4;

    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, SinkStubSession> s_sinkStubs = new();

    internal static IntPtr FindShutdownConnectionPoint(IntPtr unknown)
    {
        IntPtr cpc = QueryInterface(unknown, OpcGuids.IID_IConnectionPointContainer);
        FindConnectionPointDelegate find = GetMethod<FindConnectionPointDelegate>(cpc, 4);
        Guid iid = OpcGuids.IID_IOPCShutdown;
        int hr = find(cpc, ref iid, out IntPtr connectionPoint);
        if (hr != S_OK)
        {
            throw new InvalidOperationException($"FindConnectionPoint failed with 0x{hr:X8}.");
        }
        return connectionPoint;
    }

    internal static IntPtr CreateSinkStub()
    {
        IntPtr* vtable = AllocateSinkVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_sinkStubs[instance] = new SinkStubSession(vtable);
        return instance;
    }

    internal static void DestroySinkStub(IntPtr sink)
    {
        if (!s_sinkStubs.TryRemove(sink, out SinkStubSession? session))
        {
            return;
        }
        NativeMemory.Free((void*)sink);
        NativeMemory.Free(session.Vtable);
    }

    internal static string[] GetReasons(IntPtr sink)
    {
        if (!s_sinkStubs.TryGetValue(sink, out SinkStubSession? session))
        {
            return [];
        }
        lock (session.Gate)
        {
            return session.Reasons.ToArray();
        }
    }

    internal static (int Hr, uint Cookie) Advise(IntPtr connectionPoint, IntPtr sink)
    {
        AdviseDelegate advise = GetMethod<AdviseDelegate>(connectionPoint, 5);
        int hr = advise(connectionPoint, sink, out uint cookie);
        return (hr, cookie);
    }

    internal static int Unadvise(IntPtr connectionPoint, uint cookie)
    {
        UnadviseDelegate unadvise = GetMethod<UnadviseDelegate>(connectionPoint, 6);
        return unadvise(connectionPoint, cookie);
    }

    internal static IntPtr QueryInterface(IntPtr unknown, Guid iid)
    {
        QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(unknown, 0);
        int hr = qi(unknown, ref iid, out IntPtr returned);
        if (hr != S_OK)
        {
            throw new InvalidOperationException($"QueryInterface failed with 0x{hr:X8}.");
        }
        return returned;
    }

    private static T GetMethod<T>(IntPtr tearoff, int slot)
        where T : Delegate
    {
        IntPtr vtable = Marshal.ReadIntPtr(tearoff);
        IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(method);
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSinkVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(SinkVtableSlotCount * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&SinkQueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&SinkAddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&SinkRelease;
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&SinkShutdownRequest;
        return vtable;
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInstance(IntPtr* vtable)
    {
        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        return (IntPtr)instance;
    }

    [UnmanagedCallersOnly]
    private static int SinkQueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
    {
        if (ppv == null)
        {
            return E_POINTER;
        }
        if (!s_sinkStubs.TryGetValue(pThis, out SinkStubSession? session) || riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }
        if (*riid == s_iidUnknown || *riid == OpcGuids.IID_IOPCShutdown)
        {
            *ppv = pThis;
            Interlocked.Increment(ref session.RefCount);
            return S_OK;
        }
        *ppv = IntPtr.Zero;
        return E_NOINTERFACE;
    }

    [UnmanagedCallersOnly]
    private static uint SinkAddRef(IntPtr pThis) =>
        s_sinkStubs.TryGetValue(pThis, out SinkStubSession? session)
            ? (uint)Interlocked.Increment(ref session.RefCount)
            : 1;

    [UnmanagedCallersOnly]
    private static uint SinkRelease(IntPtr pThis) =>
        s_sinkStubs.TryGetValue(pThis, out SinkStubSession? session)
            ? (uint)Interlocked.Decrement(ref session.RefCount)
            : 0;

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031", Justification = "Cross-unmanaged-boundary catch.")]
    private static int SinkShutdownRequest(IntPtr pThis, IntPtr reason)
    {
        try
        {
            if (!s_sinkStubs.TryGetValue(pThis, out SinkStubSession? session))
            {
                return E_NOINTERFACE;
            }
            lock (session.Gate)
            {
                session.Reasons.Add(Marshal.PtrToStringUni(reason) ?? string.Empty);
            }
            return S_OK;
        }
        catch
        {
            return unchecked((int)0x80004005);
        }
    }

    private sealed class SinkStubSession
    {
        public SinkStubSession(IntPtr* vtable) => Vtable = vtable;

        public IntPtr* Vtable { get; }
        public long RefCount = 1;
        public object Gate { get; } = new();
        public List<string> Reasons { get; } = [];
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int FindConnectionPointDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppCp);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AdviseDelegate(IntPtr pThis, IntPtr pUnk, out uint pdwCookie);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int UnadviseDelegate(IntPtr pThis, uint dwCookie);
}
