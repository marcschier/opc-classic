//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using System;
    using SharpCifs.Util.Sharpen;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Forms the definition of a COM interface to be used in callbacks.
    /// Method overloads are <b>not</b> allowed.
    /// <i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.</i>
    /// </summary>
    [Serializable]
    public sealed class JILocalInterfaceDefinition {

        /// <summary>
        /// Returns status whether this interface supports <code>IDispatch</code> or not.
        /// </summary>
        public bool DispInterface { get; } = true;

        /// <summary>
        ///Creates an Interface definition. By default, the <code>dispinterface</code> property is <code>true</code>.
        /// </summary>
        /// <param name="interfaceIdentifier"> <code>IID</code> of the COM interface being implemented. </param>
        public JILocalInterfaceDefinition(string interfaceIdentifier) {
            InterfaceIdentifier = interfaceIdentifier;
        }

        /// <summary>
        /// Creates an Interface definition. Set <code>isDispInterface</code> interface to <code>false</code>
        /// if this interface does not support <code>IDispatch</code> based calls.
        /// </summary>
        /// <param name="interfaceIdentifier">  <code>IID</code> of the COM interface being implemented. </param>
        /// <param name="isDispInterface"> <code>true</code> if <code>IDispatch</code> ("<code>dispinterface</code>")
        /// is supported, <code>false</code> otherwise. </param>
        public JILocalInterfaceDefinition(string interfaceIdentifier, bool isDispInterface) {
            InterfaceIdentifier = interfaceIdentifier;
            DispInterface = isDispInterface;
        }

        /// <summary>
        /// Adds a Method Descriptor. Methods <b>must</b> be added in the same order as they appear in the IDL.
        /// Please note that overloaded methods are not allowed.
        /// </summary>
        /// <param name="methodDescriptor"> </param>
        /// <exception cref="ArgumentException"> if a method by the same name already exists. </exception>
        public void AddMethodDescriptor(JILocalMethodDescriptor methodDescriptor) {
            if (_nameVsMethodInfo.Contains(methodDescriptor.MethodName)) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_CALLBACK_OVERLOADS_NOTALLOWED));
            }
            methodDescriptor.MethodNum = _nextNum;
            _nextNum++;
            _opnumVsMethodInfo[methodDescriptor.MethodNum] = methodDescriptor;
            if (DispInterface) {
                if (methodDescriptor.MethodDispID == -1) {
                    throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_METHODDESC_DISPID_MISSING));
                }
                _dispIdVsMethodInfo[methodDescriptor.MethodDispID] = methodDescriptor;
            }
            _nameVsMethodInfo[methodDescriptor.MethodName] = methodDescriptor;
        }

        /// <summary>
        /// Returns the method descriptor identified by it's number.
        /// </summary>
        /// <param name="opnum"> </param>
        /// <returns> <code>null</code> if no method by this <code>opnum</code> was found. </returns>
        public JILocalMethodDescriptor GetMethodDescriptor(int opnum) {
            return (JILocalMethodDescriptor)_opnumVsMethodInfo[opnum];
        }

        /// <summary>
        ///Returns the method descriptor identified by it's dispId.
        /// </summary>
        /// <param name="dispId"> </param>
        /// <returns> <code>null</code> if no method by this <code>dispId</code> was found. </returns>
        public JILocalMethodDescriptor GetMethodDescriptorForDispId(int dispId) {
            return (JILocalMethodDescriptor)_dispIdVsMethodInfo[dispId];
        }


        /// <summary>
        ///Returns the method descriptor identified by it's name.
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> <code>null</code> if no method by this <code>name</code> was found. </returns>
        public JILocalMethodDescriptor GetMethodDescriptor(string name) {
            return (JILocalMethodDescriptor)_nameVsMethodInfo[name];
        }

        /// <summary>
        /// Returns all method descriptors.
        /// </summary>
        public JILocalMethodDescriptor[] MethodDescriptors =>
            _opnumVsMethodInfo.Values.Cast<JILocalMethodDescriptor>().ToArray();

        /// <summary>
        /// Returns the interface identifier (<code>IID</code>) of this definition.
        /// </summary>
        public string InterfaceIdentifier { get; } = null;

        /// <summary>
        ///Removes the method descriptor identified by it's number.
        /// <para>
        /// Please note that removal of a sequential method can have unpredictable results during a call.
        /// </para>
        /// </summary>
        /// <param name="opnum"> </param>
        /// <seealso cref="AddMethodDescriptor(JILocalMethodDescriptor)"></seealso>
        public void RemoveMethodDescriptor(int opnum) {
            var methodDescriptor = (JILocalMethodDescriptor)_opnumVsMethodInfo.GetAndRemove(opnum);
            if (methodDescriptor != null) {
                _nameVsMethodInfo.Remove(methodDescriptor.MethodName);
            }
        }

        /// <summary>
        /// Removes the method descriptor identified by it's name.
        /// </summary>
        /// <remarks>
        /// Please note that removal of a sequential method can have unpredictable results during a call.
        /// </remarks>
        /// <param name="methodName"> </param>
        /// <seealso cref="AddMethodDescriptor(JILocalMethodDescriptor)"></seealso>
        public void RemoveMethodDescriptor(string methodName) {
            var methodDescriptor = (JILocalMethodDescriptor)_nameVsMethodInfo.GetAndRemove(methodName);
            if (methodDescriptor != null) {
                _nameVsMethodInfo.Remove(methodDescriptor.MethodNum);
            }
        }

        private Hashtable _opnumVsMethodInfo = new Hashtable();
        private readonly Hashtable _dispIdVsMethodInfo = new Hashtable();
        private Hashtable _nameVsMethodInfo = new Hashtable();
        private int _nextNum;
        internal object _instance;
        internal Type _clazz;
    }
}