//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeEventSinkProxyTests
{
    private const int S_OK = 0;

    [Test]
    public async Task Advise_fire_events_and_unadvise_stops_delivery()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new RecordingAeDispatcher();
        IntPtr subscription = OpcAeEventSinkTestHelpers.CreateSubscription(dispatcher);
        IntPtr connectionPoint = OpcAeEventSinkTestHelpers.FindEventConnectionPoint(subscription);
        IntPtr sink = OpcAeEventSinkTestHelpers.CreateSinkStub();
        try
        {
            (int adviseHr, uint cookie) = OpcAeEventSinkTestHelpers.Advise(connectionPoint, sink);
            await Assert.That(adviseHr).IsEqualTo(S_OK);
            await Assert.That(OpcAeSubscriptionCcw.GetScmSinkCount(subscription)).IsEqualTo(1);

            OpcEventNotification[] notifications = OpcAeEventSinkTestHelpers.CreateNotifications();
            await dispatcher.FireAsync(refresh: false, lastRefresh: false, notifications, CancellationToken.None);

            OpcAeEventSinkTestHelpers.EventCallbackInvocation invocation = OpcAeEventSinkTestHelpers.GetInvocations(sink)[0];
            await Assert.That(invocation.ClientSubscription).IsEqualTo(dispatcher.ClientSubscription);
            await Assert.That(invocation.Refresh).IsFalse();
            await Assert.That(invocation.LastRefresh).IsFalse();
            await Assert.That(invocation.Events.Length).IsEqualTo(3);
            await Assert.That(invocation.Events[0].Source).IsEqualTo("Plant1.AreaA.Tank7");
            await Assert.That(invocation.Events[0].Message).IsEqualTo("Level high");
            await Assert.That(invocation.Events[0].Attributes[0]).IsEqualTo(OpcVariant.FromString("AreaA"));
            await Assert.That(invocation.Events[1].ActorId).IsEqualTo("operator-a");
            await Assert.That(invocation.Events[2].Severity).IsEqualTo(125U);

            int unadviseHr = OpcAeEventSinkTestHelpers.Unadvise(connectionPoint, cookie);
            await Assert.That(unadviseHr).IsEqualTo(S_OK);
            await Assert.That(OpcAeSubscriptionCcw.GetScmSinkCount(subscription)).IsEqualTo(0);

            await dispatcher.FireAsync(refresh: false, lastRefresh: false, notifications, CancellationToken.None);
            await Assert.That(OpcAeEventSinkTestHelpers.GetInvocations(sink).Length).IsEqualTo(1);
        }
        finally
        {
            OpcAeEventSinkTestHelpers.DestroySinkStub(sink);
        }
    }
}

internal class RecordingAeDispatcher : IOpcAeServerDispatcher, IOPCEventSubscriptionMgt, IOpcAeEventSinkRegistration
{
    private readonly ConcurrentDictionary<int, IOPCEventSink> _sinks = new();
    private int _nextCookie;

    public int ClientSubscription { get; protected set; } = 0xAA01;

