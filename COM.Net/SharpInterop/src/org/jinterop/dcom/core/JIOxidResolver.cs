// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using ndr;
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using System.Collections;

    /// <summary>
    /// Partially implements IOxidResolver interface, used only for ResolveOxid calls.
    /// </summary>
    internal sealed class JIOxidResolver : NdrObject {

        /// <summary>
        /// Bindings
        /// </summary>
        internal JIDualStringArray OxidBindings { get; private set; }

        /// <summary>
        /// Ipid
        /// </summary>
        internal string IPID { get; private set; }

        /// <summary>
        /// Create resolver
        /// </summary>
        /// <param name="oxid"></param>
        internal JIOxidResolver(sbyte[] oxid) {
            _odix = oxid;
        }

        /// <inheritdoc/>
        public override int Opnum => 4;

        /// <inheritdoc/>
        public override void write(NetworkDataRepresentation ndr) {
            JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr, _odix);
            JIMarshalUnMarshalHelper.serialize(ndr, typeof(short?),
                (short)1, new ArrayList(), JIFlags.FLAG_NULL);
            JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIArray),
                new JIArray(new short?[] { 7 }, true), new ArrayList(), JIFlags.FLAG_REPRESENTATION_ARRAY);
        }

        /// <inheritdoc/>
        public override void read(NetworkDataRepresentation ndr) {
            ndr.readUnsignedLong(); //pointer
            ndr.readUnsignedLong(); //some length component, irrelevant for us right now
            OxidBindings = JIDualStringArray.decode(ndr);
            try {
                var ipid2 = new UUID();
                ipid2.decode(ndr, ndr.Buffer);
                IPID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation", "read", e);
            }

            //read the auth hint
            var authenticationHint = ndr.readUnsignedLong();
            var comVersion = new JIComVersion {
                MajorVersion = ndr.readUnsignedShort(),
                MinorVersion = ndr.readUnsignedShort()
            };

            var hresult = ndr.readUnsignedLong();

            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }
        private readonly sbyte[] _odix;
    }
}