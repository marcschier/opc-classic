//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Ae.Dcom;
using Opc.Classic.Hosting;

namespace Opc.Classic.Ae.Tests;

/// <summary>
/// End-to-end dispatch tests that wire the generated client proxy against the
/// generated server dispatcher directly (no transport), proving the wire format
/// is symmetric. Reproduces the matrix failures for GetConditionState and
/// AckCondition without spinning up the sample server.
/// </summary>
public sealed class AeEndToEndDispatchTests
{
    [Test]
    public async Task GetConditionState_with_empty_attribute_ids_round_trips_through_dispatcher()
    {
        var impl = new StubEventServer();
        var dispatcher = new IOPCEventServerServerDispatcher(impl);
        var channel = new InProcessChannel(dispatcher);
        var proxy = new IOPCEventServerClientProxy(channel);

        OpcConditionState state = await proxy.GetConditionStateAsync("Random.Int4", "Condition", [], CancellationToken.None);

        await Assert.That(state).IsNotNull();
        await Assert.That(impl.LastSource).IsEqualTo("Random.Int4");
        await Assert.That(impl.LastConditionName).IsEqualTo("Condition");
        await Assert.That(impl.LastAttributeIds).IsNotNull();
        await Assert.That(impl.LastAttributeIds!.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AckCondition_with_one_event_round_trips_through_dispatcher()
    {
        var impl = new StubEventServer();
        var dispatcher = new IOPCEventServerServerDispatcher(impl);
        var channel = new InProcessChannel(dispatcher);
        var proxy = new IOPCEventServerClientProxy(channel);

        long activeFt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToFileTime();
        int[] errors = await proxy.AckConditionAsync(
            1,
            "operator1",
            "comment",
            ["Random.Int4"],
            ["Condition"],
            [activeFt],
            [42],
            CancellationToken.None);

        await Assert.That(errors).IsNotNull();
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(impl.LastAckActor).IsEqualTo("operator1");
        await Assert.That(impl.LastAckSources!.Length).IsEqualTo(1);
        await Assert.That(impl.LastAckCookies![0]).IsEqualTo(42);
    }

    private sealed class StubEventServer : IOPCEventServer
    {
        public string? LastSource { get; private set; }
        public string? LastConditionName { get; private set; }
        public int[]? LastAttributeIds { get; private set; }
        public string? LastAckActor { get; private set; }
        public string[]? LastAckSources { get; private set; }
        public int[]? LastAckCookies { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task CreateEventSubscriptionAsync(bool active, int bufferTime, int maxSize, int clientSubscription, Guid requestedInterfaceId, out IOPCEventSubscriptionMgt subscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default) =>
            throw NotImplementedException(out subscription, out revisedBufferTime, out revisedMaxSize);

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task QueryEventCategoriesAsync(int eventType, out int[] eventCategories, out string[] eventCategoryDescriptions, CancellationToken cancellationToken = default)
        {
            eventCategories = [];
            eventCategoryDescriptions = [];
            return Task.CompletedTask;
        }

        public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default) =>
            Task.FromResult<string[]>([]);

        public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default) =>
            Task.FromResult<string[]>([]);

        public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default) =>
            Task.FromResult<string[]>([]);

        public Task QueryEventAttributesAsync(int eventCategory, out int[] attributeIds, out string[] attributeDescriptions, out ushort[] attributeTypes, CancellationToken cancellationToken = default)
        {
            attributeIds = [];
            attributeDescriptions = [];
            attributeTypes = [];
            return Task.CompletedTask;
        }

        public Task TranslateToItemIDsAsync(string source, int eventCategory, string conditionName, string subConditionName, int[] attributeIds, out string[] attributeItemIds, out string[] nodeNames, out Guid[] classIds, CancellationToken cancellationToken = default)
        {
            attributeItemIds = [];
            nodeNames = [];
            classIds = [];
            return Task.CompletedTask;
        }

        public Task<OpcConditionState> GetConditionStateAsync(string source, string conditionName, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            LastSource = source;
            LastConditionName = conditionName;
            LastAttributeIds = attributeIds;
            var now = DateTimeOffset.UtcNow;
            var state = new OpcConditionState(
                state: 0x0003,
                activeSubCondition: "Active",
                activeSubConditionDefinition: "Stub",
                activeSubConditionSeverity: 500,
                activeSubConditionDescription: "Stub active",
                quality: OpcQuality.Good,
                lastAckTime: now,
                subConditionLastActive: now,
                conditionLastActive: now,
                conditionLastInactive: now,
                acknowledgerId: null,
                comment: null,
                subConditionNames: ["Active"],
                subConditionDefinitions: ["Stub"],
                subConditionSeverities: [500],
                subConditionDescriptions: ["Stub active"],
                eventAttributes: [],
                errors: []);
            return Task.FromResult(state);
        }

        public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<int[]> AckConditionAsync(int dwCount, string acknowledgerId, string comment, string[] sources, string[] conditionNames, long[] activeTimes, int[] cookies, CancellationToken cancellationToken = default)
        {
            LastAckActor = acknowledgerId;
            LastAckSources = sources;
            LastAckCookies = cookies;
            var errors = new int[dwCount];
            return Task.FromResult(errors);
        }

        public Task CreateAreaBrowserAsync(Guid requestedInterfaceId, out IOPCEventAreaBrowser areaBrowser, CancellationToken cancellationToken = default)
        {
            areaBrowser = default!;
            throw new NotImplementedException();
        }

        private static NotImplementedException NotImplementedException<T>(out T v) { v = default!; return new NotImplementedException(); }
        private static NotImplementedException NotImplementedException<T1, T2, T3>(out T1 v1, out T2 v2, out T3 v3) { v1 = default!; v2 = default!; v3 = default!; return new NotImplementedException(); }
    }

    private sealed class InProcessChannel : ICallChannel
    {
        private readonly IOPCEventServerServerDispatcher _dispatcher;

        public InProcessChannel(IOPCEventServerServerDispatcher dispatcher) => _dispatcher = dispatcher;

        public async Task<NdrCallResult> InvokeAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken = default)
        {
            DispatchResult result = await _dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false);
            return result.ToNdrCallResult();
        }
    }
}
