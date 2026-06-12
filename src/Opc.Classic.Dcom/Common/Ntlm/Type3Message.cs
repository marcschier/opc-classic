// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public sealed class Type3Message : NtlmMessage
{
    public const int MicOffset = 72;
    public const int MicLength = NtlmMic.MicLength;

    private byte[]? _version;
    private byte[]? _mic;

    public Type3Message()
    {
        Flags = GetDefaultFlags();
        Domain = GetDefaultDomain();
        User = GetDefaultUser();
        Workstation = GetDefaultWorkstation();
        LmResponse = Array.Empty<byte>();
        NtResponse = Array.Empty<byte>();
        EncryptedRandomSessionKey = Array.Empty<byte>();
    }

    public Type3Message(byte[] raw) => Parse(raw, DefaultMaxMessageSize);

    internal Type3Message(byte[] raw, int maxMessageSize) => Parse(raw, maxMessageSize);

    public Type3Message(NtlmFlags flags, byte[] lmResponse, byte[] ntResponse,
        string domain, string user, string workstation)
    {
        Flags = flags;
        LmResponse = CloneOrEmpty(lmResponse);
        NtResponse = CloneOrEmpty(ntResponse);
        Domain = domain;
        User = user;
        Workstation = workstation;
        EncryptedRandomSessionKey = Array.Empty<byte>();
    }

    public Type3Message(int flags, byte[] lmResponse, byte[] ntResponse,
        string domain, string user, string workstation)
        : this(FromInt32(flags), lmResponse, ntResponse, domain, user, workstation)
    {
    }

    public Type3Message(Type2Message type2Message)
        : this(GetDefaultFlags(type2Message), Array.Empty<byte>(), Array.Empty<byte>(),
            type2Message?.GetTarget() ?? GetDefaultDomain(), GetDefaultUser(), GetDefaultWorkstation())
    {
    }

    public Type3Message(Type2Message type2Message, string password, string domain,
        string user, string workstation, NtlmFlags flags)
        : this(flags, GetLMResponse(type2Message, password), GetNTResponse(type2Message, password),
            domain, user, workstation)
    {
    }

    public Type3Message(Type2Message type2Message, string password, string domain,
        string user, string workstation, int flags)
        : this(type2Message, password, domain, user, workstation, FromInt32(flags))
    {
    }

    public override int MessageType => 3;

    public byte[]? LmResponse { get; set; }

    public byte[]? NtResponse { get; set; }

    public string? Domain { get; set; }

    public string? User { get; set; }

    public string? Username
    {
        get => User;
        set => User = value;
    }

    public string? Workstation { get; set; }

    public byte[]? EncryptedRandomSessionKey { get; set; }

    public static string GetDefaultDomain() => string.Empty;

    public static NtlmFlags GetDefaultFlags() =>
        NtlmFlags.NtlmsspNegotiateUnicode | NtlmFlags.NtlmsspNegotiateNtlm;

    public static NtlmFlags GetDefaultFlags(Type2Message type2Message) =>
        type2Message?.Flags ?? GetDefaultFlags();

    public static string GetDefaultPassword() => string.Empty;

    public static string GetDefaultUser() => string.Empty;

    public static string GetDefaultWorkstation() => Environment.MachineName;

    public static byte[] GetLMResponse(Type2Message type2Message, string password)
    {
        ArgumentNullException.ThrowIfNull(type2Message);
        return Responses.GetLMResponse(password, type2Message.GetChallenge());
    }

    public static byte[] GetLMv2Response(Type2Message type2Message, string domain,
        string user, string password, byte[] clientChallenge)
    {
        ArgumentNullException.ThrowIfNull(type2Message);
        return Responses.GetLMv2Response(domain, user, password, type2Message.GetChallenge(), clientChallenge);
    }

    public static byte[] GetNtlMv2Response(Type2Message type2Message, byte[] responseKeyNT,
        byte[] clientChallenge)
    {
        ArgumentNullException.ThrowIfNull(type2Message);
        ArgumentNullException.ThrowIfNull(responseKeyNT);
        ArgumentNullException.ThrowIfNull(clientChallenge);

        var blob = Responses.CreateBlob(type2Message.GetTargetInformation(), clientChallenge);
        var challenge = type2Message.GetChallenge();
        var proofInput = new byte[challenge.Length + blob.Length];
        Array.Copy(challenge, 0, proofInput, 0, challenge.Length);
        Array.Copy(blob, 0, proofInput, challenge.Length, blob.Length);
        var proof = Responses.HmacMD5(proofInput, responseKeyNT);
        var response = new byte[proof.Length + blob.Length];
        Array.Copy(proof, 0, response, 0, proof.Length);
        Array.Copy(blob, 0, response, proof.Length, blob.Length);
        return response;
    }

    public static byte[] GetNTResponse(Type2Message type2Message, string password)
    {
        ArgumentNullException.ThrowIfNull(type2Message);
        return Responses.GetNTLMResponse(password, type2Message.GetChallenge());
    }

    public string? GetDomain() => Domain;

    public byte[] GetLMResponse() => CloneOrEmpty(LmResponse);

    public byte[] GetMasterKey() => CloneOrEmpty(EncryptedRandomSessionKey);

    public byte[] GetNTResponse() => CloneOrEmpty(NtResponse);

    public byte[] GetSessionKey() => CloneOrEmpty(EncryptedRandomSessionKey);

    public byte[] GetMic() => CloneOrEmpty(_mic);

    public bool HasMic => _mic is { Length: MicLength };

    public string? GetUser() => User;

    public string? GetWorkstation() => Workstation;

    public void SetDomain(string domain) => Domain = domain;

    public void SetLmResponse(byte[] lmResponse) => LmResponse = CloneOrEmpty(lmResponse);

    public void SetNtResponse(byte[] ntResponse) => NtResponse = CloneOrEmpty(ntResponse);

    public void SetSessionKey(byte[] sessionKey) => EncryptedRandomSessionKey = CloneOrEmpty(sessionKey);

    public void SetMic(byte[] mic)
    {
        ArgumentNullException.ThrowIfNull(mic);
        if (mic.Length != MicLength)
        {
            throw new ArgumentException("NTLM MIC must be exactly 16 bytes.", nameof(mic));
        }

        _mic = (byte[])mic.Clone();
    }

    public void ClearMic() => _mic = null;

    public void SetUser(string user) => User = user;

    public void SetWorkstation(string workstation) => Workstation = workstation;

    public override byte[] ToByteArray()
    {
        var encoding = StringEncoding(Flags);
        var lmResponse = LmResponse ?? Array.Empty<byte>();
        var ntResponse = NtResponse ?? Array.Empty<byte>();
        var domainBytes = string.IsNullOrEmpty(Domain) ? Array.Empty<byte>() : encoding.GetBytes(Domain);
        var userBytes = string.IsNullOrEmpty(User) ? Array.Empty<byte>() : encoding.GetBytes(User);
        var workstationBytes = string.IsNullOrEmpty(Workstation) ? Array.Empty<byte>() : encoding.GetBytes(Workstation);
        var sessionKey = EncryptedRandomSessionKey ?? Array.Empty<byte>();
        var includeMic = _mic is { Length: 16 };
        var includeVersion = (Flags & NtlmFlags.NtlmsspNegotiateVersion) != NtlmFlags.None || includeMic;
        var headerSize = 64 + (includeVersion ? 8 : 0) + (includeMic ? 16 : 0);

        var buffer = new byte[headerSize + lmResponse.Length + ntResponse.Length + domainBytes.Length +
            userBytes.Length + workstationBytes.Length + sessionKey.Length];
        var span = buffer.AsSpan();
        var offset = headerSize;

        WriteHeader(span, MessageType);
        WritePayloadFields(span.Slice(12, 8), lmResponse, ref offset);
        WritePayloadFields(span.Slice(20, 8), ntResponse, ref offset);
        WritePayloadFields(span.Slice(28, 8), domainBytes, ref offset);
        WritePayloadFields(span.Slice(36, 8), userBytes, ref offset);
        WritePayloadFields(span.Slice(44, 8), workstationBytes, ref offset);
        WritePayloadFields(span.Slice(52, 8), sessionKey, ref offset);
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(60, 4), (uint)Flags);
        if (includeVersion)
        {
            (_version ?? DefaultVersion.ToArray()).AsSpan(0, Math.Min(_version?.Length ?? 8, 8)).CopyTo(span.Slice(64, 8));
        }
        if (includeMic)
        {
            _mic!.CopyTo(span.Slice(MicOffset, MicLength));
        }

        offset = headerSize;
        CopyPayload(lmResponse, span, ref offset);
        CopyPayload(ntResponse, span, ref offset);
        CopyPayload(domainBytes, span, ref offset);
        CopyPayload(userBytes, span, ref offset);
        CopyPayload(workstationBytes, span, ref offset);
        CopyPayload(sessionKey, span, ref offset);
        return buffer;
    }

    public byte[] ToByteArrayWithMic(byte[] sessionKey, ReadOnlySpan<byte> negotiate, ReadOnlySpan<byte> challenge)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);

        var previousMic = _mic;
        _mic = new byte[MicLength];
        try
        {
            var authenticate = ToByteArray();
            var mic = NtlmMic.Compute(sessionKey, negotiate, challenge, authenticate);
            mic.CopyTo(authenticate.AsSpan(MicOffset, MicLength));
            _mic = mic;
            return authenticate;
        }
        catch
        {
            _mic = previousMic;
            throw;
        }
    }

    public override string ToString() =>
        $"Type3Message[Flags=0x{(uint)Flags:X8}, Domain={Domain}, User={User}, Workstation={Workstation}]";

    internal static Type3Message FromObject(object message) =>
        message switch
        {
            Type3Message wrapper => wrapper,
            _ => throw new ArgumentException("Expected an NTLM Type3 message.", nameof(message)),
        };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "NTLM Type3 message field parsing must remain inlined for byte-offset correlation with MS-NLMP §2.2.1.3.")]
    private void Parse(byte[] raw, int maxMessageSize)
    {
        ArgumentNullException.ThrowIfNull(raw);
        var span = raw.AsSpan();
        ValidateMessageLength(span, "NTLM Type 3 message", maxMessageSize);
        if (ReadMessageType(span) != MessageType)
        {
            throw new ArgumentException("Not a Type 3 message.", nameof(raw));
        }

        if (span.Length < 64)
        {
            throw new ArgumentException("NTLM Type 3 message too short.", nameof(raw));
        }

        var lmFields = ReadFields(span.Slice(12, 8));
        var ntFields = ReadFields(span.Slice(20, 8));
        var domainFields = ReadFields(span.Slice(28, 8));
        var userFields = ReadFields(span.Slice(36, 8));
        var workstationFields = ReadFields(span.Slice(44, 8));
        var sessionKeyFields = ReadFields(span.Slice(52, 8));
        Flags = (NtlmFlags)BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(60, 4));

        var minimumPayloadOffset = MinimumPayloadOffset(
            lmFields, ntFields, domainFields, userFields, workstationFields, sessionKeyFields);
        var headerSize = 64;
        if ((Flags & NtlmFlags.NtlmsspNegotiateVersion) != NtlmFlags.None)
        {
            if (span.Length < 72)
            {
                throw new ArgumentException("NTLM Type 3 message version flag set but version field is truncated.", nameof(raw));
            }
            headerSize = 72;
            if (minimumPayloadOffset >= 72)
            {
                _version = span.Slice(64, 8).ToArray();
            }
        }
        if (span.Length >= MicOffset + MicLength && minimumPayloadOffset >= MicOffset + MicLength)
        {
            headerSize = MicOffset + MicLength;
            _mic = span.Slice(MicOffset, MicLength).ToArray();
        }
        ValidateSecurityBufferLayout(
            span,
            headerSize,
            "NTLM Type 3 message",
            (lmFields.Length, lmFields.Offset, nameof(LmResponse)),
            (ntFields.Length, ntFields.Offset, nameof(NtResponse)),
            (domainFields.Length, domainFields.Offset, nameof(Domain)),
            (userFields.Length, userFields.Offset, nameof(User)),
            (workstationFields.Length, workstationFields.Offset, nameof(Workstation)),
            (sessionKeyFields.Length, sessionKeyFields.Offset, nameof(EncryptedRandomSessionKey)));

        var encoding = StringEncoding(Flags);
        LmResponse = ReadBytes(span, lmFields.Length, lmFields.Offset);
        NtResponse = ReadBytes(span, ntFields.Length, ntFields.Offset);
        Domain = domainFields.Length == 0 ? string.Empty : encoding.GetString(ReadBytes(span, domainFields.Length, domainFields.Offset));
        User = userFields.Length == 0 ? string.Empty : encoding.GetString(ReadBytes(span, userFields.Length, userFields.Offset));
        Workstation = workstationFields.Length == 0
            ? string.Empty
            : encoding.GetString(ReadBytes(span, workstationFields.Length, workstationFields.Offset));
        EncryptedRandomSessionKey = ReadBytes(span, sessionKeyFields.Length, sessionKeyFields.Offset);
    }

    private static byte[] CloneOrEmpty(byte[]? source) =>
        source is null ? Array.Empty<byte>() : (byte[])source.Clone();

    private static void WritePayloadFields(Span<byte> fields, byte[] payload, ref int offset)
    {
        WriteFields(fields, CheckedLength(payload.Length), (uint)offset);
        offset += payload.Length;
    }

    private static void CopyPayload(byte[] payload, Span<byte> destination, ref int offset)
    {
        payload.CopyTo(destination[offset..]);
        offset += payload.Length;
    }

    private static uint MinimumPayloadOffset(params (ushort Length, uint Offset)[] fields)
    {
        var minimum = uint.MaxValue;
        foreach (var field in fields)
        {
            if (field.Length != 0 && field.Offset < minimum)
            {
                minimum = field.Offset;
            }
        }

        return minimum == uint.MaxValue ? 0 : minimum;
    }
}
