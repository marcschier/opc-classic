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
    /// Alter context response
    /// </summary>
    public class AlterContextResponsePdu : ConnectionOrientedPdu {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
        public const int ALTER_CONTEXT_RESPONSE_TYPE = 0x0f;

        /// <inheritdoc/>
        public override int Type => ALTER_CONTEXT_RESPONSE_TYPE;

        /// <summary>
        /// Max transmit
        /// </summary>
        public int MaxTransmitFragment { get; set; } = -1;

        /// <summary>
        /// Max receive
        /// </summary>
        public int MaxReceiveFragment { get; set; } = -1;

        /// <summary>
        /// Association group id
        /// </summary>
        public int AssociationGroupId { get; set; }

        /// <summary>
        /// Secondary address
        /// </summary>
        public Port SecondaryAddress { get; set; }

        /// <summary>
        /// Result list
        /// </summary>
        public PresentationResult[] ResultList { get; set; }

        /// <inheritdoc/>
        protected internal override void ReadBody(NdrCodec ndr) {
            MaxTransmitFragment = ndr.ReadUnsignedShort();
            MaxReceiveFragment = ndr.ReadUnsignedShort();
            AssociationGroupId = ndr.ReadUnsignedLong();
            var secondaryAddress = new Port();
            secondaryAddress.Read(ndr);
            SecondaryAddress = secondaryAddress;
            ndr.Buffer.Align(4);
            var count = ndr.ReadUnsignedSmall();
            var resultList = new PresentationResult[count];
            for (var i = 0; i < count; i++) {
                resultList[i] = new PresentationResult();
                resultList[i].Read(ndr);
            }
            ResultList = resultList;
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
            var secondaryAddress = SecondaryAddress;
            if (secondaryAddress == null) {
                secondaryAddress = new Port();
            }
            secondaryAddress.Write(ndr);
            ndr.Buffer.Align(4);
            var resultList = ResultList;
            var count = resultList.Length;
            ndr.WriteUnsignedSmall((short)count);
            for (var i = 0; i < count; i++) {
                resultList[i].Write(ndr);
            }
        }
    }
}