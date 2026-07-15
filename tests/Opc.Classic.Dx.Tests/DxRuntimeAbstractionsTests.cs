// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dx.Tests;

public sealed class DxRuntimeAbstractionsTests
{
    [Test]
    public async Task FakeAdapter_PreservesValuesQualityErrorsAndHealth()
    {
        var adapter = new FakeDaAdapter();
        var item = new DxDataItem("Tank1.Level", "Area1");

        var reads = await adapter.ReadAsync(new[] { item });
        var writes = await adapter.WriteAsync(
            new[]
            {
                new DxWriteRequest(
                    item,
                    reads[0].Data.Value,
                    reads[0].Data.Quality,
                    reads[0].Data.Timestamp),
            });
        var health = await adapter.GetHealthAsync();
        await adapter.ReconnectAsync();

        await Assert.That(reads[0].Item).IsEqualTo(item);
        await Assert.That(reads[0].Data.Value).IsEqualTo(OpcVariant.FromDouble(42.25));
        await Assert.That(reads[0].Data.Quality)
            .IsEqualTo(new DxQuality(DxQualityStatus.Good));
        await Assert.That(reads[0].Data.IsSuccess).IsTrue();
        await Assert.That(writes[0].IsSuccess).IsFalse();
        await Assert.That(writes[0].ErrorDiagnostic).IsEqualTo("target rejected value");
        await Assert.That(health.IsAvailable).IsTrue();
        await Assert.That(adapter.ReconnectCount).IsEqualTo(1);
    }

    [Test]
    public async Task FakeAdapter_HonorsCancellation()
    {
        var adapter = new FakeDaAdapter();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.That(async () =>
                await adapter.ReadAsync(
                    new[] { new DxDataItem("Tank1.Level") },
                    cancellation.Token))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ManualScheduler_AdvancesDelaysDeterministically()
    {
        var initial = new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);
        var scheduler = new ManualDxScheduler(initial);
        var startingTimestamp = scheduler.GetTimestamp();
        var delay = scheduler.DelayAsync(TimeSpan.FromMilliseconds(250));

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(249));
        await Assert.That(delay.IsCompleted).IsFalse();
        await Assert.That(scheduler.PendingCount).IsEqualTo(1);

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1));
        await delay;

        await Assert.That(scheduler.UtcNow).IsEqualTo(initial.AddMilliseconds(250));
        await Assert.That(
                scheduler.GetElapsedTime(startingTimestamp, scheduler.GetTimestamp()))
            .IsEqualTo(TimeSpan.FromMilliseconds(250));
        await Assert.That(scheduler.PendingCount).IsEqualTo(0);
    }

    [Test]
    public async Task ManualScheduler_CancellationRemovesPendingDelay()
    {
        var scheduler = new ManualDxScheduler();
        using var cancellation = new CancellationTokenSource();
        var delay = scheduler.DelayAsync(TimeSpan.FromSeconds(10), cancellation.Token);

        cancellation.Cancel();

        await Assert.That(async () => await delay)
            .Throws<OperationCanceledException>();
        await Assert.That(scheduler.PendingCount).IsEqualTo(0);
    }

    [Test]
    public async Task TransferSnapshot_RejectsQueueDepthBeyondCapacity()
    {
        await Assert.That(() => new DxTransferSnapshot(
                "connection",
                DxTransferState.Running,
                queueDepth: 2,
                queueCapacity: 1))
            .Throws<ArgumentOutOfRangeException>();
    }

    private sealed class FakeDaAdapter : IDxDaAdapter
    {
        public string Name => "fake-da";

        public int ReconnectCount { get; private set; }

        public ValueTask<DxEndpointHealth> GetHealthAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new DxEndpointHealth(
                DxEndpointHealthState.Healthy,
                DateTimeOffset.UnixEpoch,
                OpcResultId.Ok));
        }

        public ValueTask ReconnectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReconnectCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask<IReadOnlyList<DxReadResult>> ReadAsync(
            IReadOnlyList<DxDataItem> items,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<DxReadResult> results =
            [
                new(
                    items[0],
                    new DxDataValue(
                        OpcVariant.FromDouble(42.25),
                        DateTimeOffset.UnixEpoch,
                        new DxQuality(DxQualityStatus.Good),
                        OpcResultId.Ok)),
            ];
            return ValueTask.FromResult(results);
        }

        public ValueTask<IReadOnlyList<DxWriteResult>> WriteAsync(
            IReadOnlyList<DxWriteRequest> requests,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<DxWriteResult> results =
            [
                new(requests[0].Item, OpcResultId.Fail, "target rejected value"),
            ];
            return ValueTask.FromResult(results);
        }
    }
}
