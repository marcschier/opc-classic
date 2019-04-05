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
    /// Alter context pdu
    /// </summary>
    public class AlterContextPdu : ConnectionOrientedPdu {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
        public const int ALTER_CONTEXT_TYPE = 0x0e;

        /// <inheritdoc/>
        public override int Type => ALTER_CONTEXT_TYPE;

        /// <summary>
        /// Max transmit
        /// </summary>
        public int MaxTransmitFragment { get; set; } = -1;

        /// <summary>
        /// Max receive
        /// </summary>
        public int MaxReceiveFragment { get; set; } = -1;

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
            var maxTransmitFragment = MaxTransmitFragment;
            var maxReceiveFragment = MaxReceiveFragment;
            ndr.WriteUnsignedShort((maxTransmitFragment == -1) ? 
                ndr.Buffer.GetCapacity() : maxTransmitFragment);
            ndr.WriteUnsignedShort((maxReceiveFragment == -1) ? 
                ndr.Buffer.GetCapacity() : maxReceiveFragment);
            ndr.WriteUnsignedLong(AssociationGroupId);
            var contextList = ContextList;
            var count = contextList.Length;
            ndr.WriteUnsignedSmall((short)count);
            for (var i = 0; i < count; i++) {
                contextList[i].Write(ndr);
            }
        }
    }
}