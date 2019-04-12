//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using SharpInterop.Common;
    using SharpInterop.Rpc.Core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using Serilog;

    /// <summary>
    /// IRemoteSCMActivator implementation.
    /// </summary>
    internal sealed class RemoteSCMActivator {

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
            private ComVersion _comVersion;
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
                var orpcThis = new OrpcThis();
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

                // now we will write the Custom Interface pointer to Activation Properties.
                try {
                    new UUID(Interfaces.IID_IActivationPropertiesIn).Encode(ndr, ndr.Buffer);
                    new UUID(Classes.CLSID_ActivationPropertiesIn).Encode(ndr, ndr.Buffer);
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "Writing uuids");
                }
                // Entire length of Payload for Custom Marshalling
                var countEntirePayload = ndr.Buffer.Index; 
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

            /// <summary>
            /// Write length
            /// </summary>
            /// <param name="countFromIndex"></param>
            /// <param name="writeAtIndex"></param>
            /// <param name="ndr"></param>
            internal void WriteEncodingLength(int countFromIndex, int writeAtIndex, NdrCodec ndr) {
                var length = ndr.Buffer.Index - countFromIndex;
                var temp = ndr.Buffer.Index;
                ndr.Buffer.Index = writeAtIndex;
                ndr.WriteUnsignedLong(length);
                ndr.Buffer.Index = temp;
            }

            // Pass the length from outside as to calculate it we need to encode the struct and that mutates the internal data structs
            // will return total length of the structure including common header and padding.
            internal int AddCommonTypeHeaderAndEncode(NdrCodec ndr, Struct strukt, int lengthOfStruct) {
                // will add the common type header and write on wire

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

                var context = new CodecContext();
                strukt.Encode(ndr, context);
                context.EncodeDeferredPointers(ndr);

                if (padding != 0) {
                    padding = 8 - padding;
                    ndr.WriteOctetArray(new byte[padding], 0, padding);
                }

                WriteEncodingLength(countFromIndex, writeAtIndex, ndr);

                return ndr.Buffer.Index - startI;
            }

            internal Struct CustomHeader {
                get {
                    var strukt = GetCustomHeader();
                    var ndr = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    var lenOfStruct = GetLengthOfStruct(strukt);
                    strukt = GetCustomHeader();
                    var len = AddCommonTypeHeaderAndEncode(ndr, strukt, lenOfStruct);
                    // now we read the length to put into this struct
                    ndr.Buffer.Index = 8;
                    // int len = ndr.readUnsignedLong() + 16; // 8 for common type header and (4 + 4) for header length and reserved.
                    strukt = GetCustomHeader();
                    strukt.RemoveMember(1);
                    try {
                        strukt.AddMember(1, len); // will push Reserved to the next place now.
                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "Adding member");
                    }

                    return strukt;
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
            internal Struct GetCustomHeader() {

                var strukt = new Struct();

                try {

                    strukt.AddMember(0); // Total Activation Blob size

                    // Correct length set in getCustomHeader.
                    strukt.AddMember(0); // Total Custom header size including the common type header (from this common type header to start of the next common type header)

                    strukt.AddMember(0);

                    strukt.AddMember(2);

                    // sending 5 cIfs
                    strukt.AddMember(5);

                    strukt.AddMember(new UUID(UUID.NIL_UUID));

                    strukt.AddMember(new ComPointer(new ComArray(new UUID[]{ new UUID("000001b9-0000-0000-c000-000000000046"), new UUID("000001ab-0000-0000-c000-000000000046"), new UUID("000001a6-0000-0000-c000-000000000046"), new UUID("000001a4-0000-0000-c000-000000000046"), new UUID("000001aa-0000-0000-c000-000000000046")
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


                    strukt.AddMember(new ComPointer(new ComArray(new int[] { lenSpecialSystemProp, lenInstantiationInfoProp, lenSecurityInfoProp, lenServerLocationProp, lenScmRequestInfoDataProp }, true)));

                    strukt.AddMember(0); // reserved

                }
                catch (InteropException e) {
                    Log.Logger.Error(e, "Adding member");
                } // don't know will correct later.

                return strukt;
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
            internal Struct GetInstantiationInfoData() {

                var strukt = new Struct();
                try {

                    strukt.AddMember(new UUID(_targetClsid));
                    strukt.AddMember(0x14); //  CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_INPROC_SERVER16
                    strukt.AddMember(0);
                    strukt.AddMember(0);
                    strukt.AddMember(2); // IUnknown and IDispatch
                    strukt.AddMember(0);
                    var ptr = new ComPointer(new ComArray(new UUID[] { new UUID(Interfaces.IID_IUnknown), new UUID(Interfaces.IID_IDispatch) }, true));
                    ptr.SetFlags(InteropFlags.FLAG_REPRESENTATION_ARRAY);
                    strukt.AddMember(ptr);

                    // size of the current struct, why ? why ???
                    strukt.AddMember(0); // don't know will replace later on. (remove and add)
                    strukt.AddMember(Convert.ToInt16((short)Interop.COMVersion.MajorVersion));
                    strukt.AddMember(Convert.ToInt16((short)Interop.COMVersion.MinorVersion));

                }
                catch (InteropException e) {
                    Log.Logger.Error(e, "Adding member");
                }


                return strukt;
            }

            /// <summary>
            /// typedef struct tagSecurityInfoData {
            /// DWORD dwAuthnFlags;
            /// COSERVERINFO* pServerInfo;
            /// DWORD* pdwReserved;
            /// } SecurityInfoData
            /// </summary>
            internal Struct SecurityInfoData {
                get {
                    var strukt = new Struct();
                    try {
                        strukt.AddMember(0);

                        var coserver = new Struct();
                        coserver.AddMember(0);
                        coserver.AddMember(new ComPointer(new ComString(_targetServer, InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                        coserver.AddMember(0);
                        coserver.AddMember(0);
                        strukt.AddMember(new ComPointer(coserver));
                        strukt.AddMember(0);
                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "Adding member");
                    }

                    return strukt;
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
            internal Struct ServerLocationInfo {
                get {
                    var strukt = new Struct();
                    try {

                        strukt.AddMember(0);
                        strukt.AddMember(0);
                        strukt.AddMember(0);
                        strukt.AddMember(0);

                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "Adding member");
                    }

                    return strukt;
                }
            }

            /// <summary>
            /// typedef struct tagScmRequestInfoData {
            /// DWORD* pdwReserved;
            /// customREMOTE_REQUEST_SCM_INFO* remoteRequest;
            /// } ScmRequestInfoData
            /// </summary>
            internal Struct ScmRequestInfoData {
                get {
                    var strukt = new Struct();
                    try {

                        strukt.AddMember(0);

                        var _customRemoteRequestSCMInfo = new Struct();
                        _customRemoteRequestSCMInfo.AddMember(2);
                        _customRemoteRequestSCMInfo.AddMember((short)1);
                        _customRemoteRequestSCMInfo.AddMember(new ComPointer(new ComArray(new short[] { 0x07 }, true)));
                        strukt.AddMember(new ComPointer(_customRemoteRequestSCMInfo));

                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "Adding member");
                    }

                    return strukt;
                }
            }


            internal Struct InstantiationInfoData {
                get {
                    var strukt = GetInstantiationInfoData();
                    var lenOfStruct = GetLengthOfStruct(strukt);
                    strukt = GetInstantiationInfoData();
                    var ndr = new NdrCodec {
                        Buffer = new NdrBuffer(new byte[512], 0)
                    };
                    var len = AddCommonTypeHeaderAndEncode(ndr, strukt, lenOfStruct);
                    // now we read the length to put into this struct
                    //            ndr.getBuffer().setIndex(8);
                    //            int len = ndr.readUnsignedLong();
                    strukt = GetInstantiationInfoData();
                    strukt.RemoveMember(7);
                    try {
                        strukt.AddMember(7, len); // will push COMVERSION to last place now.
                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "Adding member");
                    }

                    return strukt;
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
            internal Struct SpecialPropertyData {
                get {

                    var strukt = new Struct();
                    try {
                        strukt.AddMember(unchecked((int)0xFFFFFFFF));
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000001); // auth level none ? Why ?
                        strukt.AddMember(new UUID(UUID.NIL_UUID));
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x14);
                        strukt.AddMember(0x2);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x0000000000000000);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000000);
                        strukt.AddMember(0x00000000);
                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "Adding member");
                    }

                    return strukt;
                }
            }


            // discard this struct after use and create a new one
            internal int GetLengthOfStruct(Struct strukt) {
                var ndr = new NdrCodec {
                    Buffer = new NdrBuffer(new byte[512], 0)
                };
                var startI = ndr.Buffer.Index;
                var context = new CodecContext();
                strukt.Encode(ndr, context);
                context.EncodeDeferredPointers(ndr);
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

                OrpcThat.Decode(ndr);

                // MInterfacePointer** ppActProperties

                var context = new CodecContext();
                var ppActProperties = (InterfacePointer)MarshalUnMarshalHelper.Deserialize(ndr, typeof(InterfacePointer), context);

                // Class not registered or any other exception probably.
                if (ppActProperties == null) {
                    var hResult = ndr.ReadUnsignedLong();
                    throw new InteropRuntimeException(hResult);
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
                var strukt = new Struct();
                try {
                    strukt.AddMember(typeof(int));
                    strukt.AddMember(typeof(int));
                    strukt.AddMember(typeof(int));
                    strukt.AddMember(typeof(int));
                    strukt.AddMember(typeof(int)); // cIfs
                    strukt.AddMember(typeof(UUID));
                    strukt.AddMember(new ComPointer(new ComArray(typeof(UUID), null, 1, true)));
                    strukt.AddMember(new ComPointer(new ComArray(typeof(int), null, 1, true)));
                    strukt.AddMember(typeof(int));
                }
                catch (InteropException e) {
                    Log.Logger.Error(e, "Adding member");
                }

                strukt = DecodeStruct(strukt, ndr);

                SkipBytes(objectBufferLength, startIndex, ndr);

                // now we need to check for the indexes of our relevant SharpCifs.Util.Sharpen.Properties

                var clsidProps = (UUID[])((ComArray)((ComPointer)strukt.GetMember(6)).Referent).ArrayInstance;

                var clsidPropsLengths = (int[])((ComArray)((ComPointer)strukt.GetMember(7)).Referent).ArrayInstance;

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
                        strukt = new Struct();

                        if (clsidProps[i].ToString().Equals("000001b6-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                            try { // ScmReplyInfo

                                // typedef struct tagScmReplyInfoData {
                                // DWORD* pdwReserved;
                                // customREMOTE_REPLY_SCM_INFO* remoteReply;
                                // } ScmReplyInfoData;

                                strukt.AddMember(typeof(int));

                                var remoteReplyStruct = new Struct();

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
                                remoteReplyStruct.AddMember(new ComPointer(typeof(DualStringArray), false));
                                remoteReplyStruct.AddMember(typeof(UUID));
                                remoteReplyStruct.AddMember(typeof(int));
                                // COM Version can be taken as two shorts.
                                // if this COM version is less than 5.6 than we throw an exception
                                remoteReplyStruct.AddMember(typeof(short));
                                remoteReplyStruct.AddMember(typeof(short));

                                strukt.AddMember(new ComPointer(remoteReplyStruct));

                            }
                            catch (InteropException e) {
                                Log.Logger.Error(e, "Adding member");
                            }

                            strukt = DecodeStruct(strukt, ndr);
                            strukt = (Struct)((ComPointer)strukt.GetMember(1)).Referent;

                            // now we need to get the IPID and Dual String Array.
                            var ndr2 = new NdrCodec();
                            var buffer = new NdrBuffer(new byte[8], 0);
                            buffer.Buf[0] = (byte)strukt.GetMember(0);
                            buffer.Buf[1] = (byte)strukt.GetMember(1);
                            buffer.Buf[2] = (byte)strukt.GetMember(2);
                            buffer.Buf[3] = (byte)strukt.GetMember(3);
                            buffer.Buf[4] = (byte)strukt.GetMember(4);
                            buffer.Buf[5] = (byte)strukt.GetMember(5);
                            buffer.Buf[6] = (byte)strukt.GetMember(6);
                            buffer.Buf[7] = (byte)strukt.GetMember(7);
                            ndr2.Buffer = buffer;

                            _oxid = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr2, 8);
                            DualStringArrayForOxid = (DualStringArray)((ComPointer)strukt.GetMember(8)).Referent;
                            IPID = ((UUID)strukt.GetMember(9)).ToString();
                            _authenticationHint = (int)strukt.GetMember(10);
                            _comVersion = new ComVersion((short)strukt.GetMember(11), (short)strukt.GetMember(12));
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

                                    strukt.AddMember(typeof(int));
                                    strukt.AddMember(new ComPointer(new ComArray(typeof(UUID), null, 1, true)));
                                    strukt.AddMember(new ComPointer(new ComArray(typeof(int), null, 1, true))); // Hresult,
                                                                                                                // 0 is good anything else is bad and corresponding MInterfacePointer will not exist.
                                    strukt.AddMember(new ComPointer(new ComArray(typeof(InterfacePointer), null, 1, true)));

                                }
                                catch (InteropException e) {
                                    Log.Logger.Error(e, "Adding member");
                                }

                                strukt = DecodeStruct(strukt, ndr);

                                var marshalledIp = (InterfacePointer[])((ComArray)((ComPointer)strukt.GetMember(3)).Referent).ArrayInstance;

                                var iids = (UUID[])((ComArray)((ComPointer)strukt.GetMember(1)).Referent).ArrayInstance;

                                // now get the hresults and only those IIDs are supported which have 0x00000000
                                // in our case IUnknown will always be supported (naturally) where as IDispatch may or may not be.
                                var hresults = (int[])((ComArray)((ComPointer)strukt.GetMember(2)).Referent).ArrayInstance;
                                for (var j = 0; j < hresults.Length; j++) {
                                    if (hresults[j] == 0x00000000) {
                                        // pointer exists
                                        // if it is Disp IID then set dual stuff else it has to be IUnknown, save it.
                                        if (iids[j].ToString().Equals(Interfaces.IID_IUnknown, StringComparison.CurrentCultureIgnoreCase)) {
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
                                            DispRefs = ((StdObjRef)ptr.GetObjectReference(InterfacePointer.OBJREF_STANDARD)).PublicRefs;
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

            internal Struct DecodeStruct(Struct strukt, NdrCodec ndr) {
                var context = new CodecContext();
                strukt = strukt.Decode(ndr, context);
                context.DecodeDeferredPointers(ndr);
                return strukt;
            }

            public bool ActivationSuccessful { get; private set; }

            public DualStringArray DualStringArrayForOxid { get; private set; }

            public InterfacePointer MInterfacePointer { get; private set; }

            public string IPID { get; private set; }

            public bool Dual { get; private set; }

            public string DispIpid { get; set; }

            public int DispRefs { get; private set; } = 5;
        }
    }
}