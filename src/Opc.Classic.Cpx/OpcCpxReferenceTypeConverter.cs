// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections;
using System.Globalization;

namespace Opc.Classic.Cpx;

/// <summary>Deterministic reference implementation of CPX type conversion.</summary>
public sealed class OpcCpxReferenceTypeConverter : IOpcCpxTypeConverter
{
    /// <summary>Maximum supported complex-value nesting depth.</summary>
    public const int MaxNestingDepth = 32;

    /// <summary>Maximum supported element count for one repeated field.</summary>
    public const int MaxArrayElements = 65_536;

    /// <inheritdoc />
    public OpcCpxConversionResult Convert(object? value, TypeKind sourceKind, TypeKind requestedKind)
    {
        if (sourceKind == requestedKind)
        {
            return OpcCpxConversionResult.Success(value);
        }

        if (IsIntegral(sourceKind) && IsIntegral(requestedKind))
        {
            return ConvertIntegral(value, sourceKind, requestedKind);
        }

        return (sourceKind, requestedKind, value) switch
        {
            (TypeKind.Boolean, TypeKind.Int32, bool typed) => OpcCpxConversionResult.Success(typed ? 1 : 0),
            (TypeKind.Single, TypeKind.Double, float typed) => OpcCpxConversionResult.Success((double)typed),
            (TypeKind.Int32, TypeKind.Double, int typed) => OpcCpxConversionResult.Success((double)typed),
            (TypeKind.String, TypeKind.Int32, string typed)
                when int.TryParse(typed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) =>
                OpcCpxConversionResult.Success(parsed),
            (TypeKind.Double, TypeKind.Single, double typed)
                when !double.IsFinite(typed) || (typed >= -float.MaxValue && typed <= float.MaxValue) =>
                OpcCpxConversionResult.Success((float)typed),
            _ => OpcCpxConversionResult.TypeChanged(),
        };
    }

    /// <inheritdoc />
    public OpcCpxConversionResult Convert(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(requestedType);
        return ConvertComplex(value, sourceType, requestedType, null, null, 0);
    }

