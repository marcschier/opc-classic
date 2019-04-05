// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using rpc.core;
    using org.jinterop.dcom.common;
    using org.jinterop.winreg;
    using System;
    using System.Collections;

    /// <summary>
    /// IRemoteSCMActivator implementation. 
    /// </summary>
    internal sealed class JIRemoteSCMActivator {

        internal class RemoteCreateInstance : NdrOp, JIIServerActivation {
            private readonly JIRemoteSCMActivator outerInstance;


            //		HRESULT RemoteCreateInstance(
            //				[in] handle_t hRpc,
            //				[in] ORPCTHIS* orpcthis,
            //				[out] ORPCTHAT* orpcthat,
            //				[in, unique] MInterfacePointer* pUnkOuter,
            //				[in, unique] MInterfacePointer* pActProperties,
            //				[out] MInterfacePointer** ppActProperties
            //				);

            internal readonly string targetClsid;
            internal readonly string targetServer;
            internal sbyte[] oxid;
            internal JIDualStringArray dualStringArrayForOxid;
            internal string ipid;
            internal int authenticationHint = -1;
            internal JIComVersion comVersion;
            internal JIInterfacePointer mInterfacePointer;
            internal bool isDual;
            internal string dispIpid;
            internal int dispRefs = 5;
            internal sbyte[] dispOid;
            internal bool isActivationSuccessful;

            /// <summary>
            /// Create 
            /// </summary>
            /// <param name="outerInstance"></param>
            /// <param name="targetServer"></param>
            /// <param name="clsid"></param>
            public RemoteCreateInstance(JIRemoteSCMActivator outerInstance, string targetServer, string clsid) {
                this.outerInstance = outerInstance;
                targetClsid = clsid;
                this.targetServer = targetServer;
            }

            /// <inheritdoc/>
            public override int Opnum => 4;

            /// <inheritdoc/>
            public override void Write(NdrCodec ndr) {
                var orpcThis = new JIOrpcThis();
                orpcThis.encode(ndr);

                ndr.WriteUnsignedLong(0); // pUnkOuter, setting it to NULL.
                ndr.WriteUnsignedLong(0x00020000);

                var index = ndr.Buffer.Index; //recording where we have to write length
                ndr.WriteUnsignedLong(0); //Len 1

                //alignment may kick in
                var index2 = ndr.Buffer.Index; //recording where we have to write length
                ndr.WriteUnsignedLong(0); //Len 2

                var countFromIndex = ndr.Buffer.Index; //recording from where we have to write
                ndr.WriteUnsignedLong(0x574f454d); // Signature MEOW
                ndr.WriteUnsignedLong(4); // OBJREF_CUSTOM

                //now we will write the Custom Interface pointer to Activation SharpCifs.Util.Sharpen.Properties.
                try {
                    //IID_IActivationPropertiesIn
                    var iid_IActivationPropertiesIn = new UUID("000001a2-0000-0000-c000-000000000046");
                    iid_IActivationPropertiesIn.Encode(ndr, ndr.Buffer);
                    var clsid_IActivationPropertiesIn = new UUID("00000338-0000-0000-c000-000000000046");
                    clsid_IActivationPropertiesIn.Encode(ndr, ndr.Buffer);

                }
                catch (NdrException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                var countEntirePayload = ndr.Buffer.Index; //Entire length of Payload for Custom Marshalling
                ndr.WriteUnsignedLong(0); //extension
                var writeCountEntirePayloadLength_Here = ndr.Buffer.Index;
                ndr.WriteUnsignedLong(0); //write here (reserved from objref_custom)

                //Activation SharpCifs.Util.Sharpen.Properties Blob 
                var writeActivationPayload = ndr.Buffer.Index;
                ndr.WriteUnsignedLong(0); //payload to be written here

                ndr.WriteUnsignedLong(0); //reserved
                var countActivationPayload = ndr.Buffer.Index; //Only Activation Payload

                var tempStruct = CustomHeader;
                int lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                tempStruct = CustomHeader;
                addCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = outerInstance.SpecialPropertyData;
                lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                tempStruct = outerInstance.SpecialPropertyData;
                addCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = outerInstance.InstantiationInfoData;
                lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                tempStruct = outerInstance.InstantiationInfoData;
                addCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = outerInstance.SecurityInfoData;
                lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                tempStruct = outerInstance.SecurityInfoData;
                addCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = outerInstance.ServerLocationInfo;
                lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                tempStruct = outerInstance.ServerLocationInfo;
                addCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = outerInstance.ScmRequestInfoData;
                lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                tempStruct = outerInstance.ScmRequestInfoData;
                addCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                //now update the length in Common header struct.
                writeEncodingLength(countActivationPayload, countActivationPayload + 16, ndr); // Len for Activation SharpCifs.Util.Sharpen.Properties Blob

                writeEncodingLength(countActivationPayload, writeActivationPayload, ndr); // Len for Activation SharpCifs.Util.Sharpen.Properties Blob
                writeEncodingLength(countEntirePayload, writeCountEntirePayloadLength_Here, ndr); // Len for Activation SharpCifs.Util.Sharpen.Properties Blob
                writeEncodingLength(countFromIndex, index, ndr); // Len 1 for the Custom Object Ref
                writeEncodingLength(countFromIndex, index2, ndr); //Len 2 for the Custom Object Ref
            }

            internal virtual void writeEncodingLength(int countFromIndex, int writeAtIndex, NdrCodec ndr) {
                var length = ndr.Buffer.Index - countFromIndex;
                var temp = ndr.Buffer.Index;
                ndr.Buffer.Index = writeAtIndex;
                ndr.WriteUnsignedLong(length);
                ndr.Buffer.Index = temp;
            }

            internal virtual int getLength(int fromIndex, NdrCodec ndr) {
                return ndr.Buffer.Index - fromIndex;
            }

            internal virtual void writeLength(int lenVal, int writeAtIndex, NdrCodec ndr) {
                var temp = ndr.Buffer.Index;
                ndr.Buffer.Index = writeAtIndex;
                ndr.WriteUnsignedLong(lenVal);
                ndr.Buffer.Index = temp;
            }

            //Pass the length from outside as to calculate it we need to encode the struct and that mutates the internal data structs
            //will return total length of the structure including common header and padding.
            internal virtual int addCommonTypeHeaderAndEncode(NdrCodec ndr, JIStruct @struct, int lengthOfStruct) {
                //			will add the common type header and write on wire

                //common header has to be a multiple of 8 bytes. If not it has to be padded at the end.
                var padding = lengthOfStruct % 8;

                var startI = ndr.Buffer.Index;

                //2.2.6.1 Common Type Header for the Serialization Stream (MS-RPCE)
                ndr.WriteUnsignedSmall(0x01); //version
                ndr.WriteUnsignedSmall(0x10); //endianness
                ndr.WriteUnsignedShort(0x08); //common header length
                ndr.writeUnsignedLong(0xCCCCCCCC); //Filler

                //now comes the length of the entire CustomHeader without the Common Type Header and this length and Filler.
                var writeAtIndex = ndr.Buffer.Index;
                ndr.WriteUnsignedLong(0); //write here

                ndr.WriteUnsignedLong(0); //filler, set to NULL

                var countFromIndex = ndr.Buffer.Index;

                var x = 0;
                IList listOfDefferedPointers = new ArrayList();
                @struct.encode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL);
                while (x < listOfDefferedPointers.Count) {
                    var newList = new ArrayList();
                    var referent = ((JIPointer)listOfDefferedPointers[x]).getReferent();
                    if (referent is JIStruct) {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIStruct), referent, newList, JIFlags.FLAG_NULL);
                    }
                    else {
                        if (referent is JIString) {
                            JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIString), referent, newList, JIFlags.FLAG_NULL);
                        }
                        else {
                            JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIArray), referent, newList, JIFlags.FLAG_NULL);
                        }
                    }
                    x++; //incrementing index
                    listOfDefferedPointers.AddRange(x, newList);
                }

                if (padding != 0) {
                    padding = 8 - padding;
                    ndr.writeOctetArray(new sbyte[padding], 0, padding);
                }

                writeEncodingLength(countFromIndex, writeAtIndex, ndr);

                return ndr.Buffer.Index - startI;
            }

            internal virtual JIStruct CustomHeader {
                get {
                    var @struct = _getCustomHeader();
                    var ndr = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    int lenOfStruct = outerInstance.getLengthOfStruct(@struct);
                    @struct = _getCustomHeader();
                    var len = addCommonTypeHeaderAndEncode(ndr, @struct, lenOfStruct);
                    //now we read the length to put into this struct
                    ndr.Buffer.Index = 8;
                    //int len = ndr.readUnsignedLong() + 16; //8 for common type header and (4 + 4) for header length and reserved.
                    @struct = _getCustomHeader();
                    @struct.removeMember(1);
                    try {
                        @struct.addMember(1, len); //will push Reserved to the next place now.
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }


            internal virtual JIStruct _getCustomHeader() {
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

                var @struct = new JIStruct();

                try {

                    @struct.addMember(0); //Total Activation Blob size

                    //Correct length set in getCustomHeader.
                    @struct.addMember(0); //Total Custom header size including the common type header (from this common type header to start of the next common type header)

                    @struct.addMember(0);

                    @struct.addMember(2);

                    //sending 5 cIfs
                    @struct.addMember(5);

                    @struct.addMember(new UUID(UUID.NIL_UUID));

                    @struct.addMember(new JIPointer(new JIArray(new UUID[]{ new UUID("000001b9-0000-0000-c000-000000000046"), new UUID("000001ab-0000-0000-c000-000000000046"), new UUID("000001a6-0000-0000-c000-000000000046"), new UUID("000001a4-0000-0000-c000-000000000046"), new UUID("000001aa-0000-0000-c000-000000000046")
                }, true)));

                    //now come their sizes including their Common headers.
                    var ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    JIStruct tempStruct = outerInstance.SpecialPropertyData;
                    int lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                    tempStruct = outerInstance.SpecialPropertyData;
                    var lenSpecialSystemProp = addCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    tempStruct = outerInstance.InstantiationInfoData;
                    lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                    tempStruct = outerInstance.InstantiationInfoData;
                    var lenInstantiationInfoProp = addCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    tempStruct = outerInstance.SecurityInfoData;
                    lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                    tempStruct = outerInstance.SecurityInfoData;
                    var lenSecurityInfoProp = addCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    tempStruct = outerInstance.ServerLocationInfo;
                    lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                    tempStruct = outerInstance.ServerLocationInfo;
                    var lenServerLocationProp = addCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    tempStruct = outerInstance.ScmRequestInfoData;
                    lentempStruct = outerInstance.getLengthOfStruct(tempStruct);
                    tempStruct = outerInstance.ScmRequestInfoData;
                    var lenScmRequestInfoDataProp = addCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);


                    @struct.addMember(new JIPointer(new JIArray(new int?[] { lenSpecialSystemProp, lenInstantiationInfoProp, lenSecurityInfoProp, lenServerLocationProp, lenScmRequestInfoDataProp }, true)));

                    @struct.addMember(0); //reserved

                }
                catch (JIException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                } //don't know will correct later.

                return @struct;
            }

            internal virtual JIStruct _getInstantiationInfoData() {
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

                var @struct = new JIStruct();
                try {

                    @struct.addMember(new UUID(targetClsid));
                    @struct.addMember(0x14); //  CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_INPROC_SERVER16
                    @struct.addMember(0);
                    @struct.addMember(0);
                    @struct.addMember(2); //IUnknown and IDispatch
                    @struct.addMember(0);
                    var ptr = new JIPointer(new JIArray(new UUID[] { new UUID("00000000-0000-0000-c000-000000000046"), new UUID("00020400-0000-0000-c000-000000000046") }, true)) {
                        Flags = JIFlags.FLAG_REPRESENTATION_ARRAY
                    };
                    @struct.addMember(ptr);

                    //size of the current struct , why ? why ???
                    @struct.addMember(0); //don't know will replace later on. (remove and add)
                    @struct.addMember(Convert.ToInt16((short)JISystem.COMVersion.MajorVersion));
                    @struct.addMember(Convert.ToInt16((short)JISystem.COMVersion.MinorVersion));

                }
                catch (JIException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }


                return @struct;
            }

            internal virtual JIStruct SecurityInfoData {
                get {
                    /// <summary>
                    /// typedef struct tagSecurityInfoData {
                    /// DWORD dwAuthnFlags;
                    /// COSERVERINFO* pServerInfo;
                    /// DWORD* pdwReserved;
                    /// } SecurityInfoData
                    /// </summary>

                    var @struct = new JIStruct();
                    try {
                        @struct.addMember(0);

                        /// <summary>
                        /// typedef struct _COSERVERINFO {
                        /// DWORD dwReserved1;
                        /// [string] wchar_t* pwszName;
                        /// DWORD* pdwReserved;
                        /// DWORD dwReserved2;
                        /// } COSERVERINFO;
                        /// </summary>
                        var coserver = new JIStruct();
                        coserver.addMember(0);
                        coserver.addMember(new JIPointer(new JIString(targetServer, JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                        coserver.addMember(0);
                        coserver.addMember(0);
                        @struct.addMember(new JIPointer(coserver));
                        @struct.addMember(0);
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

            internal virtual JIStruct ServerLocationInfo {
                get {
                    /// <summary>
                    /// typedef struct tagLocationInfoData {
                    /// [string] wchar_t* machineName;
                    /// DWORD processId;
                    /// DWORD apartmentId;
                    /// DWORD contextId;
                    /// } LocationInfoData;
                    /// </summary>
                    var @struct = new JIStruct();
                    try {

                        @struct.addMember(0);
                        @struct.addMember(0);
                        @struct.addMember(0);
                        @struct.addMember(0);

                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

            internal virtual JIStruct ScmRequestInfoData {
                get {
                    /// <summary>
                    /// typedef struct tagScmRequestInfoData {
                    /// DWORD* pdwReserved;
                    /// customREMOTE_REQUEST_SCM_INFO* remoteRequest;
                    /// } ScmRequestInfoData
                    /// </summary>
                    var @struct = new JIStruct();
                    try {

                        @struct.addMember(0);

                        /// <summary>
                        /// typedef struct _customREMOTE_REQUEST_SCM_INFO {
                        /// DWORD ClientImpLevel;
                        /// [range(0, MAX_REQUESTED_PROTSEQS)]
                        /// unsigned short cRequestedProtseqs;
                        /// [size_is(cRequestedProtseqs)] unsigned short* pRequestedProtseqs;
                        /// } customREMOTE_REQUEST_SCM_INFO;
                        /// </summary>
                        var _customRemoteRequestSCMInfo = new JIStruct();
                        _customRemoteRequestSCMInfo.addMember(2);
                        _customRemoteRequestSCMInfo.addMember((short)1);
                        _customRemoteRequestSCMInfo.addMember(new JIPointer(new JIArray(new short?[] { (short)0x07 }, true)));
                        @struct.addMember(new JIPointer(_customRemoteRequestSCMInfo));

                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }


            internal virtual JIStruct InstantiationInfoData {
                get {
                    JIStruct @struct = outerInstance._getInstantiationInfoData();
                    int lenOfStruct = outerInstance.getLengthOfStruct(@struct);
                    @struct = outerInstance._getInstantiationInfoData();
                    var ndr = new NdrCodec {
                        Buffer = new NdrBuffer(new sbyte[512], 0)
                    };
                    var len = addCommonTypeHeaderAndEncode(ndr, @struct, lenOfStruct);
                    //now we read the length to put into this struct
                    //			ndr.getBuffer().setIndex(8);
                    //			int len = ndr.readUnsignedLong();
                    @struct = outerInstance._getInstantiationInfoData();
                    @struct.removeMember(7);
                    try {
                        @struct.addMember(7, len); //will push COMVERSION to last place now.
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

            internal virtual JIStruct SpecialPropertyData {
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

                    var @struct = new JIStruct();
                    try {
                        @struct.addMember(unchecked((int)0xFFFFFFFF));
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000001); //auth level none ? Why ?
                        @struct.addMember(new UUID(UUID.NIL_UUID));
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x14);
                        @struct.addMember(0x2);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x0000000000000000);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000000);
                        @struct.addMember(0x00000000);
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }


            //discard this struct after use and create a new one
            internal virtual int getLengthOfStruct(JIStruct @struct) {
                var ndr = new NdrCodec {
                    Buffer = new NdrBuffer(new sbyte[512], 0)
                };
                var startI = ndr.Buffer.Index;

                var x = 0;
                IList listOfDefferedPointers = new ArrayList();
                @struct.encode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL);
                while (x < listOfDefferedPointers.Count) {
                    var newList = new ArrayList();
                    var referent = ((JIPointer)listOfDefferedPointers[x]).getReferent();
                    if (referent is JIStruct) {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIStruct), referent, newList, JIFlags.FLAG_NULL);
                    }
                    else {
                        if (referent is JIString) {
                            JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIString), referent, newList, JIFlags.FLAG_NULL);
                        }
                        else {
                            JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIArray), referent, newList, JIFlags.FLAG_NULL);
                        }
                    }
                    x++; //incrementing index
                    listOfDefferedPointers.AddRange(x, newList);
                }

                return ndr.Buffer.Index - startI;
            }


            //Skip common header and return total length of the object buffer inside. We will need to skip the
            //padded bytes as well once we have analyzed the complete objectBuffer.
            internal virtual int skipCommonHeader(NdrCodec ndr) {
                ndr.ReadUnsignedSmall(); //version
                ndr.ReadUnsignedSmall(); //endianness
                ndr.ReadUnsignedShort(); //common header length
                ndr.ReadUnsignedLong(); //Filler
                var retlength = ndr.ReadUnsignedLong();
                ndr.ReadUnsignedLong(); //reserved
                return retlength;
            }

            internal virtual void skipBytes(int objectBufferLength, int startIndex, NdrCodec ndr) {
                var bytesRead = ndr.Buffer.Index - startIndex;
                if (objectBufferLength > bytesRead) {
                    ndr.readOctetArray(new sbyte[objectBufferLength - bytesRead], 0, objectBufferLength - bytesRead);
                }
            }



            public virtual void read(NdrCodec ndr) {

                JIOrpcThat.decode(ndr);

                //MInterfacePointer** ppActProperties

                var listOfDefferedPointers = new ArrayList();
                var ppActProperties = (JIInterfacePointer)JIMarshalUnMarshalHelper.deSerialize(ndr, typeof(JIInterfacePointer), listOfDefferedPointers, JIFlags.FLAG_NULL, new Hashtable());

                //Class not registered or any other exception probably.
                if (ppActProperties == null) {
                    var hResult = ndr.ReadUnsignedLong();
                    throw new JIRuntimeException(hResult);
                }

                // we should now be standing at the Activation SharpCifs.Util.Sharpen.Properties Blob right now. 	
                var totalLength = ndr.ReadUnsignedLong();
                ndr.ReadUnsignedLong(); //reserved

                //Custom Header begins
                //lets check what all has been returned back to us. We are only interested in two SharpCifs.Util.Sharpen.Properties (ScmReply and PropsOut)
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

                int objectBufferLength = outerInstance.skipCommonHeader(ndr);
                var startIndex = ndr.Buffer.Index;
                var @struct = new JIStruct();
                try {
                    @struct.addMember(typeof(int?));
                    @struct.addMember(typeof(int?));
                    @struct.addMember(typeof(int?));
                    @struct.addMember(typeof(int?));
                    @struct.addMember(typeof(int?)); //cIfs
                    @struct.addMember(typeof(UUID));
                    @struct.addMember(new JIPointer(new JIArray(typeof(UUID), null, 1, true)));
                    @struct.addMember(new JIPointer(new JIArray(typeof(int?), null, 1, true)));
                    @struct.addMember(typeof(int?));
                }
                catch (JIException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                @struct = outerInstance.decodeStruct(@struct, ndr);

                outerInstance.skipBytes(objectBufferLength, startIndex, ndr);

                //now we need to check for the indexes of our relevant SharpCifs.Util.Sharpen.Properties

                var clsidProps = (UUID[])((JIArray)((JIPointer)@struct.getMember(6)).Referent).ArrayInstance;

                var clsidPropsLengths = (int?[])((JIArray)((JIPointer)@struct.getMember(7)).Referent).ArrayInstance;

                //using the clsidPropsLengths we can skip the NDR buffer of the properties not needed.
                IList<string> requiredProps = new List<string>();
                requiredProps.Add("000001b6-0000-0000-c000-000000000046".ToUpper());
                requiredProps.Add("00000339-0000-0000-c000-000000000046".ToUpper());
                //we will go sequentially so if a property is not found we skip that many bytes ahead
                for (var i = 0; i < clsidProps.Length; i++) {
                    if (requiredProps.Contains(clsidProps[i].ToString().ToUpper())) {
                        //its present so analyse
                        objectBufferLength = outerInstance.skipCommonHeader(ndr);
                        startIndex = ndr.Buffer.Index;
                        @struct = new JIStruct();

                        if (clsidProps[i].ToString().Equals("000001b6-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                            try { //ScmReplyInfo

                                /// <summary>
                                /// typedef struct tagScmReplyInfoData {
                                /// DWORD* pdwReserved;
                                /// customREMOTE_REPLY_SCM_INFO* remoteReply;
                                /// } ScmReplyInfoData;
                                /// </summary>

                                @struct.addMember(typeof(int?));

                                var remoteReplyStruct = new JIStruct();

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
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                remoteReplyStruct.addMember(typeof(sbyte?));
                                //8 bytes (4 + 4 LE) = OXID
                                remoteReplyStruct.addMember(new JIPointer(typeof(JIDualStringArray), false));
                                remoteReplyStruct.addMember(typeof(UUID));
                                remoteReplyStruct.addMember(typeof(int?));
                                //COM Version can be taken as two shorts.
                                //if this COM version is less than 5.6 than we throw an exception
                                remoteReplyStruct.addMember(typeof(short?));
                                remoteReplyStruct.addMember(typeof(short?));

                                @struct.addMember(new JIPointer(remoteReplyStruct));

                            }
                            catch (JIException e) {
                                Console.WriteLine(e.ToString());
                                Console.Write(e.StackTrace);
                            }

                            @struct = outerInstance.decodeStruct(@struct, ndr);
                            @struct = (JIStruct)((JIPointer)@struct.getMember(1)).getReferent();

                            //now we need to get the IPID and Dual String Array.
                            var ndr2 = new NdrCodec();
                            var buffer = new NdrBuffer(new sbyte[8], 0);
                            buffer.buf[0] = unchecked((sbyte)(((sbyte?)@struct.getMember(0)) & 0xFF));
                            buffer.buf[1] = unchecked((sbyte)(((sbyte?)@struct.getMember(1)) & 0xFF));
                            buffer.buf[2] = unchecked((sbyte)(((sbyte?)@struct.getMember(2)) & 0xFF));
                            buffer.buf[3] = unchecked((sbyte)(((sbyte?)@struct.getMember(3)) & 0xFF));
                            buffer.buf[4] = unchecked((sbyte)(((sbyte?)@struct.getMember(4)) & 0xFF));
                            buffer.buf[5] = unchecked((sbyte)(((sbyte?)@struct.getMember(5)) & 0xFF));
                            buffer.buf[6] = unchecked((sbyte)(((sbyte?)@struct.getMember(6)) & 0xFF));
                            buffer.buf[7] = unchecked((sbyte)(((sbyte?)@struct.getMember(7)) & 0xFF));
                            ndr2.Buffer = buffer;

                            oxid = JIMarshalUnMarshalHelper.readOctetArrayLE(ndr2, 8);
                            dualStringArrayForOxid = (JIDualStringArray)((JIPointer)@struct.getMember(8)).getReferent();
                            ipid = ((UUID)@struct.getMember(9)).ToString();
                            authenticationHint = (int?)@struct.getMember(10);
                            comVersion = new JIComVersion((short?)@struct.getMember(11), (short?)@struct.getMember(12));
                        }
                        else {
                            if (clsidProps[i].ToString().Equals("00000339-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                                try { //PropsOutInfo

                                    /// <summary>
                                    /// typedef struct tagPropsOutInfo {
                                    /// [range(1, MAX_REQUESTED_INTERFACES)]
                                    /// DWORD cIfs;
                                    /// [size_is(cIfs)] IID* piid;
                                    /// [size_is(cIfs)] HRESULT* phresults;
                                    /// [size_is(cIfs)] MInterfacePointer** ppIntfData;
                                    /// } PropsOutInfo;
                                    /// </summary>

                                    @struct.addMember(typeof(int?));
                                    @struct.addMember(new JIPointer(new JIArray(typeof(UUID), null, 1, true)));
                                    @struct.addMember(new JIPointer(new JIArray(typeof(int?), null, 1, true))); //Hresult,
                                                                                                                //0 is good anything else is bad and corresponding MInterfacePointer will not exist. 
                                    @struct.addMember(new JIPointer(new JIArray(typeof(JIInterfacePointer), null, 1, true)));

                                }
                                catch (JIException e) {
                                    Console.WriteLine(e.ToString());
                                    Console.Write(e.StackTrace);
                                }

                                @struct = outerInstance.decodeStruct(@struct, ndr);

                                var marshalledIp = (JIInterfacePointer[])((JIArray)((JIPointer)@struct.getMember(3)).getReferent()).ArrayInstance;

                                var iids = (UUID[])((JIArray)((JIPointer)@struct.getMember(1)).getReferent()).ArrayInstance;

                                //now get the hresults and only those IIDs are supported which have 0x00000000
                                //in our case IUnknown will always be supported (naturally) where as IDispatch may or may not be.
                                var hresults = (int?[])((JIArray)((JIPointer)@struct.getMember(2)).getReferent()).ArrayInstance;
                                for (var j = 0; j < hresults.Length; j++) {
                                    if (hresults[j] == 0x00000000) {
                                        //pointer exists
                                        //if it is Disp IID then set dual stuff else it has to be IUnknown, save it.
                                        if (iids[j].ToString().Equals("00000000-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                                            //IUnknown
                                            mInterfacePointer = marshalledIp[j];
                                        }
                                        else if (iids[j].ToString().Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                                            //dual is supported since the IDispatch was obtained
                                            isDual = true;
                                            //eat this keeping only the IPID for cleanup , let the user perform another queryInterface for this.
                                            var ptr = marshalledIp[j];
                                            dispIpid = ptr.IPID;
                                            dispOid = ptr.OID;
                                            dispRefs = ((JIStdObjRef)ptr.getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
                                        }
                                    }
                                }

                            }
                        }

                        outerInstance.skipBytes(objectBufferLength, startIndex, ndr);


                    }
                    else {
                        var skip = new sbyte[clsidPropsLengths[i]];
                        ndr.readOctetArray(skip, 0, skip.Length);
                    }
                }

                isActivationSuccessful = true;
            }

            internal virtual JIStruct decodeStruct(JIStruct @struct, NdrCodec ndr) {
                IList listOfDefferedPointers = new ArrayList();
                IDictionary additionalData = new Hashtable();
                @struct = @struct.decode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL, additionalData);
                var x = 0;
                while (x < listOfDefferedPointers.Count) {
                    var newList = new ArrayList();
                    var replacement = (JIPointer)JIMarshalUnMarshalHelper.deSerialize(ndr, (JIPointer)listOfDefferedPointers[x], newList, JIFlags.FLAG_NULL, additionalData);
                    ((JIPointer)listOfDefferedPointers[x]).replaceSelfWithNewPointer(replacement);
                    x++; //incrementing index
                    listOfDefferedPointers.AddRange(x, newList);
                }

                return @struct;
            }

            public virtual bool ActivationSuccessful => isActivationSuccessful;

            public virtual JIDualStringArray DualStringArrayForOxid => dualStringArrayForOxid;

            public virtual JIInterfacePointer MInterfacePointer => mInterfacePointer;

            public virtual string IPID => ipid;

            public virtual bool Dual => isDual;

            public virtual string DispIpid {
                get => dispIpid;
                set => dispIpid = value;
            }

            public virtual int DispRefs => dispRefs;
        }
    }
}