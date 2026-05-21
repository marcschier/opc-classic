//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic;

/// <summary>
/// Authentication mechanism to use when connecting to a remote DCOM OPC server.
/// </summary>
public enum OpcAuthMode
{
    /// <summary>
    /// No authentication — the server must accept anonymous connections.
    /// Almost never works against hardened Windows DCOM servers. Useful only
    /// for lab / loopback testing.
    /// </summary>
    Anonymous = 0,

    /// <summary>
    /// NTLMv1 — legacy. Disabled by default on Windows Server 2022 and
    /// Windows 11 23H2+. Use only against legacy targets that require it,
    /// and only when explicitly opted in via the consumer's client options
    /// (the dedicated <c>OpcClientOptions.AllowNtlmV1</c> arrives in a later phase).
    /// </summary>
    NtlmV1 = 1,

    /// <summary>
    /// NTLMv2 with extended session security — the cross-platform default.
    /// Validated against MS-NLMP Appendix C test vectors by Phase 12 unit tests.
    /// </summary>
    NtlmV2 = 2,

    /// <summary>
    /// Kerberos / SPNEGO — preferred for Active Directory environments.
    /// Provided by Phase 3D's OpcClassic.Dcom.Kerberos package; until that
    /// phase ships, choosing this throws <see cref="OpcPlatformNotSupportedException"/>.
    /// </summary>
    Kerberos = 3,
}
