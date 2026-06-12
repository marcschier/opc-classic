//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Abstraction for an OPC DA server's address space. Implementations expose
/// a hierarchical namespace of branches + items reachable via
/// <see cref="DefaultBrowseServerAddressSpace"/> (DA 2.x) and
/// <see cref="DefaultBrowse"/> (DA 3.0).
/// </summary>
/// <remarks>
/// <para>
/// Server authors can implement this directly on their
/// <c>IOpcDaServer</c> (the host detects the cast at startup) or provide a
/// standalone instance. Default fallback is
/// <see cref="FlatHierarchicalNamespace"/>, an in-memory empty namespace
/// that returns no elements.
/// </para>
/// <para>
/// Namespace shape: a server is either FLAT (single level, item IDs are
/// the leaf names) or HIERARCHICAL (branches + leaves). The
/// <see cref="IsHierarchical"/> property feeds <c>QueryOrganization</c>.
/// </para>
/// </remarks>
public interface IOpcAddressSpace
{
    /// <summary>
    /// Returns <see langword="true"/> for a hierarchical namespace
    /// (OPCNS_HIERARCHIAL = 2), <see langword="false"/> for FLAT (OPCNS_FLAT = 1).
    /// </summary>
    bool IsHierarchical { get; }

    /// <summary>
    /// Lists the branches and items at <paramref name="branchPath"/>. Empty
    /// or null path = root.
    /// </summary>
    Task<OpcBrowseResult> BrowseAsync(
        string? branchPath,
        OpcBrowseElementKind kind,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a relative or qualified item-data ID to a fully qualified
    /// item ID for use with <c>IOPCItemMgt::AddItems</c>. Returns the input
    /// unchanged for flat namespaces; concatenates with the current browse
    /// position for hierarchical ones.
    /// </summary>
    Task<string> GetItemIdAsync(
        string? currentBranchPath,
        string itemDataId,
        CancellationToken cancellationToken = default);
}
