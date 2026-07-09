// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;

namespace Opc.Classic.Cpx;

/// <summary>
/// Minimal OPC Complex Data §7 type-conversion helper for server-side alternate representations.
/// </summary>
public static class OpcCpxTypeConverter
{
    /// <summary>
    /// Converts a primitive value from one CPX kind to another representative native kind.
    /// </summary>
    public static OpcCpxConversionResult Convert(object? value, TypeKind sourceKind, TypeKind requestedKind)
    {
        if (sourceKind == requestedKind)
        {
            return OpcCpxConversionResult.Success(value);
        }

        try
        {
            return (sourceKind, requestedKind, value) switch
            {
                (TypeKind.Boolean, TypeKind.Int32, bool typed) => OpcCpxConversionResult.Success(typed ? 1 : 0),
                (TypeKind.Int8, TypeKind.Int32, sbyte typed) => OpcCpxConversionResult.Success((int)typed),
                (TypeKind.UInt8, TypeKind.Int32, byte typed) => OpcCpxConversionResult.Success((int)typed),
                (TypeKind.Int16, TypeKind.Int32, short typed) => OpcCpxConversionResult.Success((int)typed),
                (TypeKind.UInt16, TypeKind.Int32, ushort typed) => OpcCpxConversionResult.Success((int)typed),
                (TypeKind.Int32, TypeKind.Int64, int typed) => OpcCpxConversionResult.Success((long)typed),
                (TypeKind.UInt32, TypeKind.Int64, uint typed) => OpcCpxConversionResult.Success((long)typed),
                (TypeKind.Single, TypeKind.Double, float typed) => OpcCpxConversionResult.Success((double)typed),
                (TypeKind.Int32, TypeKind.Double, int typed) => OpcCpxConversionResult.Success((double)typed),
                (TypeKind.String, TypeKind.Int32, string typed) when int.TryParse(typed, System.Globalization.CultureInfo.InvariantCulture, out var parsed) =>
                    OpcCpxConversionResult.Success(parsed),
                _ => OpcCpxConversionResult.TypeChanged(),
            };
        }
        catch (OverflowException)
        {
            return OpcCpxConversionResult.TypeChanged();
        }
        catch (InvalidCastException)
        {
            return OpcCpxConversionResult.TypeChanged();
        }
    }

    /// <summary>
    /// Converts fields with matching names from <paramref name="sourceType"/> to <paramref name="requestedType"/>.
    /// </summary>
    public static OpcCpxConversionResult Convert(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(requestedType);

        var fields = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var requestedField in requestedType.Fields)
        {
            if (!TryGetField(sourceType, requestedField.Name, out var sourceField)
                || !value.Fields.TryGetValue(sourceField.Name, out var sourceValue))
            {
                return OpcCpxConversionResult.TypeChanged();
            }

            var converted = Convert(sourceValue, sourceField.Kind, requestedField.Kind);
            if (converted.Error != OpcResultId.Ok.Code)
            {
                return converted;
            }

            fields[requestedField.Name] = converted.Value;
        }

        return OpcCpxConversionResult.Success(new ComplexValue
        {
            Type = ToStructType(requestedType),
            Fields = fields,
        });
    }

    private static bool TryGetField(TypeDescription type, string fieldName, out TypeField field)
    {
        foreach (var candidate in type.Fields)
        {
            if (StringComparer.Ordinal.Equals(candidate.Name, fieldName))
            {
                field = candidate;
                return true;
            }
        }

        field = null!;
        return false;
    }

    private static StructType ToStructType(TypeDescription type)
    {
        var fields = new StructField[type.Fields.Count];
        for (var i = 0; i < fields.Length; i++)
        {
            var field = type.Fields[i];
            fields[i] = new StructField
            {
                Name = field.Name,
                Kind = field.Kind,
                TypeReference = field.TypeId,
                Repeats = field.ElementCount ?? 0,
                CountFieldName = field.ElementCountFieldName,
                ByteOrder = field.ByteOrder,
            };
        }

        return new StructType
        {
            Name = type.Name,
            Fields = fields,
        };
    }
}
