// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Samples.AeServer;

namespace Opc.Classic.Samples.AeClient;

internal sealed class InProcessAeServer : IAeServer, IOpcAeServer
{
    private const uint SimpleCategory = 1;
    private const uint TrackingCategory = 2;
    private const uint ConditionCategory = 3;
    private const string DemoConditionName = "DemoCondition";
    private const string DemoSource = "Demo.Conditions";

    private static readonly TimeSpan BrowseDelay = TimeSpan.FromMilliseconds(50);

    private static readonly Action<ILogger, string, Exception?> BrowseMessage = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(1, nameof(BrowseAreasAsync)),
        "BrowseAreas: area={Area}");

    private static readonly Action<ILogger, string, string?, int, Exception?> AcknowledgeMessage =
        LoggerMessage.Define<string, string?, int>(
            LogLevel.Information,
            new EventId(2, nameof(AcknowledgeAsync)),
            "Acknowledge: actor={Actor}, comment={Comment}, count={Count}");

    private static readonly Action<ILogger, string, Exception?> EnableAreaMessage = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(3, nameof(EnableConditionsByAreaAsync)),
        "EnableConditionsByArea: area={Area}");

    private static readonly Action<ILogger, string, Exception?> DisableAreaMessage = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(4, nameof(DisableConditionsByAreaAsync)),
        "DisableConditionsByArea: area={Area}");

    private readonly ConcurrentDictionary<ConditionRef, bool> _acknowledgedConditions = new();
    private readonly SampleAeServer _statusServer;
    private readonly ILogger<InProcessAeServer> _logger;

    public InProcessAeServer(SampleAeServer statusServer, ILogger<InProcessAeServer> logger)
    {
        _statusServer = statusServer ?? throw new ArgumentNullException(nameof(statusServer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public event EventHandler<EventArgs>? ServerShutdown;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
        _statusServer.GetStatusAsync(cancellationToken);

    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
        _statusServer.QueryAvailableFiltersAsync(cancellationToken);

    public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areaQualifiedName);

        BrowseMessage(_logger, areaQualifiedName.Length == 0 ? "<root>" : areaQualifiedName, null);

        foreach (AreaBrowseElement element in GetAreaElements(areaQualifiedName))
        {
            await Task.Delay(BrowseDelay, cancellationToken).ConfigureAwait(false);
            yield return element;
        }
    }

    public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(
        EventType eventTypes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var categories = new List<uint>(3);
        if ((eventTypes & EventType.Simple) != 0)
        {
            categories.Add(SimpleCategory);
        }

        if ((eventTypes & EventType.Tracking) != 0)
        {
            categories.Add(TrackingCategory);
        }

        if ((eventTypes & EventType.Condition) != 0)
        {
            categories.Add(ConditionCategory);
        }

        return Task.FromResult<IReadOnlyList<uint>>(categories);
    }

    public Task<IReadOnlyList<string>> QueryConditionNamesAsync(
        uint eventCategory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<string> names = eventCategory == ConditionCategory
            ? [DemoConditionName]
            : [];
        return Task.FromResult(names);
    }

    public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor,
        string? comment,
        IReadOnlyList<ConditionRef> conditions,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        ArgumentNullException.ThrowIfNull(conditions);
        cancellationToken.ThrowIfCancellationRequested();

        AcknowledgeMessage(_logger, actor, comment, conditions.Count, null);

        var results = new List<AckResult>(conditions.Count);
        foreach (ConditionRef condition in conditions)
        {
            _acknowledgedConditions[condition] = true;
            results.Add(new AckResult { Condition = condition });
        }

        return Task.FromResult<IReadOnlyList<AckResult>>(results);
    }

    public Task<OpcResultId> EnableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areaQualifiedNames);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (string area in areaQualifiedNames)
        {
            EnableAreaMessage(_logger, area, null);
        }

        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> DisableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areaQualifiedNames);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (string area in areaQualifiedNames)
        {
            DisableAreaMessage(_logger, area, null);
        }

        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IAeSubscription subscription = new InProcessAeSubscription(active, CreateDemoEvents, bufferTimeMs, maxBufferSize);
        return Task.FromResult(subscription);
    }

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    private static IEnumerable<AreaBrowseElement> GetAreaElements(string areaQualifiedName)
    {
        if (areaQualifiedName.Length == 0)
        {
            yield return new AreaBrowseElement { Name = "Server", QualifiedName = "Server", IsArea = true };
            yield return new AreaBrowseElement { Name = "Demo", QualifiedName = "Demo", IsArea = true };
            yield break;
        }

        if (string.Equals(areaQualifiedName, "Server", StringComparison.Ordinal))
        {
            yield return new AreaBrowseElement { Name = "Heartbeat", QualifiedName = "Server.Heartbeat", IsSource = true };
            yield return new AreaBrowseElement { Name = "Errors", QualifiedName = "Server.Errors", IsSource = true };
            yield break;
        }

        if (string.Equals(areaQualifiedName, "Demo", StringComparison.Ordinal))
        {
            yield return new AreaBrowseElement { Name = "Conditions", QualifiedName = DemoSource, IsSource = true };
        }
    }

    private static IReadOnlyList<EventNotification> CreateDemoEvents()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset activeTime = now.AddSeconds(-5);

        return
        [
            new EventNotification
            {
                Source = "Server.Heartbeat",
                Time = now,
                Message = "Heartbeat from the in-process AE server",
                Severity = 100,
                EventCategory = SimpleCategory,
                EventType = EventType.Simple,
            },
            new EventNotification
            {
                Source = DemoSource,
                Time = now.AddMilliseconds(250),
                Message = "Demo condition became active and requires acknowledgement",
                Severity = 500,
                EventCategory = ConditionCategory,
                EventType = EventType.Condition,
                ConditionName = DemoConditionName,
                SubConditionName = "Active",
                NewState = ConditionState.Active | ConditionState.Enabled,
                AckRequired = true,
                ActiveTime = activeTime,
                Cookie = 1001,
                Quality = OpcQuality.Good,
            },
            new EventNotification
            {
                Source = DemoSource,
                Time = now.AddMilliseconds(500),
                Message = "Sample operator inspected the condition",
                Severity = 250,
                EventCategory = TrackingCategory,
                EventType = EventType.Tracking,
                Actor = "sample-operator",
            },
        ];
    }
}
