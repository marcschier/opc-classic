//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Transport;

namespace OpcClassic.Dcom.Transport;

/// <summary>
/// Factory that creates <see cref="DcomCallChannel" /> instances from async transports.
/// </summary>
public sealed class DcomCallChannelFactory
{
    private readonly IAsyncTransportFactory _transportFactory;

    /// <summary>Initializes a new instance of the <see cref="DcomCallChannelFactory" /> class.</summary>
    /// <param name="transportFactory">The transport factory used to connect to remote endpoints.</param>
    public DcomCallChannelFactory(IAsyncTransportFactory transportFactory)
    {
        ArgumentNullException.ThrowIfNull(transportFactory);

        _transportFactory = transportFactory;
    }

    /// <summary>Connects to a DCOM endpoint and returns a call channel over the connected transport.</summary>
    /// <param name="endpoint">The remote ncacn_ip_tcp endpoint.</param>
    /// <param name="clsidToActivate">CLSID reserved for the activation path; use <see cref="Guid.Empty" /> for already-activated channels.</param>
    /// <param name="authContext">The authentication context for the channel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The connected call channel.</returns>
    public async Task<ICallChannel> ConnectAsync(
        EndPoint endpoint,
        Guid clsidToActivate,
        IAuthContext authContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(authContext);
        _ = clsidToActivate;

        IAsyncTransport transport = await _transportFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
        return new DcomCallChannel(transport, authContext);
    }
}
