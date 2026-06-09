//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// BackgroundService-backed accept loop for the modern DCOM server transport path.
/// </summary>
internal sealed class ComOxidRuntimeAcceptService : BackgroundService, IAsyncDisposable {
    private readonly IAsyncEndpoint _endpoint;
    private readonly Func<IAsyncTransport, CancellationToken, ValueTask> _connectionProcessor;
    private readonly Channel<IAsyncTransport> _connections;
    private readonly List<Task> _workers = new();
    private readonly int _workerCount;
    private int _acceptedConnectionCount;
    private int _queuedConnectionCount;
    private int _processedConnectionCount;
    private bool _disposed;
    private bool _endpointDisposed;

    public ComOxidRuntimeAcceptService(
        IAsyncEndpoint endpoint,
        Func<IAsyncTransport, CancellationToken, ValueTask> connectionProcessor)
        : this(endpoint, connectionProcessor, workerCount: 0) {
    }

    public ComOxidRuntimeAcceptService(
        IAsyncEndpoint endpoint,
        Func<IAsyncTransport, CancellationToken, ValueTask> connectionProcessor,
        int workerCount) {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _connectionProcessor = connectionProcessor ?? throw new ArgumentNullException(nameof(connectionProcessor));
        _workerCount = workerCount > 0 ? workerCount : Math.Max(1, Environment.ProcessorCount);
        _connections = Channel.CreateUnbounded<IAsyncTransport>(new UnboundedChannelOptions {
            SingleReader = false,
            SingleWriter = true,
        });
    }

    public int WorkerCount => _workerCount;

    public int AcceptedConnectionCount => Volatile.Read(ref _acceptedConnectionCount);

    public int QueuedConnectionCount => Volatile.Read(ref _queuedConnectionCount);

    public int ProcessedConnectionCount => Volatile.Read(ref _processedConnectionCount);

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _connections.Writer.TryComplete();
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        await DisposeEndpointAsync().ConfigureAwait(false);
        Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Dispose() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        _connections.Writer.TryComplete();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        for (var i = 0; i < _workerCount; i++) {
            _workers.Add(Task.Run(() => ProcessConnectionsAsync(stoppingToken), CancellationToken.None));
        }

        try {
            await foreach (var transport in _endpoint.AcceptConnectionsAsync(stoppingToken).WithCancellation(stoppingToken).ConfigureAwait(false)) {
                Interlocked.Increment(ref _acceptedConnectionCount);
                await _connections.Writer.WriteAsync(transport, stoppingToken).ConfigureAwait(false);
                Interlocked.Increment(ref _queuedConnectionCount);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
        }
        catch (ChannelClosedException) when (stoppingToken.IsCancellationRequested) {
        }
        catch (Exception e) {
            Log.Logger.Warning(e, "ComOxidRuntimeAcceptService accept loop failed");
        }
        finally {
            _connections.Writer.TryComplete();
            await Task.WhenAll(_workers).ConfigureAwait(false);
        }
    }

    private async Task ProcessConnectionsAsync(CancellationToken cancellationToken) {
        try {
            await foreach (var transport in _connections.Reader.ReadAllAsync(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                await ProcessConnectionAsync(transport, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
    }

    private async ValueTask ProcessConnectionAsync(IAsyncTransport transport, CancellationToken cancellationToken) {
        try {
            await _connectionProcessor(transport, cancellationToken).ConfigureAwait(false);
            Interlocked.Increment(ref _processedConnectionCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception e) {
            Log.Logger.Warning(e, "ComOxidRuntimeAcceptService worker failed to process connection");
        }
        finally {
            await transport.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async ValueTask DisposeEndpointAsync() {
        if (_endpointDisposed) {
            return;
        }

        _endpointDisposed = true;
        await _endpoint.DisposeAsync().ConfigureAwait(false);
    }
}
