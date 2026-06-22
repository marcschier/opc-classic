// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Transport;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Class only used for Oxid ping requests between the Java client and the COM server.
/// This is not for reverse operations i.e COM client and server. That is handled
/// at the OxidResolverImpl level in ComOxidRuntimeHelper, since each of the Oxid
/// Resolver has a separate thread for COM client.
/// </summary>
internal sealed class ComOxidStub : Stub
{
    private static readonly PropertyBag kDefaults = new PropertyBag();

    static ComOxidStub()
    {
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
    /// <param name="address">Network address or binding address for the remote endpoint.</param>
    /// <param name="domain">Authentication domain used for the NTLM or Kerberos handshake.</param>
    /// <param name="username">User name used for the NTLM or Kerberos handshake.</param>
    /// <param name="password">Password used for the NTLM or Kerberos handshake.</param>
    /// <param name="useNTLMv2">Value indicating whether NTLMv2 response generation should be used.</param>
    /// <param name="isSSO">Value indicating whether single sign-on credentials should be used.</param>
    public ComOxidStub(string address, string domain, string username,
        string password, bool useNTLMv2, bool isSSO)
    {
        TransportFactory = ComTransportFactory.Instance;
        Properties = new PropertyBag(kDefaults);
        if (isSSO)
        {
            Properties.SetProperty("rpc.ntlm.sso", "true");
        }
        else
        {
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
    /// <param name="isSimplePing">Value indicating whether the ping request uses the simple ping format.</param>
    /// <param name="setId">Identifier of the ping set that owns the tracked object references.</param>
    /// <param name="listOfAdds">Object identifiers to add to the DCOM ping set.</param>
    /// <param name="listOfDels">Object identifiers to remove from the DCOM ping set.</param>
    /// <param name="seqNum">NTLM sequence number used when signing or sealing the message.</param>
    /// <returns>The sequence of call values produced by the operation.</returns>
    public byte[] Call(bool isSimplePing, byte[] setId,
        List<ObjectId> listOfAdds, List<ObjectId> listOfDels, int seqNum)
    {
        var pingObject = new ComOxidPingObject
        {
            SetId = setId,
            _listOfAdds = listOfAdds,
            _listOfDels = listOfDels,
            _seqNum = seqNum
        };

        if (isSimplePing)
        {
            pingObject.Opnum = 1;
        }
        else
        {
            pingObject.Opnum = 2;
        }

        try
        {
            Call(Semantics.IDEMPOTENT, pingObject);
        }
        catch (IOException e)
        {
            Log.Logger.Error(e, "ComOxidStub call");
        }

        // returns setId.
        return pingObject.SetId;
    }

    /// <summary>
    /// Close
    /// </summary>
    public void Close()
    {
        try
        {
            Detach();
        }
        catch (Exception e)
        {
            Log.Logger.Verbose(e, "ComOxidStub close");
        }
    }
}
