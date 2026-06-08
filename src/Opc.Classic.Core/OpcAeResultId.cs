//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>
/// Spec-defined HRESULT constants for OPC Alarms &amp; Events (AE 1.0 / 1.10).
/// </summary>
/// <remarks>
/// AE-specific codes carry values that may NUMERICALLY collide with DA codes —
/// e.g. AE's <c>OPC_E_INVALIDBRANCHNAME = 0xC0040203</c> shares its HRESULT
/// numeric value with DA 3.0's <c>OPC_E_INVALID_PID</c>. The two are
/// disambiguated by the spec context: a return value from an
/// <c>IOPCEventServer</c> method is an AE code; the same numeric value from
/// <c>IOPCBrowse</c> is a DA code. Surfaces in this static class are
/// labelled with their AE meaning.
/// <para>
/// Values sourced from <c>external/inc/opcae_er.h</c>.
/// </para>
/// </remarks>
public static class OpcAeResultId
{
    /// <summary><c>OPC_S_ALREADYACKED</c> (0x00040200) — condition was already acknowledged.</summary>
    public static OpcResultId AlreadyAcked { get; } = new(0x00040200, "OPC_S_ALREADYACKED");

    /// <summary><c>OPC_S_INVALIDBUFFERTIME</c> (0x00040201) — buffer time was not supported; server uses closest available.</summary>
    public static OpcResultId InvalidBufferTime { get; } = new(0x00040201, "OPC_S_INVALIDBUFFERTIME");

    /// <summary><c>OPC_S_INVALIDMAXSIZE</c> (0x00040202) — max-size value was not supported; server uses closest available.</summary>
    public static OpcResultId InvalidMaxSize { get; } = new(0x00040202, "OPC_S_INVALIDMAXSIZE");

    /// <summary><c>OPC_S_INVALIDKEEPALIVETIME</c> (0x00040203) — keep-alive time was not supported; server uses closest available.</summary>
    public static OpcResultId InvalidKeepAliveTime { get; } = new(0x00040203, "OPC_S_INVALIDKEEPALIVETIME");

    /// <summary><c>OPC_E_INVALIDBRANCHNAME</c> (0xC0040203) — area-browser branch name is invalid.</summary>
    public static OpcResultId InvalidBranchName { get; } = new(unchecked((int)0xC0040203u), "OPC_E_INVALIDBRANCHNAME");

    /// <summary><c>OPC_E_INVALIDTIME</c> (0xC0040204) — supplied time value is invalid (e.g., not UTC).</summary>
    public static OpcResultId InvalidTime { get; } = new(unchecked((int)0xC0040204u), "OPC_E_INVALIDTIME");

    /// <summary><c>OPC_E_BUSY</c> (0xC0040205) — server is too busy to fulfil the request.</summary>
    public static OpcResultId Busy { get; } = new(unchecked((int)0xC0040205u), "OPC_E_BUSY");

    /// <summary><c>OPC_E_NOINFO</c> (0xC0040206) — server has no information for the requested condition.</summary>
    public static OpcResultId NoInfo { get; } = new(unchecked((int)0xC0040206u), "OPC_E_NOINFO");
}
