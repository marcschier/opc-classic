using System;

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

	[Serializable]
	internal sealed class JIStdObjRef {


		private const long SerialVersionUID = 7714589108476632990L;


		private JIStdObjRef() {
		}

		private int Flags_Renamed = 0x0;
		private int PublicRefs_Renamed = -1;
		private sbyte[] Oxid_Renamed = null;
		private sbyte[] Oid = null;
		private string IpidOfthisObjectRef = null;
	//	private String oidString = null;


		/// <summary>
		/// Resolver address are taken of localhost
		/// 
		/// </summary>
		public JIStdObjRef(string ipid, JIOxid oxid, JIObjectId oid) {
			this.IpidOfthisObjectRef = ipid;
			this.Oxid_Renamed = oxid.OXID;
			this.Oid = oid.OID;
	//		this.oidString = oid.toString();
			this.PublicRefs_Renamed = 5;
		}


		/// <summary>
		/// This is used to instantiate an empty StdObjRef for 
		///   cases where the interface is not supported.
		/// </summary>
		public JIStdObjRef(string ipid) {
			this.IpidOfthisObjectRef = ipid;
			this.Flags_Renamed = 0x0;
			this.Oxid_Renamed = new sbyte[]{ 0,0,0,0,0,0,0,0 };
			this.Oid = new sbyte[]{ 0,0,0,0,0,0,0,0 };
			this.PublicRefs_Renamed = 0;
		}



		internal static JIStdObjRef Decode(NetworkDataRepresentation ndr) {
			JIStdObjRef objRef = new JIStdObjRef();

			objRef.Flags_Renamed = ndr.readUnsignedLong();
			objRef.PublicRefs_Renamed = ndr.readUnsignedLong();

			objRef.Oxid_Renamed = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8);

			objRef.Oid = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8);

	//		ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
	//	   	jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), objRef.oid, 0, objRef.oid.length);
	//	   	objRef.oidString = byteArrayOutputStream.toString();


			try {
				rpc.core.UUID ipid2 = new rpc.core.UUID();
				ipid2.decode(ndr,ndr.Buffer);
				objRef.IpidOfthisObjectRef = ipid2.ToString();
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIStdObjRef","decode",e);
			}

	//		if (JISystem.getLogger().isLoggable(Level.WARNING))
	//        {
	//			ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
	//		   	jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), objRef.oid, 0, objRef.oid.length);
	//		   	JISystem.getLogger().warning("Decode of StdObjref Adding references for " + objRef.ipidOfthisObjectRef + " , num references recieved from COM server: " + objRef.publicRefs + " , the OID is " + byteArrayOutputStream.toString());
	//		   	JISession.debug_addIpids(objRef.ipidOfthisObjectRef, 5);
	//        }


			return objRef;
		}

		public int Flags {
			get {
				return Flags_Renamed;
			}
		}

		public int PublicRefs {
			get {
				return PublicRefs_Renamed;
			}
		}

		public sbyte[] Oxid {
			get {
				return Oxid_Renamed;
			}
		}

		public sbyte[] ObjectId {
			get {
				return Oid;
			}
		}

		public string Ipid {
			get {
				return IpidOfthisObjectRef;
			}
		}


		public void Encode(NetworkDataRepresentation ndr) {
			ndr.writeUnsignedLong(Flags_Renamed);
			ndr.writeUnsignedLong(PublicRefs_Renamed);
			JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,Oxid_Renamed);
			JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,Oid);

			try {
				rpc.core.UUID ipid = new rpc.core.UUID(IpidOfthisObjectRef);
				ipid.encode(ndr,ndr.Buffer);
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIStdObjRef","encode",e);
			}
		}

		public override string ToString() {
			string retVal = "IPID: " + IpidOfthisObjectRef; //+ " , OID: " + oidString;
			return retVal;
		}
	}

}