// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Smb.Rpc;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Factory that creates <see cref="NcacnNpTransport" /> instances for <see cref="NcacnNpEndPoint" /> values.
/// </summary>
public sealed class NcacnNpTransportFactory : IAsyncTransportFactory
{
    private readonly IAuthContext _smbAuthContext;
    private readonly int _maxSmb2MessageSize;
    private readonly Smb2TransportConnector? _transportConnector;

    /// <summary>
    /// Initializes a new named-pipe transport factory.
    /// </summary>
    public NcacnNpTransportFactory(
        IAuthContext smbAuthContext,
        PropertyBag? properties = null,
        Smb2TransportConnector? transportConnector = null)
    {
        _smbAuthContext = smbAuthContext ?? throw new ArgumentNullException(nameof(smbAuthContext));
        _maxSmb2MessageSize = RpcTransportQuotas.GetInt32(
            properties,
            RpcTransportQuotas.MaxSmb2MessageSizeProperty,
            RpcTransportQuotas.DefaultMaxSmb2MessageSize,
            RpcTransportQuotas.DefaultMaxSmb2MessageSize);
        _transportConnector = transportConnector;
    }

    /// <inheritdoc />
    public async ValueTask<IAsyncTransport> ConnectAsync(
        EndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (endpoint is not NcacnNpEndPoint namedPipeEndpoint)
        {
            throw new NotSupportedException($"Endpoint type '{endpoint.GetType().FullName}' is not an ncacn_np endpoint.");
        }

        return await NcacnNpTransport.ConnectAsync(
            namedPipeEndpoint,
            _smbAuthContext,
            _maxSmb2MessageSize,
            _transportConnector,
            cancellationToken).ConfigureAwait(false);
    }
}
