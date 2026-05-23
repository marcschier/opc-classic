//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic;

/// <summary>
/// Authentication context used by DCOM call channels while binding and protecting DCE/RPC PDUs.
/// </summary>
public interface IAuthContext
{
    /// <summary>Builds the NTLM/Kerberos type1/AP-REQ token for the bind PDU.</summary>
    /// <returns>The initial authentication token, or an empty array when unauthenticated.</returns>
    byte[] BuildInitialToken();

    /// <summary>
    /// Processes the server challenge/AP-REP and returns the next token, or an empty array when complete.
    /// </summary>
    /// <param name="serverToken">The authentication token carried by the server response.</param>
    /// <returns>The next token to send, or an empty array when the handshake is complete.</returns>
    byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken);

    /// <summary>Signs and optionally seals a PDU body according to the negotiated protection level.</summary>
    /// <param name="pduBody">The mutable PDU body, excluding the common DCE/RPC header.</param>
    /// <param name="signature">The generated signature/verifier bytes.</param>
    void SignAndSeal(Span<byte> pduBody, out byte[] signature);

    /// <summary>Verifies and optionally unseals a PDU body according to the negotiated protection level.</summary>
    /// <param name="pduBody">The mutable PDU body, excluding the common DCE/RPC header.</param>
    /// <param name="signature">The signature/verifier bytes supplied by the peer.</param>
    /// <returns><see langword="true" /> when verification succeeds.</returns>
    bool VerifyAndUnseal(Span<byte> pduBody, ReadOnlyMemory<byte> signature);

    /// <summary>Gets the negotiated DCE/RPC packet-protection level.</summary>
    OpcProtectionLevel ProtectionLevel { get; }
}
