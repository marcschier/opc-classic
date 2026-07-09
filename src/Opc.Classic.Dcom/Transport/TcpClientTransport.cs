// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Pipe-backed <see cref="IAsyncTransport"/> over a connected TCP socket.
/// Used by sample clients and integration tests to dial a managed
/// <c>OpcServerListener</c> over the wire.
/// </summary>
/// <remarks>
/// <para>
/// The transport takes ownership of the supplied <see cref="TcpClient"/>;
/// <see cref="DisposeAsync"/> completes the pipes and disposes the
/// socket. For most consumers the static convenience
/// <see cref="ConnectAsync(string,int,CancellationToken)"/> is the
/// simplest entry point.
/// </para>
/// <para>
/// The transport carries raw bytes only — no DCE/RPC framing, no auth.
/// Pair it with a <see cref="DcomCallChannel"/> + <see cref="IAuthContext"/>
/// to get the full DCOM-over-IP wire path.
/// </para>
/// </remarks>
public sealed class TcpClientTransport : IAsyncTransport, IAsyncDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private int _disposed;

    /// <summary>
    /// Wraps a pre-connected <see cref="TcpClient"/> as an
    /// <see cref="IAsyncTransport"/>. The transport takes ownership of
    /// the client; do not access the client directly after construction.
    /// </summary>
    public TcpClientTransport(TcpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (!client.Connected)
        {
            throw new ArgumentException("TcpClient must be connected before constructing the transport.", nameof(client));
        }
        _client = client;
        _stream = client.GetStream();
        Input = PipeReader.Create(_stream);
        Output = PipeWriter.Create(_stream);
        RemoteEndpoint = client.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
    }

    /// <inheritdoc />
    public EndPoint RemoteEndpoint { get; }

    /// <inheritdoc />
    public PipeReader Input { get; }

    /// <inheritdoc />
    public PipeWriter Output { get; }

    /// <inheritdoc />
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
        await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }
        await Input.CompleteAsync().ConfigureAwait(false);
        await Output.CompleteAsync().ConfigureAwait(false);
        await _stream.DisposeAsync().ConfigureAwait(false);
        _client.Dispose();
    }

    /// <summary>
    /// Opens a TCP connection to the supplied host/port and returns the
    /// wrapped transport. The caller is responsible for disposing the
    /// returned transport (or the <see cref="DcomCallChannel"/> built
    /// from it).
    /// </summary>
    /// <param name="host">DNS name or IP literal.</param>
    /// <param name="port">TCP port number (typically 51300+).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task<TcpClientTransport> ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        if (port < 1 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), port, "TCP port must be in the range [1, 65535].");
        }
        var client = new TcpClient();
        try
        {
            await client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            return new TcpClientTransport(client);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
