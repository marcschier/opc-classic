// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Automation;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Reflection;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// Class representing the <code>VARIANT</code> datatype. Please use the
/// <code>byRef</code> flag based constructors for <i>by reference</i>
/// parameters in COM calls. For <code>[optional]</code> parameters use the
/// <seealso cref="CreateOPTIONAL_PARAM()"/>
/// </summary>
/// <remarks>
/// In case of direct calls to COM server using <code>CallBuilder</code>,
/// if the <code>byRef</code> flag is set then that variant should also be
/// added as the <code>[out]</code> parameter in the <code>CallBuilder</code>.
/// For developers using the <code><see cref="IDispatch"/> </code> this is not required and
/// variant would be returned back to them via <code><see cref="Variant"/>[]</code>
/// associated with <code><see cref="IDispatch"/></code> apis. An <b>important</b> note for
/// <code>Boolean</code> Arrays (<code>ComArray</code> of <code>Boolean</code>),
/// please set the <see cref="InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL"/> using the
/// <seealso cref="Flag"/> method before making a call on this object. This is
/// required since in DCOM, <code>VARIANT_BOOL</code> are 2 bytes and standard
/// <code>bool</code>s are 1 byte in length.
/// </remarks>
[Serializable]
public sealed class Variant
{

    /// <summary> array is allocated on the stack </summary>
    public const int FADF_AUTO = 0x0001;
    /// <summary> array is staticly allocated </summary>
    public const int FADF_STATIC = 0x0002;
    /// <summary> array is embedded in a structure </summary>
    public const int FADF_EMBEDDED = 0x0004;
    /// <summary> may not be resized or reallocated </summary>
    public const int FADF_FIXEDSIZE = 0x0010;
    /// <summary>  an array of records </summary>
    public const int FADF_RECORD = 0x0020;
    /// <summary> with FADF_DISPATCH, FADF_UNKNOWN </summary>
    public const int FADF_HAVEIID = 0x0040;
    /// <summary> array has a VT type </summary>
    public const int FADF_HAVEVARTYPE = 0x0080;
    /// <summary> an array of BSTRs </summary>
    public const int FADF_BSTR = 0x0100;
    /// <summary> an array of IUnknown* </summary>
    public const int FADF_UNKNOWN = 0x0200;
    /// <summary> an array of IDispatch* </summary>
    public const int FADF_DISPATCH = 0x0400;
    /// <summary> an array of VARIANTs </summary>
    public const int FADF_VARIANT = 0x0800;
    /// <summary> reserved bits </summary>
    public const int FADF_RESERVED = 0xF008;


