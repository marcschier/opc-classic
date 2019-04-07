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

namespace org.jinterop.dcom.impls.automation {



	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
	using JIComObjectImplWrapper = org.jinterop.dcom.core.JIComObjectImplWrapper;
	using JIFlags = org.jinterop.dcom.core.JIFlags;
	using JIFrameworkHelper = org.jinterop.dcom.core.JIFrameworkHelper;
	using JIPointer = org.jinterop.dcom.core.JIPointer;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIStruct = org.jinterop.dcom.core.JIStruct;
	using JIVariant = org.jinterop.dcom.core.JIVariant;

	using UUID = rpc.core.UUID;
	/// <summary>
	///@exclude
	/// 
	/// @since 1.0
	/// 
	/// </summary>
	[Serializable]
	internal sealed class JIDispatchImpl : JIComObjectImplWrapper, IJIDispatch {

		/// 
		private const long SerialVersionUID = 4908149252176353846L;

		//IJIComObject comObject = null;
		private IDictionary CacheOfDispIds = new Hashtable();
		public JIDispatchImpl(IJIComObject comObject) : base(comObject) {
			//this.comObject = comObject;
		}

		private readonly JIExcepInfo LastExcepInfo_Renamed = new JIExcepInfo();

		public const int FLAG_TYPEINFO_SUPPORTED = 1;
		public const int FLAG_TYPEINFO_NOTSUPPORTED = 0;

