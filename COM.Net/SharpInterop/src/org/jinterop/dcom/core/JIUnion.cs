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
    using rpc.core;
    using org.jinterop.dcom.common;
    using org.jinterop.winreg;
    using System.Collections;
    using System;

    /// <summary>
    /// This class represents the <code>Union</code> data type. Its usage is dictated by the discriminant
    /// which acts as a "switch" to select the correct member to be serialized\deserialzed.
    /// Sample Usage :-
    /// <code>
    /// 	JIUnion forTypeDesc = new JIUnion(Short.class); 
    ///     JIPointer ptrToTypeDesc = new JIPointer(typeDesc); 
    ///     JIPointer ptrToArrayDesc = new JIPointer(arrayDesc); 
    ///     forTypeDesc.addMember(TypeDesc.VT_PTR,ptrToTypeDesc); 
    ///     forTypeDesc.addMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc); 
    ///     forTypeDesc.addMember(TypeDesc.VT_CARRAY,ptrToArrayDesc); 
    ///     forTypeDesc.addMember(TypeDesc.VT_USERDEFINED,Integer.class);
    /// </code>
    /// The TypeDesc.VT_PTR is an <code>Integer</code> and is used as a discriminant 
    /// to select ptrTypeDesc, TypeDesc.VT_CARRAY chooses ptrArrayDesc. 
    /// </summary>
    [Serializable]
    public sealed class JIUnion {

        private Hashtable _dsVsMember = new Hashtable();
        private Type _discriminantClass;

        private JIUnion() {
        }

        /// <summary>
        /// Creates an object with discriminant type specified. Used only during deserializing
        ///  the union. Can only be of the type <code>Integer</code>,<code>Short</code>,<code>Boolean</code>
        ///  or <code>Character</code>. 
        /// </summary>
        /// <param name="discriminantClass"> </param>
        /// <exception cref="ArgumentException"> if the <code>discriminantClass</code> is not of the type as specified
        /// above. </exception>
        public JIUnion(Type discriminantClass) {
            //the discriminant can only be a int, bool or char

            if (!discriminantClass.Equals(typeof(int?)) &&
                !discriminantClass.Equals(typeof(short?)) && 
                !discriminantClass.Equals(typeof(bool?)) && 
                !discriminantClass.Equals(typeof(char?))) {
                //has to be from one of these. Rule from IDL.
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UNION_INCORRECT_DISC));
            }
            _discriminantClass = discriminantClass;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished using the <code>discriminant</code>. 
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="ArgumentException"> if any parameter is <code>null</code> </exception>
        public void addMember(object discriminant, object member) {
            if (discriminant == null || member == null) {
                throw new ArgumentException(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UNION_NULL_DISCRMINANT));
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
            _dsVsMember[discriminant] = member;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished using the <code>discriminant</code>. 
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="ArgumentException"> if <code>discriminant</code> is <code>null</code> </exception>
        public void addMember(object discriminant, JIStruct member) {
            if (discriminant == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UNION_NULL_DISCRMINANT));
            }
            if (!discriminant.GetType().Equals(_discriminantClass)) {
                throw new JIException(JIErrorCodes.JI_UNION_DISCRMINANT_MISMATCH);
            }
            if (member == null) {
                member = JIStruct.MEMBER_IS_EMPTY;
            }
            _dsVsMember[discriminant] = member;
            //do not need a seperate list of pointers like the struct , since based on the discriminant only 1 pointer
            //(if present) can be deserialized\serialized.
        }

        /// <summary>
        /// Removes the entry , identified by it's <code>discriminant</code> from the parameter list of the union. 
        /// </summary>
        /// <param name="discriminant"> </param>
        public void removeMember(object discriminant) {
            _dsVsMember.Remove(discriminant);
        }

        /// <summary>
        /// Returns the discriminant Vs there members Map. 
        /// </summary>
        public IDictionary Members => _dsVsMember;

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="listOfDefferedPointers"></param>
        /// <param name="FLAGS"></param>
        internal void encode(NetworkDataRepresentation ndr, IList listOfDefferedPointers, int FLAGS) {
            if (_dsVsMember.Count == 0 || _dsVsMember.Count > 1) {
                throw new JIRuntimeException(JIErrorCodes.JI_UNION_DISCRMINANT_SERIALIZATION_ERROR);
            }

            //first write the discriminant and then the member
            var keys = _dsVsMember.Keys.GetEnumerator();
            JIMarshalUnMarshalHelper.serialize(ndr, _discriminantClass, keys.next(), listOfDefferedPointers, FLAGS);

            keys = _dsVsMember.Values.GetEnumerator();
            object value = keys.next();

            //will not write empty union members
            if (!value.Equals(JIStruct.MEMBER_IS_EMPTY)) {
                JIMarshalUnMarshalHelper.serialize(ndr, value.GetType(), value, listOfDefferedPointers, FLAGS);
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
        internal JIUnion decode(NetworkDataRepresentation ndr, IList listOfDefferedPointers, int FLAGS, IDictionary additionalData) {
            //first read discriminant, and then call the appropriate deserializer of the member
            if (_dsVsMember.Count == 0) {
                throw new JIRuntimeException(JIErrorCodes.JI_UNION_DISCRMINANT_DESERIALIZATION_ERROR);
            }

            //shallowClone();
            //first write the discriminant and then the member
            var retVal = new JIUnion {
                _discriminantClass = _discriminantClass
            };

            var key = JIMarshalUnMarshalHelper.deSerialize(ndr, _discriminantClass, listOfDefferedPointers, FLAGS, additionalData);

            //next thing to be deserialized is the member
            var value = _dsVsMember[key];

            //should allow null since this could be a "default"
            if (value == null) {
                value = JIStruct.MEMBER_IS_EMPTY;
            }

            //will not write empty union members
            if (!value.Equals(JIStruct.MEMBER_IS_EMPTY)) {
                retVal._dsVsMember[key] = JIMarshalUnMarshalHelper.deSerialize(ndr, value, listOfDefferedPointers, FLAGS, additionalData);
            }
            else {
                retVal._dsVsMember[key] = value;
            }

            return retVal;
        }

        /// <summary>
        /// Length
        /// </summary>
        internal int Length {
            get {
                var length = 0;
                foreach (var o in _dsVsMember.Keys) { 
                    var temp = JIMarshalUnMarshalHelper.getLengthInBytes(o.GetType(), o, JIFlags.FLAG_NULL);
                    length = length > temp ? length : temp; //length of the largest member
                }
                return length + JIMarshalUnMarshalHelper.getLengthInBytes(_discriminantClass, null, JIFlags.FLAG_NULL);
            }
        }

        /// <summary>
        /// Alignment
        /// </summary>
        internal int Alignment {
            get {
                var alignment = 0;

                if (_discriminantClass.Equals(typeof(int?))) {
                    //align with 4 bytes
                    alignment = 4;
                }
                else if (_discriminantClass.Equals(typeof(short?))) {
                    //align with 2
                    alignment = 2;
                }

                return alignment;
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            return "[" + _dsVsMember + "]";
        }
    }
}