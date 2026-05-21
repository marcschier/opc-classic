//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Registry {
    using SharpInterop.Common;
    using SharpCifs.Dcerpc.Ndr;

    /// <inheritdoc/>
    public class EnumValue : NdrOp {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public PolicyHandle parentKey;
        public int index = -1;
        public object[] retval = new object[2];
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <inheritdoc/>
        public override int Opnum => 10;

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {

            // Write parent handle
            ndr.WriteOctetArray(parentKey.Handle, 0, 20);

            ndr.WriteUnsignedLong(index);

            // buffer len, since it is uint16
            ndr.WriteUnsignedShort(0);
            // buffer size, since it is uint16
            ndr.WriteUnsignedShort(2048);

            // it's a pointer
            // referent
            ndr.WriteUnsignedLong(new object().GetHashCode());
            // max count
            ndr.WriteUnsignedLong(1024);
            // offset
            ndr.WriteUnsignedLong(0);
            // actual count
            ndr.WriteUnsignedLong(0);

            // pointer
            ndr.WriteUnsignedLong(new object().GetHashCode());
            ndr.WriteUnsignedLong(0);

            ndr.WriteUnsignedLong(0);

            ndr.WriteUnsignedLong(new object().GetHashCode());
            ndr.WriteUnsignedLong(0);

            ndr.WriteUnsignedLong(new object().GetHashCode());
            ndr.WriteUnsignedLong(0);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            // buffer len, since it is uint16
            ndr.ReadUnsignedShort();
            // buffer size, since it is uint16
            ndr.ReadUnsignedShort();

            // it's a pointer
            // referent
            ndr.ReadUnsignedLong();
            // max count
            ndr.ReadUnsignedLong();
            // offset
            ndr.ReadUnsignedLong();

            var actuallength = ndr.ReadUnsignedLong(); // actuallength
            var bytes = new byte[0];
            if (actuallength != 0) {
                bytes = new byte[actuallength - 1];
            }
            var i = 0;
            // last 2 bytes, null termination will be eaten outside the loop
            while (i < actuallength - 1) {
                var retVal = ndr.ReadUnsignedShort();
                bytes[i] = (byte)retVal;
                i++;
            }
            if (actuallength != 0) {
                ndr.ReadUnsignedShort();
            }

            retval[0] = StringHelperClass.NewString(bytes);

            ndr.SkipAligned(4);

            // it's a pointer
            // referent
            ndr.ReadUnsignedLong();

            var type = ndr.ReadUnsignedLong();
            retval[1] = type;

            ndr.ReadUnsignedLong();

            ndr.ReadUnsignedLong();
            ndr.ReadUnsignedLong();

            ndr.ReadUnsignedLong();
            ndr.ReadUnsignedLong();

            var hresult = ndr.ReadUnsignedLong();
            if (hresult != 0) {
                throw new InteropRuntimeException(hresult);
            }
        }
    }
}