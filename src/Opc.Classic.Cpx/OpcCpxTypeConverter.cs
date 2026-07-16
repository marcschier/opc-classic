// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// OPC Complex Data §7 type-conversion helper for server-side alternate representations.
/// </summary>
public static class OpcCpxTypeConverter
{
    private static readonly IOpcCpxTypeConverter s_referenceConverter = new OpcCpxReferenceTypeConverter();

    /// <summary>
    /// Converts a primitive value from one CPX kind to another representative native kind.
    /// </summary>
    public static OpcCpxConversionResult Convert(object? value, TypeKind sourceKind, TypeKind requestedKind) =>
        s_referenceConverter.Convert(value, sourceKind, requestedKind);

    /// <summary>
    /// Converts fields with matching names from <paramref name="sourceType"/> to <paramref name="requestedType"/>.
    /// </summary>
    public static OpcCpxConversionResult Convert(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType) =>
        s_referenceConverter.Convert(value, sourceType, requestedType);

    /// <summary>
    /// Converts fields between types using separate dictionaries to resolve referenced types.
    /// </summary>
    public static OpcCpxConversionResult Convert(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType,
        TypeDictionary sourceDictionary,
        TypeDictionary requestedDictionary) =>
        s_referenceConverter.Convert(value, sourceType, requestedType, sourceDictionary, requestedDictionary);
}
