//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic;

/// <summary>
/// MSRPC authentication-level / DCE 1.1 packet protection level.
/// Maps onto the on-the-wire <c>RPC_C_AUTHN_LEVEL_*</c> values.
/// </summary>
/// <remarks>
/// Microsoft's mandatory DCOM hardening (KB5004442, phase-3, March 2023)
/// rejects activation requests below <see cref="Integrity"/> against
/// patched Windows DCOM servers. The library defaults to <see cref="Integrity"/>
/// for this reason; <see cref="Connect"/> remains an opt-in for unhardened
/// legacy targets.
/// </remarks>
public enum OpcProtectionLevel
{
    /// <summary>
    /// Default level — let the runtime choose. Maps to <see cref="Integrity"/>
    /// when used as the <see cref="OpcConnectData.ProtectionLevel"/> default.
    /// </summary>
    Default = 0,

    /// <summary>
    /// <c>RPC_C_AUTHN_LEVEL_NONE</c> (1) — no authentication.
    /// Rejected by hardened Windows DCOM servers (Event ID 10036).
    /// </summary>
    None = 1,

    /// <summary>
    /// <c>RPC_C_AUTHN_LEVEL_CONNECT</c> (2) — authenticate at bind only.
    /// Rejected by hardened Windows DCOM servers.
    /// </summary>
    Connect = 2,

    /// <summary>
    /// <c>RPC_C_AUTHN_LEVEL_CALL</c> (3) — authenticate at each call. Legacy.
    /// </summary>
    Call = 3,

    /// <summary>
    /// <c>RPC_C_AUTHN_LEVEL_PKT</c> (4) — authenticate each packet (no signing).
    /// </summary>
    Packet = 4,

    /// <summary>
    /// <c>RPC_C_AUTHN_LEVEL_PKT_INTEGRITY</c> (5) — authenticate and sign each packet.
    /// <strong>Required by Microsoft's mandatory DCOM hardening (KB5004442)
    /// as of March 2023</strong> — this is the library default.
    /// </summary>
    Integrity = 5,

    /// <summary>
    /// <c>RPC_C_AUTHN_LEVEL_PKT_PRIVACY</c> (6) — authenticate, sign, and encrypt.
    /// </summary>
    Privacy = 6,
}
