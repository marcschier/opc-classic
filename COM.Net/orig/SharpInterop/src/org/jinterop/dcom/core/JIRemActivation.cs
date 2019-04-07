using System;
using System.Collections;
using System.Collections.Generic;

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
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIComVersion = org.jinterop.dcom.common.JIComVersion;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using UUID = rpc.core.UUID;

	internal sealed class JIRemActivation : NdrObject, JIIServerActivation {

		private int ImpersonationLevel = JIIServerActivation_Fields.RPC_C_IMP_LEVEL_IMPERSONATE;
		private int Mode_Renamed = 0;
		private string MonikerName = null;
		private UUID Clsid = null;
		private bool ActivationSuccessful_Renamed = false;
		private JIOrpcThat Orpcthat = null;
		private sbyte[] Oxid_Renamed = null;
		private JIDualStringArray DualStringArrayForOxid_Renamed = null;
		private string Ipid = null;
		private int AuthenticationHint_Renamed = -1;
		private JIComVersion ComVersion_Renamed = null;
		private int Hresult_Renamed = -1;
		private JIInterfacePointer MInterfacePointer_Renamed = null;
		internal bool IsDual = false;
		internal string DispIpid_Renamed = null;
		internal int DispRefs_Renamed = 5;
		internal sbyte[] DispOid = null;

		public JIRemActivation(string clsid) {
			//10000002-0000-0000-0000-000000000001 Inside DCOM
			this.Clsid = new UUID(clsid);
		}

		public int Mode {
			set {
				this.Mode_Renamed = value;
			}
		}
		public int ClientImpersonationLevel {
			set {
				ImpersonationLevel = value;
			}
		}

		public void SetfileMonikerAtServer(string name) {
			if (name != null && !name.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
				MonikerName = name;
			}
		}

		public int Opnum {
			get {
				return 0;
			}
		}
		public void Write(NetworkDataRepresentation ndr) {


			JIOrpcThis orpcThis = new JIOrpcThis();
			orpcThis.Encode(ndr);

			//JIClsid of the component being activated.
			UUID uuid = new UUID();
			uuid.parse(Clsid.ToString());
			try {
				uuid.encode(ndr,ndr.buf);
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIRemActivation","write",e);
			}
			if (MonikerName == null) {
				ndr.writeUnsignedLong(0);
			}
			else {
				ndr.writeCharacterArray(MonikerName.ToCharArray(),0,MonikerName.Length); // Object Name
			}


			ndr.writeUnsignedLong(0); // Minterface pointer
			ndr.writeUnsignedLong(ImpersonationLevel); // impersonation level
			ndr.writeUnsignedLong(Mode_Renamed); //mode, when object name , interface pointer are not null , this is passed directly to IPersistFile:Load

			ndr.writeUnsignedLong(2); //No. of IIDs requested.

			ndr.writeUnsignedLong((new object()).GetHashCode());

			ndr.writeUnsignedLong(2); //Array length

			//IID of IUnknown , this is hard coded here, standard way of COM is to first get a handle to the IUnknown
			uuid.parse("00000000-0000-0000-c000-000000000046");
			try {
				uuid.encode(ndr,ndr.buf);
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIRemActivation","write",e);
			}

			//checking for IDispatch support
			uuid.parse("00020400-0000-0000-c000-000000000046");
			try {
				uuid.encode(ndr,ndr.buf);
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIRemActivation","write",e);
			}

			ndr.writeUnsignedLong(1); //Protocol Sequences available
			ndr.writeUnsignedLong(1); //Array length
			ndr.writeUnsignedShort(7); //TCP

			sbyte[] address = JISession.LocalhostAddressAsIPbytes;

			ndr.writeUnsignedShort(address[0]);
			ndr.writeUnsignedShort(address[1]);
			ndr.writeUnsignedShort(address[2]);
			ndr.writeUnsignedShort(address[3]);
			ndr.writeUnsignedShort(0);
		}


		public void Read(NetworkDataRepresentation ndr) {

			//first take out JIOrpcThat
			Orpcthat = JIOrpcThat.Decode(ndr);

			//now fill the oxid
			Oxid_Renamed = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8);

			int skipdual = ndr.readUnsignedLong();

			if (skipdual != 0) {
				ndr.readUnsignedLong();
				//now fill the dual string array for oxid bindings, the call to IRemUnknown will be
				//directed to this address and the port in that address.
				DualStringArrayForOxid_Renamed = JIDualStringArray.Decode(ndr);
			}
			//get the IPID which will be the "Object" in the call to IRemUknown. This is the IPID of the
			//component which has been specified as the JIClsid. This may differ in multiple invokations of
			//of remote activation as everytime a new object may be created at the server per call. This is all
			//server implementation dependent.
			try {
				UUID ipid2 = new UUID();
				ipid2.decode(ndr,ndr.Buffer);
				Ipid = (ipid2.ToString());
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIRemActivation","read",e);
			}

			//read the auth hint
			AuthenticationHint_Renamed = ndr.readUnsignedLong();

			ComVersion_Renamed = new JIComVersion();
			ComVersion_Renamed.MajorVersion = ndr.readUnsignedShort();
			ComVersion_Renamed.MinorVersion = ndr.readUnsignedShort();

			Hresult_Renamed = ndr.readUnsignedLong();

			if (Hresult_Renamed != 0) {
				//System.out.println("EXCEPTION FROM SERVER ! --> " + "0x" + Long.toHexString(hresult).substring(8));
				throw new JIRuntimeException(Hresult_Renamed);
			}


			//int numRet = ndr.readUnsignedLong();//Number of interface pointers returned. Currently only 2.

			JIArray array = new JIArray(typeof(JIInterfacePointer),null,1,true);
			List<object> listOfDefferedPointers = new List<object>();
			array = (JIArray)JIMarshalUnMarshalHelper.DeSerialize(ndr,array,listOfDefferedPointers,JIFlags.FLAG_NULL,new Hashtable());
			int x = 0;

			while (x < listOfDefferedPointers.Count) {

				List<object> newList = new List<object>();
				JIPointer replacement = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,(JIPointer)listOfDefferedPointers[x],newList,JIFlags.FLAG_NULL,null);
				((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
				x++;
				listOfDefferedPointers.AddRange(x,newList);
			}
			JIInterfacePointer[] arrayObjs = (JIInterfacePointer[])array.ArrayInstance;
			MInterfacePointer_Renamed = arrayObjs[0];

			if (arrayObjs[1] != null) {
				//dual is supported since the IDispatch was obtained
				IsDual = true;
				//eat this keeping only the IPID for cleanup , let the user perform another queryInterface for this.
				JIInterfacePointer ptr = arrayObjs[1];
				DispIpid_Renamed = ptr.IPID;
				DispOid = ptr.OID;
				DispRefs_Renamed = ((JIStdObjRef)ptr.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
			}

			array = new JIArray(typeof(int?),null,1,true);
			//ignore the retvals
			JIMarshalUnMarshalHelper.DeSerialize(ndr,array,null,JIFlags.FLAG_NULL,null);

			ActivationSuccessful_Renamed = true;

		}

		/* (non-Javadoc)
		 * @see org.jinterop.dcom.core.JIIServerActivation#isActivationSuccessful()
		 */
		public bool ActivationSuccessful {
			get {
				return ActivationSuccessful_Renamed;
			}
		}

		public JIOrpcThat ORPCThat {
			get {
				return Orpcthat;
			}
		}

		public sbyte[] Oxid {
			get {
				return Oxid_Renamed;
			}
		}

		/* (non-Javadoc)
		 * @see org.jinterop.dcom.core.JIIServerActivation#getDualStringArrayForOxid()
		 */
		public JIDualStringArray DualStringArrayForOxid {
			get {
				return DualStringArrayForOxid_Renamed;
			}
		}

		public int AuthenticationHint {
			get {
				return AuthenticationHint_Renamed;
			}
		}

		public JIComVersion ComVersion {
			get {
				return ComVersion_Renamed;
			}
		}

		public int Hresult {
			get {
				return Hresult_Renamed;
			}
		}

		/* (non-Javadoc)
		 * @see org.jinterop.dcom.core.JIIServerActivation#getMInterfacePointer()
		 */
		public JIInterfacePointer MInterfacePointer {
			get {
				return MInterfacePointer_Renamed;
			}
		}

		/* (non-Javadoc)
		 * @see org.jinterop.dcom.core.JIIServerActivation#getIPID()
		 */
		public string IPID {
			get {
				return Ipid;
			}
		}

		public bool Dual {
			get {
				return IsDual;
			}
		}

		public string DispIpid {
			get {
				return DispIpid_Renamed;
			}
			set {
				this.DispIpid_Renamed = value;
			}
		}

		public int DispRefs {
			get {
				return DispRefs_Renamed;
			}
		}

	}

}