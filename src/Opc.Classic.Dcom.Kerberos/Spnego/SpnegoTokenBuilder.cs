//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Convenience factory for common SPNEGO tokens.
/// </summary>
public static class SpnegoTokenBuilder
{
    /// <summary>
    /// Builds an SPNEGO negTokenInit that prefers Kerberos, falls back to NTLMSSP.
    /// </summary>
    /// <param name="kerberosApReq">Kerberos AP-REQ token to carry as the optimistic mechanism token.</param>
    /// <returns>The DER-encoded SPNEGO initial context token.</returns>
    public static byte[] BuildInitToken(ReadOnlyMemory<byte> kerberosApReq)
    {
        var init = new SpnegoNegTokenInit(
            [SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp],
            kerberosApReq,
            null);

        return SpnegoEncoder.EncodeNegTokenInit(init);
    }
}
