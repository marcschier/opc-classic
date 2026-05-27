//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Element-kind filter for <see cref="IOpcAddressSpace.BrowseAsync"/>.
/// </summary>
public enum OpcBrowseElementKind
{
    /// <summary>Both branches and items.</summary>
    All,

    /// <summary>Branches only (intermediate nodes).</summary>
    Branches,

    /// <summary>Leaf items only.</summary>
    Items,
}
