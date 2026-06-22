// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

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
    /// Validated against MS-NLMP Appendix C test vectors by unit tests.
    /// </summary>
    NtlmV2 = 2,

    /// <summary>
    /// Kerberos / SPNEGO — preferred for Active Directory environments.
    /// Provided by the Opc.Classic.Dcom.Kerberos package; until that
    /// phase ships, choosing this throws <see cref="OpcPlatformNotSupportedException"/>.
    /// </summary>
    Kerberos = 3,

    /// <summary>
    /// Windows single sign-on via <see cref="System.Net.Security.NegotiateAuthentication"/>
    /// with the current process identity (<see cref="System.Net.CredentialCache.DefaultNetworkCredentials"/>).
    /// SPNEGO over NTLM or Kerberos depending on what the target server supports.
    /// Windows-only — fails fast on Linux/macOS where there is no platform SSO
    /// identity to inherit. Use this when the MCP server runs on Windows and
    /// the calling user already has DCOM Launch/Access permission on the
    /// target AppID.
    /// </summary>
    WindowsSso = 4,
}
