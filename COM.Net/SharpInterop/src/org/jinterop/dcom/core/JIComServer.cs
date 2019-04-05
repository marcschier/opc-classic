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
    using org.jinterop.dcom.transport;
    using org.jinterop.winreg;
    using rpc;
    using Serilog;
    using System;
    using System.IO;

    /// <summary>
    /// Startup class representing a COM Server.
    /// Sample Usage :-
    ///  <code>
    ///   <seealso cref="JISession"/> session = JISession.createSession("DOMAIN","USERNAME","PASSWORD"); 
    ///   JIComServer excelServer = new JIComServer(JIProgId.valueOf("Excel.Application"),address,session); 
    ///   IJIComObject comObject = excelServer.createInstance(); 
    ///   //Obtaining the IJIDispatch (if supported) 
    ///   <seealso cref="impls.automation.IJIDispatch"/> dispatch = 
    ///     (IJIDispatch)<seealso cref="impls.JIObjectFactory"/>.narrowObject(comObject.queryInterface(IJIDispatch.IID)); 
    ///   </code>
    /// 
    /// Each instance of this class is associated with a single session only.
    /// </summary>
    public sealed class JIComServer : Stub {

        private static SharpCifs.Util.Sharpen.Properties defaults = new SharpCifs.Util.Sharpen.Properties();
        static JIComServer() {

            defaults.put("rpc.ntlm.lanManagerKey", "false");
            defaults.put("rpc.ntlm.sign", "false");
            defaults.put("rpc.ntlm.seal", "false");
            defaults.put("rpc.ntlm.keyExchange", "false");
            defaults.put("rpc.ntlm.sso", "false");
            defaults.put("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
            defaults.put("rpc.socketTimeout", 0.ToString());
            //		rpc.connectionContext = rpc.security.ntlm.NtlmConnectionContext
            //		rpc.ntlm.sign = false
            //		rpc.ntlm.seal = false
            //		rpc.ntlm.keyExchange = false

        }

        //private String address = null;
        //	private JIRemActivation remoteActivation = null;
        private JIIServerActivation serverActivation;
        private JIOxidResolver oxidResolver;
        private string clsid;
        private JISession session;
        private bool serverInstantiated;
        private string remunknownIPID;
        private readonly object mutex = new object();
        private bool timeoutModifiedfrom0;
        private readonly JIInterfacePointer interfacePtrCtor;
        private static readonly IList<string> listOfIps = new List<string>();

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
        internal JIComServer(JISession session, JIInterfacePointer interfacePointer, string ipAddress) : base() {

            if (interfacePointer == null || session == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
            }

            if (session.Stub != null) {
                throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
            }

            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                JISystem.internal_dumpMap();
            }

            //		ipAddress="192.168.1.104";
            if (ipAddress != null && !ipAddress.Trim().Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                if (!listOfIps.Contains(ipAddress)) {
                    listOfIps.Add(ipAddress.ToLower());
                }
            }

            TransportFactory = JIComTransportFactory.SingleTon;
            //now read the session and prepare information for the stub.
            SharpCifs.Util.Sharpen.Properties = new SharpCifs.Util.Sharpen.Properties(defaults);
            SharpCifs.Util.Sharpen.Properties.setProperty("rpc.security.username", session.UserName);
            SharpCifs.Util.Sharpen.Properties.setProperty("rpc.security.password", session.Password);
            SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.domain", session.Domain);
            SharpCifs.Util.Sharpen.Properties.setProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString());
            if (session.NTLMv2Enabled) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.ntlmv2", "true");
            }
            if (session.SSOEnabled) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.sso", "true");
            }


            var addressBindings = interfacePointer.StringBindings.StringBindings;

            var i = 0;
            JIStringBinding binding = null;
            JIStringBinding nameBinding = null;
            var targetAddress = ipAddress == null ? "" : ipAddress.Trim();

            {
                //		if (!targetAddress.equals(""))
                //now we choose, otherwise the first one we get.
                while (i < addressBindings.Length) {
                    binding = addressBindings[i];
                    if (binding.TowerId != 0x07) //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                    {
                        i++;
                        continue;
                    }
                    //get the one with IP address
                    var index = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
                    if (index != -1) {
                        try {

                            //						if (binding.getNetworkAddress().equalsIgnoreCase(targetAddress))
                            if (listOfIps.Contains(binding.NetworkAddress.ToLower())) {
                                nameBinding = null;
                                break;
                            }

                            //now check for the one with port
                            index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                                                                                                   //						if (index != -1 && binding.getNetworkAddress().substring(0,index).equalsIgnoreCase(targetAddress))
                            if (index != -1 && listOfIps.Contains(binding.NetworkAddress.Substring(0, index).ToLower())) {
                                nameBinding = null;
                                break;
                            }


                        }
                        catch (FormatException) {

                        }
                    }
                    else {
                        //can only come for the name, saving it incase nothing matches the target address
                        nameBinding = binding;
                    }
                    i++;
                }

                binding = nameBinding ?? binding;
            }
            //		else
            //		{
            //			//Just pick up the first one.
            //			binding = addressBindings[0];
            //		}


            //will use this last binding .
            //and currently only TCPIP is supported.
            var address = binding.NetworkAddress;
            if (address.IndexOf("[", StringComparison.Ordinal) == -1) //this does not contain the port
            {
                var ipAddr = JISystem.getIPForHostName(address); //to use the binding supplied by the user.
                if (ipAddr != null) {
                    address = ipAddr;
                }
                //use 135
                address = address + "[135]";
            }
            else {
                var index = address.IndexOf("[", StringComparison.Ordinal);
                var hostname = binding.NetworkAddress.Substring(0, index);
                var ipAddr = JISystem.getIPForHostName(hostname); //to use the binding supplied by the user.
                if (ipAddr != null) {
                    address = ipAddr + address.Substring(index);
                }
            }
            Address = "ncacn_ip_tcp:" + address;
            this.session = session;
            this.session.TargetServer = StringHelperClass.SubstringSpecial(Address, Address.IndexOf(":", StringComparison.Ordinal) + 1, Address.IndexOf("[", StringComparison.Ordinal));
            oxidResolver = new JIOxidResolver(((JIStdObjRef)interfacePointer.getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Oxid);
            try {

                Syntax = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
                Attach();
                //first send an AlterContext to the IID of the IOxidResolver
                Endpoint.Syntax.Uuid = new rpc.core.UUID("99fcfec4-5260-101b-bbcb-00aa0021347a");
                Endpoint.Syntax.Version = 0;
                ((JIComEndpoint)Endpoint).rebindEndPoint();

                call(Endpoint.IDEMPOTENT, oxidResolver);
            }
            catch (FaultException e) {
                throw new JIException(e._code, e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e1) {
                throw new JIException(e1);
            }

            // Now will setup syntax for IRemUnknown and the address.
            //syntax = "00000143-0000-0000-c000-000000000046:0.0";
            Syntax = interfacePointer.IID + ":0.0";

            //now for the new ip and the port.

            var bindings = oxidResolver.OxidBindings.StringBindings;

            binding = null;
            nameBinding = null;
            i = 0;
            //		if (!targetAddress.equals(""))
            {
                //now we choose, otherwise the first one we get.
                while (i < bindings.Length) {
                    binding = bindings[i];
                    if (binding.TowerId != 0x07) //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                    {
                        i++;
                        continue;
                    }
                    //get the one with IP address
                    var index = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
                    if (index != -1) {
                        try {

                            //						if (binding.getNetworkAddress().equalsIgnoreCase(targetAddress))
                            if (listOfIps.Contains(binding.NetworkAddress.ToLower())) {
                                nameBinding = null;
                                break;
                            }

                            //now check for the one with port
                            index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                                                                                                   //						if (index != -1 && binding.getNetworkAddress().substring(0,index).equalsIgnoreCase(targetAddress))
                            if (index != -1 && listOfIps.Contains(binding.NetworkAddress.Substring(0, index).ToLower())) {
                                nameBinding = null;
                                break;
                            }
                        }
                        catch (FormatException) {

                        }
                    }
                    else {
                        //can only come for the name, saving it incase nothing matches the target address
                        nameBinding = binding;
                    }
                    i++;
                }

                binding = nameBinding ?? binding;
            }
            //		else
            //		{
            //			//Just pick up the first one.
            //			binding = bindings[0];
            //		}


            //now set the NTLMv2 Session Security.
            if (session.SessionSecurityEnabled) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.seal", "true");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.sign", "true");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.keyExchange", "true");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.keyLength", "128");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.ntlm2", "true");
            }



            address = binding.NetworkAddress; //this will always have the port.
            var index = address.IndexOf("[", StringComparison.Ordinal);
            var hostname = binding.NetworkAddress.Substring(0, index);
            var ipAddr = JISystem.getIPForHostName(hostname); //to use the binding supplied by the user.
            if (ipAddr != null) {
                address = ipAddr + address.Substring(index);
            }

            //and currently only TCPIP is supported.
            Address = "ncacn_ip_tcp:" + address;
            remunknownIPID = oxidResolver.IPID;
            interfacePtrCtor = interfacePointer;
            this.session.Stub = this;
            this.session.Stub2 = new JIRemUnknownServer(session, remunknownIPID, Address);

        }


        /// <summary>
        ///<para><code>JIProgId</code> based constructor with the host machine for COM server being <i>LOCALHOST</i>.
        /// 
        /// </para>
        /// </summary>
        /// <param name="progId"> user-friendly string such as "Excel.Application" , "TestCOMServer.Test123" etc. </param>
        /// <param name="session"> session to be associated with. </param>
        /// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code> is associated with another server already. </exception>
        /// <exception cref="ArgumentException"> raised when either <code>progId</code> or <code>session</code> is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public JIComServer(JIProgId progId,JISession session) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public JIComServer(JIProgId progId, JISession session) : this(progId, InetAddress.LocalHost.HostAddress, session) {
        }

        /// <summary>
        /// <para><code><seealso cref="JIClsid"/></code> based constructor with the host machine for COM server being <i>LOCALHOST</i>.
        /// 
        /// </para>
        /// </summary>
        /// <param name="clsid"> 128 bit string such as "00024500-0000-0000-C000-000000000046". </param>
        /// <param name="session"> session to be associated with. </param>
        /// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code> is associated with another server already. </exception>
        /// <exception cref="ArgumentException"> raised when either <code>clsid</code> or <code>session</code> is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public JIComServer(JIClsid clsid,JISession session) throws System.ArgumentException,org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public JIComServer(JIClsid clsid, JISession session) : this(clsid, InetAddress.LocalHost.HostAddress, session) {
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
        /// <exception cref="ArgumentException"> raised when any of the parameters is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public JIComServer(JIProgId progId,String address, JISession session) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public JIComServer(JIProgId progId, string address, JISession session) : base() {

            if (progId == null || address == null || session == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
            }

            if (session.Stub != null) {
                throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
            }

            if (session.SSOEnabled) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS2));
            }

            address = address.Trim();
            address = InetAddress.getByName(address).HostAddress;

            progId.Session = session;
            progId.Server = address;
            address = "ncacn_ip_tcp:" + address + "[135]";
            var clsid = progId.CorrespondingCLSID;
            initialise(clsid, address, session);
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
        /// <exception cref="ArgumentException"> raised when any of the parameters is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public JIComServer(JIClsid clsid,String address, JISession session) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public JIComServer(JIClsid clsid, string address, JISession session) : base() {

            if (clsid == null || address == null || session == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
            }

            if (session.Stub != null) {
                throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
            }

            address = address.Trim();
            //address = address.replace(' ','');
            address = "ncacn_ip_tcp:" + InetAddress.getByName(address).HostAddress + "[135]";

            initialise(clsid, address, session);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void initialise(JIClsid clsid,String address, JISession session) throws org.jinterop.dcom.common.JIException
        private void initialise(JIClsid clsid, string address, JISession session) {
            TransportFactory = JIComTransportFactory.SingleTon;
            //now read the session and prepare information for the stub.
            SharpCifs.Util.Sharpen.Properties = new SharpCifs.Util.Sharpen.Properties(defaults);
            SharpCifs.Util.Sharpen.Properties.setProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString());
            Address = address;

            if (session.NTLMv2Enabled) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.ntlmv2", "true");
            }

            if (session.SSOEnabled) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.sso", "true");
            }
            else {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.security.username", session.UserName);
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.security.password", session.Password);
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.domain", session.Domain);
            }

            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                JISystem.internal_dumpMap();
            }

            this.clsid = clsid.CLSID.ToUpper();
            this.session = session;
            this.session.TargetServer = StringHelperClass.SubstringSpecial(address, address.IndexOf(":", StringComparison.Ordinal) + 1, address.IndexOf("[", StringComparison.Ordinal));
            try {
                init();
            }
            catch (JIException e) {
                if ((uint)e.ErrorCode == 0x80040154) {
                    Log.Logger.Warning("Got the class not registered exception , will attempt setting entries based on status flags...");
                    //try registering the dll\ocx on our own
                    //check for clsid.autoregister flag
                    //check for jisystem.autoregister flag.
                    //jisystem takes precedence over clsid.

                    if (JISystem.AutoRegistrationSet || clsid.AutoRegistrationSet) {

                        //first create the registry entries.
                        try {
                            IJIWinReg registry = null;
                            if (session.SSOEnabled) {
                                registry = JIWinRegFactory.SingleTon.getWinreg(session.TargetServer, true);
                            }
                            else {
                                registry = JIWinRegFactory.SingleTon.getWinreg(new JIDefaultAuthInfoImpl(session.Domain, session.UserName, session.Password), session.TargetServer, true);
                            }

                            JIPolicyHandle hklm = null;
                            JIPolicyHandle hkwow6432 = null;
                            try {
                                // Try 64bit first...
                                hklm = registry.winreg_OpenHKLM();
                                hkwow6432 = registry.winreg_OpenKey(hklm, "SOFTWARE\\Classes\\Wow6432Node", winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                            }
                            catch (JIException) {
                            }

                            if (hklm != null) {
                                registry.winreg_CloseKey(hklm);
                            }

                            if (hkwow6432 != null) {
                                Log.Logger.Information("Attempting to register on 64 bit");
                                // HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\ -- "AppID"
                                var key = registry.winreg_CreateKey(hkwow6432, "CLSID\\{" + this.clsid + "}", winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "AppId", ("{" + this.clsid + "}").Bytes, false, false);
                                registry.winreg_CloseKey(key);
                                Log.Logger.Information("--- winreg_SetValue --- SOFTWARE\\Classes\\Wow6432Node\\CLSID\\" + this.clsid + " -- AppID");

                                // HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\AppID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\AppID\ -- "DllSurrogate"
                                key = registry.winreg_CreateKey(hkwow6432, "AppID\\{" + this.clsid + "}", winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "DllSurrogate", "".GetBytes(), false, false);
                                registry.winreg_CloseKey(key);

                                Log.Logger.Information("--- winreg_SetValue --- SOFTWARE\\Classes\\Wow6432Node\\AppID\\" + this.clsid + " -- DllSurrogate");
                                registry.winreg_CloseKey(hkwow6432);
                            }
                            else {
                                Log.Logger.Information("Attempting to register on 32 bit");
                                var hkcr = registry.winreg_OpenHKCR();
                                var key = registry.winreg_CreateKey(hkcr, "CLSID\\{" + this.clsid + "}", winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "AppID", ("{" + this.clsid + "}").Bytes, false, false);
                                registry.winreg_CloseKey(key);
                                key = registry.winreg_CreateKey(hkcr, "AppID\\{" + this.clsid + "}", winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "DllSurrogate", "  ".GetBytes(), false, false);

                                registry.winreg_CloseKey(key);
                                registry.winreg_CloseKey(hkcr);
                            }
                            registry.closeConnection();
                        }
                        catch (UnknownHostException e1) {
                            //auto registration failed as well...
                            Log.Logger.Error(e, "JIComServer", "initialise", e1);
                            throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3, e1);
                        }
                        //lets retry
                        init();
                    }
                    else {
                        throw e;
                    }
                }
                else {
                    throw e;
                }

            }

            this.session.Stub = this;
            this.session.Stub2 = new JIRemUnknownServer(session, remunknownIPID, Address);
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void init() throws org.jinterop.dcom.common.JIException
        private void init() {
            if (serverActivation != null && serverActivation.ActivationSuccessful) {
                return;
            }

            var attachcomplete = false;
            try {
                Syntax = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
                Attach();
                // socket to COM server is established
                attachcomplete = true;
                //first send an AlterContext to the IID of the IOxidResolver
                Endpoint.Syntax.Uuid = new rpc.core.UUID("99fcfec4-5260-101b-bbcb-00aa0021347a");
                Endpoint.Syntax.Version = 0;
                ((JIComEndpoint)Endpoint).rebindEndPoint();

                //3.2.4.1.1.1 Determining RPC Binding Information for Activation
                //Commenting the below to dynamically identify DCOM versions.			
                //			JICallBuilder serverAlive = new JICallBuilder(true);
                //			serverAlive.attachSession(session);
                //			serverAlive.setOpnum(0);
                //			serverAlive.setReadOnlyHRESULT();
                //			call(Endpoint.IDEMPOTENT,serverAlive);

                var serverAlive = new JICallBuilder(true);
                serverAlive.attachSession(session);
                serverAlive.Opnum = 2;
                serverAlive.internal_COMVersion();
                try {
                    call(Endpoint.IDEMPOTENT, serverAlive);
                    JISystem.COMVersion = serverAlive.internal_getComVersion();
                }
                catch (JIRuntimeException e) {
                    if (e.HResult == JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE) {
                        JISystem.COMVersion.MajorVersion = 5;
                        JISystem.COMVersion.MinorVersion = 1;
                    }
                }

                if (JISystem.COMVersion != null && JISystem.COMVersion.MinorVersion > 1) {
                    //use SCMActivator
                    Syntax = "000001A0-0000-0000-C000-000000000046:0.0";
                    Endpoint.Syntax.Uuid = new rpc.core.UUID("000001A0-0000-0000-C000-000000000046");
                    Endpoint.Syntax.Version = 0;
                    ((JIComEndpoint)Endpoint).rebindEndPoint();
                    serverActivation = new JIRemoteSCMActivator.RemoteCreateInstance(new JIRemoteSCMActivator(), session.TargetServer, clsid);
                    call(Endpoint.IDEMPOTENT, (JIRemoteSCMActivator.RemoteCreateInstance)serverActivation);

                }
                else {
                    //setup syntax for IRemoteActivation
                    Syntax = "4d9f4ab8-7d1c-11cf-861e-0020af6e7c57:0.0";
                    Endpoint.Syntax.Uuid = new rpc.core.UUID("4d9f4ab8-7d1c-11cf-861e-0020af6e7c57");
                    Endpoint.Syntax.Version = 0;
                    ((JIComEndpoint)Endpoint).rebindEndPoint();
                    serverActivation = new JIRemActivation(clsid);
                    call(Endpoint.IDEMPOTENT, (JIRemActivation)serverActivation);
                }
            }
            catch (FaultException e) {
                serverActivation = null;
                throw new JIException(e._code, e);
            }
            catch (IOException e) {
                serverActivation = null;
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e1) {
                serverActivation = null;
                throw new JIException(e1);
            }
            finally {
                //the only time remactivation will be null will be case of an exception.
                if (attachcomplete && serverActivation == null) {
                    try {
                        Detach();
                    }
                    catch (IOException e) {
                        Log.Logger.Warning(e, "Unable to detach during init");
                    }
                }
            }

            // Now will setup syntax for IRemUnknown and the address.
            Syntax = "00000143-0000-0000-c000-000000000046:0.0";
            //now for the new ip and the port.

            var bindings = serverActivation.DualStringArrayForOxid.StringBindings;
            var i = 0;
            JIStringBinding binding = null;
            JIStringBinding nameBinding = null;
            var targetAddress = Address;
            targetAddress = StringHelperClass.SubstringSpecial(targetAddress, targetAddress.IndexOf(':') + 1, targetAddress.IndexOf('['));
            while (i < bindings.Length) {
                binding = bindings[i];
                if (binding.TowerId != 0x07) //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                {
                    i++;
                    continue;
                }
                //get the one with IP address
                var index = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
                if (index != -1) {
                    try {
                        //Integer.parseInt(binding.getNetworkAddress().substring(0,index));
                        index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                        if (index != -1 && binding.NetworkAddress.Substring(0, index).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase)) {
                            break;
                        }
                    }
                    catch (FormatException) {

                    }
                }
                else {
                    //can only come for the name, saving it incase nothing matches the target address
                    //then we are not sure which is the right IP and which might be virtual, refer to
                    //issue faced by Igor.
                    nameBinding = binding;
                    index = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                    if (binding.NetworkAddress.Substring(0, index).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase)) {
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
            if (session.SessionSecurityEnabled) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.seal", "true");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.sign", "true");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.keyExchange", "true");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.keyLength", "128");
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.ntlm2", "true");
            }



            var address = binding.NetworkAddress; //this will always have the port.
            var index = address.IndexOf("[", StringComparison.Ordinal);
            var hostname = binding.NetworkAddress.Substring(0, index);
            var ipAddr = JISystem.getIPForHostName(hostname); //to use the binding supplied by the user.
            if (ipAddr != null) {
                address = ipAddr + address.Substring(index);
            }

            //and currently only TCPIP is supported.
            Address = "ncacn_ip_tcp:" + address;
            remunknownIPID = serverActivation.IPID;
        }



        //Will give a call to IRemUnknown for the passed IID.
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: IJIComObject getInterface(String iid,String ipidOfTheTargetUnknown) throws org.jinterop.dcom.common.JIException
        internal IJIComObject getInterface(string iid, string ipidOfTheTargetUnknown) {
            IJIComObject retval = null;
            //this is still essentially serial, since all threads will have to wait for mutex before
            //entering addToSession.
            lock (mutex) {
                //now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation
                Object = remunknownIPID;
                //setObject(ipid);

                //JIRemUnknown reqUnknown = new JIRemUnknown(unknownIPID,iid,5);
                var reqUnknown = new JIRemUnknown(ipidOfTheTargetUnknown, iid);
                try {
                    session.Stub2.call(Endpoint.IDEMPOTENT, reqUnknown);
                }
                catch (FaultException e) {
                    throw new JIException(e._code, e);
                }
                catch (IOException e) {
                    throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
                }
                catch (JIRuntimeException e1) {
                    //remoteActivation = null;
                    throw new JIException(e1);
                }

                retval = JIFrameworkHelper.instantiateComObject(session, reqUnknown.InterfacePointer);
                //increasing the reference count.
                retval.addRef();
                //for querying dispatch we can't send another call
                if (!iid.Equals("00020400-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                    var success = true;
                    ((JIComObjectImpl)retval).IsDual = true;
                    //now to check whether it supports IDispatch
                    //IDispatch 00020400-0000-0000-c000-000000000046
                    var dispatch = new JIRemUnknown(retval.Ipid, "00020400-0000-0000-c000-000000000046");
                    try {
                        session.Stub2.call(Endpoint.IDEMPOTENT, dispatch);
                    }
                    catch (FaultException e) {
                        throw new JIException(e._code, e);
                    }
                    catch (IOException e) {
                        throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
                    }
                    catch (JIRuntimeException) {
                        //will eat this exception here.
                        ((JIComObjectImpl)retval).IsDual = false;
                        success = false;
                    }

                    if (success) {
                        //which means that IDispatch is supported
                        session.releaseRef(dispatch.InterfacePointer.IPID, ((JIStdObjRef)dispatch.InterfacePointer.getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs);
                    }
                }
            }

            return retval;

        }





        /// <summary>
        ///Returns an <code>IJIComObject</code> representing the COM Server.
        /// 
        /// </summary>
        /// <exception cref="JIException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public IJIComObject createInstance() throws org.jinterop.dcom.common.JIException
        public IJIComObject createInstance() {
            if (interfacePtrCtor != null) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMSTUB_WRONGCALLCREATEINSTANCE));
            }
            IJIComObject comObject = null;

            //This method is still essentially serial, since all threads will have to stop at mutex and then
            //go to addToSession after it (since there is no condition).
            lock (mutex) {
                if (serverInstantiated) {
                    throw new JIException(JIErrorCodes.JI_OBJECT_ALREADY_INSTANTIATED, (Exception)null);
                }
                //			JIStdObjRef objRef = (JIStdObjRef)(remoteActivation.getMInterfacePointer().getObjectReference(JIInterfacePointer.OBJREF_STANDARD));
                //			comObject = getObject(objRef.getIpid(),IJIUnknown.IID);
                comObject = JIFrameworkHelper.instantiateComObject(session, serverActivation.MInterfacePointer);
                if (serverActivation.Dual) {
                    //IJIComObject comObject2 = getObject(remoteActivation.dispIpid,"00020400-0000-0000-c000-000000000046");
                    //this will get garbage collected and then removed.
                    //session.addToSession(comObject2,remoteActivation.dispOid);
                    session.releaseRef(serverActivation.DispIpid, serverActivation.DispRefs);
                    serverActivation.DispIpid = null;
                    ((JIComObjectImpl)comObject).IsDual = true;
                }
                else {
                    ((JIComObjectImpl)comObject).IsDual = false;
                }
                //increasing the reference count.
                comObject.addRef();
                serverInstantiated = true;
            }

            return comObject;
        }

        /// <summary>
        ///Returns a <code>IJIComObject</code> representing the <code>COM</code> Server. To be used only with <code>JIComServer(JISession,JIInterfacePointer,String)</code> ctor,
        /// otherwise use createInstance() instead.
        /// 
        /// </summary>
        /// <exception cref="JIException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: IJIComObject getInstance() throws org.jinterop.dcom.common.JIException
        internal IJIComObject Instance {
            get {
                if (interfacePtrCtor == null) {
                    throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COMSTUB_WRONGCALLGETINSTANCE));
                }

                IJIComObject comObject = null;
                //This method is still essentially serial, since all threads will have to stop at mutex and then
                //go to addToSession after it (since there is no condition).
                lock (mutex) {
                    if (serverInstantiated) {
                        throw new JIException(JIErrorCodes.JI_OBJECT_ALREADY_INSTANTIATED, (Exception)null);
                    }

                    //			JIStdObjRef objRef = (JIStdObjRef)(interfacePtrCtor.getObjectReference(JIInterfacePointer.OBJREF_STANDARD));
                    //			comObject = getObject(objRef.getIpid(),interfacePtrCtor.getIID());
                    comObject = JIFrameworkHelper.instantiateComObject(session, interfacePtrCtor);
                    //increasing the reference count.
                    comObject.addRef();
                    serverInstantiated = true;
                }

                return comObject;
            }
        }


        protected internal string Syntax { get; private set; } = null;

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
        /// </param>
        /// <exception cref="JIException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object[] call(JICallBuilder obj,String targetIID) throws org.jinterop.dcom.common.JIException
        internal object[] call(JICallBuilder obj, string targetIID) {
            return call(obj, targetIID, session.GlobalSocketTimeout);
        }

        /// <summary>
        /// Execute a Method on the COM Interface identified by the IID
        /// 
        /// 
        /// @exclude </summary>
        /// <param name="obj"> </param>
        /// <param name="targetIID">
        /// </param>
        /// <exception cref="JIException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object[] call(JICallBuilder obj,String targetIID, int socketTimeout) throws org.jinterop.dcom.common.JIException
        internal object[] call(JICallBuilder obj, string targetIID, int socketTimeout) {
            lock (mutex) {

                if (session.SessionInDestroy && !obj._fromDestroySession) {
                    throw new JIException(JIErrorCodes.JI_SESSION_DESTROYED);
                }

                if (socketTimeout != 0) {
                    SocketTimeOut = socketTimeout;
                }
                else //for cases where it was something earlier, but is now being set to 0.
                {
                    if (timeoutModifiedfrom0) {
                        SocketTimeOut = socketTimeout;
                    }
                }

                try {

                    Attach();
                    if (!Endpoint.Syntax.Uuid.ToString().Equals(targetIID, StringComparison.CurrentCultureIgnoreCase)) {
                        //first send an AlterContext to the IID of the interface
                        Endpoint.Syntax.Uuid = new rpc.core.UUID(targetIID);
                        Endpoint.Syntax.Version = 0;
                        ((JIComEndpoint)Endpoint).rebindEndPoint();
                    }

                    Object = obj.ParentIpid;
                    call(Endpoint.IDEMPOTENT, obj);

                }
                catch (FaultException e) {
                    throw new JIException(e._code, e);
                }
                catch (IOException e) {
                    throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
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
        internal JIInterfacePointer ServerInterfacePointer =>
                //remoteactivation can be null only incase of OxidResolver ctor getting called.
                serverActivation == null ? interfacePtrCtor : serverActivation.MInterfacePointer;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void addRef_ReleaseRef(JICallBuilder obj) throws org.jinterop.dcom.common.JIException
        internal void addRef_ReleaseRef(JICallBuilder obj) {
            lock (mutex) {

                if (remunknownIPID == null) {
                    return;
                }
                //now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation or IOxidResolver
                obj.ParentIpid = remunknownIPID;
                obj.attachSession(session);
                try {
                    call(obj, JIRemUnknown.IID_IUnknown);
                }
                catch (JIRuntimeException e1) {
                    throw new JIException(e1);
                }

            }
        }

        internal void closeStub() {
            try {
                Detach();
            }
            catch (Exception) {
                //			No need to print this out.
                //			e.printStackTrace();
            }
        }

        internal int SocketTimeOut {
            set {
                if (value == 0) {
                    timeoutModifiedfrom0 = false;
                }
                else {
                    timeoutModifiedfrom0 = true;
                }

                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.socketTimeout", value.ToString());
            }
        }

    }

}