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

    /// <summary>
    /// Creates automation related objects.
    /// Internal Factory, to be used only by the framework.
    /// </summary>
    public sealed class JIAutomationFactory {

        /// <summary>
        /// Narrow object
        /// </summary>
        /// <param name="comObject"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public static IComObject NarrowObject(IComObject comObject) {
            var retval = comObject;
            switch (comObject.InterfaceIdentifier.ToUpper()) {
                case Interfaces.IID_IDispatch:
                    return new JIDispatchImpl(retval);
                case Interfaces.IID_ITypeInfo:
                    return new JITypeInfoImpl(retval);
                case Interfaces.IID_ITypeLib:
                    return new JITypeLibImpl(retval);
                case Interfaces.IID_IEnumVARIANT:
                    return new JIEnumVARIANTImpl(retval);
                default:
                    return comObject;
            }
        }
    }
}