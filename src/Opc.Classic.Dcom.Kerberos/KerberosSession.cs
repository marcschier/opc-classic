//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Security;
using System.Security.Cryptography;
using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Implements Kerberos GSS-API per-message tokens for RFC 4121 packet protection.
/// </summary>
public sealed class KerberosSession : IKerberosSession
{
    private const int HeaderLength = 16;
    private const ushort MicTokenId = 0x0404;
    private const ushort WrapTokenId = 0x0504;
    private const byte SentByAcceptorFlag = 0x01;
    private const byte SealedFlag = 0x02;
    private const byte AcceptorSubkeyFlag = 0x04;
    private const byte Filler = 0xFF;
    private const int Rc4ChecksumSize = 16;

    private readonly KerberosKey _sessionKey;
    private readonly EncryptionType _etype;
    private readonly bool _isAcceptor;
    private readonly bool _usesAcceptorSubkey;
    private long _sendSequenceNumber;
    private long _expectedPeerSequenceNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosSession" /> class from raw key bytes.
    /// </summary>
    /// <param name="sessionKey">The Kerberos session or sub-session key bytes.</param>
    /// <param name="etype">The encryption type of <paramref name="sessionKey" />.</param>
    /// <param name="initialSequenceNumber">The initial outbound and inbound sequence number.</param>
    /// <param name="isAcceptor">Whether this endpoint is the context acceptor.</param>
    /// <param name="usesAcceptorSubkey">Whether the acceptor AP-REP subkey protects this context.</param>
    public KerberosSession(
        ReadOnlySpan<byte> sessionKey,
        EncryptionType etype,
        long initialSequenceNumber = 0,
        bool isAcceptor = false,
        bool usesAcceptorSubkey = false)
        : this(new KerberosKey(key: sessionKey.ToArray(), etype: etype), etype, initialSequenceNumber, isAcceptor, usesAcceptorSubkey)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosSession" /> class.
    /// </summary>
    /// <param name="sessionKey">The Kerberos session or sub-session key.</param>
    /// <param name="etype">The encryption type of <paramref name="sessionKey" />.</param>
    /// <param name="initialSequenceNumber">The initial outbound and inbound sequence number.</param>
    /// <param name="isAcceptor">Whether this endpoint is the context acceptor.</param>
    /// <param name="usesAcceptorSubkey">Whether the acceptor AP-REP subkey protects this context.</param>
    public KerberosSession(
        KerberosKey sessionKey,
        EncryptionType etype,
        long initialSequenceNumber = 0,
        bool isAcceptor = false,
        bool usesAcceptorSubkey = false)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);
        if (initialSequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialSequenceNumber), "Sequence numbers cannot be negative.");
        }

        _sessionKey = sessionKey;
        _etype = etype;
        _sendSequenceNumber = initialSequenceNumber;
        _expectedPeerSequenceNumber = initialSequenceNumber;
        _isAcceptor = isAcceptor;
        _usesAcceptorSubkey = usesAcceptorSubkey;
        _ = GetChecksumSize();
    }

    /// <inheritdoc />
    public int SequenceNumber => _sendSequenceNumber > int.MaxValue ? int.MaxValue : (int)_sendSequenceNumber;

    /// <inheritdoc />
    public byte[] WrapMessage(ReadOnlySpan<byte> plaintext, bool confidential)
    {
        long sequenceNumber = NextSendSequenceNumber();
        byte flags = CreateFlags(confidential);
        var header = CreateWrapHeader(flags, confidential ? 0 : GetChecksumSize(), rrc: 0, sequenceNumber);

        if (confidential)
        {
            byte[] toEncrypt = new byte[plaintext.Length + HeaderLength];
            plaintext.CopyTo(toEncrypt);
            header.CopyTo(toEncrypt.AsSpan(plaintext.Length));
            byte[] encrypted = Encrypt(toEncrypt, GetSealUsage(flags));
            return Concat(header, encrypted);
        }

        var checksumHeader = (byte[])header.Clone();
        BinaryPrimitives.WriteUInt16BigEndian(checksumHeader.AsSpan(4), 0);
        BinaryPrimitives.WriteUInt16BigEndian(checksumHeader.AsSpan(6), 0);
        byte[] checksum = MakeChecksum(Concat(plaintext, checksumHeader), GetSealUsage(flags));
        return Concat(header, plaintext, checksum);
    }

    /// <inheritdoc />
    public byte[] UnwrapMessage(ReadOnlySpan<byte> wrappedToken, out bool wasConfidential)
    {
        ReadWrapHeader(wrappedToken, out byte flags, out ushort ec, out ushort rrc, out long sequenceNumber);
        ValidateReceiveSequence(sequenceNumber);

        wasConfidential = (flags & SealedFlag) != 0;
        byte[] body = RotateBodyLeft(wrappedToken[HeaderLength..], rrc);
        byte[] plaintext = wasConfidential
            ? UnwrapSealed(wrappedToken, flags, ec, body)
            : UnwrapSigned(wrappedToken, flags, ec, body);

        AcceptReceiveSequence();
        return plaintext;
    }

    private byte[] UnwrapSealed(ReadOnlySpan<byte> wrappedToken, byte flags, ushort ec, ReadOnlySpan<byte> body)
    {
        byte[] decrypted = Decrypt(body.ToArray(), GetSealUsage(flags));
        if (decrypted.Length < HeaderLength + ec)
        {
            throw new InvalidOperationException("RFC 4121 sealed Wrap token decrypted to an invalid length.");
        }

        int payloadLength = decrypted.Length - HeaderLength - ec;
        ReadOnlySpan<byte> encryptedHeader = decrypted.AsSpan(payloadLength + ec, HeaderLength);
        var expectedHeader = wrappedToken[..HeaderLength].ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(expectedHeader.AsSpan(6), 0);
        if (!FixedTimeEquals(encryptedHeader, expectedHeader))
        {
            throw new InvalidOperationException("RFC 4121 sealed Wrap token encrypted header mismatch.");
        }

        return decrypted.AsSpan(0, payloadLength).ToArray();
    }

    private byte[] UnwrapSigned(ReadOnlySpan<byte> wrappedToken, byte flags, ushort ec, ReadOnlySpan<byte> body)
    {
        int checksumSize = ec;
        if (checksumSize <= 0 || body.Length < checksumSize)
        {
            throw new InvalidOperationException("RFC 4121 Wrap token checksum length is invalid.");
        }

        ReadOnlySpan<byte> plaintext = body[..^checksumSize];
        ReadOnlySpan<byte> checksum = body[^checksumSize..];
        var checksumHeader = wrappedToken[..HeaderLength].ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(checksumHeader.AsSpan(4), 0);
        BinaryPrimitives.WriteUInt16BigEndian(checksumHeader.AsSpan(6), 0);
        byte[] expectedChecksum = MakeChecksum(Concat(plaintext, checksumHeader), GetSealUsage(flags));
        if (!FixedTimeEquals(checksum, expectedChecksum))
        {
            throw new InvalidOperationException("RFC 4121 Wrap token checksum mismatch.");
        }

        return plaintext.ToArray();
    }

    private static void ReadWrapHeader(ReadOnlySpan<byte> token, out byte flags, out ushort ec, out ushort rrc, out long sequenceNumber)
    {
        if (token.Length < HeaderLength)
        {
            throw new InvalidOperationException("RFC 4121 Wrap token is shorter than the token header.");
        }

        if (BinaryPrimitives.ReadUInt16BigEndian(token) != WrapTokenId)
        {
            throw new InvalidOperationException("RFC 4121 Wrap token has an invalid TOK_ID.");
        }

        flags = token[2];
        if (token[3] != Filler)
        {
            throw new InvalidOperationException("RFC 4121 Wrap token filler is invalid.");
        }

        ec = BinaryPrimitives.ReadUInt16BigEndian(token[4..]);
        rrc = BinaryPrimitives.ReadUInt16BigEndian(token[6..]);
        sequenceNumber = BinaryPrimitives.ReadInt64BigEndian(token[8..]);
    }

    /// <inheritdoc />
    public byte[] GetMic(ReadOnlySpan<byte> data)
    {
        long sequenceNumber = NextSendSequenceNumber();
        byte flags = CreateFlags(confidential: false);
        var header = CreateMicHeader(flags, sequenceNumber);
        byte[] checksum = MakeChecksum(Concat(data, header), GetSignUsage(flags));
        return Concat(header, checksum);
    }

    /// <inheritdoc />
    public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic)
    {
        if (mic.Length < HeaderLength)
        {
            return false;
        }

        try
        {
            ushort tokenId = BinaryPrimitives.ReadUInt16BigEndian(mic);
            if (tokenId != MicTokenId || mic[3] != Filler || mic[4] != Filler || mic[5] != Filler || mic[6] != Filler || mic[7] != Filler)
            {
                return false;
            }

            byte flags = mic[2];
            if ((flags & SealedFlag) != 0)
            {
                return false;
            }

            long sequenceNumber = BinaryPrimitives.ReadInt64BigEndian(mic[8..]);
            if (!IsExpectedReceiveSequence(sequenceNumber))
            {
                return false;
            }

            ReadOnlySpan<byte> checksum = mic[HeaderLength..];
            byte[] expectedChecksum = MakeChecksum(Concat(data, mic[..HeaderLength]), GetSignUsage(flags));
            bool verified = FixedTimeEquals(checksum, expectedChecksum);
            if (verified)
            {
                AcceptReceiveSequence();
            }

            return verified;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (SecurityException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private byte CreateFlags(bool confidential)
    {
        byte flags = 0;
        if (_isAcceptor)
        {
            flags |= SentByAcceptorFlag;
        }

        if (confidential)
        {
            flags |= SealedFlag;
        }

        if (_usesAcceptorSubkey)
        {
            flags |= AcceptorSubkeyFlag;
        }

        return flags;
    }

    private static byte[] CreateMicHeader(byte flags, long sequenceNumber)
    {
        var header = new byte[HeaderLength];
        BinaryPrimitives.WriteUInt16BigEndian(header, MicTokenId);
        header[2] = (byte)(flags & ~SealedFlag);
        header.AsSpan(3, 5).Fill(Filler);
        BinaryPrimitives.WriteInt64BigEndian(header.AsSpan(8), sequenceNumber);
        return header;
    }

    private static byte[] CreateWrapHeader(byte flags, int ec, ushort rrc, long sequenceNumber)
    {
        if (ec > ushort.MaxValue)
        {
            throw new InvalidOperationException("RFC 4121 EC field exceeds 16-bit range.");
        }

        var header = new byte[HeaderLength];
        BinaryPrimitives.WriteUInt16BigEndian(header, WrapTokenId);
        header[2] = flags;
        header[3] = Filler;
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(4), (ushort)ec);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(6), rrc);
        BinaryPrimitives.WriteInt64BigEndian(header.AsSpan(8), sequenceNumber);
        return header;
    }

    private long NextSendSequenceNumber()
    {
        if (_sendSequenceNumber == long.MaxValue)
        {
            throw new InvalidOperationException("Kerberos GSS-API sequence number exhausted.");
        }

        long sequenceNumber = _sendSequenceNumber;
        _sendSequenceNumber++;
        return sequenceNumber;
    }

    private void ValidateReceiveSequence(long sequenceNumber)
    {
        if (!IsExpectedReceiveSequence(sequenceNumber))
        {
            throw new InvalidOperationException("Kerberos GSS-API token sequence number is out of order.");
        }
    }

    private bool IsExpectedReceiveSequence(long sequenceNumber) => sequenceNumber == _expectedPeerSequenceNumber;

    private void AcceptReceiveSequence()
    {
        if (_expectedPeerSequenceNumber == long.MaxValue)
        {
            throw new InvalidOperationException("Kerberos GSS-API receive sequence number exhausted.");
        }

        _expectedPeerSequenceNumber++;
    }

    private static KeyUsage GetSealUsage(byte flags) => (flags & SentByAcceptorFlag) != 0 ? KeyUsage.AcceptorSeal : KeyUsage.InitiatorSeal;

    private static KeyUsage GetSignUsage(byte flags) => (flags & SentByAcceptorFlag) != 0 ? KeyUsage.AcceptorSign : KeyUsage.InitiatorSign;

    private int GetChecksumSize()
    {
        if (IsRc4Hmac())
        {
            return Rc4ChecksumSize;
        }

        KerberosCryptoTransformer transformer = GetTransformer();
        return transformer.ChecksumSize;
    }

    private byte[] Encrypt(ReadOnlyMemory<byte> data, KeyUsage usage)
    {
        if (IsRc4Hmac())
        {
            return Rfc4757Encrypt(data.Span, usage);
        }

        KerberosCryptoTransformer transformer = GetTransformer();
        return transformer.Encrypt(data, _sessionKey, usage).ToArray();
    }

    private byte[] Decrypt(ReadOnlyMemory<byte> data, KeyUsage usage)
    {
        if (IsRc4Hmac())
        {
            return Rfc4757Decrypt(data.Span, usage);
        }

        KerberosCryptoTransformer transformer = GetTransformer();
        return transformer.Decrypt(data, _sessionKey, usage).ToArray();
    }

    private byte[] MakeChecksum(ReadOnlyMemory<byte> data, KeyUsage usage)
    {
        if (IsRc4Hmac())
        {
            return Rfc4757MakeChecksum(data.Span, usage);
        }

        KerberosCryptoTransformer transformer = GetTransformer();
        return transformer.MakeChecksum(data, _sessionKey, usage, KeyDerivationMode.Kc, transformer.ChecksumSize).ToArray();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Maintainability", "CA1508:Avoid dead conditional code",
        Justification = "Defensive null guard; the Kerberos.NET CryptoService.CreateTransform contract is not annotated [return: NotNull] and may return null for unknown etypes added in future versions.")]
    private KerberosCryptoTransformer GetTransformer() =>
        CryptoService.CreateTransform(_etype) ?? throw new NotSupportedException($"Kerberos encryption type {_etype} is not supported for GSS-API packet protection.");

    private bool IsRc4Hmac() => _etype is EncryptionType.RC4_HMAC_NT or EncryptionType.RC4_HMAC_NT_EXP;

