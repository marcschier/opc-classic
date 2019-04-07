//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using System;
    using SharpCifs.Dcerpc.Ndr;

    /// <summary>
    /// Provides a way to express parameters for a particular method. These are only <code>[in]</code>
    /// parameters, the <code>[out]</code> parameters are decided at the implementation level. If the <code>IDL</code>
    /// method being described by this class is returning multiple objects then use the return type of the implementation
    /// as an <code>Object[]</code>
    /// For example:-
    ///
    /// IDL from Microsoft Internet Explorer is:-
    /// <code>
    /// [id(0x000000fb), helpstring("A new, hidden, non-navigated WebBrowser window is needed.")]
    ///    void NewWindow2(   [in, out] IDispatch** ppDisp,
    ///                       [in, out] VARIANT_BOOL* Cancel);
    /// </code>
    /// Corresponding <code>JILocalParamsDescriptor</code> would be :-
    /// <code>
    /// 		JILocalParamsDescriptor paramObject = new JILocalParamsDescriptor();
    /// 		paramObject.addInParamAsObject(new JIPointer(IJIComObject.class,false), JIFlags.FLAG_NULL);
    /// 		paramObject.addInParamAsType(JIVariant.class,JIFlags.FLAG_NULL);
    /// </code>
    /// and the Java implementation must return an <code>Object[]</code> in this case, for returning the 2 parameters back.
    /// <para><i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.</i></para>
    /// </summary>
    [Serializable]
    public sealed class JILocalParamsDescriptor {

        /// <summary>
        /// Parameters
        /// </summary>
        internal object[] InParams => _callObject.OutParams;

        internal JISession Session {
            set => _callObject.AttachSession(value);
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal object[] Read(NdrCodec ndr) {
            _callObject.Read2(ndr);
            return _callObject.Results;
        }

        /// <summary>
        /// Add <code>[in]</code> parameter of the type <code>clazz</code> at the end of the out parameter list.
        /// </summary>
        /// <param name="clazz"> </param>
        /// <param name="flags"> </param>
        public void AddInParamAsType(Type clazz, int flags) {
            _callObject.addOutParamAsType(clazz, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter at the end of the out parameter list. Typically callers are
        /// composite in nature <code>JIStruct</code> , <code>JIUnions</code> , <code>JIPointer</code>
        /// and <code>JIString</code> .
        /// </summary>
        /// <param name="param"> </param>
        /// <param name="flags"> </param>
        public void AddInParamAsObject(object param, int flags) {
            _callObject.addOutParamAsObject(param, flags);
        }

        /// <summary>
        /// set params
        /// </summary>
        /// <param name="params"> </param>
        /// <param name="flags"> </param>
        internal void SetInParams(object[] @params, int flags) {
            _callObject.setOutParams(@params, flags);
        }

        /// <summary>
        /// Removes <code>[in]</code> parameter at the specified index from the parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public void RemoveInParamAt(int index, int flags) {
            _callObject.removeOutParamAt(index, flags);
        }

        private readonly JICallBuilder _callObject = new JICallBuilder();
    }
}