// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Controls whether an SPNEGO acceptor may select NTLMSSP.
/// </summary>
public enum SpnegoNtlmFallbackPolicy
{
    /// <summary>
    /// Only Kerberos may be negotiated.
    /// </summary>
    Disabled,

    /// <summary>
    /// NTLMSSP may be selected only when Kerberos is not available to both peers.
    /// </summary>
    WhenKerberosUnavailable,
}
