//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da;

/// <summary>
/// A node in an OPC DA server's address-space browse tree. The result of
/// <c>IDaServer.BrowseAsync(...)</c> is a sequence of these.
/// </summary>
/// <remarks>
/// A <see cref="BrowseElement"/> can be a branch (a sub-tree without a
/// direct item identity — like a folder; <see cref="HasChildren"/> only) and/or
/// a leaf (<see cref="IsItem"/>); DA 3.0 servers may report both flags for the
/// same element, indicating a tag that also has child tags.
/// </remarks>
public sealed class BrowseElement
{
    /// <summary>
    /// Short display name (last component of the path).
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Fully-qualified item name (server-canonical). For pure branches this
    /// may be the empty string.
    /// </summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>
    /// Optional access path. Server-defined; <see langword="null"/> for the
    /// usual single-access-path tree.
    /// </summary>
    public string? ItemPath { get; init; }

    /// <summary>
    /// True if this element is a leaf — i.e. it represents an OPC item that
    /// can be added to a subscription / read.
    /// </summary>
    public bool IsItem { get; init; }

    /// <summary>
    /// True if this element has child elements (a branch). DA 3.0 servers may
    /// set both <see cref="IsItem"/> and this — an element can be both.
    /// </summary>
    public bool HasChildren { get; init; }

    /// <summary>
    /// Optional inline properties. DA 3.0 <c>IOPCBrowse::Browse</c> can be
    /// asked to include selected property values inline; populated only when
    /// requested. Empty otherwise.
    /// </summary>
    public IReadOnlyList<ItemProperty> Properties { get; init; } = Array.Empty<ItemProperty>();

    /// <inheritdoc />
    public override string ToString()
    {
        var kind = (IsItem, HasChildren) switch
        {
            (true, true) => "item+branch",
            (true, false) => "item",
            (false, true) => "branch",
            _ => "(empty)",
        };
        return $"{Name} [{kind}] -> {ItemName}";
    }
}
