//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

#pragma warning disable CA1720 // Identifier contains type name — these enum members mirror xsd:* type names verbatim

namespace OpcClassic.Xml;

/// <summary>
/// The XML-DA value-type discriminator written on the <c>xsi:type</c>
/// attribute of a <c>&lt;Value&gt;</c> element. Covers the scalar set used
/// in virtually all practical OPC XML-DA deployments; arrays, BSTR variants,
/// and vendor-specific types are not yet supported.
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
}
