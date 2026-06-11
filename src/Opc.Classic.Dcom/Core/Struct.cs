// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common.Ntlm;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// This class represents the <code>Struct</code> data type.
/// for conformant and conformant+varying arrays the maxcount
/// etc. should come at the begining of the struct.
/// </summary>
[Serializable]
public sealed class Struct
{

    /// <summary>
    /// Empty struct
    /// </summary>
    public static readonly Struct MEMBER_IS_EMPTY = new Struct();

    /// <summary>
    /// Returns the total number of members.
    /// </summary>
    public int Size => Members.Count;

    /// <summary>
    /// Returns all members as java.util.List.
    /// </summary>
    public IList<object> Members { get; } = new List<object>();

    /// <summary>
    /// Length
    /// </summary>
    internal int Length
    {
        get
        {
            var length = 0;
            var i = 0;
            while (i < Members.Count)
            {
                var o = Members[i];
                if (o is Type typeObj)
                {
                    length += MarshalUnMarshalHelper.GetLengthInBytes(typeObj, o);
                }
                else
                {
                    length += MarshalUnMarshalHelper.GetLengthInBytes(o.GetType(), o);
                }
                i++;
            }
            return length;
        }
    }

    /// <summary>
    /// Max counts
    /// </summary>
    internal List<int> ArrayMaxCounts { get; } = new List<int>();

    /// <summary>
    /// Alignment
    /// </summary>
    internal int Alignment
    {
        get
        {
            var alignment = 0;

            for (var i = 0; i < Members.Count; i++)
            {
                var memberObj = Members[i];
                Type c;
                if (memberObj is Type type)
                {
                    c = type;
                }
                else
                {
                    c = memberObj.GetType();
                }

                if (c.Equals(typeof(int)) ||
                    c.Equals(typeof(float)) ||
                    c.Equals(typeof(string)) ||
                    c.Equals(typeof(ComString)) ||
                    c.Equals(typeof(ComPointer)) ||
                    c.Equals(typeof(uint)) ||
                    c.Equals(typeof(Variant)))
                {
                    // align with 4 bytes
                    alignment = alignment <= 4 ? 4 : alignment;
                }
                else if (c.Equals(typeof(double)) ||
                         c.Equals(typeof(DateTime)) ||
                         c.Equals(typeof(long)) ||
                         c.Equals(typeof(ulong)))
                {
                    // align with 8
                    alignment = alignment <= 8 ? 8 : alignment;
                }
                else if (c.Equals(typeof(short)) ||
                         c.Equals(typeof(ushort)))
                {
                    // align with 2
                    alignment = alignment <= 2 ? 2 : alignment;
                }
                else if (c.Equals(typeof(Struct)))
                {
                    if (Members[i] is Struct structMember)
                    {
                        var align = structMember.Alignment;
                        alignment = alignment <= align ? align : alignment;
                    }
                }
                else if (c.Equals(typeof(Union)))
                {
                    if (Members[i] is Union unionMember)
                    {
                        var align = unionMember.Alignment;
                        alignment = alignment <= align ? align : alignment;
                    }
                }
                if (alignment == 8)
                {
                    break;
                }
            }

            return alignment;
        }
    }

    /// <summary>
    /// Retrieves the member at the specified index from the member list.
    /// </summary>
    /// <param name="position"> Zero based index </param>
    public object GetMember(int position) => Members[position];

    /// <summary>
    /// Adds the object as a member of this structure. This object
    /// is appended to the list of members within.
    /// Note that null has to be allowed for members who would like
    /// to send null...NPE should not be thrown
    /// </summary>
    /// <param name="member"> </param>
    /// <exception cref="InteropException"></exception>
    public void AddMember(object member) => AddMember(Members.Count, member);

