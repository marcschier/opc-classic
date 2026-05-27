//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// SMB2 signing algorithms selected by the negotiated dialect; see [MS-SMB2] §3.1.5.1.
/// </summary>
public enum Smb2SigningAlgorithm
{
    /// <summary>HMAC-SHA256 with the session key, truncated to 16 bytes for SMB 2.0.2/2.1.</summary>
    HmacSha256,

    /// <summary>AES-128-CMAC with a derived signing key for SMB 3.x.</summary>
    AesCmac,
}

/// <summary>
/// Computes and verifies the 16-byte SMB2 message signature over the complete SMB2
/// header and body with the Signature field zeroed, per [MS-SMB2] §3.1.4.1 and §2.2.1.2.
/// </summary>
public sealed class Smb2Signer
{
    /// <summary>Length of the SMB2 Signature header field in bytes; see [MS-SMB2] §2.2.1.2.</summary>
    public const int SignatureLength = 16;

    private const int AesBlockLength = 16;
    private const int SmbSigningKeyLengthBits = 128;
    private const byte AesCmacRb = 0x87;

    private readonly byte[] _signingKey;

    /// <summary>
    /// Initializes a signer with the already-selected signing key and algorithm; see [MS-SMB2] §3.1.5.1.
    /// </summary>
    /// <param name="signingKey">Session key for HMAC-SHA256, or SMB3-derived AES-CMAC signing key.</param>
    /// <param name="algorithm">The SMB2 signing algorithm selected by dialect.</param>
    public Smb2Signer(ReadOnlySpan<byte> signingKey, Smb2SigningAlgorithm algorithm)
    {
        if (signingKey.IsEmpty)
        {
            throw new ArgumentException("Signing key must not be empty.", nameof(signingKey));
        }

        if (algorithm == Smb2SigningAlgorithm.AesCmac && signingKey.Length != AesBlockLength)
        {
            throw new ArgumentException("AES-128-CMAC signing keys must be 16 bytes.", nameof(signingKey));
        }

        Algorithm = algorithm;
        _signingKey = signingKey.ToArray();
    }

    /// <summary>Gets the SMB2 signing algorithm used by this signer; see [MS-SMB2] §3.1.5.1.</summary>
    public Smb2SigningAlgorithm Algorithm { get; }

    /// <summary>
    /// Creates a signer from the authentication SessionKey and negotiated dialect, deriving SMB3 signing keys
    /// with the SP800-108 counter-mode KDF required by [MS-SMB2] §3.1.5.1.
    /// </summary>
    /// <param name="dialect">Negotiated SMB2 dialect.</param>
    /// <param name="sessionKey">Session key exported by NTLMSSP or Kerberos.</param>
    /// <param name="preauthIntegrityHash">SMB 3.1.1 PreauthIntegrityHashValue context; ignored before SMB 3.1.1.</param>
    /// <returns>A signer ready to sign SMB2 messages for the negotiated dialect.</returns>
    public static Smb2Signer CreateForDialect(
        Smb2Dialect dialect,
        ReadOnlySpan<byte> sessionKey,
        ReadOnlySpan<byte> preauthIntegrityHash = default)
    {
        if (GetAlgorithmForDialect(dialect) == Smb2SigningAlgorithm.HmacSha256)
        {
            return new Smb2Signer(sessionKey, Smb2SigningAlgorithm.HmacSha256);
        }

        byte[] signingKey = DeriveSmb3SigningKey(dialect, sessionKey, preauthIntegrityHash);
        return new Smb2Signer(signingKey, Smb2SigningAlgorithm.AesCmac);
    }