		public IJIComObject COMObject {
			get {
				return ComObject;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoCount() throws org.jinterop.dcom.common.JIException
		public int TypeInfoCount {
			get {
				JICallBuilder obj = new JICallBuilder(true);
				obj.Opnum = 0;
				obj.AddInParamAsInt(0, JIFlags.FLAG_NULL);
				obj.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
				object[] result = ComObject.Call(obj);
				return (int)((int?)result[0]);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getIDsOfNames(String apiName) throws org.jinterop.dcom.common.JIException
		public int GetIDsOfNames(string apiName) {
			if (apiName == null || apiName.Trim().Equals("")) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
			}

			IDictionary innerMap = ((IDictionary)CacheOfDispIds.GetValueOrNull(apiName));
			if (innerMap != null) {
				int? dispId = (int?)innerMap.GetValueOrNull(apiName);
				return (int)dispId;
			}



			JICallBuilder obj = new JICallBuilder(true);
			obj.Opnum = 2; //size of the array                                                                    //1st is the num elements and second is the actual values

			JIString name = new JIString(apiName.Trim(),JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
			JIArray array = new JIArray(new JIPointer[]{ new JIPointer(name) },true);
			obj.AddInParamAsUUID(UUID.NIL_UUID,JIFlags.FLAG_NULL);
			obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(1,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x800,JIFlags.FLAG_NULL);
			obj.AddOutParamAsObject(new JIArray(typeof(int?),null,1,true),JIFlags.FLAG_NULL);


			object[] result = ComObject.Call(obj);

			if (result == null && obj.Error) {
				throw new JIException(obj.HRESULT);
			}

			innerMap = new Hashtable();
			innerMap[apiName] = ((object[])((JIArray)result[0]).ArrayInstance)[0];
			CacheOfDispIds[apiName] = innerMap;


			//first will be the length , and the next will be the actual value.
			return (int)((int?)((object[])((JIArray)result[0]).ArrayInstance)[0]); //will get the dispatch ID.
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int[] getIDsOfNames(String[] apiName) throws org.jinterop.dcom.common.JIException
		public int[] GetIDsOfNames(string[] apiName) {
			if (apiName == null || apiName.Length == 0) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
			}

			bool sendForAll = false;
			//first one will be the method name
			IDictionary innerMap = ((IDictionary)CacheOfDispIds.GetValueOrNull(apiName[0]));
			if (innerMap != null) { //if name is not found will not even go in. so it is safe to assume that api name will always be there.
				int[] values = new int[innerMap.Count];
				for (int i = 0; i < apiName.Length; i++) {
					int? dispId = (int?)innerMap.GetValueOrNull(apiName[i]);
					if (dispId == null) {
						sendForAll = true;
						break;
					}
					else {
						values[i] = (int)dispId;
					}
				}

				if (!sendForAll) {
					return values; //all found returning now
				}
			}


			JICallBuilder obj = new JICallBuilder(true);
			obj.Opnum = 2; //size of the array                                                                    //1st is the num elements and second is the actual values

			JIPointer[] pointers = new JIPointer[apiName.Length];

			for (int i = 0;i < apiName.Length;i++) {
				if (apiName[i] == null || apiName[i].Trim().Equals("")) {
					throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
				}
				pointers[i] = new JIPointer(new JIString(apiName[i].Trim(),JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
			}


			JIArray array = new JIArray(pointers,true);
			JIArray arrayOut = new JIArray(typeof(int?),null,1,true);
			obj.AddInParamAsUUID(UUID.NIL_UUID,JIFlags.FLAG_NULL);
			obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(apiName.Length,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x800,JIFlags.FLAG_NULL);

			obj.AddOutParamAsObject(arrayOut,JIFlags.FLAG_NULL);

			object[] result = ComObject.Call(obj);

			if (obj.HRESULT != 0) { //exception occured
				throw new JIException(obj.HRESULT,JISystem.GetLocalizedMessage(obj.HRESULT));
			}


			JIArray arrayOfResults = (JIArray)result[0];
			int?[] arrayOfDispIds = (int?[])arrayOfResults.ArrayInstance;
			int[] retVal = new int[apiName.Length];

			innerMap = innerMap == null ? new Hashtable() : innerMap;
			for (int i = 0;i < apiName.Length;i++) {
				retVal[i] = (int)arrayOfDispIds[i];
				innerMap[apiName[i]] = arrayOfDispIds[i];
			}

			if (!CacheOfDispIds.Contains(apiName[0])) {
				CacheOfDispIds[apiName[0]] = innerMap;
			}
			return retVal;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfo(int typeInfo) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo GetTypeInfo(int typeInfo) {
			JICallBuilder obj = new JICallBuilder(true);
			obj.Opnum = 1;
			obj.AddInParamAsInt(typeInfo,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x400,JIFlags.FLAG_NULL);
			obj.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			//obj.setUpParams(new Object[]{new Integer(typeInfo),new Integer(0x400)},new Object[]{MInterfacePointer.class},JIFlags.FLAG_NULL,JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(obj);
			return (IJITypeInfo)JIObjectFactory.NarrowObject((IJIComObject)result[0]);
		}

	//	//First inparams[0] will always be variant and the inparams[1] is expected to be an JIArray
	//	public JIVariant invoke(int dispId,int dispatchFlags,Object[] inparams) throws JIException
	//	{
	//		return invoke(dispId,dispatchFlags,inparams,null);
	//	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] invoke(int dispId,int dispatchFlags,org.jinterop.dcom.core.JIArray arrayOfVariantsInParams,org.jinterop.dcom.core.JIArray arrayOfNamedDispIds,org.jinterop.dcom.core.JIVariant outParamType) throws org.jinterop.dcom.common.JIException
		public JIVariant[] Invoke(int dispId, int dispatchFlags, JIArray arrayOfVariantsInParams, JIArray arrayOfNamedDispIds, JIVariant outParamType) {
			LastExcepInfo_Renamed.ClearAll();
			JICallBuilder obj = new JICallBuilder(true);
			obj.Opnum = 3;


			JIStruct dispParams = new JIStruct();

			//now check whether any of the variants is representation of a variant ptr, if so replace it with an
			//EMPTY variant and add it to another array.
			List<object> listOfVariantPtrs = new List<object>();
			List<object> listOfPositions = new List<object>();
			JIVariant[] variants = null;
			int lengthVar = 0;
			//boolean isLastAptr = false;
			if (arrayOfVariantsInParams != null) {
				lengthVar = JIFrameworkHelper.ReverseArrayForDispatch(arrayOfVariantsInParams);
				variants = (JIVariant[])arrayOfVariantsInParams.ArrayInstance;
				for (int i = 0;i < variants.Length;i++) {
					JIVariant variant = variants[i];
					if (variant.ByRefFlagSet) {
						listOfVariantPtrs.Add(variant);
						listOfPositions.Add(new int?(i)); //for position array
						//now replace with Empty.
						//variants[i] = new JIVariant(JIVariant.POINTER);
						variants[i] = JIVariant.EMPTY();
					}
				}
			}


			int lengthPtr = 0;
			if (arrayOfNamedDispIds != null) {
				lengthPtr = JIFrameworkHelper.ReverseArrayForDispatch(arrayOfNamedDispIds);
			}

			dispParams.AddMember(new JIPointer(arrayOfVariantsInParams)); //should be an array of variants
			dispParams.AddMember(new JIPointer(arrayOfNamedDispIds)); //if there, this should be an array of variants , these too.
			dispParams.AddMember(new int?(lengthVar));
			dispParams.AddMember(new int?(lengthPtr));


			obj.AddInParamAsInt(dispId,JIFlags.FLAG_NULL);
			obj.AddInParamAsUUID(UUID.NIL_UUID,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x800,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(dispatchFlags ^ 0xFFFFFFF0,JIFlags.FLAG_NULL);
			obj.AddInParamAsStruct(dispParams,JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

			//now add the extra params if exist.
			if (listOfVariantPtrs.Count > 0) {
				//write length
				obj.AddInParamAsInt(listOfPositions.Count,JIFlags.FLAG_NULL);
				//then write the array
				obj.AddInParamAsArray(new JIArray(listOfPositions.ToArray(typeof(int?)),true),JIFlags.FLAG_NULL);
				//now write the array of variant ptrs
				obj.AddInParamAsArray(new JIArray(listOfVariantPtrs.ToArray(typeof(JIVariant)),true),JIFlags.FLAG_NULL);
			}

			obj.AddInParamAsObject(null,JIFlags.FLAG_NULL); //results --> currently all are null and this param is not required as the outparam carries this info.
			obj.AddInParamAsObject(null,JIFlags.FLAG_NULL); //excepinfo --> currently all are null and this param is not required as the excepinfo is built here.
			obj.AddInParamAsObject(null,JIFlags.FLAG_NULL); //augerr --> currently all are null and this param is not required as the excepinfo is built here.

			object[] outparams = new object[4];
			if (outParamType == null) {
				outparams[0] = typeof(JIVariant); //fill ourselves
			}
			else {
				outparams[0] = outParamType; //fill from users input
			}

			outparams[1] = ExcepInfo;
			outparams[2] = new JIPointer(typeof(int?),true);
			outparams[3] = new JIArray(typeof(JIVariant),null,1,true);



			obj.SetOutParams(outparams,JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

			object[] result = null;
			try {
				result = ComObject.Call(obj);
			}
			catch (JIException e) {
				object[] results = obj.ResultsInCaseOfException;
				if (results != null) {
					//catching here so that an extended message could be sent out
					JIStruct excepInfoRet = ((JIStruct)results[1]);
					string text1 = ((JIString)(excepInfoRet.GetMember(2))).String + " ";
					string text2 = ((JIString)(excepInfoRet.GetMember(3))).String + " [ ";
					string text3 = ((JIString)(excepInfoRet.GetMember(4))).String + " ] ";
					LastExcepInfo_Renamed.ExcepDesc_Renamed = text2;
					LastExcepInfo_Renamed.ExcepHelpfile = text3;
					LastExcepInfo_Renamed.ExcepSource_Renamed = text1;
					LastExcepInfo_Renamed.ErrorCode_Renamed = (int)((short?)excepInfoRet.GetMember(0)) != 0 ? (int)((short?)excepInfoRet.GetMember(0)) : (int)((int?)excepInfoRet.GetMember(8));


					JIAutomationException automationException = new JIAutomationException(e);
					automationException.ExcepInfo = LastExcepInfo_Renamed;
					throw automationException;
	//				throw new JIException(obj.getHRESULT(),JISystem.getLocalizedMessage(obj.getHRESULT()) + " ==> Message from Server: " +
	//				text1 + text2 + text3);
				}
				else {
					throw e;
				}
			}

			JIArray array = (JIArray)result[3];
			JIVariant[] byrefVariants = (JIVariant[])array.ArrayInstance; //will be a sinlge dimensional array.

			JIVariant[] retVal = new JIVariant[1 + byrefVariants.Length];
			retVal[0] = (JIVariant)result[0];
			Array.Copy(byrefVariants,0,retVal,1,byrefVariants.Length);

			return retVal;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void put(int dispId, Object[] inparams, boolean isRef) throws org.jinterop.dcom.common.JIException
		private void Put(int dispId, object[] inparams, bool isRef) {
			int propertyFlag = isRef ? IJIDispatch_Fields.DISPATCH_PROPERTYPUTREF : IJIDispatch_Fields.DISPATCH_PROPERTYPUT;
			object[] objectParams = inparams;
			if (objectParams == null) {
				objectParams = new object[0];
			}

			JIVariant[] variants = new JIVariant[objectParams.Length];
			for (int i = 0;i < objectParams.Length; i++) {
				JIVariant variant = null;
				object obj = objectParams[i];
				if (!(obj is JIVariant)) {
					if (obj is JIArray) {
						variant = new JIVariant((JIArray)obj,isRef);
					}
					else {
						variant = JIVariant.MakeVariant(obj,isRef);
					}

				}
				else {
					variant = (JIVariant)obj;
					 //variant = new JIVariant((JIVariant)obj);
				}

				variants[i] = variant;
			}


			Invoke(dispId,propertyFlag,new JIArray(variants,true),new JIArray(new int?[]{ new int?(IJIDispatch_Fields.DISPATCH_DISPID_PUTPUTREF) },true),null);
			//invoke(dispId,propertyFlag,new JIArray(new JIVariant[]{inparam},true),new JIArray(new Integer[]{new Integer(propertyFlag)},true),null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(int dispId, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void Put(int dispId, JIVariant inparam) {
			Put(dispId,new object[]{ inparam },false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(String name, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void Put(string name, JIVariant inparam) {
			Put(GetIDsOfNames(name),inparam);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(int dispId, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void PutRef(int dispId, JIVariant inparam) {
			Put(dispId,new object[]{ inparam },true);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(String name, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void PutRef(string name, JIVariant inparam) {
			PutRef(GetIDsOfNames(name),inparam);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant get(int dispId) throws org.jinterop.dcom.common.JIException
		public JIVariant Get(int dispId) {
			//return invoke(dispId,IJIDispatch.DISPATCH_PROPERTYGET,new Object[]{null,null,null,null,null},null);
			return ((JIVariant[])Invoke(dispId,IJIDispatch_Fields.DISPATCH_PROPERTYGET,null,null,null))[0];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] get(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] Get(int dispId, object[] inparams) {
			return CallMethodA(dispId,inparams,IJIDispatch_Fields.DISPATCH_PROPERTYGET);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] get(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] Get(string name, object[] inparams) {
			//return invoke(dispId,IJIDispatch.DISPATCH_PROPERTYGET,new Object[]{null,null,null,null,null},null);
			return Get(GetIDsOfNames(name),inparams);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant get(String name) throws org.jinterop.dcom.common.JIException
		public JIVariant Get(string name) {
			//return invoke(getIDsOfNames(name),IJIDispatch.DISPATCH_PROPERTYGET,new Object[]{null,null,null,null,null},null);
			return Get(GetIDsOfNames(name));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name) throws org.jinterop.dcom.common.JIException
		public void CallMethod(string name) {
			//invoke(getIDsOfNames(name),IJIDispatch.DISPATCH_METHOD,null,null,null);
			CallMethod(GetIDsOfNames(name));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId) throws org.jinterop.dcom.common.JIException
		public void CallMethod(int dispId) {
			//invoke(dispId,IJIDispatch.DISPATCH_METHOD,new Object[]{null,null,null,null,null},null);
			CallMethodA(dispId);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant callMethodA(String name) throws org.jinterop.dcom.common.JIException
		public JIVariant CallMethodA(string name) {
			//return invoke(getIDsOfNames(name),IJIDispatch.DISPATCH_METHOD,new Object[]{null,null,null,null,null},null);
			return CallMethodA(GetIDsOfNames(name));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant callMethodA(int dispId) throws org.jinterop.dcom.common.JIException
		public JIVariant CallMethodA(int dispId) {
			//return invoke(dispId,IJIDispatch.DISPATCH_METHOD,new Object[]{null,null,null,null,null},null);
			return ((JIVariant[])Invoke(dispId,IJIDispatch_Fields.DISPATCH_METHOD,null,null,null))[0];
		}

		//Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public void CallMethod(string name, object[] inparams) {
			CallMethodA(GetIDsOfNames(name),inparams);
		}

		//	Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public void CallMethod(int dispId, object[] inparams) {
			CallMethodA(dispId,inparams);
		}

	//	Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] CallMethodA(string name, object[] inparams) {
			return CallMethodA(GetIDsOfNames(name),inparams);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams, int FLAG) throws org.jinterop.dcom.common.JIException
		private JIVariant[] CallMethodA(int dispId, object[] inparams, int FLAG) {
			object[] objectParams = inparams;
			if (objectParams == null) {
				objectParams = new object[0];
			}

			JIVariant[] variants = new JIVariant[objectParams.Length];
			for (int i = 0;i < objectParams.Length; i++) {
				JIVariant variant = null;
				object obj = objectParams[i];
				if (!(obj is JIVariant)) {
					if (obj is JIArray) {
						variant = new JIVariant((JIArray)obj);
					}
					else {
						variant = JIVariant.MakeVariant(obj);
					}

				}
				else {
					variant = (JIVariant)obj;
					 //variant = new JIVariant((JIVariant)obj);
				}

				variants[i] = variant;
			}


	//		Integer[] array = new Integer[inparams.length];
	//		//now prepare the JIArray of dispIds.
	//		System.arraycopy(arrayOfDispIds,0,array,0,inparams.length);
	//		JIArray arrayOfValues = new JIArray(array,true);

			return Invoke(dispId,FLAG,new JIArray(variants,true),null,null);
		}

		//	Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] CallMethodA(int dispId, object[] inparams) {
			return CallMethodA(dispId,inparams,IJIDispatch_Fields.DISPATCH_METHOD);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public void CallMethod(string name, object[] inparams, int[] dispIds) {
			CallMethodA(GetIDsOfNames(name),inparams,dispIds);
		}

		//inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public void CallMethod(int dispId, object[] inparams, int[] dispIds) {
			CallMethodA(dispId,inparams,dispIds);
		}

		//inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public JIVariant[] CallMethodA(string name, object[] inparams, int[] dispIds) {
			return CallMethodA(GetIDsOfNames(name),inparams,dispIds);
		}




		//if inparams == null, dispIds is not considered
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public JIVariant[] CallMethodA(int dispId, object[] inparams, int[] dispIds) {
			if (inparams == null || inparams.Length == 0) {
				return CallMethodA(dispId,inparams);
			}

			if (dispIds == null || dispIds.Length != inparams.Length) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_PARAM_LENGTH));
			}

			int?[] array = new int?[inparams.Length];
			//now prepare the JIArray of dispIds.
			for (int i = 0; i < inparams.Length; i++) {
				array[i] = new int?(dispIds[i]);
			}

			JIArray arrayOfValues = new JIArray(array,true);

			JIVariant[] variants = new JIVariant[inparams.Length];
			for (int i = 0;i < inparams.Length; i++) {
				JIVariant variant = null;
				object obj = inparams[i];
				if (!(obj is JIVariant)) {
					if (obj is JIArray) {
						variant = new JIVariant((JIArray)obj);
					}
					else {
						variant = JIVariant.MakeVariant(obj);
					}
				}
				else {
					variant = (JIVariant)obj;
					//variant = new JIVariant((JIVariant)obj);
				}

				variants[i] = variant;
			}

			return Invoke(dispId,IJIDispatch_Fields.DISPATCH_METHOD,new JIArray(variants,true),arrayOfValues,null);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams, String[] paramNames) throws org.jinterop.dcom.common.JIException
		public void CallMethod(string name, object[] inparams, string[] paramNames) {
			CallMethodA(name,inparams,paramNames);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams, String[] paramNames) throws org.jinterop.dcom.common.JIException
		public JIVariant[] CallMethodA(string name, object[] inparams, string[] paramNames) {
			if (inparams == null || inparams.Length == 0) {
				return CallMethodA(GetIDsOfNames(name),inparams);
			}

			if (paramNames == null || paramNames.Length != inparams.Length) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_PARAM_LENGTH));
			}

			string[] names = new string[paramNames.Length + 1];
			names[0] = name;
			Array.Copy(paramNames,0,names,1,paramNames.Length);
			int[] dispIds = GetIDsOfNames(names);

			int[] newDispIds = new int[dispIds.Length - 1];

			for (int i = 0; i < newDispIds.Length; i++) {
				newDispIds[i] = dispIds[i + 1]; //skip the apiname
			}

			return CallMethodA(dispIds[0],inparams,newDispIds);
		}



		private static readonly int?[] ArrayOfDispIds = new int?[100];
		private static readonly JIStruct ExcepInfo = new JIStruct();
		static JIDispatchImpl() {
			try {
				ExcepInfo.AddMember(typeof(short?));
				ExcepInfo.AddMember(typeof(short?));
				ExcepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
				ExcepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
				ExcepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
				ExcepInfo.AddMember(typeof(int?));
				ExcepInfo.AddMember(new JIPointer(null,true));
				ExcepInfo.AddMember(new JIPointer(null,true));
				ExcepInfo.AddMember(typeof(int?));
			}
			catch (JIException e) {
				JISystem.Logger.throwing("JIDispatchImpl","static initializer",e);
			}
			for (int i = 0;i < 100;i++) {
				ArrayOfDispIds[i] = new int?(i);
			}

		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(int dispId, Object[] params) throws org.jinterop.dcom.common.JIException
		public void Put(int dispId, object[] @params) {
			Put(dispId,@params,false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(String name, Object[] params) throws org.jinterop.dcom.common.JIException
		public void Put(string name, object[] @params) {
			Put(GetIDsOfNames(name),@params,false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(int dispId, Object[] params) throws org.jinterop.dcom.common.JIException
		public void PutRef(int dispId, object[] @params) {
			Put(dispId,@params,true);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(String name, Object[] params) throws org.jinterop.dcom.common.JIException
		public void PutRef(string name, object[] @params) {
			Put(GetIDsOfNames(name),@params,true);
		}

		public JIExcepInfo LastExcepInfo {
			get {
				return LastExcepInfo_Renamed;
			}
		}

		public override string ToString() {
			return "IJIDispatch[" + base.ToString() + "]";
		}
	}

}