//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Cross-platform server-side <see cref="IAsyncEndpoint"/> backed by
/// <see cref="TcpListener"/>. Binds an <c>ncacn_ip_tcp</c> endpoint and
/// yields one <see cref="IAsyncTransport"/> per accepted connection.
/// </summary>
/// <remarks>
/// The endpoint starts listening as soon as the constructor completes so
/// callers can observe the bound port (resolves dynamic-port-0) via
/// <see cref="LocalEndpoint"/> before any consumer subscribes to
/// <see cref="AcceptConnectionsAsync"/>.
/// </remarks>
public sealed class TcpServerEndpoint : IAsyncEndpoint {
    private readonly TcpListener _listener;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="TcpServerEndpoint"/> bound to
    /// <paramref name="listenEndpoint"/> and immediately starts accepting.
    /// </summary>
    public TcpServerEndpoint(IPEndPoint listenEndpoint) {
        ArgumentNullException.ThrowIfNull(listenEndpoint);
        _listener = new TcpListener(listenEndpoint);
        _listener.Start();
    }

    /// <inheritdoc />
    public EndPoint LocalEndpoint => _listener.LocalEndpoint;

    /// <inheritdoc />
    public async IAsyncEnumerable<IAsyncTransport> AcceptConnectionsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        while (!cancellationToken.IsCancellationRequested && !_disposed) {
            TcpClient client;
            try {
                client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                yield break;
            }
            catch (ObjectDisposedException) {
                yield break;
            }
            catch (SocketException) when (_disposed) {
                yield break;
            }

            yield return new TcpServerTransport(client);
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() {
        if (_disposed) {
            return ValueTask.CompletedTask;
        }

        _disposed = true;
        try {
            _listener.Stop();
        }
        catch (SocketException) {
            // Ignored - listener was already stopped or never fully started.
        }
        _listener.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Server-side <see cref="IAsyncTransport"/> wrapping an accepted
    /// <see cref="TcpClient"/> and its <see cref="NetworkStream"/>. Mirrors
    /// the <c>TcpSocketTransport</c> client-side pattern used by
    /// <c>DcomOpcEnumCallChannelFactory</c>.
    /// </summary>
    private sealed class TcpServerTransport : IAsyncTransport {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private bool _disposed;

        public TcpServerTransport(TcpClient client) {
            _client = client;
            _stream = client.GetStream();
            Input = PipeReader.Create(_stream);
            Output = PipeWriter.Create(_stream);
            RemoteEndpoint = client.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
        }

        public EndPoint RemoteEndpoint { get; }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

        public async ValueTask DisposeAsync() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            await Input.CompleteAsync().ConfigureAwait(false);
            await Output.CompleteAsync().ConfigureAwait(false);
            await _stream.DisposeAsync().ConfigureAwait(false);
            _client.Dispose();
        }
    }
}
