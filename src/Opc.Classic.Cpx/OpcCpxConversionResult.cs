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
    /// Creates the CPX §9 result used for unsupported conversions, malformed
    /// runtime values, configured bound violations, and checked range failures.
    /// </summary>
    public static OpcCpxConversionResult TypeChanged() => new(null, OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);
}
