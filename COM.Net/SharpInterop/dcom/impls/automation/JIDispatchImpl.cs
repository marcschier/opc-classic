// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {



    using JIErrorCodes = common.JIErrorCodes;
    using JIException = common.JIException;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JICallBuilder = core.JICallBuilder;
    using JIComObjectImplWrapper = core.JIComObjectImplWrapper;
    using JIFlags = core.JIFlags;
    using JIFrameworkHelper = core.JIFrameworkHelper;
    using JIPointer = core.JIPointer;
    using JIString = core.JIString;
    using JIStruct = core.JIStruct;
    using JIVariant = core.JIVariant;

    using UUID = rpc.core.UUID;
    /// <summary>
    ///@exclude
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
	internal sealed class JIDispatchImpl : JIComObjectImplWrapper, IJIDispatch
	{

		/// 
		private const long serialVersionUID = 4908149252176353846L;

		//IJIComObject comObject = null;
		private IDictionary cacheOfDispIds = new Hashtable();
		internal JIDispatchImpl(IJIComObject comObject) : base(comObject)
		{
			//this.comObject = comObject;
		}

        public const int FLAG_TYPEINFO_SUPPORTED = 1;
		public const int FLAG_TYPEINFO_NOTSUPPORTED = 0;

        public IJIComObject COMObject => ComObject;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int getTypeInfoCount() throws org.jinterop.dcom.common.JIException
        public int TypeInfoCount
		{
			get
			{
                var obj = new JICallBuilder(true) {
                    Opnum = 0
                };
                obj.AddInParamAsInt(0, JIFlags.FLAG_NULL);
				obj.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
				var result = ComObject.Call(obj);
				return (int)(int?)result[0];
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getIDsOfNames(String apiName) throws org.jinterop.dcom.common.JIException
		public int getIDsOfNames(string apiName)
		{
			if (apiName == null || apiName.Trim().Equals(""))
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
			}

			var innerMap = (IDictionary)cacheOfDispIds[apiName];
			if (innerMap != null)
			{
				var dispId = (int?)innerMap[apiName];
				return (int)dispId;
			}



            var obj = new JICallBuilder(true) {
                Opnum = 2 //size of the array                                                                    //1st is the num elements and second is the actual values
            };

            var name = new JIString(apiName.Trim(),JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
			var array = new JIArray(new JIPointer[]{new JIPointer(name)},true);
			obj.AddInParamAsUUID(UUID.NIL_UUID,JIFlags.FLAG_NULL);
			obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(1,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x800,JIFlags.FLAG_NULL);
			obj.AddOutParamAsObject(new JIArray(typeof(int?),null,1,true),JIFlags.FLAG_NULL);


			var result = ComObject.Call(obj);

			if (result == null && obj.Error)
			{
				throw new JIException(obj.HRESULT);
			}

			innerMap = new Hashtable();
			innerMap[apiName] = ((object[])((JIArray)result[0]).ArrayInstance)[0];
			cacheOfDispIds[apiName] = innerMap;


			//first will be the length , and the next will be the actual value.
			return (int)(int?)((object[])((JIArray)result[0]).ArrayInstance)[0]; //will get the dispatch ID.
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int[] getIDsOfNames(String[] apiName) throws org.jinterop.dcom.common.JIException
		public int[] getIDsOfNames(string[] apiName)
		{
			if (apiName == null || apiName.Length == 0)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
			}

			var sendForAll = false;
			//first one will be the method name
			var innerMap = (IDictionary)cacheOfDispIds[apiName[0]];
			if (innerMap != null) //if name is not found will not even go in. so it is safe to assume that api name will always be there.
			{
				var values = new int[innerMap.Count];
				for (var i = 0; i < apiName.Length; i++)
				{
					var dispId = (int?)innerMap[apiName[i]];
                    if (dispId == null) {
                        sendForAll = true;
                        break;
                    }
                    values[i] = (int)dispId;
                }

				if (!sendForAll)
				{
					return values; //all found returning now
				}
			}


            var obj = new JICallBuilder(true) {
                Opnum = 2 //size of the array                                                                    //1st is the num elements and second is the actual values
            };

            var pointers = new JIPointer[apiName.Length];

			for (var i = 0;i < apiName.Length;i++)
			{
				if (apiName[i] == null || apiName[i].Trim().Equals(""))
				{
					throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
				}
				pointers[i] = new JIPointer(new JIString(apiName[i].Trim(),JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
			}


			var array = new JIArray(pointers,true);
			var arrayOut = new JIArray(typeof(int?),null,1,true);
			obj.AddInParamAsUUID(UUID.NIL_UUID,JIFlags.FLAG_NULL);
			obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(apiName.Length,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x800,JIFlags.FLAG_NULL);

			obj.AddOutParamAsObject(arrayOut,JIFlags.FLAG_NULL);

			var result = ComObject.Call(obj);

			if (obj.HRESULT != 0) //exception occured
			{
				throw new JIException(obj.HRESULT,JISystem.getLocalizedMessage(obj.HRESULT));
			}


			var arrayOfResults = (JIArray)result[0];
			var arrayOfDispIds = (int?[])arrayOfResults.ArrayInstance;
			var retVal = new int[apiName.Length];

			innerMap = innerMap ?? new Hashtable();
			for (var i = 0;i < apiName.Length;i++)
			{
				retVal[i] = (int)arrayOfDispIds[i];
				innerMap[apiName[i]] = arrayOfDispIds[i];
			}

			if (!cacheOfDispIds.Contains(apiName[0]))
			{
				cacheOfDispIds[apiName[0]] = innerMap;
			}
			return retVal;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfo(int typeInfo) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo getTypeInfo(int typeInfo)
		{
            var obj = new JICallBuilder(true) {
                Opnum = 1
            };
            obj.AddInParamAsInt(typeInfo,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x400,JIFlags.FLAG_NULL);
			obj.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			//obj.setUpParams(new Object[]{new Integer(typeInfo),new Integer(0x400)},new Object[]{MInterfacePointer.class},JIFlags.FLAG_NULL,JIFlags.FLAG_NULL);
			var result = ComObject.Call(obj);
			return (IJITypeInfo)JIObjectFactory.narrowObject((IJIComObject)result[0]);
		}

	//	//First inparams[0] will always be variant and the inparams[1] is expected to be an JIArray
	//	public JIVariant invoke(int dispId,int dispatchFlags,Object[] inparams) throws JIException
	//	{
	//		return invoke(dispId,dispatchFlags,inparams,null);
	//	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] invoke(int dispId,int dispatchFlags,org.jinterop.dcom.core.JIArray arrayOfVariantsInParams,org.jinterop.dcom.core.JIArray arrayOfNamedDispIds,org.jinterop.dcom.core.JIVariant outParamType) throws org.jinterop.dcom.common.JIException
		public JIVariant[] invoke(int dispId, int dispatchFlags, JIArray arrayOfVariantsInParams, JIArray arrayOfNamedDispIds, JIVariant outParamType)
		{
			LastExcepInfo.clearAll();
            var obj = new JICallBuilder(true) {
                Opnum = 3
            };


            var dispParams = new JIStruct();

			//now check whether any of the variants is representation of a variant ptr, if so replace it with an
			//EMPTY variant and add it to another array.
			var listOfVariantPtrs = new ArrayList();
			var listOfPositions = new ArrayList();
			JIVariant[] variants = null;
			var lengthVar = 0;
			//bool isLastAptr = false;
			if (arrayOfVariantsInParams != null)
			{
				lengthVar = JIFrameworkHelper.ReverseArrayForDispatch(arrayOfVariantsInParams);
				variants = (JIVariant[])arrayOfVariantsInParams.ArrayInstance;
				for (var i = 0;i < variants.Length;i++)
				{
					var variant = variants[i];
					if (variant.IsByRef)
					{
						listOfVariantPtrs.Add(variant);
						listOfPositions.Add(i); //for position array
						//now replace with Empty.
						//variants[i] = new JIVariant(JIVariant.POINTER);
						variants[i] = JIVariant.CreateEMPTY();
					}
				}
			}


			var lengthPtr = 0;
			if (arrayOfNamedDispIds != null)
			{
				lengthPtr = JIFrameworkHelper.ReverseArrayForDispatch(arrayOfNamedDispIds);
			}

			dispParams.AddMember(new JIPointer(arrayOfVariantsInParams)); //should be an array of variants
			dispParams.AddMember(new JIPointer(arrayOfNamedDispIds)); //if there, this should be an array of variants , these too.
			dispParams.AddMember(lengthVar);
			dispParams.AddMember(lengthPtr);


			obj.AddInParamAsInt(dispId,JIFlags.FLAG_NULL);
			obj.AddInParamAsUUID(UUID.NIL_UUID,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0x800,JIFlags.FLAG_NULL);
			obj.addInParamAsInt(dispatchFlags ^ 0xFFFFFFF0,JIFlags.FLAG_NULL);
			obj.AddInParamAsStruct(dispParams,JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

			//now add the extra params if exist.
			if (listOfVariantPtrs.Count > 0)
			{
				//write length
				obj.addInParamAsInt(listOfPositions.Count,JIFlags.FLAG_NULL);
				//then write the array
				obj.AddInParamAsArray(new JIArray(listOfPositions.ToArray(typeof(int?)),true),JIFlags.FLAG_NULL);
				//now write the array of variant ptrs
				obj.AddInParamAsArray(new JIArray(listOfVariantPtrs.ToArray(typeof(JIVariant)),true),JIFlags.FLAG_NULL);
			}

			obj.AddInParamAsObject(null,JIFlags.FLAG_NULL); //results --> currently all are null and this param is not required as the outparam carries this info.
			obj.AddInParamAsObject(null,JIFlags.FLAG_NULL); //excepinfo --> currently all are null and this param is not required as the excepinfo is built here.
			obj.AddInParamAsObject(null,JIFlags.FLAG_NULL); //augerr --> currently all are null and this param is not required as the excepinfo is built here.

			var outparams = new object[4];
			if (outParamType == null)
			{
				outparams[0] = typeof(JIVariant); //fill ourselves
			}
			else
			{
				outparams[0] = outParamType; //fill from users input
			}

			outparams[1] = excepInfo;
			outparams[2] = new JIPointer(typeof(int?),true);
			outparams[3] = new JIArray(typeof(JIVariant),null,1,true);



			obj.SetOutParams(outparams,JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

			object[] result = null;
			try
			{
				result = ComObject.Call(obj);
			}
			catch (JIException e)
			{
				var results = obj.ResultsInCaseOfException;
                if (results != null) {
                    //catching here so that an extended message could be sent out
                    var excepInfoRet = (JIStruct)results[1];
                    var text1 = ((JIString)excepInfoRet.GetMember(2)).String + " ";
                    var text2 = ((JIString)excepInfoRet.GetMember(3)).String + " [ ";
                    var text3 = ((JIString)excepInfoRet.GetMember(4)).String + " ] ";
                    LastExcepInfo.excepDesc = text2;
                    LastExcepInfo.excepHelpfile = text3;
                    LastExcepInfo.excepSource = text1;
                    LastExcepInfo.errorCode = (int)(short?)excepInfoRet.GetMember(0) != 0 ? (int)(short?)excepInfoRet.GetMember(0) : (int)(int?)excepInfoRet.GetMember(8);


                    var automationException = new JIAutomationException(e) {
                        ExcepInfo = LastExcepInfo
                    };
                    throw automationException;
                    //				throw new JIException(obj.getHRESULT(),JISystem.getLocalizedMessage(obj.getHRESULT()) + " ==> Message from Server: " +
                    //				text1 + text2 + text3);
                }
                throw e;
            }

			var array = (JIArray)result[3];
			var byrefVariants = (JIVariant[])array.ArrayInstance; //will be a sinlge dimensional array.

			var retVal = new JIVariant[1 + byrefVariants.Length];
			retVal[0] = (JIVariant)result[0];
			Array.Copy(byrefVariants,0,retVal,1,byrefVariants.Length);

			return retVal;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void put(int dispId, Object[] inparams, bool isRef) throws org.jinterop.dcom.common.JIException
		private void put(int dispId, object[] inparams, bool isRef)
		{
			var propertyFlag = isRef ? IJIDispatch_Fields.DISPATCH_PROPERTYPUTREF : IJIDispatch_Fields.DISPATCH_PROPERTYPUT;
			var objectParams = inparams;
			if (objectParams == null)
			{
				objectParams = new object[0];
			}

			var variants = new JIVariant[objectParams.Length];
			for (var i = 0;i < objectParams.Length; i++)
			{
				JIVariant variant = null;
				var obj = objectParams[i];
				if (!(obj is JIVariant))
				{
					if (obj is JIArray)
					{
						variant = new JIVariant((JIArray)obj,isRef);
					}
					else
					{
						variant = JIVariant.makeVariant(obj,isRef);
					}

				}
				else
				{
					variant = (JIVariant)obj;
					 //variant = new JIVariant((JIVariant)obj);
				}

				variants[i] = variant;
			}


			invoke(dispId,propertyFlag,new JIArray(variants,true),new JIArray(new int?[]{ IJIDispatch_Fields.DISPATCH_DISPID_PUTPUTREF },true),null);
			//invoke(dispId,propertyFlag,new JIArray(new JIVariant[]{inparam},true),new JIArray(new Integer[]{new Integer(propertyFlag)},true),null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(int dispId, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void put(int dispId, JIVariant inparam)
		{
			put(dispId,new object[]{inparam},false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(String name, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void put(string name, JIVariant inparam)
		{
			put(getIDsOfNames(name),inparam);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(int dispId, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void putRef(int dispId, JIVariant inparam)
		{
			put(dispId,new object[]{inparam},true);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(String name, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException
		public void putRef(string name, JIVariant inparam)
		{
			putRef(getIDsOfNames(name),inparam);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant get(int dispId) throws org.jinterop.dcom.common.JIException
		public JIVariant get(int dispId)
		{
			//return invoke(dispId,IJIDispatch.DISPATCH_PROPERTYGET,new Object[]{null,null,null,null,null},null);
			return ((JIVariant[])invoke(dispId,IJIDispatch_Fields.DISPATCH_PROPERTYGET,null,null,null))[0];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] get(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] get(int dispId, object[] inparams)
		{
			return callMethodA(dispId,inparams,IJIDispatch_Fields.DISPATCH_PROPERTYGET);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] get(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] get(string name, object[] inparams)
		{
			//return invoke(dispId,IJIDispatch.DISPATCH_PROPERTYGET,new Object[]{null,null,null,null,null},null);
			return get(getIDsOfNames(name),inparams);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant get(String name) throws org.jinterop.dcom.common.JIException
		public JIVariant get(string name)
		{
			//return invoke(getIDsOfNames(name),IJIDispatch.DISPATCH_PROPERTYGET,new Object[]{null,null,null,null,null},null);
			return get(getIDsOfNames(name));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name) throws org.jinterop.dcom.common.JIException
		public void callMethod(string name)
		{
			//invoke(getIDsOfNames(name),IJIDispatch.DISPATCH_METHOD,null,null,null);
			callMethod(getIDsOfNames(name));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId) throws org.jinterop.dcom.common.JIException
		public void callMethod(int dispId)
		{
			//invoke(dispId,IJIDispatch.DISPATCH_METHOD,new Object[]{null,null,null,null,null},null);
			callMethodA(dispId);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant callMethodA(String name) throws org.jinterop.dcom.common.JIException
		public JIVariant callMethodA(string name)
		{
			//return invoke(getIDsOfNames(name),IJIDispatch.DISPATCH_METHOD,new Object[]{null,null,null,null,null},null);
			return callMethodA(getIDsOfNames(name));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant callMethodA(int dispId) throws org.jinterop.dcom.common.JIException
		public JIVariant callMethodA(int dispId)
		{
			//return invoke(dispId,IJIDispatch.DISPATCH_METHOD,new Object[]{null,null,null,null,null},null);
			return ((JIVariant[])invoke(dispId,IJIDispatch_Fields.DISPATCH_METHOD,null,null,null))[0];
		}

		//Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public void callMethod(string name, object[] inparams)
		{
			callMethodA(getIDsOfNames(name),inparams);
		}

		//	Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public void callMethod(int dispId, object[] inparams)
		{
			callMethodA(dispId,inparams);
		}

	//	Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] callMethodA(string name, object[] inparams)
		{
			return callMethodA(getIDsOfNames(name),inparams);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams, int FLAG) throws org.jinterop.dcom.common.JIException
		private JIVariant[] callMethodA(int dispId, object[] inparams, int FLAG)
		{
			var objectParams = inparams;
			if (objectParams == null)
			{
				objectParams = new object[0];
			}

			var variants = new JIVariant[objectParams.Length];
			for (var i = 0;i < objectParams.Length; i++)
			{
				JIVariant variant = null;
				var obj = objectParams[i];
				if (!(obj is JIVariant))
				{
					if (obj is JIArray)
					{
						variant = new JIVariant((JIArray)obj);
					}
					else
					{
						variant = JIVariant.makeVariant(obj);
					}

				}
				else
				{
					variant = (JIVariant)obj;
					 //variant = new JIVariant((JIVariant)obj);
				}

				variants[i] = variant;
			}


	//		Integer[] array = new Integer[inparams.length];
	//		//now prepare the JIArray of dispIds.
	//		System.arraycopy(arrayOfDispIds,0,array,0,inparams.length);
	//		JIArray arrayOfValues = new JIArray(array,true);

			return invoke(dispId,FLAG,new JIArray(variants,true),null,null);
		}

		//	Ordinary params, will internally form Variant and the JIArray associated
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException
		public JIVariant[] callMethodA(int dispId, object[] inparams)
		{
			return callMethodA(dispId,inparams,IJIDispatch_Fields.DISPATCH_METHOD);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public void callMethod(string name, object[] inparams, int[] dispIds)
		{
			callMethodA(getIDsOfNames(name),inparams,dispIds);
		}

		//inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public void callMethod(int dispId, object[] inparams, int[] dispIds)
		{
			callMethodA(dispId,inparams,dispIds);
		}

		//inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public JIVariant[] callMethodA(string name, object[] inparams, int[] dispIds)
		{
			return callMethodA(getIDsOfNames(name),inparams,dispIds);
		}




		//if inparams == null, dispIds is not considered
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException
		public JIVariant[] callMethodA(int dispId, object[] inparams, int[] dispIds)
		{
			if (inparams == null || inparams.Length == 0)
			{
				return callMethodA(dispId,inparams);
			}

			if (dispIds == null || dispIds.Length != inparams.Length)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_PARAM_LENGTH));
			}

			var array = new int?[inparams.Length];
			//now prepare the JIArray of dispIds.
			for (var i = 0; i < inparams.Length; i++)
			{
				array[i] = dispIds[i];
			}

			var arrayOfValues = new JIArray(array,true);

			var variants = new JIVariant[inparams.Length];
			for (var i = 0;i < inparams.Length; i++)
			{
				JIVariant variant = null;
				var obj = inparams[i];
				if (!(obj is JIVariant))
				{
					if (obj is JIArray)
					{
						variant = new JIVariant((JIArray)obj);
					}
					else
					{
						variant = JIVariant.makeVariant(obj);
					}
				}
				else
				{
					variant = (JIVariant)obj;
					//variant = new JIVariant((JIVariant)obj);
				}

				variants[i] = variant;
			}

			return invoke(dispId,IJIDispatch_Fields.DISPATCH_METHOD,new JIArray(variants,true),arrayOfValues,null);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams, String[] paramNames) throws org.jinterop.dcom.common.JIException
		public void callMethod(string name, object[] inparams, string[] paramNames)
		{
			callMethodA(name,inparams,paramNames);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams, String[] paramNames) throws org.jinterop.dcom.common.JIException
		public JIVariant[] callMethodA(string name, object[] inparams, string[] paramNames)
		{
			if (inparams == null || inparams.Length == 0)
			{
				return callMethodA(getIDsOfNames(name),inparams);
			}

			if (paramNames == null || paramNames.Length != inparams.Length)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_PARAM_LENGTH));
			}

			var names = new string[paramNames.Length + 1];
			names[0] = name;
			Array.Copy(paramNames,0,names,1,paramNames.Length);
			var dispIds = getIDsOfNames(names);

			var newDispIds = new int[dispIds.Length - 1];

			for (var i = 0; i < newDispIds.Length; i++)
			{
				newDispIds[i] = dispIds[i + 1]; //skip the apiname
			}

			return callMethodA(dispIds[0],inparams,newDispIds);
		}



		private static readonly int?[] arrayOfDispIds = new int?[100];
		private static readonly JIStruct excepInfo = new JIStruct();
		static JIDispatchImpl()
		{
			try
			{
				excepInfo.AddMember(typeof(short?));
				excepInfo.AddMember(typeof(short?));
				excepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
				excepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
				excepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
				excepInfo.AddMember(typeof(int?));
				excepInfo.AddMember(new JIPointer(null,true));
				excepInfo.AddMember(new JIPointer(null,true));
				excepInfo.AddMember(typeof(int?));
			}
			catch (JIException e)
			{
				Log.Logger.Error(e, "JIDispatchImpl","static initializer",e);
			}
			for (var i = 0;i < 100;i++)
			{
				arrayOfDispIds[i] = i;
			}

		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(int dispId, Object[] params) throws org.jinterop.dcom.common.JIException
		public void put(int dispId, object[] @params)
		{
			put(dispId,@params,false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(String name, Object[] params) throws org.jinterop.dcom.common.JIException
		public void put(string name, object[] @params)
		{
			put(getIDsOfNames(name),@params,false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(int dispId, Object[] params) throws org.jinterop.dcom.common.JIException
		public void putRef(int dispId, object[] @params)
		{
			put(dispId,@params,true);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(String name, Object[] params) throws org.jinterop.dcom.common.JIException
		public void putRef(string name, object[] @params)
		{
			put(getIDsOfNames(name),@params,true);
		}

        public JIExcepInfo LastExcepInfo { get; } = new JIExcepInfo();

        public override string ToString()
		{
			return "IJIDispatch[" + base.ToString() + "]";
		}
	}

}