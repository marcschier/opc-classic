// SPDX-License-Identifier: EPL-1.0

namespace OpcClassic.Dcom.Internal.Ntlm;

public sealed class Type1Message : NtlmMessage
{
    public Type1Message()
        : base(new SharpCifs.Ntlmssp.Type1Message())
    {
    }

    public Type1Message(byte[] raw)
        : base(new SharpCifs.Ntlmssp.Type1Message(raw))
    {
    }

    public Type1Message(NtlmFlags flags, string suppliedDomain, string suppliedWorkstation)
        : base(new SharpCifs.Ntlmssp.Type1Message(ToInt32(flags), suppliedDomain, suppliedWorkstation))
    {
    }

    public Type1Message(int flags, string suppliedDomain, string suppliedWorkstation)
        : base(new SharpCifs.Ntlmssp.Type1Message(flags, suppliedDomain, suppliedWorkstation))
    {
    }

    internal Type1Message(SharpCifs.Ntlmssp.Type1Message inner)
        : base(inner)
    {
    }

    public override int MessageType => 1;

    public string SuppliedDomain
    {
        get => InnerMessage.GetSuppliedDomain();
        set => InnerMessage.SetSuppliedDomain(value);
    }

    public string SuppliedWorkstation
    {
        get => InnerMessage.GetSuppliedWorkstation();
        set => InnerMessage.SetSuppliedWorkstation(value);
    }

    public static string GetDefaultDomain() => SharpCifs.Ntlmssp.Type1Message.GetDefaultDomain();

    public static NtlmFlags GetDefaultFlags() => FromInt32(SharpCifs.Ntlmssp.Type1Message.GetDefaultFlags());

    public static string GetDefaultWorkstation() => SharpCifs.Ntlmssp.Type1Message.GetDefaultWorkstation();

    public string GetSuppliedDomain() => InnerMessage.GetSuppliedDomain();

    public string GetSuppliedWorkstation() => InnerMessage.GetSuppliedWorkstation();

    public void SetSuppliedDomain(string suppliedDomain) => InnerMessage.SetSuppliedDomain(suppliedDomain);

    public void SetSuppliedWorkstation(string suppliedWorkstation) => InnerMessage.SetSuppliedWorkstation(suppliedWorkstation);

    public override string ToString() => InnerMessage.ToString();

    public static implicit operator SharpCifs.Ntlmssp.Type1Message(Type1Message message) => message.InnerMessage;

    internal SharpCifs.Ntlmssp.Type1Message InnerMessage => (SharpCifs.Ntlmssp.Type1Message)Inner;
}
