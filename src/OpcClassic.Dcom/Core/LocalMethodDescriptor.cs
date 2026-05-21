//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using System;

    /// <summary>
    /// Describe a method of the COM <code>IDL</code> to be used in Callback
    /// implementations.
    /// Framework uses reflection to invoke methods requested by COM clients
    /// so it is absolutely essential that java methods in the implementation
    /// class conform exactly to what is described in this object.
    /// <i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl,
    /// SampleTestServer and MSShell examples for more details on how to use
    /// this class.</i>
    /// </summary>
    public sealed class LocalMethodDescriptor {

        /// <summary>
        /// Method number
        /// </summary>
        internal int MethodNum { set; get; } = -1;

        /// <summary>
        /// Returns the method name.
        /// </summary>
        public string MethodName { get; } = null;

        /// <summary>
        /// Gets the <code>DISPID</code> of this method.
        /// </summary>
        public int MethodDispID { get; } = -1;

        /// <summary>
        /// Param object
        /// </summary>
        internal LocalParamsDescriptor ParameterObject {
            get => _parameters;
            set {
                if (value == null) {
                    return;
                }
                _parameters = value;
                var parameters = value.InParams;
                InparametersAsType = new Type[parameters.Length];
                for (var i = 0; i < parameters.Length; i++) {
                    var obj = parameters[i];
                    if (obj is Type c) {
                        InparametersAsType[i] = c;
                    }
                    else {
                        InparametersAsType[i] = obj.GetType();
                    }
                }
            }
        }

        /// <summary>
        /// In params
        /// </summary>
        internal Type[] InparametersAsType { get; private set; } = new Type[0];

        /// <summary>
        /// Creates the method descriptor. The method number is set by the order in 
        /// which this instance is <see cref="LocalInterfaceDefinition"/>.
        /// This number is incremented by 1 for each subsequent and new addition 
        /// into interface definition.
        /// </summary>
        /// <param name="methodName"> name of the method. </param>
        /// <param name="parameters"> pass <code>null</code> if the 
        /// method has no parameters. </param>
        public LocalMethodDescriptor(string methodName,
            LocalParamsDescriptor parameters) {
            MethodName = methodName;
            ParameterObject = parameters;
        }

        /// <summary>
        /// Creates the method descriptor.
        /// </summary>
        /// <param name="methodName"> name of the method. </param>
        /// <param name="dispId"> <code>DISPID</code> of this method as in the
        /// <code>IDL</code> or the TypeLibrary. </param>
        /// <param name="parameters"> pass <code>null</code> if the method has
        /// no parameters. </param>
        public LocalMethodDescriptor(string methodName, int dispId,
            LocalParamsDescriptor parameters) {
            MethodName = methodName;
            MethodDispID = dispId;
            ParameterObject = parameters;
        }

        private LocalParamsDescriptor _parameters;
    }
}