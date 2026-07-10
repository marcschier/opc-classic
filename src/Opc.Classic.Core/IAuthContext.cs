// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Authentication context used by DCOM call channels while binding and protecting DCE/RPC PDUs.
/// </summary>
public interface IAuthContext
{
    /// <summary>
    /// Builds the NTLM/Kerberos type1/AP-REQ token for the bind PDU.
    /// </summary>
    /// <returns>The initial authentication token, or an empty array when unauthenticated.</returns>
    byte[] BuildInitialToken();

    /// <summary>
    /// Processes the server challenge/AP-REP and returns the next token, or an empty array when complete.
    /// </summary>
    /// <param name="serverToken">The authentication token carried by the server response.</param>
    /// <returns>The next token to send, or an empty array when the handshake is complete.</returns>
    byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken);

    /// <summary>
    /// Signs and optionally seals a DCE/RPC PDU according to the negotiated protection level.
    /// </summary>
    /// <param name="signedRegion">
    /// The mutable signed region: the entire PDU EXCEPT the trailing <c>auth_value</c> field, i.e.
    /// the common header, body, authentication padding, and the 8-byte <c>sec_trailer</c> header.
    /// Per MS-RPCE §3.3.1.5.2.2 the per-PDU signature covers exactly these bytes. When sealing is
    /// active the confidential sub-range identified by <paramref name="confidentialOffset" /> and
    /// <paramref name="confidentialLength" /> is encrypted in place.
    /// </param>
    /// <param name="confidentialOffset">
    /// Offset (within <paramref name="signedRegion" />) of the stub sub-range that is encrypted at
    /// <see cref="OpcProtectionLevel.Privacy" />. Ignored for integrity-only protection.
    /// </param>
    /// <param name="confidentialLength">Length of the encrypted stub sub-range. Ignored for integrity-only protection.</param>
    /// <param name="signature">The generated signature/verifier bytes.</param>
    void SignAndSeal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, out byte[] signature);

    /// <summary>
    /// Verifies and optionally unseals a DCE/RPC PDU according to the negotiated protection level.
    /// </summary>
    /// <param name="signedRegion">
    /// The mutable signed region: the entire PDU EXCEPT the trailing <c>auth_value</c> field (see
    /// <see cref="SignAndSeal" />). When sealing is active the confidential sub-range is decrypted in place.
    /// </param>
    /// <param name="confidentialOffset">Offset of the encrypted stub sub-range. Ignored for integrity-only protection.</param>
    /// <param name="confidentialLength">Length of the encrypted stub sub-range. Ignored for integrity-only protection.</param>
    /// <param name="signature">The signature/verifier bytes supplied by the peer.</param>
    /// <returns><see langword="true" /> when verification succeeds.</returns>
    bool VerifyAndUnseal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, ReadOnlyMemory<byte> signature);

    /// <summary>
    /// Gets the negotiated DCE/RPC packet-protection level.
    /// </summary>
    OpcProtectionLevel ProtectionLevel { get; }

    /// <summary>
    /// Gets the DCE/RPC AUTHENTICATION_SERVICE code that identifies which
    /// authentication service the auth verifier carries (per MS-RPCE
    /// §2.2.1.1.7). Defaults to <c>0</c> (no authentication); concrete
    /// implementations override for NTLMSSP (<c>0x0A</c>), Kerberos
    /// (<c>0x10</c>), or SPNEGO/Negotiate (<c>0x09</c>).
    /// </summary>
    byte AuthenticationServiceCode => 0;
}
