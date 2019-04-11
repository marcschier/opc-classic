using System;

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
    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIStruct = org.jinterop.dcom.core.JIStruct;
    using JIUnion = org.jinterop.dcom.core.JIUnion;
    using JIVariant = org.jinterop.dcom.core.JIVariant;

    using UUID = rpc.core.UUID;


    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
    internal sealed class JITypeInfoImpl : JIComObjectImplWrapper, IJITypeInfo {

        /// 
        private const long SerialVersionUID = 693590689068822035L;

        //IJIComObject comObject = null;
        //JIRemUnknown unknown = null;
        public JITypeInfoImpl(IJIComObject comObject) : base(comObject) { //, JIRemUnknown unknown
            //this.comObject = comObject;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FuncDesc getFuncDesc(int index) throws org.jinterop.dcom.common.JIException
        public FuncDesc GetFuncDesc(int index) {


            //prepare the GO here

            JICallBuilder obj = new JICallBuilder(true);
            obj.Opnum = 2;
            obj.AddInParamAsInt(index,JIFlags.FLAG_NULL);

            //now to prepare out params
            JIStruct funcDescStruct = new JIStruct();
            funcDescStruct.AddMember(typeof(int?));
            funcDescStruct.AddMember(new JIPointer(new JIArray(typeof(int?),null,1,true)));
            //first read the pointer representation. Do not want to use funcdesc but only describe
            //it. This should show the flexibility of the API.
            //TODO have to make a Pointer type which only reads the representation.
            obj.AddOutParamAsObject(new JIPointer(funcDescStruct),JIFlags.FLAG_NULL);

            //CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
            //come null and even if something comes, I don't know which pointer PVOID stands for.
            JIStruct cleanlocalstorage = new JIStruct();
            cleanlocalstorage.AddMember(typeof(int?));
            cleanlocalstorage.AddMember(typeof(int?));
            cleanlocalstorage.AddMember(typeof(int?));
            obj.AddOutParamAsObject(new JIPointer(cleanlocalstorage),JIFlags.FLAG_NULL);




            //now for member id
            //obj.addOutParamAsType(Integer.class,JIFlags.FLAG_NULL);

            //now for lprgscode, Pointer to Conformant array of SCODEs (int)
            //obj.addOutParamAsObject(new Pointer(new JIArray(Integer.class,null,1,true)), JIFlags.FLAG_NULL);

            //now for lprgelemdescParam, Pointer to Conformant array of ELEMDESC (struct)
            //define the struct
            JIStruct elemDesc = new JIStruct();

            //SAFEARRAYBOUNDS
            JIStruct safeArrayBounds = new JIStruct();
            safeArrayBounds.AddMember(typeof(int?));
            safeArrayBounds.AddMember(typeof(int?));

            //arraydesc
            JIStruct arrayDesc = new JIStruct();
            //typedesc
            JIStruct typeDesc = new JIStruct();

            arrayDesc.AddMember(typeDesc);
            arrayDesc.AddMember(typeof(short?));
            arrayDesc.AddMember(new JIArray(safeArrayBounds,new int[]{ 1 },1,true));

            JIUnion forTypeDesc = new JIUnion(typeof(short?));
            JIPointer ptrToTypeDesc = new JIPointer(typeDesc);
            JIPointer ptrToArrayDesc = new JIPointer(arrayDesc);

            forTypeDesc.AddMember(TypeDesc.VT_PTR,ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
            forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED,typeof(int?));
            typeDesc.AddMember(forTypeDesc);
            typeDesc.AddMember(typeof(short?)); //VARTYPE

            //PARAMDESC
            JIStruct paramDesc2 = new JIStruct();
            paramDesc2.AddMember(typeof(int?));
            paramDesc2.AddMember(typeof(JIVariant));
            JIStruct paramDesc = new JIStruct();
            paramDesc.AddMember(new JIPointer(paramDesc2,false));
            paramDesc.AddMember(typeof(short?));

            elemDesc.AddMember(typeDesc);
            elemDesc.AddMember(paramDesc);

            funcDescStruct.AddMember(new JIPointer(new JIArray(elemDesc,null,1,true)));
            //obj.addOutParamAsObject(new Pointer(new JIArray(elemDesc,null,1,true)), JIFlags.FLAG_NULL);

    //        obj.addOutParamAsObject(Integer.class,JIFlags.FLAG_NULL);
    //        obj.addOutParamAsObject(Integer.class,JIFlags.FLAG_NULL);
    //        obj.addOutParamAsObject(Integer.class,JIFlags.FLAG_NULL);
    //
    //        obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
    //        obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
    //
    //        obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
    //        obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);
    //
    //        obj.addOutParamAsObject(elemDesc,JIFlags.FLAG_NULL);
    //        obj.addOutParamAsObject(Short.class,JIFlags.FLAG_NULL);

            funcDescStruct.AddMember(typeof(int?));
            funcDescStruct.AddMember(typeof(int?));
            funcDescStruct.AddMember(typeof(int?));

            funcDescStruct.AddMember(typeof(short?));
            funcDescStruct.AddMember(typeof(short?));

            funcDescStruct.AddMember(typeof(short?));
            funcDescStruct.AddMember(typeof(short?));

            funcDescStruct.AddMember(elemDesc);
            funcDescStruct.AddMember(typeof(short?));


            object[] result = ComObject.Call(obj);
            FuncDesc funcDesc = new FuncDesc((JIPointer)result[0]);
            return funcDesc;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TypeAttr getTypeAttr() throws org.jinterop.dcom.common.JIException
        public TypeAttr TypeAttr {
            get {
                JICallBuilder obj = new JICallBuilder(true);
                obj.Opnum = 0;
    
    
    
                JIStruct typeAttr = new JIStruct();
                JIPointer mainPtr = new JIPointer(typeAttr);
                obj.AddOutParamAsObject(mainPtr,JIFlags.FLAG_NULL);
    
                //CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
                //come null and even if something comes, I don't know which pointer PVOID stands for.
                obj.AddOutParamAsObject(new JIPointer(typeof(int?)),JIFlags.FLAG_NULL);
    
                typeAttr.AddMember(typeof(UUID));
                typeAttr.AddMember(typeof(int?));
                typeAttr.AddMember(typeof(int?));
    
                typeAttr.AddMember(typeof(int?));
                typeAttr.AddMember(typeof(int?));
    
                typeAttr.AddMember(new JIPointer(new JIString(null,JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
    
                typeAttr.AddMember(typeof(int?));
    
                typeAttr.AddMember(typeof(int?));
    
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
                typeAttr.AddMember(typeof(short?));
    
                JIStruct typeDesc = new JIStruct();
                JIStruct arrayDesc = new JIStruct();
                JIStruct safeArrayBounds = new JIStruct();
    
                safeArrayBounds.AddMember(typeof(int?));
                safeArrayBounds.AddMember(typeof(int?));
    
                arrayDesc.AddMember(typeDesc);
                arrayDesc.AddMember(typeof(short?));
                arrayDesc.AddMember(new JIArray(safeArrayBounds,new int[]{ 1 },1,true));
    
                JIUnion forTypeDesc = new JIUnion(typeof(short?));
                JIPointer ptrToTypeDesc = new JIPointer(typeDesc);
                JIPointer ptrToArrayDesc = new JIPointer(arrayDesc);
    
                forTypeDesc.AddMember(TypeDesc.VT_PTR,ptrToTypeDesc);
                forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
                forTypeDesc.AddMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
                forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED,typeof(int?));
                typeDesc.AddMember(forTypeDesc);
                typeDesc.AddMember(typeof(short?)); //VARTYPE
    
                typeAttr.AddMember(typeDesc);
    
    
                JIStruct paramDesc = new JIStruct();
                paramDesc.AddMember(new JIPointer(typeof(JIVariant),false));
                paramDesc.AddMember(typeof(short?));
    
                typeAttr.AddMember(paramDesc);
    
                object[] result = ComObject.Call(obj);
                TypeAttr attr = new TypeAttr((JIPointer)result[0]);
                return attr;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getContainingTypeLib() throws org.jinterop.dcom.common.JIException
        public object[] ContainingTypeLib {
            get {
                JICallBuilder callObject = new JICallBuilder(true);
                callObject.AddOutParamAsObject(typeof(IJIComObject),JIFlags.FLAG_NULL);
                callObject.AddOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
                callObject.Opnum = 15;
                object[] result = ComObject.Call(callObject);
                object[] retVal = new object[2];
                retVal[0] = (IJITypeLib) JIObjectFactory.NarrowObject((IJIComObject)result[0]);
                retVal[1] = result[1];
                return retVal;
            }
        }

    //    HRESULT GetDllEntry(
    //              MEMBERID  memid,
    //              InvokeKind  invKind,
    //              BSTR FAR*  pBstrDllName,
    //              BSTR FAR*  pBstrName,
    //              unsigned short FAR*  pwOrdinal
    //            );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDllEntry(int memberId, int invKind) throws org.jinterop.dcom.common.JIException
        public object[] GetDllEntry(int memberId, int invKind) {
            if (invKind != (int)InvokeKind_Fields.INVOKE_FUNC && invKind != (int)InvokeKind_Fields.INVOKE_PROPERTYGET && invKind != (int)InvokeKind_Fields.INVOKE_PROPERTYPUTREF && invKind != (int)InvokeKind_Fields.INVOKE_PROPERTYPUT) {
                throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.E_INVALIDARG));
            }

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.AddInParamAsInt(memberId,JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(invKind,JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(1,JIFlags.FLAG_NULL); //refPtrFlags , as per the oaidl.idl...
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(typeof(short?),JIFlags.FLAG_NULL);
            callObject.Opnum = 10;
            return ComObject.Call(callObject);
        }

    //    HRESULT GetDocumentation(
    //              MEMBERID  memid,
    //              BSTR FAR*  pBstrName,
    //              BSTR FAR*  pBstrDocString,
    //              unsigned long FAR*  pdwHelpContext,
    //              BSTR FAR*  pBstrHelpFile
    //            );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDocumentation(int memberId) throws org.jinterop.dcom.common.JIException
        public object[] GetDocumentation(int memberId) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.AddInParamAsInt(memberId,JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(0xb,JIFlags.FLAG_NULL); //refPtrFlags , as per the oaidl.idl...
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            callObject.Opnum = 9;
            return ComObject.Call(callObject);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public VarDesc getVarDesc(int index) throws org.jinterop.dcom.common.JIException
        public VarDesc GetVarDesc(int index) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 3;
            callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);

            //now build the vardesc
            JIStruct vardesc = new JIStruct();
            callObject.AddOutParamAsObject(new JIPointer(vardesc),JIFlags.FLAG_NULL);
            //CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
            //come null and even if something comes, I don't know which pointer PVOID stands for.
            JIStruct cleanlocalstorage = new JIStruct();
            cleanlocalstorage.AddMember(typeof(int?));
            cleanlocalstorage.AddMember(typeof(int?));
            cleanlocalstorage.AddMember(typeof(int?));
            callObject.AddOutParamAsObject(new JIPointer(cleanlocalstorage),JIFlags.FLAG_NULL);

            vardesc.AddMember(typeof(int?)); //memberid
            vardesc.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));

            JIUnion union = new JIUnion(typeof(int?));
            union.AddMember(new int?(VarDesc.VAR_PERINSTANCE),typeof(int?));
            union.AddMember(new int?(VarDesc.VAR_DISPATCH),typeof(int?));
            union.AddMember(new int?(VarDesc.VAR_STATIC),typeof(int?));
            union.AddMember(new int?(VarDesc.VAR_CONST),typeof(JIVariant));
            vardesc.AddMember(union);

            JIStruct elemDesc = new JIStruct();

            //SAFEARRAYBOUNDS
            JIStruct safeArrayBounds = new JIStruct();
            safeArrayBounds.AddMember(typeof(int?));
            safeArrayBounds.AddMember(typeof(int?));

            //arraydesc
            JIStruct arrayDesc = new JIStruct();
            //typedesc
            JIStruct typeDesc = new JIStruct();

            arrayDesc.AddMember(typeDesc);
            arrayDesc.AddMember(typeof(short?));
            arrayDesc.AddMember(new JIArray(safeArrayBounds,new int[]{ 1 },1,true));

            JIUnion forTypeDesc = new JIUnion(typeof(short?));
            JIPointer ptrToTypeDesc = new JIPointer(typeDesc);
            JIPointer ptrToArrayDesc = new JIPointer(arrayDesc);

            forTypeDesc.AddMember(TypeDesc.VT_PTR,ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY,ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_CARRAY,ptrToArrayDesc);
            forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED,typeof(int?));
            typeDesc.AddMember(forTypeDesc);
            typeDesc.AddMember(typeof(short?)); //VARTYPE

            //PARAMDESC
            JIStruct paramDesc2 = new JIStruct();
            paramDesc2.AddMember(typeof(int?));
            paramDesc2.AddMember(typeof(JIVariant));
            JIStruct paramDesc = new JIStruct();
            paramDesc.AddMember(new JIPointer(paramDesc2,false));
            paramDesc.AddMember(typeof(short?));
    //        JIStruct paramDesc = new JIStruct();
    //        paramDesc.addMember(new JIPointer(JIVariant.class,false));
    //        //paramDesc.addMember(JIVariant.class);
    //        paramDesc.addMember(Short.class);

            elemDesc.AddMember(typeDesc);
            elemDesc.AddMember(paramDesc);

            vardesc.AddMember(elemDesc);
            vardesc.AddMember(typeof(short?));
            vardesc.AddMember(typeof(int?));

            object[] result = ComObject.Call(callObject);

            return new VarDesc((JIPointer)result[0]);

        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getNames(int memberId, int maxNames) throws org.jinterop.dcom.common.JIException
        public object[] GetNames(int memberId, int maxNames) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 4;

            //for experiment only
    //        JIArray arry = new JIArray(new Integer[]{new Integer(100),new Integer(200)},true);
    //        JIStruct struct = new JIStruct();
    //        struct.addMember(Short.valueOf((short)86));
    //        struct.addMember(arry);
    //        callObject.addInParamAsStruct(struct,JIFlags.FLAG_NULL);


            callObject.AddInParamAsInt(memberId,JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(maxNames,JIFlags.FLAG_NULL);

            callObject.AddOutParamAsObject(new JIArray(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),null,1,true,true),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);

            return ComObject.Call(callObject);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getRefTypeOfImplType(int index) throws org.jinterop.dcom.common.JIException
        public int GetRefTypeOfImplType(int index) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 5;
            callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
            return (int)((int?)(((object[])ComObject.Call(callObject))[0]));
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getImplTypeFlags(int index) throws org.jinterop.dcom.common.JIException
        public int GetImplTypeFlags(int index) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 6;
            callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
            return (int)((int?)(((object[])ComObject.Call(callObject))[0]));
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getRefTypeInfo(int hrefType) throws org.jinterop.dcom.common.JIException
        public IJITypeInfo GetRefTypeInfo(int hrefType) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 11;
            callObject.AddInParamAsInt(hrefType,JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            object[] result = ComObject.Call(callObject);
            return (IJITypeInfo) JIObjectFactory.NarrowObject((IJIComObject)result[0]);
        }

    //    public int[] getIdOfNames(String[] names) throws JIException
    //    {
    //        JICallBuilder callObject = new JICallBuilder(true);
    //        callObject.setOpnum(7);
    //
    //        JIPointer[] pointers = new JIPointer[names.length];
    //
    //        for (int i = 0;i < names.length;i++)
    //        {
    //            if (names[i] == null || names[i].trim().equals(""))
    //            {
    //                throw new IllegalArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
    //            }
    //            pointers[i] = new JIPointer(new JIString(names[i].trim(),JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
    //        }
    //
    //
    //        JIArray array = new JIArray(pointers,true);
    //        JIArray arrayOut = new JIArray(Integer.class,null,1,true);
    //
    //        callObject.addInParamAsArray(new JIArray(pointers,true),JIFlags.FLAG_NULL);
    //        callObject.addInParamAsInt(names.length,JIFlags.FLAG_NULL);
    //        callObject.addOutParamAsObject(arrayOut,JIFlags.FLAG_NULL);
    //
    //        Object[] result = comObject.call(callObject);
    //
    //        JIArray arrayOfResults = (JIArray)result[0];
    //        Integer[] arrayOfDispIds = (Integer[])arrayOfResults.getArrayInstance();
    //        int[] retVal = new int[names.length];
    //
    //        for (int i = 0;i < names.length;i++)
    //        {
    //            retVal[i] = arrayOfDispIds[i].intValue();
    //        }
    //
    //        return retVal;
    //
    //    }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.IJIComObject createInstance(String riid) throws org.jinterop.dcom.common.JIException
        public IJIComObject CreateInstance(string riid) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 13;

            callObject.AddInParamAsUUID(riid,JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            object[] result = ComObject.Call(callObject);
            return JIObjectFactory.NarrowObject((IJIComObject)result[0]);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIString getMops(int memberId) throws org.jinterop.dcom.common.JIException
        public JIString GetMops(int memberId) {
            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 14;
            callObject.AddInParamAsInt(memberId,JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            object[] result = ComObject.Call(callObject);
            return (JIString)result[0];
        }
    }

}