// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Registry;

/// <inheritdoc/>
public class CreateKey : NdrOp {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public PolicyHandle parentKey;
    public string key;
    public RegKeyAccess accessMask = (RegKeyAccess)(-1);
    public RegOption options = (RegOption)(-1);
    public int actiontaken = -1;
    public byte[] policyhandle = new byte[20];
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override int Opnum => 6;

    /// <inheritdoc/>
#pragma warning disable MA0051 // Legacy NDR encode mirrors the WinReg wire layout.
    public override void Write(NdrCodec ndr) {
        // Write parent handle
        ndr.WriteOctetArray(parentKey.Handle, 0, 20);

        // key len, since it is uint16
        ndr.WriteUnsignedShort((key.Length + 1) * 2);
        // key size, since it is uint16
        ndr.WriteUnsignedShort((key.Length + 1) * 2);

        // it's a pointer
        // referent
        ndr.WriteUnsignedLong(new object().GetHashCode());
        // max count
        ndr.WriteUnsignedLong(key.Length + 1);
        // offset
        ndr.WriteUnsignedLong(0);
        // actual count
        ndr.WriteUnsignedLong(key.Length + 1);

        var i = 0;
        while (i < key.Length) {
            ndr.WriteUnsignedShort(key[i]);
            i++;
        }

        // null termination
        ndr.WriteUnsignedShort(0);

        // now align for int
        ndr.FillAligned(4);

        // Write the class
        var clazz = "REG_SZ";
        // clazz len, since it is uint16
        ndr.WriteUnsignedShort((clazz.Length + 1) * 2);
        // clazz size, since it is uint16
        ndr.WriteUnsignedShort((clazz.Length + 1) * 2);

        // referent
        ndr.WriteUnsignedLong(new object().GetHashCode());
        // max count
        ndr.WriteUnsignedLong(clazz.Length + 1);
        // offset
        ndr.WriteUnsignedLong(0);
        // actual count
        ndr.WriteUnsignedLong(clazz.Length + 1);

        i = 0;
        while (i < clazz.Length) {
            ndr.WriteUnsignedShort(clazz[i]);
            i++;
        }

        // null termination
        ndr.WriteUnsignedShort(0);

        // now align for int
        ndr.FillAligned(4);

        // options
        ndr.WriteUnsignedLong((int)options);

        ndr.WriteUnsignedLong((int)accessMask);

        // ptr to sec desc, null
        ndr.WriteUnsignedLong(0);
        // pointer to action taken
        ndr.WriteUnsignedLong(new object().GetHashCode());
        ndr.WriteUnsignedLong(0);
    }
#pragma warning restore MA0051

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr) {
        ndr.ReadOctetArray(policyhandle, 0, 20);
        // pointer to action taken
        ndr.ReadUnsignedLong();
        actiontaken = ndr.ReadUnsignedLong();
        var hresult = ndr.ReadUnsignedLong();
        if (hresult != 0) {
            throw new InteropRuntimeException(hresult);
        }
    }

}
