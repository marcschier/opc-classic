//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using rpc.core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IRemoteSCMActivator implementation.
    /// </summary>
    internal sealed class JIRemoteSCMActivator {

        //        HRESULT RemoteCreateInstance(
        //                [in] handle_t hRpc,
        //                [in] ORPCTHIS* orpcthis,
        //                [out] ORPCTHAT* orpcthat,
        //                [in, unique] MInterfacePointer* pUnkOuter,
        //                [in, unique] MInterfacePointer* pActProperties,
        //                [out] MInterfacePointer** ppActProperties
        //                );
        internal class RemoteCreateInstance : NdrOp, IServerActivation {
            private readonly string _targetClsid;
            private readonly string _targetServer;
#pragma warning disable IDE0052 // Remove unread private members
            private byte[] _oxid;
            private int _authenticationHint = -1;
            private JIComVersion _comVersion;
            private byte[] _dispOid;
#pragma warning restore IDE0052 // Remove unread private members

            /// <summary>
            /// Create
            /// </summary>
            /// <param name="targetServer"></param>
            /// <param name="clsid"></param>
            public RemoteCreateInstance(string targetServer, string clsid) {
                _targetClsid = clsid;
                _targetServer = targetServer;
            }

            /// <inheritdoc/>
            public override int Opnum => 4;

            /// <inheritdoc/>
            public override void Write(NdrCodec ndr) {
                var orpcThis = new JIOrpcThis();
                orpcThis.Encode(ndr);

                ndr.WriteUnsignedLong(0); // pUnkOuter, setting it to NULL.
                ndr.WriteUnsignedLong(0x00020000);

                var index = ndr.Buffer.Index; // recording where we have to write length
                ndr.WriteUnsignedLong(0); // Len 1

                // alignment may kick in
                var index2 = ndr.Buffer.Index; // recording where we have to write length
                ndr.WriteUnsignedLong(0); // Len 2

                var countFromIndex = ndr.Buffer.Index; // recording from where we have to write
                ndr.WriteUnsignedLong(0x574f454d); // Signature MEOW
                ndr.WriteUnsignedLong(4); // OBJREF_CUSTOM

                // now we will write the Custom Interface pointer to Activation SharpCifs.Util.Sharpen.Properties.
                try {
                    // IID_IActivationPropertiesIn
                    var iid_IActivationPropertiesIn = new UUID("000001a2-0000-0000-c000-000000000046");
                    iid_IActivationPropertiesIn.Encode(ndr, ndr.Buffer);
                    var clsid_IActivationPropertiesIn = new UUID("00000338-0000-0000-c000-000000000046");
                    clsid_IActivationPropertiesIn.Encode(ndr, ndr.Buffer);

                }
                catch (NdrException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                var countEntirePayload = ndr.Buffer.Index; // Entire length of Payload for Custom Marshalling
                ndr.WriteUnsignedLong(0); // extension
                var writeCountEntirePayloadLength_Here = ndr.Buffer.Index;
                ndr.WriteUnsignedLong(0); // write here (reserved from objref_custom)

                // Activation SharpCifs.Util.Sharpen.Properties Blob
                var writeActivationPayload = ndr.Buffer.Index;
                ndr.WriteUnsignedLong(0); // payload to be written here

                ndr.WriteUnsignedLong(0); // reserved
                var countActivationPayload = ndr.Buffer.Index; // Only Activation Payload

                var tempStruct = CustomHeader;
                var lentempStruct = GetLengthOfStruct(tempStruct);
                tempStruct = CustomHeader;
                AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = SpecialPropertyData;
                lentempStruct = GetLengthOfStruct(tempStruct);
                tempStruct = SpecialPropertyData;
                AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = InstantiationInfoData;
                lentempStruct = GetLengthOfStruct(tempStruct);
                tempStruct = InstantiationInfoData;
                AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = SecurityInfoData;
                lentempStruct = GetLengthOfStruct(tempStruct);
                tempStruct = SecurityInfoData;
                AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = ServerLocationInfo;
                lentempStruct = GetLengthOfStruct(tempStruct);
                tempStruct = ServerLocationInfo;
                AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                tempStruct = ScmRequestInfoData;
                lentempStruct = GetLengthOfStruct(tempStruct);
                tempStruct = ScmRequestInfoData;
                AddCommonTypeHeaderAndEncode(ndr, tempStruct, lentempStruct);

                // now update the length in Common header struct.
                WriteEncodingLength(countActivationPayload, countActivationPayload + 16, ndr); // Len for Activation SharpCifs.Util.Sharpen.Properties Blob

                WriteEncodingLength(countActivationPayload, writeActivationPayload, ndr); // Len for Activation SharpCifs.Util.Sharpen.Properties Blob
                WriteEncodingLength(countEntirePayload, writeCountEntirePayloadLength_Here, ndr); // Len for Activation SharpCifs.Util.Sharpen.Properties Blob
                WriteEncodingLength(countFromIndex, index, ndr); // Len 1 for the Custom Object Ref
                WriteEncodingLength(countFromIndex, index2, ndr); // Len 2 for the Custom Object Ref
            }

            internal void WriteEncodingLength(int countFromIndex, int writeAtIndex, NdrCodec ndr) {
                var length = ndr.Buffer.Index - countFromIndex;
                var temp = ndr.Buffer.Index;
                ndr.Buffer.Index = writeAtIndex;
                ndr.WriteUnsignedLong(length);
                ndr.Buffer.Index = temp;
            }

            // Pass the length from outside as to calculate it we need to encode the struct and that mutates the internal data structs
            // will return total length of the structure including common header and padding.
            internal int AddCommonTypeHeaderAndEncode(NdrCodec ndr, JIStruct @struct, int lengthOfStruct) {
                //            will add the common type header and write on wire

                // common header has to be a multiple of 8 bytes. If not it has to be padded at the end.
                var padding = lengthOfStruct % 8;

                var startI = ndr.Buffer.Index;

                // 2.2.6.1 Common Type Header for the Serialization Stream (MS-RPCE)
                ndr.WriteUnsignedSmall(0x01); // version
                ndr.WriteUnsignedSmall(0x10); // endianness
                ndr.WriteUnsignedShort(0x08); // common header length
                ndr.WriteUnsignedLong(unchecked((int)0xCCCCCCCC)); // Filler

                // now comes the length of the entire CustomHeader without the Common Type Header and this length and Filler.
                var writeAtIndex = ndr.Buffer.Index;
                ndr.WriteUnsignedLong(0); // write here

                ndr.WriteUnsignedLong(0); // filler, set to NULL

                var countFromIndex = ndr.Buffer.Index;

                var x = 0;
                var listOfDefferedPointers = new List<object>();
                @struct.Encode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL);
                while (x < listOfDefferedPointers.Count) {
                    var newList = new List<object>();
                    var referent = ((JIPointer)listOfDefferedPointers[x]).Referent;
                    if (referent is JIStruct) {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIStruct), referent, newList, JIFlags.FLAG_NULL);
                    }
                    else {
                        if (referent is JIString) {
                            JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIString), referent, newList, JIFlags.FLAG_NULL);
                        }
                        else {
                            JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIArray), referent, newList, JIFlags.FLAG_NULL);
                        }
                    }
                    x++; // incrementing index
                    listOfDefferedPointers.InsertRange(x, newList);
                }

                if (padding != 0) {
                    padding = 8 - padding;
                    ndr.WriteOctetArray(new byte[padding], 0, padding);
                }

                WriteEncodingLength(countFromIndex, writeAtIndex, ndr);

                return ndr.Buffer.Index - startI;
            }

            internal JIStruct CustomHeader {
                get {
                    var @struct = GetCustomHeader();
                    var ndr = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    var lenOfStruct = GetLengthOfStruct(@struct);
                    @struct = GetCustomHeader();
                    var len = AddCommonTypeHeaderAndEncode(ndr, @struct, lenOfStruct);
                    // now we read the length to put into this struct
                    ndr.Buffer.Index = 8;
                    // int len = ndr.readUnsignedLong() + 16; // 8 for common type header and (4 + 4) for header length and reserved.
                    @struct = GetCustomHeader();
                    @struct.RemoveMember(1);
                    try {
                        @struct.AddMember(1, len); // will push Reserved to the next place now.
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

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
            internal JIStruct GetCustomHeader() {

                var @struct = new JIStruct();

                try {

                    @struct.AddMember(0); // Total Activation Blob size

                    // Correct length set in getCustomHeader.
                    @struct.AddMember(0); // Total Custom header size including the common type header (from this common type header to start of the next common type header)

                    @struct.AddMember(0);

                    @struct.AddMember(2);

                    // sending 5 cIfs
                    @struct.AddMember(5);

                    @struct.AddMember(new UUID(UUID.NIL_UUID));

                    @struct.AddMember(new JIPointer(new JIArray(new UUID[]{ new UUID("000001b9-0000-0000-c000-000000000046"), new UUID("000001ab-0000-0000-c000-000000000046"), new UUID("000001a6-0000-0000-c000-000000000046"), new UUID("000001a4-0000-0000-c000-000000000046"), new UUID("000001aa-0000-0000-c000-000000000046")
                }, true)));

                    // now come their sizes including their Common headers.
                    var ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    var tempStruct = SpecialPropertyData;
                    var lentempStruct = GetLengthOfStruct(tempStruct);
                    tempStruct = SpecialPropertyData;
                    var lenSpecialSystemProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    tempStruct = InstantiationInfoData;
                    lentempStruct = GetLengthOfStruct(tempStruct);
                    tempStruct = InstantiationInfoData;
                    var lenInstantiationInfoProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    tempStruct = SecurityInfoData;
                    lentempStruct = GetLengthOfStruct(tempStruct);
                    tempStruct = SecurityInfoData;
                    var lenSecurityInfoProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    tempStruct = ServerLocationInfo;
                    lentempStruct = GetLengthOfStruct(tempStruct);
                    tempStruct = ServerLocationInfo;
                    var lenServerLocationProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);

                    ndr2 = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    tempStruct = ScmRequestInfoData;
                    lentempStruct = GetLengthOfStruct(tempStruct);
                    tempStruct = ScmRequestInfoData;
                    var lenScmRequestInfoDataProp = AddCommonTypeHeaderAndEncode(ndr2, tempStruct, lentempStruct);


                    @struct.AddMember(new JIPointer(new JIArray(new int[] { lenSpecialSystemProp, lenInstantiationInfoProp, lenSecurityInfoProp, lenServerLocationProp, lenScmRequestInfoDataProp }, true)));

                    @struct.AddMember(0); // reserved

                }
                catch (JIException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                } // don't know will correct later.

                return @struct;
            }

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
            internal JIStruct GetInstantiationInfoData() {

                var @struct = new JIStruct();
                try {

                    @struct.AddMember(new UUID(_targetClsid));
                    @struct.AddMember(0x14); //  CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_INPROC_SERVER16
                    @struct.AddMember(0);
                    @struct.AddMember(0);
                    @struct.AddMember(2); // IUnknown and IDispatch
                    @struct.AddMember(0);
                    var ptr = new JIPointer(new JIArray(new UUID[] { new UUID("00000000-0000-0000-c000-000000000046"), new UUID("00020400-0000-0000-c000-000000000046") }, true));
                    ptr.SetFlags(JIFlags.FLAG_REPRESENTATION_ARRAY);
                    @struct.AddMember(ptr);

                    // size of the current struct, why ? why ???
                    @struct.AddMember(0); // don't know will replace later on. (remove and add)
                    @struct.AddMember(Convert.ToInt16((short)JISystem.COMVersion.MajorVersion));
                    @struct.AddMember(Convert.ToInt16((short)JISystem.COMVersion.MinorVersion));

                }
                catch (JIException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }


                return @struct;
            }

            /// <summary>
            /// typedef struct tagSecurityInfoData {
            /// DWORD dwAuthnFlags;
            /// COSERVERINFO* pServerInfo;
            /// DWORD* pdwReserved;
            /// } SecurityInfoData
            /// </summary>
            internal JIStruct SecurityInfoData {
                get {
                    var @struct = new JIStruct();
                    try {
                        @struct.AddMember(0);

                        var coserver = new JIStruct();
                        coserver.AddMember(0);
                        coserver.AddMember(new JIPointer(new JIString(_targetServer, JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                        coserver.AddMember(0);
                        coserver.AddMember(0);
                        @struct.AddMember(new JIPointer(coserver));
                        @struct.AddMember(0);
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

            /// <summary>
            /// typedef struct tagLocationInfoData {
            /// [string] wchar_t* machineName;
            /// DWORD processId;
            /// DWORD apartmentId;
            /// DWORD contextId;
            /// } LocationInfoData;
            /// </summary>
            internal JIStruct ServerLocationInfo {
                get {
                    var @struct = new JIStruct();
                    try {

                        @struct.AddMember(0);
                        @struct.AddMember(0);
                        @struct.AddMember(0);
                        @struct.AddMember(0);

                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

            /// <summary>
            /// typedef struct tagScmRequestInfoData {
            /// DWORD* pdwReserved;
            /// customREMOTE_REQUEST_SCM_INFO* remoteRequest;
            /// } ScmRequestInfoData
            /// </summary>
            internal JIStruct ScmRequestInfoData {
                get {
                    var @struct = new JIStruct();
                    try {

                        @struct.AddMember(0);

                        var _customRemoteRequestSCMInfo = new JIStruct();
                        _customRemoteRequestSCMInfo.AddMember(2);
                        _customRemoteRequestSCMInfo.AddMember((short)1);
                        _customRemoteRequestSCMInfo.AddMember(new JIPointer(new JIArray(new short[] { 0x07 }, true)));
                        @struct.AddMember(new JIPointer(_customRemoteRequestSCMInfo));

                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }


            internal JIStruct InstantiationInfoData {
                get {
                    var @struct = GetInstantiationInfoData();
                    var lenOfStruct = GetLengthOfStruct(@struct);
                    @struct = GetInstantiationInfoData();
                    var ndr = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    var len = AddCommonTypeHeaderAndEncode(ndr, @struct, lenOfStruct);
                    // now we read the length to put into this struct
                    //            ndr.getBuffer().setIndex(8);
                    //            int len = ndr.readUnsignedLong();
                    @struct = GetInstantiationInfoData();
                    @struct.RemoveMember(7);
                    try {
                        @struct.AddMember(7, len); // will push COMVERSION to last place now.
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }

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
            internal JIStruct SpecialPropertyData {
                get {

                    var @struct = new JIStruct();
                    try {
                        @struct.AddMember(unchecked((int)0xFFFFFFFF));
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000001); // auth level none ? Why ?
                        @struct.AddMember(new UUID(UUID.NIL_UUID));
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x14);
                        @struct.AddMember(0x2);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x0000000000000000);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000000);
                        @struct.AddMember(0x00000000);
                    }
                    catch (JIException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    return @struct;
                }
            }


            // discard this struct after use and create a new one
            internal int GetLengthOfStruct(JIStruct @struct) {
                var ndr = new NdrCodec {
                    Buffer = new NdrBuffer(new byte[512], 0)
                };
                var startI = ndr.Buffer.Index;

                var x = 0;
                var listOfDefferedPointers = new List<object>();
                @struct.Encode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL);
                while (x < listOfDefferedPointers.Count) {
                    var newList = new List<object>();
                    var referent = ((JIPointer)listOfDefferedPointers[x]).Referent;
                    if (referent is JIStruct) {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIStruct), referent, newList, JIFlags.FLAG_NULL);
                    }
                    else {
                        if (referent is JIString) {
                            JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIString), referent, newList, JIFlags.FLAG_NULL);
                        }
                        else {
                            JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIArray), referent, newList, JIFlags.FLAG_NULL);
                        }
                    }
                    x++; // incrementing index
                    listOfDefferedPointers.InsertRange(x, newList);
                }

                return ndr.Buffer.Index - startI;
            }


            // Skip common header and return total length of the object buffer inside. We will need to skip the
            // padded bytes as well once we have analyzed the complete objectBuffer.
            internal int SkipCommonHeader(NdrCodec ndr) {
                ndr.ReadUnsignedSmall(); // version
                ndr.ReadUnsignedSmall(); // endianness
                ndr.ReadUnsignedShort(); // common header length
                ndr.ReadUnsignedLong(); // Filler
                var retlength = ndr.ReadUnsignedLong();
                ndr.ReadUnsignedLong(); // reserved
                return retlength;
            }

            internal void SkipBytes(int objectBufferLength, int startIndex, NdrCodec ndr) {
                var bytesRead = ndr.Buffer.Index - startIndex;
                if (objectBufferLength > bytesRead) {
                    ndr.ReadOctetArray(new byte[objectBufferLength - bytesRead], 0, objectBufferLength - bytesRead);
                }
            }



            public override void Read(NdrCodec ndr) {

                JIOrpcThat.Decode(ndr);

                // MInterfacePointer** ppActProperties

                var listOfDefferedPointers = new List<object>();
                var ppActProperties = (JIInterfacePointer)JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(JIInterfacePointer), listOfDefferedPointers, JIFlags.FLAG_NULL, new Hashtable());

                // Class not registered or any other exception probably.
                if (ppActProperties == null) {
                    var hResult = ndr.ReadUnsignedLong();
                    throw new JIRuntimeException(hResult);
                }

                // we should now be standing at the Activation SharpCifs.Util.Sharpen.Properties Blob right now.
                var totalLength = ndr.ReadUnsignedLong();
                ndr.ReadUnsignedLong(); // reserved

                // Custom Header begins
                // lets check what all has been returned back to us. We are only interested in two SharpCifs.Util.Sharpen.Properties (ScmReply and PropsOut)
                // Must contain the following properties
                //            ScmReplyInfoData 2.2.22.2.8 Required
                //            PropsOutInfo 2.2.22.2.9 Required

                // typedef struct tagCustomHeader {
                // DWORD totalSize;
                // DWORD headerSize;
                // DWORD dwReserved;
                // DWORD destCtx;
                // [range(MIN_ACTPROP_LIMIT, MAX_ACTPROP_LIMIT)]
                // DWORD cIfs;
                // CLSID classInfoClsid;
                // [size_is(cIfs)] CLSID* pclsid;
                // [size_is(cIfs)] DWORD* pSizes;
                // DWORD* pdwReserved;
                // } CustomHeader;

                var objectBufferLength = SkipCommonHeader(ndr);
                var startIndex = ndr.Buffer.Index;
                var @struct = new JIStruct();
                try {
                    @struct.AddMember(typeof(int));
                    @struct.AddMember(typeof(int));
                    @struct.AddMember(typeof(int));
                    @struct.AddMember(typeof(int));
                    @struct.AddMember(typeof(int)); // cIfs
                    @struct.AddMember(typeof(UUID));
                    @struct.AddMember(new JIPointer(new JIArray(typeof(UUID), null, 1, true)));
                    @struct.AddMember(new JIPointer(new JIArray(typeof(int), null, 1, true)));
                    @struct.AddMember(typeof(int));
                }
                catch (JIException e) {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                @struct = DecodeStruct(@struct, ndr);

                SkipBytes(objectBufferLength, startIndex, ndr);

                // now we need to check for the indexes of our relevant SharpCifs.Util.Sharpen.Properties

                var clsidProps = (UUID[])((JIArray)((JIPointer)@struct.GetMember(6)).Referent).ArrayInstance;

                var clsidPropsLengths = (int[])((JIArray)((JIPointer)@struct.GetMember(7)).Referent).ArrayInstance;

                // using the clsidPropsLengths we can skip the NDR buffer of the properties not needed.
                IList<string> requiredProps = new List<string> {
                    "000001b6-0000-0000-c000-000000000046".ToUpper(),
                    "00000339-0000-0000-c000-000000000046".ToUpper()
                };
                // we will go sequentially so if a property is not found we skip that many bytes ahead
                for (var i = 0; i < clsidProps.Length; i++) {
                    if (requiredProps.Contains(clsidProps[i].ToString().ToUpper())) {
                        // its present so analyse
                        objectBufferLength = SkipCommonHeader(ndr);
                        startIndex = ndr.Buffer.Index;
                        @struct = new JIStruct();

                        if (clsidProps[i].ToString().Equals("000001b6-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                            try { // ScmReplyInfo

                                // typedef struct tagScmReplyInfoData {
                                // DWORD* pdwReserved;
                                // customREMOTE_REPLY_SCM_INFO* remoteReply;
                                // } ScmReplyInfoData;

                                @struct.AddMember(typeof(int));

                                var remoteReplyStruct = new JIStruct();

                                // typedef struct _customREMOTE_REPLY_SCM_INFO {
                                // OXID Oxid;
                                // DUALSTRINGARRAY* pdsaOxidBindings;
                                // IPID ipidRemUnknown;
                                // DWORD authnHint;
                                // COMVERSION serverVersion;
                                // } customREMOTE_REPLY_SCM_INFO;
                                // we need to take out oxid only way to do it is byte by byte
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                remoteReplyStruct.AddMember(typeof(byte));
                                // 8 bytes (4 + 4 LE) = OXID
                                remoteReplyStruct.AddMember(new JIPointer(typeof(JIDualStringArray), false));
                                remoteReplyStruct.AddMember(typeof(UUID));
                                remoteReplyStruct.AddMember(typeof(int));
                                // COM Version can be taken as two shorts.
                                // if this COM version is less than 5.6 than we throw an exception
                                remoteReplyStruct.AddMember(typeof(short));
                                remoteReplyStruct.AddMember(typeof(short));

                                @struct.AddMember(new JIPointer(remoteReplyStruct));

                            }
                            catch (JIException e) {
                                Console.WriteLine(e.ToString());
                                Console.Write(e.StackTrace);
                            }

                            @struct = DecodeStruct(@struct, ndr);
                            @struct = (JIStruct)((JIPointer)@struct.GetMember(1)).Referent;

                            // now we need to get the IPID and Dual String Array.
                            var ndr2 = new NdrCodec();
                            var buffer = new NdrBuffer(new byte[8], 0);
                            buffer.Buf[0] = (byte)@struct.GetMember(0);
                            buffer.Buf[1] = (byte)@struct.GetMember(1);
                            buffer.Buf[2] = (byte)@struct.GetMember(2);
                            buffer.Buf[3] = (byte)@struct.GetMember(3);
                            buffer.Buf[4] = (byte)@struct.GetMember(4);
                            buffer.Buf[5] = (byte)@struct.GetMember(5);
                            buffer.Buf[6] = (byte)@struct.GetMember(6);
                            buffer.Buf[7] = (byte)@struct.GetMember(7);
                            ndr2.Buffer = buffer;

                            _oxid = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr2, 8);
                            DualStringArrayForOxid = (JIDualStringArray)((JIPointer)@struct.GetMember(8)).Referent;
                            IPID = ((UUID)@struct.GetMember(9)).ToString();
                            _authenticationHint = (int)@struct.GetMember(10);
                            _comVersion = new JIComVersion((short)@struct.GetMember(11), (short)@struct.GetMember(12));
                        }
                        else {
                            if (clsidProps[i].ToString().Equals("00000339-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                                try { // PropsOutInfo

                                    // typedef struct tagPropsOutInfo {
                                    // [range(1, MAX_REQUESTED_INTERFACES)]
                                    // DWORD cIfs;
                                    // [size_is(cIfs)] IID* piid;
                                    // [size_is(cIfs)] HRESULT* phresults;
                                    // [size_is(cIfs)] MInterfacePointer** ppIntfData;
                                    // } PropsOutInfo;

                                    @struct.AddMember(typeof(int));
                                    @struct.AddMember(new JIPointer(new JIArray(typeof(UUID), null, 1, true)));
                                    @struct.AddMember(new JIPointer(new JIArray(typeof(int), null, 1, true))); // Hresult,
                                                                                                                // 0 is good anything else is bad and corresponding MInterfacePointer will not exist.
                                    @struct.AddMember(new JIPointer(new JIArray(typeof(JIInterfacePointer), null, 1, true)));

                                }
                                catch (JIException e) {
                                    Console.WriteLine(e.ToString());
                                    Console.Write(e.StackTrace);
                                }

                                @struct = DecodeStruct(@struct, ndr);

                                var marshalledIp = (JIInterfacePointer[])((JIArray)((JIPointer)@struct.GetMember(3)).Referent).ArrayInstance;

                                var iids = (UUID[])((JIArray)((JIPointer)@struct.GetMember(1)).Referent).ArrayInstance;

                                // now get the hresults and only those IIDs are supported which have 0x00000000
                                // in our case IUnknown will always be supported (naturally) where as IDispatch may or may not be.
                                var hresults = (int[])((JIArray)((JIPointer)@struct.GetMember(2)).Referent).ArrayInstance;
                                for (var j = 0; j < hresults.Length; j++) {
                                    if (hresults[j] == 0x00000000) {
                                        // pointer exists
                                        // if it is Disp IID then set dual stuff else it has to be IUnknown, save it.
                                        if (iids[j].ToString().Equals("00000000-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                                            // IUnknown
                                            MInterfacePointer = marshalledIp[j];
                                        }
                                        else if (iids[j].ToString().Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                                            // dual is supported since the IDispatch was obtained
                                            Dual = true;
                                            // eat this keeping only the IPID for cleanup, let the user perform another queryInterface for this.
                                            var ptr = marshalledIp[j];
                                            DispIpid = ptr.IPID;
                                            _dispOid = ptr.OID;
                                            DispRefs = ((JIStdObjRef)ptr.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
                                        }
                                    }
                                }

                            }
                        }

                        SkipBytes(objectBufferLength, startIndex, ndr);


                    }
                    else {
                        var skip = new byte[clsidPropsLengths[i]];
                        ndr.ReadOctetArray(skip, 0, skip.Length);
                    }
                }

                ActivationSuccessful = true;
            }

            internal JIStruct DecodeStruct(JIStruct @struct, NdrCodec ndr) {
                var listOfDefferedPointers = new List<object>();
                var additionalData = new Hashtable();
                @struct = @struct.Decode(ndr, listOfDefferedPointers, JIFlags.FLAG_NULL, additionalData);
                var x = 0;
                while (x < listOfDefferedPointers.Count) {
                    var newList = new List<object>();
                    var replacement = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr, (JIPointer)listOfDefferedPointers[x], newList, JIFlags.FLAG_NULL, additionalData);
                    ((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement);
                    x++; // incrementing index
                    listOfDefferedPointers.InsertRange(x, newList);
                }

                return @struct;
            }

            public bool ActivationSuccessful { get; private set; }

            public JIDualStringArray DualStringArrayForOxid { get; private set; }

            public JIInterfacePointer MInterfacePointer { get; private set; }

            public string IPID { get; private set; }

            public bool Dual { get; private set; }

            public string DispIpid { get; set; }

            public int DispRefs { get; private set; } = 5;
        }
    }
}