//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using System.Collections.Generic;

    /// <summary>
    /// Partially implements IOxidResolver interface, used only for ResolveOxid calls.
    /// </summary>
    internal sealed class JIOxidResolver : NdrOp {

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
        internal JIOxidResolver(byte[] oxid) => _odix = oxid;

        /// <inheritdoc/>
        public override int Opnum => 4;

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, _odix);
            JIMarshalUnMarshalHelper.Serialize(ndr, typeof(short),
                (short)1, new List<object>(), JIFlags.FLAG_NULL);
            JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIArray),
                new JIArray(new short[] { 7 }, true), new List<object>(),
                JIFlags.FLAG_REPRESENTATION_ARRAY);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            ndr.ReadUnsignedLong(); // pointer
            ndr.ReadUnsignedLong(); // some length component, irrelevant for us right now
            OxidBindings = JIDualStringArray.Decode(ndr);
            try {
                var ipid2 = new UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                IPID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation", "read", e);
            }

            // read the auth hint
            var authenticationHint = ndr.ReadUnsignedLong();
            var comVersion = new JIComVersion {
                MajorVersion = ndr.ReadUnsignedShort(),
                MinorVersion = ndr.ReadUnsignedShort()
            };

            var hresult = ndr.ReadUnsignedLong();

            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }
        private readonly byte[] _odix;
    }
}