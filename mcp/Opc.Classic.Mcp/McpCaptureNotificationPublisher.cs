// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using ModelContextProtocol.Server;
using Opc.Classic.Mcp.Capture;

namespace Opc.Classic.Mcp;

internal sealed class McpCaptureNotificationPublisher : ICaptureNotificationPublisher
{
    internal const string NotificationMethod = "notifications/opcclassic/capture";
    private readonly McpServer _server;

    public McpCaptureNotificationPublisher(McpServer server) =>
        _server = server ?? throw new ArgumentNullException(nameof(server));

    public ValueTask PublishAsync(CaptureNotification notification, CancellationToken cancellationToken) =>
        new(_server.SendNotificationAsync(
            NotificationMethod,
            CaptureNotificationParams.From(notification),
            serializerOptions: null,
            cancellationToken));

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed record CaptureNotificationParams(
    string SessionId,
    string SubscriptionId,
    string SubscriberId,
    CaptureSessionState State,
    long FirstIndex,
    long NextIndex,
    long TotalEmitted,
    bool Done,
    IReadOnlyList<CaptureDropRange> CursorDroppedRanges,
    int NotificationDropCount,
    long? RecoveryFromIndex,
    long? RecoveryToIndex)
{
    public static CaptureNotificationParams From(CaptureNotification notification) =>
        new(
            notification.SessionId,
            notification.SubscriptionId,
            notification.SubscriberId,
            notification.State,
            notification.FirstIndex,
            notification.NextIndex,
            notification.TotalEmitted,
            notification.Done,
            notification.CursorDroppedRanges,
            notification.NotificationDropCount,
            notification.RecoveryFromIndex,
            notification.RecoveryToIndex);
}
