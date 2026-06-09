// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Transport;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Internal;
using SharpCifs.Util.Sharpen;
using System;
using System.Collections.Generic;
using System.IO;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Class only used for Oxid ping requests between the Java client and the COM server.
/// This is not for reverse operations i.e COM client and server. That is handled
/// at the OxidResolverImpl level in ComOxidRuntimeHelper, since each of the Oxid
/// Resolver has a separate thread for COM client.
/// </summary>
internal sealed class ComOxidStub : Stub {

    private static readonly PropertyBag kDefaults = new PropertyBag();

    static ComOxidStub() {

        kDefaults.SetProperty("rpc.ntlm.lanManagerKey", "false");
        kDefaults.SetProperty("rpc.ntlm.sign", "false");
        kDefaults.SetProperty("rpc.ntlm.seal", "false");
        kDefaults.SetProperty("rpc.ntlm.keyExchange", "false");
        kDefaults.SetProperty("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
        kDefaults.SetProperty(RpcTransportQuotas.MaxNdrPayloadSizeProperty, RpcTransportQuotas.DefaultMaxNdrPayloadSize);
        kDefaults.SetProperty(RpcTransportQuotas.MaxNtlmMessageSizeProperty, RpcTransportQuotas.DefaultMaxNtlmMessageSize);
        kDefaults.SetProperty(RpcTransportQuotas.MaxSmb2MessageSizeProperty, RpcTransportQuotas.DefaultMaxSmb2MessageSize);
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
    public ComOxidStub(string address, string domain, string username,
        string password, bool useNTLMv2, bool isSSO) {
        TransportFactory = ComTransportFactory.Instance;
        Properties = new PropertyBag(kDefaults);
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
        List<ObjectId> listOfAdds, List<ObjectId> listOfDels, int seqNum) {
        var pingObject = new ComOxidPingObject {
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
            Log.Logger.Error(e, "ComOxidStub call");
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
        catch (Exception e) {
            Log.Logger.Verbose(e, "ComOxidStub close");
        }
    }
}
