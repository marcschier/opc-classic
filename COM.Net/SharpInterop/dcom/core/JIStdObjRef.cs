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
    using rpc.core;
    using System;
    using Serilog;

    [Serializable]
    internal sealed class JIStdObjRef {

        /// <summary>
        /// Flags
        /// </summary>
        public int Flags { get; private set; }

        /// <summary>
        /// Public refs
        /// </summary>
        public int PublicRefs { get; private set; } = -1;

        /// <summary>
        /// Oxid
        /// </summary>
        public byte[] Oxid { get; private set; }

        /// <summary>
        /// Object id
        /// </summary>
        public byte[] ObjectId { get; private set; }

        /// <summary>
        /// Ip id
        /// </summary>
        public string Ipid { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private JIStdObjRef() {
        }

        /// <summary>
        /// Resolver address are taken of localhost
        /// </summary>
        /// <param name="ipid"></param>
        /// <param name="oxid"></param>
        /// <param name="oid"></param>
        internal JIStdObjRef(string ipid, JIOxid oxid, JIObjectId oid) {
            Ipid = ipid;
            Oxid = oxid.OXID;
            ObjectId = oid.OID;
            PublicRefs = 5;
        }

        /// <summary>
        /// This is used to instantiate an empty StdObjRef for
        /// cases where the interface is not supported.
        /// </summary>
        /// <param name="ipid"></param>
        internal JIStdObjRef(string ipid) {
            Ipid = ipid;
            Flags = 0x0;
            Oxid = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            ObjectId = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            PublicRefs = 0;
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal static JIStdObjRef Decode(NdrCodec ndr) {
            var objRef = new JIStdObjRef {
                Flags = ndr.ReadUnsignedLong(),
                PublicRefs = ndr.ReadUnsignedLong(),
                Oxid = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8),
                ObjectId = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8)
            };
            try {
                var ipid2 = new UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                objRef.Ipid = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIStdObjRef", "decode", e);
            }
            return objRef;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        public void Encode(NdrCodec ndr) {
            ndr.WriteUnsignedLong(Flags);
            ndr.WriteUnsignedLong(PublicRefs);
            JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, Oxid);
            JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, ObjectId);
            try {
                var ipid = new UUID(Ipid);
                ipid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {

                Log.Logger.Error(e, "JIStdObjRef", "encode", e);
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            var retVal = "IPID: " + Ipid; //+ ", OID: " + oidString;
            return retVal;
        }
    }
}