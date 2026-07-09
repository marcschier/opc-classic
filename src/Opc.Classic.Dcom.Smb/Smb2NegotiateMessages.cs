// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// SMB2 NEGOTIATE request body, per [MS-SMB2] §2.2.3. Variable-length:
/// <c>StructureSize(2) + DialectCount(2) + SecurityMode(2) + Reserved(2) +
/// Capabilities(4) + ClientGuid(16) + ClientStartTime/NegotiateContextOffset(8) +
/// Dialects[DialectCount](2 each)</c>.
/// </summary>
public readonly record struct Smb2NegotiateRequest(
    ushort SecurityMode,
    uint Capabilities,
    Guid ClientGuid,
    IReadOnlyList<Smb2Dialect> Dialects,
    bool IncludeSmb311NegotiateContexts = false)
{
    private const int FixedSize = 36; // 2+2+2+2+4+16+8
    private const int NegotiateContextHeaderSize = 8;
    private const int PreauthContextDataLength = 6;
    private const int EncryptionContextDataLength = 6;

    public int WriteTo(Span<byte> destination)
    {
        int dialectsSize = Dialects.Count * sizeof(ushort);
        bool includeContexts = IncludeSmb311NegotiateContexts && ContainsDialect(Smb2Dialect.Smb311);
        int firstContextOffsetFromHeader = Smb2Constants.PacketHeaderSize + FixedSize + dialectsSize;
        int contextPrefixPadding = includeContexts ? GetAlignmentPadding(firstContextOffsetFromHeader) : 0;
        firstContextOffsetFromHeader += contextPrefixPadding;
        int contextsSize = includeContexts ? GetSmb311NegotiateContextsSize(firstContextOffsetFromHeader) : 0;
        int total = FixedSize + dialectsSize + contextPrefixPadding + contextsSize;
        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 NEGOTIATE request.", nameof(destination));
        }

        destination[..total].Clear();
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 36);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], (ushort)Dialects.Count);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[4..], SecurityMode);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[6..], 0);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[8..], Capabilities);
        if (!ClientGuid.TryWriteBytes(destination[12..28]))
        {
            throw new InvalidOperationException("ClientGuid write failed.");
        }

        if (includeContexts)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(destination[28..], (uint)firstContextOffsetFromHeader);
            BinaryPrimitives.WriteUInt16LittleEndian(destination[32..], 2);
        }

        Span<byte> dialectSpan = destination.Slice(FixedSize, dialectsSize);
        for (int i = 0; i < Dialects.Count; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dialectSpan[(i * 2)..], (ushort)Dialects[i]);
        }

        if (includeContexts)
        {
            int offset = FixedSize + dialectsSize + contextPrefixPadding;
            int preauthLength = WritePreauthIntegrityCapabilities(destination[offset..]);
            offset += preauthLength;
            offset += GetAlignmentPadding(Smb2Constants.PacketHeaderSize + offset);
            _ = WriteEncryptionCapabilities(destination[offset..]);
        }

        return total;
    }

    private bool ContainsDialect(Smb2Dialect dialect)
    {
        for (int i = 0; i < Dialects.Count; i++)
        {
            if (Dialects[i] == dialect)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetSmb311NegotiateContextsSize(int firstContextOffsetFromHeader)
    {
        const int PreauthContextLength = NegotiateContextHeaderSize + PreauthContextDataLength;
        const int EncryptionContextLength = NegotiateContextHeaderSize + EncryptionContextDataLength;
        return PreauthContextLength + GetAlignmentPadding(firstContextOffsetFromHeader + PreauthContextLength) + EncryptionContextLength;
    }

    private static int WritePreauthIntegrityCapabilities(Span<byte> destination)
    {
        const int ContextLength = NegotiateContextHeaderSize + PreauthContextDataLength;
        if (destination.Length < ContextLength)
        {
            throw new ArgumentException("Destination too small for SMB2_PREAUTH_INTEGRITY_CAPABILITIES.", nameof(destination));
        }

        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], Smb2Constants.NegotiateContextPreauthIntegrityCapabilities);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], PreauthContextDataLength);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], 0);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[8..], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[10..], 0);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[12..], Smb2Constants.PreauthHashSha512);
        return ContextLength;
    }

    private static int WriteEncryptionCapabilities(Span<byte> destination)
    {
        const int ContextLength = NegotiateContextHeaderSize + EncryptionContextDataLength;
        if (destination.Length < ContextLength)
        {
            throw new ArgumentException("Destination too small for SMB2_ENCRYPTION_CAPABILITIES.", nameof(destination));
        }

        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], Smb2Constants.NegotiateContextEncryptionCapabilities);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], EncryptionContextDataLength);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], 0);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[8..], 2);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[10..], Smb2Constants.EncryptionCipherAes128Gcm);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[12..], Smb2Constants.EncryptionCipherAes128Ccm);
        return ContextLength;
    }

    private static int GetAlignmentPadding(int offset) => (8 - (offset & 7)) & 7;
}

