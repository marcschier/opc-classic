//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// An item description as supplied to <c>IOPCItemMgt::AddItems</c> or
/// <c>IOPCItemIO::Read</c>. Extends <see cref="ItemIdentifier"/> with
/// per-add configuration: client handle, requested data type, and
/// (legacy) requested deadband.
/// </summary>
public sealed class Item : ItemIdentifier
{
    /// <summary>Construct from an identifier.</summary>
    public Item(string itemName, string? path = null) : base(itemName, path) { }

    /// <summary>Copy-construct from an existing identifier.</summary>
    public Item(ItemIdentifier identifier) : base(
        (identifier ?? throw new ArgumentNullException(nameof(identifier))).ItemName,
        identifier.Path)
    { }

    /// <summary>
    /// Opaque value the client passes; the server echoes this back in every
    /// callback / read result so the client can correlate.
    /// </summary>
    public int ClientHandle { get; init; }

    /// <summary>
    /// Requested data type for the read value. <see langword="null"/> means
    /// "the server's canonical type for this item".
    /// </summary>
    public Type? RequestedDataType { get; init; }

    /// <summary>
    /// Requested DA 3.0 per-item deadband (0..100 percent of EU range), or
    /// <see langword="null"/> to use the group's deadband.
    /// </summary>
    public float? DeadbandPercent { get; init; }

    /// <summary>
    /// Requested DA 3.0 per-item sampling rate, in milliseconds, or
    /// <see langword="null"/> to use the group's update rate.
    /// </summary>
    public int? SamplingRateMs { get; init; }
}
