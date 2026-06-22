// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Testing;
using Channels = System.Threading.Channels;

namespace Opc.Classic.Samples.SimulationServer.Ae;

/// <summary>
/// Managed in-memory OPC AE server that projects the shared simulated plant model as alarm areas and conditions.
/// </summary>
public sealed class SimAeServer : IOpcAeServer, IAeServer
{
    private const uint SimpleCategory = 0x1001u;
    private const uint ConditionCategory = 0x1002u;
    private const int AreaAttribute = 10;
    private const int LimitAttribute = 11;
    private const int UnitsAttribute = 12;
    private const int ValueAttribute = 13;

    private readonly DateTimeOffset _startup = DateTimeOffset.UtcNow;
    private readonly SimulatedPlantModel _model;
    private readonly OpcAeServerDispatcher _serverDispatcher;
    private readonly IReadOnlyList<AlarmCondition> _conditions;
    private readonly IReadOnlyDictionary<string, AlarmCondition> _conditionsByKey;
    private readonly ConcurrentDictionary<string, AckInfo> _acknowledgements = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<SimAeServer> _logger;

    /// <summary>Initializes a new instance of the <see cref="SimAeServer" /> class.</summary>
    /// <param name="model">Shared deterministic plant model.</param>
    /// <param name="loggerFactory">Logger factory used for AE diagnostics.</param>
    public SimAeServer(SimulatedPlantModel model, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _model = model;
        _logger = loggerFactory.CreateLogger<SimAeServer>();
        _conditions = BuildConditions(model);
        _conditionsByKey = _conditions.ToDictionary(static condition => ConditionKey(condition.Source, condition.ConditionName), StringComparer.OrdinalIgnoreCase);
        _serverDispatcher = new OpcAeServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? ServerShutdown;

    /// <summary>Gets the in-memory call channel used by generated AE proxies.</summary>
    public InMemoryCallChannel Channel { get; }

    /// <summary>Gets the most recent acknowledger id received by the simulation server.</summary>
    public string? LastAckActor { get; private set; }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = _startup,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = _model.ServerVersion,
            VendorInfo = _model.VendorInfo,
        });
    }

    /// <inheritdoc />
    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0x1F);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        string area = areaQualifiedName ?? string.Empty;
        foreach (AreaBrowseElement element in BrowseAreaElements(area))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return element;
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(EventType eventTypes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var categories = new List<uint>();
        if ((eventTypes & EventType.Simple) != EventType.None)
        {
            categories.Add(SimpleCategory);
        }

        if ((eventTypes & EventType.Condition) != EventType.None)
        {
            categories.Add(ConditionCategory);
        }

        return Task.FromResult<IReadOnlyList<uint>>(categories);
    }

    /// <inheritdoc />
    public Task QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EventType requested = (EventType)eventType;
        var categories = new List<int>();
        var descriptions = new List<string>();
        if ((requested & EventType.Simple) != EventType.None)
        {
            categories.Add(unchecked((int)SimpleCategory));
            descriptions.Add("Simulation simple events");
        }

        if ((requested & EventType.Condition) != EventType.None)
        {
            categories.Add(unchecked((int)ConditionCategory));
            descriptions.Add("Simulation analog limit alarms");
        }

        eventCategories = [.. categories];
        eventCategoryDescriptions = [.. descriptions];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventCategory;
        attributeIds = [AreaAttribute, LimitAttribute, UnitsAttribute, ValueAttribute];
        attributeDescriptions = ["Area", "Limit", "Units", "Current Value"];
        attributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> QueryConditionNamesAsync(uint eventCategory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(eventCategory == ConditionCategory ? UniqueConditionNames() : []);
    }

    /// <inheritdoc />
    public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(unchecked((uint)eventCategory) == ConditionCategory ? UniqueConditionNames().ToArray() : Array.Empty<string>());
    }

    /// <inheritdoc />
    public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conditionName);
        cancellationToken.ThrowIfCancellationRequested();

        AlarmCondition? condition = _conditions.FirstOrDefault(candidate => string.Equals(candidate.ConditionName, conditionName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(condition is null ? Array.Empty<string>() : SubConditionNames(condition));
    }

    /// <inheritdoc />
    public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_conditions
            .Where(condition => string.Equals(condition.Source, source, StringComparison.OrdinalIgnoreCase))
            .Select(static condition => condition.ConditionName)
            .ToArray());
    }

    /// <inheritdoc />
    public Task TranslateToItemIDsAsync(
        string source,
        int eventCategory,
        string conditionName,
        string subconditionName,
        int[] associatedAttributeIds,
        out string[] attributeItemIds,
        out string[] nodeNames,
        out Guid[] classIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(conditionName);
        ArgumentNullException.ThrowIfNull(subconditionName);
        ArgumentNullException.ThrowIfNull(associatedAttributeIds);
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventCategory;

        attributeItemIds = associatedAttributeIds.Select(id => source + ".AeAttr" + id.ToString(CultureInfo.InvariantCulture)).ToArray();
        nodeNames = associatedAttributeIds.Select(static _ => "SimulationAeNode").ToArray();
        classIds = associatedAttributeIds.Select(static _ => Guid.Empty).ToArray();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OpcConditionState> GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(conditionName);
        ArgumentNullException.ThrowIfNull(attributeIds);
        cancellationToken.ThrowIfCancellationRequested();

        AlarmCondition condition = ResolveCondition(source, conditionName);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ConditionSnapshot snapshot = Snapshot(condition, now);
        _acknowledgements.TryGetValue(ConditionKey(source, conditionName), out AckInfo? ack);
        OpcVariant[] attributes = attributeIds.Select(id => AttributeValue(condition, snapshot.Value, id)).ToArray();

        return Task.FromResult(new OpcConditionState(
            state: (ushort)snapshot.State,
            activeSubCondition: snapshot.ActiveSubCondition,
            activeSubConditionDefinition: snapshot.Definition,
            activeSubConditionSeverity: snapshot.Severity,
            activeSubConditionDescription: snapshot.Description,
            quality: OpcQuality.Good,
            lastAckTime: ack?.Time ?? DateTimeOffset.UnixEpoch,
            subConditionLastActive: snapshot.Active ? now : DateTimeOffset.UnixEpoch,
            conditionLastActive: snapshot.Active ? now : DateTimeOffset.UnixEpoch,
            conditionLastInactive: snapshot.Active ? DateTimeOffset.UnixEpoch : now,
            acknowledgerId: ack?.Actor,
            comment: ack?.Comment,
            subConditionNames: SubConditionNames(condition),
            subConditionDefinitions: SubConditionDefinitions(condition),
            subConditionSeverities: SubConditionSeverities(condition),
            subConditionDescriptions: SubConditionDescriptions(condition),
            eventAttributes: attributes,
            errors: attributeIds.Select(static _ => OpcResultId.Ok.Code).ToArray()));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor,
        string? comment,
        IReadOnlyList<ConditionRef> conditions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(conditions);
        cancellationToken.ThrowIfCancellationRequested();

        LastAckActor = actor;
        var results = new AckResult[conditions.Count];
        for (int i = 0; i < conditions.Count; i++)
        {
            ConditionRef condition = conditions[i];
            OpcResultId result = _conditionsByKey.ContainsKey(ConditionKey(condition.Source, condition.ConditionName))
                ? OpcResultId.Ok
                : OpcResultId.UnknownItemId;
            if (result.IsSuccess)
            {
                _acknowledgements[ConditionKey(condition.Source, condition.ConditionName)] = new AckInfo(actor, comment, DateTimeOffset.UtcNow);
            }

            results[i] = new AckResult { Condition = condition, ResultId = result };
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("AE acknowledge by {Actor} for {Count} condition(s).", actor, conditions.Count);
        }

        return Task.FromResult<IReadOnlyList<AckResult>>(results);
    }

    /// <inheritdoc />
    public Task<int[]> AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acknowledgerId);
        ArgumentNullException.ThrowIfNull(comment);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(conditionNames);
        ArgumentNullException.ThrowIfNull(activeTimes);
        ArgumentNullException.ThrowIfNull(cookies);
        cancellationToken.ThrowIfCancellationRequested();

        LastAckActor = acknowledgerId;
        int count = Math.Min(dwCount, Math.Min(sources.Length, conditionNames.Length));
        var results = new int[count];
        for (int i = 0; i < count; i++)
        {
            string key = ConditionKey(sources[i], conditionNames[i]);
            bool known = _conditionsByKey.ContainsKey(key);
            if (known)
            {
                _acknowledgements[key] = new AckInfo(acknowledgerId, comment, DateTimeOffset.UtcNow);
            }

            results[i] = known ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code;
        }

        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task<OpcResultId> EnableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areaQualifiedNames);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<OpcResultId> DisableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areaQualifiedNames);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areas);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(areas);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = bufferTimeMs;
        _ = maxBufferSize;
        return Task.FromResult<IAeSubscription>(new SimAeSubscription(this, active));
    }

    /// <inheritdoc />
    public Task CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out IOPCEventSubscriptionMgt subscription,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = active;
        _ = clientSubscription;
        _ = requestedInterfaceId;
        subscription = default!;
        revisedBufferTime = Math.Max(bufferTime, 50);
        revisedMaxSize = Math.Max(maxSize, 1);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        out IOPCEventAreaBrowser areaBrowser,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = requestedInterfaceId;
        areaBrowser = default!;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    private static IReadOnlyList<AlarmCondition> BuildConditions(SimulatedPlantModel model)
    {
        var conditions = new List<AlarmCondition>();
        int cookie = 1000;
        foreach (SimulatedTag tag in model.Tags)
        {
            if (!IsAlarmTag(tag))
            {
                continue;
            }

            if (!double.IsNaN(tag.HighAlarm))
            {
                conditions.Add(new AlarmCondition(tag, tag.BranchPath, tag.ItemId, tag.Name + "High", IsHigh: true, tag.HighAlarm, cookie++));
            }

            if (!double.IsNaN(tag.LowAlarm))
            {
                conditions.Add(new AlarmCondition(tag, tag.BranchPath, tag.ItemId, tag.Name + "Low", IsHigh: false, tag.LowAlarm, cookie++));
            }
        }

        return conditions;
    }

    private static bool IsAlarmTag(SimulatedTag tag) =>
        tag.BranchPath.StartsWith("Plant.Reactor", StringComparison.OrdinalIgnoreCase)
        && (!double.IsNaN(tag.HighAlarm) || !double.IsNaN(tag.LowAlarm));

    private static string ConditionKey(string source, string conditionName) => source + "::" + conditionName;

    private static string[] SubConditionNames(AlarmCondition condition) =>
        condition.IsHigh ? ["High", "HighHigh"] : ["Low", "LowLow"];

    private static string?[] SubConditionDefinitions(AlarmCondition condition) =>
        condition.IsHigh
            ? ["Above high alarm limit", "At or above configured maximum"]
            : ["Below low alarm limit", "At or below configured minimum"];

    private static uint[] SubConditionSeverities(AlarmCondition condition) =>
        condition.IsHigh ? [700u, 900u] : [650u, 850u];

    private static string?[] SubConditionDescriptions(AlarmCondition condition) =>
        condition.IsHigh
            ? ["High process alarm", "High-high process alarm"]
            : ["Low process alarm", "Low-low process alarm"];

    private static OpcVariant AttributeValue(AlarmCondition condition, double value, int attributeId) =>
        attributeId switch
        {
            AreaAttribute => OpcVariant.FromString(condition.Area),
            LimitAttribute => OpcVariant.FromDouble(condition.Limit),
            UnitsAttribute => OpcVariant.FromString(condition.Tag.Units ?? string.Empty),
            ValueAttribute => OpcVariant.FromDouble(value),
            _ => OpcVariant.FromString(string.Empty),
        };

    private IReadOnlyList<AreaBrowseElement> BrowseAreaElements(string areaQualifiedName)
    {
        if (areaQualifiedName.Length == 0)
        {
            return [new AreaBrowseElement { Name = "Plant", QualifiedName = "Plant", IsArea = true }];
        }

        string[] areaChildren = _conditions
            .Select(static condition => condition.Area)
            .Where(area => IsImmediateChildArea(areaQualifiedName, area))
            .Select(area => area[(areaQualifiedName.Length == 0 ? 0 : areaQualifiedName.Length + 1)..])
            .Select(remainder =>
            {
                int dot = remainder.IndexOf('.', StringComparison.Ordinal);
                return dot < 0 ? remainder : remainder[..dot];
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static conditionName => conditionName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var elements = new List<AreaBrowseElement>();
        foreach (string child in areaChildren)
        {
            string qualified = areaQualifiedName.Length == 0 ? child : areaQualifiedName + "." + child;
            elements.Add(new AreaBrowseElement { Name = child, QualifiedName = qualified, IsArea = true });
        }

        foreach (AlarmCondition condition in _conditions
            .Where(condition => string.Equals(condition.Area, areaQualifiedName, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(static condition => condition.Source, StringComparer.OrdinalIgnoreCase))
        {
            elements.Add(new AreaBrowseElement { Name = condition.Tag.Name, QualifiedName = condition.Source, IsSource = true });
        }

        return elements;
    }

    private static bool IsImmediateChildArea(string parent, string candidate)
    {
        if (candidate.Length <= parent.Length)
        {
            return false;
        }

        return parent.Length == 0
            || candidate.StartsWith(parent + ".", StringComparison.OrdinalIgnoreCase);
    }

    private IReadOnlyList<string> UniqueConditionNames() =>
        _conditions
            .Select(static condition => condition.ConditionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private AlarmCondition ResolveCondition(string source, string conditionName) =>
        _conditionsByKey.TryGetValue(ConditionKey(source, conditionName), out AlarmCondition? condition)
            ? condition
            : throw new OpcException(OpcResultId.UnknownItemId);

    private ConditionSnapshot Snapshot(AlarmCondition condition, DateTimeOffset timestamp)
    {
        double value = Convert.ToDouble(_model.ValueAt(condition.Tag, timestamp), CultureInfo.InvariantCulture);
        bool warningActive = condition.IsHigh ? value >= condition.Limit : value <= condition.Limit;
        bool severeActive = condition.IsHigh ? value >= condition.Tag.Maximum : value <= condition.Tag.Minimum;
        bool active = warningActive || severeActive;
        bool acknowledged = _acknowledgements.ContainsKey(ConditionKey(condition.Source, condition.ConditionName));
        var state = ConditionState.Enabled
            | (active ? ConditionState.Active : ConditionState.None)
            | (acknowledged ? ConditionState.Acknowledged : ConditionState.None);
        string? subCondition = active ? ActiveSubCondition(condition, severeActive) : null;
        uint severity = active ? ActiveSeverity(condition, severeActive) : 1u;
        string? definition = active ? ActiveDefinition(condition, severeActive) : null;
        string? description = active ? ActiveDescription(condition, severeActive) : null;
        return new ConditionSnapshot(value, active, state, subCondition, definition, severity, description);
    }

    private static string ActiveSubCondition(AlarmCondition condition, bool severeActive) =>
        condition.IsHigh
            ? severeActive ? "HighHigh" : "High"
            : severeActive ? "LowLow" : "Low";

    private static uint ActiveSeverity(AlarmCondition condition, bool severeActive) =>
        condition.IsHigh
            ? severeActive ? 900u : 700u
            : severeActive ? 850u : 650u;

    private static string ActiveDefinition(AlarmCondition condition, bool severeActive) =>
        condition.IsHigh
            ? severeActive ? "At or above configured maximum" : "Above high alarm limit"
            : severeActive ? "At or below configured minimum" : "Below low alarm limit";

    private static string ActiveDescription(AlarmCondition condition, bool severeActive) =>
        condition.IsHigh
            ? severeActive ? "High-high process alarm" : "High process alarm"
            : severeActive ? "Low-low process alarm" : "Low process alarm";

    private IEnumerable<EventNotification> CreateNotifications(string message, SubscriptionFilter filter)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (AlarmCondition condition in _conditions)
        {
            ConditionSnapshot snapshot = Snapshot(condition, now);
            EventNotification notification = CreateNotification(condition, snapshot, now, message);
            if (Matches(filter, notification))
            {
                yield return notification;
            }
        }
    }

    private static EventNotification CreateNotification(AlarmCondition condition, ConditionSnapshot snapshot, DateTimeOffset now, string message) => new()
    {
        Source = condition.Source,
        Time = now,
        Message = condition.Source + " " + condition.ConditionName + ": " + message,
        Severity = (int)(snapshot.Active ? snapshot.Severity : NominalSeverity(condition)),
        EventCategory = ConditionCategory,
        EventType = EventType.Condition,
        ConditionName = condition.ConditionName,
        SubConditionName = snapshot.ActiveSubCondition ?? (condition.IsHigh ? "High" : "Low"),
        NewState = snapshot.State,
        AckRequired = snapshot.Active,
        ActiveTime = snapshot.Active ? now : DateTimeOffset.UnixEpoch,
        Cookie = condition.Cookie,
        Quality = OpcQuality.Good,
        Attributes = new Dictionary<uint, object?>
        {
            [AreaAttribute] = condition.Area,
            [LimitAttribute] = condition.Limit,
            [UnitsAttribute] = condition.Tag.Units,
            [ValueAttribute] = snapshot.Value,
        },
    };

    private static uint NominalSeverity(AlarmCondition condition) => condition.IsHigh ? 700u : 650u;

    private static bool Matches(SubscriptionFilter filter, EventNotification notification)
    {
        if ((filter.EventTypes & notification.EventType) == EventType.None)
        {
            return false;
        }

        if (notification.Severity < filter.MinSeverity || notification.Severity > filter.MaxSeverity)
        {
            return false;
        }

        if (filter.EventCategories.Count > 0 && !filter.EventCategories.Contains(notification.EventCategory))
        {
            return false;
        }

        if (filter.Sources.Count > 0 && !filter.Sources.Any(source => string.Equals(source, notification.Source, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return filter.Areas.Count == 0 || filter.Areas.Any(area => notification.Source.StartsWith(area + ".", StringComparison.OrdinalIgnoreCase));
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
        interfaceId == IOPCEventServer.InterfaceId
            ? _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken)
            : Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));

    private sealed record AckInfo(string Actor, string? Comment, DateTimeOffset Time);

    private sealed record AlarmCondition(
        SimulatedTag Tag,
        string Area,
        string Source,
        string ConditionName,
        bool IsHigh,
        double Limit,
        int Cookie);

    private sealed record ConditionSnapshot(
        double Value,
        bool Active,
        ConditionState State,
        string? ActiveSubCondition,
        string? Definition,
        uint Severity,
        string? Description);

    private sealed class SimAeSubscription : IAeSubscription
    {
        private readonly SimAeServer _server;
        private readonly Channels.Channel<EventNotification> _events = Channels.Channel.CreateUnbounded<EventNotification>();

        /// <summary>Initializes a new instance of the <see cref="SimAeSubscription" /> class.</summary>
        /// <param name="server">Owning AE simulation server.</param>
        /// <param name="active">Whether the subscription starts active.</param>
        public SimAeSubscription(SimAeServer server, bool active)
        {
            _server = server;
            Active = active;
        }

        /// <inheritdoc />
        public bool Active { get; private set; }

        /// <inheritdoc />
        public SubscriptionFilter Filter { get; private set; } = new();

        /// <inheritdoc />
        public IAsyncEnumerable<EventNotification> Events => ReadAllAsync();

        /// <inheritdoc />
        public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Active = active;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(filter);
            cancellationToken.ThrowIfCancellationRequested();
            Filter = filter;
            WriteSnapshot("Filter applied condition state");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteSnapshot("Refresh condition state");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task CancelRefreshAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _ = _events.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }

        private void WriteSnapshot(string message)
        {
            if (!Active)
            {
                return;
            }

            foreach (EventNotification notification in _server.CreateNotifications(message, Filter))
            {
                _ = _events.Writer.TryWrite(notification);
            }
        }

        private async IAsyncEnumerable<EventNotification> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await _events.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (_events.Reader.TryRead(out EventNotification? item))
                {
                    yield return item;
                }
            }
        }
    }
}
