// SPDX-License-Identifier: EPL-1.0

namespace OpcClassic.Dcom.Internal.Ntlm;

public abstract class NtlmMessage
{
    public const string NtlmSignature = "NTLMSSP";

    protected NtlmMessage(SharpCifs.Ntlmssp.NtlmMessage inner)
    {
        Inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
    }

    internal SharpCifs.Ntlmssp.NtlmMessage Inner { get; }

    public abstract int MessageType { get; }

    public NtlmFlags Flags
    {
        get => FromInt32(Inner.GetFlags());
        set => Inner.SetFlags(ToInt32(value));
    }

    public NtlmFlags GetFlags() => Flags;

    public void SetFlags(NtlmFlags flags) => Flags = flags;

    public void SetFlags(int flags) => Inner.SetFlags(flags);

    public byte[] ToByteArray() => Inner.ToByteArray();

    public bool GetFlag(NtlmFlags flag) => Inner.GetFlag(ToInt32(flag));

    public bool GetFlag(int flag) => Inner.GetFlag(flag);

    public void SetFlag(NtlmFlags flag, bool value) => Inner.SetFlag(ToInt32(flag), value);

    public void SetFlag(int flag, bool value) => Inner.SetFlag(flag, value);

    internal static NtlmFlags FromInt32(int flags) => (NtlmFlags)unchecked((uint)flags);

    internal static int ToInt32(NtlmFlags flags) => unchecked((int)(uint)flags);
}
