//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using SharpInterop.Common;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Forms the definition of a COM interface to be used in callbacks.
    /// Method overloads are <b>not</b> allowed.
    /// <i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.</i>
    /// </summary>
    [Serializable]
    public sealed class LocalInterfaceDefinition {

        /// <summary>
        /// Returns status whether this interface supports
        /// <code>IDispatch</code> or not.
        /// </summary>
        public bool DispInterface { get; } = true;

        /// <summary>
        /// Instance
        /// </summary>
        internal object Instance { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        internal Type Type { get; set; }

#pragma warning disable RECS0154 // Parameter is never used
        /// <summary>
        /// Creates an Interface definition. By default, the
        /// <code>dispinterface</code> property is <code>true</code>.
        /// </summary>
        /// <param name="interfaceIdentifier"> <code>IID</code> of the
        /// COM interface being implemented. </param>
        public LocalInterfaceDefinition(string interfaceIdentifier) =>
#pragma warning restore RECS0154 // Parameter is never used
            InterfaceIdentifier = interfaceIdentifier;

        /// <summary>
        /// Creates an Interface definition. Set <code>isDispInterface</code>
        /// interface to <code>false</code>
        /// if this interface does not support <code>IDispatch</code> based calls.
        /// </summary>
        /// <param name="interfaceIdentifier">  <code>IID</code> of the COM
        /// interface being implemented. </param>
        /// <param name="isDispInterface"> <code>true</code> if
        /// <code>IDispatch</code> ("<code>dispinterface</code>")
        /// is supported, <code>false</code> otherwise. </param>
        public LocalInterfaceDefinition(string interfaceIdentifier,
            bool isDispInterface) {
            InterfaceIdentifier = interfaceIdentifier;
            DispInterface = isDispInterface;
        }

        /// <summary>
        /// Adds a Method Descriptor. Methods <b>must</b> be added in
        /// the same order as they appear in the IDL.
        /// Please note that overloaded methods are not allowed.
        /// </summary>
        /// <param name="methodDescriptor"> </param>
        /// <exception cref="ArgumentException"> if a method by the
        /// same name already exists. </exception>
        public void AddMethodDescriptor(LocalMethodDescriptor methodDescriptor) {
            if (_nameVsMethodInfo.Contains(methodDescriptor.MethodName)) {
                throw new ArgumentException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_CALLBACK_OVERLOADS_NOTALLOWED));
            }
            methodDescriptor.MethodNum = _nextNum;
            _nextNum++;
            _opnumVsMethodInfo.AddOrUpdate(methodDescriptor.MethodNum, methodDescriptor);
            if (DispInterface) {
                if (methodDescriptor.MethodDispID == -1) {
                    throw new ArgumentException(Interop.GetLocalizedMessage(
                        ErrorCode.INTEROP_METHODDESC_DISPID_MISSING));
                }
                _dispIdVsMethodInfo.AddOrUpdate(methodDescriptor.MethodDispID, methodDescriptor);
            }
            _nameVsMethodInfo.AddOrUpdate(methodDescriptor.MethodName, methodDescriptor);
        }

        /// <summary>
        /// Returns the method descriptor identified by it's number.
        /// </summary>
        /// <param name="opnum"> </param>
        /// <returns> <code>null</code> if no method by this
        /// <code>opnum</code> was found. </returns>
        public LocalMethodDescriptor GetMethodDescriptor(int opnum) =>
            _opnumVsMethodInfo.GetOrDefault(opnum);

        /// <summary>
        ///Returns the method descriptor identified by it's dispId.
        /// </summary>
        /// <param name="dispId"> </param>
        /// <returns> <code>null</code> if no method by this
        /// <code>dispId</code> was found. </returns>
        public LocalMethodDescriptor GetMethodDescriptorForDispId(int dispId) =>
            _dispIdVsMethodInfo.GetOrDefault(dispId);


        /// <summary>
        ///Returns the method descriptor identified by it's name.
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> <code>null</code> if no method by this
        /// <code>name</code> was found. </returns>
        public LocalMethodDescriptor GetMethodDescriptor(string name) =>
            _nameVsMethodInfo.GetOrDefault(name);

        /// <summary>
        /// Returns all method descriptors.
        /// </summary>
        public LocalMethodDescriptor[] MethodDescriptors =>
            _opnumVsMethodInfo.Values.ToArray();

        /// <summary>
        /// Returns the interface identifier (<code>IID</code>) of this definition.
        /// </summary>
        public string InterfaceIdentifier { get; }

        /// <summary>
        /// Removes the method descriptor identified by it's number.
        /// </summary>
        /// <remarks>
        /// Please note that removal of a sequential method can have
        /// unpredictable results during a call.
        /// </remarks>
        /// <param name="opnum"> </param>
        /// <seealso cref="AddMethodDescriptor(LocalMethodDescriptor)"></seealso>
        public void RemoveMethodDescriptor(int opnum) {
            var methodDescriptor = _opnumVsMethodInfo.GetAndRemove(opnum);
            if (methodDescriptor != null) {
                _nameVsMethodInfo.Remove(methodDescriptor.MethodName);
            }
        }

        /// <summary>
        /// Removes the method descriptor identified by it's name.
        /// </summary>
        /// <remarks>
        /// Please note that removal of a sequential method can have
        /// unpredictable results during a call.
        /// </remarks>
        /// <param name="methodName"> </param>
        /// <seealso cref="AddMethodDescriptor(LocalMethodDescriptor)"></seealso>
        public void RemoveMethodDescriptor(string methodName) {
            var methodDescriptor = _nameVsMethodInfo.GetAndRemove(methodName);
            if (methodDescriptor != null) {
                _nameVsMethodInfo.Remove(methodDescriptor.MethodNum.ToString());
            }
        }

        private readonly Dictionary<int, LocalMethodDescriptor> _opnumVsMethodInfo = 
            new Dictionary<int, LocalMethodDescriptor>();
        private readonly Dictionary<int, LocalMethodDescriptor> _dispIdVsMethodInfo = 
            new Dictionary<int, LocalMethodDescriptor>();
        private readonly Dictionary<string, LocalMethodDescriptor> _nameVsMethodInfo = 
            new Dictionary<string, LocalMethodDescriptor>();
        private int _nextNum;
    }
}