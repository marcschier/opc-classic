// Copyright (c) 2026 marcschier. Licensed under the MIT License.

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
    /// Creates an unsupported/stale type conversion result per CPX §9.
    /// </summary>
    public static OpcCpxConversionResult TypeChanged() => new(null, OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);
}
