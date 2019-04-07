using System;
using System.Collections;
using System.Threading;

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


	using NdrBuffer = ndr.NdrBuffer;
	using NdrException = ndr.NdrException;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using UUID = rpc.core.UUID;

	public class RequestCoPdu : ConnectionOrientedPdu, Fragmentable {

		public const int REQUEST_TYPE = 0x00;

		private sbyte[] Stub_Renamed;

		private int AllocationHint_Renamed = 0;

		private int ContextId_Renamed = 0;

		private new int Opnum_Renamed = 0;

		private UUID @object;

		private static readonly Logger Logger = Logger.getLogger("org.jinterop");

		public override int Type {
			get {
				return REQUEST_TYPE;
			}
		}

		public virtual sbyte[] Stub {
			get {
				return Stub_Renamed;
			}
			set {
				this.Stub_Renamed = value;
			}
		}


		public virtual int AllocationHint {
			get {
				return AllocationHint_Renamed;
			}
			set {
				this.AllocationHint_Renamed = value;
			}
		}


		public virtual int ContextId {
			get {
				return ContextId_Renamed;
			}
			set {
				this.ContextId_Renamed = value;
			}
		}


		public override int Opnum {
			get {
				return Opnum_Renamed;
			}
			set {
				this.Opnum_Renamed = value;
			}
		}


		public virtual UUID Object {
			get {
				return @object;
			}
			set {
				this.@object = value;
				SetFlag(PFC_OBJECT_UUID, value != null);
			}
		}


		public override void ReadPdu(NetworkDataRepresentation ndr) {
			ReadHeader(ndr);
			ReadBody(ndr);
			ReadStub(ndr);
		}

		public override void WritePdu(NetworkDataRepresentation ndr) {
			WriteHeader(ndr);
			WriteBody(ndr);
			WriteStub(ndr);
		}

		public override void ReadBody(NetworkDataRepresentation ndr) {
			UUID @object = null;
			NdrBuffer src = ndr.Buffer;
			AllocationHint = src.Dec_ndr_long();
			ContextId = src.Dec_ndr_short();
			Opnum = src.Dec_ndr_short();
			if (GetFlag(PFC_OBJECT_UUID)) {
				@object = new UUID();
				try {
					@object.Decode(ndr, src);
				}
				catch (NdrException) {
				}
			}
			Object = @object;
		}

		public override void WriteBody(NetworkDataRepresentation ndr) {
			NdrBuffer dst = ndr.Buffer;
			dst.Enc_ndr_long(AllocationHint);
			dst.Enc_ndr_short(ContextId);
			dst.Enc_ndr_short(Opnum);
			if (GetFlag(PFC_OBJECT_UUID)) {
				try {
					object.Encode(ndr, ndr.Buffer);
				}
				catch (NdrException) {
				};
			}
		}

		public virtual void ReadStub(NetworkDataRepresentation ndr) {
			NdrBuffer src = ndr.Buffer;
			src.Align(8);
			sbyte[] stub = null;
			int length = FragmentLength - src.Index;
			if (length > 0) {
				stub = new sbyte[length];
				ndr.ReadOctetArray(stub, 0, length);
			}
			Stub = stub;
		}

		public virtual void WriteStub(NetworkDataRepresentation ndr) {
			NdrBuffer dst = ndr.Buffer;
			dst.Align(8, (sbyte) 0);
			sbyte[] stub = Stub;
			if (stub != null) {
				ndr.WriteOctetArray(stub, 0, stub.Length);
			}
		}

		public virtual IEnumerator Fragment(int size) {
			sbyte[] stub = Stub;
			if (stub == null) {
				return Arrays.asList(new RequestCoPdu[] { this }).GetEnumerator();
			}

			//subtracting 8 bytes for authentication header and 16 for the authentication verifier size, someone forgot the
			//poor guys..
			int stubSize = size - (GetFlag(PFC_OBJECT_UUID) ? 40 : 24) - 8 - 16;
			if (stub.Length <= stubSize) {
				return Arrays.asList(new RequestCoPdu[] { this }).GetEnumerator();
			}
			if (Logger.isLoggable(Level.FINEST)) {
				Logger.finest("In fragment of RequestCoPdu, this packet will be fragmented while sending...\n");
			}
			return new FragmentIterator(this, stubSize);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Fragmentable assemble(java.util.Iterator fragments) throws java.io.IOException
		public virtual Fragmentable Assemble(IEnumerator fragments) {
			if (!fragments.hasNext()) {
				throw new IOException("No fragments available.");
			}
			try {
				RequestCoPdu pdu = (RequestCoPdu) fragments.next();
				sbyte[] stub = pdu.Stub;
				if (stub == null) {
					stub = new sbyte[0];
				}
				while (fragments.hasNext()) {
					RequestCoPdu fragment = (RequestCoPdu) fragments.next();
					sbyte[] fragmentStub = fragment.Stub;
					if (fragmentStub != null && fragmentStub.Length > 0) {
						sbyte[] tmp = new sbyte[stub.Length + fragmentStub.Length];
						Array.Copy(stub, 0, tmp, 0, stub.Length);
						Array.Copy(fragmentStub, 0, tmp, stub.Length, fragmentStub.Length);
						stub = tmp;
					}
				}
				int length = stub.Length;
				if (length > 0) {
					pdu.Stub = stub;
					pdu.AllocationHint = length;
				}
				else {
					pdu.Stub = null;
					pdu.AllocationHint = 0;
				}
				pdu.SetFlag(PFC_FIRST_FRAG, true);
				pdu.SetFlag(PFC_LAST_FRAG, true);
				return pdu;
			}
			catch (Exception) {
				throw new IOException("Unable to assemble PDU fragments.");
			}
		}

		public virtual object Clone() {
			try {
				return base.Clone();
			}
			catch (Exception) {
				throw new System.InvalidOperationException();
			}
		}

		private class FragmentIterator : IEnumerator {
			private readonly RequestCoPdu OuterInstance;


			internal int StubSize;

			internal int Index = 0;

	//        private boolean firstfragsent = false;

			internal int CallId = CallIdCounter++;

			public FragmentIterator(RequestCoPdu outerInstance, int stubSize) {
				this.OuterInstance = outerInstance;
				this.StubSize = stubSize;
			}

			public virtual bool HasNext() {
				return Index < outerInstance.Stub_Renamed.Length;
			}

			public virtual object Next() {
				if (Index >= outerInstance.Stub_Renamed.Length) {
					throw new NoSuchElementException();
				}
				RequestCoPdu fragment = (RequestCoPdu) RequestCoPdu.this.clone();
				int allocation = outerInstance.Stub_Renamed.Length - Index;
				fragment.AllocationHint = allocation;
				if (StubSize < allocation) {
					allocation = StubSize;
				}
				sbyte[] fragmentStub = new sbyte[allocation];
				Array.Copy(outerInstance.Stub_Renamed, Index, fragmentStub, 0, allocation);
				fragment.Stub = fragmentStub;
				int flags = outerInstance.Flags & ~(PFC_FIRST_FRAG | PFC_LAST_FRAG);
				if (Index == 0) {
					flags |= PFC_FIRST_FRAG;
				}
				Index += allocation;
				if (Index >= outerInstance.Stub_Renamed.Length) {
					flags |= PFC_LAST_FRAG;
				}
				fragment.Flags = flags;

				//always use the same callId now
				fragment.CallId = CallId;

	//            if (firstfragsent)
	//            {
	//            	//this is so that all fragments have the same callid.
	//            	fragment.setCallId(callId);
	//            }
	//            else
	//            {
	//            	firstfragsent = true;
	//            }
				if (Logger.isLoggable(Level.FINEST)) {
					Logger.finest("In FragementIterator:next(): callIdCounter is " + CallId + " ,  for thread: " + Thread.CurrentThread);
				}
				return fragment;
			}

			public virtual void Remove() {
				throw new System.NotSupportedException();
			}

		}

	}

}