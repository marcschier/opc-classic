// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Channels = System.Threading.Channels;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F8AeHdaParity
{
    [Test]
    public async Task Ae_subscription_connection_point_delivers_and_unadvise_stops_events()
    {
        var server = new MinimalAeServer();
        var sink = new RecordingEventSink();
        var dispatcher = new OpcAeServerDispatcher(
            new IAeServerToOpcAeServerAdapter(server, _ => sink),
            _ => sink);

        IOPCEventSubscriptionMgt subscription = await dispatcher.CreateEventSubscriptionAsync(
            active: true,
            bufferTime: 50,
            maxSize: 10,
            clientSubscription: 0x8001,
            requestedInterfaceId: IOPCEventSubscriptionMgt.InterfaceId,
            out _,
            out _,
            TestContext.Current!.CancellationToken);

        var connectionPoint = (Opc.Classic.Ae.Dcom.IConnectionPoint)subscription;
        int cookie = await connectionPoint.AdviseAsync(
            new OpcInterfaceRef(IOPCEventSink.InterfaceId, 0, 1, 1, 1, Guid.CreateVersion7(), 0, []),
            TestContext.Current.CancellationToken);

        await subscription.SetFilterAsync((int)EventType.Condition, [], 1, 1000, [], [], TestContext.Current.CancellationToken);
        OpcEventNotification[] first = await sink.WaitForAsync(TestContext.Current.CancellationToken);
        await Assert.That(first.Length).IsGreaterThan(0);
        await Assert.That(first[0].Source).StartsWith("Plant.Reactor");

        await connectionPoint.UnadviseAsync(cookie, TestContext.Current.CancellationToken);
        await subscription.RefreshAsync(cookie, TestContext.Current.CancellationToken);
        await Assert.That(sink.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task Hda_sync_read_raw_returns_seeded_simulation_history()
    {
        var model = new SimulatedPlantModel();
        var server = new SimHdaHostServer(model);
        int[] handles = await server.GetItemHandlesAsync(
            ["Plant.Reactor1.Temperature"],
            [0x9001],
            TestContext.Current!.CancellationToken);

        OpcHdaItem[] items = await server.ReadRawAsync(
            OpcHdaTime.FromTimestamp(model.StartTimeUtc),
            OpcHdaTime.FromTimestamp(model.StartTimeUtc.AddSeconds(2)),
            maxValues: 3,
            bounds: true,
            handles,
            TestContext.Current.CancellationToken);

        await Assert.That(handles[0]).IsGreaterThan(0);
        await Assert.That(items.Length).IsEqualTo(1);
        await Assert.That(items[0].ClientHandle).IsEqualTo(0x9001);
        await Assert.That(items[0].Values.Length).IsEqualTo(3);
        await Assert.That(items[0].Values.All(static value => value.Type == VarType.VT_R8)).IsTrue();
    }

    private sealed class RecordingEventSink : IOPCEventSink
    {
        private readonly TaskCompletionSource<OpcEventNotification[]> _next = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        public Task OnEventAsync(int clientSubscription, bool refresh, bool lastRefresh, OpcEventNotification[] events, CancellationToken cancellationToken = default)
        {
            _ = clientSubscription;
            _ = refresh;
            _ = lastRefresh;
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            _next.TrySetResult(events);
            return Task.CompletedTask;
        }

        public Task<OpcEventNotification[]> WaitForAsync(CancellationToken cancellationToken) =>
            _next.Task.WaitAsync(cancellationToken);
    }

    private sealed class MinimalAeServer : IOpcAeServer, IAeServer
    {
        public event EventHandler<EventArgs>? ServerShutdown;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Ae, State = OpcServerState.Running });

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0x1F);

        public IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(string areaQualifiedName, CancellationToken cancellationToken = default) =>
            AsyncEnumerable.Empty<AreaBrowseElement>();

        public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(EventType eventTypes, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<uint>>([1]);

        public Task<IReadOnlyList<string>> QueryConditionNamesAsync(uint eventCategory, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>(["High"]);

        public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(string actor, string? comment, IReadOnlyList<ConditionRef> conditions, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AckResult>>([]);

        public Task<OpcResultId> EnableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default) =>
            Task.FromResult(OpcResultId.Ok);

        public Task<OpcResultId> DisableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default) =>
            Task.FromResult(OpcResultId.Ok);

        public Task<IAeSubscription> CreateSubscriptionAsync(bool active, int bufferTimeMs, int maxBufferSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IAeSubscription>(new MinimalAeSubscription(active));

        public ValueTask DisposeAsync()
        {
            ServerShutdown?.Invoke(this, EventArgs.Empty);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class MinimalAeSubscription : IAeSubscription
    {
        private readonly Channels.Channel<EventNotification> _events = Channels.Channel.CreateUnbounded<EventNotification>();

        public MinimalAeSubscription(bool active) => Active = active;

        public bool Active { get; private set; }

        public SubscriptionFilter Filter { get; private set; } = new();

        public IAsyncEnumerable<EventNotification> Events => ReadAllAsync();

        public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
        {
            Active = active;
            return Task.CompletedTask;
        }

        public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default)
        {
            Filter = filter;
            if (Active)
            {
                _events.Writer.TryWrite(new Opc.Classic.Ae.EventNotification
                {
                    Source = "Plant.Reactor1.Temperature",
                    Message = "High alarm",
                    EventType = EventType.Condition,
                    EventCategory = 1,
                    Severity = 700,
                    ConditionName = "High",
                    SubConditionName = "High",
                    Time = DateTimeOffset.UtcNow,
                    ActiveTime = DateTimeOffset.UtcNow,
                    NewState = ConditionState.Enabled | ConditionState.Active,
                    Quality = OpcQuality.Good,
                });
            }

            return Task.CompletedTask;
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task CancelRefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public ValueTask DisposeAsync()
        {
            _events.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }

        private async IAsyncEnumerable<Opc.Classic.Ae.EventNotification> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await _events.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (_events.Reader.TryRead(out Opc.Classic.Ae.EventNotification? item))
                {
                    yield return item;
                }
            }
        }
    }
}
