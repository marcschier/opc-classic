// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dx.Dcom;

public sealed partial class IOPCConfigurationClientProxy
{
    /// <inheritdoc />
    public async Task<string[]> QueryDXConnectionNamesAsync(
        string browsePath,
        string[] connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        DxConnection[] masks = (connectionMasks ?? [])
            .Select(name => new DxConnection(name: name, mask: (int)DxMask.Name))
            .ToArray();
        DxConnectionQueryResult result = await QueryDXConnectionsAsync(
            browsePath,
            masks,
            recursive,
            cancellationToken).ConfigureAwait(false);
        return result.Connections.Select(connection => connection.Name ?? string.Empty).ToArray();
    }
}
