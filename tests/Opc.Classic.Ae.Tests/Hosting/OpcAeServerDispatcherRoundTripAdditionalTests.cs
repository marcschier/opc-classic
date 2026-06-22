// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Ae.Tests.Hosting;

public sealed class OpcAeServerDispatcherRoundTripAdditionalTests
{
    [Test]
    public async Task DispatchAsync_GeneratedProxyRoundTripsCatalogMethods_ReturnsConcreteResults()
    {
        var server = new RecordingAeServer();
        IOpcAeServerDispatcher dispatcher = new OpcAeServerDispatcher(server);
        InMemoryCallChannel channel = CreateChannel(dispatcher);
        var proxy = new IOPCEventServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(CancellationToken.None);
        int filters = await proxy.QueryAvailableFiltersAsync(CancellationToken.None);
        await proxy.QueryEventCategoriesAsync(7, out int[] categories, out string[] descriptions, CancellationToken.None);
        string[] conditions = await proxy.QueryConditionNamesAsync(1001, CancellationToken.None);
        string[] subConditions = await proxy.QuerySubConditionNamesAsync("Level", CancellationToken.None);
        string[] sourceConditions = await proxy.QuerySourceConditionsAsync("Plant.Area.Tank7", CancellationToken.None);
        await proxy.QueryEventAttributesAsync(1001, out int[] attributeIds, out string[] attributeDescriptions, out ushort[] attributeTypes, CancellationToken.None);
        await proxy.TranslateToItemIDsAsync(
            "Plant.Area.Tank7",
            1001,
            "Level",
            "HiHi",
            [501, 502],
            out string[] itemIds,
            out string[] nodeNames,
            out Guid[] classIds,
            CancellationToken.None);

        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Ae);
        await Assert.That(status.VendorInfo).IsEqualTo("AE adapter round-trip");
        await Assert.That(filters).IsEqualTo(0x7);
        await Assert.That(categories).IsEquivalentTo([1001, 1002]);
        await Assert.That(descriptions).IsEquivalentTo(["Process", "System"]);
        await Assert.That(conditions).IsEquivalentTo(["Level", "Pressure"]);
        await Assert.That(subConditions).IsEquivalentTo(["Hi", "HiHi"]);
        await Assert.That(sourceConditions).IsEquivalentTo(["Level", "ValveFailure"]);
        await Assert.That(attributeIds).IsEquivalentTo([501, 502]);
        await Assert.That(attributeDescriptions).IsEquivalentTo(["Batch", "Limit"]);
        await Assert.That(attributeTypes).IsEquivalentTo([(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8]);
        await Assert.That(itemIds).IsEquivalentTo(["Plant.Area.Tank7.501", "Plant.Area.Tank7.502"]);
        await Assert.That(nodeNames).IsEquivalentTo(["Tank7", "Tank7"]);
        await Assert.That(classIds).IsEquivalentTo(server.TranslateClassIds);
        await Assert.That(server.LastEventType).IsEqualTo(7);
        await Assert.That(server.LastAttributeCategory).IsEqualTo(1001);
        await Assert.That(server.LastTranslateAttributeIds).IsEquivalentTo([501, 502]);
        await Assert.That(channel.CallLog.Select(static call => call.Opnum).ToArray()).IsEquivalentTo([
            IOPCEventServer.Opnums.GetStatusAsync,
            IOPCEventServer.Opnums.QueryAvailableFiltersAsync,
            IOPCEventServer.Opnums.QueryEventCategoriesAsync,
            IOPCEventServer.Opnums.QueryConditionNamesAsync,
            IOPCEventServer.Opnums.QuerySubConditionNamesAsync,
            IOPCEventServer.Opnums.QuerySourceConditionsAsync,
            IOPCEventServer.Opnums.QueryEventAttributesAsync,
            IOPCEventServer.Opnums.TranslateToItemIDsAsync]);
    }

    [Test]
    public async Task DispatchAsync_GeneratedProxyRoundTripsConditionMethods_RecordsInputsAndErrors()
    {
        var server = new RecordingAeServer();
        var dispatcher = new OpcAeServerDispatcher(server);
        InMemoryCallChannel channel = CreateChannel(dispatcher);
        var proxy = new IOPCEventServerClientProxy(channel);
        long activeTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).ToFileTime();