    /// <inheritdoc />
    public OpcCpxConversionResult Convert(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType,
        TypeDictionary sourceDictionary,
        TypeDictionary requestedDictionary)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(requestedType);
        ArgumentNullException.ThrowIfNull(sourceDictionary);
        ArgumentNullException.ThrowIfNull(requestedDictionary);
        return ConvertComplex(value, sourceType, requestedType, sourceDictionary, requestedDictionary, 0);
    }

    private OpcCpxConversionResult ConvertComplex(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType,
        TypeDictionary? sourceDictionary,
        TypeDictionary? requestedDictionary,
        int depth) =>
        new ComplexConversion(
            this,
            value,
            sourceType,
            requestedType,
            sourceDictionary,
            requestedDictionary,
            depth).Convert();

    private OpcCpxConversionResult ConvertArray(
        object?[] elements,
        TypeField sourceField,
        TypeField requestedField,
        TypeDictionary? sourceDictionary,
        TypeDictionary? requestedDictionary,
        int depth)
    {
        if (sourceField.Kind == TypeKind.StructReference
            || requestedField.Kind == TypeKind.StructReference)
        {
            return ConvertStructArray(
                elements,
                sourceField,
                requestedField,
                sourceDictionary,
                requestedDictionary,
                depth);
        }

        var convertedElements = new object?[elements.Length];
        for (var i = 0; i < elements.Length; i++)
        {
            var converted = Convert(elements[i], sourceField.Kind, requestedField.Kind);
            if (converted.Error != OpcResultId.Ok.Code)
            {
                return converted;
            }

            convertedElements[i] = converted.Value;
        }

        return OpcCpxConversionResult.Success(convertedElements);
    }

    private OpcCpxConversionResult ConvertStructArray(
        object?[] elements,
        TypeField sourceField,
        TypeField requestedField,
        TypeDictionary? sourceDictionary,
        TypeDictionary? requestedDictionary,
        int depth)
    {
        if (!TryResolveTypes(
                sourceField,
                requestedField,
                sourceDictionary,
                requestedDictionary,
                out var sourceNestedType,
                out var requestedNestedType))
        {
            return OpcCpxConversionResult.TypeChanged();
        }

        var convertedElements = new ComplexValue[elements.Length];
        for (var i = 0; i < elements.Length; i++)
        {
            if (elements[i] is not ComplexValue nestedValue)
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            var converted = ConvertComplex(
                nestedValue,
                sourceNestedType,
                requestedNestedType,
                sourceDictionary,
                requestedDictionary,
                depth + 1);
            if (converted.Error != OpcResultId.Ok.Code || converted.Value is not ComplexValue convertedValue)
            {
                return converted.Error == OpcResultId.Ok.Code
                    ? OpcCpxConversionResult.TypeChanged()
                    : converted;
            }

            convertedElements[i] = convertedValue;
        }

        return OpcCpxConversionResult.Success(convertedElements);
    }

    private OpcCpxConversionResult ConvertField(
        object? sourceValue,
        TypeField sourceField,
        TypeField requestedField,
        TypeDictionary? sourceDictionary,
        TypeDictionary? requestedDictionary,
        int depth)
    {
        if (sourceField.Kind != TypeKind.StructReference
            && requestedField.Kind != TypeKind.StructReference)
        {
            return Convert(sourceValue, sourceField.Kind, requestedField.Kind);
        }

        if (sourceValue is not ComplexValue nestedValue
            || !TryResolveTypes(
                sourceField,
                requestedField,
                sourceDictionary,
                requestedDictionary,
                out var sourceNestedType,
                out var requestedNestedType))
        {
            return OpcCpxConversionResult.TypeChanged();
        }

        return ConvertComplex(
            nestedValue,
            sourceNestedType,
            requestedNestedType,
            sourceDictionary,
            requestedDictionary,
            depth + 1);
    }

    private static bool TryResolveTypes(
        TypeField sourceField,
        TypeField requestedField,
        TypeDictionary? sourceDictionary,
        TypeDictionary? requestedDictionary,
        out TypeDescription sourceType,
        out TypeDescription requestedType)
    {
        sourceType = null!;
        requestedType = null!;
        if (sourceField.Kind != TypeKind.StructReference
            || requestedField.Kind != TypeKind.StructReference
            || sourceField.TypeId is null
            || requestedField.TypeId is null
            || sourceDictionary is null
            || requestedDictionary is null)
        {
            return false;
        }

        sourceType = sourceDictionary.TryGetByTypeId(sourceField.TypeId)!;
        requestedType = requestedDictionary.TryGetByTypeId(requestedField.TypeId)!;
        return sourceType is not null && requestedType is not null;
    }

    private static bool IsArrayField(TypeField field) =>
        field.Kind is not TypeKind.String and not TypeKind.Blob and not TypeKind.BitString
        && (field.ElementCount is not null || field.ElementCountFieldName is not null);

    private static bool TryGetCount(
        TypeField field,
        TypeDescription containingType,
        IReadOnlyDictionary<string, object?>? values,
        Func<string, OpcCpxConversionResult>? convertCountField,
        out int count)
    {
        count = 0;
        if (field.ElementCount is { } fixedCount)
        {
            count = fixedCount;
            return fixedCount <= MaxArrayElements;
        }

        if (field.ElementCountFieldName is not { } countName
            || !TryGetField(containingType, countName, out _)
            || !TryGetCountValue(countName, values, convertCountField, out var countValue))
        {
            return false;
        }

        try
        {
            count = OpcBinaryCodecUtilities.ToNonNegativeInt32(countValue, countName);
            return count <= MaxArrayElements;
        }
        catch (Exception exception) when (exception is FormatException
            or InvalidCastException
            or InvalidOperationException
            or OverflowException)
        {
            return false;
        }
    }

    private static bool TryGetCountValue(
        string countName,
        IReadOnlyDictionary<string, object?>? values,
        Func<string, OpcCpxConversionResult>? convertCountField,
        out object? countValue)
    {
        if (values is not null)
        {
            return values.TryGetValue(countName, out countValue);
        }

        if (convertCountField is not null)
        {
            var converted = convertCountField(countName);
            countValue = converted.Value;
            return converted.Error == OpcResultId.Ok.Code;
        }

        countValue = null;
        return false;
    }

    private static bool TryMaterialize(object? value, int expectedCount, out object?[] elements)
    {
        elements = Array.Empty<object?>();
        if (value is string || value is not IEnumerable enumerable)
        {
            return false;
        }

        if (value is ICollection collection
            && (collection.Count != expectedCount || collection.Count > MaxArrayElements))
        {
            return false;
        }

        var materialized = new List<object?>(Math.Min(expectedCount, MaxArrayElements));
        foreach (var element in enumerable)
        {
            if (materialized.Count == MaxArrayElements)
            {
                return false;
            }

            materialized.Add(element);
        }

        if (materialized.Count != expectedCount)
        {
            return false;
        }

        elements = materialized.ToArray();
        return true;
    }

    private static OpcCpxConversionResult ConvertIntegral(
        object? value,
        TypeKind sourceKind,
        TypeKind requestedKind)
    {
        if (!TryGetIntegral(value, sourceKind, out var isUnsigned, out var signed, out var unsigned))
        {
            return OpcCpxConversionResult.TypeChanged();
        }

        object? converted = isUnsigned
            ? ConvertUnsigned(unsigned, requestedKind)
            : ConvertSigned(signed, requestedKind);
        return converted is null
            ? OpcCpxConversionResult.TypeChanged()
            : OpcCpxConversionResult.Success(converted);
    }

    private static object? ConvertSigned(long value, TypeKind requestedKind) =>
        requestedKind switch
        {
            TypeKind.Int8 when value is >= sbyte.MinValue and <= sbyte.MaxValue => (sbyte)value,
            TypeKind.UInt8 when value is >= byte.MinValue and <= byte.MaxValue => (byte)value,
            TypeKind.Int16 when value is >= short.MinValue and <= short.MaxValue => (short)value,
            TypeKind.UInt16 when value is >= ushort.MinValue and <= ushort.MaxValue => (ushort)value,
            TypeKind.Int32 when value is >= int.MinValue and <= int.MaxValue => (int)value,
            TypeKind.UInt32 when value is >= uint.MinValue and <= uint.MaxValue => (uint)value,
            TypeKind.Int64 => value,
            TypeKind.UInt64 when value >= 0 => (ulong)value,
            _ => null,
        };

    private static object? ConvertUnsigned(ulong value, TypeKind requestedKind) =>
        requestedKind switch
        {
            TypeKind.Int8 when value <= (ulong)sbyte.MaxValue => (sbyte)value,
            TypeKind.UInt8 when value <= byte.MaxValue => (byte)value,
            TypeKind.Int16 when value <= (ulong)short.MaxValue => (short)value,
            TypeKind.UInt16 when value <= ushort.MaxValue => (ushort)value,
            TypeKind.Int32 when value <= int.MaxValue => (int)value,
            TypeKind.UInt32 when value <= uint.MaxValue => (uint)value,
            TypeKind.Int64 when value <= long.MaxValue => (long)value,
            TypeKind.UInt64 => value,
            _ => null,
        };

    private static bool TryGetIntegral(
        object? value,
        TypeKind sourceKind,
        out bool isUnsigned,
        out long signed,
        out ulong unsigned)
    {
        isUnsigned = false;
        signed = 0;
        unsigned = 0;
        switch (sourceKind, value)
        {
            case (TypeKind.Int8, sbyte typed):
                signed = typed;
                return true;
            case (TypeKind.UInt8, byte typed):
                isUnsigned = true;
                unsigned = typed;
                return true;
            case (TypeKind.Int16, short typed):
                signed = typed;
                return true;
            case (TypeKind.UInt16, ushort typed):
                isUnsigned = true;
                unsigned = typed;
                return true;
            case (TypeKind.Int32, int typed):
                signed = typed;
                return true;
            case (TypeKind.UInt32, uint typed):
                isUnsigned = true;
                unsigned = typed;
                return true;
            case (TypeKind.Int64, long typed):
                signed = typed;
                return true;
            case (TypeKind.UInt64, ulong typed):
                isUnsigned = true;
                unsigned = typed;
                return true;
            default:
                return false;
        }
    }

    private static bool IsIntegral(TypeKind kind) =>
        kind is TypeKind.Int8
            or TypeKind.UInt8
            or TypeKind.Int16
            or TypeKind.UInt16
            or TypeKind.Int32
            or TypeKind.UInt32
            or TypeKind.Int64
            or TypeKind.UInt64;

    private static bool TryGetField(TypeDescription type, string name, out TypeField field)
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

    private static bool TryGetFieldIndex(TypeDescription type, string name, out int index)
    {
        for (var i = 0; i < type.Fields.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(type.Fields[i].Name, name))
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private static StructType ToStructType(TypeDescription type, TypeDictionary? dictionary) =>
        new()
        {
            Name = type.Name,
            DefaultByteOrder = OpcBinaryCodecUtilities.GetDefaultByteOrder(type, dictionary),
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

    private sealed class ComplexConversion
    {
        private readonly OpcCpxReferenceTypeConverter _converter;
        private readonly ComplexValue _value;
        private readonly TypeDescription _sourceType;
        private readonly TypeDescription _requestedType;
        private readonly TypeDictionary? _sourceDictionary;
        private readonly TypeDictionary? _requestedDictionary;
        private readonly int _depth;
        private readonly object?[] _convertedValues;
        private readonly FieldState[] _states;

        public ComplexConversion(
            OpcCpxReferenceTypeConverter converter,
            ComplexValue value,
            TypeDescription sourceType,
            TypeDescription requestedType,
            TypeDictionary? sourceDictionary,
            TypeDictionary? requestedDictionary,
            int depth)
        {
            _converter = converter;
            _value = value;
            _sourceType = sourceType;
            _requestedType = requestedType;
            _sourceDictionary = sourceDictionary;
            _requestedDictionary = requestedDictionary;
            _depth = depth;
            _convertedValues = new object?[requestedType.Fields.Count];
            _states = new FieldState[requestedType.Fields.Count];
        }

        public OpcCpxConversionResult Convert()
        {
            if (_depth > MaxNestingDepth || _value.Fields is null)
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            for (var i = 0; i < _requestedType.Fields.Count; i++)
            {
                var converted = ConvertFieldAt(i);
                if (converted.Error != OpcResultId.Ok.Code)
                {
                    return converted;
                }
            }

            var fields = new Dictionary<string, object?>(_requestedType.Fields.Count, StringComparer.Ordinal);
            for (var i = 0; i < _requestedType.Fields.Count; i++)
            {
                fields[_requestedType.Fields[i].Name] = _convertedValues[i];
            }

            return OpcCpxConversionResult.Success(new ComplexValue
            {
                Type = ToStructType(_requestedType, _requestedDictionary),
                Fields = fields,
            });
        }

        private OpcCpxConversionResult ConvertFieldAt(int index)
        {
            if (_states[index] == FieldState.Converted)
            {
                return OpcCpxConversionResult.Success(_convertedValues[index]);
            }

            if (_states[index] == FieldState.Converting)
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            _states[index] = FieldState.Converting;
            var requestedField = _requestedType.Fields[index];
            if (!TryGetField(_sourceType, requestedField.Name, out var sourceField)
                || !_value.Fields.TryGetValue(sourceField.Name, out var sourceValue)
                || IsArrayField(sourceField) != IsArrayField(requestedField))
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            var converted = IsArrayField(sourceField)
                ? ConvertRepeated(sourceValue, sourceField, requestedField)
                : _converter.ConvertField(
                    sourceValue,
                    sourceField,
                    requestedField,
                    _sourceDictionary,
                    _requestedDictionary,
                    _depth);
            if (converted.Error != OpcResultId.Ok.Code)
            {
                return converted;
            }

            _convertedValues[index] = converted.Value;
            _states[index] = FieldState.Converted;
            return converted;
        }

        private OpcCpxConversionResult ConvertRepeated(
            object? sourceValue,
            TypeField sourceField,
            TypeField requestedField)
        {
            if (!TryGetCount(sourceField, _sourceType, _value.Fields, null, out var sourceCount)
                || !TryGetCount(requestedField, _requestedType, null, ConvertCountField, out var requestedCount)
                || sourceCount != requestedCount
                || !TryMaterialize(sourceValue, sourceCount, out var elements))
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            return _converter.ConvertArray(
                elements,
                sourceField,
                requestedField,
                _sourceDictionary,
                _requestedDictionary,
                _depth);
        }

        private OpcCpxConversionResult ConvertCountField(string name) =>
            TryGetFieldIndex(_requestedType, name, out var index)
                ? ConvertFieldAt(index)
                : OpcCpxConversionResult.TypeChanged();
    }

    private enum FieldState
    {
        NotConverted,
        Converting,
        Converted,
    }
}
