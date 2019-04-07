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
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIComVersion = org.jinterop.dcom.common.JIComVersion;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using UUID = rpc.core.UUID;

	//import com.iwombat.foundation.IdentifierFactory;
	//import com.iwombat.util.GUIDUtil;

	[Serializable]
	internal sealed class JIOrpcThis {


		private const long SerialVersionUID = 9148006530957254901L;
		private static ThreadLocal CidForCallback = new ThreadLocal();

		private int Flags = 0;
		private JIOrpcExtentArray[] Arry = null;
		private JIComVersion Version = JISystem.COMVersion;
		private string Cid = null;

		public JIOrpcThis() {
	//		cid = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString());
			Cid = java.util.UUID.randomUUID().ToString();
		}

		public JIOrpcThis(UUID casualityIdentifier) {
			Cid = casualityIdentifier.ToString();
		}

		public int ORPCFlags {
			set {
				this.Flags = value;
			}
			get {
				return Flags;
			}
		}


		public JIOrpcExtentArray[] ExtentArray {
			set {
				this.Arry = value;
			}
			get {
				return Arry;
			}
		}


		public string CasualityIdentifier {
			get {
				return Cid;
			}
		}

		public void Encode(NetworkDataRepresentation ndr) {
			ndr.writeUnsignedShort(Version.MajorVersion); //COM Major version
			ndr.writeUnsignedShort(Version.MinorVersion); //COM minor version
			ndr.writeUnsignedLong(Flags); // No Flags
			ndr.writeUnsignedLong(0); // Reserved ...always 0.

			//the order here is important since the cid is always filled from the ctor hence will never be null.
			string cid2 = CidForCallback.get() == null ? Cid : (string)CidForCallback.get();
	//		System.out.println(cid2);
			UUID uuid = new UUID(cid2);
			try {
				uuid.encode(ndr,ndr.Buffer);
			}
			catch (NdrException e) {
				JISystem.Logger.throwing("JIOrpcThis","encode",e);
			}

			int i = 0;
			if (Arry != null && Arry.Length != 0) {
				ndr.writeUnsignedLong(Arry.Length);
				ndr.writeUnsignedLong(0);
				while (i < Arry.Length) {
					JIOrpcExtentArray arryy = Arry[i];
					uuid = new UUID(arryy.GUID);
					try {
						uuid.encode(ndr,ndr.Buffer);
					}
					catch (NdrException e) {
						JISystem.Logger.throwing("JIOrpcThis","encode",e);
					}

					ndr.writeUnsignedLong(arryy.SizeOfData);
					ndr.writeOctetArray(arryy.Data,0,arryy.SizeOfData);
					i++;
				}
			}
			else {
				ndr.writeUnsignedLong(0);
			}
		}

		internal static JIOrpcThis Decode(NetworkDataRepresentation ndr) {
			JIOrpcThis retval = new JIOrpcThis();
			IDictionary map = new Hashtable();
			int majorVersion = (int)((short?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null, JIFlags.FLAG_NULL,map)));
			int minorVersion = (int)((short?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null, JIFlags.FLAG_NULL,map)));

			retval.Version = new JIComVersion(majorVersion,minorVersion);
			retval.Flags = (int)((int?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null, JIFlags.FLAG_NULL,map)));

			JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null, JIFlags.FLAG_NULL,map); //reserved.

			UUID uuid = new UUID();
			try {
				uuid.decode(ndr,ndr.Buffer);
				retval.Cid = uuid.ToString();
			}
			catch (NdrException e) {
				JISystem.Logger.throwing("JIOrpcThis","decode",e);
			}


			JIStruct orpcextentarray = new JIStruct();
			try {
			//create the orpcextent struct
			/*
			 *  typedef struct tagORPC_EXTENT
		{
			GUID                    id;          // Extension identifier.
			unsigned long           size;        // Extension size.
			[size_is((size+7)&~7)]  byte data[]; // Extension data.
		} ORPC_EXTENT;
	
			 */

				JIStruct orpcextent = new JIStruct();
				orpcextent.AddMember(typeof(UUID));
				orpcextent.AddMember(typeof(int?)); //length
				orpcextent.AddMember(new JIArray(typeof(sbyte?),null,1,true));
			//create the orpcextentarray struct
			/*
			 *    typedef struct tagORPC_EXTENT_ARRAY
		{
			unsigned long size;     // Num extents.
			unsigned long reserved; // Must be zero.
			[size_is((size+1)&~1,), unique] ORPC_EXTENT **extent; // extents
		} ORPC_EXTENT_ARRAY;
	
			 */


				orpcextentarray.AddMember(typeof(int?));
				orpcextentarray.AddMember(typeof(int?));
				//this is since the pointer is [unique]
				orpcextentarray.AddMember(new JIPointer(new JIArray(new JIPointer(orpcextent),null,1,true)));
			}
			catch (JIException) {
				//this won't fail...i am certain :)...
			}

			IList listOfDefferedPointers = new List<object>();
			JIPointer orpcextentarrayptr = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,new JIPointer(orpcextentarray),listOfDefferedPointers,JIFlags.FLAG_NULL,map);
			int x = 0;

			while (x < listOfDefferedPointers.Count) {
				List<object> newList = new List<object>();
				JIPointer replacement = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,(JIPointer)listOfDefferedPointers[x],newList,JIFlags.FLAG_NULL,map);
				((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
				x++;
				listOfDefferedPointers.AddRange(x,newList);
			}

			List<object> extentArrays = new List<object>();
			//now read whether extend array exists or not
			if (!orpcextentarrayptr.Null) {
				JIPointer[] pointers = (JIPointer[])((JIArray)((JIPointer)((JIStruct)orpcextentarrayptr.GetReferent()).GetMember(2)).GetReferent()).ArrayInstance;
				for (int i = 0;i < pointers.Length;i++) {
					if (pointers[i].Null) {
						continue;
					}

					JIStruct orpcextent2 = (JIStruct)pointers[i].GetReferent();
					sbyte?[] byteArray = (sbyte?[])((JIArray)orpcextent2.GetMember(2)).ArrayInstance;

					extentArrays.Add(new JIOrpcExtentArray(((UUID)orpcextent2.GetMember(0)).ToString(),byteArray.Length,byteArray));
				}

			}

			retval.Arry = (JIOrpcExtentArray[])extentArrays.ToArray(typeof(JIOrpcExtentArray));

			//decode can only be executed incase of a request made from the server side in case of a callback. so the thread making this
			//callback will store the cid from the decode operation in the threadlocal variable. In case an encode is performed using the
			//same thread then we know that this is a nested call. Hence will replace the cid with the thread local cid. For the calls being in
			//case of encode this value will not be used if the encode thread is of the client and not of JIComOxidRuntimeHelper.
			CidForCallback.set(retval.Cid);
			return retval;
		}

	}

}