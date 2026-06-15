//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;

namespace Opc.Classic.Cpx;

/// <summary>
/// Minimal OPC Complex Data §8 data-filter evaluator for FieldId equality filters.
/// </summary>
public static class OpcCpxDataFilter
{
    /// <summary>
    /// Validates uniqueness and syntax for a named data filter.
    /// </summary>
    public static int ValidateNewFilter(string name, string expression, IEnumerable<string> existingNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(existingNames);

        foreach (var existing in existingNames)
        {
            if (StringComparer.Ordinal.Equals(existing, name))
            {
                return OpcComplexDataResult.OPCCPX_E_FILTER_DUPLICATE;
            }
        }

        return TryParse(expression, out _) ? OpcResultId.Ok.Code : OpcComplexDataResult.OPCCPX_E_FILTER_INVALID;
    }

    /// <summary>
    /// Applies a single FieldId equality expression such as <c>Status == Good</c>.
    /// </summary>
    public static OpcCpxFilterResult Apply(ComplexValue value, TypeDescription type, string expression)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(type);

        if (!TryParse(expression, out var parsed))
        {
            return OpcCpxFilterResult.Invalid();
        }

        if (!ContainsField(type, parsed.FieldId) || !value.Fields.TryGetValue(parsed.FieldId, out var actual))
        {
            return OpcCpxFilterResult.ErrorResult();
        }

        if (!EqualsLiteral(actual, parsed.Literal))
        {
            return OpcCpxFilterResult.NoData(new ComplexValue { Type = ToStructType(type), Fields = new Dictionary<string, object?>(StringComparer.Ordinal) });
        }

        return OpcCpxFilterResult.Success(value);
    }

    private static bool TryParse(string expression, out ParsedFilter parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var opIndex = expression.IndexOf("==", StringComparison.Ordinal);
        var opLength = 2;
        if (opIndex < 0)
        {
            opIndex = expression.IndexOf('=', StringComparison.Ordinal);
            opLength = 1;
        }

        if (opIndex <= 0 || opIndex + opLength >= expression.Length)
        {
            return false;
        }

        var fieldId = expression[..opIndex].Trim();
        var literal = expression[(opIndex + opLength)..].Trim();
        if (fieldId.Length == 0 || literal.Length == 0 || fieldId.Contains(' ', StringComparison.Ordinal))
        {
            return false;
        }

        parsed = new ParsedFilter(fieldId.Trim('[', ']'), TrimQuotes(literal));
        return parsed.FieldId.Length != 0;
    }

    private static bool EqualsLiteral(object? actual, string literal) => actual switch
    {
        null => literal.Equals("null", StringComparison.OrdinalIgnoreCase),
        bool typed => bool.TryParse(literal, out var expected) && typed == expected,
        sbyte typed => long.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        byte typed => ulong.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        short typed => long.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        ushort typed => ulong.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        int typed => long.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        uint typed => ulong.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        long typed => long.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        ulong typed => ulong.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected) && typed == expected,
        float typed => float.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out var expected) && typed.Equals(expected),
        double typed => double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out var expected) && typed.Equals(expected),
        string typed => typed.Equals(literal, StringComparison.Ordinal),
        _ => actual.ToString()?.Equals(literal, StringComparison.Ordinal) == true,
    };

    private static bool ContainsField(TypeDescription type, string fieldId)
    {
        foreach (var field in type.Fields)
        {
            if (StringComparer.Ordinal.Equals(field.Name, fieldId))
            {
                return true;
            }
        }

        return false;
    }

    private static string TrimQuotes(string value) =>
        value.Length >= 2 && ((value[0] == '\'' && value[^1] == '\'') || (value[0] == '"' && value[^1] == '"'))
            ? value[1..^1]
            : value;

    private static StructType ToStructType(TypeDescription type) => new()
    {
        Name = type.Name,
        Fields = type.Fields.Select(static field => new StructField { Name = field.Name, Kind = field.Kind }).ToArray(),
    };

    private readonly record struct ParsedFilter(string FieldId, string Literal);
}