    public Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
        Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));

    public Task<IOPCEventSubscriptionMgt> CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = active;
        _ = requestedInterfaceId;
        ClientSubscription = clientSubscription;
        revisedBufferTime = bufferTime;
        revisedMaxSize = maxSize;
        return Task.FromResult<IOPCEventSubscriptionMgt>(this);
    }

    public Task<int> AdviseEventSinkAsync(IOPCEventSink sink, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int cookie = Interlocked.Increment(ref _nextCookie);
        _sinks[cookie] = sink;
        return Task.FromResult(cookie);
    }

    public Task UnadviseEventSinkAsync(int connection, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _sinks.TryRemove(connection, out _);
        return Task.CompletedTask;
    }

    public async Task FireAsync(bool refresh, bool lastRefresh, OpcEventNotification[] notifications, CancellationToken cancellationToken)
    {
        foreach (IOPCEventSink sink in _sinks.Values)
        {
            await sink.OnEventAsync(ClientSubscription, refresh, lastRefresh, notifications, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task SetFilterAsync(int eventType, int[] eventCategories, int lowSeverity, int highSeverity, string[] areas, string[] sources, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task GetFilterAsync(out int eventType, out int[] eventCategories, out int lowSeverity, out int highSeverity, out string[] areas, out string[] sources, CancellationToken cancellationToken = default)
    {
        eventType = (int)EventType.All;
        eventCategories = [];
        lowSeverity = 0;
        highSeverity = 1000;
        areas = [];
        sources = [];
        return Task.CompletedTask;
    }

    public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default) =>
        Task.FromResult(Array.Empty<int>());

    public virtual Task RefreshAsync(int connection, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public virtual Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken cancellationToken = default)
    {
        active = true;
        bufferTime = 100;
        maxSize = 10;
        clientSubscription = ClientSubscription;
        return Task.CompletedTask;
    }

    public Task SetStateAsync(bool active, int bufferTime, int maxSize, int clientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default)
    {
        _ = active;
        ClientSubscription = clientSubscription;
        revisedBufferTime = bufferTime;
        revisedMaxSize = maxSize;
        return Task.CompletedTask;
    }

    protected bool TryGetSink(int connection, out IOPCEventSink? sink) =>
        _sinks.TryGetValue(connection, out sink);
}

[SupportedOSPlatform("windows")]
internal static unsafe class OpcAeEventSinkTestHelpers
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_POINTER = unchecked((int)0x80004003);
    private const int VariantValueOffset = 8;
    private const int SinkVtableSlotCount = 4;
    private const ushort VtEmpty = 0;
    private const ushort VtNull = 1;
    private const ushort VtI4 = 3;
    private const ushort VtR8 = 5;
    private const ushort VtBstr = 8;
    private const ushort VtBool = 11;
    private const ushort VtUi4 = 19;

    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, SinkStubSession> s_sinkStubs = new();

    internal sealed record EventCallbackInvocation(int ClientSubscription, bool Refresh, bool LastRefresh, ReceivedEvent[] Events);

    internal sealed record ReceivedEvent(
        ushort ChangeMask,
        ushort NewState,
        string? Source,
        long Time,
        string? Message,
        uint EventType,
        uint EventCategory,
        uint Severity,
        string? ConditionName,
        string? SubconditionName,
        ushort Quality,
        bool AckRequired,
        long ActiveTime,
        uint Cookie,
        OpcVariant[] Attributes,
        string? ActorId);

    internal static IntPtr CreateSinkStub(bool supportsEventSink = true)
    {
        IntPtr* vtable = AllocateSinkVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_sinkStubs[instance] = new SinkStubSession(vtable, supportsEventSink);
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

    internal static EventCallbackInvocation[] GetInvocations(IntPtr sink)
    {
        if (!s_sinkStubs.TryGetValue(sink, out SinkStubSession? session))
        {
            return [];
        }
        lock (session.Gate)
        {
            return session.Invocations.ToArray();
        }
    }

    internal static IntPtr CreateSubscription(IOpcAeServerDispatcher dispatcher)
    {
        IntPtr ccw = OpcAeServerCcw.Create(dispatcher, s_iidUnknown);
        IntPtr eventServer = QueryInterface(ccw, IOPCEventServer.InterfaceId);
        CreateEventSubscriptionDelegate create = GetMethod<CreateEventSubscriptionDelegate>(eventServer, 4);
        Guid iid = IOPCEventSubscriptionMgt.InterfaceId;
        IntPtr pRevisedBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
        IntPtr pRevisedMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
        try
        {
            int hr = create(eventServer, 1, 100, 10, 0xAA01, ref iid, out IntPtr subscription, pRevisedBufferTime, pRevisedMaxSize);
            if (hr != S_OK)
            {
                throw new InvalidOperationException($"CreateEventSubscription failed with 0x{hr:X8}.");
            }
            return subscription;
        }
        finally
        {
            Marshal.FreeCoTaskMem(pRevisedBufferTime);
            Marshal.FreeCoTaskMem(pRevisedMaxSize);
        }
    }

    internal static IntPtr FindEventConnectionPoint(IntPtr subscription)
    {
        IntPtr cpc = QueryInterface(subscription, OpcGuids.IID_IConnectionPointContainer);
        FindConnectionPointDelegate find = GetMethod<FindConnectionPointDelegate>(cpc, 4);
        Guid iid = IOPCEventSink.InterfaceId;
        int hr = find(cpc, ref iid, out IntPtr connectionPoint);
        if (hr != S_OK)
        {
            throw new InvalidOperationException($"FindConnectionPoint failed with 0x{hr:X8}.");
        }
        return connectionPoint;
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

    internal static int InvokeRefresh(IntPtr subscription, int connection)
    {
        RefreshDelegate refresh = GetMethod<RefreshDelegate>(subscription, 7);
        return refresh(subscription, connection);
    }

    internal static int InvokeCancelRefresh(IntPtr subscription, int connection)
    {
        CancelRefreshDelegate cancel = GetMethod<CancelRefreshDelegate>(subscription, 8);
        return cancel(subscription, connection);
    }

    internal static OpcEventNotification[] CreateNotifications()
    {
        DateTimeOffset now = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        return
        [
            new OpcEventNotification(1, (ushort)(ConditionState.Active | ConditionState.Enabled), "Plant1.AreaA.Tank7", now, "Level high", (uint)EventType.Condition, 1001, 850, "Level", "HiHi", OpcQuality.Good, true, now.AddSeconds(-10), 7001, [OpcVariant.FromString("AreaA"), OpcVariant.FromInt32(95)], "operator-a"),
            new OpcEventNotification(0, 0, "Plant1.AreaA.Tank7", now.AddSeconds(1), "Operator inspected", (uint)EventType.Tracking, 1002, 250, null, null, OpcQuality.Good, false, now.AddSeconds(1), 0, [OpcVariant.FromString("clipboard")], "operator-a"),
            new OpcEventNotification(0, 0, "Plant1.AreaB.Pump1", now.AddSeconds(2), "Pump started", (uint)EventType.Simple, 1003, 125, null, null, OpcQuality.Good, false, now.AddSeconds(2), 0, [OpcVariant.FromDouble(42.5)], null),
        ];
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSinkVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(SinkVtableSlotCount * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&SinkQueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&SinkAddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&SinkRelease;
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, int, uint, IntPtr, int>)&SinkOnEvent;
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
        if (*riid == s_iidUnknown || (session.SupportsEventSink && *riid == IOPCEventSink.InterfaceId))
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
    private static int SinkOnEvent(IntPtr pThis, uint clientSubscription, int refresh, int lastRefresh, uint count, IntPtr events)
    {
        try
        {
            if (!s_sinkStubs.TryGetValue(pThis, out SinkStubSession? session))
            {
                return E_NOINTERFACE;
            }
            var invocation = new EventCallbackInvocation(
                unchecked((int)clientSubscription),
                refresh != 0,
                lastRefresh != 0,
                ReadEvents(events, checked((int)count)));
            lock (session.Gate)
            {
                session.Invocations.Add(invocation);
            }
            return S_OK;
        }
        catch
        {
            return unchecked((int)0x80004005);
        }
    }

    private static ReceivedEvent[] ReadEvents(IntPtr events, int count)
    {
        if (count == 0)
        {
            return [];
        }
        var result = new ReceivedEvent[count];
        int structSize = sizeof(NativeOneEventStruct);
        byte* basePtr = (byte*)events;
        for (int i = 0; i < count; i++)
        {
            NativeOneEventStruct* native = (NativeOneEventStruct*)(basePtr + (i * structSize));
            result[i] = new ReceivedEvent(
                native->wChangeMask,
                native->wNewState,
                ReadBstr(native->szSource),
                native->ftTime,
                ReadBstr(native->szMessage),
                native->dwEventType,
                native->dwEventCategory,
                native->dwSeverity,
                ReadBstr(native->szConditionName),
                ReadBstr(native->szSubconditionName),
                native->wQuality,
                native->bAckRequired != 0,
                native->ftActiveTime,
                native->dwCookie,
                ReadVariants(native->pEventAttributes, checked((int)native->dwNumEventAttrs)),
                ReadBstr(native->szActorID));
        }
        return result;
    }

    private static string? ReadBstr(IntPtr value) =>
        value == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(value);

    private static OpcVariant[] ReadVariants(IntPtr values, int count)
    {
        if (count == 0)
        {
            return [];
        }
        var result = new OpcVariant[count];
        int variantSize = IntPtr.Size == 8 ? 24 : 16;
        for (int i = 0; i < count; i++)
        {
            result[i] = ReadVariant(values + (i * variantSize));
        }
        return result;
    }

    private static OpcVariant ReadVariant(IntPtr value)
    {
        ushort vt = unchecked((ushort)Marshal.ReadInt16(value));
        IntPtr payload = value + VariantValueOffset;
        return vt switch
        {
            VtEmpty => OpcVariant.Empty,
            VtNull => OpcVariant.Null,
            VtI4 => OpcVariant.FromInt32(Marshal.ReadInt32(payload)),
            VtUi4 => OpcVariant.FromUInt32(unchecked((uint)Marshal.ReadInt32(payload))),
            VtR8 => OpcVariant.FromDouble(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(payload))),
            VtBool => OpcVariant.FromBoolean(Marshal.ReadInt16(payload) != 0),
            VtBstr => OpcVariant.FromString(ReadBstr(Marshal.ReadIntPtr(payload)) ?? string.Empty),
            _ => OpcVariant.Empty,
        };
    }

    private static IntPtr QueryInterface(IntPtr unknown, Guid iid)
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

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeOneEventStruct
    {
        public ushort wChangeMask;
        public ushort wNewState;
        public IntPtr szSource;
        public long ftTime;
        public IntPtr szMessage;
        public uint dwEventType;
        public uint dwEventCategory;
        public uint dwSeverity;
        public IntPtr szConditionName;
        public IntPtr szSubconditionName;
        public ushort wQuality;
        public ushort wReserved;
        public int bAckRequired;
        public long ftActiveTime;
        public uint dwCookie;
        public uint dwNumEventAttrs;
        public IntPtr pEventAttributes;
        public IntPtr szActorID;
    }

    private sealed class SinkStubSession
    {
        public SinkStubSession(IntPtr* vtable, bool supportsEventSink)
        {
            Vtable = vtable;
            SupportsEventSink = supportsEventSink;
        }

        public IntPtr* Vtable { get; }
        public bool SupportsEventSink { get; }
        public long RefCount = 1;
        public object Gate { get; } = new();
        public List<EventCallbackInvocation> Invocations { get; } = [];
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CreateEventSubscriptionDelegate(IntPtr pThis, int active, int bufferTime, int maxSize, int clientSubscription, ref Guid riid, out IntPtr ppUnk, IntPtr pRevisedBufferTime, IntPtr pRevisedMaxSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int FindConnectionPointDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppCp);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AdviseDelegate(IntPtr pThis, IntPtr pUnk, out uint pdwCookie);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int UnadviseDelegate(IntPtr pThis, uint dwCookie);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int RefreshDelegate(IntPtr pThis, int connection);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CancelRefreshDelegate(IntPtr pThis, int connection);
}
