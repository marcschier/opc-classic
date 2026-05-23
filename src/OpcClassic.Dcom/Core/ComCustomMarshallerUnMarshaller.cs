//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using OpcClassic.Dcom.Internal.LegacyNdr;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Must be implemented by Classes providing marshall, unmarshall support
    /// for OBJREF_CUSTOM.
    /// </summary>
    public abstract class ComCustomMarshallerUnMarshaller {

        /// <summary>
        /// Clsid
        /// </summary>
        public string CLSID { get; }

        /// <summary>
        /// Create marshaller
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="comObject"></param>
        protected ComCustomMarshallerUnMarshaller(string clsid, IComObject comObject) :
            this(clsid, comObject, false) {
        }

        /// <summary>
        /// Create marshaller
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="comObject"></param>
        /// <param name="isTemplate"></param>
        protected ComCustomMarshallerUnMarshaller(string clsid,
            IComObject comObject, bool isTemplate) {
            CLSID = clsid;
            if (isTemplate) {
                ComObject = new ComObjectImpl(comObject.AssociatedSession,
                    ((IComObjectInternal)comObject).GetInterfacePointer());

                ((ComObjectImpl)ComObject).CustomObject = this;
            }
            else {
                ComObject = comObject;
            }
        }

        /// <summary>
        /// Me
        /// </summary>
        public IComObject ComObject { get; }

        /// <summary>
        /// Implement for custom encoding. Called by the framework.
        /// </summary>
        /// <param name="ndr"> </param>
        /// <param name="context"> </param>
        public abstract void Encode(NdrCodec ndr,CodecContext context);

        /// <summary>
        /// Implement for custom decoding. Called by the framework.
        /// </summary>
        /// <param name="newMe"></param>
        /// <param name="ndr"> </param>
        /// <param name="context"> </param>
        /// <returns></returns>
        public abstract ComCustomMarshallerUnMarshaller Decode(IComObject newMe,
            NdrCodec ndr, CodecContext context);

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <param name="context"></param>
        protected void Serialize(NdrCodec ndr, Type c, object value, CodecContext context) =>
            MarshalUnMarshalHelper.Serialize(ndr, c, value, context);

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected object Deserialize(NdrCodec ndr, object obj, CodecContext context) =>
            MarshalUnMarshalHelper.Deserialize(ndr, obj, context);

        /// <summary>
        /// Length in bytes
        /// </summary>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        protected static int GetLengthInBytes(Type c, object obj, int flag = InteropFlags.FLAG_NULL) =>
            MarshalUnMarshalHelper.GetLengthInBytes(c, obj, flag);
    }
}