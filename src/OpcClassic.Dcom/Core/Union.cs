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
    using OpcClassic.Dcom.Internal.LegacyNdr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class represents the <code>Union</code> data type.
    /// Its usage is dictated by the discriminant
    /// which acts as a "switch" to select the correct member
    /// to be serialized\deserialzed.
    /// Sample Usage :
    /// <code>
    ///    <see cref="Union"/> forTypeDesc = new <see cref="Union"/>(typeof(short));
    ///    <see cref="ComPointer"/> ptrToTypeDesc = new <see cref="ComPointer"/>(typeDesc);
    ///    <see cref="ComPointer"/> ptrToArrayDesc = new <see cref="ComPointer"/>(arrayDesc);
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
    public sealed class Union {

        /// <summary>
        /// Returns the discriminant Vs there members Map.
        /// </summary>
        public Hashtable Members { get; } = new Hashtable();

        /// <summary>
        /// Private
        /// </summary>
        private Union() {
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
        public Union(Type discriminantClass) {
            // the discriminant can only be a int, bool or char

            if (!discriminantClass.Equals(typeof(int)) &&
                !discriminantClass.Equals(typeof(short)) &&
                !discriminantClass.Equals(typeof(bool)) &&
                !discriminantClass.Equals(typeof(char))) {
                // has to be from one of these. Rule from IDL.
                throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UNION_INCORRECT_DISC));
            }
            _discriminantClass = discriminantClass;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished
        /// using the <code>discriminant</code>.
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="InteropException"> </exception>
        /// <exception cref="ArgumentException"> if any parameter is
        /// <code>null</code> </exception>
        public void AddMember(object discriminant, object member) {
            if (discriminant == null || member == null) {
                throw new ArgumentException(
                    Interop.GetLocalizedMessage(ErrorCode.INTEROP_UNION_NULL_DISCRMINANT));
            }
            if (!discriminant.GetType().Equals(_discriminantClass)) {
                throw new InteropException(ErrorCode.INTEROP_UNION_DISCRMINANT_MISMATCH);
            }
            if (member.GetType().Equals(typeof(ComPointer)) && !((ComPointer)member).Reference) {
                ((ComPointer)member).Deffered = true;
            }
            else if (member.GetType().Equals(typeof(ComString))) {
                ((ComString)member).Deffered = true;
            }
            Members[discriminant] = member;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished
        /// using the <code>discriminant</code>.
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="InteropException"> </exception>
        /// <exception cref="ArgumentException"> if <code>discriminant</code>
        /// is <code>null</code> </exception>
        public void AddMember(object discriminant, Struct member) {
            if (discriminant == null) {
                throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UNION_NULL_DISCRMINANT));
            }
            if (!discriminant.GetType().Equals(_discriminantClass)) {
                throw new InteropException(ErrorCode.INTEROP_UNION_DISCRMINANT_MISMATCH);
            }
            if (member == null) {
                member = Struct.MEMBER_IS_EMPTY;
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
        /// <param name="context"></param>
        internal void Encode(NdrCodec ndr, CodecContext context) {
            if (Members.Count == 0 || Members.Count > 1) {
                throw new InteropRuntimeException((int)
                    ErrorCode.INTEROP_UNION_DISCRMINANT_SERIALIZATION_ERROR);
            }
            // first write the discriminant and then the member
            var keys = Members.Keys.Iterator();
            MarshalUnMarshalHelper.Serialize(ndr, _discriminantClass, keys.Next(), context);

            keys = Members.Values.Iterator();
            var value = keys.Next();

            // will not write empty union members
            if (!value.Equals(Struct.MEMBER_IS_EMPTY)) {
                MarshalUnMarshalHelper.Serialize(ndr, value.GetType(), value, context);
            }
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal Union Decode(NdrCodec ndr, CodecContext context) {
            // first read discriminant, and then call the appropriate deserializer of the member
            if (Members.Count == 0) {
                throw new InteropRuntimeException(ErrorCode.INTEROP_UNION_DISCRMINANT_DESERIALIZATION_ERROR);
            }
            // first write the discriminant and then the member
            var retVal = new Union {
                _discriminantClass = _discriminantClass
            };
            var key = MarshalUnMarshalHelper.Deserialize(ndr, _discriminantClass, context);
            // next thing to be deserialized is the member
            var value = Members[key];
            // should allow null since this could be a "default"
            if (value == null) {
                value = Struct.MEMBER_IS_EMPTY;
            }
            // will not write empty union members
            if (!value.Equals(Struct.MEMBER_IS_EMPTY)) {
                retVal.Members[key] = MarshalUnMarshalHelper.Deserialize(ndr, value, context);
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
                    var temp = MarshalUnMarshalHelper.GetLengthInBytes(
                        o.GetType(), o);
                    length = length > temp ? length : temp; // length of the largest member
                }
                return length + MarshalUnMarshalHelper.GetLengthInBytes(
                    _discriminantClass, null);
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