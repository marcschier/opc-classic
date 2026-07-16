// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

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
    private const int RpcAesExtraCount = 16;
    private const int Rc4MicInnerLength = 24;
    private const int Rc4WrapInnerLength = 32;
    private static readonly byte[] KerberosMechanismOid =
        [0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x12, 0x01, 0x02, 0x02];
    private static readonly byte[] ZeroAesInitializationVector = new byte[16];

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
    /// Initializes a new instance with independently negotiated directional sequence numbers.
    /// </summary>
    public KerberosSession(
        ReadOnlySpan<byte> sessionKey,
        EncryptionType etype,
        long initialSendSequenceNumber,
        long initialReceiveSequenceNumber,
        bool isAcceptor,
        bool usesAcceptorSubkey)
        : this(
            new KerberosKey(key: sessionKey.ToArray(), etype: etype),
            etype,
            initialSendSequenceNumber,
            initialReceiveSequenceNumber,
            isAcceptor,
            usesAcceptorSubkey)
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
        : this(
            sessionKey,
            etype,
            initialSequenceNumber,
            initialSequenceNumber,
            isAcceptor,
            usesAcceptorSubkey)
    {
    }

    /// <summary>
    /// Initializes a new instance with independently negotiated directional sequence numbers.
    /// </summary>
    public KerberosSession(
        KerberosKey sessionKey,
        EncryptionType etype,
        long initialSendSequenceNumber,
        long initialReceiveSequenceNumber,
        bool isAcceptor,
        bool usesAcceptorSubkey)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);
        if (initialSendSequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialSendSequenceNumber),
                "Sequence numbers cannot be negative.");
        }
        if (initialReceiveSequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialReceiveSequenceNumber),
                "Sequence numbers cannot be negative.");
        }

        _sessionKey = sessionKey;
        _etype = etype;
        _sendSequenceNumber = initialSendSequenceNumber;
        _expectedPeerSequenceNumber = initialReceiveSequenceNumber;
        _isAcceptor = isAcceptor;
        _usesAcceptorSubkey = usesAcceptorSubkey;
        _ = GetChecksumSize();
    }

    /// <inheritdoc />
    public int SequenceNumber => _sendSequenceNumber > int.MaxValue ? int.MaxValue : (int)_sendSequenceNumber;

    /// <inheritdoc />
    public int GetRpcVerifierLength(bool confidential)
    {
        if (IsRc4Hmac())
        {
            return GetInitialContextTokenLength(
                confidential ? Rc4WrapInnerLength : Rc4MicInnerLength);
        }

        int checksumSize = GetChecksumSize();
        return confidential
            ? checked(HeaderLength + (3 * HeaderLength) + checksumSize)
            : checked(HeaderLength + checksumSize);
    }

    /// <inheritdoc />
    public byte[] ProtectRpcMessage(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        bool confidential)
    {
        ValidateRpcSegments(signedRegion, confidentialOffset, confidentialLength);
        return IsRc4Hmac()
            ? ProtectRc4RpcMessage(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                confidential)
            : confidential
                ? WrapAesRpcMessage(
                    signedRegion,
                    confidentialOffset,
                    confidentialLength)
                : GetMic(signedRegion);
    }

    /// <inheritdoc />
    public void UnprotectRpcMessage(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> verifier,
        bool confidential)
    {
        ValidateRpcSegments(signedRegion, confidentialOffset, confidentialLength);
        if (IsRc4Hmac())
        {
            UnprotectRc4RpcMessage(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                verifier,
                confidential);
            return;
        }

        if (confidential)
        {
            UnwrapAesRpcMessage(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                verifier);
            return;
        }

        if (!VerifyMic(signedRegion, verifier))
        {
            throw new SecurityException("Kerberos GSS_GetMICEx verifier validation failed.");
        }
    }

    /// <summary>
    /// Gets the encoded Kerberos GSS Wrap-token length for the supplied plaintext length.
    /// </summary>
    public int GetWrapTokenLength(int plaintextLength, bool confidential)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(plaintextLength);
        if (IsRc4Hmac())
        {
            return checked(
                GetInitialContextTokenLength(
                    Rc4WrapInnerLength,
                    plaintextLength + 1)
                + plaintextLength
                + 1);
        }
        if (!confidential)
        {
            return checked(HeaderLength + plaintextLength + GetChecksumSize());
        }

        KerberosCryptoTransformer transformer = GetTransformer();
        return checked(
            HeaderLength
            + transformer.BlockSize
            + plaintextLength
            + HeaderLength
            + transformer.ChecksumSize);
    }

    /// <inheritdoc />
    public byte[] WrapMessage(ReadOnlySpan<byte> plaintext, bool confidential)
    {
        if (IsRc4Hmac())
        {
            return WrapRc4Message(plaintext, confidential);
        }

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
        if (IsRc4Hmac() && !wrappedToken.IsEmpty && wrappedToken[0] == 0x60)
        {
            return UnwrapRc4Message(wrappedToken, out wasConfidential);
        }

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

    private void ReadWrapHeader(ReadOnlySpan<byte> token, out byte flags, out ushort ec, out ushort rrc, out long sequenceNumber)
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
        if (!HasExpectedPeerFlags(flags))
        {
            throw new InvalidOperationException(
                "RFC 4121 Wrap token direction or acceptor-subkey flag is invalid.");
        }
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
        if (IsRc4Hmac())
        {
            byte[] rpcBuffer = data.ToArray();
            return ProtectRc4RpcMessage(
                rpcBuffer,
                confidentialOffset: 0,
                confidentialLength: rpcBuffer.Length,
                confidential: false);
        }

        long sequenceNumber = NextSendSequenceNumber();
        byte flags = CreateFlags(confidential: false);
        var header = CreateMicHeader(flags, sequenceNumber);
        byte[] checksum = MakeChecksum(Concat(data, header), GetSignUsage(flags));
        return Concat(header, checksum);
    }

    /// <inheritdoc />
    public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic) =>
        IsRc4Hmac()
            ? VerifyRc4Mic(data, mic)
            : VerifyAesMic(data, mic);

    private bool VerifyRc4Mic(
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> mic)
    {
        try
        {
            byte[] rpcBuffer = data.ToArray();
            UnprotectRc4RpcMessage(
                rpcBuffer,
                confidentialOffset: 0,
                confidentialLength: rpcBuffer.Length,
                mic,
                confidential: false);
            return true;
        }
        catch (Exception exception) when (
            exception is ArgumentException
                or CryptographicException
                or InvalidOperationException
                or SecurityException)
        {
            return false;
        }
    }

    private bool VerifyAesMic(
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> mic)
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
            if ((flags & SealedFlag) != 0
                || !HasExpectedPeerFlags(flags))
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

    private byte[] WrapAesRpcMessage(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength)
    {
        long sequenceNumber = NextSendSequenceNumber();
        byte flags = CreateFlags(confidential: true);
        int checksumSize = GetChecksumSize();
        ushort rrc = checked((ushort)(HeaderLength + checksumSize));
        byte[] wireHeader = CreateWrapHeader(
            flags,
            RpcAesExtraCount,
            rrc,
            sequenceNumber);
        byte[] cryptoHeader = (byte[])wireHeader.Clone();
        BinaryPrimitives.WriteUInt16BigEndian(cryptoHeader.AsSpan(6), 0);

        byte[] confounder = RandomNumberGenerator.GetBytes(HeaderLength);
        byte[] filler = new byte[RpcAesExtraCount];
        filler.AsSpan().Fill(Filler);
        ReadOnlySpan<byte> confidential = signedRegion.Slice(
            confidentialOffset,
            confidentialLength);
        byte[] cleartext = Concat(
            confounder,
            confidential,
            filler,
            cryptoHeader);
        byte[] encrypted = AesCtsEncrypt(
            cleartext,
            GetAesEncryptionKey(GetSealUsage(flags)),
            ZeroAesInitializationVector);
        byte[] checksum = IsAesSha2()
            ? MakeAesSha2RpcChecksum(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                encrypted,
                GetSealUsage(flags))
            : MakeAesSha1RpcChecksum(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                confounder,
                confidential,
                filler,
                cryptoHeader,
                GetSealUsage(flags));
        byte[] rotated = RotateRight(
            Concat(encrypted, checksum),
            rrc + RpcAesExtraCount);
        int verifierDataLength = rotated.Length - confidentialLength;
        rotated.AsSpan(verifierDataLength).CopyTo(
            signedRegion.Slice(confidentialOffset, confidentialLength));
        return Concat(wireHeader, rotated.AsSpan(0, verifierDataLength));
    }

    private void UnwrapAesRpcMessage(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> verifier)
    {
        int checksumSize = GetChecksumSize();
        KeyUsage usage = ReadAesRpcHeader(
            verifier,
            checksumSize,
            out ushort ec,
            out ushort rrc,
            out long sequenceNumber);
        ValidateReceiveSequence(sequenceNumber);

        byte[] rotated = Concat(
            verifier[HeaderLength..],
            signedRegion.Slice(confidentialOffset, confidentialLength));
        byte[] tokenBody = RotateLeft(rotated, rrc + ec);
        int encryptedLength = tokenBody.Length - checksumSize;
        int expectedEncryptedLength =
            HeaderLength + confidentialLength + RpcAesExtraCount + HeaderLength;
        if (encryptedLength != expectedEncryptedLength)
        {
            throw new InvalidOperationException(
                "Kerberos GSS_WrapEx ciphertext length is invalid.");
        }

        ReadOnlySpan<byte> encrypted = tokenBody.AsSpan(0, encryptedLength);
        ReadOnlySpan<byte> checksum = tokenBody.AsSpan(encryptedLength);
        if (IsAesSha2())
        {
            VerifyAesSha2RpcChecksum(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                encrypted,
                checksum,
                usage);
        }

        byte[] plaintext = DecryptAndValidateAesRpcBody(
            signedRegion,
            confidentialOffset,
            confidentialLength,
            encrypted,
            checksum,
            verifier[..HeaderLength],
            expectedEncryptedLength,
            usage);
        plaintext.CopyTo(
            signedRegion.Slice(confidentialOffset, confidentialLength));
        AcceptReceiveSequence();
    }

    private KeyUsage ReadAesRpcHeader(
        ReadOnlySpan<byte> verifier,
        int checksumSize,
        out ushort ec,
        out ushort rrc,
        out long sequenceNumber)
    {
        if (verifier.Length != GetRpcVerifierLength(confidential: true)
            || BinaryPrimitives.ReadUInt16BigEndian(verifier) != WrapTokenId
            || verifier[3] != Filler)
        {
            throw new InvalidOperationException(
                "Kerberos GSS_WrapEx verifier has an invalid length or header.");
        }

        byte flags = verifier[2];
        if ((flags & SealedFlag) == 0 || !HasExpectedPeerFlags(flags))
        {
            throw new InvalidOperationException(
                "Kerberos GSS_WrapEx verifier direction, sealing, or subkey flags are invalid.");
        }

        ec = BinaryPrimitives.ReadUInt16BigEndian(verifier[4..]);
        rrc = BinaryPrimitives.ReadUInt16BigEndian(verifier[6..]);
        if (ec != RpcAesExtraCount || rrc != HeaderLength + checksumSize)
        {
            throw new InvalidOperationException(
                "Kerberos GSS_WrapEx verifier has invalid EC or RRC framing.");
        }

        sequenceNumber = BinaryPrimitives.ReadInt64BigEndian(verifier[8..]);
        return GetSealUsage(flags);
    }

    private void VerifyAesSha2RpcChecksum(
        ReadOnlySpan<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> encrypted,
        ReadOnlySpan<byte> checksum,
        KeyUsage usage)
    {
        byte[] expected = MakeAesSha2RpcChecksum(
            signedRegion,
            confidentialOffset,
            confidentialLength,
            encrypted,
            usage);
        if (!FixedTimeEquals(checksum, expected))
        {
            throw new SecurityException(
                "Kerberos GSS_WrapEx checksum validation failed.");
        }
    }

    private byte[] DecryptAndValidateAesRpcBody(
        ReadOnlySpan<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> encrypted,
        ReadOnlySpan<byte> checksum,
        ReadOnlySpan<byte> verifierHeader,
        int expectedEncryptedLength,
        KeyUsage usage)
    {
        byte[] cleartext = AesCtsDecrypt(
            encrypted,
            GetAesEncryptionKey(usage),
            ZeroAesInitializationVector);
        if (cleartext.Length != expectedEncryptedLength)
        {
            throw new InvalidOperationException(
                "Kerberos GSS_WrapEx plaintext length is invalid.");
        }

        ReadOnlySpan<byte> confounder = cleartext.AsSpan(0, HeaderLength);
        ReadOnlySpan<byte> plaintext = cleartext.AsSpan(
            HeaderLength,
            confidentialLength);
        ReadOnlySpan<byte> filler = cleartext.AsSpan(
            HeaderLength + confidentialLength,
            RpcAesExtraCount);
        ReadOnlySpan<byte> encryptedHeader = cleartext.AsSpan(
            HeaderLength + confidentialLength + RpcAesExtraCount,
            HeaderLength);
        byte[] expectedHeader = verifierHeader.ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(expectedHeader.AsSpan(6), 0);
        if (!FixedTimeEquals(encryptedHeader, expectedHeader))
        {
            throw new SecurityException(
                "Kerberos GSS_WrapEx encrypted header validation failed.");
        }

        if (!IsAesSha2())
        {
            byte[] expected = MakeAesSha1RpcChecksum(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                confounder,
                plaintext,
                filler,
                encryptedHeader,
                usage);
            if (!FixedTimeEquals(checksum, expected))
            {
                throw new SecurityException(
                    "Kerberos GSS_WrapEx checksum validation failed.");
            }
        }

        return plaintext.ToArray();
    }

    private byte[] MakeAesSha1RpcChecksum(
        ReadOnlySpan<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> confounder,
        ReadOnlySpan<byte> confidential,
        ReadOnlySpan<byte> filler,
        ReadOnlySpan<byte> cryptoHeader,
        KeyUsage usage)
    {
        byte[] checksumInput = Concat(
            confounder,
            signedRegion[..confidentialOffset],
            confidential,
            signedRegion[(confidentialOffset + confidentialLength)..],
            filler,
            cryptoHeader);
        return MakeChecksum(checksumInput, usage);
    }

    private byte[] MakeAesSha2RpcChecksum(
        ReadOnlySpan<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> encrypted,
        KeyUsage usage)
    {
        ReadOnlySpan<byte> encryptedConfounder = encrypted[..HeaderLength];
        ReadOnlySpan<byte> encryptedConfidential = encrypted.Slice(
            HeaderLength,
            confidentialLength);
        ReadOnlySpan<byte> encryptedTail = encrypted[
            (HeaderLength + confidentialLength)..];
        byte[] checksumInput = Concat(
            ZeroAesInitializationVector,
            encryptedConfounder,
            signedRegion[..confidentialOffset],
            encryptedConfidential,
            signedRegion[(confidentialOffset + confidentialLength)..],
            encryptedTail);
        return MakeChecksum(checksumInput, usage);
    }

    private byte[] WrapRc4Message(
        ReadOnlySpan<byte> plaintext,
        bool confidential)
    {
        uint sequence = unchecked((uint)NextSendSequenceNumber());
        byte[] padded = new byte[plaintext.Length + 1];
        plaintext.CopyTo(padded);
        padded[^1] = 0x01;
        byte[] innerWrap = CreateRc4WrapHeader(confidential);
        byte[] confounder = RandomNumberGenerator.GetBytes(8);
        byte[] checksum = Rfc4757MakeChecksum(
            Concat(innerWrap.AsSpan(0, 8), confounder, padded),
            (KeyUsage)13);
        checksum.AsSpan(0, 8).CopyTo(innerWrap.AsSpan(16, 8));

        byte[] outputData = padded.ToArray();
        if (confidential)
        {
            var rc4 = new Rc4(DeriveRc4DataKey(sequence));
            rc4.Process(confounder, innerWrap.AsSpan(24, 8));
            rc4.Process(padded, outputData);
        }
        else
        {
            confounder.CopyTo(innerWrap, 24);
        }

        EncryptRc4Sequence(
            sequence,
            LocalRc4Direction,
            innerWrap.AsSpan(16, 8),
            innerWrap.AsSpan(8, 8));
        byte[] signature = WrapInitialContextToken(
            innerWrap,
            outputData.Length);
        return Concat(signature, outputData);
    }

    private byte[] UnwrapRc4Message(
        ReadOnlySpan<byte> wrappedToken,
        out bool wasConfidential)
    {
        int outerHeaderLength = ReadInitialContextTokenLength(
            wrappedToken,
            out _);
        int signatureLength = checked(
            outerHeaderLength
            + KerberosMechanismOid.Length
            + Rc4WrapInnerLength);
        if (wrappedToken.Length <= signatureLength)
        {
            throw new InvalidOperationException(
                "RFC 4757 Wrap token is missing its padded message data.");
        }

        ReadOnlySpan<byte> signature = wrappedToken[..signatureLength];
        ReadOnlySpan<byte> inputData = wrappedToken[signatureLength..];
        ReadOnlySpan<byte> inner = UnwrapInitialContextToken(
            signature,
            Rc4WrapInnerLength,
            inputData.Length);
        wasConfidential = ReadRc4WrapConfidentiality(inner);
        uint sequence = DecryptAndValidateRc4Sequence(
            inner.Slice(8, 8),
            inner.Slice(16, 8));
        ValidateReceiveSequence(unchecked((uint)sequence));

        byte[] confounder;
        byte[] padded;
        if (wasConfidential)
        {
            var rc4 = new Rc4(DeriveRc4DataKey(sequence));
            confounder = new byte[8];
            padded = new byte[inputData.Length];
            rc4.Process(inner.Slice(24, 8), confounder);
            rc4.Process(inputData, padded);
        }
        else
        {
            confounder = inner.Slice(24, 8).ToArray();
            padded = inputData.ToArray();
        }

        VerifyRc4WrapChecksum(inner, confounder, padded);
        if (padded.Length == 0 || padded[^1] != 0x01)
        {
            throw new SecurityException(
                "RFC 4757 Wrap token padding is invalid.");
        }

        AcceptReceiveSequence();
        return padded.AsSpan(0, padded.Length - 1).ToArray();
    }

    private static byte[] CreateRc4WrapHeader(bool confidential)
    {
        byte[] innerWrap = new byte[Rc4WrapInnerLength];
        innerWrap[0] = 0x02;
        innerWrap[1] = 0x01;
        innerWrap[2] = 0x11;
        innerWrap[3] = 0x00;
        innerWrap[4] = confidential ? (byte)0x10 : Filler;
        innerWrap[5] = confidential ? (byte)0x00 : Filler;
        innerWrap[6] = Filler;
        innerWrap[7] = Filler;
        return innerWrap;
    }

    private static bool ReadRc4WrapConfidentiality(
        ReadOnlySpan<byte> inner)
    {
        if (inner.Length != Rc4WrapInnerLength
            || !inner[..4].SequenceEqual(
                new byte[] { 0x02, 0x01, 0x11, 0x00 })
            || inner[6] != Filler
            || inner[7] != Filler)
        {
            throw new InvalidOperationException(
                "RFC 4757 GSS_WrapEx token header is invalid.");
        }

        if (inner[4] == 0x10 && inner[5] == 0x00)
        {
            return true;
        }
        if (inner[4] == Filler && inner[5] == Filler)
        {
            return false;
        }

        throw new InvalidOperationException(
            "RFC 4757 GSS_WrapEx token sealing algorithm is invalid.");
    }

    private void VerifyRc4WrapChecksum(
        ReadOnlySpan<byte> inner,
        ReadOnlySpan<byte> confounder,
        ReadOnlySpan<byte> padded)
    {
        byte[] expectedChecksum = Rfc4757MakeChecksum(
            Concat(inner[..8], confounder, padded),
            (KeyUsage)13);
        if (!FixedTimeEquals(
                inner.Slice(16, 8),
                expectedChecksum.AsSpan(0, 8)))
        {
            throw new SecurityException(
                "RFC 4757 GSS_WrapEx checksum validation failed.");
        }
    }

    private byte[] ProtectRc4RpcMessage(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        bool confidential)
    {
        long sequenceNumber = NextSendSequenceNumber();
        uint sequence = unchecked((uint)sequenceNumber);
        if (!confidential)
        {
            byte[] innerMic = new byte[Rc4MicInnerLength];
            innerMic[0] = 0x01;
            innerMic[1] = 0x01;
            innerMic[2] = 0x11;
            innerMic[3] = 0x00;
            innerMic.AsSpan(4, 4).Fill(Filler);
            byte[] micChecksum = Rfc4757MakeChecksum(
                Concat(innerMic.AsSpan(0, 8), signedRegion),
                (KeyUsage)15);
            micChecksum.AsSpan(0, 8).CopyTo(innerMic.AsSpan(16, 8));
            EncryptRc4Sequence(
                sequence,
                LocalRc4Direction,
                innerMic.AsSpan(16, 8),
                innerMic.AsSpan(8, 8));
            return WrapInitialContextToken(innerMic);
        }

        byte[] innerWrap = CreateRc4WrapHeader(confidential: true);

        byte[] confounder = RandomNumberGenerator.GetBytes(8);
        byte[] plaintextRegion = signedRegion.ToArray();
        // MS-RPCE supplies its already-aligned PDU body in DCE style, where
        // the GSS provider does not append the non-DCE RFC 4757 0x01 pad.
        byte[] wrapChecksum = Rfc4757MakeChecksum(
            Concat(innerWrap.AsSpan(0, 8), confounder, plaintextRegion),
            (KeyUsage)13);
        wrapChecksum.AsSpan(0, 8).CopyTo(innerWrap.AsSpan(16, 8));

        var rc4 = new Rc4(DeriveRc4DataKey(sequence));
        rc4.Process(confounder, innerWrap.AsSpan(24, 8));
        Span<byte> confidentialSegment = signedRegion.Slice(
            confidentialOffset,
            confidentialLength);
        byte[] encrypted = new byte[confidentialLength];
        rc4.Process(confidentialSegment, encrypted);
        encrypted.CopyTo(confidentialSegment);
        EncryptRc4Sequence(
            sequence,
            LocalRc4Direction,
            innerWrap.AsSpan(16, 8),
            innerWrap.AsSpan(8, 8));
        return WrapInitialContextToken(innerWrap);
    }

    private void UnprotectRc4RpcMessage(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> verifier,
        bool confidential)
    {
        int expectedInnerLength = confidential
            ? Rc4WrapInnerLength
            : Rc4MicInnerLength;
        ReadOnlySpan<byte> inner = UnwrapInitialContextToken(
            verifier,
            expectedInnerLength);
        if (!confidential)
        {
            VerifyRc4RpcMic(signedRegion, inner);
            return;
        }

        UnwrapRc4RpcPrivacy(
            signedRegion,
            confidentialOffset,
            confidentialLength,
            inner);
    }

    private void VerifyRc4RpcMic(
        ReadOnlySpan<byte> signedRegion,
        ReadOnlySpan<byte> inner)
    {
        if (!inner[..8].SequenceEqual(
                new byte[] { 0x01, 0x01, 0x11, 0x00, 0xFF, 0xFF, 0xFF, 0xFF }))
        {
            throw new InvalidOperationException(
                "RFC 4757 GSS_GetMICEx token header is invalid.");
        }

        uint sequence = DecryptAndValidateRc4Sequence(
            inner.Slice(8, 8),
            inner.Slice(16, 8));
        ValidateReceiveSequence(unchecked((uint)sequence));
        byte[] expected = Rfc4757MakeChecksum(
            Concat(inner[..8], signedRegion),
            (KeyUsage)15);
        if (!FixedTimeEquals(inner.Slice(16, 8), expected.AsSpan(0, 8)))
        {
            throw new SecurityException(
                "RFC 4757 GSS_GetMICEx checksum validation failed.");
        }

        AcceptReceiveSequence();
    }

    private void UnwrapRc4RpcPrivacy(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlySpan<byte> inner)
    {
        if (!ReadRc4WrapConfidentiality(inner))
        {
            throw new InvalidOperationException(
                "RFC 4757 RPC GSS_WrapEx token must provide confidentiality.");
        }

        uint wrapSequence = DecryptAndValidateRc4Sequence(
            inner.Slice(8, 8),
            inner.Slice(16, 8));
        ValidateReceiveSequence(unchecked((uint)wrapSequence));

        var rc4 = new Rc4(DeriveRc4DataKey(wrapSequence));
        byte[] confounder = new byte[8];
        rc4.Process(inner.Slice(24, 8), confounder);
        ReadOnlySpan<byte> ciphertext = signedRegion.Slice(
            confidentialOffset,
            confidentialLength);
        byte[] plaintext = new byte[confidentialLength];
        rc4.Process(ciphertext, plaintext);
        byte[] plaintextRegion = ReplaceSegment(
            signedRegion,
            confidentialOffset,
            confidentialLength,
            plaintext);
        byte[] expectedChecksum = Rfc4757MakeChecksum(
            Concat(inner[..8], confounder, plaintextRegion),
            (KeyUsage)13);
        if (!FixedTimeEquals(
                inner.Slice(16, 8),
                expectedChecksum.AsSpan(0, 8)))
        {
            throw new SecurityException(
                "RFC 4757 GSS_WrapEx checksum validation failed.");
        }

        plaintext.CopyTo(
            signedRegion.Slice(confidentialOffset, confidentialLength));
        AcceptReceiveSequence();
    }

    private ReadOnlyMemory<byte> GetAesEncryptionKey(KeyUsage usage) =>
        _etype switch
        {
            EncryptionType.AES128_CTS_HMAC_SHA1_96 =>
                new RpcAesSha1Transformer(
                    16,
                    EncryptionType.AES128_CTS_HMAC_SHA1_96,
                    ChecksumType.HMAC_SHA1_96_AES128)
                    .DeriveEncryptionKey(_sessionKey, usage),
            EncryptionType.AES256_CTS_HMAC_SHA1_96 =>
                new RpcAesSha1Transformer(
                    32,
                    EncryptionType.AES256_CTS_HMAC_SHA1_96,
                    ChecksumType.HMAC_SHA1_96_AES256)
                    .DeriveEncryptionKey(_sessionKey, usage),
            EncryptionType.AES128_CTS_HMAC_SHA256_128 =>
                new RpcAes128Sha256Transformer()
                    .DeriveEncryptionKey(_sessionKey, usage),
            EncryptionType.AES256_CTS_HMAC_SHA384_192 =>
                new RpcAes256Sha384Transformer()
                    .DeriveEncryptionKey(_sessionKey, usage),
            _ => throw new NotSupportedException(
                $"Kerberos encryption type {_etype} is not supported for MS-RPCE GSS_WrapEx."),
        };

    private byte[] DeriveRc4DataKey(uint sequence)
    {
        byte[] key = GetRawKey();
        byte[] localKey = new byte[key.Length];
        for (int index = 0; index < key.Length; index++)
        {
            localKey[index] = (byte)(key[index] ^ 0xF0);
        }

        byte[] baseKey = HMACMD5.HashData(localKey, new byte[sizeof(int)]);
        Span<byte> sequenceBytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(sequenceBytes, sequence);
        return HMACMD5.HashData(baseKey, sequenceBytes);
    }

    private void EncryptRc4Sequence(
        uint sequence,
        byte direction,
        ReadOnlySpan<byte> checksum,
        Span<byte> destination)
    {
        Span<byte> plaintext = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(plaintext, sequence);
        plaintext[4..].Fill(direction);
        byte[] key = GetRawKey();
        byte[] sequenceKey = HMACMD5.HashData(
            key,
            new byte[sizeof(int)]);
        sequenceKey = HMACMD5.HashData(sequenceKey, checksum);
        Rc4Transform(sequenceKey, plaintext).CopyTo(destination);
    }

    private uint DecryptAndValidateRc4Sequence(
        ReadOnlySpan<byte> encryptedSequence,
        ReadOnlySpan<byte> checksum)
    {
        byte[] key = GetRawKey();
        byte[] sequenceKey = HMACMD5.HashData(
            key,
            new byte[sizeof(int)]);
        sequenceKey = HMACMD5.HashData(sequenceKey, checksum);
        byte[] plaintext = Rc4Transform(sequenceKey, encryptedSequence);
        byte expectedDirection = PeerRc4Direction;
        if (plaintext.AsSpan(4).IndexOfAnyExcept(expectedDirection) >= 0)
        {
            throw new SecurityException(
                "RFC 4757 token direction is invalid.");
        }

        return BinaryPrimitives.ReadUInt32BigEndian(plaintext);
    }

    private byte LocalRc4Direction => _isAcceptor ? byte.MaxValue : (byte)0;

    private byte PeerRc4Direction => _isAcceptor ? (byte)0 : byte.MaxValue;

    private static byte[] WrapInitialContextToken(
        ReadOnlySpan<byte> innerToken,
        int includedDataLength = 0)
    {
        int contentLength = checked(
            KerberosMechanismOid.Length
            + innerToken.Length
            + includedDataLength);
        int lengthOctets = GetDerLengthOctetCount(contentLength);
        byte[] token = new byte[
            1
            + lengthOctets
            + KerberosMechanismOid.Length
            + innerToken.Length];
        token[0] = 0x60;
        WriteDerLength(token.AsSpan(1, lengthOctets), contentLength);
        int contentOffset = 1 + lengthOctets;
        KerberosMechanismOid.CopyTo(token, contentOffset);
        innerToken.CopyTo(
            token.AsSpan(contentOffset + KerberosMechanismOid.Length));
        return token;
    }

    private static ReadOnlySpan<byte> UnwrapInitialContextToken(
        ReadOnlySpan<byte> token,
        int expectedInnerLength,
        int includedDataLength = 0)
    {
        int headerLength = ReadInitialContextTokenLength(
            token,
            out int declaredContentLength);
        int expectedContentLength = checked(
            KerberosMechanismOid.Length
            + expectedInnerLength
            + includedDataLength);
        int expectedSignatureLength = checked(
            headerLength
            + KerberosMechanismOid.Length
            + expectedInnerLength);
        if (token.Length != expectedSignatureLength
            || declaredContentLength != expectedContentLength
            || !token.Slice(
                    headerLength,
                    KerberosMechanismOid.Length)
                .SequenceEqual(KerberosMechanismOid))
        {
            throw new InvalidOperationException(
                "RFC 4757 per-message InitialContextToken framing is invalid.");
        }

        return token[
            (headerLength + KerberosMechanismOid.Length)..];
    }

    private static int GetInitialContextTokenLength(
        int innerLength,
        int includedDataLength = 0)
    {
        int contentLength = checked(
            KerberosMechanismOid.Length
            + innerLength
            + includedDataLength);
        return checked(
            1
            + GetDerLengthOctetCount(contentLength)
            + KerberosMechanismOid.Length
            + innerLength);
    }

    private static int ReadInitialContextTokenLength(
        ReadOnlySpan<byte> token,
        out int contentLength)
    {
        if (token.Length < 2 || token[0] != 0x60)
        {
            throw new InvalidOperationException(
                "RFC 4757 per-message InitialContextToken tag is invalid.");
        }

        byte firstLength = token[1];
        if ((firstLength & 0x80) == 0)
        {
            contentLength = firstLength;
            return 2;
        }

        int lengthOctets = firstLength & 0x7F;
        if (lengthOctets is 0 or > sizeof(int)
            || token.Length < 2 + lengthOctets)
        {
            throw new InvalidOperationException(
                "RFC 4757 per-message InitialContextToken length is invalid.");
        }

        contentLength = 0;
        for (int index = 0; index < lengthOctets; index++)
        {
            contentLength = checked(
                (contentLength << 8) | token[2 + index]);
        }
        if (contentLength < 128)
        {
            throw new InvalidOperationException(
                "RFC 4757 per-message InitialContextToken length is not DER encoded.");
        }

        return 2 + lengthOctets;
    }

    private static int GetDerLengthOctetCount(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length < 128)
        {
            return 1;
        }

        int value = length;
        int payloadOctets = 0;
        while (value > 0)
        {
            payloadOctets++;
            value >>= 8;
        }
        return 1 + payloadOctets;
    }

    private static void WriteDerLength(
        Span<byte> destination,
        int length)
    {
        if (length < 128)
        {
            destination[0] = (byte)length;
            return;
        }

        int payloadOctets = destination.Length - 1;
        destination[0] = (byte)(0x80 | payloadOctets);
        int value = length;
        for (int index = payloadOctets; index > 0; index--)
        {
            destination[index] = (byte)value;
            value >>= 8;
        }
    }

    private static void ValidateRpcSegments(
        ReadOnlySpan<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength)
    {
        if (confidentialOffset < 0
            || confidentialLength < 0
            || confidentialOffset > signedRegion.Length - confidentialLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidentialOffset),
                "The confidential RPC segment must be contained in the signed region.");
        }
    }

    private static byte[] ReplaceSegment(
        ReadOnlySpan<byte> source,
        int offset,
        int length,
        ReadOnlySpan<byte> replacement)
    {
        if (replacement.Length != length)
        {
            throw new ArgumentException(
                "Replacement length must match the source segment.",
                nameof(replacement));
        }

        byte[] result = source.ToArray();
        replacement.CopyTo(result.AsSpan(offset, length));
        return result;
    }

    private bool HasExpectedPeerFlags(byte flags)
    {
        bool sentByAcceptor = (flags & SentByAcceptorFlag) != 0;
        bool acceptorSubkey = (flags & AcceptorSubkeyFlag) != 0;
        return sentByAcceptor == !_isAcceptor
            && acceptorSubkey == _usesAcceptorSubkey;
    }

    private bool IsAesSha2() =>
        _etype is EncryptionType.AES128_CTS_HMAC_SHA256_128
            or EncryptionType.AES256_CTS_HMAC_SHA384_192;

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

    private static byte[] AesCtsEncrypt(
        ReadOnlySpan<byte> plaintext,
        ReadOnlyMemory<byte> key,
        ReadOnlySpan<byte> initializationVector)
    {
        if (plaintext.Length < HeaderLength)
        {
            throw new ArgumentException(
                "AES CTS plaintext must contain at least one block.",
                nameof(plaintext));
        }

        int padding = HeaderLength - (plaintext.Length % HeaderLength);
        int paddedLength = padding == HeaderLength
            ? plaintext.Length
            : plaintext.Length + padding;
        byte[] padded = new byte[paddedLength];
        plaintext.CopyTo(padded);
        using Aes aes = Aes.Create();
        aes.Key = key.ToArray();
        byte[] encrypted = aes.EncryptCbc(
            padded,
            initializationVector,
            PaddingMode.None);
        if (encrypted.Length >= 2 * HeaderLength)
        {
            SwapLastAesBlocks(encrypted);
        }

        return encrypted.AsSpan(0, plaintext.Length).ToArray();
    }

    private static byte[] AesCtsDecrypt(
        ReadOnlySpan<byte> ciphertext,
        ReadOnlyMemory<byte> key,
        ReadOnlySpan<byte> initializationVector)
    {
        if (ciphertext.Length < HeaderLength)
        {
            throw new ArgumentException(
                "AES CTS ciphertext must contain at least one block.",
                nameof(ciphertext));
        }

        int padding = HeaderLength - (ciphertext.Length % HeaderLength);
        byte[] padded;
        using Aes aes = Aes.Create();
        aes.Key = key.ToArray();
        if (padding == HeaderLength)
        {
            padded = ciphertext.ToArray();
        }
        else
        {
            int paddedLength = ciphertext.Length + padding;
            padded = new byte[paddedLength];
            ciphertext.CopyTo(padded);
            int depadOffset = ciphertext.Length - (2 * HeaderLength) + padding;
            byte[] decryptedPad = aes.DecryptCbc(
                ciphertext.Slice(depadOffset, HeaderLength),
                initializationVector,
                PaddingMode.None);
            decryptedPad.AsSpan(HeaderLength - padding)
                .CopyTo(padded.AsSpan(ciphertext.Length));
        }

        if (ciphertext.Length >= 2 * HeaderLength)
        {
            SwapLastAesBlocks(padded);
        }

        return aes.DecryptCbc(
            padded,
            initializationVector,
            PaddingMode.None).AsSpan(0, ciphertext.Length).ToArray();
    }

    private static void SwapLastAesBlocks(Span<byte> value)
    {
        int firstOffset = value.Length - (2 * HeaderLength);
        int secondOffset = value.Length - HeaderLength;
        for (int index = 0; index < HeaderLength; index++)
        {
            (value[firstOffset + index], value[secondOffset + index]) =
                (value[secondOffset + index], value[firstOffset + index]);
        }
    }

    private static byte[] RotateRight(ReadOnlySpan<byte> value, int count)
    {
        if (value.IsEmpty)
        {
            return [];
        }

        count %= value.Length;
        if (count == 0)
        {
            return value.ToArray();
        }

        byte[] output = new byte[value.Length];
        value[^count..].CopyTo(output);
        value[..^count].CopyTo(output.AsSpan(count));
        return output;
    }

    private static byte[] RotateLeft(ReadOnlySpan<byte> value, int count)
    {
        if (value.IsEmpty)
        {
            return [];
        }

        count %= value.Length;
        if (count == 0)
        {
            return value.ToArray();
        }

        byte[] output = new byte[value.Length];
        value[count..].CopyTo(output);
        value[..count].CopyTo(output.AsSpan(value.Length - count));
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

    private static byte[] Concat(
        ReadOnlySpan<byte> first,
        ReadOnlySpan<byte> second,
        ReadOnlySpan<byte> third,
        ReadOnlySpan<byte> fourth)
    {
        var output = new byte[
            first.Length + second.Length + third.Length + fourth.Length];
        int offset = 0;
        first.CopyTo(output.AsSpan(offset));
        offset += first.Length;
        second.CopyTo(output.AsSpan(offset));
        offset += second.Length;
        third.CopyTo(output.AsSpan(offset));
        offset += third.Length;
        fourth.CopyTo(output.AsSpan(offset));
        return output;
    }

    private static byte[] Concat(
        ReadOnlySpan<byte> first,
        ReadOnlySpan<byte> second,
        ReadOnlySpan<byte> third,
        ReadOnlySpan<byte> fourth,
        ReadOnlySpan<byte> fifth,
        ReadOnlySpan<byte> sixth)
    {
        var output = new byte[
            first.Length
            + second.Length
            + third.Length
            + fourth.Length
            + fifth.Length
            + sixth.Length];
        int offset = 0;
        first.CopyTo(output.AsSpan(offset));
        offset += first.Length;
        second.CopyTo(output.AsSpan(offset));
        offset += second.Length;
        third.CopyTo(output.AsSpan(offset));
        offset += third.Length;
        fourth.CopyTo(output.AsSpan(offset));
        offset += fourth.Length;
        fifth.CopyTo(output.AsSpan(offset));
        offset += fifth.Length;
        sixth.CopyTo(output.AsSpan(offset));
        return output;
    }

    private static bool FixedTimeEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private sealed class RpcAesSha1Transformer : AESTransformer
    {
        private readonly EncryptionType _encryptionType;
        private readonly ChecksumType _checksumType;

        public RpcAesSha1Transformer(
            int keySize,
            EncryptionType encryptionType,
            ChecksumType checksumType)
            : base(keySize)
        {
            _encryptionType = encryptionType;
            _checksumType = checksumType;
        }

        public override ChecksumType ChecksumType => _checksumType;

        public override EncryptionType EncryptionType => _encryptionType;

        public ReadOnlyMemory<byte> DeriveEncryptionKey(
            KerberosKey key,
            KeyUsage usage) =>
            GetOrDeriveKey(key, usage, KeyDerivationMode.Ke);
    }

    private sealed class RpcAes128Sha256Transformer :
        AES128Sha256Transformer
    {
        public ReadOnlyMemory<byte> DeriveEncryptionKey(
            KerberosKey key,
            KeyUsage usage) =>
            GetOrDeriveKey(key, usage, KeyDerivationMode.Ke);
    }

    private sealed class RpcAes256Sha384Transformer :
        AES256Sha384Transformer
    {
        public ReadOnlyMemory<byte> DeriveEncryptionKey(
            KerberosKey key,
            KeyUsage usage) =>
            GetOrDeriveKey(key, usage, KeyDerivationMode.Ke);
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
