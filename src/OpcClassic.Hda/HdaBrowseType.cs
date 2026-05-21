//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Hda;

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
}