/// <summary>
/// SMB2 NEGOTIATE response body, per [MS-SMB2] §2.2.4.
/// </summary>
public readonly record struct Smb2NegotiateResponse(
    ushort SecurityMode,
    Smb2Dialect Dialect,
    Guid ServerGuid,
    uint Capabilities,
    uint MaxTransactSize,
    uint MaxReadSize,
    uint MaxWriteSize,
    ReadOnlyMemory<byte> SecurityBuffer,
    Smb2EncryptionAlgorithm? EncryptionAlgorithm = null)
{
    /// <summary>
    /// Parses an SMB2 NEGOTIATE response body (excluding the 64-byte packet header).
    /// </summary>
    public static Smb2NegotiateResponse Read(ReadOnlySpan<byte> source)
    {
        Smb2MessageBounds.EnsureBodyWithinDefaultQuota(source, "SMB2 NEGOTIATE response");
        if (source.Length < 64)
        {
            throw new Smb2ProtocolException("SMB2 NEGOTIATE response too short.");
        }
        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 65)
        {
            throw new Smb2ProtocolException($"Unexpected NEGOTIATE StructureSize {structureSize}; expected 65.");
        }

        ushort securityMode = BinaryPrimitives.ReadUInt16LittleEndian(source[2..]);
        var dialect = (Smb2Dialect)BinaryPrimitives.ReadUInt16LittleEndian(source[4..]);
        ushort negotiateContextCount = BinaryPrimitives.ReadUInt16LittleEndian(source[6..]);
        var serverGuid = new Guid(source.Slice(8, 16));
        uint capabilities = BinaryPrimitives.ReadUInt32LittleEndian(source[24..]);
        uint maxTransactSize = BinaryPrimitives.ReadUInt32LittleEndian(source[28..]);
        uint maxReadSize = BinaryPrimitives.ReadUInt32LittleEndian(source[32..]);
        uint maxWriteSize = BinaryPrimitives.ReadUInt32LittleEndian(source[36..]);
        // SystemTime [40..48], ServerStartTime [48..56]
        ushort secBufferOffset = BinaryPrimitives.ReadUInt16LittleEndian(source[56..]);
        ushort secBufferLength = BinaryPrimitives.ReadUInt16LittleEndian(source[58..]);
        uint negotiateContextOffset = BinaryPrimitives.ReadUInt32LittleEndian(source[60..]);

        byte[] securityBuffer;
        if (secBufferLength == 0)
        {
            securityBuffer = Array.Empty<byte>();
        }
        else
        {
            // Offsets are relative to the start of the SMB2 header. Caller passes
            // the body slice starting AT the header, so the offset is from there.
            securityBuffer = Smb2MessageBounds.GetPayloadSlice(
                source,
                secBufferOffset,
                secBufferLength,
                "NEGOTIATE response SecurityBuffer").ToArray();
        }

        Smb2EncryptionAlgorithm? encryptionAlgorithm = dialect == Smb2Dialect.Smb311
            ? ReadSelectedEncryptionAlgorithm(source, negotiateContextOffset, negotiateContextCount)
            : null;

        return new Smb2NegotiateResponse(
            SecurityMode: securityMode,
            Dialect: dialect,
            ServerGuid: serverGuid,
            Capabilities: capabilities,
            MaxTransactSize: maxTransactSize,
            MaxReadSize: maxReadSize,
            MaxWriteSize: maxWriteSize,
            SecurityBuffer: securityBuffer,
            EncryptionAlgorithm: encryptionAlgorithm);
    }

    private static Smb2EncryptionAlgorithm? ReadSelectedEncryptionAlgorithm(
        ReadOnlySpan<byte> source,
        uint negotiateContextOffset,
        ushort negotiateContextCount)
    {
        if (negotiateContextCount == 0)
        {
            return null;
        }
        if (negotiateContextOffset < Smb2Constants.PacketHeaderSize || negotiateContextOffset > int.MaxValue)
        {
            throw new Smb2ProtocolException("NEGOTIATE response NegotiateContextOffset out of range.");
        }

        int offset = (int)negotiateContextOffset - Smb2Constants.PacketHeaderSize;
        for (int i = 0; i < negotiateContextCount; i++)
        {
            if (offset < 0 || offset + 8 > source.Length)
            {
                throw new Smb2ProtocolException("NEGOTIATE response context header out of range.");
            }

            ushort contextType = BinaryPrimitives.ReadUInt16LittleEndian(source[offset..]);
            ushort dataLength = BinaryPrimitives.ReadUInt16LittleEndian(source[(offset + 2)..]);
            int dataOffset = offset + 8;
            if (dataOffset + dataLength > source.Length)
            {
                throw new Smb2ProtocolException("NEGOTIATE response context data out of range.");
            }

            if (contextType == Smb2Constants.NegotiateContextEncryptionCapabilities)
            {
                return ReadEncryptionCapabilities(source.Slice(dataOffset, dataLength));
            }

            offset = Align8(dataOffset + dataLength);
        }

        return null;
    }

    private static Smb2EncryptionAlgorithm? ReadEncryptionCapabilities(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            throw new Smb2ProtocolException("SMB2_ENCRYPTION_CAPABILITIES response too short.");
        }

        ushort cipherCount = BinaryPrimitives.ReadUInt16LittleEndian(data);
        if (cipherCount != 1)
        {
            throw new Smb2ProtocolException("SMB2_ENCRYPTION_CAPABILITIES response CipherCount must be 1.");
        }

        ushort cipherId = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
        if (cipherId == 0)
        {
            return null;
        }
        if (!Smb2Crypter.TryGetAlgorithmForCipherId(cipherId, out Smb2EncryptionAlgorithm algorithm))
        {
            throw new Smb2ProtocolException($"Unsupported SMB2 encryption cipher 0x{cipherId:X4}.");
        }

        return algorithm;
    }

    private static int Align8(int offset) => (offset + 7) & ~7;
}
