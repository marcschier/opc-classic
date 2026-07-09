// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

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
public sealed class Union
{
    /// <summary>
    /// Returns the discriminant Vs there members Map.
    /// </summary>
    public IDictionary<object, object> Members { get; } = new Dictionary<object, object>();

    /// <summary>
    /// Private
    /// </summary>
    private Union()
    {
    }

    /// <summary>
    /// Creates an object with discriminant type specified. Used only during deserializing
    /// the union. Can only be of the type <code>Integer</code>,<code>Short</code>,<code>Boolean</code>
    /// or <code>Character</code>.
    /// </summary>
    /// <param name="discriminantClass">Union discriminant type used to choose the serialized arm.</param>
    /// <exception cref="ArgumentException"> if the
    /// <code>discriminantClass</code> is not of the type as specified
    /// above. </exception>
    public Union(Type discriminantClass)
    {
        // the discriminant can only be a int, bool or char

        if (!discriminantClass.Equals(typeof(int)) &&
            !discriminantClass.Equals(typeof(short)) &&
            !discriminantClass.Equals(typeof(bool)) &&
            !discriminantClass.Equals(typeof(char)))
        {
            // has to be from one of these. Rule from IDL.
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UNION_INCORRECT_DISC), nameof(discriminantClass));
        }
        _discriminantClass = discriminantClass;
    }

    /// <summary>
    /// Adds a member to this Union. The <code>member</code> is distinguished
    /// using the <code>discriminant</code>.
    /// </summary>
    /// <param name="discriminant">Union discriminant value that selects the member arm.</param>
    /// <param name="member">Structure or union member descriptor to register.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <exception cref="ArgumentException"> if any parameter is
    /// <code>null</code> </exception>
    public void AddMember(object discriminant, object member)
    {
        if (discriminant == null || member == null)
        {
            throw new ArgumentException(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_UNION_NULL_DISCRMINANT), nameof(discriminant));
        }
        if (!discriminant.GetType().Equals(_discriminantClass))
        {
            throw new InteropException(ErrorCode.INTEROP_UNION_DISCRMINANT_MISMATCH);
        }
        if (member.GetType().Equals(typeof(ComPointer)) && !((ComPointer)member).Reference)
        {
            ((ComPointer)member).Deffered = true;
        }
        else if (member.GetType().Equals(typeof(ComString)))
        {
            ((ComString)member).Deffered = true;
        }
        Members[discriminant] = member;
    }

    /// <summary>
    /// Adds a member to this Union. The <code>member</code> is distinguished
    /// using the <code>discriminant</code>.
    /// </summary>
    /// <param name="discriminant">Union discriminant value that selects the member arm.</param>
    /// <param name="member">Structure or union member descriptor to register.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <exception cref="ArgumentException"> if <code>discriminant</code>
    /// is <code>null</code> </exception>
    public void AddMember(object discriminant, Struct member)
    {
        if (discriminant == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UNION_NULL_DISCRMINANT), nameof(discriminant));
        }
        if (!discriminant.GetType().Equals(_discriminantClass))
        {
            throw new InteropException(ErrorCode.INTEROP_UNION_DISCRMINANT_MISMATCH);
        }
        if (member == null)
        {
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
    /// <param name="discriminant">Union discriminant value whose member arm should be removed.</param>
    public void RemoveMember(object discriminant) => Members.Remove(discriminant);

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    internal void Encode(NdrCodec ndr, CodecContext context)
    {
        if (Members.Count == 0 || Members.Count > 1)
        {
            throw new InteropRuntimeException((int)
                ErrorCode.INTEROP_UNION_DISCRMINANT_SERIALIZATION_ERROR);
        }
        // first write the discriminant and then the member
        var entry = Members.First();
        MarshalUnMarshalHelper.Serialize(ndr, _discriminantClass, entry.Key, context);

        var value = entry.Value;

        // will not write empty union members
        if (!value.Equals(Struct.MEMBER_IS_EMPTY))
        {
            MarshalUnMarshalHelper.Serialize(ndr, value.GetType(), value, context);
        }
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>A new <see cref="Union"/> instance built from <paramref name="ndr"/>.</returns>
    internal Union Decode(NdrCodec ndr, CodecContext context)
    {
        // first read discriminant, and then call the appropriate deserializer of the member
        if (Members.Count == 0)
        {
            throw new InteropRuntimeException(ErrorCode.INTEROP_UNION_DISCRMINANT_DESERIALIZATION_ERROR);
        }
        // first write the discriminant and then the member
        var retVal = new Union
        {
            _discriminantClass = _discriminantClass
        };
        var key = MarshalUnMarshalHelper.Deserialize(ndr, _discriminantClass, context);
        // next thing to be deserialized is the member
        var value = Members[key];
        // should allow null since this could be a "default"
        if (value == null)
        {
            value = Struct.MEMBER_IS_EMPTY;
        }
        // will not write empty union members
        if (!value.Equals(Struct.MEMBER_IS_EMPTY))
        {
            retVal.Members[key] = MarshalUnMarshalHelper.Deserialize(ndr, value, context);
        }
        else
        {
            retVal.Members[key] = value;
        }
        return retVal;
    }

    /// <summary>
    /// Length
    /// </summary>
    internal int Length
    {
        get
        {
            var length = 0;
            foreach (var o in Members.Keys)
            {
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
    internal int Alignment
    {
        get
        {
            var alignment = 0;
            if (_discriminantClass.Equals(typeof(int)))
            {
                // align with 4 bytes
                alignment = 4;
            }
            else if (_discriminantClass.Equals(typeof(short)))
            {
                // align with 2
                alignment = 2;
            }
            return alignment;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => "[members: " + Members.Count + "]";

    private Type _discriminantClass;
}
