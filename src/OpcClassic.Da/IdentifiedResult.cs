//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Da;

/// <summary>
/// The outcome of an operation identified by item but not carrying a value
/// payload — write results, validate results, item-add results, etc.
/// </summary>
public sealed class IdentifiedResult : ItemIdentifier
{
    /// <summary>Construct.</summary>
    public IdentifiedResult(string itemName, string? path = null) : base(itemName, path) { }

    /// <summary>Copy-construct from an identifier.</summary>
    public IdentifiedResult(ItemIdentifier identifier) : base(
        (identifier ?? throw new ArgumentNullException(nameof(identifier))).ItemName,
        identifier.Path)
    { }

    /// <summary>Client handle as set on the originating <see cref="Item"/>.</summary>
    public int ClientHandle { get; init; }

    /// <summary>Per-item HRESULT.</summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;

    /// <summary>Per-item server diagnostic info (server-supplied, optional).</summary>
    public string? DiagnosticInfo { get; init; }
}