    /// <summary>
    /// Selects HMAC-SHA256 for SMB 2.0.2/2.1 and AES-CMAC for SMB 3.x, per [MS-SMB2] §3.1.5.1.
    /// </summary>
    /// <param name="dialect">Negotiated SMB2 dialect.</param>
    /// <returns>The signing algorithm for <paramref name="dialect" />.</returns>
    public static Smb2SigningAlgorithm GetAlgorithmForDialect(Smb2Dialect dialect) => dialect switch
    {
        Smb2Dialect.Smb202 or Smb2Dialect.Smb210 => Smb2SigningAlgorithm.HmacSha256,
        Smb2Dialect.Smb300 or Smb2Dialect.Smb302 or Smb2Dialect.Smb311 => Smb2SigningAlgorithm.AesCmac,
        _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "Unsupported SMB2 dialect for signing."),
    };

    /// <summary>
    /// Derives the SMB3 SigningKey with the SP800-108 counter-mode KDF cited by [MS-SMB2] §3.1.5.1.
    /// SMB 3.0/3.0.2 use label "SMB2AESCMAC" and context "SmbSign\0"; SMB 3.1.1 uses
    /// label "SMBSigningKey" and the PreauthIntegrityHashValue as context.
    /// </summary>
    /// <param name="dialect">Negotiated SMB3 dialect.</param>
    /// <param name="sessionKey">Session key exported by NTLMSSP or Kerberos.</param>
    /// <param name="preauthIntegrityHash">SMB 3.1.1 PreauthIntegrityHashValue context.</param>
    /// <returns>The 16-byte AES-128-CMAC signing key.</returns>
    public static byte[] DeriveSmb3SigningKey(
        Smb2Dialect dialect,
        ReadOnlySpan<byte> sessionKey,
        ReadOnlySpan<byte> preauthIntegrityHash = default)
    {
        return dialect switch
        {
            Smb2Dialect.Smb300 or Smb2Dialect.Smb302 => DeriveKeyCounterMode(
                sessionKey,
                "SMB2AESCMAC"u8,
                "SmbSign\0"u8,
                SmbSigningKeyLengthBits),
            Smb2Dialect.Smb311 => preauthIntegrityHash.IsEmpty
                ? throw new ArgumentException(
                    "SMB 3.1.1 signing key derivation requires the PreauthIntegrityHashValue.",
                    nameof(preauthIntegrityHash))
                : DeriveKeyCounterMode(
                    sessionKey,
                    "SMBSigningKey"u8,
                    preauthIntegrityHash,
                    SmbSigningKeyLengthBits),
            _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "SMB3 signing key derivation requires an SMB 3.x dialect."),
        };
    }

    /// <summary>
    /// Implements the SP800-108 counter-mode KDF used by [MS-SMB2] §3.1.5.1:
    /// HMAC-SHA256(KI, i || Label || 0x00 || Context || L), returning the leftmost L bits.
    /// </summary>
    /// <param name="key">KDF input key KI.</param>
    /// <param name="label">KDF label.</param>
    /// <param name="context">KDF context.</param>
    /// <param name="lengthBits">Requested output length in bits.</param>
    /// <returns>The derived key bytes.</returns>
    public static byte[] DeriveKeyCounterMode(
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> label,
        ReadOnlySpan<byte> context,
        int lengthBits)
    {
        if (key.IsEmpty)
        {
            throw new ArgumentException("KDF key must not be empty.", nameof(key));
        }
        if (lengthBits <= 0 || (lengthBits % 8) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lengthBits), lengthBits, "KDF length must be a positive multiple of 8 bits.");
        }

        byte[] input = new byte[sizeof(uint) + label.Length + 1 + context.Length + sizeof(uint)];
        Span<byte> inputSpan = input;
        BinaryPrimitives.WriteUInt32BigEndian(inputSpan, 1);
        label.CopyTo(inputSpan[sizeof(uint)..]);
        int separatorIndex = sizeof(uint) + label.Length;
        inputSpan[separatorIndex] = 0;
        context.CopyTo(inputSpan[(separatorIndex + 1)..]);
        BinaryPrimitives.WriteUInt32BigEndian(inputSpan[^sizeof(uint)..], (uint)lengthBits);

        byte[] mac = HMACSHA256.HashData(key, inputSpan);
        int outputLength = lengthBits / 8;
        if (outputLength > mac.Length)
        {
            CryptographicOperations.ZeroMemory(mac);
            throw new ArgumentOutOfRangeException(nameof(lengthBits), lengthBits, "KDF length exceeds one HMAC-SHA256 block.");
        }

        byte[] derived = mac.AsSpan(0, outputLength).ToArray();
        CryptographicOperations.ZeroMemory(mac);
        return derived;
    }

    /// <summary>
    /// Computes the SMB2 signature over a complete header+body with Signature zeroed, per [MS-SMB2] §3.1.4.1.
    /// </summary>
    /// <param name="message">Complete SMB2 message starting at the 64-byte header.</param>
    /// <param name="destination">Destination for the 16-byte signature.</param>
    public void ComputeSignature(ReadOnlySpan<byte> message, Span<byte> destination)
    {
        ValidateMessageAndDestination(message, destination);

        byte[] canonical = message.ToArray();
        canonical.AsSpan(48, SignatureLength).Clear();

        switch (Algorithm)
        {
            case Smb2SigningAlgorithm.HmacSha256:
                ComputeHmacSha256Signature(canonical, destination);
                break;
            case Smb2SigningAlgorithm.AesCmac:
                ComputeAesCmac(_signingKey, canonical, destination);
                break;
            default:
                throw new InvalidOperationException($"Unsupported SMB2 signing algorithm {Algorithm}.");
        }
    }

    /// <summary>
    /// Writes the SMB2 signature into the message Signature field after zeroing that field,
    /// per [MS-SMB2] §3.1.4.1 and §2.2.1.2.
    /// </summary>
    /// <param name="message">Mutable complete SMB2 message starting at the 64-byte header.</param>
    public void Sign(Span<byte> message)
    {
        if (message.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new ArgumentException("SMB2 message too short for a packet header.", nameof(message));
        }

        Span<byte> signature = message.Slice(48, SignatureLength);
        signature.Clear();
        ComputeSignature(message, signature);
    }

    /// <summary>
    /// Verifies the SMB2 Signature field in constant time after computing over the zeroed field,
    /// per [MS-SMB2] §3.1.4.1.
    /// </summary>
    /// <param name="message">Complete SMB2 message starting at the 64-byte header.</param>
    /// <returns><see langword="true" /> when the signature matches.</returns>
    public bool VerifySignature(ReadOnlySpan<byte> message)
    {
        if (message.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new ArgumentException("SMB2 message too short for a packet header.", nameof(message));
        }

        Span<byte> expected = stackalloc byte[SignatureLength];
        ComputeSignature(message, expected);
        return CryptographicOperations.FixedTimeEquals(expected, message.Slice(48, SignatureLength));
    }

    /// <summary>
    /// Computes AES-128-CMAC per NIST SP800-38B for SMB 3.x signing in [MS-SMB2] §3.1.5.1.
    /// </summary>
    /// <param name="key">The 16-byte AES key.</param>
    /// <param name="data">Data to authenticate.</param>
    /// <param name="destination">Destination for the 16-byte CMAC tag.</param>
    public static void ComputeAesCmac(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data, Span<byte> destination)
    {
        if (key.Length != AesBlockLength)
        {
            throw new ArgumentException("AES-128-CMAC requires a 16-byte key.", nameof(key));
        }
        if (destination.Length < SignatureLength)
        {
            throw new ArgumentException("Destination must hold a 16-byte CMAC tag.", nameof(destination));
        }

        using Aes aes = Aes.Create();
        aes.Key = key.ToArray();

        Span<byte> zero = stackalloc byte[AesBlockLength];
        zero.Clear();
        Span<byte> l = stackalloc byte[AesBlockLength];
        EncryptAesBlock(aes, zero, l);

        Span<byte> k1 = stackalloc byte[AesBlockLength];
        Span<byte> k2 = stackalloc byte[AesBlockLength];
        DoubleCmacSubkey(l, k1);
        DoubleCmacSubkey(k1, k2);

        int blockCount = data.IsEmpty ? 1 : ((data.Length + AesBlockLength - 1) / AesBlockLength);
        bool finalBlockComplete = !data.IsEmpty && (data.Length % AesBlockLength) == 0;

        Span<byte> lastBlock = stackalloc byte[AesBlockLength];
        lastBlock.Clear();
        if (finalBlockComplete)
        {
            data.Slice((blockCount - 1) * AesBlockLength, AesBlockLength).CopyTo(lastBlock);
            XorBlock(lastBlock, k1, lastBlock);
        }
        else
        {
            int finalLength = data.Length % AesBlockLength;
            if (finalLength > 0)
            {
                data[^finalLength..].CopyTo(lastBlock);
            }
            lastBlock[finalLength] = 0x80;
            XorBlock(lastBlock, k2, lastBlock);
        }

        Span<byte> x = stackalloc byte[AesBlockLength];
        x.Clear();
        Span<byte> y = stackalloc byte[AesBlockLength];
        for (int i = 0; i < blockCount - 1; i++)
        {
            XorBlock(x, data.Slice(i * AesBlockLength, AesBlockLength), y);
            EncryptAesBlock(aes, y, x);
        }

        XorBlock(x, lastBlock, y);
        EncryptAesBlock(aes, y, destination[..SignatureLength]);
    }

    private static void ValidateMessageAndDestination(ReadOnlySpan<byte> message, Span<byte> destination)
    {
        if (message.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new ArgumentException("SMB2 message too short for a packet header.", nameof(message));
        }
        if (destination.Length < SignatureLength)
        {
            throw new ArgumentException("Destination must hold a 16-byte SMB2 signature.", nameof(destination));
        }
    }

    private void ComputeHmacSha256Signature(ReadOnlySpan<byte> canonicalMessage, Span<byte> destination)
    {
        byte[] hash = HMACSHA256.HashData(_signingKey, canonicalMessage);
        hash.AsSpan(0, SignatureLength).CopyTo(destination);
        CryptographicOperations.ZeroMemory(hash);
    }

    private static void EncryptAesBlock(Aes aes, ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length != AesBlockLength || output.Length < AesBlockLength)
        {
            throw new ArgumentException("AES-CMAC block encryption requires 16-byte blocks.", nameof(input));
        }

#pragma warning disable CA5358 // AES-CMAC uses AES-ECB as the SP800-38B block-cipher primitive, not as an encryption mode.
        if (!aes.TryEncryptEcb(input, output[..AesBlockLength], PaddingMode.None, out int bytesWritten) ||
            bytesWritten != AesBlockLength)
        {
            throw new CryptographicException("AES-CMAC block encryption failed.");
        }
#pragma warning restore CA5358
    }

    private static void DoubleCmacSubkey(ReadOnlySpan<byte> input, Span<byte> output)
    {
        int carry = 0;
        for (int i = AesBlockLength - 1; i >= 0; i--)
        {
            int value = (input[i] << 1) | carry;
            output[i] = (byte)value;
            carry = (input[i] & 0x80) == 0 ? 0 : 1;
        }

        if (carry != 0)
        {
            output[AesBlockLength - 1] ^= AesCmacRb;
        }
    }

    private static void XorBlock(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right, Span<byte> destination)
    {
        for (int i = 0; i < AesBlockLength; i++)
        {
            destination[i] = (byte)(left[i] ^ right[i]);
        }
    }
}
