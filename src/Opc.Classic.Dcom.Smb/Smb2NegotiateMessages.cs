//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;

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
    IReadOnlyList<Smb2Dialect> Dialects)
{
    public int WriteTo(Span<byte> destination)
    {
        const int FixedSize = 36; // 2+2+2+2+4+16+8
        int dialectsSize = Dialects.Count * sizeof(ushort);
        int total = FixedSize + dialectsSize;
        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 NEGOTIATE request.", nameof(destination));
        }

        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 36);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], (ushort)Dialects.Count);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[4..], SecurityMode);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[6..], 0);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[8..], Capabilities);
        if (!ClientGuid.TryWriteBytes(destination[12..28]))
        {
            throw new InvalidOperationException("ClientGuid write failed.");
        }
        destination.Slice(28, 8).Clear();

        Span<byte> dialectSpan = destination.Slice(FixedSize, dialectsSize);
        for (int i = 0; i < Dialects.Count; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dialectSpan[(i * 2)..], (ushort)Dialects[i]);
        }
        return total;
    }
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
    ReadOnlyMemory<byte> SecurityBuffer)
{
    /// <summary>Parses an SMB2 NEGOTIATE response body (excluding the 64-byte packet header).</summary>
    public static Smb2NegotiateResponse Read(ReadOnlySpan<byte> source)
    {
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
        ushort dialect = BinaryPrimitives.ReadUInt16LittleEndian(source[4..]);
        // skip NegotiateContextCount at [6..8]
        var serverGuid = new Guid(source.Slice(8, 16));
        uint capabilities = BinaryPrimitives.ReadUInt32LittleEndian(source[24..]);
        uint maxTransactSize = BinaryPrimitives.ReadUInt32LittleEndian(source[28..]);
        uint maxReadSize = BinaryPrimitives.ReadUInt32LittleEndian(source[32..]);
        uint maxWriteSize = BinaryPrimitives.ReadUInt32LittleEndian(source[36..]);
        // SystemTime [40..48], ServerStartTime [48..56]
        ushort secBufferOffset = BinaryPrimitives.ReadUInt16LittleEndian(source[56..]);
        ushort secBufferLength = BinaryPrimitives.ReadUInt16LittleEndian(source[58..]);

        byte[] securityBuffer;
        if (secBufferLength == 0)
        {
            securityBuffer = Array.Empty<byte>();
        }
        else
        {
            // Offsets are relative to the start of the SMB2 header. Caller passes
            // the body slice starting AT the header, so the offset is from there.
            int offset = secBufferOffset - Smb2Constants.PacketHeaderSize;
            if (offset < 0 || offset + secBufferLength > source.Length)
            {
                throw new Smb2ProtocolException("NEGOTIATE response SecurityBuffer offset out of range.");
            }
            securityBuffer = source.Slice(offset, secBufferLength).ToArray();
        }

        return new Smb2NegotiateResponse(
            SecurityMode: securityMode,
            Dialect: (Smb2Dialect)dialect,
            ServerGuid: serverGuid,
            Capabilities: capabilities,
            MaxTransactSize: maxTransactSize,
            MaxReadSize: maxReadSize,
            MaxWriteSize: maxWriteSize,
            SecurityBuffer: securityBuffer);
    }
}
