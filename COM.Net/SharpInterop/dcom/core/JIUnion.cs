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
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class represents the <code>Union</code> data type.
    /// Its usage is dictated by the discriminant
    /// which acts as a "switch" to select the correct member
    /// to be serialized\deserialzed.
    /// Sample Usage :-
    /// <code>
    ///    JIUnion forTypeDesc = new JIUnion(typeof(short));
    ///    JIPointer ptrToTypeDesc = new JIPointer(typeDesc);
    ///    JIPointer ptrToArrayDesc = new JIPointer(arrayDesc);
    ///    forTypeDesc.AddMember(TypeDesc.VT_PTR,ptrToTypeDesc);
    ///    forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
    ///    forTypeDesc.AddMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
    ///    forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED,typeof(int));
    /// </code>
    /// The TypeDesc.VT_PTR is an <code>int</code> and is used
    /// as a discriminant to select ptrTypeDesc, TypeDesc.VT_CARRAY
    /// chooses ptrArrayDesc.
    /// </summary>
    [Serializable]
    public sealed class JIUnion {

        /// <summary>
        /// Returns the discriminant Vs there members Map.
        /// </summary>
        public Hashtable Members { get; } = new Hashtable();

        /// <summary>
        /// Private
        /// </summary>
        private JIUnion() {
        }

        /// <summary>
        /// Creates an object with discriminant type specified. Used only during deserializing
        /// the union. Can only be of the type <code>Integer</code>,<code>Short</code>,<code>Boolean</code>
        /// or <code>Character</code>.
        /// </summary>
        /// <param name="discriminantClass"></param>
        /// <exception cref="ArgumentException"> if the
        /// <code>discriminantClass</code> is not of the type as specified
        /// above. </exception>
        public JIUnion(Type discriminantClass) {
            // the discriminant can only be a int, bool or char

            if (!discriminantClass.Equals(typeof(int)) &&
                !discriminantClass.Equals(typeof(short)) &&
                !discriminantClass.Equals(typeof(bool)) &&
                !discriminantClass.Equals(typeof(char))) {
                // has to be from one of these. Rule from IDL.
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNION_INCORRECT_DISC));
            }
            _discriminantClass = discriminantClass;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished
        /// using the <code>discriminant</code>.
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="ArgumentException"> if any parameter is
        /// <code>null</code> </exception>
        public void AddMember(object discriminant, object member) {
            if (discriminant == null || member == null) {
                throw new ArgumentException(
                    JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNION_NULL_DISCRMINANT));
            }
            if (!discriminant.GetType().Equals(_discriminantClass)) {
                throw new JIException(JIErrorCodes.JI_UNION_DISCRMINANT_MISMATCH);
            }
            if (member.GetType().Equals(typeof(JIPointer)) && !((JIPointer)member).Reference) {
                ((JIPointer)member).Deffered = true;
            }
            else if (member.GetType().Equals(typeof(JIString))) {
                ((JIString)member).Deffered = true;
            }
            Members[discriminant] = member;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished
        /// using the <code>discriminant</code>.
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="ArgumentException"> if <code>discriminant</code>
        /// is <code>null</code> </exception>
        public void AddMember(object discriminant, JIStruct member) {
            if (discriminant == null) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNION_NULL_DISCRMINANT));
            }
            if (!discriminant.GetType().Equals(_discriminantClass)) {
                throw new JIException(JIErrorCodes.JI_UNION_DISCRMINANT_MISMATCH);
            }
            if (member == null) {
                member = JIStruct.MEMBER_IS_EMPTY;
            }
            Members[discriminant] = member;
            // do not need a seperate list of pointers like the struct,
            // since based on the discriminant only 1 pointer
            // (if present) can be deserialized\serialized.
        }

        /// <summary>
        /// Removes the entry, identified by it's <code>discriminant</code>
        /// from the parameter list of the union.
        /// </summary>
        /// <param name="discriminant"> </param>
        public void RemoveMember(object discriminant) => Members.Remove(discriminant);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="listOfDefferedPointers"></param>
        /// <param name="FLAGS"></param>
        internal void Encode(NdrCodec ndr, List<object> listOfDefferedPointers, int FLAGS) {
            if (Members.Count == 0 || Members.Count > 1) {
                throw new JIRuntimeException((int)
                    JIErrorCodes.JI_UNION_DISCRMINANT_SERIALIZATION_ERROR);
            }

            // first write the discriminant and then the member
            var keys = Members.Keys.Iterator();
            JIMarshalUnMarshalHelper.Serialize(ndr, _discriminantClass,
                keys.Next(), listOfDefferedPointers, FLAGS);

            keys = Members.Values.Iterator();
            var value = keys.Next();

            // will not write empty union members
            if (!value.Equals(JIStruct.MEMBER_IS_EMPTY)) {
                JIMarshalUnMarshalHelper.Serialize(ndr, value.GetType(), value,
                    listOfDefferedPointers, FLAGS);
            }
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="listOfDefferedPointers"></param>
        /// <param name="FLAGS"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal JIUnion Decode(NdrCodec ndr, List<object> listOfDefferedPointers,
            int FLAGS, IDictionary<object, object> additionalData) {
            // first read discriminant, and then call the appropriate deserializer of the member
            if (Members.Count == 0) {
                throw new JIRuntimeException(JIErrorCodes.JI_UNION_DISCRMINANT_DESERIALIZATION_ERROR);
            }

            // shallowClone();
            // first write the discriminant and then the member
            var retVal = new JIUnion {
                _discriminantClass = _discriminantClass
            };

            var key = JIMarshalUnMarshalHelper.Deserialize(ndr, _discriminantClass,
                listOfDefferedPointers, FLAGS, additionalData);
            // next thing to be deserialized is the member
            var value = Members[key];
            // should allow null since this could be a "default"
            if (value == null) {
                value = JIStruct.MEMBER_IS_EMPTY;
            }

            // will not write empty union members
            if (!value.Equals(JIStruct.MEMBER_IS_EMPTY)) {
                retVal.Members[key] = JIMarshalUnMarshalHelper.Deserialize(
                    ndr, value, listOfDefferedPointers, FLAGS, additionalData);
            }
            else {
                retVal.Members[key] = value;
            }

            return retVal;
        }

        /// <summary>
        /// Length
        /// </summary>
        internal int Length {
            get {
                var length = 0;
                foreach (var o in Members.Keys) {
                    var temp = JIMarshalUnMarshalHelper.GetLengthInBytes(
                        o.GetType(), o, JIFlags.FLAG_NULL);
                    length = length > temp ? length : temp; // length of the largest member
                }
                return length + JIMarshalUnMarshalHelper.GetLengthInBytes(
                    _discriminantClass, null, JIFlags.FLAG_NULL);
            }
        }

        /// <summary>
        /// Alignment
        /// </summary>
        internal int Alignment {
            get {
                var alignment = 0;
                if (_discriminantClass.Equals(typeof(int))) {
                    // align with 4 bytes
                    alignment = 4;
                }
                else if (_discriminantClass.Equals(typeof(short))) {
                    // align with 2
                    alignment = 2;
                }
                return alignment;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => "[" + Members + "]";

        private Type _discriminantClass;
    }
}