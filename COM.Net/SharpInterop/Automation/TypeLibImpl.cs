//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Automation {
    using SharpInterop.Core;
    using SharpInterop.Rpc.Core;
    using System;

    /// <summary>
    /// Type lib
    /// </summary>
    [Serializable]
    internal sealed class TypeLibImpl : ComObjectImplWrapper, ITypeLib {

        /// <summary>
        /// Create type lib
        /// </summary>
        /// <param name="comObject"></param>
        internal TypeLibImpl(IComObject comObject) :
            base(comObject) {
        }

        /// <inheritdoc/>
        public int TypeInfoCount {
            get {
                var callObject = new CallBuilder(true) {
                    Opnum = 0
                };
                callObject.AddOutParamAsType(typeof(int));
                var result = ComObject.Call(callObject);
                return (int)result[0];
            }
        }

        /// <inheritdoc/>
        public ITypeInfo GetTypeInfo(int index) {
            var callObject = new CallBuilder(true) {
                Opnum = 1
            };
            callObject.AddInParamAsInt(index);
            callObject.AddOutParamAsType(typeof(IComObject));
            var result = ComObject.Call(callObject);
            return (ITypeInfo)ObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <inheritdoc/>
        public int GetTypeInfoType(int index) {
            var callObject = new CallBuilder(true) {
                Opnum = 2
            };
            callObject.AddInParamAsInt(index);
            callObject.AddOutParamAsType(typeof(int));
            var result = ComObject.Call(callObject);
            return (int)result[0];
        }

        /// <inheritdoc/>
        public ITypeInfo GetTypeInfoOfGuid(string uuid) {
            var callObject = new CallBuilder(true) {
                Opnum = 3
            };
            callObject.AddInParamAsUUID(uuid);
            callObject.AddOutParamAsType(typeof(IComObject));
            var result = ComObject.Call(callObject);
            return (ITypeInfo)ObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <inheritdoc/>
        public void GetLibAttr() {
            var callObject = new CallBuilder(true) {
                Opnum = 4
            };

            var tlibattr = new Struct();
            tlibattr.AddMember(typeof(UUID));
            tlibattr.AddMember(typeof(int));
            tlibattr.AddMember(typeof(int));
            tlibattr.AddMember(typeof(short));
            tlibattr.AddMember(typeof(short));
            tlibattr.AddMember(typeof(short));

            callObject.AddOutParamAsObject(new ComPointer(tlibattr));
            callObject.AddOutParamAsType(typeof(int)); // CLEANUPSTORAGE
            var result = ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public object[] GetDocumentation(int memberId) {
            var callObject = new CallBuilder(true);
            callObject.AddInParamAsInt(memberId);
            callObject.AddInParamAsInt(0xb); // refPtrFlags, as per the oaidl.idl...
            callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
            callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
            callObject.AddOutParamAsObject(typeof(int));
            callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
            callObject.Opnum = 6;
            return ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public object[] FindName(ComString nameBuf, int hashValue, short found) {
            var callObject = new CallBuilder(true) {
                Opnum = 8
            };
            callObject.AddInParamAsString(nameBuf.String, nameBuf.Type);
            callObject.AddInParamAsInt(hashValue);
            callObject.AddInParamAsShort(found);

            callObject.AddOutParamAsObject(new ComArray(typeof(IComObject), null, 1, true, true));
            callObject.AddOutParamAsObject(new ComArray(typeof(int), null, 1, true, true));
            callObject.AddOutParamAsType(typeof(short));
            callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));

            return ComObject.Call(callObject);
        }
    }
}