//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Smb;

/// <summary>SMB 3.x encryption algorithms; see [MS-SMB2] §2.2.3.1.2 and §3.1.4.3.</summary>
public enum Smb2EncryptionAlgorithm
{
    /// <summary>AES-128-CCM with an 11-byte nonce and a 16-byte tag.</summary>
    AesCcm,

    /// <summary>AES-128-GCM with a 12-byte nonce and a 16-byte tag.</summary>
    AesGcm,
}

/// <summary>
/// Encrypts and decrypts SMB 3.x transform messages using AES-128-CCM or AES-128-GCM;
/// see [MS-SMB2] §3.1.4.3 and §3.2.5.1.1.1.
/// </summary>
public sealed class Smb2Crypter
{
    /// <summary>Length of SMB 3.x AES-128 encryption keys in bytes; see [MS-SMB2] §3.1.4.2.</summary>
    public const int KeyLength = 16;

    /// <summary>Length of the SMB2 transform Signature/authentication tag in bytes; see [MS-SMB2] §2.2.41.</summary>
    public const int AuthenticationTagLength = 16;

    /// <summary>Length of the AES-CCM nonce carried in TRANSFORM_HEADER.Nonce; see [MS-SMB2] §2.2.41.</summary>
    public const int AesCcmNonceLength = 11;

    /// <summary>Length of the AES-GCM nonce carried in TRANSFORM_HEADER.Nonce; see [MS-SMB2] §2.2.41.</summary>
    public const int AesGcmNonceLength = 12;

    private const int SmbEncryptionKeyLengthBits = 128;

    private readonly byte[] _key;

    /// <summary>Initializes an SMB3 crypter with a derived encryption or decryption key; see [MS-SMB2] §3.1.4.3.</summary>
    public Smb2Crypter(ReadOnlySpan<byte> key, Smb2EncryptionAlgorithm algorithm)
    {
        if (key.Length != KeyLength)
        {
            throw new ArgumentException("SMB3 AES-128 encryption keys must be 16 bytes.", nameof(key));
        }

        Algorithm = algorithm;
        _key = key.ToArray();
    }

    /// <summary>Gets the negotiated SMB3 encryption algorithm; see [MS-SMB2] §2.2.3.1.2.</summary>
    public Smb2EncryptionAlgorithm Algorithm { get; }

    /// <summary>Gets the algorithm nonce length used as AEAD input; see [MS-SMB2] §2.2.41.</summary>
    public int NonceLength => GetNonceLength(Algorithm);

    /// <summary>
    /// Derives the client-to-server Session.EncryptionKey from Session.SessionKey; see [MS-SMB2] §3.2.5.3.1 and §3.1.4.2.
    /// </summary>
    public static byte[] DeriveSmb3ClientEncryptionKey(
        Smb2Dialect dialect,
        ReadOnlySpan<byte> sessionKey,
        ReadOnlySpan<byte> preauthIntegrityHash = default) =>
        DeriveSmb3ClientCipherKey(dialect, sessionKey, preauthIntegrityHash, clientToServer: true);

    /// <summary>
    /// Derives the server-to-client Session.DecryptionKey from Session.SessionKey; see [MS-SMB2] §3.2.5.3.1 and §3.1.4.2.
    /// </summary>
    public static byte[] DeriveSmb3ClientDecryptionKey(
        Smb2Dialect dialect,
        ReadOnlySpan<byte> sessionKey,
        ReadOnlySpan<byte> preauthIntegrityHash = default) =>
        DeriveSmb3ClientCipherKey(dialect, sessionKey, preauthIntegrityHash, clientToServer: false);

