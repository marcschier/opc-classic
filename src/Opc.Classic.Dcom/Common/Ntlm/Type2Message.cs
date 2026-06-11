// SPDX-License-Identifier: MIT

using System;
using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public sealed class Type2Message : NtlmMessage
{
    private byte[]? _version;

    public Type2Message()
    {
        Flags = GetDefaultFlags();
        Challenge = new byte[8];
        Context = new byte[8];
        Target = GetDefaultDomain();
        TargetInformation = GetDefaultTargetInformation();
    }

    public Type2Message(byte[] raw) => Parse(raw, DefaultMaxMessageSize);

    internal Type2Message(byte[] raw, int maxMessageSize) => Parse(raw, maxMessageSize);

    public Type2Message(NtlmFlags flags, byte[] challenge, string target)
    {
        Flags = flags;
        SetChallenge(challenge);
        Context = new byte[8];
        Target = target;
        TargetInformation = GetDefaultTargetInformation();
    }

    public Type2Message(int flags, byte[] challenge, string target)
        : this(FromInt32(flags), challenge, target)
    {
    }

    public Type2Message(Type1Message type1Message)
    {
        Flags = GetDefaultFlags(type1Message);
        Challenge = new byte[8];
        Context = new byte[8];
        Target = GetDefaultDomain();
        TargetInformation = GetDefaultTargetInformation();
    }

    public Type2Message(Type1Message type1Message, byte[] challenge, string target)
        : this(GetDefaultFlags(type1Message), challenge, target)
    {
    }

    public override int MessageType => 2;

    public string? Target { get; set; }

    public byte[]? Challenge { get; set; }

    public byte[]? TargetInformation { get; set; }

    public byte[]? Context { get; set; }

    public byte[] GetChallenge() => CloneOrEmpty(Challenge);

    public byte[] GetContext() => CloneOrEmpty(Context);

    public static string GetDefaultDomain() => string.Empty;

    public static NtlmFlags GetDefaultFlags() =>
        NtlmFlags.NtlmsspNegotiateUnicode | NtlmFlags.NtlmsspNegotiateNtlm;

    public static NtlmFlags GetDefaultFlags(Type1Message type1Message) =>
        type1Message?.Flags ?? GetDefaultFlags();

    public static byte[] GetDefaultTargetInformation()
    {
        var workstationBytes = Encoding.Unicode.GetBytes(Environment.MachineName);
        var targetInformation = new byte[4 + workstationBytes.Length + 4];
        BinaryPrimitives.WriteUInt16LittleEndian(targetInformation.AsSpan(0, 2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(targetInformation.AsSpan(2, 2), CheckedLength(workstationBytes.Length));
        workstationBytes.CopyTo(targetInformation.AsSpan(4));
        return targetInformation;
    }

    public string? GetTarget() => Target;

    public byte[] GetTargetInformation() => CloneOrEmpty(TargetInformation);

    public void SetChallenge(byte[] challenge)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        if (challenge.Length != 8)
        {
            throw new ArgumentException("NTLM server challenge must be exactly 8 bytes.", nameof(challenge));
        }

        Challenge = (byte[])challenge.Clone();
    }

    public void SetContext(byte[] context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Length != 8)
        {
            throw new ArgumentException("NTLM context must be exactly 8 bytes.", nameof(context));
        }

        Context = (byte[])context.Clone();
    }

    public void SetTarget(string target) => Target = target;

    public void SetTargetInformation(byte[] targetInformation)
    {
        ArgumentNullException.ThrowIfNull(targetInformation);
        TargetInformation = (byte[])targetInformation.Clone();
    }

    public override byte[] ToByteArray()
    {
        var flags = Flags;
        var encoding = StringEncoding(flags);
        var targetBytes = string.IsNullOrEmpty(Target) ? Array.Empty<byte>() : encoding.GetBytes(Target);
        var targetInformationBytes = TargetInformation ?? Array.Empty<byte>();
        if (targetInformationBytes.Length != 0)
        {
            flags |= NtlmFlags.NtlmsspNegotiateTargetInfo;
        }

        var includeVersion = (flags & NtlmFlags.NtlmsspNegotiateVersion) != NtlmFlags.None;
        var headerSize = includeVersion ? 56 : 48;
        var buffer = new byte[headerSize + targetBytes.Length + targetInformationBytes.Length];
        var span = buffer.AsSpan();

        WriteHeader(span, MessageType);
        WriteFields(span.Slice(12, 8), CheckedLength(targetBytes.Length), (uint)headerSize);
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(20, 4), (uint)flags);
        GetFixedBytes(Challenge, 8, nameof(Challenge)).CopyTo(span.Slice(24, 8));
        GetFixedBytes(Context, 8, nameof(Context)).CopyTo(span.Slice(32, 8));
        WriteFields(span.Slice(40, 8), CheckedLength(targetInformationBytes.Length), (uint)(headerSize + targetBytes.Length));
        if (includeVersion)
        {
            (_version ?? DefaultVersion.ToArray()).AsSpan(0, Math.Min(_version?.Length ?? 8, 8)).CopyTo(span.Slice(48, 8));
        }

        targetBytes.CopyTo(span[headerSize..]);
        targetInformationBytes.CopyTo(span[(headerSize + targetBytes.Length)..]);
        return buffer;
    }

    public override string ToString() =>
        $"Type2Message[Flags=0x{(uint)Flags:X8}, Target={Target}, Challenge={Convert.ToHexString(GetChallenge())}]";

    private void Parse(byte[] raw, int maxMessageSize)
    {
        ArgumentNullException.ThrowIfNull(raw);
        var span = raw.AsSpan();
        ValidateMessageLength(span, "NTLM Type 2 message", maxMessageSize);
        if (ReadMessageType(span) != MessageType)
        {
            throw new ArgumentException("Not a Type 2 message.", nameof(raw));
        }

        if (span.Length < 48)
        {
            throw new ArgumentException("NTLM Type 2 message too short.", nameof(raw));
        }

        var (targetLength, targetOffset) = ReadFields(span.Slice(12, 8));
        Flags = (NtlmFlags)BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(20, 4));
        Challenge = span.Slice(24, 8).ToArray();
        Context = span.Slice(32, 8).ToArray();
        var (targetInformationLength, targetInformationOffset) = ReadFields(span.Slice(40, 8));
        var headerSize = 48;
        if ((Flags & NtlmFlags.NtlmsspNegotiateVersion) != NtlmFlags.None)
        {
            if (span.Length < 56)
            {
                throw new ArgumentException("NTLM Type 2 message version flag set but version field is truncated.", nameof(raw));
            }
            headerSize = 56;
            _version = span.Slice(48, 8).ToArray();
        }
        ValidateSecurityBufferLayout(
            span,
            headerSize,
            "NTLM Type 2 message",
            (targetLength, targetOffset, nameof(Target)),
            (targetInformationLength, targetInformationOffset, nameof(TargetInformation)));

        var encoding = StringEncoding(Flags);
        Target = targetLength == 0
            ? string.Empty
            : encoding.GetString(ReadBytes(span, targetLength, targetOffset));
        TargetInformation = ReadBytes(span, targetInformationLength, targetInformationOffset);
    }

    private static byte[] CloneOrEmpty(byte[]? source) =>
        source is null ? Array.Empty<byte>() : (byte[])source.Clone();

    private static byte[] GetFixedBytes(byte[]? source, int expectedLength, string name)
    {
        if (source is null)
        {
            return new byte[expectedLength];
        }

        if (source.Length != expectedLength)
        {
            throw new InvalidOperationException($"NTLM {name} must be exactly {expectedLength} bytes.");
        }

        return source;
    }
}
