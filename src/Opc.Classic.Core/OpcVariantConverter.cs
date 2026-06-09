//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic;

/// <summary>
/// Bidirectional bridge between the loosely-typed <see cref="object"/>?
/// representation used by existing DA APIs (notably
/// <c>Opc.Classic.Da.ItemValue.Value</c>) and the strongly-typed
/// <see cref="OpcVariant"/> carrier introduced in Phase 5E.3a.
/// </summary>
/// <remarks>
/// Designed to allow gradual migration: code that already has
/// <c>object? value</c> can use <see cref="FromObject"/> to lift it
/// into the typed VARIANT vocabulary without touching the source.
/// </remarks>
public static class OpcVariantConverter {
    /// <summary>
    /// Converts a .NET-typed value to an <see cref="OpcVariant"/>.
    /// <see langword="null"/> maps to <see cref="OpcVariant.Null"/>.
    /// Unsupported types raise <see cref="ArgumentException"/>.
    /// </summary>
    public static OpcVariant FromObject(object? value) => value switch {
        null => OpcVariant.Null,
        bool b => OpcVariant.FromBoolean(b),
        sbyte i1 => OpcVariant.FromInt8(i1),
        byte u1 => OpcVariant.FromUInt8(u1),
        short i2 => OpcVariant.FromInt16(i2),
        ushort u2 => OpcVariant.FromUInt16(u2),
        int i4 => OpcVariant.FromInt32(i4),
        uint u4 => OpcVariant.FromUInt32(u4),
        long i8 => OpcVariant.FromInt64(i8),
        ulong u8 => OpcVariant.FromUInt64(u8),
        float r4 => OpcVariant.FromSingle(r4),
        double r8 => OpcVariant.FromDouble(r8),
        string s => OpcVariant.FromString(s),
        DateTime dt => OpcVariant.FromDate(dt),
        Guid g => OpcVariant.FromClsid(g),
        _ => throw new ArgumentException(
            $"OpcVariantConverter.FromObject cannot map {value.GetType()} to a VARIANT; supported types are bool/sbyte/byte/short/ushort/int/uint/long/ulong/float/double/string/DateTime/Guid.",
            nameof(value)),
    };

    /// <summary>
    /// Returns the boxed .NET-typed value carried by the variant, or
    /// <see langword="null"/> for <see cref="VarType.VT_EMPTY"/> /
    /// <see cref="VarType.VT_NULL"/>.
    /// </summary>
    public static object? ToObject(OpcVariant variant) => variant.Boxed;

    /// <summary>
    /// True if <see cref="FromObject"/> would succeed for the given value
    /// (i.e., it is null or one of the supported .NET types).
    /// </summary>
    public static bool CanConvert(object? value) => value switch {
        null => true,
        bool or sbyte or byte or short or ushort or int or uint
            or long or ulong or float or double or string or DateTime or Guid => true,
        _ => false,
    };
}
