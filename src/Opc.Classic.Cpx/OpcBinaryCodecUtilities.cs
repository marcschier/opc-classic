//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Opc.Classic.Cpx;

internal static class OpcBinaryCodecUtilities
{
    public static ByteOrder GetDefaultByteOrder(TypeDescription type, TypeDictionary? dictionary)
    {
        var bigEndian = type.DefaultBigEndian ?? dictionary?.DefaultBigEndian ?? true;
        return bigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
    }

    public static ByteOrder GetByteOrder(TypeDescription type, TypeField field, TypeDictionary? dictionary) =>
        field.ByteOrder ?? GetDefaultByteOrder(type, dictionary);

    public static string GetStringEncoding(TypeDescription type, TypeField field, TypeDictionary? dictionary) =>
        field.StringEncoding
        ?? type.DefaultStringEncoding
        ?? dictionary?.DefaultStringEncoding
        ?? TypeDictionary.DefaultOpcBinaryStringEncoding;

    public static int GetCharWidth(TypeDescription type, TypeField field, TypeDictionary? dictionary) =>
        field.CharWidth
        ?? type.DefaultCharWidth
        ?? dictionary?.DefaultCharWidth
        ?? 2;

    public static TypeDescription ResolveType(TypeField field, TypeDictionary? dictionary)
    {
        if (field.TypeId is null)
        {
            throw new InvalidOperationException($"Field '{field.Name}' does not declare a referenced TypeID.");
        }

        if (dictionary?.TryGetByTypeId(field.TypeId) is { } byTypeId)
        {
            return byTypeId;
        }

        if (dictionary?.TryGet(field.TypeId) is { } byName)
        {
            return byName;
        }

        throw new KeyNotFoundException($"TypeID '{field.TypeId}' was not found in the OPCBinary dictionary.");
    }

    public static StructType CreateStructType(TypeDescription type, TypeDictionary? dictionary) =>
        new()
        {
            Name = type.Name,
            DefaultByteOrder = GetDefaultByteOrder(type, dictionary),
            IsDefault = false,
            Fields = type.Fields.Select(static field => new StructField
            {
                Name = field.Name,
                Kind = field.Kind,
                TypeReference = field.TypeId,
                Repeats = field.ElementCount ?? 0,
                CountFieldName = field.ElementCountFieldName,
                ByteOrder = field.ByteOrder,
            }).ToArray(),
        };

    public static int? GetElementCount(TypeField field, IReadOnlyDictionary<string, object?> values)
    {
        if (field.ElementCount is { } fixedCount)
        {
            return fixedCount;
        }

        if (field.ElementCountFieldName is not null && values.TryGetValue(field.ElementCountFieldName, out var countValue))
        {
            return ToNonNegativeInt32(countValue, field.ElementCountFieldName);
        }

        return null;
    }

    public static int ToNonNegativeInt32(object? value, string name)
    {
        var count = value switch
        {
            byte typed => typed,
            sbyte typed => typed,
            short typed => typed,
            ushort typed => typed,
            int typed => typed,
            uint typed => checked((int)typed),
            long typed => checked((int)typed),
            ulong typed => checked((int)typed),
            string typed => int.Parse(typed, NumberStyles.Integer, CultureInfo.InvariantCulture),
            _ => throw new InvalidCastException($"Field '{name}' does not contain an integer element count."),
        };

        if (count < 0)
        {
            throw new InvalidOperationException($"Field '{name}' contains a negative element count.");
        }

        return count;
    }

    public static byte[] DecodeHex(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);

        var cleaned = new string(hex.Where(static c => !char.IsWhiteSpace(c)).ToArray());
        if (cleaned.Length % 2 != 0)
        {
            throw new FormatException("A hex string must contain an even number of digits.");
        }

        var bytes = new byte[cleaned.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(cleaned.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }

    public static string DecodeString(ReadOnlySpan<byte> bytes, string encodingName, int charWidth, ByteOrder byteOrder)
    {
        var encoding = GetEncoding(encodingName, charWidth, byteOrder);
        return encoding.GetString(bytes).TrimEnd('\0');
    }

    public static byte[] EncodeString(string value, string encodingName, int charWidth, ByteOrder byteOrder)
    {
        ArgumentNullException.ThrowIfNull(value);
        var encoding = GetEncoding(encodingName, charWidth, byteOrder);
        return encoding.GetBytes(value);
    }

    public static int GetPrimitiveSize(TypeKind kind) =>
        kind switch
        {
            TypeKind.Boolean => 1,
            TypeKind.Int8 => 1,
            TypeKind.UInt8 => 1,
            TypeKind.Int16 => 2,
            TypeKind.UInt16 => 2,
            TypeKind.Int32 => 4,
            TypeKind.UInt32 => 4,
            TypeKind.Single => 4,
            TypeKind.Int64 => 8,
            TypeKind.UInt64 => 8,
            TypeKind.Double => 8,
            TypeKind.FileTime => 8,
            TypeKind.Guid => 16,
            _ => throw new NotSupportedException($"Type kind '{kind}' does not have a fixed primitive size."),
        };

    private static Encoding GetEncoding(string encodingName, int charWidth, ByteOrder byteOrder)
    {
        if (encodingName.Equals("ASCII", StringComparison.OrdinalIgnoreCase)
            || encodingName.Equals("Ascii", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.ASCII;
        }

        if (encodingName.Equals("UTF-8", StringComparison.OrdinalIgnoreCase)
            || encodingName.Equals("UTF8", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.UTF8;
        }

        if (encodingName.Equals("UCS-2", StringComparison.OrdinalIgnoreCase)
            || encodingName.Equals("UTF-16", StringComparison.OrdinalIgnoreCase)
            || encodingName.Equals("UTF16", StringComparison.OrdinalIgnoreCase)
            || charWidth == 2)
        {
            return byteOrder == ByteOrder.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;
        }

        if (charWidth == 1)
        {
            return Encoding.ASCII;
        }

        throw new NotSupportedException($"String encoding '{encodingName}' with character width {charWidth.ToString(CultureInfo.InvariantCulture)} is not supported.");
    }
}
