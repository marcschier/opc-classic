//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//


namespace SharpInterop.Automation {
    using SharpInterop.Common;
    using SharpInterop.Core;
    using SharpInterop.Rpc.Core;
    using Opc.Classic.Dcom.Internal;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Dispatch implementation
    /// </summary>
    [Serializable]
    internal sealed class DispatchImpl : ComObjectImplWrapper, IDispatch {

        /// <summary>
        /// Create implementation
        /// </summary>
        /// <param name="comObject"></param>
        internal DispatchImpl(IComObject comObject) :
            base(comObject) {
        }

        public const int FLAG_TYPEINFO_SUPPORTED = 1;
        public const int FLAG_TYPEINFO_NOTSUPPORTED = 0;

        /// <inheritdoc/>
        public int TypeInfoCount {
            get {
                var obj = new CallBuilder(true) {
                    Opnum = 0
                };
                obj.AddInParamAsInt(0);
                obj.AddOutParamAsType(typeof(int));
                var result = ComObject.Call(obj);
                return (int)result[0];
            }
        }

        /// <inheritdoc/>
        public int GetIDsOfNames(string apiName) {
            if (apiName == null || apiName.Trim().Equals("")) {
                throw new ArgumentException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
            }

            var innerMap = _cacheOfDispIds.GetOrDefault(apiName);
            if (innerMap != null) {
                var dispId = innerMap[apiName];
                return dispId;
            }

            var obj = new CallBuilder(true) {
                Opnum = 2 // size of the array
                // 1st is the num elements and second is the actual values
            };

            var name = new ComString(apiName.Trim(), InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
            var array = new ComArray(new ComPointer[] { new ComPointer(name) }, true);
            obj.AddInParamAsUUID(UUID.NIL_UUID);
            obj.AddInParamAsArray(array);
            obj.AddInParamAsInt(1);
            obj.AddInParamAsInt(0x800);
            obj.AddOutParamAsObject(
                new ComArray(typeof(int), null, 1, true));

            var result = ComObject.Call(obj);
            if (result == null && obj.Error) {
                throw new InteropException(obj.HRESULT);
            }

            var dispid = (int)((object[])((ComArray)result[0]).ArrayInstance)[0];
            innerMap = new Dictionary<string, int> {
                [apiName] = dispid
            };
            _cacheOfDispIds.Add(apiName, innerMap);

            // first will be the length, and the next will be the actual value.
            return dispid;
            // will get the dispatch ID.
        }

        /// <inheritdoc/>
        public int[] GetIDsOfNames(string[] apiName) {
            if (apiName == null || apiName.Length == 0) {
                throw new ArgumentException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
            }

            var sendForAll = false;
            // first one will be the method name
            var innerMap = _cacheOfDispIds.GetOrDefault(apiName[0]);
            if (innerMap != null) {
                // if name is not found will not even go in. so it is safe to assume
                // that api name will always be there.
                var values = new int[innerMap.Count];
                for (var i = 0; i < apiName.Length; i++) {
                    if (!innerMap.TryGetValue(apiName[i], out var dispId)) {
                        sendForAll = true;
                        break;
                    }
                    values[i] = dispId;
                }

                if (!sendForAll) {
                    return values; // all found returning now
                }
            }


            var obj = new CallBuilder(true) {
                Opnum = 2 // size of the array
                // 1st is the num elements and second is the actual values
            };

            var pointers = new ComPointer[apiName.Length];
            for (var i = 0; i < apiName.Length; i++) {
                if (apiName[i] == null || apiName[i].Trim().Equals("")) {
                    throw new ArgumentException(Interop.GetLocalizedMessage(
                        ErrorCode.INTEROP_DISP_INCORRECT_VALUE_FOR_GETIDNAMES));
                }
                pointers[i] = new ComPointer(
                    new ComString(apiName[i].Trim(), InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            }

            var array = new ComArray(pointers, true);
            var arrayOut = new ComArray(typeof(int), null, 1, true);
            obj.AddInParamAsUUID(UUID.NIL_UUID);
            obj.AddInParamAsArray(array);
            obj.AddInParamAsInt(apiName.Length);
            obj.AddInParamAsInt(0x800);
            obj.AddOutParamAsObject(arrayOut);

            var result = ComObject.Call(obj);

            if (obj.HRESULT != 0) { // exception occured
                throw new InteropException(obj.HRESULT,
                    Interop.GetLocalizedMessage((ErrorCode)obj.HRESULT));
            }

            var arrayOfResults = (ComArray)result[0];
            var arrayOfDispIds = (int[])arrayOfResults.ArrayInstance;
            var retVal = new int[apiName.Length];

            innerMap = innerMap ?? new Dictionary<string, int>();
            for (var i = 0; i < apiName.Length; i++) {
                retVal[i] = arrayOfDispIds[i];
                innerMap[apiName[i]] = arrayOfDispIds[i];
            }

            if (!_cacheOfDispIds.ContainsKey(apiName[0])) {
                _cacheOfDispIds.Add(apiName[0], innerMap);
            }
            return retVal;
        }

        /// <inheritdoc/>
        public ITypeInfo GetTypeInfo(int typeInfo) {
            var obj = new CallBuilder(true) {
                Opnum = 1
            };
            obj.AddInParamAsInt(typeInfo);
            obj.AddInParamAsInt(0x400);
            obj.AddOutParamAsType(typeof(IComObject));
            var result = ComObject.Call(obj);
            return (ITypeInfo)ObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="dispId"></param>
        /// <param name="dispatchFlags"></param>
        /// <param name="arrayOfVariantsInParams"></param>
        /// <param name="arrayOfNamedDispIds"></param>
        /// <param name="outParamType"></param>
        /// <exception cref="InteropException"></exception>
        /// <returns></returns>
        public Variant[] Invoke(int dispId, int dispatchFlags, ComArray arrayOfVariantsInParams,
            ComArray arrayOfNamedDispIds, Variant outParamType) {
            LastExcepInfo.ClearAll();
            var obj = new CallBuilder(true) {
                Opnum = 3
            };

            var dispParams = new Struct();
            // now check whether any of the variants is representation of a variant ptr,
            // if so replace it with an EMPTY variant and add it to another array.
            var listOfVariantPtrs = new List<Variant>();
            var listOfPositions = new List<int>();
            var lengthVar = 0;
            // bool isLastAptr = false;
            if (arrayOfVariantsInParams != null) {
                lengthVar = FrameworkHelper.ReverseArrayForDispatch(arrayOfVariantsInParams);
                var variants = (Variant[])arrayOfVariantsInParams.ArrayInstance;
                for (var i = 0; i < variants.Length; i++) {
                    var variant = variants[i];
                    if (variant.IsByRef) {
                        listOfVariantPtrs.Add(variant);
                        listOfPositions.Add(i); // for position array
                                                // now replace with Empty.
                                                // variants[i] = new <see cref="Variant"/>(<see cref="Variant"/>.POINTER);
                        variants[i] = Variant.CreateEMPTY();
                    }
                }
            }


            var lengthPtr = 0;
            if (arrayOfNamedDispIds != null) {
                lengthPtr = FrameworkHelper.ReverseArrayForDispatch(arrayOfNamedDispIds);
            }
            // should be an array of variants
            dispParams.AddMember(new ComPointer(arrayOfVariantsInParams));
            // if there, this should be an array of variants, these too.
            dispParams.AddMember(new ComPointer(arrayOfNamedDispIds));
            dispParams.AddMember(lengthVar);
            dispParams.AddMember(lengthPtr);

            obj.AddInParamAsInt(dispId);
            obj.AddInParamAsUUID(UUID.NIL_UUID);
            obj.AddInParamAsInt(0x800);
            obj.AddInParamAsInt((int)(dispatchFlags ^ 0xFFFFFFF0));
            obj.AddInParamAsStruct(dispParams, InteropFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

            // now add the extra params if exist.
            if (listOfVariantPtrs.Count > 0) {
                // write length
                obj.AddInParamAsInt(listOfPositions.Count);
                // then write the array
                obj.AddInParamAsArray(new ComArray(listOfPositions.ToArray(), true));
                // now write the array of variant ptrs
                obj.AddInParamAsArray(new ComArray(listOfVariantPtrs.ToArray(), true));
            }

            // results --> currently all are null and this param is not required as the outparam carries this info.
            obj.AddInParamAsObject(null);
            // excepinfo --> currently all are null and this param is not required as the excepinfo is built here.
            obj.AddInParamAsObject(null);
            // augerr --> currently all are null and this param is not required as the excepinfo is built here.
            obj.AddInParamAsObject(null);

            var outparams = new object[4];
            if (outParamType == null) {
                outparams[0] = typeof(Variant); // fill ourselves
            }
            else {
                outparams[0] = outParamType; // fill from users input
            }

            outparams[1] = kExcepInfo;
            outparams[2] = new ComPointer(typeof(int), true);
            outparams[3] = new ComArray(typeof(Variant), null, 1, true);

            obj.SetOutParams(outparams, InteropFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);

            object[] result;
            try {
                result = ComObject.Call(obj);
            }
            catch (InteropException e) {
                var results = obj.ResultsInCaseOfException;
                if (results != null) {
                    // catching here so that an extended message could be sent out
                    var excepInfoRet = (Struct)results[1];
                    var text1 = ((ComString)excepInfoRet.GetMember(2)).String + " ";
                    var text2 = ((ComString)excepInfoRet.GetMember(3)).String + " [ ";
                    var text3 = ((ComString)excepInfoRet.GetMember(4)).String + " ] ";
                    LastExcepInfo.ExcepDesc = text2;
                    LastExcepInfo.HelpFilePath = text3;
                    LastExcepInfo.ExcepSource = text1;
                    LastExcepInfo.ErrorCode = (int)excepInfoRet.GetMember(0) != 0 ?
                        (int)excepInfoRet.GetMember(0) : (int)excepInfoRet.GetMember(8);

                    var automationException = new AutomationException(e) {
                        ExcepInfo = LastExcepInfo
                    };
                    throw automationException;
                }
                throw e;
            }

            var array = (ComArray)result[3];
            var byrefVariants = (Variant[])array.ArrayInstance; // will be a sinlge dimensional array.

            var retVal = new Variant[1 + byrefVariants.Length];
            retVal[0] = (Variant)result[0];
            Array.Copy(byrefVariants, 0, retVal, 1, byrefVariants.Length);

            return retVal;
        }

        /// <summary>
        /// Internal put
        /// </summary>
        /// <param name="dispId"></param>
        /// <param name="inparams"></param>
        /// <param name="isRef"></param>
        /// <exception cref="InteropException"></exception>
        private void Put(int dispId, object[] inparams, bool isRef) {
            var propertyFlag = isRef ?
                DispatchFlags.DISPATCH_PROPERTYPUTREF : DispatchFlags.DISPATCH_PROPERTYPUT;
            var objectParams = inparams;
            if (objectParams == null) {
                objectParams = new object[0];
            }

            var variants = new Variant[objectParams.Length];
            for (var i = 0; i < objectParams.Length; i++) {
                var obj = objectParams[i];

                Variant variant;
                if (!(obj is Variant)) {
                    if (obj is ComArray) {
                        variant = new Variant((ComArray)obj, isRef);
                    }
                    else {
                        variant = Variant.MakeVariant(obj, isRef);
                    }
                }
                else {
                    variant = (Variant)obj;
                    // variant = new <see cref="Variant"/>((<see cref="Variant"/>)obj);
                }
                variants[i] = variant;
            }
            Invoke(dispId, propertyFlag, new ComArray(variants, true),
                new ComArray(new int[] { DispatchFlags.DISPATCH_DISPID_PUTPUTREF }, true), null);
        }

        /// <inheritdoc/>
        public void Put(int dispId, Variant inparam) =>
            Put(dispId, new object[] { inparam }, false);

        /// <inheritdoc/>
        public void Put(string name, Variant inparam) =>
            Put(GetIDsOfNames(name), inparam);

        /// <inheritdoc/>
        public void PutRef(int dispId, Variant inparam) =>
            Put(dispId, new object[] { inparam }, true);

        /// <inheritdoc/>
        public void PutRef(string name, Variant inparam) =>
            PutRef(GetIDsOfNames(name), inparam);

        /// <inheritdoc/>
        public Variant Get(int dispId) =>
            Invoke(dispId, DispatchFlags.DISPATCH_PROPERTYGET, null, null, null)[0];

        /// <inheritdoc/>
        public Variant[] Get(int dispId, params object[] inparams) =>
            CallMethodA(dispId, inparams, DispatchFlags.DISPATCH_PROPERTYGET);

        public Variant[] Get(string name, params object[] inparams) =>
            Get(GetIDsOfNames(name), inparams);

        /// <inheritdoc/>
        public Variant Get(string name) =>
            Get(GetIDsOfNames(name));

        /// <inheritdoc/>
        public void CallMethod(string name) =>
            CallMethod(GetIDsOfNames(name));

        /// <inheritdoc/>
        public void CallMethod(int dispId) =>
            CallMethodA(dispId);

        /// <inheritdoc/>
        public Variant CallMethodA(string name) =>
            CallMethodA(GetIDsOfNames(name));

        /// <inheritdoc/>
        public Variant CallMethodA(int dispId) =>
            Invoke(dispId, DispatchFlags.DISPATCH_METHOD, null, null, null)[0];

        /// <inheritdoc/>
        public void CallMethod(string name, params object[] inparams) =>
            CallMethodA(GetIDsOfNames(name), inparams);

        /// <inheritdoc/>
        public void CallMethod(int dispId, params object[] inparams) =>
            CallMethodA(dispId, inparams);

        /// <inheritdoc/>
        public Variant[] CallMethodA(string name, params object[] inparams) =>
            CallMethodA(GetIDsOfNames(name), inparams);

        /// <summary>
        /// Call ansi method
        /// </summary>
        /// <param name="dispId"></param>
        /// <param name="inparams"></param>
        /// <param name="flag"></param>
        /// <exception cref="InteropException"></exception>
        /// <returns></returns>
        private Variant[] CallMethodA(int dispId, object[] inparams, int flag = InteropFlags.FLAG_NULL) {
            var objectParams = inparams;
            if (objectParams == null) {
                objectParams = new object[0];
            }

            var variants = new Variant[objectParams.Length];
            for (var i = 0; i < objectParams.Length; i++) {
                var obj = objectParams[i];

                Variant variant;
                if (!(obj is Variant)) {
                    if (obj is ComArray) {
                        variant = new Variant((ComArray)obj);
                    }
                    else {
                        variant = Variant.MakeVariant(obj);
                    }

                }
                else {
                    variant = (Variant)obj;
                    // variant = new <see cref="Variant"/>((<see cref="Variant"/>)obj);
                }

                variants[i] = variant;
            }
            return Invoke(dispId, flag, new ComArray(variants, true), null, null);
        }

        /// <inheritdoc/>
        public Variant[] CallMethodA(int dispId, params object[] inparams) =>
            CallMethodA(dispId, inparams, DispatchFlags.DISPATCH_METHOD);

        /// <inheritdoc/>
        public void CallMethod(string name, object[] inparams, int[] dispIds) =>
            CallMethodA(GetIDsOfNames(name), inparams, dispIds);

        /// <inheritdoc/>
        public void CallMethod(int dispId, object[] inparams, int[] dispIds) =>
            CallMethodA(dispId, inparams, dispIds);

        /// <inheritdoc/>
        public Variant[] CallMethodA(string name, object[] inparams, int[] dispIds) =>
            CallMethodA(GetIDsOfNames(name), inparams, dispIds);

        /// <inheritdoc/>
        public Variant[] CallMethodA(int dispId, object[] inparams, int[] dispIds) {
            if (inparams == null || inparams.Length == 0) {
                return CallMethodA(dispId, inparams);
            }

            if (dispIds == null || dispIds.Length != inparams.Length) {
                throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_DISP_INCORRECT_PARAM_LENGTH));
            }

            var array = new int[inparams.Length];
            // now prepare the ComArray of dispIds.
            for (var i = 0; i < inparams.Length; i++) {
                array[i] = dispIds[i];
            }

            var arrayOfValues = new ComArray(array, true);

            var variants = new Variant[inparams.Length];
            for (var i = 0; i < inparams.Length; i++) {
                var obj = inparams[i];

                Variant variant;
                if (!(obj is Variant)) {
                    if (obj is ComArray) {
                        variant = new Variant((ComArray)obj);
                    }
                    else {
                        variant = Variant.MakeVariant(obj);
                    }
                }
                else {
                    variant = (Variant)obj;
                    // variant = new <see cref="Variant"/>((<see cref="Variant"/>)obj);
                }

                variants[i] = variant;
            }
            return Invoke(dispId, DispatchFlags.DISPATCH_METHOD,
                new ComArray(variants, true), arrayOfValues, null);
        }

        /// <inheritdoc/>
        public void CallMethod(string name, object[] inparams, string[] paramNames) =>
            CallMethodA(name, inparams, paramNames);

        /// <inheritdoc/>
        public Variant[] CallMethodA(string name, object[] inparams, string[] paramNames) {
            if (inparams == null || inparams.Length == 0) {
                return CallMethodA(GetIDsOfNames(name), inparams);
            }

            if (paramNames == null || paramNames.Length != inparams.Length) {
                throw new ArgumentException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_DISP_INCORRECT_PARAM_LENGTH));
            }

            var names = new string[paramNames.Length + 1];
            names[0] = name;
            Array.Copy(paramNames, 0, names, 1, paramNames.Length);
            var dispIds = GetIDsOfNames(names);

            var newDispIds = new int[dispIds.Length - 1];

            for (var i = 0; i < newDispIds.Length; i++) {
                newDispIds[i] = dispIds[i + 1]; // skip the apiname
            }

            return CallMethodA(dispIds[0], inparams, newDispIds);
        }

        /// <inheritdoc/>
        public void Put(int dispId, params object[] inparams) =>
            Put(dispId, inparams, false);

        /// <inheritdoc/>
        public void Put(string name, params object[] inparams) =>
            Put(GetIDsOfNames(name), inparams, false);

        /// <inheritdoc/>
        public void PutRef(int dispId, params object[] inparams) =>
            Put(dispId, inparams, true);

        /// <inheritdoc/>
        public void PutRef(string name, params object[] inparams) =>
            Put(GetIDsOfNames(name), inparams, true);

        /// <inheritdoc/>
        public override string ToString() =>
            "IDispatch [" + base.ToString() + "]";

        /// <inheritdoc/>
        public ExcepInfo LastExcepInfo { get; } = new ExcepInfo();

        /// <summary>
        /// Static initialization
        /// </summary>
        static DispatchImpl() {
            try {
                kExcepInfo.AddMember(typeof(short));
                kExcepInfo.AddMember(typeof(short));
                kExcepInfo.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
                kExcepInfo.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
                kExcepInfo.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
                kExcepInfo.AddMember(typeof(int));
                kExcepInfo.AddMember(new ComPointer(null, true));
                kExcepInfo.AddMember(new ComPointer(null, true));
                kExcepInfo.AddMember(typeof(int));
            }
            catch (InteropException e) {
                Log.Logger.Error(e, "DispatchImpl static initializer");
            }
        }

        private static readonly Struct kExcepInfo = new Struct();
        private readonly Dictionary<string, Dictionary<string, int>> _cacheOfDispIds = 
            new Dictionary<string, Dictionary<string, int>>();
    }
}