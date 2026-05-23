//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Cpx;

/// <summary>
/// A decoded complex-data value — the result of applying a
/// <see cref="StructType"/> from a <see cref="TypeDictionary"/> to the raw
/// bytes of a DA item value.
/// </summary>
/// <remarks>
/// Field values are typed-but-erased: the implementation knows what type each
/// field is (per the struct definition), but in the managed bag they're
/// stored as <see langword="object"/>?. Consumers cast based on the
/// <see cref="StructField.Kind"/> of the corresponding field.
/// </remarks>
public sealed class ComplexValue
{
    /// <summary>The struct type this value conforms to.</summary>
    public required StructType Type { get; init; }

    /// <summary>Decoded field values, keyed by <see cref="StructField.Name"/>.</summary>
    public IReadOnlyDictionary<string, object?> Fields { get; init; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>
    /// Look up a field's value by name. Throws <see cref="KeyNotFoundException"/>
    /// when the field doesn't exist.
    /// </summary>
    public object? this[string fieldName] => Fields[fieldName];

    /// <summary>
    /// Try to read a field's value with a strongly-typed cast. Returns
    /// <see langword="false"/> when the field doesn't exist or the value is
    /// the wrong type for <typeparamref name="T"/>.
    /// </summary>
    public bool TryGet<T>(string fieldName, out T value)
    {
        if (Fields.TryGetValue(fieldName, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        value = default!;
        return false;
    }
}
