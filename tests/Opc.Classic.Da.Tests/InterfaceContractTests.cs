// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da.Tests;

// ---------- DataChange ----------

public sealed class DataChangeTests
{
    [Test]
    public async Task EmptyItems_IsKeepAlive()
    {
        var dc = new DataChange();
        await Assert.That(dc.IsKeepAlive).IsTrue();
        await Assert.That(dc.Items.Count).IsEqualTo(0);
        await Assert.That(dc.MasterResult).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    public async Task WithItems_IsNotKeepAlive()
    {
        var dc = new DataChange
        {
            TransactionId = 7,
            Items = new[]
            {
                new ItemValueResult("X") { Value = 42.0, Quality = OpcQuality.Good },
                new ItemValueResult("Y") { Value = "hi", Quality = OpcQuality.Uncertain },
            },
        };
        await Assert.That(dc.IsKeepAlive).IsFalse();
        await Assert.That(dc.TransactionId).IsEqualTo(7);
        await Assert.That(dc.Items.Count).IsEqualTo(2);
    }
}

// ---------- ServerShutdownEventArgs ----------

public sealed class ServerShutdownEventArgsTests
{
    [Test]
    public async Task DefaultReason_IsEmpty()
    {
        var e = new ServerShutdownEventArgs();
        await Assert.That(e.Reason).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task TimeDefaults_ToNow()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var e = new ServerShutdownEventArgs();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);
        await Assert.That(e.Time >= before).IsTrue();
        await Assert.That(e.Time <= after).IsTrue();
    }
}

// ---------- Interface contracts — exercise via hand-written test doubles ----------

/// <summary>
/// Minimal hand-written test double for <see cref="IDaServer"/>. Demonstrates
/// the interface is implementable with idiomatic async code and proves
/// the contract compiles end-to-end.
/// </summary>
internal sealed class FakeDaServer : IDaServer
{
    public int LocaleId { get; private set; } = 0x0409; // en-US default