#pragma warning disable CA5350, CA5351 // RFC 4757 requires MD5/HMAC-MD5 for RC4-HMAC compatibility.
    private byte[] Rfc4757Encrypt(ReadOnlySpan<byte> data, KeyUsage usage)
    {
        byte[] key = GetRawKey();
        byte[] k2 = HMACMD5.HashData(key, GetRc4Salt(usage));
        byte[] plaintext = new byte[8 + data.Length];
        RandomNumberGenerator.Fill(plaintext.AsSpan(0, 8));
        data.CopyTo(plaintext.AsSpan(8));

        byte[] checksum = HMACMD5.HashData(k2, plaintext);
        byte[] k3 = HMACMD5.HashData(k2, checksum);
        byte[] encrypted = Rc4Transform(k3, plaintext);
        return Concat(checksum, encrypted);
    }

    private byte[] Rfc4757Decrypt(ReadOnlySpan<byte> data, KeyUsage usage)
    {
        if (data.Length < Rc4ChecksumSize + 8)
        {
            throw new InvalidOperationException("RFC 4757 ciphertext is shorter than the checksum and confounder.");
        }

        ReadOnlySpan<byte> checksum = data[..Rc4ChecksumSize];
        ReadOnlySpan<byte> ciphertext = data[Rc4ChecksumSize..];
        byte[] key = GetRawKey();
        byte[] k2 = HMACMD5.HashData(key, GetRc4Salt(usage));
        byte[] k3 = HMACMD5.HashData(k2, checksum);
        byte[] plaintext = Rc4Transform(k3, ciphertext);
        byte[] expectedChecksum = HMACMD5.HashData(k2, plaintext);
        if (!FixedTimeEquals(checksum, expectedChecksum))
        {
            throw new SecurityException("Invalid RFC 4757 checksum.");
        }

        return plaintext.AsSpan(8).ToArray();
    }

    private byte[] Rfc4757MakeChecksum(ReadOnlySpan<byte> data, KeyUsage usage)
    {
        byte[] key = GetRawKey();
        byte[] ksign = HMACMD5.HashData(key, "signaturekey\0"u8);
        byte[] checksumInput = new byte[sizeof(int) + data.Length];
        BinaryPrimitives.WriteInt32LittleEndian(checksumInput, (int)usage);
        data.CopyTo(checksumInput.AsSpan(sizeof(int)));
        byte[] digest = MD5.HashData(checksumInput);
        return HMACMD5.HashData(ksign, digest);
    }
