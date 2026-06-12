//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Standard OPC DA property IDs (1-12) per OPC DA 3.0 specification §6.5,
/// each paired with its canonical description and VARTYPE.
/// </summary>
/// <remarks>
/// Property IDs 1-100 are reserved for the OPC Foundation. IDs 100-4999 are
/// "vendor-defined" but conventionally aligned to specific meanings (alarm
/// limits, EU bounds, etc.). IDs 5000+ are server-private. This class
/// publishes the OPC-defined set (1-12) only.
/// </remarks>
public static class OpcStandardProperties
{
    /// <summary>Property ID 1 = item canonical data type (VT_I2).</summary>
    public const int CanonicalDataType = 1;

    /// <summary>Property ID 2 = item value (VARIANT).</summary>
    public const int Value = 2;

    /// <summary>Property ID 3 = item quality (VT_I2).</summary>
    public const int Quality = 3;

    /// <summary>Property ID 4 = item timestamp (VT_DATE).</summary>
    public const int Timestamp = 4;

    /// <summary>Property ID 5 = item access rights (VT_I4 bitmask).</summary>
    public const int AccessRights = 5;

    /// <summary>Property ID 6 = server scan rate in milliseconds (VT_R4).</summary>
    public const int ScanRate = 6;

    /// <summary>Property ID 7 = engineering units type (VT_I2, 0=NoEU, 1=Analog, 2=Enumerated).</summary>
    public const int EuType = 7;

    /// <summary>Property ID 8 = engineering units info (VARIANT: SAFEARRAY of double for analog, BSTR for enumerated).</summary>
    public const int EuInfo = 8;

    /// <summary>Property ID 100 = EU label (VT_BSTR, e.g. "RPM", "psi"). Vendor-private but conventional.</summary>
    public const int EuLabel = 100;

    /// <summary>Property ID 101 = item description (VT_BSTR).</summary>
    public const int Description = 101;

    /// <summary>Property ID 102 = high-EU bound (VT_R8).</summary>
    public const int HighEu = 102;

    /// <summary>Property ID 103 = low-EU bound (VT_R8).</summary>
    public const int LowEu = 103;

    /// <summary>The full list of property descriptors published by the default impl.</summary>
    public static IReadOnlyList<OpcStandardProperty> All { get; } =
    [
        new(CanonicalDataType, VarType.VT_I2, "Item Canonical DataType"),
        new(Value, VarType.VT_VARIANT, "Item Value"),
        new(Quality, VarType.VT_I2, "Item Quality"),
        new(Timestamp, VarType.VT_DATE, "Item Timestamp"),
        new(AccessRights, VarType.VT_I4, "Item Access Rights"),
        new(ScanRate, VarType.VT_R4, "Server Scan Rate"),
        new(EuType, VarType.VT_I2, "Item EU Type"),
        new(EuInfo, VarType.VT_VARIANT, "Item EU Info"),
    ];
}
