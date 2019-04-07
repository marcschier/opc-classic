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


	using NdrBuffer = ndr.NdrBuffer;
	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIComVersion = org.jinterop.dcom.common.JIComVersion;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using UUID = rpc.core.UUID;

	 /// <summary>
	 /// IRemoteSCMActivator implementation. 
	 /// 
	 /// @since 2.09
	 /// 
	 /// </summary>
	internal sealed class JIRemoteSCMActivator {

		internal class RemoteCreateInstance : NdrObject, JIIServerActivation {
			private readonly JIRemoteSCMActivator OuterInstance;


	//		HRESULT RemoteCreateInstance(
	//				[in] handle_t hRpc,
	//				[in] ORPCTHIS* orpcthis,
	//				[out] ORPCTHAT* orpcthat,
	//				[in, unique] MInterfacePointer* pUnkOuter,
	//				[in, unique] MInterfacePointer* pActProperties,
	//				[out] MInterfacePointer** ppActProperties
	//				);

			internal readonly string TargetClsid;
			internal readonly string TargetServer;
			internal sbyte[] Oxid = null;
			internal JIDualStringArray DualStringArrayForOxid = null;
			internal string Ipid = null;
			internal int AuthenticationHint = -1;
			internal JIComVersion ComVersion = null;
			internal JIInterfacePointer MInterfacePointer = null;
			internal bool IsDual = false;
			internal string DispIpid = null;
			internal int DispRefs = 5;
			internal sbyte[] DispOid = null;
			internal bool IsActivationSuccessful = false;

			public RemoteCreateInstance(JIRemoteSCMActivator outerInstance, string targetServer, string clsid) {
				this.OuterInstance = outerInstance;
				this.TargetClsid = clsid;
				this.TargetServer = targetServer;
			}

			public virtual int Opnum {
				get {
					return 4;
				}
			}

			public virtual void Write(NetworkDataRepresentation ndr) {
				JIOrpcThis orpcThis = new JIOrpcThis();
				orpcThis.Encode(ndr);

				 ndr.writeUnsignedLong(0); // pUnkOuter, setting it to NULL.

				 ndr.writeUnsignedLong(0x00020000);

				 int index = ndr.Buffer.Index; //recording where we have to write length

				 ndr.writeUnsignedLong(0); //Len 1

				//alignment may kick in
				 int index2 = ndr.Buffer.Index; //recording where we have to write length

				 ndr.writeUnsignedLong(0); //Len 2

				 int countFromIndex = ndr.Buffer.Index; //recording from where we have to write

				 ndr.writeUnsignedLong(0x574f454d); // Signature MEOW
				 ndr.writeUnsignedLong(4); // OBJREF_CUSTOM

				 //now we will write the Custom Interface pointer to Activation Properties.
				 try {
					//IID_IActivationPropertiesIn
					 UUID iid_IActivationPropertiesIn = new UUID("000001a2-0000-0000-c000-000000000046");
					 iid_IActivationPropertiesIn.encode(ndr,ndr.Buffer);
					 UUID clsid_IActivationPropertiesIn = new UUID("00000338-0000-0000-c000-000000000046");
					 clsid_IActivationPropertiesIn.encode(ndr,ndr.Buffer);

				 }
				 Catch (NdrException e) {
					 Console.WriteLine(e.ToString());
					 Console.Write(e.StackTrace);
				 }

				 int countEntirePayload = ndr.Buffer.Index; //Entire length of Payload for Custom Marshalling
				 ndr.writeUnsignedLong(0); //extension

				 int writeCountEntirePayloadLength_Here = ndr.Buffer.Index;
				 ndr.writeUnsignedLong(0); //write here (reserved from objref_custom)


				//Activation Properties Blob 
				 int writeActivationPayload = ndr.Buffer.Index;
				 ndr.writeUnsignedLong(0); //payload to be written here

				 ndr.writeUnsignedLong(0); //reserved

				 int countActivationPayload = ndr.Buffer.Index; //Only Activation Payload


				JIStruct tempStruct = CustomHeader;
				int lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
				tempStruct = CustomHeader;
				AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

				tempStruct = outerInstance.SpecialPropertyData;
				lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
				tempStruct = outerInstance.SpecialPropertyData;
				AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

				tempStruct = outerInstance.InstantiationInfoData;
				lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
				tempStruct = outerInstance.InstantiationInfoData;
				AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

				tempStruct = outerInstance.SecurityInfoData;
				lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
				tempStruct = outerInstance.SecurityInfoData;
				AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

				tempStruct = outerInstance.ServerLocationInfo;
				lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
				tempStruct = outerInstance.ServerLocationInfo;
				AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

				tempStruct = outerInstance.ScmRequestInfoData;
				lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
				tempStruct = outerInstance.ScmRequestInfoData;
				AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

				//now update the length in Common header struct.
				WriteEncodingLength(countActivationPayload, countActivationPayload + 16, ndr); // Len for Activation Properties Blob

				 WriteEncodingLength(countActivationPayload, writeActivationPayload, ndr); // Len for Activation Properties Blob
				 WriteEncodingLength(countEntirePayload, writeCountEntirePayloadLength_Here, ndr); // Len for Activation Properties Blob
				 WriteEncodingLength(countFromIndex, index, ndr); // Len 1 for the Custom Object Ref
				 WriteEncodingLength(countFromIndex, index2, ndr); //Len 2 for the Custom Object Ref

			}

			public virtual void WriteEncodingLength(int countFromIndex, int writeAtIndex, NetworkDataRepresentation ndr) {
				int length = ndr.Buffer.Index - countFromIndex;
				int temp = ndr.Buffer.Index;
				ndr.Buffer.Index = writeAtIndex;
				ndr.writeUnsignedLong(length);
				ndr.Buffer.Index = temp;
			}

			public virtual int GetLength(int fromIndex, NetworkDataRepresentation ndr) {
				return ndr.Buffer.Index - fromIndex;
			}

			public virtual void WriteLength(int lenVal, int writeAtIndex, NetworkDataRepresentation ndr) {
				int temp = ndr.Buffer.Index;
				ndr.Buffer.Index = writeAtIndex;
				ndr.writeUnsignedLong(lenVal);
				ndr.Buffer.Index = temp;
			}

			//Pass the length from outside as to calculate it we need to encode the struct and that mutates the internal data structs
			//will return total length of the structure including common header and padding.
			public virtual int AddCommonTypeHeaderAndEncode(NetworkDataRepresentation ndr, JIStruct @struct, int lengthOfStruct) {
	//			will add the common type header and write on wire

				//common header has to be a multiple of 8 bytes. If not it has to be padded at the end.
				int padding = lengthOfStruct % 8;

				int startI = ndr.Buffer.Index;

				//2.2.6.1 Common Type Header for the Serialization Stream (MS-RPCE)
				 ndr.writeUnsignedSmall(0x01); //version
				 ndr.writeUnsignedSmall(0x10); //endianness
				 ndr.writeUnsignedShort(0x08); //common header length
				 ndr.writeUnsignedLong(0xCCCCCCCC); //Filler

				 //now comes the length of the entire CustomHeader without the Common Type Header and this length and Filler.
				 int writeAtIndex = ndr.Buffer.Index;
				 ndr.writeUnsignedLong(0); //write here

				 ndr.writeUnsignedLong(0); //filler, set to NULL

				 int countFromIndex = ndr.Buffer.Index;

				 int x = 0;
					IList listOfDefferedPointers = new List<object>();
					@struct.Encode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL);
					while (x < listOfDefferedPointers.Count) {
						List<object> newList = new List<object>();
						object referent = ((JIPointer)listOfDefferedPointers[x]).GetReferent();
						if (referent is JIStruct) {
							JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIStruct),referent,newList, JIFlags.FLAG_NULL);
						}
						else {
						if (referent is JIString) {
							JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIString),referent,newList, JIFlags.FLAG_NULL);
						}
						else {
							JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIArray),referent,newList, JIFlags.FLAG_NULL);
						}
						}
						x++; //incrementing index
						listOfDefferedPointers.AddRange(x,newList);
					}

				 if (padding != 0) {
					 padding = 8 - padding;
					 ndr.writeOctetArray(new sbyte[padding], 0, padding);
				 }

				WriteEncodingLength(countFromIndex, writeAtIndex, ndr);

				return ndr.Buffer.Index - startI;
			}

			public virtual JIStruct CustomHeader {
				get {
					JIStruct @struct = GetCustomHeader();
					NetworkDataRepresentation ndr = new NetworkDataRepresentation();
					ndr.Buffer = new NdrBuffer(new sbyte[512], 0);
					int lenOfStruct = outerInstance.GetLengthOfStruct(@struct);
					@struct = GetCustomHeader();
					int len = AddCommonTypeHeaderAndEncode(ndr, @struct, lenOfStruct);
					//now we read the length to put into this struct
					ndr.Buffer.Index = 8;
					//int len = ndr.readUnsignedLong() + 16; //8 for common type header and (4 + 4) for header length and reserved.
					@struct = GetCustomHeader();
					@struct.RemoveMember(1);
					try {
						@struct.AddMember(1, new int?(len)); //will push Reserved to the next place now.
					}
					Catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
    
					return @struct;
				}
			}


			public virtual JIStruct GetCustomHeader() {
				/// <summary>
				/// typedef struct tagCustomHeader {
				/// DWORD totalSize;
				/// DWORD headerSize;
				/// DWORD dwReserved;
				/// DWORD destCtx;
				/// [range(MIN_ACTPROP_LIMIT, MAX_ACTPROP_LIMIT)]
				/// DWORD cIfs;
				/// CLSID classInfoClsid;
				/// [size_is(cIfs)] CLSID* pclsid;
				/// [size_is(cIfs)] DWORD* pSizes;
				/// DWORD* pdwReserved;
				/// } CustomHeader;
				/// </summary>

				JIStruct @struct = new JIStruct();

				try {

					@struct.AddMember(new int?(0)); //Total Activation Blob size

					//Correct length set in getCustomHeader.
					@struct.AddMember(new int?(0)); //Total Custom header size including the common type header (from this common type header to start of the next common type header)

					@struct.AddMember(new int?(0));

					@struct.AddMember(new int?(2));

					//sending 5 cIfs
					@struct.AddMember(new int?(5));

					@struct.AddMember(new UUID(UUID.NIL_UUID));

					@struct.addMember(new JIPointer(new JIArray(new UUID[]{ new UUID("000001b9-0000-0000-c000-000000000046"), new UUID("000001ab-0000-0000-c000-000000000046"), new UUID("000001a6-0000-0000-c000-000000000046"), new UUID("000001a4-0000-0000-c000-000000000046"), new UUID("000001aa-0000-0000-c000-000000000046")
				},true)));

					//now come their sizes including their Common headers.
					NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
					ndr2.Buffer = new NdrBuffer(new sbyte[512], 0);
					JIStruct tempStruct = outerInstance.SpecialPropertyData;
					int lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
					tempStruct = outerInstance.SpecialPropertyData;
					int lenSpecialSystemProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

					ndr2 = new NetworkDataRepresentation();
					ndr2.Buffer = new NdrBuffer(new sbyte[512], 0);
					tempStruct = outerInstance.InstantiationInfoData;
					lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
					tempStruct = outerInstance.InstantiationInfoData;
					int lenInstantiationInfoProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

					ndr2 = new NetworkDataRepresentation();
					ndr2.Buffer = new NdrBuffer(new sbyte[512], 0);
					tempStruct = outerInstance.SecurityInfoData;
					lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
					tempStruct = outerInstance.SecurityInfoData;
					int lenSecurityInfoProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

					ndr2 = new NetworkDataRepresentation();
					ndr2.Buffer = new NdrBuffer(new sbyte[512], 0);
					tempStruct = outerInstance.ServerLocationInfo;
					lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
					tempStruct = outerInstance.ServerLocationInfo;
					int lenServerLocationProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

					ndr2 = new NetworkDataRepresentation();
					ndr2.Buffer = new NdrBuffer(new sbyte[512], 0);
					tempStruct = outerInstance.ScmRequestInfoData;
					lentempStruct = outerInstance.GetLengthOfStruct(tempStruct);
					tempStruct = outerInstance.ScmRequestInfoData;
					int lenScmRequestInfoDataProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);


					@struct.AddMember(new JIPointer(new JIArray(new int?[]{ lenSpecialSystemProp, lenInstantiationInfoProp, lenSecurityInfoProp, lenServerLocationProp, lenScmRequestInfoDataProp }, true)));

					@struct.AddMember(new int?(0)); //reserved

			}
				Catch (JIException e) {
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				} //don't know will correct later.

				return @struct;
		}

			public virtual JIStruct GetInstantiationInfoData() {
				/// <summary>
				/// typedef struct tagInstantiationInfoData {
				/// CLSID classId;
				/// DWORD classCtx;
				/// DWORD actvflags;
				/// long fIsSurrogate;
				/// [range(1, MAX_REQUESTED_INTERFACES)]
				/// DWORD cIID;
				/// DWORD instFlag;
				/// [size_is(cIID)] IID* pIID;
				/// DWORD thisSize;
				/// COMVERSION clientCOMVersion;
				/// } InstantiationInfoData
				/// 
				/// </summary>

				JIStruct @struct = new JIStruct();
				try {

					@struct.AddMember(new UUID(TargetClsid));
					@struct.AddMember(new int?(0x14)); //  CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_INPROC_SERVER16
					@struct.AddMember(new int?(0));
					@struct.AddMember(new int?(0));
					@struct.AddMember(new int?(2)); //IUnknown and IDispatch
					@struct.AddMember(new int?(0));
					JIPointer ptr = new JIPointer(new JIArray(new UUID[]{ new UUID("00000000-0000-0000-c000-000000000046"), new UUID("00020400-0000-0000-c000-000000000046") }, true));
					ptr.Flags = JIFlags.FLAG_REPRESENTATION_ARRAY;
					@struct.AddMember(ptr);

					//size of the current struct , why ? why ???
					@struct.AddMember(new int?(0)); //don't know will replace later on. (remove and add)
					@struct.AddMember(Convert.ToInt16((short)JISystem.COMVersion.MajorVersion));
					@struct.AddMember(Convert.ToInt16((short)JISystem.COMVersion.MinorVersion));

				}
				Catch (JIException e) {
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}


				return @struct;
			}

			public virtual JIStruct SecurityInfoData {
				get {
					/// <summary>
					/// typedef struct tagSecurityInfoData {
					/// DWORD dwAuthnFlags;
					/// COSERVERINFO* pServerInfo;
					/// DWORD* pdwReserved;
					/// } SecurityInfoData
					/// </summary>
    
					JIStruct @struct = new JIStruct();
					try {
						@struct.AddMember(new int?(0));
    
						/// <summary>
						/// typedef struct _COSERVERINFO {
						/// DWORD dwReserved1;
						/// [string] wchar_t* pwszName;
						/// DWORD* pdwReserved;
						/// DWORD dwReserved2;
						/// } COSERVERINFO;
						/// </summary>
						JIStruct coserver = new JIStruct();
						coserver.AddMember(new int?(0));
						coserver.AddMember(new JIPointer(new JIString(TargetServer, JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
						coserver.AddMember(new int?(0));
						coserver.AddMember(new int?(0));
						@struct.AddMember(new JIPointer(coserver));
						@struct.AddMember(new int?(0));
					}
					Catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
    
					return @struct;
				}
			}

			public virtual JIStruct ServerLocationInfo {
				get {
					/// <summary>
					/// typedef struct tagLocationInfoData {
					/// [string] wchar_t* machineName;
					/// DWORD processId;
					/// DWORD apartmentId;
					/// DWORD contextId;
					/// } LocationInfoData;
					/// </summary>
					JIStruct @struct = new JIStruct();
					try {
    
						@struct.AddMember(new int?(0));
						@struct.AddMember(new int?(0));
						@struct.AddMember(new int?(0));
						@struct.AddMember(new int?(0));
    
					}
					Catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
    
					return @struct;
				}
			}

			public virtual JIStruct ScmRequestInfoData {
				get {
					/// <summary>
					/// typedef struct tagScmRequestInfoData {
					/// DWORD* pdwReserved;
					/// customREMOTE_REQUEST_SCM_INFO* remoteRequest;
					/// } ScmRequestInfoData
					/// </summary>
					JIStruct @struct = new JIStruct();
					try {
    
						@struct.AddMember(new int?(0));
    
						/// <summary>
						/// typedef struct _customREMOTE_REQUEST_SCM_INFO {
						/// DWORD ClientImpLevel;
						/// [range(0, MAX_REQUESTED_PROTSEQS)]
						/// unsigned short cRequestedProtseqs;
						/// [size_is(cRequestedProtseqs)] unsigned short* pRequestedProtseqs;
						/// } customREMOTE_REQUEST_SCM_INFO;
						/// </summary>
						JIStruct _customRemoteRequestSCMInfo = new JIStruct();
						_customRemoteRequestSCMInfo.AddMember(new int?(2));
						_customRemoteRequestSCMInfo.AddMember(new short?((short)1));
						_customRemoteRequestSCMInfo.AddMember(new JIPointer(new JIArray(new short?[]{ (short)0x07 }, true)));
						@struct.AddMember(new JIPointer(_customRemoteRequestSCMInfo));
    
					}
					Catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
    
					return @struct;
				}
			}


			public virtual JIStruct InstantiationInfoData {
				get {
					JIStruct @struct = outerInstance.GetInstantiationInfoData();
					int lenOfStruct = outerInstance.GetLengthOfStruct(@struct);
					@struct = outerInstance.GetInstantiationInfoData();
					NetworkDataRepresentation ndr = new NetworkDataRepresentation();
					ndr.Buffer = new NdrBuffer(new sbyte[512], 0);
					int len = AddCommonTypeHeaderAndEncode(ndr, @struct, lenOfStruct);
					//now we read the length to put into this struct
		//			ndr.getBuffer().setIndex(8);
		//			int len = ndr.readUnsignedLong();
					@struct = outerInstance.GetInstantiationInfoData();
					@struct.RemoveMember(7);
					try {
						@struct.AddMember(7, new int?(len)); //will push COMVERSION to last place now.
					}
					Catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
    
					return @struct;
				}
			}

			public virtual JIStruct SpecialPropertyData {
				get {
					/// <summary>
					/// typedef struct tagSpecialPropertiesData {
					/// unsigned long dwSessionId;
					/// long fRemoteThisSessionId;
					/// long fClientImpersonating;
					/// long fPartitionIDPresent;
					/// DWORD dwDefaultAuthnLvl;
					/// GUID guidPartition;
					/// DWORD dwPRTFlags;
					/// DWORD dwOrigClsctx;
					/// DWORD dwFlags;
					/// DWORD Reserved1;
					/// unsigned __int64 Reserved2;
					/// DWORD Reserved3[5];
					/// } SpecialPropertiesData;
					/// </summary>
    
					JIStruct @struct = new JIStruct();
					try {
						@struct.AddMember(new int?(unchecked((int)0xFFFFFFFF)));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000001)); //auth level none ? Why ?
						@struct.AddMember(new UUID(UUID.NIL_UUID));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x14));
						@struct.AddMember(new int?(0x2));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new long?(0x0000000000000000));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000000));
						@struct.AddMember(new int?(0x00000000));
					}
					Catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
    
					return @struct;
				}
			}


			//discard this struct after use and create a new one
			public virtual int GetLengthOfStruct(JIStruct @struct) {
				NetworkDataRepresentation ndr = new NetworkDataRepresentation();
				ndr.Buffer = new NdrBuffer(new sbyte[512], 0);
				int startI = ndr.Buffer.Index;

				int x = 0;
				IList listOfDefferedPointers = new List<object>();
				@struct.Encode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL);
				while (x < listOfDefferedPointers.Count) {
					List<object> newList = new List<object>();
					object referent = ((JIPointer)listOfDefferedPointers[x]).GetReferent();
					if (referent is JIStruct) {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIStruct),referent,newList, JIFlags.FLAG_NULL);
					}
					else {
					if (referent is JIString) {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIString),referent,newList, JIFlags.FLAG_NULL);
					}
					else {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIArray),referent,newList, JIFlags.FLAG_NULL);
					}
					}
					x++; //incrementing index
					listOfDefferedPointers.AddRange(x,newList);
				}

				return ndr.Buffer.Index - startI;
			}


			//Skip common header and return total length of the object buffer inside. We will need to skip the
			//padded bytes as well once we have analyzed the complete objectBuffer.
			public virtual int SkipCommonHeader(NetworkDataRepresentation ndr) {
				 ndr.readUnsignedSmall(); //version
				 ndr.readUnsignedSmall(); //endianness
				 ndr.readUnsignedShort(); //common header length
				 ndr.readUnsignedLong(); //Filler
				 int retlength = ndr.readUnsignedLong();
				 ndr.readUnsignedLong(); //reserved
				 return retlength;
			}

			public virtual void SkipBytes(int objectBufferLength, int startIndex, NetworkDataRepresentation ndr) {
				int bytesRead = ndr.Buffer.Index - startIndex;
				if (objectBufferLength > bytesRead) {
					ndr.readOctetArray(new sbyte[objectBufferLength - bytesRead], 0, objectBufferLength - bytesRead);
				}
			}



			public virtual void Read(NetworkDataRepresentation ndr) {

				JIOrpcThat.Decode(ndr);

				//MInterfacePointer** ppActProperties

				List<object> listOfDefferedPointers = new List<object>();
				JIInterfacePointer ppActProperties = (JIInterfacePointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(JIInterfacePointer),listOfDefferedPointers,JIFlags.FLAG_NULL,new Hashtable());

				//Class not registered or any other exception probably.
				if (ppActProperties == null) {
					int hResult = ndr.readUnsignedLong();
					throw new JIRuntimeException(hResult);
				}

				// we should now be standing at the Activation Properties Blob right now. 	
				int totalLength = ndr.readUnsignedLong();
				ndr.readUnsignedLong(); //reserved

				//Custom Header begins
				//lets check what all has been returned back to us. We are only interested in two Properties (ScmReply and PropsOut)
				//Must contain the following properties
	//			ScmReplyInfoData 2.2.22.2.8 Required
	//			PropsOutInfo 2.2.22.2.9 Required

				/// <summary>
				/// typedef struct tagCustomHeader {
				/// DWORD totalSize;
				/// DWORD headerSize;
				/// DWORD dwReserved;
				/// DWORD destCtx;
				/// [range(MIN_ACTPROP_LIMIT, MAX_ACTPROP_LIMIT)]
				/// DWORD cIfs;
				/// CLSID classInfoClsid;
				/// [size_is(cIfs)] CLSID* pclsid;
				/// [size_is(cIfs)] DWORD* pSizes;
				/// DWORD* pdwReserved;
				/// } CustomHeader;
				/// </summary>

				int objectBufferLength = outerInstance.SkipCommonHeader(ndr);
				int startIndex = ndr.Buffer.Index;
				JIStruct @struct = new JIStruct();
				try {
					@struct.AddMember(typeof(int?));
					@struct.AddMember(typeof(int?));
					@struct.AddMember(typeof(int?));
					@struct.AddMember(typeof(int?));
					@struct.AddMember(typeof(int?)); //cIfs
					@struct.AddMember(typeof(UUID));
					@struct.AddMember(new JIPointer(new JIArray(typeof(UUID),null,1,true)));
					@struct.AddMember(new JIPointer(new JIArray(typeof(int?),null,1,true)));
					@struct.AddMember(typeof(int?));
				}
				Catch (JIException e) {
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				@struct = outerInstance.DecodeStruct(@struct, ndr);

				outerInstance.SkipBytes(objectBufferLength, startIndex, ndr);

				//now we need to check for the indexes of our relevant Properties

				UUID[] clsidProps = (UUID[])((JIArray)((JIPointer)@struct.GetMember(6)).GetReferent()).ArrayInstance;

				int?[] clsidPropsLengths = (int?[])((JIArray)((JIPointer)@struct.GetMember(7)).GetReferent()).ArrayInstance;

				//using the clsidPropsLengths we can skip the NDR buffer of the properties not needed.
				IList<string> requiredProps = new List<string>();
				requiredProps.Add("000001b6-0000-0000-c000-000000000046".ToUpper());
				requiredProps.Add("00000339-0000-0000-c000-000000000046".ToUpper());
				//we will go sequentially so if a property is not found we skip that many bytes ahead
				for (int i = 0; i < clsidProps.Length; i++) {
					if (requiredProps.Contains(clsidProps[i].ToString().ToUpper())) {
						//its present so analyse
						objectBufferLength = outerInstance.SkipCommonHeader(ndr);
						startIndex = ndr.Buffer.Index;
						@struct = new JIStruct();

						if (clsidProps[i].ToString().Equals("000001b6-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
							try
							{ //ScmReplyInfo

								/// <summary>
								/// typedef struct tagScmReplyInfoData {
								/// DWORD* pdwReserved;
								/// customREMOTE_REPLY_SCM_INFO* remoteReply;
								/// } ScmReplyInfoData;
								/// </summary>

								@struct.AddMember(typeof(int?));

								JIStruct remoteReplyStruct = new JIStruct();

								/// <summary>
								/// typedef struct _customREMOTE_REPLY_SCM_INFO {
								/// OXID Oxid;
								/// DUALSTRINGARRAY* pdsaOxidBindings;
								/// IPID ipidRemUnknown;
								/// DWORD authnHint;
								/// COMVERSION serverVersion;
								/// } customREMOTE_REPLY_SCM_INFO;
								/// </summary>
								//we need to take out oxid only way to do it is byte by byte
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								remoteReplyStruct.AddMember(typeof(sbyte?));
								//8 bytes (4 + 4 LE) = OXID
								remoteReplyStruct.AddMember(new JIPointer(typeof(JIDualStringArray),false));
								remoteReplyStruct.AddMember(typeof(UUID));
								remoteReplyStruct.AddMember(typeof(int?));
								//COM Version can be taken as two shorts.
								//if this COM version is less than 5.6 than we throw an exception
								remoteReplyStruct.AddMember(typeof(short?));
								remoteReplyStruct.AddMember(typeof(short?));

								@struct.AddMember(new JIPointer(remoteReplyStruct));

							}
							Catch (JIException e) {
								Console.WriteLine(e.ToString());
								Console.Write(e.StackTrace);
							}

							@struct = outerInstance.DecodeStruct(@struct, ndr);
							@struct = (JIStruct)(((JIPointer)@struct.GetMember(1)).GetReferent());

							//now we need to get the IPID and Dual String Array.
							NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
							NdrBuffer buffer = new NdrBuffer(new sbyte[8], 0);
							buffer.buf[0] = unchecked((sbyte)(((sbyte?)@struct.GetMember(0)) & 0xFF));
							buffer.buf[1] = unchecked((sbyte)(((sbyte?)@struct.GetMember(1)) & 0xFF));
							buffer.buf[2] = unchecked((sbyte)(((sbyte?)@struct.GetMember(2)) & 0xFF));
							buffer.buf[3] = unchecked((sbyte)(((sbyte?)@struct.GetMember(3)) & 0xFF));
							buffer.buf[4] = unchecked((sbyte)(((sbyte?)@struct.GetMember(4)) & 0xFF));
							buffer.buf[5] = unchecked((sbyte)(((sbyte?)@struct.GetMember(5)) & 0xFF));
							buffer.buf[6] = unchecked((sbyte)(((sbyte?)@struct.GetMember(6)) & 0xFF));
							buffer.buf[7] = unchecked((sbyte)(((sbyte?)@struct.GetMember(7)) & 0xFF));
							ndr2.Buffer = buffer;

							Oxid = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr2,8);
							DualStringArrayForOxid = (JIDualStringArray)(((JIPointer)@struct.GetMember(8)).GetReferent());
							Ipid = ((UUID)@struct.GetMember(9)).ToString();
							AuthenticationHint = (int?)@struct.GetMember(10);
							ComVersion = new JIComVersion((short?)@struct.GetMember(11), (short?)@struct.GetMember(12));
						}
						else {
						if (clsidProps[i].ToString().Equals("00000339-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
							try
							{ //PropsOutInfo

								/// <summary>
								/// typedef struct tagPropsOutInfo {
								/// [range(1, MAX_REQUESTED_INTERFACES)]
								/// DWORD cIfs;
								/// [size_is(cIfs)] IID* piid;
								/// [size_is(cIfs)] HRESULT* phresults;
								/// [size_is(cIfs)] MInterfacePointer** ppIntfData;
								/// } PropsOutInfo;
								/// </summary>

								@struct.AddMember(typeof(int?));
								@struct.AddMember(new JIPointer(new JIArray(typeof(UUID),null,1,true)));
								@struct.AddMember(new JIPointer(new JIArray(typeof(int?),null,1,true))); //Hresult,
								//0 is good anything else is bad and corresponding MInterfacePointer will not exist. 
								@struct.AddMember(new JIPointer(new JIArray(typeof(JIInterfacePointer),null,1,true)));

							}
							Catch (JIException e) {
								Console.WriteLine(e.ToString());
								Console.Write(e.StackTrace);
							}

							@struct = outerInstance.DecodeStruct(@struct, ndr);

							JIInterfacePointer[] marshalledIp = (JIInterfacePointer[])((JIArray)(((JIPointer)@struct.GetMember(3)).GetReferent())).ArrayInstance;

							UUID[] iids = (UUID[])((JIArray)(((JIPointer)@struct.GetMember(1)).GetReferent())).ArrayInstance;

							//now get the hresults and only those IIDs are supported which have 0x00000000
							//in our case IUnknown will always be supported (naturally) where as IDispatch may or may not be.
							int?[] hresults = (int?[])((JIArray)(((JIPointer)@struct.GetMember(2)).GetReferent())).ArrayInstance;
							for (int j = 0; j < hresults.Length; j++) {
								if (hresults[j] == 0x00000000) {
									//pointer exists
									//if it is Disp IID then set dual stuff else it has to be IUnknown, save it.
									if (iids[j].ToString().Equals("00000000-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
										//IUnknown
										MInterfacePointer = marshalledIp[j];
									}
									else if (iids[j].ToString().Equals("", StringComparison.CurrentCultureIgnoreCase)) {
										//dual is supported since the IDispatch was obtained
										IsDual = true;
										//eat this keeping only the IPID for cleanup , let the user perform another queryInterface for this.
										JIInterfacePointer ptr = marshalledIp[j];
										DispIpid = ptr.IPID;
										DispOid = ptr.OID;
										DispRefs = ((JIStdObjRef)ptr.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
									}
								}
							}

						}
						}

						outerInstance.SkipBytes(objectBufferLength, startIndex, ndr);


					}
					else {
						sbyte[] skip = new sbyte[clsidPropsLengths[i]];
						ndr.readOctetArray(skip, 0, skip.Length);
					}
				}

				IsActivationSuccessful = true;
			}

			public virtual JIStruct DecodeStruct(JIStruct @struct, NetworkDataRepresentation ndr) {
				IList listOfDefferedPointers = new List<object>();
				IDictionary additionalData = new Hashtable();
				@struct = @struct.Decode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL, additionalData);
				int x = 0;
				while (x < listOfDefferedPointers.Count) {
					List<object> newList = new List<object>();
					JIPointer replacement = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr, ((JIPointer)listOfDefferedPointers[x]), newList, JIFlags.FLAG_NULL, additionalData);
					((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement);
					x++; //incrementing index
					listOfDefferedPointers.AddRange(x,newList);
				}

				return @struct;
			}

			public virtual bool ActivationSuccessful {
				get {
					return IsActivationSuccessful;
				}
			}

			public virtual JIDualStringArray DualStringArrayForOxid {
				get {
					return DualStringArrayForOxid;
				}
			}

			public virtual JIInterfacePointer MInterfacePointer {
				get {
					return MInterfacePointer;
				}
			}

			public virtual string IPID {
				get {
					return Ipid;
				}
			}

			public virtual bool Dual {
				get {
					return IsDual;
				}
			}

			public virtual string DispIpid {
				get {
					return DispIpid;
				}
				set {
					this.DispIpid = value;
				}
			}

			public virtual int DispRefs {
				get {
					return DispRefs;
				}
			}


	}


}

}