//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Hda;

/// <summary>
/// HDA address-space browse-element type, mirroring OPCHDA_BROWSETYPE.
/// </summary>
public enum HdaBrowseType
{
    /// <summary>A branch (sub-tree) in the HDA namespace.</summary>
    Branch = 1,
    /// <summary>A leaf (a historical item).</summary>
    Leaf = 2,
    /// <summary>A flat-namespace item (HDA allows non-hierarchical layouts).</summary>
    Flat = 3,
    /// <summary>All item names visible at the current browse position.</summary>
    Items = 4,
}
