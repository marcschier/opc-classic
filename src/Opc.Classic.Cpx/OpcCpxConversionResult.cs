// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// Result of a CPX type conversion attempt.
/// </summary>
public readonly record struct OpcCpxConversionResult(object? Value, int Error)
{
    /// <summary>
    /// Creates a successful conversion result.
    /// </summary>
    public static OpcCpxConversionResult Success(object? value) => new(value, OpcResultId.Ok.Code);

    /// <summary>
    /// Creates the CPX result used when dictionary or type metadata changed
    /// while a conversion was in progress.
    /// </summary>
    public static OpcCpxConversionResult TypeChanged() => new(null, OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);

    /// <summary>Creates an OPC_E_BADTYPE conversion result.</summary>
    public static OpcCpxConversionResult BadType() => new(null, OpcResultId.BadType.Code);

    /// <summary>Creates an OPC_E_RANGE conversion result.</summary>
    public static OpcCpxConversionResult Range() => new(null, OpcResultId.Range.Code);

    internal static OpcCpxConversionResult FromError(int error) => new(null, error);
}
