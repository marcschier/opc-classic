using System;
using System.Collections;

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


	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JISystem = org.jinterop.dcom.common.JISystem;


	/// <summary>
	///<para>Forms the definition of a COM interface to be used in callbacks. Method overloads are <b>not</b> allowed.
	/// 
	/// </para>
	/// <para><i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
	/// and MSShell examples for more details on how to use this class.</i><br>
	/// 
	/// @since 2.0 (formerly JIInterfaceDefinition)
	/// 
	/// </para>
	/// </summary>
	[Serializable]
	public sealed class JILocalInterfaceDefinition {
		private const long SerialVersionUID = 7683984211902254797L;
		private string InterfaceIdentifier_Renamed = null;
		private IDictionary OpnumVsMethodInfo = new Hashtable();
		private IDictionary DispIdVsMethodInfo = new Hashtable();
		private IDictionary NameVsMethodInfo = new Hashtable();
		private int NextNum = 0;
		internal object Instance = null;
		internal Type Clazz = null;
		private bool DispInterface_Renamed = true;

		/// <summary>
		///Creates an Interface definition. By default, the <code>dispinterface</code> property is <code>true</code>.
		/// </summary>
		/// <param name="interfaceIdentifier"> <code>IID</code> of the COM interface being implemented. </param>
		public JILocalInterfaceDefinition(string interfaceIdentifier) {
			this.InterfaceIdentifier_Renamed = interfaceIdentifier;
		}

		 /// <summary>
		 ///Creates an Interface definition. Set <code>isDispInterface</code> interface to <code>false</code>
		 /// if this interface does not support <code>IDispatch</code> based calls.
		 /// </summary>
		 /// <param name="interfaceIdentifier">  <code>IID</code> of the COM interface being implemented. </param>
		 /// <param name="isDispInterface"> <code>true</code> if <code>IDispatch</code> ("<code>dispinterface</code>")
		 /// is supported , <code>false</code> otherwise. </param>
		public JILocalInterfaceDefinition(string interfaceIdentifier, bool isDispInterface) {
			this.InterfaceIdentifier_Renamed = interfaceIdentifier;
			this.DispInterface_Renamed = isDispInterface;
		}

		/// <summary>
		///Adds a Method Descriptor. Methods <b>must</b> be added in the same order as they appear in the IDL.
		/// 
		/// <para> Please note that overloaded methods are not allowed.
		/// </para>
		/// </summary>
		/// <param name="methodDescriptor"> </param>
		/// <exception cref="IllegalArgumentException"> if a method by the same name already exists. </exception>
		public void AddMethodDescriptor(JILocalMethodDescriptor methodDescriptor) {
			if (NameVsMethodInfo.Contains(methodDescriptor.MethodName)) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_CALLBACK_OVERLOADS_NOTALLOWED));
			}

			methodDescriptor.MethodNum = NextNum;
			NextNum++;

			OpnumVsMethodInfo[new int?(methodDescriptor.MethodNum)] = methodDescriptor;
			if (DispInterface_Renamed) {
				if (methodDescriptor.MethodDispID == -1) {
					throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_METHODDESC_DISPID_MISSING));
				}
				DispIdVsMethodInfo[new int?(methodDescriptor.MethodDispID)] = methodDescriptor;
			}

			NameVsMethodInfo[methodDescriptor.MethodName] = methodDescriptor;


		}

		/// <summary>
		/// Returns the method descriptor identified by it's number. <br>
		/// </summary>
		/// <param name="opnum"> </param>
		/// <returns> <code>null</code> if no method by this <code>opnum</code> was found. </returns>
		public JILocalMethodDescriptor GetMethodDescriptor(int opnum) {
			return (JILocalMethodDescriptor)OpnumVsMethodInfo.GetValueOrNull(new int?(opnum));
		}

		/// <summary>
		///Returns the method descriptor identified by it's dispId. <br>
		/// </summary>
		/// <param name="dispId"> </param>
		/// <returns> <code>null</code> if no method by this <code>dispId</code> was found. </returns>
		public JILocalMethodDescriptor GetMethodDescriptorForDispId(int dispId) {
			return (JILocalMethodDescriptor)DispIdVsMethodInfo.GetValueOrNull(new int?(dispId));
		}


		/// <summary>
		///Returns the method descriptor identified by it's name. <br>
		/// </summary>
		/// <param name="name"> </param>
		/// <returns> <code>null</code> if no method by this <code>name</code> was found. </returns>
		public JILocalMethodDescriptor GetMethodDescriptor(string name) {
			return (JILocalMethodDescriptor)NameVsMethodInfo.GetValueOrNull(name);
		}

		/// <summary>
		///Returns all method descriptors. <br>
		/// 
		/// @return
		/// </summary>
		public JILocalMethodDescriptor[] MethodDescriptors {
			get {
				return (JILocalMethodDescriptor[])OpnumVsMethodInfo.Values.toArray(new JILocalMethodDescriptor[OpnumVsMethodInfo.Values.size()]);
			}
		}

		/// <summary>
		///Returns the interface identifier (<code>IID</code>) of this definition. <br>
		/// 
		/// @return
		/// </summary>
		public string InterfaceIdentifier {
			get {
				return InterfaceIdentifier_Renamed;
			}
		}

		/// <summary>
		///Removes the method descriptor identified by it's number.
		/// <para>
		/// Please note that removal of a sequential method can have unpredictable results during a call. <br>
		/// </para>
		/// </summary>
		/// <param name="opnum"> </param>
		/// <seealso cref= #addMethodDescriptor(JILocalMethodDescriptor) </seealso>
		public void RemoveMethodDescriptor(int opnum) {
			JILocalMethodDescriptor methodDescriptor = (JILocalMethodDescriptor)OpnumVsMethodInfo.Remove(new int?(opnum));
			if (methodDescriptor != null) {
				NameVsMethodInfo.Remove(methodDescriptor.MethodName);
			}
		}

		/// <summary>
		///Removes the method descriptor identified by it's name. <para>
		/// </para>
		/// <para>
		/// Please note that removal of a sequential method can have unpredictable results during a call. <br>
		/// </para>
		/// </summary>
		/// <param name="methodName"> </param>
		/// <seealso cref= #addMethodDescriptor(JILocalMethodDescriptor) </seealso>
		public void RemoveMethodDescriptor(string methodName) {
			JILocalMethodDescriptor methodDescriptor = (JILocalMethodDescriptor)NameVsMethodInfo.Remove(methodName);
			if (methodDescriptor != null) {
				NameVsMethodInfo.Remove(new int?(methodDescriptor.MethodNum));
			}
		}

		/// <summary>
		///Returns status whether this interface supports <code>IDispatch</code> or not.
		/// </summary>
		/// <returns> <code>true</code> if <code>IDispatch</code> is supported. </returns>
		public bool DispInterface {
			get {
				return DispInterface_Renamed;
			}
		}
	}


}