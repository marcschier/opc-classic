using System;
using System.Collections;

/// <summary>
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
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {


	using NdrException = ndr.NdrException;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

	/// <summary>
	///<para> Class representing a Marshalled Interface Pointer. You will never use the members of this
	/// class directly, but always as an implementation of <code>IJIComObject</code> interface.
	/// <br>
	/// Sample Usage:-
	/// <br><code>
	/// IJIComObject connectionPointContainer = (IJIComObject)ieObject.queryInterface("B196B284-BAB4-101A-B69C-00AA00341D07"); <br>
	/// JICallBuilder object = new JICallBuilder(connectionPointContainer.getIpid(),true); <br>
	/// object.setOpnum(1); <br>
	/// object.addInParamAsUUID("34A715A0-6587-11D0-924A-0020AFC7AC4D",JIFlags.FLAG_NULL); <br>
	/// object.addOutParamAsObject(JIInterfacePointer.class,JIFlags.FLAG_NULL); <br>
	/// Object[] objects = (Object[])connectionPointContainer.call(object); //find connection point <br>
	/// JIInterfacePointer connectionPtr = (JIInterfacePointer)objects[0]; <br>
	/// IJIComObject connectionPointer = JIObjectFactory.createCOMInstance(connectionPointContainer,connectionPtr); <br>
	/// </code>
	/// </para>
	/// @since 1.0
	/// 
	/// </summary>
	[Serializable]
	internal sealed class JIInterfacePointer {

	//	static boolean inTest = true;

		private JIPointer Member = null;
		private const long SerialVersionUID = 2508592294719469453L;
		internal static readonly sbyte[] OBJREF_SIGNATURE = new sbyte[] { 0x4d,0x45,0x4f,0x57 }; // 'MEOW'
		internal const int OBJREF_STANDARD = 0x1; // standard marshaled objref
		internal const int OBJREF_HANDLER = 0x2; // handler marshaled objref
		internal const int OBJREF_CUSTOM = 0x4; // custom marshaled objref

		// Flag values for a STDOBJREF (standard part of an OBJREF).
		// SORF_OXRES1 - SORF_OXRES8 are reserved for the object exporters
		// use only, object importers must ignore them and must not enforce MBZ.
		internal const int SORF_OXRES1 = 0x1; // reserved for exporter
		internal const int SORF_OXRES2 = 0x20; // reserved for exporter
		internal const int SORF_OXRES3 = 0x40; // reserved for exporter
		internal const int SORF_OXRES4 = 0x80; // reserved for exporter
		internal const int SORF_OXRES5 = 0x100; // reserved for exporter
		internal const int SORF_OXRES6 = 0x200; // reserved for exporter
		internal const int SORF_OXRES7 = 0x400; // reserved for exporter
		internal const int SORF_OXRES8 = 0x800; // reserved for exporter
		internal const int SORF_NULL = 0x0; // convenient for initializing SORF
		internal const int SORF_NOPING = 0x1000; // Pinging is not required


		public bool CustomObjRef {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).CustomObjRef;
			}
		}

		public string CustomCLSID {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).CustomCLSID;
			}
		}

		private JIInterfacePointer() {
		}

		/// <summary>
		/// Called from Oxid Resolver master, the resolver address are put in here itself
		/// </summary>
		/// <param name="iid"> </param>
		/// <param name="ipid"> </param>
		/// <param name="oxid"> </param>
		/// <param name="oid"> </param>
		public JIInterfacePointer(string iid, int port, JIStdObjRef objref) {
			Member = new JIPointer(new JIInterfacePointerBody(iid,port,objref),false);
		}

		public JIInterfacePointer(string iid, JIInterfacePointer interfacePointer) {
			Member = new JIPointer(new JIInterfacePointerBody(iid,interfacePointer),false);
		}

		public bool Deffered {
			set {
				Member.Deffered = true;
			}
		}

		internal static JIInterfacePointer Decode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
			JIInterfacePointer ptr = new JIInterfacePointer();
			if ((FLAG & JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) == JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) {
				ptr.Member = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,new JIPointer(typeof(JIInterfacePointerBody),true),defferedPointers,FLAG,additionalData);
			}
			else {
				ptr.Member = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,new JIPointer(typeof(JIInterfacePointerBody)),defferedPointers,FLAG,additionalData);
			}
			//the pointer is null, no point of it's wrapper being present, so return null from here as well
			if (ptr.Member.Null) {
				ptr = null;
			}
			return ptr;
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public int ObjectType {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).ObjectType;
			}
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="objectType">
		/// @return </param>
		public object GetObjectReference(int objectType) {
			return ((JIInterfacePointerBody)(Member.GetReferent())).GetObjectReference(objectType);
		}

		/// <summary>
		///Returns the Interface Identifier for this MIP.
		/// </summary>
		/// <returns> String representation of 128 bit uuid. </returns>
		public string IID {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).IID;
			}
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public string IPID {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).IPID;
			}
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public sbyte[] OID {
			get {
				return ((JIStdObjRef)((JIInterfacePointerBody)(Member.GetReferent())).GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).ObjectId;
			}
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public sbyte[] OXID {
			get {
				return ((JIStdObjRef)((JIInterfacePointerBody)(Member.GetReferent())).GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Oxid;
			}
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public JIDualStringArray StringBindings {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).StringBindings;
			}
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public int Length {
			get {
				return ((JIInterfacePointerBody)(Member.GetReferent())).Length;
			}
		}


		public void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG) {

			if ((FLAG & JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT) == JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT) {
				//just encode a null.
				JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),defferedPointers,FLAG);
				return;
			}
			JIMarshalUnMarshalHelper.Serialize(ndr,Member.GetType(),Member,defferedPointers,FLAG);
		}



		public override string ToString() {
			string retVal = "JIInterfacePointer[IID:" + IID + " , ObjRef: " + GetObjectReference(JIInterfacePointer.OBJREF_STANDARD) + "]";
			return retVal;
		}

		public static bool IsOxidEqual(JIInterfacePointer src, JIInterfacePointer target) {
			if (src == null || target == null) {
				throw new System.NullReferenceException();
			}

			return Arrays.Equals(src.OXID, target.OXID);
		}

	//    public static void main(String[] args) {
	//
	//
	//		byte[] buffer = new byte[183];
	//		FileInputStream inputStream;
	//		try {
	//			inputStream = new FileInputStream("F:/tmp/experiment/rawip2");
	//			inputStream.read(new byte[13],0,13);
	//			inputStream.read(buffer,0,183);
	//		} catch (Exception e) {
	//			// TODO Auto-generated catch block
	//			e.printStackTrace();
	//		}
	//		NetworkDataRepresentation ndr = new NetworkDataRepresentation();
	//		NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);
	//		ndr.setBuffer(ndrBuffer);
	//		ndrBuffer.length = 183;
	//
	//    	JIInterfacePointer ptr = JIInterfacePointer.decode(ndr, new ArrayList(), 0, new HashMap());
	//    	try {
	//    		JISystem.getLogger().setLevel(Level.FINEST);
	//			JISystem.setInBuiltLogHandler(false);
	//
	//		} catch (SecurityException e1) {
	//			// TODO Auto-generated catch block
	//			e1.printStackTrace();
	//		} catch (IOException e1) {
	//			// TODO Auto-generated catch block
	//			e1.printStackTrace();
	//		}
	//
	//    	JISession session = JISession.createSession("deepspace9", "administrator", "enterprise");
	//    	session.useSessionSecurity(true);
	//    	try {
	//    		JIComServer comServer = new JIComServer(session,ptr,null);
	//			IJIComObject comObject = comServer.getInstance();
	//			comObject.queryInterface("87bc18dc-c8b3-11d5-ae96-00b0d0e93ca1");
	//		} catch (JIException e) {
	//			// TODO Auto-generated catch block
	//			e.printStackTrace();
	//		}
	//	}


	}

	/// <summary>
	/// @exclude
	/// </summary>
	[Serializable]
	internal class JIInterfacePointerBody {
			private const long SerialVersionUID = 2597456459096838320L;
			private string Iid = null;
			private string CustomCLSID_Renamed = null;
			private int ObjectType_Renamed = -1;
			private JIStdObjRef StdObjRef = null;
			private int Length_Renamed = -1;
			private JIDualStringArray ResolverAddr = null;
			private int Port = -1; //to be used when doing local resolution.

			private JIInterfacePointerBody() {
			}



	//	    private byte[] customObjRefDefn = null;

			public virtual bool CustomObjRef {
				get {
					return this.ObjectType_Renamed == JIInterfacePointer.OBJREF_CUSTOM;
				}
			}

			public virtual string CustomCLSID {
				get {
					return this.CustomCLSID_Renamed;
				}
			}

	//	    byte[] getCustomObjRefDefn()
	//	    {
	//	    	return this.customObjRefDefn;
	//	    }

			/// <summary>
			/// Called from Oxid Resolver master, the resolver address are put in here itself
			/// </summary>
			/// <param name="iid"> </param>
			/// <param name="ipid"> </param>
			/// <param name="oxid"> </param>
			/// <param name="oid"> </param>
			public JIInterfacePointerBody(string iid, int port, JIStdObjRef objref) {
				this.Iid = iid;
				this.StdObjRef = objref;
				this.Port = port;
				ResolverAddr = new JIDualStringArray(port);
				Length_Renamed = 40 + 4 + 4 + 16 + ResolverAddr.Length;
			}

			public JIInterfacePointerBody(string iid, JIInterfacePointer interfacePointer) {
				this.Iid = iid;
				StdObjRef = (JIStdObjRef)interfacePointer.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD);
				ResolverAddr = interfacePointer.StringBindings;
				Length_Renamed = 40 + 4 + 4 + 16 + ResolverAddr.Length;
			}

	//	    private static int ff = 0;
			internal static JIInterfacePointerBody Decode(NetworkDataRepresentation ndr, int Flags) {
				if ((Flags & JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) == JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) {
					return Decode2(ndr);
				}

				int length = ndr.readUnsignedLong();
				ndr.readUnsignedLong(); //length

				JIInterfacePointerBody ptr = new JIInterfacePointerBody();
				ptr.Length_Renamed = length;
				//check for MEOW
				sbyte[] b = new sbyte[4];
				ndr.readOctetArray(b,0,4);

				int i = 0;
				while (i != 4) {
					//not MEOW then what ?
					if (b[i] != JIInterfacePointer.OBJREF_SIGNATURE[i]) {
						return null;
					}
					i++;
				}

				//TODO only STDOBJREF supported for now

				if ((ptr.ObjectType_Renamed = ndr.readUnsignedLong()) != JIInterfacePointer.OBJREF_STANDARD) {

					try {
						rpc.core.UUID ipid2 = new rpc.core.UUID();
						ipid2.decode(ndr,ndr.Buffer);
						ptr.Iid = ipid2.ToString();
					}
					catch (NdrException e) {
						JISystem.Logger.throwing("JIInterfacePointer","decode",e);
					}

					//now for CLSID 
					try {
						rpc.core.UUID ipid2 = new rpc.core.UUID();
						ipid2.decode(ndr,ndr.Buffer);
						ptr.CustomCLSID_Renamed = ipid2.ToString();
					}
					catch (NdrException e) {
						JISystem.Logger.throwing("JIInterfacePointer","decode",e);
					}

					//extension
					ndr.readUnsignedLong();

					//reserved
					ndr.readUnsignedLong();

					//We copy everything into the custom byte[] and return
					//IID, CLSID, NULL
	//				byte[] header = new byte[16 + 16 + 4];
	//				ndr.readOctetArray(header, 0, header.length);
	//				System.out.println(ff++);
	//				jcifs.util.Hexdump.hexdump(System.out, header, 0, header.length);
	//				System.out.println();
	//				int index = ndr.getBuffer().index;
	//				//Header, length, size(length) 
	//				ptr.customObjRefDefn = new byte[header.length + ndr.readUnsignedLong() + 4];
	//				System.arraycopy(header, 0, ptr.customObjRefDefn, 0, header.length);
	//				ndr.getBuffer().setIndex(index);
	//				ndr.readOctetArray(ptr.customObjRefDefn, header.length, ptr.customObjRefDefn.length - header.length);
					return ptr;
				}

				try {
					rpc.core.UUID ipid2 = new rpc.core.UUID();
					ipid2.decode(ndr,ndr.Buffer);
					ptr.Iid = ipid2.ToString();
				}
				catch (NdrException e) {
					JISystem.Logger.throwing("JIInterfacePointer","decode",e);
				}

				ptr.StdObjRef = JIStdObjRef.Decode(ndr);

				ptr.ResolverAddr = JIDualStringArray.Decode(ndr);

				return ptr;
			}

			internal static JIInterfacePointerBody Decode2(NetworkDataRepresentation ndr) {


				JIInterfacePointerBody ptr = new JIInterfacePointerBody();

				//check for MEOW
				sbyte[] b = new sbyte[4];
				ndr.readOctetArray(b,0,4);

				int i = 0;
				while (i != 4) {
					//not MEOW then what ?
					if (b[i] != JIInterfacePointer.OBJREF_SIGNATURE[i]) {
						return null;
					}
					i++;
				}

				//TODO only STDOBJREF supported for now

				if ((ptr.ObjectType_Renamed = ndr.readUnsignedLong()) != JIInterfacePointer.OBJREF_STANDARD) {
					return null;
				}

				try {
					rpc.core.UUID ipid2 = new rpc.core.UUID();
					ipid2.decode(ndr,ndr.Buffer);
					ptr.Iid = ipid2.ToString();
				}
				catch (NdrException e) {
					JISystem.Logger.throwing("JIInterfacePointer","decode",e);
				}

				ptr.StdObjRef = JIStdObjRef.Decode(ndr);

				ptr.ResolverAddr = JIDualStringArray.Decode(ndr);

				return ptr;
			}

			/// <summary>
			/// @exclude
			/// @return
			/// </summary>
			public virtual int ObjectType {
				get {
					return ObjectType_Renamed;
				}
			}

			/// <summary>
			/// @exclude </summary>
			/// <param name="objectType">
			/// @return </param>
			public virtual object GetObjectReference(int objectType) {
				if (objectType == JIInterfacePointer.OBJREF_STANDARD) {
					return StdObjRef;
				}
				else {
					return null;
				}
			}

			/// <summary>
			///Returns the Interface Identifier for this MIP.
			/// </summary>
			/// <returns> String representation of 128 bit uuid. </returns>
			public virtual string IID {
				get {
					return Iid;
				}
			}

			/// <summary>
			/// @exclude
			/// @return
			/// </summary>
			public virtual string IPID {
				get {
					return StdObjRef.Ipid;
				}
			}

			/// <summary>
			/// @exclude
			/// @return
			/// </summary>
			public virtual sbyte[] OID {
				get {
					return StdObjRef.ObjectId;
				}
			}

			/// <summary>
			/// @exclude
			/// @return
			/// </summary>
			public virtual JIDualStringArray StringBindings {
				get {
					return ResolverAddr;
				}
			}

			/// <summary>
			/// @exclude
			/// @return
			/// </summary>
			public virtual int Length {
				get {
					return Length_Renamed;
				}
			}


			public virtual void Encode(NetworkDataRepresentation ndr, int FLAGS) {

				//now for length
				//the length for STDOBJREF is fixed 40 bytes : 4,4,8,8,16.
				//Dual string array has to be computed, since that can vary. MEOW = 4., flag stdobjref = 4
				// + 16 bytes of ipid
				int length = 0;
				if (!CustomObjRef) {
					length = 40 + 4 + 4 + 16 + ResolverAddr.Length;
				}


				ndr.writeUnsignedLong(length);
				ndr.writeUnsignedLong(length);

				//for OBJREF_CUSTOM we will correct this length after the custom object has been marshalled.
				//this object is marshalled 4 + 4 + 40 bytes after this point. The length of the length itself is not included. 

				ndr.writeOctetArray(JIInterfacePointer.OBJREF_SIGNATURE,0,4);

				if (CustomObjRef) {
					ndr.writeUnsignedLong(JIInterfacePointer.OBJREF_CUSTOM);
					try {
						rpc.core.UUID ipid2 = new rpc.core.UUID(Iid);
						ipid2.encode(ndr,ndr.Buffer);
						ipid2 = new rpc.core.UUID(CustomCLSID_Renamed);
						ipid2.encode(ndr,ndr.Buffer);
						ndr.writeUnsignedLong(0); //extension
						ndr.writeUnsignedLong(0); //reserved, now the spec say that this is ignored by the server but the
						//the WMIO marshaller puts the length of the entire buffer here. If this is the case then we will have to go
						//4 bytes back and rewrite this with total lengths in the custom marshaller.
					}
					catch (NdrException e) {
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}

					return; //rest will be filled by the Custom Marshaller.
				}

				//std ref
				ndr.writeUnsignedLong(JIInterfacePointer.SORF_OXRES1);

				try {
					rpc.core.UUID ipid2 = new rpc.core.UUID(Iid);

					if ((FLAGS & JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) == JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) {
						ipid2 = new rpc.core.UUID(IJIComObject_Fields.IID);
					}
					else if ((FLAGS & JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) == JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) {
						ipid2 = new rpc.core.UUID(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID);
					}

					ipid2.encode(ndr,ndr.Buffer);
				}
				catch (NdrException e) {
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				StdObjRef.Encode(ndr);

				ResolverAddr.Encode(ndr);


			}


	}
}