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

	public class AlterContextPdu : ConnectionOrientedPdu
	{

		public const int ALTER_CONTEXT_TYPE = 0x0e;

		private PresentationContext[] contextList;

		private int maxTransmitFragment = -1;

		private int maxReceiveFragment = -1;

		private int associationGroupId;

        public override int Type => ALTER_CONTEXT_TYPE;

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
			var maxTransmitFragment = MaxTransmitFragment;
			var maxReceiveFragment = MaxReceiveFragment;
			ndr.writeUnsignedShort((maxTransmitFragment == -1) ? ndr.Buffer.Capacity : maxTransmitFragment);
			ndr.writeUnsignedShort((maxReceiveFragment == -1) ? ndr.Buffer.Capacity : maxReceiveFragment);
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