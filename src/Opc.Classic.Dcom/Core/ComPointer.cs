// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Representation of a COM pointer.
/// </summary>
[Serializable]
public sealed class ComPointer {

    /// <summary>
    /// Deferred
    /// </summary>
    internal bool Deffered { set; get; }

    /// <summary>
    /// Returns the referent identifier.
    /// </summary>
    /// <summary>
    /// Set referent id
    /// </summary>
    /// <param name="value"></param>
    public int ReferentId { get; internal set; } = -1;

    /// <summary>
    /// Returns the referent encapsulated by this pointer.
    /// </summary>
    /// <returns>Referent object</returns>
    public object Referent {
        get => IsNull ? null : _referent;
        internal set => _referent = value;
    }

    /// <summary>
    /// Returns status whether this is a reference type pointer
    /// or not.
    /// </summary>
    /// <returns> <code>true</code> if this is a reference type
    /// pointer. </returns>
    public bool Reference { get; private set; }

    /// <summary>
    /// Length
    /// </summary>
    internal int Length {
        get {
            // 4 for pointer
            if (IsNull) {
                return kPointerSize;
            }
            if (_referent is Type) {
                return 4 + MarshalUnMarshalHelper.GetLengthInBytes((Type)_referent,
                    _referent);
            }
            return 4 + MarshalUnMarshalHelper.GetLengthInBytes(_referent.GetType(),
                _referent);
        }
    }

    /// <summary>
    /// Returns status if this pointer is <code>null</code>.
    /// </summary>
    /// <returns> <code>true</code> if the pointer is
    /// <code>null</code>. </returns>
    public bool IsNull { get; private set; }

    /// <summary>
    /// Sets the flags associated with the referent.
    /// </summary>
    /// <param name="value"></param>
    internal void SetFlags(int value) => _flags = value;

    /// <summary>
    /// Set reference type pointer
    /// </summary>
    internal void SetIsReferenceTypePtr() => Reference = true;

    /// <summary>
    /// Create pointer
    /// </summary>
    private ComPointer() { }

    /// <summary>
    /// Creates an instance of this class where the referent is
    /// <code>value</code>.
    /// Used when serializing this pointer. This pointer is
    /// <b>not</b> of reference type.
    /// </summary>
    /// <param name="value"> </param>
    public ComPointer(object value) :
        this(value, false) {
    }

    /// <summary>
    /// Creates an instance of this class where the referent
    /// is of the type <code>value</code>. Used when deserializing
    /// this pointer.
    /// </summary>
    /// <param name="value"> <code>null</code> is acceptable </param>
    /// <param name="isReferenceTypePtr"> <code>true</code> if
    /// a referent identifier will not precede this ptr. </param>
    public ComPointer(Type value, bool isReferenceTypePtr) {
        // null pointer.
        if (value == null) {
            value = typeof(int);
            isReferenceTypePtr = true;
            IsNull = true;
        }
        // Should not defer since the enclosing struct, union, array
        // will defer it by itself.  this is important since, ptr to
        // a ptr to a ptr (and more) will need to deserialize completely
        // after the first deferement i.e they are not further deffered.
        _referent = value;
        Reference = isReferenceTypePtr;
    }

    /// <summary>
    /// Some COM servers send referentId (pointer) as null but
    /// the referent is not.
    /// To be used only when you know this is the case.
    /// Better leave it unsed.
    /// </summary>
    public void TreatNullSpecially() => _nullSpecial = true;

