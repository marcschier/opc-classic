// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using org.jinterop.dcom.transport;
    using org.jinterop.dcom.common;
    using rpc;
    using System.Collections;
    using System.IO;
    using System;
    using Serilog;

    /// <summary>
    /// Class only used for Oxid ping requests between the Java client and the COM server.
    /// This is not for reverse operations i.e COM client and server. That is handled 
    /// at the OxidResolverImpl level in JIComOxidRuntimeHelper, since each of the Oxid 
    /// Resolver has a separate thread for COM client.  
    /// </summary>
    internal sealed class JIComOxidStub : Stub {

        private static SharpCifs.Util.Sharpen.Properties defaults = new SharpCifs.Util.Sharpen.Properties();

        static JIComOxidStub() {

            defaults.put("rpc.ntlm.lanManagerKey", "false");
            defaults.put("rpc.ntlm.sign", "false");
            defaults.put("rpc.ntlm.seal", "false");
            defaults.put("rpc.ntlm.keyExchange", "false");
            defaults.put("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");

        }

        protected override string Syntax => "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";

        public JIComOxidStub(string address, string domain, string username, string password, bool useNTLMv2, bool isSSO) : base() {
            TransportFactory = JIComTransportFactory.SingleTon;
            SharpCifs.Util.Sharpen.Properties = new SharpCifs.Util.Sharpen.Properties(defaults);

            if (isSSO) {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.sso", "true");
            }
            else {
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.security.username", username);
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.security.password", password);
                SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.domain", domain);
            }

            Address = "ncacn_ip_tcp:" + address + "[135]";
            SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.ntlmv2", useNTLMv2.ToString());
        }

        public byte[] call(bool isSimplePing, byte[] setId, ArrayList listOfAdds, ArrayList listOfDels, int seqNum) {
            var pingObject = new JiComOxidPingObject {
                setId = setId,
                listOfAdds = listOfAdds,
                listOfDels = listOfDels,
                seqNum = seqNum
            };

            if (isSimplePing) {
                pingObject.opnum = 1;
            }
            else {
                pingObject.opnum = 2;
            }

            try {
                call(Endpoint.IDEMPOTENT, pingObject);
            }
            catch (IOException e) {
                Log.Logger.Error(e, "JIComOxidStub", "call", e);
            }

            //returns setId.
            return pingObject.setId;
        }

        public void close() {
            try {
                Detach();
            }
            catch (Exception ex) {
                Log.Logger.Verbose(ex, "JIComOxidStub close");  
            }
        }
    }
}