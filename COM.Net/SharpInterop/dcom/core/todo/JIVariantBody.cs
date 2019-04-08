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
    using SharpCifs.Util.Sharpen;
    using org.jinterop.dcom.common;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using org.jinterop.dcom.impls.automation;

    /// <summary>
    /// Variant body
    /// </summary>
    [Serializable]
    internal class JIVariantBody {

        public const short VT_PTR = 0x1A;
        public const short VT_SAFEARRAY = 0x1B;
        public const short VT_CARRAY = 0x1C;
        public const short VT_USERDEFINED = 0x1D;

        /// <summary>
        /// Empty type
        /// </summary>
        internal sealed class EMPTY { }

        /// <summary>
        /// Null type
        /// </summary>
        internal sealed class NULL { }

        /// <summary>
        /// Scode type
        /// </summary>
        internal sealed class SCODE {
            internal int errorCode;
            internal SCODE() {
            }
            internal SCODE(int errorCode) {
                this.errorCode = errorCode;
            }
        }

        static JIVariantBody() {
            _type3.Add(typeof(int?));
            _type3.Add(typeof(short?));
            _type3.Add(typeof(float?));
            _type3.Add(typeof(bool?));
            _type3.Add(typeof(char?));
            _type3.Add(typeof(sbyte?));
            _type3.Add(typeof(EMPTY));
            _type3.Add(typeof(NULL));
            _type3.Add(typeof(SCODE));
            _type3.Add(typeof(JIUnsignedByte));
            _type3.Add(typeof(JIUnsignedShort));
            _type3.Add(typeof(JIUnsignedInteger));
        }

        /// <summary>
        /// Is by ref
        /// </summary>
        internal virtual bool IsByRef => _isByRef;

        /// <summary>
        /// Body is null
        /// </summary>
        internal virtual bool IsNull => _isNull;

        /// <summary>
        /// Variant type
        /// </summary>
        internal virtual int Type => _isArray ? JIVariant.VT_ARRAY | _type : _type;

        /// <summary>
        /// Setting up a <code>VARIANT</code> with an object. Used via serializing the <code>VARIANT</code>.
        /// The class of the object determines its type.
        /// </summary>
        /// <param name="referent"> </param>
        /// <param name="isByRef"></param>
        internal JIVariantBody(object referent, bool isByRef) :
            this(referent, isByRef, -1) {
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="referent"></param>
        /// <param name="isByRef"></param>
        /// <param name="dataType"></param>
        private JIVariantBody(object referent, bool isByRef, int dataType) {
            _object = referent ?? new EMPTY();

            if (_object is JIString && ((JIString)_object).Type != JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                throw new JIRuntimeException(JIErrorCodes.JI_VARIANT_BSTR_ONLY);
            }
            if (_object is bool?) {
                _flag = JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
            }

            _isByRef = isByRef;

            //for an unsupported type this could be null
            //but then this is my bug, any thread entering this ctor , will support a type.
            var types = JIVariant.GetSupportedType(_object, dataType);
            if (types != null) {
                _type = (int)types | (isByRef ? JIVariant.VT_BYREF : 0);
            }
            else {
                throw new JIRuntimeException(JIErrorCodes.JI_VARIANT_UNSUPPORTED_TYPE);
            }
            Log.Logger.Verbose("In VariantBody(Object,bool,int) : dataType is " + dataType +
                " , referent class is " + _object.GetType() + ", byRef is " + isByRef);
            if (dataType == JIVariant.VT_NULL) {
                _isNull = true;
                _object = 0;
            }
        }

        /// <summary>
        ///Setting up a <code>VARIANT</code> with a NULL value.
        ///Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        internal JIVariantBody(NULL value) : this(0, false) {
            _isNull = true;
            _type = JIVariant.VT_NULL;
        }

        /// <summary>
        ///Setting up a <code>VARIANT</code> with a SCODE value and it's errorCode.
        ///Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="errorCode"> </param>
        /// <param name="isByRef"></param>
        internal JIVariantBody(SCODE value, int errorCode, bool isByRef) :
            this(errorCode, isByRef) {
            _isScode = true;
            _type = JIVariant.VT_ERROR;
        }

        /// <summary>
        /// Create safe array
        /// </summary>
        /// <param name="safeArray"></param>
        /// <param name="nestedClass"></param>
        /// <param name="is2Dimensional"></param>
        /// <param name="isByRef"></param>
        /// <param name="flag"></param>
        internal JIVariantBody(JIStruct safeArray, Type nestedClass, bool is2Dimensional, bool isByRef, int flag) {
            _flag = flag;
            //can't convert the array here , since this will have deffered pointers which may not be complete.
            _safeArrayStruct = safeArray;
            _isArray = true;
            if (_safeArrayStruct == null) {
                _isNull = true;
            }
            _nestedArraysRealClass = nestedClass;
            _is2Dimensional = is2Dimensional;
            //please remember JIVariant is a pointer and VariantBody is just the referent part of that.
            //for an unsupported type this could be null
            //but then this is my bug, any thread entering this ctor , will support a type.
            _isByRef = isByRef;
            var types = (int?)JIVariant.GetSupportedType(nestedClass, flag);
            if (types != null) {
                _type = (int)types | (isByRef ? JIVariant.VT_BYREF : 0);
            }
            else {
                throw new JIRuntimeException(JIErrorCodes.JI_VARIANT_UNSUPPORTED_TYPE);
            }
        }

        /// <summary>
        /// Returns the contained object.
        /// </summary>
        /// <exception cref="JIException"></exception>
        internal virtual object Object => _object ?? Array;

        /// <summary>
        /// Returns the array
        /// </summary>
        /// <exception cref="JIException"></exception>
        internal virtual JIArray Array {
            get {
                JIArray retVal = null;
                //TODO convert it to the right type based on the variantType before returning it.
                //everything is sent encapsulated in a variant(in safearray) , so an Integer[] will
                //go as a variant array for each integer, only the variantType = arry of ints. so convert the
                //array in the right format before returning it to the user. That is he must get Int[] within a JIArray
                //back.
                if (_safeArrayStruct != null) {
                    retVal = (JIArray)((JIPointer)_safeArrayStruct.GetMember(7)).GetReferent();

                    if (_is2Dimensional) {
                        var obj3 = (object[])retVal.ArrayInstance; //these will all be variants
                                                                   //correct the array here , i.e reform the 2 dimensional array before returning back.
                        var safeArrayBound = (JIArray)_safeArrayStruct.GetMember(8);

                        var safeArrayBound2 = (JIStruct[])safeArrayBound.ArrayInstance;
                        //should only be 2 since we support only 2 dim.

                        var firstDim = (int)(int?)safeArrayBound2[0].GetMember(0);
                        var secondDim = (int)(int?)safeArrayBound2[1].GetMember(0);

                        object obj = System.Array.CreateInstance(_nestedArraysRealClass, new int[] { firstDim, secondDim });
                        var obj2 = (object[][])obj;
                        var k = 0;
                        for (var i = 0; i < secondDim; i++) {
                            for (var j = 0; j < firstDim; j++) {
                                //						if (nestedArraysRealClass == JIVariant.class)
                                //						{
                                //							obj2[j][i] = ((JIVariant[])obj3)[k++];
                                //						}
                                //						else
                                //						{
                                //							obj2[j][i] = ((JIVariant[])obj3)[k++].getObject();
                                //						}
                                obj2[j][i] = obj3[k++];
                            }
                        }

                        retVal = new JIArray(obj2);

                    }
                    else {

                        if (_nestedArraysRealClass != null) {
                            var obj = (object[])retVal.ArrayInstance; //these will all be variants
                            object obj2 = System.Array.CreateInstance(_nestedArraysRealClass, obj.Length);
                            for (var i = 0; i < obj.Length; i++) {
                                //						if (nestedArraysRealClass == JIVariant.class)
                                //						{
                                //							Array.set(obj2,i,((JIVariant[])obj)[i]);//should be the native type
                                //						}
                                //						else
                                //						{
                                //							Array.set(obj2,i,((JIVariant[])obj)[i].getObject());//should be the native type
                                //						}

                                //Array.set(obj2,i,obj[i]);
                                ((object[])obj2)[i] = obj[i];
                            }
                            retVal = new JIArray(obj2);
                        }
                        else {
                            throw new JIException(JIErrorCodes.JI_VARIANT_UNSUPPORTED_TYPE);
                        }
                    }
                }
                return retVal;
            }
        }

        /// <summary>
        /// Retrieves the contained object as int.
        /// </summary>
        internal virtual int ObjectAsInt {
            get {
                try {
                    return (int)(int?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as long
        /// </summary>
        internal virtual long ObjectAsLong {
            get {
                try {
                    return (long)(long?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as unsigned
        /// </summary>
        internal virtual IJIUnsigned ObjectAsUnsigned {
            get {
                try {
                    return (IJIUnsigned)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as scode
        /// </summary>
        internal virtual int ObjectAsSCODE {
            get {
                try {
                    return ((SCODE)_object).errorCode;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as float.
        /// </summary>
        internal virtual float ObjectAsFloat {
            get {
                try {
                    return (float)(float?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as double.
        /// </summary>
        internal virtual double ObjectAsDouble {
            get {
                try {
                    return (double)(double?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as short.
        /// </summary>
        internal virtual short ObjectAsShort {
            get {
                try {
                    return (short)(short?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as bool.
        /// </summary>
        internal virtual bool ObjectAsBoolean {
            get {
                try {
                    return (bool)(bool?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as JIString.
        /// </summary>
        internal virtual JIString ObjectAsString {
            get {
                try {
                    return (JIString)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as Date.
        /// </summary>
        internal virtual DateTime ObjectAsDate {
            get {
                try {
                    return (DateTime)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as char.
        /// </summary>
        internal virtual char ObjectAsChar {
            get {
                try {
                    return (char)(char?)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the contained object as Variant.
        /// </summary>
        internal virtual JIVariant ObjectAsVariant {
            get {
                try {
                    return (JIVariant)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Returns the contained object as com object
        /// </summary>
        internal virtual IJIComObject ObjectAsComObject {
            get {
                try {
                    return (IJIComObject)_object;
                }
                catch (InvalidCastException e) {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        /// <summary>
        /// Encode object
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        internal virtual void Encode(NdrCodec ndr, List<object> defferedPointers, int flag) {

            {
                //		try
                flag |= _flag;
                //align with 8 boundary
                ndr.FillAligned(8);

                var start = ndr.Buffer.Index;

                //			if (safeArrayStruct != null)
                //			{
                //				//length for the array
                //				length = fillArrayType(ndr);
                //			}
                //			else
                //			{
                //				ndr.writeUnsignedLong(variantType);
                //			}

                //just a place holder for length
                ndr.WriteUnsignedLong(-1); // was 0xffffffff
                ndr.WriteUnsignedLong(0);

                //Type
                var varType = getVarType(_object != null ? _object.GetType() : _nestedArraysRealClass, _object);

                //For IUnknown , since the inner object is a JIComObjectImpl it will be fine.
                if ((flag & JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT) ==
                            JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT) {
                    varType = _isByRef ? 0x4000 | JIVariant.VT_DISPATCH : JIVariant.VT_DISPATCH;
                }
                ndr.WriteUnsignedShort(varType);

                //reserved bytes
                ndr.WriteUnsignedSmall(0xCC);
                ndr.WriteUnsignedSmall(0xCC);
                ndr.WriteUnsignedSmall(0xCC);
                ndr.WriteUnsignedSmall(0xCC);
                ndr.WriteUnsignedSmall(0xCC);
                ndr.WriteUnsignedSmall(0xCC);

                if (_object != null) {
                    ndr.WriteUnsignedLong(varType);
                }
                else {
                    if (!_isByRef) {
                        ndr.WriteUnsignedLong(JIVariant.VT_ARRAY);
                    }
                    else {
                        ndr.WriteUnsignedLong(JIVariant.VT_BYREF_VT_ARRAY);
                    }
                }

                if (_isByRef) {
                    var byRefFlag = -1;
                    if (_isArray) { //object arrays will come here....
                        byRefFlag = 4;
                    }
                    else {
                        //no idea what these flags are but 0x10 is for variant, 0x8 for date, and 0x4 is for others
                        if (_type == JIVariant.VT_BYREF_VT_VARIANT) {
                            byRefFlag = 0x10;
                        }
                        else if (_type == JIVariant.VT_BYREF_VT_DATE || _type == JIVariant.VT_BYREF_VT_CY) {
                            byRefFlag = 8;
                        }
                        else {
                            byRefFlag = 4;
                        }
                    }
                    ndr.WriteUnsignedLong(byRefFlag);
                }

                //we should not use the deffered pointers here, but pass our own one, so that only they are written...
                var varDefferedPointers = new List<object>();

                //we should use flag here, since the decision should be based on this only.
                setValue(ndr, _object, varDefferedPointers, flag);

                //making changes to write the deffered pointers here itself , since we need to put the entire Variant completed to the length
                //as in varType.
                var x = 0;
                while (x < varDefferedPointers.Count) {
                    var newList = new List<object>();
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIPointer), (JIPointer)varDefferedPointers[x], newList, flag);
                    x++; //incrementing index
                    varDefferedPointers.InsertRange(x, newList);
                }

                var currentIndex = 0;
                var length = (currentIndex = ndr.Buffer.Index) - start;
                var value = (int)length / 8;
                if (length % 8.0 != 0) //entire variant is aligned by 8 bytes.
                {
                    value++;
                }
                ndr.Buffer.Index = start;
                ndr.WriteUnsignedLong(value);
                ndr.Buffer.Index = currentIndex;

                Log.Logger.Verbose("Variant length is " + length + " , value " + value + " , variant type" + _type);
                //			if (safeArrayStruct != null && isArray)
                //			{
                //				//SafeArray have the alignment rule , that all Size <=4 are aligned by 4 and size 8 is aligned by 8.
                //				//Variant is aligned by 4, Interface pointers are aligned by 4 as well.
                //				//but this should not exceed the length
                //				index = new Integer(ndr.getBuffer().getIndex()).doubleValue();
                //				length = length * 8 + start;
                //				if (index < length)
                //				{
                //					Integer size = (Integer)safeArrayStruct.getMember(2);
                //					long i = 0;
                //					if (size.intValue() == 8)
                //					{
                //						if (index%8.0 != 0)
                //						{
                //							i = (i=Math.round(index%8.0)) == 0 ? 0 : 8 - i ;
                //							if (index + i <= length)
                //							{
                //								ndr.writeOctetArray(new byte[(int)i],0,(int)i);
                //							}
                //							else
                //							{
                //								ndr.writeOctetArray(new byte[(length - (int)index)],0,(int)(length - (int)index));
                //							}
                //						}
                //					}
                //					else
                //					{
                //						//align by 4...
                //						//TODO this needs to be tested for Structs and Unions.
                //						if (index%4.0 != 0)
                //						{
                //							i = (i=Math.round(index%4.0)) == 0 ? 0 : 4 - i ;
                //							if (index + i <= length)
                //							{
                //								ndr.writeOctetArray(new byte[(int)i],0,(int)i);
                //							}
                //							else
                //							{
                //								ndr.writeOctetArray(new byte[(length - (int)index)],0,(int)(length - (int)index));
                //							}
                //						}
                //					}
                //
                //
                //				}
                //			}


            }
            //		catch (JIException e)
            //		{
            //			throw new JIRuntimeException(e.getErrorCode());
            //		}
        }

        //multiple of 8.
        //	private int getMaxLength(Class c, bool isByRef, Object obj)
        //	{
        //		int length = 3; //Empty
        //		if (type3.contains(c))
        //		{
        //			length = 3;
        //			if (isByRef)
        //			{
        //				length = length + 1; //for the pointer
        //			}
        //		}
        //		else
        //		if(c.equals(Long.class) || c.equals(Double.class) || c.equals(Date.class) || c.equals(JICurrency.class))
        //		{
        //			length = 4;
        //			//here the byref can be left out since it will cover 24 bytes properly
        //		}
        //		else
        //		if(c.equals(JIString.class))
        //		{
        //
        //			int strlen = 0;
        //			if (obj != null && ((JIString)obj).getString() != null)
        //			{
        //				strlen = ((JIString)obj).getString().length();
        //			}
        //
        //			//20 is of variant, 4+4+4+4 of bstr(user,maxlen,actlen,offset) , (strlen*2) of the actual array
        //			double value = 20 + 16 + strlen*2;
        //			if (isByRef)
        //			{
        //				value = value + 4;
        //			}
        //			double d = value%8.0;
        //			length = (int)value/8;
        //			if (d != 0.0)
        //			{
        //				length++;
        //			}
        //
        //
        //		}else // for Interface pointers without
        //		if((obj instanceof IJIComObject))
        //		{
        //			double value = ((IJIComObject)obj).internal_getInterfacePointer().getLength();
        //			if (isByRef)
        //			{
        //				value = value + 4;
        //			}
        //
        //			value = value + 20 + 4 + 4 + 4; //20 of variant , 4 of the ptr, 4 of max count, 4 of actual count
        //
        //			double d = value%8.0;
        //			length = (int)value/8;
        //			if (d != 0.0)
        //			{
        //				length++;
        //			}
        //			//length += 4;
        //			//double a = ((IJIComObject)obj).getInterfacePointer().getLength()/8.0;
        //			//length = 4 + (int)Math.ceil(a);
        //		}
        //
        //
        //		return length;
        //
        //	}

        //returns the length in bytes
        private int getMaxLength2(Type c, object obj) {
            var length = 0;

            //since this is getMaxLength2 and hence will either contain
            //proper type 3 elements and not EMPTY,NULL,SCODE since these are parts of Variant.
            //and not simple types like Integer, JIUnsignedXXX or Float etc.
            if (_type3.Contains(c)) {
                length = JIMarshalUnMarshalHelper.GetLengthInBytes(c, obj, _flag);
            }
            else if (c.Equals(typeof(long?)) || c.Equals(typeof(double?)) || c.Equals(typeof(DateTime)) || c.Equals(typeof(JICurrency))) {
                length = 8;
            }
            else if (c.Equals(typeof(JIString))) {
                length = JIMarshalUnMarshalHelper.GetLengthInBytes(c, obj, _flag);
            }
            else if (obj is IJIComObject) {
                // for Interface pointers without
                double value = ((IJIComObject)obj).Internal_getInterfacePointer().Length;
                value = value + 4 + 4 + 4; //20 of variant , 4 of the ptr, 4 of max count, 4 of actual count
            }
            return length;
        }

        /// <summary>
        /// Get array length for var type
        /// </summary>
        /// <exception cref="JIException"></exception>
        private int ArrayLengthForVarType {
            get {
                //now the array will be of variants, nestedArraysRealClass identifies the class itself
                //for iteration we need the variants and then there members.

                var objArray = (JIArray)((JIPointer)_safeArrayStruct.GetMember(7)).GetReferent();
                var array = (object[])objArray.ArrayInstance;

                double length = 20; //variant
                if (_isByRef) {
                    length += 4; //byref
                }

                //SafeArray is 44
                length += 44;

                var isVariantArray = ((short)(short?)_safeArrayStruct.GetMember(1) & JIVariant.FADF_VARIANT) == JIVariant.FADF_VARIANT ? true : false;

                if (array != null) {
                    length += 4; //for max count of the array.
                    if (isVariantArray) {
                        //each variant is 3 (size 20 = 20/8 = 3)
                        for (var i = 0; i < array.Length; i++) {
                            var variant = (JIVariant)array[i];
                            length += variant.GetLengthInBytes(_flag); //* 8;//((VariantBody)(variant.member.getReferent())).variantType * 8;
                        }

                        //now for the "user" pointer part
                        //length = length + array.length * 4;
                    }
                    else {
                        //normal non variant array has been sent...
                        for (var i = 0; i < array.Length; i++) {
                            length += getMaxLength2(array[i].GetType(), array[i]);
                        }
                    }
                }
                else {
                    length += 4; //for the null 0000.
                }

                var value = (int)length / 8;
                if (length % 8.0 != 0) {
                    value++;
                }

                return value;
            }
        }

        internal static JIVariantBody Decode(NdrCodec ndr, List<object> defferedPointers,
            int flag, IDictionary<object, object> additionalData) {
            //bool readLong = false;
            var index = (double)ndr.Buffer.Index;
            if (index % 8.0 != 0) {
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
            }

            var start = ndr.Buffer.Index;
            var length = ndr.ReadUnsignedLong(); //read the potential length
            ndr.ReadUnsignedLong(); //read the reserved byte

            var variantType = ndr.ReadUnsignedShort(); //varType

            //read reserved bytes
            ndr.ReadUnsignedShort();
            ndr.ReadUnsignedShort();
            ndr.ReadUnsignedShort();

            ndr.ReadUnsignedLong(); //32 bit varType

            JIVariantBody variant = null;

            var varDefferedPointers = new List<object>();
            if ((variantType & JIVariant.VT_ARRAY) == 0x2000) {
                var isByRef = (variantType & JIVariant.VT_BYREF) == 0 ? false : true;
                //the struct may be null if the array has nothing
                var safeArray = GetDecodedValueAsArray(ndr, varDefferedPointers, variantType & ~JIVariant.VT_ARRAY, isByRef, additionalData, flag);
                var type2 = variantType;
                if (isByRef) {
                    type2 &= ~JIVariant.VT_BYREF; //so that actual type can be determined
                }

                type2 &= 0x0FFF;
                var flagofFlags = flag;
                if (type2 == JIVariant.VT_INT) {
                    flagofFlags |= JIFlags.FLAG_REPRESENTATION_VT_INT;
                }
                else {
                    if (type2 == JIVariant.VT_UINT) {
                        flagofFlags |= JIFlags.FLAG_REPRESENTATION_VT_UINT;
                    }
                    else {
                        if (type2 == JIVariant.VT_BOOL) {
                            flag = flagofFlags |= JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
                        }
                    }
                }

                if (safeArray != null) {
                    variant = new JIVariantBody(safeArray, (Type)JIVariant.GetSupportedClass(type2 & ~JIVariant.VT_ARRAY), ((object[])((JIArray)safeArray.GetMember(8)).ArrayInstance).Length > 1 ? true : false, isByRef, flagofFlags);
                }
                else {
                    variant = new JIVariantBody(null, (Type)JIVariant.GetSupportedClass(type2 & ~JIVariant.VT_ARRAY), false, isByRef, flagofFlags);
                }

                variant._flag = flagofFlags;

            }
            else {
                var isByRef = (variantType & JIVariant.VT_BYREF) == 0 ? false : true;
                variant = new JIVariantBody(GetDecodedValue(ndr, varDefferedPointers, variantType, isByRef, additionalData, flag), isByRef, variantType);
                var type2 = variantType & 0x0FFF;
                if (type2 == JIVariant.VT_INT) {
                    variant._flag = JIFlags.FLAG_REPRESENTATION_VT_INT;
                }
                else {
                    if (type2 == JIVariant.VT_UINT) {
                        variant._flag = JIFlags.FLAG_REPRESENTATION_VT_UINT;
                    }
                }
            }


            var x = 0;
            while (x < varDefferedPointers.Count) {

                var newList = new List<object>();
                var replacement = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr, (JIPointer)varDefferedPointers[x], newList, flag, additionalData);
                ((JIPointer)varDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
                x++;
                varDefferedPointers.InsertRange(x, newList);
            }

            if (variant._isArray && variant._safeArrayStruct != null) {
                //SafeArray have the alignment rule , that all Size <=4 are aligned by 4 and size 8 is aligned by 8.
                //Variant is aligned by 4, Interface pointers are aligned by 4 as well.
                //but this should not exceed the length
                index = (double)ndr.Buffer.Index;
                length = length * 8 + start;
                if (index < length) {
                    var safeArrayStruct = variant._safeArrayStruct;
                    var size = (int?)safeArrayStruct.GetMember(2);
                    long i = 0;
                    if ((int)size == 8) {
                        if (index % 8.0 != 0) {
                            i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                            if (index + i <= length) {
                                ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
                            }
                            else {
                                ndr.ReadOctetArray(new byte[(length - (int)index)], 0, (int)(length - (int)index));
                            }
                        }
                    }
                    else {
                        //align by 4...
                        //TODO this needs to be tested for Structs and Unions.
                        if (index % 4.0 != 0) {
                            i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
                            if (index + i <= length) {
                                ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
                            }
                            else {
                                ndr.ReadOctetArray(new byte[(length - (int)index)], 0, (int)(length - (int)index));
                            }
                        }
                    }
                }

                //SafeArray is complete
                JIArray array = null;
                try {
                    array = variant.Array;
                }
                catch (JIException e) {
                    throw new JIRuntimeException(e.ErrorCode);
                }
                var variantMain = new JIVariant(array, variant._isByRef, variant._flag);
                variant = (JIVariantBody)variantMain._member.GetReferent();
            }

            return variant;
        }

        /// <summary>
        /// Variants need specialised handling and the standard serializers may or maynot be used.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type getVarClass(int type) {
            Type c = null;
            //now first to check if this is a pointer or not.
            type &= 0x0FFF; //0x4XXX & 0x0FFF = real type
            switch (type) {
                case 0: //VT_EMPTY , Not specified.
                    c = typeof(EMPTY);
                    break;
                case 1: // VT_NULL , Null.
                    c = typeof(NULL);
                    break;
                case 10:
                    c = typeof(SCODE); //VT_ERROR,Scodes.
                    break;
                default:
                    c = JIVariant.GetSupportedClass(type);
                    if (c == null) {
                        //TODO log this , what has come that i don't support.
                    }
                    break;
            }
            return c;
        }

        /// <summary>
        /// Get var type
        /// </summary>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private int getVarType(Type c, object obj) {
            var type = 0; //EMPTY
            if (obj is IJIDispatch) {
                return _isByRef ? 0x4000 | JIVariant.VT_DISPATCH : JIVariant.VT_DISPATCH;
            }
            if (obj is IJIComObject) {
                return _isByRef ? 0x4000 | JIVariant.VT_UNKNOWN : JIVariant.VT_UNKNOWN;
            }
            if (c != null) {
                var type2 = JIVariant.GetSupportedType(c, _flag);
                if (type2 != null) {
                    type = (int)type2;
                }
                else {
                    Log.Logger.Warning("In getVarType: Unsupported Type found ! " + c + " , please add this to the supportedType map ! ");
                    //make that an array of variants
                    type2 = (int?)JIVariant.GetSupportedType(typeof(JIVariant), _flag);
                }

                if (_isNull) {
                    type = 1;
                }
                else if (_isScode) {
                    type = 10; //scode
                }
                else if (_isArray) {
                    type = 0x2000 | type; //0xC; should not assume an array of variants anymore
                }
            }
            if (_isByRef && type != 0 && !c.Equals(typeof(JIArray))) {
                //then it is a pointer. have to set it correctly
                type |= 0x4000;
            }
            return type;
        }

        private static object GetDecodedValue(NdrCodec ndr, List<object> defferedPointers,
            int type, bool isByRef, IDictionary<object, object> additionalData, int flag) {

            object obj = null;
            var c = getVarClass(type);
            if (c != null) {
                if (isByRef) {
                    ndr.ReadUnsignedLong(); //Read the Pointer
                }
                if (c.Equals(typeof(SCODE))) {
                    obj = JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(int?), null, flag, additionalData);
                    obj = new SCODE((int)(int?)obj);
                    type = JIVariant.VT_ERROR;
                }
                else if (c.Equals(typeof(NULL))) {
                    //have read 20 bytes
                    //JIMarshalUnMarshalHelper.deSerialize(ndr,Integer.class,null,JIFlags.FLAG_NULL);//read the last 4 bytes, since there could be parameters before this.
                    obj = NULL;
                    type = JIVariant.VT_NULL;
                }
                else if (c.Equals(typeof(EMPTY))) //empty is 20 bytes
                   {
                    obj = JIVariantBody.EMPTY;
                    type = JIVariant.VT_EMPTY;
                }
                else if (c.Equals(typeof(JIString))) {
                    obj = new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
                    obj = ((JIString)obj).Decode(ndr, null, flag, additionalData);
                }
                else if (c.Equals(typeof(bool?))) {
                    obj = JIMarshalUnMarshalHelper.Deserialize(ndr, c, defferedPointers, flag | JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL, additionalData);
                }
                else {
                    obj = JIMarshalUnMarshalHelper.Deserialize(ndr, c, defferedPointers, flag, additionalData);
                }
            }
            return obj;
        }

        /// <summary>
        /// Get decoded value
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="type"></param>
        /// <param name="isByRef"></param>
        /// <param name="additionalData"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private static JIStruct GetDecodedValueAsArray(NdrCodec ndr,
            List<object> defferedPointers, int type, bool isByRef,
            IDictionary<object, object> additionalData, int flag) {
            //int newFLAG = flag;
            if (isByRef) {
                ndr.ReadUnsignedLong(); //read the pointer
                type &= ~JIVariant.VT_BYREF; //so that actual type can be determined
            }

            //read pointer referent id
            if (ndr.ReadUnsignedLong() == 0) {
                return null;
            }

            ndr.ReadUnsignedLong(); //1

            var safeArray = new JIStruct();
            try {
                safeArray.AddMember(typeof(short?)); //dim

                var safeArrayBound = new JIStruct();
                safeArrayBound.AddMember(typeof(int?));
                safeArrayBound.AddMember(typeof(int?)); //starts at 0

                safeArray.AddMember(typeof(short?)); //flags
                safeArray.AddMember(typeof(int?)); //size
                safeArray.AddMember(typeof(short?)); //locks
                safeArray.AddMember(typeof(short?)); //locks
                safeArray.AddMember(typeof(int?)); //safearrayunion
                safeArray.AddMember(typeof(int?)); //size in safearrayunion

                var c = (Type)JIVariant._supportedTypes_classes[type];
                if (c == null) {
                    Log.Logger.Warning("From JIVariant: while decoding an Array, type " + type + " , was not found in supportedTypes_classes map , hence using JIVariant instead...");
                    //not available , lets try with JIVariant.
                    //This is a bug, I should have the type.
                    c = typeof(JIVariant);
                }

                if (c == typeof(bool?)) {
                    flag |= JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
                }
                //HARDCODING to JIVariant...kindof forgotten why I even wrote the code below.
                //since all of the examples I have come across always return a Variant array.
                //then why did I typify this thing to it's class (like JIString), it produces an
                //exception when the result is returned back is not an array of strings...
                //c = JIVariant.class;
                JIArray values = null;
                if (c == typeof(JIString)) {
                    values = new JIArray(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), null, 1, true);
                    safeArray.AddMember(new JIPointer(values)); //single dimension array, will convert it into the
                                                                //[] or [][] after inspecting dimension read.
                }
                else {
                    values = new JIArray(c, null, 1, true);
                    safeArray.AddMember(new JIPointer(values)); //single dimension array, will convert it into the
                                                                //[] or [][] after inspecting dimension read.
                }

                safeArray.AddMember(new JIArray(safeArrayBound, null, 1, true));

                safeArray = (JIStruct)JIMarshalUnMarshalHelper.Deserialize(ndr, safeArray, defferedPointers, flag, additionalData);

                //now set the right class after examining the flags , only set for JIVariant.class now., the BSTR would already be set previously.
                var features = (short?)safeArray.GetMember(1);
                //this condition is being kept in the front since the feature flags can be a combination of FADF_VARIANT and the
                //other flags , in which case the Variant takes priority (since they will all be wrapped as variants).
                if (((short)features & JIVariant.FADF_VARIANT) == JIVariant.FADF_VARIANT) {
                    values.UpdateClazz(typeof(JIVariant));
                }
                else if ((((short)features & JIVariant.FADF_DISPATCH) == JIVariant.FADF_DISPATCH) || (((short)features & JIVariant.FADF_UNKNOWN) == JIVariant.FADF_UNKNOWN)) {
                    values.UpdateClazz(typeof(IJIComObject));
                }
                //For JIStrings , it will be done before these above conditions are examined.


            }
            catch (JIException e) {
                throw new JIRuntimeException(e.ErrorCode);
            }


            return safeArray;
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="obj"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        private void setValue(NdrCodec ndr, object obj, List<object> defferedPointers, int flag) {
            if (_isNull) {
                return; //null , is only 20 bytes
            }
            if (obj != null) {
                var c = obj.GetType();

                if (c.Equals(typeof(EMPTY))) //20 bytes
                {
                    return;
                }
                if (obj is IJIComObject) {
                    c = typeof(IJIComObject);
                }
                JIMarshalUnMarshalHelper.Serialize(ndr, c, obj, defferedPointers, flag);
            }
            else {

                ndr.WriteUnsignedLong(new object().GetHashCode()); //pointer referentId
                ndr.WriteUnsignedLong(1);

                JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIStruct), _safeArrayStruct, defferedPointers, flag);
            }
        }

        /// <summary>
        /// Whether body is in form of array
        /// </summary>
        internal virtual bool IsArray => _isArray;

        /// <summary>
        /// Total length in bytes
        /// </summary>
        internal virtual int LengthInBytes {
            get {
                if (_safeArrayStruct == null && _object.GetType().Equals(typeof(EMPTY))) {
                    return 28;
                }

                if (_isArray) {
                    var length = 0;
                    //			JIArray objArray = (JIArray)((JIPointer)safeArrayStruct.getMember(7)).getReferent();
                    //			Object[] array = (Object[])objArray.getArrayInstance();
                    //			for (int i = 0; i < array.length; i++)
                    //			{
                    //				Class c = array[i].getClass();
                    //				length = length + JIMarshalUnMarshalHelper.getLengthInBytes(c,array[i],flag);
                    //			}
                    //			return length;
                    try {
                        length = ArrayLengthForVarType * 8;
                    }
                    catch (JIException e) {
                        throw new Exception("", e);
                    }

                    return length;
                }
                var c = _object.GetType();

                if (_object is IJIComObject) {
                    c = typeof(IJIComObject);
                }
                else {
                    if (c.Equals(typeof(SCODE))) {
                        return 24 + 4; //4 for integer scode.
                    }
                    if (c.Equals(typeof(NULL)) || c.Equals(typeof(EMPTY))) {
                        return 24;
                    }
                }

                return 24 + JIMarshalUnMarshalHelper.GetLengthInBytes(c, _object, _flag);
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            var retVal = "";
            if (_object == null) {
                retVal += "obj is null , ";
            }
            else {
                retVal += _object.ToString();
            }
            if (_isArray) {
                if (_is2Dimensional) {
                    retVal += "2 dimensional array , ";
                }
                else {
                    retVal = "1 dimensional array , ";
                }
                if (_safeArrayStruct != null) {
                    retVal += _safeArrayStruct.ToString();
                }
            }

            return retVal;
        }

        private readonly bool _is2Dimensional;
        private object _object;
        private readonly int _type = -1;
        private JIStruct _safeArrayStruct;
        private bool _isArray;
        private readonly bool _isScode;
        private readonly bool _isNull;
        private readonly Type _nestedArraysRealClass;
        private static List<object> _type3 = new List<object>();
        private bool _isByRef;
        internal int _flag;
    }
}