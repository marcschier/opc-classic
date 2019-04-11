using System;
using System.Collections;
using System.Collections.Generic;

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

    /// <summary>
    /// This class represents the <code>Struct</code> data type. <br>
    /// 
    /// @since 1.0
    /// </summary>
    //for conformant and conformant+varying arrays the maxcount etc. should come at the begining of the struct.
    [Serializable]
    public sealed class JIStruct {


        private const long SerialVersionUID = 7708214775854162549L;

        private IList ListOfMembers = new List<object>();
        private IList ListOfMaxCounts = new List<object>(); //keeps a list of Max counts for each array dimension, arrays
                                                        //following one another are inserted in sequential order.

        private IList ListOfDimensions = new List<object>();
        private bool ArrayAdded = false;

        public static readonly JIStruct MEMBER_IS_EMPTY = new JIStruct();

        /// <summary>
        /// Adds the object as a member of this structure. This object is appended to the list of members within. <br>
        /// </summary>
        /// <param name="member"> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addMember(Object member) throws org.jinterop.dcom.common.JIException
        public void AddMember(object member) {
            //null has to be allowed for members who would like to send null...NPE should not be thrown
            AddMember(ListOfMembers.Count,member);
        }

        /// <summary>
        /// Adds object as member to the index specified. <br>
        /// </summary>
        /// <param name="position"> Zero based index </param>
        /// <param name="member"> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addMember(int position,Object member) throws org.jinterop.dcom.common.JIException
        public void AddMember(int position, object member) {
            //null has to be allowed for members who would like to send null...NPE should not be thrown
            member = member == null ? new int?(0) : member;

            Type memberClass = member.GetType();

            //An array has already been added , now a new member cannot be added
            if (ArrayAdded && position == ListOfMembers.Count && !memberClass.Equals(typeof(JIArray))) {
                throw new JIException(JIErrorCodes.JI_STRUCT_ARRAY_AT_END);
            }

            //arrays can only be the last element of this struct.
            if (memberClass.Equals(typeof(JIArray))) {
                //this condition will also allow that if another nested struct has an array , this new array is added at the
                // very end.
                if (position != ListOfMembers.Count) {
                    throw new JIException(JIErrorCodes.JI_STRUCT_ARRAY_ONLY_AT_END);
                }

                ArrayAdded = true;

                //Fixed arrays like char[50] are serialzed\deserialized in place itself.
                if (((JIArray)member).Conformant || ((JIArray)member).Varying) {
                    //since there could be two arrays.
                    ListOfMaxCounts.AddRange(((JIArray)member).ConformantMaxCounts);
                    ListOfDimensions.Add(new int?(((JIArray)member).Dimensions));
                }
            }

            //struct part of another struct
            if (memberClass.Equals(typeof(JIStruct))) {
                //if this has an array then , this struct has to be the last member in the struct list.
                if (((JIStruct)member).ArrayAdded && ArrayAdded && position != (ListOfMembers.Count - 1)) {
                    throw new JIException(JIErrorCodes.JI_STRUCT_INCORRECT_NESTED_STRUCT_POS);
                }

                if (ArrayAdded && ((JIStruct)member).ArrayAdded) {
                    //means that we have to move the maxcount of the internal struct to this struct.
                    ArrayAdded = true;
                    ListOfMaxCounts.AddRange(((JIStruct)member).ArrayMaxCounts);
                    ((JIStruct)member).ListOfMaxCounts.Clear(); //this is a "move" of max counts to the
                                                                   //outer struct

                    ListOfDimensions.AddRange(((JIStruct)member).ListOfDimensions);
                    ((JIStruct)member).ListOfDimensions.Clear();

                }
                else {
                if (!ArrayAdded && ((JIStruct)member).ArrayAdded) {
                    if (position == ListOfMembers.Count) {
                        ArrayAdded = true;
                        ListOfMaxCounts.AddRange(((JIStruct)member).ArrayMaxCounts);
                        ((JIStruct)member).ListOfMaxCounts.Clear(); //this is a "move" of max counts to the
                                                                       //outer struct

                        ListOfDimensions.AddRange(((JIStruct)member).ListOfDimensions);
                        ((JIStruct)member).ListOfDimensions.Clear();
                    }
                    else {
                        throw new JIException(JIErrorCodes.JI_STRUCT_INCORRECT_NESTED_STRUCT_POS2);
                    }
                }
                }


            }


            if (memberClass.Equals(typeof(JIPointer)) && !((JIPointer)member).Reference) {
                //send this to the end and put the place holder of the pointer here
                ((JIPointer)member).Deffered = true;
            }
            else if (memberClass.Equals(typeof(JIVariant))) {
                ((JIVariant)member).Deffered = true;
            }
            else {
            if (memberClass.Equals(typeof(JIString))) {
                ((JIString)member).Deffered = true;
            }
            else
    //        if (memberClass.equals(JIInterfacePointer.class))
    //        {
    //            ((JIInterfacePointer)member).setDeffered(true);
    //        }
    //        else
    //        if (memberClass.equals(JIDispatchImpl.class))
    //        {
    //            ((JIComObjectImplWrapper)member).getInterfacePointer().setDeffered(true);
    //        }
    //        else
    //        if (memberClass.equals(JIComObjectImpl.class))
    //        {
    //            ((IJIComObject)member).getInterfacePointer().setDeffered(true);
    //        }
            {
            if (memberClass.Equals(typeof(IJIComObject))) {
                ((IJIComObject)member).Internal_setDeffered(true);
            }
            }
            }
            //else the pointer will be serialized "inplace".

            ListOfMembers.Insert(position,member);
        }

        /// <summary>
        /// Removes the member from the specified index. <br>
        /// </summary>
        /// <param name="index"> </param>
        public void RemoveMember(int index) {
            object member = ListOfMembers.Remove(index);
            if (member is JIArray) {
                //we need to remove it's max count values also.
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
                ListOfMaxCounts.removeAll(((JIArray)member).ConformantMaxCounts);

            }
            else {
            if (member is JIStruct && ((JIStruct)member).ArrayAdded) {
                //we need to remove it's max count values also.
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
                ListOfMaxCounts.removeAll(((JIStruct)member).ArrayMaxCounts);
            }
            }

            if (ListOfMaxCounts.Count == 0) {
                ArrayAdded = false;
            }
        }


        /// <summary>
        /// Returns all members as java.util.List. <br>
        /// 
        /// @return
        /// </summary>
        public IList Members {
            get {
                return ListOfMembers;
            }
        }

        /// <summary>
        /// Retrieves the member at the specified index from the member list. <br>
        /// </summary>
        /// <param name="position"> Zero based index.
        /// @return </param>
        public object GetMember(int position) {
            return ListOfMembers[position];
        }

        /// <summary>
        /// Returns the total number of members.
        /// 
        /// @return
        /// </summary>
        public int Size {
            get {
                return ListOfMembers.Count;
            }
        }

        public void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG) {
            //first write all Max counts and then the rest of the structs
            for (int i = 0;i < ListOfMaxCounts.Count;i++) {
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),(int?)ListOfMaxCounts[i],null,FLAG);
            }

            int i = 0;
            while (i < ListOfMembers.Count) {
                object o = ListOfMembers[i]; {
                    if (o is JIArray) {
                        //if this array is conformant then reset it's conformancy , since the length would have been
                        //written before.
                        ((JIArray)o).Conformant = false;
                    }
                    JIMarshalUnMarshalHelper.Serialize(ndr,o.GetType(),o,defferedPointers,FLAG);
                    if (o is JIArray) {
                        //noew reset this, so that next time when the same struct is written everything goes proper.
                        ((JIArray)o).Conformant = ((JIArray)o).Conformant;
                    }
                }
                i++;
            }
        }

        public JIStruct Decode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
            JIStruct retVal = new JIStruct();
            List<object> listOfMaxCounts2 = new List<object>();
            //first read all Max counts and then the rest of the structs
            for (int i = 0;i < ListOfDimensions.Count;i++) {
                for (int j = 0;j < (int)((int?)ListOfDimensions[i]);j++) {
                    listOfMaxCounts2.Add(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,FLAG,additionalData));
                }
            }


            int i = 0;
            int j = 0; //index only for the conformant \ varying arrays
            while (i < ListOfMembers.Count) {
                object o = ListOfMembers[i];
                IList maxCountTemp = null;
                if (o is JIArray) {
                    if (((JIArray)o).Conformant || ((JIArray)o).Varying) {
                        //if this array is conformant then reset it's conformancy , since the length would have been
                        //read before.
                        ((JIArray)o).Conformant = false;
                        maxCountTemp = ((JIArray)o).ConformantMaxCounts;
                        ((JIArray)o).MaxCountAndUpperBounds = listOfMaxCounts2.subList(j,(int)((int?)ListOfDimensions[j]));
                        j++;
                    }
                }
                object o1 = JIMarshalUnMarshalHelper.DeSerialize(ndr,o,defferedPointers,FLAG,additionalData);
                if (o is JIArray) {
                    if (((JIArray)o).Conformant || ((JIArray)o).Varying) {
                        //now reset this, so that next time when the same struct is written everything goes proper.
                        ((JIArray)o).Conformant = ((JIArray)o).Conformant;
                        ((JIArray)o).MaxCountAndUpperBounds = maxCountTemp;
                    }
                }
                try {
                retVal.AddMember(o1); //listOfMembers.add(o);
                }
                catch (JIException e) {
                    throw new JIRuntimeException(e.ErrorCode);
                }
                i++;
            }

            //do not copy other members since the addMember above will take care of all the conditions.
            return retVal;
        }

        public int Length {
            get {
                int length = 0;
                int i = 0;
                while (i < ListOfMembers.Count) {
                    object o = ListOfMembers[i];
                    if (o is Type) {
                        length += JIMarshalUnMarshalHelper.GetLengthInBytes((Type)o,o,JIFlags.FLAG_NULL);
                    }
                    else {
                        length += JIMarshalUnMarshalHelper.GetLengthInBytes(o.GetType(),o,JIFlags.FLAG_NULL);
                    }
                    i++;
                }
                return length;
            }
        }

        public IList ArrayMaxCounts {
            get {
                return ListOfMaxCounts;
            }
        }

        public int Alignment {
            get {
                int alignment = 0;
    
                for (int i = 0;i < ListOfMembers.Count; i++) {
                    Type c = ListOfMembers[i].GetType();
                    bool isClass = false;
                    if (c.Equals(typeof(Type))) {
                        isClass = true;
                        c = (Type)ListOfMembers[i];
                    }
    
                    if (c.Equals(typeof(int?)) || c.Equals(typeof(float?)) || c.Equals(typeof(string)) || c.Equals(typeof(JIString)) || c.Equals(typeof(JIPointer)) || c.Equals(typeof(JIUnsignedInteger)) || c.Equals(typeof(JIVariant))) {
                        //align with 4 bytes
                        alignment = alignment <= 4 ? 4 : alignment;
                    }
                    else if (c.Equals(typeof(double?)) || c.Equals(typeof(DateTime?)) || c.Equals(typeof(long?))) {
                        //align with 8
                        alignment = alignment <= 8 ? 8 : alignment;
                    }
                    else if (c.Equals(typeof(short?)) || c.Equals(typeof(JIUnsignedShort))) {
                        //align with 2
                        alignment = alignment <= 2 ? 2 : alignment;
                    }
                    else {
                    if (c.Equals(typeof(JIStruct))) {
                        if (!isClass) {
                            int align = ((JIStruct)ListOfMembers[i]).Alignment;
                            alignment = alignment <= align ? align : alignment;
                        }
                        else {
                            //incorrect entry !!!...
                        }
                    }
                    else {
                    if (c.Equals(typeof(JIUnion))) {
                        if (!isClass) {
                            int align = ((JIUnion)ListOfMembers[i]).Alignment;
                            alignment = alignment <= align ? align : alignment;
                        }
                        else {
                            //incorrect entry !!!...
                        }
                    }
                    }
                    }
                    if (alignment == 8) {
                        break;
                    }
                }
    
                return alignment;
            }
        }

    //    public String toString()
    //    {
    //        return "[" + listOfMembers + "]";
    //    }

    }

}