    /// <summary>
    /// Creates an instance of this class where the referent is
    /// <code>value</code>. Used when serializing this pointer.
    /// </summary>
    /// <param name="value"> <code>null</code> is acceptable
    /// </param>
    /// <param name="isReferenceTypePtr"> <code>true</code>
    /// if a referent Identifier will not precede this ptr. </param>
    public ComPointer(object value, bool isReferenceTypePtr) {
        if (value == null) {
            // since a null is being sent for a pointer,
            // it has to be shown as 0x0.
            value = 0;
            isReferenceTypePtr = true;
            IsNull = true;
        }

        // Should not defer since the enclosing struct, union, array
        // will defer it by itself.  this is important since, ptr to
        // a ptr to a ptr (and more) will need to deserialize completely
        // after the first deferement i.e they are not further deffered.
        _referent = value;
        ReferentId = new object().GetHashCode();
        Reference = isReferenceTypePtr;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="context"></param>
    internal void Encode(NdrCodec ndr, CodecContext context) {
        var oldFlags = context.Flag;
        try {
            context.Flag |= _flags;
            if (IsNull) {
                MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0, context);
                return;
            }
            // it is deffered or part of an array, this logic will not get called twice since the
            // deffered list will come in withb FLAG_NULL
            if (!IsNull && (Deffered ||
                (context.Flag & InteropFlags.FLAG_REPRESENTATION_ARRAY) == InteropFlags.FLAG_REPRESENTATION_ARRAY)) /*||
                (context.Flag & InteropFlags.FLAG_REPRESENTATION_NESTED_POINTER ) == InteropFlags.FLAG_REPRESENTATION_NESTED_POINTER*/
            {
                var referentIdToPut = ReferentId == -1 ? _referent.GetHashCode() : ReferentId;
                MarshalUnMarshalHelper.Serialize(ndr, typeof(int), referentIdToPut, context);
                Deffered = false;
                Reference = true;
                context.DefferedPointers.Add(this);
                return;
            }

            if (!IsNull && !Reference) {
                var referentIdToPut = ReferentId == -1 ?
                    _referent.GetHashCode() : ReferentId;
                MarshalUnMarshalHelper.Serialize(ndr, typeof(int), referentIdToPut, context);
            }
            try {
                if (!IsNull && _referent.GetType().Equals(typeof(Variant)) && ((Variant)_referent).IsArray) {
                    MarshalUnMarshalHelper.Serialize(
                        ndr, typeof(int), ((object[])((Variant)_referent).Object).Length, context);
                }
            }
            catch (InteropException e) {
                throw new InteropRuntimeException(e.ErrorCode);
            }
            MarshalUnMarshalHelper.Serialize(ndr, _referent.GetType(), _referent, context);
        }
        finally {
            // Restore flags
            context.Flag = oldFlags;
        }
    }


    /// <summary>
    /// class of type being decoded. If the type being expected is an array, the varType
    /// should be the actual array type and not ComArray.
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    internal ComPointer Decode(NdrCodec ndr, CodecContext context) {
        var oldFlags = context.Flag;
        try {
            context.Flag |= _flags;

            var retVal = new ComPointer {
                IsNull = IsNull,
                _nullSpecial = _nullSpecial
            };
            retVal.SetFlags(_flags);

            // retVal.isDeffered = isDeffered;
            if (Deffered ||
                (context.Flag & InteropFlags.FLAG_REPRESENTATION_ARRAY) == InteropFlags.FLAG_REPRESENTATION_ARRAY)
            /*|| (context.Flag & InteropFlags.FLAG_REPRESENTATION_NESTED_POINTER ) == InteropFlags.FLAG_REPRESENTATION_NESTED_POINTER */
            {
                retVal.ReferentId = (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context);
                retVal._referent = _referent; // will only be the class or object
                if (retVal.ReferentId == 0 && !_nullSpecial) {
                    // null pointer
                    // just return
                    retVal.IsNull = true;
                    retVal.Deffered = false;
                    return retVal;
                }

                retVal.Deffered = false;
                retVal.Reference = true;
                context.DefferedPointers.Add(retVal);
                return retVal;
            }

            if (!Reference) {
                // referentId = ndr.readUnsignedLong();
                retVal.ReferentId = (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context);
                retVal._referent = _referent; // will only be the class or object
                if (retVal.ReferentId == 0 && !_nullSpecial) {
                    // null pointer
                    // just return
                    retVal.IsNull = true;
                    return retVal;
                }
            }
            retVal._referent = MarshalUnMarshalHelper.Deserialize(ndr, _referent, context);
            return retVal;
        }
        finally {
            context.Flag = oldFlags;
        }
    }

    /// <summary>
    /// Internal replace
    /// </summary>
    /// <param name="replacement"></param>
    internal void ReplaceSelfWithNewPointer(ComPointer replacement) {
        Deffered = replacement.Deffered;
        IsNull = replacement.IsNull;
        Reference = replacement.Reference;
        _referent = replacement._referent;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        _referent == null ? "[null]" : "[" + _referent.ToString() + "]";

    private const int kPointerSize = 4;
    private bool _nullSpecial;
    private object _referent;
    private int _flags;
}
