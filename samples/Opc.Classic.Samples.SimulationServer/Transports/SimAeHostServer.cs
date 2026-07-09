// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Dcom;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// A managed OPC AE server, backed by the shared <see cref="SimulatedPlantModel" />, that the
/// <see cref="OpcAeServerHost" /> serves over the real cross-platform transport. It answers
/// the AE "root" calls (status, available filters, event categories/conditions/attributes,
/// translate-to-item-ids) derived from the model's analog alarm tags. Subscription, area
/// browser, and acknowledge tearoffs require object-IPID routing that the AE host does not yet
/// expose over the wire, so those throw <see cref="NotSupportedException" /> (matching the
/// host's current loopback capability).
/// </summary>
public sealed class SimAeHostServer : IOpcAeServer
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    private const int ProcessCategory = 1001;
    private readonly SimulatedPlantModel _model;

    /// <summary>Initializes a new instance of the <see cref="SimAeHostServer" /> class.</summary>
    /// <param name="model">The shared deterministic plant model to serve.</param>
    public SimAeHostServer(SimulatedPlantModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = _model.ServerVersion,
            VendorInfo = _model.VendorInfo + " (AE)",
            GroupCount = 0,
            BandWidth = 0,
        });
    }

    /// <inheritdoc />
    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Area, Source, Category, Severity, EventType.
        return Task.FromResult(0x1F);
    }

    /// <inheritdoc />
    public Task QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken = default)
    {
        _ = eventType;
        cancellationToken.ThrowIfCancellationRequested();
        eventCategories = [ProcessCategory];
        eventCategoryDescriptions = ["Process Alarm"];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
    {
        _ = eventCategory;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new[] { "High", "Low" });
    }

    /// <inheritdoc />
    public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conditionName);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(conditionName == "High" ? new[] { "High", "HighHigh" } : new[] { "Low", "LowLow" });
    }

    /// <inheritdoc />
    public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
    {
        _ = source;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new[] { "High", "Low" });
    }

    /// <inheritdoc />
    public Task QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken = default)
    {
        _ = eventCategory;
        cancellationToken.ThrowIfCancellationRequested();
        attributeIds = [501, 502];
        attributeDescriptions = ["Limit", "EngineeringUnits"];
        attributeTypes = [(ushort)VarType.VT_R8, (ushort)VarType.VT_BSTR];
        return Task.CompletedTask;
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
        ArgumentNullException.ThrowIfNull(associatedAttributeIds);
        cancellationToken.ThrowIfCancellationRequested();
        attributeItemIds = [.. associatedAttributeIds.Select(id => source + "." + id.ToString(System.Globalization.CultureInfo.InvariantCulture))];
        nodeNames = [.. associatedAttributeIds.Select(static _ => string.Empty)];
        classIds = [.. associatedAttributeIds.Select(static _ => Guid.Empty)];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OpcConditionState> GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE condition-state tearoffs are not exposed over the transport yet.");

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
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE event-subscription tearoffs are not exposed over the transport yet.");

    /// <inheritdoc />
    public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE condition enable/disable is not exposed over the transport yet.");

    /// <inheritdoc />
    public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE condition enable/disable is not exposed over the transport yet.");

    /// <inheritdoc />
    public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE condition enable/disable is not exposed over the transport yet.");

    /// <inheritdoc />
    public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE condition enable/disable is not exposed over the transport yet.");

    /// <inheritdoc />
    public Task<int[]> AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE acknowledge is not exposed over the transport yet.");

    /// <inheritdoc />
    public Task CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        out IOPCEventAreaBrowser areaBrowser,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("AE area-browser tearoffs are not exposed over the transport yet.");
}
