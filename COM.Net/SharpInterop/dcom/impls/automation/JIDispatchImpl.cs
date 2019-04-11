// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using rpc.core;
    using Serilog;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Dispatch implementation
    /// </summary>
    [Serializable]
    internal sealed class JIDispatchImpl : JIComObjectImplWrapper, IJIDispatch {

        internal JIDispatchImpl(IJIComObject comObject) : base(comObject) {
            //this.comObject = comObject;
        }

        //IJIComObject comObject = null;
        private readonly Hashtable _cacheOfDispIds = new Hashtable();
        public const int FLAG_TYPEINFO_SUPPORTED = 1;
        public const int FLAG_TYPEINFO_NOTSUPPORTED = 0;


        /// <inheritdoc/>
        public int TypeInfoCount {
            get {
                var obj = new JICallBuilder(true) {
                    Opnum = 0
                };
                obj.AddInParamAsInt(0, JIFlags.FLAG_NULL);
                obj.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
                var result = ComObject.Call(obj);
                return (int)result[0];
            }
        }

        /// <inheritdoc/>
        public int GetIDsOfNames(string apiName) {
            if (apiName == null || apiName.Trim().Equals("")) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
            }

            var innerMap = (IDictionary<object, object>)_cacheOfDispIds[apiName];
            if (innerMap != null) {
                var dispId = (int)innerMap[apiName];
                return (int)dispId;
            }

            var obj = new JICallBuilder(true) {
                Opnum = 2 //size of the array
                // 1st is the num elements and second is the actual values
            };

            var name = new JIString(apiName.Trim(), JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
            var array = new JIArray(new JIPointer[] { new JIPointer(name) }, true);
            obj.AddInParamAsUUID(UUID.NIL_UUID, JIFlags.FLAG_NULL);
            obj.AddInParamAsArray(array, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(1, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0x800, JIFlags.FLAG_NULL);
            obj.AddOutParamAsObject(new JIArray(typeof(int), null, 1, true), JIFlags.FLAG_NULL);

            var result = ComObject.Call(obj);
            if (result == null && obj.Error) {
                throw new JIException(obj.HRESULT);
            }

            innerMap = new Hashtable {
                [apiName] = ((object[])((JIArray)result[0]).ArrayInstance)[0]
            };
            _cacheOfDispIds[apiName] = innerMap;

            //first will be the length, and the next will be the actual value.
            return (int)((object[])((JIArray)result[0]).ArrayInstance)[0]; // will get the dispatch ID.
        }

        /// <inheritdoc/>
        public int[] GetIDsOfNames(string[] apiName) {
            if (apiName == null || apiName.Length == 0) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
            }

            var sendForAll = false;
            //first one will be the method name
            var innerMap = (IDictionary<object, object>)_cacheOfDispIds[apiName[0]];
            if (innerMap != null) //if name is not found will not even go in. so it is safe to assume that api name will always be there.
            {
                var values = new int[innerMap.Count];
                for (var i = 0; i < apiName.Length; i++) {
                    if (!innerMap.TryGetValue(apiName[i], out var dispId)) {
                        sendForAll = true;
                        break;
                    }
                    values[i] = (int)dispId;
                }

                if (!sendForAll) {
                    return values; //all found returning now
                }
            }


            var obj = new JICallBuilder(true) {
                Opnum = 2 //size of the array
                //1st is the num elements and second is the actual values
            };

            var pointers = new JIPointer[apiName.Length];
            for (var i = 0; i < apiName.Length; i++) {
                if (apiName[i] == null || apiName[i].Trim().Equals("")) {
                    throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
                }
                pointers[i] = new JIPointer(new JIString(apiName[i].Trim(), JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            }

            var array = new JIArray(pointers, true);
            var arrayOut = new JIArray(typeof(int), null, 1, true);
            obj.AddInParamAsUUID(UUID.NIL_UUID, JIFlags.FLAG_NULL);
            obj.AddInParamAsArray(array, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(apiName.Length, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0x800, JIFlags.FLAG_NULL);
            obj.AddOutParamAsObject(arrayOut, JIFlags.FLAG_NULL);

            var result = ComObject.Call(obj);

            if (obj.HRESULT != 0) { //exception occured
                throw new JIException(obj.HRESULT, JISystem.GetLocalizedMessage((JIErrorCodes)obj.HRESULT));
            }

            var arrayOfResults = (JIArray)result[0];
            var arrayOfDispIds = (int?[])arrayOfResults.ArrayInstance;
            var retVal = new int[apiName.Length];

            innerMap = innerMap ?? new Hashtable();
            for (var i = 0; i < apiName.Length; i++) {
                retVal[i] = (int)arrayOfDispIds[i];
                innerMap[apiName[i]] = arrayOfDispIds[i];
            }

            if (!_cacheOfDispIds.Contains(apiName[0])) {
                _cacheOfDispIds[apiName[0]] = innerMap;
            }
            return retVal;
        }

        /// <inheritdoc/>
        public IJITypeInfo GetTypeInfo(int typeInfo) {
            var obj = new JICallBuilder(true) {
                Opnum = 1
            };
            obj.AddInParamAsInt(typeInfo, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0x400, JIFlags.FLAG_NULL);
            obj.AddOutParamAsType(typeof(IJIComObject), JIFlags.FLAG_NULL);
            //obj.setUpParams(new Object[]{new Integer(typeInfo),new Integer(0x400)},new Object[]{MInterfacePointer.class},JIFlags.FLAG_NULL,JIFlags.FLAG_NULL);
            var result = ComObject.Call(obj);
            return (IJITypeInfo)JIObjectFactory.NarrowObject((IJIComObject)result[0]);
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="dispId"></param>
        /// <param name="dispatchFlags"></param>
        /// <param name="arrayOfVariantsInParams"></param>
        /// <param name="arrayOfNamedDispIds"></param>
        /// <param name="outParamType"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public JIVariant[] Invoke(int dispId, int dispatchFlags, JIArray arrayOfVariantsInParams, 
            JIArray arrayOfNamedDispIds, JIVariant outParamType) {
            LastExcepInfo.ClearAll();
            var obj = new JICallBuilder(true) {
                Opnum = 3
            };

            var dispParams = new JIStruct();

            //now check whether any of the variants is representation of a variant ptr, if so replace it with an
            //EMPTY variant and add it to another array.
            var listOfVariantPtrs = new List<object>();
            var listOfPositions = new List<object>();
            JIVariant[] variants = null;
            var lengthVar = 0;
            //bool isLastAptr = false;
            if (arrayOfVariantsInParams != null) {
                lengthVar = JIFrameworkHelper.ReverseArrayForDispatch(arrayOfVariantsInParams);
                variants = (JIVariant[])arrayOfVariantsInParams.ArrayInstance;
                for (var i = 0; i < variants.Length; i++) {
                    var variant = variants[i];
                    if (variant.IsByRef) {
                        listOfVariantPtrs.Add(variant);
                        listOfPositions.Add(i); //for position array
                                                //now replace with Empty.
                                                //variants[i] = new JIVariant(JIVariant.POINTER);
                        variants[i] = JIVariant.CreateEMPTY();
                    }
                }
            }


            var lengthPtr = 0;
            if (arrayOfNamedDispIds != null) {
                lengthPtr = JIFrameworkHelper.ReverseArrayForDispatch(arrayOfNamedDispIds);
            }

            dispParams.AddMember(new JIPointer(arrayOfVariantsInParams)); //should be an array of variants
            dispParams.AddMember(new JIPointer(arrayOfNamedDispIds)); //if there, this should be an array of variants, these too.
            dispParams.AddMember(lengthVar);
            dispParams.AddMember(lengthPtr);

            obj.AddInParamAsInt(dispId, JIFlags.FLAG_NULL);
            obj.AddInParamAsUUID(UUID.NIL_UUID, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0x800, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt((int)(dispatchFlags ^ 0xFFFFFFF0), JIFlags.FLAG_NULL);
            obj.AddInParamAsStruct(dispParams, JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

            //now add the extra params if exist.
            if (listOfVariantPtrs.Count > 0) {
                //write length
                obj.AddInParamAsInt(listOfPositions.Count, JIFlags.FLAG_NULL);
                //then write the array
                obj.AddInParamAsArray(new JIArray(listOfPositions.Cast<int>().ToArray(), true), JIFlags.FLAG_NULL);
                //now write the array of variant ptrs
                obj.AddInParamAsArray(new JIArray(listOfVariantPtrs.Cast<JIVariant>().ToArray(), true), JIFlags.FLAG_NULL);
            }

            obj.AddInParamAsObject(null, JIFlags.FLAG_NULL); //results --> currently all are null and this param is not required as the outparam carries this info.
            obj.AddInParamAsObject(null, JIFlags.FLAG_NULL); //excepinfo --> currently all are null and this param is not required as the excepinfo is built here.
            obj.AddInParamAsObject(null, JIFlags.FLAG_NULL); //augerr --> currently all are null and this param is not required as the excepinfo is built here.

            var outparams = new object[4];
            if (outParamType == null) {
                outparams[0] = typeof(JIVariant); //fill ourselves
            }
            else {
                outparams[0] = outParamType; //fill from users input
            }

            outparams[1] = kExcepInfo;
            outparams[2] = new JIPointer(typeof(int), true);
            outparams[3] = new JIArray(typeof(JIVariant), null, 1, true);

            obj.SetOutParams(outparams, JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

            object[] result = null;
            try {
                result = ComObject.Call(obj);
            }
            catch (JIException e) {
                var results = obj.ResultsInCaseOfException;
                if (results != null) {
                    //catching here so that an extended message could be sent out
                    var excepInfoRet = (JIStruct)results[1];
                    var text1 = ((JIString)excepInfoRet.GetMember(2)).String + " ";
                    var text2 = ((JIString)excepInfoRet.GetMember(3)).String + " [ ";
                    var text3 = ((JIString)excepInfoRet.GetMember(4)).String + " ] ";
                    LastExcepInfo.ExcepDesc = text2;
                    LastExcepInfo.HelpFilePath = text3;
                    LastExcepInfo.ExcepSource = text1;
                    LastExcepInfo.ErrorCode = (int)excepInfoRet.GetMember(0) != 0 ?
                        (int)excepInfoRet.GetMember(0) : (int)excepInfoRet.GetMember(8);

                    var automationException = new JIAutomationException(e) {
                        ExcepInfo = LastExcepInfo
                    };
                    throw automationException;
                }
                throw e;
            }

            var array = (JIArray)result[3];
            var byrefVariants = (JIVariant[])array.ArrayInstance; //will be a sinlge dimensional array.

            var retVal = new JIVariant[1 + byrefVariants.Length];
            retVal[0] = (JIVariant)result[0];
            Array.Copy(byrefVariants, 0, retVal, 1, byrefVariants.Length);

            return retVal;
        }

        /// <summary>
        /// Internal put
        /// </summary>
        /// <param name="dispId"></param>
        /// <param name="inparams"></param>
        /// <param name="isRef"></param>
        /// <exception cref="JIException"></exception>
        private void Put(int dispId, object[] inparams, bool isRef) {
            var propertyFlag = isRef ? 
                DispatchFlags.DISPATCH_PROPERTYPUTREF : DispatchFlags.DISPATCH_PROPERTYPUT;
            var objectParams = inparams;
            if (objectParams == null) {
                objectParams = new object[0];
            }

            var variants = new JIVariant[objectParams.Length];
            for (var i = 0; i < objectParams.Length; i++) {
                JIVariant variant = null;
                var obj = objectParams[i];
                if (!(obj is JIVariant)) {
                    if (obj is JIArray) {
                        variant = new JIVariant((JIArray)obj, isRef);
                    }
                    else {
                        variant = JIVariant.MakeVariant(obj, isRef);
                    }
                }
                else {
                    variant = (JIVariant)obj;
                    //variant = new JIVariant((JIVariant)obj);
                }
                variants[i] = variant;
            }


            Invoke(dispId, propertyFlag, new JIArray(variants, true),
                new JIArray(new int[] { DispatchFlags.DISPATCH_DISPID_PUTPUTREF }, true), null);
        }

        /// <inheritdoc/>
        public void Put(int dispId, JIVariant inparam) => 
            Put(dispId, new object[] { inparam }, false);

        /// <inheritdoc/>
        public void Put(string name, JIVariant inparam) => 
            Put(GetIDsOfNames(name), inparam);

        /// <inheritdoc/>
        public void PutRef(int dispId, JIVariant inparam) => 
            Put(dispId, new object[] { inparam }, true);

        /// <inheritdoc/>
        public void PutRef(string name, JIVariant inparam) => 
            PutRef(GetIDsOfNames(name), inparam);

        /// <inheritdoc/>
        public JIVariant Get(int dispId) =>
            Invoke(dispId, DispatchFlags.DISPATCH_PROPERTYGET, null, null, null)[0];

        /// <inheritdoc/>
        public JIVariant[] Get(int dispId, object[] inparams) => 
            CallMethodA(dispId, inparams, DispatchFlags.DISPATCH_PROPERTYGET);

        public JIVariant[] Get(string name, object[] inparams) =>
            Get(GetIDsOfNames(name), inparams);

        /// <inheritdoc/>
        public JIVariant Get(string name) =>
            Get(GetIDsOfNames(name));

        /// <inheritdoc/>
        public void CallMethod(string name) =>
            CallMethod(GetIDsOfNames(name));

        /// <inheritdoc/>
        public void CallMethod(int dispId) =>
            CallMethodA(dispId);

        /// <inheritdoc/>
        public JIVariant CallMethodA(string name) =>
            CallMethodA(GetIDsOfNames(name));

        /// <inheritdoc/>
        public JIVariant CallMethodA(int dispId) =>
            Invoke(dispId, DispatchFlags.DISPATCH_METHOD, null, null, null)[0];

        /// <inheritdoc/>
        public void CallMethod(string name, object[] inparams) =>
            CallMethodA(GetIDsOfNames(name), inparams);

        /// <inheritdoc/>
        public void CallMethod(int dispId, object[] inparams) => 
            CallMethodA(dispId, inparams);

        /// <inheritdoc/>
        public JIVariant[] CallMethodA(string name, object[] inparams) => 
            CallMethodA(GetIDsOfNames(name), inparams);

        /// <summary>
        /// Call ansi method
        /// </summary>
        /// <param name="dispId"></param>
        /// <param name="inparams"></param>
        /// <param name="flag"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        private JIVariant[] CallMethodA(int dispId, object[] inparams, int flag) {
            var objectParams = inparams;
            if (objectParams == null) {
                objectParams = new object[0];
            }

            var variants = new JIVariant[objectParams.Length];
            for (var i = 0; i < objectParams.Length; i++) {
                JIVariant variant = null;
                var obj = objectParams[i];
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
            return Invoke(dispId, flag, new JIArray(variants, true), null, null);
        }

        /// <inheritdoc/>
        public JIVariant[] CallMethodA(int dispId, object[] inparams) => 
            CallMethodA(dispId, inparams, DispatchFlags.DISPATCH_METHOD);

        /// <inheritdoc/>
        public void CallMethod(string name, object[] inparams, int[] dispIds) => 
            CallMethodA(GetIDsOfNames(name), inparams, dispIds);

        /// <inheritdoc/>
        public void CallMethod(int dispId, object[] inparams, int[] dispIds) => 
            CallMethodA(dispId, inparams, dispIds);

        /// <inheritdoc/>
        public JIVariant[] CallMethodA(string name, object[] inparams, int[] dispIds) => 
            CallMethodA(GetIDsOfNames(name), inparams, dispIds);

        /// <inheritdoc/>
        public JIVariant[] CallMethodA(int dispId, object[] inparams, int[] dispIds) {
            if (inparams == null || inparams.Length == 0) {
                return CallMethodA(dispId, inparams);
            }

            if (dispIds == null || dispIds.Length != inparams.Length) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_DISP_INCORRECT_PARAM_LENGTH));
            }

            var array = new int?[inparams.Length];
            //now prepare the JIArray of dispIds.
            for (var i = 0; i < inparams.Length; i++) {
                array[i] = dispIds[i];
            }

            var arrayOfValues = new JIArray(array, true);

            var variants = new JIVariant[inparams.Length];
            for (var i = 0; i < inparams.Length; i++) {
                JIVariant variant = null;
                var obj = inparams[i];
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
            return Invoke(dispId, DispatchFlags.DISPATCH_METHOD, 
                new JIArray(variants, true), arrayOfValues, null);
        }

        /// <inheritdoc/>
        public void CallMethod(string name, object[] inparams, string[] paramNames) => 
            CallMethodA(name, inparams, paramNames);

        /// <inheritdoc/>
        public JIVariant[] CallMethodA(string name, object[] inparams, string[] paramNames) {
            if (inparams == null || inparams.Length == 0) {
                return CallMethodA(GetIDsOfNames(name), inparams);
            }

            if (paramNames == null || paramNames.Length != inparams.Length) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_DISP_INCORRECT_PARAM_LENGTH));
            }

            var names = new string[paramNames.Length + 1];
            names[0] = name;
            Array.Copy(paramNames, 0, names, 1, paramNames.Length);
            var dispIds = GetIDsOfNames(names);

            var newDispIds = new int[dispIds.Length - 1];

            for (var i = 0; i < newDispIds.Length; i++) {
                newDispIds[i] = dispIds[i + 1]; //skip the apiname
            }

            return CallMethodA(dispIds[0], inparams, newDispIds);
        }

        /// <inheritdoc/>
        public void Put(int dispId, object[] @params) =>
            Put(dispId, @params, false);

        /// <inheritdoc/>
        public void Put(string name, object[] @params) => 
            Put(GetIDsOfNames(name), @params, false);

        /// <inheritdoc/>
        public void PutRef(int dispId, object[] @params) =>
            Put(dispId, @params, true);

        /// <inheritdoc/>
        public void PutRef(string name, object[] @params) => 
            Put(GetIDsOfNames(name), @params, true);

        /// <inheritdoc/>
        public override string ToString() => 
            "IJIDispatch[" + base.ToString() + "]";

        /// <inheritdoc/>
        public JIExcepInfo LastExcepInfo { get; } = new JIExcepInfo();


        /// <summary>
        /// Static initialization
        /// </summary>
        static JIDispatchImpl() {
            try {
                kExcepInfo.AddMember(typeof(short));
                kExcepInfo.AddMember(typeof(short));
                kExcepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
                kExcepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
                kExcepInfo.AddMember(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR));
                kExcepInfo.AddMember(typeof(int));
                kExcepInfo.AddMember(new JIPointer(null, true));
                kExcepInfo.AddMember(new JIPointer(null, true));
                kExcepInfo.AddMember(typeof(int));
            }
            catch (JIException e) {
                Log.Logger.Error(e, "JIDispatchImpl static initializer");
            }
            for (var i = 0; i < 100; i++) {
                kArrayOfDispIds[i] = i;
            }

        }

        private static readonly int?[] kArrayOfDispIds = new int?[100];
        private static readonly JIStruct kExcepInfo = new JIStruct();
    }
}