//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Smb;
using Opc.Classic.Dcom.Smb.Rpc;
using Opc.Classic.Dcom.Transport;


namespace Opc.Classic.Dcom.Rpc.Ncacn_Np;

/// <summary>Legacy DCE/RPC transport over SMB2 named pipes.</summary>
public sealed class RpcTransport : ITransport, IDisposable
{
    private readonly MemoryStream _pendingRequest = new();
    private readonly NcacnNpEndPoint _endpoint;
    private readonly Smb2TransportConnector? _transportConnector;
    private Smb2RpcTransportAdapter? _adapter;
    private bool _attached;
    private bool _disposed;

    /// <inheritdoc />
    public string Protocol => "ncacn_np";

    /// <inheritdoc />
    public PropertyBag Properties { get; }

    /// <summary>Create transport.</summary>
    public RpcTransport(string address, PropertyBag properties)
        : this(address, properties, transportConnector: null)
    {
    }

    /// <summary>Create transport with an injectable SMB2 connector.</summary>
    public RpcTransport(
        string address,
        PropertyBag properties,
        Smb2TransportConnector? transportConnector)
    {
        Properties = properties ?? TransportFactory.DefaultProperties;
        _endpoint = Parse(address);
        _transportConnector = transportConnector;
    }

    /// <inheritdoc />
    public IEndpoint Attach(PresentationSyntax syntax)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_attached)
        {
            throw new RpcException("Transport already attached.");
        }

        _adapter = BuildAdapter();
        _attached = true;
        return new ConnectionOrientedEndpoint(this, syntax);
    }

    /// <inheritdoc />
    public void Close()
    {
        try
        {
            _adapter?.Dispose();
        }
        finally
        {
            _adapter = null;
            _pendingRequest.SetLength(0);
            _attached = false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Close();
        _pendingRequest.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Send(NdrBuffer buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_attached || _adapter is null)
        {
            throw new RpcException("Transport not attached.");
        }

        var frame = new ReadOnlyMemory<byte>(buffer.Buf, 0, buffer.Length);
        if (IsWriteOnlyPdu(frame.Span))
        {
            _adapter.Write(frame);
            return;
        }

        _pendingRequest.Write(buffer.Buf, 0, buffer.Length);
    }

    /// <inheritdoc />
    public void Receive(NdrBuffer buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_attached || _adapter is null)
        {
            throw new RpcException("Transport not attached.");
        }

        ReadOnlyMemory<byte> response;
        if (_pendingRequest.Length == 0)
        {
            response = _adapter.Read(ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        }
        else
        {
            response = _adapter.Transceive(
                _pendingRequest.ToArray(),
                ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
            _pendingRequest.SetLength(0);
        }

        if (response.Length > buffer.GetCapacity())
        {
            throw new IOException($"Received DCE/RPC fragment length {response.Length} exceeds buffer capacity {buffer.GetCapacity()}.");
        }

        buffer.Reset();
        byte[] responseBytes = response.ToArray();
        buffer.WriteOctetArray(responseBytes, 0, responseBytes.Length);
        buffer.SetIndex(0);
    }

    private Smb2RpcTransportAdapter BuildAdapter()
    {
        NtlmAuthentication authentication = CreateNtlmAuthentication();
        var address = new SmbRpcAddress.Parsed(
            _endpoint.Host,
            "IPC$",
            _endpoint.PipeName,
            UserName: null,
            Domain: null,
            Password: null);

        var builder = new Smb2RpcTransportBuilder(
                address,
                CreateBlobProvider(authentication),
                () => authentication.EstablishedSessionKey)
            .UsePort(_endpoint.Port)
            .UseMaxSmb2MessageSize(MaxSmb2MessageSize());
        if (_transportConnector is not null)
        {
            builder.UseTransportConnector(_transportConnector);
        }

        return builder.Build();
    }

    private NtlmAuthentication CreateNtlmAuthentication()
    {
        var ntlmProperties = new PropertyBag();
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.lanManagerKey", "false");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.sign", "false");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.seal", "false");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.keyExchange", "false");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.keyLength", "128");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.ntlm2", "true");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.ntlmv2", "true");
        CopyNtlmProperty(ntlmProperties, "rpc.ntlm.allowV1", "false");
        ntlmProperties.SetProperty("rpc.ntlm.sso", "false");
        ntlmProperties.SetProperty("rpc.ntlm.domain", ReadProperty("rpc.ncacn_np.domain", "Opc.Classic.Dcom.Common.Ntlm.domain"));
        ntlmProperties.SetProperty(Security.USERNAME, ReadProperty("rpc.ncacn_np.username", "Opc.Classic.Dcom.Common.Ntlm.username"));
        ntlmProperties.SetProperty(Security.PASSWORD, Uri.UnescapeDataString(ReadProperty("rpc.ncacn_np.password", "Opc.Classic.Dcom.Common.Ntlm.password") ?? string.Empty));
        return new NtlmAuthentication(ntlmProperties);
    }

    private void CopyNtlmProperty(PropertyBag target, string propertyName, string defaultValue)
    {
        object? value = Properties.GetProperty(propertyName);
        target.SetProperty(propertyName, value ?? defaultValue);
    }

    private string? ReadProperty(string propertyName, string legacyPropertyName)
    {
        if (Properties.GetProperty(propertyName) is string value)
        {
            return value;
        }

        var envName = legacyPropertyName.Replace('.', '_').Replace('-', '_').ToUpperInvariant();
        return Environment.GetEnvironmentVariable(envName);
    }

    private int MaxSmb2MessageSize() => RpcTransportQuotas.GetInt32(
        Properties,
        RpcTransportQuotas.MaxSmb2MessageSizeProperty,
        RpcTransportQuotas.DefaultMaxSmb2MessageSize,
        RpcTransportQuotas.DefaultMaxSmb2MessageSize);

    private static NtlmsspBlobProvider CreateBlobProvider(NtlmAuthentication authentication)
    {
        object? ntlmMessage = null;
        return serverBlob =>
        {
            if (ntlmMessage is null)
            {
                var type1 = authentication.CreateType1();
                byte[] negotiate = type1.ToByteArray();
                authentication.SetNegotiateMessage(negotiate);
                ntlmMessage = type1;
                return negotiate;
            }

            if (serverBlob.IsEmpty)
            {
                return null;
            }

            var type2 = new Type2Message(serverBlob.ToArray());
            authentication.SetChallengeMessage(serverBlob.Span);
            var type3 = authentication.CreateType3(type2);
            ntlmMessage = type3;
            return type3.ToByteArray();
        };
    }

    private static NcacnNpEndPoint Parse(string address)
    {
        if (address is null)
        {
            throw new ProviderException("Null address.");
        }

        if (!address.StartsWith("ncacn_np:", StringComparison.OrdinalIgnoreCase))
        {
            throw new ProviderException("Not an ncacn_np address.");
        }

        try
        {
            return NcacnNpEndPoint.Parse(address);
        }
        catch (FormatException ex)
        {
            throw new ProviderException(ex.Message);
        }
    }

    private static bool IsWriteOnlyPdu(ReadOnlySpan<byte> pdu) =>
        pdu.Length > 3 &&
        (pdu[2] == 0x10 ||
         (pdu[3] & 0x40) != 0 ||
         (pdu[3] & 0x02) == 0);
}
