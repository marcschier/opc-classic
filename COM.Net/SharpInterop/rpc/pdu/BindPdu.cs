//
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
//
// j-Interop (Pure Java implementation of DCOM protocol)
//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace rpc.pdu {
    using rpc.core;
    using SharpCifs.Dcerpc.Ndr;

    /// <summary>
    /// Bind pdu
    /// </summary>
    public class BindPdu : ConnectionOrientedPdu {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
        public const int BIND_TYPE = 0x0b;

        /// <inheritdoc/>
        public override int Type => BIND_TYPE;

        /// <summary>
        /// max transmit
        /// </summary>
        public int MaxTransmitFragment { get; set; } = MUST_RECEIVE_FRAGMENT_SIZE;

        /// <summary>
        /// Max receive
        /// </summary>
        public int MaxReceiveFragment { get; set; } = MUST_RECEIVE_FRAGMENT_SIZE;

        /// <summary>
        /// Association group
        /// </summary>
        public int AssociationGroupId { get; set; }

        /// <summary>
        /// Context list
        /// </summary>
        public PresentationContext[] ContextList { get; set; }


        /// <inheritdoc/>
        protected internal override void ReadBody(NdrCodec ndr) {
            MaxTransmitFragment = ndr.ReadUnsignedShort();
            MaxReceiveFragment = ndr.ReadUnsignedShort();
            AssociationGroupId = ndr.ReadUnsignedLong();
            var count = ndr.ReadUnsignedSmall();
            var contextList = new PresentationContext[count];
            for (var i = 0; i < count; i++) {
                contextList[i] = new PresentationContext();
                contextList[i].Read(ndr);
            }
            ContextList = contextList;
        }

        /// <inheritdoc/>
        protected internal override void WriteBody(NdrCodec ndr) {
            ndr.WriteUnsignedShort(MaxTransmitFragment);
            ndr.WriteUnsignedShort(MaxReceiveFragment);
            ndr.WriteUnsignedLong(AssociationGroupId);
            var contextList = ContextList;
            var count = contextList.Length;
            ndr.WriteUnsignedSmall((short)count);
            for (var i = 0; i < count; i++) {
                contextList[i].Write(ndr);
            }
        }

        /// <summary>
        /// Helper
        /// </summary>
        public void ResetCallIdCounter() => s_callIdCounter = 0;
    }
}