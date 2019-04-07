//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//
namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using System.Collections.Generic;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Remote unknown
    /// </summary>
    internal sealed class JIRemUnknown : NdrOp {

        /// <summary>
        /// Iunknown
        /// </summary>
        public const string IID_IUnknown = "00000143-0000-0000-c000-000000000046";
        //	public static final String IID_IDispatch = "00020400-0000-0000-c000-000000000046";

        /// <summary>
        /// Interface pointer
        /// </summary>
        public JIInterfacePointer InterfacePointer { get; private set; }

        /// <inheritdoc/>
        public override int Opnum => 6;
        // opnum is 3 as this is a COM interface and 0,1,2 are occupied by IUnknown
        // 3,4,5 by IRemUnknown and we are going to call IRemUnknown2.QI so that we get MIPs.

        /// <summary>
        /// Create unknown
        /// </summary>
        /// <param name="ipidOfIUnknown"></param>
        /// <param name="requestedIID"></param>
        internal JIRemUnknown(string ipidOfIUnknown, string requestedIID) {
            _ipidOfIUnknown = ipidOfIUnknown;
            _requestedIID = requestedIID;
        }

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            var orpcthis = new JIOrpcThis();
            orpcthis.Encode(ndr);

            //now write the IPID
            var uuid = new UUID(_ipidOfIUnknown);
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemUnknown write");
            }

            ndr.WriteUnsignedShort(1); //1 interfaces. (requested IID)
            ndr.WriteUnsignedShort(0); //byte alignment
            ndr.WriteUnsignedLong(1); //length of the array
            uuid = new UUID(_requestedIID);
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemUnknown Performing a QueryInterface for " +
                    _requestedIID);
            }

            ndr.WriteUnsignedLong(0);
            // TODO Index Matching , there seems to be a bug in
            // the jarapac system, it only reads upto (length - 6) bytes and one has to have another
            // call after that or incomplete request will go.
            // in case no param is present just put an unsigned long = 0.
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            JIOrpcThat.Decode(ndr);
            ndr.ReadUnsignedLong(); //size will be one
            var hresult1 = ndr.ReadUnsignedLong();
            if (hresult1 != 0) {
                //something happened.
                throw new JIRuntimeException(hresult1);
            }
            //array length
            ndr.ReadUnsignedLong();
            //and now the JIInterfacePointer itself.
            InterfacePointer = JIInterfacePointer.Decode(
                ndr, new List<object>(), JIFlags.FLAG_NULL, new Hashtable());
            //final hresult
            hresult1 = ndr.ReadUnsignedLong();
            if (hresult1 != 0) {
                //something happened.
                throw new JIRuntimeException(hresult1);
            }
        }

        private readonly string _ipidOfIUnknown;
        private readonly string _requestedIID;
    }
}