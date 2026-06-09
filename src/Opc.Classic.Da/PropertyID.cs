//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707 // OPC IDL naming convention preserves underscores for the well-known property names
#pragma warning disable IDE1006

using System;

namespace Opc.Classic.Da;

/// <summary>
/// Identifies an OPC DA item property. The numeric <see cref="Code"/> is the
/// canonical wire identifier (<c>DWORD</c> in OPC IDL); the <see cref="Name"/>
/// is an optional human-readable label.
/// </summary>
/// <remarks>
/// <para>
/// OPC Common defines cross-spec property-ID bands by convention: DA uses 1–99,
/// AE uses 300–399, and HDA uses 400–499. This type documents DA property IDs
/// and does not enforce those cross-spec ownership ranges at runtime.
/// </para>
/// <para>
/// OPC DA categorizes property codes into:
/// </para>
/// <list type="bullet">
///   <item><description><strong>1–99 — Mandatory properties</strong>: every server must support them.</description></item>
///   <item><description><strong>100–199 — Recommended optional</strong>: well-known but not required.</description></item>
///   <item><description><strong>200–299 — DX-specific</strong>: defined by OPC Data eXchange.</description></item>
///   <item><description><strong>300–399 — Complex-data optional</strong>: defined by OPC Complex Data.</description></item>
///   <item><description><strong>400–4999 — Reserved.</strong></description></item>
///   <item><description><strong>5000+ — Vendor-specific.</strong></description></item>
/// </list>
/// </remarks>
public readonly record struct PropertyID(int Code, string? Name = null) {
    /// <inheritdoc />
    public override string ToString() =>
        Name is null ? Code.ToString(System.Globalization.CultureInfo.InvariantCulture)
                     : $"{Code}: {Name}";

    // ---------- Mandatory properties (1–8) ----------

    /// <summary>1 — Item Canonical DataType (VARTYPE).</summary>
    public static PropertyID DataType { get; } = new(1, "Item Canonical DataType");

    /// <summary>2 — Item Value (VARIANT).</summary>
    public static PropertyID Value { get; } = new(2, "Item Value");

    /// <summary>3 — Item Quality (OPC DA quality WORD).</summary>
    public static PropertyID Quality { get; } = new(3, "Item Quality");

    /// <summary>4 — Item Timestamp (FILETIME).</summary>
    public static PropertyID Timestamp { get; } = new(4, "Item Timestamp");

    /// <summary>5 — Item Access Rights (Readable / Writable bitmask).</summary>
    public static PropertyID AccessRights { get; } = new(5, "Item Access Rights");

    /// <summary>6 — Server Scan Rate (float, milliseconds).</summary>
    public static PropertyID ScanRate { get; } = new(6, "Server Scan Rate");

    /// <summary>7 — Item EU Type (none / analog / enumerated).</summary>
    public static PropertyID EuType { get; } = new(7, "Item EU Type");

    /// <summary>8 — Item EU Info (analog: float[2] hi/lo; enumerated: BSTR[] labels).</summary>
    public static PropertyID EuInfo { get; } = new(8, "Item EU Info");

    // ---------- Recommended optional (100–199) ----------

    /// <summary>100 — Engineering units (BSTR — e.g. "DEGC").</summary>
    public static PropertyID EuUnits { get; } = new(100, "Item EU Units");

    /// <summary>101 — Item description (BSTR).</summary>
    public static PropertyID Description { get; } = new(101, "Item Description");

    /// <summary>102 — Engineering-units high range (double).</summary>
    public static PropertyID HighEu { get; } = new(102, "High EU");

    /// <summary>103 — Engineering-units low range (double).</summary>
    public static PropertyID LowEu { get; } = new(103, "Low EU");

    /// <summary>104 — Instrument high range (double).</summary>
    public static PropertyID HighInstrumentRange { get; } = new(104, "High Instrument Range");

    /// <summary>105 — Instrument low range (double).</summary>
    public static PropertyID LowInstrumentRange { get; } = new(105, "Low Instrument Range");

    /// <summary>106 — Contact close label (BSTR — e.g. "ON").</summary>
    public static PropertyID ContactCloseLabel { get; } = new(106, "Contact Close Label");

    /// <summary>107 — Contact open label (BSTR — e.g. "OFF").</summary>
    public static PropertyID ContactOpenLabel { get; } = new(107, "Contact Open Label");

    /// <summary>108 — Item timezone (LONG — minutes offset from UTC).</summary>
    public static PropertyID TimeZone { get; } = new(108, "Item Timezone");

    // ---------- Complex Data extension properties (300-303 are most common) ----------

    /// <summary>300 — Complex Data dictionary item name.</summary>
    public static PropertyID DictionaryItemId { get; } = new(300, "Dictionary Item ID");

    /// <summary>301 — Complex Data type description item name.</summary>
    public static PropertyID TypeDescriptionItemId { get; } = new(301, "Type Description Item ID");
}
