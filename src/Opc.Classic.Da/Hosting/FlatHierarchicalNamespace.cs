// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default flat address-space implementation that returns no elements.
/// Used as the host's fallback when the user's <c>IOpcDaServer</c> doesn't
/// implement <see cref="IOpcAddressSpace"/>.
/// </summary>
public sealed class FlatHierarchicalNamespace : IOpcAddressSpace
{
    /// <summary>
    /// Per OPC DA 2.05a §4.6.4, FLAT = OPCNS_FLAT (1).
    /// </summary>
    public bool IsHierarchical => false;

    /// <inheritdoc />
    public Task<OpcBrowseResult> BrowseAsync(string? branchPath, OpcBrowseElementKind kind, CancellationToken cancellationToken = default)
    {
        _ = branchPath; _ = kind;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(OpcBrowseResult.Empty);
    }

    /// <inheritdoc />
    public Task<string> GetItemIdAsync(string? currentBranchPath, string itemDataId, CancellationToken cancellationToken = default)
    {
        _ = currentBranchPath;
        ArgumentException.ThrowIfNullOrEmpty(itemDataId);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(itemDataId);
    }
}
