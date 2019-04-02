// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 
namespace org.jinterop.dcom.impls
{

	using JIErrorCodes = common.JIErrorCodes;
	using JIException = common.JIException;
	using JISystem = common.JISystem;
	using IJIComObject = core.IJIComObject;
	using JIFrameworkHelper = core.JIFrameworkHelper;
	using JILocalCoClass = core.JILocalCoClass;
	using JISession = core.JISession;
	using Internal_JIAutomationFactory = automation.Internal_JIAutomationFactory;


	/// <summary>
	/// Factory class for creating COM objects. 
	/// Sample Usage:-
	/// <code>
	///  //Assume comObject is the reference to IJIComObject, obtained earlier... 
	///  newComObject = (IJIComObject)comObject.queryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6");//ISWbemLocator 
	///  //This will obtain the dispatch interface 
	///  dispatch = (IJIDispatch)JIObjectFactory.narrowObject(newComObject.queryInterface(IJIDispatch.IID)); 
	/// </code>
	/// </summary>
	public class JIObjectFactory
	{

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
		/// <exception cref="System.ArgumentException"> if any parameter is <code>null</code> or <code>sourceUUID</code> is empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String attachEventHandler(org.jinterop.dcom.core.IJIComObject comObject,String sourceUUID,org.jinterop.dcom.core.IJIComObject eventListener) throws org.jinterop.dcom.common.JIException
		public static string attachEventHandler(IJIComObject comObject, string sourceUUID, IJIComObject eventListener)
		{
			return JIFrameworkHelper.attachEventHandler(comObject, sourceUUID, eventListener);

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
		public static void detachEventHandler(IJIComObject comObject, string identifier)
		{
			JIFrameworkHelper.detachEventHandler(comObject, identifier);
		}

		/// <summary>
		///<i>Narrows</i> the <code>comObject</code> into its right type based on it's <code>IID</code>. For example, passing a
		/// <code>comObject</code> which is a COM <code>IDispatch</code> reference will return a reference which can be safely casted
		/// to <code>IJIDispatch</code> interface.
		/// </summary>
		/// <param name="comObject">
		/// </param>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="System.ArgumentException"> if <code>comObject</code> is <code>null</code> or a local reference. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject narrowObject(final org.jinterop.dcom.core.IJIComObject comObject) throws org.jinterop.dcom.common.JIException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public static IJIComObject narrowObject(IJIComObject comObject)
		{
			if (comObject == null || comObject.LocalReference)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMFACTORY_ILLEGAL_ARG));
			}

			//Will later on add another way to dynamically moving to factories.
			var retval = Internal_JIAutomationFactory.narrowObject(comObject);

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
		/// </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject buildObject(org.jinterop.dcom.core.JISession session,org.jinterop.dcom.core.JILocalCoClass javaComponent) throws org.jinterop.dcom.common.JIException
		public static IJIComObject buildObject(JISession session, JILocalCoClass javaComponent)
		{
			return JIFrameworkHelper.instantiateLocalComObject(session, javaComponent);
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
		public static void releaseObject(JISession session, JILocalCoClass javaComponent)
		{
			JIFrameworkHelper.releaseLocalComponent(session, javaComponent);
		}

		/// <summary>
		/// Returns a COM Object from raw bytes. These bytes must conform to the Marshalled Interface Pointer template as per DCOM specifications.
		/// </summary>
		/// <param name="session"> session to attach <code>comObject</code> to. If required the framework will create a new session
		/// for this <code>comObject</code> and link the <code>session</code> to the new one. This new session will be
		/// destroyed when the parent <code>session</code> is destroyed. </param>
		/// <param name="rawBytes"> bytes representing the interface pointer.
		/// </param>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="System.ArgumentException"> if <code>rawBytes</code> is an invalid representation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject buildObject(org.jinterop.dcom.core.JISession session, byte[] rawBytes) throws org.jinterop.dcom.common.JIException
		public static IJIComObject buildObject(JISession session, sbyte[] rawBytes)
		{
			return narrowObject(JIFrameworkHelper.instantiateComObject(session, rawBytes,null));
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
		/// </param>
		/// <exception cref="JIException"> </exception>
		/// <exception cref="System.ArgumentException"> if <code>rawBytes</code> is an invalid representation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject buildObject(org.jinterop.dcom.core.JISession session, byte[] rawBytes, String ipAddress) throws org.jinterop.dcom.common.JIException
		public static IJIComObject buildObject(JISession session, sbyte[] rawBytes, string ipAddress)
		{
			return narrowObject(JIFrameworkHelper.instantiateComObject(session, rawBytes,ipAddress));
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
		 /// </param>
		 /// <exception cref="JIException"> </exception>
		 /// <exception cref="System.ArgumentException"> if <code>comObject</code> is <code>null</code> or a local reference. </exception>
		 /// <seealso cref= IJIComObject#isLocalReference() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject narrowObject(org.jinterop.dcom.core.JISession session, org.jinterop.dcom.core.IJIComObject comObject) throws org.jinterop.dcom.common.JIException
		public static IJIComObject narrowObject(JISession session, IJIComObject comObject)
		{
			return narrowObject(JIFrameworkHelper.instantiateComObject(session, comObject));
		}
	}

}