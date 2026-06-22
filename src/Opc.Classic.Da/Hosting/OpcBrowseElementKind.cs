// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Element-kind filter for <see cref="IOpcAddressSpace.BrowseAsync"/>.
/// </summary>
public enum OpcBrowseElementKind
{
    /// <summary>
    /// Both branches and items.
    /// </summary>
    All,

    /// <summary>
    /// Branches only (intermediate nodes).
    /// </summary>
    Branches,

    /// <summary>
    /// Leaf items only.
    /// </summary>
    Items,
}
