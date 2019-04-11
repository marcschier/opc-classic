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
    using System;

    /// <summary>
    /// Type info
    /// </summary>
    [Serializable]
    internal sealed class JITypeInfoImpl : JIComObjectImplWrapper, IJITypeInfo {

        /// <summary>
        /// Create implementation
        /// </summary>
        /// <param name="comObject"></param>
        internal JITypeInfoImpl(IComObject comObject) :
            base(comObject) {
        }

        /// <inheritdoc/>
        public FuncDesc GetFuncDesc(int index) {
            var obj = new JICallBuilder(true) {
                Opnum = 2
            };
            obj.AddInParamAsInt(index, JIFlags.FLAG_NULL);

            // now to prepare out params
            var funcDescStruct = new JIStruct();
            funcDescStruct.AddMember(typeof(int));
            funcDescStruct.AddMember(new JIPointer(new JIArray(typeof(int), null, 1, true)));
            // first read the pointer representation. Do not want to use funcdesc but only describe
            // it. This should show the flexibility of the API.
            // TODO have to make a Pointer type which only reads the representation.
            obj.AddOutParamAsObject(new JIPointer(funcDescStruct), JIFlags.FLAG_NULL);

            // CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
            // come null and even if something comes, I don't know which pointer PVOID stands for.
            var cleanlocalstorage = new JIStruct();
            cleanlocalstorage.AddMember(typeof(int));
            cleanlocalstorage.AddMember(typeof(int));
            cleanlocalstorage.AddMember(typeof(int));
            obj.AddOutParamAsObject(new JIPointer(cleanlocalstorage), JIFlags.FLAG_NULL);

            // now for member id
            // obj.addOutParamAsType(Integer.class,JIFlags.FLAG_NULL);
            // now for lprgscode, Pointer to Conformant array of SCODEs (int)
            // obj.addOutParamAsObject(new Pointer(new JIArray(Integer.class,null,1,true)), JIFlags.FLAG_NULL);
            // now for lprgelemdescParam, Pointer to Conformant array of ELEMDESC (struct)
            // define the struct
            var elemDesc = new JIStruct();

            // SAFEARRAYBOUNDS
            var safeArrayBounds = new JIStruct();
            safeArrayBounds.AddMember(typeof(int));
            safeArrayBounds.AddMember(typeof(int));

            // arraydesc
            var arrayDesc = new JIStruct();
            // typedesc
            var typeDesc = new JIStruct();

            arrayDesc.AddMember(typeDesc);
            arrayDesc.AddMember(typeof(short));
            arrayDesc.AddMember(new JIArray(safeArrayBounds, new int[] { 1 }, 1, true));

            var forTypeDesc = new JIUnion(typeof(short));
            var ptrToTypeDesc = new JIPointer(typeDesc);
            var ptrToArrayDesc = new JIPointer(arrayDesc);

            forTypeDesc.AddMember(TypeDesc.VT_PTR, ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY, ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_CARRAY, ptrToArrayDesc);
            forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED, typeof(int));
            typeDesc.AddMember(forTypeDesc);
            typeDesc.AddMember(typeof(short)); // VARTYPE

            // PARAMDESC
            var paramDesc2 = new JIStruct();
            paramDesc2.AddMember(typeof(int));
            paramDesc2.AddMember(typeof(JIVariant));
            var paramDesc = new JIStruct();
            paramDesc.AddMember(new JIPointer(paramDesc2, false));
            paramDesc.AddMember(typeof(short));

            elemDesc.AddMember(typeDesc);
            elemDesc.AddMember(paramDesc);

            funcDescStruct.AddMember(new JIPointer(new JIArray(elemDesc, null, 1, true)));
            // obj.addOutParamAsObject(new Pointer(new JIArray(elemDesc,null,1,true)), JIFlags.FLAG_NULL);

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

            funcDescStruct.AddMember(typeof(int));
            funcDescStruct.AddMember(typeof(int));
            funcDescStruct.AddMember(typeof(int));

            funcDescStruct.AddMember(typeof(short));
            funcDescStruct.AddMember(typeof(short));

            funcDescStruct.AddMember(typeof(short));
            funcDescStruct.AddMember(typeof(short));

            funcDescStruct.AddMember(elemDesc);
            funcDescStruct.AddMember(typeof(short));


            var result = ComObject.Call(obj);
            var funcDesc = new FuncDesc((JIPointer)result[0]);
            return funcDesc;
        }

        /// <inheritdoc/>
        public TypeAttr TypeAttr {
            get {
                var obj = new JICallBuilder(true) {
                    Opnum = 0
                };

                var typeAttr = new JIStruct();
                var mainPtr = new JIPointer(typeAttr);
                obj.AddOutParamAsObject(mainPtr, JIFlags.FLAG_NULL);

                // CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
                // come null and even if something comes, I don't know which pointer PVOID stands for.
                obj.AddOutParamAsObject(new JIPointer(typeof(int)), JIFlags.FLAG_NULL);

                typeAttr.AddMember(typeof(UUID));
                typeAttr.AddMember(typeof(int));
                typeAttr.AddMember(typeof(int));

                typeAttr.AddMember(typeof(int));
                typeAttr.AddMember(typeof(int));

                typeAttr.AddMember(new JIPointer(
                    new JIString(null, JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));

                typeAttr.AddMember(typeof(int));
                typeAttr.AddMember(typeof(int));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));
                typeAttr.AddMember(typeof(short));

                var typeDesc = new JIStruct();
                var arrayDesc = new JIStruct();
                var safeArrayBounds = new JIStruct();

                safeArrayBounds.AddMember(typeof(int));
                safeArrayBounds.AddMember(typeof(int));

                arrayDesc.AddMember(typeDesc);
                arrayDesc.AddMember(typeof(short));
                arrayDesc.AddMember(new JIArray(safeArrayBounds, new int[] { 1 }, 1, true));

                var forTypeDesc = new JIUnion(typeof(short));
                var ptrToTypeDesc = new JIPointer(typeDesc);
                var ptrToArrayDesc = new JIPointer(arrayDesc);

                forTypeDesc.AddMember(TypeDesc.VT_PTR, ptrToTypeDesc);
                forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY, ptrToTypeDesc);
                forTypeDesc.AddMember(TypeDesc.VT_CARRAY, ptrToArrayDesc);
                forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED, typeof(int));
                typeDesc.AddMember(forTypeDesc);
                typeDesc.AddMember(typeof(short)); // VARTYPE

                typeAttr.AddMember(typeDesc);
                var paramDesc = new JIStruct();
                paramDesc.AddMember(new JIPointer(typeof(JIVariant), false));
                paramDesc.AddMember(typeof(short));

                typeAttr.AddMember(paramDesc);
                var result = ComObject.Call(obj);
                var attr = new TypeAttr((JIPointer)result[0]);
                return attr;
            }
        }

        /// <inheritdoc/>
        public object[] ContainingTypeLib {
            get {
                var callObject = new JICallBuilder(true);
                callObject.AddOutParamAsObject(typeof(IComObject), JIFlags.FLAG_NULL);
                callObject.AddOutParamAsObject(typeof(int), JIFlags.FLAG_NULL);
                callObject.Opnum = 15;
                var result = ComObject.Call(callObject);
                var retVal = new object[2];
                retVal[0] = (IJITypeLib)JIObjectFactory.NarrowObject((IComObject)result[0]);
                retVal[1] = result[1];
                return retVal;
            }
        }

        /// <inheritdoc/>
        public object[] GetDllEntry(int memberId, int invKind) {
            if (invKind != (int)InvokeKind.INVOKE_FUNC &&
                invKind != (int)InvokeKind.INVOKE_PROPERTYGET &&
                invKind != (int)InvokeKind.INVOKE_PROPERTYPUTREF &&
                invKind != (int)InvokeKind.INVOKE_PROPERTYPUT) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.E_INVALIDARG));
            }
            var callObject = new JICallBuilder(true);
            callObject.AddInParamAsInt(memberId, JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(invKind, JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(1, JIFlags.FLAG_NULL); // refPtrFlags, as per the oaidl.idl...
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(typeof(short), JIFlags.FLAG_NULL);
            callObject.Opnum = 10;
            return ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public object[] GetDocumentation(int memberId) {
            var callObject = new JICallBuilder(true);
            callObject.AddInParamAsInt(memberId, JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(0xb, JIFlags.FLAG_NULL); // refPtrFlags, as per the oaidl.idl...
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(typeof(int), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);
            callObject.Opnum = 9;
            return ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public VarDesc GetVarDesc(int index) {
            var callObject = new JICallBuilder(true) {
                Opnum = 3
            };
            callObject.AddInParamAsInt(index, JIFlags.FLAG_NULL);

            // now build the vardesc
            var vardesc = new JIStruct();
            callObject.AddOutParamAsObject(new JIPointer(vardesc), JIFlags.FLAG_NULL);
            // CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
            // come null and even if something comes, I don't know which pointer PVOID stands for.
            var cleanlocalstorage = new JIStruct();
            cleanlocalstorage.AddMember(typeof(int));
            cleanlocalstorage.AddMember(typeof(int));
            cleanlocalstorage.AddMember(typeof(int));
            callObject.AddOutParamAsObject(new JIPointer(cleanlocalstorage), JIFlags.FLAG_NULL);

            vardesc.AddMember(typeof(int)); // memberid
            vardesc.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));

            var union = new JIUnion(typeof(int));
            union.AddMember(VarDesc.VAR_PERINSTANCE, typeof(int));
            union.AddMember(VarDesc.VAR_DISPATCH, typeof(int));
            union.AddMember(VarDesc.VAR_STATIC, typeof(int));
            union.AddMember(VarDesc.VAR_CONST, typeof(JIVariant));
            vardesc.AddMember(union);

            var elemDesc = new JIStruct();

            // SAFEARRAYBOUNDS
            var safeArrayBounds = new JIStruct();
            safeArrayBounds.AddMember(typeof(int));
            safeArrayBounds.AddMember(typeof(int));

            // arraydesc
            var arrayDesc = new JIStruct();
            // typedesc
            var typeDesc = new JIStruct();

            arrayDesc.AddMember(typeDesc);
            arrayDesc.AddMember(typeof(short));
            arrayDesc.AddMember(new JIArray(safeArrayBounds, new int[] { 1 }, 1, true));

            var forTypeDesc = new JIUnion(typeof(short));
            var ptrToTypeDesc = new JIPointer(typeDesc);
            var ptrToArrayDesc = new JIPointer(arrayDesc);

            forTypeDesc.AddMember(TypeDesc.VT_PTR, ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY, ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_CARRAY, ptrToArrayDesc);
            forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED, typeof(int));
            typeDesc.AddMember(forTypeDesc);
            typeDesc.AddMember(typeof(short)); // VARTYPE

            // PARAMDESC
            var paramDesc2 = new JIStruct();
            paramDesc2.AddMember(typeof(int));
            paramDesc2.AddMember(typeof(JIVariant));
            var paramDesc = new JIStruct();
            paramDesc.AddMember(new JIPointer(paramDesc2, false));
            paramDesc.AddMember(typeof(short));
            //        JIStruct paramDesc = new JIStruct();
            //        paramDesc.addMember(new JIPointer(JIVariant.class,false));
            //        // paramDesc.addMember(JIVariant.class);
            //        paramDesc.addMember(Short.class);

            elemDesc.AddMember(typeDesc);
            elemDesc.AddMember(paramDesc);

            vardesc.AddMember(elemDesc);
            vardesc.AddMember(typeof(short));
            vardesc.AddMember(typeof(int));

            var result = ComObject.Call(callObject);

            return new VarDesc((JIPointer)result[0]);

        }

        /// <inheritdoc/>
        public object[] GetNames(int memberId, int maxNames) {
            var callObject = new JICallBuilder(true) {
                Opnum = 4
            };
            callObject.AddInParamAsInt(memberId, JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(maxNames, JIFlags.FLAG_NULL);

            callObject.AddOutParamAsObject(new JIArray(
                new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), null, 1, true, true), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);

            return ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public int GetRefTypeOfImplType(int index) {
            var callObject = new JICallBuilder(true) {
                Opnum = 5
            };
            callObject.AddInParamAsInt(index, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            return (int)ComObject.Call(callObject)[0];
        }

        /// <inheritdoc/>
        public int GetImplTypeFlags(int index) {
            var callObject = new JICallBuilder(true) {
                Opnum = 6
            };
            callObject.AddInParamAsInt(index, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            return (int)ComObject.Call(callObject)[0];
        }

        /// <inheritdoc/>
        public IJITypeInfo GetRefTypeInfo(int hrefType) {
            var callObject = new JICallBuilder(true) {
                Opnum = 11
            };
            callObject.AddInParamAsInt(hrefType, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IComObject), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return (IJITypeInfo)JIObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <inheritdoc/>
        public IComObject CreateInstance(string riid) {
            var callObject = new JICallBuilder(true) {
                Opnum = 13
            };
            callObject.AddInParamAsUUID(riid, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IComObject), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return JIObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <inheritdoc/>
        public JIString GetMops(int memberId) {
            var callObject = new JICallBuilder(true) {
                Opnum = 14
            };
            callObject.AddInParamAsInt(memberId, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return (JIString)result[0];
        }
    }
}