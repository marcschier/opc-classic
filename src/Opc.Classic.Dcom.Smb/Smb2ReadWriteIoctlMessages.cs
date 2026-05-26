//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Smb;

/// <summary>SMB2 READ request body, per [MS-SMB2] §2.2.19.</summary>
internal readonly record struct Smb2ReadRequest(
    uint Length,
    ulong Offset,
    ulong FileIdPersistent,
    ulong FileIdVolatile,
    uint MinimumCount)
{
    public int WriteTo(Span<byte> destination)
    {
        if (destination.Length < 49)
        {
            throw new ArgumentException("Destination too small for SMB2 READ request.", nameof(destination));
        }
        destination[..49].Clear();
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 49);
        destination[2] = 0;     // Padding (offset of payload past header)
        destination[3] = 0;     // Flags
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], Length);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], Offset);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], FileIdPersistent);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[24..], FileIdVolatile);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[32..], MinimumCount);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[36..], 0);    // Channel
        BinaryPrimitives.WriteUInt32LittleEndian(destination[40..], 0);    // RemainingBytes
        BinaryPrimitives.WriteUInt16LittleEndian(destination[44..], 0);    // ReadChannelInfoOffset
        BinaryPrimitives.WriteUInt16LittleEndian(destination[46..], 0);    // ReadChannelInfoLength
        destination[48] = 0;                                              // Buffer[0] padding
        return 49;
    }
}

/// <summary>SMB2 READ response body, per [MS-SMB2] §2.2.20.</summary>
internal readonly record struct Smb2ReadResponse(ReadOnlyMemory<byte> Data)
{
    public static Smb2ReadResponse Read(ReadOnlySpan<byte> source)
    {
        if (source.Length < 16)
        {
            throw new Smb2ProtocolException("SMB2 READ response too short.");
        }
        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 17)
        {
            throw new Smb2ProtocolException($"Unexpected READ StructureSize {structureSize}; expected 17.");
        }

        byte dataOffset = source[2];
        uint dataLength = BinaryPrimitives.ReadUInt32LittleEndian(source[4..]);

        if (dataLength == 0)
        {
            return new Smb2ReadResponse(ReadOnlyMemory<byte>.Empty);
        }

        int offset = dataOffset - Smb2Constants.PacketHeaderSize;
        if (offset < 0 || offset + dataLength > source.Length)
        {
            throw new Smb2ProtocolException("READ response Data offset out of range.");
        }

        var data = new byte[dataLength];
        source.Slice(offset, (int)dataLength).CopyTo(data);
        return new Smb2ReadResponse(data);
    }
}

/// <summary>SMB2 WRITE request body, per [MS-SMB2] §2.2.21.</summary>
internal readonly record struct Smb2WriteRequest(
    ulong Offset,
    ulong FileIdPersistent,
    ulong FileIdVolatile,
    ReadOnlyMemory<byte> Data)
{
    public int WriteTo(Span<byte> destination)
    {
        const int FixedSize = 48;
        int total = FixedSize + Data.Length;
        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 WRITE request.", nameof(destination));
        }
        destination[..FixedSize].Clear();
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 49);
        ushort dataOffset = (ushort)(Smb2Constants.PacketHeaderSize + FixedSize);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], dataOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], (uint)Data.Length);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], Offset);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], FileIdPersistent);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[24..], FileIdVolatile);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[32..], 0);    // Channel
        BinaryPrimitives.WriteUInt32LittleEndian(destination[36..], 0);    // RemainingBytes
        BinaryPrimitives.WriteUInt16LittleEndian(destination[40..], 0);    // WriteChannelInfoOffset
        BinaryPrimitives.WriteUInt16LittleEndian(destination[42..], 0);    // WriteChannelInfoLength
        BinaryPrimitives.WriteUInt32LittleEndian(destination[44..], 0);    // Flags
        if (!Data.IsEmpty)
        {
            Data.Span.CopyTo(destination[FixedSize..]);
        }
        return total;
    }
}

