//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using SharpInterop.Common;
    using SharpInterop.Rpc.Core;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using System.Collections.Generic;

    /// <summary>
    /// Partially implements IOxidResolver interface, used only for ResolveOxid calls.
    /// </summary>
    internal sealed class OxidResolver : NdrOp {

        /// <summary>
        /// Bindings
        /// </summary>
        internal DualStringArray OxidBindings { get; private set; }

        /// <summary>
        /// Ipid
        /// </summary>
        internal string IPID { get; private set; }

#pragma warning disable RECS0154 // Parameter is never used
        /// <summary>
        /// Create resolver
        /// </summary>
        /// <param name="oxid"></param>
        internal OxidResolver(byte[] oxid) => _odix = oxid;
#pragma warning restore RECS0154 // Parameter is never used

        /// <inheritdoc/>
        public override int Opnum => 4;

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, _odix);
            var context = new CodecContext();
            MarshalUnMarshalHelper.Serialize(ndr, typeof(short), (short)1, context);
            context.Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY;
            MarshalUnMarshalHelper.Serialize(ndr, typeof(ComArray),
                new ComArray(new short[] { 7 }, true), context);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            ndr.ReadUnsignedLong(); // pointer
            ndr.ReadUnsignedLong(); // some length component, irrelevant for us right now
            OxidBindings = DualStringArray.Decode(ndr);
            try {
                var ipid2 = new UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                IPID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "RemActivation read");
            }

            // read the auth hint
            var authenticationHint = ndr.ReadUnsignedLong();
            var comVersion = new ComVersion {
                MajorVersion = ndr.ReadUnsignedShort(),
                MinorVersion = ndr.ReadUnsignedShort()
            };

            var hresult = ndr.ReadUnsignedLong();

            if (hresult != 0) {
                throw new InteropRuntimeException(hresult);
            }
        }
        private readonly byte[] _odix;
    }
}