//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Well-known SPNEGO and negotiated mechanism object identifiers.
/// </summary>
public static class SpnegoOids {
    /// <summary>
    /// SPNEGO pseudo-mechanism object identifier.
    /// </summary>
    public const string Spnego = "1.3.6.1.5.5.2";

    /// <summary>
    /// Kerberos v5 GSS-API mechanism object identifier.
    /// </summary>
    public const string KerberosV5 = "1.2.840.113554.1.2.2";

    /// <summary>
    /// NTLMSSP mechanism object identifier.
    /// </summary>
    public const string Ntlmssp = "1.3.6.1.4.1.311.2.2.10";
}
