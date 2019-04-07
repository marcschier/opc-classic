//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using System;
    using System.Collections.Generic;
    using org.jinterop.dcom.common;
    using SharpCifs.Dcerpc.Ndr;

    /// <summary>
    ///<para>Represents a C++ array which can display both <i>conformant and standard</i>
    /// behaviors. Since this class forms a wrapper on the actual array, the developer
    /// is expected to provide complete and final arrays (of Objects) to this class.
    /// Modifying the wrapped array afterwards <b>will</b> have unexpected results.
    ///  </para>
    /// <para>
    /// <i>Please refer to <b>MSExcel</b> examples for more details on how to use this
    /// class.</i>
    /// </para>
    ///  <para>
    /// <b>Note</b>: Wrapped Arrays can be at most two dimensional in nature. Above
    /// that is not supported by the library.
    /// </para>
    /// </summary>
    [Serializable]
    public sealed class JIArray {

        /// <summary>
        /// Private constructor
        /// </summary>
        private JIArray() {}

        /// <summary>
        /// Creates an array object of the type specified by <code>clazz</code>. This is used
        /// to prepare a template for decoding an array of that type. Used only for setting as an
        /// <code>[out]</code> parameter in a JICallBuilder.
        /// For example:-
        /// This call creates a template for a single dimension Integer array of size 10.
        /// <code>
        /// JIArray array = new JIArray(Integer.class,new int[]{10},1,false);
        /// </code>
        /// </summary>
        /// <param name="clazz"> class whose instances will be members of the deserialized array. </param>
        /// <param name="upperBounds"> highest index for each dimension. </param>
        /// <param name="dimension"> number of dimensions </param>
        /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
        /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied and its length
        /// is not equal to the <code>dimension</code> parameter. </exception>
        public JIArray(Type clazz, int[] upperBounds, int dimension, bool isConformant) {
            ArrayClass = clazz;
            Init2(upperBounds, dimension, isConformant, false);
        }

        /// <summary>
        /// Refer to <seealso cref="JIArray(Type, int[], int, bool)"/>
        /// </summary>
        /// <param name="clazz"> class whose instances will be members of the deserialized array. </param>
        /// <param name="upperBounds"> highest index for each dimension. </param>
        /// <param name="dimension"> number of dimensions </param>
        /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
        /// <param name="isVarying"> declares whether the array is <i>varying</i> or not. </param>
        /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied
        /// and its length is not equal to the <code>dimension</code> parameter. </exception>
        public JIArray(Type clazz, int[] upperBounds, int dimension, bool isConformant, bool isVarying) {
            ArrayClass = clazz;
            Init2(upperBounds, dimension, isConformant, isVarying);
        }

        /// <summary>
        /// Creates an array object with members of the type <code>template</code>.
        /// This constructor is used to prepare a template for decoding an array and is
        /// exclusively for composites like <code>JIStruct</code>, <code>JIPointer</code>,
        /// <code>JIUnion</code>, <code>JIString</code> where more information on the
        /// structure of the composite is required before trying to deserialize it.
        /// Sample Usage:-
        ///
        /// <code>
        ///   JIStruct safeArrayBounds = new JIStruct();
        ///   safeArrayBounds.addMember(Integer.class);
        ///   safeArrayBounds.addMember(Integer.class);
        ///   //arraydesc
        ///   JIStruct arrayDesc = new JIStruct();
        ///   //typedesc
        ///   JIStruct typeDesc = new JIStruct();
        ///   arrayDesc.addMember(typeDesc);
        ///   arrayDesc.addMember(Short.class);
        ///   arrayDesc.addMember(<b>new JIArray(safeArrayBounds,new int[]{1},1,true)</b>);
        /// </code>
        /// </summary>
        /// <param name="template"> can be only of the type <code>JIStruct</code>, <code>JIPointer</code>,
        /// <code>JIUnion</code>, <code>JIString</code> </param>
        /// <param name="upperBounds"> highest index for each dimension. </param>
        /// <param name="dimension"> number of dimensions </param>
        /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
        /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied and its length
        /// is not equal to the <code>dimension</code> parameter. </exception>
        /// <exception cref="ArgumentException"> if <code>template</code> is null or is not of the
        /// specified types. </exception>
        public JIArray(object template, int[] upperBounds, int dimension, bool isConformant) {
            if (template == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_ARRAY_TEMPLATE_NULL));
            }
            if (!template.GetType().Equals(typeof(JIStruct)) && !template.GetType().Equals(typeof(JIUnion)) &&
                !template.GetType().Equals(typeof(JIPointer)) && !template.GetType().Equals(typeof(JIString))) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_ARRAY_INCORRECT_TEMPLATE_PARAM));
            }
            _template = template;
            ArrayClass = template.GetType();
            Init2(upperBounds, dimension, isConformant, false);
        }


        /// <summary>
        /// Refer to <seealso cref="JIArray(object, int[], int, bool)"/> for details.
        /// </summary>
        /// <param name="template"> can be only of the type <code>JIStruct</code>, <code>JIPointer</code>,
        /// <code>JIUnion</code>, <code>JIString</code> </param>
        /// <param name="upperBounds"> highest index for each dimension. </param>
        /// <param name="dimension"> number of dimensions </param>
        /// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
        /// <param name="isVarying"> declares whether the array is <i>varying</i> or not. </param>
        /// <exception cref="ArgumentException"> if <code>upperBounds</code> is supplied and its length
        /// is not equal to the <code>dimension</code> parameter. </exception>
        /// <exception cref="ArgumentException"> if <code>template</code> is null or is not of the
        /// specified types. </exception>
        //for structs, pointers , unions.
        public JIArray(object template, int[] upperBounds, int dimension, bool isConformant, bool isVarying) {
            if (template == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_ARRAY_TEMPLATE_NULL));
            }

            if (!template.GetType().Equals(typeof(JIStruct)) && !template.GetType().Equals(typeof(JIUnion)) &&
                !template.GetType().Equals(typeof(JIPointer)) && !template.GetType().Equals(typeof(JIString))) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_ARRAY_INCORRECT_TEMPLATE_PARAM));
            }

            if (JISystem.COMVersion.MinorVersion == 6 && template.GetType().Equals(typeof(JIPointer))) {
                if (((JIPointer)template).GetReferent().GetType() == typeof(IJIComObject)) {
                    //in this case this pointer will be a reference type pointer and not deffered one.
                    //change in MS specs since DCOM 5.4
                    _isArrayOfCOMObjects_56DCOM = true;
                    ((JIPointer)template).SetIsReferenceTypePtr();
                }
            }

            _template = template;
            ArrayClass = template.GetType();

            Init2(upperBounds, dimension, isConformant, isVarying);
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="upperBounds"></param>
        /// <param name="dimension"></param>
        /// <param name="isConformant"></param>
        /// <param name="isVarying"></param>
        private void Init2(int[] upperBounds, int dimension, bool isConformant, bool isVarying) {
            UpperBounds = upperBounds;
            Dimensions = dimension;
            _isConformant = isConformant;
            _isConformantProxy = isConformant;
            _isVarying = isVarying;
            _isVaryingProxy = isVarying;

            if (upperBounds != null) {
                //have to supply the upperbounds for each dimension , no gaps in between
                if (upperBounds.Length != dimension) {
                    throw new ArgumentException(JISystem.getLocalizedMessage(
                        JIErrorCodes.JI_ARRAY_UPPERBNDS_DIM_NOTMATCH));
                }
            }

            for (var i = 0; upperBounds != null && i < upperBounds.Length; i++) {
                NumElementsInAllDimensions += upperBounds[i];
                if (isConformant) {
                    ConformantMaxCounts.Add(upperBounds[i]);
                }
            }
            //numElementsInAllDimensions = numElementsInAllDimensions * dimension;
        }

        /// <summary>
        /// Creates an object with <i>array</i> parameter as the nested Array.
        /// This constructor is used when the developer wants to send an array to
        /// COM server.
        /// Sample Usage :-
        /// <code>
        /// JIArray array = new JIArray(new JIString[]{new JIString(name)},true);
        /// </code>
        /// </summary>
        /// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
        /// <param name="isConformant"> declares whether the array is <code>conformant</code> or not. </param>
        /// <exception cref="ArgumentException"> if the <code>array</code> is not an array or
        /// is of primitive type or is an array of <code>java.lang.Object</code>. </exception>
        public JIArray(object array, bool isConformant) {
            _isConformant = isConformant;
            _isConformantProxy = isConformant;
            Init(array);
        }

        /// <summary>
        /// Refer <seealso cref="JIArray(object, bool)"/>
        /// </summary>
        /// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
        /// <param name="isConformant"> declares whether the array is <code>conformant</code> or not. </param>
        /// <param name="isVarying"> declares whether the array is <code>varying</code> or not. </param>
        /// <exception cref="ArgumentException"> if the <code>array</code> is not an array or
        /// is of primitive type or is an array of <code>java.lang.Object</code>. </exception>
        public JIArray(object array, bool isConformant, bool isVarying) {
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
        /// Sample Usage :-
        /// <code>
        /// JIArray array = new JIArray(new JIString[]{new JIString(name)},true);
        /// </code>
        /// </summary>
        /// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
        /// <exception cref="ArgumentException"> if the <code>array</code> is not an array or
        /// is of primitive type or is an array of <code>java.lang.Object</code>. </exception>
        public JIArray(object array) {
            Init(array);
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="array"></param>
        private void Init(object array) {
            if (!array.GetType().IsArray) {
                throw new ArgumentException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_ARRAY_PARAM_ONLY));
            }
            if (array.GetType().IsPrimitive) {
                throw new ArgumentException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_ARRAY_PRIMITIVE_NOTACCEPT));
            }

            //bad way...but what the heck...
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always
            // yield results identical to the Java Class.getName method:
            if (array.GetType().ToString().IndexOf("java.lang.Object", StringComparison.Ordinal) != -1) {
                throw new ArgumentException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_ARRAY_TYPE_INCORRECT));
            }
            ArrayInstance = array;

            var upperBounds2 = new List<object>();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            var name = array.GetType().FullName;
            var subArray = array;
            NumElementsInAllDimensions = 1;
            while (name.StartsWith("[", StringComparison.Ordinal)) {
                name = name.Substring(1);
                var x = ((object[])subArray).Length;
                upperBounds2.Add(x);
                NumElementsInAllDimensions *= x;
                if (_isConformant) {
                    ConformantMaxCounts.Add(x);
                }
                ArrayClass = subArray.GetType().GetElementType();
                if (x == 0) //In which ever index the length is 0 , the array stops there, example Byte[0],Byte[0][10],Byte[10][0]
                {
                    break;
                }
                subArray = ((object[])subArray)[0]; // was: Array.get(subArray, 0);
                Dimensions++;
            }

            if (Dimensions == -1) {
                NumElementsInAllDimensions = 0;
                Dimensions++;
            }

            UpperBounds = new int[upperBounds2.Count];
            for (var i = 0; i < upperBounds2.Count; i++) {
                UpperBounds[i] = (int)(int?)upperBounds2[i];
            }
            Dimensions++; //since it starts from -1.
            _sizeOfNestedArrayInBytes = ComputeLengthArray(array);
        }

        private int ComputeLengthArray(object array) {
            var length = 0;
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always
            // yield results identical to the Java Class.getName method:
            var name = array.GetType().FullName;
            var o = (object[])array;
            for (var i = 0; i < o.Length; i++) {
                if (name[1] != '[') {
                    var o1 = (object[])array;
                    for (var j = 0; j < o1.Length; j++) {
                        length += JIMarshalUnMarshalHelper.GetLengthInBytes(
                            o1.GetType().GetElementType(), o1[j], JIFlags.FLAG_NULL);
                    }
                    return length;
                }
                // JAVA TO C# CONVERTER WARNING
                length += ComputeLengthArray(o[i] /*Array.get(array, i)*/);
            }

            return length;
        }

        /// <summary>
        /// Returns the nested Array.
        /// </summary>
        /// <returns> array Object which can be type casted based on value
        /// returned by <seealso cref="ArrayClass"/>. </returns>
        public object ArrayInstance { get; private set; } = null;

        /// <summary>
        /// Class of the nested Array.
        /// </summary>
        /// <returns> <code>class</code>  </returns>
        public Type ArrayClass { get; private set; } = null;

        /// <summary>
        /// Array of integers depicting highest index for each dimension.
        /// </summary>
        /// <returns> <code>int[]</code> </returns>
        public int[] UpperBounds { get; private set; } = null;

        /// <summary>
        /// Returns the dimensions of the Array.
        /// </summary>
        /// <returns> <code>int</code> </returns>
        public int Dimensions { get; private set; } = -1;

        /// <summary>
        /// Total size
        /// </summary>
        internal int SizeOfAllElementsInBytes {
            get {
                //int length = numElementsInAllDimensions *
                // JIMarshalUnMarshalHelper.getLengthInBytes(clazz,((Object[])memberArray)[0],JIFlags.FLAG_NULL);

                //this means that decode has created this array, and we need to compute the size to stay consistent.
                if (_sizeOfNestedArrayInBytes == -1) {
                    _sizeOfNestedArrayInBytes = ComputeLengthArray(ArrayInstance);
                }
                return _sizeOfNestedArrayInBytes;
            }
        }

        /// <summary>
        /// Status whether the array is <code>conformant</code> or not.
        /// </summary>
        /// <returns> <code>true</code> is array is <code>conformant</code>. </returns>
        public bool Conformant {
            get => _isConformant;
            set => _isConformantProxy = value;
        }

        /// <summary>
        /// Status whether the array is <code>varying</code> or not.
        /// </summary>
        /// <returns> <code>true</code> is array is <code>varying</code>. </returns>
        public bool Varying {
            get => _isVarying;
            set => _isVaryingProxy = value;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="array"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        internal void Encode(NdrCodec ndr, object array, List<object> defferedPointers, int flag) {
            //	List<object> listofDefferedPointers = new List<object>();

            if (_isConformantProxy) {
                //first write the max counts ...First to last dimension.
                var i = 0;
                while (i < ConformantMaxCounts.Count) {
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), ConformantMaxCounts[i], defferedPointers, flag);
                    i++;
                }

                _isConformantProxy = false; //this is since encode is recursive.
            }

            if (_isVaryingProxy) {
                //write the offset and the actual count
                var i = 0;
                while (i < ConformantMaxCounts.Count) {
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, defferedPointers, flag); //offset
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), ConformantMaxCounts[i], defferedPointers, flag); //actual count
                    i++;
                }

                _isVaryingProxy = false; //this is since encode is recursive.
            }

            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            var name = array.GetType().FullName;
            var o = (object[])array;
            for (var i = 0; i < o.Length; i++) {
                if (name[1] != '[') {
                    var o1 = (object[])array;
                    for (var j = 0; j < o1.Length; j++) {
                        JIMarshalUnMarshalHelper.Serialize(ndr, ArrayClass, o1[j],
                            defferedPointers, flag | JIFlags.FLAG_REPRESENTATION_ARRAY);
                    }
                    return;
                }
                // JAVA TO C# CONVERTER WARNING
                Encode(ndr, o[i] /*Array.get(array, i)*/, defferedPointers, flag);
            }
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="arrayType"></param>
        /// <param name="dimension"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal object Decode(NdrCodec ndr, Type arrayType, int dimension,
            List<object> defferedPointers, int flag, IDictionary<object, object> additionalData) {
            var retVal = new JIArray {
                _isConformantProxy = _isConformantProxy,
                _isVaryingProxy = _isVaryingProxy
            };
            if (_isConformantProxy) {

                //first read the max counts ...First to last dimension.
                var i = 0;
                while (i < dimension) {
                    retVal.ConformantMaxCounts.Add(JIMarshalUnMarshalHelper.Deserialize(
                        ndr, typeof(int?), defferedPointers, flag, additionalData));
                    i++;
                }

                //isConformantProxy = false; //this is since decode is recursive.

                if (UpperBounds == null) {
                    //max elements will come now.
                    retVal.NumElementsInAllDimensions = 0;
                    retVal.UpperBounds = new int[retVal.ConformantMaxCounts.Count];
                    i = 0;
                    while (i < retVal.ConformantMaxCounts.Count) {
                        retVal.UpperBounds[i] = (int)(int?)retVal.ConformantMaxCounts[i];
                        retVal.NumElementsInAllDimensions *= retVal.UpperBounds[i];
                        i++;
                    }
                    if (i == 0) {
                        NumElementsInAllDimensions = 0;
                    }
                    //retVal.numElementsInAllDimensions = retVal.numElementsInAllDimensions * dimension;
                }
            }
            else { //this is the case when it is non conformant or coming from struct.
                retVal.UpperBounds = UpperBounds;
                retVal.ConformantMaxCounts = ConformantMaxCounts;
                retVal.NumElementsInAllDimensions = NumElementsInAllDimensions;
            }

            if (_isVaryingProxy) {
                //first read the max counts ...First to last dimension.
                var i = 0;
                retVal.ConformantMaxCounts.Clear(); //can't take the max count size now
                retVal.UpperBounds = null;
                retVal.NumElementsInAllDimensions = 0;

                while (i < dimension) {
                    JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(int?), defferedPointers, flag, null); // offset
					retVal.ConformantMaxCounts.Add(JIMarshalUnMarshalHelper.Deserialize(ndr,
                        typeof(int?), defferedPointers, flag, additionalData)); //actual count
                    i++;
                }

                //isConformantProxy = false; //this is since decode is recursive.

                if (UpperBounds == null) {
                    //max elements will come now.
                    retVal.NumElementsInAllDimensions = 1;
                    retVal.UpperBounds = new int[retVal.ConformantMaxCounts.Count];
                    i = 0;
                    while (i < retVal.ConformantMaxCounts.Count) {
                        retVal.UpperBounds[i] = (int)(int?)retVal.ConformantMaxCounts[i];
                        retVal.NumElementsInAllDimensions *= retVal.UpperBounds[i];
                        i++;
                    }
                    if (i == 0) {
                        NumElementsInAllDimensions = 0;
                    }
                    //retVal.numElementsInAllDimensions = retVal.numElementsInAllDimensions * dimension;
                }
            }

            retVal._isConformant = _isConformant;
            retVal._isVarying = _isVarying;
            retVal._template = _template;
            retVal.ArrayInstance = RecurseDecode(retVal, ndr, arrayType, dimension, defferedPointers, flag, additionalData);
            retVal.ArrayClass = ArrayClass;
            retVal.Dimensions = Dimensions;
            retVal._sizeOfNestedArrayInBytes = -1;
            // setting here so that when a call actually comes for it's lenght
            // the getLength will compute. This is required since while decoding
            // many pointers are still not complete and their length cannot be decided.
            return retVal;
        }

        /// <summary>
        /// Recurse decoder
        /// </summary>
        /// <param name="retVal"></param>
        /// <param name="ndr"></param>
        /// <param name="arrayType"></param>
        /// <param name="dimension"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        private object RecurseDecode(JIArray retVal, NdrCodec ndr, Type arrayType,
            int dimension, List<object> defferedPointers, int flag, IDictionary<object, object> additionalData) {
            object array = null;
            var c = arrayType;
            for (var j = 0; j < dimension; j++) {
                array = Array.CreateInstance(c, retVal.UpperBounds[retVal.UpperBounds.Length - j - 1]);
                c = array.GetType();
            }

            for (var i = 0; i < retVal.UpperBounds[retVal.UpperBounds.Length - dimension]; i++) {
                if (dimension == 1) {
                    //fill value here
                    //Array.set(array,i,new Float(i));
                    if (_template == null) {
                        ((Array)array).SetValue(JIMarshalUnMarshalHelper.Deserialize(ndr,
                            c.GetElementType() ?? c, defferedPointers, flag | JIFlags.FLAG_REPRESENTATION_ARRAY, additionalData), i);
                    }
                    else {
                        if (_isArrayOfCOMObjects_56DCOM) {
                            //not setting the array flag here.
                            ((Array)array).SetValue(JIMarshalUnMarshalHelper.Deserialize(ndr,
                                _template, defferedPointers, flag, additionalData), i);
                        }
                        else {
                            ((Array)array).SetValue(JIMarshalUnMarshalHelper.Deserialize(ndr,
                                _template, defferedPointers, flag | JIFlags.FLAG_REPRESENTATION_ARRAY, additionalData), i);
                        }
                    }
                }
                else {
                    ((Array)array).SetValue(RecurseDecode(retVal, ndr, arrayType,
                        dimension - 1, defferedPointers, flag, additionalData), i);
                }
            }

            return array;
        }

        /// <summary>
        ///	Reverses Array elements for IJIDispatch.
        /// </summary>
        internal int ReverseArrayForDispatch() {
            if (ArrayInstance == null) {
                return 0;
            }

            var stack = new Stack<object>();
            int i;
            for (i = 0; i < ((object[])ArrayInstance).Length; i++)
            {
                stack.Push(((object[])ArrayInstance)[i]);
            }
            i = 0;
            while (stack.Count > 0) {
                ((object[])ArrayInstance)[i++] = stack.Pop();
            }
            return i;
        }

        internal List<object> ConformantMaxCounts { get; private set; } = new List<object>();

        internal List<object> MaxCountAndUpperBounds {
            set {
                ConformantMaxCounts = value;
                //	if (upperBounds == null) this will always be null since this api will get called from a decode and
                //in that the upperBounds is always null, since one does not know the dim expected.
                if (ConformantMaxCounts.Count > 0) {
                    //max elements will come now.
                    NumElementsInAllDimensions = 1;
                    UpperBounds = new int[ConformantMaxCounts.Count];
                    var i = 0;
                    while (i < ConformantMaxCounts.Count) {
                        UpperBounds[i] = (int)(int?)ConformantMaxCounts[i];
                        NumElementsInAllDimensions *= UpperBounds[i];
                        i++;
                    }
                    if (i == 0) {
                        NumElementsInAllDimensions = 0;
                    }
                }
                else {
                    UpperBounds = null;
                    NumElementsInAllDimensions = 0;
                }
            }
        }

        internal int NumElementsInAllDimensions { get; private set; }

        /// <summary>
        /// Used only from the JIVariant.getDecodedValueAsArray. It is required
        /// when the real class of the array is determined after the SafeArray Struct has been
        /// processed. SA in COM can contain these along with normal types as well :-
        /// FADF_BSTR 0x0100 An array of BSTRs.
        /// FADF_UNKNOWN 0x0200 An array of IUnknown*.
        /// FADF_DISPATCH 0x0400 An array of IDispatch*.
        /// FADF_VARIANT 0x0800 An array of VARIANTs.
        /// I have noticed that the "type" of the array doesn't always convey the right thing,
        /// so this "feature" flag of the SA shas to be looked into.
        /// As can be seen above except only BSTR require a template others do not. But the logic
        /// for the JIString(BSTR) already works fine. So I will use this
        /// flag only to set the JIVariant.class , whereever the "type" does not specify it but
        /// the "feature" does.
        /// </summary>
        /// <param name="c"> </param>
        internal void UpdateClazz(Type c) {
            ArrayClass = c;
        }

        /// <inheritdoc/>
        public override string ToString() {
            var retVal = "[Type: " + ArrayClass + " , ";
            if (ArrayInstance == null) {
                retVal += "memberArray is null , ";
            }
            else {
                retVal += ArrayInstance + " , ";
            }

            if (_isConformant) {
                retVal += " conformant , ";
            }
            if (_isVarying) {
                retVal += " varying , ";
            }

            return retVal + "]";
        }

        private bool _isConformant;
        private bool _isVarying;
        private bool _isConformantProxy;
        private bool _isVaryingProxy;
        private object _template;
        private readonly bool _isArrayOfCOMObjects_56DCOM;
        private int _sizeOfNestedArrayInBytes; //used in both encoding and decoding.
    }
}