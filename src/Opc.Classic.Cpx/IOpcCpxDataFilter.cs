// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>Validates and evaluates bounded OPC Complex Data filter expressions.</summary>
public interface IOpcCpxDataFilter
{
    /// <summary>Validates uniqueness and syntax for a named data filter.</summary>
    int ValidateNewFilter(string name, string expression, IEnumerable<string> existingNames);

    /// <summary>Applies a data filter to a decoded complex value.</summary>
    OpcCpxFilterResult Apply(ComplexValue value, TypeDescription type, string expression);
}
