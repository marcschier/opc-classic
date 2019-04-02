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
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JICallBuilder = core.JICallBuilder;
    using JIComObjectImplWrapper = core.JIComObjectImplWrapper;
    using JIFlags = core.JIFlags;
    using JIPointer = core.JIPointer;
    using JIString = core.JIString;
    using JIStruct = core.JIStruct;
    using JIUnion = core.JIUnion;
    using JIVariant = core.JIVariant;

    using UUID = rpc.core.UUID;


    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
	internal sealed class JITypeInfoImpl : JIComObjectImplWrapper, IJITypeInfo
	{

		/// 
		private const long serialVersionUID = 693590689068822035L;

		//IJIComObject comObject = null;
		//JIRemUnknown unknown = null;
		internal JITypeInfoImpl(IJIComObject comObject) : base(comObject) //, JIRemUnknown unknown
		{
			//this.comObject = comObject;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FuncDesc getFuncDesc(int index) throws org.jinterop.dcom.common.JIException
		public FuncDesc getFuncDesc(int index)
		{


            //prepare the GO here

            var obj = new JICallBuilder(true) {
                Opnum = 2
            };
            obj.addInParamAsInt(index,JIFlags.FLAG_NULL);

			//now to prepare out params
			var funcDescStruct = new JIStruct();
			funcDescStruct.addMember(typeof(int?));
			funcDescStruct.addMember(new JIPointer(new JIArray(typeof(int?),null,1,true)));
			//first read the pointer representation. Do not want to use funcdesc but only describe
			//it. This should show the flexibility of the API.
			//TODO have to make a Pointer type which only reads the representation.
			obj.addOutParamAsObject(new JIPointer(funcDescStruct),JIFlags.FLAG_NULL);

			//CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
			//come null and even if something comes, I don't know which pointer PVOID stands for.
			var cleanlocalstorage = new JIStruct();
			cleanlocalstorage.addMember(typeof(int?));
			cleanlocalstorage.addMember(typeof(int?));
			cleanlocalstorage.addMember(typeof(int?));
			obj.addOutParamAsObject(new JIPointer(cleanlocalstorage),JIFlags.FLAG_NULL);




			//now for member id
			//obj.addOutParamAsType(Integer.class,JIFlags.FLAG_NULL);

			//now for lprgscode, Pointer to Conformant array of SCODEs (int)
			//obj.addOutParamAsObject(new Pointer(new JIArray(Integer.class,null,1,true)), JIFlags.FLAG_NULL);

			//now for lprgelemdescParam, Pointer to Conformant array of ELEMDESC (struct)
			//define the struct
			var elemDesc = new JIStruct();

			//SAFEARRAYBOUNDS
			var safeArrayBounds = new JIStruct();
			safeArrayBounds.addMember(typeof(int?));
			safeArrayBounds.addMember(typeof(int?));

			//arraydesc
			var arrayDesc = new JIStruct();
			//typedesc
			var typeDesc = new JIStruct();

			arrayDesc.addMember(typeDesc);
			arrayDesc.addMember(typeof(short?));
			arrayDesc.addMember(new JIArray(safeArrayBounds,new int[]{1},1,true));

			var forTypeDesc = new JIUnion(typeof(short?));
			var ptrToTypeDesc = new JIPointer(typeDesc);
			var ptrToArrayDesc = new JIPointer(arrayDesc);

			forTypeDesc.addMember(TypeDesc.VT_PTR,ptrToTypeDesc);
			forTypeDesc.addMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
			forTypeDesc.addMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
			forTypeDesc.addMember(TypeDesc.VT_USERDEFINED,typeof(int?));
			typeDesc.addMember(forTypeDesc);
			typeDesc.addMember(typeof(short?)); //VARTYPE

			//PARAMDESC
			var paramDesc2 = new JIStruct();
			paramDesc2.addMember(typeof(int?));
			paramDesc2.addMember(typeof(JIVariant));
			var paramDesc = new JIStruct();
			paramDesc.addMember(new JIPointer(paramDesc2,false));
			paramDesc.addMember(typeof(short?));

			elemDesc.addMember(typeDesc);
			elemDesc.addMember(paramDesc);

			funcDescStruct.addMember(new JIPointer(new JIArray(elemDesc,null,1,true)));
			//obj.addOutParamAsObject(new Pointer(new JIArray(elemDesc,null,1,true)), JIFlags.FLAG_NULL);

	//		obj.addOutParamAsObject(Integer.class,JIFlags.FLAG_NULL);
	//		obj.addOutParamAsObject(Integer.class,JIFlags.FLAG_NULL);
	//		obj.addOutParamAsObject(Integer.class,JIFlags.FLAG_NULL);
	//
	//		obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
	//		obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
	//
	//		obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
	//		obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
	//
	//		obj.addOutParamAsObject(elemDesc,JIFlags.FLAG_NULL);
	//		obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);

			funcDescStruct.addMember(typeof(int?));
			funcDescStruct.addMember(typeof(int?));
			funcDescStruct.addMember(typeof(int?));

			funcDescStruct.addMember(typeof(short?));
			funcDescStruct.addMember(typeof(short?));

			funcDescStruct.addMember(typeof(short?));
			funcDescStruct.addMember(typeof(short?));

			funcDescStruct.addMember(elemDesc);
			funcDescStruct.addMember(typeof(short?));


			var result = comObject.call(obj);
			var funcDesc = new FuncDesc((JIPointer)result[0]);
			return funcDesc;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TypeAttr getTypeAttr() throws org.jinterop.dcom.common.JIException
		public TypeAttr TypeAttr
		{
			get
			{
                var obj = new JICallBuilder(true) {
                    Opnum = 0
                };



                var typeAttr = new JIStruct();
				var mainPtr = new JIPointer(typeAttr);
				obj.addOutParamAsObject(mainPtr,JIFlags.FLAG_NULL);
    
				//CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
				//come null and even if something comes, I don't know which pointer PVOID stands for.
				obj.addOutParamAsObject(new JIPointer(typeof(int?)),JIFlags.FLAG_NULL);
    
				typeAttr.addMember(typeof(UUID));
				typeAttr.addMember(typeof(int?));
				typeAttr.addMember(typeof(int?));
    
				typeAttr.addMember(typeof(int?));
				typeAttr.addMember(typeof(int?));
    
				typeAttr.addMember(new JIPointer(new JIString(null,JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
    
				typeAttr.addMember(typeof(int?));
    
				typeAttr.addMember(typeof(int?));
    
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
				typeAttr.addMember(typeof(short?));
    
				var typeDesc = new JIStruct();
				var arrayDesc = new JIStruct();
				var safeArrayBounds = new JIStruct();
    
				safeArrayBounds.addMember(typeof(int?));
				safeArrayBounds.addMember(typeof(int?));
    
				arrayDesc.addMember(typeDesc);
				arrayDesc.addMember(typeof(short?));
				arrayDesc.addMember(new JIArray(safeArrayBounds,new int[]{1},1,true));
    
				var forTypeDesc = new JIUnion(typeof(short?));
				var ptrToTypeDesc = new JIPointer(typeDesc);
				var ptrToArrayDesc = new JIPointer(arrayDesc);
    
				forTypeDesc.addMember(TypeDesc.VT_PTR,ptrToTypeDesc);
				forTypeDesc.addMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
				forTypeDesc.addMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
				forTypeDesc.addMember(TypeDesc.VT_USERDEFINED,typeof(int?));
				typeDesc.addMember(forTypeDesc);
				typeDesc.addMember(typeof(short?)); //VARTYPE
    
				typeAttr.addMember(typeDesc);
    
    
				var paramDesc = new JIStruct();
				paramDesc.addMember(new JIPointer(typeof(JIVariant),false));
				paramDesc.addMember(typeof(short?));
    
				typeAttr.addMember(paramDesc);
    
				var result = comObject.call(obj);
				var attr = new TypeAttr((JIPointer)result[0]);
				return attr;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getContainingTypeLib() throws org.jinterop.dcom.common.JIException
		public object[] ContainingTypeLib
		{
			get
			{
				var callObject = new JICallBuilder(true);
				callObject.addOutParamAsObject(typeof(IJIComObject),JIFlags.FLAG_NULL);
				callObject.addOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
				callObject.Opnum = 15;
				var result = comObject.call(callObject);
				var retVal = new object[2];
				retVal[0] = (IJITypeLib) JIObjectFactory.narrowObject((IJIComObject)result[0]);
				retVal[1] = result[1];
				return retVal;
			}
		}

	//	HRESULT GetDllEntry(
	//			  MEMBERID  memid,
	//			  InvokeKind  invKind,
	//			  BSTR FAR*  pBstrDllName,
	//			  BSTR FAR*  pBstrName,
	//			  unsigned short FAR*  pwOrdinal
	//			);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDllEntry(int memberId, int invKind) throws org.jinterop.dcom.common.JIException
		public object[] getDllEntry(int memberId, int invKind)
		{
			if (invKind != (int)InvokeKind_Fields.INVOKE_FUNC && invKind != (int)InvokeKind_Fields.INVOKE_PROPERTYGET && invKind != (int)InvokeKind_Fields.INVOKE_PROPERTYPUTREF && invKind != (int)InvokeKind_Fields.INVOKE_PROPERTYPUT)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.E_INVALIDARG));
			}

			var callObject = new JICallBuilder(true);
			callObject.addInParamAsInt(memberId,JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(invKind,JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(1,JIFlags.FLAG_NULL); //refPtrFlags , as per the oaidl.idl...
			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(typeof(short?),JIFlags.FLAG_NULL);
			callObject.Opnum = 10;
			return comObject.call(callObject);
		}

	//	HRESULT GetDocumentation(
	//			  MEMBERID  memid,
	//			  BSTR FAR*  pBstrName,
	//			  BSTR FAR*  pBstrDocString,
	//			  unsigned long FAR*  pdwHelpContext,
	//			  BSTR FAR*  pBstrHelpFile
	//			);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDocumentation(int memberId) throws org.jinterop.dcom.common.JIException
		public object[] getDocumentation(int memberId)
		{
			var callObject = new JICallBuilder(true);
			callObject.addInParamAsInt(memberId,JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(0xb,JIFlags.FLAG_NULL); //refPtrFlags , as per the oaidl.idl...
			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.Opnum = 9;
			return comObject.call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public VarDesc getVarDesc(int index) throws org.jinterop.dcom.common.JIException
		public VarDesc getVarDesc(int index)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 3
            };
            callObject.addInParamAsInt(index,JIFlags.FLAG_NULL);

			//now build the vardesc
			var vardesc = new JIStruct();
			callObject.addOutParamAsObject(new JIPointer(vardesc),JIFlags.FLAG_NULL);
			//CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
			//come null and even if something comes, I don't know which pointer PVOID stands for.
			var cleanlocalstorage = new JIStruct();
			cleanlocalstorage.addMember(typeof(int?));
			cleanlocalstorage.addMember(typeof(int?));
			cleanlocalstorage.addMember(typeof(int?));
			callObject.addOutParamAsObject(new JIPointer(cleanlocalstorage),JIFlags.FLAG_NULL);

			vardesc.addMember(typeof(int?)); //memberid
			vardesc.addMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));

			var union = new JIUnion(typeof(int?));
			union.addMember(VarDesc.VAR_PERINSTANCE, typeof(int?));
			union.addMember(VarDesc.VAR_DISPATCH, typeof(int?));
			union.addMember(VarDesc.VAR_STATIC, typeof(int?));
			union.addMember(VarDesc.VAR_CONST, typeof(JIVariant));
			vardesc.addMember(union);

			var elemDesc = new JIStruct();

			//SAFEARRAYBOUNDS
			var safeArrayBounds = new JIStruct();
			safeArrayBounds.addMember(typeof(int?));
			safeArrayBounds.addMember(typeof(int?));

			//arraydesc
			var arrayDesc = new JIStruct();
			//typedesc
			var typeDesc = new JIStruct();

			arrayDesc.addMember(typeDesc);
			arrayDesc.addMember(typeof(short?));
			arrayDesc.addMember(new JIArray(safeArrayBounds,new int[]{1},1,true));

			var forTypeDesc = new JIUnion(typeof(short?));
			var ptrToTypeDesc = new JIPointer(typeDesc);
			var ptrToArrayDesc = new JIPointer(arrayDesc);

			forTypeDesc.addMember(TypeDesc.VT_PTR,ptrToTypeDesc);
			forTypeDesc.addMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
			forTypeDesc.addMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
			forTypeDesc.addMember(TypeDesc.VT_USERDEFINED,typeof(int?));
			typeDesc.addMember(forTypeDesc);
			typeDesc.addMember(typeof(short?)); //VARTYPE

			//PARAMDESC
			var paramDesc2 = new JIStruct();
			paramDesc2.addMember(typeof(int?));
			paramDesc2.addMember(typeof(JIVariant));
			var paramDesc = new JIStruct();
			paramDesc.addMember(new JIPointer(paramDesc2,false));
			paramDesc.addMember(typeof(short?));
	//		JIStruct paramDesc = new JIStruct();
	//		paramDesc.addMember(new JIPointer(JIVariant.class,false));
	//		//paramDesc.addMember(JIVariant.class);
	//		paramDesc.addMember(Short.class);

			elemDesc.addMember(typeDesc);
			elemDesc.addMember(paramDesc);

			vardesc.addMember(elemDesc);
			vardesc.addMember(typeof(short?));
			vardesc.addMember(typeof(int?));

			var result = comObject.call(callObject);

			return new VarDesc((JIPointer)result[0]);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getNames(int memberId, int maxNames) throws org.jinterop.dcom.common.JIException
		public object[] getNames(int memberId, int maxNames)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 4
            };

            //for experiment only
            //		JIArray arry = new JIArray(new Integer[]{new Integer(100),new Integer(200)},true);
            //		JIStruct struct = new JIStruct();
            //		struct.addMember(Short.valueOf((short)86));
            //		struct.addMember(arry);
            //		callObject.addInParamAsStruct(struct,JIFlags.FLAG_NULL);


            callObject.addInParamAsInt(memberId,JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(maxNames,JIFlags.FLAG_NULL);

			callObject.addOutParamAsObject(new JIArray(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),null,1,true,true),JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);

			return comObject.call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getRefTypeOfImplType(int index) throws org.jinterop.dcom.common.JIException
		public int getRefTypeOfImplType(int index)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 5
            };
            callObject.addInParamAsInt(index,JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			return (int)(int?)((object[])comObject.call(callObject))[0];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getImplTypeFlags(int index) throws org.jinterop.dcom.common.JIException
		public int getImplTypeFlags(int index)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 6
            };
            callObject.addInParamAsInt(index,JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			return (int)(int?)((object[])comObject.call(callObject))[0];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getRefTypeInfo(int hrefType) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo getRefTypeInfo(int hrefType)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 11
            };
            callObject.addInParamAsInt(hrefType,JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var result = comObject.call(callObject);
			return (IJITypeInfo) JIObjectFactory.narrowObject((IJIComObject)result[0]);
		}

	//	public int[] getIdOfNames(String[] names) throws JIException
	//	{
	//		JICallBuilder callObject = new JICallBuilder(true);
	//		callObject.setOpnum(7);
	//
	//		JIPointer[] pointers = new JIPointer[names.length];
	//
	//		for (int i = 0;i < names.length;i++)
	//		{
	//			if (names[i] == null || names[i].trim().equals(""))
	//			{
	//				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
	//			}
	//			pointers[i] = new JIPointer(new JIString(names[i].trim(),JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
	//		}
	//
	//
	//		JIArray array = new JIArray(pointers,true);
	//		JIArray arrayOut = new JIArray(Integer.class,null,1,true);
	//
	//		callObject.addInParamAsArray(new JIArray(pointers,true),JIFlags.FLAG_NULL);
	//		callObject.addInParamAsInt(names.length,JIFlags.FLAG_NULL);
	//		callObject.addOutParamAsObject(arrayOut,JIFlags.FLAG_NULL);
	//
	//		Object[] result = comObject.call(callObject);
	//
	//		JIArray arrayOfResults = (JIArray)result[0];
	//		Integer[] arrayOfDispIds = (Integer[])arrayOfResults.getArrayInstance();
	//		int[] retVal = new int[names.length];
	//
	//		for (int i = 0;i < names.length;i++)
	//		{
	//			retVal[i] = arrayOfDispIds[i].intValue();
	//		}
	//
	//		return retVal;
	//
	//	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.IJIComObject createInstance(String riid) throws org.jinterop.dcom.common.JIException
		public IJIComObject createInstance(string riid)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 13
            };

            callObject.addInParamAsUUID(riid,JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var result = comObject.call(callObject);
			return JIObjectFactory.narrowObject((IJIComObject)result[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIString getMops(int memberId) throws org.jinterop.dcom.common.JIException
		public JIString getMops(int memberId)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 14
            };
            callObject.addInParamAsInt(memberId,JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			var result = comObject.call(callObject);
			return (JIString)result[0];
		}
	}

}