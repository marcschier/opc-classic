// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

[Serializable]
public sealed class UnknownInterfacePointerBody : InterfacePointerBody
{
    internal UnknownInterfacePointerBody(int objectType, string iid, byte[] rawBytes)
        : base(objectType, iid, null, null)
    {
        RawBytes = rawBytes ?? Array.Empty<byte>();
        Length = GetEncodedLength();
    }

    public byte[] RawBytes { get; }

    internal static UnknownInterfacePointerBody Decode(NdrCodec ndr, int objectType, string iid, int bodyLength) =>
        new UnknownInterfacePointerBody(objectType, iid, ReadRemainingBytes(ndr, bodyLength));

    protected override int GetEncodedLength() => ObjRefHeaderLength + RawBytes.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags)
    {
        if (RawBytes.Length > 0)
        {
            ndr.WriteOctetArray(RawBytes, 0, RawBytes.Length);
        }
    }
}
