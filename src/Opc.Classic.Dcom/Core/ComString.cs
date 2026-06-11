// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Class representing a COM string. The Wide Char (<code>LPWSTR</code>) and the
/// <code>BSTR</code> are both encoded by the server in "UTF-16LE". This encoding
/// will be preserved by the library for all to and fro operations.
/// </summary>
[Serializable]
public sealed class ComString
{

    /// <summary>
    /// Represents <code><see cref="Variant"/></code> for this object,
    /// it is valid only if this object is a <code>BSTR</code>
    /// (<see cref="InteropFlags.FLAG_REPRESENTATION_STRING_BSTR" />) type.
    /// </summary>
    public readonly Variant Variant;

    /// <summary>
    /// Represents <code><see cref="Variant"/>(byRef = true)</code> for this object,
    /// it is valid only if this object is a <code>BSTR</code>
    /// (<see cref="InteropFlags.FLAG_REPRESENTATION_STRING_BSTR" />) type.
    /// </summary>
    public readonly Variant VariantByRef;

    /// <summary>
    /// Creates an object of the specified type. Used while deserialiazing
    /// this object.
    /// </summary>
    /// <param name="type"> <see cref="InteropFlags"/> string flags </param>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_BSTR"> </seealso>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR"> </seealso>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR"> </seealso>
    /// <exception cref="ArgumentException">
    /// if <code>type</code> is not a string flag.</exception>
    public ComString(int type)
    {
        Type = type;
        if (type == InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ||
            type == InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)
        {
            _member = new ComPointer(typeof(string), true);
        }
        else if (type == InteropFlags.FLAG_REPRESENTATION_STRING_BSTR)
        {
            _member = new ComPointer(typeof(string), false);
        }
        else
        {
            throw new ArgumentException(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_FLAG_ERROR), nameof(type));
        }
        Variant = null;
        VariantByRef = null;
        _member.SetFlags(type | InteropFlags.FLAG_REPRESENTATION_VALID_STRING);
    }

    /// <summary>
    /// Creates a string object of a given <code>type</code>.
    /// </summary>
    /// <param name="str"> value encapsulated by this object. </param>
    /// <param name="type"> <see cref="InteropFlags"/> string flags </param>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_BSTR"> </seealso>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR"> </seealso>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR"> </seealso>
    /// <exception cref="ArgumentException">
    /// if <code>type</code> is not a string flag. </exception>
    public ComString(string str, int type)
    {
        str = str ?? "";
        Type = type;
        if (type == InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ||
            type == InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)
        {
            _member = new ComPointer(str, true);
            Variant = null;
            VariantByRef = null;
        }
        else if (type == InteropFlags.FLAG_REPRESENTATION_STRING_BSTR)
        {
            _member = new ComPointer(str, false)
            {
                ReferentId = 0x72657355 // "User" in LEndian.
            };
            Variant = new Variant(this);
            VariantByRef = new Variant(this, true);
        }
        else
        {
            throw new ArgumentException(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_FLAG_ERROR), nameof(type));
        }
        _member.SetFlags(type | InteropFlags.FLAG_REPRESENTATION_VALID_STRING);

    }

    /// <summary>
    /// Creates a object of the <code>BSTR</code> type.
    /// </summary>
    /// <param name="str"> value encapsulated by this object. </param>
    public ComString(string str) :
        this(str, InteropFlags.FLAG_REPRESENTATION_STRING_BSTR)
    {
    }

    /// <summary>
    /// String encapsulated by this object. The encoding scheme
    /// for <code>LPWSTR</code> and <code>BSTR</code> strings is "UTF-16LE".
    /// </summary>
    public string String => _member.Referent?.ToString();

    /// <summary>
    /// Type representing this object.
    /// </summary>
    /// <returns> <see cref="InteropFlags"/> string flags </returns>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_BSTR"> </seealso>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR"> </seealso>
    /// <seealso cref="InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR"> </seealso>
    public int Type { get; } = InteropFlags.FLAG_NULL;

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    internal void Encode(NdrCodec ndr, CodecContext context)
    {
        var flags = context.Flag;
        try
        {
            context.Flag |= Type;
            MarshalUnMarshalHelper.Serialize(ndr, _member.GetType(), _member, context);
        }
        finally
        {
            context.Flag = flags;
        }
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>A new <see cref="ComString"/> instance built from <paramref name="ndr"/>.</returns>
    internal ComString Decode(NdrCodec ndr, CodecContext context)
    {
        var flags = context.Flag;
        try
        {
            context.Flag |= Type;
            var newString = new ComString(Type)
            {
                _member = (ComPointer)MarshalUnMarshalHelper.Deserialize(ndr, _member, context)
            };
            context.Flag = flags;
            return newString;
        }
        finally
        {
            context.Flag = flags;
        }
    }

    internal bool Deffered
    {
        set
        {
            // this condition is required so that only BSTRs are value
            // and also since this member could be value and
            // setting it to true would spoil the logic
            // this is incorrect logic in the bug sent by Kevin, the
            // ONEVENTSTRUCT consists of LPWSTRs which are value
            if (_member != null && !_member.Reference)
            {
                _member.Deffered = value;
            }
        }
        get => _member.Deffered;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        _member == null ? "[null]" : "[Type: " + Type + ", " + _member + "]";

    private ComPointer _member;
}
