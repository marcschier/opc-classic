//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Hda;

/// <summary>
/// A single historical value: timestamp + value + quality. Mirrors
/// <c>OPCHDA_ITEM</c>'s per-element shape.
/// </summary>
public sealed class HdaItemValue
{
    /// <summary>Server-supplied UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>The historical value (or aggregate result for aggregated reads).</summary>
    public object? Value { get; init; }

    /// <summary>HDA-specific quality. Defaults to <see cref="OpcQuality.Bad"/>.</summary>
    public OpcQuality Quality { get; init; } = OpcQuality.Bad;
}
