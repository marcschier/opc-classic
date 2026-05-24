//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

#pragma warning disable CA5351 // NTLMSSP message signing is specified as HMAC-MD5.

namespace Opc.Classic.Dcom.Internal.Ntlm;

/// <summary>
/// Computes NTLMv2 extended-session-security SIGNATURE_BLOCK values.
/// </summary>
public static class NtlmMessageSignature {
    /// <summary>
    /// The fixed NTLM message-signature length.
    /// </summary>
    public const int SignatureLength = 16;

    /// <summary>
    /// Computes an NTLM SIGNATURE_BLOCK over the supplied message.
    /// </summary>
    /// <param name="signingKey">The negotiated NTLM signing key.</param>
    /// <param name="message">The message bytes to sign.</param>
    /// <param name="sequenceNumber">The NTLM sequence number. SPNEGO mechListMIC uses zero.</param>
    /// <returns>The 16-byte NTLM SIGNATURE_BLOCK.</returns>
    public static byte[] Sign(ReadOnlySpan<byte> signingKey, ReadOnlySpan<byte> message, uint sequenceNumber = 0) {
        if (signingKey.IsEmpty) {
            throw new ArgumentException("The NTLM signing key must not be empty.", nameof(signingKey));
        }

        var hmacInput = new byte[checked(sizeof(uint) + message.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(hmacInput.AsSpan(0, sizeof(uint)), sequenceNumber);
        message.CopyTo(hmacInput.AsSpan(sizeof(uint)));

        byte[] checksum;
        using (var hmac = new HMACMD5(signingKey.ToArray())) {
            checksum = hmac.ComputeHash(hmacInput);
        }

        var signature = new byte[SignatureLength];
        signature[0] = 0x01;
        checksum.AsSpan(0, 8).CopyTo(signature.AsSpan(4, 8));
        BinaryPrimitives.WriteUInt32LittleEndian(signature.AsSpan(12, sizeof(uint)), sequenceNumber);
        return signature;
    }

    /// <summary>
    /// Verifies an NTLM SIGNATURE_BLOCK over the supplied message.
    /// </summary>
    /// <param name="signingKey">The negotiated NTLM signing key.</param>
    /// <param name="message">The message bytes to verify.</param>
    /// <param name="signature">The 16-byte NTLM SIGNATURE_BLOCK.</param>
    /// <param name="sequenceNumber">The NTLM sequence number. SPNEGO mechListMIC uses zero.</param>
    /// <returns><see langword="true" /> when the signature matches.</returns>
    public static bool Verify(
        ReadOnlySpan<byte> signingKey,
        ReadOnlySpan<byte> message,
        ReadOnlySpan<byte> signature,
        uint sequenceNumber = 0) {
        if (signature.Length != SignatureLength) {
            return false;
        }

        var expected = Sign(signingKey, message, sequenceNumber);
        return CryptographicOperations.FixedTimeEquals(expected, signature);
    }
}
