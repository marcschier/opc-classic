// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common.Ntlm;
using System;
using System.IO;
using System.Threading;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Connection
/// </summary>
public class NtlmConnection : DefaultConnection
{

    /// <summary>
    /// Create connection
    /// </summary>
    /// <param name="properties"></param>
    public NtlmConnection(PropertyBag properties)
    {
        _authentication = new NtlmAuthentication(properties);
        _properties = properties;
    }

    /// <summary>
    /// Set transmit length
    /// </summary>
    public int TransmitLength
    {
        set => _transmitBuffer = new NdrBuffer(new byte[value], 0);
        get => _transmitBuffer.Length;
    }

    /// <summary>
    /// Set receive length
    /// </summary>
    public int ReceiveLength
    {
        set => _receiveBuffer = new NdrBuffer(new byte[value], 0);
        get => _receiveBuffer.Length;
    }

    /// <inheritdoc/>
    protected internal override void IncomingRebind(AuthenticationVerifier verifier)
    {
        var maxNtlmMessageSize = MaxNtlmMessageSize();
        if (verifier.Body.Length < 12)
        {
            throw new IOException("NTLM verifier body is too short for a message header.");
        }
        if (verifier.Body.Length > maxNtlmMessageSize)
        {
            throw new IOException($"NTLM verifier body length {verifier.Body.Length} exceeds the configured quota of {maxNtlmMessageSize} bytes.");
        }

        switch (verifier.Body[8])
        {
            case 1:
                // server gets negotiate from client
                // setSecurity(null);
                _contextId = verifier.ContextId;
                _authentication.SetNegotiateMessage(verifier.Body);
                _ntlm = new Type1Message(verifier.Body, maxNtlmMessageSize);
                break;
            case 2:
                // client gets challenge from server
                _authentication.SetChallengeMessage(verifier.Body);
                _ntlm = new Type2Message(verifier.Body, maxNtlmMessageSize);
                break;
            case 3:
                // server gets authenticate from client
                _ntlm = new Type3Message(verifier.Body, maxNtlmMessageSize);
                if (UseNtlm2SessionSecurity())
                {
                    _authentication.CreateSecurityWhenServerWithMic(_ntlm, verifier.Body);
                    _security = _authentication.Security;
                }
                break;
            default:
                throw new IOException("Invalid NTLM message type.");
        }
    }

    /// <inheritdoc/>
    protected internal override AuthenticationVerifier OutgoingRebind()
    {
        if (_ntlm == null)
        {
            // client sends negotiate to server
            //  setSecurity(null);
            _contextId = Interlocked.Increment(ref _contextSerial);
            _ntlm = _authentication.CreateType1();
        }
        else if (_ntlm is Type1Message type1)
        {
            // server sends challenge to client
            _ntlm = _authentication.CreateType2(type1);
        }
        else if (_ntlm is Type2Message type2) // client sends authenticate to server
        {
            _ntlm = _authentication.CreateType3(type2);
            if (UseNtlm2SessionSecurity())
            {
                _security = _authentication.Security;
            }
        }
        else if (_ntlm is Type3Message)
        {
            // this simply means that we have sent the response to the challenge
            // now is the time to send the Auth Context only
            //             return new AuthenticationVerifier(
            //                     NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM,Security.PROTECTION_LEVEL_CONNECT,
            //                             contextId, new byte[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});
            return null;
        }
        else
        {
            throw new IOException("Unrecognized NTLM message.");
        }
        var protectionLevel = _ntlm.GetFlag(NtlmFlags.NtlmsspNegotiateSeal) ?
            ProtectionLevel.PROTECTION_LEVEL_PRIVACY :
                _ntlm.GetFlag(NtlmFlags.NtlmsspNegotiateSign) ?
                    ProtectionLevel.PROTECTION_LEVEL_INTEGRITY :
                    ProtectionLevel.PROTECTION_LEVEL_CONNECT;
        var body = _ntlm.ToByteArray();
        if (_ntlm is Type1Message)
        {
            _authentication.SetNegotiateMessage(body);
        }
        else if (_ntlm is Type2Message)
        {
            _authentication.SetChallengeMessage(body);
        }

        return new AuthenticationVerifier(NtlmAuthentication.AUTHENTICATIONSERVICENTLM,
            protectionLevel, _contextId, body);
    }

    private bool UseNtlm2SessionSecurity()
    {
        var value = _properties.GetProperty("rpc.ntlm.ntlm2");
        return value == null || Convert.ToBoolean(value);
    }

    private int MaxNtlmMessageSize() => RpcTransportQuotas.GetInt32(
        _properties,
        RpcTransportQuotas.MaxNtlmMessageSizeProperty,
        RpcTransportQuotas.DefaultMaxNtlmMessageSize,
        RpcTransportQuotas.DefaultMaxNtlmMessageSize);

    private static int _contextSerial;
    private readonly PropertyBag _properties;
    private readonly NtlmAuthentication _authentication;
    private NtlmMessage _ntlm;
}
