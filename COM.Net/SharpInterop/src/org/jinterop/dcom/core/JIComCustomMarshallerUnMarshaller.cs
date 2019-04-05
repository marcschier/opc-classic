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
    using System.Collections;

    /// <summary>
    /// Must be implemented by Classes providing marshall, unmarshall support
    /// for OBJREF_CUSTOM.
    /// </summary>
    public abstract class JIComCustomMarshallerUnMarshaller {

        /// <summary>
        /// Clsid
        /// </summary>
        public string CLSID { get; }


        /// <summary>
        /// Create marshaller
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="comObject"></param>
		public JIComCustomMarshallerUnMarshaller(string clsid, IJIComObject comObject) :
            this(clsid, comObject, false) {
        }

        /// <summary>
        /// Create marshaller
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="comObject"></param>
        /// <param name="isTemplate"></param>
		public JIComCustomMarshallerUnMarshaller(string clsid, IJIComObject comObject, bool isTemplate) {
            CLSID = clsid;
            if (isTemplate) {
                _me = new JIComObjectImpl(comObject.AssociatedSession, comObject.internal_getInterfacePointer());
                ((JIComObjectImpl)_me).CustomObject = this;
            }
            else {
                _me = comObject;
            }
        }

        /// <summary>
        /// Me
        /// </summary>
        public virtual IJIComObject ComObject => _me;

        /// <summary>
        /// Implement for custom encoding. Called by the framework.
        /// </summary>
        /// <param name="ndr"> </param>
        /// <param name="defferedPointers"> </param>
        /// <param name="FLAG"> </param>
        public abstract void encode(NdrCodec ndr, IList defferedPointers, int FLAG);

        /// <summary>
        /// Implement for custom decoding. Called by the framework. 
        /// </summary>
        /// <param name="newMe"></param>
        /// <param name="ndr"> </param>
        /// <param name="defferedPointers"> </param>
        /// <param name="FLAG"> </param>
        /// <param name="additionalData">
        /// </param>
        public abstract JIComCustomMarshallerUnMarshaller decode(IJIComObject newMe, 
            NdrCodec ndr, IList defferedPointers, int FLAG, IDictionary additionalData);


        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        protected internal virtual void serialize(NdrCodec ndr, Type c,
            object value, IList defferedPointers, int FLAG) {
            JIMarshalUnMarshalHelper.serialize(ndr, c, value, defferedPointers, FLAG);
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="obj"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        protected internal virtual object deSerialize(NdrCodec ndr, object obj,
            IList defferedPointers, int FLAG, IDictionary additionalData) {
            return JIMarshalUnMarshalHelper.deSerialize(ndr, obj, defferedPointers, FLAG, additionalData);
        }

        /// <summary>
        /// Length in bytes
        /// </summary>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        /// <param name="FLAG"></param>
        /// <returns></returns>
        protected internal static int getLengthInBytes(Type c, object obj, int FLAG) {
            return JIMarshalUnMarshalHelper.getLengthInBytes(c, obj, FLAG);
        }

        private readonly IJIComObject _me;
    }
}