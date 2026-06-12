//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// SMB2 packet header (synchronous form) per [MS-SMB2] §2.2.1.2. Fixed 64 bytes;
/// the async form differs only in that the AsyncId+Reserved2 fields replace
/// TreeId+ProcessId for cancelable long-running operations (we don't issue any).
/// </summary>
public readonly record struct Smb2PacketHeader(
    uint CreditCharge,
    uint Status,
    Smb2Command Command,
    uint CreditRequestResponse,
    uint Flags,
    uint NextCommand,
    ulong MessageId,
    uint ProcessId,
    uint TreeId,
    ulong SessionId,
    ReadOnlyMemory<byte> Signature)
{
    /// <summary>
    /// Writes the synchronous header to <paramref name="destination" /> starting at offset 0.
    /// </summary>
    public void Write(Span<byte> destination)
    {
        if (destination.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new ArgumentException(
                $"Destination must be at least {Smb2Constants.PacketHeaderSize} bytes.",
                nameof(destination));
        }

        Smb2Constants.ProtocolId.AsSpan().CopyTo(destination[..4]);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[4..], Smb2Constants.PacketHeaderSize);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[6..], (ushort)CreditCharge);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[8..], Status);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[12..], (ushort)Command);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[14..], (ushort)CreditRequestResponse);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[16..], Flags);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[20..], NextCommand);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[24..], MessageId);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[32..], ProcessId);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[36..], TreeId);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[40..], SessionId);

        Span<byte> sigDest = destination.Slice(48, 16);
        if (Signature.IsEmpty)
        {
            sigDest.Clear();
        }
        else
        {
            Signature.Span.CopyTo(sigDest);
        }
    }

    /// <summary>
    /// Parses a synchronous SMB2 header from <paramref name="source" />.
    /// </summary>
    public static Smb2PacketHeader Read(ReadOnlySpan<byte> source)
    {
        if (source.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new ArgumentException(
                $"Source must be at least {Smb2Constants.PacketHeaderSize} bytes.",
                nameof(source));
        }
        if (!source[..4].SequenceEqual(Smb2Constants.ProtocolId))
        {
            throw new Smb2ProtocolException("Invalid SMB2 ProtocolId.");
        }

        ushort headerSize = BinaryPrimitives.ReadUInt16LittleEndian(source[4..]);
        if (headerSize != Smb2Constants.PacketHeaderSize)
        {
            throw new Smb2ProtocolException(
                $"Unexpected SMB2 StructureSize {headerSize}; expected {Smb2Constants.PacketHeaderSize}.");
        }

        var signature = new byte[16];
        source.Slice(48, 16).CopyTo(signature);

        return new Smb2PacketHeader(
            CreditCharge: BinaryPrimitives.ReadUInt16LittleEndian(source[6..]),
            Status: BinaryPrimitives.ReadUInt32LittleEndian(source[8..]),
            Command: (Smb2Command)BinaryPrimitives.ReadUInt16LittleEndian(source[12..]),
            CreditRequestResponse: BinaryPrimitives.ReadUInt16LittleEndian(source[14..]),
            Flags: BinaryPrimitives.ReadUInt32LittleEndian(source[16..]),
            NextCommand: BinaryPrimitives.ReadUInt32LittleEndian(source[20..]),
            MessageId: BinaryPrimitives.ReadUInt64LittleEndian(source[24..]),
            ProcessId: BinaryPrimitives.ReadUInt32LittleEndian(source[32..]),
            TreeId: BinaryPrimitives.ReadUInt32LittleEndian(source[36..]),
            SessionId: BinaryPrimitives.ReadUInt64LittleEndian(source[40..]),
            Signature: signature);
    }
}

/// <summary>
/// NetBIOS-over-TCP framing: a 4-byte big-endian length prefix (with the high
/// byte set to 0x00 for SMB direct messages). See [MS-CIFS] §2.2.1.
/// </summary>
public static class NetBiosFraming
{
    /// <summary>
    /// Size of the NetBIOS frame header in bytes.
    /// </summary>
    public const int HeaderSize = 4;

    /// <summary>
    /// Writes the NetBIOS frame header for an SMB2 payload of the given length.
    /// </summary>
    public static void WriteHeader(Span<byte> destination, int payloadLength)
    {
        if (payloadLength < 0 || payloadLength > Smb2Constants.MaxNetBiosFrameSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(payloadLength),
                payloadLength,
                $"NetBIOS frame payload length must be 0..{Smb2Constants.MaxNetBiosFrameSize}.");
        }
        if (destination.Length < HeaderSize)
        {
            throw new ArgumentException(
                $"Destination must be at least {HeaderSize} bytes.",
                nameof(destination));
        }

        destination[0] = 0;
        destination[1] = (byte)((payloadLength >> 16) & 0xFF);
        destination[2] = (byte)((payloadLength >> 8) & 0xFF);
        destination[3] = (byte)(payloadLength & 0xFF);
    }

    /// <summary>
    /// Parses a NetBIOS frame header and returns the payload length.
    /// </summary>
    public static int ReadPayloadLength(ReadOnlySpan<byte> source) =>
        ReadPayloadLength(source, Smb2Constants.MaxNetBiosFrameSize);

    /// <summary>
    /// Parses a NetBIOS frame header and enforces a payload quota.
    /// </summary>
    internal static int ReadPayloadLength(ReadOnlySpan<byte> source, int maxPayloadLength)
    {
        if (maxPayloadLength < 0 || maxPayloadLength > Smb2Constants.MaxNetBiosFrameSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPayloadLength),
                maxPayloadLength,
                $"NetBIOS frame payload quota must be 0..{Smb2Constants.MaxNetBiosFrameSize}.");
        }
        if (source.Length < HeaderSize)
        {
            throw new ArgumentException(
                $"Source must be at least {HeaderSize} bytes.",
                nameof(source));
        }
        if (source[0] != 0)
        {
            throw new Smb2ProtocolException(
                $"Unexpected NetBIOS frame type 0x{source[0]:X2}; expected 0x00 (SMB direct).");
        }

        int payloadLength = (source[1] << 16) | (source[2] << 8) | source[3];
        if (payloadLength > maxPayloadLength)
        {
            throw new Smb2ProtocolException(
                $"NetBIOS frame payload length {payloadLength} exceeds the configured SMB2 quota of {maxPayloadLength} bytes.");
        }

        return payloadLength;
    }
}
