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
    using org.jinterop.dcom.common;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Representation of a COM pointer.
    /// </summary>
    [Serializable]
    public sealed class JIPointer {

        /// <summary>
        /// Create pointer
        /// </summary>
        private JIPointer() {}

        /// <summary>
        /// Creates an instance of this class where the referent is <code>value</code>.
        /// Used when serializing this pointer. This pointer is <b>not</b> of reference type.
        /// </summary>
        /// <param name="value"> </param>
        public JIPointer(object value) : this(value, false) {
        }

        /// <summary>
        /// Creates an instance of this class where the referent is of the type
        /// <code>value</code>. Used when deserializing this pointer.
        /// </summary>
        /// <param name="value"> <code>null</code> is acceptable </param>
        /// <param name="isReferenceTypePtr"> <code>true</code> if a referent identifier
        /// will not precede this ptr. </param>
        public JIPointer(Type value, bool isReferenceTypePtr) {
            //null pointer.
            if (value == null) {
                value = typeof(int?);
                isReferenceTypePtr = true;
                IsNull = true;
            }
            //Should not defer since the enclosing struct,union,array will defer it by itself
            // this is important since , ptr to a ptr to a ptr (and more) will need to
            //deserialize completely after the first deferement i.e they are not further deffered.
            _referent = value;
            Reference = isReferenceTypePtr;
        }

        /// <summary>
        /// Some COM servers send referentId (pointer) as null but the referent is not.
        /// To be used only when you know this is the case. Better leave it unsed.
        /// </summary>
        public void TreatNullSpecially() {
            _nullSpecial = true;
        }

        /// <summary>
        /// Creates an instance of this class where the referent is <code>value</code>.
        ///  Used when serializing this pointer.
        /// </summary>
        /// <param name="value"> <code>null</code> is acceptable </param>
        /// <param name="isReferenceTypePtr"> <code>true</code> if a referent Identifier
        /// will not precede this ptr. </param>
        public JIPointer(object value, bool isReferenceTypePtr) {
            if (value == null) {
                //since a null is being sent for a pointer, it has to be shown as 0x0.
                value = 0;
                isReferenceTypePtr = true;
                IsNull = true;
            }

            //		if (value.getClass().equals(JIArray.class))
            //		{
            //			if (((JIArray)value).getDimensions() > 1)
            //				throw new System.ArgumentException("Only single dimension arrays accepted");
            //		}

            //Should not defer since the enclosing struct,union,array will defer it by itself
            // this is important since , ptr to a ptr to a ptr (and more) will need to
            //deserialize completely after the first deferement i.e they are not further deffered.

            _referent = value;
            _referentId = new object().GetHashCode();
            Reference = isReferenceTypePtr;
        }

        /// <summary>
        /// Sets the flags associated with the referent.
        /// </summary>
        internal int Flags {
            set => _flags = value;
        }

        /// <summary>
        /// Set reference type pointer
        /// </summary>
        internal void SetIsReferenceTypePtr() {
            Reference = true;
        }

        /// <summary>
        /// Returns the referent encapsulated by this pointer.
        /// </summary>
        public object GetReferent() {
            return IsNull ? null : _referent;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        internal void Encode(NdrCodec ndr, List<object> defferedPointers, int flag) {

            flag |= _flags;
            if (IsNull) {
                JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, defferedPointers, flag);
                return;
            }
            //it is deffered or part of an array, this logic will not get called twice since the
            //deffered list will come in withb FLAG_NULL
            if (!IsNull && (Deffered || (flag & JIFlags.FLAG_REPRESENTATION_ARRAY) == JIFlags.FLAG_REPRESENTATION_ARRAY)) /*||
						(flag & JIFlags.FLAG_REPRESENTATION_NESTED_POINTER ) == JIFlags.FLAG_REPRESENTATION_NESTED_POINTER*/
            {
                var referentIdToPut = _referentId == -1 ? _referent.GetHashCode() : _referentId;
                JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), referentIdToPut, defferedPointers, flag);
                Deffered = false;
                Reference = true;
                //			try{
                defferedPointers.Add(this);
                //			}catch(NullPointerException e)
                //			{
                //				int ni = 0;
                //			}
                return;
            }

            if (!IsNull && !Reference) {
                var referentIdToPut = _referentId == -1 ? _referent.GetHashCode() : _referentId;
                JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), referentIdToPut, defferedPointers, flag);
            }

            try {
                if (!IsNull && _referent.GetType().Equals(typeof(JIVariant)) && ((JIVariant)_referent).IsArray) {
                    //write the length first before all elements
                    //ndr.writeUnsignedLong(((Object[])(((JIVariant)referent).getObject())).length);
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), ((object[])((JIVariant)_referent).Object).Length, defferedPointers, flag);
                }
            }
            catch (JIException e) {
                throw new JIRuntimeException(e.ErrorCode);
            }

            JIMarshalUnMarshalHelper.Serialize(ndr, _referent.GetType(), _referent, defferedPointers, flag);
        }


        /// <summary>
        /// class of type being decoded. If the type being expected is an array , the varType
        /// should be the actual array type and not JIArray.
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal JIPointer Decode(NdrCodec ndr, List<object> defferedPointers, int flag,
            IDictionary<object, object> additionalData) {
            //shallowClone();
            flag |= _flags;

            var retVal = new JIPointer {
                Flags = _flags,
                IsNull = IsNull,
                _nullSpecial = _nullSpecial
            };

            //retVal.isDeffered = isDeffered;
            if (Deffered || (flag & JIFlags.FLAG_REPRESENTATION_ARRAY) == JIFlags.FLAG_REPRESENTATION_ARRAY)
            /*|| (flag & JIFlags.FLAG_REPRESENTATION_NESTED_POINTER ) == JIFlags.FLAG_REPRESENTATION_NESTED_POINTER */
            {
                retVal._referentId = (int)(int?)JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(int?),
                    defferedPointers, flag, additionalData);
                retVal._referent = _referent; //will only be the class or object
                if (retVal._referentId == 0 && !_nullSpecial) {
                    //null pointer
                    // just return
                    retVal.IsNull = true;
                    retVal.Deffered = false;
                    return retVal;
                }

                retVal.Deffered = false;
                retVal.Reference = true;
                defferedPointers.Add(retVal);
                return retVal;
            }

            if (!Reference) {
                //referentId = ndr.readUnsignedLong();
                retVal._referentId = (int)(int?)JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(int?),
                    defferedPointers, flag, additionalData);
                retVal._referent = _referent; //will only be the class or object
                if (retVal._referentId == 0 && !_nullSpecial) {
                    //null pointer
                    // just return
                    retVal.IsNull = true;
                    return retVal;
                }
            }
            retVal._referent = JIMarshalUnMarshalHelper.Deserialize(ndr, _referent,
                defferedPointers, flag, additionalData);
            return retVal;
        }

        /// <summary>
        /// Deferred
        /// </summary>
        internal bool Deffered { set; get; } = false;

        /// <summary>
        /// Set referent id
        /// </summary>
        /// <param name="value"></param>
        internal void SetReferent(int value) {
            _referentId = value;
        }

        /// <summary>
        /// Returns the referent identifier.
        /// </summary>
        public int? ReferentIdentifier => _referentId;

        /// <summary>
        /// Returns status whether this is a reference type pointer or not.
        /// </summary>
        /// <returns> <code>true</code> if this is a reference type pointer. </returns>
        public bool Reference { get; private set; } = false;

        /// <summary>
        /// Length
        /// </summary>
        internal int Length {
            get {
                //4 for pointer
                if (IsNull) {
                    return kPointerSize;
                }
                if (_referent is Type) {
                    return 4 + JIMarshalUnMarshalHelper.GetLengthInBytes((Type)_referent,
                        _referent, JIFlags.FLAG_NULL);
                }
                return 4 + JIMarshalUnMarshalHelper.GetLengthInBytes(_referent.GetType(),
                    _referent, JIFlags.FLAG_NULL);
            }
        }

        /// <summary>
        /// Internal replace
        /// </summary>
        /// <param name="replacement"></param>
        internal void ReplaceSelfWithNewPointer(JIPointer replacement) {
            Deffered = replacement.Deffered;
            IsNull = replacement.IsNull;
            Reference = replacement.Reference;
            _referent = replacement._referent;
        }

        /// <summary>
        /// Returns status if this pointer is <code>null</code>.
        /// </summary>
        /// <returns> <code>true</code> if the pointer is <code>null</code>. </returns>
        public bool IsNull { get; private set; }

        /// <summary>
        /// Set value
        /// </summary>
        internal object Value {
            set => _referent = value;
        }

        /// <inheritdoc/>
        public override string ToString() {
            return _referent == null ? "[null]" : "[" + _referent.ToString() + "]";
        }

        private const int kPointerSize = 4;
        private bool _nullSpecial;
        private object _referent;
        private int _referentId = -1;
        private int _flags;
    }
}