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
    using System;
    using System.Collections.Generic;
    using SharpCifs.Util.Sharpen;
    using System.Linq;

    /// <summary>
    /// Class representing a Marshalled Interface Pointer. You will never use the members of this
    /// class directly, but always as an implementation of <code>IJIComObject</code> interface.
    /// Sample Usage:-
    /// <code>
    /// IJIComObject connectionPointContainer = (IJIComObject)ieObject.queryInterface("B196B284-BAB4-101A-B69C-00AA00341D07");
    /// JICallBuilder object = new JICallBuilder(connectionPointContainer.getIpid(),true);
    /// object.setOpnum(1);
    /// object.addInParamAsUUID("34A715A0-6587-11D0-924A-0020AFC7AC4D",JIFlags.FLAG_NULL);
    /// object.addOutParamAsObject(JIInterfacePointer.class,JIFlags.FLAG_NULL);
    /// Object[] objects = (Object[])connectionPointContainer.call(object); //find connection point
    /// JIInterfacePointer connectionPtr = (JIInterfacePointer)objects[0];
    /// IJIComObject connectionPointer = JIObjectFactory.createCOMInstance(connectionPointContainer,connectionPtr);
    /// </code>
    /// </summary>
    [Serializable]
    public /*internal*/ sealed class JIInterfacePointer {

        /// <summary>
        /// Custom object
        /// </summary>
        internal bool CustomObjRef => ((JIInterfacePointerBody)_member.GetReferent())
            .CustomObjRef;

        /// <summary>
        /// Custom class id
        /// </summary>
        internal string CustomCLSID => ((JIInterfacePointerBody)_member.GetReferent())
            .CustomCLSID;

        /// <summary>
        /// Deferred
        /// </summary>
        internal bool Deffered {
            set => _member.Deffered = true;
        }

        /// <summary>
        /// Object type
        /// </summary>
        internal int ObjectType => ((JIInterfacePointerBody)_member.GetReferent()).ObjectType;

        /// <summary>
        /// Object reference of specified type
        /// </summary>
        /// <param name="objectType"></param>
        internal object GetObjectReference(int objectType) {
            return ((JIInterfacePointerBody)_member.GetReferent()).GetObjectReference(objectType);
        }

        /// <summary>
        /// Returns the Interface Identifier for this MIP.
        /// </summary>
        public string IID => ((JIInterfacePointerBody)_member.GetReferent()).IID;

        /// <summary>
        /// IP Id
        /// </summary>
        public string IPID => ((JIInterfacePointerBody)_member.GetReferent()).IPID;

        /// <summary>
        /// Oid
        /// </summary>
        public byte[] OID => ((JIStdObjRef)((JIInterfacePointerBody)_member.GetReferent())
            .GetObjectReference(OBJREF_STANDARD)).ObjectId;

        /// <summary>
        /// Oxid
        /// </summary>
        internal byte[] OXID => ((JIStdObjRef)((JIInterfacePointerBody)_member.GetReferent())
            .GetObjectReference(OBJREF_STANDARD)).Oxid;

        /// <summary>
        /// String bindings
        /// </summary>
        internal JIDualStringArray StringBindings => ((JIInterfacePointerBody)_member.GetReferent())
            .StringBindings;

        /// <summary>
        /// Length
        /// </summary>
        internal int Length => ((JIInterfacePointerBody)_member.GetReferent()).Length;

        /// <summary>
        /// Hidden constructor
        /// </summary>
        private JIInterfacePointer() { }

        /// <summary>
        /// Called from Oxid Resolver master, the resolver address are put in here itself
        /// </summary>
        /// <param name="iid"> </param>
        /// <param name="port"></param>
        /// <param name="objref"></param>
        internal JIInterfacePointer(string iid, int port, JIStdObjRef objref) {
            _member = new JIPointer(new JIInterfacePointerBody(iid, port, objref), false);
        }

        /// <summary>
        /// Create interface pointer
        /// </summary>
        /// <param name="iid"></param>
        /// <param name="interfacePointer"></param>
        internal JIInterfacePointer(string iid, JIInterfacePointer interfacePointer) {
            _member = new JIPointer(new JIInterfacePointerBody(iid, interfacePointer), false);
        }

        /// <inheritdoc/>
        public override string ToString() {
            var retVal = "JIInterfacePointer[IID:" + IID + " , ObjRef: " +
                GetObjectReference(OBJREF_STANDARD) + "]";
            return retVal;
        }

        /// <summary>
        /// Helper to compare to interface pointers
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsOxidEqual(JIInterfacePointer src, JIInterfacePointer target) {
            if (src == null) {
                throw new ArgumentNullException(nameof(src));
            }
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            return src.OXID.SequenceEqual(target.OXID);
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal static JIInterfacePointer Decode(NdrCodec ndr,
            List<object> defferedPointers, int FLAG, IDictionary<object, object> additionalData) {
            var ptr = new JIInterfacePointer();
            if ((FLAG & JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) ==
                        JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) {
                ptr._member = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr,
                    new JIPointer(typeof(JIInterfacePointerBody), true), defferedPointers, FLAG, additionalData);
            }
            else {
                ptr._member = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr,
                    new JIPointer(typeof(JIInterfacePointerBody)), defferedPointers, FLAG, additionalData);
            }
            //the pointer is null, no point of it's wrapper being present, so return null from here as well
            if (ptr._member.Null) {
                ptr = null;
            }
            return ptr;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        internal void Encode(NdrCodec ndr, List<object> defferedPointers, int FLAG) {
            if ((FLAG & JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT) ==
                        JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT) {
                //just encode a null.
                JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, defferedPointers, FLAG);
                return;
            }
            JIMarshalUnMarshalHelper.Serialize(ndr, _member.GetType(), _member, defferedPointers, FLAG);
        }

        internal static readonly byte[] OBJREF_SIGNATURE = { 0x4d, 0x45, 0x4f, 0x57 }; // 'MEOW'
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

        private JIPointer _member;
    }
}