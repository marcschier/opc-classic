//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Buffers.Binary;

namespace SharpInterop.Registry; 
/// <inheritdoc/>
public class QueryValue : NdrOp {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public PolicyHandle parentKey;
    public string key = "";
    public int bufferLength = -1;
    public RegValueType type = (RegValueType)(-1);
    public byte[] buffer;
    public byte[][] buffer2 = new byte[2048][];
    public byte[] policyhandle = new byte[20];
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override int Opnum => 17;

    /// <inheritdoc/>
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

        // pointer to type
        ndr.WriteUnsignedLong(new object().GetHashCode());
        ndr.WriteUnsignedLong(0);

        // pointer to data
        ndr.WriteUnsignedLong(new object().GetHashCode());
        // max count
        ndr.WriteUnsignedLong(bufferLength);
        ndr.WriteUnsignedLong(0); // offset
        ndr.WriteUnsignedLong(0); // actual

        // pointer to size
        ndr.WriteUnsignedLong(new object().GetHashCode());
        ndr.WriteUnsignedLong(bufferLength);

        // pointer to length
        ndr.WriteUnsignedLong(new object().GetHashCode());
        ndr.WriteUnsignedLong(0);
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr) {
        var i = 0;
        // pointer
        ndr.ReadUnsignedLong();
        type = (RegValueType)ndr.ReadUnsignedLong(); // type
        var retval = new byte[bufferLength];
        // StringBuffer buffer = new StringBuffer();
        // pointer to data
        ndr.ReadUnsignedLong();
        var maxcount = ndr.ReadUnsignedLong(); // maxcount
        var offset = ndr.ReadUnsignedLong(); // offset
        switch (type) {
            case RegValueType.REG_EXPAND_SZ: // for environment variable strings
            case RegValueType.REG_SZ:

                var actuallength = (int)Math.Round(ndr.ReadUnsignedLong() / 2.0); // actuallength

                // last 2 bytes, null termination will be eaten outside the loop
                while (i < actuallength - 1) {
                    var retVal = ndr.ReadUnsignedShort();
                    // even though this is a unicode string, but will not have anything else
                    // other than ascii charset, which is supported by all encodings.
                    // buffer.append(new String(new byte[]{(byte)retVal}));
                    retval[i] = (byte)retVal;
                    i++;
                }
                if (actuallength != 0) {
                    ndr.ReadUnsignedShort();
                }

                break;
            case RegValueType.REG_DWORD:
                i = ndr.ReadUnsignedLong();
                var value = ndr.ReadUnsignedLong();
                BinaryPrimitives.WriteInt32LittleEndian(retval, value);
                break;
            case RegValueType.REG_NONE:
            case RegValueType.REG_BINARY:
                i = ndr.ReadUnsignedLong();
                ndr.ReadOctetArray(retval, 0, i);
                break;
            case RegValueType.REG_MULTI_SZ:
                actuallength = (int)Math.Round(ndr.ReadUnsignedLong() / 2.0); // actuallength
                int kk = 0, ll = 0;
                i = 0;
                // last 2 bytes, null termination will be eaten outside the loop
                while (i < actuallength - 1) {
                    var retVal = ndr.ReadUnsignedShort();
                    if (retVal == 0) {
                        // reached end of one string
                        buffer2[kk] = new byte[ll];
                        Array.Copy(retval, 0, buffer2[kk], 0, ll);
                        kk++;
                        ll = -1; // it will become 0 next
                        retval = new byte[bufferLength];
                    }
                    else {
                        retval[ll] = (byte)retVal;
                    }
                    i++;
                    ll++;
                }
                if (actuallength != 0) {
                    ndr.ReadUnsignedShort();
                }

                break;
            default:
                throw new InteropRuntimeException((int)ErrorCode.INTEROP_WINREG_EXCEPTION4);
        }

        ndr.SkipAligned(4);

        // pointer to size
        ndr.ReadUnsignedLong();
        ndr.ReadUnsignedLong();

        // pointer to length
        ndr.ReadUnsignedLong();
        ndr.ReadUnsignedLong();

        var hresult = ndr.ReadUnsignedLong();
        if (hresult != 0) {
            throw new InteropRuntimeException(hresult);
        }

        if (type != RegValueType.REG_MULTI_SZ) {
            buffer = new byte[i];
            Array.Copy(retval, 0, buffer, 0, i);
        }
        // key = buffer.toString();
    }
}
