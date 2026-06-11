//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1720 // Identifier contains type name — these enum members mirror xsd:* and XML-DA ArrayOf* names verbatim

namespace Opc.Classic.Xml;

/// <summary>
/// The XML-DA value-type discriminator written on the <c>xsi:type</c>
/// attribute of a <c>&lt;Value&gt;</c> element.
/// </summary>
public enum XmlDaValueType
{
    /// <summary>Unknown / unsupported xsi:type — raw text preserved in <see cref="XmlDaValue.RawText"/>.</summary>
    Unknown = 0,

    /// <summary><c>xsd:string</c>.</summary>
    String = 1,

    /// <summary><c>xsd:int</c> (32-bit signed).</summary>
    Int32 = 2,

    /// <summary><c>xsd:long</c> (64-bit signed).</summary>
    Int64 = 3,

    /// <summary><c>xsd:short</c> (16-bit signed).</summary>
    Int16 = 4,

    /// <summary><c>xsd:byte</c> (8-bit signed) — also covers DA's VT_I1.</summary>
    Int8 = 5,

    /// <summary><c>xsd:unsignedByte</c>.</summary>
    UInt8 = 6,

    /// <summary><c>xsd:unsignedShort</c>.</summary>
    UInt16 = 7,

    /// <summary><c>xsd:unsignedInt</c>.</summary>
    UInt32 = 8,

    /// <summary><c>xsd:unsignedLong</c>.</summary>
    UInt64 = 9,

    /// <summary><c>xsd:float</c> (IEEE-754 single precision).</summary>
    Single = 10,

    /// <summary><c>xsd:double</c> (IEEE-754 double precision).</summary>
    Double = 11,

    /// <summary><c>xsd:boolean</c>.</summary>
    Boolean = 12,

    /// <summary><c>xsd:dateTime</c> — UTC offset preserved as a <see cref="System.DateTimeOffset"/>.</summary>
    DateTime = 13,

    /// <summary><c>xsd:decimal</c>.</summary>
    Decimal = 14,

    /// <summary><c>xsd:time</c>.</summary>
    Time = 15,

    /// <summary><c>xsd:date</c>.</summary>
    Date = 16,

    /// <summary><c>xsd:duration</c>.</summary>
    Duration = 17,

    /// <summary><c>xsd:QName</c>.</summary>
    QName = 18,

    /// <summary>XML-DA <c>ArrayOfByte</c> (signed 8-bit elements).</summary>
    ArrayOfByte = 19,

    /// <summary>XML-DA <c>ArrayOfShort</c>.</summary>
    ArrayOfShort = 20,

    /// <summary>XML-DA <c>ArrayOfInt</c>.</summary>
    ArrayOfInt = 21,

    /// <summary>XML-DA <c>ArrayOfLong</c>.</summary>
    ArrayOfLong = 22,

    /// <summary>XML-DA <c>ArrayOfFloat</c>.</summary>
    ArrayOfFloat = 23,

    /// <summary>XML-DA <c>ArrayOfDouble</c>.</summary>
    ArrayOfDouble = 24,

    /// <summary>XML-DA <c>ArrayOfString</c>.</summary>
    ArrayOfString = 25,

    /// <summary>XML-DA <c>ArrayOfBoolean</c>.</summary>
    ArrayOfBoolean = 26,

    /// <summary>Alias for XML-DA <c>ArrayOfBoolean</c>.</summary>
    ArrayOfBool = ArrayOfBoolean,

    /// <summary>XML-DA <c>ArrayOfDateTime</c>.</summary>
    ArrayOfDateTime = 27,

    /// <summary><c>xsd:base64Binary</c> byte array.</summary>
    Base64Binary = 28,
}
