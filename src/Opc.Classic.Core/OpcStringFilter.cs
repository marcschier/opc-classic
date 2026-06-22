// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Matches OPC Common string-filter patterns using the VB LIKE-style wildcards
/// defined by the OPC Common specification.
/// </summary>
public static class OpcStringFilter
{
    /// <summary>
    /// Returns <see langword="true" /> when <paramref name="value" /> matches <paramref name="pattern" />.
    /// </summary>
    /// <remarks>
    /// Supported pattern tokens are <c>*</c> for any character sequence, <c>?</c> for any single
    /// character, <c>#</c> for an ASCII digit, and character lists such as <c>[ABC]</c>,
    /// <c>[A-Z]</c>, and <c>[!0-9]</c>.
    /// </remarks>
    /// <param name="value">The candidate string to test.</param>
    /// <param name="pattern">The OPC string-filter pattern.</param>
    /// <param name="caseSensitive">Whether literal and character-list comparisons are case-sensitive.</param>
    public static bool MatchPattern(string value, string pattern, bool caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(pattern);

        // Jagged memoization table: memo[valueIndex][patternIndex].
        // Inner arrays are sized to pattern.Length + 1; default null = unknown.
        var memo = new bool?[value.Length + 1][];
        for (int i = 0; i < memo.Length; i++)
        {
            memo[i] = new bool?[pattern.Length + 1];
        }
        return MatchCore(value, 0, pattern, 0, caseSensitive, memo);
    }

    private static bool MatchCore(
        string value,
        int valueIndex,
        string pattern,
        int patternIndex,
        bool caseSensitive,
        bool?[][] memo)
    {
        int memoPatternIndex = patternIndex;
        if (memo[valueIndex][memoPatternIndex].HasValue)
        {
            return memo[valueIndex][memoPatternIndex]!.Value;
        }

        bool result;
        if (patternIndex == pattern.Length)
        {
            result = valueIndex == value.Length;
        }
        else if (pattern[patternIndex] == '*')
        {
            while (patternIndex + 1 < pattern.Length && pattern[patternIndex + 1] == '*')
            {
                patternIndex++;
            }

            result = MatchCore(value, valueIndex, pattern, patternIndex + 1, caseSensitive, memo)
                || (valueIndex < value.Length && MatchCore(value, valueIndex + 1, pattern, patternIndex, caseSensitive, memo));
        }
        else if (valueIndex == value.Length)
        {
            result = false;
        }
        else
        {
            result = MatchSingle(value[valueIndex], pattern, patternIndex, caseSensitive, out int nextPatternIndex)
                && MatchCore(value, valueIndex + 1, pattern, nextPatternIndex, caseSensitive, memo);
        }

        memo[valueIndex][memoPatternIndex] = result;
        return result;
    }

    private static bool MatchSingle(char value, string pattern, int patternIndex, bool caseSensitive, out int nextPatternIndex)
    {
        char token = pattern[patternIndex];
        nextPatternIndex = patternIndex + 1;

        return token switch
        {
            '?' => true,
            '#' => value is >= '0' and <= '9',
            '[' => TryMatchCharacterList(value, pattern, patternIndex, caseSensitive, out nextPatternIndex),
            _ => Equals(value, token, caseSensitive),
        };
    }

    private static bool TryMatchCharacterList(
        char value,
        string pattern,
        int patternIndex,
        bool caseSensitive,
        out int nextPatternIndex)
    {
        nextPatternIndex = patternIndex + 1;
        int listStart = patternIndex + 1;
        if (listStart >= pattern.Length)
        {
            return false;
        }

        bool negate = pattern[listStart] == '!';
        if (negate)
        {
            listStart++;
        }

        int listEnd = pattern.IndexOf(']', listStart);
        if (listEnd <= listStart)
        {
            return false;
        }

        bool matched = false;
        for (int i = listStart; i < listEnd; i++)
        {
            if (i + 2 < listEnd && pattern[i + 1] == '-')
            {
                matched |= InRange(value, pattern[i], pattern[i + 2], caseSensitive);
                i += 2;
            }
            else
            {
                matched |= Equals(value, pattern[i], caseSensitive);
            }
        }

        nextPatternIndex = listEnd + 1;
        return negate ? !matched : matched;
    }

    private static bool InRange(char value, char first, char last, bool caseSensitive)
    {
        char candidate = Normalize(value, caseSensitive);
        char lower = Normalize(first, caseSensitive);
        char upper = Normalize(last, caseSensitive);
        if (lower > upper)
        {
            (lower, upper) = (upper, lower);
        }

        return candidate >= lower && candidate <= upper;
    }

    private static bool Equals(char left, char right, bool caseSensitive) =>
        Normalize(left, caseSensitive) == Normalize(right, caseSensitive);

    private static char Normalize(char value, bool caseSensitive) =>
        caseSensitive ? value : char.ToUpperInvariant(value);
}