    /// <summary>
    /// Get out param for type
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="isArray">Value indicating whether the variant stores an array value.</param>
    /// <returns>A by-reference VARIANT suitable for an Automation out parameter of the requested type.</returns>
    public static Variant OUTPARAMforType(Type c, bool isArray)
    {
        Variant variant = null;
        if (!isArray)
        {
            try
            {
                variant = MakeVariant(_outTypesMap.GetOrDefault(c), true);
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
            {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            if (c == typeof(IDispatch))
            {
                return CreateOUT_IDISPATCH();
            }
            if (c == typeof(IComObject))
            {
                return CreateOUT_IUNKNOWN();
            }
            if (c == typeof(Variant))
            {
                return CreateEMPTY_BYREF();
            }
            if (c == typeof(ComString))
            {
                return new Variant("", true);
            }
        }
        else
        {
            try
            {
                var oo = _outTypesMap.GetOrDefault(c);
                if (oo != null)
                {
                    // we will always send a single dimension array.
                    object x = CreateSupportedArray(c, 1);
                    ((Array)x).SetValue(oo, 0);
                    variant = new Variant(new ComArray(x, true), true);
                }
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
            {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            if (c == typeof(IDispatch))
            {
                IComObject[] arry = { new ComObjectImpl(null, new InterfacePointer(null, -1, null)) };
                variant = new Variant(new ComArray(arry, true), true)
                {
                    Flag =
                        InteropFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT |
                        InteropFlags.FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT
                };
            }
            else if (c == typeof(IComObject))
            {
                IComObject[] arry = { new ComObjectImpl(null, new InterfacePointer(null, -1, null)) };
                variant = new Variant(new ComArray(arry, true), true)
                {
                    Flag =
                        InteropFlags.FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT |
                        InteropFlags.FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT
                };
            }
            else
            {
                if (c == typeof(Variant))
                {
                    return CreateVARIANTARRAY();
                }
                if (c == typeof(ComString) || c == typeof(string))
                {
                    return CreateBSTRARRAY();
                }
            }
        }
        return variant;
    }

    /// <summary>
    /// Returns a <see cref="Variant"/> (of the right type) based on the <code>o.getClass()</code>
    /// </summary>
    /// <param name="o">Object instance being compared, marshaled, or converted.</param>
    /// <param name="isByRef">
    /// </param>
    public static Variant MakeVariant(object o, bool isByRef = false)
    {
        if (o == null || o.GetType() == typeof(object))
        {
            if (isByRef)
            {
                return CreateEMPTY_BYREF();
            }
            return CreateEMPTY();
        }
        var c = o.GetType();
        if (c.IsArray)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_VARIANT_ONLY_COMARRAY_EXCEPTED), nameof(o));
        }
        if (c == typeof(Variant))
        {
            return new Variant((Variant)o);
        }
        try
        {
            ConstructorInfo ctor = null;
            // now we look at the class and return a <see cref="Variant"/>.
            if (c == typeof(bool))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(bool), typeof(bool) });
            }
            else if (c == typeof(char))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(char), typeof(bool) });
            }
            else if (c == typeof(sbyte))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(sbyte), typeof(bool) });
            }
            else if (c == typeof(byte))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(byte), typeof(bool) });
            }
            else if (c == typeof(short))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(short), typeof(bool) });
            }
            else if (c == typeof(ushort))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(ushort), typeof(bool) });
            }
            else if (c == typeof(int))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(int), typeof(bool) });
            }
            else if (c == typeof(uint))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(uint), typeof(bool) });
            }
            else if (c == typeof(long))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(long), typeof(bool) });
            }
            else if (c == typeof(ulong))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(ulong), typeof(bool) });
            }
            else if (c == typeof(float))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(float), typeof(bool) });
            }
            else if (c == typeof(double))
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(double), typeof(bool) });
            }
            else if (o is IComObject)
            {
                ctor = typeof(Variant).GetConstructor(new Type[] { typeof(IComObject), typeof(bool) });
            }
            else
            {
                // should cover all the rest cases.
                ctor = typeof(Variant).GetConstructor(new Type[] { c, typeof(bool) });
            }
            // TODO N1.2-followup: replace reflective Variant construction with a generated factory table.
            return (Variant)ctor.Invoke(new object[] { o, Convert.ToBoolean(isByRef) });
        }
        catch (Exception e)
        {
            Log.Logger.Warning(e, "Could not create Variant for " + o + ", isByRef " + isByRef);
        }
        return null;
    }

    /// <summary>
    /// Get supported type
    /// </summary>
    /// <param name="type">COM or NDR type descriptor for the value being processed.</param>
    /// <returns>The requested supported class value.</returns>
    internal static Type GetSupportedClass(VariantType type) =>
        _supportedTypes_classes.GetOrDefault(type);

    internal static Array CreateSupportedArray(Type elementType, int length)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        if (elementType == typeof(object))
        {
            return new object[length];
        }
        if (elementType == typeof(string))
        {
            return new string[length];
        }
        if (elementType == typeof(sbyte))
        {
            return new sbyte[length];
        }
        if (elementType == typeof(char))
        {
            return new char[length];
        }
        if (elementType == typeof(decimal))
        {
            return new decimal[length];
        }
        if (elementType == typeof(IDispatch))
        {
            return new IDispatch[length];
        }
        if (elementType == typeof(IComObject))
        {
            return new IComObject[length];
        }

        var variantType = GetSupportedType(elementType);
        if (variantType == null)
        {
            throw new NotSupportedException($"Type {elementType} is not supported in VARIANT array context.");
        }

        return CreateSupportedArray(variantType.Value, length);
    }

    internal static Array CreateSupportedJaggedArray(Type elementType, int firstLength, int secondLength)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        if (elementType == typeof(object))
        {
            return CreateJaggedArray<object>(firstLength, secondLength);
        }
        if (elementType == typeof(string))
        {
            return CreateJaggedArray<string>(firstLength, secondLength);
        }
        if (elementType == typeof(sbyte))
        {
            return CreateJaggedArray<sbyte>(firstLength, secondLength);
        }
        if (elementType == typeof(char))
        {
            return CreateJaggedArray<char>(firstLength, secondLength);
        }
        if (elementType == typeof(decimal))
        {
            return CreateJaggedArray<decimal>(firstLength, secondLength);
        }
        if (elementType == typeof(IDispatch))
        {
            return CreateJaggedArray<IDispatch>(firstLength, secondLength);
        }
        if (elementType == typeof(IComObject))
        {
            return CreateJaggedArray<IComObject>(firstLength, secondLength);
        }

        var variantType = GetSupportedType(elementType);
        if (variantType == null)
        {
            throw new NotSupportedException($"Type {elementType} is not supported in VARIANT array context.");
        }

        return CreateSupportedJaggedArray(variantType.Value, firstLength, secondLength);
    }

    private static object[] CreateSupportedReferenceArray(Type elementType, int length)
    {
        var array = CreateSupportedArray(elementType, length);
        if (array is object[] objectArray)
        {
            return objectArray;
        }

        throw new NotSupportedException($"Type {elementType} is not supported in object-backed VARIANT array context.");
    }

    private static Array CreateSupportedArray(VariantType variantType, int length)
    {
        var vt = variantType & VariantType.VT_TYPEMASK;
        return vt switch
        {
            VariantType.VT_EMPTY => new Empty[length],
            VariantType.VT_NULL => new Null[length],
            VariantType.VT_I2 => new short[length],
            VariantType.VT_I4 or VariantType.VT_INT => new int[length],
            VariantType.VT_R4 => new float[length],
            VariantType.VT_R8 => new double[length],
            VariantType.VT_CY => new Currency[length],
            VariantType.VT_DATE => new DateTime[length],
            VariantType.VT_BSTR => new ComString[length],
            VariantType.VT_DISPATCH => new IComObject[length],
            VariantType.VT_ERROR => new Scode[length],
            VariantType.VT_BOOL => new bool[length],
            VariantType.VT_VARIANT => new Variant[length],
            VariantType.VT_UNKNOWN => new IComObject[length],
            VariantType.VT_DECIMAL => new decimal[length],
            VariantType.VT_I1 => new char[length],
            VariantType.VT_UI1 => new byte[length],
            VariantType.VT_UI2 => new ushort[length],
            VariantType.VT_UI4 or VariantType.VT_UINT => new uint[length],
            VariantType.VT_I8 => new long[length],
            VariantType.VT_UI8 => new ulong[length],
            VariantType.VT_ARRAY => new ComArray[length],
            _ => throw new NotSupportedException($"VT_* {vt} is not supported in VARIANT array context.")
        };
    }

    private static Array CreateSupportedJaggedArray(VariantType variantType, int firstLength, int secondLength)
    {
        var vt = variantType & VariantType.VT_TYPEMASK;
        return vt switch
        {
            VariantType.VT_EMPTY => CreateJaggedArray<Empty>(firstLength, secondLength),
            VariantType.VT_NULL => CreateJaggedArray<Null>(firstLength, secondLength),
            VariantType.VT_I2 => CreateJaggedArray<short>(firstLength, secondLength),
            VariantType.VT_I4 or VariantType.VT_INT => CreateJaggedArray<int>(firstLength, secondLength),
            VariantType.VT_R4 => CreateJaggedArray<float>(firstLength, secondLength),
            VariantType.VT_R8 => CreateJaggedArray<double>(firstLength, secondLength),
            VariantType.VT_CY => CreateJaggedArray<Currency>(firstLength, secondLength),
            VariantType.VT_DATE => CreateJaggedArray<DateTime>(firstLength, secondLength),
            VariantType.VT_BSTR => CreateJaggedArray<ComString>(firstLength, secondLength),
            VariantType.VT_DISPATCH => CreateJaggedArray<IComObject>(firstLength, secondLength),
            VariantType.VT_ERROR => CreateJaggedArray<Scode>(firstLength, secondLength),
            VariantType.VT_BOOL => CreateJaggedArray<bool>(firstLength, secondLength),
            VariantType.VT_VARIANT => CreateJaggedArray<Variant>(firstLength, secondLength),
            VariantType.VT_UNKNOWN => CreateJaggedArray<IComObject>(firstLength, secondLength),
            VariantType.VT_DECIMAL => CreateJaggedArray<decimal>(firstLength, secondLength),
            VariantType.VT_I1 => CreateJaggedArray<char>(firstLength, secondLength),
            VariantType.VT_UI1 => CreateJaggedArray<byte>(firstLength, secondLength),
            VariantType.VT_UI2 => CreateJaggedArray<ushort>(firstLength, secondLength),
            VariantType.VT_UI4 or VariantType.VT_UINT => CreateJaggedArray<uint>(firstLength, secondLength),
            VariantType.VT_I8 => CreateJaggedArray<long>(firstLength, secondLength),
            VariantType.VT_UI8 => CreateJaggedArray<ulong>(firstLength, secondLength),
            VariantType.VT_ARRAY => CreateJaggedArray<ComArray>(firstLength, secondLength),
            _ => throw new NotSupportedException($"VT_* {vt} is not supported in VARIANT array context.")
        };
    }

    private static T[][] CreateJaggedArray<T>(int firstLength, int secondLength)
    {
        var array = new T[firstLength][];
        for (var i = 0; i < firstLength; i++)
        {
            array[i] = new T[secondLength];
        }

        return array;
    }

    /// <summary>
    /// Get supported type id
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="flag">Flag value that controls the requested operation.</param>
    /// <returns>The requested supported type value.</returns>
    internal static VariantType? GetSupportedType(Type c, int flag = InteropFlags.FLAG_NULL)
    {
        if (!_supportedTypes.TryGetValue(c, out var type))
        {
            if (typeof(IComObject) == c)
            {
                return VariantType.VT_UNKNOWN;
            }
            if (typeof(IDispatch) == c)
            {
                return VariantType.VT_DISPATCH;
            }
            return null;
        }
        var retVal = type;
        if (retVal == VariantType.VT_I4 &&
            (flag & InteropFlags.FLAG_REPRESENTATION_VT_INT) ==
                    InteropFlags.FLAG_REPRESENTATION_VT_INT)
        {
            // means that if retval came back as VariantType.VT_I4, we should make that VariantType.VT_INT
            return VariantType.VT_INT;
        }
        else if (retVal == VariantType.VT_UI4 &&
            (flag & InteropFlags.FLAG_REPRESENTATION_VT_UINT) ==
                    InteropFlags.FLAG_REPRESENTATION_VT_UINT)
        {
            return VariantType.VT_UINT;
        }
        return retVal;
    }

    /// <summary>
    /// Get supported type id
    /// </summary>
    /// <param name="o">Object instance being compared, marshaled, or converted.</param>
    /// <param name="defaultType">Type descriptor that determines how the value is marshaled.</param>
    /// <returns>The requested supported type value.</returns>
    internal static VariantType GetSupportedType(object o, VariantType defaultType)
    {
        var retval = defaultType;
        var c = o.GetType();
        if (_supportedTypes.ContainsKey(c))
        {
            retval = _supportedTypes[c];
        }
        // Order is important since <see cref="IDispatch"/> derieves from <see cref="IComObject"/>
        if (o is IDispatch)
        {
            retval = VariantType.VT_DISPATCH;
        }
        else if (o is IComObject)
        {
            retval = VariantType.VT_UNKNOWN;
        }
        return retval;
    }

    /// <summary>
    /// EMPTY <code>VARIANT</code>.
    /// This is not Thread Safe, hence a new instance must be taken each time.
    /// </summary>
    public static Variant CreateEMPTY() => new Variant(false, null);

    /// <summary>
    /// EMPTY BYREF <code>VARIANT</code>.
    /// This is not Thread Safe, hence a new instance must be taken each time. Used for a
    /// <code>[out] VARIANT*</code> .
    /// </summary>
    public static Variant CreateEMPTY_BYREF() => new Variant(CreateEMPTY());

    /// <summary>
    /// <code>VARIANT</code> for <code>([out] IUnknown*)</code>.
    /// This is not Thread Safe, hence a new instance must be taken each time.
    /// </summary>
    public static Variant CreateOUT_IUNKNOWN()
    {
        var retval = new Variant(new ComObjectImpl(null, new InterfacePointer(null, -1, null)), true)
        {
            Flag =
                InteropFlags.FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT |
                InteropFlags.FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT
        };
        return retval;
    }

    /// <summary>
    /// <code>VARIANT</code> for <code>([out] IDispatch*)</code>.
    /// This is not Thread Safe, hence a new instance must be taken each time.
    /// </summary>
    /// <remarks>
    /// Note that this must also be used when the interface pointer is a
    /// subclass of <code>IDispatch</code> i.e. supports automation (or is a
    /// <code>dispinterface</code>).
    /// </remarks>
    public static Variant CreateOUT_IDISPATCH()
    {
        var retval = new Variant(
            new ComObjectImpl(null, new InterfacePointer(null, -1, null)), true)
        {
            Flag =
                InteropFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT |
                InteropFlags.FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT
        };
        return retval;
    }

    /// <summary>
    /// NULL <code>VARIANT</code>.
    /// This is not Thread Safe, hence a new instance must be taken each time.
    /// </summary>
    public static Variant CreateNULL() =>
        new Variant(false, new VariantBody(Null.Value));

    /// <summary>
    /// OPTIONAL PARAM. Pass this when a parameter is <code>[optional]</code>
    /// for a COM call.
    /// This is not Thread Safe, hence a new instance must be taken each time.
    /// </summary>
    public static Variant CreateOPTIONAL_PARAM() =>
        new Variant(new Scode(ErrorCode.DISP_E_PARAMNOTFOUND));

    /// <summary>
    /// Helper method for creating an array of <code>BSTR</code>s,
    /// IDL signature <code>[in, out] SAFEARRAY(BSTR) *p</code>.
    /// The return value can directly be used in an <code><see cref="IDispatch"/></code>call.
    /// </summary>
    public static Variant CreateBSTRARRAY() =>
        new Variant(new ComArray(new ComString[] { new ComString("") }, true), true);

    /// <summary>
    /// Helper method for creating an array of <code>VARIANT</code>s,
    /// IDL signature <code>[in, out] SAFEARRAY(VARIANT) *p</code>
    /// OR <code>[in,out] VARIANT *pArray</code>. The return value
    /// can directly be used in an <code><see cref="IDispatch"/></code> call.
    /// </summary>
    public static Variant CreateVARIANTARRAY() =>
        new Variant(new ComArray(new Variant[] { CreateEMPTY() }, true), true);

    /// <summary>
    /// Called when this variant is nested
    /// </summary>
    internal bool Deffered
    {
        set
        {
            if (_member != null && !_member.Reference)
            {
                _member.Deffered = value;
            }
        }
    }

    /// <summary>
    /// Sets a <see cref="InteropFlags"/> value to be used while encoding
    /// (marshalling) this Variant.
    /// </summary>
    public int Flag
    {
        set
        {
            var variantBody = (VariantBody)_member.Referent;
            variantBody._flag |= value;
        }
        get
        {
            var variantBody = (VariantBody)_member.Referent;
            return variantBody._flag;
        }
    }

    /// <summary>
    /// Returns whether this variant is a <code>NULL</code> variant.
    /// </summary>
    /// <returns> <code>true</code> if the variant is a
    /// <code>NULL</code> </returns>
    public bool IsNull
    {
        get
        {
            if (_member == null)
            {
                return true;
            }
            var variantBody = (VariantBody)_member.Referent;
            return variantBody == null || variantBody.IsNull;
        }
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> as reference to another.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="variant">VARIANT instance whose type and body should be copied.</param>
    public Variant(Variant variant) :
        this(true, variant)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with an <code>int</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// Used when the variant type is VariantType.VT_I4.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. LONG* </param>
    public Variant(int value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>long</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// Used when the variant type is VariantType.VT_I8.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. </param>
    public Variant(long value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>float</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. FLOAT* </param>
    public Variant(float value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>bool</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. VARIANT_BOOL* </param>
    public Variant(bool value, bool isByRef = false) :
        this(isByRef, (object)value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>double</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to be
    /// represented as a pointer. DOUBLE* </param>
    public Variant(double value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>short</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to be
    /// represented as a pointer. SHORT* </param>
    public Variant(short value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>char</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to be
    /// represented as a pointer. CHAR* </param>
    public Variant(char value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code><see cref="ComString"/></code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to be
    /// represented as a pointer. BSTR* </param>
    public Variant(ComString value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>String</code>.
    /// Used via serializing the <code>VARIANT</code>. Internally a
    /// <code><see cref="ComString"/></code> is formed with it's default type
    /// <code>BSTR</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to be
    /// represented as a pointer. BSTR* </param>
    public Variant(string value, bool isByRef = false) :
        this(new ComString(value), isByRef)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with an <code><see cref="IComObject"/></code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to be
    /// represented as a pointer. <see cref="IComObject"/>** </param>
    public Variant(IComObject value, bool isByRef = false) :
        this(isByRef, value)
    {
        if (value is IDispatch)
        {
            Flag = InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
        }
        else
        {
            Flag = InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
        }
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>SCODE</code>
    /// value and it's <code>errorCode</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="scode">COM status code stored in the VARIANT.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. SCODE* </param>
    public Variant(Scode scode, bool isByRef = false) :
        this(isByRef, scode)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with an <code>DateTime</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. Date* </param>
    public Variant(DateTime value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>Currency</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. Date* </param>
    public Variant(Currency value, bool isByRef = false) :
        this(isByRef, value)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>byte</code>
    /// value. Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="number">Numeric value stored in the VARIANT body.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. </param>
    public Variant(byte number, bool isByRef = false) :
        this(isByRef, number)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>ushort</code>
    /// value. Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="number">Numeric value stored in the VARIANT body.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. </param>
    public Variant(ushort number, bool isByRef = false) :
        this(isByRef, number)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>uint</code>
    /// value. Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="number">Numeric value stored in the VARIANT body.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. </param>
    public Variant(uint number, bool isByRef = false) :
        this(isByRef, number)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>ulong</code>
    /// value. Used via serializing the <code>VARIANT</code>.
    /// </summary>
    /// <param name="number">Numeric value stored in the VARIANT body.</param>
    /// <param name="isByRef"> <code>true</code> if the value is to
    /// be represented as a pointer. </param>
    public Variant(ulong number, bool isByRef = false) :
        this(isByRef, number)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>ComArray</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// Only 1 and 2 dimensional array is supported.
    /// </summary>
    /// <param name="array">Array instance being inspected, encoded, or copied.</param>
    /// <param name="flag"> <see cref="InteropFlags"/> value </param>
    public Variant(ComArray array, int flag) :
        this(array, false, flag)
    {
    }

    /// <summary>
    /// Setting up a <code>VARIANT</code> with a <code>ComArray</code>.
    /// Used via serializing the <code>VARIANT</code>.
    /// Only 1 and 2 dimensional array is supported.
    /// </summary>
    /// <param name="array">Array instance being inspected, encoded, or copied.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    /// <param name="flag"> <see cref="InteropFlags"/> value </param>
    public Variant(ComArray array, bool isByRef = false,
        int flag = InteropFlags.FLAG_NULL) :
        this(isByRef, array, flag)
    {
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private Variant()
    {
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    private Variant(bool isByRef, object obj)
    {
        if (obj != null && obj.GetType().IsArray)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_VARIANT_ONLY_COMARRAY_EXCEPTED), nameof(obj));
        }
        if (obj is InterfacePointer)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_VARIANT_TYPE_INCORRECT), nameof(obj));
        }
        if (obj is VariantBody)
        {
            _member = new ComPointer(obj);
        }
        else
        {
            _member = new ComPointer(new VariantBody(obj, isByRef));
        }
        _member.ReferentId = 0x72657355; // "User" in LEndian.
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// <param name="isByRef">Value indicating whether the variant stores a by-reference value.</param>
    /// <param name="array">Array instance being inspected, encoded, or copied.</param>
    /// <param name="flag">Flag value that controls the requested operation.</param>
    private Variant(bool isByRef, ComArray array, int flag = InteropFlags.FLAG_NULL)
    {
        var is2Dim = false;

        if (array == null)
        {
            _member = new ComPointer(new VariantBody(null, false))
            {
                ReferentId = 0x72657355 // "User" in LEndian.
            };
            return;
        }

        Type c;
        object[] newArrayObj;
        switch (array.Dimensions)
        {
            case 1:
                var obj = (object[])array.ArrayInstance;
                newArrayObj = obj;
                c = obj.GetType().GetElementType();
                break;
            case 2:
                // The 2 dimensional array is serialized like this first the index
                // [0,0]  and then [1,0] then [0,1] then [1,1], then [0,2] then [1,2]
                // and so on . so what i will do here is that create a single dimension
                // flat array of the members in the order specified above, after
                // examining this Object[][] and let the 1 dimension serializing logic
                // take over.
                var obj2 = (object[][])array.ArrayInstance;
                // variants = new <see cref="Variant"/>[array.getNumElementsInAllDimensions()];

                // JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                var name = obj2.GetType().FullName;
                object subArray = obj2;
                name = name.Substring(1);
                var firstDim = ((object[])subArray).Length;
                subArray = firstDim > 0 ? ((object[])subArray)[0] : null;
                var secondDim = subArray is object[] subArrayValues ? subArrayValues.Length : 0;
                c = subArray?.GetType().GetElementType() ?? typeof(object);
                var k = 0;
                newArrayObj = CreateSupportedReferenceArray(c, array.NumElementsInAllDimensions);
                for (var i = 0; i < secondDim; i++)
                {
                    for (var j = 0; j < firstDim; j++)
                    {
                        newArrayObj[k++] = obj2[j][i];
                    }
                }
                is2Dim = true;
                break;
            default:
                throw new ArgumentException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_VARIANT_VARARRAYS_2DIMRES), nameof(array));
        }
        // should always be conformant since this is part of a safe array.
        var array2 = new ComArray(newArrayObj, true);

        var safeArray = new Struct();
        try
        {
            safeArray.AddMember((short)array.Dimensions); // dim
            var elementSize = -1;
            short flags = FADF_HAVEVARTYPE;
            if (c == typeof(Variant))
            {
                flags = (short)(flags | FADF_VARIANT);
                elementSize = 16; // (Variant is pointer whose size is 16)
            }
            else
            {
                if (kArryInits.Contains(c))
                {
                    if (c == typeof(ComString))
                    {
                        flags = (short)(flags | FADF_BSTR);
                    }
                    else
                    {
                        if (c == typeof(IComObject))
                        {
                            flags = (short)(flags | FADF_UNKNOWN);
                            flag |= InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
                        }
                        else
                        {
                            if (c == typeof(IDispatch))
                            {
                                flags = (short)(flags | FADF_DISPATCH);
                                flag |= InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
                            }
                        }
                    }
                    elementSize = 4; // Since all these are pointers inherently
                }
                else
                {
                    // JStruct and <see cref="Union"/>s are expected to be encapsulated within pointers...they usually are :)
                    elementSize = MarshalUnMarshalHelper.GetLengthInBytes(c, null, c == typeof(bool) ?
                        InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL : InteropFlags.FLAG_NULL); // All other types, basic types
                }
            }

            Struct safeArrayBound = null;

            var upperBounds = array.UpperBounds;
            var arrayOfSafeArrayBounds = new Struct[array.Dimensions];
            for (var i = 0; i < array.Dimensions; i++)
            {
                safeArrayBound = new Struct();
                safeArrayBound.AddMember(upperBounds[i]);
                safeArrayBound.AddMember(0); // starts at 0
                arrayOfSafeArrayBounds[i] = safeArrayBound;
            }

            var arrayOfSafeArrayBounds2 = new ComArray(arrayOfSafeArrayBounds, true);

            safeArray.AddMember(flags); // flags
            if (elementSize > 0)
            {
                safeArray.AddMember(elementSize);
            }
            else
            {
                elementSize = MarshalUnMarshalHelper.GetLengthInBytes(c, null, flag);
                safeArray.AddMember(elementSize); // size
            }

            safeArray.AddMember((short)0); // locks
            safeArray.AddMember((short)GetSupportedType(c, flag)); // variant array, safearrayunion

            // peculiarity here, windows seems to be sending the signed type in VarType32...
            if (c == typeof(byte))
            {
                safeArray.AddMember(GetSupportedType(typeof(sbyte), flag)); // safearrayunion
            }
            else if (c == typeof(ushort))
            {
                safeArray.AddMember(GetSupportedType(typeof(short), flag)); // safearrayunion
            }
            else if (c == typeof(uint))
            {
                safeArray.AddMember(GetSupportedType(typeof(int), flag)); // safearrayunion
            }
            else if (c == typeof(ulong))
            {
                safeArray.AddMember(GetSupportedType(typeof(long), flag)); // safearrayunion
            }
            else if (c == typeof(bool))
            {
                safeArray.AddMember(GetSupportedType(typeof(short), flag)); // safearrayunion
            }
            else if (c == typeof(double))
            {
                safeArray.AddMember(GetSupportedType(typeof(long), flag)); // safearrayunion
            }
            else if (c == typeof(float))
            {
                safeArray.AddMember(GetSupportedType(typeof(int), flag)); // safearrayunion
            }
            else
            {
                safeArray.AddMember(GetSupportedType(c, flag)); // safearrayunion
            }
            safeArray.AddMember(array2.NumElementsInAllDimensions); // size in safearrayunion
            var ptr2RealArray = new ComPointer(array2);
            safeArray.AddMember(ptr2RealArray);
            safeArray.AddMember(arrayOfSafeArrayBounds2);
        }
        catch (InteropException e)
        {
            throw new InteropRuntimeException(e.ErrorCode);
        }

        var variant2 = new VariantBody(safeArray, c, is2Dim, isByRef, flag);
        _member = new ComPointer(variant2, false)
        {
            ReferentId = 0x72657355 // "User" in LEndian.
        };
    }

    /// <summary>
    /// Returns the contained object.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public object Object
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).Object;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>int</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public int ObjectAsInt
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsInt;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>float</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public float ObjectAsFloat
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsFloat;
        }
    }

    /// <summary>
    /// Retrieves the contained objects errorCode.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public int ObjectAsSCODE
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsSCODE;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>double</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public double ObjectAsDouble
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsDouble;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>short</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public short ObjectAsShort
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsShort;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>bool</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public bool ObjectAsBoolean
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsBoolean;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code><see cref="ComString"/></code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public ComString ObjectAsString
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsString;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>String</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public string ObjectAsString2
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsString.String;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>DateTime</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public DateTime ObjectAsDate
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsDate;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>char</code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public char ObjectAsChar
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsChar;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code><see cref="IComObject"/></code>.
    /// Return value must be "narrowed" to get the expected type.
    /// for example : If expected type is an <code><see cref="IDispatch"/></code>,
    /// then the return value must pass through
    /// <code><see cref="ObjectFactory"/>.NarrowInstance(
    ///     <see cref="IComObject"/>)</code> to get to the right type.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public IComObject ObjectAsComObject
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsComObject;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code><see cref="Variant"/></code>.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public Variant ObjectAsVariant
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsVariant;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>ComArray</code>.
    /// Only 1 and 2 dim arrays are supported currently.
    /// Please note that this array is <b>not</b> backed by this
    /// variant and is a <b>new</b> copy. If the array
    /// is <code><see cref="IComObject"/></code>s, please make sure to use
    /// <code><see cref="ObjectFactory"/>.narrowObject()</code> to
    /// get the right instance.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public ComArray ObjectAsArray
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).Array;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>long</code>,
    /// used when the expected type is VariantType.VT_I8.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public long ObjectAsLong
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsLong;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>ulong</code> number.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public ulong ObjectAsUlong
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsUlong;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>uint</code> number.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public uint ObjectAsUnsigned
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsUnsigned;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>ushort</code> number.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public ushort ObjectAsUShort
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsUShort;
        }
    }

    /// <summary>
    /// Retrieves the contained object as <code>byte</code> number.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public byte ObjectAsByte
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).ObjectAsByte;
        }
    }

    /// <summary>
    /// Encode variant
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    internal void Encode(NdrCodec ndr, CodecContext context)
    {
        _member.Deffered = true;
        // this is since this could be part of an array or a struct...for normal calls
        // as soon as this call finishes a call will be given from Callobject for it's variantbody.
        MarshalUnMarshalHelper.Serialize(ndr, _member.GetType(), _member, context);
    }

    /// <summary>
    /// Decode variant
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>A new <see cref="Variant"/> instance built from <paramref name="ndr"/>.</returns>
    internal static Variant Decode(NdrCodec ndr, CodecContext context)
    {
        var variant = new Variant();
        var pointer = new ComPointer(typeof(VariantBody))
        {
            // this is since this could be part of an array or a struct...for normal calls
            Deffered = true
        };
        // as soon as this call finishes a call will be given from
        // Callobject for it's variantbody.
        variant._member = (ComPointer)MarshalUnMarshalHelper.Deserialize(
            ndr, pointer, context);
        return variant;
    }

    /// <summary>
    /// Returns whether the variant is an array
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public bool IsArray
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).IsArray;
        }
    }

    /// <summary>
    /// Returns length in bytes
    /// </summary>
    /// <param name="flag">Flag value that controls the requested operation.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal int GetLengthInBytes(int flag = InteropFlags.FLAG_NULL)
    {
        CheckValidity();
        return MarshalUnMarshalHelper.GetLengthInBytes(_member.GetType(),
            _member, flag);
    }

    /// <summary>
    /// Whether the ref flag is set
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public bool IsByRef
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).IsByRef;
        }
    }

    /// <summary>
    /// Returns the referent as integer. This can be used along with the
    /// <code><see cref="Variant"/>.VariantType.VT_<i>XX</i></code> flags
    /// to find out the type of the referent.
    /// For example :
    /// <code>
    /// switch(variant.Type)
    /// {
    ///    case VariantType.VT_VARIANT:
    ///       value = variant.ObjectAsVariant();
    ///       break;
    ///    case VariantType.VT_NULL: ...
    ///       break;
    /// }
    /// </code>
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public VariantType Type
    {
        get
        {
            CheckValidity();
            return ((VariantBody)_member.Referent).Type;
        }
    }

    /// <summary>
    /// Helper to check validity
    /// </summary>
    private void CheckValidity()
    {
        if (_member == null || _member.IsNull)
        {
            throw new InteropException(ErrorCode.INTEROP_VARIANT_IS_NULL);
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        _member == null ? "[null]" : "[" + _member + "]";


    // CAUTION NO PTR TYPE SHOULD BE PART OF THIS MAP !!!
    private static readonly Dictionary<Type, VariantType> _supportedTypes =
        new Dictionary<Type, VariantType>
        {
            [typeof(object)] = VariantType.VT_VARIANT,
            [typeof(Variant)] = VariantType.VT_VARIANT,
            [typeof(int)] = VariantType.VT_I4,
            [typeof(uint)] = VariantType.VT_UI4,
            [typeof(float)] = VariantType.VT_R4,
            [typeof(bool)] = VariantType.VT_BOOL,
            [typeof(double)] = VariantType.VT_R8,
            [typeof(short)] = VariantType.VT_I2,
            [typeof(ushort)] = VariantType.VT_UI2,
            [typeof(sbyte)] = VariantType.VT_I1,
            [typeof(char)] = VariantType.VT_I1,
            [typeof(byte)] = VariantType.VT_UI1,
            [typeof(ComString)] = VariantType.VT_BSTR,
            [typeof(Scode)] = VariantType.VT_ERROR,
            [typeof(Empty)] = VariantType.VT_EMPTY,
            [typeof(Null)] = VariantType.VT_NULL,
            [typeof(ComArray)] = VariantType.VT_ARRAY,
            [typeof(DateTime)] = VariantType.VT_DATE,
            [typeof(Currency)] = VariantType.VT_CY,
            [typeof(long)] = VariantType.VT_I8,
            [typeof(ulong)] = VariantType.VT_UI8
        };
    private static readonly Dictionary<VariantType, Type> _supportedTypes_classes =
        new Dictionary<VariantType, Type>
        {
            [VariantType.VT_DATE] = typeof(DateTime),
            [VariantType.VT_CY] = typeof(Currency),
            [VariantType.VT_VARIANT] = typeof(Variant),
            [VariantType.VT_I4] = typeof(int),
            [VariantType.VT_INT] = typeof(int),
            [VariantType.VT_UI4] = typeof(uint),
            [VariantType.VT_UINT] = typeof(uint),
            [VariantType.VT_R4] = typeof(float),
            [VariantType.VT_BOOL] = typeof(bool),
            [VariantType.VT_R8] = typeof(double),
            [VariantType.VT_I2] = typeof(short),
            [VariantType.VT_UI2] = typeof(ushort),
            [VariantType.VT_I1] = typeof(char),
            [VariantType.VT_UI1] = typeof(byte),
            [VariantType.VT_BSTR] = typeof(ComString),
            [VariantType.VT_ERROR] = typeof(Scode),
            [VariantType.VT_EMPTY] = typeof(Empty),
            [VariantType.VT_NULL] = typeof(Null),
            [VariantType.VT_ARRAY] = typeof(ComArray),
            [VariantType.VT_UNKNOWN] = typeof(IComObject),
            [VariantType.VT_DISPATCH] = typeof(IComObject),
            [VariantType.VT_I8] = typeof(long),
            [VariantType.VT_UI8] = typeof(ulong)
        };
    private static readonly Dictionary<Type, object> _outTypesMap =
        new Dictionary<Type, object>
        {
            [typeof(int)] = 0,
            [typeof(short)] = (short)0,
            [typeof(float)] = 0.0f,
            [typeof(double)] = 0.0,
            [typeof(bool)] = false,
            [typeof(string)] = "",
            [typeof(Currency)] = new Currency("0.0"),
            [typeof(DateTime)] = DateTime.Now,
            [typeof(char)] = '9',
            [typeof(byte)] = (byte)0,
            [typeof(ushort)] = (ushort)0,
            [typeof(uint)] = 0u,
            [typeof(long)] = 0L,
            [typeof(ulong)] = 0uL
        };
    private static readonly List<Type> kArryInits =
        new List<Type> {
            typeof(ComString),
            typeof(ComPointer),
            typeof(IComObject),
            typeof(IDispatch) // this can only happen in case of an array
        };
    internal ComPointer _member;
}
