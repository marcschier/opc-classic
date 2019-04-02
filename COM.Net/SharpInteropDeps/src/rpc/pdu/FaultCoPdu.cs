using System;
using System.Collections;

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


	using NdrBuffer = ndr.NdrBuffer;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class FaultCoPdu : ConnectionOrientedPdu, FaultCodes, Fragmentable
	{

		public const int FAULT_TYPE = 0x03;

		private sbyte[] stub;

		private int allocationHint;

		private int contextId;

		private int cancelCount;

		private int status = FaultCodes_Fields.UNSPECIFIED_REJECTION;

        public override int Type => FAULT_TYPE;

        public virtual sbyte[] Stub {
            get => stub;
            set => stub = value;
        }


        public virtual int AllocationHint {
            get => allocationHint;
            set => allocationHint = value;
        }


        public virtual int ContextId {
            get => contextId;
            set => contextId = value;
        }


        public virtual int CancelCount {
            get => cancelCount;
            set => cancelCount = value;
        }


        public virtual int Status {
            get => status;
            set => status = value;
        }


        protected internal override void readPdu(NetworkDataRepresentation ndr)
		{
			readHeader(ndr);
			readBody(ndr);
			readStub(ndr);
		}

		protected internal override void writePdu(NetworkDataRepresentation ndr)
		{
			writeHeader(ndr);
			writeBody(ndr);
			writeStub(ndr);
		}

		protected internal override void readBody(NetworkDataRepresentation ndr)
		{
			AllocationHint = ndr.readUnsignedLong();
			ContextId = ndr.readUnsignedShort();
			CancelCount = ndr.readUnsignedSmall();
			Status = (int) ndr.readUnsignedLong();
		}

		protected internal override void writeBody(NetworkDataRepresentation ndr)
		{
			ndr.writeUnsignedLong(AllocationHint);
			ndr.writeUnsignedShort(ContextId);
			ndr.writeUnsignedSmall((short) CancelCount);
			ndr.writeUnsignedLong(Status);
		}

		protected internal virtual void readStub(NetworkDataRepresentation ndr)
		{
			var buf = ndr.Buffer;
			buf.align(8);
			sbyte[] stub = null;
			var length = FragmentLength - buf.Index;
			if (length > 0)
			{
				stub = new sbyte[length];
				ndr.readOctetArray(stub, 0, length);
			}
			Stub = stub;
		}

		protected internal virtual void writeStub(NetworkDataRepresentation ndr)
		{
			var buf = ndr.Buffer;
			buf.align(8, (sbyte) 0);
			var stub = Stub;
			if (stub != null)
			{
				ndr.writeOctetArray(stub, 0, stub.Length);
			}
		}

		public virtual IEnumerator fragment(int size)
		{
			var stub = Stub;
			if (stub == null)
			{
				return Arrays.asList(new FaultCoPdu[] {this}).GetEnumerator();
			}
			var stubSize = size - 24;
			if (stub.Length <= stubSize)
			{
				return Arrays.asList(new FaultCoPdu[] {this}).GetEnumerator();
			}
			return new FragmentIterator(this, stubSize);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Fragmentable assemble(java.util.Iterator fragments) throws java.io.IOException
		public virtual Fragmentable assemble(IEnumerator fragments)
		{
			if (!fragments.hasNext())
			{
				throw new IOException("No fragments available.");
			}
			try
			{
				var pdu = (FaultCoPdu) fragments.next();
				var stub = pdu.Stub;
				if (stub == null)
				{
					stub = new sbyte[0];
				}
				while (fragments.hasNext())
				{
					var fragment_Renamed = (FaultCoPdu) fragments.next();
					var fragmentStub = fragment_Renamed.Stub;
					if (fragmentStub != null && fragmentStub.Length > 0)
					{
						var tmp = new sbyte[stub.Length + fragmentStub.Length];
						Array.Copy(stub, 0, tmp, 0, stub.Length);
						Array.Copy(fragmentStub, 0, tmp, stub.Length, fragmentStub.Length);
						stub = tmp;
					}
				}
				var length = stub.Length;
				if (length > 0)
				{
					pdu.Stub = stub;
					pdu.AllocationHint = length;
				}
				else
				{
					pdu.Stub = null;
					pdu.AllocationHint = 0;
				}
				pdu.setFlag(PFC_FIRST_FRAG, true);
				pdu.setFlag(PFC_LAST_FRAG, true);
				return pdu;
			}
			catch (Exception)
			{
				throw new IOException("Unable to assemble PDU fragments.");
			}
		}

		public virtual object clone()
		{
			try
			{
				return base.clone();
			}
			catch (Exception)
			{
				throw new InvalidOperationException();
			}
		}

		private class FragmentIterator : IEnumerator
		{
			private readonly FaultCoPdu outerInstance;


			internal int stubSize;

			internal int index;

			public FragmentIterator(FaultCoPdu outerInstance, int stubSize)
			{
				this.outerInstance = outerInstance;
				this.stubSize = stubSize;
			}

			public virtual bool hasNext()
			{
				return index < outerInstance.stub.Length;
			}

			public virtual object next()
			{
				if (index >= outerInstance.stub.Length)
				{
					throw new NoSuchElementException();
				}
				var fragment = (FaultCoPdu) FaultCoPdu.this.clone();
				var allocation = outerInstance.stub.Length - index;
				fragment.AllocationHint = allocation;
				if (stubSize < allocation)
				{
					allocation = stubSize;
				}
				var fragmentStub = new sbyte[allocation];
				Array.Copy(outerInstance.stub, index, fragmentStub, 0, allocation);
				fragment.Stub = fragmentStub;
				var flags = outerInstance.Flags & ~(PFC_FIRST_FRAG | PFC_LAST_FRAG);
				if (index == 0)
				{
					flags |= PFC_FIRST_FRAG;
				}
				index += allocation;
				if (index >= outerInstance.stub.Length)
				{
					flags |= PFC_LAST_FRAG;
				}
				fragment.Flags = flags;
				return fragment;
			}

			public virtual void remove()
			{
				throw new NotSupportedException();
			}

		}

	}

}