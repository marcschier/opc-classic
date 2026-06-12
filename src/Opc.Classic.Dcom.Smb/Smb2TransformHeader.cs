//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// Codec for the 52-byte SMB2 TRANSFORM_HEADER used by SMB 3.x encryption; see [MS-SMB2] §2.2.41.
/// </summary>
public readonly record struct Smb2TransformHeader(
    ReadOnlyMemory<byte> Signature,
    ReadOnlyMemory<byte> Nonce,
    uint OriginalMessageSize,
    ushort Reserved,
    ushort Flags,
    ulong SessionId)
{
    /// <summary>Fixed size of SMB2 TRANSFORM_HEADER in bytes; see [MS-SMB2] §2.2.41.</summary>
    public const int Size = 52;

    /// <summary>Size of the Signature field in bytes; see [MS-SMB2] §2.2.41.</summary>
    public const int SignatureLength = 16;

    /// <summary>Size of the Nonce field in bytes; see [MS-SMB2] §2.2.41.</summary>
    public const int NonceLength = 16;

    /// <summary>Offset of authenticated data within the transform header; see [MS-SMB2] §3.1.4.3.</summary>
    public const int AuthenticatedDataOffset = 20;

    /// <summary>Length of authenticated data within the transform header; see [MS-SMB2] §3.1.4.3.</summary>
    public const int AuthenticatedDataLength = Size - AuthenticatedDataOffset;

    /// <summary>Writes this transform header to <paramref name="destination" />; see [MS-SMB2] §2.2.41.</summary>
    public void Write(Span<byte> destination)
    {
        if (destination.Length < Size)
        {
            throw new ArgumentException($"Destination must be at least {Size} bytes.", nameof(destination));
        }
        if (!Signature.IsEmpty && Signature.Length != SignatureLength)
        {
            throw new ArgumentException("TRANSFORM_HEADER Signature must be 16 bytes.", nameof(destination));
        }
        if (Nonce.Length != NonceLength)
        {
            throw new ArgumentException("TRANSFORM_HEADER Nonce must be 16 bytes.", nameof(destination));
        }

        Smb2Constants.TransformProtocolId.CopyTo(destination[..4]);
        Span<byte> signatureDestination = destination.Slice(4, SignatureLength);
        if (Signature.IsEmpty)
        {
            signatureDestination.Clear();
        }
        else
        {
            Signature.Span.CopyTo(signatureDestination);
        }

        Nonce.Span.CopyTo(destination.Slice(20, NonceLength));
        BinaryPrimitives.WriteUInt32LittleEndian(destination[36..], OriginalMessageSize);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[40..], Reserved);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[42..], Flags);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[44..], SessionId);
    }

    /// <summary>Parses an SMB2 TRANSFORM_HEADER from <paramref name="source" />; see [MS-SMB2] §2.2.41.</summary>
    public static Smb2TransformHeader Read(ReadOnlySpan<byte> source)
    {
        if (source.Length < Size)
        {
            throw new Smb2ProtocolException("SMB2 transform message too short for TRANSFORM_HEADER.");
        }
        if (!HasTransformProtocolId(source))
        {
            throw new Smb2ProtocolException("Invalid SMB2 TRANSFORM_HEADER ProtocolId.");
        }

        byte[] signature = new byte[SignatureLength];
        source.Slice(4, SignatureLength).CopyTo(signature);
        byte[] nonce = new byte[NonceLength];
        source.Slice(20, NonceLength).CopyTo(nonce);

        return new Smb2TransformHeader(
            Signature: signature,
            Nonce: nonce,
            OriginalMessageSize: BinaryPrimitives.ReadUInt32LittleEndian(source[36..]),
            Reserved: BinaryPrimitives.ReadUInt16LittleEndian(source[40..]),
            Flags: BinaryPrimitives.ReadUInt16LittleEndian(source[42..]),
            SessionId: BinaryPrimitives.ReadUInt64LittleEndian(source[44..]));
    }

    /// <summary>Returns whether <paramref name="source" /> starts with the SMB2 transform ProtocolId; see [MS-SMB2] §2.2.41.</summary>
    public static bool HasTransformProtocolId(ReadOnlySpan<byte> source) =>
        source.Length >= 4 && source[..4].SequenceEqual(Smb2Constants.TransformProtocolId);

    /// <summary>
    /// Gets the associated-data slice authenticated by AES-CCM/GCM, excluding ProtocolId and Signature;
    /// see [MS-SMB2] §3.1.4.3.
    /// </summary>
    public static ReadOnlySpan<byte> GetAssociatedData(ReadOnlySpan<byte> transformMessage)
    {
        if (transformMessage.Length < Size)
        {
            throw new Smb2ProtocolException("SMB2 transform message too short for associated data.");
        }

        return transformMessage.Slice(AuthenticatedDataOffset, AuthenticatedDataLength);
    }
}
