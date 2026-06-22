// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Automation;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// Variant body
/// </summary>
[Serializable]
internal sealed class VariantBody
{
    public const short VT_PTR = 0x1A;
    public const short VT_SAFEARRAY = 0x1B;
    public const short VT_CARRAY = 0x1C;
    public const short VT_USERDEFINED = 0x1D;

    /// <summary>
    /// Is by ref
    /// </summary>
    internal bool IsByRef { get; }

    /// <summary>
    /// Body is null
    /// </summary>
    internal bool IsNull { get; }

    /// <summary>
    /// Variant type
    /// </summary>
    internal VariantType Type => IsArray ? VariantType.VT_ARRAY | _type : _type;

    /// <summary>
    /// Setting up a <code>VARIANT</code> with an object. Used via serializing the <code>VARIANT</code>.
    /// The class of the object determines its type.
    /// </summary>
    /// <param name="referent">NDR referent identifier used when the VARIANT body is marshaled by reference.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    internal VariantBody(object referent, bool isByRef) :
        this(referent, isByRef, (VariantType)(-1))
    {
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// <param name="referent">NDR referent identifier used when the VARIANT body is marshaled by reference.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    /// <param name="dataType">Type descriptor that determines how the value is marshaled.</param>
    private VariantBody(object referent, bool isByRef, VariantType dataType)
    {
        _object = referent ?? new Empty();

        if (_object is ComString comString &&
            comString.Type != InteropFlags.FLAG_REPRESENTATION_STRING_BSTR)
        {
            throw new InteropRuntimeException(ErrorCode.INTEROP_VARIANT_BSTR_ONLY);
        }
        if (_object is bool)
        {
            _flag = InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
        }

        IsByRef = isByRef;
        var types = Variant.GetSupportedType(_object, dataType);
        _type = types | (isByRef ? VariantType.VT_BYREF : VariantType.VT_EMPTY);
        Log.Logger.Verbose("In VariantBody(Object,bool,int) : dataType is " + dataType +
            ", referent class is " + _object.GetType() + ", byRef is " + isByRef);
        if (dataType == VariantType.VT_NULL)
        {
            IsNull = true;
            _object = 0;
        }
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a NULL value.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    internal VariantBody(Null value) : this(0, false)
    {
        ArgumentNullException.ThrowIfNull(value);
        IsNull = true;
        _type = VariantType.VT_NULL;
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a SCODE value and it's errorCode.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    internal VariantBody(Scode value, bool isByRef) :
        this(value.ErrorCode, isByRef)
    {
        _isScode = true;
        _type = VariantType.VT_ERROR;
    }

    /// <summary>
    /// Create safe array
    /// </summary>
    /// <param name="safeArray">SAFEARRAY payload stored by the VARIANT body.</param>
    /// <param name="nestedClass">Nested COM class metadata used when the VARIANT contains a structured value.</param>
    /// <param name="is2Dimensional">Value indicating whether the SAFEARRAY contains two dimensions.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    /// <param name="flag">Flag value that controls the requested operation.</param>
    internal VariantBody(Struct safeArray, Type nestedClass, bool is2Dimensional, bool isByRef, int flag = InteropFlags.FLAG_NULL)
    {
        _flag = flag;
        // can't convert the array here, since this will have deffered pointers which may not be complete.
        _safeArrayStruct = safeArray;
        IsArray = true;
        if (_safeArrayStruct == null)
        {
            IsNull = true;
        }
        _nestedArraysRealClass = nestedClass;
        _is2Dimensional = is2Dimensional;
        // please remember <see cref="Variant"/> is a pointer and VariantBody is just the referent part of that.
        // for an unsupported type this could be null
        // but then this is my bug, any thread entering this ctor, will support a type.
        IsByRef = isByRef;
        var types = Variant.GetSupportedType(nestedClass, flag);
        if (types != null)
        {
            _type = types.Value | (isByRef ? VariantType.VT_BYREF : VariantType.VT_EMPTY);
        }
        else
        {
            throw new InteropRuntimeException(ErrorCode.INTEROP_VARIANT_UNSUPPORTED_TYPE);
        }
    }

    /// <summary>
    /// Returns the contained object.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal object Object => _object ?? Array;

    /// <summary>
    /// Returns the array
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal ComArray Array
    {
        get
        {
            ComArray retVal = null;
            // TODO convert it to the right type based on the variantType before returning it.
            // everything is sent encapsulated in a variant(in safearray), so an Integer[] will
            // go as a variant array for each integer, only the variantType = arry of ints. so convert the
            // array in the right format before returning it to the user. That is he must get 
            // int[] within a ComArray back.
            if (_safeArrayStruct != null)
            {
                retVal = (ComArray)((ComPointer)_safeArrayStruct.GetMember(7)).Referent;

                if (_is2Dimensional)
                {
                    var obj3 = (Array)retVal.ArrayInstance; // these will all be variants
                                                            // correct the array here, i.e reform the 2 dimensional array before returning back.
                    var safeArrayBound = (ComArray)_safeArrayStruct.GetMember(8);

                    var safeArrayBound2 = (Struct[])safeArrayBound.ArrayInstance;
                    // should only be 2 since we support only 2 dim.

                    var firstDim = (int)safeArrayBound2[0].GetMember(0);
                    var secondDim = (int)safeArrayBound2[1].GetMember(0);

                    var obj2 = Variant.CreateSupportedJaggedArray(_nestedArraysRealClass, firstDim, secondDim);
                    var k = 0;
                    for (var i = 0; i < secondDim; i++)
                    {
                        for (var j = 0; j < firstDim; j++)
                        {
                            //                        if (nestedArraysRealClass == <see cref="Variant"/>.class)
                            //                        {
                            //                            obj2[j][i] = ((<see cref="Variant"/>[])obj3)[k++];
                            //                        }
                            //                        else
                            //                        {
                            //                            obj2[j][i] = ((<see cref="Variant"/>[])obj3)[k++].getObject();
                            //                        }
                            ((Array)obj2.GetValue(j)).SetValue(GetSafeArrayElementValue(obj3.GetValue(k++), _nestedArraysRealClass), i);
                        }
                    }

                    retVal = new ComArray(obj2);
                }
                else
                {
                    if (_nestedArraysRealClass != null)
                    {
                        var obj = (Array)retVal.ArrayInstance; // these will all be variants
                        var obj2 = Variant.CreateSupportedArray(_nestedArraysRealClass, obj.Length);
                        for (var i = 0; i < obj.Length; i++)
                        {
                            //                        if (nestedArraysRealClass == <see cref="Variant"/>.class)
                            //                        {
                            //                            Array.set(obj2,i,((<see cref="Variant"/>[])obj)[i]);// should be the native type
                            //                        }
                            //                        else
                            //                        {
                            //                            Array.set(obj2,i,((<see cref="Variant"/>[])obj)[i].getObject());// should be the native type
                            //                        }

                            // Array.set(obj2,i,obj[i]);
                            obj2.SetValue(GetSafeArrayElementValue(obj.GetValue(i), _nestedArraysRealClass), i);
                        }
                        retVal = new ComArray(obj2);
                    }
                    else
                    {
                        throw new InteropException(ErrorCode.INTEROP_VARIANT_UNSUPPORTED_TYPE);
                    }
                }
            }
            return retVal;
        }
    }

    /// <summary>
    /// Retrieves the contained object as int.
    /// </summary>
    internal int ObjectAsInt
    {
        get
        {
            try
            {
                return (int)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as long
    /// </summary>
    internal long ObjectAsLong
    {
        get
        {
            try
            {
                return (long)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as ulong
    /// </summary>
    internal ulong ObjectAsUlong
    {
        get
        {
            try
            {
                return (ulong)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as unsigned
    /// </summary>
    internal byte ObjectAsByte
    {
        get
        {
            try
            {
                return (byte)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as unsigned
    /// </summary>
    internal ushort ObjectAsUShort
    {
        get
        {
            try
            {
                return (ushort)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as unsigned
    /// </summary>
    internal uint ObjectAsUnsigned
    {
        get
        {
            try
            {
                return (uint)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as scode
    /// </summary>
    internal int ObjectAsSCODE
    {
        get
        {
            try
            {
                return ((Scode)_object).ErrorCode;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as float.
    /// </summary>
    internal float ObjectAsFloat
    {
        get
        {
            try
            {
                return (float)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as double.
    /// </summary>
    internal double ObjectAsDouble
    {
        get
        {
            try
            {
                return (double)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as short.
    /// </summary>
    internal short ObjectAsShort
    {
        get
        {
            try
            {
                return (short)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as bool.
    /// </summary>
    internal bool ObjectAsBoolean
    {
        get
        {
            try
            {
                return (bool)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as <see cref="ComString"/>.
    /// </summary>
    internal ComString ObjectAsString
    {
        get
        {
            try
            {
                return (ComString)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as Date.
    /// </summary>
    internal DateTime ObjectAsDate
    {
        get
        {
            try
            {
                return (DateTime)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as char.
    /// </summary>
    internal char ObjectAsChar
    {
        get
        {
            try
            {
                return (char)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the contained object as Variant.
    /// </summary>
    internal Variant ObjectAsVariant
    {
        get
        {
            try
            {
                return (Variant)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Returns the contained object as com object
    /// </summary>
    internal IComObject ObjectAsComObject
    {
        get
        {
            try
            {
                return (IComObject)_object;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }
    }

    /// <summary>
    /// Encode object
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    internal void Encode(NdrCodec ndr, CodecContext context)
    {
        // Start local decoder context
        var localContext = new CodecContext
        {
            ComObjects = context.ComObjects,
            CurrentSession = context.CurrentSession,
            Flag = context.Flag | _flag
        };

        // align with 8 boundary
        ndr.FillAligned(8);

        var start = ndr.Buffer.Index;

        // just a place holder for length
        ndr.WriteUnsignedLong(-1); // was 0xffffffff
        ndr.WriteUnsignedLong(0);

        // Type
        var varType = GetVarType(_object != null ? _object.GetType() : _nestedArraysRealClass, _object);

        // For IUnknown, since the inner object is a ComObjectImpl it will be fine.
        if ((localContext.Flag & InteropFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT) ==
                                 InteropFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT)
        {
            varType = IsByRef ? VariantType.VT_BYREF | VariantType.VT_DISPATCH : VariantType.VT_DISPATCH;
        }
        ndr.WriteUnsignedShort((int)varType);

        // reserved bytes
        ndr.WriteUnsignedSmall(0xCC);
        ndr.WriteUnsignedSmall(0xCC);
        ndr.WriteUnsignedSmall(0xCC);
        ndr.WriteUnsignedSmall(0xCC);
        ndr.WriteUnsignedSmall(0xCC);
        ndr.WriteUnsignedSmall(0xCC);

        if (_object != null)
        {
            ndr.WriteUnsignedLong((int)varType);
        }
        else
        {
            if (!IsByRef)
            {
                ndr.WriteUnsignedLong((int)VariantType.VT_ARRAY);
            }
            else
            {
                ndr.WriteUnsignedLong((int)VariantType.VT_BYREF_VT_ARRAY);
            }
        }

        if (IsByRef)
        {
            int byRefFlag;
            if (IsArray)
            { // object arrays will come here....
                byRefFlag = 4;
            }
            else
            {
                // no idea what these flags are but 0x10 is for variant, 0x8 for date, and 0x4 is for others
                if (_type == VariantType.VT_BYREF_VT_VARIANT)
                {
                    byRefFlag = 0x10;
                }
                else if (_type == VariantType.VT_BYREF_VT_DATE || _type == VariantType.VT_BYREF_VT_CY)
                {
                    byRefFlag = 8;
                }
                else
                {
                    byRefFlag = 4;
                }
            }
            ndr.WriteUnsignedLong(byRefFlag);
        }

        // we should use flag here, since the decision should be based on this only.
        SetValue(ndr, _object, localContext);

        // making changes to write the deffered pointers here itself, since we need to put the entire Variant completed to the length
        // as in varType.
        localContext.EncodeDeferredPointers(ndr, false);

        var currentIndex = ndr.Buffer.Index;
        ndr.Buffer.Index = start;
        ndr.FillAligned(8);
        ndr.Buffer.Index = currentIndex;
    }

    /// <summary>
    /// Returns the length in bytes
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <returns>The requested max length2 value.</returns>
    private int GetMaxLength2(Type c, object obj)
    {
        var length = 0;

        // since this is GetMaxLength2 and hence will either contain
        // proper type 3 elements and not EMPTY,NULL,SCODE since these are parts of Variant.
        // and not simple types like Integer or Float etc.
        if (kType3.Contains(c))
        {
            length = MarshalUnMarshalHelper.GetLengthInBytes(c, obj, _flag);
        }
        else if (c.Equals(typeof(long)) ||
            c.Equals(typeof(double)) ||
            c.Equals(typeof(DateTime)) ||
            c.Equals(typeof(Currency)))
        {
            length = 8;
        }
        else if (c.Equals(typeof(ComString)))
        {
            length = MarshalUnMarshalHelper.GetLengthInBytes(c, obj, _flag);
        }
        else if (obj is IComObject)
        {
            // for Interface pointers without
            double value = ((IComObjectInternal)obj).GetInterfacePointer().Length;
            value = value + 4 + 4 + 4; // 20 of variant, 4 of the ptr, 4 of max count, 4 of actual count
        }
        return length;
    }

    /// <summary>
    /// Get array length for var type
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    private int ArrayLengthForVarType
    {
        get
        {
            // now the array will be of variants, nestedArraysRealClass identifies the class itself
            // for iteration we need the variants and then there members.

            var objArray = (ComArray)((ComPointer)_safeArrayStruct.GetMember(7)).Referent;
            var array = (object[])objArray.ArrayInstance;

            var length = 20; // variant
            if (IsByRef)
            {
                length += 4; // byref
            }

            // SafeArray is 44
            length += 44;

            var isVariantArray = ((short)_safeArrayStruct.GetMember(1) & Variant.FADF_VARIANT) ==
                Variant.FADF_VARIANT;

            if (array != null)
            {
                length += 4; // for max count of the array.
                if (isVariantArray)
                {
                    // each variant is 3 (size 20 = 20/8 = 3)
                    for (var i = 0; i < array.Length; i++)
                    {
                        var variant = (Variant)array[i];
                        length += variant.GetLengthInBytes(_flag);
                    }

                    // now for the "user" pointer part
                    // length = length + array.length * 4;
                }
                else
                {
                    // normal non variant array has been sent...
                    for (var i = 0; i < array.Length; i++)
                    {
                        length += GetMaxLength2(array[i].GetType(), array[i]);
                    }
                }
            }
            else
            {
                length += 4; // for the null 0000.
            }
            var value = length / 8;
            if (length % 8 != 0)
            {
                value++;
            }
            return value;
        }
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>A new <see cref="VariantBody"/> instance built from <paramref name="ndr"/>.</returns>
    internal static VariantBody Decode(NdrCodec ndr, CodecContext context)
    {
        // Start local decoder context
        var localContext = new CodecContext
        {
            ComObjects = context.ComObjects,
            CurrentSession = context.CurrentSession,
            Flag = context.Flag
        };

        ndr.SkipAligned(8);
        var start = ndr.Buffer.Index;
        var length = ndr.ReadUnsignedLong(); // read the potential length
        ndr.ReadUnsignedLong(); // read the reserved byte

        var variantType = (VariantType)ndr.ReadUnsignedShort(); // varType

        // read reserved bytes
        ndr.ReadUnsignedShort();
        ndr.ReadUnsignedShort();
        ndr.ReadUnsignedShort();

        ndr.ReadUnsignedLong(); // 32 bit varType

        VariantBody variant;
        if ((variantType & VariantType.VT_ARRAY) == VariantType.VT_ARRAY)
        {
            var isByRef = (variantType & VariantType.VT_BYREF) != VariantType.VT_EMPTY;
            // the struct may be null if the array has nothing
            var safeArray = GetDecodedValueAsArray(ndr, variantType & ~VariantType.VT_ARRAY, isByRef, context);
            var type2 = variantType;
            if (isByRef)
            {
                type2 &= ~VariantType.VT_BYREF; // so that actual type can be determined
            }

            type2 &= VariantType.VT_TYPEMASK;
            if (type2 == VariantType.VT_INT)
            {
                localContext.Flag |= InteropFlags.FLAG_REPRESENTATION_VT_INT;
            }
            else
            {
                if (type2 == VariantType.VT_UINT)
                {
                    localContext.Flag |= InteropFlags.FLAG_REPRESENTATION_VT_UINT;
                }
                else
                {
                    if (type2 == VariantType.VT_BOOL)
                    {
                        localContext.Flag |= InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
                    }
                }
            }

            if (safeArray != null)
            {
                variant = new VariantBody(safeArray, Variant.GetSupportedClass(type2 & ~VariantType.VT_ARRAY),
                    ((object[])((ComArray)safeArray.GetMember(8)).ArrayInstance).Length > 1, isByRef, localContext.Flag);
            }
            else
            {
                variant = new VariantBody(null, Variant.GetSupportedClass(type2 & ~VariantType.VT_ARRAY),
                    false, isByRef, localContext.Flag);
            }

            variant._flag = localContext.Flag;
        }
        else
        {
            var isByRef = (variantType & VariantType.VT_BYREF) != VariantType.VT_EMPTY;
            variant = new VariantBody(GetDecodedValue(ndr, variantType, isByRef, localContext), isByRef, variantType);
            var type2 = variantType & VariantType.VT_TYPEMASK;
            if (type2 == VariantType.VT_INT)
            {
                variant._flag = InteropFlags.FLAG_REPRESENTATION_VT_INT;
            }
            else
            {
                if (type2 == VariantType.VT_UINT)
                {
                    variant._flag = InteropFlags.FLAG_REPRESENTATION_VT_UINT;
                }
            }
        }

        // Finally Decode all deferred pointers
        localContext.DecodeDeferredPointers(ndr);

        if (variant.IsArray && variant._safeArrayStruct != null)
        {
            // SafeArray have the alignment rule, that all Size <=4 are aligned by 4 and size 8 is aligned by 8.
            // Variant is aligned by 4, Interface pointers are aligned by 4 as well.
            // but this should not exceed the length
            var index = ndr.Buffer.Index;
            length = (length * 8) + start;
            if (index < length)
            {
                var safeArrayStruct = variant._safeArrayStruct;
                var size = (int)safeArrayStruct.GetMember(2);
                if (size == 8)
                {
                    ndr.SkipAligned(8);
                }
                else
                {
                    // align by 4...
                    // TODO this needs to be tested for Structs and Unions.
                    ndr.SkipAligned(4);
                }
            }

            ComArray array;

            // SafeArray is complete
            try
            {
                array = variant.Array;
            }
            catch (InteropException e)
            {
                throw new InteropRuntimeException(e.ErrorCode);
            }
            var variantMain = new Variant(array, variant.IsByRef, variant._flag);
            variant = (VariantBody)variantMain._member.Referent;
        }

        return variant;
    }

    /// <summary>
    /// Variants need specialised handling and the standard serializers may or maynot be used.
    /// </summary>
    /// <param name="type">COM or NDR type descriptor for the value being processed.</param>
    /// <returns>The requested var class value.</returns>
    private static Type GetVarClass(VariantType type)
    {
        // now first to check if this is a pointer or not.
        type &= VariantType.VT_TYPEMASK; // 0x4XXX & 0x0FFF = real type

        Type c;
        switch (type)
        {
            case VariantType.VT_EMPTY: // Not specified.
                c = typeof(Empty);
                break;
            case VariantType.VT_NULL: //  Null.
                c = typeof(Null);
                break;
            case VariantType.VT_ERROR:
                c = typeof(Scode); // Scode.
                break;
            default:
                c = Variant.GetSupportedClass(type);
                if (c == null)
                {
                    // TODO log this, what has come that i don't support.
                }
                break;
        }
        return c;
    }

    /// <summary>
    /// Get var type
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <returns>The requested var type value.</returns>
    private VariantType GetVarType(Type c, object obj)
    {
        var type = VariantType.VT_EMPTY;
        if (obj is IDispatch)
        {
            return IsByRef ? VariantType.VT_BYREF | VariantType.VT_DISPATCH : VariantType.VT_DISPATCH;
        }
        if (obj is IComObject)
        {
            return IsByRef ? VariantType.VT_BYREF | VariantType.VT_UNKNOWN : VariantType.VT_UNKNOWN;
        }
        if (c != null)
        {
            var type2 = Variant.GetSupportedType(c, _flag);
            if (type2 != null)
            {
                type = type2.Value;
            }
            else
            {
                Log.Logger.Warning("In getVarType: Unsupported Type found ! " + c +
                    ", please add this to the supportedType map ! ");
                // make that an array of variants
                type2 = Variant.GetSupportedType(typeof(Variant), _flag);
            }

            if (IsNull)
            {
                type = VariantType.VT_NULL;
            }
            else if (_isScode)
            {
                type = VariantType.VT_ERROR; // scode
            }
            else if (IsArray)
            {
                type = VariantType.VT_ARRAY | type;
            }
        }
        if (IsByRef && type != VariantType.VT_EMPTY && !c.Equals(typeof(ComArray)))
        {
            // then it is a pointer. have to set it correctly
            type |= VariantType.VT_BYREF;
        }
        return type;
    }

    /// <summary>
    /// Get decoded value
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="type">COM or NDR type descriptor for the value being processed.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>The requested decoded value value.</returns>
    private static object GetDecodedValue(NdrCodec ndr, VariantType type, bool isByRef,
        CodecContext context)
    {
        object obj = null;
        var c = GetVarClass(type);
        if (c != null)
        {
            if (isByRef)
            {
                ndr.ReadUnsignedLong(); // Read the Pointer
            }
            if (c.Equals(typeof(Scode)))
            {
                obj = MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context);
                obj = new Scode((int)obj);
            }
            else if (c.Equals(typeof(Null)))
            {
                // have read 20 bytes
                obj = Null.Value;
            }
            else if (c.Equals(typeof(Empty))) // empty is 20 bytes
            {
                obj = Empty.Value;
            }
            else if (c.Equals(typeof(ComString)))
            {
                obj = new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
                obj = ((ComString)obj).Decode(ndr, context);
            }
            else if (c.Equals(typeof(bool)))
            {
                var oldFlags = context.Flag;
                context.Flag |= InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
                obj = MarshalUnMarshalHelper.Deserialize(ndr, c, context);
                context.Flag = oldFlags;
            }
            else
            {
                obj = MarshalUnMarshalHelper.Deserialize(ndr, c, context);
            }
        }
        return obj;
    }

    /// <summary>
    /// Get decoded value
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="type">COM or NDR type descriptor for the value being processed.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>The requested decoded value as array value.</returns>
    private static Struct GetDecodedValueAsArray(NdrCodec ndr,
        VariantType type, bool isByRef, CodecContext context)
    {
        // int newFLAG = flag;
        if (isByRef)
        {
            ndr.ReadUnsignedLong(); // read the pointer
            type &= ~VariantType.VT_BYREF; // so that actual type can be determined
        }

        // read pointer referent id
        if (ndr.ReadUnsignedLong() == 0)
        {
            return null;
        }

        ndr.ReadUnsignedLong(); // 1

        var safeArray = new Struct();
        try
        {
            safeArray.AddMember(typeof(short)); // dim

            var safeArrayBound = new Struct();
            safeArrayBound.AddMember(typeof(int));
            safeArrayBound.AddMember(typeof(int)); // starts at 0

            safeArray.AddMember(typeof(short)); // flags
            safeArray.AddMember(typeof(int)); // size
            safeArray.AddMember(typeof(short)); // locks
            safeArray.AddMember(typeof(short)); // locks
            safeArray.AddMember(typeof(int)); // safearrayunion
            safeArray.AddMember(typeof(int)); // size in safearrayunion

            var c = Variant.GetSupportedClass(type);
            if (c == null)
            {
                Log.Logger.Warning("From Variant: while decoding an Array, type " + type + ", " +
                    "was not found in supportedTypes_classes map, hence using Variant instead...");
                // not available, lets try with <see cref="Variant"/>.
                // This is a bug, I should have the type.
                c = typeof(Variant);
            }

            // HARDCODING to <see cref="Variant"/>...kindof forgotten why I even wrote the code below.
            // since all of the examples I have come across always return a Variant array.
            // then why did I typify this thing to it's class (like <see cref="ComString"/>), it produces an
            // exception when the result is returned back is not an array of strings...
            // c = <see cref="Variant"/>.class;
            ComArray values = null;
            if (c == typeof(ComString))
            {
                values = new ComArray(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR), null, 1, true);
                safeArray.AddMember(new ComPointer(values)); // single dimension array, will convert it into the
                                                             // [] or [][] after inspecting dimension read.
            }
            else
            {
                values = new ComArray(c, null, 1, true);
                safeArray.AddMember(new ComPointer(values)); // single dimension array, will convert it into the
                                                             // [] or [][] after inspecting dimension read.
            }

            safeArray.AddMember(new ComArray(safeArrayBound, null, 1, true));

            var oldFlags = context.Flag;
            if (c == typeof(bool))
            {
                context.Flag |= InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
            }
            safeArray = (Struct)MarshalUnMarshalHelper.Deserialize(ndr, safeArray, context);
            context.Flag = oldFlags;

            // now set the right class after examining the flags, only set for <see cref="Variant"/>.class now., the BSTR would already be set previously.
            var features = (short)safeArray.GetMember(1);
            // this condition is being kept in the front since the feature flags can be a combination of FADF_VARIANT and the
            // other flags, in which case the Variant takes priority (since they will all be wrapped as variants).
            if ((features & Variant.FADF_VARIANT) == Variant.FADF_VARIANT)
            {
                values.UpdateType(typeof(Variant));
            }
            else if (((features & Variant.FADF_DISPATCH) == Variant.FADF_DISPATCH) ||
                    ((features & Variant.FADF_UNKNOWN) == Variant.FADF_UNKNOWN))
            {
                values.UpdateType(typeof(IComObject));
            }
            // For <see cref="ComString"/>s, it will be done before these above conditions are examined.
        }
        catch (InteropException e)
        {
            throw new InteropRuntimeException(e.ErrorCode);
        }
        return safeArray;
    }

    /// <summary>
    /// Set value
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    private void SetValue(NdrCodec ndr, object obj, CodecContext context)
    {
        if (IsNull)
        {
            return; // null, is only 20 bytes
        }
        if (obj != null)
        {
            var c = obj.GetType();

            if (c.Equals(typeof(Empty))) // 20 bytes
            {
                return;
            }
            if (obj is IComObject)
            {
                c = typeof(IComObject);
            }
            MarshalUnMarshalHelper.Serialize(ndr, c, obj, context);
        }
        else
        {
            ndr.WriteUnsignedLong(new object().GetHashCode()); // pointer referentId
            ndr.WriteUnsignedLong(1);

            MarshalUnMarshalHelper.Serialize(ndr, typeof(Struct), _safeArrayStruct, context);
        }
    }

    /// <summary>
    /// Whether body is in form of array
    /// </summary>
    internal bool IsArray { get; }

    /// <summary>
    /// Total length in bytes
    /// </summary>
    internal int LengthInBytes
    {
        get
        {
            if (_safeArrayStruct == null && _object.GetType().Equals(typeof(Empty)))
            {
                return 28;
            }

            if (IsArray)
            {
                int length;
                try
                {
                    length = ArrayLengthForVarType * 8;
                }
                catch (InteropException e)
                {
                    throw new InvalidOperationException("Unable to compute VARIANT array length.", e);
                }

                return length;
            }
            var c = _object.GetType();

            if (_object is IComObject)
            {
                c = typeof(IComObject);
            }
            else
            {
                if (c.Equals(typeof(Scode)))
                {
                    return 24 + 4; // 4 for integer scode.
                }
                if (c.Equals(typeof(Null)) || c.Equals(typeof(Empty)))
                {
                    return 24;
                }
            }
            return 24 + MarshalUnMarshalHelper.GetLengthInBytes(c, _object, _flag);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var retVal = "";
        if (_object == null)
        {
            retVal += "obj is null, ";
        }
        else
        {
            retVal += _object.ToString();
        }
        if (IsArray)
        {
            if (_is2Dimensional)
            {
                retVal += "2 dimensional array, ";
            }
            else
            {
                retVal = "1 dimensional array, ";
            }
            if (_safeArrayStruct != null)
            {
                retVal += _safeArrayStruct.ToString();
            }
        }

        return retVal;
    }

    private static object GetSafeArrayElementValue(object value, Type elementType)
    {
        if (elementType != typeof(Variant) && value is Variant variant)
        {
            return variant.Object;
        }

        return value;
    }

    private readonly bool _is2Dimensional;
    private readonly object _object;
    private readonly VariantType _type = (VariantType)(-1);
    private readonly Struct _safeArrayStruct;
    private readonly bool _isScode;
    private readonly Type _nestedArraysRealClass;
    internal int _flag;
    private static readonly List<Type> kType3 =
        new List<Type> {
            typeof(int),
            typeof(short),
            typeof(float),
            typeof(bool),
            typeof(char),
            typeof(sbyte),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(Empty),
            typeof(Null),
            typeof(Scode)
        };
}
