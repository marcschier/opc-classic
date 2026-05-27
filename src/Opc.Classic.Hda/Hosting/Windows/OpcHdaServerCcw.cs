//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Windows COM-callable wrapper (CCW) over an <see cref="IOpcHdaServer" />.
/// Exposes separate tearoff vtables for <c>IUnknown</c>, <c>IOPCHDA_Server</c>,
/// <c>IOPCHDA_SyncRead</c>, and <c>IOPCHDA_AsyncRead</c>.
/// </summary>
/// <remarks>
/// Each supported interface is represented by a native tearoff whose first slot
/// points at its vtable. <c>QueryInterface(IID_IUnknown)</c> on any tearoff
/// returns the canonical identity pointer; all tearoffs share a single refcount
/// and managed <see cref="GCHandle" />. The simple-marshaling HDA server methods
/// have real bodies in <see cref="OpcHdaServerCcwMethods" />; read methods are
/// wired as vtable slots and return <c>E_NOTIMPL</c> until native VARIANT and
/// callback marshaling helpers are available.
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcHdaServerCcw
{
    internal const int S_OK = 0;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    internal static readonly ConcurrentDictionary<IntPtr, CcwSession> s_tearoffs = new();

    /// <summary>Builds a CCW and returns the requested interface tearoff with refcount = 1.</summary>
    public static IntPtr Create(IOpcHdaServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var handle = GCHandle.Alloc(server, GCHandleType.Normal);
        var session = new CcwSession(handle) { RefCount = 1 };
        InitializeTearoffs(session);
        RegisterTearoffs(session);
        return ResolveTearoff(session, requestedIid);
    }

    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown ||
        iid == IOPCHDA_Server.InterfaceId ||
        iid == IOPCHDA_SyncRead.InterfaceId ||
        iid == IOPCHDA_AsyncRead.InterfaceId;

    public static long GetReferenceCount(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? Interlocked.Read(ref session.RefCount)
            : -1L;

    public static IntPtr GetUnknownTearoff(IntPtr anyTearoff) =>
        s_tearoffs.TryGetValue(anyTearoff, out CcwSession? session)
            ? session.UnknownTearoff
            : IntPtr.Zero;

    internal static IOpcHdaServer? ResolveServer(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? session.ServerHandle.Target as IOpcHdaServer
            : null;

    private static void InitializeTearoffs(CcwSession session)
    {
        session.UnknownVtable = AllocateUnknownVtable();
        session.ServerVtable = AllocateServerVtable();
        session.SyncReadVtable = AllocateSyncReadVtable();
        session.AsyncReadVtable = AllocateAsyncReadVtable();

        session.UnknownTearoff = AllocateTearoff(session.UnknownVtable);
        session.ServerTearoff = AllocateTearoff(session.ServerVtable);
        session.SyncReadTearoff = AllocateTearoff(session.SyncReadVtable);
        session.AsyncReadTearoff = AllocateTearoff(session.AsyncReadVtable);
    }

    private static void RegisterTearoffs(CcwSession session)
    {
        s_tearoffs[session.UnknownTearoff] = session;
        s_tearoffs[session.ServerTearoff] = session;
        s_tearoffs[session.SyncReadTearoff] = session;
        s_tearoffs[session.AsyncReadTearoff] = session;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateUnknownVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(3 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateServerVtable()
    {
        // 3 IUnknown + 7 IOPCHDA_Server methods, including CreateBrowse at opnum 9.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(10 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, IntPtr*, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.GetItemAttributes;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.GetAggregates;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, IntPtr*, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcHdaServerCcwMethods.GetHistorianStatus;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.GetItemHandles;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.ReleaseItemHandles;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.ValidateItemIDs;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.CreateBrowse;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSyncReadVtable()
    {
        // 3 IUnknown + 5 IOPCHDA_SyncRead methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, uint, int, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncReadRaw;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, long, uint, IntPtr, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncReadProcessed;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncReadAtTime;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, uint, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncReadModified;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, uint, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncReadAttribute;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateAsyncReadVtable()
    {
        // 3 IUnknown + 8 IOPCHDA_AsyncRead methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(11 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, int, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncReadRaw;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, long, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncAdviseRaw;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, long, uint, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncReadProcessed;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, long, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncAdviseProcessed;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncReadAtTime;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncReadModified;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncReadAttribute;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaServerCcwMethods.AsyncCancel;
        return v;
    }

    private static void PopulateIUnknown(IntPtr* v)
    {
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateTearoff(IntPtr* vtable)
    {
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
        if (riid == null || !s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            *ppv = IntPtr.Zero;
            return riid == null ? E_INVALIDARG : E_NOINTERFACE;
        }

        IntPtr target = ResolveTearoff(session, *riid);
        if (target == IntPtr.Zero)
        {
            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        *ppv = target;
        Interlocked.Increment(ref session.RefCount);
        return S_OK;
    }

    private static IntPtr ResolveTearoff(CcwSession session, Guid iid)
    {
        if (iid == IID_IUnknown)
        {
            return session.UnknownTearoff;
        }
        if (iid == IOPCHDA_Server.InterfaceId)
        {
            return session.ServerTearoff;
        }
        if (iid == IOPCHDA_SyncRead.InterfaceId)
        {
            return session.SyncReadTearoff;
        }
        if (iid == IOPCHDA_AsyncRead.InterfaceId)
        {
            return session.AsyncReadTearoff;
        }
        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return 1;
        }
        return (uint)Interlocked.Increment(ref session.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return 0;
        }
        long next = Interlocked.Decrement(ref session.RefCount);
        if (next > 0)
        {
            return (uint)next;
        }
        DisposeSession(session);
        return 0;
    }

    private static void DisposeSession(CcwSession session)
    {
        if (Interlocked.Exchange(ref session.Disposed, 1) != 0)
        {
            return;
        }

        s_tearoffs.TryRemove(session.UnknownTearoff, out _);
        s_tearoffs.TryRemove(session.ServerTearoff, out _);
        s_tearoffs.TryRemove(session.SyncReadTearoff, out _);
        s_tearoffs.TryRemove(session.AsyncReadTearoff, out _);

        FreeNative(session.UnknownTearoff);
        FreeNative(session.ServerTearoff);
        FreeNative(session.SyncReadTearoff);
        FreeNative(session.AsyncReadTearoff);
        FreeNative(session.UnknownVtable);
        FreeNative(session.ServerVtable);
        FreeNative(session.SyncReadVtable);
        FreeNative(session.AsyncReadVtable);
        if (session.ServerHandle.IsAllocated)
        {
            session.ServerHandle.Free();
        }
    }

    private static void FreeNative(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            NativeMemory.Free((void*)ptr);
        }
    }

    private static void FreeNative(IntPtr* ptr)
    {
        if (ptr != null)
        {
            NativeMemory.Free(ptr);
        }
    }

    internal sealed class CcwSession
    {
        public CcwSession(GCHandle serverHandle)
        {
            ServerHandle = serverHandle;
        }

        public GCHandle ServerHandle { get; }

        public long RefCount;
        public int Disposed;
        public IntPtr UnknownTearoff;
        public IntPtr* UnknownVtable;
        public IntPtr ServerTearoff;
        public IntPtr* ServerVtable;
        public IntPtr SyncReadTearoff;
        public IntPtr* SyncReadVtable;
        public IntPtr AsyncReadTearoff;
        public IntPtr* AsyncReadVtable;
    }
}
