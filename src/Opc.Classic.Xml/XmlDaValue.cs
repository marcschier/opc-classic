//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Opc.Classic.Xml;

/// <summary>
/// A single XML-DA value, paired with its xsi:type discriminator and the
/// original wire text for debugging / round-tripping. Strongly-typed
/// accessors return <see langword="null"/> when the requested .NET type
/// doesn't match the carrier <see cref="Type"/>.
/// </summary>
public sealed record XmlDaValue {
    /// <summary>Creates an unknown-type carrier preserving the raw text only.</summary>
    public static XmlDaValue Unknown(string rawText) =>
        new() { Type = XmlDaValueType.Unknown, RawText = rawText, Boxed = null };

    /// <summary>Creates a string value.</summary>
    public static XmlDaValue OfString(string text) =>
        new() { Type = XmlDaValueType.String, RawText = text, Boxed = text };

    /// <summary>Creates an 8-bit signed integer value.</summary>
    public static XmlDaValue OfInt8(sbyte value) =>
        new() { Type = XmlDaValueType.Int8, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates an 8-bit unsigned integer value.</summary>
    public static XmlDaValue OfUInt8(byte value) =>
        new() { Type = XmlDaValueType.UInt8, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 16-bit signed integer value.</summary>
    public static XmlDaValue OfInt16(short value) =>
        new() { Type = XmlDaValueType.Int16, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 16-bit unsigned integer value.</summary>
    public static XmlDaValue OfUInt16(ushort value) =>
        new() { Type = XmlDaValueType.UInt16, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 32-bit signed integer value.</summary>
    public static XmlDaValue OfInt32(int value) =>
        new() { Type = XmlDaValueType.Int32, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 32-bit unsigned integer value.</summary>
    public static XmlDaValue OfUInt32(uint value) =>
        new() { Type = XmlDaValueType.UInt32, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 64-bit signed integer value.</summary>
    public static XmlDaValue OfInt64(long value) =>
        new() { Type = XmlDaValueType.Int64, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 64-bit unsigned integer value.</summary>
    public static XmlDaValue OfUInt64(ulong value) =>
        new() { Type = XmlDaValueType.UInt64, RawText = value.ToString(CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a 32-bit single-precision floating-point value.</summary>
    public static XmlDaValue OfSingle(float value) =>
        new() { Type = XmlDaValueType.Single, RawText = XmlConvert.ToString(value), Boxed = value };

    /// <summary>Creates a 64-bit double-precision floating-point value.</summary>
    public static XmlDaValue OfDouble(double value) =>
        new() { Type = XmlDaValueType.Double, RawText = XmlConvert.ToString(value), Boxed = value };

    /// <summary>Creates a boolean value.</summary>
    public static XmlDaValue OfBoolean(bool value) =>
        new() { Type = XmlDaValueType.Boolean, RawText = value ? "true" : "false", Boxed = value };

    /// <summary>Creates a UTC date-time value.</summary>
    public static XmlDaValue OfDateTime(DateTimeOffset value) =>
        new() { Type = XmlDaValueType.DateTime, RawText = value.UtcDateTime.ToString("o", CultureInfo.InvariantCulture), Boxed = value };

    /// <summary>Creates a decimal value.</summary>
    public static XmlDaValue OfDecimal(decimal value) =>
        new() { Type = XmlDaValueType.Decimal, RawText = XmlConvert.ToString(value), Boxed = value };

    /// <summary>Creates an XML Schema time value.</summary>
    public static XmlDaValue OfTime(TimeOnly value) =>
        OfParsedTime(value.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture), value);

    /// <summary>Creates an XML Schema date value.</summary>
    public static XmlDaValue OfDate(DateOnly value) =>
        OfParsedDate(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), value);

    /// <summary>Creates an XML Schema duration value.</summary>
    public static XmlDaValue OfDuration(TimeSpan value) =>
        new() { Type = XmlDaValueType.Duration, RawText = XmlConvert.ToString(value), Boxed = value };

    /// <summary>Creates an XML Schema QName value from lexical text.</summary>
    public static XmlDaValue OfQName(string lexicalValue) {
        ArgumentNullException.ThrowIfNull(lexicalValue);
        return new() {
            Type = XmlDaValueType.QName,
            RawText = lexicalValue,
            Boxed = CreateQualifiedName(lexicalValue, string.Empty),
        };
    }

    /// <summary>Creates an XML Schema QName value.</summary>
    public static XmlDaValue OfQName(XmlQualifiedName value) {
        ArgumentNullException.ThrowIfNull(value);
        return new() { Type = XmlDaValueType.QName, RawText = value.Name, Boxed = value };
    }

    /// <summary>Creates an XML-DA ArrayOfByte value.</summary>
    public static XmlDaValue OfArrayOfByte(sbyte[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfByte, RawText = Join(copy), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfShort value.</summary>
    public static XmlDaValue OfArrayOfShort(short[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfShort, RawText = Join(copy), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfInt value.</summary>
    public static XmlDaValue OfArrayOfInt(int[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfInt, RawText = Join(copy), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfLong value.</summary>
    public static XmlDaValue OfArrayOfLong(long[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfLong, RawText = Join(copy), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfFloat value.</summary>
    public static XmlDaValue OfArrayOfFloat(float[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfFloat, RawText = Join(copy, XmlConvert.ToString), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfDouble value.</summary>
    public static XmlDaValue OfArrayOfDouble(double[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfDouble, RawText = Join(copy, XmlConvert.ToString), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfString value.</summary>
    public static XmlDaValue OfArrayOfString(string?[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfString, RawText = string.Join(" ", copy), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfBoolean value.</summary>
    public static XmlDaValue OfArrayOfBoolean(bool[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.ArrayOfBoolean, RawText = Join(copy, static v => v ? "true" : "false"), Boxed = copy };
    }

    /// <summary>Creates an XML-DA ArrayOfBoolean value.</summary>
    public static XmlDaValue OfArrayOfBool(bool[] values) => OfArrayOfBoolean(values);

    /// <summary>Creates an XML-DA ArrayOfDateTime value.</summary>
    public static XmlDaValue OfArrayOfDateTime(DateTimeOffset[] values) {
        var copy = CloneArray(values);
        return new() {
            Type = XmlDaValueType.ArrayOfDateTime,
            RawText = Join(copy, static v => v.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)),
            Boxed = copy,
        };
    }

    /// <summary>Creates an XML Schema base64Binary value.</summary>
    public static XmlDaValue OfBase64Binary(byte[] values) {
        var copy = CloneArray(values);
        return new() { Type = XmlDaValueType.Base64Binary, RawText = Convert.ToBase64String(copy), Boxed = copy };
    }

    /// <summary>The xsi:type discriminator that defined this value.</summary>
    public required XmlDaValueType Type { get; init; }

    /// <summary>The verbatim wire text inside the <c>&lt;Value&gt;</c> element.</summary>
    public required string RawText { get; init; }

    /// <summary>The .NET-typed value (boxed). Null for <see cref="XmlDaValueType.Unknown"/>.</summary>
    public required object? Boxed { get; init; }

    /// <summary>Returns the string content if <see cref="Type"/> is <see cref="XmlDaValueType.String"/>, else null.</summary>
    public string? AsString() => Type == XmlDaValueType.String ? (string?)Boxed : null;

    /// <summary>Returns the sbyte content if <see cref="Type"/> is <see cref="XmlDaValueType.Int8"/>, else null.</summary>
    public sbyte? AsInt8() => Type == XmlDaValueType.Int8 ? (sbyte?)Boxed : null;

    /// <summary>Returns the byte content if <see cref="Type"/> is <see cref="XmlDaValueType.UInt8"/>, else null.</summary>
    public byte? AsUInt8() => Type == XmlDaValueType.UInt8 ? (byte?)Boxed : null;

    /// <summary>Returns the short content if <see cref="Type"/> is <see cref="XmlDaValueType.Int16"/>, else null.</summary>
    public short? AsInt16() => Type == XmlDaValueType.Int16 ? (short?)Boxed : null;

    /// <summary>Returns the ushort content if <see cref="Type"/> is <see cref="XmlDaValueType.UInt16"/>, else null.</summary>
    public ushort? AsUInt16() => Type == XmlDaValueType.UInt16 ? (ushort?)Boxed : null;

    /// <summary>Returns the int content if <see cref="Type"/> is <see cref="XmlDaValueType.Int32"/>, else null.</summary>
    public int? AsInt32() => Type == XmlDaValueType.Int32 ? (int?)Boxed : null;

    /// <summary>Returns the uint content if <see cref="Type"/> is <see cref="XmlDaValueType.UInt32"/>, else null.</summary>
    public uint? AsUInt32() => Type == XmlDaValueType.UInt32 ? (uint?)Boxed : null;

    /// <summary>Returns the long content if <see cref="Type"/> is <see cref="XmlDaValueType.Int64"/>, else null.</summary>
    public long? AsInt64() => Type == XmlDaValueType.Int64 ? (long?)Boxed : null;

    /// <summary>Returns the ulong content if <see cref="Type"/> is <see cref="XmlDaValueType.UInt64"/>, else null.</summary>
    public ulong? AsUInt64() => Type == XmlDaValueType.UInt64 ? (ulong?)Boxed : null;

    /// <summary>Returns the float content if <see cref="Type"/> is <see cref="XmlDaValueType.Single"/>, else null.</summary>
    public float? AsSingle() => Type == XmlDaValueType.Single ? (float?)Boxed : null;

    /// <summary>Returns the double content if <see cref="Type"/> is <see cref="XmlDaValueType.Double"/>, else null.</summary>
    public double? AsDouble() => Type == XmlDaValueType.Double ? (double?)Boxed : null;

    /// <summary>Returns the bool content if <see cref="Type"/> is <see cref="XmlDaValueType.Boolean"/>, else null.</summary>
    public bool? AsBoolean() => Type == XmlDaValueType.Boolean ? (bool?)Boxed : null;

    /// <summary>Returns the dateTime content if <see cref="Type"/> is <see cref="XmlDaValueType.DateTime"/>, else null.</summary>
    public DateTimeOffset? AsDateTime() => Type == XmlDaValueType.DateTime ? (DateTimeOffset?)Boxed : null;

    /// <summary>Returns the decimal content if <see cref="Type"/> is <see cref="XmlDaValueType.Decimal"/>, else null.</summary>
    public decimal? AsDecimal() => Type == XmlDaValueType.Decimal ? (decimal?)Boxed : null;

    /// <summary>Returns the time content if <see cref="Type"/> is <see cref="XmlDaValueType.Time"/>, else null.</summary>
    public TimeOnly? AsTime() => Type == XmlDaValueType.Time ? (TimeOnly?)Boxed : null;

    /// <summary>Returns the date content if <see cref="Type"/> is <see cref="XmlDaValueType.Date"/>, else null.</summary>
    public DateOnly? AsDate() => Type == XmlDaValueType.Date ? (DateOnly?)Boxed : null;

    /// <summary>Returns the duration content if <see cref="Type"/> is <see cref="XmlDaValueType.Duration"/>, else null.</summary>
    public TimeSpan? AsDuration() => Type == XmlDaValueType.Duration ? (TimeSpan?)Boxed : null;

    /// <summary>Returns the QName content if <see cref="Type"/> is <see cref="XmlDaValueType.QName"/>, else null.</summary>
    public XmlQualifiedName? AsQName() => Type == XmlDaValueType.QName ? (XmlQualifiedName?)Boxed : null;

    /// <summary>Returns the ArrayOfByte content if the type matches, else null.</summary>
    public sbyte[]? AsArrayOfByte() => AsArray<sbyte>(XmlDaValueType.ArrayOfByte);

    /// <summary>Returns the ArrayOfShort content if the type matches, else null.</summary>
    public short[]? AsArrayOfShort() => AsArray<short>(XmlDaValueType.ArrayOfShort);

    /// <summary>Returns the ArrayOfInt content if the type matches, else null.</summary>
    public int[]? AsArrayOfInt() => AsArray<int>(XmlDaValueType.ArrayOfInt);

    /// <summary>Returns the ArrayOfLong content if the type matches, else null.</summary>
    public long[]? AsArrayOfLong() => AsArray<long>(XmlDaValueType.ArrayOfLong);

    /// <summary>Returns the ArrayOfFloat content if the type matches, else null.</summary>
    public float[]? AsArrayOfFloat() => AsArray<float>(XmlDaValueType.ArrayOfFloat);

    /// <summary>Returns the ArrayOfDouble content if the type matches, else null.</summary>
    public double[]? AsArrayOfDouble() => AsArray<double>(XmlDaValueType.ArrayOfDouble);

    /// <summary>Returns the ArrayOfString content if the type matches, else null.</summary>
    public string?[]? AsArrayOfString() => AsArray<string?>(XmlDaValueType.ArrayOfString);

    /// <summary>Returns the ArrayOfBoolean content if the type matches, else null.</summary>
    public bool[]? AsArrayOfBoolean() => AsArray<bool>(XmlDaValueType.ArrayOfBoolean);

    /// <summary>Returns the ArrayOfBoolean content if the type matches, else null.</summary>
    public bool[]? AsArrayOfBool() => AsArrayOfBoolean();

    /// <summary>Returns the ArrayOfDateTime content if the type matches, else null.</summary>
    public DateTimeOffset[]? AsArrayOfDateTime() => AsArray<DateTimeOffset>(XmlDaValueType.ArrayOfDateTime);

    /// <summary>Returns the base64Binary content if the type matches, else null.</summary>
    public byte[]? AsBase64Binary() => AsArray<byte>(XmlDaValueType.Base64Binary);

    internal static XmlDaValue OfParsedTime(string rawText, TimeOnly value) =>
        new() { Type = XmlDaValueType.Time, RawText = rawText, Boxed = value };

    internal static XmlDaValue OfParsedDate(string rawText, DateOnly value) =>
        new() { Type = XmlDaValueType.Date, RawText = rawText, Boxed = value };

    internal static XmlDaValue OfParsedQName(string rawText, string namespaceUri) =>
        new() { Type = XmlDaValueType.QName, RawText = rawText, Boxed = CreateQualifiedName(rawText, namespaceUri) };

    private T[]? AsArray<T>(XmlDaValueType expectedType) =>
        Type == expectedType && Boxed is T[] values ? values.ToArray() : null;

    private static T[] CloneArray<T>(T[] values) {
        ArgumentNullException.ThrowIfNull(values);
        return values.ToArray();
    }

    private static string Join<T>(T[] values) where T : IFormattable =>
        Join(values, static value => value.ToString(null, CultureInfo.InvariantCulture));

    private static string Join<T>(T[] values, Func<T, string> format) =>
        string.Join(" ", values.Select(format));

    private static XmlQualifiedName CreateQualifiedName(string lexicalValue, string namespaceUri) {
        int colon = lexicalValue.LastIndexOf(':');
        string localName = colon >= 0 ? lexicalValue[(colon + 1)..] : lexicalValue;
        return new XmlQualifiedName(localName, namespaceUri);
    }
}
