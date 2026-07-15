// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections;

namespace Opc.Classic.Cpx;

internal static class OpcCpxTypeSemantics
{
    public static int ValidateRuntimeType(object? value, TypeKind kind) =>
        value is not null && HasExpectedClrType(value, kind)
            ? OpcResultId.Ok.Code
            : OpcResultId.BadType.Code;

    public static int ValidateFieldShape(TypeField field)
    {
        if ((field.Kind == TypeKind.StructReference) != (field.TypeId is not null)
            || (field.ElementCount is not null && field.ElementCountFieldName is not null)
            || (field.MinOccurs is { } minimum
                && minimum > (field.ElementCount ?? 1)))
        {
            return OpcResultId.BadType.Code;
        }

        if (field.Kind == TypeKind.BitString && field.Length is not > 0)
        {
            return OpcResultId.BadType.Code;
        }

        if (field.Length is { } length
            && IsFixedPrimitive(field.Kind)
            && length != OpcBinaryCodecUtilities.GetPrimitiveSize(field.Kind))
        {
            return OpcResultId.BadType.Code;
        }

        return OpcResultId.Ok.Code;
    }

    public static int ValidateScalar(
        object? value,
        TypeField field,
        TypeDescription containingType,
        TypeDictionary? dictionary,
        IReadOnlyDictionary<string, object?>? containingValues = null)
    {
        var shapeError = ValidateFieldShape(field);
        if (shapeError != OpcResultId.Ok.Code)
        {
            return shapeError;
        }

        if (value is null)
        {
            return field.MinOccurs == 0 ? OpcResultId.Ok.Code : OpcResultId.BadType.Code;
        }

        if (!HasExpectedClrType(value, field.Kind))
        {
            return OpcResultId.BadType.Code;
        }

        return field.Kind switch
        {
            TypeKind.String => ValidateString((string)value, field, containingType, dictionary, containingValues),
            TypeKind.Blob => ValidateBytes((byte[])value, field, containingValues, bitString: false),
            TypeKind.BitString => ValidateBytes((byte[])value, field, containingValues, bitString: true),
            _ => OpcResultId.Ok.Code,
        };
    }

    public static bool IsRepeated(TypeField field) =>
        field.Kind is not TypeKind.String and not TypeKind.Blob and not TypeKind.BitString
        && (field.ElementCount is not null || field.ElementCountFieldName is not null);

    public static int Materialize(object? value, out object?[] elements)
    {
        elements = Array.Empty<object?>();
        if (value is string || value is not IEnumerable enumerable)
        {
            return OpcResultId.BadType.Code;
        }

        if (value is ICollection collection && collection.Count > OpcCpxReferenceTypeConverter.MaxArrayElements)
        {
            return OpcResultId.Range.Code;
        }

        var materialized = new List<object?>();
        foreach (var element in enumerable)
        {
            if (materialized.Count == OpcCpxReferenceTypeConverter.MaxArrayElements)
            {
                return OpcResultId.Range.Code;
            }

            materialized.Add(element);
        }

        elements = materialized.ToArray();
        return OpcResultId.Ok.Code;
    }

    public static int ValidateOccurrenceCount(
        TypeField field,
        TypeDescription containingType,
        IReadOnlyDictionary<string, object?>? values,
        int actualCount)
    {
        var shapeError = ValidateFieldShape(field);
        if (shapeError != OpcResultId.Ok.Code)
        {
            return shapeError;
        }

        if (field.MinOccurs is { } minimum)
        {
            if (actualCount < minimum
                || (field.ElementCount is { } maximum && actualCount > maximum))
            {
                return OpcResultId.Range.Code;
            }

            return OpcResultId.Ok.Code;
        }

        if (field.ElementCount is { } fixedCount)
        {
            return actualCount == fixedCount ? OpcResultId.Ok.Code : OpcResultId.Range.Code;
        }

        if (field.ElementCountFieldName is not { } countName
            || !TryGetField(containingType, countName, out _)
            || values is null
            || !values.TryGetValue(countName, out var countValue))
        {
            return OpcResultId.BadType.Code;
        }

        try
        {
            return actualCount == OpcBinaryCodecUtilities.ToNonNegativeInt32(countValue, countName)
                ? OpcResultId.Ok.Code
                : OpcResultId.Range.Code;
        }
        catch (Exception exception) when (exception is FormatException
            or InvalidCastException
            or InvalidOperationException
            or OverflowException)
        {
            return OpcResultId.BadType.Code;
        }
    }

    public static bool TryGetField(TypeDescription type, string name, out TypeField field)
    {
        foreach (var candidate in type.Fields)
        {
            if (StringComparer.Ordinal.Equals(candidate.Name, name))
            {
                field = candidate;
                return true;
            }
        }

        field = null!;
        return false;
    }

