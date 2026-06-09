//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Identifies SPNEGO-negotiated security mechanisms.
/// </summary>
public enum SpnegoMech {
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

