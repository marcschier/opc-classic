// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Numerics;

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
        if (OpcCpxTypeSemantics.ValidateRuntimeType(value, sourceKind) != OpcResultId.Ok.Code)
        {
            return OpcCpxConversionResult.BadType();
        }

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
            (TypeKind.String, TypeKind.Int32, string typed) => ConvertStringToInt32(typed),
            (TypeKind.Double, TypeKind.Single, double typed)
                when !double.IsFinite(typed) || (typed >= -float.MaxValue && typed <= float.MaxValue) =>
                OpcCpxConversionResult.Success((float)typed),
            (TypeKind.Double, TypeKind.Single, double) => OpcCpxConversionResult.Range(),
            _ => OpcCpxConversionResult.BadType(),
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
            return OpcCpxConversionResult.BadType();
        }

        var convertedElements = new ComplexValue[elements.Length];
        for (var i = 0; i < elements.Length; i++)
        {
            if (elements[i] is not ComplexValue nestedValue)
            {
                return OpcCpxConversionResult.BadType();
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
                    ? OpcCpxConversionResult.BadType()
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
        TypeDescription sourceContainingType,
        TypeDescription requestedContainingType,
        IReadOnlyDictionary<string, object?> sourceValues,
        IReadOnlyDictionary<string, object?>? requestedValues,
        TypeDictionary? sourceDictionary,
        TypeDictionary? requestedDictionary,
        int depth)
    {
        if (!HaveValidShapes(sourceField, requestedField))
        {
            return OpcCpxConversionResult.BadType();
        }

        var sourceValueError = ValidateFieldValue(
            sourceValue,
            sourceField,
            sourceContainingType,
            sourceValues,
            sourceDictionary);
        if (sourceValueError != OpcResultId.Ok.Code)
        {
            return OpcCpxConversionResult.FromError(sourceValueError);
        }

        if (sourceValue is null)
        {
            return ConvertNull(
                requestedField,
                requestedContainingType,
                requestedValues,
                requestedDictionary);
        }

        if (sourceField.Kind != TypeKind.StructReference
            && requestedField.Kind != TypeKind.StructReference)
        {
            return ConvertPrimitiveField(
                sourceValue,
                sourceField,
                requestedField,
                requestedContainingType,
                requestedValues,
                requestedDictionary);
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
            return OpcCpxConversionResult.BadType();
        }

        return ConvertComplex(
            nestedValue,
            sourceNestedType,
            requestedNestedType,
            sourceDictionary,
            requestedDictionary,
            depth + 1);
    }

    private OpcCpxConversionResult ConvertPrimitiveField(
        object sourceValue,
        TypeField sourceField,
        TypeField requestedField,
        TypeDescription requestedContainingType,
        IReadOnlyDictionary<string, object?>? requestedValues,
        TypeDictionary? requestedDictionary)
    {
        var converted = Convert(sourceValue, sourceField.Kind, requestedField.Kind);
        if (converted.Error != OpcResultId.Ok.Code)
        {
            return converted;
        }

        var requestedValueError = ValidateFieldValue(
            converted.Value,
            requestedField,
            requestedContainingType,
            requestedValues,
            requestedDictionary);
        return requestedValueError == OpcResultId.Ok.Code
            ? converted
            : OpcCpxConversionResult.FromError(requestedValueError);
    }

    private static OpcCpxConversionResult ConvertNull(
        TypeField requestedField,
        TypeDescription requestedContainingType,
        IReadOnlyDictionary<string, object?>? requestedValues,
        TypeDictionary? requestedDictionary)
    {
        var error = ValidateFieldValue(
            null,
            requestedField,
            requestedContainingType,
            requestedValues,
            requestedDictionary);
        return error == OpcResultId.Ok.Code
            ? OpcCpxConversionResult.Success(null)
            : OpcCpxConversionResult.FromError(error);
    }

    private static int ValidateFieldValue(
        object? value,
        TypeField field,
        TypeDescription containingType,
        IReadOnlyDictionary<string, object?>? containingValues,
        TypeDictionary? dictionary) =>
        OpcCpxTypeSemantics.ValidateScalar(
            value,
            field,
            containingType,
            dictionary,
            containingValues);

    private static bool HaveValidShapes(TypeField sourceField, TypeField requestedField) =>
        OpcCpxTypeSemantics.ValidateFieldShape(sourceField) == OpcResultId.Ok.Code
        && OpcCpxTypeSemantics.ValidateFieldShape(requestedField) == OpcResultId.Ok.Code;

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
        OpcCpxTypeSemantics.IsRepeated(field);

    private static OpcCpxConversionResult ConvertIntegral(
        object? value,
        TypeKind sourceKind,
        TypeKind requestedKind)
    {
        if (!TryGetIntegral(value, sourceKind, out var isUnsigned, out var signed, out var unsigned))
        {
            return OpcCpxConversionResult.BadType();
        }

        object? converted = isUnsigned
            ? ConvertUnsigned(unsigned, requestedKind)
            : ConvertSigned(signed, requestedKind);
        return converted is null
            ? OpcCpxConversionResult.Range()
            : OpcCpxConversionResult.Success(converted);
    }

    private static OpcCpxConversionResult ConvertStringToInt32(string value)
    {
        if (!BigInteger.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return OpcCpxConversionResult.BadType();
        }

        return parsed >= int.MinValue && parsed <= int.MaxValue
            ? OpcCpxConversionResult.Success((int)parsed)
            : OpcCpxConversionResult.Range();
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
                return _depth > MaxNestingDepth
                    ? OpcCpxConversionResult.Range()
                    : OpcCpxConversionResult.BadType();
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
                return OpcCpxConversionResult.BadType();
            }

            _states[index] = FieldState.Converting;
            var requestedField = _requestedType.Fields[index];
            if (!TryGetField(_sourceType, requestedField.Name, out var sourceField))
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            if (!_value.Fields.TryGetValue(sourceField.Name, out var sourceValue))
            {
                if (sourceField.MinOccurs == 0 && requestedField.MinOccurs == 0)
                {
                    _convertedValues[index] = null;
                    _states[index] = FieldState.Converted;
                    return OpcCpxConversionResult.Success(null);
                }

                return OpcCpxConversionResult.BadType();
            }

            if (IsArrayField(sourceField) != IsArrayField(requestedField))
            {
                return OpcCpxConversionResult.BadType();
            }

            var converted = IsArrayField(sourceField)
                ? ConvertRepeated(sourceValue, sourceField, requestedField)
                : _converter.ConvertField(
                    sourceValue,
                    sourceField,
                    requestedField,
                    _sourceType,
                    _requestedType,
                    _value.Fields,
                    GetRequestedContainingValues(requestedField),
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
            if (OpcCpxTypeSemantics.ValidateFieldShape(sourceField) != OpcResultId.Ok.Code
                || OpcCpxTypeSemantics.ValidateFieldShape(requestedField) != OpcResultId.Ok.Code)
            {
                return OpcCpxConversionResult.BadType();
            }

            var materializeError = OpcCpxTypeSemantics.Materialize(sourceValue, out var elements);
            if (materializeError != OpcResultId.Ok.Code)
            {
                return OpcCpxConversionResult.FromError(materializeError);
            }

            var sourceCountError = OpcCpxTypeSemantics.ValidateOccurrenceCount(
                sourceField,
                _sourceType,
                _value.Fields,
                elements.Length);
            if (sourceCountError != OpcResultId.Ok.Code)
            {
                return OpcCpxConversionResult.FromError(sourceCountError);
            }

            IReadOnlyDictionary<string, object?>? requestedValues = null;
            if (requestedField.ElementCountFieldName is { } countName)
            {
                var countResult = ConvertCountField(countName);
                if (countResult.Error != OpcResultId.Ok.Code)
                {
                    return countResult;
                }

                requestedValues = new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    [countName] = countResult.Value,
                };
            }

            var requestedCountError = OpcCpxTypeSemantics.ValidateOccurrenceCount(
                requestedField,
                _requestedType,
                requestedValues,
                elements.Length);
            if (requestedCountError != OpcResultId.Ok.Code)
            {
                return OpcCpxConversionResult.FromError(requestedCountError);
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
                : OpcCpxConversionResult.BadType();

        private IReadOnlyDictionary<string, object?>? GetRequestedContainingValues(TypeField field)
        {
            if (field.ElementCountFieldName is not { } countName)
            {
                return null;
            }

            var converted = ConvertCountField(countName);
            return converted.Error == OpcResultId.Ok.Code
                ? new Dictionary<string, object?>(StringComparer.Ordinal) { [countName] = converted.Value }
                : null;
        }
    }

    private enum FieldState
    {
        NotConverted,
        Converting,
        Converted,
    }
}
