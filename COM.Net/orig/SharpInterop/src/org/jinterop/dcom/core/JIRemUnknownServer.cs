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


	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JIComEndpoint = org.jinterop.dcom.transport.JIComEndpoint;
	using JIComTransportFactory = org.jinterop.dcom.transport.JIComTransportFactory;

	using Endpoint = rpc.Endpoint;
	using FaultException = rpc.FaultException;
	using Stub = rpc.Stub;



	 internal sealed class JIRemUnknownServer : Stub {

		private static Properties Defaults = new Properties();
		static JIRemUnknownServer() {

			Defaults.put("rpc.ntlm.lanManagerKey","false");
			Defaults.put("rpc.ntlm.sign","false");
			Defaults.put("rpc.ntlm.seal","false");
			Defaults.put("rpc.ntlm.keyExchange","false");
			Defaults.put("rpc.connectionContext","rpc.security.ntlm.NtlmConnectionContext");
			Defaults.put("rpc.socketTimeout", (new int?(0)).ToString());
		}

		private JISession Session = null;
		private string Syntax_Renamed = null;
		private string RemunknownIPID = null;
		private readonly object Mutex = new object();
		private bool TimeoutModifiedfrom0 = false;

		/// <summary>
		/// Interface pointer to the initialized COM server , must be called immediately after the JIComServer has been 
		/// initialized. And closeStub must be called where we call closeStub of JIComServer.
		/// </summary>
		/// <param name="session"> </param>
		/// <param name="interfacePointer"> </param>
		/// <param name="address"> in the "ncacn_ip_tcp:host[port]" format </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JIRemUnknownServer(JISession session, String remUnknownIpid, String address) throws org.jinterop.dcom.common.JIException
		public JIRemUnknownServer(JISession session, string remUnknownIpid, string address) : base() {

			this.Session = session;
			base.TransportFactory = JIComTransportFactory.SingleTon;
			base.Properties = new Properties(Defaults);
			base.Properties.setProperty("rpc.socketTimeout", (new int?(session.GlobalSocketTimeout)).ToString());

			if (session.NTLMv2Enabled) {
				base.Properties.setProperty("rpc.ntlm.ntlmv2", "true");
			}

			if (session.SSOEnabled) {
				base.Properties.setProperty("rpc.ntlm.sso", "true");
			}
			else {
				base.Properties.setProperty("rpc.security.username", session.UserName);
				base.Properties.setProperty("rpc.security.password", session.Password);
				base.Properties.setProperty("rpc.ntlm.domain", session.Domain);
			}

			//now set the NTLMv2 Session Security.
			if (session.SessionSecurityEnabled) {
				base.Properties.setProperty("rpc.ntlm.seal", "true");
				base.Properties.setProperty("rpc.ntlm.sign", "true");
				base.Properties.setProperty("rpc.ntlm.keyExchange", "true");
				base.Properties.setProperty("rpc.ntlm.keyLength", "128");
				base.Properties.setProperty("rpc.ntlm.ntlm2", "true");
			}


			// Now will setup syntax for IRemUnknown and the address. 
			Syntax_Renamed = "00000143-0000-0000-c000-000000000046:0.0";
			//and currently only TCPIP is supported.
			Address = address;
			this.RemunknownIPID = remUnknownIpid;
			this.Session.Stub2 = this;
		}

		public string Syntax {
			get {
				return Syntax_Renamed;
			}
		}

		/// <summary>
		/// Execute a Method on the COM Interface identified by the IID
		/// 
		/// 
		/// @exclude </summary>
		/// <param name="obj"> </param>
		/// <param name="targetIID">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object[] call(JICallBuilder obj,String targetIID, int socketTimeout) throws org.jinterop.dcom.common.JIException
		public object[] Call(JICallBuilder obj, string targetIID, int socketTimeout) {
			lock (Mutex) {

				if (Session.SessionInDestroy && !obj.FromDestroySession) {
					throw new JIException(JIErrorCodes.JI_SESSION_DESTROYED);
				}

				if (socketTimeout != 0) {
					SocketTimeOut = socketTimeout;
				}
				else { //for cases where it was something earlier, but is now being set to 0.
					if (TimeoutModifiedfrom0) {
						SocketTimeOut = socketTimeout;
					}
				}

				try {

					attach();
					if (!Endpoint.Syntax.Uuid.ToString().Equals(targetIID, StringComparison.CurrentCultureIgnoreCase)) {
						//first send an AlterContext to the IID of the interface
						Endpoint.Syntax.Uuid = new rpc.core.UUID(targetIID);
						Endpoint.Syntax.setVersion(0,0);
						((JIComEndpoint)Endpoint).RebindEndPoint();
					}

					Object = obj.ParentIpid;
					call(Endpoint.IDEMPOTENT,obj);

				}
				catch (FaultException e) {
					throw new JIException(e.status,e);
				}
				catch (IOException e) {
					throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
				}
				catch (JIRuntimeException e1) {
					throw new JIException(e1);
				}

				return obj.Results;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void addRef_ReleaseRef(JICallBuilder obj) throws org.jinterop.dcom.common.JIException
		public void AddRef_ReleaseRef(JICallBuilder obj) {
			lock (Mutex) {

				if (RemunknownIPID == null) {
					return;
				}
				//now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation or IOxidResolver
				obj.ParentIpid = RemunknownIPID;
				obj.AttachSession(Session);
				try {
					Call(obj,JIRemUnknown.IID_IUnknown, Session.GlobalSocketTimeout);
				}
				catch (JIRuntimeException e1) {
					throw new JIException(e1);
				}

			}
		}

		public void CloseStub() {
			try {
				detach();
			}
			catch (IOException) {
	//			e.printStackTrace();
			}
		}

		public int SocketTimeOut {
			set {
				if (value == 0) {
					TimeoutModifiedfrom0 = false;
				}
				else {
					TimeoutModifiedfrom0 = true;
				}
    
				Properties.setProperty("rpc.socketTimeout", (new int?(value)).ToString());
			}
		}

	 }

}