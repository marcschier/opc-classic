// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Automation;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
///<para>Represents a C++ array which can display both <i>conformant and standard</i>
/// behaviors. Since this class forms a wrapper on the actual array, the developer
/// is expected to provide complete and final arrays (of Objects) to this class.
/// Modifying the wrapped array afterwards <b>will</b> have unexpected results.
/// </para>
/// <para>
/// <i>Please refer to <b>MSExcel</b> examples for more details on how to use this
/// class.</i>
/// </para>
/// <para>
/// <b>Note</b>: Wrapped Arrays can be at most two dimensional in nature. Above
/// that is not supported by the library.
/// </para>
/// </summary>
[Serializable]
public sealed class ComArray
{

    /// <summary>
    /// Returns the nested Array.
    /// </summary>
    /// <returns> array Object which can be type casted based on value
    /// returned by <seealso cref="ArrayType"/>. </returns>
    public object ArrayInstance { get; private set; }

    /// <summary>
    /// Class of the nested Array.
    /// </summary>
    /// <returns> <code>class</code>  </returns>
    public Type ArrayType { get; private set; }

    /// <summary>
    /// Array of integers depicting highest index for each dimension.
    /// </summary>
    /// <returns> <code>int[]</code> </returns>
    public int[] UpperBounds { get; private set; }

    /// <summary>
    /// Returns the dimensions of the Array.
    /// </summary>
    /// <returns> <code>int</code> </returns>
    public int Dimensions { get; private set; } = -1;

    /// <summary>
    /// Total size
    /// </summary>
    internal int SizeOfAllElementsInBytes
    {
        get
        {
            // this means that decode has created this array, and we
            // need to compute the size to stay consistent.
            if (_sizeOfNestedArrayInBytes == -1)
            {
                _sizeOfNestedArrayInBytes = ComputeLengthArray(ArrayInstance);
            }
            return _sizeOfNestedArrayInBytes;
        }
    }

    /// <summary>
    /// Status whether the array is <code>conformant</code> or not.
    /// </summary>
    /// <returns> <code>true</code> is array is <code>conformant</code>.
    /// </returns>
    public bool Conformant
    {
        get => _isConformant;
        set => _isConformantProxy = value;
    }

