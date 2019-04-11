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

namespace org.jinterop.dcom.common {

    /// <summary>
    /// Framework Internal class.
    /// 
    /// @exclude
    /// <para>Internally used class from JICallBuilder, since the read(), write() do not throw
    /// exceptions. The IJIComObject call or QI or any other APIs will always throw checked JIException
    /// </para>
    /// </summary>
    public sealed class JIRuntimeException : Exception {


        private const long SerialVersionUID = 4972599190342284084L;
        private object[] Parameters_Renamed = null;
        private int Hresult = 0;
        public JIRuntimeException(int hresult) {
            //error code
            this.Hresult = hresult;
        }

        public JIRuntimeException(int hresult, object[] parameters) {
            //error code
            this.Hresult = hresult;
            this.Parameters_Renamed = parameters;
        }

        public int HResult {
            get {
                return Hresult;
            }
        }

        public object[] Parameters {
            get {
                return Parameters_Renamed;
            }
        }

        public string Message {
            get {
                return JISystem.GetLocalizedMessage(Hresult);
            }
        }
    }

}