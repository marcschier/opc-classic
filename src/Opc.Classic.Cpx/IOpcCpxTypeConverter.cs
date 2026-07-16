// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>Converts OPC Complex Data values between declared CPX types.</summary>
public interface IOpcCpxTypeConverter
{
    /// <summary>Converts a primitive value between CPX kinds.</summary>
    OpcCpxConversionResult Convert(object? value, TypeKind sourceKind, TypeKind requestedKind);

    /// <summary>Converts a complex value between directly described CPX types.</summary>
    OpcCpxConversionResult Convert(ComplexValue value, TypeDescription sourceType, TypeDescription requestedType);

    /// <summary>Converts a complex value using independent source and requested dictionaries.</summary>
    OpcCpxConversionResult Convert(
        ComplexValue value,
        TypeDescription sourceType,
        TypeDescription requestedType,
        TypeDictionary sourceDictionary,
        TypeDictionary requestedDictionary);
}
