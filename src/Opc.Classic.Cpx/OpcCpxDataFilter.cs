// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// OPC Complex Data §8 data-filter helper for bounded server-side filtering.
/// </summary>
public static class OpcCpxDataFilter
{
    private static readonly IOpcCpxDataFilter s_referenceFilter = new OpcCpxReferenceDataFilter();

    /// <summary>
    /// Validates uniqueness and syntax for a named data filter.
    /// </summary>
    public static int ValidateNewFilter(string name, string expression, IEnumerable<string> existingNames) =>
        s_referenceFilter.ValidateNewFilter(name, expression, existingNames);

    /// <summary>
    /// Applies a bounded reference filter expression such as
    /// <c>Status == Good AND Count &gt; 0</c>.
    /// </summary>
    public static OpcCpxFilterResult Apply(ComplexValue value, TypeDescription type, string expression) =>
        s_referenceFilter.Apply(value, type, expression);

    /// <summary>Applies a bounded filter using declared nested dictionary types.</summary>
    public static OpcCpxFilterResult Apply(
        ComplexValue value,
        TypeDescription type,
        TypeDictionary dictionary,
        string expression) =>
        s_referenceFilter.Apply(value, type, dictionary, expression);
}
