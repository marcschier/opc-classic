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

    using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
    using JIException = org.jinterop.dcom.common.JIException;
    using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
    using JISystem = org.jinterop.dcom.common.JISystem;


    /// <summary>
    ///<para> This class represents the <code>Union</code> data type. Its usage is dictated by the discriminant
    /// </para>
    /// which acts as a "switch" to select the correct member to be serialized\deserialzed. <para>
    /// 
    /// Sample Usage :-
    /// 
    /// <br>
    /// <code>
    ///     JIUnion forTypeDesc = new JIUnion(Short.class); <br>
    /// JIPointer ptrToTypeDesc = new JIPointer(typeDesc); <br>
    /// JIPointer ptrToArrayDesc = new JIPointer(arrayDesc); <br>
    /// forTypeDesc.addMember(TypeDesc.VT_PTR,ptrToTypeDesc); <br>
    /// forTypeDesc.addMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc); <br>
    /// forTypeDesc.addMember(TypeDesc.VT_CARRAY,ptrToArrayDesc); <br>
    /// </para>
    /// forTypeDesc.addMember(TypeDesc.VT_USERDEFINED,Integer.class); <para>
    /// </code>
    /// 
    /// The TypeDesc.VT_PTR is an <code>Integer</code> and is used as a discriminant to select ptrTypeDesc, TypeDesc.VT_CARRAY
    ///  chooses ptrArrayDesc. <br>
    /// 
    /// 
    /// 
    /// @since 1.0
    /// </para>
    /// </summary>
    [Serializable]
    public sealed class JIUnion {


        private const long SerialVersionUID = -3353313619137076876L;
        private Hashtable DsVsMember = new Hashtable();
        private Type DiscriminantClass = null;
        //private int length = 0;
        //private int lengthOfDisc = 0;
        //private Union clone = null;
        private JIUnion() {
        }

        /// <summary>
        /// Creates an object with discriminant type specified. Used only during deserializing
        ///  the union. Can only be of the type <code>Integer</code>,<code>Short</code>,<code>Boolean</code>
        ///  or <code>Character</code>. <br>
        /// </summary>
        /// <param name="discriminantClass"> </param>
        /// <exception cref="IllegalArgumentException"> if the <code>discriminantClass</code> is not of the type as specified
        /// above. </exception>
        public JIUnion(Type discriminantClass) {
            //the discriminant can only be a int, boolean or char

            if (!discriminantClass.Equals(typeof(int?)) && !discriminantClass.Equals(typeof(short?)) && !discriminantClass.Equals(typeof(bool?)) && !discriminantClass.Equals(typeof(char?))) {
                //has to be from one of these. Rule from IDL.
                throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNION_INCORRECT_DISC));
            }

            this.DiscriminantClass = discriminantClass;

        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished using the <code>discriminant</code>. <br>
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if any parameter is <code>null</code> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addMember(Object discriminant, Object member) throws org.jinterop.dcom.common.JIException
        public void AddMember(object discriminant, object member) {
            if (discriminant == null || member == null) {
                throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNION_NULL_DISCRMINANT));
            }

            if (!discriminant.GetType().Equals(DiscriminantClass)) {
                throw new JIException(JIErrorCodes.JI_UNION_DISCRMINANT_MISMATCH);
            }

            if (member.GetType().Equals(typeof(JIPointer)) && !((JIPointer)member).Reference) {
                ((JIPointer)member).Deffered = true;
            }
            else {
            if (member.GetType().Equals(typeof(JIString))) {
                ((JIString)member).Deffered = true;
            }
            }

            DsVsMember[discriminant] = member;
        }

        /// <summary>
        /// Adds a member to this Union. The <code>member</code> is distinguished using the <code>discriminant</code>. <br>
        /// </summary>
        /// <param name="discriminant"> </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if <code>discriminant</code> is <code>null</code> </exception>
        //used both for reading and writing
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addMember(Object discriminant, JIStruct member) throws org.jinterop.dcom.common.JIException
        public void AddMember(object discriminant, JIStruct member) {
            if (discriminant == null) {
                throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNION_NULL_DISCRMINANT));
            }

            if (!discriminant.GetType().Equals(DiscriminantClass)) {
                throw new JIException(JIErrorCodes.JI_UNION_DISCRMINANT_MISMATCH);
            }

            if (member == null) {
                member = JIStruct.MEMBER_IS_EMPTY;
            }

            DsVsMember[discriminant] = member;
            //do not need a seperate list of pointers like the struct , since based on the discriminant only 1 pointer
            //(if present) can be deserialized\serialized.
        }

        /// <summary>
        ///Removes the entry , identified by it's <code>discriminant</code> from the parameter list of the union. <br>
        /// </summary>
        /// <param name="discriminant"> </param>
        public void RemoveMember(object discriminant) {
            DsVsMember.Remove(discriminant);
        }

        /// <summary>
        /// Returns the discriminant Vs there members Map. <br>
        /// 
        /// @return
        /// </summary>
        public IDictionary Members {
            get {
                return DsVsMember;
            }
        }

        public void Encode(NetworkDataRepresentation ndr, IList listOfDefferedPointers, int FLAGS) {
            if (DsVsMember.Count == 0 || DsVsMember.Count > 1) {
                throw new JIRuntimeException(JIErrorCodes.JI_UNION_DISCRMINANT_SERIALIZATION_ERROR);
            }

            //first write the discriminant and then the member
            IEnumerator keys = DsVsMember.Keys.GetEnumerator();
            JIMarshalUnMarshalHelper.Serialize(ndr,DiscriminantClass,keys.next(),listOfDefferedPointers,FLAGS);

            keys = DsVsMember.Values.GetEnumerator();
            object value = keys.next();

            //will not write empty union members
            if (!value.Equals(JIStruct.MEMBER_IS_EMPTY)) {
                JIMarshalUnMarshalHelper.Serialize(ndr,value.GetType(),value,listOfDefferedPointers,FLAGS);
            }

        }

        public JIUnion Decode(NetworkDataRepresentation ndr, IList listOfDefferedPointers, int FLAGS, IDictionary additionalData) {
            //first read discriminant, and then call the appropriate deserializer of the member
            if (DsVsMember.Count == 0) {
                throw new JIRuntimeException(JIErrorCodes.JI_UNION_DISCRMINANT_DESERIALIZATION_ERROR);
            }

            //shallowClone();
            //first write the discriminant and then the member
            JIUnion retVal = new JIUnion();
            retVal.DiscriminantClass = DiscriminantClass;

            object key = JIMarshalUnMarshalHelper.DeSerialize(ndr,DiscriminantClass,listOfDefferedPointers,FLAGS,additionalData);

            //next thing to be deserialized is the member
            object value = DsVsMember.GetValueOrNull(key);

            //should allow null since this could be a "default"
            if (value == null) {
                value = JIStruct.MEMBER_IS_EMPTY;
            }

            //will not write empty union members
            if (!value.Equals(JIStruct.MEMBER_IS_EMPTY)) {
                retVal.DsVsMember[key] = JIMarshalUnMarshalHelper.DeSerialize(ndr,value,listOfDefferedPointers,FLAGS,additionalData);
            }
            else {
                retVal.DsVsMember[key] = value;
            }

            return retVal;
        }

        public int Length {
            get {
                int length = 0;
                IEnumerator itr = DsVsMember.Keys.GetEnumerator();
                while (itr.hasNext()) {
                    object o = itr.next();
                    int temp = JIMarshalUnMarshalHelper.GetLengthInBytes(o.GetType(),o,JIFlags.FLAG_NULL);
                    length = length > temp ? length : temp; //length of the largest member
                }
    
                return length + JIMarshalUnMarshalHelper.GetLengthInBytes(DiscriminantClass,null,JIFlags.FLAG_NULL);
            }
        }

        public int Alignment {
            get {
                int alignment = 0;
    
                if (DiscriminantClass.Equals(typeof(int?))) {
                    //align with 4 bytes
                    alignment = 4;
                }
                else if (DiscriminantClass.Equals(typeof(short?))) {
                    //align with 2
                    alignment = 2;
                }
    
                return alignment;
            }
        }

    //    public String toString()
    //    {
    //        return  "[" + dsVsMember +  "]";
    //    }
    }

}