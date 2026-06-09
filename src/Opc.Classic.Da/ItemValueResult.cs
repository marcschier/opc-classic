//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da;

/// <summary>
/// The outcome of reading an OPC DA item. Extends <see cref="ItemValue"/>
/// with the per-item <see cref="OpcResultId"/> the server returned for that
/// row. On bulk reads, individual rows can succeed or fail independently;
/// always inspect <see cref="ResultId"/> before consuming <see cref="ItemValue.Value"/>.
/// </summary>
public sealed class ItemValueResult : ItemValue {
    /// <summary>Construct.</summary>
    public ItemValueResult(string itemName, string? path = null) : base(itemName, path) { }

    /// <summary>Copy-construct from an <see cref="ItemValue"/>.</summary>
    public ItemValueResult(ItemValue value)
        : base(value) {
        ClientHandle = value.ClientHandle;
        Value = value.Value;
        Quality = value.Quality;
        Timestamp = value.Timestamp;
    }

    /// <summary>The per-item HRESULT the server returned.</summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;
}
