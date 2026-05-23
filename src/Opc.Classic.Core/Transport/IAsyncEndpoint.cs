//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Opc.Classic.Transport;

/// <summary>
/// Server-side asynchronous DCE/RPC listener endpoint.
/// </summary>
public interface IAsyncEndpoint : IAsyncDisposable
{
    /// <summary>Gets the local endpoint where this listener accepts connections.</summary>
    EndPoint LocalEndpoint { get; }

    /// <summary>
    /// Accepts inbound transports as an async stream for consumption by hosted services.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of accepted transports.</returns>
    IAsyncEnumerable<IAsyncTransport> AcceptConnectionsAsync(
        CancellationToken cancellationToken = default);
}
