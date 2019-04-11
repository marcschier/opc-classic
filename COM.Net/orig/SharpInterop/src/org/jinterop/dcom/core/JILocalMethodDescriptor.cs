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

namespace org.jinterop.dcom.core {


    /// <summary>
    /// Describe a method of the COM <code>IDL</code> to be used in Callback implementations.
    /// Framework uses java reflection to invoke methods requested by COM clients so it is
    /// absolutely essential that java methods in the implementation class conform exactly to
    /// what is described in this object.
    /// <para>
    /// <i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.</i><br>
    /// 
    /// @since 2.0 (formerly JIMethodDescriptor)
    /// </para>
    /// </summary>
    public sealed class JILocalMethodDescriptor {
        private string MethodName_Renamed = null;
        private int MethodNum_Renamed = -1;
        private int DispId = -1;
        private Type[] InparametersAsClass_Renamed = new Type[0];
        private JILocalParamsDescriptor Parameters = null;

        /// <summary>
        ///Creates the method descriptor. The method number is set by the order in which this instance is
        /// added to the <code>JILocalInterfaceDefinition</code>. This number is incremented by 1 for each subsequent
        /// and new addition into interface definition.
        /// </summary>
        /// <param name="methodName"> name of the method. </param>
        /// <param name="parameters"> pass <code>null</code> if the method has no parameters. </param>
        public JILocalMethodDescriptor(string methodName, JILocalParamsDescriptor parameters) {
            this.MethodName_Renamed = methodName;
            ParameterObject = parameters;
        }

        /// <summary>
        /// Creates the method descriptor.
        /// </summary>
        /// <param name="methodName"> name of the method. </param>
        /// <param name="dispId"> <code>DISPID</code> of this method as in the <code>IDL</code> or the TypeLibrary. </param>
        /// <param name="parameters"> pass <code>null</code> if the method has no parameters. </param>
        public JILocalMethodDescriptor(string methodName, int dispId, JILocalParamsDescriptor parameters) {
            this.MethodName_Renamed = methodName;
            this.DispId = dispId;
            ParameterObject = parameters;
        }

        public int MethodNum {
            set {
                this.MethodNum_Renamed = value;
            }
            get {
                return MethodNum_Renamed;
            }
        }

        private JILocalParamsDescriptor ParameterObject {
            set {
    
                if (value == null) {
                    return;
                }
    
                this.Parameters = value;
                object[] @params = value.InParams;
                InparametersAsClass_Renamed = new Type[@params.Length];
    
                for (int i = 0; i < @params.Length; i++) {
                    object obj = @params[i];
                    if (obj is Type) {
                        Type c = (Type)obj;
    
                        {
                            //get the primitive members here
                            if (c.Equals(typeof(bool?))) {
                                c = typeof(bool);
                            }
                            else if (c.Equals(typeof(char?))) {
                                c = typeof(char);
                            }
                            else if (c.Equals(typeof(sbyte?))) {
                                c = typeof(sbyte);
                            }
                            else if (c.Equals(typeof(short?))) {
                                c = typeof(short);
                            }
                            else if (c.Equals(typeof(int?))) {
                                c = typeof(int);
                            }
                            else if (c.Equals(typeof(long?))) {
                                c = typeof(long);
                            }
                            else if (c.Equals(typeof(float?))) {
                                c = typeof(float);
                            }
                            else if (c.Equals(typeof(double?))) {
                                c = typeof(double);
                            }
                            else if (c.Equals(typeof(Void))) {
                                c = typeof(void);
                            }
                        }
                        InparametersAsClass_Renamed[i] = c;
                    }
                    else {
                        InparametersAsClass_Renamed[i] = obj.GetType();
                    }
                }
    
            }
            get {
                return Parameters;
            }
        }

        /// <summary>
        ///Returns the method name.
        /// 
        /// @return
        /// </summary>
        public string MethodName {
            get {
                return MethodName_Renamed;
            }
        }


        /// <summary>
        ///Gets the <code>DISPID</code> of this method.
        /// 
        /// @return
        /// </summary>
        public int MethodDispID {
            get {
                return DispId;
            }
        }


        /// <summary>
        /// @exclude
        /// @return
        /// </summary>
        public Type[] InparametersAsClass {
            get {
                return InparametersAsClass_Renamed;
            }
        }
    }
}