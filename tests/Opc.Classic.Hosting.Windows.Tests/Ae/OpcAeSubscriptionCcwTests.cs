// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeSubscriptionCcwTests
{
    private const int S_OK = 0;
    private const int E_NOTIMPL = unchecked((int)0x80004001);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task CreateEventSubscription_returns_subscription_pointer_from_dispatcher()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);

        Helpers.CreateSubscriptionResult result = Helpers.InvokeCreateEventSubscription(eventServer, active: true, bufferTime: 250, maxSize: 11, clientSubscription: 0x1234);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Subscription).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(result.RevisedBufferTime).IsEqualTo(260);
        await Assert.That(result.RevisedMaxSize).IsEqualTo(31);
        await Assert.That(dispatcher.LastRequestedInterfaceId).IsEqualTo(IOPCEventSubscriptionMgt.InterfaceId);
        await Assert.That(dispatcher.LastClientSubscription).IsEqualTo(0x1234);
    }

    [Test]
    public async Task SetFilter_and_GetFilter_round_trip_native_arrays()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr subscription = Helpers.CreateSubscription(dispatcher);

        int setHr = Helpers.InvokeSetFilter(subscription, eventType: 0x5, categories: [1001, 1002], lowSeverity: 100, highSeverity: 900, areas: ["Plant1.AreaA"], sources: ["Plant1.AreaA.Tank7"]);
        Helpers.FilterResult filter = Helpers.InvokeGetFilter(subscription);

        await Assert.That(setHr).IsEqualTo(S_OK);
        await Assert.That(dispatcher.EventType).IsEqualTo(0x5);
        await Assert.That(dispatcher.EventCategories).IsEquivalentTo([1001, 1002]);
        await Assert.That(dispatcher.LowSeverity).IsEqualTo(100);
        await Assert.That(dispatcher.HighSeverity).IsEqualTo(900);
        await Assert.That(dispatcher.Areas).IsEquivalentTo(["Plant1.AreaA"]);
        await Assert.That(dispatcher.Sources).IsEquivalentTo(["Plant1.AreaA.Tank7"]);
        await Assert.That(filter.Hr).IsEqualTo(S_OK);
        await Assert.That(filter.EventType).IsEqualTo(0x5);
        await Assert.That(filter.EventCategories).IsEquivalentTo([1001, 1002]);
        await Assert.That(filter.LowSeverity).IsEqualTo(100);
        await Assert.That(filter.HighSeverity).IsEqualTo(900);
        await Assert.That(filter.Areas).IsEquivalentTo(["Plant1.AreaA"]);
        await Assert.That(filter.Sources).IsEquivalentTo(["Plant1.AreaA.Tank7"]);
    }

    [Test]
    public async Task SelectReturnedAttributes_and_GetReturnedAttributes_round_trip_attribute_ids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr subscription = Helpers.CreateSubscription(dispatcher);

        int setHr = Helpers.InvokeSelectReturnedAttributes(subscription, eventCategory: 1001, attributeIds: [3, 5, 8]);
        Helpers.IntArrayResult getResult = Helpers.InvokeGetReturnedAttributes(subscription, eventCategory: 1001);

        await Assert.That(setHr).IsEqualTo(S_OK);
        await Assert.That(dispatcher.LastAttributeCategory).IsEqualTo(1001);
        await Assert.That(dispatcher.ReturnedAttributes).IsEquivalentTo([3, 5, 8]);
        await Assert.That(getResult.Hr).IsEqualTo(S_OK);
        await Assert.That(getResult.Values).IsEquivalentTo([3, 5, 8]);
    }

    [Test]
    public async Task Refresh_dispatches_and_fires_event_sink_callback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr subscription = Helpers.CreateSubscription(dispatcher);

        int hr = Helpers.InvokeRefresh(subscription, connection: 77);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(dispatcher.LastRefreshConnection).IsEqualTo(77);
        await Assert.That(dispatcher.EventSink.CallCount).IsEqualTo(1);
        await Assert.That(dispatcher.EventSink.LastClientSubscription).IsEqualTo(dispatcher.ClientSubscription);
        await Assert.That(dispatcher.EventSink.LastRefresh).IsTrue();
        await Assert.That(dispatcher.EventSink.LastRefreshComplete).IsTrue();
        await Assert.That(dispatcher.EventSink.LastEventCount).IsEqualTo(1);
    }

    [Test]
    public async Task CancelRefresh_dispatches_connection_cookie()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr subscription = Helpers.CreateSubscription(dispatcher);

        int hr = Helpers.InvokeCancelRefresh(subscription, connection: 88);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(dispatcher.LastCancelRefreshConnection).IsEqualTo(88);
    }

    [Test]
    public async Task GetState_returns_active_buffering_and_client_handle()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);
        Helpers.CreateSubscriptionResult createResult = Helpers.InvokeCreateEventSubscription(eventServer, active: false, bufferTime: 500, maxSize: 44, clientSubscription: 0xCAFE);
        IntPtr subscription = createResult.Subscription;

        Helpers.SubscriptionStateResult result = Helpers.InvokeGetState(subscription);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Active).IsEqualTo(0);
        await Assert.That(result.BufferTime).IsEqualTo(500);
        await Assert.That(result.MaxSize).IsEqualTo(44);
        await Assert.That(result.ClientSubscription).IsEqualTo(0xCAFE);
    }

    [Test]
    public async Task SetState_updates_state_and_returns_revised_buffering()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr subscription = Helpers.CreateSubscription(dispatcher);

        Helpers.SetStateResult result = Helpers.InvokeSetState(subscription, active: false, bufferTime: 700, maxSize: 9, clientSubscription: 0xBEEF);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.RevisedBufferTime).IsEqualTo(705);
        await Assert.That(result.RevisedMaxSize).IsEqualTo(16);
        await Assert.That(dispatcher.Active).IsFalse();
        await Assert.That(dispatcher.BufferTime).IsEqualTo(700);
        await Assert.That(dispatcher.MaxSize).IsEqualTo(9);
        await Assert.That(dispatcher.ClientSubscription).IsEqualTo(0xBEEF);
    }

    [Test]
    public async Task Release_removes_subscription_through_dispatcher()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr subscription = Helpers.CreateSubscription(dispatcher);

        uint refCount = Helpers.InvokeRelease(subscription);

        await Assert.That(refCount).IsEqualTo(0u);
        await Assert.That(OpcAeSubscriptionCcw.GetReferenceCount(subscription)).IsEqualTo(-1L);
        await Assert.That(dispatcher.RemoveSubscriptionCallCount).IsEqualTo(1);
        await Assert.That(dispatcher.RemovedSubscription).IsEqualTo(dispatcher);
    }

    private sealed class StubAeServerDispatcher : IOpcAeServerDispatcher, IOPCEventSubscriptionMgt
    {
        public Guid LastRequestedInterfaceId { get; private set; }
        public bool LastActive { get; private set; }
        public int LastClientSubscription { get; private set; }
        public int EventType { get; private set; } = 0x1F;
        public int[] EventCategories { get; private set; } = [1, 2];
        public int LowSeverity { get; private set; }
        public int HighSeverity { get; private set; } = 1000;
        public string[] Areas { get; private set; } = [];
        public string[] Sources { get; private set; } = [];
        public int LastAttributeCategory { get; private set; }
        public int[] ReturnedAttributes { get; private set; } = [];
        public int LastRefreshConnection { get; private set; }
        public int LastCancelRefreshConnection { get; private set; }
        public bool Active { get; set; } = true;
        public int BufferTime { get; set; } = 100;
        public int MaxSize { get; set; } = 10;
        public int ClientSubscription { get; set; } = 0xAA01;
        public int RemoveSubscriptionCallCount { get; private set; }
        public IOPCEventSubscriptionMgt? RemovedSubscription { get; private set; }
        public RecordingEventSink EventSink { get; } = new();

        public Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            Task.FromResult(new NdrCallResult(E_NOTIMPL, ReadOnlyMemory<byte>.Empty));

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
            LastActive = active;
            Active = active;
            BufferTime = bufferTime;
            MaxSize = maxSize;
            ClientSubscription = clientSubscription;
            LastClientSubscription = clientSubscription;
            LastRequestedInterfaceId = requestedInterfaceId;
            revisedBufferTime = bufferTime + 10;
            revisedMaxSize = maxSize + 20;
            return Task.FromResult<IOPCEventSubscriptionMgt>(this);
        }

        public Task RemoveSubscriptionAsync(IOPCEventSubscriptionMgt subscription, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RemoveSubscriptionCallCount++;
            RemovedSubscription = subscription;
            return Task.CompletedTask;
        }

        public Task SetFilterAsync(int eventType, int[] eventCategories, int lowSeverity, int highSeverity, string[] areas, string[] sources, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EventType = eventType;
            EventCategories = eventCategories;
            LowSeverity = lowSeverity;
            HighSeverity = highSeverity;
            Areas = areas;
            Sources = sources;
            return Task.CompletedTask;
        }

        public Task GetFilterAsync(out int eventType, out int[] eventCategories, out int lowSeverity, out int highSeverity, out string[] areas, out string[] sources, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            eventType = EventType;
            eventCategories = EventCategories;
            lowSeverity = LowSeverity;
            highSeverity = HighSeverity;
            areas = Areas;
            sources = Sources;
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastAttributeCategory = eventCategory;
            ReturnedAttributes = attributeIds;
            return Task.CompletedTask;
        }

        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastAttributeCategory = eventCategory;
            return Task.FromResult(ReturnedAttributes);
        }

        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRefreshConnection = connection;
            return EventSink.OnEventAsync(
                ClientSubscription,
                refresh: true,
                lastRefresh: true,
                [new OpcEventNotification(0, 0, "Plant1.AreaA.Tank7", DateTimeOffset.UnixEpoch, "Refresh", 1, 1, 500, null, null, OpcQuality.Good, false, DateTimeOffset.UnixEpoch, 0, [], null)],
                cancellationToken);
        }

        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastCancelRefreshConnection = connection;
            return Task.CompletedTask;
        }

        public Task GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            active = Active;
            bufferTime = BufferTime;
            maxSize = MaxSize;
            clientSubscription = ClientSubscription;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(bool active, int bufferTime, int maxSize, int clientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Active = active;
            BufferTime = bufferTime;
            MaxSize = maxSize;
            ClientSubscription = clientSubscription;
            revisedBufferTime = bufferTime + 5;
            revisedMaxSize = maxSize + 7;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingEventSink : IOPCEventSink
    {
        public int CallCount { get; private set; }
        public int LastClientSubscription { get; private set; }
        public bool LastRefresh { get; private set; }
        public bool LastRefreshComplete { get; private set; }
        public int LastEventCount { get; private set; }

        public Task OnEventAsync(int clientSubscription, bool refresh, bool lastRefresh, OpcEventNotification[] events, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            LastClientSubscription = clientSubscription;
            LastRefresh = refresh;
            LastRefreshComplete = lastRefresh;
            LastEventCount = events.Length;
            return Task.CompletedTask;
        }
    }

    private static class Helpers
    {
        internal readonly record struct CreateSubscriptionResult(int Hr, IntPtr Subscription, int RevisedBufferTime, int RevisedMaxSize);
        internal readonly record struct FilterResult(int Hr, int EventType, int[] EventCategories, int LowSeverity, int HighSeverity, string[] Areas, string[] Sources);
        internal readonly record struct IntArrayResult(int Hr, int[] Values);
        internal readonly record struct SubscriptionStateResult(int Hr, int Active, int BufferTime, int MaxSize, int ClientSubscription);
        internal readonly record struct SetStateResult(int Hr, int RevisedBufferTime, int RevisedMaxSize);

        internal static IntPtr CreateEventServer(IOpcAeServerDispatcher dispatcher)
        {
            IntPtr ccw = OpcAeServerCcw.Create(dispatcher, IID_IUnknown);
            return InvokeQI(ccw, IOPCEventServer.InterfaceId);
        }

        internal static IntPtr CreateSubscription(IOpcAeServerDispatcher dispatcher)
        {
            IntPtr eventServer = CreateEventServer(dispatcher);
            CreateSubscriptionResult result = InvokeCreateEventSubscription(eventServer, active: true, bufferTime: 100, maxSize: 10, clientSubscription: 0xAA01);
            if (result.Hr != S_OK)
            {
                throw new InvalidOperationException($"CreateEventSubscription failed with 0x{result.Hr:X8}.");
            }
            return result.Subscription;
        }

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static CreateSubscriptionResult InvokeCreateEventSubscription(IntPtr eventServer, bool active, int bufferTime, int maxSize, int clientSubscription)
        {
            CreateEventSubscriptionDelegate create = GetMethod<CreateEventSubscriptionDelegate>(eventServer, 4);
            IntPtr pRevisedBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pRevisedMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            Guid iid = IOPCEventSubscriptionMgt.InterfaceId;
            try
            {
                int hr = create(eventServer, active ? 1 : 0, bufferTime, maxSize, clientSubscription, ref iid, out IntPtr subscription, pRevisedBufferTime, pRevisedMaxSize);
                return new CreateSubscriptionResult(hr, subscription, Marshal.ReadInt32(pRevisedBufferTime), Marshal.ReadInt32(pRevisedMaxSize));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pRevisedBufferTime);
                Marshal.FreeCoTaskMem(pRevisedMaxSize);
            }
        }

        internal static int InvokeSetFilter(IntPtr subscription, int eventType, int[] categories, int lowSeverity, int highSeverity, string[] areas, string[] sources)
        {
            SetFilterDelegate setFilter = GetMethod<SetFilterDelegate>(subscription, 3);
            IntPtr categoryPtr = AllocateInt32Array(categories);
            IntPtr areaPtr = AllocateStringPointerArray(areas);
            IntPtr sourcePtr = AllocateStringPointerArray(sources);
            try
            {
                return setFilter(subscription, eventType, categories.Length, categoryPtr, lowSeverity, highSeverity, areas.Length, areaPtr, sources.Length, sourcePtr);
            }
            finally
            {
                FreeCoTaskMem(categoryPtr);
                FreeStringPointerArray(areaPtr, areas.Length);
                FreeStringPointerArray(sourcePtr, sources.Length);
            }
        }

        internal static FilterResult InvokeGetFilter(IntPtr subscription)
        {
            GetFilterDelegate getFilter = GetMethod<GetFilterDelegate>(subscription, 4);
            IntPtr pEventType = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pCategoryCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pLowSeverity = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pHighSeverity = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pAreaCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pSourceCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr categoriesPtr = IntPtr.Zero;
            IntPtr areasPtr = IntPtr.Zero;
            IntPtr sourcesPtr = IntPtr.Zero;
            try
            {
                int hr = getFilter(subscription, pEventType, pCategoryCount, out categoriesPtr, pLowSeverity, pHighSeverity, pAreaCount, out areasPtr, pSourceCount, out sourcesPtr);
                int categoryCount = Marshal.ReadInt32(pCategoryCount);
                int areaCount = Marshal.ReadInt32(pAreaCount);
                int sourceCount = Marshal.ReadInt32(pSourceCount);
                return new FilterResult(
                    hr,
                    Marshal.ReadInt32(pEventType),
                    ReadInt32Array(categoriesPtr, categoryCount),
                    Marshal.ReadInt32(pLowSeverity),
                    Marshal.ReadInt32(pHighSeverity),
                    ReadStringPointerArray(areasPtr, areaCount),
                    ReadStringPointerArray(sourcesPtr, sourceCount));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pEventType);
                Marshal.FreeCoTaskMem(pCategoryCount);
                Marshal.FreeCoTaskMem(pLowSeverity);
                Marshal.FreeCoTaskMem(pHighSeverity);
                Marshal.FreeCoTaskMem(pAreaCount);
                Marshal.FreeCoTaskMem(pSourceCount);
                FreeCoTaskMem(categoriesPtr);
                FreeStringPointerArray(areasPtr, CountStringPointers(areasPtr));
                FreeStringPointerArray(sourcesPtr, CountStringPointers(sourcesPtr));
            }
        }

        internal static int InvokeSelectReturnedAttributes(IntPtr subscription, int eventCategory, int[] attributeIds)
        {
            SelectReturnedAttributesDelegate select = GetMethod<SelectReturnedAttributesDelegate>(subscription, 5);
            IntPtr attributesPtr = AllocateInt32Array(attributeIds);
            try
            {
                return select(subscription, eventCategory, attributeIds.Length, attributesPtr);
            }
            finally
            {
                FreeCoTaskMem(attributesPtr);
            }
        }

        internal static IntArrayResult InvokeGetReturnedAttributes(IntPtr subscription, int eventCategory)
        {
            GetReturnedAttributesDelegate get = GetMethod<GetReturnedAttributesDelegate>(subscription, 6);
            IntPtr pCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr attributesPtr = IntPtr.Zero;
            try
            {
                int hr = get(subscription, eventCategory, pCount, out attributesPtr);
                int count = Marshal.ReadInt32(pCount);
                return new IntArrayResult(hr, ReadInt32Array(attributesPtr, count));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pCount);
                FreeCoTaskMem(attributesPtr);
            }
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

        internal static SubscriptionStateResult InvokeGetState(IntPtr subscription)
        {
            GetStateDelegate getState = GetMethod<GetStateDelegate>(subscription, 9);
            IntPtr pActive = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pClientSubscription = Marshal.AllocCoTaskMem(sizeof(int));
            try
            {
                int hr = getState(subscription, pActive, pBufferTime, pMaxSize, pClientSubscription);
                return new SubscriptionStateResult(hr, Marshal.ReadInt32(pActive), Marshal.ReadInt32(pBufferTime), Marshal.ReadInt32(pMaxSize), Marshal.ReadInt32(pClientSubscription));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pActive);
                Marshal.FreeCoTaskMem(pBufferTime);
                Marshal.FreeCoTaskMem(pMaxSize);
                Marshal.FreeCoTaskMem(pClientSubscription);
            }
        }

        internal static SetStateResult InvokeSetState(IntPtr subscription, bool active, int bufferTime, int maxSize, int clientSubscription)
        {
            SetStateDelegate setState = GetMethod<SetStateDelegate>(subscription, 10);
            IntPtr pActive = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pRevisedBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pRevisedMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            try
            {
                Marshal.WriteInt32(pActive, active ? 1 : 0);
                Marshal.WriteInt32(pBufferTime, bufferTime);
                Marshal.WriteInt32(pMaxSize, maxSize);
                int hr = setState(subscription, pActive, pBufferTime, pMaxSize, clientSubscription, pRevisedBufferTime, pRevisedMaxSize);
                return new SetStateResult(hr, Marshal.ReadInt32(pRevisedBufferTime), Marshal.ReadInt32(pRevisedMaxSize));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pActive);
                Marshal.FreeCoTaskMem(pBufferTime);
                Marshal.FreeCoTaskMem(pMaxSize);
                Marshal.FreeCoTaskMem(pRevisedBufferTime);
                Marshal.FreeCoTaskMem(pRevisedMaxSize);
            }
        }

        internal static uint InvokeRelease(IntPtr subscription)
        {
            ReleaseDelegate release = GetMethod<ReleaseDelegate>(subscription, 2);
            return release(subscription);
        }

        private static IntPtr AllocateInt32Array(int[] values)
        {
            if (values.Length == 0)
            {
                return IntPtr.Zero;
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteInt32(ptr, i * sizeof(int), values[i]);
            }
            return ptr;
        }

        private static IntPtr AllocateStringPointerArray(string[] values)
        {
            if (values.Length == 0)
            {
                return IntPtr.Zero;
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * IntPtr.Size);
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteIntPtr(ptr, i * IntPtr.Size, Marshal.StringToBSTR(values[i]));
            }
            return ptr;
        }

        private static int[] ReadInt32Array(IntPtr ptr, int count)
        {
            if (count == 0)
            {
                return [];
            }

            var values = new int[count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Marshal.ReadInt32(ptr, i * sizeof(int));
            }
            return values;
        }

        private static string[] ReadStringPointerArray(IntPtr ptr, int count)
        {
            if (count == 0)
            {
                return [];
            }

            var values = new string[count];
            for (int i = 0; i < values.Length; i++)
            {
                IntPtr valuePtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                values[i] = Marshal.PtrToStringBSTR(valuePtr) ?? string.Empty;
            }
            return values;
        }

        private static int CountStringPointers(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return 0;
            }

            int count = 0;
            while (Marshal.ReadIntPtr(ptr, count * IntPtr.Size) != IntPtr.Zero)
            {
                count++;
            }
            return count;
        }

        private static void FreeCoTaskMem(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        private static void FreeStringPointerArray(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                IntPtr valuePtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                Marshal.FreeBSTR(valuePtr);
            }
            Marshal.FreeCoTaskMem(ptr);
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ReleaseDelegate(IntPtr pThis);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CreateEventSubscriptionDelegate(IntPtr pThis, int active, int bufferTime, int maxSize, int clientSubscription, ref Guid riid, out IntPtr ppUnk, IntPtr pRevisedBufferTime, IntPtr pRevisedMaxSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetFilterDelegate(IntPtr pThis, int eventType, int categoryCount, IntPtr eventCategories, int lowSeverity, int highSeverity, int areaCount, IntPtr areas, int sourceCount, IntPtr sources);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetFilterDelegate(IntPtr pThis, IntPtr pEventType, IntPtr pCategoryCount, out IntPtr ppEventCategories, IntPtr pLowSeverity, IntPtr pHighSeverity, IntPtr pAreaCount, out IntPtr ppAreaList, IntPtr pSourceCount, out IntPtr ppSourceList);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SelectReturnedAttributesDelegate(IntPtr pThis, int eventCategory, int count, IntPtr attributeIds);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetReturnedAttributesDelegate(IntPtr pThis, int eventCategory, IntPtr pCount, out IntPtr ppAttributeIds);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int RefreshDelegate(IntPtr pThis, int connection);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CancelRefreshDelegate(IntPtr pThis, int connection);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetStateDelegate(IntPtr pThis, IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, IntPtr pClientSubscription);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetStateDelegate(IntPtr pThis, IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, int clientSubscription, IntPtr pRevisedBufferTime, IntPtr pRevisedMaxSize);
    }
}
