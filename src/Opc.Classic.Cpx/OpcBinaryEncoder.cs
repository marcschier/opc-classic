// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Collections;
using System.Globalization;

namespace Opc.Classic.Cpx;

/// <summary>
/// Encodes <see cref="ComplexValue"/> instances into OPCBinary item payloads.
/// </summary>
public static class OpcBinaryEncoder
{
    /// <summary>
    /// Encode a value using a type from <paramref name="dictionary"/>.
    /// </summary>
    public static byte[] Encode(ComplexValue value, TypeDictionary dictionary, string typeId)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeId);
        var type = dictionary.TryGetByTypeId(typeId) ?? dictionary.TryGet(typeId)
            ?? throw new KeyNotFoundException($"TypeID '{typeId}' was not found in the OPCBinary dictionary.");
        return Encode(value, type, dictionary);
    }

    /// <summary>
    /// Encode a value using the supplied type description.
    /// </summary>
    public static byte[] Encode(ComplexValue value, TypeDescription type, TypeDictionary? dictionary = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(type);

        var writer = new OpcBinaryBufferWriter();
        EncodeValue(writer, value, type, dictionary);
        return writer.ToArray();
    }

    private static void EncodeValue(OpcBinaryBufferWriter writer, ComplexValue value, TypeDescription type, TypeDictionary? dictionary)
    {
        for (var i = 0; i < type.Fields.Count; i++)
        {
            var field = type.Fields[i];
            if (!value.Fields.TryGetValue(field.Name, out var rawValue))
            {
                throw new KeyNotFoundException($"Complex value is missing field '{field.Name}'.");
            }

            EncodeField(writer, rawValue, value.Fields, type, field, dictionary);
            if (field.Kind == TypeKind.BitString && (i == type.Fields.Count - 1 || type.Fields[i + 1].Kind != TypeKind.BitString))
            {
                writer.AlignToByte();
            }
        }
    }

    private static void EncodeField(
        OpcBinaryBufferWriter writer,
        object? rawValue,
        IReadOnlyDictionary<string, object?> containingFields,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary)
    {
        if (field.Kind == TypeKind.String)
        {
            EncodeString(writer, rawValue, containingFields, type, field, dictionary);
            return;
        }

        if (field.Kind == TypeKind.BitString)
        {
            EncodeBitString(writer, rawValue, containingFields, field);
            return;
        }

        if (field.Kind == TypeKind.Blob)
        {
            EncodeBlob(writer, rawValue, containingFields, type, field, dictionary);
            return;
        }

        var elementCount = OpcBinaryCodecUtilities.GetElementCount(field, containingFields);
        if (field.Kind == TypeKind.StructReference)
        {
            var referencedType = OpcBinaryCodecUtilities.ResolveType(field, dictionary);
            if (elementCount is { } count)
            {
                var elements = MaterializeElements(rawValue, count, field.Name);
                for (var i = 0; i < elements.Length; i++)
                {
                    EncodeValue(writer, ToComplexValue(elements[i], field.Name), referencedType, dictionary);
                }

                return;
            }

            EncodeValue(writer, ToComplexValue(rawValue, field.Name), referencedType, dictionary);
            return;
        }

        var byteOrder = OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary);
        if (elementCount is { } primitiveCount)
        {
            var elements = MaterializeElements(rawValue, primitiveCount, field.Name);
            for (var i = 0; i < elements.Length; i++)
            {
                EncodePrimitive(writer, field.Kind, elements[i], byteOrder);
            }

            return;
        }

        EncodePrimitive(writer, field.Kind, rawValue, byteOrder);
    }

    private static void EncodeString(
        OpcBinaryBufferWriter writer,
        object? rawValue,
        IReadOnlyDictionary<string, object?> containingFields,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary)
    {
        var value = rawValue as string ?? Convert.ToString(rawValue, CultureInfo.InvariantCulture)
            ?? throw new InvalidCastException($"Field '{field.Name}' cannot be converted to a string.");
        var byteOrder = OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary);
        var charWidth = OpcBinaryCodecUtilities.GetCharWidth(type, field, dictionary);
        var bytes = OpcBinaryCodecUtilities.EncodeString(value, OpcBinaryCodecUtilities.GetStringEncoding(type, field, dictionary), charWidth, byteOrder);

        if (field.Length is { } fixedByteLength)
        {
            WriteFixed(writer, bytes, fixedByteLength, field.Name);
            return;
        }

        if (field.ElementCountFieldName is not null || field.ElementCount is not null)
        {
            var fixedStringByteLength = (OpcBinaryCodecUtilities.GetElementCount(field, containingFields) ?? 0) * charWidth;
            WriteFixed(writer, bytes, fixedStringByteLength, field.Name);
            return;
        }

        if (field.FieldTerminator is not null)
        {
            writer.Write(bytes);
            writer.Write(OpcBinaryCodecUtilities.DecodeHex(field.FieldTerminator));
            return;
        }

        EncodePrimitive(writer, TypeKind.Int32, bytes.Length, byteOrder);
        writer.Write(bytes);
    }

    private static void EncodeBitString(
        OpcBinaryBufferWriter writer,
        object? rawValue,
        IReadOnlyDictionary<string, object?> containingFields,
        TypeField field)
    {
        var bytes = ToByteArray(rawValue, field.Name);
        writer.WriteBits(bytes, GetBitCount(field, containingFields), field.Name);
    }

    private static void EncodeBlob(
        OpcBinaryBufferWriter writer,
        object? rawValue,
        IReadOnlyDictionary<string, object?> containingFields,
        TypeDescription type,
        TypeField field,
        TypeDictionary? dictionary)
    {
        var bytes = ToByteArray(rawValue, field.Name);

        if (field.Length is { } fixedByteLength)
        {
            WriteFixed(writer, bytes, fixedByteLength, field.Name);
            return;
        }

        if (field.ElementCountFieldName is not null || field.ElementCount is not null)
        {
            WriteFixed(writer, bytes, OpcBinaryCodecUtilities.GetElementCount(field, containingFields) ?? 0, field.Name);
            return;
        }

        if (field.FieldTerminator is not null)
        {
            writer.Write(bytes);
            writer.Write(OpcBinaryCodecUtilities.DecodeHex(field.FieldTerminator));
            return;
        }

        EncodePrimitive(writer, TypeKind.Int32, bytes.Length, OpcBinaryCodecUtilities.GetByteOrder(type, field, dictionary));
        writer.Write(bytes);
    }

    private static int GetBitCount(TypeField field, IReadOnlyDictionary<string, object?> containingFields)
    {
        if (field.Length is not { } bitLength)
        {
            throw new InvalidOperationException($"BitString field '{field.Name}' must declare a Length in bits.");
        }

        var elementCount = OpcBinaryCodecUtilities.GetElementCount(field, containingFields) ?? 1;
        return checked(bitLength * elementCount);
    }

    private static byte[] ToByteArray(object? rawValue, string fieldName) =>
        rawValue switch
        {
            byte[] typed => typed,
            ReadOnlyMemory<byte> typed => typed.ToArray(),
            Memory<byte> typed => typed.ToArray(),
            _ => throw new InvalidCastException($"Field '{fieldName}' cannot be converted to a byte array."),
        };

    private static void EncodePrimitive(OpcBinaryBufferWriter writer, TypeKind kind, object? value, ByteOrder byteOrder)
    {
        switch (kind)
        {
            case TypeKind.Boolean:
                writer.WriteByte(ToBoolean(value) ? (byte)1 : (byte)0);
                break;
            case TypeKind.Int8:
                writer.WriteByte(unchecked((byte)Convert.ToSByte(value, CultureInfo.InvariantCulture)));
                break;
            case TypeKind.UInt8:
                writer.WriteByte(Convert.ToByte(value, CultureInfo.InvariantCulture));
                break;
            case TypeKind.Int16:
                WriteInt16(writer, Convert.ToInt16(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.UInt16:
                WriteUInt16(writer, Convert.ToUInt16(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.Int32:
                WriteInt32(writer, Convert.ToInt32(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.UInt32:
                WriteUInt32(writer, Convert.ToUInt32(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.Int64:
                WriteInt64(writer, Convert.ToInt64(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.UInt64:
                WriteUInt64(writer, Convert.ToUInt64(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.Single:
                WriteSingle(writer, Convert.ToSingle(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.Double:
                WriteDouble(writer, Convert.ToDouble(value, CultureInfo.InvariantCulture), byteOrder);
                break;
            case TypeKind.FileTime:
                WriteInt64(writer, ToFileTimeUtc(value), byteOrder);
                break;
            case TypeKind.Guid:
                writer.Write(ToGuid(value).ToByteArray());
                break;
            default:
                throw new NotSupportedException($"Type kind '{kind}' is not supported by the OPCBinary encoder.");
        }
    }

    private static void WriteInt16(OpcBinaryBufferWriter writer, short value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[2];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteUInt16(OpcBinaryBufferWriter writer, ushort value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[2];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteInt32(OpcBinaryBufferWriter writer, int value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[4];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteUInt32(OpcBinaryBufferWriter writer, uint value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[4];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteInt64(OpcBinaryBufferWriter writer, long value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[8];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteUInt64(OpcBinaryBufferWriter writer, ulong value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[8];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteSingle(OpcBinaryBufferWriter writer, float value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[4];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static void WriteDouble(OpcBinaryBufferWriter writer, double value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[8];
        if (byteOrder == ByteOrder.BigEndian)
        {
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        }

        writer.Write(buffer);
    }

    private static bool ToBoolean(object? value) =>
        value switch
        {
            bool typed => typed,
            string typed => bool.Parse(typed),
            _ => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
        };

    private static long ToFileTimeUtc(object? value) =>
        value switch
        {
            DateTime typed => typed.ToUniversalTime().ToFileTimeUtc(),
            DateTimeOffset typed => typed.UtcDateTime.ToFileTimeUtc(),
            long typed => typed,
            string typed => DateTimeOffset.Parse(typed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime.ToFileTimeUtc(),
            _ => Convert.ToInt64(value, CultureInfo.InvariantCulture),
        };

    private static Guid ToGuid(object? value) =>
        value switch
        {
            Guid typed => typed,
            string typed => Guid.Parse(typed),
            _ => throw new InvalidCastException("Value cannot be converted to a GUID."),
        };

    private static ComplexValue ToComplexValue(object? value, string fieldName) =>
        value as ComplexValue ?? throw new InvalidCastException($"Field '{fieldName}' must contain a ComplexValue.");

    private static object?[] MaterializeElements(object? value, int expectedCount, string fieldName)
    {
        if (value is string)
        {
            throw new InvalidCastException($"Field '{fieldName}' must contain {expectedCount.ToString(CultureInfo.InvariantCulture)} elements, not a string.");
        }

        if (value is not IEnumerable enumerable)
        {
            if (expectedCount == 1)
            {
                return new[] { value };
            }

            throw new InvalidCastException($"Field '{fieldName}' must contain {expectedCount.ToString(CultureInfo.InvariantCulture)} elements.");
        }

        var elements = new List<object?>();
        foreach (var element in enumerable)
        {
            elements.Add(element);
        }

        if (elements.Count != expectedCount)
        {
            throw new InvalidOperationException($"Field '{fieldName}' contains {elements.Count.ToString(CultureInfo.InvariantCulture)} elements; expected {expectedCount.ToString(CultureInfo.InvariantCulture)}.");
        }

        return elements.ToArray();
    }

    private static void WriteFixed(OpcBinaryBufferWriter writer, ReadOnlySpan<byte> bytes, int fixedByteLength, string fieldName)
    {
        if (fixedByteLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fixedByteLength), fixedByteLength, "Fixed byte length cannot be negative.");
        }

        if (bytes.Length > fixedByteLength)
        {
            throw new InvalidOperationException($"Encoded value for field '{fieldName}' exceeds its fixed length.");
        }

        writer.Write(bytes);
        writer.WriteZeros(fixedByteLength - bytes.Length);
    }

    private sealed class OpcBinaryBufferWriter
    {
        private readonly List<byte> _bytes = new();
        private int _bitOffset;

        public void AlignToByte() => _bitOffset = 0;

        public void WriteByte(byte value)
        {
            EnsureByteAligned();
            _bytes.Add(value);
        }

        public void Write(ReadOnlySpan<byte> bytes)
        {
            EnsureByteAligned();
            foreach (var value in bytes)
            {
                _bytes.Add(value);
            }
        }

        public void WriteBits(ReadOnlySpan<byte> bytes, int bitCount, string fieldName)
        {
            if (bitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Bit count cannot be negative.");
            }

            var requiredBytes = (bitCount + 7) / 8;
            if (bytes.Length > requiredBytes)
            {
                throw new InvalidOperationException($"Encoded value for field '{fieldName}' exceeds its fixed bit length.");
            }

            for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                if (_bitOffset == 0)
                {
                    _bytes.Add(0);
                }

                var sourceByte = bitIndex / 8 < bytes.Length ? bytes[bitIndex / 8] : (byte)0;
                var bit = (sourceByte >> (7 - (bitIndex % 8))) & 1;
                if (bit != 0)
                {
                    _bytes[^1] |= (byte)(1 << (7 - _bitOffset));
                }

                _bitOffset++;
                if (_bitOffset == 8)
                {
                    _bitOffset = 0;
                }
            }
        }

        public void WriteZeros(int count)
        {
            EnsureByteAligned();
            for (var i = 0; i < count; i++)
            {
                _bytes.Add(0);
            }
        }

        public byte[] ToArray() => _bytes.ToArray();

        private void EnsureByteAligned()
        {
            if (_bitOffset != 0)
            {
                throw new InvalidOperationException("OPCBinary byte-aligned field was written before BitString padding was completed.");
            }
        }
    }
}
