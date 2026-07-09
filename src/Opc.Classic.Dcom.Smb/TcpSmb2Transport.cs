// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net.Sockets;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// Abstraction over the raw byte-stream used to exchange SMB2 messages with a
/// server. Implementations frame SMB2 messages with the NetBIOS-over-TCP
/// 4-byte length prefix per [MS-CIFS] §2.2.1.
/// </summary>
public interface ISmb2Transport : IAsyncDisposable
{
    /// <summary>
    /// Send a complete SMB2 message (header + body) framed for the wire.
    /// </summary>
    Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken);

    /// <summary>
    /// Receive the next complete SMB2 message (header + body), de-framed.
    /// </summary>
    Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Production SMB2 transport over TCP port 445 with NetBIOS-over-TCP framing.
/// </summary>
public sealed class TcpSmb2Transport : ISmb2Transport
{
    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;
    private readonly int _maxPayloadLength;
    private bool _disposed;

    private TcpSmb2Transport(TcpClient tcp, int maxPayloadLength)
    {
        _tcp = tcp;
        _stream = tcp.GetStream();
        _maxPayloadLength = maxPayloadLength;
    }

    /// <summary>
    /// Opens a TCP connection to <paramref name="host" /> on the SMB port.
    /// </summary>
    public static Task<TcpSmb2Transport> ConnectAsync(
        string host,
        int port = 445,
        CancellationToken cancellationToken = default) =>
        ConnectAsync(host, port, Smb2Constants.MaxNetBiosFrameSize, cancellationToken);

    internal static async Task<TcpSmb2Transport> ConnectAsync(
        string host,
        int port,
        int maxPayloadLength,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        if (maxPayloadLength < 0 || maxPayloadLength > Smb2Constants.MaxNetBiosFrameSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPayloadLength),
                maxPayloadLength,
                $"SMB2 payload quota must be 0..{Smb2Constants.MaxNetBiosFrameSize} bytes.");
        }
        var tcp = new TcpClient();
        try
        {
            await tcp.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            return new TcpSmb2Transport(tcp, maxPayloadLength);
        }
        catch
        {
            tcp.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        var frame = new byte[NetBiosFraming.HeaderSize + packet.Length];
        NetBiosFraming.WriteHeader(frame, packet.Length);
        packet.CopyTo(frame.AsMemory(NetBiosFraming.HeaderSize));
        await _stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
        await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        var header = new byte[NetBiosFraming.HeaderSize];
        await ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        int payloadLength = NetBiosFraming.ReadPayloadLength(header, _maxPayloadLength);
        var payload = new byte[payloadLength];
        await ReadExactlyAsync(payload, cancellationToken).ConfigureAwait(false);
        return payload;
    }

    private async Task ReadExactlyAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        int total = 0;
        while (total < buffer.Length)
        {
            int read = await _stream.ReadAsync(buffer[total..], cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new EndOfStreamException(
                    $"TCP stream closed after {total} of {buffer.Length} expected bytes.");
            }
            total += read;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        await _stream.DisposeAsync().ConfigureAwait(false);
        _tcp.Dispose();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
