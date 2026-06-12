//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// SMB2 TREE_CONNECT request body, per [MS-SMB2] §2.2.9.
/// </summary>
public readonly record struct Smb2TreeConnectRequest(string Path)
{
    public int WriteTo(Span<byte> destination)
    {
        if (Path is null)
        {
            throw new InvalidOperationException("TREE_CONNECT Path must not be null.");
        }

        const int FixedSize = 8;
        int pathBytes = Encoding.Unicode.GetByteCount(Path);
        if (pathBytes > ushort.MaxValue)
        {
            throw new InvalidOperationException("TREE_CONNECT Path exceeds 65535 bytes when UTF-16LE encoded.");
        }
        int total = FixedSize + pathBytes;
        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 TREE_CONNECT request.", nameof(destination));
        }

        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 9);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], 0);
        ushort pathOffset = (ushort)(Smb2Constants.PacketHeaderSize + FixedSize);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[4..], pathOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[6..], (ushort)pathBytes);

        Encoding.Unicode.GetBytes(Path, destination[FixedSize..]);
        return total;
    }
}

/// <summary>
/// SMB2 TREE_CONNECT response body, per [MS-SMB2] §2.2.10.
/// </summary>
public readonly record struct Smb2TreeConnectResponse(
    byte ShareType,
    uint ShareFlags,
    uint Capabilities,
    uint MaximalAccess)
{
    /// <summary>
    /// Parses an SMB2 TREE_CONNECT response body (excluding the 64-byte packet header).
    /// </summary>
    public static Smb2TreeConnectResponse Read(ReadOnlySpan<byte> source)
    {
        Smb2MessageBounds.EnsureBodyWithinDefaultQuota(source, "SMB2 TREE_CONNECT response");
        if (source.Length < 16)
        {
            throw new Smb2ProtocolException("SMB2 TREE_CONNECT response too short.");
        }
        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 16)
        {
            throw new Smb2ProtocolException($"Unexpected TREE_CONNECT StructureSize {structureSize}; expected 16.");
        }

        return new Smb2TreeConnectResponse(
            ShareType: source[2],
            ShareFlags: BinaryPrimitives.ReadUInt32LittleEndian(source[4..]),
            Capabilities: BinaryPrimitives.ReadUInt32LittleEndian(source[8..]),
            MaximalAccess: BinaryPrimitives.ReadUInt32LittleEndian(source[12..]));
    }
}

/// <summary>
/// SMB2 TREE_DISCONNECT request (empty body except 4-byte structure-size, per [MS-SMB2] §2.2.11).
/// </summary>
internal static class Smb2TreeDisconnect
{
    public static int Write(Span<byte> destination)
    {
        if (destination.Length < 4)
        {
            throw new ArgumentException("Destination too small for SMB2 TREE_DISCONNECT.", nameof(destination));
        }
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 4);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], 0);
        return 4;
    }
}

/// <summary>
/// SMB2 LOGOFF request (empty body except 4-byte structure-size, per [MS-SMB2] §2.2.7).
/// </summary>
internal static class Smb2Logoff
{
    public static int Write(Span<byte> destination)
    {
        if (destination.Length < 4)
        {
            throw new ArgumentException("Destination too small for SMB2 LOGOFF.", nameof(destination));
        }
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 4);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], 0);
        return 4;
    }
}