    /// <summary>
    /// Encrypts a complete SMB2 header+body into TRANSFORM_HEADER+ciphertext; the 16-byte AEAD tag is stored in Signature.
    /// See [MS-SMB2] §3.1.4.3.
    /// </summary>
    public byte[] EncryptMessage(ReadOnlySpan<byte> plaintextMessage, ReadOnlySpan<byte> nonce, ulong sessionId)
    {
        ValidatePlaintextMessage(plaintextMessage);
        ValidateNonce(nonce, Algorithm);
        if (sessionId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sessionId), sessionId, "Encrypted SMB3 messages require a nonzero SessionId.");
        }

        byte[] encryptedMessage = new byte[Smb2TransformHeader.Size + plaintextMessage.Length];
        byte[] transformNonce = new byte[Smb2TransformHeader.NonceLength];
        nonce.CopyTo(transformNonce);

        var header = new Smb2TransformHeader(
            Signature: ReadOnlyMemory<byte>.Empty,
            Nonce: transformNonce,
            OriginalMessageSize: (uint)plaintextMessage.Length,
            Reserved: 0,
            Flags: Smb2Constants.TransformFlagsEncrypted,
            SessionId: sessionId);
        header.Write(encryptedMessage);

        Span<byte> ciphertext = encryptedMessage.AsSpan(Smb2TransformHeader.Size, plaintextMessage.Length);
        Span<byte> tag = encryptedMessage.AsSpan(4, AuthenticationTagLength);
        EncryptCore(nonce, plaintextMessage, ciphertext, tag, Smb2TransformHeader.GetAssociatedData(encryptedMessage));
        return encryptedMessage;
    }

    /// <summary>
    /// Decrypts a TRANSFORM_HEADER+ciphertext message and verifies the Signature tag; see [MS-SMB2] §3.2.5.1.1.1.
    /// </summary>
    public byte[] DecryptMessage(ReadOnlySpan<byte> encryptedMessage, ulong expectedSessionId = 0)
    {
        if (encryptedMessage.Length <= Smb2TransformHeader.Size)
        {
            throw new Smb2ProtocolException("SMB2 transform message does not contain ciphertext.");
        }

        var header = Smb2TransformHeader.Read(encryptedMessage);
        if (header.Flags != Smb2Constants.TransformFlagsEncrypted)
        {
            throw new Smb2ProtocolException("SMB2 TRANSFORM_HEADER Flags/EncryptionAlgorithm must be 0x0001.");
        }
        if (expectedSessionId != 0 && header.SessionId != expectedSessionId)
        {
            throw new Smb2ProtocolException("SMB2 TRANSFORM_HEADER SessionId did not match the expected session.");
        }

        int ciphertextLength = encryptedMessage.Length - Smb2TransformHeader.Size;
        if (header.OriginalMessageSize != ciphertextLength)
        {
            throw new Smb2ProtocolException("SMB2 TRANSFORM_HEADER OriginalMessageSize did not match ciphertext length.");
        }

        byte[] plaintextMessage = new byte[ciphertextLength];
        ReadOnlySpan<byte> ciphertext = encryptedMessage[Smb2TransformHeader.Size..];
        ReadOnlySpan<byte> tag = header.Signature.Span;
        ReadOnlySpan<byte> nonce = header.Nonce.Span[..NonceLength];
        try
        {
            DecryptCore(nonce, ciphertext, tag, plaintextMessage, Smb2TransformHeader.GetAssociatedData(encryptedMessage));
        }
        catch (CryptographicException ex)
        {
            throw new Smb2ProtocolException("SMB2 transform authentication failed.", ex);
        }

        ValidatePlaintextMessage(plaintextMessage);
        return plaintextMessage;
    }

    internal static bool TryGetAlgorithmForCipherId(ushort cipherId, out Smb2EncryptionAlgorithm algorithm)
    {
        switch (cipherId)
        {
            case Smb2Constants.EncryptionCipherAes128Ccm:
                algorithm = Smb2EncryptionAlgorithm.AesCcm;
                return true;
            case Smb2Constants.EncryptionCipherAes128Gcm:
                algorithm = Smb2EncryptionAlgorithm.AesGcm;
                return true;
            default:
                algorithm = default;
                return false;
        }
    }

    internal static Smb2EncryptionAlgorithm GetDefaultAlgorithmForDialect(Smb2Dialect dialect) => dialect switch
    {
        Smb2Dialect.Smb300 or Smb2Dialect.Smb302 or Smb2Dialect.Smb311 => Smb2EncryptionAlgorithm.AesCcm,
        _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "SMB3 encryption requires an SMB 3.x dialect."),
    };

    internal static void ValidateDialectAlgorithm(Smb2Dialect dialect, Smb2EncryptionAlgorithm algorithm)
    {
        switch (dialect)
        {
            case Smb2Dialect.Smb300 or Smb2Dialect.Smb302 when algorithm != Smb2EncryptionAlgorithm.AesCcm:
                throw new ArgumentException("SMB 3.0 and 3.0.2 support only AES-128-CCM encryption.", nameof(algorithm));
            case Smb2Dialect.Smb300 or Smb2Dialect.Smb302 or Smb2Dialect.Smb311:
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "SMB3 encryption requires an SMB 3.x dialect.");
        }
    }

    private static byte[] DeriveSmb3ClientCipherKey(
        Smb2Dialect dialect,
        ReadOnlySpan<byte> sessionKey,
        ReadOnlySpan<byte> preauthIntegrityHash,
        bool clientToServer)
    {
        return dialect switch
        {
            Smb2Dialect.Smb300 or Smb2Dialect.Smb302 => Smb2Signer.DeriveKeyCounterMode(
                sessionKey,
                "SMB2AESCCM"u8,
                clientToServer ? "ServerIn \0"u8 : "ServerOut\0"u8,
                SmbEncryptionKeyLengthBits),
            Smb2Dialect.Smb311 => preauthIntegrityHash.IsEmpty
                ? throw new ArgumentException(
                    "SMB 3.1.1 encryption key derivation requires the PreauthIntegrityHashValue.",
                    nameof(preauthIntegrityHash))
                : Smb2Signer.DeriveKeyCounterMode(
                    sessionKey,
                    clientToServer ? "SMBC2SCipherKey"u8 : "SMBS2CCipherKey"u8,
                    preauthIntegrityHash,
                    SmbEncryptionKeyLengthBits),
            _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "SMB3 encryption key derivation requires an SMB 3.x dialect."),
        };
    }

    private static int GetNonceLength(Smb2EncryptionAlgorithm algorithm) => algorithm switch
    {
        Smb2EncryptionAlgorithm.AesCcm => AesCcmNonceLength,
        Smb2EncryptionAlgorithm.AesGcm => AesGcmNonceLength,
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Unsupported SMB3 encryption algorithm."),
    };

    private static void ValidateNonce(ReadOnlySpan<byte> nonce, Smb2EncryptionAlgorithm algorithm)
    {
        int expectedLength = GetNonceLength(algorithm);
        if (nonce.Length != expectedLength)
        {
            throw new ArgumentException($"SMB3 {algorithm} encryption requires a {expectedLength}-byte nonce.", nameof(nonce));
        }
    }

    private static void ValidatePlaintextMessage(ReadOnlySpan<byte> plaintextMessage)
    {
        if (plaintextMessage.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new Smb2ProtocolException("SMB2 plaintext message too short for a packet header.");
        }
        if (Smb2TransformHeader.HasTransformProtocolId(plaintextMessage))
        {
            throw new Smb2ProtocolException("Nested SMB2 transform messages are not allowed.");
        }
        if (!plaintextMessage[..4].SequenceEqual(Smb2Constants.ProtocolId))
        {
            throw new Smb2ProtocolException("SMB2 plaintext message ProtocolId was invalid after decryption.");
        }
    }

    private void EncryptCore(
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> plaintext,
        Span<byte> ciphertext,
        Span<byte> tag,
        ReadOnlySpan<byte> associatedData)
    {
        switch (Algorithm)
        {
            case Smb2EncryptionAlgorithm.AesCcm:
                using (var aes = new AesCcm(_key))
                {
                    aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
                }
                break;
            case Smb2EncryptionAlgorithm.AesGcm:
                using (var aes = new AesGcm(_key, AuthenticationTagLength))
                {
                    aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
                }
                break;
            default:
                throw new InvalidOperationException($"Unsupported SMB3 encryption algorithm {Algorithm}.");
        }
    }

    private void DecryptCore(
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> ciphertext,
        ReadOnlySpan<byte> tag,
        Span<byte> plaintext,
        ReadOnlySpan<byte> associatedData)
    {
        switch (Algorithm)
        {
            case Smb2EncryptionAlgorithm.AesCcm:
                using (var aes = new AesCcm(_key))
                {
                    aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
                }
                break;
            case Smb2EncryptionAlgorithm.AesGcm:
                using (var aes = new AesGcm(_key, AuthenticationTagLength))
                {
                    aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
                }
                break;
            default:
                throw new InvalidOperationException($"Unsupported SMB3 encryption algorithm {Algorithm}.");
        }
    }
}
