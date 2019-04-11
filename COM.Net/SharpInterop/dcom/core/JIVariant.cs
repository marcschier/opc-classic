//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.impls.automation;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Class representing the <code>VARIANT</code> datatype. Please use the 
    /// <code>byRef</code> flag based constructors for <i>by reference</i>  
    /// parameters in COM calls. For <code>[optional]</code> parameters use the
    /// <seealso cref="CreateOPTIONAL_PARAM()"/>
    /// </summary>
    /// <remarks>
    /// In case of direct calls to COM server using <code>JICallBuilder</code>, 
    /// if the <code>byRef</code> flag is set then that variant should also be 
    /// added as the <code>[out]</code> parameter in the <code>JICallBuilder</code>.
    /// For developers using the <code>IJIDispatch </code> this is not required and 
    /// variant would be returned back to them via <code>JIVariant[]</code>  
    /// associated with <code>IJIDispatch</code> apis. An <b>important</b> note for 
    /// <code>Boolean</code> Arrays (<code>JIArray</code> of <code>Boolean</code>), 
    /// please set the <code>JIFlag.FLAG_REPRESENTATION_VARIANT_BOOL</code> using the 
    /// <seealso cref="Flag"/> method before making a call on this object. This is 
    /// required since in DCOM, <code>VARIANT_BOOL</code> are 2 bytes and standard 
    /// <code>bool</code>s are 1 byte in length.
    /// </remarks>
    [Serializable]
    public sealed class JIVariant {

        /// <summary> id </summary>
        public const int VT_NULL = 0x00000001;
        /// <summary> id </summary>
        public const int VT_EMPTY = 0x00000000;
        /// <summary> id </summary>
        public const int VT_I4 = 0x00000003;
        /// <summary> id </summary>
        public const int VT_UI1 = 0x00000011;
        /// <summary> id </summary>
        public const int VT_I2 = 0x00000002;
        /// <summary> id </summary>
        public const int VT_R4 = 0x00000004;
        /// <summary> id </summary>
        public const int VT_R8 = 0x00000005;
        /// <summary> id </summary>
        public const int VT_VARIANT = 0x0000000c;
        /// <summary> id </summary>
        public const int VT_BOOL = 0x0000000b;
        /// <summary> id </summary>
        public const int VT_ERROR = 0x0000000a;
        /// <summary> id </summary>
        public const int VT_CY = 0x00000006;
        /// <summary> id </summary>
        public const int VT_DATE = 0x00000007;
        /// <summary> id </summary>
        public const int VT_BSTR = 0x00000008;
        /// <summary> id </summary>
        public const int VT_UNKNOWN = 0x0000000d;
        /// <summary> id </summary>
        public const int VT_DECIMAL = 0x0000000e;
        /// <summary> id </summary>
        public const int VT_DISPATCH = 0x00000009;
        /// <summary> id </summary>
        public const int VT_ARRAY = 0x00002000;
        /// <summary> id </summary>
        public const int VT_BYREF = 0x00004000;

        /// <summary> id </summary>
        public const int VT_BYREF_VT_UI1 = VT_BYREF | VT_UI1; //0x00004011;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_I2 = VT_BYREF | VT_I2; //0x00004002;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_I4 = VT_BYREF | VT_I4; //0x00004003;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_R4 = VT_BYREF | VT_R4; //0x00004004;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_R8 = VT_BYREF | VT_R8; //0x00004005;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_BOOL = VT_BYREF | VT_BOOL; //0x0000400b;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_ERROR = VT_BYREF | VT_ERROR; //0x0000400a;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_CY = VT_BYREF | VT_CY; //0x00004006;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_DATE = VT_BYREF | VT_DATE; //0x00004007;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_BSTR = VT_BYREF | VT_BSTR; //0x00004008;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_UNKNOWN = VT_BYREF | VT_UNKNOWN; //0x0000400d;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_DISPATCH = VT_BYREF | VT_DISPATCH; //0x00004009;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_ARRAY = VT_BYREF | VT_ARRAY; //0x00006000;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_VARIANT = VT_BYREF | VT_VARIANT; //0x0000400c;

        /// <summary> id </summary>
        public const int VT_I1 = 0x00000010;
        /// <summary> id </summary>
        public const int VT_UI2 = 0x00000012;
        /// <summary> id </summary>
        public const int VT_UI4 = 0x00000013;
        /// <summary> id </summary>
        public const int VT_I8 = 0x00000014;
        /// <summary> id </summary>
        public const int VT_INT = 0x00000016;
        /// <summary> id </summary>
        public const int VT_UINT = 0x00000017;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_DECIMAL = VT_BYREF | VT_DECIMAL; //0x0000400e;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_I1 = VT_BYREF | VT_I1; //0x00004010;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_UI2 = VT_BYREF | VT_UI2; //0x00004012;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_UI4 = VT_BYREF | VT_UI4; //0x00004013;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_I8 = VT_BYREF | VT_I8; //0x00004014;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_INT = VT_BYREF | VT_INT; //0x00004016;
        /// <summary> id </summary>
        public const int VT_BYREF_VT_UINT = VT_BYREF | VT_UINT; //0x00004017;

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

        internal static Hashtable _supportedTypes = new Hashtable();
        internal static Hashtable _supportedTypes_classes = new Hashtable();
        internal static Hashtable _outTypesMap = new Hashtable();

        /// <summary>
        /// Initialize variant
        /// </summary>
        static JIVariant() {
            //CAUTION NO PTR TYPE SHOULD BE PART OF THIS MAP !!!
            _outTypesMap[typeof(int)] = 0;
            _outTypesMap[typeof(int)] = 0;
            _outTypesMap[typeof(short)] = (short)0;
            _outTypesMap[typeof(short)] = (short)0;
            _outTypesMap[typeof(float)] = 0.0;
            _outTypesMap[typeof(float)] = 0.0;
            _outTypesMap[typeof(double)] = 0.0;
            _outTypesMap[typeof(double)] = 0.0;
            _outTypesMap[typeof(bool)] = false;
            _outTypesMap[typeof(bool)] = false;
            _outTypesMap[typeof(string)] = "";
            _outTypesMap[typeof(JICurrency)] = new JICurrency("0.0");
            _outTypesMap[typeof(DateTime)] = DateTime.Now;
            _outTypesMap[typeof(char)] = '9';
            _outTypesMap[typeof(char)] = '9';
            _outTypesMap[typeof(JIUnsignedByte)] = JIUnsignedFactory.GetUnsigned((short)0, JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE);
            _outTypesMap[typeof(JIUnsignedShort)] = JIUnsignedFactory.GetUnsigned(0, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
            _outTypesMap[typeof(JIUnsignedInteger)] = JIUnsignedFactory.GetUnsigned(0, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            _outTypesMap[typeof(long)] = 0;
            _outTypesMap[typeof(long)] = 0;
            _supportedTypes[typeof(object)] = VT_VARIANT;
            _supportedTypes[typeof(JIVariant)] = VT_VARIANT;
            _supportedTypes[typeof(int)] = VT_I4;
            _supportedTypes[typeof(JIUnsignedInteger)] = VT_UI4;
            _supportedTypes[typeof(float)] = VT_R4;
            _supportedTypes[typeof(bool)] = VT_BOOL;
            _supportedTypes[typeof(double)] = VT_R8;
            _supportedTypes[typeof(short)] = VT_I2;
            _supportedTypes[typeof(JIUnsignedShort)] = VT_UI2;
            _supportedTypes[typeof(sbyte)] = VT_I1;
            _supportedTypes[typeof(char)] = VT_I1;
            _supportedTypes[typeof(JIUnsignedByte)] = VT_UI1;
            _supportedTypes[typeof(JIString)] = VT_BSTR;
            //		supportedTypes.put(IJIUnknown.class,new Integer(VT_UNKNOWN));
            //		supportedTypes.put(IJIDispatch.class,new Integer(VT_DISPATCH));
            _supportedTypes[typeof(SCODE)] = VT_ERROR;
            _supportedTypes[typeof(EMPTY)] = VT_EMPTY;
            _supportedTypes[typeof(NULL)] = VT_NULL;
            _supportedTypes[typeof(JIArray)] = VT_ARRAY;
            //		supportedTypes.put(JIComObjectImpl.class,new Integer(VT_UNKNOWN));
            //		supportedTypes.put(JIDispatchImpl.class,new Integer(VT_DISPATCH));
            _supportedTypes[typeof(DateTime)] = VT_DATE;
            _supportedTypes[typeof(JICurrency)] = VT_CY;
            _supportedTypes[typeof(long)] = VT_I8;

            _supportedTypes_classes[VT_DATE] = typeof(DateTime);
            _supportedTypes_classes[VT_CY] = typeof(JICurrency);
            _supportedTypes_classes[VT_VARIANT] = typeof(JIVariant);
            _supportedTypes_classes[VT_I4] = typeof(int);
            _supportedTypes_classes[VT_INT] = typeof(int);
            _supportedTypes_classes[VT_UI4] = typeof(JIUnsignedInteger);
            _supportedTypes_classes[VT_UINT] = typeof(JIUnsignedInteger);
            _supportedTypes_classes[VT_R4] = typeof(float);
            _supportedTypes_classes[VT_BOOL] = typeof(bool);
            _supportedTypes_classes[VT_R8] = typeof(double);
            _supportedTypes_classes[VT_I2] = typeof(short);
            _supportedTypes_classes[VT_UI2] = typeof(JIUnsignedShort);
            _supportedTypes_classes[VT_I1] = typeof(char);
            _supportedTypes_classes[VT_UI1] = typeof(JIUnsignedByte);
            _supportedTypes_classes[VT_BSTR] = typeof(JIString);
            _supportedTypes_classes[VT_ERROR] = typeof(SCODE);
            _supportedTypes_classes[VT_EMPTY] = typeof(EMPTY);
            _supportedTypes_classes[VT_NULL] = typeof(NULL);
            _supportedTypes_classes[VT_ARRAY] = typeof(JIArray);
            _supportedTypes_classes[VT_UNKNOWN] = typeof(IJIComObject);
            _supportedTypes_classes[VT_DISPATCH] = typeof(IJIComObject);
            _supportedTypes_classes[VT_I8] = typeof(long);

            //for by ref types, do it at runtime.
            kArryInits.Add(typeof(JIString));
            kArryInits.Add(typeof(JIPointer));
            //		arryInits.add(JIComObjectImpl.class);
            //		arryInits.add(JIDispatchImpl.class);
            //		arryInits.add(IJIUnknown.class);
            kArryInits.Add(typeof(IJIComObject));
            kArryInits.Add(typeof(IJIDispatch)); //this can only happen in case of an array
        }

        /// <summary>
        /// Get out param for type
        /// </summary>
        /// <param name="c"></param>
        /// <param name="isArray"></param>
        /// <returns></returns>
        public static JIVariant OUTPARAMforType(Type c, bool isArray) {
            JIVariant variant = null;
            if (!isArray) {
                try {
                    variant = MakeVariant(_outTypesMap[c], true);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                }

                if (c == typeof(IJIDispatch)) {
                    return CreateOUT_IDISPATCH();
                }
                if (c == typeof(IJIComObject)) {
                    return CreateOUT_IUNKNOWN();
                }
                if (c == typeof(JIVariant)) {
                    return CreateEMPTY_BYREF();
                }
                if (c == typeof(JIString)) {
                    return new JIVariant("", true);
                }
            }
            else {
                try {
                    var oo = _outTypesMap[c];
                    if (oo != null) {
                        //we will always send a single dimension array.
                        object x = Array.CreateInstance(c, 1);
                        ((Array)x).SetValue(oo, 0);
                        variant = new JIVariant(new JIArray(x, true), true);
                    }
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                }

                if (c == typeof(IJIDispatch)) {
                    IJIComObject[] arry = { new JIComObjectImpl(null, new JIInterfacePointer(null, -1, null)) };
                    variant = new JIVariant(new JIArray(arry, true), true) {
                        Flag =
                            JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT |
                            JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT
                    };
                }
                else if (c == typeof(IJIComObject)) {
                    IJIComObject[] arry = { new JIComObjectImpl(null, new JIInterfacePointer(null, -1, null)) };
                    variant = new JIVariant(new JIArray(arry, true), true) {
                        Flag =
                            JIFlags.FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT |
                            JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT
                    };
                }
                else {
                    if (c == typeof(JIVariant)) {
                        return CreateVARIANTARRAY();
                    }
                    if (c == typeof(JIString) || c == typeof(string)) {
                        return CreateBSTRARRAY();
                    }
                }
            }
            return variant;
        }

        /// <summary>
        /// Returns a JIVariant (of the right type) based on the <code>o.getClass()</code>
        /// </summary>
        /// <param name="o"> </param>
        /// <param name="isByRef">
        /// </param>
        public static JIVariant MakeVariant(object o, bool isByRef = false) {
            if (o == null || o.GetType() == typeof(object)) {
                if (isByRef) {
                    return CreateEMPTY_BYREF();
                }
                return CreateEMPTY();
            }
            var c = o.GetType();
            if (c.IsArray) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_VARIANT_ONLY_JIARRAY_EXCEPTED));
            }
            if (c == typeof(JIVariant)) {
                return new JIVariant((JIVariant)o);
            }
            try {
                ConstructorInfo ctor = null;
                //now we look at the class and return a JIVariant.
                if (c == typeof(bool)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(bool), typeof(bool) });
                }
                else if (c == typeof(char)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(char), typeof(bool) });
                }
                else if (c == typeof(sbyte)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(sbyte), typeof(bool) });
                }
                else if (c == typeof(short)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(short), typeof(bool) });
                }
                else if (c == typeof(int)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(int), typeof(bool) });
                }
                else if (c == typeof(long)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(long), typeof(bool) });
                }
                else if (c == typeof(float)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(float), typeof(bool) });
                }
                else if (c == typeof(double)) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(double), typeof(bool) });
                }
                else if (o is IJIComObject) {
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { typeof(IJIComObject), typeof(bool) });
                }
                else {
                    //should cover all the rest cases.
                    ctor = typeof(JIVariant).GetConstructor(new Type[] { c, typeof(bool) });
                }
                return (JIVariant)ctor.Invoke(new object[] { o, Convert.ToBoolean(isByRef) });
            }
            catch (Exception e) {
                Log.Logger.Warning(e, "Could not create Variant for " + o + ", isByRef " + isByRef);
            }
            return null;
        }

        /// <summary>
        /// Get supported type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetSupportedClass(int? type) {
            if (!_supportedTypes_classes.TryGetValue(type, out var result)) {
                return null;
            }
            return (Type)result;
        }

        /// <summary>
        /// Get supported type id
        /// </summary>
        /// <param name="c"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        internal static int? GetSupportedType(Type c, int flag) {
            if (!_supportedTypes.TryGetValue(c, out var type)) {
                if (typeof(IJIComObject) == c) {
                    return VT_UNKNOWN;
                }
                if (typeof(IJIDispatch) == c) {
                    return VT_DISPATCH;
                }
                return null;
            }
            var retVal = (int)type;
            if (retVal == VT_I4 &&
                (flag & JIFlags.FLAG_REPRESENTATION_VT_INT) == JIFlags.FLAG_REPRESENTATION_VT_INT) {
                // means that if retval came back as VT_I4, we should make that VT_INT
                return VT_INT;
            }
            else if (retVal == VT_UI4 &&
                (flag & JIFlags.FLAG_REPRESENTATION_VT_UINT) == JIFlags.FLAG_REPRESENTATION_VT_UINT) {
                return VT_UINT;
            }
            return retVal;
        }

        /// <summary>
        /// Get supported type id
        /// </summary>
        /// <param name="o"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        internal static int GetSupportedType(object o, int defaultType) {
            var retval = defaultType;
            var c = o.GetType();
            if (_supportedTypes.ContainsKey(c)) {
                retval = (int)_supportedTypes[c];
            }
            // Order is important since IJIDispatch derieves from IJIComObject
            if (o is IJIDispatch) {
                retval = VT_DISPATCH;
            }
            else if (o is IJIComObject) {
                retval = VT_UNKNOWN;
            }
            return retval;
        }

        /// <summary>
        /// EMPTY <code>VARIANT</code>.
        /// This is not Thread Safe, hence a new instance must be taken each time.
        /// </summary>
        public static JIVariant CreateEMPTY() => new JIVariant(false, null);

        /// <summary>
        /// EMPTY BYREF <code>VARIANT</code>.
        /// This is not Thread Safe, hence a new instance must be taken each time. Used for a
        /// <code>[out] VARIANT*</code> .
        /// </summary>
        public static JIVariant CreateEMPTY_BYREF() => new JIVariant(CreateEMPTY());

        /// <summary>
        /// <code>VARIANT</code> for <code>([out] IUnknown*)</code>.
        /// This is not Thread Safe, hence a new instance must be taken each time.
        /// </summary>
        public static JIVariant CreateOUT_IUNKNOWN() {
            var retval = new JIVariant(new JIComObjectImpl(null, new JIInterfacePointer(null, -1, null)), true) {
                Flag =
                    JIFlags.FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT |
                    JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT
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
        public static JIVariant CreateOUT_IDISPATCH() {
            var retval = new JIVariant(
                new JIComObjectImpl(null, new JIInterfacePointer(null, -1, null)), true) {
                Flag =
                    JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT |
                    JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT
            };
            return retval;
        }

        /// <summary>
        /// NULL <code>VARIANT</code>.
        /// This is not Thread Safe, hence a new instance must be taken each time.
        /// </summary>
        public static JIVariant CreateNULL() => 
            new JIVariant(false, new JIVariantBody(NULL.Value));

        /// <summary>
        /// OPTIONAL PARAM. Pass this when a parameter is <code>[optional]</code> 
        /// for a COM call.
        /// This is not Thread Safe, hence a new instance must be taken each time.
        /// </summary>
        public static JIVariant CreateOPTIONAL_PARAM() => 
            new JIVariant(new SCODE(JIErrorCodes.DISP_E_PARAMNOTFOUND));

        /// <summary>
        /// Helper method for creating an array of <code>BSTR</code>s, 
        /// IDL signature <code>[in, out] SAFEARRAY(BSTR) *p</code>.
        /// The return value can directly be used in an <code>IJIDispatch</code>call.
        /// </summary>
        public static JIVariant CreateBSTRARRAY() => 
            new JIVariant(new JIArray(new JIString[] { new JIString("") }, true), true);

        /// <summary>
        /// Helper method for creating an array of <code>VARIANT</code>s, 
        /// IDL signature <code>[in, out] SAFEARRAY(VARIANT) *p</code>
        /// OR <code>[in,out] VARIANT *pArray</code>. The return value 
        /// can directly be used in an <code>IJIDispatch</code> call.
        /// </summary>
        public static JIVariant CreateVARIANTARRAY() => 
            new JIVariant(new JIArray(new JIVariant[] { CreateEMPTY() }, true), true);

        /// <summary>
        /// Called when this variant is nested
        /// </summary>
        internal bool Deffered {
            set {
                if (_member != null && !_member.Reference) {
                    _member.Deffered = value;
                }
            }
        }

        /// <summary>
        /// Sets a <code>JIFlags</code> value to be used while encoding 
        /// (marshalling) this Variant.
        /// </summary>
        public int Flag {
            set {
                var variantBody = (JIVariantBody)_member.GetReferent();
                variantBody._flag |= value;
            }
            get {
                var variantBody = (JIVariantBody)_member.GetReferent();
                return variantBody._flag;
            }
        }

        /// <summary>
        /// Returns whether this variant is a <code>NULL</code> variant.
        /// </summary>
        /// <returns> <code>true</code> if the variant is a 
        /// <code>NULL</code> </returns>
        public bool IsNull {
            get {
                if (_member == null) {
                    return true;
                }
                var variantBody = (JIVariantBody)_member.GetReferent();
                return variantBody == null ? true : variantBody.IsNull;
            }
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> as reference to another. 
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="variant"> </param>
        public JIVariant(JIVariant variant) :
            this(true, variant) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with an <code>int</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// Used when the variant type is VT_I4.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to 
        /// be represented as a pointer. LONG* </param>
        public JIVariant(int value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>long</code>.
        /// Used via serializing the <code>VARIANT</code>.
        /// Used when the variant type is VT_I8.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to 
        /// be represented as a pointer. </param>
        public JIVariant(long value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>float</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to 
        /// be represented as a pointer. FLOAT* </param>
        public JIVariant(float value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>bool</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to 
        /// be represented as a pointer. VARIANT_BOOL* </param>
        public JIVariant(bool value, bool isByRef = false) :
            this(isByRef, (object)value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>double</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to be 
        /// represented as a pointer. DOUBLE* </param>
        public JIVariant(double value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>short</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to be
        /// represented as a pointer. SHORT* </param>
        public JIVariant(short value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>char</code>.
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to be
        /// represented as a pointer. CHAR* </param>
        public JIVariant(char value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>JIString</code>.
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to be
        /// represented as a pointer. BSTR* </param>
        public JIVariant(JIString value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>String</code>. 
        /// Used via serializing the <code>VARIANT</code>. Internally a
        /// <code>JIString</code> is formed with it's default type 
        /// <code>BSTR</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to be
        /// represented as a pointer. BSTR* </param>
        public JIVariant(string value, bool isByRef = false) :
            this(new JIString(value), isByRef) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with an <code>IJIComObject</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to be
        /// represented as a pointer. IJIComObject** </param>
        public JIVariant(IJIComObject value, bool isByRef = false) :
            this(isByRef, value) {
            if (value is IJIDispatch) {
                Flag = JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
            }
            else {
                Flag = JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
            }
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>SCODE</code> 
        /// value and it's <code>errorCode</code>.
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="scode"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to
        /// be represented as a pointer. SCODE* </param>
        public JIVariant(SCODE scode, bool isByRef = false) :
            this(isByRef, scode) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with an <code>java.util.Date</code>.
        /// Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to 
        /// be represented as a pointer. Date* </param>
        public JIVariant(DateTime value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        ///Setting up a <code>VARIANT</code> with a <code>JICurrency</code>.
        ///Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to 
        /// be represented as a pointer. Date* </param>
        public JIVariant(JICurrency value, bool isByRef = false) :
            this(isByRef, value) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>unsigned</code> 
        /// value. Used via serializing the <code>VARIANT</code>.
        /// </summary>
        /// <param name="number"> </param>
        /// <param name="isByRef"> <code>true</code> if the value is to
        /// be represented as a pointer. </param>
        public JIVariant(IJIUnsigned number, bool isByRef = false) :
            this(isByRef, number) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>JIArray</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// Only 1 and 2 dimensional array is supported.
        /// </summary>
        /// <param name="array"> </param>
        /// <param name="flag"> JIFlag value </param>
        public JIVariant(JIArray array, int flag) :
            this(array, false, flag) {
        }

        /// <summary>
        /// Setting up a <code>VARIANT</code> with a <code>JIArray</code>. 
        /// Used via serializing the <code>VARIANT</code>.
        /// Only 1 and 2 dimensional array is supported.
        /// </summary>
        /// <param name="array"> </param>
        /// <param name="isByRef"> </param>
        /// <param name="flag"> JIFlag value </param>
        public JIVariant(JIArray array, bool isByRef = false, 
            int flag = JIFlags.FLAG_NULL) :
            this(isByRef, array, flag) {
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private JIVariant() {
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isByRef"></param>
        private JIVariant(bool isByRef, object obj) {
            if (obj != null && obj.GetType().IsArray) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_VARIANT_ONLY_JIARRAY_EXCEPTED));
            }
            if (obj is JIInterfacePointer) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_VARIANT_TYPE_INCORRECT));
            }
            if (obj is JIVariantBody) {
                _member = new JIPointer(obj);
            }
            else {
                _member = new JIPointer(new JIVariantBody(obj, isByRef));
            }
            _member.SetReferent(0x72657355); //"User" in LEndian.
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="isByRef"></param>
        /// <param name="array"></param>
        /// <param name="flag"></param>
        private JIVariant(bool isByRef, JIArray array, int flag) {
            JIVariantBody variant2 = null;
            JIArray array2 = null;
            Type c = null;
            object[] newArrayObj = null;
            var is2Dim = false;

            if (array == null) {
                _member = new JIPointer(new JIVariantBody(null, false));
                _member.SetReferent(0x72657355); //"User" in LEndian.
                return;
            }

            switch (array.Dimensions) {
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
                    //variants = new JIVariant[array.getNumElementsInAllDimensions()];

                    // TODO
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                    var name = obj2.GetType().FullName;
                    object subArray = obj2;
                    name = name.Substring(1);
                    var firstDim = ((object[])subArray).Length;
                    // TODO:      subArray = Array.get(subArray, 0);
                    var secondDim = ((object[])subArray).Length;
                    var k = 0;
                    newArrayObj = (object[])Array.CreateInstance(subArray.GetType().GetElementType(), array.NumElementsInAllDimensions);
                    for (var i = 0; i < secondDim; i++) {
                        for (var j = 0; j < firstDim; j++) {
                            newArrayObj[k++] = obj2[j][i];
                        }
                    }
                    c = subArray.GetType().GetElementType();
                    is2Dim = true;
                    break;
                default:
                    throw new ArgumentException(JISystem.GetLocalizedMessage(
                        JIErrorCodes.JI_VARIANT_VARARRAYS_2DIMRES));
            }
            //should always be conformant since this is part of a safe array.
            array2 = new JIArray(newArrayObj, true); 

            var safeArray = new JIStruct();
            try {
                safeArray.AddMember((short)array.Dimensions); //dim
                var elementSize = -1;
                short flags = FADF_HAVEVARTYPE;
                if (c == typeof(JIVariant)) {
                    flags = (short)(flags | FADF_VARIANT);
                    elementSize = 16; //(Variant is pointer whose size is 16)
                }
                else {
                    if (kArryInits.Contains(c)) {
                        if (c == typeof(JIString)) {
                            flags = (short)(flags | FADF_BSTR);
                        }
                        else {
                            if (c == typeof(IJIComObject)) {
                                flags = (short)(flags | FADF_UNKNOWN);
                                flag |= JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
                            }
                            else {
                                if (c == typeof(IJIDispatch)) {
                                    flags = (short)(flags | FADF_DISPATCH);
                                    flag |= JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
                                }
                            }
                        }
                        elementSize = 4; //Since all these are pointers inherently
                    }
                    else {
                        //JStruct and JIUnions are expected to be encapsulated within pointers...they usually are :)
                        elementSize = JIMarshalUnMarshalHelper.GetLengthInBytes(c, null, c == typeof(bool) ?
                            JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL : JIFlags.FLAG_NULL); //All other types, basic types
                    }
                }

                JIStruct safeArrayBound = null;

                var upperBounds = array.UpperBounds;
                var arrayOfSafeArrayBounds = new JIStruct[array.Dimensions];
                for (var i = 0; i < array.Dimensions; i++) {
                    safeArrayBound = new JIStruct();
                    safeArrayBound.AddMember(upperBounds[i]);
                    safeArrayBound.AddMember(0); //starts at 0
                    arrayOfSafeArrayBounds[i] = safeArrayBound;
                }

                var arrayOfSafeArrayBounds2 = new JIArray(arrayOfSafeArrayBounds, true);

                safeArray.AddMember(flags); //flags
                if (elementSize > 0) {
                    safeArray.AddMember(elementSize);
                }
                else {
                    elementSize = JIMarshalUnMarshalHelper.GetLengthInBytes(c, null, flag);
                    safeArray.AddMember(elementSize); //size
                }

                safeArray.AddMember((short)0); //locks
                safeArray.AddMember((short)GetSupportedType(c, flag)); //variant array, safearrayunion
                                                                       //peculiarity here, windows seems to be sending the signed type in VarType32...
                if (c == typeof(JIUnsignedByte)) {
                    safeArray.AddMember(GetSupportedType(typeof(sbyte), flag)); //safearrayunion
                }
                else if (c == typeof(JIUnsignedShort)) {
                    safeArray.AddMember(GetSupportedType(typeof(short), flag)); //safearrayunion
                }
                else if (c == typeof(JIUnsignedInteger)) {
                    safeArray.AddMember(GetSupportedType(typeof(int), flag)); //safearrayunion
                }
                else if (c == typeof(bool)) {
                    safeArray.AddMember(GetSupportedType(typeof(short), flag)); //safearrayunion
                }
                else if (c == typeof(double)) {
                    safeArray.AddMember(GetSupportedType(typeof(long), flag)); //safearrayunion
                }
                else if (c == typeof(float)) {
                    safeArray.AddMember(GetSupportedType(typeof(int), flag)); //safearrayunion
                }
                else {
                    safeArray.AddMember(GetSupportedType(c, flag)); //safearrayunion
                }
                safeArray.AddMember(array2.NumElementsInAllDimensions); //size in safearrayunion
                var ptr2RealArray = new JIPointer(array2);
                safeArray.AddMember(ptr2RealArray);
                safeArray.AddMember(arrayOfSafeArrayBounds2);
            }
            catch (JIException e) {
                throw new JIRuntimeException(e.ErrorCode);
            }

            variant2 = new JIVariantBody(safeArray, c, is2Dim, isByRef, flag);
            _member = new JIPointer(variant2, false);
            _member.SetReferent(0x72657355); //"User" in LEndian.
        }

        /// <summary>
        /// Returns the contained object.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public object Object {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).Object;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>int</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public int ObjectAsInt {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsInt;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>float</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public float ObjectAsFloat {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsFloat;
            }
        }

        /// <summary>
        /// Retrieves the contained objects errorCode.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public int ObjectAsSCODE {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsSCODE;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>double</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public double ObjectAsDouble {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsDouble;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>short</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public short ObjectAsShort {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsShort;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>bool</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public bool ObjectAsBoolean {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsBoolean;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>JIString</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public JIString ObjectAsString {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsString;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>String</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public string ObjectAsString2 {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsString.String;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>DateTime</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public DateTime ObjectAsDate {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsDate;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>char</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public char ObjectAsChar {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsChar;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>IJIComObject</code>.
        /// Return value must be "narrowed" to get the expected type.
        /// for example :- If expected type is an <code>IJIDispatch</code>,
        /// then the return value must pass through
        /// <code>JIObjectFactory.narrowInstance(IJIComObject)</code> to get to the right type.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public IJIComObject ObjectAsComObject {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsComObject;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>JIVariant</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public JIVariant ObjectAsVariant {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsVariant;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>JIArray</code>. Only 1 and 2 dim arrays are supported currently.
        /// Please note that this array is <b>not</b> backed by this variant and is a <b>new</b> copy. If the array
        /// is <code>IJIComObject</code>s, please make sure to use <code>JIObjectFactory.narrowObject()</code> to
        /// get the right instance.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public JIArray ObjectAsArray {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).Array;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>long</code>, used when the expected type is VT_I8.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public long ObjectAsLong {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsLong;
            }
        }

        /// <summary>
        /// Retrieves the contained object as <code>unsigned</code> number.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public IJIUnsigned ObjectAsUnsigned {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).ObjectAsUnsigned;
            }
        }

        /// <summary>
        /// Encode variant
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        internal void Encode(NdrCodec ndr, List<object> defferedPointers, int flag) {
            _member.Deffered = true;
            // this is since this could be part of an array or a struct...for normal calls
            // as soon as this call finishes a call will be given from JICallobject for it's variantbody.
            JIMarshalUnMarshalHelper.Serialize(ndr, _member.GetType(), _member, defferedPointers, flag);
        }

        /// <summary>
        /// Decode variant
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal static JIVariant Decode(NdrCodec ndr, List<object> defferedPointers,
            int flag, IDictionary<object, object> additionalData) {
            var variant = new JIVariant();
            var @ref = new JIPointer(typeof(JIVariantBody)) {
                Deffered = true //this is since this could be part of an array or a struct...for normal calls
            };
            //as soon as this call finishes a call will be given from JICallobject for it's variantbody.
            variant._member = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(
                ndr, @ref, defferedPointers, flag, additionalData);
            return variant;
        }

        /// <summary>
        /// Returns whether the variant is an array
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public bool IsArray {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).IsArray;
            }
        }

        /// <summary>
        /// Returns length in bytes
        /// </summary>
        /// <param name="flag"></param>
        /// <exception cref="JIException"> </exception>
        internal int GetLengthInBytes(int flag) {
            CheckValidity();
            return JIMarshalUnMarshalHelper.GetLengthInBytes(_member.GetType(), _member, flag);
        }

        /// <summary>
        /// Whether the ref flag is set
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public bool IsByRef {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).IsByRef;
            }
        }

        /// <summary>
        /// Returns the referent as integer. This can be used along with the
        /// <code>JIVariant.VT_<i>XX</i></code> flags to find out the type of the referent.
        /// For example :-
        /// <code>
        /// switch(variant.getType())
        /// {
        /// 	case JIVariant.VT_VARIANT: value = variant.getObjectAsVariant();
        ///  break;
        ///  case JIVariant.VT_NULL: ...
        ///  break;
        /// }
        /// </code>
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public int Type {
            get {
                CheckValidity();
                return ((JIVariantBody)_member.GetReferent()).Type;
            }
        }

        /// <summary>
        /// Helper to check validity
        /// </summary>
        private void CheckValidity() {
            if (_member == null || _member.IsNull) {
                throw new JIException(JIErrorCodes.JI_VARIANT_IS_NULL);
            }
        }

        /// <inheritdoc/>
        public override string ToString() => _member == null ? "[null]" : "[" + _member + "]";

        internal JIPointer _member;
        private static readonly List<object> kArryInits = new List<object>();
    }
}