//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>
/// OPC Data Access quality field — a 16-bit value (transmitted on the wire as
/// <c>WORD</c>) packing four sub-fields per OPC DA §6.8 (quality flags):
/// <list type="table">
///   <listheader><term>Bits</term><description>Field</description></listheader>
///   <item><term>0-1</term><description><see cref="Quality"/> (0=Bad, 1=Uncertain, 2=Reserved, 3=Good)</description></item>
///   <item><term>2-5</term><description><see cref="Substatus"/> (vendor-specific within each quality)</description></item>
///   <item><term>6-7</term><description><see cref="Limit"/> (00=Not Limited, 01=Low, 10=High, 11=Constant)</description></item>
///   <item><term>8-15</term><description>Vendor-specific extension byte (preserved as-is)</description></item>
/// </list>
/// </summary>
public readonly record struct OpcQuality(ushort RawValue)
{
    /// <summary>
    /// Bit mask for the quality sub-field (bits 0-1).
    /// </summary>
    public const ushort QualityMask = 0b0000_0000_0000_0011;

    /// <summary>
    /// Bit mask for the substatus sub-field (bits 2-5).
    /// </summary>
    public const ushort SubstatusMask = 0b0000_0000_0011_1100;

    /// <summary>
    /// Bit mask for the limit sub-field (bits 6-7).
    /// </summary>
    public const ushort LimitMask = 0b0000_0000_1100_0000;

    /// <summary>
    /// Bit mask for the vendor extension (bits 8-15).
    /// </summary>
    public const ushort VendorMask = 0b1111_1111_0000_0000;

    /// <summary>
    /// Bad quality (0).
    /// </summary>
    public static OpcQuality Bad { get; } = new(0b00);

    /// <summary>
    /// Uncertain quality (1).
    /// </summary>
    public static OpcQuality Uncertain { get; } = new(0b01);

    /// <summary>
    /// Good quality (3) — substatus "Non-Specific Good".
    /// </summary>
    public static OpcQuality Good { get; } = new(0b11);

    /// <summary>
    /// The top-level quality category.
    /// </summary>
    public OpcQualityKind Quality => (OpcQualityKind)(RawValue & QualityMask);

    /// <summary>
    /// The substatus code (0-15 within the current quality).
    /// </summary>
    public int Substatus => (RawValue & SubstatusMask) >> 2;

    /// <summary>
    /// The limit field.
    /// </summary>
    public OpcQualityLimit Limit => (OpcQualityLimit)((RawValue & LimitMask) >> 6);

    /// <summary>
    /// Vendor-specific extension byte (upper 8 bits).
    /// </summary>
    public byte VendorExtension => (byte)((RawValue & VendorMask) >> 8);

    /// <summary>
    /// Compose a quality value from its sub-fields.
    /// </summary>
    public static OpcQuality Compose(
        OpcQualityKind quality,
        int substatus = 0,
        OpcQualityLimit limit = OpcQualityLimit.NotLimited,
        byte vendorExtension = 0)
    {
        if (substatus is < 0 or > 0b1111)
        {
            throw new ArgumentOutOfRangeException(nameof(substatus), substatus,
                "Substatus must be 0..15 (4 bits).");
        }
        var raw = (ushort)(
            ((int)quality & 0b11) |
            ((substatus & 0b1111) << 2) |
            (((int)limit & 0b11) << 6) |
            (vendorExtension << 8));
        return new OpcQuality(raw);
    }

    /// <summary>
    /// Returns a new quality with the substatus replaced.
    /// </summary>
    public OpcQuality WithSubstatus(int substatus) =>
        Compose(Quality, substatus, Limit, VendorExtension);

    /// <summary>
    /// Returns a new quality with the limit replaced.
    /// </summary>
    public OpcQuality WithLimit(OpcQualityLimit limit) =>
        Compose(Quality, Substatus, limit, VendorExtension);

    public override string ToString()
        => $"{Quality} (sub={Substatus}, limit={Limit}, vendor=0x{VendorExtension:X2})";
}

