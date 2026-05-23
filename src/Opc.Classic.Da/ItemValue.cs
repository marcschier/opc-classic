//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// A value associated with an OPC DA item — what a server returns from a
/// read, what a client sends to a write, what a subscription delivers on
/// data change.
/// </summary>
/// <remarks>
/// Mirrors the on-the-wire <c>OPCITEMSTATE</c> struct but as a clean managed
/// type. The DA 2.x quality WORD is decoded into <see cref="Quality"/>;
/// the FILETIME is decoded into <see cref="Timestamp"/>.
/// </remarks>
public class ItemValue : ItemIdentifier
{
    /// <summary>Construct.</summary>
    public ItemValue(string itemName, string? path = null) : base(itemName, path) { }

    /// <summary>Copy-construct from a bare identifier.</summary>
    public ItemValue(ItemIdentifier identifier) : base(
        (identifier ?? throw new ArgumentNullException(nameof(identifier))).ItemName,
        identifier.Path)
    { }

    /// <summary>
    /// Client handle as set on the originating <see cref="Item"/>.
    /// Servers echo this back; clients use it to correlate values to items.
    /// </summary>
    public int ClientHandle { get; init; }

    /// <summary>The current value. May be <see langword="null"/> for bad-quality reads.</summary>
    public object? Value { get; init; }

    /// <summary>
    /// Decoded quality WORD. Defaults to <see cref="OpcQuality.Bad"/> if
    /// not explicitly set — callers should always set this on writes.
    /// </summary>
    public OpcQuality Quality { get; init; } = OpcQuality.Bad;

    /// <summary>Server-supplied UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }
}
