// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Mcp.Capture;

internal sealed class CaptureNotificationSubscription : IAsyncDisposable
{
    internal const int DefaultSubscriberCapacity = 1024;
    internal const int DefaultNotificationQueueCapacity = 16;
    internal const int DefaultPollIntervalMilliseconds = 100;
    private const int MaxNotificationQueueCapacity = 256;

    private readonly object _sync = new();
    private readonly Queue<CaptureNotification> _queue = [];
    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _stopping = new();
    private readonly CaptureSession _session;
    private readonly ICaptureNotificationPublisher _publisher;
    private readonly ILogger _logger;
    private readonly Action<string> _completed;
    private readonly string _cursorId;
    private readonly int _queueCapacity;
    private readonly TimeSpan _interval;
    private Task? _runTask;
    private bool _producerDone;
    private int _pendingDrops;
    private long? _recoveryFrom;
    private long? _recoveryTo;
    private int _disposed;
    private int _resourcesDisposed;

    public CaptureNotificationSubscription(
        CaptureNotificationSubscriptionInfo info,
        string cursorId,
        CaptureSession session,
        ICaptureNotificationPublisher publisher,
        ILogger logger,
        Action<string> completed)
    {
        Info = NormalizeInfo(info);
        _cursorId = cursorId;
        _session = session;
        _publisher = publisher;
        _logger = logger;
        _completed = completed;
        _queueCapacity = Info.NotificationQueueCapacity;
        _interval = TimeSpan.FromMilliseconds(Info.PollIntervalMilliseconds);
    }

    public CaptureNotificationSubscriptionInfo Info { get; }

    internal static CaptureNotificationSubscriptionInfo NormalizeInfo(
        CaptureNotificationSubscriptionInfo info) =>
        info with
        {
            SinceIndex = Math.Max(0, info.SinceIndex),
            SubscriberCapacity = Math.Clamp(
                info.SubscriberCapacity <= 0 ? DefaultSubscriberCapacity : info.SubscriberCapacity,
                1,
                5000),
            NotificationQueueCapacity = Math.Clamp(
                info.NotificationQueueCapacity <= 0
                    ? DefaultNotificationQueueCapacity
                    : info.NotificationQueueCapacity,
                1,
                MaxNotificationQueueCapacity),
            PollIntervalMilliseconds = Math.Clamp(
                info.PollIntervalMilliseconds <= 0
                    ? DefaultPollIntervalMilliseconds
                    : info.PollIntervalMilliseconds,
                10,
                5000),
        };

    public void Start() => _runTask = RunAsync();

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Advisory notification failures must not affect capture.")]
    private async Task RunAsync()
    {
        Task sender = SendAsync(_stopping.Token);
        try
        {
            await ProduceAsync(_stopping.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_stopping.IsCancellationRequested) { }
        catch (ObjectDisposedException) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Capture notification subscription {SubscriptionId} failed.", Info.SubscriptionId);
        }
        finally
        {
            lock (_sync) { _producerDone = true; }
            _signal.Release();
        }
        try { await sender.ConfigureAwait(false); }
        catch (OperationCanceledException) when (_stopping.IsCancellationRequested) { }
        try { await _session.CloseTailSubscriberAsync(_cursorId).ConfigureAwait(false); }
        catch (ObjectDisposedException) { }
        try { await _publisher.DisposeAsync().ConfigureAwait(false); }
        catch (Exception ex) { _logger.LogWarning(ex, "Capture notification publisher dispose failed."); }
        _completed(Info.SubscriptionId);
        DisposeResources();
    }

    private async Task ProduceAsync(CancellationToken cancellationToken)
    {
        long since = Info.SinceIndex;
        CaptureSessionState? previousState = null;
        while (true)
        {
            TailSubscriberSnapshot snapshot =
                await _session.ReadTailSubscriberSnapshotAsync(
                    _cursorId,
                    since,
                    cancellationToken).ConfigureAwait(false);
            if (snapshot.NextIndex > since
                || snapshot.DroppedRanges.Count > 0
                || snapshot.SessionState != previousState
                || snapshot.Done)
            {
                Enqueue(new CaptureNotification(
                    Info.SessionId,
                    Info.SubscriptionId,
                    Info.SubscriberId,
                    snapshot.SessionState,
                    snapshot.FirstIndex,
                    snapshot.NextIndex,
                    snapshot.TotalEmitted,
                    snapshot.Done,
                    snapshot.DroppedRanges));
            }
            since = snapshot.NextIndex;
            previousState = snapshot.SessionState;
            if (snapshot.Done) return;
            await Task.Delay(_interval, cancellationToken).ConfigureAwait(false);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Publisher failures are converted into recovery metadata.")]
    private async Task SendAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            CaptureNotification notification;
            lock (_sync)
            {
                if (_queue.Count == 0)
                {
                    if (_producerDone) return;
                    continue;
                }
                notification = ApplyDrops(_queue.Dequeue());
            }
            try
            {
                await _publisher.PublishAsync(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                lock (_sync) { RecordDrop(notification); }
                _logger.LogWarning(ex, "Capture notification delivery failed.");
            }
        }
    }

    private void Enqueue(CaptureNotification notification)
    {
        bool release;
        lock (_sync)
        {
            release = _queue.Count < _queueCapacity;
            if (!release) RecordDrop(_queue.Dequeue());
            _queue.Enqueue(notification);
        }
        if (release) _signal.Release();
    }

    private CaptureNotification ApplyDrops(CaptureNotification notification)
    {
        if (_pendingDrops == 0) return notification;
        CaptureNotification result = notification with
        {
            NotificationDropCount = notification.NotificationDropCount + _pendingDrops,
            RecoveryFromIndex = Min(notification.RecoveryFromIndex, _recoveryFrom),
            RecoveryToIndex = Max(notification.RecoveryToIndex, _recoveryTo),
        };
        _pendingDrops = 0;
        _recoveryFrom = null;
        _recoveryTo = null;
        return result;
    }

    private void RecordDrop(CaptureNotification notification)
    {
        _pendingDrops += 1 + notification.NotificationDropCount;
        long? first = notification.RecoveryFromIndex;
        long? last = notification.RecoveryToIndex;
        if (notification.NextIndex > notification.FirstIndex)
        {
            first = Min(first, notification.FirstIndex);
            last = Max(last, notification.NextIndex - 1);
        }
        foreach (CaptureDropRange range in notification.CursorDroppedRanges)
        {
            first = Min(first, range.FirstIndex);
            last = Max(last, range.LastIndex);
        }
        _recoveryFrom = Min(_recoveryFrom, first);
        _recoveryTo = Max(_recoveryTo, last);
    }

    private static long? Min(long? a, long? b) => a is null ? b : b is null ? a : Math.Min(a.Value, b.Value);
    private static long? Max(long? a, long? b) => a is null ? b : b is null ? a : Math.Max(a.Value, b.Value);

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        try
        {
            _stopping.Cancel();
            _signal.Release();
        }
        catch (ObjectDisposedException)
        {
        }
        if (_runTask is not null) await _runTask.ConfigureAwait(false);
        DisposeResources();
    }

    private void DisposeResources()
    {
        if (Interlocked.Exchange(ref _resourcesDisposed, 1) != 0) return;
        _stopping.Dispose();
        _signal.Dispose();
    }
}