/// <summary>SMB2 WRITE response body, per [MS-SMB2] §2.2.22.</summary>
internal readonly record struct Smb2WriteResponse(uint Count)
{
    public static Smb2WriteResponse Read(ReadOnlySpan<byte> source)
    {
        if (source.Length < 16)
        {
            throw new Smb2ProtocolException("SMB2 WRITE response too short.");
        }
        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 17)
        {
            throw new Smb2ProtocolException($"Unexpected WRITE StructureSize {structureSize}; expected 17.");
        }
        return new Smb2WriteResponse(BinaryPrimitives.ReadUInt32LittleEndian(source[4..]));
    }
}

/// <summary>SMB2 IOCTL request body, per [MS-SMB2] §2.2.31. Designed for FSCTL_PIPE_TRANSCEIVE.</summary>
internal readonly record struct Smb2IoctlRequest(
    uint CtlCode,
    ulong FileIdPersistent,
    ulong FileIdVolatile,
    ReadOnlyMemory<byte> Input,
    uint MaxOutputResponse,
    bool IsFsctl)
{
    public int WriteTo(Span<byte> destination)
    {
        const int FixedSize = 56;
        int total = FixedSize + Input.Length;
        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 IOCTL request.", nameof(destination));
        }
        destination[..FixedSize].Clear();
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 57);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], 0);     // Reserved
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], CtlCode);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], FileIdPersistent);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], FileIdVolatile);

        ushort inputOffset = (ushort)(Smb2Constants.PacketHeaderSize + FixedSize);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[24..], inputOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[28..], (uint)Input.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[32..], 0);    // MaxInputResponse
        BinaryPrimitives.WriteUInt32LittleEndian(destination[36..], 0);    // OutputOffset
        BinaryPrimitives.WriteUInt32LittleEndian(destination[40..], 0);    // OutputCount
        BinaryPrimitives.WriteUInt32LittleEndian(destination[44..], MaxOutputResponse);
        // Flags: SMB2_0_IOCTL_IS_FSCTL = 0x00000001
        BinaryPrimitives.WriteUInt32LittleEndian(destination[48..], IsFsctl ? 1u : 0u);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[52..], 0);    // Reserved2

        if (!Input.IsEmpty)
        {
            Input.Span.CopyTo(destination[FixedSize..]);
        }
        return total;
    }
}

/// <summary>SMB2 IOCTL response body, per [MS-SMB2] §2.2.32.</summary>
internal readonly record struct Smb2IoctlResponse(
    uint CtlCode,
    ulong FileIdPersistent,
    ulong FileIdVolatile,
    ReadOnlyMemory<byte> Output)
{
    public static Smb2IoctlResponse Read(ReadOnlySpan<byte> source)
    {
        if (source.Length < 48)
        {
            throw new Smb2ProtocolException("SMB2 IOCTL response too short.");
        }
        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 49)
        {
            throw new Smb2ProtocolException($"Unexpected IOCTL StructureSize {structureSize}; expected 49.");
        }
        uint ctlCode = BinaryPrimitives.ReadUInt32LittleEndian(source[4..]);
        ulong fidPersistent = BinaryPrimitives.ReadUInt64LittleEndian(source[8..]);
        ulong fidVolatile = BinaryPrimitives.ReadUInt64LittleEndian(source[16..]);
        uint outputOffset = BinaryPrimitives.ReadUInt32LittleEndian(source[32..]);
        uint outputCount = BinaryPrimitives.ReadUInt32LittleEndian(source[36..]);

        byte[] output;
        if (outputCount == 0)
        {
            output = Array.Empty<byte>();
        }
        else
        {
            int offset = (int)outputOffset - Smb2Constants.PacketHeaderSize;
            if (offset < 0 || offset + outputCount > source.Length)
            {
                throw new Smb2ProtocolException("IOCTL response Output offset out of range.");
            }
            output = source.Slice(offset, (int)outputCount).ToArray();
        }

        return new Smb2IoctlResponse(ctlCode, fidPersistent, fidVolatile, output);
    }
}
