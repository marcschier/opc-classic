//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Transport;

/// <summary>
/// Cross-platform pipelines-backed transport contract for DCE/RPC over
/// ncacn_ip_tcp. Replaces the legacy <c>Opc.Classic.Dcom.Rpc.ITransport</c>
/// in the Phase 2C end-to-end refactor.
/// </summary>
public interface IAsyncTransport : IAsyncDisposable
{
    /// <summary>Gets the remote network endpoint associated with this transport.</summary>
    EndPoint RemoteEndpoint { get; }

    /// <summary>Gets the inbound byte stream received from the remote endpoint.</summary>
    PipeReader Input { get; }

    /// <summary>Gets the outbound byte stream sent to the remote endpoint.</summary>
    PipeWriter Output { get; }

    /// <summary>Flushes bytes written to <see cref="Output" />.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when pending outbound bytes are flushed.</returns>
    ValueTask FlushAsync(CancellationToken cancellationToken = default);
}
