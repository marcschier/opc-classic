//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Opc.Classic.Cpx;

/// <summary>
/// Decodes OPCBinary item payloads into <see cref="ComplexValue"/> instances.
/// </summary>
public static class OpcBinaryDecoder
{
    /// <summary>Decode a payload using a type from <paramref name="dictionary"/>.</summary>
    public static ComplexValue Decode(byte[] data, TypeDictionary dictionary, string typeId)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);
        var type = dictionary.TryGetByTypeId(typeId) ?? dictionary.TryGet(typeId)
            ?? throw new KeyNotFoundException($"TypeID '{typeId}' was not found in the OPCBinary dictionary.");
        return Decode(data, type, dictionary);
    }

    /// <summary>Decode a payload using the supplied type description.</summary>
    public static ComplexValue Decode(byte[] data, TypeDescription type, TypeDictionary? dictionary = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Decode(data.AsSpan(), type, dictionary);
    }

    /// <summary>Decode a payload using the supplied type description.</summary>
    public static ComplexValue Decode(ReadOnlySpan<byte> data, TypeDescription type, TypeDictionary? dictionary = null)
    {
        ArgumentNullException.ThrowIfNull(type);
        var reader = new OpcBinarySpanReader(data);
        var result = DecodeValue(ref reader, type, dictionary);
        if (!reader.End)
        {
            throw new FormatException("OPCBinary payload contains trailing bytes after the decoded value.");
        }

        return result;
    }

    private static ComplexValue DecodeValue(ref OpcBinarySpanReader reader, TypeDescription type, TypeDictionary? dictionary)
    {
        var fields = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var field in type.Fields)
        {
            fields[field.Name] = DecodeField(ref reader, type, field, dictionary, fields);
        }

        return new ComplexValue
        {
            Type = OpcBinaryCodecUtilities.CreateStructType(type, dictionary),
            Fields = fields,
        };
    }

    private static object? DecodeField(
        ref OpcBinarySpanReader reader,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary,
        IReadOnlyDictionary<string, object?> decodedFields)
    {
        if (field.Kind == TypeKind.String)
        {
            return DecodeString(ref reader, type, field, dictionary, decodedFields);
        }

        if (field.Kind == TypeKind.Blob)
        {
            return DecodeBlob(ref reader, type, field, dictionary, decodedFields);
        }

        var elementCount = OpcBinaryCodecUtilities.GetElementCount(field, decodedFields);
        if (field.Kind == TypeKind.StructReference)
        {
            var referencedType = OpcBinaryCodecUtilities.ResolveType(field, dictionary);
            if (elementCount is { } count)
            {
                var values = new ComplexValue[count];
                for (var i = 0; i < count; i++)
                {
                    values[i] = DecodeValue(ref reader, referencedType, dictionary);
                }

                return values;
            }

            return DecodeValue(ref reader, referencedType, dictionary);
        }

        if (elementCount is { } primitiveCount)
        {
            var values = new object?[primitiveCount];
            for (var i = 0; i < primitiveCount; i++)
            {
                values[i] = DecodePrimitive(ref reader, field.Kind, OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary));
            }

            return values;
        }

        return DecodePrimitive(ref reader, field.Kind, OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary));
    }

    private static string DecodeString(
        ref OpcBinarySpanReader reader,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary,
        IReadOnlyDictionary<string, object?> decodedFields)
    {
        var byteOrder = OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary);
        var charWidth = OpcBinaryCodecUtilities.GetCharWidth(type, field, dictionary);
        var encoding = OpcBinaryCodecUtilities.GetStringEncoding(type, field, dictionary);
        ReadOnlySpan<byte> bytes;
        if (field.FieldTerminator is not null && field.Length is null && field.ElementCountFieldName is null && field.ElementCount is null)
        {
            bytes = reader.ReadUntil(OpcBinaryCodecUtilities.DecodeHex(field.FieldTerminator), consumeTerminator: true);
        }
        else
        {
            var byteCount = GetStringByteCount(ref reader, type, field, dictionary, decodedFields, byteOrder, charWidth);
            bytes = reader.Read(byteCount);
        }

        return OpcBinaryCodecUtilities.DecodeString(bytes, encoding, charWidth, byteOrder);
    }

    private static int GetStringByteCount(
        ref OpcBinarySpanReader reader,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary,
        IReadOnlyDictionary<string, object?> decodedFields,
        ByteOrder byteOrder,
        int charWidth)
    {
        if (field.Length is { } byteLength)
        {
            return byteLength;
        }

        if (field.ElementCountFieldName is not null || field.ElementCount is not null)
        {
            return (OpcBinaryCodecUtilities.GetElementCount(field, decodedFields) ?? 0) * charWidth;
        }

        if (field.FieldTerminator is not null)
        {
            throw new InvalidOperationException("Terminated string fields are read directly and do not have a byte count.");
        }

        var lengthField = new TypeField(field.Name, TypeKind.Int32, ByteOrder: byteOrder);
        return (int)DecodePrimitive(ref reader, TypeKind.Int32, OpcBinaryCodecUtilities.GetByteOrder(type, lengthField, dictionary))!;
    }

    private static byte[] DecodeBlob(
        ref OpcBinarySpanReader reader,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary,
        IReadOnlyDictionary<string, object?> decodedFields)
    {
        if (field.Length is { } byteLength)
        {
            return reader.Read(byteLength).ToArray();
        }

        if (field.ElementCountFieldName is not null || field.ElementCount is not null)
        {
            return reader.Read(OpcBinaryCodecUtilities.GetElementCount(field, decodedFields) ?? 0).ToArray();
        }

        if (field.FieldTerminator is not null)
        {
            var terminator = OpcBinaryCodecUtilities.DecodeHex(field.FieldTerminator);
            return reader.ReadUntil(terminator, consumeTerminator: true).ToArray();
        }

        var byteOrder = OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary);
        var length = (int)DecodePrimitive(ref reader, TypeKind.Int32, byteOrder)!;
        return reader.Read(length).ToArray();
    }

    private static object DecodePrimitive(ref OpcBinarySpanReader reader, TypeKind kind, ByteOrder byteOrder) =>
        kind switch
        {
            TypeKind.Boolean => reader.ReadByte() != 0,
            TypeKind.Int8 => unchecked((sbyte)reader.ReadByte()),
            TypeKind.UInt8 => reader.ReadByte(),
            TypeKind.Int16 => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadInt16BigEndian(reader.Read(2))
                : BinaryPrimitives.ReadInt16LittleEndian(reader.Read(2)),
            TypeKind.UInt16 => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadUInt16BigEndian(reader.Read(2))
                : BinaryPrimitives.ReadUInt16LittleEndian(reader.Read(2)),
            TypeKind.Int32 => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadInt32BigEndian(reader.Read(4))
                : BinaryPrimitives.ReadInt32LittleEndian(reader.Read(4)),
            TypeKind.UInt32 => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadUInt32BigEndian(reader.Read(4))
                : BinaryPrimitives.ReadUInt32LittleEndian(reader.Read(4)),
            TypeKind.Int64 => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadInt64BigEndian(reader.Read(8))
                : BinaryPrimitives.ReadInt64LittleEndian(reader.Read(8)),
            TypeKind.UInt64 => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadUInt64BigEndian(reader.Read(8))
                : BinaryPrimitives.ReadUInt64LittleEndian(reader.Read(8)),
            TypeKind.Single => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadSingleBigEndian(reader.Read(4))
                : BinaryPrimitives.ReadSingleLittleEndian(reader.Read(4)),
            TypeKind.Double => byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadDoubleBigEndian(reader.Read(8))
                : BinaryPrimitives.ReadDoubleLittleEndian(reader.Read(8)),
            TypeKind.FileTime => DateTime.FromFileTimeUtc(byteOrder == ByteOrder.BigEndian
                ? BinaryPrimitives.ReadInt64BigEndian(reader.Read(8))
                : BinaryPrimitives.ReadInt64LittleEndian(reader.Read(8))),
            TypeKind.Guid => new Guid(reader.Read(16)),
            _ => throw new NotSupportedException($"Type kind '{kind}' is not supported by the OPCBinary decoder."),
        };

    private ref struct OpcBinarySpanReader
    {
        private ReadOnlySpan<byte> _remaining;

        public OpcBinarySpanReader(ReadOnlySpan<byte> data)
        {
            _remaining = data;
        }

        public bool End => _remaining.IsEmpty;

        public byte ReadByte()
        {
            if (_remaining.IsEmpty)
            {
                throw new FormatException("Unexpected end of OPCBinary payload.");
            }

            var value = _remaining[0];
            _remaining = _remaining[1..];
            return value;
        }

        public ReadOnlySpan<byte> Read(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount, "Byte count cannot be negative.");
            }

            if (_remaining.Length < byteCount)
            {
                throw new FormatException("Unexpected end of OPCBinary payload.");
            }

            var value = _remaining[..byteCount];
            _remaining = _remaining[byteCount..];
            return value;
        }

        public ReadOnlySpan<byte> ReadUntil(ReadOnlySpan<byte> terminator, bool consumeTerminator)
        {
            if (terminator.IsEmpty)
            {
                throw new FormatException("Field terminator cannot be empty.");
            }

            var index = _remaining.IndexOf(terminator);
            if (index < 0)
            {
                throw new FormatException("OPCBinary field terminator was not found.");
            }

            var value = _remaining[..index];
            _remaining = _remaining[(index + (consumeTerminator ? terminator.Length : 0))..];
            return value;
        }
    }
}
