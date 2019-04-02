// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {


    /// <summary>
    /// Describe a method of the COM <code>IDL</code> to be used in Callback implementations.
    /// Framework uses java reflection to invoke methods requested by COM clients so it is
    /// absolutely essential that java methods in the implementation class conform exactly to
    /// what is described in this object.
    /// <para>
    /// <i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.</i>
    /// 
    /// @since 2.0 (formerly JIMethodDescriptor)
    /// </para>
    /// </summary>
    public sealed class JILocalMethodDescriptor
	{
        private JILocalParamsDescriptor parameters;

		/// <summary>
		///Creates the method descriptor. The method number is set by the order in which this instance is
		/// added to the <code>JILocalInterfaceDefinition</code>. This number is incremented by 1 for each subsequent
		/// and new addition into interface definition.
		/// </summary>
		/// <param name="methodName"> name of the method. </param>
		/// <param name="parameters"> pass <code>null</code> if the method has no parameters. </param>
		public JILocalMethodDescriptor(string methodName, JILocalParamsDescriptor parameters)
		{
			MethodName = methodName;
			ParameterObject = parameters;
		}

		/// <summary>
		/// Creates the method descriptor.
		/// </summary>
		/// <param name="methodName"> name of the method. </param>
		/// <param name="dispId"> <code>DISPID</code> of this method as in the <code>IDL</code> or the TypeLibrary. </param>
		/// <param name="parameters"> pass <code>null</code> if the method has no parameters. </param>
		public JILocalMethodDescriptor(string methodName, int dispId, JILocalParamsDescriptor parameters)
		{
			MethodName = methodName;
			MethodDispID = dispId;
			ParameterObject = parameters;
		}

        internal int MethodNum { set; get; } = -1;

        private JILocalParamsDescriptor ParameterObject {
            set {

                if (value == null) {
                    return;
                }

                parameters = value;
                var @params = value.InParams;
                InparametersAsClass = new Type[@params.Length];

                for (var i = 0; i < @params.Length; i++) {
                    var obj = @params[i];
                    if (obj is Type) {
                        var c = (Type)obj;

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
                        InparametersAsClass[i] = c;
                    }
                    else {
                        InparametersAsClass[i] = obj.GetType();
                    }
                }

            }
            get => parameters;
        }

        /// <summary>
        ///Returns the method name.
        /// 
        /// @return
        /// </summary>
        public string MethodName { get; } = null;


        /// <summary>
        ///Gets the <code>DISPID</code> of this method.
        /// 
        /// @return
        /// </summary>
        public int MethodDispID { get; } = -1;


        /// <summary>
        /// @exclude
        /// @return
        /// </summary>
        internal Type[] InparametersAsClass { get; private set; } = new Type[0];
    }
}