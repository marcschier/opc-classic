//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Xml;

/// <summary>
/// A single XML-DA value, paired with its xsi:type discriminator and the
/// original wire text for debugging / round-tripping. Strongly-typed
/// accessors return <see langword="null"/> when the requested .NET type
/// doesn't match the carrier <see cref="Type"/>.
/// </summary>
public sealed record XmlDaValue
{
    /// <summary>Creates an unknown-type carrier preserving the raw text only.</summary>
    public static XmlDaValue Unknown(string rawText) =>
        new() { Type = XmlDaValueType.Unknown, RawText = rawText, Boxed = null };

    /// <summary>Creates a string value.</summary>
    public static XmlDaValue OfString(string text) =>
        new() { Type = XmlDaValueType.String, RawText = text, Boxed = text };

    /// <summary>Creates a 32-bit signed integer value.</summary>
    public static XmlDaValue OfInt32(int value) =>
        new() { Type = XmlDaValueType.Int32, RawText = value.ToString(System.Globalization.CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 64-bit double-precision floating-point value.</summary>
    public static XmlDaValue OfDouble(double value) =>
        new() { Type = XmlDaValueType.Double, RawText = value.ToString("R", System.Globalization.CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a boolean value.</summary>
    public static XmlDaValue OfBoolean(bool value) =>
        new() { Type = XmlDaValueType.Boolean, RawText = value ? "true" : "false", Boxed = value };

    /// <summary>Creates a UTC date-time value.</summary>
    public static XmlDaValue OfDateTime(DateTimeOffset value) =>
        new() { Type = XmlDaValueType.DateTime, RawText = value.UtcDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>The xsi:type discriminator that defined this value.</summary>
    public required XmlDaValueType Type { get; init; }

    /// <summary>The verbatim wire text inside the <c>&lt;Value&gt;</c> element.</summary>
    public required string RawText { get; init; }

    /// <summary>The .NET-typed value (boxed). Null for <see cref="XmlDaValueType.Unknown"/>.</summary>
    public required object? Boxed { get; init; }

    /// <summary>Returns the string content if <see cref="Type"/> is <see cref="XmlDaValueType.String"/>, else null.</summary>
    public string? AsString() => Type == XmlDaValueType.String ? (string?)Boxed : null;

    /// <summary>Returns the int content if <see cref="Type"/> is <see cref="XmlDaValueType.Int32"/>, else null.</summary>
    public int? AsInt32() => Type == XmlDaValueType.Int32 ? (int?)Boxed : null;

    /// <summary>Returns the double content if <see cref="Type"/> is <see cref="XmlDaValueType.Double"/>, else null.</summary>
    public double? AsDouble() => Type == XmlDaValueType.Double ? (double?)Boxed : null;

    /// <summary>Returns the bool content if <see cref="Type"/> is <see cref="XmlDaValueType.Boolean"/>, else null.</summary>
    public bool? AsBoolean() => Type == XmlDaValueType.Boolean ? (bool?)Boxed : null;

    /// <summary>Returns the dateTime content if <see cref="Type"/> is <see cref="XmlDaValueType.DateTime"/>, else null.</summary>
    public DateTimeOffset? AsDateTime() => Type == XmlDaValueType.DateTime ? (DateTimeOffset?)Boxed : null;
}
