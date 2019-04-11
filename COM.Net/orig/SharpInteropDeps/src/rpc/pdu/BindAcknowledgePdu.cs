/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>


namespace rpc.pdu {

    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
    using Port = rpc.core.Port;
    using PresentationResult = rpc.core.PresentationResult;

    public class BindAcknowledgePdu : ConnectionOrientedPdu {

        public const int BIND_ACKNOWLEDGE_TYPE = 0x0c;

        private PresentationResult[] ResultList_Renamed;

        private int MaxTransmitFragment_Renamed = MUST_RECEIVE_FRAGMENT_SIZE;

        private int MaxReceiveFragment_Renamed = MUST_RECEIVE_FRAGMENT_SIZE;

        private int AssociationGroupId_Renamed = 0;

        private Port SecondaryAddress_Renamed;

        public override int Type {
            get {
                return BIND_ACKNOWLEDGE_TYPE;
            }
        }

        public virtual int MaxTransmitFragment {
            get {
                return MaxTransmitFragment_Renamed;
            }
            set {
                this.MaxTransmitFragment_Renamed = value;
            }
        }


        public virtual int MaxReceiveFragment {
            get {
                return MaxReceiveFragment_Renamed;
            }
            set {
                this.MaxReceiveFragment_Renamed = value;
            }
        }


        public virtual int AssociationGroupId {
            get {
                return AssociationGroupId_Renamed;
            }
            set {
                this.AssociationGroupId_Renamed = value;
            }
        }


        public virtual Port SecondaryAddress {
            get {
                return SecondaryAddress_Renamed;
            }
            set {
                this.SecondaryAddress_Renamed = value;
            }
        }


        public virtual PresentationResult[] ResultList {
            get {
                return ResultList_Renamed;
            }
            set {
                this.ResultList_Renamed = value;
            }
        }


        public override void ReadBody(NetworkDataRepresentation ndr) {
            MaxTransmitFragment = ndr.ReadUnsignedShort();
            MaxReceiveFragment = ndr.ReadUnsignedShort();
            AssociationGroupId = (int) ndr.ReadUnsignedLong();
            Port secondaryAddress = new Port();
            secondaryAddress.Read(ndr);
            SecondaryAddress = secondaryAddress;
            ndr.Buffer.Align(4);
            int count = ndr.ReadUnsignedSmall();
            PresentationResult[] resultList = new PresentationResult[count];
            for (int i = 0; i < count; i++) {
                resultList[i] = new PresentationResult();
                resultList[i].Read(ndr);
            }
            ResultList = resultList;
        }

        public override void WriteBody(NetworkDataRepresentation ndr) {
            ndr.WriteUnsignedShort(MaxTransmitFragment);
            ndr.WriteUnsignedShort(MaxReceiveFragment);
            ndr.WriteUnsignedLong(AssociationGroupId);
            Port secondaryAddress = SecondaryAddress;
            if (secondaryAddress == null) {
                secondaryAddress = new Port();
            }
            secondaryAddress.Write(ndr);
            ndr.Buffer.Align(4);
            PresentationResult[] resultList = ResultList;
            int count = resultList.Length;
            ndr.WriteUnsignedSmall((short) count);
            for (int i = 0; i < count; i++) {
                resultList[i].Write(ndr);
            }
        }

    }

}