//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Xml;

/// <summary>
/// XML-DA "access path" + quality / timestamp metadata that accompanies a
/// single value on the wire. The XML-DA <c>OPCQuality</c> attribute group
/// is projected onto the existing managed <see cref="Opc.Classic.OpcQuality"/>
/// type (same 8-bit packing); XML-DA timestamps map to .NET
/// <see cref="System.DateTimeOffset"/>.
/// </summary>
public static class XmlDaQualityCompat
{
    /// <summary>
    /// Converts a managed <see cref="OpcQuality"/> into its XML-DA wire
    /// representation: a single byte where bits 6-7 are kind, bits 4-5
    /// are limit, bits 0-3 are sub-status. XML-DA carries the low 8 bits
    /// only — the OPC DA vendor extension in the high byte is dropped.
    /// </summary>
    public static byte ToWireByte(OpcQuality quality) => unchecked((byte)(quality.RawValue & 0xFFu));

    /// <summary>
    /// Parses an XML-DA quality wire byte back into the managed type.
    /// The high byte (vendor extension) is zero by construction.
    /// </summary>
    public static OpcQuality FromWireByte(byte raw) => new(raw);
}
