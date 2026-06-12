// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

[Serializable]
public sealed class CustomInterfacePointerBody : InterfacePointerBody
{
    internal CustomInterfacePointerBody(
        string iid,
        string clsid,
        int cbExtension,
        int reserved,
        byte[] objectData)
        : base(InterfacePointer.OBJREF_CUSTOM, iid, null, null)
    {
        CustomCLSID = clsid;
        ExtensionSize = cbExtension;
        Reserved = reserved;
        ObjectData = objectData ?? Array.Empty<byte>();
        Length = GetEncodedLength();
    }

    public string CLSID => CustomCLSID;

    public int ExtensionSize { get; private set; }

    public int Reserved { get; private set; }

    public byte[] ObjectData { get; private set; }

    internal static CustomInterfacePointerBody Decode(NdrCodec ndr, string iid, int bodyLength)
    {
        var clsid = ReadUuid(ndr, "OBJREF_CUSTOM decode");
        var cbExtension = ndr.ReadUnsignedLong();
        var reserved = ndr.ReadUnsignedLong();
        var objectDataLength = bodyLength > 0
            ? Math.Max(0, bodyLength - CustomBodyHeaderLength)
            : GetRemainingByteCount(ndr);
        var objectData = ReadRemainingBytes(ndr, objectDataLength);
        return new CustomInterfacePointerBody(iid, clsid, cbExtension, reserved, objectData);
    }

    protected override int GetEncodedLength() =>
        ObjRefHeaderLength + CustomBodyHeaderLength + ObjectData.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags)
    {
        WriteUuid(ndr, CustomCLSID, "OBJREF_CUSTOM encode");
        ndr.WriteUnsignedLong(ExtensionSize);
        ndr.WriteUnsignedLong(Reserved);
        if (ObjectData.Length > 0)
        {
            ndr.WriteOctetArray(ObjectData, 0, ObjectData.Length);
        }
    }
}
