// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Hda.Hosting;

namespace Opc.Classic.Integration.Tests.Support;

internal sealed class StubHdaServer : IOpcHdaServer
{
    private int _statusCallCount;
    private int _attributeCallCount;
    private int _validateCallCount;

    public int StatusCallCount => Volatile.Read(ref _statusCallCount);
    public int AttributeCallCount => Volatile.Read(ref _attributeCallCount);
    public int ValidateCallCount => Volatile.Read(ref _validateCallCount);
    public string[] LastItemIds { get; private set; } = [];
    public int[] LastClientHandles { get; private set; } = [];
    public int[] LastReleasedHandles { get; private set; } = [];

    public Task GetItemAttributesAsync(
        out int[] attributeIds,
        out string[] attributeNames,
        out string[] attributeDescriptions,
        out int[] attributeDataTypes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _attributeCallCount);
        attributeIds = [1, 2];
        attributeNames = ["DataType", "Description"];
        attributeDescriptions = ["Variant type", "Human text"];
        attributeDataTypes = [(int)VarType.VT_I4, (int)VarType.VT_BSTR];
        return Task.CompletedTask;
    }

    public Task GetAggregatesAsync(
        out int[] aggregateIds,
        out string[] aggregateNames,
        out string[] aggregateDescriptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        aggregateIds = [1, 4];
        aggregateNames = ["Interpolative", "Average"];
        aggregateDescriptions = ["Interpolated value", "Time average"];
        return Task.CompletedTask;
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _statusCallCount);
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(10),
            LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(11),
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 20, 1),
            MaxReturnValues = 500,
            VendorInfo = "Loopback HDA Stub Server",
        });
    }

    public Task<int[]> GetItemHandlesAsync(
        string[] itemIds,
        int[] clientHandles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastItemIds = itemIds;
        LastClientHandles = clientHandles;
        return Task.FromResult(itemIds.Select(static (_, index) => 501 + index).ToArray());
    }

    public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastReleasedHandles = serverHandles;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _validateCallCount);
        LastItemIds = itemIds;
        return Task.FromResult(itemIds
            .Select(static itemId => itemId == "Missing.Hda.Tag" ? OpcResultId.UnknownItemId.Code : OpcResultId.Ok.Code)
            .ToArray());
    }
}
