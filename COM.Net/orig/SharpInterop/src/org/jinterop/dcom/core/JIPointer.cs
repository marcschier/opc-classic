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

    using JIException = org.jinterop.dcom.common.JIException;
    using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;


    /// <summary>
    /// Representation of a COM pointer.
    /// 
    /// @since 1.0
    /// </summary>
    [Serializable]
    public sealed class JIPointer {


        private const long SerialVersionUID = -3434037097460692619L;
        private object Referent_Renamed = null;
        private bool IsReferenceTypePtr = false;
        private bool IsDeffered = false;
        private int ReferentId = -1;
        private bool IsNull = false;
        private int Flags_Renamed = JIFlags.FLAG_NULL;
        private JIPointer() {
        }

        /// <summary>
        /// Creates an instance of this class where the referent is of the type <code>value</code>.
        /// Used when deserializing this pointer.
        /// </summary>
        /// <param name="value"> <code>null</code> is acceptable </param>
        /// <param name="isReferenceTypePtr"> <code>true</code> if a referent identifier will not precede this ptr. </param>
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

            this.Referent_Renamed = value;
            this.IsReferenceTypePtr = isReferenceTypePtr;
        }

        private bool NullSpecial = false;
        /// <summary>
        /// Some COM servers send referentId (pointer) as null but the referent is not. To be used only when you know this is the case.
        /// Better leave it unsed.
        /// </summary>
        public void TreatNullSpecially() {
            NullSpecial = true;
        }

        /// <summary>
        /// Creates an instance of this class where the referent is <code>value</code>.
        ///  Used when serializing this pointer.
        /// </summary>
        /// <param name="value"> <code>null</code> is acceptable </param>
        /// <param name="isReferenceTypePtr"> <code>true</code> if a referent Identifier will not precede this ptr. </param>
        public JIPointer(object value, bool isReferenceTypePtr) {
            if (value == null) {
                //since a null is being sent for a pointer , it has to be shown
                //as 0x0.
                value = new int?(0);
                isReferenceTypePtr = true;
                IsNull = true;
            }

    //        if (value.getClass().equals(JIArray.class))
    //        {
    //            if (((JIArray)value).getDimensions() > 1)
    //                throw new IllegalArgumentException("Only single dimension arrays accepted");
    //        }

        //Should not defer since the enclosing struct,union,array will defer it by itself
        // this is important since , ptr to a ptr to a ptr (and more) will need to
        //deserialize completely after the first deferement i.e they are not further deffered.

            this.Referent_Renamed = value;
            this.ReferentId = (new object()).GetHashCode();
            this.IsReferenceTypePtr = isReferenceTypePtr;
        }

        /// <summary>
        /// Sets the flags associated with the referent.
        /// 
        /// @exclude </summary>
        /// <param name="flags"> JIFlags only. </param>
        public int Flags {
            set {
                this.Flags_Renamed = value;
            }
        }

        public void SetIsReferenceTypePtr() {
            IsReferenceTypePtr = true;
        }

        /// <summary>
        ///Creates an instance of this class where the referent is <code>value</code>.
        /// Used when serializing this pointer. This pointer is <b>not</b> of reference type.
        /// </summary>
        /// <param name="value"> </param>
        public JIPointer(object value) : this(value,false) {
        }

        /// <summary>
        /// Returns the referent encapsulated by this pointer.
        /// 
        /// @return
        /// </summary>
        public object GetReferent() {
            return IsNull ? null:Referent_Renamed;
        }

        public void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG) {

            FLAG = FLAG | Flags_Renamed;
            if (IsNull) {
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),defferedPointers,FLAG);
                return;
            }
            //it is deffered or part of an array, this logic will not get called twice since the
            //deffered list will come in withb FLAG_NULL
            if (!IsNull && (IsDeffered || (FLAG & JIFlags.FLAG_REPRESENTATION_ARRAY) == JIFlags.FLAG_REPRESENTATION_ARRAY)) /*||
                        (FLAG & JIFlags.FLAG_REPRESENTATION_NESTED_POINTER ) == JIFlags.FLAG_REPRESENTATION_NESTED_POINTER*/ {
                int referentIdToPut = ReferentId == -1 ? Referent_Renamed.GetHashCode() : ReferentId;
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(referentIdToPut),defferedPointers,FLAG);
                IsDeffered = false;
                IsReferenceTypePtr = true;
    //            try{
                defferedPointers.Add(this);
    //            }catch(NullPointerException e)
    //            {
    //                int ni = 0;
    //            }
                return;
                        }

            if (!IsNull && !IsReferenceTypePtr) {
                int referentIdToPut = ReferentId == -1 ? Referent_Renamed.GetHashCode() : ReferentId;
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(referentIdToPut),defferedPointers,FLAG);
            }

            try {
                if (!IsNull && Referent_Renamed.GetType().Equals(typeof(JIVariant)) && ((JIVariant)Referent_Renamed).Array) {
                    //write the length first before all elements
                    //ndr.writeUnsignedLong(((Object[])(((JIVariant)referent).getObject())).length);
                    JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(((object[])(((JIVariant)Referent_Renamed).Object)).Length),defferedPointers,FLAG);
                }
            }
            catch (JIException e) {
                throw new JIRuntimeException(e.ErrorCode);
            }



            JIMarshalUnMarshalHelper.Serialize(ndr,Referent_Renamed.GetType(),Referent_Renamed,defferedPointers,FLAG);


        }

        //class of type being decoded. If the type being expected is an array , the varType
        //should be the actual array type and not JIArray.
        public JIPointer Decode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
            //shallowClone();
            FLAG = FLAG | Flags_Renamed;

            JIPointer retVal = new JIPointer();
            retVal.Flags = Flags_Renamed;
            retVal.IsNull = IsNull;
            retVal.NullSpecial = NullSpecial;

            //retVal.isDeffered = isDeffered;
            if (IsDeffered || (FLAG & JIFlags.FLAG_REPRESENTATION_ARRAY) == JIFlags.FLAG_REPRESENTATION_ARRAY)
                    /*|| (FLAG & JIFlags.FLAG_REPRESENTATION_NESTED_POINTER ) == JIFlags.FLAG_REPRESENTATION_NESTED_POINTER */ {
                retVal.ReferentId = (int)((int?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),defferedPointers,FLAG,additionalData));
                retVal.Referent_Renamed = Referent_Renamed; //will only be the class or object
                if (retVal.ReferentId == 0 && !NullSpecial) {
                    //null pointer
                    // just return
                    retVal.IsNull = true;
                    retVal.IsDeffered = false;
                    return retVal;
                }

                retVal.IsDeffered = false;
                retVal.IsReferenceTypePtr = true;
                defferedPointers.Add(retVal);
                return retVal;
                    }

            if (!IsReferenceTypePtr) {
                //referentId = ndr.readUnsignedLong();
                retVal.ReferentId = (int)((int?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),defferedPointers,FLAG,additionalData));
                retVal.Referent_Renamed = Referent_Renamed; //will only be the class or object
                if (retVal.ReferentId == 0 && !NullSpecial) {
                    //null pointer
                    // just return
                    retVal.IsNull = true;
                    return retVal;
                }
            }


            retVal.Referent_Renamed = JIMarshalUnMarshalHelper.DeSerialize(ndr,Referent_Renamed,defferedPointers,FLAG,additionalData);
            return retVal;
        }

        public bool Deffered {
            set {
                IsDeffered = value;
            }
            get {
                return IsDeffered;
            }
        }


        public void SetReferent(int referent) {
            this.ReferentId = referent;
        }

        /// <summary>
        /// Returns status whether this is a reference type pointer or not.
        /// </summary>
        /// <returns> <code>true</code> if this is a reference type pointer. </returns>
        public bool Reference {
            get {
                return IsReferenceTypePtr;
            }
        }

        /// <summary>
        /// Returns the referent identifier.
        /// 
        /// @return
        /// </summary>
        public int? ReferentIdentifier {
            get {
                return new int?(ReferentId);
            }
        }

        /// <summary>
        /// @exclude
        /// @return
        /// </summary>
        public int Length {
            get {
                if (IsNull) {
                    return 4;
                }
                //4 for pointer
                if (Referent_Renamed is Type) {
                    return 4 + JIMarshalUnMarshalHelper.GetLengthInBytes((Type)Referent_Renamed,Referent_Renamed,JIFlags.FLAG_NULL);
                }
                return 4 + JIMarshalUnMarshalHelper.GetLengthInBytes(Referent_Renamed.GetType(),Referent_Renamed,JIFlags.FLAG_NULL);
            }
        }



        public void ReplaceSelfWithNewPointer(JIPointer replacement) {
            this.IsDeffered = replacement.IsDeffered;
            this.IsNull = replacement.IsNull;
            this.IsReferenceTypePtr = replacement.IsReferenceTypePtr;
            this.Referent_Renamed = replacement.Referent_Renamed;
        }

        /// <summary>
        /// Returns status if this pointer is <code>null</code>.
        /// </summary>
        /// <returns> <code>true</code> if the pointer is <code>null</code>. </returns>
        public bool Null {
            get {
                return IsNull;
            }
        }

        public object Value {
            set {
                Referent_Renamed = value;
            }
        }

        public override string ToString() {
            return Referent_Renamed == null ? "[null]" : "[" + Referent_Renamed.ToString() + "]";
        }
    }

}