//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Top-level managed-server listener that combines an
/// <see cref="IAsyncEndpoint"/> with an
/// <see cref="RpcServerConnectionProcessor"/>. Owns the accept loop,
/// tracks in-flight per-connection processors, and drains them cleanly
/// on shutdown.
/// </summary>
/// <remarks>
/// <para>
/// One <see cref="OpcServerListener"/> per logical server (DA/AE/HDA) —
/// each gets its own <see cref="IAsyncEndpoint"/> binding and its own
/// dispatcher set. The endpoint's <see cref="IAsyncEndpoint.LocalEndpoint"/>
/// is surfaced via <see cref="LocalEndpoint"/> so OXID resolver bindings
/// and tests can read back the actually-bound port (resolves dynamic
/// port-0).
/// </para>
/// <para>
/// Each accepted connection runs in its own task spawned by the accept
/// loop. Tasks are tracked in <see cref="_inFlight"/> so
/// <see cref="StopAsync"/> can wait for them to drain.
/// </para>
/// </remarks>
public sealed class OpcServerListener : IAsyncDisposable {
    private static readonly Action<ILogger, EndPoint, Exception?> ListenerStarting =
        LoggerMessage.Define<EndPoint>(LogLevel.Information, new EventId(1, nameof(ListenerStarting)),
            "OpcServerListener: starting accept loop on {Endpoint}");

    private static readonly Action<ILogger, EndPoint, Exception?> ListenerStopping =
        LoggerMessage.Define<EndPoint>(LogLevel.Information, new EventId(2, nameof(ListenerStopping)),
            "OpcServerListener: stopping accept loop on {Endpoint}");

    private static readonly Action<ILogger, EndPoint, Exception> AcceptLoopFailed =
        LoggerMessage.Define<EndPoint>(LogLevel.Error, new EventId(3, nameof(AcceptLoopFailed)),
            "OpcServerListener: accept loop on {Endpoint} faulted");

    private static readonly Action<ILogger, EndPoint, Exception> ConnectionProcessorFailed =
        LoggerMessage.Define<EndPoint>(LogLevel.Warning, new EventId(4, nameof(ConnectionProcessorFailed)),
            "OpcServerListener: connection processor for {Remote} failed");

    private readonly IAsyncEndpoint _endpoint;
    private readonly RpcServerConnectionProcessor _processor;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Task, byte> _inFlight = new();
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;
    private bool _disposed;

    /// <summary>
    /// Initializes a new listener wrapping the given endpoint and
    /// connection processor.
    /// </summary>
    public OpcServerListener(
        IAsyncEndpoint endpoint,
        RpcServerConnectionProcessor processor,
        ILogger? logger = null) {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(processor);
        _endpoint = endpoint;
        _processor = processor;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Gets the bound network endpoint (resolves dynamic-port-0).</summary>
    public EndPoint LocalEndpoint => _endpoint.LocalEndpoint;

    /// <summary>Gets the number of connections currently being processed.</summary>
    public int InFlightConnectionCount => _inFlight.Count;

    /// <summary>
    /// Starts the accept loop. Returns immediately; the loop runs on a
    /// background task until <see cref="StopAsync"/> or the
    /// <paramref name="cancellationToken"/> fires.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_acceptLoop is not null) {
                throw new InvalidOperationException("OpcServerListener is already started.");
            }

            ListenerStarting(_logger, _endpoint.LocalEndpoint, null);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken loopToken = _cts.Token;
            _acceptLoop = Task.Run(() => AcceptLoopAsync(loopToken), CancellationToken.None);
        }
        finally {
            _lifecycleLock.Release();
        }
    }

    /// <summary>
    /// Cancels the accept loop, waits for in-flight connections to
    /// drain (bounded by <paramref name="cancellationToken"/>), and
    /// disposes the underlying endpoint.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken) {
        if (_disposed) {
            return;
        }

        // Lifecycle-bound critical section: snapshot + null out the cancellation
        // source and accept loop under the lock so a concurrent StartAsync /
        // StopAsync cannot observe a partially-mutated state. The actual
        // await-on-drain happens OUTSIDE the lock so a slow drain doesn't
        // deadlock a parallel Start.
        CancellationTokenSource? cts;
        Task? acceptLoop;
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            if (_disposed) {
                return;
            }

            ListenerStopping(_logger, _endpoint.LocalEndpoint, null);
            cts = _cts;
            acceptLoop = _acceptLoop;
            _cts = null;
            _acceptLoop = null;
        }
        finally {
            _lifecycleLock.Release();
        }

        await DrainAfterStopAsync(cts, acceptLoop, cancellationToken).ConfigureAwait(false);
    }

    private async Task DrainAfterStopAsync(CancellationTokenSource? cts, Task? acceptLoop, CancellationToken cancellationToken) {
        if (cts is not null) {
            await cts.CancelAsync().ConfigureAwait(false);
        }

        if (acceptLoop is not null) {
            try {
                await acceptLoop.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) {
                // Bounded by the caller's cancellation token.
            }
        }

        Task[] outstanding = _inFlight.Keys.ToArray();
        if (outstanding.Length > 0) {
            try {
                await Task.WhenAll(outstanding).WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) {
                // Drain best-effort; remaining connections are dropped by endpoint dispose.
            }
        }

        cts?.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        try {
            await StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally {
            _disposed = true;
            await _endpoint.DisposeAsync().ConfigureAwait(false);
            // Lifecycle lock intentionally NOT disposed: a lifecycle-bound
            // waiter (StartAsync / StopAsync racing with DisposeAsync) may
            // still need to Release() after observing _disposed=true. Leaving
            // the semaphore alive avoids a SemaphoreFullException on Release
            // and matches the same pattern used by DaCallbackEndpoint.
        }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken) {
        try {
            await foreach (IAsyncTransport transport in _endpoint
                .AcceptConnectionsAsync(cancellationToken)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false)) {
                IAsyncTransport capturedTransport = transport;
                Task connectionTask = Task.Run(
                    () => RunConnectionAsync(capturedTransport, cancellationToken),
                    CancellationToken.None);
                _inFlight.TryAdd(connectionTask, 0);
                _ = connectionTask.ContinueWith(
                    (t, state) => ((ConcurrentDictionary<Task, byte>)state!).TryRemove(t, out _),
                    _inFlight,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception ex) {
            AcceptLoopFailed(_logger, _endpoint.LocalEndpoint, ex);
        }
    }

    private async Task RunConnectionAsync(IAsyncTransport transport, CancellationToken cancellationToken) {
        try {
            await _processor.ProcessConnectionAsync(transport, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception ex) {
            ConnectionProcessorFailed(_logger, transport.RemoteEndpoint, ex);
        }
        finally {
            await transport.DisposeAsync().ConfigureAwait(false);
        }
    }
}
