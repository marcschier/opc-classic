// SPDX-License-Identifier: EPL-1.0

namespace OpcClassic.Dcom.Internal.Ntlm;

public sealed class Type3Message : NtlmMessage
{
    public Type3Message()
        : base(new SharpCifs.Ntlmssp.Type3Message())
    {
    }

    public Type3Message(byte[] raw)
        : base(new SharpCifs.Ntlmssp.Type3Message(raw))
    {
    }

    public Type3Message(NtlmFlags flags, byte[] lmResponse, byte[] ntResponse,
        string domain, string user, string workstation)
        : base(new SharpCifs.Ntlmssp.Type3Message(ToInt32(flags), lmResponse, ntResponse,
            domain, user, workstation))
    {
    }

    public Type3Message(int flags, byte[] lmResponse, byte[] ntResponse,
        string domain, string user, string workstation)
        : base(new SharpCifs.Ntlmssp.Type3Message(flags, lmResponse, ntResponse,
            domain, user, workstation))
    {
    }

    public Type3Message(Type2Message type2Message)
        : base(new SharpCifs.Ntlmssp.Type3Message(type2Message.InnerMessage))
    {
    }

    public Type3Message(Type2Message type2Message, string password, string domain,
        string user, string workstation, NtlmFlags flags)
        : base(new SharpCifs.Ntlmssp.Type3Message(type2Message.InnerMessage, password,
            domain, user, workstation, ToInt32(flags)))
    {
    }

    public Type3Message(Type2Message type2Message, string password, string domain,
        string user, string workstation, int flags)
        : base(new SharpCifs.Ntlmssp.Type3Message(type2Message.InnerMessage, password,
            domain, user, workstation, flags))
    {
    }

    internal Type3Message(SharpCifs.Ntlmssp.Type3Message inner)
        : base(inner)
    {
    }

    public override int MessageType => 3;

    public string Domain
    {
        get => InnerMessage.GetDomain();
        set => InnerMessage.SetDomain(value);
    }

    public string User
    {
        get => InnerMessage.GetUser();
        set => InnerMessage.SetUser(value);
    }

    public string Workstation
    {
        get => InnerMessage.GetWorkstation();
        set => InnerMessage.SetWorkstation(value);
    }

    public static string GetDefaultDomain() => SharpCifs.Ntlmssp.Type3Message.GetDefaultDomain();

    public static NtlmFlags GetDefaultFlags() => FromInt32(SharpCifs.Ntlmssp.Type3Message.GetDefaultFlags());

    public static NtlmFlags GetDefaultFlags(Type2Message type2Message) =>
        FromInt32(SharpCifs.Ntlmssp.Type3Message.GetDefaultFlags(type2Message.InnerMessage));

    public static string GetDefaultPassword() => SharpCifs.Ntlmssp.Type3Message.GetDefaultPassword();

    public static string GetDefaultUser() => SharpCifs.Ntlmssp.Type3Message.GetDefaultUser();

    public static string GetDefaultWorkstation() => SharpCifs.Ntlmssp.Type3Message.GetDefaultWorkstation();

    public static byte[] GetLMResponse(Type2Message type2Message, string password) =>
        SharpCifs.Ntlmssp.Type3Message.GetLMResponse(type2Message.InnerMessage, password);

    public static byte[] GetLMv2Response(Type2Message type2Message, string domain,
        string user, string password, byte[] clientChallenge) =>
        SharpCifs.Ntlmssp.Type3Message.GetLMv2Response(type2Message.InnerMessage,
            domain, user, password, clientChallenge);

    public static byte[] GetNtlMv2Response(Type2Message type2Message, byte[] responseKeyNT,
        byte[] clientChallenge) =>
        SharpCifs.Ntlmssp.Type3Message.GetNtlMv2Response(type2Message.InnerMessage,
            responseKeyNT, clientChallenge);

    public static byte[] GetNTResponse(Type2Message type2Message, string password) =>
        SharpCifs.Ntlmssp.Type3Message.GetNTResponse(type2Message.InnerMessage, password);

    public string GetDomain() => InnerMessage.GetDomain();

    public byte[] GetLMResponse() => InnerMessage.GetLMResponse();

    public byte[] GetMasterKey() => InnerMessage.GetMasterKey();

    public byte[] GetNTResponse() => InnerMessage.GetNTResponse();

    public byte[] GetSessionKey() => InnerMessage.GetSessionKey();

    public string GetUser() => InnerMessage.GetUser();

    public string GetWorkstation() => InnerMessage.GetWorkstation();

    public void SetDomain(string domain) => InnerMessage.SetDomain(domain);

    public void SetLmResponse(byte[] lmResponse) => InnerMessage.SetLmResponse(lmResponse);

    public void SetNtResponse(byte[] ntResponse) => InnerMessage.SetNtResponse(ntResponse);

    public void SetSessionKey(byte[] sessionKey) => InnerMessage.SetSessionKey(sessionKey);

    public void SetUser(string user) => InnerMessage.SetUser(user);

    public void SetWorkstation(string workstation) => InnerMessage.SetWorkstation(workstation);

    public override string ToString() => InnerMessage.ToString();

    public static implicit operator SharpCifs.Ntlmssp.Type3Message(Type3Message message) => message.InnerMessage;

    internal static Type3Message FromObject(object message)
    {
        return message switch
        {
            Type3Message wrapper => wrapper,
            SharpCifs.Ntlmssp.Type3Message inner => new Type3Message(inner),
            _ => throw new System.ArgumentException("Expected an NTLM Type3 message.", nameof(message)),
        };
    }

    internal SharpCifs.Ntlmssp.Type3Message InnerMessage => (SharpCifs.Ntlmssp.Type3Message)Inner;
}
