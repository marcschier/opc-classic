// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.common {
    using System;

    /// <summary>
    /// Framework Internal class.
    /// </summary>
    /// <remarks>Internally used class from JICallBuilder, 
    /// since the read(), write() do not throw
    /// exceptions. The IJIComObject call or QI or any other APIs
    /// will always throw checked JIException
    /// </remarks>
    public sealed class JIRuntimeException : Exception {

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="hresult"></param>
        public JIRuntimeException(int hresult) {
            HResult = hresult;
        }

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="hresult"></param>
        /// <param name="parameters"></param>
		public JIRuntimeException(int hresult, object[] parameters) : 
            this(hresult) {
            Parameters = parameters;
        }

        /// <summary>
        /// Params
        /// </summary>
        public object[] Parameters { get; } = null;

        /// <summary>
        /// Get message
        /// </summary>
        public override string Message => JISystem.getLocalizedMessage(HResult);
    }

}