//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Windows COM-callable wrapper (CCW) over an <see cref="OpcDaGroup"/>
/// instance. Exposes native tearoffs for group state, item management,
/// synchronous I/O, asynchronous I/O, and connection-point sink binding so
/// SCM-activated DCOM clients can invoke group operations after
/// <c>IOPCServer::AddGroup</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Identity.</b> Each supported interface is a separate native "tearoff"
/// pointer. <c>QueryInterface(IID_IUnknown)</c> on any tearoff returns the
/// canonical IUnknown identity pointer (rule MS-DCOM §3.2.6). All tearoffs
/// share a single <see cref="CcwSession"/> holding the refcount and the
/// <see cref="GCHandle"/> on the managed group.
/// </para>
/// <para>
/// <b>Method coverage.</b> IOPCGroupStateMgt(2), IOPCItemMgt, selected
/// IOPCAsyncIO2/3 methods, and IConnectionPoint(Container) methods are
/// wired to real managed bodies. IOPCSyncIO(2) and value-bearing async write
/// methods deliberately return <c>E_NOTIMPL</c> for this MVP because full
/// <c>VARIANT</c>, <c>OPCITEMSTATE</c>, and <c>OPCITEMVQT</c> array marshaling
/// is a follow-up.
/// </para>
/// <para>
/// <b>SCM sinks.</b> Windows connection-point <c>Advise</c> stores
/// <see cref="OpcDataCallbackProxy"/> instances in <see cref="CcwSession.ScmSinks"/>.
/// This intentionally coexists with the managed <c>OpcDaGroup</c> DCOM-transport
/// subscription dictionary; a future sink abstraction can unify the two fan-out
/// paths without changing the COM vtable contract.
/// </para>
/// <para>
/// <b>Lifetime.</b> When the last <c>Release</c> drops the shared refcount to
/// zero, SCM sink proxies are disposed before all tearoff instances, vtables,
/// and the GCHandle are freed. The CCW is fully self-contained — no leak-at-exit.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcDaGroupCcw
{
    internal const int S_OK = 0;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);
    internal const int CONNECT_E_NOCONNECTION = unchecked((int)0x80040200);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    // Tearoff pointer -> session. Multiple tearoffs map to the same session.
    internal static readonly ConcurrentDictionary<IntPtr, CcwSession> s_tearoffs = new();

    /// <summary>
    /// Builds a CCW around <paramref name="group"/> and returns the IUnknown identity pointer with refcount = 1.
    /// </summary>
    public static IntPtr Create(OpcDaGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var handle = GCHandle.Alloc(group, GCHandleType.Normal);
        var session = new CcwSession(handle) { RefCount = 1 };
        AllocateSessionTearoffs(session);
        RegisterSessionTearoffs(session);
        return session.UnknownTearoff;
    }

    /// <summary>
    /// Test helper: returns the current refcount, or -1 if the pointer is not a known tearoff.
    /// </summary>
    public static long GetReferenceCount(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? Interlocked.Read(ref session.RefCount)
            : -1L;

    /// <summary>
    /// Test helper: looks up the canonical IUnknown tearoff for a CCW pointer (any tearoff of the same session).
    /// </summary>
    public static IntPtr GetUnknownTearoff(IntPtr anyTearoff) =>
        s_tearoffs.TryGetValue(anyTearoff, out CcwSession? session)
            ? session.UnknownTearoff
            : IntPtr.Zero;

    /// <summary>
    /// Test helper: returns the number of Windows SCM callback sinks held by the CCW session.
    /// </summary>
    public static int GetScmSinkCount(IntPtr anyTearoff) =>
        s_tearoffs.TryGetValue(anyTearoff, out CcwSession? session)
            ? session.ScmSinks.Count
            : -1;

    internal static OpcDaGroup? ResolveGroup(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? session.GroupHandle.Target as OpcDaGroup
            : null;

    internal static CcwSession? ResolveSession(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) ? session : null;

    internal static int ReturnTearoff(CcwSession session, IntPtr tearoff, IntPtr* ppv)
    {
        if (ppv == null || tearoff == IntPtr.Zero)
        {
            return E_INVALIDARG;
        }

        *ppv = tearoff;
        Interlocked.Increment(ref session.RefCount);
        return S_OK;
    }

    private static void AllocateSessionTearoffs(CcwSession session)
    {
        session.UnknownVtable = AllocateUnknownVtable();
        session.GroupStateMgtVtable = AllocateGroupStateMgt2Vtable();
        session.ItemMgtVtable = AllocateItemMgtVtable();
        session.SyncIoVtable = AllocateSyncIoVtable();
        session.SyncIo2Vtable = AllocateSyncIo2Vtable();
        session.AsyncIo2Vtable = AllocateAsyncIo2Vtable();
        session.AsyncIo3Vtable = AllocateAsyncIo3Vtable();
        session.ConnectionPointVtable = AllocateConnectionPointVtable();
        session.ConnectionPointContainerVtable = AllocateConnectionPointContainerVtable();

        session.UnknownTearoff = AllocateTearoff(session.UnknownVtable);
        session.GroupStateMgtTearoff = AllocateTearoff(session.GroupStateMgtVtable);
        session.ItemMgtTearoff = AllocateTearoff(session.ItemMgtVtable);
        session.SyncIoTearoff = AllocateTearoff(session.SyncIoVtable);
        session.SyncIo2Tearoff = AllocateTearoff(session.SyncIo2Vtable);
        session.AsyncIo2Tearoff = AllocateTearoff(session.AsyncIo2Vtable);
        session.AsyncIo3Tearoff = AllocateTearoff(session.AsyncIo3Vtable);
        session.ConnectionPointTearoff = AllocateTearoff(session.ConnectionPointVtable);
        session.ConnectionPointContainerTearoff = AllocateTearoff(session.ConnectionPointContainerVtable);
    }

    private static void RegisterSessionTearoffs(CcwSession session)
    {
        RegisterTearoff(session.UnknownTearoff, session);
        RegisterTearoff(session.GroupStateMgtTearoff, session);
        RegisterTearoff(session.ItemMgtTearoff, session);
        RegisterTearoff(session.SyncIoTearoff, session);
        RegisterTearoff(session.SyncIo2Tearoff, session);
        RegisterTearoff(session.AsyncIo2Tearoff, session);
        RegisterTearoff(session.AsyncIo3Tearoff, session);
        RegisterTearoff(session.ConnectionPointTearoff, session);
        RegisterTearoff(session.ConnectionPointContainerTearoff, session);
    }

    private static void RegisterTearoff(IntPtr tearoff, CcwSession session) => s_tearoffs[tearoff] = session;

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
    private static IntPtr* AllocateGroupStateMgt2Vtable()
    {
        // 3 IUnknown + 4 IOPCGroupStateMgt + 2 IOPCGroupStateMgt2 = 9 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(9 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.GetState;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.SetState;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.SetName;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)&OpcDaGroupCcwMethods.CloneGroup;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcDaGroupCcwMethods.SetKeepAlive;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.GetKeepAlive;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateItemMgtVtable()
    {
        // 3 IUnknown + 7 IOPCItemMgt = 10 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(10 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwMethods.AddItems;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwMethods.ValidateItems;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, int>)&OpcDaGroupCcwMethods.RemoveItems;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int, IntPtr*, int>)&OpcDaGroupCcwMethods.SetActiveState;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwMethods.SetClientHandles;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwMethods.SetDatatypes;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&OpcDaGroupCcwMethods.CreateEnumerator;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSyncIoVtable()
    {
        // 3 IUnknown + 2 IOPCSyncIO = 5 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwSyncIoMethods.Read;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwSyncIoMethods.Write;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSyncIo2Vtable()
    {
        // 3 IUnknown + 2 IOPCSyncIO + 2 IOPCSyncIO2 = 7 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwSyncIoMethods.Read;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwSyncIoMethods.Write;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, IntPtr*, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwSyncIoMethods.ReadMaxAge;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwSyncIoMethods.WriteVqt;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateAsyncIo2Vtable()
    {
        // 3 IUnknown + 6 IOPCAsyncIO2 + one compatibility GetConnectionPointContainer slot = 10 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(10 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        FillAsyncIo2Slots(v);
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcDaGroupCcwAsyncIoMethods.GetConnectionPointContainer;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateAsyncIo3Vtable()
    {
        // 3 IUnknown + IOPCAsyncIO2 slots + 3 IOPCAsyncIO3 slots + compatibility slot = 13 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(13 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        FillAsyncIo2Slots(v);
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint*, IntPtr*, int>)&OpcDaGroupCcwAsyncIoMethods.ReadMaxAge;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint*, IntPtr*, int>)&OpcDaGroupCcwAsyncIoMethods.WriteVqt;
        v[11] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, uint*, int>)&OpcDaGroupCcwAsyncIoMethods.RefreshMaxAge;
        v[12] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcDaGroupCcwAsyncIoMethods.GetConnectionPointContainer;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointVtable()
    {
        // 3 IUnknown + 5 IConnectionPoint = 8 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, int>)&OpcDaGroupCcwConnectionPointMethods.GetConnectionInterface;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcDaGroupCcwConnectionPointMethods.GetConnectionPointContainer;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint*, int>)&OpcDaGroupCcwConnectionPointMethods.Advise;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcDaGroupCcwConnectionPointMethods.Unadvise;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcDaGroupCcwConnectionPointMethods.EnumConnections;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointContainerVtable()
    {
        // 3 IUnknown + 2 IConnectionPointContainer = 5 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcDaGroupCcwConnectionPointMethods.EnumConnectionPoints;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&OpcDaGroupCcwConnectionPointMethods.FindConnectionPoint;
        return v;
    }

    private static void FillUnknownSlots(IntPtr* v)
    {
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
    }

    private static void FillAsyncIo2Slots(IntPtr* v)
    {
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, uint, uint*, IntPtr*, int>)&OpcDaGroupCcwAsyncIoMethods.Read;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint*, IntPtr*, int>)&OpcDaGroupCcwAsyncIoMethods.Write;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, uint*, int>)&OpcDaGroupCcwAsyncIoMethods.Refresh2;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcDaGroupCcwAsyncIoMethods.Cancel2;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&OpcDaGroupCcwAsyncIoMethods.SetEnable;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, int*, int>)&OpcDaGroupCcwAsyncIoMethods.GetEnable;
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
        if (riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_INVALIDARG;
        }
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
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
        if (iid == IOPCGroupStateMgt.InterfaceId || iid == IOPCGroupStateMgt2.InterfaceId)
        {
            return session.GroupStateMgtTearoff;
        }
        if (iid == IOPCItemMgt.InterfaceId)
        {
            return session.ItemMgtTearoff;
        }
        if (iid == IOPCSyncIO.InterfaceId)
        {
            return session.SyncIoTearoff;
        }
        if (iid == IOPCSyncIO2.InterfaceId)
        {
            return session.SyncIo2Tearoff;
        }
        if (iid == IOPCAsyncIO2.InterfaceId)
        {
            return session.AsyncIo2Tearoff;
        }
        if (iid == IOPCAsyncIO3.InterfaceId)
        {
            return session.AsyncIo3Tearoff;
        }
        if (iid == IConnectionPoint.InterfaceId)
        {
            return session.ConnectionPointTearoff;
        }
        if (iid == IConnectionPointContainer.InterfaceId)
        {
            return session.ConnectionPointContainerTearoff;
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

        DisposeScmSinks(session);
        RemoveSessionTearoffs(session);
        FreeSessionTearoffs(session);
        FreeSessionVtables(session);
        if (session.GroupHandle.IsAllocated)
        {
            session.GroupHandle.Free();
        }
    }

    private static void RemoveSessionTearoffs(CcwSession session)
    {
        RemoveTearoff(session.UnknownTearoff);
        RemoveTearoff(session.GroupStateMgtTearoff);
        RemoveTearoff(session.ItemMgtTearoff);
        RemoveTearoff(session.SyncIoTearoff);
        RemoveTearoff(session.SyncIo2Tearoff);
        RemoveTearoff(session.AsyncIo2Tearoff);
        RemoveTearoff(session.AsyncIo3Tearoff);
        RemoveTearoff(session.ConnectionPointTearoff);
        RemoveTearoff(session.ConnectionPointContainerTearoff);
    }

    private static void RemoveTearoff(IntPtr tearoff) => s_tearoffs.TryRemove(tearoff, out _);

    private static void DisposeScmSinks(CcwSession session)
    {
        foreach (OpcDataCallbackProxy sink in session.ScmSinks.Values)
        {
            sink.Dispose();
        }
        session.ScmSinks.Clear();
    }

    private static void FreeSessionTearoffs(CcwSession session)
    {
        FreeTearoff(session.UnknownTearoff);
        FreeTearoff(session.GroupStateMgtTearoff);
        FreeTearoff(session.ItemMgtTearoff);
        FreeTearoff(session.SyncIoTearoff);
        FreeTearoff(session.SyncIo2Tearoff);
        FreeTearoff(session.AsyncIo2Tearoff);
        FreeTearoff(session.AsyncIo3Tearoff);
        FreeTearoff(session.ConnectionPointTearoff);
        FreeTearoff(session.ConnectionPointContainerTearoff);
    }

    private static void FreeSessionVtables(CcwSession session)
    {
        FreeVtable(session.UnknownVtable);
        FreeVtable(session.GroupStateMgtVtable);
        FreeVtable(session.ItemMgtVtable);
        FreeVtable(session.SyncIoVtable);
        FreeVtable(session.SyncIo2Vtable);
        FreeVtable(session.AsyncIo2Vtable);
        FreeVtable(session.AsyncIo3Vtable);
        FreeVtable(session.ConnectionPointVtable);
        FreeVtable(session.ConnectionPointContainerVtable);
    }

    private static void FreeTearoff(IntPtr tearoff)
    {
        if (tearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)tearoff);
        }
    }

    private static void FreeVtable(IntPtr* vtable)
    {
        if (vtable != null)
        {
            NativeMemory.Free(vtable);
        }
    }

    /// <summary>
    /// Shared state across all tearoffs of one CCW.
    /// </summary>
    internal sealed class CcwSession
    {
        public CcwSession(GCHandle groupHandle)
        {
            GroupHandle = groupHandle;
        }

        public GCHandle GroupHandle { get; }

        public long RefCount;
        public int Disposed;
        public IntPtr UnknownTearoff;
        public IntPtr* UnknownVtable;
        public IntPtr GroupStateMgtTearoff;
        public IntPtr* GroupStateMgtVtable;
        public IntPtr ItemMgtTearoff;
        public IntPtr* ItemMgtVtable;
        public IntPtr SyncIoTearoff;
        public IntPtr* SyncIoVtable;
        public IntPtr SyncIo2Tearoff;
        public IntPtr* SyncIo2Vtable;
        public IntPtr AsyncIo2Tearoff;
        public IntPtr* AsyncIo2Vtable;
        public IntPtr AsyncIo3Tearoff;
        public IntPtr* AsyncIo3Vtable;
        public IntPtr ConnectionPointTearoff;
        public IntPtr* ConnectionPointVtable;
        public IntPtr ConnectionPointContainerTearoff;
        public IntPtr* ConnectionPointContainerVtable;
        public int NextScmSinkCookie;
        public ConcurrentDictionary<int, OpcDataCallbackProxy> ScmSinks { get; } = new();
    }
}
