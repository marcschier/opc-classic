// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// Browse-tree filter passed to <c>IDaServer.BrowseAsync</c>.
/// Matches the on-the-wire <c>OPCBROWSEFILTER</c> enum.
/// </summary>
public enum BrowseFilters
{
    /// <summary>
    /// Return both branches and leaves.
    /// </summary>
    All = 1,
    /// <summary>
    /// Return only branches (sub-namespaces / folders).
    /// </summary>
    Branch = 2,
    /// <summary>
    /// Return only leaves (items / tags).
    /// </summary>
    Leaf = 3,
}
