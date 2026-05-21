//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace OpcClassic.Cpx;

/// <summary>
/// A named struct definition from an OPCBinary type dictionary.
/// </summary>
public sealed class StructType
{
    /// <summary>Struct type name — referenced by <see cref="StructField.TypeReference"/>.</summary>
    public required string Name { get; init; }

    /// <summary>Default byte order applied to fields that don't specify one.</summary>
    public ByteOrder DefaultByteOrder { get; init; } = ByteOrder.LittleEndian;

    /// <summary>
    /// True if this is the type-dictionary's "default" struct — the wrapper
    /// served as the item's complex value when no explicit
    /// <c>TypeDescriptionItemId</c> property is selected (DA Property ID 301).
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>The struct's fields, in declared order.</summary>
    public IReadOnlyList<StructField> Fields { get; init; } = Array.Empty<StructField>();
}
