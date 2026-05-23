// SPDX-License-Identifier: MIT

using System;
using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public abstract class NtlmMessage
{
    public const string NtlmSignature = "NTLMSSP";

    private static readonly byte[] SignatureBytes = Encoding.ASCII.GetBytes("NTLMSSP\0");

    public abstract int MessageType { get; }

    public NtlmFlags Flags { get; set; }

    public NtlmFlags GetFlags() => Flags;

    public void SetFlags(NtlmFlags flags) => Flags = flags;

    public void SetFlags(int flags) => Flags = FromInt32(flags);

    public abstract byte[] ToByteArray();

    public bool GetFlag(NtlmFlags flag) => (Flags & flag) != 0;

    public bool GetFlag(int flag) => (((uint)Flags) & unchecked((uint)flag)) != 0;

    public void SetFlag(NtlmFlags flag, bool value) =>
        Flags = value ? (Flags | flag) : (Flags & ~flag);

    public void SetFlag(int flag, bool value) => SetFlag(FromInt32(flag), value);

    internal static NtlmFlags FromInt32(int flags) => (NtlmFlags)unchecked((uint)flags);

    internal static int ToInt32(NtlmFlags flags) => unchecked((int)(uint)flags);

    protected static void WriteHeader(Span<byte> dest, int messageType)
    {
        if (dest.Length < 12)
        {
            throw new ArgumentException("NTLM message buffer too short.", nameof(dest));
        }

        SignatureBytes.CopyTo(dest);
        BinaryPrimitives.WriteInt32LittleEndian(dest.Slice(8, 4), messageType);
    }

    protected static int ReadMessageType(ReadOnlySpan<byte> raw)
    {
        if (raw.Length < 12)
        {
            throw new ArgumentException("NTLM message too short.", nameof(raw));
        }

        if (!raw[..8].SequenceEqual(SignatureBytes))
        {
            throw new ArgumentException("Bad NTLMSSP signature.", nameof(raw));
        }

        return BinaryPrimitives.ReadInt32LittleEndian(raw.Slice(8, 4));
    }

    protected static void WriteFields(Span<byte> dest, ushort length, uint bufferOffset)
    {
        if (dest.Length < 8)
        {
            throw new ArgumentException("NTLM security buffer too short.", nameof(dest));
        }

        BinaryPrimitives.WriteUInt16LittleEndian(dest[..2], length);
        BinaryPrimitives.WriteUInt16LittleEndian(dest.Slice(2, 2), length);
        BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(4, 4), bufferOffset);
    }

    protected static (ushort Length, uint Offset) ReadFields(ReadOnlySpan<byte> src)
    {
        if (src.Length < 8)
        {
            throw new ArgumentException("NTLM security buffer too short.", nameof(src));
        }

        var len = BinaryPrimitives.ReadUInt16LittleEndian(src[..2]);
        var offset = BinaryPrimitives.ReadUInt32LittleEndian(src.Slice(4, 4));
        return (len, offset);
    }

    protected static byte[] ReadBytes(ReadOnlySpan<byte> raw, ushort length, uint offset)
    {
        if (length == 0)
        {
            return Array.Empty<byte>();
        }

        if (offset > (uint)raw.Length || length > raw.Length - (int)offset)
        {
            throw new ArgumentException("NTLM security buffer points outside the message.", nameof(raw));
        }

        return raw.Slice((int)offset, length).ToArray();
    }

    protected static ushort CheckedLength(int length)
    {
        if (length > ushort.MaxValue)
        {
            throw new InvalidOperationException("NTLM field length exceeds 65535 bytes.");
        }

        return (ushort)length;
    }

    protected static Encoding StringEncoding(NtlmFlags flags) =>
        (flags & NtlmFlags.NtlmsspNegotiateUnicode) != 0 ? Encoding.Unicode : Encoding.ASCII;
}
