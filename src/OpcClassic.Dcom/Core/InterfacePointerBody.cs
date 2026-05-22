//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using SharpCifs.Dcerpc.Ndr;
    using OpcClassic.Dcom.Internal;
    using System;

    /// <summary>
    /// Interface pointer body
    /// </summary>
    [Serializable]
    internal class InterfacePointerBody {

        /// <summary>
        /// Create body
        /// </summary>
        private InterfacePointerBody() {}

        /// <summary>
        /// Custom object
        /// </summary>
        internal bool CustomObjRef => ObjectType == InterfacePointer.OBJREF_CUSTOM;

        /// <summary>
        /// Custom class id
        /// </summary>
        internal string CustomCLSID { get; private set; }

        /// <summary>
        /// Object type
        /// </summary>
        internal int ObjectType { get; private set; } = -1;

        /// <summary>
        /// Returns object reference
        /// </summary>
        /// <param name="type"></param>
        /// <returns>object reference</returns>
        internal object GetObjectReference(int type) {
            if (type == InterfacePointer.OBJREF_STANDARD) {
                return _stdObjRef;
            }
            return null;
        }

        /// <summary>
        /// Returns the Interface Identifier for this MIP.
        /// </summary>
        /// <returns> String representation of 128 bit uuid. </returns>
        internal string IID { get; private set; }

        /// <summary>
        /// Ip id
        /// </summary>
        internal string IPID => _stdObjRef.Ipid;

        /// <summary>
        /// Oid
        /// </summary>
        internal byte[] OID => _stdObjRef.ObjectId;

        /// <summary>
        /// String bindings
        /// </summary>
        internal DualStringArray StringBindings { get; private set; }

        /// <summary>
        /// Length
        /// </summary>
        internal int Length { get; private set; } = -1;

        /// <summary>
        /// Called from Oxid Resolver master, the resolver address are put in here itself
        /// </summary>
        /// <param name="iid"> </param>
        /// <param name="port"></param>
        /// <param name="objref"></param>
        internal InterfacePointerBody(string iid, int port, StdObjRef objref) {
            IID = iid;
            _stdObjRef = objref;
            _port = port;
            StringBindings = new DualStringArray(port);
            Length = 40 + 4 + 4 + 16 + StringBindings.Length;
        }

        /// <summary>
        /// Create body
        /// </summary>
        /// <param name="iid"></param>
        /// <param name="interfacePointer"></param>
        internal InterfacePointerBody(string iid, InterfacePointer interfacePointer) {
            IID = iid;
            _stdObjRef = (StdObjRef)interfacePointer.GetObjectReference(InterfacePointer.OBJREF_STANDARD);
            StringBindings = interfacePointer.StringBindings;
            Length = 40 + 4 + 4 + 16 + StringBindings.Length;
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="Flags"></param>
        /// <returns></returns>
        internal static InterfacePointerBody Decode(NdrCodec ndr, int Flags) {
            if ((Flags & InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) ==
                         InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) {
                return Decode2(ndr);
            }

            var length = ndr.ReadUnsignedLong();
            ndr.ReadUnsignedLong(); // length

            var ptr = new InterfacePointerBody {
                Length = length
            };
            // check for MEOW
            var b = new byte[4];
            ndr.ReadOctetArray(b, 0, 4);

            var i = 0;
            while (i != 4) {
                // not MEOW then what ?
                if (b[i] != InterfacePointer.OBJREF_SIGNATURE[i]) {
                    return null;
                }
                i++;
            }

            // TODO only STDOBJREF supported for now

            if ((ptr.ObjectType = ndr.ReadUnsignedLong()) != InterfacePointer.OBJREF_STANDARD) {
                try {
                    var ipid2 = new SharpInterop.Rpc.Core.UUID();
                    ipid2.Decode(ndr, ndr.Buffer);
                    ptr.IID = ipid2.ToString();
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "InterfacePointer decode");
                }

                // now for CLSID
                try {
                    var ipid2 = new SharpInterop.Rpc.Core.UUID();
                    ipid2.Decode(ndr, ndr.Buffer);
                    ptr.CustomCLSID = ipid2.ToString();
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "InterfacePointer decode");
                }

                // extension
                ndr.ReadUnsignedLong();

                // reserved
                ndr.ReadUnsignedLong();

                return ptr;
            }

            try {
                var ipid2 = new SharpInterop.Rpc.Core.UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                ptr.IID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "InterfacePointer decode");
            }

            ptr._stdObjRef = StdObjRef.Decode(ndr);
            ptr.StringBindings = DualStringArray.Decode(ndr);
            return ptr;
        }


        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal static InterfacePointerBody Decode2(NdrCodec ndr) {
            var ptr = new InterfacePointerBody();

            // check for MEOW
            var b = new byte[4];
            ndr.ReadOctetArray(b, 0, 4);

            var i = 0;
            while (i != 4) {
                // not MEOW then what ?
                if (b[i] != InterfacePointer.OBJREF_SIGNATURE[i]) {
                    return null;
                }
                i++;
            }

            // TODO only STDOBJREF supported for now
            if ((ptr.ObjectType = ndr.ReadUnsignedLong()) != InterfacePointer.OBJREF_STANDARD) {
                return null;
            }

            try {
                var ipid2 = new SharpInterop.Rpc.Core.UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                ptr.IID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "InterfacePointer decode");
            }

            ptr._stdObjRef = StdObjRef.Decode(ndr);
            ptr.StringBindings = DualStringArray.Decode(ndr);
            return ptr;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="flags"></param>
        internal void Encode(NdrCodec ndr, int flags) {

            // now for length
            // the length for STDOBJREF is fixed 40 bytes : 4,4,8,8,16.
            // Dual string array has to be computed, since that can vary. MEOW = 4., flag stdobjref = 4
            // + 16 bytes of ipid
            var length = 0;
            if (!CustomObjRef) {
                length = 40 + 4 + 4 + 16 + StringBindings.Length;
            }

            ndr.WriteUnsignedLong(length);
            ndr.WriteUnsignedLong(length);

            // for OBJREF_CUSTOM we will correct this length after the custom object has been marshalled.
            // this object is marshalled 4 + 4 + 40 bytes after this point. The length of the length itself is not included.

            ndr.WriteOctetArray(InterfacePointer.OBJREF_SIGNATURE, 0, 4);

            if (CustomObjRef) {
                ndr.WriteUnsignedLong(InterfacePointer.OBJREF_CUSTOM);
                try {
                    var ipid2 = new SharpInterop.Rpc.Core.UUID(IID);
                    ipid2.Encode(ndr, ndr.Buffer);
                    ipid2 = new SharpInterop.Rpc.Core.UUID(CustomCLSID);
                    ipid2.Encode(ndr, ndr.Buffer);
                    ndr.WriteUnsignedLong(0); // extension
                    ndr.WriteUnsignedLong(0); // reserved, now the spec say that this is ignored by the server but the
                                              // the WMIO marshaller puts the length of the entire buffer here. If this is the case then we will have to go
                                              // 4 bytes back and rewrite this with total lengths in the custom marshaller.
                }
                catch (NdrException e) {
                    // TODO Auto-generated catch block
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                return; // rest will be filled by the Custom Marshaller.
            }

            // std ref
            ndr.WriteUnsignedLong(InterfacePointer.SORF_OXRES1);

            try {
                var ipid2 = new SharpInterop.Rpc.Core.UUID(IID);
                if ((flags & InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) ==
                             InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) {
                    ipid2 = new SharpInterop.Rpc.Core.UUID(Interfaces.IID_IUnknown);
                }
                else if ((flags & InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) ==
                                  InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) {
                    ipid2 = new SharpInterop.Rpc.Core.UUID(Interfaces.IID_IDispatch);
                }
                ipid2.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            _stdObjRef.Encode(ndr);

            StringBindings.Encode(ndr);
        }

        private StdObjRef _stdObjRef;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly int _port = -1; // to be used when doing local resolution.
#pragma warning restore IDE0052 // Remove unread private members
    }
}