    /// <summary>
    /// Adds object as member to the index specified.
    /// </summary>
    /// <param name="position"> Zero based index </param>
    /// <param name="member"> </param>
    /// <exception cref="InteropException"></exception>
    public void AddMember(int position, object member)
    {
        // null has to be allowed for members who would like to send null...NPE should not be thrown
        member = member ?? 0;
        var memberClass = member.GetType();
        // An array has already been added, now a new member cannot be added
        if (_arrayAdded && position == Members.Count && !memberClass.Equals(typeof(ComArray)))
        {
            throw new InteropException(ErrorCode.INTEROP_STRUCT_ARRAY_AT_END);
        }

        // arrays can only be the last element of this struct.
        if (memberClass.Equals(typeof(ComArray)))
        {
            // this condition will also allow that if another nested struct has an array,
            // this new array is added at the
            // very end.
            if (position != Members.Count)
            {
                throw new InteropException(ErrorCode.INTEROP_STRUCT_ARRAY_ONLY_AT_END);
            }
            _arrayAdded = true;
            // Fixed arrays like char[50] are serialzed\deserialized in place itself.
            if (((ComArray)member).Conformant || ((ComArray)member).Varying)
            {
                // since there could be two arrays.
                ArrayMaxCounts.AddRange(((ComArray)member).ConformantMaxCounts);
                _listOfDimensions.Add(((ComArray)member).Dimensions);
            }
        }

        // struct part of another struct
        if (memberClass.Equals(typeof(Struct)))
        {
            // if this has an array then, this struct has to be the last member in the struct list.
            if (member is Struct structMember)
            {
                if (structMember._arrayAdded && _arrayAdded && position != (Members.Count - 1))
                {
                    throw new InteropException(ErrorCode.INTEROP_STRUCT_INCORRECT_NESTED_STRUCT_POS);
                }

                if (_arrayAdded && structMember._arrayAdded)
                {
                    // means that we have to move the maxcount of the internal struct to this struct.
                    _arrayAdded = true;
                    ArrayMaxCounts.AddRange(structMember.ArrayMaxCounts);
                    structMember.ArrayMaxCounts.Clear(); // this is a "move" of max counts to the
                                                         // outer struct

                    _listOfDimensions.AddRange(structMember._listOfDimensions);
                    structMember._listOfDimensions.Clear();

                }
                else if (!_arrayAdded && structMember._arrayAdded)
                {
                    if (position == Members.Count)
                    {
                        _arrayAdded = true;
                        ArrayMaxCounts.AddRange(structMember.ArrayMaxCounts);
                        structMember.ArrayMaxCounts.Clear(); // this is a "move" of max counts to the
                                                             // outer struct

                        _listOfDimensions.AddRange(structMember._listOfDimensions);
                        structMember._listOfDimensions.Clear();
                    }
                    else
                    {
                        throw new InteropException(ErrorCode.INTEROP_STRUCT_INCORRECT_NESTED_STRUCT_POS2);
                    }
                }
            }
        }
        if (memberClass.Equals(typeof(ComPointer)) && !((ComPointer)member).Reference)
        {
            // send this to the end and put the place holder of the pointer here
            ((ComPointer)member).Deffered = true;
        }
        else if (memberClass.Equals(typeof(Variant)))
        {
            ((Variant)member).Deffered = true;
        }
        else if (memberClass.Equals(typeof(ComString)))
        {
            ((ComString)member).Deffered = true;
        }
        // else if (memberClass.equals(<see cref="InterfacePointer"/>.class)) {
        //            ((<see cref="InterfacePointer"/>)member).Deffered = true;
        //        }
        // else if (memberClass.equals(DispatchImpl.class)) {
        //            ((ComObjectImplWrapper)member).getInterfacePointer().Deffered = true;
        //        }
        // else if (memberClass.equals(ComObjectImpl.class)) {
        //            ((<see cref="IComObject"/>)member).getInterfacePointer().Deffered = true;
        //        }
        else if (memberClass.Equals(typeof(IComObject)))
        {
            ((IComObjectInternal)member).SetDeffered(true);
        }
        // else the pointer will be serialized "inplace".
        Members.Insert(position, member);
    }

    /// <summary>
    /// Removes the member from the specified index.
    /// </summary>
    /// <param name="index"> </param>
    public void RemoveMember(int index)
    {
        var member = Members.GetAndRemoveAt(index);
        if (member is ComArray comArray)
        {
            // we need to remove it's max count values also.
            ArrayMaxCounts.RemoveAll(comArray.ConformantMaxCounts);

        }
        else if (member is Struct structMember && structMember._arrayAdded)
        {
            // we need to remove it's max count values also.
            ArrayMaxCounts.RemoveAll(structMember.ArrayMaxCounts);
        }
        if (ArrayMaxCounts.Count == 0)
        {
            _arrayAdded = false;
        }
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="context"></param>
    internal void Encode(NdrCodec ndr, CodecContext context)
    {
        // first write all Max counts and then the rest of the structs
        for (var i = 0; i < ArrayMaxCounts.Count; i++)
        {
            MarshalUnMarshalHelper.Serialize(ndr, typeof(int), ArrayMaxCounts[i], context);
        }

        for (var i = 0; i < Members.Count; i++)
        {
            var o = Members[i];
            var conformant = false;
            if (o is ComArray arr1)
            {
                // if this array is conformant then reset it's conformancy, since the length would have been
                // written before.
                conformant = arr1.Conformant;
                arr1.Conformant = false;
            }
            MarshalUnMarshalHelper.Serialize(ndr, o.GetType(), o, context);
            if (conformant && o is ComArray arr2)
            {
                // noew reset this, so that next time when the same struct is written everything goes proper.
                arr2.Conformant = true;
            }
        }
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    internal Struct Decode(NdrCodec ndr, CodecContext context)
    {
        var retVal = new Struct();
        var listOfMaxCounts2 = new List<int>();
        // first read all Max counts and then the rest of the structs
        int j;
        int i;
        for (i = 0; i < _listOfDimensions.Count; i++)
        {
            for (j = 0; j < _listOfDimensions[i]; j++)
            {
                listOfMaxCounts2.Add((int)MarshalUnMarshalHelper.Deserialize(ndr,
                    typeof(int), context));
            }
        }
        i = 0;
        j = 0; // index only for the conformant \ varying arrays
        while (i < Members.Count)
        {
            var o = Members[i];
            List<int> maxCountTemp = null;
            if (o is ComArray comArray)
            {
                if (comArray.Conformant || comArray.Varying)
                {
                    // if this array is conformant then reset it's conformancy, since the length would have been
                    // read before.
                    comArray.Conformant = false;
                    maxCountTemp = comArray.ConformantMaxCounts;
                    comArray.MaxCountAndUpperBounds = listOfMaxCounts2.SubList(j, _listOfDimensions[j]).ToList();
                    j++;
                }
            }
            var o1 = MarshalUnMarshalHelper.Deserialize(ndr, o, context);
            if (o is ComArray comArray2)
            {
                if (comArray2.Conformant || comArray2.Varying)
                {
                    // now reset this, so that next time when the same struct is written everything goes proper.
                    comArray2.Conformant = comArray2.Conformant;
                    comArray2.MaxCountAndUpperBounds = maxCountTemp;
                }
            }
            try
            {
                retVal.AddMember(o1); // listOfMembers.add(o);
            }
            catch (InteropException e)
            {
                throw new InteropRuntimeException(e.ErrorCode);
            }
            i++;
        }

        // do not copy other members since the addMember above will take care of all the conditions.
        return retVal;
    }

    private readonly List<int> _listOfDimensions = new List<int>();
    public override string ToString() => "[" + string.Join(", ", Members) + "]";

    private bool _arrayAdded;
}
