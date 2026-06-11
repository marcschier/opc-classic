//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Globalization;

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA time specifier — either an absolute UTC instant or a relative
/// "NOW±duration" expression. Mirrors the on-the-wire <c>OPCHDA_TIME</c>
/// (bool IsRelative + string + DateTime).
/// </summary>
/// <remarks>
/// Relative times use the canonical HDA grammar:
/// <c>NOW [+|- &lt;number&gt;&lt;unit&gt;]...</c> where unit is one of
/// <c>S M H D W MO Y</c>. Example: <c>"NOW-1H"</c>, <c>"NOW-7D+12H"</c>.
/// </remarks>
public readonly struct HdaTime : IEquatable<HdaTime>
{
    private readonly DateTimeOffset _absolute;
    private readonly string? _relative;

    private HdaTime(DateTimeOffset absolute)
    {
        _absolute = absolute;
        _relative = null;
    }

    private HdaTime(string relative)
    {
        _absolute = default;
        _relative = relative;
    }

    /// <summary>True if this is a relative-time expression (e.g. <c>"NOW-1H"</c>).</summary>
    public bool IsRelative => _relative is not null;

    /// <summary>
    /// The absolute UTC timestamp this represents. For relative times, this is
    /// the result of evaluating the expression at <paramref name="evaluationTime"/>.
    /// </summary>
    public DateTimeOffset ResolveAt(DateTimeOffset evaluationTime)
    {
        if (_relative is null)
        {
            return _absolute;
        }
        return ResolveRelative(_relative, evaluationTime);
    }

    /// <summary>The relative-expression string, or <see langword="null"/> if absolute.</summary>
    public string? Expression => _relative;

    /// <summary>Create an absolute HDA time.</summary>
    public static HdaTime Absolute(DateTimeOffset utc) => new(utc.ToUniversalTime());

    /// <summary>Create a relative HDA time from an expression like <c>"NOW-1H"</c>.</summary>
    public static HdaTime Relative(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var trimmed = expression.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Relative expression cannot be empty.", nameof(expression));
        }
        // Validate by parsing — throws FormatException on bad input.
        _ = ResolveRelative(trimmed, DateTimeOffset.UtcNow);
        return new HdaTime(trimmed);
    }

    /// <summary>Convenience: <c>HdaTime.Now</c> = the relative expression <c>"NOW"</c>.</summary>
    public static HdaTime Now { get; } = Relative("NOW");

    /// <inheritdoc />
    public bool Equals(HdaTime other) =>
        string.Equals(_relative, other._relative, StringComparison.Ordinal)
        && _absolute == other._absolute;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is HdaTime t && Equals(t);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_relative, _absolute);

    /// <summary>Value-equality operator.</summary>
    public static bool operator ==(HdaTime left, HdaTime right) => left.Equals(right);

    /// <summary>Value-inequality operator.</summary>
    public static bool operator !=(HdaTime left, HdaTime right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() =>
        _relative ?? _absolute.ToString("o", CultureInfo.InvariantCulture);

    private static DateTimeOffset ResolveRelative(string expression, DateTimeOffset now)
    {
        var s = expression.AsSpan().Trim();
        const string NowPrefix = "NOW";
        if (!s.StartsWith(NowPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException(
                $"HDA relative time must start with 'NOW'. Got '{expression}'.");
        }
        s = s[NowPrefix.Length..].Trim();
        var current = now;
        while (!s.IsEmpty)
        {
            var sign = s[0] switch
            {
                '+' => +1,
                '-' => -1,
                _ => throw new FormatException(
                    $"Expected '+' or '-' after NOW. Got '{expression}'."),
            };
            s = s[1..];
            var digits = 0;
            while (digits < s.Length && char.IsDigit(s[digits]))
            {
                digits++;
            }
            if (digits == 0)
            {
                throw new FormatException($"Expected number in '{expression}'.");
            }
            var n = int.Parse(s[..digits], CultureInfo.InvariantCulture);
            s = s[digits..];
            var unitEnd = 0;
            while (unitEnd < s.Length && char.IsLetter(s[unitEnd]))
            {
                unitEnd++;
            }
            if (unitEnd == 0)
            {
                throw new FormatException($"Expected time unit in '{expression}'.");
            }
            var unit = s[..unitEnd].ToString().ToUpperInvariant();
            s = s[unitEnd..].Trim();
            current = unit switch
            {
                "S" => current.AddSeconds(sign * n),
                "M" => current.AddMinutes(sign * n),
                "H" => current.AddHours(sign * n),
                "D" => current.AddDays(sign * n),
                "W" => current.AddDays(sign * n * 7),
                "MO" => current.AddMonths(sign * n),
                "Y" => current.AddYears(sign * n),
                _ => throw new FormatException(
                    $"Unknown HDA time unit '{unit}' in '{expression}'."),
            };
        }
        return current;
    }
}
