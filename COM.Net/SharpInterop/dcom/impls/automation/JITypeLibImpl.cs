//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.core;
    using rpc.core;
    using System;

    /// <summary>
    /// Type lib
    /// </summary>
    [Serializable]
    internal sealed class JITypeLibImpl : JIComObjectImplWrapper, IJITypeLib {

        /// <summary>
        /// Create type lib
        /// </summary>
        /// <param name="comObject"></param>
        internal JITypeLibImpl(IComObject comObject) :
            base(comObject) {
        }

        /// <inheritdoc/>
        public int TypeInfoCount {
            get {
                var callObject = new JICallBuilder(true) {
                    Opnum = 0
                };
                callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
                var result = ComObject.Call(callObject);
                return (int)result[0];
            }
        }

        /// <inheritdoc/>
        public IJITypeInfo GetTypeInfo(int index) {
            var callObject = new JICallBuilder(true) {
                Opnum = 1
            };
            callObject.AddInParamAsInt(index, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IComObject), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return (IJITypeInfo)JIObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <inheritdoc/>
        public int GetTypeInfoType(int index) {
            var callObject = new JICallBuilder(true) {
                Opnum = 2
            };
            callObject.AddInParamAsInt(index, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return (int)result[0];
        }

        /// <inheritdoc/>
        public IJITypeInfo GetTypeInfoOfGuid(string uuid) {
            var callObject = new JICallBuilder(true) {
                Opnum = 3
            };
            callObject.AddInParamAsUUID(uuid, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IComObject), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return (IJITypeInfo)JIObjectFactory.NarrowObject((IComObject)result[0]);
        }

        /// <inheritdoc/>
        public void GetLibAttr() {
            var callObject = new JICallBuilder(true) {
                Opnum = 4
            };

            var tlibattr = new JIStruct();
            tlibattr.AddMember(typeof(UUID));
            tlibattr.AddMember(typeof(int));
            tlibattr.AddMember(typeof(int));
            tlibattr.AddMember(typeof(short));
            tlibattr.AddMember(typeof(short));
            tlibattr.AddMember(typeof(short));

            callObject.AddOutParamAsObject(new JIPointer(tlibattr), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL); // CLEANUPSTORAGE
            var result = ComObject.Call(callObject);
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
            callObject.Opnum = 6;
            return ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public object[] FindName(JIString nameBuf, int hashValue, short found) {
            var callObject = new JICallBuilder(true) {
                Opnum = 8
            };
            callObject.AddInParamAsString(nameBuf.String, nameBuf.Type);
            callObject.AddInParamAsInt(hashValue, JIFlags.FLAG_NULL);
            callObject.AddInParamAsShort(found, JIFlags.FLAG_NULL);

            callObject.AddOutParamAsObject(new JIArray(typeof(IComObject), null, 1, true, true), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIArray(typeof(int), null, 1, true, true), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(short), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_NULL);

            return ComObject.Call(callObject);
        }
    }
}