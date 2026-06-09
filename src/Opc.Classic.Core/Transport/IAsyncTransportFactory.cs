//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Transport;

/// <summary>
/// Creates pipelines-backed DCE/RPC transports for remote endpoints.
/// </summary>
public interface IAsyncTransportFactory {
    /// <summary>Connects to a remote DCE/RPC endpoint.</summary>
    /// <param name="endpoint">The remote endpoint to connect to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The connected transport.</returns>
    ValueTask<IAsyncTransport> ConnectAsync(
        EndPoint endpoint,
        CancellationToken cancellationToken = default);
}
