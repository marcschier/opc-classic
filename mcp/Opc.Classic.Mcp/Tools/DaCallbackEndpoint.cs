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
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// Loopback-only inbound DCOM endpoint for <see cref="IOPCDataCallback"/>
/// pushes from an OPC server. One instance is owned per
/// <see cref="Opc.Classic.Mcp.Sessions.DaClientState"/>; lazily started
/// on first subscribe and stopped when the last subscription is removed.
/// </summary>
/// <remarks>
/// <para>
/// This is the Track AU scaffold built on top of the AP3 sink. The endpoint
/// hosts an <see cref="OpcServerListener"/> bound to <see cref="IPAddress.Loopback"/>
/// and routes inbound calls via per-IPID <see cref="OpcObjectRegistry"/>
/// lookup. Each registered sink gets its own IPID so a multi-subscription
/// client routes notifications back to the correct
/// <see cref="DaDataCallbackSink"/>.
/// </para>
/// <para>
/// <b>Loopback only.</b> The default bind address is
/// <see cref="IPAddress.Loopback"/> and there is intentionally no
/// environment-variable opt-in for routable binds. Production
/// inbound-callback bring-up against external OPC servers requires
/// <c>IObjectExporter</c> OXID-resolution support which is tracked
/// separately as AP1/AP2/AP4 — see <c>docs/interop/da-callbacks.md</c>.
/// </para>
/// </remarks>
public sealed class DaCallbackEndpoint : IAsyncDisposable
{
    private static readonly Action<ILogger, EndPoint, Exception?> EndpointStarted =
        LoggerMessage.Define<EndPoint>(LogLevel.Information, new EventId(1, nameof(EndpointStarted)),
            "DaCallbackEndpoint: loopback IOPCDataCallback listener bound to {Endpoint}");

    private static readonly Action<ILogger, EndPoint, Exception?> EndpointStopped =
        LoggerMessage.Define<EndPoint>(LogLevel.Information, new EventId(2, nameof(EndpointStopped)),
            "DaCallbackEndpoint: loopback IOPCDataCallback listener on {Endpoint} stopped");

    private readonly ILogger _logger;
    private readonly OpcObjectRegistry _registry = new();
    private readonly ConcurrentDictionary<Guid, IOPCDataCallback> _sinksByIpid = new();
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private OpcServerListener? _listener;
    private bool _disposed;

    /// <summary>Creates an unstarted endpoint.</summary>
    public DaCallbackEndpoint(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>True once <see cref="StartAsync"/> has bound the listener.</summary>
    public bool IsRunning => _listener is not null;

    /// <summary>Gets the bound network endpoint; null until <see cref="StartAsync"/> completes.</summary>
    public IPEndPoint? LocalEndpoint => _listener?.LocalEndpoint as IPEndPoint;

    /// <summary>Number of currently-registered sink IPIDs.</summary>
    public int RegisteredSinkCount => _sinksByIpid.Count;

    /// <summary>Starts the loopback listener. Idempotent — calling again is a no-op.</summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_listener is not null)
            {
                return;
            }

            var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
            var processor = new RpcServerConnectionProcessor(
                dispatchers: new Dictionary<Guid, IOpcServerDispatcher>(),
                objectRegistry: _registry,
                logger: _logger);
            var listener = new OpcServerListener(endpoint, processor, _logger);
            await listener.StartAsync(cancellationToken).ConfigureAwait(false);
            _listener = listener;

            EndpointStarted(_logger, listener.LocalEndpoint, null);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>
    /// Registers <paramref name="sink"/> under a fresh IPID and returns it.
    /// The IPID is what callers embed in the sink OBJREF handed to
    /// <c>IConnectionPoint::Advise</c>.
    /// </summary>
    public Guid RegisterSink(IOPCDataCallback sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _lifecycleLock.Wait();
        try
        {
            if (_listener is null)
            {
                throw new InvalidOperationException("DaCallbackEndpoint must be started before registering sinks.");
            }

            var dispatcher = new IOPCDataCallbackServerDispatcher(sink);
            Guid ipid = _registry.Register(new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCDataCallback.InterfaceId] = dispatcher,
            });
            _sinksByIpid[ipid] = sink;
            return ipid;
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>Unregisters a previously-registered sink IPID. Returns false when the IPID is not known.</summary>
    public bool UnregisterSink(Guid ipid)
    {
        if (ipid == Guid.Empty || _disposed)
        {
            return false;
        }

        _lifecycleLock.Wait();
        try
        {
            if (!_sinksByIpid.TryRemove(ipid, out _))
            {
                return false;
            }

            _registry.Unregister(ipid);
            return true;
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>
    /// Builds an OBJREF_STANDARD interface pointer for the sink registered
    /// under <paramref name="ipid"/>, pointing back at this endpoint's
    /// bound TCP endpoint. Caller hands this to
    /// <c>IConnectionPoint::Advise</c>.
    /// </summary>
    public IOpcInterfaceRef BuildSinkObjRef(Guid ipid)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _lifecycleLock.Wait();
        try
        {
            if (_listener?.LocalEndpoint is not IPEndPoint endpoint)
            {
                throw new InvalidOperationException("DaCallbackEndpoint is not started or did not bind a TCP endpoint.");
            }

            if (!_sinksByIpid.ContainsKey(ipid))
            {
                throw new ArgumentException($"IPID {ipid:D} is not registered with this endpoint.", nameof(ipid));
            }

            return OpcSinkObjRefBuilder.Build(IOPCDataCallback.InterfaceId, ipid, endpoint);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>Stops the listener; idempotent.</summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            return Task.CompletedTask;
        }
        return StopCoreAsync(cancellationToken);
    }

    private async Task StopCoreAsync(CancellationToken cancellationToken)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            OpcServerListener? listener = _listener;
            if (listener is null)
            {
                return;
            }

            _listener = null;
            await listener.StopAsync(cancellationToken).ConfigureAwait(false);
            await listener.DisposeAsync().ConfigureAwait(false);
            EndpointStopped(_logger, listener.LocalEndpoint, null);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await StopCoreAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            _disposed = true;
            _sinksByIpid.Clear();
            _lifecycleLock.Dispose();
        }
    }
}