#pragma warning restore CA5350, CA5351

    private byte[] GetRawKey()
    {
        ReadOnlyMemory<byte> key = _sessionKey.GetKey();
        if (key.Length != 16)
        {
            throw new NotSupportedException("RC4-HMAC Kerberos packet protection requires a 128-bit session key.");
        }

        return key.ToArray();
    }

    private static byte[] GetRc4Salt(KeyUsage usage)
    {
        int saltValue = (int)usage;
        if (saltValue == 3)
        {
            saltValue = 8;
        }
        else if (saltValue == 23)
        {
            saltValue = 13;
        }

        var salt = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(salt, saltValue);
        return salt;
    }

    private static byte[] Rc4Transform(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input)
    {
        var cipher = new Rc4(key);
        var output = new byte[input.Length];
        cipher.Process(input, output);
        return output;
    }

    private static byte[] RotateBodyLeft(ReadOnlySpan<byte> body, ushort rrc)
    {
        if (body.IsEmpty || rrc == 0)
        {
            return body.ToArray();
        }

        int count = rrc % body.Length;
        if (count == 0)
        {
            return body.ToArray();
        }

        var output = new byte[body.Length];
        body[count..].CopyTo(output);
        body[..count].CopyTo(output.AsSpan(body.Length - count));
        return output;
    }

    private static byte[] Concat(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
    {
        var output = new byte[first.Length + second.Length];
        first.CopyTo(output);
        second.CopyTo(output.AsSpan(first.Length));
        return output;
    }

    private static byte[] Concat(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second, ReadOnlySpan<byte> third)
    {
        var output = new byte[first.Length + second.Length + third.Length];
        first.CopyTo(output);
        second.CopyTo(output.AsSpan(first.Length));
        third.CopyTo(output.AsSpan(first.Length + second.Length));
        return output;
    }

    private static bool FixedTimeEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private sealed class Rc4
    {
        private readonly byte[] _state = new byte[256];
        private byte _i;
        private byte _j;

        public Rc4(ReadOnlySpan<byte> key)
        {
            if (key.IsEmpty)
            {
                throw new ArgumentException("RC4 key cannot be empty.", nameof(key));
            }

            for (int n = 0; n < _state.Length; n++)
            {
                _state[n] = (byte)n;
            }

            byte j = 0;
            for (int n = 0; n < _state.Length; n++)
            {
                j = (byte)(j + _state[n] + key[n % key.Length]);
                (_state[n], _state[j]) = (_state[j], _state[n]);
            }
        }

        public void Process(ReadOnlySpan<byte> input, Span<byte> output)
        {
            if (output.Length < input.Length)
            {
                throw new ArgumentException("RC4 output is shorter than input.", nameof(output));
            }

            for (int k = 0; k < input.Length; k++)
            {
                _i = (byte)(_i + 1);
                _j = (byte)(_j + _state[_i]);
                (_state[_i], _state[_j]) = (_state[_j], _state[_i]);
                byte keyStream = _state[(byte)(_state[_i] + _state[_j])];
                output[k] = (byte)(input[k] ^ keyStream);
            }
        }
    }
}
