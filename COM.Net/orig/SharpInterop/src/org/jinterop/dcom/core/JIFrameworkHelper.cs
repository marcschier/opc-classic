using System;
using System.Collections;
using System.Collections.Generic;

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


	using NdrBuffer = ndr.NdrBuffer;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	/// <summary>
	/// Internal Framework Helper class. Do not use outside of framework.
	/// 
	/// @exclude
	/// </summary>
	public sealed class JIFrameworkHelper {

		/// <summary>
		/// @exclude </summary>
		/// <param name="src"> </param>
		/// <param name="target"> </param>
		internal static void Link2Sessions(JISession src, JISession target) {
			if (src == null || target == null) {
				throw new System.NullReferenceException();
			}

			JISession.LinkTwoSessions(src, target);
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="src"> </param>
		/// <param name="target"> </param>
		internal static void UnLinkSession(JISession src, JISession unlinkedSession) {
			if (src == null || unlinkedSession == null) {
				throw new System.NullReferenceException();
			}

			JISession.UnLinkSession(src, unlinkedSession);
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="src"> </param>
		/// <param name="target"> </param>
		internal static JISession ResolveSessionForOXID(sbyte[] oxid) {
			return JISession.ResolveSessionForOxid(new JIOxid(oxid));
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="src"> </param>
		/// <param name="target"> </param>
		 internal static JIInterfacePointer GetInterfacePointerOfStub(JISession session) {
			return session.Stub.ServerInterfacePointer;
		 }

		 /// <summary>
		 /// Must be called once and only once from JICallBuilder "read" to create the right pointer in case of man in the middle scenario and
		 /// add it to the session.
		 /// </summary>
		 /// <param name="session"> </param>
		 /// <param name="ptr">
		 /// @return </param>
		 /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static IJIComObject instantiateComObject(JISession session, final JIInterfacePointer ptr) throws org.jinterop.dcom.common.JIException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		internal static IJIComObject InstantiateComObject(JISession session, JIInterfacePointer ptr) {
			IJIComObject retval = InstantiateComObject2(session,ptr);
			AddComObjectToSession(retval.AssociatedSession, retval);
			return retval;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static IJIComObject instantiateComObject2(JISession session, final JIInterfacePointer ptr) throws org.jinterop.dcom.common.JIException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		internal static IJIComObject InstantiateComObject2(JISession session, JIInterfacePointer ptr) {
			if (ptr == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMFACTORY_ILLEGAL_ARG));
			}

			IJIComObject retval = null;
			JIInterfacePointer stubPtr = JIFrameworkHelper.GetInterfacePointerOfStub(session);
			if (!JIInterfacePointer.IsOxidEqual(stubPtr, ptr)) {
				if (JISystem.Logger.isLoggable(Level.WARNING)) {
					JISystem.Logger.warning("NEW SESSION IDENTIFIED ! for ptr " + ptr);
				}
				//first check if a session for this OXID does not already exist and thus its stub
				JISession newsession = JIFrameworkHelper.ResolveSessionForOXID(ptr.OXID);
				if (newsession == null) {
					//new COM server pointer
					newsession = JISession.CreateSession(session);
					newsession.GlobalSocketTimeout = session.GlobalSocketTimeout;
					newsession.UseSessionSecurity(session.SessionSecurityEnabled);
					newsession.UseNTLMv2(session.NTLMv2Enabled);
					JIComServer comServer = new JIComServer(newsession,ptr,null);
					retval = comServer.Instance;
					JIFrameworkHelper.Link2Sessions(session, newsession);
				}
	//			else
	//			{
	//				retval = new JIComObjectImpl(newsession,ptr);
	//			}

				//this is so that the reference gets added correctly.
				session = newsession;
			}

			if (retval == null) {
				retval = new JIComObjectImpl(session,ptr);
			}

			return retval;
		}

		internal static void AddComObjectToSession(JISession session, IJIComObject comObject) {
			session.AddToSession(comObject,comObject.Internal_getInterfacePointer().OID);
		}


		/// <summary>
		/// Returns an Interface Pointer representation for the Java Component
		/// 
		/// @exclude </summary>
		/// <param name="javaComponent">
		/// @return </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IJIComObject instantiateLocalComObject(JISession session,JILocalCoClass javaComponent) throws org.jinterop.dcom.common.JIException
		public static IJIComObject InstantiateLocalComObject(JISession session, JILocalCoClass javaComponent) {
			return new JIComObjectImpl(session,JIComOxidRuntime.GetInterfacePointer(session,javaComponent),true);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void releaseLocalComponent(JISession session, JILocalCoClass javaComponent) throws org.jinterop.dcom.common.JIException
		public static void ReleaseLocalComponent(JISession session, JILocalCoClass javaComponent) {
			JIComOxidRuntime.ReleaseLocalComponent(session, javaComponent);
		}

		/// <summary>
		/// Returns an Interface Pointer representation from raw bytes.
		/// 
		/// @exclude </summary>
		/// <param name="session"> </param>
		/// <param name="rawBytes">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IJIComObject instantiateComObject(JISession session,byte[] rawBytes, String ipAddress) throws org.jinterop.dcom.common.JIException
		public static IJIComObject InstantiateComObject(JISession session, sbyte[] rawBytes, string ipAddress) {
			NetworkDataRepresentation ndr = new NetworkDataRepresentation();
			NdrBuffer ndrBuffer = new NdrBuffer(rawBytes,0);
			ndr.Buffer = ndrBuffer;
			ndrBuffer.length = rawBytes.Length;

			//this is a brand new session.
			if (session.Stub == null) {
				 JIComServer comServer = new JIComServer(session,JIInterfacePointer.Decode(ndr, new List<object>(), JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2, new Hashtable()),ipAddress);
				 return comServer.Instance;
			}
			else {
				IJIComObject retval = InstantiateComObject(session, JIInterfacePointer.Decode(ndr, new List<object>(), JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2, new Hashtable()));
				//increasing the reference count.
				retval.AddRef();
				return retval;
			}
		}

		/// <summary>
		/// Typically used in the Man-In-The-Middle scenario, where one j-Interop system interacts with another over the wire.
		/// Or the IJIComObject is deserialized from a Database and is right now drifting.
		/// 
		/// @exclude </summary>
		/// <param name="session"> </param>
		/// <param name="comObject">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IJIComObject instantiateComObject(JISession session,IJIComObject comObject) throws org.jinterop.dcom.common.JIException
		public static IJIComObject InstantiateComObject(JISession session, IJIComObject comObject) {
			if (comObject.AssociatedSession != null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_SESSION_ALREADY_ATTACHED));
			}

			if (comObject.LocalReference) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMOBJ_LOCAL_REF));
			}

			return InstantiateComObject(session, comObject.Internal_getInterfacePointer());
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="comObject"> </param>
		/// <param name="identifier"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void detachEventHandler(IJIComObject comObject, String identifier) throws org.jinterop.dcom.common.JIException
		public static void DetachEventHandler(IJIComObject comObject, string identifier) {
			object[] connectionInfo = comObject.Internal_getConnectionInfo(identifier);
			if (connectionInfo == null) {
				throw new JIException(JIErrorCodes.JI_CALLBACK_INVALID_ID);
			}

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Detaching event handler for  comObject: " + comObject.InterfaceIdentifier + " , identifier: " + identifier);
			}

			IJIComObject connectionPointer = (IJIComObject)connectionInfo[0];

			//first use the cookie to detach.
			JICallBuilder @object = new JICallBuilder(true);
			@object.Opnum = 3;
			@object.AddInParamAsInt((int)((int?)connectionInfo[1]),JIFlags.FLAG_NULL);
			connectionPointer.Call(@object);
			//now release the connectionPointer.
			connectionPointer.Release();
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="comObject"> </param>
		/// <param name="sourceUUID"> </param>
		/// <param name="eventListener">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String attachEventHandler(IJIComObject comObject,String sourceUUID,IJIComObject eventListener) throws org.jinterop.dcom.common.JIException
		public static string AttachEventHandler(IJIComObject comObject, string sourceUUID, IJIComObject eventListener) {
			if (eventListener == null || comObject == null || sourceUUID == null || sourceUUID.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_CALLBACK_INVALID_PARAMS));
			}

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Attaching event handler for  comObject: " + comObject.InterfaceIdentifier + " , sourceUUID: " + sourceUUID + " , eventListener: " + eventListener.InterfaceIdentifier + " and eventListner IPID: " + eventListener.Ipid);
			}
			//IID of IConnectionPointContainer :- B196B284-BAB4-101A-B69C-00AA00341D07
			IJIComObject connectionPointContainer = (IJIComObject)comObject.QueryInterface("B196B284-BAB4-101A-B69C-00AA00341D07");
			JICallBuilder @object = new JICallBuilder(true);
			@object.Opnum = 1;
			@object.AddInParamAsUUID(sourceUUID,JIFlags.FLAG_NULL);
			@object.AddOutParamAsObject(typeof(IJIComObject),JIFlags.FLAG_NULL);
			object[] objects = (object[])connectionPointContainer.Call(@object); //find connection point
			IJIComObject connectionPointer = (IJIComObject)objects[0];

			@object.ReInit();
			@object.Opnum = 2;
			@object.AddInParamAsComObject(eventListener, JIFlags.FLAG_NULL);
			@object.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			object[] obj = connectionPointer.Call(@object);

			//used to unadvise from the connectionpoint
			int? dwcookie = ((int?)obj[0]);

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Event handler returned cookie " + dwcookie);
			}
			connectionPointContainer.Release();

			return comObject.Internal_setConnectionInfo(connectionPointer,dwcookie);

		}

		public static int ReverseArrayForDispatch(JIArray arrayToReverse) {
			return arrayToReverse.ReverseArrayForDispatch();
		}
	}

}