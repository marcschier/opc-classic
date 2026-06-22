// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// SMB2 SESSION_SETUP request body, per [MS-SMB2] §2.2.5.
/// </summary>
internal readonly record struct Smb2SessionSetupRequest(
    byte Flags,
    byte SecurityMode,
    uint Capabilities,
    uint Channel,
    ulong PreviousSessionId,
    ReadOnlyMemory<byte> SecurityBlob)
{
    public int WriteTo(Span<byte> destination)
    {
        const int FixedSize = 24;
        if (SecurityBlob.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("SESSION_SETUP SecurityBlob exceeds 65535 bytes.");
        }
        int total = FixedSize + SecurityBlob.Length;
        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 SESSION_SETUP request.", nameof(destination));
        }

        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 25);
        destination[2] = Flags;
        destination[3] = SecurityMode;
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], Capabilities);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[8..], Channel);
        ushort securityBufferOffset = (ushort)(Smb2Constants.PacketHeaderSize + FixedSize);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[12..], securityBufferOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[14..], (ushort)SecurityBlob.Length);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], PreviousSessionId);

        if (!SecurityBlob.IsEmpty)
        {
            SecurityBlob.Span.CopyTo(destination.Slice(FixedSize, SecurityBlob.Length));
        }
        return total;
    }
}

/// <summary>
/// SMB2 SESSION_SETUP response body, per [MS-SMB2] §2.2.6.
/// </summary>
internal readonly record struct Smb2SessionSetupResponse(
    ushort SessionFlags,
    ReadOnlyMemory<byte> SecurityBlob)
{
    public static Smb2SessionSetupResponse Read(ReadOnlySpan<byte> source)
    {
        Smb2MessageBounds.EnsureBodyWithinDefaultQuota(source, "SMB2 SESSION_SETUP response");
        if (source.Length < 8)
        {
            throw new Smb2ProtocolException("SMB2 SESSION_SETUP response too short.");
        }

        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 9)
        {
            throw new Smb2ProtocolException($"Unexpected SESSION_SETUP StructureSize {structureSize}; expected 9.");
        }

        ushort sessionFlags = BinaryPrimitives.ReadUInt16LittleEndian(source[2..]);
        ushort secBlobOffset = BinaryPrimitives.ReadUInt16LittleEndian(source[4..]);
        ushort secBlobLength = BinaryPrimitives.ReadUInt16LittleEndian(source[6..]);

        byte[] securityBlob;
        if (secBlobLength == 0)
        {
            securityBlob = Array.Empty<byte>();
        }
        else
        {
            securityBlob = Smb2MessageBounds.GetPayloadSlice(
                source,
                secBlobOffset,
                secBlobLength,
                "SESSION_SETUP response SecurityBuffer").ToArray();
        }

        return new Smb2SessionSetupResponse(sessionFlags, securityBlob);
    }
}
