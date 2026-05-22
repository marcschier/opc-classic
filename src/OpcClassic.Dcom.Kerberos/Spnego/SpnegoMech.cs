//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Dcom.Kerberos.Spnego;

/// <summary>
/// Identifies SPNEGO-negotiated security mechanisms.
/// </summary>
public enum SpnegoMech
{
    /// <summary>
    /// SPNEGO pseudo-mechanism.
    /// </summary>
    Spnego,

    /// <summary>
    /// Kerberos v5 GSS-API mechanism.
    /// </summary>
    KerberosV5,

    /// <summary>
    /// NTLMSSP mechanism.
    /// </summary>
    Ntlmssp,
}

