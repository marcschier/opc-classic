//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>
/// Spec-defined HRESULT constants for OPC Historical Data Access (HDA 1.x).
/// </summary>
/// <remarks>
/// HDA uses a distinct code subrange (0xC0041xxx / 0x40041xxx) so values do
/// not collide with DA or AE codes. The 0x40041xxx-style success codes carry
/// FACILITY_ITF severity-success-with-customer-bit; treat them as success.
/// <para>
/// Values sourced from <c>external/inc/OpcHda_Error.h</c>.
/// </para>
/// </remarks>
public static class OpcHdaResultId {
    /// <summary><c>OPC_E_MAXEXCEEDED</c> (0xC0041001) — too many values requested (e.g., past server's NumValues max).</summary>
    public static OpcResultId MaxExceeded { get; } = new(unchecked((int)0xC0041001u), "OPC_E_MAXEXCEEDED");

    /// <summary><c>OPC_S_NODATA</c> (0x40041002) — no data exists in the requested range (warning).</summary>
    public static OpcResultId NoData { get; } = new(0x40041002, "OPC_S_NODATA");

    /// <summary><c>OPC_S_MOREDATA</c> (0x40041003) — more data exists than was returned (warning).</summary>
    public static OpcResultId MoreData { get; } = new(0x40041003, "OPC_S_MOREDATA");

    /// <summary><c>OPC_E_INVALIDAGGREGATE</c> (0xC0041004) — aggregate ID is not supported.</summary>
    public static OpcResultId InvalidAggregate { get; } = new(unchecked((int)0xC0041004u), "OPC_E_INVALIDAGGREGATE");

    /// <summary><c>OPC_S_CURRENTVALUE</c> (0x40041005) — only the current value is available (warning).</summary>
    public static OpcResultId CurrentValue { get; } = new(0x40041005, "OPC_S_CURRENTVALUE");

    /// <summary><c>OPC_S_EXTRADATA</c> (0x40041006) — returned more data than the client asked for (warning).</summary>
    public static OpcResultId ExtraData { get; } = new(0x40041006, "OPC_S_EXTRADATA");

    /// <summary><c>OPC_E_UNKNOWNATTRID</c> (0xC0041008) — requested attribute ID is unknown to the server.</summary>
    public static OpcResultId UnknownAttrId { get; } = new(unchecked((int)0xC0041008u), "OPC_E_UNKNOWNATTRID");

    /// <summary><c>OPC_E_NOT_AVAIL</c> (0xC0041009) — requested attribute or item is not currently available.</summary>
    public static OpcResultId NotAvail { get; } = new(unchecked((int)0xC0041009u), "OPC_E_NOT_AVAIL");

    /// <summary><c>OPC_E_INVALIDDATATYPE</c> (0xC004100A) — requested data type is invalid for this operation.</summary>
    public static OpcResultId InvalidDataType { get; } = new(unchecked((int)0xC004100Au), "OPC_E_INVALIDDATATYPE");

    /// <summary><c>OPC_E_DATAEXISTS</c> (0xC004100B) — historical data already exists at the supplied timestamp.</summary>
    public static OpcResultId DataExists { get; } = new(unchecked((int)0xC004100Bu), "OPC_E_DATAEXISTS");

    /// <summary><c>OPC_E_INVALIDATTRID</c> (0xC004100C) — supplied attribute ID is invalid.</summary>
    public static OpcResultId InvalidAttrId { get; } = new(unchecked((int)0xC004100Cu), "OPC_E_INVALIDATTRID");

    /// <summary><c>OPC_E_NODATAEXISTS</c> (0xC004100D) — no data exists at the supplied timestamp.</summary>
    public static OpcResultId NoDataExists { get; } = new(unchecked((int)0xC004100Du), "OPC_E_NODATAEXISTS");

    /// <summary><c>OPC_S_INSERTED</c> (0x4004100E) — new data was inserted (warning).</summary>
    public static OpcResultId Inserted { get; } = new(0x4004100E, "OPC_S_INSERTED");

    /// <summary><c>OPC_S_REPLACED</c> (0x4004100F) — existing data was replaced (warning).</summary>
    public static OpcResultId Replaced { get; } = new(0x4004100F, "OPC_S_REPLACED");
}
