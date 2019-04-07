using System;
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


	using JIDefaultAuthInfoImpl = org.jinterop.dcom.common.JIDefaultAuthInfoImpl;
	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
	using JIComEndpoint = org.jinterop.dcom.transport.JIComEndpoint;
	using JIComTransportFactory = org.jinterop.dcom.transport.JIComTransportFactory;
	using IJIWinReg = org.jinterop.winreg.IJIWinReg;
	using JIPolicyHandle = org.jinterop.winreg.JIPolicyHandle;
	using JIWinRegFactory = org.jinterop.winreg.JIWinRegFactory;

	using Endpoint = rpc.Endpoint;
	using FaultException = rpc.FaultException;
	using Stub = rpc.Stub;


	/// <summary>
	/// Startup class representing a COM Server.
	///  <para>
	/// Sample Usage :-
	///  <br>
	///  <code>
	/// 
	///  <seealso cref="JISession"/> session = JISession.createSession("DOMAIN","USERNAME","PASSWORD"); <br>
	/// JIComServer excelServer = new JIComServer(JIProgId.valueOf("Excel.Application"),address,session); <br>
	///  IJIComObject comObject = excelServer.createInstance(); <br>
	///  //Obtaining the IJIDispatch (if supported) <br>
	///  <seealso cref="IJIDispatch"/> dispatch = (IJIDispatch)<seealso cref="JIObjectFactory"/>.narrowObject(comObject.queryInterface(IJIDispatch.IID)); <br>
	///  </code>
	/// 
	/// </para>
	///  <para>Each instance of this class is associated with a single session only.
	/// 
	/// @since 1.0
	/// 
	/// </para>
	/// </summary>
	public sealed class JIComServer : Stub {

		private static Properties Defaults = new Properties();
		static JIComServer() {

			Defaults.put("rpc.ntlm.lanManagerKey","false");
			Defaults.put("rpc.ntlm.sign","false");
			Defaults.put("rpc.ntlm.seal","false");
			Defaults.put("rpc.ntlm.keyExchange","false");
			Defaults.put("rpc.ntlm.sso","false");
			Defaults.put("rpc.connectionContext","rpc.security.ntlm.NtlmConnectionContext");
			Defaults.put("rpc.socketTimeout", (new int?(0)).ToString());
	//		rpc.connectionContext = rpc.security.ntlm.NtlmConnectionContext
	//		rpc.ntlm.sign = false
	//		rpc.ntlm.seal = false
	//		rpc.ntlm.keyExchange = false

		}

		//private String address = null;
	//	private JIRemActivation remoteActivation = null;
		private JIIServerActivation ServerActivation = null;
		private JIOxidResolver OxidResolver = null;
		private string Clsid = null;
		private string Syntax_Renamed = null;
		private JISession Session = null;
		private bool ServerInstantiated = false;
		private string RemunknownIPID = null;
		private readonly object Mutex = new object();
		private bool TimeoutModifiedfrom0 = false;
		private JIInterfacePointer InterfacePtrCtor = null;
		private static readonly IList<string> ListOfIps = new List<string>();

		private JIComServer() {
		}

		/// <summary>
		///<para> Instantiates a JIComServer represented by the interfacePointer param. There are cases where a COM server may hand down a
		/// reference to a different COM server(which may or may not be on the same machine) and we would like to hook in between.
		/// The <code>IJIComObject</code> interface is usable only in the context of the current JIComServer, but when the interfacePointer
		/// is of a completely different COM server, the JIObjectFactory APIs will not work. The reason is the interface pointer passed to those
		/// APIs expects to belong only to a single and same COM server (say 'A'). If by any chance, that COM server passes a reference to you
		/// of another COM server (say 'B') on a different machine, the <code>IJIComObject</code> so returned from <code>JIObjectFactory</code> APIs
		/// will result in "Method not found" Exceptions (or others) since the pointer returned via that will always place calls to  'A' instead of 'B'.
		/// Under such scenarios you must use this API. This is not a usual case and for reasons related to nature of DCOM, will be very well documented
		/// in the Developers guide of your COM server.
		/// 
		/// </para>
		/// <para>The DCOM specs refer to this as the "middleman" case. (Section 3.3.1) </para>
		/// </p> </summary>
		/// <param name="session"> Please use a new session and not an already bounded one. The <code>JISession.createSession(JISession)</code> can be used to create a new session. </param>
		/// <param name="interfacePointer"> reference to a different COM server pointer. </param>
		/// <param name="ipAddress">		  Can be <code>null</code>. Sometimes there are many adapters (virtual as well) on the Target machine to which this interface pointer belongs,
		/// 						  which may get sent as part of the interface pointer and consequently this call will fail since it is a possibility that IP is not reachable via this machine.
		/// 						  The developer can send in the valid IP and if found in the interface pointer list will be used to talk to the target machine, overriding the other IP addresses present in the interface pointer.
		/// 						  If this IP is not found then the "machine name" binding will be used. If this param is <code>null</code> then the first binding obtained from the interface pointer is used. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JIComServer(JISession session, JIInterfacePointer interfacePointer,String ipAddress) throws org.jinterop.dcom.common.JIException
		public JIComServer(JISession session, JIInterfacePointer interfacePointer, string ipAddress) : base() {

			if (interfacePointer == null || session == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
			}

			if (session.Stub != null) {
				throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
			}

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Internal_dumpMap();
			}

	//		ipAddress="192.168.1.104";
			if (ipAddress != null && !ipAddress.Trim().Equals("", StringComparison.CurrentCultureIgnoreCase)) {
				if (!ListOfIps.Contains(ipAddress)) {
					ListOfIps.Add(ipAddress.ToLower());
				}
			}

			base.TransportFactory = JIComTransportFactory.SingleTon;
			//now read the session and prepare information for the stub.
			base.Properties = new Properties(Defaults);
			base.Properties.setProperty("rpc.security.username", session.UserName);
			base.Properties.setProperty("rpc.security.password", session.Password);
			base.Properties.setProperty("rpc.ntlm.domain", session.Domain);
			base.Properties.setProperty("rpc.socketTimeout", (new int?(session.GlobalSocketTimeout)).ToString());
			if (session.NTLMv2Enabled) {
				base.Properties.setProperty("rpc.ntlm.ntlmv2", "true");
			}
			if (session.SSOEnabled) {
				base.Properties.setProperty("rpc.ntlm.sso", "true");
			}


			JIStringBinding[] addressBindings = interfacePointer.StringBindings.StringBindings;

			int i = 0;
			JIStringBinding binding = null;
			JIStringBinding nameBinding = null;
			string targetAddress = ipAddress == null ? "" : ipAddress.Trim();

			{
	//		if (!targetAddress.equals(""))
				//now we choose, otherwise the first one we get.
				while (i < addressBindings.Length) {
					binding = addressBindings[i];
					if (binding.TowerId != 0x07) { //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
						i++;
						continue;
					}
					//get the one with IP address
					int index = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
					if (index != -1) {
						try {

	//						if (binding.getNetworkAddress().equalsIgnoreCase(targetAddress))
							if (ListOfIps.Contains(binding.NetworkAddress.ToLower())) {
								nameBinding = null;
								break;
							}

							//now check for the one with port
							index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
	//						if (index != -1 && binding.getNetworkAddress().substring(0,index).equalsIgnoreCase(targetAddress))
							if (index != -1 && ListOfIps.Contains(binding.NetworkAddress.Substring(0,index).ToLower())) {
								nameBinding = null;
								break;
							}


						}
						catch (System.FormatException) {

						}
					}
					else {
						//can only come for the name, saving it incase nothing matches the target address
						nameBinding = binding;
					}
					i++;
				}

				binding = nameBinding == null ? binding : nameBinding;
			}
	//		else
	//		{
	//			//Just pick up the first one.
	//			binding = addressBindings[0];
	//		}


			//will use this last binding .
			//and currently only TCPIP is supported.
			string address = binding.NetworkAddress;
			if (address.IndexOf("[", StringComparison.Ordinal) == -1) { //this does not contain the port
				string ipAddr = JISystem.GetIPForHostName(address); //to use the binding supplied by the user.
				if (ipAddr != null) {
					address = ipAddr;
				}
				//use 135
				address = address + "[135]";
			}
			else {
				int index = address.IndexOf("[", StringComparison.Ordinal);
				string hostname = binding.NetworkAddress.Substring(0,index);
				string ipAddr = JISystem.GetIPForHostName(hostname); //to use the binding supplied by the user.
				if (ipAddr != null) {
					address = ipAddr + address.Substring(index);
				}
			}
			base.Address = "ncacn_ip_tcp:" + address;
			this.Session = session;
			this.Session.TargetServer = StringHelperClass.SubstringSpecial(Address, Address.IndexOf(":", StringComparison.Ordinal) + 1,Address.IndexOf("[", StringComparison.Ordinal));
			OxidResolver = new JIOxidResolver(((JIStdObjRef)interfacePointer.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Oxid);
			try {

				Syntax_Renamed = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
				attach();
				//first send an AlterContext to the IID of the IOxidResolver
				Endpoint.Syntax.Uuid = new rpc.core.UUID("99fcfec4-5260-101b-bbcb-00aa0021347a");
				Endpoint.Syntax.setVersion(0,0);
				((JIComEndpoint)Endpoint).RebindEndPoint();

				Call(Endpoint.IDEMPOTENT,OxidResolver);
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

			// Now will setup syntax for IRemUnknown and the address.
			//syntax = "00000143-0000-0000-c000-000000000046:0.0";
			Syntax_Renamed = interfacePointer.IID + ":0.0";

			//now for the new ip and the port.

			JIStringBinding[] bindings = OxidResolver.OxidBindings.StringBindings;

			binding = null;
			nameBinding = null;
			i = 0;
	//		if (!targetAddress.equals(""))
			{
				//now we choose, otherwise the first one we get.
				while (i < bindings.Length) {
					binding = bindings[i];
					if (binding.TowerId != 0x07) { //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
						i++;
						continue;
					}
					//get the one with IP address
					int index = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
					if (index != -1) {
						try {

	//						if (binding.getNetworkAddress().equalsIgnoreCase(targetAddress))
							if (ListOfIps.Contains(binding.NetworkAddress.ToLower())) {
								nameBinding = null;
								break;
							}

							//now check for the one with port
							index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
	//						if (index != -1 && binding.getNetworkAddress().substring(0,index).equalsIgnoreCase(targetAddress))
							if (index != -1 && ListOfIps.Contains(binding.NetworkAddress.Substring(0,index).ToLower())) {
								nameBinding = null;
								break;
							}
						}
						catch (System.FormatException) {

						}
					}
					else {
						//can only come for the name, saving it incase nothing matches the target address
						nameBinding = binding;
					}
					i++;
				}

				binding = nameBinding == null ? binding : nameBinding;
			}
	//		else
	//		{
	//			//Just pick up the first one.
	//			binding = bindings[0];
	//		}


			//now set the NTLMv2 Session Security.
			if (session.SessionSecurityEnabled) {
				base.Properties.setProperty("rpc.ntlm.seal", "true");
				base.Properties.setProperty("rpc.ntlm.sign", "true");
				base.Properties.setProperty("rpc.ntlm.keyExchange", "true");
				base.Properties.setProperty("rpc.ntlm.keyLength", "128");
				base.Properties.setProperty("rpc.ntlm.ntlm2", "true");
			}



			address = binding.NetworkAddress; //this will always have the port.
			int index = address.IndexOf("[", StringComparison.Ordinal);
			string hostname = binding.NetworkAddress.Substring(0,index);
			string ipAddr = JISystem.GetIPForHostName(hostname); //to use the binding supplied by the user.
			if (ipAddr != null) {
				address = ipAddr + address.Substring(index);
			}

			//and currently only TCPIP is supported.
			Address = "ncacn_ip_tcp:" + address;
			RemunknownIPID = OxidResolver.IPID;
			InterfacePtrCtor = interfacePointer;
			this.Session.Stub = this;
			this.Session.Stub2 = new JIRemUnknownServer(session, RemunknownIPID, Address);

		}


		/// <summary>
		///<para><code>JIProgId</code> based constructor with the host machine for COM server being <i>LOCALHOST</i>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="progId"> user-friendly string such as "Excel.Application" , "TestCOMServer.Test123" etc. </param>
		/// <param name="session"> session to be associated with. </param>
		/// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code> is associated with another server already. </exception>
		/// <exception cref="IllegalArgumentException"> raised when either <code>progId</code> or <code>session</code> is <code>null</code>. </exception>
		/// <exception cref="UnknownHostException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComServer(JIProgId progId,JISession session) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public JIComServer(JIProgId progId, JISession session) : this(progId,InetAddress.LocalHost.HostAddress,session) {
		}

		/// <summary>
		/// <para><code><seealso cref="JIClsid"/></code> based constructor with the host machine for COM server being <i>LOCALHOST</i>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="clsid"> 128 bit string such as "00024500-0000-0000-C000-000000000046". </param>
		/// <param name="session"> session to be associated with. </param>
		/// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code> is associated with another server already. </exception>
		/// <exception cref="IllegalArgumentException"> raised when either <code>clsid</code> or <code>session</code> is <code>null</code>. </exception>
		/// <exception cref="UnknownHostException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComServer(JIClsid clsid,JISession session) throws IllegalArgumentException,org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public JIComServer(JIClsid clsid, JISession session) : this(clsid,InetAddress.LocalHost.HostAddress,session) {
		}

		/// <summary>
		///<para>Refer <seealso cref="#JIComServer(JIProgId, JISession)"/> for details.
		/// 
		/// </para>
		/// </summary>
		/// <param name="progId"> user-friendly string such as "Excel.Application" , "TestCOMServer.Test123" etc. </param>
		/// <param name="address"> address of the host where the <code>COM</code> object resides.This should be in the IEEE IP format (e.g. 192.168.170.6) or a resolvable HostName. </param>
		/// <param name="session"> session to be associated with. </param>
		/// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code> is associated with another server already. </exception>
		/// <exception cref="IllegalArgumentException"> raised when any of the parameters is <code>null</code>. </exception>
		/// <exception cref="UnknownHostException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComServer(JIProgId progId,String address, JISession session) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public JIComServer(JIProgId progId, string address, JISession session) : base() {

			if (progId == null || address == null || session == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
			}

			if (session.Stub != null) {
				throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
			}

			if (session.SSOEnabled) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS2));
			}

			address = address.Trim();
			address = InetAddress.getByName(address).HostAddress;

			progId.Session = session;
			progId.Server = address;
			address = "ncacn_ip_tcp:" + address + "[135]";
			JIClsid clsid = progId.CorrespondingCLSID;
			Initialise(clsid,address,session);
		}

		/// <summary>
		/// <para>Refer <seealso cref="#JIComServer(JIClsid, JISession)"/> for details.
		/// 
		/// 
		/// </para>
		/// </summary>
		/// <param name="clsid"> 128 bit string such as "00024500-0000-0000-C000-000000000046". </param>
		/// <param name="address"> address of the host where the <code>COM</code> object resides.This should be in the IEEE IP format (e.g. 192.168.170.6) or a resolvable HostName. </param>
		/// <param name="session"> session to be associated with. </param>
		/// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code> is associated with another server already. </exception>
		/// <exception cref="IllegalArgumentException"> raised when any of the parameters is <code>null</code>. </exception>
		/// <exception cref="UnknownHostException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComServer(JIClsid clsid,String address, JISession session) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public JIComServer(JIClsid clsid, string address, JISession session) : base() {

			if (clsid == null || address == null || session == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
			}

			if (session.Stub != null) {
				throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
			}

			address = address.Trim();
			//address = address.replace(' ','');
			address = "ncacn_ip_tcp:" + InetAddress.getByName(address).HostAddress + "[135]";

			Initialise(clsid,address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialise(JIClsid clsid,String address, JISession session) throws org.jinterop.dcom.common.JIException
		private void Initialise(JIClsid clsid, string address, JISession session) {
			base.TransportFactory = JIComTransportFactory.SingleTon;
			//now read the session and prepare information for the stub.
			base.Properties = new Properties(Defaults);
			base.Properties.setProperty("rpc.socketTimeout", (new int?(session.GlobalSocketTimeout)).ToString());
			base.Address = address;

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

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Internal_dumpMap();
			}

			this.Clsid = clsid.CLSID.ToUpper();
			this.Session = session;
			this.Session.TargetServer = StringHelperClass.SubstringSpecial(address, address.IndexOf(":", StringComparison.Ordinal) + 1,address.IndexOf("[", StringComparison.Ordinal));
			try {
				Init();
			}
			catch (JIException e) {
				if (e.ErrorCode == 0x80040154) {
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.warning("Got the class not registered exception , will attempt setting entries based on status flags...");
					}
					//try registering the dll\ocx on our own
					//check for clsid.autoregister flag
					//check for jisystem.autoregister flag.
					//jisystem takes precedence over clsid.

					if (JISystem.AutoRegistrationSet || clsid.AutoRegistrationSet) {

						//first create the registry entries.
						try {
							IJIWinReg registry = null;
							if (session.SSOEnabled) {
								registry = JIWinRegFactory.SingleTon.GetWinreg(session.TargetServer,true);
							}
							else {
								registry = JIWinRegFactory.SingleTon.GetWinreg(new JIDefaultAuthInfoImpl(session.Domain,session.UserName,session.Password),session.TargetServer,true);
							}

							JIPolicyHandle hklm = null;
							JIPolicyHandle hkwow6432 = null;
							try {
								// Try 64bit first...
								hklm = registry.Winreg_OpenHKLM();
								hkwow6432 = registry.Winreg_OpenKey(hklm,"SOFTWARE\\Classes\\Wow6432Node", org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
							}
							catch (JIException) {
							}

							if (hklm != null) {
								registry.Winreg_CloseKey(hklm);
							}

							if (hkwow6432 != null) {
								JISystem.Logger.info("Attempting to register on 64 bit");
								// HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\ -- "AppID"
								JIPolicyHandle key = registry.Winreg_CreateKey(hkwow6432, "CLSID\\{" + this.Clsid + "}", org.jinterop.winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
								registry.Winreg_SetValue(key, "AppId", ("{" + this.Clsid + "}").Bytes, false, false);
								registry.Winreg_CloseKey(key);
								JISystem.Logger.info("--- winreg_SetValue --- SOFTWARE\\Classes\\Wow6432Node\\CLSID\\" + this.Clsid + " -- AppID");

								// HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\AppID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\AppID\ -- "DllSurrogate"
								key = registry.Winreg_CreateKey(hkwow6432, "AppID\\{" + this.Clsid + "}", org.jinterop.winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
								registry.Winreg_SetValue(key, "DllSurrogate", "".GetBytes(), false, false);
								registry.Winreg_CloseKey(key);

								JISystem.Logger.info("--- winreg_SetValue --- SOFTWARE\\Classes\\Wow6432Node\\AppID\\" + this.Clsid + " -- DllSurrogate");
								registry.Winreg_CloseKey(hkwow6432);
							}
							else {
								JISystem.Logger.info("Attempting to register on 32 bit");
								JIPolicyHandle hkcr = registry.Winreg_OpenHKCR();
								JIPolicyHandle key = registry.Winreg_CreateKey(hkcr,"CLSID\\{" + this.Clsid + "}",org.jinterop.winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE,org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
								registry.Winreg_SetValue(key,"AppID",("{" + this.Clsid + "}").Bytes,false,false);
								registry.Winreg_CloseKey(key);
								key = registry.Winreg_CreateKey(hkcr,"AppID\\{" + this.Clsid + "}",org.jinterop.winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE,org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
								registry.Winreg_SetValue(key,"DllSurrogate", "  ".GetBytes(),false,false);

								registry.Winreg_CloseKey(key);
								registry.Winreg_CloseKey(hkcr);
							}
							registry.CloseConnection();
						}
						catch (UnknownHostException e1) {
							//auto registration failed as well...
							JISystem.Logger.throwing("JIComServer","initialise",e1);
							throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3,e1);
						}
						//lets retry
						Init();
					}
					else {
						throw e;
					}
				}
				else {
					throw e;
				}

			}

			this.Session.Stub = this;
			this.Session.Stub2 = new JIRemUnknownServer(session, RemunknownIPID, Address);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void init() throws org.jinterop.dcom.common.JIException
		private void Init() {
			if (ServerActivation != null && ServerActivation.ActivationSuccessful) {
				return;
			}

			bool attachcomplete = false;
			try {
				Syntax_Renamed = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
				attach();
				// socket to COM server is established
				attachcomplete = true;
				//first send an AlterContext to the IID of the IOxidResolver
				Endpoint.Syntax.Uuid = new rpc.core.UUID("99fcfec4-5260-101b-bbcb-00aa0021347a");
				Endpoint.Syntax.setVersion(0,0);
				((JIComEndpoint)Endpoint).RebindEndPoint();

				//3.2.4.1.1.1 Determining RPC Binding Information for Activation
				//Commenting the below to dynamically identify DCOM versions.			
	//			JICallBuilder serverAlive = new JICallBuilder(true);
	//			serverAlive.attachSession(session);
	//			serverAlive.setOpnum(0);
	//			serverAlive.setReadOnlyHRESULT();
	//			call(Endpoint.IDEMPOTENT,serverAlive);

				JICallBuilder serverAlive = new JICallBuilder(true);
				serverAlive.AttachSession(Session);
				serverAlive.Opnum = 2;
				serverAlive.Internal_COMVersion();
				try {
					Call(Endpoint.IDEMPOTENT,serverAlive);
					JISystem.COMVersion = serverAlive.Internal_getComVersion();
				}
				catch (JIRuntimeException e) {
					if (e.HResult == JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE) {
						JISystem.COMVersion.MajorVersion = 5;
						JISystem.COMVersion.MinorVersion = 1;
					}
				}

				if (JISystem.COMVersion != null && JISystem.COMVersion.MinorVersion > 1) {
					//use SCMActivator
					Syntax_Renamed = "000001A0-0000-0000-C000-000000000046:0.0";
					Endpoint.Syntax.Uuid = new rpc.core.UUID("000001A0-0000-0000-C000-000000000046");
					Endpoint.Syntax.setVersion(0,0);
					((JIComEndpoint)Endpoint).RebindEndPoint();
					ServerActivation = new JIRemoteSCMActivator.RemoteCreateInstance((new JIRemoteSCMActivator()), Session.TargetServer, Clsid);
					Call(Endpoint.IDEMPOTENT, (JIRemoteSCMActivator.RemoteCreateInstance)ServerActivation);

				}
				else {
					//setup syntax for IRemoteActivation
					Syntax_Renamed = "4d9f4ab8-7d1c-11cf-861e-0020af6e7c57:0.0";
					Endpoint.Syntax.Uuid = new rpc.core.UUID("4d9f4ab8-7d1c-11cf-861e-0020af6e7c57");
					Endpoint.Syntax.setVersion(0,0);
					((JIComEndpoint)Endpoint).RebindEndPoint();
					ServerActivation = new JIRemActivation(Clsid);
					Call(Endpoint.IDEMPOTENT,(JIRemActivation)ServerActivation);
				}
			}
			catch (FaultException e) {
				ServerActivation = null;
				throw new JIException(e.status,e);
			}
			catch (IOException e) {
				ServerActivation = null;
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e1) {
				ServerActivation = null;
				throw new JIException(e1);
			}
			finally {
				//the only time remactivation will be null will be case of an exception.
				if (attachcomplete && ServerActivation == null) {
					try {
						detach();
					}
					catch (IOException e) {
						if (JISystem.Logger.isLoggable(Level.WARNING)) {
							JISystem.Logger.warning("Unable to detach during init: " + e);
						}
					}
				}
			}

			// Now will setup syntax for IRemUnknown and the address.
			Syntax_Renamed = "00000143-0000-0000-c000-000000000046:0.0";
			//now for the new ip and the port.

			JIStringBinding[] bindings = ServerActivation.DualStringArrayForOxid.StringBindings;
			int i = 0;
			JIStringBinding binding = null;
			JIStringBinding nameBinding = null;
			string targetAddress = Address;
			targetAddress = StringHelperClass.SubstringSpecial(targetAddress, targetAddress.IndexOf(':') + 1,targetAddress.IndexOf('['));
			while (i < bindings.Length) {
				binding = bindings[i];
				if (binding.TowerId != 0x07) { //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
					i++;
					continue;
				}
				//get the one with IP address
				int index = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
				if (index != -1) {
					try {
						//Integer.parseInt(binding.getNetworkAddress().substring(0,index));
						index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
						if (index != -1 && binding.NetworkAddress.Substring(0,index).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase)) {
							break;
						}
					}
					catch (System.FormatException) {

					}
				}
				else {
					//can only come for the name, saving it incase nothing matches the target address
					//then we are not sure which is the right IP and which might be virtual, refer to
					//issue faced by Igor.
					nameBinding = binding;
					index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
					if (binding.NetworkAddress.Substring(0,index).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase)) {
						break;
					}
				}
				i++;
			}

			if (binding == null) {
				binding = nameBinding;
			}
			//will use this last binding .
			//and currently only TCPIP is supported.
			//now set the NTLMv2 Session Security.
			if (Session.SessionSecurityEnabled) {
				base.Properties.setProperty("rpc.ntlm.seal", "true");
				base.Properties.setProperty("rpc.ntlm.sign", "true");
				base.Properties.setProperty("rpc.ntlm.keyExchange", "true");
				base.Properties.setProperty("rpc.ntlm.keyLength", "128");
				base.Properties.setProperty("rpc.ntlm.ntlm2", "true");
			}



			string address = binding.NetworkAddress; //this will always have the port.
			int index = address.IndexOf("[", StringComparison.Ordinal);
			string hostname = binding.NetworkAddress.Substring(0,index);
			string ipAddr = JISystem.GetIPForHostName(hostname); //to use the binding supplied by the user.
			if (ipAddr != null) {
				address = ipAddr + address.Substring(index);
			}

			//and currently only TCPIP is supported.
			Address = "ncacn_ip_tcp:" + address;
			RemunknownIPID = ServerActivation.IPID;
		}



		//Will give a call to IRemUnknown for the passed IID.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: IJIComObject getInterface(String iid,String ipidOfTheTargetUnknown) throws org.jinterop.dcom.common.JIException
		public IJIComObject GetInterface(string iid, string ipidOfTheTargetUnknown) {
			IJIComObject retval = null;
			//this is still essentially serial, since all threads will have to wait for mutex before
			//entering addToSession.
			lock (Mutex) {
				//now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation
				Object = RemunknownIPID;
				//setObject(ipid);

				//JIRemUnknown reqUnknown = new JIRemUnknown(unknownIPID,iid,5);
				JIRemUnknown reqUnknown = new JIRemUnknown(ipidOfTheTargetUnknown,iid);
				try {
					this.Session.Stub2.call(Endpoint.IDEMPOTENT,reqUnknown);
				}
				catch (FaultException e) {
					throw new JIException(e.status,e);
				}
				catch (IOException e) {
					throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
				}
				catch (JIRuntimeException e1) {
					//remoteActivation = null;
					throw new JIException(e1);
				}

				retval = JIFrameworkHelper.InstantiateComObject(Session, reqUnknown.InterfacePointer);
				//increasing the reference count.
				retval.AddRef();
				//for querying dispatch we can't send another call
				if (!iid.Equals("00020400-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
					bool success = true;
					((JIComObjectImpl)retval).IsDual = true;
					//now to check whether it supports IDispatch
					//IDispatch 00020400-0000-0000-c000-000000000046
					JIRemUnknown dispatch = new JIRemUnknown(retval.Ipid,"00020400-0000-0000-c000-000000000046");
					try {
						this.Session.Stub2.call(Endpoint.IDEMPOTENT,dispatch);
					}
					catch (FaultException e) {
						throw new JIException(e.status,e);
					}
					catch (IOException e) {
						throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
					}
					catch (JIRuntimeException) {
						//will eat this exception here.
						((JIComObjectImpl)retval).IsDual = false;
						success = false;
					}

					if (success) {
						//which means that IDispatch is supported
						Session.ReleaseRef(dispatch.InterfacePointer.IPID,((JIStdObjRef)dispatch.InterfacePointer.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs);
					}
				}
			}

			return retval;

		}





		/// <summary>
		///Returns an <code>IJIComObject</code> representing the COM Server.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIComObject createInstance() throws org.jinterop.dcom.common.JIException
		public IJIComObject CreateInstance() {
			if (InterfacePtrCtor != null) {
				throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMSTUB_WRONGCALLCREATEINSTANCE));
			}
			IJIComObject comObject = null;

			//This method is still essentially serial, since all threads will have to stop at mutex and then
			//go to addToSession after it (since there is no condition).
			lock (Mutex) {
				if (ServerInstantiated) {
					throw new JIException(JIErrorCodes.JI_OBJECT_ALREADY_INSTANTIATED,(Exception)null);
				}
	//			JIStdObjRef objRef = (JIStdObjRef)(remoteActivation.getMInterfacePointer().getObjectReference(JIInterfacePointer.OBJREF_STANDARD));
	//			comObject = getObject(objRef.getIpid(),IJIUnknown.IID);
				comObject = JIFrameworkHelper.InstantiateComObject(Session, ServerActivation.MInterfacePointer);
				if (ServerActivation.Dual) {
					//IJIComObject comObject2 = getObject(remoteActivation.dispIpid,"00020400-0000-0000-c000-000000000046");
					//this will get garbage collected and then removed.
					//session.addToSession(comObject2,remoteActivation.dispOid);
					Session.ReleaseRef(ServerActivation.DispIpid,ServerActivation.DispRefs);
					ServerActivation.DispIpid = null;
					((JIComObjectImpl)comObject).IsDual = true;
				}
				else {
					((JIComObjectImpl)comObject).IsDual = false;
				}
				//increasing the reference count.
				comObject.AddRef();
				ServerInstantiated = true;
			}

			return comObject;
		}

		/// <summary>
		///Returns a <code>IJIComObject</code> representing the <code>COM</code> Server. To be used only with <code>JIComServer(JISession,JIInterfacePointer,String)</code> ctor,
		/// otherwise use createInstance() instead.
		/// 
		/// @return </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: IJIComObject getInstance() throws org.jinterop.dcom.common.JIException
		public IJIComObject Instance {
			get {
				if (InterfacePtrCtor == null) {
					throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMSTUB_WRONGCALLGETINSTANCE));
				}
    
				IJIComObject comObject = null;
				//This method is still essentially serial, since all threads will have to stop at mutex and then
				//go to addToSession after it (since there is no condition).
				lock (Mutex) {
					if (ServerInstantiated) {
						throw new JIException(JIErrorCodes.JI_OBJECT_ALREADY_INSTANTIATED,(Exception)null);
					}
    
		//			JIStdObjRef objRef = (JIStdObjRef)(interfacePtrCtor.getObjectReference(JIInterfacePointer.OBJREF_STANDARD));
		//			comObject = getObject(objRef.getIpid(),interfacePtrCtor.getIID());
					comObject = JIFrameworkHelper.InstantiateComObject(Session,InterfacePtrCtor);
					//increasing the reference count.
					comObject.AddRef();
					ServerInstantiated = true;
				}
    
				return comObject;
			}
		}


		public string Syntax {
			get {
				return Syntax_Renamed;
			}
		}

	//	/**
	//	 * @exclude
	//	 * @return
	//	 */
	//	String getIpid()
	//	{
	//		if (remoteActivation != null && remoteActivation.isActivationSuccessful())
	//		{
	//			return remoteActivation.getIPID();
	//		}
	//		else
	//			return null;
	//	}

		/// <summary>
		/// Execute a Method on the COM Interface identified by the IID.
		/// 
		/// 
		/// @exclude </summary>
		/// <param name="obj"> </param>
		/// <param name="targetIID">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object[] call(JICallBuilder obj,String targetIID) throws org.jinterop.dcom.common.JIException
		public object[] Call(JICallBuilder obj, string targetIID) {
			return Call(obj, targetIID, Session.GlobalSocketTimeout);
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
					Call(Endpoint.IDEMPOTENT,obj);

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

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public JIInterfacePointer ServerInterfacePointer {
			get {
				//remoteactivation can be null only incase of OxidResolver ctor getting called.
				return ServerActivation == null ? InterfacePtrCtor : ServerActivation.MInterfacePointer;
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
					Call(obj,JIRemUnknown.IID_IUnknown);
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
			catch (Exception) {
	//			No need to print this out.
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