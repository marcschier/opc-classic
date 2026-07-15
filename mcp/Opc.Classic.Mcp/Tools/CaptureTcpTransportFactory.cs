// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Mcp.Tools;

internal sealed class CaptureTcpTransportFactory : IAsyncTransportFactory
{
    public async ValueTask<IAsyncTransport> ConnectAsync(
        EndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        return endpoint switch
        {
            DnsEndPoint dns => await TcpClientTransport.ConnectAsync(
                dns.Host,
                dns.Port,
                cancellationToken).ConfigureAwait(false),
            IPEndPoint ip => await TcpClientTransport.ConnectAsync(
                ip.Address.ToString(),
                ip.Port,
                cancellationToken).ConfigureAwait(false),
            _ => throw new NotSupportedException(
                $"Endpoint type '{endpoint.GetType().FullName}' is not supported by TCP capture target cleanup."),
        };
    }
}
