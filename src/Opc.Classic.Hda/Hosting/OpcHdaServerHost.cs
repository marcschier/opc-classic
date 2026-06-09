//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hosting;

namespace Opc.Classic.Hda.Hosting;

/// <summary>
/// HDA-specific <see cref="IOpcServerHost"/> implementation for managed in-process servers.
/// </summary>
public sealed class OpcHdaServerHost : IOpcServerHost, IDisposable, IAsyncDisposable {
    private static readonly Action<ILogger, Guid, string, Exception?> StartingHost = LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(1, nameof(StartingHost)),
        "OpcHdaServerHost starting: CLSID={Clsid}, ProgId={ProgId}");

    private static readonly Action<ILogger, Guid, EndPoint, Exception?> HostListeningOn = LoggerMessage.Define<Guid, EndPoint>(
        LogLevel.Information,
        new EventId(2, nameof(HostListeningOn)),
        "OpcHdaServerHost listening: CLSID={Clsid}, endpoint={Endpoint}");

    private static readonly Action<ILogger, Guid, Exception?> StoppingHost = LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(3, nameof(StoppingHost)),
        "OpcHdaServerHost stopping: CLSID={Clsid}");

    private readonly IOpcHdaServer _serverImpl;
    private readonly OpcHdaServerOptions _options;
    private readonly ILogger<OpcHdaServerHost> _logger;
    private OpcServerListener? _listener;

    /// <summary>Initializes a new instance of the <see cref="OpcHdaServerHost"/> class.</summary>
    public OpcHdaServerHost(
        IOpcHdaServer serverImpl,
        IOptions<OpcHdaServerOptions> options,
        ILogger<OpcHdaServerHost> logger) {
        _serverImpl = serverImpl ?? throw new ArgumentNullException(nameof(serverImpl));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string SpecName => "HDA";

    /// <inheritdoc />
    public OpcClsidRegistration Registration => new(
        Clsid: _options.Clsid,
        ProgId: _options.ProgId,
        AssemblyName: typeof(IOpcHdaServer).Assembly.GetName().Name ?? "Opc.Classic.Hda",
        TypeName: _serverImpl.GetType().FullName ?? "Unknown",
        FriendlyName: _options.FriendlyName);

    /// <summary>
    /// Gets the local network endpoint the listener is bound to once
    /// <see cref="StartAsync"/> has completed.
    /// </summary>
    public EndPoint? LocalEndpoint => _listener?.LocalEndpoint;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) {
        StartingHost(_logger, _options.Clsid, _options.ProgId, null);

        IPEndPoint listenEndpoint = ListenAddressParser.Parse(_options.ListenAddress ?? "127.0.0.1:0");
        var endpoint = new TcpServerEndpoint(listenEndpoint);
        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher> {
            [IOPCHDA_Server.InterfaceId] = new IOPCHDA_ServerServerDispatcher(_serverImpl),
        };

        // Register additional HDA interface dispatchers when the user impl
        // provides them. Each dispatcher reads the request, calls the matching
        // method on the impl, and encodes the response per the OPC HDA 1.20 IDL.
        if (_serverImpl is IOPCHDA_SyncRead syncRead) {
            dispatchers[IOPCHDA_SyncRead.InterfaceId] = new IOPCHDA_SyncReadServerDispatcher(syncRead);
        }
        if (_serverImpl is IOPCHDA_SyncUpdate syncUpdate) {
            dispatchers[IOPCHDA_SyncUpdate.InterfaceId] = new IOPCHDA_SyncUpdateServerDispatcher(syncUpdate);
        }
        if (_serverImpl is IOPCHDA_SyncAnnotations syncAnnotations) {
            dispatchers[IOPCHDA_SyncAnnotations.InterfaceId] = new IOPCHDA_SyncAnnotationsServerDispatcher(syncAnnotations);
        }
        // IOPCHDA_Browser: per-instance browser objects are created by
        // IOPCHDA_Server::CreateBrowse and tracked via per-IPID dispatch
        // (sub-object pattern); skipped at root level for now.
        if (_serverImpl is IOPCHDA_AsyncRead asyncRead) {
            dispatchers[IOPCHDA_AsyncRead.InterfaceId] = new IOPCHDA_AsyncReadServerDispatcher(asyncRead);
        }
        if (_serverImpl is IOPCHDA_AsyncUpdate asyncUpdate) {
            dispatchers[IOPCHDA_AsyncUpdate.InterfaceId] = new IOPCHDA_AsyncUpdateServerDispatcher(asyncUpdate);
        }
        if (_serverImpl is IOPCHDA_AsyncAnnotations asyncAnnotations) {
            dispatchers[IOPCHDA_AsyncAnnotations.InterfaceId] = new IOPCHDA_AsyncAnnotationsServerDispatcher(asyncAnnotations);
        }
        if (_serverImpl is IOPCHDA_Playback playback) {
            dispatchers[IOPCHDA_Playback.InterfaceId] = new IOPCHDA_PlaybackServerDispatcher(playback);
        }

        var processor = new RpcServerConnectionProcessor(dispatchers, _logger);
        _listener = new OpcServerListener(endpoint, processor, _logger);

        Task started = _listener.StartAsync(cancellationToken);
        HostListeningOn(_logger, _options.Clsid, _listener.LocalEndpoint, null);
        return started;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) {
        StoppingHost(_logger, _options.Clsid, null);

        OpcServerListener? listener = _listener;
        _listener = null;
        if (listener is not null) {
            await listener.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "IDisposable is synchronous; the underlying StopAsync is async.")]
    public void Dispose() {
        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => new(StopAsync(CancellationToken.None));
}
