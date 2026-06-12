//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// SMB2 CREATE request body, per [MS-SMB2] §2.2.13.
/// </summary>
internal readonly record struct Smb2CreateRequest(
    uint DesiredAccess,
    uint FileAttributes,
    uint ShareAccessMask,
    uint Disposition,
    uint CreateOptionsMask,
    string Name)
{
    public int WriteTo(Span<byte> destination)
    {
        if (Name is null)
        {
            throw new InvalidOperationException("CREATE Name must not be null.");
        }

        const int FixedSize = 56;
        int nameBytes = Encoding.Unicode.GetByteCount(Name);
        if (nameBytes > ushort.MaxValue)
        {
            throw new InvalidOperationException("CREATE Name exceeds 65535 bytes when UTF-16LE encoded.");
        }

        // [MS-SMB2] §2.2.13: a zero-length name still has NameOffset set; a one-byte
        // pad ensures FixedSize+nameBytes is at least 1.
        int payload = Math.Max(nameBytes, 1);
        int total = FixedSize + payload;

        if (destination.Length < total)
        {
            throw new ArgumentException("Destination too small for SMB2 CREATE request.", nameof(destination));
        }

        destination[..total].Clear();
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 57);
        destination[2] = 0;     // SecurityFlags (reserved)
        destination[3] = 0;     // RequestedOplockLevel = SMB2_OPLOCK_LEVEL_NONE
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], 0);     // ImpersonationLevel = Anonymous
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], 0);     // SmbCreateFlags
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], 0);    // Reserved
        BinaryPrimitives.WriteUInt32LittleEndian(destination[24..], DesiredAccess);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[28..], FileAttributes);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[32..], ShareAccessMask);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[36..], Disposition);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[40..], CreateOptionsMask);

        ushort nameOffset = (ushort)(Smb2Constants.PacketHeaderSize + FixedSize);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[44..], nameOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[46..], (ushort)nameBytes);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[48..], 0);    // CreateContextsOffset
        BinaryPrimitives.WriteUInt32LittleEndian(destination[52..], 0);    // CreateContextsLength

        if (nameBytes > 0)
        {
            Encoding.Unicode.GetBytes(Name, destination[FixedSize..]);
        }

        return total;
    }
}

/// <summary>
/// SMB2 CREATE response body, per [MS-SMB2] §2.2.14.
/// </summary>
internal readonly record struct Smb2CreateResponse(
    ulong FileIdPersistent,
    ulong FileIdVolatile)
{
    public static Smb2CreateResponse Read(ReadOnlySpan<byte> source)
    {
        Smb2MessageBounds.EnsureBodyWithinDefaultQuota(source, "SMB2 CREATE response");
        if (source.Length < 88)
        {
            throw new Smb2ProtocolException("SMB2 CREATE response too short.");
        }
        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (structureSize != 89)
        {
            throw new Smb2ProtocolException($"Unexpected CREATE StructureSize {structureSize}; expected 89.");
        }
        return new Smb2CreateResponse(
            FileIdPersistent: BinaryPrimitives.ReadUInt64LittleEndian(source[64..]),
            FileIdVolatile: BinaryPrimitives.ReadUInt64LittleEndian(source[72..]));
    }
}

/// <summary>
/// SMB2 CLOSE request body, per [MS-SMB2] §2.2.15.
/// </summary>
internal readonly record struct Smb2CloseRequest(
    ulong FileIdPersistent,
    ulong FileIdVolatile)
{
    public int WriteTo(Span<byte> destination)
    {
        if (destination.Length < 24)
        {
            throw new ArgumentException("Destination too small for SMB2 CLOSE request.", nameof(destination));
        }
        BinaryPrimitives.WriteUInt16LittleEndian(destination[0..], 24);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], 0);     // Flags
        BinaryPrimitives.WriteUInt32LittleEndian(destination[4..], 0);     // Reserved
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], FileIdPersistent);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], FileIdVolatile);
        return 24;
    }
}
