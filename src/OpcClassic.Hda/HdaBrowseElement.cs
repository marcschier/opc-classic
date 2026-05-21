//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Hda;

/// <summary>
/// A node in an OPC HDA server's browse tree.
/// </summary>
public sealed class HdaBrowseElement
{
    /// <summary>Short display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Fully-qualified item ID for historical-read operations.</summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>Branch / Leaf / Flat classification.</summary>
    public HdaBrowseType BrowseType { get; init; }
}
