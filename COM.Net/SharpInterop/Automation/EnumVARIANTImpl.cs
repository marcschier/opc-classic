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

    /// <summary>
    /// Enum variant
    /// </summary>
    internal sealed class EnumVARIANTImpl : ComObjectImplWrapper, IEnumVariant {

        /// <summary>
        /// Create implementation
        /// </summary>
        /// <param name="comObject"></param>
        internal EnumVARIANTImpl(IComObject comObject) : base(comObject) {
        }

        /// <inheritdoc/>
        public object[] Next(int celt) {
            var callObject = new CallBuilder(true) {
                Opnum = 0
            };
            callObject.AddInParamAsInt(celt);
            callObject.AddOutParamAsObject(
                new ComArray(typeof(Variant), null, 1, true, true));
            callObject.AddOutParamAsType(typeof(int));
            var result = ComObject.Call(callObject);
            return result;
        }

        /// <inheritdoc/>
        public void Skip(int celt) {
            var callObject = new CallBuilder(true) {
                Opnum = 1
            };
            callObject.AddInParamAsInt(celt);
            var result = ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public void Reset() {
            var callObject = new CallBuilder(true) {
                Opnum = 2
            };
            var result = ComObject.Call(callObject);
        }

        /// <inheritdoc/>
        public IEnumVariant Clone() {
            var callObject = new CallBuilder(true) {
                Opnum = 3
            };
            callObject.AddOutParamAsObject(typeof(IComObject));
            var result = ComObject.Call(callObject);
            return (IEnumVariant)ObjectFactory.NarrowObject((IComObject)result[0]);
        }
    }
}