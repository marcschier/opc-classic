//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic;

/// <summary>
/// Wraps an OPC HRESULT into a managed value type with a friendly description.
/// </summary>
/// <remarks>
/// OPC Classic methods return <c>HRESULT</c>s. Some are well-known per the OPC
/// specifications (e.g. <c>OPC_E_INVALIDHANDLE = 0xC0040001</c>); others are
/// vendor-specific and require <c>IOPCServer::GetErrorString(HRESULT, LCID)</c>
/// to resolve to a human-readable message.
/// <para>
/// This is a <see langword="readonly"/> <see langword="record"/> <see langword="struct"/>
/// — equality, hash, and pattern-matching come for free.
/// </para>
/// </remarks>
public readonly record struct OpcResultId(int Code, string? Description)
{
    /// <summary>The HRESULT severity bit (bit 31): true => failure.</summary>
    public bool IsFailure => (Code & unchecked((int)0x80000000)) != 0;

    /// <summary>The HRESULT severity bit (bit 31): false => success.</summary>
    public bool IsSuccess => !IsFailure;

    /// <summary>Facility code (bits 16-26 of the HRESULT).</summary>
    public int Facility => (Code >> 16) & 0x07FF;

    /// <summary>Code part of the HRESULT (bits 0-15).</summary>
    public int CodePart => Code & 0xFFFF;

    /// <summary>The OPC Foundation facility (FACILITY_OPC = 4).</summary>
    public const int FacilityOpc = 4;

    /// <summary><c>S_OK</c> (0x00000000).</summary>
    public static OpcResultId Ok { get; } = new(0x00000000, "S_OK");

    /// <summary><c>S_FALSE</c> (0x00000001).</summary>
    public static OpcResultId False { get; } = new(0x00000001, "S_FALSE");

    /// <summary><c>E_FAIL</c> (0x80004005).</summary>
    public static OpcResultId Fail { get; } = new(unchecked((int)0x80004005u), "E_FAIL");

    /// <summary><c>E_INVALIDARG</c> (0x80070057).</summary>
    public static OpcResultId InvalidArg { get; } = new(unchecked((int)0x80070057u), "E_INVALIDARG");

    /// <summary><c>E_NOTIMPL</c> (0x80004001).</summary>
    public static OpcResultId NotImplemented { get; } = new(unchecked((int)0x80004001u), "E_NOTIMPL");

    /// <summary><c>E_OUTOFMEMORY</c> (0x8007000E).</summary>
    public static OpcResultId OutOfMemory { get; } = new(unchecked((int)0x8007000Eu), "E_OUTOFMEMORY");

    // --- OPC Foundation general result codes (FACILITY_OPC) ---

    /// <summary><c>OPC_E_INVALIDHANDLE</c> (0xC0040001) — server handle is invalid.</summary>
    public static OpcResultId InvalidHandle { get; } = new(unchecked((int)0xC0040001u), "OPC_E_INVALIDHANDLE");

    /// <summary><c>OPC_E_BADTYPE</c> (0xC0040004) — requested data type unsupported.</summary>
    public static OpcResultId BadType { get; } = new(unchecked((int)0xC0040004u), "OPC_E_BADTYPE");

    /// <summary><c>OPC_E_PUBLIC</c> (0xC0040005) — public groups are not supported.</summary>
    public static OpcResultId Public { get; } = new(unchecked((int)0xC0040005u), "OPC_E_PUBLIC");

    /// <summary><c>OPC_E_BADRIGHTS</c> (0xC0040006) — item has incompatible access rights.</summary>
    public static OpcResultId BadRights { get; } = new(unchecked((int)0xC0040006u), "OPC_E_BADRIGHTS");

    /// <summary><c>OPC_E_UNKNOWNITEMID</c> (0xC0040007) — item ID does not exist on server.</summary>
    public static OpcResultId UnknownItemId { get; } = new(unchecked((int)0xC0040007u), "OPC_E_UNKNOWNITEMID");

    /// <summary><c>OPC_E_INVALIDITEMID</c> (0xC0040008) — item ID syntax is invalid.</summary>
    public static OpcResultId InvalidItemId { get; } = new(unchecked((int)0xC0040008u), "OPC_E_INVALIDITEMID");

    /// <summary><c>OPC_E_INVALIDFILTER</c> (0xC0040009) — filter string is malformed.</summary>
    public static OpcResultId InvalidFilter { get; } = new(unchecked((int)0xC0040009u), "OPC_E_INVALIDFILTER");

    /// <summary><c>OPC_E_UNKNOWNPATH</c> (0xC004000A) — browse path does not exist.</summary>
    public static OpcResultId UnknownPath { get; } = new(unchecked((int)0xC004000Au), "OPC_E_UNKNOWNPATH");

    /// <summary><c>OPC_E_RANGE</c> (0xC004000B) — value is out of range.</summary>
    public static OpcResultId Range { get; } = new(unchecked((int)0xC004000Bu), "OPC_E_RANGE");

    /// <summary><c>OPC_E_DUPLICATENAME</c> (0xC004000C) — duplicate group name.</summary>
    public static OpcResultId DuplicateName { get; } = new(unchecked((int)0xC004000Cu), "OPC_E_DUPLICATENAME");

    /// <summary><c>OPC_S_UNSUPPORTEDRATE</c> (0x0004000D) — server forced a different update rate.</summary>
    public static OpcResultId UnsupportedRate { get; } = new(0x0004000D, "OPC_S_UNSUPPORTEDRATE");

    /// <summary><c>OPC_S_CLAMP</c> (0x0004000E) — server clamped a value to the allowed range.</summary>
    public static OpcResultId Clamp { get; } = new(0x0004000E, "OPC_S_CLAMP");

    /// <summary><c>OPC_S_INUSE</c> (0x0004000F) — operation could not be performed because the group is in use.</summary>
    public static OpcResultId InUse { get; } = new(0x0004000F, "OPC_S_INUSE");

    /// <summary><c>OPC_E_INVALIDCONFIGFILE</c> (0xC0040010).</summary>
    public static OpcResultId InvalidConfigFile { get; } = new(unchecked((int)0xC0040010u), "OPC_E_INVALIDCONFIGFILE");

    /// <summary><c>OPC_E_NOTFOUND</c> (0xC0040011) — server cannot find the public group.</summary>
    public static OpcResultId NotFound { get; } = new(unchecked((int)0xC0040011u), "OPC_E_NOTFOUND");

    // --- OPC DA 3.0 result codes (FACILITY_OPC) ---

    /// <summary><c>OPC_E_INVALID_PID</c> (0xC0040203) — property ID is invalid for the item.</summary>
    public static OpcResultId InvalidPid { get; } = new(unchecked((int)0xC0040203u), "OPC_E_INVALID_PID");

    /// <summary><c>OPC_E_DEADBANDNOTSET</c> (0xC0040400) — no deadband has been set for the group.</summary>
    public static OpcResultId DeadbandNotSet { get; } = new(unchecked((int)0xC0040400u), "OPC_E_DEADBANDNOTSET");

    /// <summary><c>OPC_E_DEADBANDNOTSUPPORTED</c> (0xC0040401) — item does not support deadband.</summary>
    public static OpcResultId DeadbandNotSupported { get; } = new(unchecked((int)0xC0040401u), "OPC_E_DEADBANDNOTSUPPORTED");

    /// <summary><c>OPC_E_NOBUFFERING</c> (0xC0040402) — buffering is not supported by this group.</summary>
    public static OpcResultId NoBuffering { get; } = new(unchecked((int)0xC0040402u), "OPC_E_NOBUFFERING");

    /// <summary><c>OPC_E_INVALIDCONTINUATIONPOINT</c> (0xC0040403) — browse continuation point is invalid.</summary>
    public static OpcResultId InvalidContinuationPoint { get; } = new(unchecked((int)0xC0040403u), "OPC_E_INVALIDCONTINUATIONPOINT");

    /// <summary><c>OPC_S_DATAQUEUEOVERFLOW</c> (0x00040404) — server's data buffer overflowed (warning).</summary>
    public static OpcResultId DataQueueOverflow { get; } = new(0x00040404, "OPC_S_DATAQUEUEOVERFLOW");

    /// <summary><c>OPC_E_RATENOTSET</c> (0xC0040405) — no sampling rate has been set for the item.</summary>
    public static OpcResultId RateNotSet { get; } = new(unchecked((int)0xC0040405u), "OPC_E_RATENOTSET");

    /// <summary><c>OPC_E_NOTSUPPORTED</c> (0xC0040406) — operation not supported by this server.</summary>
    public static OpcResultId NotSupported { get; } = new(unchecked((int)0xC0040406u), "OPC_E_NOTSUPPORTED");

    public override string ToString()
    {
        var hex = $"0x{Code:X8}";
        return Description is null ? hex : $"{hex} ({Description})";
    }
}
