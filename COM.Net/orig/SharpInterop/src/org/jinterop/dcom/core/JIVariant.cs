using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {



	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

	/// <summary>
	///<para>Class representing the <code>VARIANT</code> datatype.
	/// </para>
	/// <para>Please use the <code>byRef</code> flag based constructors for <i>by reference</i>
	/// parameters in COM calls. For <code>[optional]</code> parameters use the
	/// <seealso cref="#OPTIONAL_PARAM()"/>
	/// </para>
	/// <para>In case of direct calls to COM server using <code>JICallBuilder</code>, if the <code>byRef</code> flag is set then
	/// that variant should also be added as the <code>[out]</code> parameter in the <code>JICallBuilder</code>.
	/// For developers using the <code>IJIDispatch </code> this is not required and variant would be returned back to them
	/// via <code>JIVariant[]</code> associated with <code>IJIDispatch</code> apis.
	/// </para>
	/// <para>
	/// 
	/// An <b>important</b> note for <code>Boolean</code> Arrays (<code>JIArray</code> of <code>Boolean</code>),
	/// please set the <code>JIFlag.FLAG_REPRESENTATION_VARIANT_BOOL</code> using the <seealso cref="#setFlag(int)"/>
	/// method before making a call on this object. This is required since in DCOM ,  <code>VARIANT_BOOL</code> are 2 bytes
	/// and standard <code>boolean</code>s are 1 byte in length.
	/// </para>
	/// @since 1.0
	/// </summary>
	[Serializable]
	public sealed class JIVariant {

		private const long SerialVersionUID = 5101290038004040628L;
		private sealed class EMPTY {
		}
		private sealed class NULL {
		}
		private sealed class SCODE {
		}

		public const int VT_NULL = 0x00000001;
		public const int VT_EMPTY = 0x00000000;
		public const int VT_I4 = 0x00000003;
		public const int VT_UI1 = 0x00000011;
		public const int VT_I2 = 0x00000002;
		public const int VT_R4 = 0x00000004;
		public const int VT_R8 = 0x00000005;
		public const int VT_VARIANT = 0x0000000c;
		public const int VT_BOOL = 0x0000000b;
		public const int VT_ERROR = 0x0000000a;
		public const int VT_CY = 0x00000006;
		public const int VT_DATE = 0x00000007;
		public const int VT_BSTR = 0x00000008;
		public const int VT_UNKNOWN = 0x0000000d;
		public const int VT_DECIMAL = 0x0000000e;
		public const int VT_DISPATCH = 0x00000009;
		public const int VT_ARRAY = 0x00002000;
		public const int VT_BYREF = 0x00004000;
		public static readonly int VT_BYREF_VT_UI1 = VT_BYREF | VT_UI1; //0x00004011;
		public static readonly int VT_BYREF_VT_I2 = VT_BYREF | VT_I2; //0x00004002;
		public static readonly int VT_BYREF_VT_I4 = VT_BYREF | VT_I4; //0x00004003;
		public static readonly int VT_BYREF_VT_R4 = VT_BYREF | VT_R4; //0x00004004;
		public static readonly int VT_BYREF_VT_R8 = VT_BYREF | VT_R8; //0x00004005;
		public static readonly int VT_BYREF_VT_BOOL = VT_BYREF | VT_BOOL; //0x0000400b;
		public static readonly int VT_BYREF_VT_ERROR = VT_BYREF | VT_ERROR; //0x0000400a;
		public static readonly int VT_BYREF_VT_CY = VT_BYREF | VT_CY; //0x00004006;
		public static readonly int VT_BYREF_VT_DATE = VT_BYREF | VT_DATE; //0x00004007;
		public static readonly int VT_BYREF_VT_BSTR = VT_BYREF | VT_BSTR; //0x00004008;
		public static readonly int VT_BYREF_VT_UNKNOWN = VT_BYREF | VT_UNKNOWN; //0x0000400d;
		public static readonly int VT_BYREF_VT_DISPATCH = VT_BYREF | VT_DISPATCH; //0x00004009;
		public static readonly int VT_BYREF_VT_ARRAY = VT_BYREF | VT_ARRAY; //0x00006000;
		public static readonly int VT_BYREF_VT_VARIANT = VT_BYREF | VT_VARIANT; //0x0000400c;

		public const int VT_I1 = 0x00000010;
		public const int VT_UI2 = 0x00000012;
		public const int VT_UI4 = 0x00000013;
		public const int VT_I8 = 0x00000014;
		public const int VT_INT = 0x00000016;
		public const int VT_UINT = 0x00000017;
		public static readonly int VT_BYREF_VT_DECIMAL = VT_BYREF | VT_DECIMAL; //0x0000400e;
		public static readonly int VT_BYREF_VT_I1 = VT_BYREF | VT_I1; //0x00004010;
		public static readonly int VT_BYREF_VT_UI2 = VT_BYREF | VT_UI2; //0x00004012;
		public static readonly int VT_BYREF_VT_UI4 = VT_BYREF | VT_UI4; //0x00004013;
		public static readonly int VT_BYREF_VT_I8 = VT_BYREF | VT_I8; //0x00004014;
		public static readonly int VT_BYREF_VT_INT = VT_BYREF | VT_INT; //0x00004016;
		public static readonly int VT_BYREF_VT_UINT = VT_BYREF | VT_UINT; //0x00004017;

		public const int FADF_AUTO = 0x0001; // array is allocated on the stack
		public const int FADF_STATIC = 0x0002; // array is staticly allocated
		public const int FADF_EMBEDDED = 0x0004; // array is embedded in a structure
		public const int FADF_FIXEDSIZE = 0x0010; // may not be resized or reallocated
		public const int FADF_RECORD = 0x0020; // an array of records
		public const int FADF_HAVEIID = 0x0040; // with FADF_DISPATCH, FADF_UNKNOWN
												/* array has an IID for interfaces */
		public const int FADF_HAVEVARTYPE = 0x0080; // array has a VT type
		public const int FADF_BSTR = 0x0100; // an array of BSTRs
		public const int FADF_UNKNOWN = 0x0200; // an array of IUnknown*
		public const int FADF_DISPATCH = 0x0400; // an array of IDispatch*
		public const int FADF_VARIANT = 0x0800; // an array of VARIANTs
		public const int FADF_RESERVED = 0xF008; // reserved bits


		internal static Hashtable SupportedTypes = new Hashtable();
		internal static Hashtable SupportedTypes_classes = new Hashtable();
		internal static Hashtable OutTypesMap = new Hashtable();
		static JIVariant() {
			//CAUTION NO PTR TYPE SHOULD BE PART OF THIS MAP !!!
			OutTypesMap[typeof(int)] = new int?(0);
			OutTypesMap[typeof(int?)] = new int?(0);
			OutTypesMap[typeof(short)] = new short?((short)0);
			OutTypesMap[typeof(short?)] = new short?((short)0);
			OutTypesMap[typeof(float)] = new float?(0.0);
			OutTypesMap[typeof(float?)] = new float?(0.0);
			OutTypesMap[typeof(double)] = new double?(0.0);
			OutTypesMap[typeof(double?)] = new double?(0.0);
			OutTypesMap[typeof(bool)] = false;
			OutTypesMap[typeof(bool?)] = false;
			OutTypesMap[typeof(string)] = "";
			OutTypesMap[typeof(JICurrency)] = new JICurrency("0.0");
			OutTypesMap[typeof(DateTime?)] = DateTime.Now;
			OutTypesMap[typeof(char)] = new char?('9');
			OutTypesMap[typeof(char?)] = new char?('9');
			OutTypesMap[typeof(JIUnsignedByte)] = JIUnsignedFactory.GetUnsigned(new short?((short)0), JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE);
			OutTypesMap[typeof(JIUnsignedShort)] = JIUnsignedFactory.GetUnsigned(new int?(0), JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
			OutTypesMap[typeof(JIUnsignedInteger)] = JIUnsignedFactory.GetUnsigned(new long?(0), JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
			OutTypesMap[typeof(long)] = new long?(0);
			OutTypesMap[typeof(long?)] = new long?(0);

			SupportedTypes[typeof(object)] = new int?(VT_VARIANT);
			SupportedTypes[typeof(JIVariant)] = new int?(VT_VARIANT);
			SupportedTypes[typeof(int?)] = new int?(VT_I4);
			SupportedTypes[typeof(JIUnsignedInteger)] = new int?(VT_UI4);
			SupportedTypes[typeof(float?)] = new int?(VT_R4);
			SupportedTypes[typeof(bool?)] = new int?(VT_BOOL);
			SupportedTypes[typeof(double?)] = new int?(VT_R8);
			SupportedTypes[typeof(short?)] = new int?(VT_I2);
			SupportedTypes[typeof(JIUnsignedShort)] = new int?(VT_UI2);
			SupportedTypes[typeof(sbyte?)] = new int?(VT_I1);
			SupportedTypes[typeof(char?)] = new int?(VT_I1);
			SupportedTypes[typeof(JIUnsignedByte)] = new int?(VT_UI1);
			SupportedTypes[typeof(JIString)] = new int?(VT_BSTR);
	//		supportedTypes.put(IJIUnknown.class,new Integer(VT_UNKNOWN));
	//		supportedTypes.put(IJIDispatch.class,new Integer(VT_DISPATCH));
			SupportedTypes[typeof(JIVariant.SCODE)] = new int?(VT_ERROR);
			SupportedTypes[typeof(JIVariant.EMPTY_Renamed)] = new int?(VT_EMPTY);
			SupportedTypes[typeof(JIVariant.NULL_Renamed)] = new int?(VT_NULL);
			SupportedTypes[typeof(VariantBody.SCODE)] = new int?(VT_ERROR);
			SupportedTypes[typeof(VariantBody.EMPTY)] = new int?(VT_EMPTY);
			SupportedTypes[typeof(VariantBody.NULL)] = new int?(VT_NULL);
			SupportedTypes[typeof(JIArray)] = new int?(VT_ARRAY);
	//		supportedTypes.put(JIComObjectImpl.class,new Integer(VT_UNKNOWN));
	//		supportedTypes.put(JIDispatchImpl.class,new Integer(VT_DISPATCH));
			SupportedTypes[typeof(DateTime?)] = new int?(VT_DATE);
			SupportedTypes[typeof(JICurrency)] = new int?(VT_CY);
			SupportedTypes[typeof(long?)] = new int?(VT_I8);

			SupportedTypes_classes[new int?(VT_DATE)] = typeof(DateTime?);
			SupportedTypes_classes[new int?(VT_CY)] = typeof(JICurrency);
			SupportedTypes_classes[new int?(VT_VARIANT)] = typeof(JIVariant);
			SupportedTypes_classes[new int?(VT_I4)] = typeof(int?);
			SupportedTypes_classes[new int?(VT_INT)] = typeof(int?);
			SupportedTypes_classes[new int?(VT_UI4)] = typeof(JIUnsignedInteger);
			SupportedTypes_classes[new int?(VT_UINT)] = typeof(JIUnsignedInteger);
			SupportedTypes_classes[new int?(VT_R4)] = typeof(float?);
			SupportedTypes_classes[new int?(VT_BOOL)] = typeof(bool?);
			SupportedTypes_classes[new int?(VT_R8)] = typeof(double?);
			SupportedTypes_classes[new int?(VT_I2)] = typeof(short?);
			SupportedTypes_classes[new int?(VT_UI2)] = typeof(JIUnsignedShort);
			SupportedTypes_classes[new int?(VT_I1)] = typeof(char?);
			SupportedTypes_classes[new int?(VT_UI1)] = typeof(JIUnsignedByte);
			SupportedTypes_classes[new int?(VT_BSTR)] = typeof(JIString);
			SupportedTypes_classes[new int?(VT_ERROR)] = typeof(JIVariant.SCODE);
			SupportedTypes_classes[new int?(VT_EMPTY)] = typeof(EMPTY);
			SupportedTypes_classes[new int?(VT_NULL)] = typeof(NULL);
			SupportedTypes_classes[new int?(VT_ARRAY)] = typeof(JIArray);
			SupportedTypes_classes[new int?(VT_UNKNOWN)] = typeof(IJIComObject);
			SupportedTypes_classes[new int?(VT_DISPATCH)] = typeof(IJIComObject);
			SupportedTypes_classes[new int?(VT_I8)] = typeof(long?);

			//for by ref types, do it at runtime.
			ArryInits.Add(typeof(JIString));
			ArryInits.Add(typeof(JIPointer));
	//		arryInits.add(JIComObjectImpl.class);
	//		arryInits.add(JIDispatchImpl.class);
	//		arryInits.add(IJIUnknown.class);
			ArryInits.Add(typeof(IJIComObject));
			ArryInits.Add(typeof(IJIDispatch)); //this can only happen in case of an array
		}


		public static JIVariant OUTPARAMforType(Type c, bool isArray) {
			JIVariant variant = null;
			if (!isArray) {
				try {
					variant = MakeVariant(OutTypesMap.GetValueOrNull(c),true);
				}
				catch (Exception) {
					//eaten and now try from other types

				}

				if (c.Equals(typeof(IJIDispatch))) {
					return OUT_IDISPATCH();
				}
				else {
				if (c.Equals(typeof(IJIComObject))) {
					return OUT_IUNKNOWN();
				}
				else {
				if (c.Equals(typeof(JIVariant))) {
					return EMPTY_BYREF();
				}
				else {
				if (c.Equals(typeof(JIString))) {
					return new JIVariant("",true);
				}
				}
				}
				}
			}
			else {
				try {
					object oo = OutTypesMap.GetValueOrNull(c);
					if (oo != null) {
						//we will always send a single dimension array.
						object x = Array.CreateInstance(c, 1);
						((System.Array)x).SetValue(oo, 0);
						variant = new JIVariant(new JIArray(x,true),true);
					}
				}
				catch (Exception) {
					//eaten and now try from other types

				}

				if (c.Equals(typeof(IJIDispatch))) {
					IJIComObject[] arry = new IJIComObject[]{ new JIComObjectImpl(null, new JIInterfacePointer(null,-1,null)) };
					variant = new JIVariant(new JIArray(arry,true),true);
					variant.Flag = JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT | JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT;
				}
				else {
				if (c.Equals(typeof(IJIComObject))) {
					IJIComObject[] arry = new IJIComObject[]{ new JIComObjectImpl(null, new JIInterfacePointer(null,-1,null)) };
					variant = new JIVariant(new JIArray(arry,true),true);
					variant.Flag = JIFlags.FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT | JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT;
				}
				else {
				if (c.Equals(typeof(JIVariant))) {
					return VARIANTARRAY();
				}
				else {
				if (c.Equals(typeof(JIString)) || c.Equals(typeof(string))) {
					return BSTRARRAY();
				}
				}
				}
				}
			}


			return variant;
		}

		/// <summary>
		/// Returns a JIVariant (of the right type) based on the <code>o.getClass()</code>
		/// </summary>
		/// <param name="o">
		/// @return </param>
		public static JIVariant MakeVariant(object o) {
			return MakeVariant(o,false);
		}

		/// <summary>
		/// Returns a JIVariant (of the right type) based on the <code>o.getClass()</code>
		/// </summary>
		/// <param name="o"> </param>
		/// <param name="isByRef">
		/// @return </param>
		public static JIVariant MakeVariant(object o, bool isByRef) {
			if (o == null || o.GetType().Equals(typeof(object))) {
				if (isByRef) {
					return JIVariant.EMPTY_BYREF();
				}
				else {
					return JIVariant.EMPTY();
				}
			}

			Type c = o.GetType();
			if (c.IsArray) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(0x00001029));
			}

			if (c.Equals(typeof(JIVariant))) {
				return new JIVariant((JIVariant)o);
			}


			try {

				Constructor ctor = null;
				//now we look at the class and return a JIVariant.
				if (c.Equals(typeof(bool?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(bool),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(char?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(char),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(sbyte?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(sbyte),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(short?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(short),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(int?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(int),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(long?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(long),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(float?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(float),typeof(bool) });
				}
				else {
				if (c.Equals(typeof(double?))) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(double),typeof(bool) });
				}
				else {
				if (o is IJIComObject) {
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ typeof(IJIComObject),typeof(bool) });
				}
				else {
					//should cover all the rest cases.
					ctor = typeof(JIVariant).GetConstructor(new Type[]{ c,typeof(bool) });
				}
				}
				}
				}
				}
				}
				}
				}
				}
				return (JIVariant)ctor.newInstance(new object[]{ o, Convert.ToBoolean(isByRef) });
			}
			catch (Exception) {
				if (JISystem.Logger.isLoggable(Level.WARNING)) {
					JISystem.Logger.warning("Could not create Variant for " + o + " , isByRef " + isByRef);
				}
			}

			return null;
		}

		internal static Type GetSupportedClass(int? type) {
			return (Type)SupportedTypes_classes.GetValueOrNull(type);
		}

		internal static int? GetSupportedType(Type c, int FLAG) {
			int? retVal = (int?)SupportedTypes.GetValueOrNull(c);

			if (retVal == null && typeof(IJIComObject).Equals(c)) {
				retVal = new int?(VT_UNKNOWN);
			}

			if (retVal == null && typeof(IJIDispatch).Equals(c)) {
				retVal = new int?(VT_DISPATCH);
			}
			//means that if retval came back as VT_I4, we should make that VT_INT
			if ((int)retVal == VT_I4 && (FLAG & JIFlags.FLAG_REPRESENTATION_VT_INT) == JIFlags.FLAG_REPRESENTATION_VT_INT) {
				retVal = new int?(VT_INT);
			}
			else {
			if ((int)retVal == VT_UI4 && (FLAG & JIFlags.FLAG_REPRESENTATION_VT_UINT) == JIFlags.FLAG_REPRESENTATION_VT_UINT) {
				retVal = new int?(VT_UINT);
			}
			}

			return retVal;
		}

		internal static int? GetSupportedType(object o, int defaultType) {
			Type c = o.GetType();
			int? retval = (int?)SupportedTypes.GetValueOrNull(c);

			// Order is important since IJIDispatch derieves from IJIComObject
			if (retval == null && o is IJIDispatch) {
				retval = new int?(VT_DISPATCH);
			}

			if (retval == null && o is IJIComObject) {
				retval = new int?(VT_UNKNOWN);
			}

			return retval;
		}

		/// <summary>
		/// EMPTY <code>VARIANT</code>
		/// </summary>
		internal static readonly JIVariant EMPTY_Renamed = new JIVariant(new EMPTY());

		/// <summary>
		/// EMPTY <code>VARIANT</code>. This is not Thread Safe , hence a new instance must be taken each time.
		/// 
		/// </summary>
		public static JIVariant EMPTY() {
			return new JIVariant(new EMPTY());
		}

		/// <summary>
		/// EMPTY BYREF <code>VARIANT</code>
		/// </summary>
		internal static readonly JIVariant EMPTY_BYREF_Renamed = new JIVariant(EMPTY_Renamed);


		/// <summary>
		/// EMPTY BYREF <code>VARIANT</code>. This is not Thread Safe , hence a new instance must be taken each time. Used for a 
		/// <code>[out] VARIANT*</code> .
		/// 
		/// </summary>
		public static JIVariant EMPTY_BYREF() {
			return new JIVariant(EMPTY());
		}

		/// <summary>
		/// <code>VARIANT</code> for <code>([out] IUnknown*)</code>. This is not Thread Safe , hence a new instance must be taken each time.
		/// </summary>
		public static JIVariant OUT_IUNKNOWN() {
			JIVariant retval = new JIVariant(new JIComObjectImpl(null, new JIInterfacePointer(null,-1,null)),true);
			retval.Flag = JIFlags.FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT | JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT;
			return retval;
		}

		/// <summary>
		/// <code>VARIANT</code> for <code>([out] IDispatch*)</code>. This is not Thread Safe , hence a new instance must be taken each time.
		/// <br/>Note that this must also be used when the interface pointer is a subclass of <code>IDispatch</code> i.e. supports automation (or is a
		/// <code>dispinterface</code>).
		/// </summary>
		public static JIVariant OUT_IDISPATCH() {
			JIVariant retval = new JIVariant(new JIComObjectImpl(null, new JIInterfacePointer(null,-1,null)),true);
			retval.Flag = JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT | JIFlags.FLAG_REPRESENTATION_SET_JIINTERFACEPTR_NULL_FOR_VARIANT;
			return retval;
		}

		/// <summary>
		/// NULL <code>VARIANT</code>
		/// </summary>
		internal static readonly JIVariant NULL_Renamed = new JIVariant(new NULL());

		/// <summary>
		/// NULL <code>VARIANT</code> . This is not Thread Safe , hence a new instance must be taken each time.
		/// 
		/// </summary>
		public static JIVariant NULL() {
			return new JIVariant(new NULL());
		}

		/// <summary>
		/// OPTIONAL PARAM. Pass this when a parameter is optional for a COM api call.
		/// </summary>
		internal static readonly JIVariant OPTIONAL_PARAM_Renamed = new JIVariant(JIVariant.SCODE,JIErrorCodes.DISP_E_PARAMNOTFOUND);

		/// <summary>
		/// OPTIONAL PARAM. Pass this when a parameter is <code>[optional]</code> for a COM call.
		/// This is not Thread Safe , hence a new instance must be taken each time.
		/// 
		/// </summary>
		public static JIVariant OPTIONAL_PARAM() {
			return new JIVariant(JIVariant.SCODE,JIErrorCodes.DISP_E_PARAMNOTFOUND);
		}


		/// <summary>
		/// SCODE <code>VARIANT</code>
		/// </summary>
		public static readonly SCODE SCODE = new SCODE();

		/// <summary>
		/// Helper method for creating an array of <code>BSTR</code>s , IDL signature <code>[in, out] SAFEARRAY(BSTR) *p</code>.
		/// The return value can directly be used in an <code>IJIDispatch</code>call.
		/// 
		/// @return
		/// </summary>
		public static JIVariant BSTRARRAY() {
			return new JIVariant(new JIArray(new JIString[]{ new JIString("") }, true),true);
		}

		/// <summary>
		/// Helper method for creating an array of <code>VARIANT</code>s , IDL signature <code>[in, out] SAFEARRAY(VARIANT) *p</code>
		/// OR <code>[in,out] VARIANT *pArray</code>. The return value can directly be used in an <code>IJIDispatch</code> call.
		/// 
		/// @return
		/// </summary>
		public static JIVariant VARIANTARRAY() {
			return new JIVariant(new JIArray(new JIVariant[]{ JIVariant.EMPTY() }, true),true);
		}


		internal JIPointer Member = null;

		private JIVariant() {
		}

		//The class of the object determines its type.
	//	/**
	//	 * Setting up a <code>VARIANT</code> with an object. Used via serializing the <code>VARIANT</code>.
	//	 *
	//	 * @param obj
	//	 */
		private void Init(object obj) {
			Init(obj,false);
		}

	//	/** For internal use only !. Please do not call this directly from outside. It will lead to unexpected results.
	//	 *
	//	 * @exclude
	//	 * @param obj
	//	 * @param isByRef
	//	 */
	//	public JIVariant(Object obj, boolean isByRef)
	//	{
	//		init(obj,isByRef);
	//	}

		private void Init(object obj, bool isByRef) {
			if (obj != null && obj.GetType().IsArray) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_VARIANT_ONLY_JIARRAY_EXCEPTED));
			}

			if (obj != null && obj.GetType().Equals(typeof(JIInterfacePointer))) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_VARIANT_TYPE_INCORRECT));
			}

			//this case comes only for SCODE and EMPTY, and in these cases the isByRef flag will be set in the
			//previous call itself.
			if (obj is VariantBody) {
				Member = new JIPointer(obj);
			}
			else {
				VariantBody variantBody = new VariantBody(obj,isByRef);
				Member = new JIPointer(variantBody);
	//			if (obj != null && obj instanceof JIVariant)
	//			{
	//				VariantBody var = (VariantBody)((JIVariant)obj).member.getReferent();
	//				try {
	//					variantBody.variantType = var.getVariantType() + 3 + 1;
	//				} catch (JIException e) {
	//					throw new JIRuntimeException(e.getErrorCode());
	//				}
	//			}
			}

			Member.SetReferent(0x72657355); //"User" in LEndian.

		}

		/// <summary>
		///Called when this variant is nested
		/// </summary>
		/// <param name="deffered"> </param>
		public bool Deffered {
			set {
				if (Member != null && !Member.Reference) {
					Member.Deffered = value;
				}
			}
		}

		/// <summary>
		/// Sets a <code>JIFlags</code> value to be used while encoding (marshalling) this Variant.
		/// </summary>
		/// <param name="FLAG"> </param>
		public int Flag {
			set {
				VariantBody variantBody = ((VariantBody)Member.GetReferent());
				variantBody.FLAG |= value;
			}
			get {
				VariantBody variantBody = ((VariantBody)Member.GetReferent());
				return variantBody.FLAG;
			}
		}



		/// <summary>
		///Returns whether this variant is a <code>NULL</code> variant.
		/// </summary>
		/// <returns> <code>true</code> if the variant is a <code>NULL</code> </returns>
		public bool Null {
			get {
				if (Member == null) {
					return true;
				}
				VariantBody variantBody = ((VariantBody)Member.GetReferent());
				return variantBody == null ? true: variantBody.Null;
			}
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> as reference to another. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="variant"> </param>
		public JIVariant(JIVariant variant) {
			Init((object)variant,true);
		}



		/// <summary>
		///Setting up a <code>VARIANT</code> with an <code>int</code>. Used via serializing the <code>VARIANT</code>.
		/// Used when the variant type is VT_I4.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. LONG* </param>
		public JIVariant(int value, bool isByRef) {
			Init(new int?(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>long</code>. Used via serializing the <code>VARIANT</code>.
		/// Used when the variant type is VT_I8.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. </param>
		public JIVariant(long value, bool isByRef) {
			Init(new long?(value),isByRef);
		}



		/// <summary>
		/// Setting up a <code>VARIANT</code> with a <code>float</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. FLOAT* </param>
		public JIVariant(float value, bool isByRef) {
			Init(new float?(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>boolean</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. VARIANT_BOOL* </param>
		public JIVariant(bool value, bool isByRef) {
			Init(Convert.ToBoolean(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>double</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. DOUBLE* </param>
		public JIVariant(double value, bool isByRef) {
			Init(new double?(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>short</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. SHORT* </param>
		public JIVariant(short value, bool isByRef) {
			Init(new short?(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>char</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. CHAR* </param>
		public JIVariant(char value, bool isByRef) {
			Init(new char?(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>JIString</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. BSTR* </param>
		public JIVariant(JIString value, bool isByRef) {
			Init(value,isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>String</code>. Used via serializing the <code>VARIANT</code>. Internally a
		/// <code>JIString</code> is formed with it's default type <code>BSTR</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. BSTR* </param>
		public JIVariant(string value, bool isByRef) {
			Init(new JIString(value),isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>String</code>. Used via serializing the <code>VARIANT</code>. Internally a
		/// <code>JIString</code> is formed with it's default type <code>BSTR</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(string value) : this(new JIString(value)) {
		}

	//	/**Setting up a <code>VARIANT</code> with a IJIDispatch. Used via serializing the <code>VARIANT</code>.
	//	 *
	//	 * @param value
	//	 * @param isByRef true if the value is to be represented as a pointer. IJIDispatch**
	//	 */
	//	public JIVariant(IJIDispatch value, boolean isByRef)
	//	{
	//		this((Object)value,isByRef);
	//	}

		/// <summary>
		///Setting up a <code>VARIANT</code> with an <code>IJIComObject</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. IJIComObject** </param>
		public JIVariant(IJIComObject value, bool isByRef) {
			Init((object)value,isByRef);
			if (value is IJIDispatch) {
				Flag = JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
			}
			else {
				Flag = JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
			}
		}



		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>SCODE</code> value and it's <code>errorCode</code>. Used via serializing the <code>VARIANT</code>.
		/// 
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="errorCode"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. SCODE* </param>
		public JIVariant(SCODE value, int errorCode, bool isByRef) {
			Init(new VariantBody(VariantBody.SCODE,errorCode,isByRef),isByRef);
		}


		/// <summary>
		///Setting up a <code>VARIANT</code> with an <code>int</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(int value) : this(value,false) {
		}

		/// <summary>
		/// Setting up a <code>VARIANT</code> with a <code>float</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(float value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a  <code>boolean</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(bool value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>double</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(double value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>short</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(short value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>char</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(char value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>JIString</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(JIString value) : this(value,false) {
		}

	//	/**Setting up a <code>VARIANT</code> with a IJIDispatch. Used via serializing the <code>VARIANT</code>.
	//	 *
	//	 * @param value
	//	 */
	//	public JIVariant(IJIDispatch value)
	//	{
	//		this((Object)value);
	//	}

		/// <summary>
		///Setting up a <code>VARIANT</code> with an <code>IJIComObject</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(IJIComObject value) : this(value,false) {
			if (value is IJIDispatch) {
				Flag = JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
			}
			else {
				Flag = JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
			}
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with an <code>java.util.Date</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(DateTime? value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with an <code>java.util.Date</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. Date* </param>
		public JIVariant(DateTime? value, bool isByRef) {
			Init((object)value,isByRef);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>JICurrency</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public JIVariant(JICurrency value) : this(value,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>JICurrency</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. JICurrency* </param>
		public JIVariant(JICurrency value, bool isByRef) {
			Init((object)value,isByRef);
		}


		/// <summary>
		/// Setting up a <code>VARIANT</code> with an <code>EMPTY</code> value. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		private JIVariant(EMPTY value) {
			Init((object)null);
		}


		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>NULL</code> value. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		private JIVariant(NULL value) {
			Init(new VariantBody(VariantBody.NULL));
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>SCODE</code> value and it's <code>errorCode</code>. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="errorCode"> </param>
		public JIVariant(SCODE value, int errorCode) {
			Init(new VariantBody(VariantBody.SCODE,errorCode,false));
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>JIArray</code>. Used via serializing the <code>VARIANT</code>.
		/// Only 1 and 2 dimensional array is supported.
		/// </summary>
		/// <param name="array"> </param>
		/// <param name="FLAG"> JIFlag value </param>
		public JIVariant(JIArray array, int FLAG) : this(array,false,FLAG) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>JIArray</code>. Used via serializing the <code>VARIANT</code>.
		/// Only 1 and 2 dimensional array is supported.
		/// </summary>
		/// <param name="array"> </param>
		/// <param name="isByRef"> </param>
		/// <param name="FLAG"> JIFlag value </param>
		public JIVariant(JIArray array, bool isByRef, int FLAG) {
			InitArrays(array, isByRef, FLAG);
		}
		/// <summary>
		/// Setting up a <code>VARIANT</code> with a <code>JIArray</code>. Used via serializing the <code>VARIANT</code>.
		/// Only 1 and 2 dimensional array is supported.
		/// </summary>
		/// <param name="array"> </param>
		/// <param name="isByRef"> </param>
		public JIVariant(JIArray array, bool isByRef) {
			InitArrays(array, isByRef, JIFlags.FLAG_NULL);
		}

		private static readonly IList ArryInits = new List<object>();
		private void InitArrays(JIArray array, bool isByRef, int FLAG) {
			VariantBody variant2 = null;
			JIArray array2 = null;
			Type c = null;
			object[] newArrayObj = null;
			bool is2Dim = false;

			if (array == null) {
				Init(null,false);
				return;
			}

			switch (array.Dimensions) {
				case 1:
					object[] obj = (object [])array.ArrayInstance;
					newArrayObj = obj;
					c = obj.GetType().GetElementType();
					break;
				case 2:
					/*The 2 dimensional array is serialized like this first the index [0,0]  and then [1,0] then [0,1] then [1,1], then [0,2] then [1,2]
					 and so on . so what i will do here is that create a single dimension flat array of the members in the order specified above, after examining this Object[][] and let the
					 1 dimension serializing logic take over.*/
					object[][] obj2 = (object [][])array.ArrayInstance;
					//variants = new JIVariant[array.getNumElementsInAllDimensions()];

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					string name = obj2.GetType().FullName;
					object subArray = obj2;
					name = name.Substring(1);
					int firstDim = ((object[])subArray).Length;
					subArray = Array.get(subArray,0);
					int secondDim = ((object[])subArray).Length;
					int k = 0;
					newArrayObj = (object[])Array.CreateInstance(subArray.GetType().GetElementType(), array.NumElementsInAllDimensions);
					for (int i = 0; i < secondDim;i++) {
						for (int j = 0;j < firstDim;j++) {
							newArrayObj[k++] = obj2[j][i];
						}
					}


					c = subArray.GetType().GetElementType();
					is2Dim = true;
					break;
				default:
					throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_VARIANT_VARARRAYS_2DIMRES));
			}

			array2 = new JIArray(newArrayObj,true); //should always be conformant since this is part of a safe array.

			JIStruct safeArray = new JIStruct();
			try {
				safeArray.AddMember(new short?((short)array.Dimensions)); //dim
				int elementSize = -1;
				short flags = JIVariant.FADF_HAVEVARTYPE;
				if (c.Equals(typeof(JIVariant))) {
					flags = (short)(flags | JIVariant.FADF_VARIANT);
					elementSize = 16; //(Variant is pointer whose size is 16)
				}
				else {
				if (ArryInits.Contains(c)) {
					if (c.Equals(typeof(JIString))) {
						flags = (short)(flags | JIVariant.FADF_BSTR);
					}
					else {
					if (c.Equals(typeof(IJIComObject))) {
						flags = (short)(flags | JIVariant.FADF_UNKNOWN);
						FLAG |= JIFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID;
					}
					else {
					if (c.Equals(typeof(IJIDispatch))) {
						flags = (short)(flags | JIVariant.FADF_DISPATCH);
						FLAG |= JIFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID;
					}
					}
					}
					elementSize = 4; //Since all these are pointers inherently
				}
				else {
					//JStruct and JIUnions are expected to be encapsulated within pointers...they usually are :)
					elementSize = JIMarshalUnMarshalHelper.GetLengthInBytes(c, null, c == typeof(bool?) ? JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL : JIFlags.FLAG_NULL); //All other types, basic types
				}
				}


				JIStruct safeArrayBound = null;

				int[] upperBounds = array.UpperBounds;
				JIStruct[] arrayOfSafeArrayBounds = new JIStruct[array.Dimensions];
				for (int i = 0; i < array.Dimensions;i++) {
					safeArrayBound = new JIStruct();
					safeArrayBound.AddMember(new int?(upperBounds[i]));
					safeArrayBound.AddMember(new int?(0)); //starts at 0
					arrayOfSafeArrayBounds[i] = safeArrayBound;
				}

				JIArray arrayOfSafeArrayBounds2 = new JIArray(arrayOfSafeArrayBounds,true);

				safeArray.AddMember(new short?(flags)); //flags
				if (elementSize > 0) {
					safeArray.AddMember(new int?(elementSize));
				}
				else {
					elementSize = JIMarshalUnMarshalHelper.GetLengthInBytes(c, null, FLAG);
					safeArray.AddMember(new int?(elementSize)); //size
				}

				safeArray.AddMember(new short?((short)0)); //locks
				safeArray.AddMember(new short?((short)JIVariant.GetSupportedType(c, FLAG))); //variant array, safearrayunion
				//peculiarity here, windows seems to be sending the signed type in VarType32...
				if (c.Equals(typeof(JIUnsignedByte))) {
					safeArray.AddMember(JIVariant.GetSupportedType(typeof(sbyte?),FLAG)); //safearrayunion
				}
				else if (c.Equals(typeof(JIUnsignedShort))) {
					safeArray.AddMember(JIVariant.GetSupportedType(typeof(short?),FLAG)); //safearrayunion
				}
				else if (c.Equals(typeof(JIUnsignedInteger))) {
					safeArray.AddMember(JIVariant.GetSupportedType(typeof(int?),FLAG)); //safearrayunion
				}
				else if (c.Equals(typeof(bool?))) {
					safeArray.AddMember(JIVariant.GetSupportedType(typeof(short?),FLAG)); //safearrayunion
				}
				else if (c.Equals(typeof(double?))) {
					safeArray.AddMember(JIVariant.GetSupportedType(typeof(long?),FLAG)); //safearrayunion
				}
				else if (c.Equals(typeof(float?))) {
					safeArray.AddMember(JIVariant.GetSupportedType(typeof(int?),FLAG)); //safearrayunion
				}
				else {
					safeArray.AddMember(JIVariant.GetSupportedType(c,FLAG)); //safearrayunion
				}
				safeArray.AddMember(new int?(array2.NumElementsInAllDimensions)); //size in safearrayunion
				JIPointer ptr2RealArray = new JIPointer(array2);
				safeArray.AddMember(ptr2RealArray);
				safeArray.AddMember(arrayOfSafeArrayBounds2);
			}
			catch (JIException e) {
				throw new JIRuntimeException(e.ErrorCode);
			}

			variant2 = new VariantBody(safeArray,c,is2Dim,isByRef,FLAG);
			Init(variant2,false);

		}

		/// <summary>
		/// Setting up a <code>VARIANT</code> with a <code>JIArray</code>. Used via serializing the <code>VARIANT</code>. <br>
		/// Only 1 and 2 dimensional array is supported.
		/// </summary>
		/// <param name="array"> </param>
		public JIVariant(JIArray array) : this(array,false) {
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>unsigned</code> value. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="number"> </param>
		public JIVariant(IJIUnsigned number) {
			Init((object)number);
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a <code>unsigned</code> value. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="number"> </param>
		/// <param name="isByRef"> <code>true</code> if the value is to be represented as a pointer. </param>
		public JIVariant(IJIUnsigned number, bool isByRef) {
			Init((object)number,isByRef);
		}

		/// <summary>
		/// Returns the contained object.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getObject() throws org.jinterop.dcom.common.JIException
		public object Object {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).Object;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>int</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getObjectAsInt() throws org.jinterop.dcom.common.JIException
		public int ObjectAsInt {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsInt;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>float</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public float getObjectAsFloat() throws org.jinterop.dcom.common.JIException
		public float ObjectAsFloat {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsFloat;
			}
		}

		/// <summary>
		///Retrieves the contained objects errorCode.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getObjectAsSCODE() throws org.jinterop.dcom.common.JIException
		public int ObjectAsSCODE {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsSCODE;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>double</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getObjectAsDouble() throws org.jinterop.dcom.common.JIException
		public double ObjectAsDouble {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsDouble;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>short</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public short getObjectAsShort() throws org.jinterop.dcom.common.JIException
		public short ObjectAsShort {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsShort;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>boolean</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean getObjectAsBoolean() throws org.jinterop.dcom.common.JIException
		public bool ObjectAsBoolean {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsBoolean;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>JIString</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIString getObjectAsString() throws org.jinterop.dcom.common.JIException
		public JIString ObjectAsString {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsString;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>String</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getObjectAsString2() throws org.jinterop.dcom.common.JIException
		public string ObjectAsString2 {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsString.String;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>java.util.Date</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.Date getObjectAsDate() throws org.jinterop.dcom.common.JIException
		public DateTime? ObjectAsDate {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsDate;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>char</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public char getObjectAsChar() throws org.jinterop.dcom.common.JIException
		public char ObjectAsChar {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsChar;
			}
		}


	//	/**Retrieves the contained object as JIInterfacePointer.
	//	 *
	//	 * @return
	//	 * @throws JIException
	//	 * @deprecated Please use getObjectAsComObject instead.
	//	 */
	//	public JIInterfacePointer getObjectAsInterfacePointer() throws JIException
	//	{
	//		checkValidity();
	//		return  ((VariantBody)member.getReferent()).getObjectAsInterfacePointer();
	//	}

	//	/**Retrieves the contained object as IJIComObject. Return value can be safely typecasted to the expected type. for e.g. :- If expected type is an IJIDispatch ,
	//	 * then the return value can be safely type casted to it.
	//	 *
	//	 * @param template <code>IJIComObject</code> whose basic parameters such as <code>JIComServer</code> will be used while creating the new Instance.
	//	 * @return
	//	 * @throws JIException
	//	 * @deprecated
	//	 */
	//	public IJIComObject getObjectAsComObject(IJIComObject template) throws JIException
	//	{
	//		checkValidity();
	//		return JIObjectFactory.createCOMInstance(template,((VariantBody)member.getReferent()).getObjectAsInterfacePointer());
	//	}

		/// <summary>
		///Retrieves the contained object as <code>IJIComObject</code>. Return value must be "narrowed" to get the expected type.
		/// <para>for example :- If expected type is an <code>IJIDispatch</code>,
		/// then the return value must pass through <code>JIObjectFactory.narrowInstance(IJIComObject)</code> to get to the right type.
		/// 
		/// @return
		/// </para>
		/// </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIComObject getObjectAsComObject() throws org.jinterop.dcom.common.JIException
		public IJIComObject ObjectAsComObject {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsComObject;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>JIVariant</code>.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIVariant getObjectAsVariant() throws org.jinterop.dcom.common.JIException
		public JIVariant ObjectAsVariant {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsVariant;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>JIArray</code>. Only 1 and 2 dim arrays are supported currently.
		/// Please note that this array is <b>not</b> backed by this variant and is a <b>new</b> copy. If the array
		/// is <code>IJIComObject</code>s, please make sure to use <code>JIObjectFactory.narrowObject()</code> to
		/// get the right instance.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIArray getObjectAsArray() throws org.jinterop.dcom.common.JIException
		public JIArray ObjectAsArray {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).Array;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>long</code>, used when the expected type is VT_I8.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long getObjectAsLong() throws org.jinterop.dcom.common.JIException
		public long ObjectAsLong {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsLong;
			}
		}

		/// <summary>
		///Retrieves the contained object as <code>unsigned</code> number.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIUnsigned getObjectAsUnsigned() throws org.jinterop.dcom.common.JIException
		public IJIUnsigned ObjectAsUnsigned {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ObjectAsUnsigned;
			}
		}

		public void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG) {
			Member.Deffered = true; //this is since this could be part of an array or a struct...for normal calls
			//as soon as this call finishes a call will be given from JICallobject for it's variantbody.
			JIMarshalUnMarshalHelper.Serialize(ndr,Member.GetType(),Member,defferedPointers,FLAG);
		}


		internal static JIVariant Decode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
			JIVariant variant = new JIVariant();
			JIPointer @ref = new JIPointer(typeof(VariantBody));
			@ref.Deffered = true; //this is since this could be part of an array or a struct...for normal calls
			//as soon as this call finishes a call will be given from JICallobject for it's variantbody.
			variant.Member = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,@ref,defferedPointers,FLAG,additionalData);
			return variant;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isArray() throws org.jinterop.dcom.common.JIException
		public bool Array {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).Array;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int getLengthInBytes(int FLAG) throws org.jinterop.dcom.common.JIException
		public int GetLengthInBytes(int FLAG) {
			CheckValidity();
			return JIMarshalUnMarshalHelper.GetLengthInBytes(Member.GetType(),Member,FLAG);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isByRefFlagSet() throws org.jinterop.dcom.common.JIException
		public bool ByRefFlagSet {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).ByRef;
			}
		}

		/// <summary>
		/// Returns the referent as integer. This can be used along with the
		/// <code>JIVariant.VT_<i>XX</i></code> flags to find out the type of the referent.
		/// <P>
		/// For example :-
		/// <para>
		/// <code>
		/// switch(variant.getType())<br>
		/// {<br>
		/// 	case JIVariant.VT_VARIANT: value = variant.getObjectAsVariant();<br>
		///  break; <br>
		///  case JIVariant.VT_NULL: ... <br>
		///  break; <br>
		/// }<br>
		/// </code>
		/// 
		/// @return
		/// </para>
		/// </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getType() throws org.jinterop.dcom.common.JIException
		public int Type {
			get {
				CheckValidity();
				return ((VariantBody)Member.GetReferent()).Type;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void checkValidity() throws org.jinterop.dcom.common.JIException
		private void CheckValidity() {
			if (Member == null || Member.Null) {
				throw new JIException(JIErrorCodes.JI_VARIANT_IS_NULL);
			}
		}

		public override string ToString() {
			return Member == null ? "[null]" : "[" + Member.ToString() + "]";
		}

	}

	[Serializable]
	internal class VariantBody {

		private const long SerialVersionUID = -8484108480626831102L;
		public const short VT_PTR = 0x1A;
		public const short VT_SAFEARRAY = 0x1B;
		public const short VT_CARRAY = 0x1C;
		public const short VT_USERDEFINED = 0x1D;


		internal sealed class EMPTY {
		}
		internal sealed class NULL {
		}
		internal sealed class SCODE {
			internal int ErrorCode;
			public SCODE() {
			}
										   public SCODE(int errorCode) {
											   this.ErrorCode = errorCode;
										   }
		}

		/// <summary>
		/// EMPTY <code>VARIANT</code>
		/// </summary>
		public static readonly EMPTY EMPTY = new EMPTY();

		/// <summary>
		/// NULL <code>VARIANT</code>
		/// </summary>
		public static readonly NULL NULL = new NULL();

		/// <summary>
		/// SCODE <code>VARIANT</code>
		/// </summary>
		public static readonly SCODE SCODE = new SCODE();

		private bool Is2Dimensional = false;
		private object Obj = null;
		private int Type_Renamed = -1;
		//private JIArray objArray = null;
		private JIStruct SafeArrayStruct = null;
		private bool IsArray = false;
		private bool IsScode = false;
		private bool IsNull = false;
		private Type NestedArraysRealClass = null;
		private static List<object> Type3 = new List<object>();
		private bool IsByRef = false;

		internal int FLAG = JIFlags.FLAG_NULL;
	//	int variantType = 0x1d; //base jump

		static VariantBody() {
			Type3.Add(typeof(int?));
			Type3.Add(typeof(short?));
			Type3.Add(typeof(float?));
			Type3.Add(typeof(bool?));
			Type3.Add(typeof(char?));
			Type3.Add(typeof(sbyte?));
			Type3.Add(typeof(EMPTY));
			Type3.Add(typeof(NULL));
			Type3.Add(typeof(SCODE));
			Type3.Add(typeof(JIUnsignedByte));
			Type3.Add(typeof(JIUnsignedShort));
			Type3.Add(typeof(JIUnsignedInteger));
		}

		public virtual bool ByRef {
			get {
				return IsByRef;
			}
		}

		public virtual bool Null {
			get {
				return IsNull;
			}
		}

		public virtual int Type {
			get {
				return IsArray ? JIVariant.VT_ARRAY | Type_Renamed : Type_Renamed;
			}
		}
		//The class of the object determines its type.
		/// <summary>
		/// Setting up a <code>VARIANT</code> with an object. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="referent"> </param>
		public VariantBody(object referent, bool isByRef) : this(referent,isByRef,-1) {
		}


		private VariantBody(object referent, bool isByRef, int dataType) {
			this.Obj = referent == null ? VariantBody.EMPTY : referent;

			if (Obj is JIString && ((JIString)Obj).Type != JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
				throw new JIRuntimeException(JIErrorCodes.JI_VARIANT_BSTR_ONLY);
			}

			if (Obj is bool?) {
				FLAG = JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
			}


			this.IsByRef = isByRef;
	//		variantType = getMaxLength(this.obj.getClass(),isByRef,obj);

			//for an unsupported type this could be null
			//but then this is my bug, any thread entering this ctor , will support a type.
			int? types = ((int?)JIVariant.GetSupportedType(Obj,dataType));
			if (types != null) {
				Type_Renamed = (int)types | (isByRef ? JIVariant.VT_BYREF:0);
			}
			else {
				throw new JIRuntimeException(JIErrorCodes.JI_VARIANT_UNSUPPORTED_TYPE);
			}

	//		if (JISystem.getLogger().isLoggable(Level.INFO))
	//		{
	//			JISystem.getLogger().info("In VariantBody(Object,boolean,int) : dataType is " + dataType + " , referent class is " + this.obj.getClass() + " , byRef is " + isByRef);
	//		}
			if (dataType == JIVariant.VT_NULL) {
				IsNull = true;
				Obj = new int?(0);
			}
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a NULL value. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		public VariantBody(NULL value) : this(new int?(0),false) {
			IsNull = true;
			Type_Renamed = JIVariant.VT_NULL;
		}

		/// <summary>
		///Setting up a <code>VARIANT</code> with a SCODE value and it's errorCode. Used via serializing the <code>VARIANT</code>.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="errorCode"> </param>
		 public VariantBody(SCODE value, int errorCode, bool isByRef) : this(new int?(errorCode),isByRef) {
			IsScode = true;
			Type_Renamed = JIVariant.VT_ERROR;
		 }



		public VariantBody(JIStruct safeArray, Type nestedClass, bool is2Dimensional, bool isByRef, int FLAG) {
			this.FLAG = FLAG;
			//can't convert the array here , since this will have deffered pointers which may not be complete.
			SafeArrayStruct = safeArray;
			IsArray = true;
			if (SafeArrayStruct == null) {
				IsNull = true;
			}

			this.NestedArraysRealClass = nestedClass;
			this.Is2Dimensional = is2Dimensional;
			//please remember JIVariant is a pointer and VariantBody is just the referent part of that.


			//for an unsupported type this could be null
			//but then this is my bug, any thread entering this ctor , will support a type.
			this.IsByRef = isByRef;
			int? types = ((int?)JIVariant.GetSupportedType(nestedClass,FLAG));
			if (types != null) {
				Type_Renamed = (int)types | (isByRef ? JIVariant.VT_BYREF:0);
			}
			else {
				throw new JIRuntimeException(JIErrorCodes.JI_VARIANT_UNSUPPORTED_TYPE);
			}
		}

	//	VariantBody(JIArray obj, Class nestedClass, boolean is2Dimensional,boolean isByRef)
	//	{
	//
	//		this.objArray = obj;
	//		isArray = true;
	//		this.nestedArraysRealClass = nestedClass;
	//		this.is2Dimensional = is2Dimensional;
	//		//please remember JIVariant is a pointer and VariantBody is just the referent part of that.
	//
	//
	//		//for an unsupported type this could be null
	//		//but then this is my bug, any thread entering this ctor , will support a type.
	//		this.isByRef = isByRef;
	//		Integer types = ((Integer)JIVariant.supportedTypes.get(obj.getClass()));
	//		if (types != null)
	//		{
	//			type = types.intValue() | (isByRef ? JIVariant.VT_BYREF:0);
	//		}
	//
	//	}

		/// <summary>
		/// Returns the contained object.
		/// 
		/// @return
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object getObject() throws org.jinterop.dcom.common.JIException
		public virtual object Object {
			get {
				return Obj == null ? Array : Obj;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JIArray getArray() throws org.jinterop.dcom.common.JIException
		public virtual JIArray Array {
			get {
				JIArray retVal = null;
				//TODO convert it to the right type based on the variantType before returning it.
				//everything is sent encapsulated in a variant(in safearray) , so an Integer[] will
				//go as a variant array for each integer, only the variantType = arry of ints. so convert the
				//array in the right format before returning it to the user. That is he must get Int[] within a JIArray
				//back.
				if (SafeArrayStruct != null) {
					retVal = (JIArray)((JIPointer)SafeArrayStruct.GetMember(7)).GetReferent();
    
					if (Is2Dimensional) {
						object[] obj3 = (object[])retVal.ArrayInstance; //these will all be variants
						//correct the array here , i.e reform the 2 dimensional array before returning back.
						JIArray safeArrayBound = (JIArray)SafeArrayStruct.GetMember(8);
    
						JIStruct[] safeArrayBound2 = (JIStruct[]) safeArrayBound.ArrayInstance;
						//should only be 2 since we support only 2 dim.
    
						int firstDim = (int)((int?)safeArrayBound2[0].GetMember(0));
						int secondDim = (int)((int?)safeArrayBound2[1].GetMember(0));
    
						object obj = Array.CreateInstance(NestedArraysRealClass,new int[]{ firstDim,secondDim });
						object[][] obj2 = (object[][])obj;
						int k = 0;
						for (int i = 0; i < secondDim;i++) {
							for (int j = 0;j < firstDim;j++) {
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
    
						if (NestedArraysRealClass != null) {
							object[] obj = (object[])retVal.ArrayInstance; //these will all be variants
							object obj2 = Array.CreateInstance(NestedArraysRealClass,obj.Length);
							for (int i = 0;i < obj.Length;i++) {
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
		///Retrieves the contained object as int.
		/// 
		/// @return
		/// </summary>
		public virtual int ObjectAsInt {
			get {
				try {
					return (int)((int?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		public virtual long ObjectAsLong {
			get {
				try {
					return (long)((long?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		public virtual IJIUnsigned ObjectAsUnsigned {
			get {
				try {
					return ((IJIUnsigned)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		public virtual int ObjectAsSCODE {
			get {
				try {
					return ((SCODE)Obj).ErrorCode;
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as float.
		/// 
		/// @return
		/// </summary>
		public virtual float ObjectAsFloat {
			get {
				try {
					return (float)((float?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as double.
		/// 
		/// @return
		/// </summary>
		public virtual double ObjectAsDouble {
			get {
				try {
					return (double)((double?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as short.
		/// 
		/// @return
		/// </summary>
		public virtual short ObjectAsShort {
			get {
				try {
					return (short)((short?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as boolean.
		/// 
		/// @return
		/// </summary>
		public virtual bool ObjectAsBoolean {
			get {
				try {
					return (bool)((bool?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as JIString.
		/// 
		/// @return
		/// </summary>
		public virtual JIString ObjectAsString {
			get {
				try {
					return ((JIString)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as Date.
		/// 
		/// @return
		/// </summary>
		public virtual DateTime? ObjectAsDate {
			get {
				try {
					return ((DateTime?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as char.
		/// 
		/// @return
		/// </summary>
		public virtual char ObjectAsChar {
			get {
				try {
					return (char)((char?)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		/// <summary>
		///Retrieves the contained object as Variant.
		/// 
		/// @return
		/// </summary>
		public virtual JIVariant ObjectAsVariant {
			get {
				try {
					return ((JIVariant)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}

		public virtual IJIComObject ObjectAsComObject {
			get {
				try {
					return ((IJIComObject)Obj);
				}
				catch (System.InvalidCastException e) {
					throw new System.InvalidOperationException(e.Message);
				}
			}
		}


		public virtual void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG) {

		{
	//		try
				FLAG |= this.FLAG;
				//align with 8 boundary
				double index = (double)(new int?(ndr.Buffer.Index));
				if (index % 8.0 != 0) {
					long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
					ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
				}

				int start = ndr.Buffer.Index;

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
				ndr.writeUnsignedLong(0xFFFFFFFF);

				ndr.writeUnsignedLong(0);


				//Type
				int varType = GetVarType(Obj != null ? Obj.GetType() : NestedArraysRealClass, Obj);

				//For IUnknown , since the inner object is a JIComObjectImpl it will be fine.
				if ((FLAG & JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT) == JIFlags.FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT) {
					varType = IsByRef ? 0x4000 | JIVariant.VT_DISPATCH : JIVariant.VT_DISPATCH;
				}
				ndr.writeUnsignedShort(varType);

				//reserved bytes
				ndr.writeUnsignedSmall(0xCC);
				ndr.writeUnsignedSmall(0xCC);
				ndr.writeUnsignedSmall(0xCC);
				ndr.writeUnsignedSmall(0xCC);
				ndr.writeUnsignedSmall(0xCC);
				ndr.writeUnsignedSmall(0xCC);

				if (Obj != null) {
					ndr.writeUnsignedLong(varType);
				}
				else {
					if (!IsByRef) {
						ndr.writeUnsignedLong(JIVariant.VT_ARRAY);
					}
					else {
						ndr.writeUnsignedLong(JIVariant.VT_BYREF_VT_ARRAY);
					}
				}


				if (IsByRef) {
					int flag = -1;
					if (IsArray) { //object arrays will come here....
						flag = 4;
					}
					else {
						//no idea what these flags are but 0x10 is for variant, 0x8 for date, and 0x4 is for others
						switch (Type_Renamed) {
							case JIVariant.VT_BYREF_VT_VARIANT:
								flag = 0x10;
							break;
							case JIVariant.VT_BYREF_VT_DATE:
							case JIVariant.VT_BYREF_VT_CY:
								flag = 8;
								break;
							default:
								flag = 4;
							break;
						}
					}
					ndr.writeUnsignedLong(flag);

				}

				//we should not use the deffered pointers here, but pass our own one, so that only they are written...
				IList varDefferedPointers = new List<object>();

				//we should use FLAG here, since the decision should be based on this only.
				SetValue(ndr,Obj,varDefferedPointers,FLAG);

				//making changes to write the deffered pointers here itself , since we need to put the entire Variant completed to the length
				//as in varType.
				int x = 0;
				while (x < varDefferedPointers.Count) {
					List<object> newList = new List<object>();
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIPointer),(JIPointer)varDefferedPointers[x],newList,FLAG);
					x++; //incrementing index
					varDefferedPointers.AddRange(x,newList);
				}

				int currentIndex = 0;
				int length = (currentIndex = ndr.Buffer.Index) - start;
				int value = (int) length / 8;
				if (length % 8.0 != 0) { //entire variant is aligned by 8 bytes.
					value++;
				}
				ndr.Buffer.Index = start;
				ndr.writeUnsignedLong(value);
				ndr.Buffer.Index = currentIndex;

				if (JISystem.Logger.isLoggable(Level.FINEST)) {
					JISystem.Logger.finest("Variant length is " + length + " , value " + value + " , variant type" + Type_Renamed);
				}
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
	//	private int getMaxLength(Class c, boolean isByRef, Object obj)
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
		private int GetMaxLength2(Type c, object obj) {
			int length = 0;

			//since this is getMaxLength2 and hence will either contain
			//proper type 3 elements and not EMPTY,NULL,SCODE since these are parts of Variant.
			//and not simple types like Integer, JIUnsignedXXX or Float etc.
			if (Type3.Contains(c)) {
				length = JIMarshalUnMarshalHelper.GetLengthInBytes(c, obj, FLAG);
			}
			else {
			if (c.Equals(typeof(long?)) || c.Equals(typeof(double?)) || c.Equals(typeof(DateTime?)) || c.Equals(typeof(JICurrency))) {
				length = 8;
			}
			else {
			if (c.Equals(typeof(JIString))) {
				length = JIMarshalUnMarshalHelper.GetLengthInBytes(c, obj, FLAG);
			}
			else { // for Interface pointers without
			if (obj is IJIComObject) {
				double value = ((IJIComObject)obj).Internal_getInterfacePointer().Length;
				value = value + 4 + 4 + 4; //20 of variant , 4 of the ptr, 4 of max count, 4 of actual count
			}
			}
			}
			}

			return length;

		}

	//	int getVariantType() throws JIException
	//	{
	//		return safeArrayStruct == null ? variantType : getArrayLengthForVarType();
	//	}

	//	private int fillArrayType(NetworkDataRepresentation ndr) throws JIException
	//	{
	//		int length = getArrayLengthForVarType();
	//		ndr.writeUnsignedLong(length);
	//		return length;
	//	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int getArrayLengthForVarType() throws org.jinterop.dcom.common.JIException
		private int ArrayLengthForVarType {
			get {
				//now the array will be of variants, nestedArraysRealClass identifies the class itself
				//for iteration we need the variants and then there members.
    
				JIArray objArray = (JIArray)((JIPointer)SafeArrayStruct.GetMember(7)).GetReferent();
				object[] array = (object[])objArray.ArrayInstance;
    
				double length = 20; //variant
				if (IsByRef) {
					length = length + 4; //byref
				}
    
				//SafeArray is 44
				length += 44;
    
    
				bool isVariantArray = ((short)((short?)SafeArrayStruct.GetMember(1)) & JIVariant.FADF_VARIANT) == JIVariant.FADF_VARIANT ? true : false;
    
				if (array != null) {
					length += 4; //for max count of the array.
					if (isVariantArray) {
						//each variant is 3 (size 20 = 20/8 = 3)
						for (int i = 0;i < array.Length;i++) {
							JIVariant variant = (JIVariant)array[i];
							length += variant.GetLengthInBytes(FLAG); //* 8;//((VariantBody)(variant.member.getReferent())).variantType * 8;
						}
    
						//now for the "user" pointer part
						//length = length + array.length * 4;
					}
					else {
						//normal non variant array has been sent...
						for (int i = 0;i < array.Length;i++) {
							length += GetMaxLength2(array[i].GetType(),array[i]);
						}
					}
				}
				else {
					length += 4; //for the null 0000.
				}
    
				int value = (int) length / 8;
				if (length % 8.0 != 0) {
					value++;
				}
    
				return value;
			}
		}

		internal static VariantBody Decode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
			//boolean readLong = false;
			double index = (double)(new int?(ndr.Buffer.Index));
			if (index % 8.0 != 0) {
				long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
			}

			int start = ndr.Buffer.Index;
			int length = ndr.readUnsignedLong(); //read the potential length
			ndr.readUnsignedLong(); //read the reserved byte

			int variantType = ndr.readUnsignedShort(); //varType

			//read reserved bytes
			ndr.readUnsignedShort();
			ndr.readUnsignedShort();
			ndr.readUnsignedShort();

			ndr.readUnsignedLong(); //32 bit varType

			VariantBody variant = null;

			IList varDefferedPointers = new List<object>();
			if ((variantType & JIVariant.VT_ARRAY) == 0x2000) {
				bool isByRef = (variantType & JIVariant.VT_BYREF) == 0 ? false : true;
				//the struct may be null if the array has nothing
				JIStruct safeArray = GetDecodedValueAsArray(ndr,varDefferedPointers,variantType & ~JIVariant.VT_ARRAY,isByRef,additionalData,FLAG);
				int type2 = variantType;
				if (isByRef) {
					type2 = type2 & ~JIVariant.VT_BYREF; //so that actual type can be determined
				}

				type2 = type2 & 0x0FFF;
				int flagofFlags = FLAG;
				if (type2 == JIVariant.VT_INT) {
					flagofFlags |= JIFlags.FLAG_REPRESENTATION_VT_INT;
				}
				else {
				if (type2 == JIVariant.VT_UINT) {
					flagofFlags |= JIFlags.FLAG_REPRESENTATION_VT_UINT;
				}
				else {
				if (type2 == JIVariant.VT_BOOL) {
					FLAG = flagofFlags |= JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
				}
				}
				}

				if (safeArray != null) {
					variant = new VariantBody(safeArray, (Type)JIVariant.GetSupportedClass(new int?(type2 & ~JIVariant.VT_ARRAY)),((object[])((JIArray)safeArray.GetMember(8)).ArrayInstance).Length > 1 ? true : false,isByRef,flagofFlags);
				}
				else {
					variant = new VariantBody(null, (Type)JIVariant.GetSupportedClass(new int?(type2 & ~JIVariant.VT_ARRAY)),false,isByRef,flagofFlags);
				}

				variant.FLAG = flagofFlags;

			}
			else {
				bool isByRef = (variantType & JIVariant.VT_BYREF) == 0 ? false : true;
				variant = new VariantBody(GetDecodedValue(ndr,varDefferedPointers,variantType,isByRef,additionalData,FLAG),isByRef,variantType);
				int type2 = variantType & 0x0FFF;
				if (type2 == JIVariant.VT_INT) {
					variant.FLAG = JIFlags.FLAG_REPRESENTATION_VT_INT;
				}
				else {
				if (type2 == JIVariant.VT_UINT) {
					variant.FLAG = JIFlags.FLAG_REPRESENTATION_VT_UINT;
				}
				}
			}


			int x = 0;
			while (x < varDefferedPointers.Count) {

				List<object> newList = new List<object>();
				JIPointer replacement = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,(JIPointer)varDefferedPointers[x],newList,FLAG,additionalData);
				((JIPointer)varDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
				x++;
				varDefferedPointers.AddRange(x,newList);
			}

			if (variant.IsArray && variant.SafeArrayStruct != null) {
				//SafeArray have the alignment rule , that all Size <=4 are aligned by 4 and size 8 is aligned by 8.
				//Variant is aligned by 4, Interface pointers are aligned by 4 as well.
				//but this should not exceed the length
				index = (double)(new int?(ndr.Buffer.Index));
				length = length * 8 + start;
				if (index < length) {
					JIStruct safeArrayStruct = variant.SafeArrayStruct;
					int? size = (int?)safeArrayStruct.GetMember(2);
					long i = 0;
					if ((int)size == 8) {
						if (index % 8.0 != 0) {
							i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
							if (index + i <= length) {
								ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
							}
							else {
								ndr.readOctetArray(new sbyte[(length - (int)index)],0,(int)(length - (int)index));
							}
						}
					}
					else {
						//align by 4...
						//TODO this needs to be tested for Structs and Unions.
						if (index % 4.0 != 0) {
							i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
							if (index + i <= length) {
								ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
							}
							else {
								ndr.readOctetArray(new sbyte[(length - (int)index)],0,(int)(length - (int)index));
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
				JIVariant variantMain = new JIVariant(array,variant.IsByRef,variant.FLAG);
				variant = (VariantBody)variantMain.Member.GetReferent();
			}

			return variant;
		}


		//Variants need specialised handling and the standard serializers may or maynot be used.
		private static Type GetVarClass(int type) {
			Type c = null;
			//now first to check if this is a pointer or not.
			type = type & 0x0FFF; //0x4XXX & 0x0FFF = real type
			switch (type) {
				case 0: //VT_EMPTY , Not specified.
					c = typeof(VariantBody.EMPTY);
					break;
				case 1: // VT_NULL , Null.
					c = typeof(VariantBody.NULL);
					break;
				case 10:
					c = typeof(VariantBody.SCODE); //VT_ERROR,Scodes.
					break;
				default:
					c = (Type)JIVariant.GetSupportedClass(new int?(type));
					if (c == null) {
						//TODO log this , what has come that i don't support.
					}
					break;
			}

			return c;
		}


		private int GetVarType(Type c, object obj) {
			int type = 0; //EMPTY

			if (obj is IJIDispatch) {
				return IsByRef ? 0x4000 | JIVariant.VT_DISPATCH : JIVariant.VT_DISPATCH;
			}

			if (obj is IJIComObject) {
				return IsByRef ? 0x4000 | JIVariant.VT_UNKNOWN : JIVariant.VT_UNKNOWN;
			}

			if (c != null) {
				int? type2 = (int?)JIVariant.GetSupportedType(c,FLAG);

				if (type2 != null) {
					type = (int)type2;
				}
				else {
					JISystem.Logger.warning("In getVarType: Unsupported Type found ! " + c + " , please add this to the supportedType map ! ");
					//make that an array of variants
					type2 = (int?)JIVariant.GetSupportedType(typeof(JIVariant),FLAG);
				}

				if (IsNull) {
					type = 1;
				}
				else if (IsScode) {
					type = 10; //scode
				}
				else if (IsArray) {
					type = (int) 0x2000 | type; //0xC; should not assume an array of variants anymore
				}
			}

			if (IsByRef && type != 0 && !c.Equals(typeof(JIArray))) {
				//then it is a pointer. have to set it correctly
				type = type | 0x4000;
			}
			return type;
		}

		private static object GetDecodedValue(NetworkDataRepresentation ndr, IList defferedPointers, int type, bool isByRef, IDictionary additionalData, int FLAG) {

			object obj = null;
			Type c = GetVarClass(type);
			if (c != null) {
				if (isByRef) {
					ndr.readUnsignedLong(); //Read the Pointer
				}

				if (c.Equals(typeof(VariantBody.SCODE))) {
					obj = JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,FLAG,additionalData);
					obj = new SCODE((int)((int?)obj));
					type = JIVariant.VT_ERROR;
				}
				else {
				if (c.Equals(typeof(VariantBody.NULL))) {
					//have read 20 bytes
					//JIMarshalUnMarshalHelper.deSerialize(ndr,Integer.class,null,JIFlags.FLAG_NULL);//read the last 4 bytes, since there could be parameters before this.
					obj = NULL;
					type = JIVariant.VT_NULL;
				}
				else {
				if (c.Equals(typeof(VariantBody.EMPTY))) { //empty is 20 bytes
					obj = VariantBody.EMPTY;
					type = JIVariant.VT_EMPTY;
				}
				else {
				if (c.Equals(typeof(JIString))) {
					obj = new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
					obj = ((JIString)obj).Decode(ndr,null,FLAG,additionalData);
				}
				else {
					if (c.Equals(typeof(bool?))) {
						obj = JIMarshalUnMarshalHelper.DeSerialize(ndr,c,defferedPointers,FLAG | JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL,additionalData);
					}
				else {
						obj = JIMarshalUnMarshalHelper.DeSerialize(ndr,c,defferedPointers,FLAG,additionalData);
				}
				}
				}
				}
				}
			}

			return obj;
		}

		private static JIStruct GetDecodedValueAsArray(NetworkDataRepresentation ndr, IList defferedPointers, int type, bool isByRef, IDictionary additionalData, int FLAG) {
			//int newFLAG = FLAG;
			if (isByRef) {
				ndr.readUnsignedLong(); //read the pointer
				type = type & ~JIVariant.VT_BYREF; //so that actual type can be determined
			}

			if (ndr.readUnsignedLong() == 0) { //read pointer referent id
				return null;
			}

			ndr.readUnsignedLong(); //1

			JIStruct safeArray = new JIStruct();
			try {
				safeArray.AddMember(typeof(short?)); //dim

				JIStruct safeArrayBound = new JIStruct();
				safeArrayBound.AddMember(typeof(int?));
				safeArrayBound.AddMember(typeof(int?)); //starts at 0

				safeArray.AddMember(typeof(short?)); //flags
				safeArray.AddMember(typeof(int?)); //size
				safeArray.AddMember(typeof(short?)); //locks
				safeArray.AddMember(typeof(short?)); //locks
				safeArray.AddMember(typeof(int?)); //safearrayunion
				safeArray.AddMember(typeof(int?)); //size in safearrayunion

				Type c = (Type)JIVariant.SupportedTypes_classes.GetValueOrNull(new int?(type));
				if (c == null) {
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.warning("From JIVariant: while decoding an Array, type " + type + " , was not found in supportedTypes_classes map , hence using JIVariant instead...");
					}
					//not available , lets try with JIVariant.
					//This is a bug, I should have the type.
					c = typeof(JIVariant);
				}

				if (c == typeof(bool?)) {
					FLAG |= JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL;
				}
				//HARDCODING to JIVariant...kindof forgotten why I even wrote the code below.
				//since all of the examples I have come across always return a Variant array.
				//then why did I typify this thing to it's class (like JIString), it produces an
				//exception when the result is returned back is not an array of strings...
				//c = JIVariant.class;
				JIArray values = null;
				if (c == typeof(JIString)) {
					values = new JIArray(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),null,1,true);
					safeArray.AddMember(new JIPointer(values)); //single dimension array, will convert it into the
					//[] or [][] after inspecting dimension read.
				}
				else {
					values = new JIArray(c,null,1,true);
					safeArray.AddMember(new JIPointer(values)); //single dimension array, will convert it into the
																							//[] or [][] after inspecting dimension read.
				}

				safeArray.AddMember(new JIArray(safeArrayBound,null,1,true));

				safeArray = (JIStruct)JIMarshalUnMarshalHelper.DeSerialize(ndr,safeArray,defferedPointers,FLAG,additionalData);

				//now set the right class after examining the flags , only set for JIVariant.class now., the BSTR would already be set previously.
				short? features = (short?)safeArray.GetMember(1);
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


		private void SetValue(NetworkDataRepresentation ndr, object obj, IList defferedPointers, int FLAG) {
			if (IsNull) {
				return; //null , is only 20 bytes
			}
			if (obj != null) {
				Type c = obj.GetType();

					if (c.Equals(typeof(EMPTY))) { //20 bytes
						return;
					}
	//				else
	//				if (c.equals(Boolean.class))
	//				{
	//					ndr.writeUnsignedShort(((Boolean)obj).booleanValue() == true ? 0xFFFF: 0x0000);
	//					ndr.writeUnsignedShort(0);
	//				}
					else {
						if (obj is IJIComObject) {
							c = typeof(IJIComObject);
						}
						JIMarshalUnMarshalHelper.Serialize(ndr,c,obj,defferedPointers,FLAG);
					}
			}
			else {

				ndr.writeUnsignedLong((new object()).GetHashCode()); //pointer referentId
				ndr.writeUnsignedLong(1);

				JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIStruct),SafeArrayStruct,defferedPointers,FLAG);




			}
		}



		public virtual bool Array {
			get {
				return IsArray;
			}
		}

		public virtual int LengthInBytes {
			get {
				if (SafeArrayStruct == null && Obj.GetType().Equals(typeof(VariantBody.EMPTY))) {
					return 28;
				}
    
				if (IsArray) {
					int length = 0;
		//			JIArray objArray = (JIArray)((JIPointer)safeArrayStruct.getMember(7)).getReferent();
		//			Object[] array = (Object[])objArray.getArrayInstance();
		//			for (int i = 0; i < array.length; i++)
		//			{
		//				Class c = array[i].getClass();
		//				length = length + JIMarshalUnMarshalHelper.getLengthInBytes(c,array[i],FLAG);
		//			}
		//			return length;
					try {
						length = ArrayLengthForVarType * 8;
					}
					catch (JIException e) {
						throw new Exception(e);
					}
    
					return length;
				}
				else {
					Type c = Obj.GetType();
    
					if (Obj is IJIComObject) {
						c = typeof(IJIComObject);
					}
					else {
					if (c.Equals(typeof(VariantBody.SCODE))) {
						return 24 + 4; //4 for integer scode.
					}
					else {
					if (c.Equals(typeof(VariantBody.NULL)) || c.Equals(typeof(VariantBody.EMPTY))) {
						return 24;
					}
					}
					}
    
					return 24 + JIMarshalUnMarshalHelper.GetLengthInBytes(c,Obj,FLAG);
				}
			}
		}

		public override string ToString() {
			string retVal = "";
			if (Obj == null) {
				retVal += "obj is null , ";
			}
			else {
				retVal += Obj.ToString();
			}
			if (IsArray) {
				if (Is2Dimensional) {
					retVal += "2 dimensional array , ";
				}
				else {
					retVal = "1 dimensional array , ";
				}

				if (SafeArrayStruct != null) {
					retVal += SafeArrayStruct.ToString();
				}
			}

			return retVal;
		}

	}

}