    /// <summary>
    /// Status whether the array is <code>varying</code> or not.
    /// </summary>
    /// <returns> <code>true</code> is array is <code>varying</code>.
    /// </returns>
    public bool Varying
    {
        get => _isVarying;
        set => _isVaryingProxy = value;
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ComArray() { }

    /// <summary>
    /// Creates an array object of the type specified by <code>clazz</code>. This is used
    /// to prepare a template for decoding an array of that type. Used only for setting as an
    /// <code>[out]</code> parameter in a CallBuilder.
    /// For example:
    /// This call creates a template for a single dimension Integer array of size 10.
    /// <code>
    /// ComArray array = new ComArray(Integer.class,new int[]{10},1,false);
    /// </code>
    /// </summary>
    /// <param name="clazz"> class whose instances will be members of the deserialized array. </param>
    /// <param name="upperBounds"> highest index for each dimension. </param>
    /// <param name="dimension"> number of dimensions </param>
    /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
    /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied and its length
    /// is not equal to the <code>dimension</code> parameter. </exception>
    public ComArray(Type clazz, int[] upperBounds, int dimension, bool isConformant)
    {
        ArrayType = clazz;
        Init2(upperBounds, dimension, isConformant, false);
    }

    /// <summary>
    /// Refer to <seealso cref="ComArray(Type, int[], int, bool)"/>
    /// </summary>
    /// <param name="clazz"> class whose instances will be members of the deserialized array. </param>
    /// <param name="upperBounds"> highest index for each dimension. </param>
    /// <param name="dimension"> number of dimensions </param>
    /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
    /// <param name="isVarying"> declares whether the array is <i>varying</i> or not. </param>
    /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied
    /// and its length is not equal to the <code>dimension</code> parameter. </exception>
    public ComArray(Type clazz, int[] upperBounds, int dimension, bool isConformant, bool isVarying)
    {
        ArrayType = clazz;
        Init2(upperBounds, dimension, isConformant, isVarying);
    }

    /// <summary>
    /// Creates an array object with members of the type <code>template</code>.
    /// This constructor is used to prepare a template for decoding an array and is
    /// exclusively for composites like <code><see cref="Struct"/></code>, 
    /// <code><see cref="ComPointer"/></code>, <code><see cref="Union"/></code>,
    /// <code><see cref="ComString"/></code> where more information on the
    /// structure of the composite is required before trying to deserialize it.
    /// Sample Usage:
    ///
    /// <code>
    ///  <see cref="Struct"/> safeArrayBounds = new <see cref="Struct"/>();
    ///  safeArrayBounds.AddMember(typeof(int));
    ///  safeArrayBounds.AddMember(typeof(int);
    ///  // arraydesc
    ///  <see cref="Struct"/> arrayDesc = new <see cref="Struct"/>();
    ///  // typedesc
    ///  <see cref="Struct"/> typeDesc = new <see cref="Struct"/>();
    ///  arrayDesc.AddMember(typeDesc);
    ///  arrayDesc.AddMember(typeof(short));
    ///  arrayDesc.AddMember(<b>new ComArray(safeArrayBounds,new int[]{1},1,true)</b>);
    /// </code>
    /// </summary>
    /// <param name="template"> can be only of the type <code><see cref="Struct"/></code>,
    /// <code><see cref="ComPointer"/></code>, <code><see cref="Union"/></code>,
    /// <code><see cref="ComString"/></code> </param>
    /// <param name="upperBounds"> highest index for each dimension. </param>
    /// <param name="dimension"> number of dimensions </param>
    /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
    /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied and its length
    /// is not equal to the <code>dimension</code> parameter. </exception>
    /// <exception cref="ArgumentException"> if <code>template</code> is null or is not of the
    /// specified types. </exception>
    public ComArray(object template, int[] upperBounds, int dimension, bool isConformant)
    {
        if (template == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_ARRAY_TEMPLATE_NULL), nameof(template));
        }
        if (!template.GetType().Equals(typeof(Struct)) && !template.GetType().Equals(typeof(Union)) &&
            !template.GetType().Equals(typeof(ComPointer)) && !template.GetType().Equals(typeof(ComString)))
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_ARRAY_INCORRECT_TEMPLATE_PARAM), nameof(template));
        }
        _template = template;
        ArrayType = template.GetType();
        Init2(upperBounds, dimension, isConformant, false);
    }


    /// <summary>
    /// Refer to <seealso cref="ComArray(object, int[], int, bool)"/> for details.
    /// </summary>
    /// <param name="template"> can be only of the type <code><see cref="Struct"/></code>, 
    /// <code><see cref="ComPointer"/></code>,
    /// <code><see cref="Union"/></code>, <code><see cref="ComString"/></code> </param>
    /// <param name="upperBounds"> highest index for each dimension. </param>
    /// <param name="dimension"> number of dimensions </param>
    /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
    /// <param name="isVarying"> declares whether the array is <i>varying</i> or not. </param>
    /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied and its length
    /// is not equal to the <code>dimension</code> parameter. </exception>
    /// <exception cref="ArgumentException"> if <code>template</code> is null or is not of the
    /// specified types. </exception>
    // for structs, pointers, unions.
    public ComArray(object template, int[] upperBounds, int dimension, bool isConformant, bool isVarying)
    {
        if (template == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_ARRAY_TEMPLATE_NULL), nameof(template));
        }

        if (!template.GetType().Equals(typeof(Struct)) && !template.GetType().Equals(typeof(Union)) &&
            !template.GetType().Equals(typeof(ComPointer)) && !template.GetType().Equals(typeof(ComString)))
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_ARRAY_INCORRECT_TEMPLATE_PARAM), nameof(template));
        }

        if (Interop.COMVersion.MinorVersion == 6 && template.GetType().Equals(typeof(ComPointer)))
        {
            if (((ComPointer)template).Referent.GetType() == typeof(IComObject))
            {
                // in this case this pointer will be a reference type pointer and not deffered one.
                // change in MS specs since DCOM 5.4
                _isArrayOfCOMObjects_56DCOM = true;
                ((ComPointer)template).SetIsReferenceTypePtr();
            }
        }

        _template = template;
        ArrayType = template.GetType();

        Init2(upperBounds, dimension, isConformant, isVarying);
    }

    /// <summary>
    /// Init
    /// </summary>
    /// <param name="upperBounds">Upper bound for each SAFEARRAY dimension.</param>
    /// <param name="dimension">Array dimension being inspected or converted.</param>
    /// <param name="isConformant">Value indicating whether the array carries NDR conformant-array bounds.</param>
    /// <param name="isVarying">Value indicating whether the array carries NDR varying-array range metadata.</param>
    private void Init2(int[] upperBounds, int dimension, bool isConformant, bool isVarying)
    {
        UpperBounds = upperBounds;
        Dimensions = dimension;
        _isConformant = isConformant;
        _isConformantProxy = isConformant;
        _isVarying = isVarying;
        _isVaryingProxy = isVarying;

        if (upperBounds != null)
        {
            // have to supply the upperbounds for each dimension, no gaps in between
            if (upperBounds.Length != dimension)
            {
                throw new ArgumentException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_ARRAY_UPPERBNDS_DIM_NOTMATCH), nameof(upperBounds));
            }
        }

        for (var i = 0; upperBounds != null && i < upperBounds.Length; i++)
        {
            NumElementsInAllDimensions += upperBounds[i];
            if (isConformant)
            {
                ConformantMaxCounts.Add(upperBounds[i]);
            }
        }
        // numElementsInAllDimensions = numElementsInAllDimensions * dimension;
    }

    /// <summary>
    /// Creates an object with <i>array</i> parameter as the nested Array.
    /// This constructor is used when the developer wants to send an array to
    /// COM server.
    /// Sample Usage :
    /// <code>
    /// ComArray array = new ComArray(new <see cref="ComString"/>[]{
    ///   new <see cref="ComString"/>(name)
    /// },true);
    /// </code>
    /// </summary>
    /// <param name="array"> Array of any type. Primitive arrays are not allowed.
    /// </param>
    /// <param name="isConformant"> declares whether the array is
    /// <code>conformant</code> or not. </param>
    /// <exception cref="ArgumentException"> if the <code>array</code> is not an array or
    /// is of primitive type or is an array of <code>System.Object</code>.
    /// </exception>
    public ComArray(object array, bool isConformant)
    {
        _isConformant = isConformant;
        _isConformantProxy = isConformant;
        Init(array);
    }

    /// <summary>
    /// Refer <seealso cref="ComArray(object, bool)"/>
    /// </summary>
    /// <param name="array"> Array of any type. Primitive arrays are not allowed.
    /// </param>
    /// <param name="isConformant"> declares whether the array is 
    /// <code>conformant</code> or not. </param>
    /// <param name="isVarying"> declares whether the array is
    /// <code>varying</code> or not. </param>
    /// <exception cref="ArgumentException"> if the <code>array</code> 
    /// is not an array or
    /// is of primitive type or is an array of <code>System.Object</code>. 
    /// </exception>
    public ComArray(object array, bool isConformant, bool isVarying)
    {
        _isConformant = isConformant;
        _isConformantProxy = isConformant;
        _isVarying = isVarying;
        _isVaryingProxy = isVarying;
        Init(array);
    }

    /// <summary>
    /// Creates an object with <i>array</i> parameter as the nested Array.
    /// This constructor forms a <code>non-conformant</code> array and is used
    /// when the developer wants to send an array to COM server.
    /// Sample Usage :
    /// <code>
    /// ComArray array = new ComArray(new <see cref="ComString"/>[]{
    ///     new <see cref="ComString"/>(name)
    ///     },true);
    /// </code>
    /// </summary>
    /// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
    /// <exception cref="ArgumentException"> if the <code>array</code> is not an array or
    /// is of primitive type or is an array of <code>System.Object</code>. </exception>
    public ComArray(object array) => Init(array);

    /// <summary>
    /// Init
    /// </summary>
    /// <param name="array">Array instance being inspected, encoded, or copied.</param>
    private void Init(object array)
    {
        if (!array.GetType().IsArray)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_ARRAY_PARAM_ONLY), nameof(array));
        }
        if (array.GetType().IsPrimitive)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_ARRAY_PRIMITIVE_NOTACCEPT), nameof(array));
        }

        // bad way...but what the heck...
        // JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always
        // yield results identical to the Java Class.getName method:
        if (array.GetType().ToString().IndexOf("System.Object", StringComparison.Ordinal) != -1)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_ARRAY_TYPE_INCORRECT), nameof(array));
        }
        ArrayInstance = array;

        var upperBounds2 = new List<object>();

        // JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
        var name = array.GetType().FullName;
        var subArray = array;
        NumElementsInAllDimensions = 1;
        while (name.StartsWith('['))
        {
            name = name.Substring(1);
            var x = ((object[])subArray).Length;
            upperBounds2.Add(x);
            NumElementsInAllDimensions *= x;
            if (_isConformant)
            {
                ConformantMaxCounts.Add(x);
            }
            ArrayType = subArray.GetType().GetElementType();
            if (x == 0) // In which ever index the length is 0, the array stops there, example Byte[0],Byte[0][10],Byte[10][0]
            {
                break;
            }
            subArray = ((object[])subArray)[0]; // was: Array.get(subArray, 0);
            Dimensions++;
        }

        if (Dimensions == -1)
        {
            NumElementsInAllDimensions = 0;
            Dimensions++;
        }

        UpperBounds = new int[upperBounds2.Count];
        for (var i = 0; i < upperBounds2.Count; i++)
        {
            UpperBounds[i] = (int)upperBounds2[i];
        }
        Dimensions++; // since it starts from -1.
        _sizeOfNestedArrayInBytes = ComputeLengthArray(array);
    }

    /// <summary>
    /// Compute length
    /// </summary>
    /// <param name="array">Array instance being inspected, encoded, or copied.</param>
    /// <returns>The numeric compute length array value.</returns>
    private int ComputeLengthArray(object array)
    {
        var length = 0;

        // JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always
        // yield results identical to the Java Class.getName method:
        var name = array.GetType().FullName;
        var o = (object[])array;
        for (var i = 0; i < o.Length; i++)
        {
            if (name[1] != '[')
            {
                var o1 = (object[])array;
                for (var j = 0; j < o1.Length; j++)
                {
                    length += MarshalUnMarshalHelper.GetLengthInBytes(
                        o1.GetType().GetElementType(), o1[j]);
                }
                return length;
            }
            // JAVA TO C# CONVERTER WARNING
            length += ComputeLengthArray(o[i] /*Array.get(array, i)*/);
        }

        return length;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="array">Array instance being inspected, encoded, or copied.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    internal void Encode(NdrCodec ndr, object array, CodecContext context)
    {
        if (_isConformantProxy)
        {
            // first write the max counts ...First to last dimension.
            var i = 0;
            while (i < ConformantMaxCounts.Count)
            {
                MarshalUnMarshalHelper.Serialize(ndr, typeof(int), ConformantMaxCounts[i], context);
                i++;
            }

            _isConformantProxy = false; // this is since encode is recursive.
        }
        if (_isVaryingProxy)
        {
            // write the offset and the actual count
            var i = 0;
            while (i < ConformantMaxCounts.Count)
            {
                MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0, context); // offset
                MarshalUnMarshalHelper.Serialize(ndr, typeof(int), ConformantMaxCounts[i], context); // actual count
                i++;
            }

            _isVaryingProxy = false; // this is since encode is recursive.
        }

        // JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
        var name = array.GetType().FullName;
        var o = (object[])array;
        for (var i = 0; i < o.Length; i++)
        {
            if (name[1] != '[')
            {
                var o1 = (object[])array;
                var oldFlag = context.Flag;
                context.Flag |= InteropFlags.FLAG_REPRESENTATION_ARRAY;
                for (var j = 0; j < o1.Length; j++)
                {

                    MarshalUnMarshalHelper.Serialize(ndr, ArrayType, o1[j], context);
                }
                context.Flag = oldFlag;
                return;
            }

            // JAVA TO C# CONVERTER WARNING
            Encode(ndr, o[i] /*Array.get(array, i)*/, context);
        }
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="arrayType">Element type used by the SAFEARRAY or COM array descriptor.</param>
    /// <param name="dimension">Array dimension being inspected or converted.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>A new <see cref="object"/> instance built from <paramref name="ndr"/>.</returns>
    internal object Decode(NdrCodec ndr, Type arrayType, int dimension, CodecContext context)
    {
        var retVal = new ComArray
        {
            _isConformantProxy = _isConformantProxy,
            _isVaryingProxy = _isVaryingProxy
        };
        if (_isConformantProxy)
        {

            // first read the max counts ...First to last dimension.
            var i = 0;
            while (i < dimension)
            {
                retVal.ConformantMaxCounts.Add(
                    (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context));
                i++;
            }

            // isConformantProxy = false; // this is since decode is recursive.

            if (UpperBounds == null)
            {
                // max elements will come now.
                retVal.NumElementsInAllDimensions = 0;
                retVal.UpperBounds = new int[retVal.ConformantMaxCounts.Count];
                i = 0;
                while (i < retVal.ConformantMaxCounts.Count)
                {
                    retVal.UpperBounds[i] = retVal.ConformantMaxCounts[i];
                    retVal.NumElementsInAllDimensions *= retVal.UpperBounds[i];
                    i++;
                }
                if (i == 0)
                {
                    NumElementsInAllDimensions = 0;
                }
                // retVal.numElementsInAllDimensions = retVal.numElementsInAllDimensions * dimension;
            }
        }
        else
        { // this is the case when it is non conformant or coming from struct.
            retVal.UpperBounds = UpperBounds;
            retVal.ConformantMaxCounts = ConformantMaxCounts;
            retVal.NumElementsInAllDimensions = NumElementsInAllDimensions;
        }

        if (_isVaryingProxy)
        {
            // first read the max counts ...First to last dimension.
            var i = 0;
            retVal.ConformantMaxCounts.Clear(); // can't take the max count size now
            retVal.UpperBounds = null;
            retVal.NumElementsInAllDimensions = 0;

            while (i < dimension)
            {
                MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context); // offset
                retVal.ConformantMaxCounts.Add(
                    (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context)); // actual count
                i++;
            }

            // isConformantProxy = false; // this is since decode is recursive.

            if (UpperBounds == null)
            {
                // max elements will come now.
                retVal.NumElementsInAllDimensions = 1;
                retVal.UpperBounds = new int[retVal.ConformantMaxCounts.Count];
                i = 0;
                while (i < retVal.ConformantMaxCounts.Count)
                {
                    retVal.UpperBounds[i] = retVal.ConformantMaxCounts[i];
                    retVal.NumElementsInAllDimensions *= retVal.UpperBounds[i];
                    i++;
                }
                if (i == 0)
                {
                    NumElementsInAllDimensions = 0;
                }
                // retVal.numElementsInAllDimensions = retVal.numElementsInAllDimensions * dimension;
            }
        }

        retVal._isConformant = _isConformant;
        retVal._isVarying = _isVarying;
        retVal._template = _template;
        retVal.ArrayInstance = RecurseDecode(retVal, ndr, arrayType, dimension, context);
        retVal.ArrayType = ArrayType;
        retVal.Dimensions = Dimensions;
        retVal._sizeOfNestedArrayInBytes = -1;
        // setting here so that when a call actually comes for it's lenght
        // the getLength will compute. This is required since while decoding
        // many pointers are still not complete and their length cannot be decided.
        return retVal;
    }

    private static Array CreateDecodeArray(Type elementType, int length, int dimension)
    {
        if (dimension is < 1 or > 2)
        {
            throw new NotSupportedException($"Array dimension {dimension} is not supported in DCOM array context.");
        }

        if (elementType == typeof(object))
        {
            return CreateDecodeArray<object>(length, dimension);
        }
        if (elementType == typeof(int))
        {
            return CreateDecodeArray<int>(length, dimension);
        }
        if (elementType == typeof(uint))
        {
            return CreateDecodeArray<uint>(length, dimension);
        }
        if (elementType == typeof(short))
        {
            return CreateDecodeArray<short>(length, dimension);
        }
        if (elementType == typeof(ushort))
        {
            return CreateDecodeArray<ushort>(length, dimension);
        }
        if (elementType == typeof(sbyte))
        {
            return CreateDecodeArray<sbyte>(length, dimension);
        }
        if (elementType == typeof(byte))
        {
            return CreateDecodeArray<byte>(length, dimension);
        }
        if (elementType == typeof(long))
        {
            return CreateDecodeArray<long>(length, dimension);
        }
        if (elementType == typeof(ulong))
        {
            return CreateDecodeArray<ulong>(length, dimension);
        }
        if (elementType == typeof(float))
        {
            return CreateDecodeArray<float>(length, dimension);
        }
        if (elementType == typeof(double))
        {
            return CreateDecodeArray<double>(length, dimension);
        }
        if (elementType == typeof(bool))
        {
            return CreateDecodeArray<bool>(length, dimension);
        }
        if (elementType == typeof(char))
        {
            return CreateDecodeArray<char>(length, dimension);
        }
        if (elementType == typeof(string))
        {
            return CreateDecodeArray<string>(length, dimension);
        }
        if (elementType == typeof(ComString))
        {
            return CreateDecodeArray<ComString>(length, dimension);
        }
        if (elementType == typeof(Variant))
        {
            return CreateDecodeArray<Variant>(length, dimension);
        }
        if (elementType == typeof(IComObject))
        {
            return CreateDecodeArray<IComObject>(length, dimension);
        }
        if (elementType == typeof(IDispatch))
        {
            return CreateDecodeArray<IDispatch>(length, dimension);
        }
        if (elementType == typeof(ComPointer))
        {
            return CreateDecodeArray<ComPointer>(length, dimension);
        }
        if (elementType == typeof(Struct))
        {
            return CreateDecodeArray<Struct>(length, dimension);
        }
        if (elementType == typeof(Union))
        {
            return CreateDecodeArray<Union>(length, dimension);
        }
        if (elementType == typeof(UUID))
        {
            return CreateDecodeArray<UUID>(length, dimension);
        }
        if (elementType == typeof(InterfacePointer))
        {
            return CreateDecodeArray<InterfacePointer>(length, dimension);
        }
        if (elementType == typeof(DualStringArray))
        {
            return CreateDecodeArray<DualStringArray>(length, dimension);
        }

        throw new NotSupportedException($"Type {elementType} is not supported in DCOM array context.");
    }

    private static Array CreateDecodeArray<T>(int length, int dimension) =>
        dimension == 1 ? new T[length] : new T[length][];

    /// <summary>
    /// Recurse decoder
    /// </summary>
    /// <param name="retVal">Return value produced by the invoked COM method.</param>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="arrayType">Element type used by the SAFEARRAY or COM array descriptor.</param>
    /// <param name="dimension">Array dimension being inspected or converted.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>The decoded array contents, including nested arrays when present.</returns>
    private object RecurseDecode(ComArray retVal, NdrCodec ndr, Type arrayType,
        int dimension, CodecContext context)
    {
        object array = CreateDecodeArray(arrayType, retVal.UpperBounds[retVal.UpperBounds.Length - dimension], dimension);

        for (var i = 0; i < retVal.UpperBounds[retVal.UpperBounds.Length - dimension]; i++)
        {
            if (dimension == 1)
            {
                // fill value here
                // Array.set(array,i,new Float(i));
                if (_template == null)
                {
                    var flags = context.Flag;
                    context.Flag |= InteropFlags.FLAG_REPRESENTATION_ARRAY;
                    ((Array)array).SetValue(MarshalUnMarshalHelper.Deserialize(ndr, arrayType, context), i);
                    context.Flag = flags;
                }
                else
                {
                    if (_isArrayOfCOMObjects_56DCOM)
                    {
                        // not setting the array flag here.
                        ((Array)array).SetValue(MarshalUnMarshalHelper.Deserialize(ndr,
                            _template, context), i);
                    }
                    else
                    {
                        var flags = context.Flag;
                        context.Flag |= InteropFlags.FLAG_REPRESENTATION_ARRAY;
                        ((Array)array).SetValue(MarshalUnMarshalHelper.Deserialize(ndr,
                            _template, context), i);
                        context.Flag = flags;
                    }
                }
            }
            else
            {
                ((Array)array).SetValue(RecurseDecode(retVal, ndr, arrayType, dimension - 1, context), i);
            }
        }

        return array;
    }

    /// <summary>
    ///   Reverses Array elements for <see cref="IDispatch"/>.
    /// </summary>
    internal int ReverseArrayForDispatch()
    {
        if (ArrayInstance == null)
        {
            return 0;
        }

        var stack = new Stack<object>();
        int i;
        for (i = 0; i < ((object[])ArrayInstance).Length; i++)
        {
            stack.Push(((object[])ArrayInstance)[i]);
        }
        i = 0;
        while (stack.Count > 0)
        {
            ((object[])ArrayInstance)[i++] = stack.Pop();
        }
        return i;
    }

    internal List<int> ConformantMaxCounts { get; private set; } = new List<int>();

    internal List<int> MaxCountAndUpperBounds
    {
        set
        {
            ConformantMaxCounts = value;
            //    if (upperBounds == null) this will always be null since this api will get called from a decode and
            // in that the upperBounds is always null, since one does not know the dim expected.
            if (ConformantMaxCounts.Count > 0)
            {
                // max elements will come now.
                NumElementsInAllDimensions = 1;
                UpperBounds = new int[ConformantMaxCounts.Count];
                var i = 0;
                while (i < ConformantMaxCounts.Count)
                {
                    UpperBounds[i] = ConformantMaxCounts[i];
                    NumElementsInAllDimensions *= UpperBounds[i];
                    i++;
                }
                if (i == 0)
                {
                    NumElementsInAllDimensions = 0;
                }
            }
            else
            {
                UpperBounds = null;
                NumElementsInAllDimensions = 0;
            }
        }
    }

    internal int NumElementsInAllDimensions { get; private set; }

    /// <summary>
    /// Used only from the <see cref="Variant"/>.getDecodedValueAsArray. It is required
    /// when the real class of the array is determined after the SafeArray Struct has been
    /// processed. SA in COM can contain these along with normal types as well :
    /// FADF_BSTR 0x0100 An array of BSTRs.
    /// FADF_UNKNOWN 0x0200 An array of IUnknown*.
    /// FADF_DISPATCH 0x0400 An array of IDispatch*.
    /// FADF_VARIANT 0x0800 An array of VARIANTs.
    /// I have noticed that the "type" of the array doesn't always convey the right thing,
    /// so this "feature" flag of the SA shas to be looked into.
    /// As can be seen above except only BSTR require a template others do not. But the logic
    /// for the <see cref="ComString"/>(BSTR) already works fine. So I will use this
    /// flag only to set the <see cref="Variant"/>.class, whereever the "type" does not specify it but
    /// the "feature" does.
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    internal void UpdateType(Type c) => ArrayType = c;

    /// <inheritdoc/>
    public override string ToString()
    {
        var retVal = "[Type: " + ArrayType + ", ";
        if (ArrayInstance == null)
        {
            retVal += "memberArray is null, ";
        }
        else
        {
            retVal += ArrayInstance + ", ";
        }

        if (_isConformant)
        {
            retVal += " conformant, ";
        }
        if (_isVarying)
        {
            retVal += " varying, ";
        }

        return retVal + "]";
    }

    private bool _isConformant;
    private bool _isVarying;
    private bool _isConformantProxy;
    private bool _isVaryingProxy;
    private object _template;
    private readonly bool _isArrayOfCOMObjects_56DCOM;
    private int _sizeOfNestedArrayInBytes; // used in both encoding and decoding.
}