    private static bool HasExpectedClrType(object value, TypeKind kind) =>
        kind switch
        {
            TypeKind.Boolean => value is bool,
            TypeKind.Int8 => value is sbyte,
            TypeKind.UInt8 => value is byte,
            TypeKind.Int16 => value is short,
            TypeKind.UInt16 => value is ushort,
            TypeKind.Int32 => value is int,
            TypeKind.UInt32 => value is uint,
            TypeKind.Int64 => value is long,
            TypeKind.UInt64 => value is ulong,
            TypeKind.Single => value is float,
            TypeKind.Double => value is double,
            TypeKind.String => value is string,
            TypeKind.FileTime => value is DateTime or DateTimeOffset,
            TypeKind.Guid => value is Guid,
            TypeKind.Blob or TypeKind.BitString => value is byte[],
            TypeKind.StructReference => value is ComplexValue,
            _ => false,
        };

    private static int ValidateString(
        string value,
        TypeField field,
        TypeDescription containingType,
        TypeDictionary? dictionary,
        IReadOnlyDictionary<string, object?>? containingValues)
    {
        var bytes = OpcBinaryCodecUtilities.EncodeString(
            value,
            OpcBinaryCodecUtilities.GetStringEncoding(containingType, field, dictionary),
            OpcBinaryCodecUtilities.GetCharWidth(containingType, field, dictionary),
            OpcBinaryCodecUtilities.GetByteOrder(containingType, field, dictionary));
        var charWidth = OpcBinaryCodecUtilities.GetCharWidth(containingType, field, dictionary);
        var capacityResult = GetByteCapacity(field, containingValues);
        if (capacityResult.Error != OpcResultId.Ok.Code)
        {
            return capacityResult.Error;
        }

        var capacityValue = (long)capacityResult.Capacity * (field.Length is null ? charWidth : 1);
        if (capacityValue > int.MaxValue)
        {
            return OpcResultId.Range.Code;
        }

        var capacity = capacityResult.HasCapacity ? (int)capacityValue : (int?)null;
        return capacity is null || bytes.Length <= capacity
            ? OpcResultId.Ok.Code
            : OpcResultId.Range.Code;
    }

    private static int ValidateBytes(
        byte[] value,
        TypeField field,
        IReadOnlyDictionary<string, object?>? containingValues,
        bool bitString)
    {
        if (!bitString)
        {
            var capacityResult = GetByteCapacity(field, containingValues);
            if (capacityResult.Error != OpcResultId.Ok.Code)
            {
                return capacityResult.Error;
            }

            var capacity = capacityResult.HasCapacity ? capacityResult.Capacity : (int?)null;
            return capacity is null || value.Length <= capacity
                ? OpcResultId.Ok.Code
                : OpcResultId.Range.Code;
        }

        var elementCount = GetElementCount(field, containingValues);
        if (elementCount.Error != OpcResultId.Ok.Code)
        {
            return elementCount.Error;
        }

        int bitCount;
        try
        {
            bitCount = checked(field.Length!.Value * elementCount.Count);
        }
        catch (OverflowException)
        {
            return OpcResultId.Range.Code;
        }

        var byteCount = (int)((bitCount + 7L) / 8L);
        if (value.Length != byteCount)
        {
            return OpcResultId.Range.Code;
        }

        var unusedBits = byteCount * 8 - bitCount;
        if (unusedBits > 0 && (value[^1] & ((1 << unusedBits) - 1)) != 0)
        {
            return OpcResultId.BadType.Code;
        }

        return OpcResultId.Ok.Code;
    }

    private static (int Capacity, int Error, bool HasCapacity) GetByteCapacity(
        TypeField field,
        IReadOnlyDictionary<string, object?>? containingValues)
    {
        if (field.Length is { } length)
        {
            return (length, OpcResultId.Ok.Code, true);
        }

        var count = GetElementCount(field, containingValues);
        return count.Error == OpcResultId.Ok.Code
            ? (count.Count, OpcResultId.Ok.Code, count.HasCount)
            : (0, count.Error, false);
    }

    private static (int Count, int Error, bool HasCount) GetElementCount(
        TypeField field,
        IReadOnlyDictionary<string, object?>? containingValues)
    {
        if (field.ElementCount is { } fixedCount)
        {
            return (fixedCount, OpcResultId.Ok.Code, true);
        }

        if (field.ElementCountFieldName is not { } countName)
        {
            return (1, OpcResultId.Ok.Code, false);
        }

        if (containingValues is null || !containingValues.TryGetValue(countName, out var countValue))
        {
            return (0, OpcResultId.BadType.Code, false);
        }

        try
        {
            return (
                OpcBinaryCodecUtilities.ToNonNegativeInt32(countValue, countName),
                OpcResultId.Ok.Code,
                true);
        }
        catch (Exception exception) when (exception is FormatException
            or InvalidCastException
            or InvalidOperationException
            or OverflowException)
        {
            return (0, OpcResultId.BadType.Code, false);
        }
    }

    private static bool IsFixedPrimitive(TypeKind kind) =>
        kind is TypeKind.Boolean
            or TypeKind.Int8
            or TypeKind.UInt8
            or TypeKind.Int16
            or TypeKind.UInt16
            or TypeKind.Int32
            or TypeKind.UInt32
            or TypeKind.Int64
            or TypeKind.UInt64
            or TypeKind.Single
            or TypeKind.Double
            or TypeKind.FileTime
            or TypeKind.Guid;
}
