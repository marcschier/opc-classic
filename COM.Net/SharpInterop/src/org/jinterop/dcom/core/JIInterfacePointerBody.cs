// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 
namespace org.jinterop.dcom.core {
    using ndr;
    using org.jinterop.dcom.common;
    using System;

    /// <summary>
    /// Interface pointer body
    /// </summary>
    [Serializable]
    internal class JIInterfacePointerBody {

        /// <summary>
        /// Create body
        /// </summary>
        private JIInterfacePointerBody() {}

        /// <summary>
        /// Custom object
        /// </summary>
        internal virtual bool CustomObjRef => _objectType == JIInterfacePointer.OBJREF_CUSTOM;

        /// <summary>
        /// Custom class id
        /// </summary>
        internal virtual string CustomCLSID => _customCLSID;

        /// <summary>
        /// Object type
        /// </summary>
        internal virtual int ObjectType => _objectType;

        /// <summary>
        /// Returns object reference
        /// </summary>
        /// <param name="type"></param>
        /// <returns>object reference</returns>
        internal virtual object getObjectReference(int type) {
            if (type == JIInterfacePointer.OBJREF_STANDARD) {
                return _stdObjRef;
            }
            return null;
        }

        /// <summary>
        /// Returns the Interface Identifier for this MIP.
        /// </summary>
        /// <returns> String representation of 128 bit uuid. </returns>
        internal virtual string IID => _iid;

        /// <summary>
        /// Ip id
        /// </summary>
        internal virtual string IPID => _stdObjRef.Ipid;

        /// <summary>
        /// Oid
        /// </summary>
        internal virtual sbyte[] OID => _stdObjRef.ObjectId;

        /// <summary>
        /// String bindings
        /// </summary>
        internal virtual JIDualStringArray StringBindings => _resolverAddr;

        /// <summary>
        /// Length
        /// </summary>
        internal virtual int Length => _length;

        /// <summary>
        /// Called from Oxid Resolver master, the resolver address are put in here itself
        /// </summary>
        /// <param name="iid"> </param>
        /// <param name="port"></param>
        /// <param name="objref"></param>
        internal JIInterfacePointerBody(string iid, int port, JIStdObjRef objref) {
            _iid = iid;
            _stdObjRef = objref;
            _port = port;
            _resolverAddr = new JIDualStringArray(port);
            _length = 40 + 4 + 4 + 16 + _resolverAddr.Length;
        }

        /// <summary>
        /// Create body
        /// </summary>
        /// <param name="iid"></param>
        /// <param name="interfacePointer"></param>
        internal JIInterfacePointerBody(string iid, JIInterfacePointer interfacePointer) {
            _iid = iid;
            _stdObjRef = (JIStdObjRef)interfacePointer.getObjectReference(JIInterfacePointer.OBJREF_STANDARD);
            _resolverAddr = interfacePointer.StringBindings;
            _length = 40 + 4 + 4 + 16 + _resolverAddr.Length;
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="Flags"></param>
        /// <returns></returns>
        internal static JIInterfacePointerBody decode(NetworkDataRepresentation ndr, int Flags) {
            if ((Flags & JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) == 
                         JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) {
                return decode2(ndr);
            }

            var length = ndr.readUnsignedLong();
            ndr.readUnsignedLong(); //length

            var ptr = new JIInterfacePointerBody {
                _length = length
            };
            //check for MEOW
            var b = new sbyte[4];
            ndr.readOctetArray(b, 0, 4);

            var i = 0;
            while (i != 4) {
                //not MEOW then what ?
                if (b[i] != JIInterfacePointer.OBJREF_SIGNATURE[i]) {
                    return null;
                }
                i++;
            }

            //TODO only STDOBJREF supported for now

            if ((ptr._objectType = ndr.readUnsignedLong()) != JIInterfacePointer.OBJREF_STANDARD) {
                try {
                    var ipid2 = new rpc.core.UUID();
                    ipid2.decode(ndr, ndr.Buffer);
                    ptr._iid = ipid2.ToString();
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "JIInterfacePointer", "decode", e);
                }

                //now for CLSID 
                try {
                    var ipid2 = new rpc.core.UUID();
                    ipid2.decode(ndr, ndr.Buffer);
                    ptr._customCLSID = ipid2.ToString();
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "JIInterfacePointer", "decode", e);
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
                var ipid2 = new rpc.core.UUID();
                ipid2.decode(ndr, ndr.Buffer);
                ptr._iid = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIInterfacePointer", "decode", e);
            }

            ptr._stdObjRef = JIStdObjRef.decode(ndr);
            ptr._resolverAddr = JIDualStringArray.decode(ndr);
            return ptr;
        }


        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal static JIInterfacePointerBody decode2(NetworkDataRepresentation ndr) {
            var ptr = new JIInterfacePointerBody();

            //check for MEOW
            var b = new sbyte[4];
            ndr.readOctetArray(b, 0, 4);

            var i = 0;
            while (i != 4) {
                //not MEOW then what ?
                if (b[i] != JIInterfacePointer.OBJREF_SIGNATURE[i]) {
                    return null;
                }
                i++;
            }

            //TODO only STDOBJREF supported for now
            if ((ptr._objectType = ndr.readUnsignedLong()) != JIInterfacePointer.OBJREF_STANDARD) {
                return null;
            }

            try {
                var ipid2 = new rpc.core.UUID();
                ipid2.decode(ndr, ndr.Buffer);
                ptr._iid = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIInterfacePointer", "decode", e);
            }

            ptr._stdObjRef = JIStdObjRef.decode(ndr);
            ptr._resolverAddr = JIDualStringArray.decode(ndr);
            return ptr;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="flags"></param>
        internal virtual void encode(NetworkDataRepresentation ndr, int flags) {

            //now for length
            //the length for STDOBJREF is fixed 40 bytes : 4,4,8,8,16.
            //Dual string array has to be computed, since that can vary. MEOW = 4., flag stdobjref = 4
            // + 16 bytes of ipid
            var length = 0;
            if (!CustomObjRef) {
                length = 40 + 4 + 4 + 16 + _resolverAddr.Length;
            }

            ndr.writeUnsignedLong(length);
            ndr.writeUnsignedLong(length);

            //for OBJREF_CUSTOM we will correct this length after the custom object has been marshalled.
            //this object is marshalled 4 + 4 + 40 bytes after this point. The length of the length itself is not included. 

            ndr.writeOctetArray(JIInterfacePointer.OBJREF_SIGNATURE, 0, 4);

            if (CustomObjRef) {
                ndr.writeUnsignedLong(JIInterfacePointer.OBJREF_CUSTOM);
                try {
                    var ipid2 = new rpc.core.UUID(_iid);
                    ipid2.encode(ndr, ndr.Buffer);
                    ipid2 = new rpc.core.UUID(_customCLSID);
                    ipid2.encode(ndr, ndr.Buffer);
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
                var ipid2 = new rpc.core.UUID(_iid);

                if ((flags & JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) == JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) {
                    ipid2 = new rpc.core.UUID(JiIUnknown.IID);
                }
                else if ((flags & JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) == JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) {
                    ipid2 = new rpc.core.UUID(impls.automation.IJIDispatch_Fields.IID);
                }

                ipid2.encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            _stdObjRef.encode(ndr);

            _resolverAddr.encode(ndr);
        }

        private string _iid;
        private string _customCLSID;
        private int _objectType = -1;
        private JIStdObjRef _stdObjRef;
        private int _length = -1;
        private JIDualStringArray _resolverAddr;
        private readonly int _port = -1; //to be used when doing local resolution.
    }
}