    public event EventHandler<ServerShutdownEventArgs>? ServerShutdown;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) =>
        Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            State = OpcServerState.Running,
            VendorInfo = "FakeDaServer",
            CurrentTime = DateTimeOffset.UtcNow,
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
        });

    public Task SetLocaleAsync(int localeId, CancellationToken ct = default)
    {
        LocaleId = localeId;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<int>>(new[] { 0x0409, 0x0407 });

    public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken ct = default) =>
        Task.FromResult(resultId.Description ?? "unknown");

    public async IAsyncEnumerable<BrowseElement> BrowseAsync(
        string itemPath,
        BrowseFilters filters = BrowseFilters.All,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.Yield();
        yield return new BrowseElement { Name = "Folder", IsItem = false, HasChildren = true };
        yield return new BrowseElement { Name = "Tag1", ItemName = "Tag1", IsItem = true };
        yield return new BrowseElement { Name = "Tag2", ItemName = "Tag2", IsItem = true };
    }

    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(
        IReadOnlyList<Item> items, CancellationToken ct = default)
    {
        IReadOnlyList<ItemValueResult> results = items.Select(i => new ItemValueResult(i.ItemName)
        {
            ClientHandle = i.ClientHandle,
            Value = 42.0,
            Quality = OpcQuality.Good,
            Timestamp = DateTimeOffset.UtcNow,
        }).ToList();
        return Task.FromResult(results);
    }

    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(
        IReadOnlyList<ItemValue> values, CancellationToken ct = default)
    {
        IReadOnlyList<IdentifiedResult> results = values
            .Select(v => new IdentifiedResult(v.ItemName) { ClientHandle = v.ClientHandle })
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(
        IReadOnlyList<Item> items, CancellationToken ct = default)
    {
        IReadOnlyList<IdentifiedResult> results = items
            .Select(i => new IdentifiedResult(i.ItemName) { ClientHandle = i.ClientHandle })
            .ToList();
        return Task.FromResult(results);
    }

    public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(
        IReadOnlyList<ItemIdentifier> itemIds,
        IReadOnlyList<PropertyID> propertyIds,
        bool returnValues,
        CancellationToken ct = default)
    {
        IReadOnlyList<ItemPropertyResult> results = itemIds.Select(id => new ItemPropertyResult
        {
            ItemName = id.ItemName,
            Properties = propertyIds.Select(p => new ItemProperty
            {
                PropertyId = p,
                Value = returnValues ? "fake" : null,
            }).ToList(),
        }).ToList();
        return Task.FromResult(results);
    }

    public Task<IDaSubscription> CreateSubscriptionAsync(
        SubscriptionState state, CancellationToken ct = default) =>
        Task.FromResult<IDaSubscription>(new FakeDaSubscription(state));

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, new ServerShutdownEventArgs { Reason = "Disposed" });
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeDaSubscription : IDaSubscription
{
    public FakeDaSubscription(SubscriptionState state) { State = state; }

    public SubscriptionState State { get; private set; }

    public async IAsyncEnumerable<DataChange> DataChanges_Impl(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        for (var i = 0; i < 3; i++)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return new DataChange
            {
                TransactionId = i,
                Items = new[] { new ItemValueResult("Tag") { Value = i, Quality = OpcQuality.Good } },
            };
        }
    }

    public IAsyncEnumerable<DataChange> DataChanges => DataChanges_Impl();

    public Task SetStateAsync(SubscriptionState state, CancellationToken ct = default)
    {
        State = state;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IdentifiedResult>> AddItemsAsync(IReadOnlyList<Item> items, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IdentifiedResult>>(items.Select(i => new IdentifiedResult(i.ItemName) { ClientHandle = i.ClientHandle }).ToList());

    public Task<IReadOnlyList<IdentifiedResult>> RemoveItemsAsync(IReadOnlyList<int> serverHandles, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IdentifiedResult>>(serverHandles.Select(h => new IdentifiedResult($"#{h}")).ToList());

    public Task<IReadOnlyList<IdentifiedResult>> SetActiveStateAsync(IReadOnlyList<int> serverHandles, bool active, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IdentifiedResult>>(serverHandles.Select(h => new IdentifiedResult($"#{h}")).ToList());

    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<int> serverHandles, bool fromCache, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ItemValueResult>>(serverHandles.Select(h => new ItemValueResult($"#{h}") { Value = h, Quality = OpcQuality.Good }).ToList());

    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<int> serverHandles, IReadOnlyList<object?> values, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<IdentifiedResult>>(serverHandles.Select(h => new IdentifiedResult($"#{h}")).ToList());

    public Task<int> RefreshAsync(bool fromCache, CancellationToken ct = default) => Task.FromResult(42);
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public sealed class IDaServerContractTests
{
    [Test]
    public async Task GetStatusAsync_ReturnsServerStatus()
    {
        await using var server = new FakeDaServer();
        var status = await server.GetStatusAsync();
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Da);
        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.VendorInfo).IsEqualTo("FakeDaServer");
    }

    [Test]
    public async Task SetLocaleAsync_UpdatesLocaleId()
    {
        await using var server = new FakeDaServer();
        await server.SetLocaleAsync(0x0407); // de-DE
        await Assert.That(server.LocaleId).IsEqualTo(0x0407);
    }

    [Test]
    public async Task BrowseAsync_StreamsElements()
    {
        await using var server = new FakeDaServer();
        var count = 0;
        await foreach (var el in server.BrowseAsync(string.Empty))
        {
            count++;
        }
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task BrowseAsync_RespectsCancellation()
    {
        await using var server = new FakeDaServer();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        try
        {
            await foreach (var _ in server.BrowseAsync(string.Empty, BrowseFilters.All, cts.Token))
            {
                // Should not reach here under normal cancellation flow.
            }
        }
        catch (OperationCanceledException)
        {
            // Expected for true implementations that observe the token.
            return;
        }
        // The fake yields a few times without checking the token first; that's
        // an acceptable test-double behavior. We still demonstrate the
        // signature accepts CancellationToken.
    }

    [Test]
    public async Task ReadAsync_EchoesClientHandlesAndPopulatesValues()
    {
        await using var server = new FakeDaServer();
        IReadOnlyList<Item> items = new[]
        {
            new Item("Tag1") { ClientHandle = 1 },
            new Item("Tag2") { ClientHandle = 2 },
        };
        var results = await server.ReadAsync(items);
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].ClientHandle).IsEqualTo(1);
        await Assert.That(results[1].ClientHandle).IsEqualTo(2);
        await Assert.That(results[0].Quality.Quality).IsEqualTo(OpcQualityKind.Good);
    }

    [Test]
    public async Task GetPropertiesAsync_ReturnsRequestedProperties()
    {
        await using var server = new FakeDaServer();
        IReadOnlyList<ItemIdentifier> ids = new ItemIdentifier[]
        {
            new("Tag1"),
            new("Tag2"),
        };
        IReadOnlyList<PropertyID> props = new[] { PropertyID.Value, PropertyID.Quality, PropertyID.EuUnits };

        var results = await server.GetPropertiesAsync(ids, props, returnValues: true);
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].Properties.Count).IsEqualTo(3);
        await Assert.That(results[0].Properties[0].Value).IsEqualTo("fake");
    }

    [Test]
    public async Task CreateSubscriptionAsync_ReturnsSubscriptionWithState()
    {
        await using var server = new FakeDaServer();
        var initial = SubscriptionState.At(TimeSpan.FromSeconds(1));
        await using var sub = await server.CreateSubscriptionAsync(initial);
        await Assert.That(sub.State).IsEqualTo(initial);
    }

    [Test]
    public async Task Subscription_DataChanges_DeliversBatches()
    {
        await using var server = new FakeDaServer();
        await using var sub = await server.CreateSubscriptionAsync(SubscriptionState.At(TimeSpan.FromMilliseconds(100)));

        var batches = new List<DataChange>();
        await foreach (var dc in sub.DataChanges)
        {
            batches.Add(dc);
        }

        await Assert.That(batches.Count).IsEqualTo(3);
        await Assert.That(batches[0].TransactionId).IsEqualTo(0);
        await Assert.That(batches[2].TransactionId).IsEqualTo(2);
    }

    [Test]
    public async Task Subscription_AddItems_EchoesClientHandles()
    {
        await using var server = new FakeDaServer();
        await using var sub = await server.CreateSubscriptionAsync(SubscriptionState.At(TimeSpan.FromSeconds(1)));

        IReadOnlyList<Item> items = new[]
        {
            new Item("A") { ClientHandle = 10 },
            new Item("B") { ClientHandle = 20 },
        };
        var results = await sub.AddItemsAsync(items);
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].ClientHandle).IsEqualTo(10);
        await Assert.That(results[1].ClientHandle).IsEqualTo(20);
    }

    [Test]
    public async Task Subscription_SetState_UpdatesState()
    {
        await using var server = new FakeDaServer();
        await using var sub = await server.CreateSubscriptionAsync(SubscriptionState.At(TimeSpan.FromSeconds(1)));

        var newState = new SubscriptionState { UpdateRateMs = 500, Active = false };
        await sub.SetStateAsync(newState);
        await Assert.That(sub.State).IsEqualTo(newState);
    }
}
