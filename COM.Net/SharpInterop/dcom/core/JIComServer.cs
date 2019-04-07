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
    using System.Collections.Generic;
    using SharpCifs.Util.Sharpen;
    using System.Net;

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
    /// Each instance of this class is associated with a single session only.
    /// </summary>
    public sealed class JIComServer : Stub {

        private static Properties defaults = new Properties();
        static JIComServer() {

            defaults.SetProperty("rpc.ntlm.lanManagerKey", "false");
            defaults.SetProperty("rpc.ntlm.sign", "false");
            defaults.SetProperty("rpc.ntlm.seal", "false");
            defaults.SetProperty("rpc.ntlm.keyExchange", "false");
            defaults.SetProperty("rpc.ntlm.sso", "false");
            defaults.SetProperty("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
            defaults.SetProperty("rpc.socketTimeout", 0.ToString());
            //		rpc.connectionContext = rpc.security.ntlm.NtlmConnectionContext
            //		rpc.ntlm.sign = false
            //		rpc.ntlm.seal = false
            //		rpc.ntlm.keyExchange = false

        }


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
        /// </summary>
        /// <exception cref="JIException"></exception>
        /// <param name="session"> Please use a new session and not an already bounded one. The <code>JISession.createSession(JISession)</code> can be used to create a new session. </param>
        /// <param name="interfacePointer"> reference to a different COM server pointer. </param>
        /// <param name="ipAddress"> Can be <code>null</code>. Sometimes there are many adapters (virtual as well) on the Target machine to which this interface pointer belongs,
        /// which may get sent as part of the interface pointer and consequently this call will fail since it is a possibility that IP is not reachable via this machine.
        /// The developer can send in the valid IP and if found in the interface pointer list will be used to talk to the target machine, overriding the other IP addresses present in the interface pointer.
        /// If this IP is not found then the "machine name" binding will be used. If this param is <code>null</code> then the first binding obtained from the interface pointer is used. </param>
        internal JIComServer(JISession session, JIInterfacePointer interfacePointer, string ipAddress) {
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
                if (!_listOfIps.Contains(ipAddress)) {
                    _listOfIps.Add(ipAddress.ToLower());
                }
            }

            TransportFactory = JIComTransportFactory.SingleTon;
            //now read the session and prepare information for the stub.
            Properties = new Properties(defaults);
            Properties.SetProperty("rpc.security.username", session.UserName);
            Properties.SetProperty("rpc.security.password", session.Password);
            Properties.SetProperty("rpc.ntlm.domain", session.Domain);
            Properties.SetProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString());
            if (session.NTLMv2Enabled) {
                Properties.SetProperty("rpc.ntlm.ntlmv2", "true");
            }
            if (session.SSOEnabled) {
                Properties.SetProperty("rpc.ntlm.sso", "true");
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
                    if (binding.TowerId != 0x07) {
                        //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                        i++;
                        continue;
                    }
                    //get the one with IP address
                    var idx = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
                    if (idx != -1) {
                        try {
                            if (_listOfIps.Contains(binding.NetworkAddress.ToLower())) {
                                nameBinding = null;
                                break;
                            }

                            //now check for the one with port
                            idx = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                            if (idx != -1 && _listOfIps.Contains(binding.NetworkAddress.Substring(0, idx).ToLower())) {
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

            //will use this last binding .
            //and currently only TCPIP is supported.
            var address = binding.NetworkAddress;
            if (address.IndexOf("[", StringComparison.Ordinal) == -1) { //this does not contain the port
                var addr = JISystem.getIPForHostName(address); //to use the binding supplied by the user.
                if (addr != null) {
                    address = addr;
                }
                //use 135
                address += "[135]";
            }
            else {
                var idx = address.IndexOf("[", StringComparison.Ordinal);
                var host = binding.NetworkAddress.Substring(0, idx);
                var addr = JISystem.getIPForHostName(host); //to use the binding supplied by the user.
                if (addr != null) {
                    address = addr + address.Substring(idx);
                }
            }
            Address = "ncacn_ip_tcp:" + address;
            _session = session;
            _session.TargetServer = Address.SubstringSpecial(
                Address.IndexOf(":", StringComparison.Ordinal) + 1,
                Address.IndexOf("[", StringComparison.Ordinal));
            _oxidResolver = new JIOxidResolver(((JIStdObjRef)
                interfacePointer.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Oxid);
            try {
                _syntax = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
                Attach();
                //first send an AlterContext to the IID of the IOxidResolver
                Endpoint.Syntax.Uuid = new rpc.core.UUID("99fcfec4-5260-101b-bbcb-00aa0021347a");
                Endpoint.Syntax.Version = 0;
                ((JIComEndpoint)Endpoint).rebindEndPoint();

                Call(Semantics.IDEMPOTENT, _oxidResolver);
            }
            catch (FaultException e) {
                throw new JIException((int)e.Code, e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e1) {
                throw new JIException(e1);
            }

            // Now will setup syntax for IRemUnknown and the address.
            //syntax = "00000143-0000-0000-c000-000000000046:0.0";
            _syntax = interfacePointer.IID + ":0.0";

            //now for the new ip and the port.

            var bindings = _oxidResolver.OxidBindings.StringBindings;

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
                    var idx = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
                    if (idx != -1) {
                        try {
                            if (_listOfIps.Contains(binding.NetworkAddress.ToLower())) {
                                nameBinding = null;
                                break;
                            }

                            //now check for the one with port
                            idx = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                            if (idx != -1 && _listOfIps.Contains(binding.NetworkAddress.Substring(0, idx).ToLower())) {
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

            // now set the NTLMv2 Session Security.
            if (session.SessionSecurityEnabled) {
                Properties.SetProperty("rpc.ntlm.seal", "true");
                Properties.SetProperty("rpc.ntlm.sign", "true");
                Properties.SetProperty("rpc.ntlm.keyExchange", "true");
                Properties.SetProperty("rpc.ntlm.keyLength", "128");
                Properties.SetProperty("rpc.ntlm.ntlm2", "true");
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
            _remunknownIPID = _oxidResolver.IPID;
            _interfacePtrCtor = interfacePointer;
            _session.Stub = this;
            _session.Stub2 = new JIRemUnknownServer(session, _remunknownIPID, Address);
        }

        /// <summary>
        /// <code>JIProgId</code> based constructor with the host machine for COM
        /// server being <i>LOCALHOST</i>.
        /// </summary>
        /// <param name="progId"> user-friendly string such as "Excel.Application",
        /// "TestCOMServer.Test123" etc. </param>
        /// <param name="session"> session to be associated with. </param>
        /// <exception cref="JIException"> will <i>also</i> get thrown in case the <code>session</code>
        /// is associated with another server already. </exception>
        /// <exception cref="ArgumentException"> raised when either <code>progId</code>
        /// or <code>session</code> is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        public JIComServer(JIProgId progId, JISession session) :
            this(progId, Dns.GetHostName(), session) {
        }

        /// <summary>
        /// <code><seealso cref="JIClsid"/></code> based constructor with the host
        /// machine for COM server being <i>LOCALHOST</i>.
        /// </summary>
        /// <param name="clsid"> 128 bit string such as "00024500-0000-0000-C000-000000000046". </param>
        /// <param name="session"> session to be associated with. </param>
        /// <exception cref="JIException"> will <i>also</i> get thrown in case the
        /// <code>session</code> is associated with another server already. </exception>
        /// <exception cref="ArgumentException"> raised when either <code>clsid</code>
        /// or <code>session</code> is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        public JIComServer(JIClsid clsid, JISession session) :
            this(clsid, Dns.GetHostName(), session) {
        }

        /// <summary>
        /// Refer <seealso cref="JIComServer(JIProgId, JISession)"/> for details.
        /// </summary>
        /// <param name="progId"> user-friendly string such as "Excel.Application",
        /// "TestCOMServer.Test123" etc. </param>
        /// <param name="address"> address of the host where the <code>COM</code> object resides.
        /// This should be in the IEEE IP format (e.g. 192.168.170.6) or a resolvable HostName. </param>
        /// <param name="session"> session to be associated with. </param>
        /// <exception cref="JIException"> will <i>also</i> get thrown in case the
        /// <code>session</code> is associated with another server already. </exception>
        /// <exception cref="ArgumentException"> raised when any of the parameters
        /// is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        public JIComServer(JIProgId progId, string address, JISession session) {
            if (progId == null || address == null || session == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
            }
            if (session.Stub != null) {
                throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
            }

            if (session.SSOEnabled) {
                throw new ArgumentException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS2));
            }

            address = address.Trim();
            address = Dns.GetHostAddresses(address).GetFirst()?.ToString();

            progId.Session = session;
            progId.Server = address;
            address = "ncacn_ip_tcp:" + address + "[135]";
            var clsid = progId.CorrespondingCLSID;
            Initialise(clsid, address, session);
        }

        /// <summary>
        /// Refer <seealso cref="JIComServer(JIClsid, JISession)"/> for details.
        /// </summary>
        /// <param name="clsid">128 bit string such as "00024500-0000-0000-C000-000000000046".
        /// </param>
        /// <param name="address"> address of the host where the <code>COM</code> object
        /// resides.This should be in the IEEE IP format (e.g. 192.168.170.6) or a
        /// resolvable HostName. </param>
        /// <param name="session"> session to be associated with. </param>
        /// <exception cref="JIException"> will <i>also</i> get thrown in case the
        /// <code>session</code> is associated with another server already. </exception>
        /// <exception cref="ArgumentException"> raised when any of the parameters
        /// is <code>null</code>. </exception>
        /// <exception cref="UnknownHostException"> </exception>
        public JIComServer(JIClsid clsid, string address, JISession session) {
            if (clsid == null || address == null || session == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_COMSTUB_ILLEGAL_ARGUMENTS));
            }
            if (session.Stub != null) {
                throw new JIException(JIErrorCodes.JI_SESSION_ALREADY_ESTABLISHED);
            }
            address = address.Trim();
            address = Dns.GetHostAddresses(address).GetFirst()?.ToString();
            address = "ncacn_ip_tcp:" + address + "[135]";
            Initialise(clsid, address, session);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="address"></param>
        /// <param name="session"></param>
        /// <exception cref="JIException"></exception>
        private void Initialise(JIClsid clsid, string address, JISession session) {
            TransportFactory = JIComTransportFactory.SingleTon;
            //now read the session and prepare information for the stub.
            Properties = new Properties(defaults);
            Properties.SetProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString());
            Address = address;

            if (session.NTLMv2Enabled) {
                Properties.SetProperty("rpc.ntlm.ntlmv2", "true");
            }

            if (session.SSOEnabled) {
                Properties.SetProperty("rpc.ntlm.sso", "true");
            }
            else {
                Properties.SetProperty("rpc.security.username", session.UserName);
                Properties.SetProperty("rpc.security.password", session.Password);
                Properties.SetProperty("rpc.ntlm.domain", session.Domain);
            }

            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                JISystem.internal_dumpMap();
            }

            _clsid = clsid.CLSID.ToUpper();
            _session = session;
            _session.TargetServer = address.SubstringSpecial(address.IndexOf(":", StringComparison.Ordinal) + 1, address.IndexOf("[", StringComparison.Ordinal));
            try {
                Init();
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
                                var key = registry.winreg_CreateKey(hkwow6432, "CLSID\\{" + _clsid + "}",
                                    winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "AppId", ("{" + _clsid + "}").GetBytes(), false, false);
                                registry.winreg_CloseKey(key);
                                Log.Logger.Information("--- winreg_SetValue --- SOFTWARE\\Classes\\Wow6432Node\\CLSID\\" + _clsid + " -- AppID");

                                // HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\AppID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\AppID\ -- "DllSurrogate"
                                key = registry.winreg_CreateKey(hkwow6432, "AppID\\{" + _clsid + "}",
                                    winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "DllSurrogate", "".GetBytes(), false, false);
                                registry.winreg_CloseKey(key);

                                Log.Logger.Information("--- winreg_SetValue --- SOFTWARE\\Classes\\Wow6432Node\\AppID\\" +
                                    _clsid + " -- DllSurrogate");
                                registry.winreg_CloseKey(hkwow6432);
                            }
                            else {
                                Log.Logger.Information("Attempting to register on 32 bit");
                                var hkcr = registry.winreg_OpenHKCR();
                                var key = registry.winreg_CreateKey(hkcr, "CLSID\\{" + _clsid + "}",
                                    winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
                                registry.winreg_SetValue(key, "AppID", ("{" + _clsid + "}").GetBytes(), false, false);
                                registry.winreg_CloseKey(key);
                                key = registry.winreg_CreateKey(hkcr, "AppID\\{" + _clsid + "}",
                                    winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);
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

            _session.Stub = this;
            _session.Stub2 = new JIRemUnknownServer(session, _remunknownIPID, Address);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <exception cref="JIException"></exception>
        private void Init() {
            if (_serverActivation != null && _serverActivation.ActivationSuccessful) {
                return;
            }

            var attachcomplete = false;
            try {
                _syntax = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
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
                //			Call(Semantics.IDEMPOTENT,serverAlive);

                var serverAlive = new JICallBuilder(true);
                serverAlive.AttachSession(_session);
                serverAlive.Opnum = 2;
                serverAlive.Internal_COMVersion();
                try {
                    Call(Semantics.IDEMPOTENT, serverAlive);
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
                    _syntax = "000001A0-0000-0000-C000-000000000046:0.0";
                    Endpoint.Syntax.Uuid = new rpc.core.UUID("000001A0-0000-0000-C000-000000000046");
                    Endpoint.Syntax.Version = 0;
                    ((JIComEndpoint)Endpoint).rebindEndPoint();
                    _serverActivation = new JIRemoteSCMActivator.RemoteCreateInstance(new JIRemoteSCMActivator(), _session.TargetServer, _clsid);
                    Call(Semantics.IDEMPOTENT, (JIRemoteSCMActivator.RemoteCreateInstance)_serverActivation);
                }
                else {
                    //setup syntax for IRemoteActivation
                    _syntax = "4d9f4ab8-7d1c-11cf-861e-0020af6e7c57:0.0";
                    Endpoint.Syntax.Uuid = new rpc.core.UUID("4d9f4ab8-7d1c-11cf-861e-0020af6e7c57");
                    Endpoint.Syntax.Version = 0;
                    ((JIComEndpoint)Endpoint).rebindEndPoint();
                    _serverActivation = new JIRemActivation(_clsid);
                    Call(Semantics.IDEMPOTENT, (JIRemActivation)_serverActivation);
                }
            }
            catch (FaultException e) {
                _serverActivation = null;
                throw new JIException((int)e.Code, e);
            }
            catch (IOException e) {
                _serverActivation = null;
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e1) {
                _serverActivation = null;
                throw new JIException(e1);
            }
            finally {
                //the only time remactivation will be null will be case of an exception.
                if (attachcomplete && _serverActivation == null) {
                    try {
                        Detach();
                    }
                    catch (IOException e) {
                        Log.Logger.Warning(e, "Unable to detach during init");
                    }
                }
            }

            // Now will setup syntax for IRemUnknown and the address.
            _syntax = "00000143-0000-0000-c000-000000000046:0.0";
            //now for the new ip and the port.

            var bindings = _serverActivation.DualStringArrayForOxid.StringBindings;
            var i = 0;
            JIStringBinding binding = null;
            JIStringBinding nameBinding = null;
            var targetAddress = Address;
            targetAddress = targetAddress.SubstringSpecial(targetAddress.IndexOf(':') + 1, targetAddress.IndexOf('['));
            while (i < bindings.Length) {
                binding = bindings[i];
                if (binding.TowerId != 0x07) //this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                {
                    i++;
                    continue;
                }
                //get the one with IP address
                var idx = binding.NetworkAddress.IndexOf(".", StringComparison.Ordinal);
                if (idx != -1) {
                    try {
                        idx = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                        if (idx != -1 && binding.NetworkAddress.Substring(0, idx).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase)) {
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
                    idx = binding.NetworkAddress.IndexOf("[", StringComparison.Ordinal); //this contains the port
                    if (binding.NetworkAddress.Substring(0, idx).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase)) {
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
            if (_session.SessionSecurityEnabled) {
                Properties.SetProperty("rpc.ntlm.seal", "true");
                Properties.SetProperty("rpc.ntlm.sign", "true");
                Properties.SetProperty("rpc.ntlm.keyExchange", "true");
                Properties.SetProperty("rpc.ntlm.keyLength", "128");
                Properties.SetProperty("rpc.ntlm.ntlm2", "true");
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
            _remunknownIPID = _serverActivation.IPID;
        }

        /// <summary>
        /// Will give a call to IRemUnknown for the passed IID
        /// </summary>
        /// <param name="iid"></param>
        /// <param name="ipidOfTheTargetUnknown"></param>
        /// <returns></returns>
        /// <exception cref="JIException"></exception>
        internal IJIComObject GetInterface(string iid, string ipidOfTheTargetUnknown) {
            IJIComObject retval = null;
            //this is still essentially serial, since all threads will have to wait for mutex before
            //entering addToSession.
            lock (_mutex) {
                //now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation
                Object = _remunknownIPID;
                //setObject(ipid);

                //JIRemUnknown reqUnknown = new JIRemUnknown(unknownIPID,iid,5);
                var reqUnknown = new JIRemUnknown(ipidOfTheTargetUnknown, iid);
                try {
                    _session.Stub2.Call(Semantics.IDEMPOTENT, reqUnknown);
                }
                catch (FaultException e) {
                    throw new JIException((int)e.Code, e);
                }
                catch (IOException e) {
                    throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
                }
                catch (JIRuntimeException e1) {
                    //remoteActivation = null;
                    throw new JIException(e1);
                }

                retval = JIFrameworkHelper.InstantiateComObject(_session, reqUnknown.InterfacePointer);
                //increasing the reference count.
                retval.AddRef();
                //for querying dispatch we can't send another call
                if (!iid.Equals("00020400-0000-0000-c000-000000000046", StringComparison.CurrentCultureIgnoreCase)) {
                    var success = true;
                    ((JIComObjectImpl)retval).IsDual = true;
                    //now to check whether it supports IDispatch
                    //IDispatch 00020400-0000-0000-c000-000000000046
                    var dispatch = new JIRemUnknown(retval.Ipid, "00020400-0000-0000-c000-000000000046");
                    try {
                        _session.Stub2.Call(Semantics.IDEMPOTENT, dispatch);
                    }
                    catch (FaultException e) {
                        throw new JIException((int)e.Code, e);
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
                        _session.releaseRef(dispatch.InterfacePointer.IPID, ((JIStdObjRef)dispatch.InterfacePointer.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs);
                    }
                }
            }

            return retval;

        }

        /// <summary>
        /// Returns an <code>IJIComObject</code> representing the COM Server.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public IJIComObject CreateInstance() {
            if (_interfacePtrCtor != null) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(
                    JIErrorCodes.JI_COMSTUB_WRONGCALLCREATEINSTANCE));
            }
            IJIComObject comObject = null;

            //This method is still essentially serial, since all threads will have to stop at mutex and then
            //go to addToSession after it (since there is no condition).
            lock (_mutex) {
                if (_serverInstantiated) {
                    throw new JIException(JIErrorCodes.JI_OBJECT_ALREADY_INSTANTIATED, (Exception)null);
                }
                comObject = JIFrameworkHelper.InstantiateComObject(_session, _serverActivation.MInterfacePointer);
                if (_serverActivation.Dual) {
                    //IJIComObject comObject2 = getObject(remoteActivation.dispIpid,"00020400-0000-0000-c000-000000000046");
                    //this will get garbage collected and then removed.
                    //session.addToSession(comObject2,remoteActivation.dispOid);
                    _session.releaseRef(_serverActivation.DispIpid, _serverActivation.DispRefs);
                    _serverActivation.DispIpid = null;
                    ((JIComObjectImpl)comObject).IsDual = true;
                }
                else {
                    ((JIComObjectImpl)comObject).IsDual = false;
                }
                //increasing the reference count.
                comObject.AddRef();
                _serverInstantiated = true;
            }

            return comObject;
        }

        /// <summary>
        /// Returns a <code>IJIComObject</code> representing the <code>COM</code> Server.
        /// To be used only with <code>JIComServer(JISession,JIInterfacePointer,String)</code> ctor,
        /// otherwise use createInstance() instead.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        internal IJIComObject Instance {
            get {
                if (_interfacePtrCtor == null) {
                    throw new InvalidOperationException(JISystem.getLocalizedMessage(
                        JIErrorCodes.JI_COMSTUB_WRONGCALLGETINSTANCE));
                }

                IJIComObject comObject = null;
                //This method is still essentially serial, since all threads will have to stop at mutex and then
                //go to addToSession after it (since there is no condition).
                lock (_mutex) {
                    if (_serverInstantiated) {
                        throw new JIException(JIErrorCodes.JI_OBJECT_ALREADY_INSTANTIATED, (Exception)null);
                    }
                    comObject = JIFrameworkHelper.InstantiateComObject(_session, _interfacePtrCtor);
                    //increasing the reference count.
                    comObject.AddRef();
                    _serverInstantiated = true;
                }

                return comObject;
            }
        }

        /// <summary>
        /// Syntax
        /// </summary>
        protected override string Syntax => _syntax;

        /// <summary>
        /// Execute a Method on the COM Interface identified by the IID.
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="targetIID">
        /// </param>
        /// <exception cref="JIException"> </exception>
        internal object[] Call(JICallBuilder obj, string targetIID) {
            return Call(obj, targetIID, _session.GlobalSocketTimeout);
        }

        /// <summary>
        /// Execute a Method on the COM Interface identified by the IID
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="targetIID">
        /// </param>
        /// <param name="socketTimeout"></param>
        /// <exception cref="JIException"> </exception>
        internal object[] Call(JICallBuilder obj, string targetIID, int socketTimeout) {
            lock (_mutex) {

                if (_session.SessionInDestroy && !obj._fromDestroySession) {
                    throw new JIException(JIErrorCodes.JI_SESSION_DESTROYED);
                }

                if (socketTimeout != 0) {
                    SocketTimeOut = socketTimeout;
                }
                else //for cases where it was something earlier, but is now being set to 0.
                {
                    if (_timeoutModifiedfrom0) {
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
                    Call(Semantics.IDEMPOTENT, obj);

                }
                catch (FaultException e) {
                    throw new JIException((int)e.Code, e);
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
        /// Server interface pointer
        /// </summary>
        internal JIInterfacePointer ServerInterfacePointer =>
                //remoteactivation can be null only incase of OxidResolver ctor getting called.
                _serverActivation == null ? _interfacePtrCtor : _serverActivation.MInterfacePointer;

        /// <summary>
        /// Add ref release
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="JIException"> </exception>
        internal void AddRef_ReleaseRef(JICallBuilder obj) {
            lock (_mutex) {

                if (_remunknownIPID == null) {
                    return;
                }
                //now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation or IOxidResolver
                obj.ParentIpid = _remunknownIPID;
                obj.AttachSession(_session);
                try {
                    Call(obj, JIRemUnknown.IID_IUnknown);
                }
                catch (JIRuntimeException e1) {
                    throw new JIException(e1);
                }

            }
        }

        /// <summary>
        /// Close
        /// </summary>
        internal void CloseStub() {
            try {
                Detach();
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }
        }

        /// <summary>
        /// Socket timeout
        /// </summary>
        internal int SocketTimeOut {
            set {
                if (value == 0) {
                    _timeoutModifiedfrom0 = false;
                }
                else {
                    _timeoutModifiedfrom0 = true;
                }

                Properties.SetProperty("rpc.socketTimeout", value.ToString());
            }
        }

        private JIIServerActivation _serverActivation;
        private JIOxidResolver _oxidResolver;
        private string _clsid;
        private JISession _session;
        private bool _serverInstantiated;
        private string _remunknownIPID;
        private readonly object _mutex = new object();
        private string _syntax;
        private bool _timeoutModifiedfrom0;
        private readonly JIInterfacePointer _interfacePtrCtor;
        private static readonly List<string> _listOfIps = new List<string>();
    }
}