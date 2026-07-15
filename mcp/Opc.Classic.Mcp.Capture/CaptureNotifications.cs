// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

internal interface ICaptureNotificationPublisher : IAsyncDisposable
{
    ValueTask PublishAsync(CaptureNotification notification, CancellationToken cancellationToken);
}

internal sealed class NullCaptureNotificationPublisher : ICaptureNotificationPublisher
{
    public static NullCaptureNotificationPublisher Instance { get; } = new();
    private NullCaptureNotificationPublisher() { }
    public ValueTask PublishAsync(CaptureNotification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed record CaptureNotification(
    string SessionId,
    string SubscriptionId,
    string SubscriberId,
    CaptureSessionState State,
    long FirstIndex,
    long NextIndex,
    long TotalEmitted,
    bool Done,
    IReadOnlyList<CaptureDropRange> CursorDroppedRanges,
    int NotificationDropCount = 0,
    long? RecoveryFromIndex = null,
    long? RecoveryToIndex = null);

internal sealed record CaptureNotificationSubscriptionInfo(
    string SubscriptionId,
    string SessionId,
    string SubscriberId,
    long SinceIndex,
    int SubscriberCapacity,
    int NotificationQueueCapacity,
    int PollIntervalMilliseconds);
