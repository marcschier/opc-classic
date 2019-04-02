using System;
using System.Collections;
using System.Threading;

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
	using NdrException = ndr.NdrException;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using UUID = core.UUID;

	public class RequestCoPdu : ConnectionOrientedPdu, Fragmentable
	{

		public const int REQUEST_TYPE = 0x00;

		private sbyte[] stub;

		private int allocationHint;

		private int contextId;

		private new readonly int opnum;

		private UUID @object;

		private static readonly Logger logger = Logger.getLogger("org.jinterop");

        public override int Type => REQUEST_TYPE;

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


        public override int Opnum {
            get => opnum;
            set => value = value;
        }


        public virtual UUID Object {
            get => @object;
            set {
                @object = value;
                setFlag(PFC_OBJECT_UUID, value != null);
            }
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
			UUID @object = null;
			var src = ndr.Buffer;
			AllocationHint = src.dec_ndr_long();
			ContextId = src.dec_ndr_short();
			Opnum = src.dec_ndr_short();
			if (getFlag(PFC_OBJECT_UUID))
			{
				@object = new UUID();
				try
				{
					@object.decode(ndr, src);
				}
				catch (NdrException)
				{
				}
			}
			Object = @object;
		}

		protected internal override void writeBody(NetworkDataRepresentation ndr)
		{
			var dst = ndr.Buffer;
			dst.enc_ndr_long(AllocationHint);
			dst.enc_ndr_short(ContextId);
			dst.enc_ndr_short(Opnum);
			if (getFlag(PFC_OBJECT_UUID))
			{
				try
				{
					object.encode(ndr, ndr.Buffer);
				}
				catch (NdrException)
				{
				};
			}
		}

		protected internal virtual void readStub(NetworkDataRepresentation ndr)
		{
			var src = ndr.Buffer;
			src.align(8);
			sbyte[] stub = null;
			var length = FragmentLength - src.Index;
			if (length > 0)
			{
				stub = new sbyte[length];
				ndr.readOctetArray(stub, 0, length);
			}
			Stub = stub;
		}

		protected internal virtual void writeStub(NetworkDataRepresentation ndr)
		{
			var dst = ndr.Buffer;
			dst.align(8, (sbyte) 0);
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
				return Arrays.asList(new RequestCoPdu[] {this}).GetEnumerator();
			}

			//subtracting 8 bytes for authentication header and 16 for the authentication verifier size, someone forgot the
			//poor guys..
			var stubSize = size - (getFlag(PFC_OBJECT_UUID) ? 40 : 24) - 8 - 16;
			if (stub.Length <= stubSize)
			{
				return Arrays.asList(new RequestCoPdu[] {this}).GetEnumerator();
			}
			if (logger.isLoggable(Level.FINEST))
			{
				logger.finest("In fragment of RequestCoPdu, this packet will be fragmented while sending...\n");
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
				var pdu = (RequestCoPdu) fragments.next();
				var stub = pdu.Stub;
				if (stub == null)
				{
					stub = new sbyte[0];
				}
				while (fragments.hasNext())
				{
					var fragment_Renamed = (RequestCoPdu) fragments.next();
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
			private readonly RequestCoPdu outerInstance;


			internal int stubSize;

			internal int index;

	//        private bool firstfragsent = false;

			internal int callId = callIdCounter++;

			public FragmentIterator(RequestCoPdu outerInstance, int stubSize)
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
				var fragment = (RequestCoPdu) RequestCoPdu.this.clone();
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

				//always use the same callId now
				fragment.CallId = callId;

	//            if (firstfragsent)
	//            {
	//            	//this is so that all fragments have the same callid.
	//            	fragment.setCallId(callId);
	//            }
	//            else
	//            {
	//            	firstfragsent = true;
	//            }
				if (logger.isLoggable(Level.FINEST))
				{
					logger.finest("In FragementIterator:next(): callIdCounter is " + callId + " ,  for thread: " + Thread.CurrentThread);
				}
				return fragment;
			}

			public virtual void remove()
			{
				throw new NotSupportedException();
			}

		}

	}

}