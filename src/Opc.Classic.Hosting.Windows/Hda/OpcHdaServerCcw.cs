// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Dcom;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Windows COM-callable wrapper (CCW) over an <see cref="IOpcHdaServer" />.
/// Exposes separate tearoff vtables for <c>IUnknown</c>, <c>IOPCHDA_Server</c>, <c>IOPCCommon</c>,
/// HDA read interfaces, and the connection-point callback surface.
/// </summary>
/// <remarks>
/// Each supported interface is represented by a native tearoff whose first slot
/// points at its vtable. <c>QueryInterface(IID_IUnknown)</c> on any tearoff
/// returns the canonical identity pointer; all tearoffs share a single refcount
/// and managed <see cref="GCHandle" />. The simple-marshaling HDA server methods
/// and read bodies have real implementations in <see cref="OpcHdaServerCcwMethods" />.
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcHdaServerCcw
{
    internal const int S_OK = 0;
    internal const int S_FALSE = 1;
    internal static readonly int E_NOINTERFACE = global::Opc.Classic.OpcResultId.NoInterface.Code;
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);
    internal const int CONNECT_E_NOCONNECTION = unchecked((int)0x80040200);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    internal static readonly ConcurrentDictionary<IntPtr, CcwSession> s_tearoffs = new();

    /// <summary>
    /// Builds a CCW and returns the requested interface tearoff with refcount = 1.
    /// </summary>
    public static IntPtr Create(IOpcHdaServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var handle = GCHandle.Alloc(server, GCHandleType.Normal);
        var session = new CcwSession(handle, new OpcHdaServerDispatcher(server)) { RefCount = 1 };
        InitializeTearoffs(session);
        RegisterTearoffs(session);
        return ResolveTearoff(session, requestedIid);
    }

    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown ||
        iid == IOPCHDA_Server.InterfaceId ||
        iid == OpcCommonClientProxy.InterfaceId ||
        iid == IOPCHDA_SyncRead.InterfaceId ||
        iid == IOPCHDA_SyncUpdate.InterfaceId ||
        iid == IOPCHDA_SyncAnnotations.InterfaceId ||
        iid == IOPCHDA_AsyncRead.InterfaceId ||
        iid == IOPCHDA_AsyncUpdate.InterfaceId ||
        iid == IOPCHDA_AsyncAnnotations.InterfaceId ||
        iid == IOPCHDA_Playback.InterfaceId ||
        iid == OpcGuids.IID_IConnectionPoint ||
        iid == OpcGuids.IID_IConnectionPointContainer;

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

    internal static IOpcHdaServerDispatcher? ResolveDispatcher(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) ? session.Dispatcher : null;

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

    private static void InitializeTearoffs(CcwSession session)
    {
        session.UnknownVtable = AllocateUnknownVtable();
        session.ServerVtable = AllocateServerVtable();
        session.CommonVtable = AllocateCommonVtable();
        session.SyncReadVtable = AllocateSyncReadVtable();
        session.SyncUpdateVtable = AllocateSyncUpdateVtable();
        session.SyncAnnotationsVtable = AllocateSyncAnnotationsVtable();
        session.AsyncReadVtable = AllocateAsyncReadVtable();
        session.AsyncUpdateVtable = AllocateAsyncUpdateVtable();
        session.AsyncAnnotationsVtable = AllocateAsyncAnnotationsVtable();
        session.PlaybackVtable = AllocatePlaybackVtable();
        session.ConnectionPointVtable = AllocateConnectionPointVtable();
        session.ConnectionPointContainerVtable = AllocateConnectionPointContainerVtable();

        session.UnknownTearoff = AllocateTearoff(session.UnknownVtable);
        session.ServerTearoff = AllocateTearoff(session.ServerVtable);
        session.CommonTearoff = AllocateTearoff(session.CommonVtable);
        session.SyncReadTearoff = AllocateTearoff(session.SyncReadVtable);
        session.SyncUpdateTearoff = AllocateTearoff(session.SyncUpdateVtable);
        session.SyncAnnotationsTearoff = AllocateTearoff(session.SyncAnnotationsVtable);
        session.AsyncReadTearoff = AllocateTearoff(session.AsyncReadVtable);
        session.AsyncUpdateTearoff = AllocateTearoff(session.AsyncUpdateVtable);
        session.AsyncAnnotationsTearoff = AllocateTearoff(session.AsyncAnnotationsVtable);
        session.PlaybackTearoff = AllocateTearoff(session.PlaybackVtable);
        session.ConnectionPointTearoff = AllocateTearoff(session.ConnectionPointVtable);
        session.ConnectionPointContainerTearoff = AllocateTearoff(session.ConnectionPointContainerVtable);
    }

    private static void RegisterTearoffs(CcwSession session)
    {
        s_tearoffs[session.UnknownTearoff] = session;
        s_tearoffs[session.ServerTearoff] = session;
        s_tearoffs[session.CommonTearoff] = session;
        s_tearoffs[session.SyncReadTearoff] = session;
        s_tearoffs[session.SyncUpdateTearoff] = session;
        s_tearoffs[session.SyncAnnotationsTearoff] = session;
        s_tearoffs[session.AsyncReadTearoff] = session;
        s_tearoffs[session.AsyncUpdateTearoff] = session;
        s_tearoffs[session.AsyncAnnotationsTearoff] = session;
        s_tearoffs[session.PlaybackTearoff] = session;
        s_tearoffs[session.ConnectionPointTearoff] = session;
        s_tearoffs[session.ConnectionPointContainerTearoff] = session;
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
    private static IntPtr* AllocateCommonVtable()
    {
        // 3 IUnknown + 5 IOPCCommon methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&CommonSetLocaleId;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, int>)&CommonGetLocaleId;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, IntPtr*, int>)&CommonQueryAvailableLocaleIds;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr*, int>)&CommonGetErrorString;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&CommonSetClientName;
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
    private static IntPtr* AllocateSyncUpdateVtable()
    {
        // 3 IUnknown + 6 IOPCHDA_SyncUpdate methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(9 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcHdaSyncUpdateCcw.QueryCapabilities;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaSyncUpdateCcw.Insert;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaSyncUpdateCcw.Replace;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaSyncUpdateCcw.InsertReplace;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, uint, IntPtr, IntPtr*, int>)&OpcHdaSyncUpdateCcw.DeleteRaw;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaSyncUpdateCcw.DeleteAtTime;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSyncAnnotationsVtable()
    {
        // 3 IUnknown + 3 IOPCHDA_SyncAnnotations methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(6 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcHdaServerCcwMethods.SyncAnnotationsQueryCapabilities;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncReadAnnotations;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.SyncAnnotationsInsert;
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

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateAsyncUpdateVtable()
    {
        // 3 IUnknown + 7 IOPCHDA_AsyncUpdate methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(10 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcHdaAsyncUpdateCcw.QueryCapabilities;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaAsyncUpdateCcw.Insert;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaAsyncUpdateCcw.Replace;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaAsyncUpdateCcw.InsertReplace;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaAsyncUpdateCcw.DeleteRaw;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaAsyncUpdateCcw.DeleteAtTime;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaAsyncUpdateCcw.Cancel;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateAsyncAnnotationsVtable()
    {
        // 3 IUnknown + 4 IOPCHDA_AsyncAnnotations methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcHdaServerCcwMethods.AsyncAnnotationsQueryCapabilities;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncReadAnnotations;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaServerCcwMethods.AsyncAnnotationsInsert;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaServerCcwMethods.AsyncCancel;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocatePlaybackVtable()
    {
        // 3 IUnknown + 3 IOPCHDA_Playback methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(6 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, long, long, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaPlaybackCcw.ReadRawWithUpdate;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, long, uint, long, uint, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcHdaPlaybackCcw.ReadProcessedWithUpdate;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaPlaybackCcw.Cancel;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointVtable()
    {
        // 3 IUnknown + 5 IConnectionPoint methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, int>)&OpcHdaServerCcwConnectionPointMethods.GetConnectionInterface;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcHdaServerCcwConnectionPointMethods.GetConnectionPointContainer;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint*, int>)&OpcHdaServerCcwConnectionPointMethods.Advise;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaServerCcwConnectionPointMethods.Unadvise;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcHdaServerCcwConnectionPointMethods.EnumConnections;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointContainerVtable()
    {
        // 3 IUnknown + 2 IConnectionPointContainer methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        PopulateIUnknown(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcHdaServerCcwConnectionPointMethods.EnumConnectionPoints;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&OpcHdaServerCcwConnectionPointMethods.FindConnectionPoint;
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
        if (iid == OpcCommonClientProxy.InterfaceId)
        {
            return session.CommonTearoff;
        }
        if (iid == IOPCHDA_SyncRead.InterfaceId)
        {
            return session.SyncReadTearoff;
        }
        if (iid == IOPCHDA_SyncUpdate.InterfaceId)
        {
            return session.SyncUpdateTearoff;
        }
        if (iid == IOPCHDA_SyncAnnotations.InterfaceId)
        {
            return session.SyncAnnotationsTearoff;
        }
        if (iid == IOPCHDA_AsyncRead.InterfaceId)
        {
            return session.AsyncReadTearoff;
        }
        if (iid == IOPCHDA_AsyncUpdate.InterfaceId)
        {
            return session.AsyncUpdateTearoff;
        }
        if (iid == IOPCHDA_AsyncAnnotations.InterfaceId)
        {
            return session.AsyncAnnotationsTearoff;
        }
        if (iid == IOPCHDA_Playback.InterfaceId)
        {
            return session.PlaybackTearoff;
        }
        if (iid == OpcGuids.IID_IConnectionPoint)
        {
            return session.ConnectionPointTearoff;
        }
        if (iid == OpcGuids.IID_IConnectionPointContainer)
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

        UnregisterTearoffs(session);
        DisposeSessionState(session);
        FreeSessionNative(session);
        if (session.ServerHandle.IsAllocated)
        {
            session.ServerHandle.Free();
        }
    }

    private static void UnregisterTearoffs(CcwSession session)
    {
        s_tearoffs.TryRemove(session.UnknownTearoff, out _);
        s_tearoffs.TryRemove(session.ServerTearoff, out _);
        s_tearoffs.TryRemove(session.CommonTearoff, out _);
        s_tearoffs.TryRemove(session.SyncReadTearoff, out _);
        s_tearoffs.TryRemove(session.SyncUpdateTearoff, out _);
        s_tearoffs.TryRemove(session.SyncAnnotationsTearoff, out _);
        s_tearoffs.TryRemove(session.AsyncReadTearoff, out _);
        s_tearoffs.TryRemove(session.AsyncUpdateTearoff, out _);
        s_tearoffs.TryRemove(session.AsyncAnnotationsTearoff, out _);
        s_tearoffs.TryRemove(session.PlaybackTearoff, out _);
        s_tearoffs.TryRemove(session.ConnectionPointTearoff, out _);
        s_tearoffs.TryRemove(session.ConnectionPointContainerTearoff, out _);
    }

    private static void FreeSessionNative(CcwSession session)
    {
        FreeNative(session.UnknownTearoff);
        FreeNative(session.ServerTearoff);
        FreeNative(session.CommonTearoff);
        FreeNative(session.SyncReadTearoff);
        FreeNative(session.SyncUpdateTearoff);
        FreeNative(session.SyncAnnotationsTearoff);
        FreeNative(session.AsyncReadTearoff);
        FreeNative(session.AsyncUpdateTearoff);
        FreeNative(session.AsyncAnnotationsTearoff);
        FreeNative(session.PlaybackTearoff);
        FreeNative(session.ConnectionPointTearoff);
        FreeNative(session.ConnectionPointContainerTearoff);
        FreeNative(session.UnknownVtable);
        FreeNative(session.ServerVtable);
        FreeNative(session.CommonVtable);
        FreeNative(session.SyncReadVtable);
        FreeNative(session.SyncUpdateVtable);
        FreeNative(session.SyncAnnotationsVtable);
        FreeNative(session.AsyncReadVtable);
        FreeNative(session.AsyncUpdateVtable);
        FreeNative(session.AsyncAnnotationsVtable);
        FreeNative(session.PlaybackVtable);
        FreeNative(session.ConnectionPointVtable);
        FreeNative(session.ConnectionPointContainerVtable);
    }

    private static void DisposeSessionState(CcwSession session)
    {
        foreach (CancellationTokenSource cts in session.PendingOperations.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        session.PendingOperations.Clear();

        foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
        {
            sink.Dispose();
        }
        session.ScmSinks.Clear();
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

    [UnmanagedCallersOnly]
    private static int CommonSetLocaleId(IntPtr pThis, uint dwLcid) =>
        InvokeCommonNoPayloadResult(pThis, OpcCommonClientProxy.Opnums.SetLocaleId, (ref NdrWriter writer) => writer.WriteInt32(unchecked((int)dwLcid)));

    [UnmanagedCallersOnly]
    private static int CommonGetLocaleId(IntPtr pThis, uint* pdwLcid)
    {
        if (pdwLcid == null)
        {
            return E_INVALIDARG;
        }
        *pdwLcid = 0;
        int hr = InvokeCommon(pThis, OpcCommonClientProxy.Opnums.GetLocaleId, null, out ReadOnlyMemory<byte> payload);
        if (hr != S_OK)
        {
            return hr;
        }
        var reader = new NdrReader(payload.Span);
        *pdwLcid = unchecked((uint)reader.ReadInt32());
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int CommonQueryAvailableLocaleIds(IntPtr pThis, uint* pdwCount, IntPtr* ppdwLcid)
    {
        if (pdwCount == null || ppdwLcid == null)
        {
            return E_INVALIDARG;
        }
        *pdwCount = 0;
        *ppdwLcid = IntPtr.Zero;
        int hr = InvokeCommon(pThis, OpcCommonClientProxy.Opnums.QueryAvailableLocaleIds, null, out ReadOnlyMemory<byte> payload);
        if (hr != S_OK)
        {
            return hr;
        }
        var reader = new NdrReader(payload.Span);
        int[] localeIds = reader.ReadConformantInt32Array();
        IntPtr native = Marshal.AllocCoTaskMem(checked(localeIds.Length * sizeof(int)));
        for (int i = 0; i < localeIds.Length; i++)
        {
            Marshal.WriteInt32(native, i * sizeof(int), localeIds[i]);
        }
        *pdwCount = (uint)localeIds.Length;
        *ppdwLcid = native;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int CommonGetErrorString(IntPtr pThis, int dwError, IntPtr* ppString)
    {
        if (ppString == null)
        {
            return E_INVALIDARG;
        }
        *ppString = IntPtr.Zero;
        int hr = InvokeCommon(pThis, OpcCommonClientProxy.Opnums.GetErrorString, (ref NdrWriter writer) => writer.WriteInt32(dwError), out ReadOnlyMemory<byte> payload);
        if (hr != S_OK)
        {
            return hr;
        }
        var reader = new NdrReader(payload.Span);
        *ppString = Marshal.StringToCoTaskMemUni(reader.ReadUnicodeStringPtr() ?? string.Empty);
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int CommonSetClientName(IntPtr pThis, IntPtr szName) =>
        InvokeCommonNoPayloadResult(
            pThis,
            OpcCommonClientProxy.Opnums.SetClientName,
            (ref NdrWriter writer) => writer.WriteUnicodeStringPtr(szName == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(szName) ?? string.Empty));

    private static int InvokeCommonNoPayloadResult(IntPtr pThis, int opnum, NdrWriteAction? write) =>
        InvokeCommon(pThis, opnum, write, out _);

    private static int InvokeCommon(IntPtr pThis, int opnum, NdrWriteAction? write, out ReadOnlyMemory<byte> responsePayload)
    {
        responsePayload = ReadOnlyMemory<byte>.Empty;
        IOpcHdaServerDispatcher? dispatcher = ResolveDispatcher(pThis);
        if (dispatcher is null)
        {
            return E_NOTIMPL;
        }
        try
        {
            byte[] request = write is null ? Array.Empty<byte>() : WritePayload(write);
#pragma warning disable VSTHRD002 // Synchronous bridge across the COM ABI.
            NdrCallResult result = dispatcher.DispatchAsync(
                OpcCommonClientProxy.InterfaceId,
                opnum,
                request,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            responsePayload = result.ResponsePayload;
            return result.Hresult;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch.
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        for (int size = 256; size <= 8192; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                write(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < 8192)
            {
            }
        }
        throw new InvalidOperationException("Unable to encode the IOPCCommon CCW payload.");
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    internal sealed class CcwSession
    {
        public CcwSession(GCHandle serverHandle, IOpcHdaServerDispatcher dispatcher)
        {
            ServerHandle = serverHandle;
            Dispatcher = dispatcher;
        }

        public GCHandle ServerHandle { get; }
        public IOpcHdaServerDispatcher Dispatcher { get; }

        public long RefCount;
        public int Disposed;
        public IntPtr UnknownTearoff;
        public IntPtr* UnknownVtable;
        public IntPtr ServerTearoff;
        public IntPtr* ServerVtable;
        public IntPtr CommonTearoff;
        public IntPtr* CommonVtable;
        public IntPtr SyncReadTearoff;
        public IntPtr* SyncReadVtable;
        public IntPtr SyncUpdateTearoff;
        public IntPtr* SyncUpdateVtable;
        public IntPtr SyncAnnotationsTearoff;
        public IntPtr* SyncAnnotationsVtable;
        public IntPtr AsyncReadTearoff;
        public IntPtr* AsyncReadVtable;
        public IntPtr AsyncUpdateTearoff;
        public IntPtr* AsyncUpdateVtable;
        public IntPtr AsyncAnnotationsTearoff;
        public IntPtr* AsyncAnnotationsVtable;
        public IntPtr PlaybackTearoff;
        public IntPtr* PlaybackVtable;
        public IntPtr ConnectionPointTearoff;
        public IntPtr* ConnectionPointVtable;
        public IntPtr ConnectionPointContainerTearoff;
        public IntPtr* ConnectionPointContainerVtable;
        public ConcurrentDictionary<int, OpcHdaCallbackProxy> ScmSinks { get; } = new();
        public ConcurrentDictionary<int, CancellationTokenSource> PendingOperations { get; } = new();
        public int NextScmSinkCookie;
        public int NextCancelId;
    }
}
