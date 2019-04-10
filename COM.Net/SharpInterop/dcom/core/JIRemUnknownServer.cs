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
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    /// <summary>
    /// Represents unknown server
    /// </summary>
    internal sealed class JIRemUnknownServer : Stub {

        private static Properties _defaults = new Properties();
        static JIRemUnknownServer() {
            _defaults.SetProperty("rpc.ntlm.lanManagerKey", "false");
            _defaults.SetProperty("rpc.ntlm.sign", "false");
            _defaults.SetProperty("rpc.ntlm.seal", "false");
            _defaults.SetProperty("rpc.ntlm.keyExchange", "false");
            _defaults.SetProperty("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
            _defaults.SetProperty("rpc.socketTimeout", 0.ToString());
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

        /// <summary>
        /// Syntax
        /// </summary>
        protected override string Syntax { get; }

        /// <summary>
        /// Interface pointer to the initialized COM server, must be
        /// called immediately after the JIComServer has been
        /// initialized. And closeStub must be called where we
        /// call closeStub of JIComServer.
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="remUnknownIpid"> </param>
        /// <param name="address"> in the "ncacn_ip_tcp:host[port]" format </param>
        /// <exception cref="JIException"> </exception>
        internal JIRemUnknownServer(JISession session, string remUnknownIpid,
            string address) {

            _session = session;
            TransportFactory = JIComTransportFactory.Instance;
            Properties = new Properties(_defaults);
            Properties.SetProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString());

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

            //now set the NTLMv2 Session Security.
            if (session.SessionSecurityEnabled) {
                Properties.SetProperty("rpc.ntlm.seal", "true");
                Properties.SetProperty("rpc.ntlm.sign", "true");
                Properties.SetProperty("rpc.ntlm.keyExchange", "true");
                Properties.SetProperty("rpc.ntlm.keyLength", "128");
                Properties.SetProperty("rpc.ntlm.ntlm2", "true");
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
        /// <param name="socketTimeout"></param>
        /// </summary>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        internal object[] Call(JICallBuilder obj, string targetIID, int socketTimeout) {
            lock (_mutex) {
                if (_session.SessionInDestroy && !obj.FromDestroySession) {
                    throw new JIException(JIErrorCodes.JI_SESSION_DESTROYED);
                }

                if (socketTimeout != 0) {
                    SocketTimeOut = socketTimeout;
                }
                else {
                    //for cases where it was something earlier, but is now being set to 0.
                    if (_timeoutModifiedfrom0) {
                        SocketTimeOut = socketTimeout;
                    }
                }
                try {
                    Attach();
                    if (!Endpoint.Syntax.Uuid.ToString().Equals(targetIID,
                        StringComparison.CurrentCultureIgnoreCase)) {
                        //first send an AlterContext to the IID of the interface
                        Endpoint.Syntax.Uuid = new rpc.core.UUID(targetIID);
                        Endpoint.Syntax.Version = 0;
                        ((JIComEndpoint)Endpoint).RebindEndPoint();
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
        /// Add ref release
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="JIException"></exception>
        internal void AddRef_ReleaseRef(JICallBuilder obj) {
            lock (_mutex) {
                if (_remunknownIPID == null) {
                    return;
                }
                // now also set the Object ID for IRemUnknown call this will be the
                // IPID of the returned JIRemActivation or IOxidResolver
                obj.ParentIpid = _remunknownIPID;
                obj.AttachSession(_session);
                try {
                    Call(obj, JIRemUnknown.IID_IUnknown, _session.GlobalSocketTimeout);
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
            catch (IOException) {
            }
        }

        private readonly JISession _session;
        private readonly string _remunknownIPID;
        private readonly object _mutex = new object();
        private bool _timeoutModifiedfrom0;
    }
}