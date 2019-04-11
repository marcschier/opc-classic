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

    /// <summary>
    /// Enum variant
    /// </summary>
    internal sealed class JIEnumVARIANTImpl : JIComObjectImplWrapper, IJIEnumVariant {

        /// <summary>
        /// Create implementation
        /// </summary>
        /// <param name="comObject"></param>
        internal JIEnumVARIANTImpl(IJIComObject comObject) : base(comObject) {
        }

        /// <inheritdoc/>
        public object[] Next(int celt) {
            var callObject = new JICallBuilder(true) {
                Opnum = 0
            };
            callObject.AddInParamAsInt(celt, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(
                new JIArray(typeof(JIVariant), null, 1, true, true), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return result;
        }

        /// <inheritdoc/>
        public void Skip(int celt) {
            var callObject = new JICallBuilder(true) {
                Opnum = 1
            };
            callObject.AddInParamAsInt(celt, JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public void Reset() {
            var callObject = new JICallBuilder(true) {
                Opnum = 2
            };
            var result = ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public IJIEnumVariant Clone() {
            var callObject = new JICallBuilder(true) {
                Opnum = 3
            };
            callObject.AddOutParamAsObject(typeof(IJIComObject), JIFlags.FLAG_NULL);
            var result = ComObject.Call(callObject);
            return (IJIEnumVariant)JIObjectFactory.NarrowObject((IJIComObject)result[0]);
        }
    }
}