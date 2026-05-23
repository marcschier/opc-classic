// SPDX-License-Identifier: EPL-1.0

namespace OpcClassic.Dcom.Internal.Ntlm;

public sealed class Type2Message : NtlmMessage
{
    public Type2Message()
        : base(new SharpCifs.Ntlmssp.Type2Message())
    {
    }

    public Type2Message(byte[] raw)
        : base(new SharpCifs.Ntlmssp.Type2Message(raw))
    {
    }

    public Type2Message(NtlmFlags flags, byte[] challenge, string target)
        : base(new SharpCifs.Ntlmssp.Type2Message(ToInt32(flags), challenge, target))
    {
    }

    public Type2Message(int flags, byte[] challenge, string target)
        : base(new SharpCifs.Ntlmssp.Type2Message(flags, challenge, target))
    {
    }

    public Type2Message(Type1Message type1Message)
        : base(new SharpCifs.Ntlmssp.Type2Message(type1Message.InnerMessage))
    {
    }

    public Type2Message(Type1Message type1Message, byte[] challenge, string target)
        : base(new SharpCifs.Ntlmssp.Type2Message(type1Message.InnerMessage, challenge, target))
    {
    }

    internal Type2Message(SharpCifs.Ntlmssp.Type2Message inner)
        : base(inner)
    {
    }

    public override int MessageType => 2;

    public string Target
    {
        get => InnerMessage.GetTarget();
        set => InnerMessage.SetTarget(value);
    }

    public byte[] GetChallenge() => InnerMessage.GetChallenge();

    public byte[] GetContext() => InnerMessage.GetContext();

    public static string GetDefaultDomain() => SharpCifs.Ntlmssp.Type2Message.GetDefaultDomain();

    public static NtlmFlags GetDefaultFlags() => FromInt32(SharpCifs.Ntlmssp.Type2Message.GetDefaultFlags());

    public static NtlmFlags GetDefaultFlags(Type1Message type1Message) =>
        FromInt32(SharpCifs.Ntlmssp.Type2Message.GetDefaultFlags(type1Message.InnerMessage));

    public static byte[] GetDefaultTargetInformation() => SharpCifs.Ntlmssp.Type2Message.GetDefaultTargetInformation();

    public string GetTarget() => InnerMessage.GetTarget();

    public byte[] GetTargetInformation() => InnerMessage.GetTargetInformation();

    public void SetChallenge(byte[] challenge) => InnerMessage.SetChallenge(challenge);

    public void SetContext(byte[] context) => InnerMessage.SetContext(context);

    public void SetTarget(string target) => InnerMessage.SetTarget(target);

    public void SetTargetInformation(byte[] targetInformation) => InnerMessage.SetTargetInformation(targetInformation);

    public override string ToString() => InnerMessage.ToString();

    public static implicit operator SharpCifs.Ntlmssp.Type2Message(Type2Message message) => message.InnerMessage;

    internal SharpCifs.Ntlmssp.Type2Message InnerMessage => (SharpCifs.Ntlmssp.Type2Message)Inner;
}
