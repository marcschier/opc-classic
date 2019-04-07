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

namespace org.jinterop.dcom.impls {

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIFrameworkHelper = org.jinterop.dcom.core.JIFrameworkHelper;
	using JILocalCoClass = org.jinterop.dcom.core.JILocalCoClass;
	using JISession = org.jinterop.dcom.core.JISession;
	using Internal_JIAutomationFactory = org.jinterop.dcom.impls.automation.Internal_JIAutomationFactory;



	/// <summary>
	///<para>Factory class for creating COM objects. <p>
	/// 
	/// Sample Usage:-
	/// <br>
	/// 
	/// <code>
	///  //Assume comObject is the reference to IJIComObject, obtained earlier... <br>
	/// newComObject = (IJIComObject)comObject.queryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6");//ISWbemLocator <br>
	/// //This will obtain the dispatch interface <br>
	/// dispatch = (IJIDispatch)JIObjectFactory.narrowObject(newComObject.queryInterface(IJIDispatch.IID)); <br>
	/// </code>
	/// 
	/// @since 2.0 (formerly JIComFactory)
	/// </para>
	/// </summary>
	public class JIObjectFactory {

		/// <summary>
		///<para> Attaches an event handler to <code>comObject</code> for the source event interface of COM , identified by the
		/// <code>sourceUUID</code>. The event listener is itself identified by <code>eventListener</code>. An exception will be raised if
		/// <code>sourceUUID</code> is not supported by the COM Server.
		///  </para>
		/// </summary>
		/// <param name="comObject"> object to which the listener will be attached. </param>
		/// <param name="sourceUUID"> <code>IID</code> of the call back interface. </param>
		/// <param name="eventListener"> <code>IJIComObject</code> obtained using <seealso cref="#buildObject(JISession, JILocalCoClass)"/> </param>
		/// <returns> string identifier for this connection, please save this for eventual release using <seealso cref="#detachEventHandler(IJIComObject, String)"/> </returns>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="IllegalArgumentException"> if any parameter is <code>null</code> or <code>sourceUUID</code> is empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String attachEventHandler(org.jinterop.dcom.core.IJIComObject comObject,String sourceUUID,org.jinterop.dcom.core.IJIComObject eventListener) throws org.jinterop.dcom.common.JIException
		public static string AttachEventHandler(IJIComObject comObject, string sourceUUID, IJIComObject eventListener) {
			return JIFrameworkHelper.AttachEventHandler(comObject, sourceUUID, eventListener);

		}
		/// <summary>
		///Detaches the event handler identified by <code>identifier</code> and associated with this <code>comObject</code>. This method
		/// will raise an exception if the <code>identifier</code> is invalid.
		/// </summary>
		/// <param name="comObject"> </param>
		/// <param name="identifier"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void detachEventHandler(org.jinterop.dcom.core.IJIComObject comObject, String identifier) throws org.jinterop.dcom.common.JIException
		public static void DetachEventHandler(IJIComObject comObject, string identifier) {
			JIFrameworkHelper.DetachEventHandler(comObject, identifier);
		}

