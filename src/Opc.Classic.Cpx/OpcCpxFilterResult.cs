// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// Result of a CPX data-filter evaluation.
/// </summary>
public readonly record struct OpcCpxFilterResult(ComplexValue? Value, int Error)
{
    /// <summary>
    /// Creates a successful filter result with matching data.
    /// </summary>
    public static OpcCpxFilterResult Success(ComplexValue value) => new(value, OpcResultId.Ok.Code);

    /// <summary>
    /// Creates an invalid filter-syntax result.
    /// </summary>
    public static OpcCpxFilterResult Invalid() => new(null, OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);

    /// <summary>
    /// Creates a runtime filter evaluation error.
    /// </summary>
    public static OpcCpxFilterResult ErrorResult() => new(null, OpcComplexDataResult.OPCCPX_E_FILTER_ERROR);

    /// <summary>
    /// Creates a successful no-data result when a filter excludes all fields.
    /// </summary>
    public static OpcCpxFilterResult NoData(ComplexValue value) => new(value, OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
}
