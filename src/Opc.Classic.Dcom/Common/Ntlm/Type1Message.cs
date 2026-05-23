// SPDX-License-Identifier: MIT

using System;
using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public sealed class Type1Message : NtlmMessage
{
    private byte[]? _version;

    public Type1Message()
    {
        Flags = GetDefaultFlags();
        SuppliedDomain = GetDefaultDomain();
        SuppliedWorkstation = GetDefaultWorkstation();
    }

    public Type1Message(byte[] raw) => Parse(raw);

    public Type1Message(NtlmFlags flags, string suppliedDomain, string suppliedWorkstation)
    {
        Flags = flags;
        SuppliedDomain = suppliedDomain;
        SuppliedWorkstation = suppliedWorkstation;
    }

    public Type1Message(int flags, string suppliedDomain, string suppliedWorkstation)
        : this(FromInt32(flags), suppliedDomain, suppliedWorkstation)
    {
    }

    public override int MessageType => 1;

    public string? SuppliedDomain { get; set; }

    public string? SuppliedWorkstation { get; set; }

    public static string GetDefaultDomain() => string.Empty;

    public static NtlmFlags GetDefaultFlags() =>
        NtlmFlags.NtlmsspNegotiateUnicode | NtlmFlags.NtlmsspNegotiateNtlm;

    public static string GetDefaultWorkstation() => Environment.MachineName;

    public string? GetSuppliedDomain() => SuppliedDomain;

    public string? GetSuppliedWorkstation() => SuppliedWorkstation;

    public void SetSuppliedDomain(string suppliedDomain) => SuppliedDomain = suppliedDomain;

    public void SetSuppliedWorkstation(string suppliedWorkstation) => SuppliedWorkstation = suppliedWorkstation;

    public override byte[] ToByteArray()
    {
        var flags = Flags;
        var domainBytes = string.IsNullOrEmpty(SuppliedDomain)
            ? Array.Empty<byte>()
            : Encoding.ASCII.GetBytes(SuppliedDomain);
        var workstationBytes = string.IsNullOrEmpty(SuppliedWorkstation)
            ? Array.Empty<byte>()
            : Encoding.ASCII.GetBytes(SuppliedWorkstation);

        if (domainBytes.Length != 0)
        {
            flags |= NtlmFlags.NtlmsspNegotiateOemDomainSupplied;
        }

        if (workstationBytes.Length != 0)
        {
            flags |= NtlmFlags.NtlmsspNegotiateOemWorkstationSupplied;
        }

        var includeVersion = (flags & NtlmFlags.NtlmsspNegotiateVersion) != 0;
        var headerSize = includeVersion ? 40 : 32;
        var buffer = new byte[headerSize + domainBytes.Length + workstationBytes.Length];
        var span = buffer.AsSpan();

        WriteHeader(span, MessageType);
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(12, 4), (uint)flags);
        WriteFields(span.Slice(16, 8), CheckedLength(domainBytes.Length), (uint)headerSize);
        WriteFields(span.Slice(24, 8), CheckedLength(workstationBytes.Length), (uint)(headerSize + domainBytes.Length));
        if (includeVersion)
        {
            (_version ?? DefaultVersion.ToArray()).AsSpan(0, Math.Min(_version?.Length ?? 8, 8)).CopyTo(span.Slice(32, 8));
        }

        domainBytes.CopyTo(span[headerSize..]);
        workstationBytes.CopyTo(span[(headerSize + domainBytes.Length)..]);
        return buffer;
    }

    public override string ToString() =>
        $"Type1Message[Flags=0x{(uint)Flags:X8}, Domain={SuppliedDomain}, Workstation={SuppliedWorkstation}]";

    private void Parse(byte[] raw)
    {
        ArgumentNullException.ThrowIfNull(raw);
        var span = raw.AsSpan();
        if (ReadMessageType(span) != MessageType)
        {
            throw new ArgumentException("Not a Type 1 message.", nameof(raw));
        }

        if (span.Length < 32)
        {
            throw new ArgumentException("NTLM Type 1 message too short.", nameof(raw));
        }

        Flags = (NtlmFlags)BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(12, 4));
        var (domainLength, domainOffset) = ReadFields(span.Slice(16, 8));
        var (workstationLength, workstationOffset) = ReadFields(span.Slice(24, 8));
        if ((Flags & NtlmFlags.NtlmsspNegotiateVersion) != 0 && span.Length >= 40)
        {
            _version = span.Slice(32, 8).ToArray();
        }

        SuppliedDomain = domainLength == 0
            ? string.Empty
            : Encoding.ASCII.GetString(ReadBytes(span, domainLength, domainOffset));
        SuppliedWorkstation = workstationLength == 0
            ? string.Empty
            : Encoding.ASCII.GetString(ReadBytes(span, workstationLength, workstationOffset));
    }
}
