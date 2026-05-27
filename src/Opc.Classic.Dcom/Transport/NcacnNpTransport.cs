//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Smb;
using Opc.Classic.Dcom.Smb.Rpc;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Pipelines-backed DCE/RPC transport over SMB2 named pipes (<c>ncacn_np</c>).
/// </summary>
public sealed class NcacnNpTransport : IAsyncTransport
{
    private const int Auth3PduType = 0x10;
    private const int PduTypeOffset = 2;
    private const int PduFlagsOffset = 3;
    private const int PfcLastFrag = 0x02;
    private const int PfcMaybe = 0x40;

    private readonly Smb2RpcTransportAdapter _adapter;
    private readonly Pipe _input = new();
    private readonly Pipe _output = new();
    private readonly SemaphoreSlim _flushLock = new(1, 1);
    private readonly int _maxReadFragment;
    private bool _disposed;

    private NcacnNpTransport(NcacnNpEndPoint endpoint, Smb2RpcTransportAdapter adapter)
    {
        RemoteEndpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _maxReadFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE;
    }

    /// <inheritdoc />
    public EndPoint RemoteEndpoint { get; }

    /// <inheritdoc />
    public PipeReader Input => _input.Reader;

    /// <inheritdoc />
    public PipeWriter Output => _output.Writer;

    /// <summary>Connects to the endpoint and opens the named pipe over IPC$.</summary>
    public static async ValueTask<NcacnNpTransport> ConnectAsync(
        NcacnNpEndPoint endpoint,
        IAuthContext smbAuthContext,
        int maxSmb2MessageSize = 0x1FFFF,
        Smb2TransportConnector? transportConnector = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(smbAuthContext);

        var parsed = new SmbRpcAddress.Parsed(
            endpoint.Host,
            "IPC$",
            endpoint.PipeName,
            UserName: null,
            Domain: null,
            Password: null);
        var builder = new Smb2RpcTransportBuilder(
                parsed,
                CreateBlobProvider(smbAuthContext),
                CreateSessionKeyProvider(smbAuthContext))
            .UsePort(endpoint.Port)
            .UseMaxSmb2MessageSize(maxSmb2MessageSize);

        if (transportConnector is not null)
        {
            builder.UseTransportConnector(transportConnector);
        }

        Smb2RpcTransportAdapter adapter = await builder.BuildAsync(cancellationToken).ConfigureAwait(false);
        return new NcacnNpTransport(endpoint, adapter);
    }

    /// <inheritdoc />
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _flushLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            FlushResult flush = await _output.Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            if (flush.IsCanceled)
            {
                return;
            }

            ReadResult read = await _output.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            byte[] request = read.Buffer.ToArray();
            _output.Reader.AdvanceTo(read.Buffer.End);
            if (request.Length == 0)
            {
                return;
            }

            if (IsWriteOnlyPdu(request))
            {
                await _adapter.WriteAsync(request, cancellationToken).ConfigureAwait(false);
                return;
            }

            ReadOnlyMemory<byte> response = await _adapter.TransceiveAsync(
                request,
                _maxReadFragment,
                cancellationToken).ConfigureAwait(false);
            await WriteResponseFragmentsAsync(response, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _flushLock.Release();
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
        _flushLock.Dispose();
        await _input.Writer.CompleteAsync().ConfigureAwait(false);
        await _input.Reader.CompleteAsync().ConfigureAwait(false);
        await _output.Writer.CompleteAsync().ConfigureAwait(false);
        await _output.Reader.CompleteAsync().ConfigureAwait(false);
        await _adapter.DisposeAsync().ConfigureAwait(false);
    }

    private static NtlmsspBlobProvider CreateBlobProvider(IAuthContext authContext)
    {
        var initialSent = false;
        return serverBlob =>
        {
            if (!initialSent)
            {
                initialSent = true;
                return authContext.BuildInitialToken();
            }

            if (serverBlob.IsEmpty)
            {
                return null;
            }

            byte[] nextToken = authContext.ProcessChallengeToken(serverBlob);
            return nextToken.Length == 0 ? null : nextToken;
        };
    }

    private static Smb2SessionKeyProvider? CreateSessionKeyProvider(IAuthContext authContext) =>
        authContext is IAuthSessionKeyProvider sessionKeyProvider
            ? sessionKeyProvider.GetSessionKey
            : null;

    private async ValueTask WriteResponseFragmentsAsync(
        ReadOnlyMemory<byte> response,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            await _input.Writer.WriteAsync(response, cancellationToken).ConfigureAwait(false);
            await _input.Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            if (IsLastFragment(response.Span))
            {
                return;
            }

            response = await _adapter.ReadAsync(_maxReadFragment, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsWriteOnlyPdu(ReadOnlySpan<byte> pdu) =>
        pdu.Length > PduFlagsOffset &&
        (pdu[PduTypeOffset] == Auth3PduType ||
         (pdu[PduFlagsOffset] & PfcMaybe) != 0 ||
         !IsLastFragment(pdu));

    private static bool IsLastFragment(ReadOnlySpan<byte> pdu) =>
        pdu.Length <= PduFlagsOffset || (pdu[PduFlagsOffset] & PfcLastFrag) != 0;
}

