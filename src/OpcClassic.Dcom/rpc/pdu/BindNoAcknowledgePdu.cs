//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.pdu {
    using SharpCifs.Dcerpc.Ndr;
    using SharpInterop.Rpc.Core;

    /// <summary>
    /// Bind nack
    /// </summary>
    public class BindNoAcknowledgePdu : ConnectionOrientedPdu {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
        public const int BIND_NO_ACKNOWLEDGE_TYPE = 0x0d;

        /// <inheritdoc/>
        public override int Type => BIND_NO_ACKNOWLEDGE_TYPE;

        /// <summary>
        /// Reject reason
        /// </summary>
        public BindNoAcknowledgeReason RejectReason { get; set; }

        /// <summary>
        /// Version list
        /// </summary>
        public ProtocolVersion[] VersionList { get; set; }

        /// <inheritdoc/>
        protected internal override void ReadBody(NdrCodec ndr) {
            var reason = ndr.ReadUnsignedSmall();
            RejectReason = (BindNoAcknowledgeReason)reason;
            ProtocolVersion[] versionList = null;
            if (RejectReason == BindNoAcknowledgeReason.PROTOCOL_VERSION_NOT_SUPPORTED) {
                var count = ndr.ReadUnsignedSmall();
                versionList = new ProtocolVersion[count];
                for (var i = 0; i < count; i++) {
                    versionList[i] = new ProtocolVersion();
                    versionList[i].Read(ndr);
                }
            }
            VersionList = versionList;
        }

        /// <inheritdoc/>
        protected internal override void WriteBody(NdrCodec ndr) {
            var reason = (short)RejectReason;
            ndr.WriteUnsignedSmall(reason);
            if (RejectReason != BindNoAcknowledgeReason.PROTOCOL_VERSION_NOT_SUPPORTED) {
                return;
            }
            var versionList = VersionList;
            var count = (versionList != null) ? versionList.Length : 0;
            for (var i = 0; i < count; i++) {
                versionList[i].Write(ndr);
            }
        }
    }
}