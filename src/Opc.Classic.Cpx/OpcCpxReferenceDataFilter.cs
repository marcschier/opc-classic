// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Text;

namespace Opc.Classic.Cpx;

/// <summary>
/// Bounded, AOT-safe reference evaluator for a deliberately small subset of
/// OPC Complex Data filter expressions.
/// </summary>
/// <remarks>
/// This type is intended for conformance testing and simple server policies.
/// It is not a general-purpose expression engine and intentionally rejects
/// functions, arithmetic, unary operators, and vendor-specific syntax.
/// </remarks>
public sealed class OpcCpxReferenceDataFilter : IOpcCpxDataFilter
{
    /// <summary>Maximum accepted expression length, in UTF-16 code units.</summary>
    public const int MaxExpressionLength = 4_096;

    /// <summary>Maximum parenthesis nesting depth.</summary>
    public const int MaxNestingDepth = 32;

    /// <summary>Maximum number of comparisons in one expression.</summary>
    public const int MaxComparisons = 128;

    /// <summary>Maximum number of segments in one field path.</summary>
    public const int MaxPathSegments = 32;

    /// <summary>Maximum decoded literal length.</summary>
    public const int MaxLiteralLength = 1_024;

    /// <inheritdoc />
    public int ValidateNewFilter(string name, string expression, IEnumerable<string> existingNames)
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