		/// <summary>
		///<i>Narrows</i> the <code>comObject</code> into its right type based on it's <code>IID</code>. For example, passing a
		/// <code>comObject</code> which is a COM <code>IDispatch</code> reference will return a reference which can be safely casted
		/// to <code>IJIDispatch</code> interface.
		/// </summary>
		/// <param name="comObject">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="IllegalArgumentException"> if <code>comObject</code> is <code>null</code> or a local reference. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject narrowObject(final org.jinterop.dcom.core.IJIComObject comObject) throws org.jinterop.dcom.common.JIException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public static IJIComObject NarrowObject(IJIComObject comObject) {
			if (comObject == null || comObject.LocalReference) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMFACTORY_ILLEGAL_ARG));
			}

			//Will later on add another way to dynamically moving to factories.
			IJIComObject retval = Internal_JIAutomationFactory.NarrowObject(comObject);

			return retval;
		}

		/// <summary>
		/// Returns a <b>local</b> COM Object representation for the Java component. <code>IJIComObject.IsLocalReference()</code>
		/// method will return <code>true</code> for all objects built by this method. Another important point to note is that a
		/// <code>javaComponent</code> can only export one reference to itself. Reusing the same <code>javaComponent</code> in another
		/// call to this method will raise an exception.
		/// </summary>
		/// <param name="session"> session to attach <code>comObject</code> to. </param>
		/// <param name="javaComponent">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject buildObject(org.jinterop.dcom.core.JISession session,org.jinterop.dcom.core.JILocalCoClass javaComponent) throws org.jinterop.dcom.common.JIException
		public static IJIComObject BuildObject(JISession session, JILocalCoClass javaComponent) {
			return JIFrameworkHelper.InstantiateLocalComObject(session, javaComponent);
		}

		/// <summary>
		/// To be called after one is done using the local Java CoClass. Recommended to be called from the <code>finalize()</code> method of the 
		/// Java CoClass.
		/// </summary>
		/// <param name="session"> </param>
		/// <param name="javaComponent"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void releaseObject(org.jinterop.dcom.core.JISession session, org.jinterop.dcom.core.JILocalCoClass javaComponent) throws org.jinterop.dcom.common.JIException
		public static void ReleaseObject(JISession session, JILocalCoClass javaComponent) {
			JIFrameworkHelper.ReleaseLocalComponent(session, javaComponent);
		}

		/// <summary>
		/// Returns a COM Object from raw bytes. These bytes must conform to the Marshalled Interface Pointer template as per DCOM specifications.
		/// </summary>
		/// <param name="session"> session to attach <code>comObject</code> to. If required the framework will create a new session
		/// for this <code>comObject</code> and link the <code>session</code> to the new one. This new session will be
		/// destroyed when the parent <code>session</code> is destroyed. </param>
		/// <param name="rawBytes"> bytes representing the interface pointer.
		/// @return </param>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="IllegalArgumentException"> if <code>rawBytes</code> is an invalid representation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject buildObject(org.jinterop.dcom.core.JISession session, byte[] rawBytes) throws org.jinterop.dcom.common.JIException
		public static IJIComObject BuildObject(JISession session, sbyte[] rawBytes) {
			return NarrowObject(JIFrameworkHelper.InstantiateComObject(session, rawBytes,null));
		}

		/// <summary>
		/// Returns a COM Object from raw bytes. These bytes must conform to the Marshalled Interface Pointer template as per DCOM specifications.
		/// </summary>
		/// <param name="session"> session to attach <code>comObject</code> to. If required the framework will create a new session
		/// for this <code>comObject</code> and link the <code>session</code> to the new one. This new session will be
		/// destroyed when the parent <code>session</code> is destroyed. </param>
		/// <param name="rawBytes"> bytes representing the interface pointer. </param>
		/// <param name="ipAddress">	can be <code>null</code>. Sometimes there are many adapters (virtual as well) on the Target machine to which this interface pointer belongs,
		/// which may get sent as part of the interface pointer and consequently this call will fail since it is a possibility that IP is not reachable via this machine.
		/// The developer can send in the valid IP and if found in the interface pointer list will be used to talk to the target machine, overriding the other IP addresses
		/// present in the interface pointer. If this IP is not found then the "machine name" binding will be used. If this param is <code>null</code> then the first
		/// binding obtained from the interface pointer is used.
		/// @return </param>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="IllegalArgumentException"> if <code>rawBytes</code> is an invalid representation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject buildObject(org.jinterop.dcom.core.JISession session, byte[] rawBytes, String ipAddress) throws org.jinterop.dcom.common.JIException
		public static IJIComObject BuildObject(JISession session, sbyte[] rawBytes, string ipAddress) {
			return NarrowObject(JIFrameworkHelper.InstantiateComObject(session, rawBytes,ipAddress));
		}

		 /// <summary>
		 /// Typically used in the Man-In-The-Middle scenario.
		 /// <para> Some possible use-cases :-
		 /// <ul>
		 /// <li>One j-Interop system interacts with another over the wire.</li>
		 ///  <li>The <code>IJIComObject</code> is read from a database and is not <i>attached</i> to a session.</li>
		 /// </ul>
		 /// </para>
		 /// </summary>
		 /// <param name="session"> session to attach <code>comObject</code> to. If required the framework will create a new session
		 /// for this <code>comObject</code> and link the <code>session</code> to the new one. This new session will be
		 /// destroyed when the parent <code>session</code> is destroyed. </param>
		 /// <param name="comObject"> <i>drifting</i> object.
		 /// @return </param>
		 /// <exception cref="JIException"> </exception>
		 /// <exception cref="IllegalArgumentException"> if <code>comObject</code> is <code>null</code> or a local reference. </exception>
		 /// <seealso cref= IJIComObject#isLocalReference() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject narrowObject(org.jinterop.dcom.core.JISession session, org.jinterop.dcom.core.IJIComObject comObject) throws org.jinterop.dcom.common.JIException
		public static IJIComObject NarrowObject(JISession session, IJIComObject comObject) {
			return NarrowObject(JIFrameworkHelper.InstantiateComObject(session, comObject));
		}
	}

}