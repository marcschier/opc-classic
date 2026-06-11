// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;

namespace Opc.Classic.Samples.AeServer;

/// <summary>
/// Reference managed AE server used by the AE sample EXE. Implements both the
/// low-level <see cref="IOpcAeServer"/> contract (which routes the DCOM-mapped
/// methods) and the high-level <see cref="IAeServer"/> contract (which the
/// dispatcher uses to adapt CreateEventSubscription via
/// CreateEventSubscriptionAdapterAsync).
/// </summary>
public sealed class SampleAeServer : IAeServer, IOpcAeServer
{
    private const uint SimpleCategory = 1;
    private const uint TrackingCategory = 2;
    private const uint ConditionCategory = 3;
    private const string DemoConditionName = "DemoCondition";
    private const string DemoSource = "Demo.Conditions";

    private static readonly TimeSpan BrowseDelay = TimeSpan.FromMilliseconds(50);

    private static readonly Action<ILogger, Exception?> GetStatusMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(GetStatusAsync)),
        "GetStatus");

    private static readonly Action<ILogger, string, Exception?> BrowseMessage = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(2, nameof(BrowseAreasAsync)),
        "BrowseAreas: area={Area}");

    private static readonly Action<ILogger, string, string?, int, Exception?> AcknowledgeMessage =
        LoggerMessage.Define<string, string?, int>(
            LogLevel.Information,
            new EventId(3, nameof(AcknowledgeAsync)),
            "Acknowledge: actor={Actor}, comment={Comment}, count={Count}");

    private static readonly Action<ILogger, string, string, Exception?> ConditionStateMessage =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(4, "GetConditionStateAsync"),
            "GetConditionState: source={Source}, condition={Condition}");

    private static readonly DateTimeOffset StartupTime = DateTimeOffset.UtcNow;

    private readonly ConcurrentDictionary<ConditionRef, bool> _acknowledgedConditions = new();
    private readonly ILogger<SampleAeServer> _logger;

    public SampleAeServer(ILogger<SampleAeServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public event EventHandler<EventArgs>? ServerShutdown;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetStatusMessage(_logger, null);

        var now = DateTimeOffset.UtcNow;
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = StartupTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = 0,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Opc.Classic .NET AE Sample",
        };

        return Task.FromResult(status);
    }

    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0x1F);
    }

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
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> DisableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areaQualifiedNames);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IAeSubscription subscription = new SampleAeSubscription(active, CreateDemoEvents, bufferTimeMs, maxBufferSize);
        return Task.FromResult(subscription);
    }

    // Direct override of IOPCEventServer.GetConditionStateAsync so the
    // DCOM-mapped IOPCEventServer::GetConditionState call returns a usable
    // payload (the dispatcher does not have an IAeServer adapter for this
    // method).
    Task<OpcConditionState> IOPCEventServer.GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConditionStateMessage(_logger, source ?? string.Empty, conditionName ?? string.Empty, null);

        var now = DateTimeOffset.UtcNow;
        var state = new OpcConditionState(
            state: 0x0003, // OPC_CONDITION_ENABLED | OPC_CONDITION_ACTIVE
            activeSubCondition: "Active",
            activeSubConditionDefinition: "Sample sub-condition",
            activeSubConditionSeverity: 500,
            activeSubConditionDescription: $"Condition '{conditionName}' on source '{source}' is active",
            quality: OpcQuality.Good,
            lastAckTime: now.AddMinutes(-5),
            subConditionLastActive: now.AddSeconds(-30),
            conditionLastActive: now.AddSeconds(-30),
            conditionLastInactive: now.AddMinutes(-10),
            acknowledgerId: null,
            comment: null,
            subConditionNames: ["Active"],
            subConditionDefinitions: ["Sample sub-condition"],
            subConditionSeverities: [500],
            subConditionDescriptions: ["Active sub-condition"],
            eventAttributes: [],
            errors: []);
        return Task.FromResult(state);
    }

    // Direct override of IOPCEventServer.AckConditionAsync. The dispatcher
    // does not adapt this to IAeServer.AcknowledgeAsync because the DCOM
    // signature uses positional cookie/source arrays rather than the
    // managed ConditionRef list.
    Task<int[]> IOPCEventServer.AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sources);
        cancellationToken.ThrowIfCancellationRequested();
        // Mirror IAeServer.AcknowledgeAsync semantics: accept any condition
        // and report S_OK per entry.
        AcknowledgeMessage(_logger, acknowledgerId ?? "<anonymous>", comment, sources.Length, null);
        int[] results = new int[sources.Length];
        return Task.FromResult(results);
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
