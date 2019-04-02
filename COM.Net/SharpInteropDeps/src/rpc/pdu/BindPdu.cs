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
	using PresentationContext = core.PresentationContext;

	public class BindPdu : ConnectionOrientedPdu
	{

		public const int BIND_TYPE = 0x0b;

		private PresentationContext[] contextList;

		private int maxTransmitFragment = MUST_RECEIVE_FRAGMENT_SIZE;

		private int maxReceiveFragment = MUST_RECEIVE_FRAGMENT_SIZE;

		private int associationGroupId;

		public virtual void resetCallIdCounter()
		{
			callIdCounter = 0;
		}

        public override int Type => BIND_TYPE;

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


        public virtual PresentationContext[] ContextList {
            get => contextList;
            set => contextList = value;
        }


        protected internal override void readBody(NetworkDataRepresentation ndr)
		{
			MaxTransmitFragment = ndr.readUnsignedShort();
			MaxReceiveFragment = ndr.readUnsignedShort();
			AssociationGroupId = (int) ndr.readUnsignedLong();
			var count = ndr.readUnsignedSmall();
			var contextList = new PresentationContext[count];
			for (var i = 0; i < count; i++)
			{
				contextList[i] = new PresentationContext();
				contextList[i].read(ndr);
			}
			ContextList = contextList;
		}

		protected internal override void writeBody(NetworkDataRepresentation ndr)
		{
			ndr.writeUnsignedShort(MaxTransmitFragment);
			ndr.writeUnsignedShort(MaxReceiveFragment);
			ndr.writeUnsignedLong(AssociationGroupId);
			var contextList = ContextList;
			var count = contextList.Length;
			ndr.writeUnsignedSmall((short) count);
			for (var i = 0; i < count; i++)
			{
				contextList[i].write(ndr);
			}
		}

	}

}