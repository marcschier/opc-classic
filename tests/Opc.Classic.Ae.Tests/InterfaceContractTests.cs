// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.CompilerServices;

namespace Opc.Classic.Ae.Tests;

public sealed class ConditionRefTests
{
    [Test]
    public async Task ToString_FormatsSourceAndCondition()
    {
        var r = new ConditionRef("Tank1", "HighLimit");
        await Assert.That(r.ToString()).IsEqualTo("Tank1::HighLimit");
    }

    [Test]
    public async Task ValueEquality_OnAllFields()
    {
        var a = new ConditionRef("S", "C");
        var b = new ConditionRef("S", "C");
        var c = new ConditionRef("S", "X");
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a == b).IsTrue();
        await Assert.That(a == c).IsFalse();
    }
}

public sealed class AreaBrowseElementTests
{
    [Test]
    public async Task Default_IsNeitherAreaNorSource()
    {
        var e = new AreaBrowseElement();
        await Assert.That(e.IsArea).IsFalse();
        await Assert.That(e.IsSource).IsFalse();
        await Assert.That(e.Name).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Initializer_AssignsAllFields()
    {
        var e = new AreaBrowseElement
        {
            Name = "Tanks",
            QualifiedName = "Plant1/Tanks",
            IsArea = true,
            IsSource = false,
        };
        await Assert.That(e.QualifiedName).IsEqualTo("Plant1/Tanks");
        await Assert.That(e.IsArea).IsTrue();
        await Assert.That(e.IsSource).IsFalse();
    }
}

// ---- AE interface contracts — exercise with hand-written fakes ----

internal sealed class FakeAeServer : IAeServer
{
    public event EventHandler<EventArgs>? ServerShutdown;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) =>
        Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            State = OpcServerState.Running,
            VendorInfo = "FakeAeServer",
        });

    public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.Yield();
        yield return new AreaBrowseElement { Name = "Plant1", QualifiedName = "Plant1", IsArea = true };
        yield return new AreaBrowseElement { Name = "Tank1", QualifiedName = "Plant1/Tank1", IsSource = true };
    }

    public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(EventType eventTypes, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<uint>>(new uint[] { 100, 200 });

    public Task<IReadOnlyList<string>> QueryConditionNamesAsync(uint eventCategory, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>(new[] { "HighLimit", "LowLimit" });

    public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor, string? comment, IReadOnlyList<ConditionRef> conditions, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AckResult>>(conditions
            .Select(c => new AckResult { Condition = c })
            .ToList());

    public Task<OpcResultId> EnableConditionsByAreaAsync(IReadOnlyList<string> areas, CancellationToken ct = default) =>
        Task.FromResult(OpcResultId.Ok);

    public Task<OpcResultId> DisableConditionsByAreaAsync(IReadOnlyList<string> areas, CancellationToken ct = default) =>
        Task.FromResult(OpcResultId.Ok);

    public Task<IAeSubscription> CreateSubscriptionAsync(
        bool active, int bufferTimeMs, int maxBufferSize, CancellationToken ct = default) =>
        Task.FromResult<IAeSubscription>(new FakeAeSubscription(active));

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeAeSubscription : IAeSubscription
{
    public FakeAeSubscription(bool active) { Active = active; }

    public bool Active { get; private set; }
    public SubscriptionFilter Filter { get; private set; } = new();
    public IAsyncEnumerable<EventNotification> Events => EventsImpl();

    private async IAsyncEnumerable<EventNotification> EventsImpl(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (var i = 0; i < 3; i++)
        {
            await Task.Yield();
            yield return new EventNotification
            {
                Source = $"Source{i}",
                Severity = 500 + i,
                Message = $"Event {i}",
                EventType = EventType.Simple,
            };
        }
    }

    public Task SetActiveAsync(bool active, CancellationToken ct = default)
    {
        Active = active;
        return Task.CompletedTask;
    }

    public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken ct = default)
    {
        Filter = filter;
        return Task.CompletedTask;
    }

    public Task RefreshAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CancelRefreshAsync(CancellationToken ct = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public sealed class IAeServerContractTests
{
    [Test]
    public async Task GetStatusAsync_ReturnsAeStatus()
    {
        await using var server = new FakeAeServer();
        var status = await server.GetStatusAsync();
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Ae);
        await Assert.That(status.VendorInfo).IsEqualTo("FakeAeServer");
    }

    [Test]
    public async Task BrowseAreasAsync_StreamsElements()
    {
        await using var server = new FakeAeServer();
        var count = 0;
        await foreach (var _ in server.BrowseAreasAsync(string.Empty))
        {
            count++;
        }
        await Assert.That(count).IsEqualTo(2);
    }

    [Test]
    public async Task QueryEventCategoriesAsync_ReturnsCategoryIds()
    {
        await using var server = new FakeAeServer();
        var cats = await server.QueryEventCategoriesAsync(EventType.All);
        await Assert.That(cats.Count).IsEqualTo(2);
        await Assert.That(cats[0]).IsEqualTo(100u);
    }

    [Test]
    public async Task AcknowledgeAsync_EchoesConditions()
    {
        await using var server = new FakeAeServer();
        IReadOnlyList<ConditionRef> conds = new[]
        {
            new ConditionRef("Tank1", "HighLimit"),
            new ConditionRef("Tank2", "LowLimit"),
        };
        var results = await server.AcknowledgeAsync("alice", "investigating", conds);
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].Condition.Source).IsEqualTo("Tank1");
        await Assert.That(results[0].ResultId).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    public async Task CreateSubscriptionAsync_PreservesActiveFlag()
    {
        await using var server = new FakeAeServer();
        await using var sub = await server.CreateSubscriptionAsync(active: true, bufferTimeMs: 0, maxBufferSize: 0);
        await Assert.That(sub.Active).IsTrue();
    }

    [Test]
    public async Task Subscription_Events_StreamsNotifications()
    {
        await using var server = new FakeAeServer();
        await using var sub = await server.CreateSubscriptionAsync(active: true, bufferTimeMs: 0, maxBufferSize: 0);

        var events = new List<EventNotification>();
        await foreach (var e in sub.Events)
        {
            events.Add(e);
        }
        await Assert.That(events.Count).IsEqualTo(3);
        await Assert.That(events[0].Severity).IsEqualTo(500);
        await Assert.That(events[2].Source).IsEqualTo("Source2");
    }

    [Test]
    public async Task Subscription_SetActive_UpdatesState()
    {
        await using var server = new FakeAeServer();
        await using var sub = await server.CreateSubscriptionAsync(active: true, bufferTimeMs: 0, maxBufferSize: 0);
        await sub.SetActiveAsync(false);
        await Assert.That(sub.Active).IsFalse();
    }

    [Test]
    public async Task Subscription_SetFilter_UpdatesFilter()
    {
        await using var server = new FakeAeServer();
        await using var sub = await server.CreateSubscriptionAsync(active: true, bufferTimeMs: 0, maxBufferSize: 0);
        var newFilter = new SubscriptionFilter { MinSeverity = 250, EventTypes = EventType.Condition };
        await sub.SetFilterAsync(newFilter);
        await Assert.That(sub.Filter.MinSeverity).IsEqualTo(250);
        await Assert.That(sub.Filter.EventTypes).IsEqualTo(EventType.Condition);
    }
}
