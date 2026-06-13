// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Registry;

/// <inheritdoc/>
public class SetValue : NdrOp
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public PolicyHandle parentKey;
    public string valueName;
    public RegValueType clazzType = (RegValueType)(-1);
    public int lengthInBytes = -1;
    public byte[] data; // should be in the right encoding for Strings.
    public byte[][] data2; // reg_
    public int dword;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override int Opnum => 22;

    /// <inheritdoc/>
#pragma warning disable MA0051 // Legacy NDR encode mirrors the WinReg wire layout.
    public override void Write(NdrCodec ndr)
    {
        // Write parent handle
        ndr.WriteOctetArray(parentKey.Handle, 0, 20);

        // key len, since it is uint16
        ndr.WriteUnsignedShort((valueName.Length + 1) * 2);
        // key size, since it is uint16
        ndr.WriteUnsignedShort((valueName.Length + 1) * 2);

        // it's a pointer
        // referent
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

        // now align for int
        ndr.FillAligned(4);

        // Write the type.
        ndr.WriteUnsignedLong((int)clazzType);

        i = 0;
        if (lengthInBytes != 0)
        {
            switch (clazzType)
            {
                case RegValueType.REG_EXPAND_SZ: // for environment variable strings
                case RegValueType.REG_SZ:
                    // for strings, strings are null terminated, length in
                    // bytes will NOT include the null termination
                    // character writing the max count
                    ndr.WriteUnsignedLong((lengthInBytes + 1) * 2);

                    while (i < data.Length)
                    {
                        ndr.WriteUnsignedShort(data[i]);
                        i++;
                    }

                    // null termination
                    ndr.WriteUnsignedShort(0);

                    // now align for int
                    ndr.FillAligned(4);

                    ndr.WriteUnsignedLong((lengthInBytes + 1) * 2);

                    break;
                case RegValueType.REG_DWORD:
                    ndr.WriteUnsignedLong(lengthInBytes);
                    ndr.WriteUnsignedLong(dword);
                    ndr.WriteUnsignedLong(lengthInBytes);
                    break;
                case RegValueType.REG_NONE:
                    data = Array.Empty<byte>();
                    lengthInBytes = 0;
                    goto case RegValueType.REG_BINARY;
                case RegValueType.REG_BINARY:
                    ndr.WriteUnsignedLong(lengthInBytes);
                    ndr.WriteOctetArray(data, 0, lengthInBytes);
                    ndr.FillAligned(4);
                    ndr.WriteUnsignedLong(lengthInBytes);
                    break;
                case RegValueType.REG_MULTI_SZ:
                    // for strings, strings are null terminated, length in bytes will NOT include the null termination
                    // character.  Writing the max count, this will be computed before hand
                    ndr.WriteUnsignedLong(lengthInBytes);

                    for (i = 0; i < data2.Length; i++)
                    {
                        for (var j = 0; j < data2[i].Length; j++)
                        {
                            ndr.WriteUnsignedShort(data2[i][j]);
                        }
                        // null termination for each string
                        ndr.WriteUnsignedShort(0);
                    }
                    // null termination for the multi sz.
                    ndr.WriteUnsignedShort(0);

                    ndr.FillAligned(4);
                    // now align for int

                    ndr.WriteUnsignedLong(lengthInBytes);
                    break;
                default:
                    throw new InteropRuntimeException((int)ErrorCode.INTEROP_WINREG_EXCEPTION4);
            }
        }
        else
        {
            // for data
            ndr.WriteUnsignedLong(0);
            // for length
            ndr.WriteUnsignedLong(0);
        }
    }
#pragma warning restore MA0051

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
