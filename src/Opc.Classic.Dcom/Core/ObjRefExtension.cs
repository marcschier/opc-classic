// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc.Core;
using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

[Serializable]
public sealed class ObjRefExtension {
    public ObjRefExtension(string dataId, byte[] payload)
        : this(dataId, payload?.Length ?? 0, RoundUpToEight(payload?.Length ?? 0), PadToEight(payload)) {
    }

    private ObjRefExtension(string dataId, int size, int roundedSize, byte[] data) {
        DataId = dataId;
        Size = size;
        RoundedSize = roundedSize;
        Data = data ?? Array.Empty<byte>();
    }

    public string DataId { get; }

    public int Size { get; }

    public int RoundedSize { get; }

    public byte[] Data { get; }

    public int Length => 16 + 4 + 4 + RoundedSize;

    internal static ObjRefExtension Decode(NdrCodec ndr) {
        var dataId = InterfacePointerBody.ReadUuid(ndr, "OBJREF_EXTENDED extension decode");
        var size = ndr.ReadUnsignedLong();
        var roundedSize = ndr.ReadUnsignedLong();
        var data = InterfacePointerBody.ReadRemainingBytes(ndr, roundedSize);
        return new ObjRefExtension(dataId, size, roundedSize, data);
    }

    internal void Encode(NdrCodec ndr) {
        InterfacePointerBody.WriteUuid(ndr, DataId, "OBJREF_EXTENDED extension encode");
        ndr.WriteUnsignedLong(Size);
        ndr.WriteUnsignedLong(RoundedSize);
        if (Data.Length > 0) {
            ndr.WriteOctetArray(Data, 0, Data.Length);
        }
    }

    private static int RoundUpToEight(int length) => (length + 7) & ~7;

    private static byte[] PadToEight(byte[] payload) {
        if (payload == null || payload.Length == 0) {
            return Array.Empty<byte>();
        }
        var data = new byte[RoundUpToEight(payload.Length)];
        Buffer.BlockCopy(payload, 0, data, 0, payload.Length);
        return data;
    }
}
