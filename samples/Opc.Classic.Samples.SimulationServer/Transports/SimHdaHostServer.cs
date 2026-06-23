// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Hda.Hosting;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// A managed OPC HDA server, backed by the shared <see cref="SimulatedPlantModel" />, that the
/// <see cref="OpcHdaServerHost" /> serves over the real cross-platform transport. It answers
/// the HDA "root" calls (status, item attributes, aggregates, item-handle management, and
/// item-id validation) against the model's deterministic historian. Raw/processed read
/// tearoffs require object-IPID routing not yet exposed over the wire.
/// </summary>
public sealed class SimHdaHostServer : IOpcHdaServer
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    private readonly SimulatedPlantModel _model;
    private int _nextHandle = 0x5000;

    /// <summary>Initializes a new instance of the <see cref="SimHdaHostServer" /> class.</summary>
    /// <param name="model">The shared deterministic plant model to serve.</param>
    public SimHdaHostServer(SimulatedPlantModel model)
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
            Spec = OpcStatusSpec.Hda,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = _model.ServerVersion,
            MaxReturnValues = 500,
            VendorInfo = _model.VendorInfo + " (HDA)",
        });
    }

    /// <inheritdoc />
    public Task GetItemAttributesAsync(
        out int[] attributeIds,
        out string[] attributeNames,
        out string[] attributeDescriptions,
        out int[] attributeDataTypes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        attributeIds = [1, 2];
        attributeNames = ["DataType", "Description"];
        attributeDescriptions = ["Variant type", "Human text"];
        attributeDataTypes = [(int)VarType.VT_I4, (int)VarType.VT_BSTR];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task GetAggregatesAsync(
        out int[] aggregateIds,
        out string[] aggregateNames,
        out string[] aggregateDescriptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        aggregateIds = [1, 4, 5, 6];
        aggregateNames = ["Interpolative", "Average", "Minimum", "Maximum"];
        aggregateDescriptions = ["Interpolated value", "Time average", "Minimum value", "Maximum value"];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> GetItemHandlesAsync(
        string[] itemIds,
        int[] clientHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        var handles = new int[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            handles[i] = _model.TryGetTag(itemIds[i], out _)
                ? Interlocked.Increment(ref _nextHandle)
                : 0;
        }

        return Task.FromResult(handles);
    }

    /// <inheritdoc />
    public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Array.ConvertAll(serverHandles, static _ => OpcResultId.Ok.Code));
    }

    /// <inheritdoc />
    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Array.ConvertAll(
            itemIds,
            itemId => _model.TryGetTag(itemId, out _) ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code));
    }
}
