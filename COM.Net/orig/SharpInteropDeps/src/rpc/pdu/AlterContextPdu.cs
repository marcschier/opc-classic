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
	using PresentationContext = rpc.core.PresentationContext;

	public class AlterContextPdu : ConnectionOrientedPdu {

		public const int ALTER_CONTEXT_TYPE = 0x0e;

		private PresentationContext[] ContextList_Renamed;

		private int MaxTransmitFragment_Renamed = -1;

		private int MaxReceiveFragment_Renamed = -1;

		private int AssociationGroupId_Renamed = 0;

		public override int Type {
			get {
				return ALTER_CONTEXT_TYPE;
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


		public virtual PresentationContext[] ContextList {
			get {
				return ContextList_Renamed;
			}
			set {
				this.ContextList_Renamed = value;
			}
		}


		public override void ReadBody(NetworkDataRepresentation ndr) {
			MaxTransmitFragment = ndr.ReadUnsignedShort();
			MaxReceiveFragment = ndr.ReadUnsignedShort();
			AssociationGroupId = (int) ndr.ReadUnsignedLong();
			int count = ndr.ReadUnsignedSmall();
			PresentationContext[] contextList = new PresentationContext[count];
			for (int i = 0; i < count; i++) {
				contextList[i] = new PresentationContext();
				contextList[i].Read(ndr);
			}
			ContextList = contextList;
		}

		public override void WriteBody(NetworkDataRepresentation ndr) {
			int maxTransmitFragment = MaxTransmitFragment;
			int maxReceiveFragment = MaxReceiveFragment;
			ndr.WriteUnsignedShort((maxTransmitFragment == -1) ? ndr.Buffer.Capacity : maxTransmitFragment);
			ndr.WriteUnsignedShort((maxReceiveFragment == -1) ? ndr.Buffer.Capacity : maxReceiveFragment);
			ndr.WriteUnsignedLong(AssociationGroupId);
			PresentationContext[] contextList = ContextList;
			int count = contextList.Length;
			ndr.WriteUnsignedSmall((short) count);
			for (int i = 0; i < count; i++) {
				contextList[i].Write(ndr);
			}
		}

	}

}