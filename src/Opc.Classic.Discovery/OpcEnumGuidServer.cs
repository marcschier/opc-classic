// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom;

namespace Opc.Classic.Discovery.Dcom;

internal sealed class OpcEnumGuidServer : IOPCEnumGUIDServer
{
    private readonly IReadOnlyList<Guid> _classIds;
    private readonly Guid _interfaceId;
    private readonly Func<IReadOnlyList<Guid>, int, Guid, IOpcInterfaceRef> _cloneFactory;
    private int _index;

    public OpcEnumGuidServer(
        IReadOnlyList<Guid> classIds,
        Guid interfaceId,
        Func<IReadOnlyList<Guid>, int, Guid, IOpcInterfaceRef> cloneFactory,
        int index = 0)
    {
        _classIds = classIds ?? throw new ArgumentNullException(nameof(classIds));
        _interfaceId = interfaceId;
        _cloneFactory = cloneFactory ?? throw new ArgumentNullException(nameof(cloneFactory));
        _index = Math.Clamp(index, 0, classIds.Count);
    }

    public Task<OpcEnumGuidNextResult> NextAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        int fetched = Math.Min(count, _classIds.Count - _index);
        var batch = new Guid[fetched];
        for (int i = 0; i < batch.Length; i++)
        {
            batch[i] = _classIds[_index++];
        }

        return Task.FromResult(new OpcEnumGuidNextResult(batch, fetched));
    }

    public Task<int> SkipAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        int skipped = Math.Min(count, _classIds.Count - _index);
        _index += skipped;
        return Task.FromResult(skipped);
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _index = 0;
        return Task.CompletedTask;
    }

    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_cloneFactory(_classIds, _index, _interfaceId));
    }
}
