//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.IO.Pipelines;
using System.Net;
using Opc.Classic.Transport;

namespace Opc.Classic.Testing;

/// <summary>
/// In-memory implementation of <see cref="IAsyncTransport"/> backed by a pair of
/// <see cref="Pipe"/> instances - one for inbound bytes (test feeds via
/// <see cref="WriteInboundAsync"/>), one for outbound (test reads via
/// <see cref="ReadOutbound"/>).
/// </summary>
public sealed class InMemoryAsyncTransport : IAsyncTransport
{
    private readonly Pipe _inbound = new();
    private readonly Pipe _outbound = new();

    /// <inheritdoc />
    public EndPoint RemoteEndpoint { get; } = new IPEndPoint(IPAddress.Loopback, 0);

    /// <inheritdoc />
    public PipeReader Input => _inbound.Reader;

    /// <inheritdoc />
    public PipeWriter Output => _outbound.Writer;

    /// <inheritdoc />
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        await _outbound.Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Gets the outbound side that tests read as if they were the remote endpoint.</summary>
    public PipeReader ReadOutbound => _outbound.Reader;

    /// <summary>Test helper: feed bytes onto the inbound side as if the remote endpoint sent them.</summary>
    /// <param name="data">The bytes to feed to <see cref="Input"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes after bytes are written.</returns>
    public async ValueTask WriteInboundAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        await _inbound.Writer.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _inbound.Writer.CompleteAsync().ConfigureAwait(false);
        await _outbound.Writer.CompleteAsync().ConfigureAwait(false);
    }
}
