//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.transport;
    using rpc;
    using Serilog;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Class only used for Oxid ping requests between the Java client and the COM server.
    /// This is not for reverse operations i.e COM client and server. That is handled
    /// at the OxidResolverImpl level in JIComOxidRuntimeHelper, since each of the Oxid
    /// Resolver has a separate thread for COM client.
    /// </summary>
    internal sealed class JIComOxidStub : Stub {

        private static readonly Properties kDefaults = new Properties();

        static JIComOxidStub() {

            kDefaults.SetProperty("rpc.ntlm.lanManagerKey", "false");
            kDefaults.SetProperty("rpc.ntlm.sign", "false");
            kDefaults.SetProperty("rpc.ntlm.seal", "false");
            kDefaults.SetProperty("rpc.ntlm.keyExchange", "false");
            kDefaults.SetProperty("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
        }

        /// <inheritdoc/>
        protected override string Syntax => "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";

        /// <summary>
        /// Create stub
        /// </summary>
        /// <param name="address"></param>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="useNTLMv2"></param>
        /// <param name="isSSO"></param>
        public JIComOxidStub(string address, string domain, string username,
            string password, bool useNTLMv2, bool isSSO) {
            TransportFactory = JIComTransportFactory.Instance;
            Properties = new Properties(kDefaults);
            if (isSSO) {
                Properties.SetProperty("rpc.ntlm.sso", "true");
            }
            else {
                Properties.SetProperty("rpc.security.username", username);
                Properties.SetProperty("rpc.security.password", password);
                Properties.SetProperty("rpc.ntlm.domain", domain);
            }

            Address = "ncacn_ip_tcp:" + address + "[135]";
            Properties.SetProperty("rpc.ntlm.ntlmv2", useNTLMv2.ToString());
        }

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="isSimplePing"></param>
        /// <param name="setId"></param>
        /// <param name="listOfAdds"></param>
        /// <param name="listOfDels"></param>
        /// <param name="seqNum"></param>
        /// <returns></returns>
        public byte[] Call(bool isSimplePing, byte[] setId,
            List<object> listOfAdds, List<object> listOfDels, int seqNum) {
            var pingObject = new JiComOxidPingObject {
                SetId = setId,
                _listOfAdds = listOfAdds,
                _listOfDels = listOfDels,
                _seqNum = seqNum
            };

            if (isSimplePing) {
                pingObject.Opnum = 1;
            }
            else {
                pingObject.Opnum = 2;
            }

            try {
                Call(Semantics.IDEMPOTENT, pingObject);
            }
            catch (IOException e) {
                Log.Logger.Error(e, "JIComOxidStub", "call", e);
            }

            // returns setId.
            return pingObject.SetId;
        }

        /// <summary>
        /// Close
        /// </summary>
        public void Close() {
            try {
                Detach();
            }
            catch (Exception ex) {
                Log.Logger.Verbose(ex, "JIComOxidStub close");
            }
        }
    }
}