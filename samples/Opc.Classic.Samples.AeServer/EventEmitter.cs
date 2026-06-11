// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Samples.AeServer;

public sealed class EventEmitter : BackgroundService
{
    private const int ErrorEveryTicks = 3;
    private const int ConditionEveryTicks = 2;
    private const string SimpleEventType = "Simple";
    private const string TrackingEventType = "Tracking";
    private const uint InfoSeverity = 100;
    private const uint WarningSeverity = 500;
    private const uint ErrorSeverity = 700;

    private static readonly TimeSpan EventPeriod = TimeSpan.FromSeconds(5);

    private static readonly Action<ILogger, string, string, uint, Exception?> HeartbeatEventFired =
        LoggerMessage.Define<string, string, uint>(
            LogLevel.Information,
            new EventId(1, nameof(HeartbeatEventFired)),
            "Heartbeat event fired: source={Source}, type={EventType}, severity={Severity}");

    private static readonly Action<ILogger, string, string, uint, Exception?> ErrorEventFired =
        LoggerMessage.Define<string, string, uint>(
            LogLevel.Warning,
            new EventId(2, nameof(ErrorEventFired)),
            "Simulated error event fired: source={Source}, type={EventType}, severity={Severity}");

    private static readonly Action<ILogger, string, string, string, string, uint, Exception?> ConditionTransitionFired =
        LoggerMessage.Define<string, string, string, string, uint>(
            LogLevel.Information,
            new EventId(3, nameof(ConditionTransitionFired)),
            "Condition transition event fired: source={Source}, condition={Condition}, type={EventType}, state={State}, severity={Severity}");

    private readonly ILogger<EventEmitter> _logger;

    public EventEmitter(ILogger<EventEmitter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(EventPeriod);
        var tick = 0;
        var conditionActive = false;

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                tick++;
                HeartbeatEventFired(_logger, "Server.Heartbeat", SimpleEventType, InfoSeverity, null);

                if (tick % ErrorEveryTicks == 0)
                {
                    ErrorEventFired(_logger, "Server.Errors", SimpleEventType, ErrorSeverity, null);
                }

                if (tick % ConditionEveryTicks == 0)
                {
                    conditionActive = !conditionActive;
                    ConditionTransitionFired(
                        _logger,
                        "Demo.Conditions",
                        "DemoCondition",
                        TrackingEventType,
                        conditionActive ? "Active" : "Inactive",
                        conditionActive ? WarningSeverity : InfoSeverity,
                        null);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
