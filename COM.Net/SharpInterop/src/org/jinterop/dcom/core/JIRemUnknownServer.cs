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
    using rpc;
    using System;
    using System.IO;

    /// <summary>
    /// Represents unknown server
    /// </summary>
    internal sealed class JIRemUnknownServer : Stub {

        private static Properties defaults = new Properties();
        static JIRemUnknownServer() {

            defaults.put("rpc.ntlm.lanManagerKey", "false");
            defaults.put("rpc.ntlm.sign", "false");
            defaults.put("rpc.ntlm.seal", "false");
            defaults.put("rpc.ntlm.keyExchange", "false");
            defaults.put("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
            defaults.put("rpc.socketTimeout", 0.ToString());
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

                Properties.setProperty("rpc.socketTimeout", value.ToString());
            }
        }

        /// <summary>
        /// Syntax
        /// </summary>
        protected override string Syntax { get; }

        /// <summary>
        /// Interface pointer to the initialized COM server , must be called immediately after the JIComServer has been 
        /// initialized. And closeStub must be called where we call closeStub of JIComServer.
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="remUnknownIpid"> </param>
        /// <param name="address"> in the "ncacn_ip_tcp:host[port]" format </param>
        /// <exception cref="JIException"> </exception>
        internal JIRemUnknownServer(JISession session, string remUnknownIpid, string address) {

            _session = session;
            TransportFactory = JIComTransportFactory.SingleTon;
            Properties = new Properties(defaults);
            Properties.setProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString());

            if (session.NTLMv2Enabled) {
                Properties.setProperty("rpc.ntlm.ntlmv2", "true");
            }

            if (session.SSOEnabled) {
                Properties.setProperty("rpc.ntlm.sso", "true");
            }
            else {
                Properties.setProperty("rpc.security.username", session.UserName);
                Properties.setProperty("rpc.security.password", session.Password);
                Properties.setProperty("rpc.ntlm.domain", session.Domain);
            }

            //now set the NTLMv2 Session Security.
            if (session.SessionSecurityEnabled) {
                Properties.setProperty("rpc.ntlm.seal", "true");
                Properties.setProperty("rpc.ntlm.sign", "true");
                Properties.setProperty("rpc.ntlm.keyExchange", "true");
                Properties.setProperty("rpc.ntlm.keyLength", "128");
                Properties.setProperty("rpc.ntlm.ntlm2", "true");
            }

            // Now will setup syntax for IRemUnknown and the address. 
            Syntax = "00000143-0000-0000-c000-000000000046:0.0";
            //and currently only TCPIP is supported.
            Address = address;
            _remunknownIPID = remUnknownIpid;
            _session.Stub2 = this;
        }

        /// <summary>
        /// Execute a Method on the COM Interface identified by the IID
        /// <param name="obj"> </param>
        /// <param name="targetIID"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        internal object[] call(JICallBuilder obj, string targetIID, int socketTimeout) {
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

                    attach();
                    if (!Endpoint.Syntax.Uuid.ToString().Equals(targetIID, StringComparison.CurrentCultureIgnoreCase)) {
                        //first send an AlterContext to the IID of the interface
                        Endpoint.Syntax.Uuid = new rpc.core.UUID(targetIID);
                        Endpoint.Syntax.setVersion(0, 0);
                        ((JIComEndpoint)Endpoint).rebindEndPoint();
                    }

                    Object = obj.ParentIpid;
                    call(Endpoint.IDEMPOTENT, obj);

                }
                catch (FaultException e) {
                    throw new JIException(e.status, e);
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
        /// Add ref release
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="JIException"></exception>
        internal void addRef_ReleaseRef(JICallBuilder obj) {
            lock (_mutex) {
                if (_remunknownIPID == null) {
                    return;
                }
                //now also set the Object ID for IRemUnknown call this will be the IPID of the returned JIRemActivation or IOxidResolver
                obj.ParentIpid = _remunknownIPID;
                obj.attachSession(_session);
                try {
                    call(obj, JIRemUnknown.IID_IUnknown, _session.GlobalSocketTimeout);
                }
                catch (JIRuntimeException e1) {
                    throw new JIException(e1);
                }
            }
        }

        internal void closeStub() {
            try {
                detach();
            }
            catch (IOException) {
            }
        }

        private JISession _session;
        private readonly string _remunknownIPID;
        private readonly object _mutex = new object();
        private bool _timeoutModifiedfrom0;
    }
}