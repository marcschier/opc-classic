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
    public class OpenHKLM : NdrOp {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public byte[] policyhandle = new byte[20];
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <inheritdoc/>
        public override int Opnum => 2;

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            // referent
            ndr.WriteUnsignedLong(new object().GetHashCode());
            // system name
            ndr.WriteUnsignedShort(40736);
            // length
            ndr.WriteUnsignedShort(1);
            ndr.WriteUnsignedLong(0x2000000);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            ndr.ReadOctetArray(policyhandle, 0, 20);
            var hresult = ndr.ReadUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }
    }
}