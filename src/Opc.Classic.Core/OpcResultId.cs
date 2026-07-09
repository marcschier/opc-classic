// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

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
    /// <summary>
    /// The HRESULT severity bit (bit 31): true => failure.
    /// </summary>
    public bool IsFailure => (Code & unchecked((int)0x80000000)) != 0;

    /// <summary>
    /// The HRESULT severity bit (bit 31): false => success.
    /// </summary>
    public bool IsSuccess => !IsFailure;

    /// <summary>
    /// Facility code (bits 16-26 of the HRESULT).
    /// </summary>
    public int Facility => (Code >> 16) & 0x07FF;

    /// <summary>
    /// Code part of the HRESULT (bits 0-15).
    /// </summary>
    public int CodePart => Code & 0xFFFF;

    /// <summary>
    /// The OPC Foundation facility (FACILITY_OPC = 4).
    /// </summary>
    public const int FacilityOpc = 4;

    /// <summary>
    /// <c>HRESULT_FROM_WIN32(w)</c> per [MS-ERREF] §2.1.1: maps a non-zero
    /// Win32 error code to its canonical HRESULT representation
    /// (sets the severity bit and <c>FACILITY_WIN32 = 7</c>).
    /// Zero codes return <see cref="Ok"/>.
    /// </summary>
    /// <param name="win32Code">The Win32 error code (typically from <c>GetLastError</c>).</param>
    /// <returns>The HRESULT-shaped <see cref="OpcResultId"/>.</returns>
    public static OpcResultId FromWin32(uint win32Code)
    {
        if (win32Code == 0)
        {
            return Ok;
        }

        // Already a wrapped HRESULT? (severity-bit + FACILITY_WIN32)
        const uint Win32WrappedMask = 0xFFFF0000u;
        const uint Win32WrappedPrefix = 0x80070000u;
        if ((win32Code & Win32WrappedMask) == Win32WrappedPrefix)
        {
            return new OpcResultId(unchecked((int)win32Code), $"WIN32({win32Code & 0xFFFFu})");
        }

        var hresult = unchecked((int)(0x80070000u | (win32Code & 0xFFFFu)));
        return new OpcResultId(hresult, $"WIN32({win32Code & 0xFFFFu})");
    }

    /// <summary>
    /// Maps an <c>NTSTATUS</c> value into HRESULT space per
    /// [MS-ERREF] §2.1.1: the N bit (0x10000000) is set so callers can
    /// distinguish promoted-NTSTATUS HRESULTs from native ones.
    /// Already-promoted NTSTATUS values (N bit already set) are returned
    /// as-is.
    /// </summary>
    /// <param name="ntStatus">The NTSTATUS value (typically from <c>NtStatus</c> enum).</param>
    /// <returns>The HRESULT-shaped <see cref="OpcResultId"/>.</returns>
    public static OpcResultId FromNtStatus(uint ntStatus)
    {
        // N bit already set => spec says return as-is.
        const uint NtBit = 0x10000000u;
        var hresult = unchecked((int)(ntStatus | NtBit));
        return new OpcResultId(hresult, $"NTSTATUS(0x{ntStatus:X8})");
    }

    /// <summary>
    /// <c>S_OK</c> (0x00000000).
    /// </summary>
    public static OpcResultId Ok { get; } = new(0x00000000, "S_OK");

    /// <summary>
    /// <c>S_FALSE</c> (0x00000001).
    /// </summary>
    public static OpcResultId False { get; } = new(0x00000001, "S_FALSE");

    /// <summary>
    /// <c>E_FAIL</c> (0x80004005).
    /// </summary>
    public static OpcResultId Fail { get; } = new(unchecked((int)0x80004005u), "E_FAIL");

    /// <summary>
    /// <c>E_INVALIDARG</c> (0x80070057).
    /// </summary>
    public static OpcResultId InvalidArg { get; } = new(unchecked((int)0x80070057u), "E_INVALIDARG");

    /// <summary>
    /// <c>E_NOTIMPL</c> (0x80004001).
    /// </summary>
    public static OpcResultId NotImplemented { get; } = new(unchecked((int)0x80004001u), "E_NOTIMPL");

    /// <summary>
    /// <c>E_OUTOFMEMORY</c> (0x8007000E).
    /// </summary>
    public static OpcResultId OutOfMemory { get; } = new(unchecked((int)0x8007000Eu), "E_OUTOFMEMORY");

    /// <summary>
    /// <c>E_NOINTERFACE</c> (0x80004002) — the requested interface is not supported by the object.
    /// </summary>
    public static OpcResultId NoInterface { get; } = new(unchecked((int)0x80004002u), "E_NOINTERFACE");

    /// <summary>
    /// <c>E_POINTER</c> (0x80004003) — invalid pointer argument.
    /// </summary>
    public static OpcResultId Pointer { get; } = new(unchecked((int)0x80004003u), "E_POINTER");

    /// <summary>
    /// <c>E_ABORT</c> (0x80004004) — operation aborted.
    /// </summary>
    public static OpcResultId Abort { get; } = new(unchecked((int)0x80004004u), "E_ABORT");

    /// <summary>
    /// <c>E_ACCESSDENIED</c> (0x80070005) — general access-denied error.
    /// </summary>
    public static OpcResultId AccessDenied { get; } = new(unchecked((int)0x80070005u), "E_ACCESSDENIED");

    // --- OPC Foundation general result codes (FACILITY_OPC) ---

    /// <summary>
    /// <c>OPC_E_INVALIDHANDLE</c> (0xC0040001) — server handle is invalid.
    /// </summary>
    public static OpcResultId InvalidHandle { get; } = new(unchecked((int)0xC0040001u), "OPC_E_INVALIDHANDLE");

    /// <summary>
    /// <c>OPC_E_BADTYPE</c> (0xC0040004) — requested data type unsupported.
    /// </summary>
    public static OpcResultId BadType { get; } = new(unchecked((int)0xC0040004u), "OPC_E_BADTYPE");

    /// <summary>
    /// <c>OPC_E_PUBLIC</c> (0xC0040005) — public groups are not supported.
    /// </summary>
    public static OpcResultId Public { get; } = new(unchecked((int)0xC0040005u), "OPC_E_PUBLIC");

    /// <summary>
    /// <c>OPC_E_BADRIGHTS</c> (0xC0040006) — item has incompatible access rights.
    /// </summary>
    public static OpcResultId BadRights { get; } = new(unchecked((int)0xC0040006u), "OPC_E_BADRIGHTS");

    /// <summary>
    /// <c>OPC_E_UNKNOWNITEMID</c> (0xC0040007) — item ID does not exist on server.
    /// </summary>
    public static OpcResultId UnknownItemId { get; } = new(unchecked((int)0xC0040007u), "OPC_E_UNKNOWNITEMID");

    /// <summary>
    /// <c>OPC_E_INVALIDITEMID</c> (0xC0040008) — item ID syntax is invalid.
    /// </summary>
    public static OpcResultId InvalidItemId { get; } = new(unchecked((int)0xC0040008u), "OPC_E_INVALIDITEMID");

    /// <summary>
    /// <c>OPC_E_INVALIDFILTER</c> (0xC0040009) — filter string is malformed.
    /// </summary>
    public static OpcResultId InvalidFilter { get; } = new(unchecked((int)0xC0040009u), "OPC_E_INVALIDFILTER");

    /// <summary>
    /// <c>OPC_E_UNKNOWNPATH</c> (0xC004000A) — browse path does not exist.
    /// </summary>
    public static OpcResultId UnknownPath { get; } = new(unchecked((int)0xC004000Au), "OPC_E_UNKNOWNPATH");

    /// <summary>
    /// <c>OPC_E_RANGE</c> (0xC004000B) — value is out of range.
    /// </summary>
    public static OpcResultId Range { get; } = new(unchecked((int)0xC004000Bu), "OPC_E_RANGE");

    /// <summary>
    /// <c>OPC_E_DUPLICATENAME</c> (0xC004000C) — duplicate group name.
    /// </summary>
    public static OpcResultId DuplicateName { get; } = new(unchecked((int)0xC004000Cu), "OPC_E_DUPLICATENAME");

    /// <summary>
    /// <c>OPC_S_UNSUPPORTEDRATE</c> (0x0004000D) — server forced a different update rate.
    /// </summary>
    public static OpcResultId UnsupportedRate { get; } = new(0x0004000D, "OPC_S_UNSUPPORTEDRATE");

    /// <summary>
    /// <c>OPC_S_CLAMP</c> (0x0004000E) — server clamped a value to the allowed range.
    /// </summary>
    public static OpcResultId Clamp { get; } = new(0x0004000E, "OPC_S_CLAMP");

    /// <summary>
    /// <c>OPC_S_INUSE</c> (0x0004000F) — operation could not be performed because the group is in use.
    /// </summary>
    public static OpcResultId InUse { get; } = new(0x0004000F, "OPC_S_INUSE");

    /// <summary>
    /// <c>OPC_E_INVALIDCONFIGFILE</c> (0xC0040010).
    /// </summary>
    public static OpcResultId InvalidConfigFile { get; } = new(unchecked((int)0xC0040010u), "OPC_E_INVALIDCONFIGFILE");

    /// <summary>
    /// <c>OPC_E_NOTFOUND</c> (0xC0040011) — server cannot find the public group.
    /// </summary>
    public static OpcResultId NotFound { get; } = new(unchecked((int)0xC0040011u), "OPC_E_NOTFOUND");

    // --- OPC DA 3.0 result codes (FACILITY_OPC) ---

    /// <summary>
    /// <c>OPC_E_INVALID_PID</c> (0xC0040203) — property ID is invalid for the item.
    /// </summary>
    public static OpcResultId InvalidPid { get; } = new(unchecked((int)0xC0040203u), "OPC_E_INVALID_PID");

    /// <summary>
    /// <c>OPC_E_DEADBANDNOTSET</c> (0xC0040400) — no deadband has been set for the group.
    /// </summary>
    public static OpcResultId DeadbandNotSet { get; } = new(unchecked((int)0xC0040400u), "OPC_E_DEADBANDNOTSET");

    /// <summary>
    /// <c>OPC_E_DEADBANDNOTSUPPORTED</c> (0xC0040401) — item does not support deadband.
    /// </summary>
    public static OpcResultId DeadbandNotSupported { get; } = new(unchecked((int)0xC0040401u), "OPC_E_DEADBANDNOTSUPPORTED");

    /// <summary>
    /// <c>OPC_E_NOBUFFERING</c> (0xC0040402) — buffering is not supported by this group.
    /// </summary>
    public static OpcResultId NoBuffering { get; } = new(unchecked((int)0xC0040402u), "OPC_E_NOBUFFERING");

    /// <summary>
    /// <c>OPC_E_INVALIDCONTINUATIONPOINT</c> (0xC0040403) — browse continuation point is invalid.
    /// </summary>
    public static OpcResultId InvalidContinuationPoint { get; } = new(unchecked((int)0xC0040403u), "OPC_E_INVALIDCONTINUATIONPOINT");

    /// <summary>
    /// <c>OPC_S_DATAQUEUEOVERFLOW</c> (0x00040404) — server's data buffer overflowed (warning).
    /// </summary>
    public static OpcResultId DataQueueOverflow { get; } = new(0x00040404, "OPC_S_DATAQUEUEOVERFLOW");

    /// <summary>
    /// <c>OPC_E_RATENOTSET</c> (0xC0040405) — no sampling rate has been set for the item.
    /// </summary>
    public static OpcResultId RateNotSet { get; } = new(unchecked((int)0xC0040405u), "OPC_E_RATENOTSET");

    /// <summary>
    /// <c>OPC_E_NOTSUPPORTED</c> (0xC0040406) — operation not supported by this server.
    /// </summary>
    public static OpcResultId NotSupported { get; } = new(unchecked((int)0xC0040406u), "OPC_E_NOTSUPPORTED");

    public override string ToString()
    {
        var hex = $"0x{Code:X8}";
        return Description is null ? hex : $"{hex} ({Description})";
    }
}
