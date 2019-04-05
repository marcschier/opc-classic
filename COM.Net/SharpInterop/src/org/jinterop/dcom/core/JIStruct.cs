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
    using System.Collections;

    /// <summary>
    /// This class represents the <code>Struct</code> data type.
    /// for conformant and conformant+varying arrays the maxcount etc. 
    /// should come at the begining of the struct.
    /// </summary>
    [Serializable]
    public sealed class JIStruct {

        /// <summary>
        /// Empty struct
        /// </summary>
        public static readonly JIStruct MEMBER_IS_EMPTY = new JIStruct();

        /// <summary>
        /// Adds the object as a member of this structure. This object is appended to the list of members within. 
        /// </summary>
        /// <param name="member"> </param>
        /// <exception cref="JIException"></exception>
        public void addMember(object member) {
            //null has to be allowed for members who would like to send null...NPE should not be thrown
            addMember(Members.Count, member);
        }

        /// <summary>
        /// Adds object as member to the index specified. 
        /// </summary>
        /// <param name="position"> Zero based index </param>
        /// <param name="member"> </param>
        /// <exception cref="JIException"></exception>
        public void addMember(int position, object member) {
            //null has to be allowed for members who would like to send null...NPE should not be thrown
            member = member ?? 0;
            var memberClass = member.GetType();
            //An array has already been added , now a new member cannot be added
            if (_arrayAdded && position == Members.Count && !memberClass.Equals(typeof(JIArray))) {
                throw new JIException(JIErrorCodes.JI_STRUCT_ARRAY_AT_END);
            }

            //arrays can only be the last element of this struct.
            if (memberClass.Equals(typeof(JIArray))) {
                //this condition will also allow that if another nested struct has an array , this new array is added at the
                // very end.
                if (position != Members.Count) {
                    throw new JIException(JIErrorCodes.JI_STRUCT_ARRAY_ONLY_AT_END);
                }
                _arrayAdded = true;
                //Fixed arrays like char[50] are serialzed\deserialized in place itself.
                if (((JIArray)member).Conformant || ((JIArray)member).Varying) {
                    //since there could be two arrays.
                    ArrayMaxCounts.AddRange(((JIArray)member).ConformantMaxCounts);
                    _listOfDimensions.Add(((JIArray)member).Dimensions);
                }
            }

            //struct part of another struct
            if (memberClass.Equals(typeof(JIStruct))) {
                //if this has an array then , this struct has to be the last member in the struct list.
                if (((JIStruct)member)._arrayAdded && _arrayAdded && position != (Members.Count - 1)) {
                    throw new JIException(JIErrorCodes.JI_STRUCT_INCORRECT_NESTED_STRUCT_POS);
                }

                if (_arrayAdded && ((JIStruct)member)._arrayAdded) {
                    //means that we have to move the maxcount of the internal struct to this struct.
                    _arrayAdded = true;
                    ArrayMaxCounts.AddRange(((JIStruct)member).ArrayMaxCounts);
                    ((JIStruct)member).ArrayMaxCounts.Clear(); //this is a "move" of max counts to the
                                                               //outer struct

                    _listOfDimensions.AddRange(((JIStruct)member)._listOfDimensions);
                    ((JIStruct)member)._listOfDimensions.Clear();

                }
                else if (!_arrayAdded && ((JIStruct)member)._arrayAdded) {
                    if (position == Members.Count) {
                        _arrayAdded = true;
                        ArrayMaxCounts.AddRange(((JIStruct)member).ArrayMaxCounts);
                        ((JIStruct)member).ArrayMaxCounts.Clear(); //this is a "move" of max counts to the
                                                                   //outer struct

                        _listOfDimensions.AddRange(((JIStruct)member)._listOfDimensions);
                        ((JIStruct)member)._listOfDimensions.Clear();
                    }
                    else {
                        throw new JIException(JIErrorCodes.JI_STRUCT_INCORRECT_NESTED_STRUCT_POS2);
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
            else if (memberClass.Equals(typeof(JIString))) {
                ((JIString)member).Deffered = true;
            }
            // else if (memberClass.equals(JIInterfacePointer.class)) {
            //			((JIInterfacePointer)member).Deffered = true;
            //		}
            // else if (memberClass.equals(JIDispatchImpl.class)) {
            //			((JIComObjectImplWrapper)member).getInterfacePointer().Deffered = true;
            //		}
            // else if (memberClass.equals(JIComObjectImpl.class)) {
            //			((IJIComObject)member).getInterfacePointer().Deffered = true;
            //		}
            else if (memberClass.Equals(typeof(IJIComObject))) {
                ((IJIComObject)member).internal_setDeffered(true);
            }
            //else the pointer will be serialized "inplace".
            Members.Insert(position, member);
        }

        /// <summary>
        /// Removes the member from the specified index. 
        /// </summary>
        /// <param name="index"> </param>
        public void removeMember(int index) {
            object member = Members.GetAndRemoveAt(index);
            if (member is JIArray) {
                //we need to remove it's max count values also.
                //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
                ArrayMaxCounts.removeAll(((JIArray)member).ConformantMaxCounts);

            }
            else if (member is JIStruct && ((JIStruct)member)._arrayAdded) {
                //we need to remove it's max count values also.
                //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
                ArrayMaxCounts.removeAll(((JIStruct)member).ArrayMaxCounts);
            }
            if (ArrayMaxCounts.Count == 0) {
                _arrayAdded = false;
            }
        }


        /// <summary>
        /// Returns all members as java.util.List. 
        /// </summary>
        public IList Members { get; } = new ArrayList();

        /// <summary>
        /// Retrieves the member at the specified index from the member list. 
        /// </summary>
        /// <param name="position"> Zero based index </param>
        public object getMember(int position) {
            return Members[position];
        }

        /// <summary>
        /// Returns the total number of members.
        /// </summary>
        public int Size => Members.Count;

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        internal void encode(NdrCodec ndr, IList defferedPointers, int FLAG) {
            //first write all Max counts and then the rest of the structs
            for (var i = 0; i < ArrayMaxCounts.Count; i++) {
                JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), (int?)ArrayMaxCounts[i], null, FLAG);
            }

            for (var i = 0; i < Members.Count; i++) {
                var o = Members[i];
                var conformant = false;
                if (o is JIArray arr1) {
                    //if this array is conformant then reset it's conformancy , since the length would have been
                    //written before.
                    conformant = arr1.Conformant;
                    arr1.Conformant = false;
                }
                JIMarshalUnMarshalHelper.serialize(ndr, o.GetType(), o, defferedPointers, FLAG);
                if (conformant && o is JIArray arr2) {
                    //noew reset this, so that next time when the same struct is written everything goes proper.
                    arr2.Conformant = true;
                }
            }
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal JIStruct decode(NdrCodec ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
            var retVal = new JIStruct();
            var listOfMaxCounts2 = new ArrayList();
            //first read all Max counts and then the rest of the structs
            for (var i = 0; i < _listOfDimensions.Count; i++) {
                for (var j = 0; j < (int)(int?)_listOfDimensions[i]; j++) {
                    listOfMaxCounts2.Add(JIMarshalUnMarshalHelper.deSerialize(ndr, typeof(int?), null, FLAG, additionalData));
                }
            }
            var i = 0;
            var j = 0; //index only for the conformant \ varying arrays
            while (i < Members.Count) {
                var o = Members[i];
                IList maxCountTemp = null;
                if (o is JIArray) {
                    if (((JIArray)o).Conformant || ((JIArray)o).Varying) {
                        //if this array is conformant then reset it's conformancy , since the length would have been
                        //read before.
                        ((JIArray)o).Conformant = false;
                        maxCountTemp = ((JIArray)o).ConformantMaxCounts;
                        ((JIArray)o).MaxCountAndUpperBounds = listOfMaxCounts2.subList(j, (int)(int?)_listOfDimensions[j]);
                        j++;
                    }
                }
                var o1 = JIMarshalUnMarshalHelper.deSerialize(ndr, o, defferedPointers, FLAG, additionalData);
                if (o is JIArray) {
                    if (((JIArray)o).Conformant || ((JIArray)o).Varying) {
                        //now reset this, so that next time when the same struct is written everything goes proper.
                        ((JIArray)o).Conformant = ((JIArray)o).Conformant;
                        ((JIArray)o).MaxCountAndUpperBounds = maxCountTemp;
                    }
                }
                try {
                    retVal.addMember(o1); //listOfMembers.add(o);
                }
                catch (JIException e) {
                    throw new JIRuntimeException(e.ErrorCode);
                }
                i++;
            }

            //do not copy other members since the addMember above will take care of all the conditions.
            return retVal;
        }

        /// <summary>
        /// Length
        /// </summary>
        internal int Length {
            get {
                var length = 0;
                var i = 0;
                while (i < Members.Count) {
                    var o = Members[i];
                    if (o is Type) {
                        length += JIMarshalUnMarshalHelper.getLengthInBytes((Type)o, o, JIFlags.FLAG_NULL);
                    }
                    else {
                        length += JIMarshalUnMarshalHelper.getLengthInBytes(o.GetType(), o, JIFlags.FLAG_NULL);
                    }
                    i++;
                }
                return length;
            }
        }

        /// <summary>
        /// Max counts
        /// </summary>
        internal IList ArrayMaxCounts { get; } = new ArrayList();

        /// <summary>
        /// Alignment
        /// </summary>
        internal int Alignment {
            get {
                var alignment = 0;

                for (var i = 0; i < Members.Count; i++) {
                    var c = Members[i].GetType();
                    var isClass = false;
                    if (c.Equals(typeof(Type))) {
                        isClass = true;
                        c = (Type)Members[i];
                    }

                    if (c.Equals(typeof(int?)) ||
                        c.Equals(typeof(float?)) ||
                        c.Equals(typeof(string)) ||
                        c.Equals(typeof(JIString)) ||
                        c.Equals(typeof(JIPointer)) ||
                        c.Equals(typeof(JIUnsignedInteger)) ||
                        c.Equals(typeof(JIVariant))) {
                        //align with 4 bytes
                        alignment = alignment <= 4 ? 4 : alignment;
                    }
                    else if (c.Equals(typeof(double?)) || c.Equals(typeof(DateTime)) || c.Equals(typeof(long?))) {
                        //align with 8
                        alignment = alignment <= 8 ? 8 : alignment;
                    }
                    else if (c.Equals(typeof(short?)) || c.Equals(typeof(JIUnsignedShort))) {
                        //align with 2
                        alignment = alignment <= 2 ? 2 : alignment;
                    }
                    else if (c.Equals(typeof(JIStruct))) {
                        if (!isClass) {
                            var align = ((JIStruct)Members[i]).Alignment;
                            alignment = alignment <= align ? align : alignment;
                        }
                    }
                    else if (c.Equals(typeof(JIUnion))) {
                        if (!isClass) {
                            var align = ((JIUnion)Members[i]).Alignment;
                            alignment = alignment <= align ? align : alignment;
                        }
                    }
                    if (alignment == 8) {
                        break;
                    }
                }

                return alignment;
            }
        }

        private IList _listOfDimensions = new ArrayList();
        private bool _arrayAdded;
    }
}