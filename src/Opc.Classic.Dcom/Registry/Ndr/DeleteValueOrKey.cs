// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Registry;

/// <inheritdoc/>
public class DeleteValueOrKey : NdrOp
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public PolicyHandle parentKey;
    public string valueName;
    public bool isKey;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override int Opnum => isKey ? 7 : 8;

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr)
    {
        // Write parent handle
        ndr.WriteOctetArray(parentKey.Handle, 0, 20);
        // key len, since it is uint16
        ndr.WriteUnsignedShort((valueName.Length + 1) * 2);
        // key size, since it is uint16
        ndr.WriteUnsignedShort((valueName.Length + 1) * 2);

        // it's a pointer - referent
        ndr.WriteUnsignedLong(new object().GetHashCode());
        // max count
        ndr.WriteUnsignedLong(valueName.Length + 1);
        // offset
        ndr.WriteUnsignedLong(0);
        // actual count
        ndr.WriteUnsignedLong(valueName.Length + 1);

        var i = 0;
        while (i < valueName.Length)
        {
            ndr.WriteUnsignedShort(valueName[i]);
            i++;
        }
        // null termination
        ndr.WriteUnsignedShort(0);
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr)
    {
        var hresult = ndr.ReadUnsignedLong();
        if (hresult != 0)
        {
            throw new InteropRuntimeException(hresult);
        }
    }
}
