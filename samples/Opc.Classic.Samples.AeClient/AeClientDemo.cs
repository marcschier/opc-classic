// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Ae;

namespace Opc.Classic.Samples.AeClient;

internal sealed class AeClientDemo : BackgroundService
{
    private const string AckActor = "sample-client";
    private const string AckComment = "Acknowledged by the self-contained AE client sample";

    private static readonly Action<ILogger, OpcServerState, string?, Version?, Exception?> StatusMessage =
        LoggerMessage.Define<OpcServerState, string?, Version?>(
            LogLevel.Information,
            new EventId(1, nameof(StatusMessage)),
            "Server status: state={State}, vendor={Vendor}, version={Version}");

    private static readonly Action<ILogger, int, string, string, string, Exception?> BrowseNodeMessage =
        LoggerMessage.Define<int, string, string, string>(
            LogLevel.Information,
            new EventId(2, nameof(BrowseNodeMessage)),
            "Area browse: depth={Depth}, kind={Kind}, name={Name}, qualified={QualifiedName}");

    private static readonly Action<ILogger, string, EventType, int, string, Exception?> EventMessage =
        LoggerMessage.Define<string, EventType, int, string>(
            LogLevel.Information,
            new EventId(3, nameof(EventMessage)),
            "Event received: source={Source}, type={EventType}, severity={Severity}, message={Message}");

    private static readonly Action<ILogger, string, OpcResultId, Exception?> AckResultMessage =
        LoggerMessage.Define<string, OpcResultId>(
            LogLevel.Information,
            new EventId(4, nameof(AckResultMessage)),
            "Ack result: condition={Condition}, result={Result}");

    private readonly LoopbackAeClient _client;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<AeClientDemo> _logger;

    public AeClientDemo(
        LoopbackAeClient client,
        IHostApplicationLifetime lifetime,
        ILogger<AeClientDemo> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunDemoAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task RunDemoAsync(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            OpcServerStatus status = await _client.GetStatusAsync(cancellationToken).ConfigureAwait(false);
            StatusMessage(_logger, status.State, status.VendorInfo, status.ServerVersion, null);

            await BrowseAreaAsync(_client, string.Empty, 0, cancellationToken).ConfigureAwait(false);
            await _client.EnableConditionsByAreaAsync(["Demo"], cancellationToken).ConfigureAwait(false);

            ConditionRef? conditionToAck = await SubscribeAndCaptureAckTargetAsync(cancellationToken).ConfigureAwait(false);
            if (conditionToAck is { } condition)
            {
                IReadOnlyList<AckResult> results = await _client.AcknowledgeAsync(
                    AckActor,
                    AckComment,
                    [condition],
                    cancellationToken).ConfigureAwait(false);

                foreach (AckResult result in results)
                {
                    AckResultMessage(_logger, result.Condition.ToString(), result.ResultId, null);
                }
            }

            await _client.DisableConditionsByAreaAsync(["Demo"], cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await _client.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task BrowseAreaAsync(
        IAeServer client,
        string areaQualifiedName,
        int depth,
        CancellationToken cancellationToken)
    {
        await foreach (AreaBrowseElement element in client.BrowseAreasAsync(areaQualifiedName, cancellationToken)
            .ConfigureAwait(false))
        {
            string kind = element.IsArea ? "Area" : "Source";
            BrowseNodeMessage(_logger, depth, kind, element.Name, element.QualifiedName, null);

            if (element.IsArea)
            {
                await BrowseAreaAsync(client, element.QualifiedName, depth + 1, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<ConditionRef?> SubscribeAndCaptureAckTargetAsync(CancellationToken cancellationToken)
    {
        IAeSubscription subscription = await _client.CreateSubscriptionAsync(
            active: true,
            bufferTimeMs: 100,
            maxBufferSize: 10,
            cancellationToken).ConfigureAwait(false);

        await using (subscription.ConfigureAwait(false))
        {
            await subscription.SetFilterAsync(
                new SubscriptionFilter { EventTypes = EventType.All, MinSeverity = 0, MaxSeverity = 1000 },
                cancellationToken).ConfigureAwait(false);

            ConditionRef? conditionToAck = null;
            await foreach (EventNotification notification in subscription.Events
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                EventMessage(_logger, notification.Source, notification.EventType, notification.Severity, notification.Message, null);
                if (notification.EventType == EventType.Condition &&
                    notification.AckRequired &&
                    notification.ConditionName is not null)
                {
                    conditionToAck = new ConditionRef(notification.Source, notification.ConditionName);
                }
            }

            await subscription.SetActiveAsync(false, cancellationToken).ConfigureAwait(false);
            return conditionToAck;
        }
    }
}