        return TryParse(expression, out _)
            ? OpcResultId.Ok.Code
            : OpcComplexDataResult.OPCCPX_E_FILTER_INVALID;
    }

    /// <inheritdoc />
    public OpcCpxFilterResult Apply(ComplexValue value, TypeDescription type, string expression)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(type);

        if (!TryParse(expression, out var root))
        {
            return OpcCpxFilterResult.Invalid();
        }

        var result = root.Evaluate(new EvaluationContext(value, type));
        return result switch
        {
            Evaluation.True => OpcCpxFilterResult.Success(value),
            Evaluation.False => OpcCpxFilterResult.NoData(CreateEmptyValue(type)),
            _ => OpcCpxFilterResult.ErrorResult(),
        };
    }

    private static bool TryParse(string? expression, out Node root)
    {
        root = null!;
        if (string.IsNullOrWhiteSpace(expression) || expression.Length > MaxExpressionLength)
        {
            return false;
        }

        var parser = new Parser(expression);
        return parser.TryParse(out root);
    }

    private static ComplexValue CreateEmptyValue(TypeDescription type) => new()
    {
        Type = new StructType
        {
            Name = type.Name,
            Fields = type.Fields.Select(static field => new StructField
            {
                Name = field.Name,
                Kind = field.Kind,
                TypeReference = field.TypeId,
                Repeats = field.ElementCount ?? 0,
            }).ToArray(),
        },
        Fields = new Dictionary<string, object?>(StringComparer.Ordinal),
    };

    private static bool ContainsTopLevelField(TypeDescription type, string fieldName)
    {
        foreach (var field in type.Fields)
        {
            if (StringComparer.Ordinal.Equals(field.Name, fieldName))
            {
                return true;
            }
        }

        return false;
    }

    private static Evaluation Compare(object? actual, ComparisonOperator comparisonOperator, Literal literal)
    {
        if (actual is null)
        {
            if (literal.IsQuoted || !literal.Value.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return comparisonOperator switch
                {
                    ComparisonOperator.Equal => Evaluation.False,
                    ComparisonOperator.NotEqual => Evaluation.True,
                    _ => Evaluation.Error,
                };
            }

            return comparisonOperator switch
            {
                ComparisonOperator.Equal => Evaluation.True,
                ComparisonOperator.NotEqual => Evaluation.False,
                _ => Evaluation.Error,
            };
        }

        return actual switch
        {
            bool typed => CompareBoolean(typed, comparisonOperator, literal.Value),
            sbyte typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            byte typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            short typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            ushort typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            int typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            uint typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            long typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            ulong typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Integer),
            decimal typed => CompareDecimal(typed, comparisonOperator, literal.Value, NumberStyles.Float),
            float typed => CompareSingle(typed, comparisonOperator, literal.Value),
            double typed => CompareDouble(typed, comparisonOperator, literal.Value),
            string typed => ApplyComparison(
                string.Compare(typed, literal.Value, StringComparison.Ordinal),
                comparisonOperator),
            DateTime typed => CompareDateTime(typed, comparisonOperator, literal.Value),
            DateTimeOffset typed => CompareDateTimeOffset(typed, comparisonOperator, literal.Value),
            Guid typed => CompareGuid(typed, comparisonOperator, literal.Value),
            _ => Evaluation.Error,
        };
    }

    private static Evaluation CompareBoolean(bool actual, ComparisonOperator comparisonOperator, string literal)
    {
        if (!bool.TryParse(literal, out var expected))
        {
            return Evaluation.Error;
        }

        return comparisonOperator switch
        {
            ComparisonOperator.Equal => actual == expected ? Evaluation.True : Evaluation.False,
            ComparisonOperator.NotEqual => actual != expected ? Evaluation.True : Evaluation.False,
            _ => Evaluation.Error,
        };
    }

    private static Evaluation CompareDecimal(
        decimal actual,
        ComparisonOperator comparisonOperator,
        string literal,
        NumberStyles styles)
    {
        if (!decimal.TryParse(literal, styles, CultureInfo.InvariantCulture, out var expected))
        {
            return Evaluation.Error;
        }

        return ApplyComparison(actual.CompareTo(expected), comparisonOperator);
    }

    private static Evaluation CompareSingle(float actual, ComparisonOperator comparisonOperator, string literal)
    {
        if (!float.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out var expected))
        {
            return Evaluation.Error;
        }

        if (comparisonOperator == ComparisonOperator.Equal)
        {
            return actual.Equals(expected) ? Evaluation.True : Evaluation.False;
        }

        if (comparisonOperator == ComparisonOperator.NotEqual)
        {
            return !actual.Equals(expected) ? Evaluation.True : Evaluation.False;
        }

        return ApplyComparison(actual.CompareTo(expected), comparisonOperator);
    }

    private static Evaluation CompareDouble(double actual, ComparisonOperator comparisonOperator, string literal)
    {
        if (!double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out var expected))
        {
            return Evaluation.Error;
        }

        if (comparisonOperator == ComparisonOperator.Equal)
        {
            return actual.Equals(expected) ? Evaluation.True : Evaluation.False;
        }

        if (comparisonOperator == ComparisonOperator.NotEqual)
        {
            return !actual.Equals(expected) ? Evaluation.True : Evaluation.False;
        }

        return ApplyComparison(actual.CompareTo(expected), comparisonOperator);
    }

    private static Evaluation CompareDateTime(
        DateTime actual,
        ComparisonOperator comparisonOperator,
        string literal)
    {
        if (!DateTime.TryParse(
            literal,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
            out var expected))
        {
            return Evaluation.Error;
        }

        return ApplyComparison(actual.CompareTo(expected), comparisonOperator);
    }

    private static Evaluation CompareDateTimeOffset(
        DateTimeOffset actual,
        ComparisonOperator comparisonOperator,
        string literal)
    {
        if (!DateTimeOffset.TryParse(
            literal,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
            out var expected))
        {
            return Evaluation.Error;
        }

        return ApplyComparison(actual.CompareTo(expected), comparisonOperator);
    }

    private static Evaluation CompareGuid(Guid actual, ComparisonOperator comparisonOperator, string literal)
    {
        if (!Guid.TryParse(literal, out var expected))
        {
            return Evaluation.Error;
        }

        return comparisonOperator switch
        {
            ComparisonOperator.Equal => actual == expected ? Evaluation.True : Evaluation.False,
            ComparisonOperator.NotEqual => actual != expected ? Evaluation.True : Evaluation.False,
            _ => Evaluation.Error,
        };
    }

    private static Evaluation ApplyComparison(int comparison, ComparisonOperator comparisonOperator)
    {
        var matches = comparisonOperator switch
        {
            ComparisonOperator.Equal => comparison == 0,
            ComparisonOperator.NotEqual => comparison != 0,
            ComparisonOperator.LessThan => comparison < 0,
            ComparisonOperator.LessThanOrEqual => comparison <= 0,
            ComparisonOperator.GreaterThan => comparison > 0,
            ComparisonOperator.GreaterThanOrEqual => comparison >= 0,
            _ => false,
        };

        return matches ? Evaluation.True : Evaluation.False;
    }

    private enum Evaluation
    {
        False,
        True,
        Error,
    }

    private enum ComparisonOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }

    private enum BooleanOperator
    {
        And,
        Or,
    }

    private sealed record Literal(string Value, bool IsQuoted);

    private abstract class Node
    {
        public abstract Evaluation Evaluate(EvaluationContext context);
    }

    private sealed class ComparisonNode(
        string[] path,
        ComparisonOperator comparisonOperator,
        Literal literal) : Node
    {
        public override Evaluation Evaluate(EvaluationContext context)
        {
            if (!context.TryResolve(path, out var actual))
            {
                return Evaluation.Error;
            }

            return Compare(actual, comparisonOperator, literal);
        }
    }

    private sealed class BooleanNode(Node left, BooleanOperator booleanOperator, Node right) : Node
    {
        public override Evaluation Evaluate(EvaluationContext context)
        {
            var leftResult = left.Evaluate(context);
            if (leftResult == Evaluation.Error)
            {
                return Evaluation.Error;
            }

            if (booleanOperator == BooleanOperator.And && leftResult == Evaluation.False)
            {
                return Evaluation.False;
            }

            if (booleanOperator == BooleanOperator.Or && leftResult == Evaluation.True)
            {
                return Evaluation.True;
            }

            return right.Evaluate(context);
        }
    }

    private sealed class EvaluationContext(ComplexValue value, TypeDescription type)
    {
        public bool TryResolve(string[] path, out object? result)
        {
            result = null;
            if (path.Length == 0 || !ContainsTopLevelField(type, path[0]))
            {
                return false;
            }

            object? current = value;
            foreach (var segment in path)
            {
                if (current is not ComplexValue complex
                    || !complex.Fields.TryGetValue(segment, out current))
                {
                    return false;
                }
            }

            result = current;
            return true;
        }
    }

    private sealed class Parser(string expression)
    {
        private int _index;
        private int _comparisonCount;
        private int _parenthesisDepth;

        public bool TryParse(out Node root)
        {
            root = null!;
            SkipWhitespace();
            if (!TryParseOr(out root))
            {
                return false;
            }

            SkipWhitespace();
            return _index == expression.Length;
        }

        private bool TryParseOr(out Node node)
        {
            if (!TryParseAnd(out node))
            {
                return false;
            }

            while (true)
            {
                if (TryMatchKeyword("OR") || TryMatchSymbol("||"))
                {
                    if (!TryParseAnd(out var right))
                    {
                        return false;
                    }

                    node = new BooleanNode(node, BooleanOperator.Or, right);
                    continue;
                }

                return true;
            }
        }

        private bool TryParseAnd(out Node node)
        {
            if (!TryParsePrimary(out node))
            {
                return false;
            }

            while (true)
            {
                if (TryMatchKeyword("AND") || TryMatchSymbol("&&"))
                {
                    if (!TryParsePrimary(out var right))
                    {
                        return false;
                    }

                    node = new BooleanNode(node, BooleanOperator.And, right);
                    continue;
                }

                return true;
            }
        }

        private bool TryParsePrimary(out Node node)
        {
            node = null!;
            SkipWhitespace();

            if (TryMatchCharacter('('))
            {
                if (_parenthesisDepth >= MaxNestingDepth)
                {
                    return false;
                }

                _parenthesisDepth++;
                if (!TryParseOr(out node))
                {
                    return false;
                }

                if (!TryMatchCharacter(')'))
                {
                    return false;
                }

                _parenthesisDepth--;
                return true;
            }

            return TryParseComparison(out node);
        }

        private bool TryParseComparison(out Node node)
        {
            node = null!;
            if (_comparisonCount >= MaxComparisons
                || !TryParsePath(out var path)
                || !TryParseComparisonOperator(out var comparisonOperator)
                || !TryParseLiteral(out var literal))
            {
                return false;
            }

            _comparisonCount++;
            node = new ComparisonNode(path, comparisonOperator, literal);
            return true;
        }

        private bool TryParsePath(out string[] path)
        {
            path = [];
            var segments = new List<string>();
            if (!TryParsePathSegment(out var first))
            {
                return false;
            }

            segments.Add(first);
            while (true)
            {
                var savedIndex = _index;
                SkipWhitespace();
                if (!TryMatchCharacter('.'))
                {
                    _index = savedIndex;
                    break;
                }

                if (segments.Count >= MaxPathSegments || !TryParsePathSegment(out var segment))
                {
                    return false;
                }

                segments.Add(segment);
            }

            path = segments.ToArray();
            return true;
        }

        private bool TryParsePathSegment(out string segment)
        {
            segment = string.Empty;
            SkipWhitespace();
            if (_index >= expression.Length)
            {
                return false;
            }

            if (expression[_index] == '[')
            {
                return TryParseBracketedPathSegment(out segment);
            }

            var start = _index;
            if (!IsIdentifierStart(expression[_index]))
            {
                return false;
            }

            _index++;
            while (_index < expression.Length && IsIdentifierPart(expression[_index]))
            {
                _index++;
            }

            segment = expression[start.._index];
            return true;
        }

        private bool TryParseBracketedPathSegment(out string segment)
        {
            segment = string.Empty;
            _index++;
            var builder = new StringBuilder();

            while (_index < expression.Length)
            {
                var character = expression[_index++];
                if (character != ']')
                {
                    if (char.IsControl(character))
                    {
                        return false;
                    }

                    builder.Append(character);
                    continue;
                }

                if (_index < expression.Length && expression[_index] == ']')
                {
                    _index++;
                    builder.Append(']');
                    continue;
                }

                segment = builder.ToString().Trim();
                return segment.Length != 0;
            }

            return false;
        }

        private bool TryParseComparisonOperator(out ComparisonOperator comparisonOperator)
        {
            SkipWhitespace();
            if (TryMatchRaw("<="))
            {
                comparisonOperator = ComparisonOperator.LessThanOrEqual;
                return true;
            }

            if (TryMatchRaw(">="))
            {
                comparisonOperator = ComparisonOperator.GreaterThanOrEqual;
                return true;
            }

            if (TryMatchRaw("=="))
            {
                comparisonOperator = ComparisonOperator.Equal;
                return true;
            }

            if (TryMatchRaw("!=") || TryMatchRaw("<>"))
            {
                comparisonOperator = ComparisonOperator.NotEqual;
                return true;
            }

            if (TryMatchRaw("="))
            {
                comparisonOperator = ComparisonOperator.Equal;
                return true;
            }

            if (TryMatchRaw("<"))
            {
                comparisonOperator = ComparisonOperator.LessThan;
                return true;
            }

            if (TryMatchRaw(">"))
            {
                comparisonOperator = ComparisonOperator.GreaterThan;
                return true;
            }

            comparisonOperator = default;
            return false;
        }

        private bool TryParseLiteral(out Literal literal)
        {
            literal = null!;
            SkipWhitespace();
            if (_index >= expression.Length)
            {
                return false;
            }

            var quote = expression[_index];
            if (quote is '\'' or '"')
            {
                return TryParseQuotedLiteral(quote, out literal);
            }

            var start = _index;
            while (_index < expression.Length)
            {
                var character = expression[_index];
                if (char.IsWhiteSpace(character)
                    || character is '(' or ')' or '&' or '|' or '=' or '!' or '<' or '>'
                    || character is '[' or ']' or '\'' or '"' or ',' or ';'
                    || !IsBareLiteralPart(character))
                {
                    break;
                }

                if (char.IsControl(character))
                {
                    return false;
                }

                _index++;
            }

            var length = _index - start;
            if (length is <= 0 or > MaxLiteralLength)
            {
                return false;
            }

            literal = new Literal(expression[start.._index], false);
            return true;
        }

        private bool TryParseQuotedLiteral(char quote, out Literal literal)
        {
            literal = null!;
            _index++;
            var builder = new StringBuilder();

            while (_index < expression.Length)
            {
                var character = expression[_index++];
                if (character != quote)
                {
                    if (char.IsControl(character) && !char.IsWhiteSpace(character))
                    {
                        return false;
                    }

                    builder.Append(character);
                    if (builder.Length > MaxLiteralLength)
                    {
                        return false;
                    }

                    continue;
                }

                if (_index < expression.Length && expression[_index] == quote)
                {
                    _index++;
                    builder.Append(quote);
                    if (builder.Length > MaxLiteralLength)
                    {
                        return false;
                    }

                    continue;
                }

                literal = new Literal(builder.ToString(), true);
                return true;
            }

            return false;
        }

        private bool TryMatchKeyword(string keyword)
        {
            var savedIndex = _index;
            SkipWhitespace();
            if (_index + keyword.Length > expression.Length
                || !expression.AsSpan(_index, keyword.Length).Equals(keyword, StringComparison.OrdinalIgnoreCase))
            {
                _index = savedIndex;
                return false;
            }

            var end = _index + keyword.Length;
            if (end < expression.Length && IsIdentifierPart(expression[end]))
            {
                _index = savedIndex;
                return false;
            }

            _index = end;
            return true;
        }

        private bool TryMatchSymbol(string symbol)
        {
            var savedIndex = _index;
            SkipWhitespace();
            if (!TryMatchRaw(symbol))
            {
                _index = savedIndex;
                return false;
            }

            return true;
        }

        private bool TryMatchCharacter(char character)
        {
            SkipWhitespace();
            if (_index >= expression.Length || expression[_index] != character)
            {
                return false;
            }

            _index++;
            return true;
        }

        private bool TryMatchRaw(string text)
        {
            if (_index + text.Length > expression.Length
                || !expression.AsSpan(_index, text.Length).SequenceEqual(text))
            {
                return false;
            }

            _index += text.Length;
            return true;
        }

        private void SkipWhitespace()
        {
            while (_index < expression.Length && char.IsWhiteSpace(expression[_index]))
            {
                _index++;
            }
        }

        private static bool IsIdentifierStart(char character) =>
            char.IsLetter(character) || character == '_';

        private static bool IsIdentifierPart(char character) =>
            char.IsLetterOrDigit(character) || character is '_' or '-';

        private static bool IsBareLiteralPart(char character) =>
            char.IsLetterOrDigit(character) || character is '_' or '-' or '+' or '.' or ':';
    }
}
