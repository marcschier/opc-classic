// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public abstract class NtlmMessage
{
    public const string NtlmSignature = "NTLMSSP";

    private static readonly byte[] SignatureBytes = Encoding.ASCII.GetBytes("NTLMSSP\0");

    internal const int DefaultMaxMessageSize = 64 * 1024 - 1;

    protected static ReadOnlySpan<byte> DefaultVersion => [10, 0, 0, 0, 0, 0, 0, 15];
    public abstract int MessageType { get; }
    public NtlmFlags Flags { get; set; }

    public abstract byte[] ToByteArray();
    public bool GetFlag(NtlmFlags flag) => (Flags & flag) != NtlmFlags.None;

    public void SetFlag(NtlmFlags flag, bool value) =>
        Flags = value ? (Flags | flag) : (Flags & ~flag);

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

    protected static void ValidateMessageLength(ReadOnlySpan<byte> raw, string messageName, int maxMessageSize)
    {
        if (maxMessageSize <= 0 || maxMessageSize > DefaultMaxMessageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxMessageSize),
                maxMessageSize,
                $"NTLM message quota must be 1..{DefaultMaxMessageSize} bytes.");
        }

        if (raw.Length > maxMessageSize)
        {
            throw new ArgumentException(
                $"{messageName} length {raw.Length} exceeds the configured NTLMSSP quota of {maxMessageSize} bytes.",
                nameof(raw));
        }
    }

    protected static void ValidateSecurityBufferLayout(
        ReadOnlySpan<byte> raw,
        int headerSize,
        string messageName,
        params (ushort Length, uint Offset, string Name)[] fields)
    {
        var ranges = new (uint Start, uint End, string Name)[fields.Length];
        var rangeCount = 0;
        foreach (var field in fields)
        {
            if (field.Length == 0)
            {
                continue;
            }

            if (field.Offset < headerSize)
            {
                throw new ArgumentException(
                    $"{messageName} {field.Name} security buffer overlaps the NTLMSSP header.",
                    nameof(raw));
            }

            ulong end = (ulong)field.Offset + field.Length;
            if (field.Offset > raw.Length || end > (uint)raw.Length)
            {
                throw new ArgumentException(
                    $"{messageName} {field.Name} security buffer points outside the message.",
                    nameof(raw));
            }

            ranges[rangeCount++] = (field.Offset, (uint)end, field.Name);
        }

        for (var i = 0; i < rangeCount; i++)
        {
            for (var j = i + 1; j < rangeCount; j++)
            {
                if (ranges[i].Start < ranges[j].End && ranges[j].Start < ranges[i].End)
                {
                    throw new ArgumentException(
                        $"{messageName} security buffers {ranges[i].Name} and {ranges[j].Name} overlap.",
                        nameof(raw));
                }
            }
        }
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
        (flags & NtlmFlags.NtlmsspNegotiateUnicode) != NtlmFlags.None ? Encoding.Unicode : Encoding.ASCII;
}
