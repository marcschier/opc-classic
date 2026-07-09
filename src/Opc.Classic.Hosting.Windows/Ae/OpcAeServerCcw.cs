// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting.Windows;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Hosting.Windows;

/// <summary>
/// Windows COM-callable wrapper (CCW) over an <see cref="IOpcAeServer"/>.
/// Exposes separate tearoff vtables for <c>IUnknown</c>,
/// <c>IOPCEventServer</c>, <c>IOPCCommon</c>, and a legacy direct <c>IOPCEventSubscriptionMgt</c> tearoff.
/// </summary>
/// <remarks>
/// <para>
/// <b>Identity.</b> Each supported interface is a separate native tearoff
/// pointer. <c>QueryInterface(IID_IUnknown)</c> on any tearoff returns the
/// canonical identity pointer. All tearoffs share one <see cref="CcwSession"/>
/// holding the refcount and managed server <see cref="GCHandle"/>.
/// </para>
/// <para>
/// <b>Subscription tearoff.</b> AE clients normally receive
/// <c>IOPCEventSubscriptionMgt</c> from <c>CreateEventSubscription</c>, which
/// returns a dedicated <see cref="OpcAeSubscriptionCcw" />. The legacy direct
/// subscription tearoff remains reachable by direct QI for tests or for managed
/// servers that also implement <see cref="IOPCEventSubscriptionMgt"/>.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcAeServerCcw
{
    internal const int S_OK = 0;
    internal static readonly int E_NOINTERFACE = global::Opc.Classic.OpcResultId.NoInterface.Code;
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    // Tearoff pointer -> session. Multiple tearoffs map to the same session.
    internal static readonly ConcurrentDictionary<IntPtr, CcwSession> s_tearoffs = new();

    /// <summary>
    /// Builds a CCW around <paramref name="server"/> for the requested IID with refcount = 1.
    /// </summary>
    /// <returns>A CCW tearoff pointer, or <see cref="IntPtr.Zero"/> if the IID isn't supported.</returns>
    public static IntPtr Create(IOpcAeServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var serverHandle = GCHandle.Alloc(server, GCHandleType.Normal);
        var dispatcherHandle = GCHandle.Alloc(new OpcAeServerDispatcher(server), GCHandleType.Normal);
        var session = new CcwSession(serverHandle, dispatcherHandle) { RefCount = 1 };

        IntPtr* unknownVtable = AllocateUnknownVtable();
        IntPtr* eventServerVtable = AllocateEventServerVtable();
        IntPtr* commonVtable = AllocateCommonVtable();
        IntPtr* subscriptionMgtVtable = AllocateSubscriptionMgtVtable();
        IntPtr* connectionPointVtable = AllocateConnectionPointVtable();
        IntPtr* connectionPointContainerVtable = AllocateConnectionPointContainerVtable();

        IntPtr unknownTearoff = AllocateTearoff(unknownVtable);
        IntPtr eventServerTearoff = AllocateTearoff(eventServerVtable);
        IntPtr commonTearoff = AllocateTearoff(commonVtable);
        IntPtr subscriptionMgtTearoff = AllocateTearoff(subscriptionMgtVtable);
        IntPtr connectionPointTearoff = AllocateTearoff(connectionPointVtable);
        IntPtr connectionPointContainerTearoff = AllocateTearoff(connectionPointContainerVtable);

        session.UnknownTearoff = unknownTearoff;
        session.UnknownVtable = unknownVtable;
        session.EventServerTearoff = eventServerTearoff;
        session.EventServerVtable = eventServerVtable;
        session.CommonTearoff = commonTearoff;
        session.CommonVtable = commonVtable;
        session.SubscriptionMgtTearoff = subscriptionMgtTearoff;
        session.SubscriptionMgtVtable = subscriptionMgtVtable;
        session.ConnectionPointTearoff = connectionPointTearoff;
        session.ConnectionPointVtable = connectionPointVtable;
        session.ConnectionPointContainerTearoff = connectionPointContainerTearoff;
        session.ConnectionPointContainerVtable = connectionPointContainerVtable;
        if (server is IAeServer aeServerWithShutdown)
        {
            aeServerWithShutdown.ServerShutdown += session.OnServerShutdown;
        }

        s_tearoffs[unknownTearoff] = session;
        s_tearoffs[eventServerTearoff] = session;
        s_tearoffs[commonTearoff] = session;
        s_tearoffs[subscriptionMgtTearoff] = session;
        s_tearoffs[connectionPointTearoff] = session;
        s_tearoffs[connectionPointContainerTearoff] = session;

        return ResolveTearoff(session, requestedIid);
    }

    /// <summary>
    /// Builds a dispatcher-backed CCW for tests and hosts that already route AE calls.
    /// </summary>
    /// <returns>A CCW tearoff pointer, or <see cref="IntPtr.Zero"/> if the IID isn't supported.</returns>
    public static IntPtr Create(IOpcAeServerDispatcher dispatcher, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var dispatcherHandle = GCHandle.Alloc(dispatcher, GCHandleType.Normal);
        var session = new CcwSession(default, dispatcherHandle) { RefCount = 1 };

        IntPtr* unknownVtable = AllocateUnknownVtable();
        IntPtr* eventServerVtable = AllocateEventServerVtable();
        IntPtr* commonVtable = AllocateCommonVtable();
        IntPtr* subscriptionMgtVtable = AllocateSubscriptionMgtVtable();
        IntPtr* connectionPointVtable = AllocateConnectionPointVtable();
        IntPtr* connectionPointContainerVtable = AllocateConnectionPointContainerVtable();

        IntPtr unknownTearoff = AllocateTearoff(unknownVtable);
        IntPtr eventServerTearoff = AllocateTearoff(eventServerVtable);
        IntPtr commonTearoff = AllocateTearoff(commonVtable);
        IntPtr subscriptionMgtTearoff = AllocateTearoff(subscriptionMgtVtable);
        IntPtr connectionPointTearoff = AllocateTearoff(connectionPointVtable);
        IntPtr connectionPointContainerTearoff = AllocateTearoff(connectionPointContainerVtable);

        session.UnknownTearoff = unknownTearoff;
        session.UnknownVtable = unknownVtable;
        session.EventServerTearoff = eventServerTearoff;
        session.EventServerVtable = eventServerVtable;
        session.CommonTearoff = commonTearoff;
        session.CommonVtable = commonVtable;
        session.SubscriptionMgtTearoff = subscriptionMgtTearoff;
        session.SubscriptionMgtVtable = subscriptionMgtVtable;
        session.ConnectionPointTearoff = connectionPointTearoff;
        session.ConnectionPointVtable = connectionPointVtable;
        session.ConnectionPointContainerTearoff = connectionPointContainerTearoff;
        session.ConnectionPointContainerVtable = connectionPointContainerVtable;
        // Dispatcher-only overload does not directly hold an IAeServer reference;
        // shutdown wiring is provided by the dispatcher itself via OnServerShutdown.

        s_tearoffs[unknownTearoff] = session;
        s_tearoffs[eventServerTearoff] = session;
        s_tearoffs[commonTearoff] = session;
        s_tearoffs[subscriptionMgtTearoff] = session;
        s_tearoffs[connectionPointTearoff] = session;
        s_tearoffs[connectionPointContainerTearoff] = session;

        return ResolveTearoff(session, requestedIid);
    }

    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown ||
        iid == IOPCEventServer.InterfaceId ||
        iid == OpcCommonClientProxy.InterfaceId ||
        iid == IOPCEventSubscriptionMgt.InterfaceId ||
        iid == OpcGuids.IID_IConnectionPoint ||
        iid == OpcGuids.IID_IConnectionPointContainer;

    /// <summary>
    /// Test helper: returns the current refcount, or -1 if the pointer is not a known tearoff.
    /// </summary>
    public static long GetReferenceCount(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? Interlocked.Read(ref session.RefCount)
            : -1L;

    /// <summary>
    /// Test helper: looks up the canonical IUnknown tearoff for a CCW pointer.
    /// </summary>
    public static IntPtr GetUnknownTearoff(IntPtr anyTearoff) =>
        s_tearoffs.TryGetValue(anyTearoff, out CcwSession? session)
            ? session.UnknownTearoff
            : IntPtr.Zero;

    internal static IOpcAeServer? ResolveServer(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) && session.ServerHandle.IsAllocated
            ? session.ServerHandle.Target as IOpcAeServer
            : null;

    internal static IOpcAeServerDispatcher? ResolveDispatcher(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) && session.DispatcherHandle.IsAllocated
            ? session.DispatcherHandle.Target as IOpcAeServerDispatcher
            : null;

    internal static IOPCEventSubscriptionMgt? ResolveSubscription(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) && session.ServerHandle.IsAllocated
            ? session.ServerHandle.Target as IOPCEventSubscriptionMgt
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateUnknownVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(3 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateEventServerVtable()
    {
        // 3 IUnknown + 16 IOPCEventServer methods (opnums 3..18) = 19 slots.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(19 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetStatus;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, int, int, Guid*, IntPtr*, IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.CreateEventSubscription;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.QueryAvailableFilters;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, IntPtr*, int>)&OpcAeServerCcwMethods.QueryEventCategories;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.QueryConditionNames;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.QuerySubConditionNames;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.QuerySourceConditions;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, IntPtr*, IntPtr*, int>)&OpcAeServerCcwMethods.QueryEventAttributes;
        v[11] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, IntPtr, IntPtr, int, IntPtr, IntPtr*, IntPtr*, IntPtr*, int>)&OpcAeServerCcwMethods.TranslateToItemIDs;
        v[12] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetConditionState;
        v[13] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.EnableConditionByArea;
        v[14] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.EnableConditionBySource;
        v[15] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.DisableConditionByArea;
        v[16] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.DisableConditionBySource;
        v[17] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.AckCondition;
        v[18] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&OpcAeServerCcwMethods.CreateAreaBrowser;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateCommonVtable()
    {
        // 3 IUnknown + 5 IOPCCommon methods.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&CommonSetLocaleId;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, int>)&CommonGetLocaleId;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, IntPtr*, int>)&CommonQueryAvailableLocaleIds;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr*, int>)&CommonGetErrorString;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&CommonSetClientName;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSubscriptionMgtVtable()
    {
        // 3 IUnknown + 8 IOPCEventSubscriptionMgt methods (opnums 3..10) = 11 slots.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(11 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, IntPtr, int, int, int, IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.SetFilter;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetFilter;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, IntPtr, int>)&OpcAeServerCcwMethods.SetReturnedAttributes;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetReturnedAttributes;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&OpcAeServerCcwMethods.Refresh;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&OpcAeServerCcwMethods.CancelRefresh;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.GetState;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.SetState;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, int>)&ShutdownGetConnectionInterface;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&ShutdownGetConnectionPointContainer;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint*, int>)&ShutdownAdvise;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&ShutdownUnadvise;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&ShutdownEnumConnections;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointContainerVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&ShutdownEnumConnectionPoints;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&ShutdownFindConnectionPoint;
        return v;
    }

    private static void FillUnknownSlots(IntPtr* v)
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
        if (iid == IOPCEventServer.InterfaceId)
        {
            return session.EventServerTearoff;
        }
        if (iid == OpcCommonClientProxy.InterfaceId)
        {
            return session.CommonTearoff;
        }
        if (iid == IOPCEventSubscriptionMgt.InterfaceId)
        {
            return session.SubscriptionMgtTearoff;
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
        s_tearoffs.TryRemove(session.UnknownTearoff, out _);
        s_tearoffs.TryRemove(session.EventServerTearoff, out _);
        s_tearoffs.TryRemove(session.CommonTearoff, out _);
        s_tearoffs.TryRemove(session.SubscriptionMgtTearoff, out _);
        s_tearoffs.TryRemove(session.ConnectionPointTearoff, out _);
        s_tearoffs.TryRemove(session.ConnectionPointContainerTearoff, out _);
        FreeTearoffs(session);
        FreeVtables(session);
        if (session.ServerHandle.IsAllocated)
        {
            if (session.ServerHandle.Target is IAeServer aeServer)
            {
                aeServer.ServerShutdown -= session.OnServerShutdown;
            }
            session.ServerHandle.Free();
        }
        if (session.DispatcherHandle.IsAllocated)
        {
            session.DispatcherHandle.Free();
        }
    }

    private static void FreeTearoffs(CcwSession session)
    {
        if (session.UnknownTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.UnknownTearoff);
        }
        if (session.EventServerTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.EventServerTearoff);
        }
        if (session.CommonTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.CommonTearoff);
        }
        if (session.SubscriptionMgtTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.SubscriptionMgtTearoff);
        }
        if (session.ConnectionPointTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.ConnectionPointTearoff);
        }
        if (session.ConnectionPointContainerTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.ConnectionPointContainerTearoff);
        }
    }

    private static void FreeVtables(CcwSession session)
    {
        if (session.UnknownVtable != null)
        {
            NativeMemory.Free(session.UnknownVtable);
        }
        if (session.EventServerVtable != null)
        {
            NativeMemory.Free(session.EventServerVtable);
        }
        if (session.CommonVtable != null)
        {
            NativeMemory.Free(session.CommonVtable);
        }
        if (session.SubscriptionMgtVtable != null)
        {
            NativeMemory.Free(session.SubscriptionMgtVtable);
        }
        if (session.ConnectionPointVtable != null)
        {
            NativeMemory.Free(session.ConnectionPointVtable);
        }
        if (session.ConnectionPointContainerVtable != null)
        {
            NativeMemory.Free(session.ConnectionPointContainerVtable);
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
        IOpcAeServerDispatcher? dispatcher = ResolveDispatcher(pThis);
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

    [UnmanagedCallersOnly]
    private static int ShutdownGetConnectionInterface(IntPtr pThis, Guid* piid)
    {
        _ = pThis;
        if (piid == null)
        {
            return E_INVALIDARG;
        }
        *piid = OpcGuids.IID_IOPCShutdown;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ShutdownGetConnectionPointContainer(IntPtr pThis, IntPtr* ppCpc)
    {
        WriteNull(ppCpc);
        if (ppCpc == null)
        {
            return E_INVALIDARG;
        }
        return s_tearoffs.TryGetValue(pThis, out CcwSession? session)
            ? ReturnTearoff(session, session.ConnectionPointContainerTearoff, ppCpc)
            : E_FAIL;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int ShutdownAdvise(IntPtr pThis, IntPtr pUnk, uint* pdwCookie)
    {
        if (pdwCookie != null)
        {
            *pdwCookie = 0;
        }
        if (pdwCookie == null || pUnk == IntPtr.Zero)
        {
            return E_INVALIDARG;
        }
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return E_FAIL;
        }
        OpcShutdownSinkProxy? proxy = null;
        try
        {
            proxy = new OpcShutdownSinkProxy(pUnk);
            int cookie = Interlocked.Increment(ref session.NextShutdownCookie);
            if (!session.ShutdownSinks.TryAdd(cookie, proxy))
            {
                proxy.Dispose();
                return E_FAIL;
            }
            proxy = null;
            *pdwCookie = unchecked((uint)cookie);
            return S_OK;
        }
        catch (Exception ex)
        {
            proxy?.Dispose();
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    private static int ShutdownUnadvise(IntPtr pThis, uint cookie)
    {
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return E_FAIL;
        }
        if (!session.ShutdownSinks.TryRemove(unchecked((int)cookie), out OpcShutdownSinkProxy? proxy))
        {
            return unchecked((int)0x80040200);
        }
        proxy.Dispose();
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ShutdownEnumConnections(IntPtr pThis, IntPtr* ppEnum)
    {
        _ = pThis;
        WriteNull(ppEnum);
        return ppEnum == null ? E_INVALIDARG : E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int ShutdownEnumConnectionPoints(IntPtr pThis, IntPtr* ppEnum)
    {
        WriteNull(ppEnum);
        if (ppEnum == null)
        {
            return E_INVALIDARG;
        }
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return E_FAIL;
        }
        AddRefComPointer(session.ConnectionPointTearoff);
        Opc.Classic.Da.Hosting.Windows.OpcEnumConnectionPointsEnumerator? enumerator = null;
        try
        {
            enumerator = new Opc.Classic.Da.Hosting.Windows.OpcEnumConnectionPointsEnumerator([session.ConnectionPointTearoff]);
            *ppEnum = Opc.Classic.Da.Hosting.Windows.OpcEnumConnectionPointsCcw.Create(enumerator);
            enumerator = null;
            return S_OK;
        }
        finally
        {
            enumerator?.Dispose();
        }
    }

    [UnmanagedCallersOnly]
    private static int ShutdownFindConnectionPoint(IntPtr pThis, Guid* riid, IntPtr* ppCp)
    {
        WriteNull(ppCp);
        if (riid == null || ppCp == null)
        {
            return E_INVALIDARG;
        }
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return E_FAIL;
        }
        return *riid == OpcGuids.IID_IOPCShutdown
            ? ReturnTearoff(session, session.ConnectionPointTearoff, ppCp)
            : E_NOINTERFACE;
    }

    private static int ReturnTearoff(CcwSession session, IntPtr tearoff, IntPtr* ppv)
    {
        if (ppv == null || tearoff == IntPtr.Zero)
        {
            return E_INVALIDARG;
        }
        *ppv = tearoff;
        Interlocked.Increment(ref session.RefCount);
        return S_OK;
    }

    private static void AddRefComPointer(IntPtr pointer)
    {
        IntPtr* vtable = *(IntPtr**)pointer;
        var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
        _ = addRef(pointer);
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        ArgumentException => E_INVALIDARG,
        ObjectDisposedException => E_FAIL,
        _ => E_FAIL,
    };

    private static void WriteNull(IntPtr* ppv)
    {
        if (ppv != null)
        {
            *ppv = IntPtr.Zero;
        }
    }

    /// <summary>
    /// Shared state across all tearoffs of one CCW.
    /// </summary>
    internal sealed class CcwSession
    {
        public CcwSession(GCHandle serverHandle, GCHandle dispatcherHandle)
        {
            ServerHandle = serverHandle;
            DispatcherHandle = dispatcherHandle;
        }

        public GCHandle ServerHandle { get; }
        public GCHandle DispatcherHandle { get; }

        public long RefCount;
        public int Disposed;
        public IntPtr UnknownTearoff;
        public IntPtr* UnknownVtable;
        public IntPtr EventServerTearoff;
        public IntPtr* EventServerVtable;
        public IntPtr CommonTearoff;
        public IntPtr* CommonVtable;
        public IntPtr SubscriptionMgtTearoff;
        public IntPtr* SubscriptionMgtVtable;
        public IntPtr ConnectionPointTearoff;
        public IntPtr* ConnectionPointVtable;
        public IntPtr ConnectionPointContainerTearoff;
        public IntPtr* ConnectionPointContainerVtable;
        public ConcurrentDictionary<int, OpcShutdownSinkProxy> ShutdownSinks { get; } = new();
        public int NextShutdownCookie;

        public void OnServerShutdown(object? sender, EventArgs e)
        {
            foreach (OpcShutdownSinkProxy sink in ShutdownSinks.Values)
            {
                sink.ShutdownRequest(string.Empty);
            }
        }
    }
}
