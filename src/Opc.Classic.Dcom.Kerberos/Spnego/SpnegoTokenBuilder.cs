//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Convenience factory for common SPNEGO tokens.
/// </summary>
public static class SpnegoTokenBuilder {
    private static readonly string[] KerberosPreferredMechTypes = [SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp];

    /// <summary>
    /// Builds an SPNEGO negTokenInit that prefers Kerberos, falls back to NTLMSSP.
    /// </summary>
    /// <param name="kerberosApReq">Kerberos AP-REQ token to carry as the optimistic mechanism token.</param>
    /// <returns>The DER-encoded SPNEGO initial context token.</returns>
    public static byte[] BuildInitToken(ReadOnlyMemory<byte> kerberosApReq) =>
        SpnegoEncoder.EncodeNegTokenInit(CreateKerberosPreferredInit(kerberosApReq));

    /// <summary>
    /// Builds an SPNEGO negTokenInit and returns the encoded MechTypeList used for mechListMIC.
    /// </summary>
    /// <param name="kerberosApReq">Kerberos AP-REQ token to carry as the optimistic mechanism token.</param>
    /// <param name="mechListBytes">Exact DER bytes of the MechTypeList SEQUENCE.</param>
    /// <returns>The DER-encoded SPNEGO initial context token.</returns>
    public static byte[] BuildInitToken(ReadOnlyMemory<byte> kerberosApReq, out byte[] mechListBytes) {
        var init = CreateKerberosPreferredInit(kerberosApReq);
        mechListBytes = init.MechListBytes.ToArray();
        return SpnegoEncoder.EncodeNegTokenInit(init);
    }

    /// <summary>
    /// Creates the Kerberos-first NegTokenInit used by DCOM authentication.
    /// </summary>
    /// <param name="kerberosApReq">Kerberos AP-REQ token to carry as the optimistic mechanism token.</param>
    /// <returns>The Kerberos-preferred initial negotiation fields.</returns>
    public static SpnegoNegTokenInit CreateKerberosPreferredInit(ReadOnlyMemory<byte> kerberosApReq) =>
        new(
            KerberosPreferredMechTypes,
            kerberosApReq,
            null,
            SpnegoEncoder.EncodeMechTypeList(KerberosPreferredMechTypes));
}
