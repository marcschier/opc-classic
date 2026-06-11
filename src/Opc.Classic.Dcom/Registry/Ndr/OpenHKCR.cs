// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Registry;

/// <inheritdoc/>
public class OpenHKCR : NdrOp
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public byte[] policyhandle = new byte[20];
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override int Opnum => 0;

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr)
    {
        // referent
        ndr.WriteUnsignedLong(new object().GetHashCode());
        // system name
        ndr.WriteUnsignedShort(49736);
        // length
        ndr.WriteUnsignedShort(1);
        ndr.WriteUnsignedLong(0x2000000);
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr)
    {
        ndr.ReadOctetArray(policyhandle, 0, 20);
        var hresult = ndr.ReadUnsignedLong();
        if (hresult != 0)
        {
            throw new InteropRuntimeException(hresult);
        }
    }
}
