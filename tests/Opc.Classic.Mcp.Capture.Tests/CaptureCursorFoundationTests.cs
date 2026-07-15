// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureCursorFoundationTests
{
    private static readonly Guid kInterfaceId =
        Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Test]
    public async Task MultipleNotificationSubscribersShareOneIncrementalProducer()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root);
            var source = new IncrementalSource();
            source.Add(Packet("initial"));
            CaptureSession session = await manager.CreateAndStartAsync(
                "incremental",
                _ => source,
                new CaptureStartRequest(),
                CancellationToken.None);
            var first = new RecordingPublisher();
            var second = new RecordingPublisher();

            CaptureNotificationSubscriptionInfo firstInfo =
                await manager.SubscribeNotificationsAsync(
                    session.Id, 0, "first", 16, 8, 10, first, CancellationToken.None);
            CaptureNotificationSubscriptionInfo secondInfo =
                await manager.SubscribeNotificationsAsync(
                    session.Id, 0, "second", 16, 8, 10, second, CancellationToken.None);
            await first.WaitForIndexAsync(1, TestContext.Current!.CancellationToken);
            await second.WaitForIndexAsync(1, TestContext.Current.CancellationToken);

            source.AddRange([Packet("next-1"), Packet("next-2")]);
            await first.WaitForIndexAsync(3, TestContext.Current.CancellationToken);
            await second.WaitForIndexAsync(3, TestContext.Current.CancellationToken);

            await Assert.That(source.IncrementalReadCount).IsEqualTo(2);
            await Assert.That(source.ReplayReadCount).IsEqualTo(0);
            await Assert.That(await manager.UnsubscribeNotificationsAsync(firstInfo.SubscriptionId)).IsTrue();
            await Assert.That(await manager.UnsubscribeNotificationsAsync(secondInfo.SubscriptionId)).IsTrue();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task SubscribeNotifications_CursorLimitFailsBeforeReturningSubscriptionId()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root);
            CaptureSession session = await manager.CreateAndStartAsync(
                "incremental",
                _ => new IncrementalSource(),
                new CaptureStartRequest(),
                CancellationToken.None);
            for (int i = 0; i < 64; i++)
            {
                await session.ReserveTailSubscriberAsync(
                    $"reserved-{i}",
                    1,
                    startProducer: false,
                    CancellationToken.None);
            }
            var publisher = new RecordingPublisher();

            await Assert.That(async () => await manager.SubscribeNotificationsAsync(
                    session.Id,
                    0,
                    "overflow",
                    1,
                    1,
                    10,
                    publisher,
                    CancellationToken.None))
                .Throws<CaptureException>();

            await Assert.That(manager.NotificationSubscriptionCount).IsEqualTo(0);
            await Assert.That(publisher.DisposeCallCount).IsEqualTo(1);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task SubscribeNotifications_ProducerInitializationFailureDoesNotReturnSubscription()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root);
            var source = new IncrementalSource { ReadException = new IOException("initial read failed") };
            source.Add(Packet("unreadable"));
            CaptureSession session = await manager.CreateAndStartAsync(
                "incremental",
                _ => source,
                new CaptureStartRequest(),
                CancellationToken.None);
            var publisher = new RecordingPublisher();

            await Assert.That(async () => await manager.SubscribeNotificationsAsync(
                    session.Id, 0, "init-failure", 8, 4, 10, publisher, CancellationToken.None))
                .Throws<IOException>();
            await Assert.That(manager.NotificationSubscriptionCount).IsEqualTo(0);
            await Assert.That(publisher.DisposeCallCount).IsEqualTo(1);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task NamedCursorRetryAndDropRecoveryRemainAuthoritative()
    {
        string folder = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new IncrementalSource();
            source.Add(Packet("0"));
            source.Add(Packet("1"));
            source.Add(Packet("2"));
            source.Add(Packet("3"));
            await using var session = new CaptureSession(
                "replay",
                "incremental",
                source,
                folder,
                new CaptureStartRequest());
            await session.StartAsync(CancellationToken.None);

            DrainTailResult first = await session.DrainSubscriberTailAsync(
                0, 10, "bounded", 2, CancellationToken.None);
            DrainTailResult retry = await session.DrainSubscriberTailAsync(
                0, 10, "bounded", 2, CancellationToken.None);
            DrainTailResult recovered = await session.DrainTailAsync(
                0, 10, CancellationToken.None);

            await Assert.That(first.Pdus.Count).IsEqualTo(2);
            await Assert.That(first.DroppedRanges).IsEquivalentTo([new CaptureDropRange(0, 1)]);
            await Assert.That(retry.Pdus.SequenceEqual(first.Pdus)).IsTrue();
            await Assert.That(retry.DroppedRanges).IsEquivalentTo(first.DroppedRanges!);
            await Assert.That(recovered.Pdus.Count).IsEqualTo(4);
            await Assert.That(recovered.NextIndex).IsEqualTo(4);
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    [Test]
    public async Task ConcurrentTailCloseRemoveAndDisposeWaitForActiveCursorOperation()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new IncrementalSource
            {
                ReadGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously),
            };
            source.Add(Packet("blocked"));
            var manager = new CaptureSessionManager(root);
            CaptureSession session = await manager.CreateAndStartAsync(
                "incremental",
                _ => source,
                new CaptureStartRequest(),
                CancellationToken.None);

            Task<DrainTailResult> tail = session.DrainSubscriberTailAsync(
                0, 10, "concurrent", 10, TestContext.Current!.CancellationToken);
            await source.ReadEntered.Task.WaitAsync(TestContext.Current.CancellationToken);
            Task<bool> close = session.CloseTailSubscriberAsync("concurrent");
            Task<bool> remove = manager.RemoveAsync(session.Id, CancellationToken.None);
            await Task.Delay(20, TestContext.Current.CancellationToken);

            await Assert.That(tail.IsCompleted).IsFalse();
            await Assert.That(remove.IsCompleted).IsFalse();
            source.ReadGate.SetResult();

            DrainTailResult result = await tail;
            await close;
            await Assert.That(await remove).IsTrue();
            await manager.DisposeAsync();

            await Assert.That(result.Pdus.Count).IsEqualTo(1);
            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Disposed);
            await Assert.That(async () => await session.DrainTailAsync(0, 1, CancellationToken.None))
                .Throws<ObjectDisposedException>();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task PublisherFailureAndSlowDeliveryPreserveTailRecoveryIndexes()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root);
            var source = new IncrementalSource();
            source.Add(Packet("first"));
            CaptureSession session = await manager.CreateAndStartAsync(
                "incremental",
                _ => source,
                new CaptureStartRequest(),
                CancellationToken.None);
            var publisher = new FailThenBlockPublisher();
            CaptureNotificationSubscriptionInfo info =
                await manager.SubscribeNotificationsAsync(
                    session.Id, 0, "failure", 2, 1, 10, publisher, CancellationToken.None);
            await publisher.Failed.Task.WaitAsync(TestContext.Current!.CancellationToken);

            source.Add(Packet("second"));
            source.Add(Packet("third"));
            source.Add(Packet("fourth"));
            CaptureNotification recovered = await publisher.Recovered.Task.WaitAsync(
                TestContext.Current.CancellationToken);
            DrainTailResult tail = await session.DrainTailAsync(
                recovered.RecoveryFromIndex ?? 0,
                10,
                TestContext.Current.CancellationToken);

            await Assert.That(recovered.NotificationDropCount).IsGreaterThanOrEqualTo(1);
            await Assert.That(recovered.RecoveryFromIndex.HasValue).IsTrue();
            await Assert.That(tail.NextIndex).IsEqualTo(4);
            await Assert.That(await manager.UnsubscribeNotificationsAsync(info.SubscriptionId)).IsTrue();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    private static CapturedPacket Packet(string tag)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(tag);
        return new CapturedPacket(
            DateTimeOffset.UtcNow,
            data.Length,
            data,
            0,
            new Dictionary<string, string?>
            {
                ["iid"] = kInterfaceId.ToString("D"),
                ["opnum"] = "5",
                ["direction"] = "request",
            });
    }

    private sealed class IncrementalSource : ICaptureSource, IIncrementalCaptureSource
    {
        private readonly object _sync = new();
        private readonly List<CapturedPacket> _packets = [];

        public TaskCompletionSource? ReadGate { get; init; }
        public Exception? ReadException { get; init; }
        public TaskCompletionSource ReadEntered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int IncrementalReadCount { get; private set; }
        public int ReplayReadCount { get; private set; }
        public long PacketCount
        {
            get
            {
                lock (_sync)
                {
                    return _packets.Count;
                }
            }
        }
        public long ByteCount
        {
            get
            {
                lock (_sync)
                {
                    return _packets.Sum(p => (long)p.OriginalLength);
                }
            }
        }
        public int LinkType => 0;

        public void Add(CapturedPacket packet)
        {
            lock (_sync)
            {
                _packets.Add(packet);
            }
        }

        public void AddRange(IEnumerable<CapturedPacket> packets)
        {
            lock (_sync)
            {
                _packets.AddRange(packets);
            }
        }
        public Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public string? GetRawPcapFilePath() => null;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public async IAsyncEnumerable<CapturedPacket> ReadFromAsync(
            long packetIndex,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            IncrementalReadCount++;
            ReadEntered.TrySetResult();
            if (ReadException is not null)
            {
                throw ReadException;
            }
            if (ReadGate is not null)
            {
                await ReadGate.Task.WaitAsync(cancellationToken);
            }
            CapturedPacket[] snapshot;
            lock (_sync)
            {
                snapshot = _packets.Skip((int)packetIndex).ToArray();
            }
            foreach (CapturedPacket packet in snapshot)
            {
                yield return packet;
            }
        }

        public async IAsyncEnumerable<CapturedPacket> ReadAllAsync(
            long? maxPackets,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ReplayReadCount++;
            CapturedPacket[] snapshot;
            lock (_sync)
            {
                snapshot = _packets.ToArray();
            }
            foreach (CapturedPacket packet in snapshot.Take((int)(maxPackets ?? int.MaxValue)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return packet;
                await Task.Yield();
            }
        }
    }

    private sealed class RecordingPublisher : ICaptureNotificationPublisher
    {
        private readonly ConcurrentQueue<CaptureNotification> _notifications = new();
        private readonly SemaphoreSlim _signal = new(0);
        public int DisposeCallCount { get; private set; }

        public ValueTask PublishAsync(CaptureNotification notification, CancellationToken cancellationToken)
        {
            _notifications.Enqueue(notification);
            _signal.Release();
            return ValueTask.CompletedTask;
        }

        public async Task<CaptureNotification> WaitForIndexAsync(
            long nextIndex,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                await _signal.WaitAsync(cancellationToken);
                if (_notifications.TryDequeue(out CaptureNotification? notification)
                    && notification.NextIndex >= nextIndex)
                {
                    return notification;
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            DisposeCallCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailThenBlockPublisher : ICaptureNotificationPublisher
    {
        private int _calls;
        public TaskCompletionSource Failed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<CaptureNotification> Recovered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask PublishAsync(CaptureNotification notification, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _calls) == 1)
            {
                Failed.TrySetResult();
                throw new InvalidOperationException("publisher failure");
            }
            if (notification.NotificationDropCount > 0)
            {
                Recovered.TrySetResult(notification);
            }
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
