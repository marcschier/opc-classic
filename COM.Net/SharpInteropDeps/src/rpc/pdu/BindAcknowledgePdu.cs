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


namespace rpc.pdu
{

	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using Port = core.Port;
	using PresentationResult = core.PresentationResult;

	public class BindAcknowledgePdu : ConnectionOrientedPdu
	{

		public const int BIND_ACKNOWLEDGE_TYPE = 0x0c;

		private PresentationResult[] resultList;

		private int maxTransmitFragment = MUST_RECEIVE_FRAGMENT_SIZE;

		private int maxReceiveFragment = MUST_RECEIVE_FRAGMENT_SIZE;

		private int associationGroupId;

		private Port secondaryAddress;

        public override int Type => BIND_ACKNOWLEDGE_TYPE;

        public virtual int MaxTransmitFragment {
            get => maxTransmitFragment;
            set => maxTransmitFragment = value;
        }


        public virtual int MaxReceiveFragment {
            get => maxReceiveFragment;
            set => maxReceiveFragment = value;
        }


        public virtual int AssociationGroupId {
            get => associationGroupId;
            set => associationGroupId = value;
        }


        public virtual Port SecondaryAddress {
            get => secondaryAddress;
            set => secondaryAddress = value;
        }


        public virtual PresentationResult[] ResultList {
            get => resultList;
            set => resultList = value;
        }


        protected internal override void readBody(NetworkDataRepresentation ndr)
		{
			MaxTransmitFragment = ndr.readUnsignedShort();
			MaxReceiveFragment = ndr.readUnsignedShort();
			AssociationGroupId = (int) ndr.readUnsignedLong();
			var secondaryAddress = new Port();
			secondaryAddress.read(ndr);
			SecondaryAddress = secondaryAddress;
			ndr.Buffer.align(4);
			var count = ndr.readUnsignedSmall();
			var resultList = new PresentationResult[count];
			for (var i = 0; i < count; i++)
			{
				resultList[i] = new PresentationResult();
				resultList[i].read(ndr);
			}
			ResultList = resultList;
		}

		protected internal override void writeBody(NetworkDataRepresentation ndr)
		{
			ndr.writeUnsignedShort(MaxTransmitFragment);
			ndr.writeUnsignedShort(MaxReceiveFragment);
			ndr.writeUnsignedLong(AssociationGroupId);
			var secondaryAddress = SecondaryAddress;
			if (secondaryAddress == null)
			{
				secondaryAddress = new Port();
			}
			secondaryAddress.write(ndr);
			ndr.Buffer.align(4);
			var resultList = ResultList;
			var count = resultList.Length;
			ndr.writeUnsignedSmall((short) count);
			for (var i = 0; i < count; i++)
			{
				resultList[i].write(ndr);
			}
		}

	}

}