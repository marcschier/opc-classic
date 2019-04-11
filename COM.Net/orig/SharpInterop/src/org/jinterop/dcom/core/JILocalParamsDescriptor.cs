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

    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;


    /// <summary>
    ///<para>Provides a way to express parameters for a particular method. These are only <code>[in]</code>
    /// parameters, the <code>[out]</code> parameters are decided at the implementation level. If the <code>IDL</code>
    /// method being described by this class is returning multiple objects then use the return type of the implementation
    /// as an <code>Object[]</code>
    /// 
    /// </para>
    /// <para>
    /// For example:- <br>
    /// 
    /// IDL from Microsoft Internet Explorer is:- <br>
    /// <code>
    /// [id(0x000000fb), helpstring("A new, hidden, non-navigated WebBrowser window is needed.")] <br>
    ///    void NewWindow2(   [in, out] IDispatch** ppDisp,
    /// </para>
    ///                       [in, out] VARIANT_BOOL* Cancel); <para>
    /// </code>
    /// Corresponding <code>JILocalParamsDescriptor</code> would be :- <br>
    /// <code>
    ///         JILocalParamsDescriptor paramObject = new JILocalParamsDescriptor(); <br>
    ///         paramObject.addInParamAsObject(new JIPointer(IJIComObject.class,false), JIFlags.FLAG_NULL); <br>
    ///         paramObject.addInParamAsType(JIVariant.class,JIFlags.FLAG_NULL);<br>
    /// </code>
    /// and the Java implementation must return an <code>Object[]</code> in this case, for returning the 2 parameters back.
    /// </para>
    /// <para><i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.</i><br>
    /// 
    /// @since 2.0 (formerly JIParameterObject)
    /// 
    /// </para>
    /// </summary>
    [Serializable]
    public sealed class JILocalParamsDescriptor {

        private JICallBuilder CallObject = new JICallBuilder();
        private const long SerialVersionUID = -4274963180104543505L;

        /// <summary>
        /// @exclude </summary>
        /// <param name="ndr">
        /// @return </param>
        public object[] Read(NetworkDataRepresentation ndr) {
            CallObject.Read2(ndr);
            return CallObject.Results;
        }


        /// <summary>
        /// Add <code>[in]</code> parameter of the type <code>clazz</code> at the end of the out parameter list.
        /// </summary>
        /// <param name="clazz"> </param>
        /// <param name="FLAGS"> </param>
        public void AddInParamAsType(Type clazz, int FLAGS) {
            CallObject.AddOutParamAsType(clazz,FLAGS);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter at the end of the out parameter list. Typically callers are
        /// composite in nature <code>JIStruct</code> , <code>JIUnions</code> , <code>JIPointer</code>
        /// and <code>JIString</code> .
        /// </summary>
        /// <param name="param"> </param>
        /// <param name="FLAGS"> </param>
        public void AddInParamAsObject(object param, int FLAGS) {
            CallObject.AddOutParamAsObject(param,FLAGS);
        }

        /// <summary>
        /// @exclude </summary>
        /// <param name="params"> </param>
        /// <param name="FLAGS"> </param>
        public void SetInParams(object[] @params, int FLAGS) {
            CallObject.SetOutParams(@params,FLAGS);
        }

        /// <summary>
        ///Removes <code>[in]</code> parameter at the specified index from the parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="FLAGS"> from JIFlags (if need be). </param>
        public void RemoveInParamAt(int index, int FLAGS) {
            CallObject.RemoveOutParamAt(index,FLAGS);
        }

        /// <summary>
        /// @exclude
        /// @return
        /// </summary>
        public object[] InParams {
            get {
                return CallObject.OutParams;
            }
        }

        public JISession Session {
            set {
                CallObject.AttachSession(value);
            }
        }

    }

}