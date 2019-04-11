// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http:// www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.winreg {
    using org.jinterop.dcom.common;
    using SharpCifs.Dcerpc.Ndr;

    /// <inheritdoc/>
    public class SaveFile : NdrOp {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public JIPolicyHandle parentKey;
        public string fileName;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <inheritdoc/>
        public override int Opnum => 20;

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            // Write parent handle
            ndr.WriteOctetArray(parentKey.Handle, 0, 20);

            // key len, since it is uint16
            ndr.WriteUnsignedShort((fileName.Length + 1) * 2);
            // key size, since it is uint16
            ndr.WriteUnsignedShort((fileName.Length + 1) * 2);

            // it's a pointer
            // referent
            ndr.WriteUnsignedLong(new object().GetHashCode());
            // max count
            ndr.WriteUnsignedLong(fileName.Length + 1);
            // offset
            ndr.WriteUnsignedLong(0);
            // actual count
            ndr.WriteUnsignedLong(fileName.Length + 1);

            var i = 0;
            while (i < fileName.Length) {
                ndr.WriteUnsignedShort(fileName[i]);
                i++;
            }

            // null termination
            ndr.WriteUnsignedShort(0);
            // now align for int
            ndr.FillAligned(4);

            ndr.WriteUnsignedLong(0);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            var hresult = ndr.ReadUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }
    }
}