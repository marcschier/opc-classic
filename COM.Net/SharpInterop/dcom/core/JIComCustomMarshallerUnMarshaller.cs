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
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;

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
		protected JIComCustomMarshallerUnMarshaller(string clsid, IJIComObject comObject) :
            this(clsid, comObject, false) {
        }

        /// <summary>
        /// Create marshaller
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="comObject"></param>
        /// <param name="isTemplate"></param>
		protected JIComCustomMarshallerUnMarshaller(string clsid,
            IJIComObject comObject, bool isTemplate) {
            CLSID = clsid;
            if (isTemplate) {
                _me = new JIComObjectImpl(comObject.AssociatedSession,
                    comObject.Internal_getInterfacePointer());
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
        /// <param name="flag"> </param>
        public abstract void Encode(NdrCodec ndr, List<object> defferedPointers, int flag);

        /// <summary>
        /// Implement for custom decoding. Called by the framework.
        /// </summary>
        /// <param name="newMe"></param>
        /// <param name="ndr"> </param>
        /// <param name="defferedPointers"> </param>
        /// <param name="flag"> </param>
        /// <param name="additionalData">
        /// </param>
        public abstract JIComCustomMarshallerUnMarshaller Decode(IJIComObject newMe,
            NdrCodec ndr, List<object> defferedPointers, int flag,
            IDictionary<object, object> additionalData);

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        protected void Serialize(NdrCodec ndr, Type c,
            object value, List<object> defferedPointers, int flag) {
            JIMarshalUnMarshalHelper.Serialize(ndr, c, value, defferedPointers, flag);
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="obj"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        protected virtual object Deserialize(NdrCodec ndr, object obj,
            List<object> defferedPointers, int flag, IDictionary<object, object> additionalData) {
            return JIMarshalUnMarshalHelper.Deserialize(ndr, obj, defferedPointers,
                flag, additionalData);
        }

        /// <summary>
        /// Length in bytes
        /// </summary>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        protected static int GetLengthInBytes(Type c, object obj, int flag) {
            return JIMarshalUnMarshalHelper.GetLengthInBytes(c, obj, flag);
        }

        private readonly IJIComObject _me;
    }
}