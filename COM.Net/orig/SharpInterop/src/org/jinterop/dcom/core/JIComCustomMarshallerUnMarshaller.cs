using System;
using System.Collections;

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


    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    /// <summary>
    /// Must be implemented by Classes providing marshall, unmarshall support
    /// for OBJREF_CUSTOM.
    /// 
    /// @author vikram
    /// 
    /// </summary>
    public abstract class JIComCustomMarshallerUnMarshaller {

        public readonly string CLSID;
        private readonly IJIComObject Me;
        public JIComCustomMarshallerUnMarshaller(string CLSID, IJIComObject comObject) : this(CLSID, comObject, false) {
        }

        public JIComCustomMarshallerUnMarshaller(string CLSID, IJIComObject comObject, bool isTemplate) {
            this.CLSID = CLSID;
            if (isTemplate) {
                Me = new JIComObjectImpl(comObject.AssociatedSession, comObject.Internal_getInterfacePointer());
                ((JIComObjectImpl)Me).CustomObject = this;
            }
            else {
                Me = comObject;
            }
        }


        public virtual IJIComObject ComObject {
            get {
                return Me;
            }
        }

        /// <summary>
        /// Implement for custom encoding. Called by the framework.
        /// </summary>
        /// <param name="ndr"> </param>
        /// <param name="defferedPointers"> </param>
        /// <param name="FLAG"> </param>
        public abstract void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG);

        /// <summary>
        /// Implement for custom decoding. Called by the framework. 
        /// </summary>
        /// <param name="ndr"> </param>
        /// <param name="defferedPointers"> </param>
        /// <param name="FLAG"> </param>
        /// <param name="additionalData">
        /// @return </param>
        public abstract JIComCustomMarshallerUnMarshaller Decode(IJIComObject newMe, NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData);

        public virtual void Serialize(NetworkDataRepresentation ndr, Type c, object value, IList defferedPointers, int FLAG) {
            JIMarshalUnMarshalHelper.Serialize(ndr, c, value, defferedPointers, FLAG);
        }

        public virtual object DeSerialize(NetworkDataRepresentation ndr, object obj, IList defferedPointers, int FLAG, IDictionary additionalData) {
            return JIMarshalUnMarshalHelper.DeSerialize(ndr, obj, defferedPointers, FLAG, additionalData);
        }

        protected internal static int GetLengthInBytes(Type c, object obj, int FLAG) {
            return JIMarshalUnMarshalHelper.GetLengthInBytes(c, obj, FLAG);
        }

    }

}