        OpcConditionState state = await proxy.GetConditionStateAsync("Plant.Area.Tank7", "Level", [501], CancellationToken.None);
        await proxy.EnableConditionByAreaAsync(["Plant.Area"], CancellationToken.None);
        await proxy.EnableConditionBySourceAsync(["Plant.Area.Tank7"], CancellationToken.None);
        await proxy.DisableConditionByAreaAsync(["Plant.Area.Disabled"], CancellationToken.None);
        await proxy.DisableConditionBySourceAsync(["Plant.Area.Pump9"], CancellationToken.None);
        int[] errors = await proxy.AckConditionAsync(
            2,
            "operator1",
            "acknowledged",
            ["Plant.Area.Tank7", "Plant.Area.Pump9"],
            ["Level", "Pressure"],
            [activeTime, activeTime + 1],
            [11, 12],
            CancellationToken.None);

        await Assert.That(state.ActiveSubCondition).IsEqualTo("HiHi");
        await Assert.That(state.EventAttributes).IsEquivalentTo([OpcVariant.FromString("Batch42")]);
        await Assert.That(server.LastConditionSource).IsEqualTo("Plant.Area.Tank7");
        await Assert.That(server.LastConditionAttributeIds).IsEquivalentTo([501]);
        await Assert.That(server.EnabledAreas).IsEquivalentTo(["Plant.Area"]);
        await Assert.That(server.EnabledSources).IsEquivalentTo(["Plant.Area.Tank7"]);
        await Assert.That(server.DisabledAreas).IsEquivalentTo(["Plant.Area.Disabled"]);
        await Assert.That(server.DisabledSources).IsEquivalentTo(["Plant.Area.Pump9"]);
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.InvalidArg.Code]);
        await Assert.That(server.LastAckActor).IsEqualTo("operator1");
        await Assert.That(server.LastAckCookies).IsEquivalentTo([11, 12]);
        await Assert.That(channel.CallLog.Select(static call => call.Opnum).ToArray()).IsEquivalentTo([
            IOPCEventServer.Opnums.GetConditionStateAsync,
            IOPCEventServer.Opnums.EnableConditionByAreaAsync,
            IOPCEventServer.Opnums.EnableConditionBySourceAsync,
            IOPCEventServer.Opnums.DisableConditionByAreaAsync,
            IOPCEventServer.Opnums.DisableConditionBySourceAsync,
            IOPCEventServer.Opnums.AckConditionAsync]);
    }

    [Test]
    public async Task DispatchAsync_UnknownInterface_ReturnsNotImplementedWithoutCallingServer()
    {
        var server = new RecordingAeServer();
        IOpcAeServerDispatcher dispatcher = new OpcAeServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            Guid.Parse("11111111-2222-3333-4444-555555555555"),
            IOPCEventServer.Opnums.QueryAvailableFiltersAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
        await Assert.That(server.QueryAvailableFiltersCallCount).IsEqualTo(0);
    }

    [Test]
    public async Task CreateEventSubscriptionAsync_FallbackAdapter_ManagesFilterRefreshAndDisposal()
    {
        var server = new RecordingAeServer();
        IOpcAeServerDispatcher dispatcher = new OpcAeServerDispatcher(server);

        IOPCEventSubscriptionMgt subscription = await dispatcher.CreateEventSubscriptionAsync(
            active: true,
            bufferTime: 50,
            maxSize: 2,
            clientSubscription: 0x777,
            requestedInterfaceId: IOPCEventSubscriptionMgt.InterfaceId,
            out int revisedBufferTime,
            out int revisedMaxSize,
            CancellationToken.None);
        var sink = new RecordingEventSink();
        int connection = await dispatcher.AdviseEventSinkAsync(subscription, sink, CancellationToken.None);

        await subscription.SetFilterAsync(
            (int)EventType.Condition,
            [1001, 1002],
            100,
            900,
            ["Plant.Area"],
            ["Plant.Area.Tank7"],
            CancellationToken.None);
        await subscription.GetFilterAsync(
            out int eventType,
            out int[] eventCategories,
            out int lowSeverity,
            out int highSeverity,
            out string[] areas,
            out string[] sources,
            CancellationToken.None);
        await subscription.SetReturnedAttributesAsync(1001, [501, 502], CancellationToken.None);
        int[] returnedAttributes = await subscription.GetReturnedAttributesAsync(1001, CancellationToken.None);
        await subscription.SetStateAsync(false, 75, 3, 0x888, out int revisedSetBufferTime, out int revisedSetMaxSize, CancellationToken.None);
        await subscription.GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken.None);
        await subscription.RefreshAsync(connection, CancellationToken.None);
        await subscription.CancelRefreshAsync(connection, CancellationToken.None);
        await dispatcher.UnadviseEventSinkAsync(subscription, connection, CancellationToken.None);
        await dispatcher.RemoveSubscriptionAsync(subscription, CancellationToken.None);

        await Assert.That(revisedBufferTime).IsEqualTo(50);
        await Assert.That(revisedMaxSize).IsEqualTo(2);
        await Assert.That(eventType).IsEqualTo((int)EventType.Condition);
        await Assert.That(eventCategories).IsEquivalentTo([1001, 1002]);
        await Assert.That(lowSeverity).IsEqualTo(100);
        await Assert.That(highSeverity).IsEqualTo(900);
        await Assert.That(areas).IsEquivalentTo(["Plant.Area"]);
        await Assert.That(sources).IsEquivalentTo(["Plant.Area.Tank7"]);
        await Assert.That(returnedAttributes).IsEquivalentTo([501, 502]);
        await Assert.That(revisedSetBufferTime).IsEqualTo(75);
        await Assert.That(revisedSetMaxSize).IsEqualTo(3);
        await Assert.That(active).IsFalse();
        await Assert.That(bufferTime).IsEqualTo(75);
        await Assert.That(maxSize).IsEqualTo(3);
        await Assert.That(clientSubscription).IsEqualTo(0x888);
        await Assert.That(server.CreatedSubscription).IsNotNull();
        await Assert.That(server.CreatedSubscription!.RefreshCalls).IsEqualTo(1);
        await Assert.That(server.CreatedSubscription.CancelRefreshCalls).IsEqualTo(1);
        await Assert.That(server.CreatedSubscription.Disposed).IsTrue();
        await Assert.That(sink.LastClientSubscription).IsEqualTo(0x888);
        await Assert.That(sink.LastRefresh).IsTrue();
        await Assert.That(sink.LastRefreshComplete).IsTrue();
        await Assert.That(sink.LastEvents).IsEmpty();
    }

    private static InMemoryCallChannel CreateChannel(IOpcAeServerDispatcher dispatcher) =>
        new((iid, opnum, payload, cancellationToken) =>
            dispatcher.DispatchAsync(iid, opnum, payload, cancellationToken));

    private static OpcServerStatus CreateStatus() => new()
    {
        Spec = OpcStatusSpec.Ae,
        StartTime = DateTimeOffset.UnixEpoch,
        CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(5),
        LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(6),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 10, 7),
        VendorInfo = "AE adapter round-trip",
    };

    private sealed class RecordingAeServer : IOpcAeServer, IAeServer
    {
        public Guid[] TranslateClassIds { get; } =
            [Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), Guid.Parse("ffffffff-1111-2222-3333-444444444444")];

        public int LastEventType { get; private set; }
        public int LastAttributeCategory { get; private set; }
        public string? LastConditionSource { get; private set; }
        public int[] LastConditionAttributeIds { get; private set; } = [];
        public int[] LastTranslateAttributeIds { get; private set; } = [];
        public string[] EnabledAreas { get; private set; } = [];
        public string[] EnabledSources { get; private set; } = [];
        public string[] DisabledAreas { get; private set; } = [];
        public string[] DisabledSources { get; private set; } = [];
        public string? LastAckActor { get; private set; }
        public int[] LastAckCookies { get; private set; } = [];
        public int QueryAvailableFiltersCallCount { get; private set; }
        public RecordingAeSubscription? CreatedSubscription { get; private set; }

        public event EventHandler<EventArgs>? ServerShutdown
        {
            add { }
            remove { }
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateStatus());

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
        {
            QueryAvailableFiltersCallCount++;
            return Task.FromResult(0x7);
        }

        public Task CreateEventSubscriptionAsync(
            bool active,
            int bufferTime,
            int maxSize,
            int clientSubscription,
            Guid requestedInterfaceId,
            out IOPCEventSubscriptionMgt subscription,
            out int revisedBufferTime,
            out int revisedMaxSize,
            CancellationToken cancellationToken = default)
        {
            _ = active;
            _ = bufferTime;
            _ = maxSize;
            _ = clientSubscription;
            _ = requestedInterfaceId;
            subscription = default!;
            revisedBufferTime = 0;
            revisedMaxSize = 0;
            throw new OpcException(OpcResultId.NotImplemented);
        }

        public Task QueryEventCategoriesAsync(
            int eventType,
            out int[] eventCategories,
            out string[] eventCategoryDescriptions,
            CancellationToken cancellationToken = default)
        {
            LastEventType = eventType;
            eventCategories = [1001, 1002];
            eventCategoryDescriptions = ["Process", "System"];
            return Task.CompletedTask;
        }

        public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            _ = eventCategory;
            return Task.FromResult(new[] { "Level", "Pressure" });
        }

        public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
        {
            _ = conditionName;
            return Task.FromResult(new[] { "Hi", "HiHi" });
        }

        public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
        {
            _ = source;
            return Task.FromResult(new[] { "Level", "ValveFailure" });
        }

        public Task QueryEventAttributesAsync(
            int eventCategory,
            out int[] attributeIds,
            out string[] attributeDescriptions,
            out ushort[] attributeTypes,
            CancellationToken cancellationToken = default)
        {
            LastAttributeCategory = eventCategory;
            attributeIds = [501, 502];
            attributeDescriptions = ["Batch", "Limit"];
            attributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
            return Task.CompletedTask;
        }

        public Task TranslateToItemIDsAsync(
            string source,
            int eventCategory,
            string conditionName,
            string subconditionName,
            int[] associatedAttributeIds,
            out string[] attributeItemIds,
            out string[] nodeNames,
            out Guid[] classIds,
            CancellationToken cancellationToken = default)
        {
            _ = eventCategory;
            _ = conditionName;
            _ = subconditionName;
            LastTranslateAttributeIds = associatedAttributeIds;
            attributeItemIds = associatedAttributeIds.Select(id => source + "." + id).ToArray();
            nodeNames = associatedAttributeIds.Select(static _ => "Tank7").ToArray();
            classIds = TranslateClassIds;
            return Task.CompletedTask;
        }

        public Task<OpcConditionState> GetConditionStateAsync(
            string source,
            string conditionName,
            int[] attributeIds,
            CancellationToken cancellationToken = default)
        {
            LastConditionSource = source;
            LastConditionAttributeIds = attributeIds;
            return Task.FromResult(new OpcConditionState(
                state: (ushort)(ConditionState.Active | ConditionState.Enabled),
                activeSubCondition: "HiHi",
                activeSubConditionDefinition: "High-high",
                activeSubConditionSeverity: 900,
                activeSubConditionDescription: "High-high active",
                quality: OpcQuality.Good,
                lastAckTime: DateTimeOffset.UnixEpoch,
                subConditionLastActive: DateTimeOffset.UnixEpoch.AddSeconds(1),
                conditionLastActive: DateTimeOffset.UnixEpoch.AddSeconds(2),
                conditionLastInactive: DateTimeOffset.UnixEpoch.AddSeconds(3),
                acknowledgerId: "operator0",
                comment: "previous",
                subConditionNames: ["Hi", "HiHi"],
                subConditionDefinitions: ["High", "High-high"],
                subConditionSeverities: [700, 900],
                subConditionDescriptions: ["High active", "High-high active"],
                        eventAttributes: [OpcVariant.FromString("Batch42")],
                errors: [OpcResultId.Ok.Code]));
        }

        public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
        {
            EnabledAreas = areas;
            return Task.CompletedTask;
        }

        public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
        {
            EnabledSources = sources;
            return Task.CompletedTask;
        }

        public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
        {
            DisabledAreas = areas;
            return Task.CompletedTask;
        }

        public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
        {
            DisabledSources = sources;
            return Task.CompletedTask;
        }

        public Task<int[]> AckConditionAsync(
            int dwCount,
            string acknowledgerId,
            string comment,
            string[] sources,
            string[] conditionNames,
            long[] activeTimes,
            int[] cookies,
            CancellationToken cancellationToken = default)
        {
            _ = dwCount;
            _ = comment;
            _ = sources;
            _ = conditionNames;
            _ = activeTimes;
            LastAckActor = acknowledgerId;
            LastAckCookies = cookies;
            return Task.FromResult(new[] { OpcResultId.Ok.Code, OpcResultId.InvalidArg.Code });
        }

        public Task CreateAreaBrowserAsync(
            Guid requestedInterfaceId,
            out IOPCEventAreaBrowser areaBrowser,
            CancellationToken cancellationToken = default)
        {
            _ = requestedInterfaceId;
            areaBrowser = default!;
            throw new OpcException(OpcResultId.NotImplemented);
        }

        public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
            string areaQualifiedName,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = areaQualifiedName;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield break;
        }

        public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(
            EventType eventTypes,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<uint>>([1001u, 1002u]);

        public Task<IReadOnlyList<string>> QueryConditionNamesAsync(
            uint eventCategory,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>(["Level", "Pressure"]);

        public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
            string actor,
            string? comment,
            IReadOnlyList<ConditionRef> conditions,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AckResult>>(conditions
                .Select(condition => new AckResult { Condition = condition, ResultId = OpcResultId.Ok })
                .ToArray());

        public Task<OpcResultId> EnableConditionsByAreaAsync(
            IReadOnlyList<string> areaQualifiedNames,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OpcResultId.Ok);

        public Task<OpcResultId> DisableConditionsByAreaAsync(
            IReadOnlyList<string> areaQualifiedNames,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OpcResultId.Ok);

        public Task<IAeSubscription> CreateSubscriptionAsync(
            bool active,
            int bufferTimeMs,
            int maxBufferSize,
            CancellationToken cancellationToken = default)
        {
            CreatedSubscription = new RecordingAeSubscription(active);
            return Task.FromResult<IAeSubscription>(CreatedSubscription);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class RecordingAeSubscription : IAeSubscription
    {
        public RecordingAeSubscription(bool active) => Active = active;

        public bool Active { get; private set; }
        public SubscriptionFilter Filter { get; private set; } = new();
        public IAsyncEnumerable<EventNotification> Events => ReadEventsAsync();
        public int RefreshCalls { get; private set; }
        public int CancelRefreshCalls { get; private set; }
        public bool Disposed { get; private set; }

        public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
        {
            Active = active;
            return Task.CompletedTask;
        }

        public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default)
        {
            Filter = filter;
            return Task.CompletedTask;
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            RefreshCalls++;
            return Task.CompletedTask;
        }

        public Task CancelRefreshAsync(CancellationToken cancellationToken = default)
        {
            CancelRefreshCalls++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }

        private static async IAsyncEnumerable<EventNotification> ReadEventsAsync()
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class RecordingEventSink : IOPCEventSink
    {
        public int LastClientSubscription { get; private set; }
        public bool LastRefresh { get; private set; }
        public bool LastRefreshComplete { get; private set; }
        public OpcEventNotification[] LastEvents { get; private set; } = [];

        public Task OnEventAsync(
            int clientSubscription,
            bool refresh,
            bool lastRefresh,
            OpcEventNotification[] events,
            CancellationToken cancellationToken = default)
        {
            LastClientSubscription = clientSubscription;
            LastRefresh = refresh;
            LastRefreshComplete = lastRefresh;
            LastEvents = events;
            return Task.CompletedTask;
        }
    }
}
