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
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// DA-specific <see cref="IOpcServerHost"/> implementation for managed in-process servers.
/// </summary>
public sealed class OpcDaServerHost : IOpcServerHost, IDisposable, IAsyncDisposable
{
    private static readonly Action<ILogger, Guid, string, Exception?> StartingHost = LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(1, nameof(StartingHost)),
        "OpcDaServerHost starting: CLSID={Clsid}, ProgId={ProgId}");

    private static readonly Action<ILogger, Guid, EndPoint, Exception?> HostListeningOn = LoggerMessage.Define<Guid, EndPoint>(
        LogLevel.Information,
        new EventId(2, nameof(HostListeningOn)),
        "OpcDaServerHost listening: CLSID={Clsid}, endpoint={Endpoint}");

    private static readonly Action<ILogger, Guid, Exception?> StoppingHost = LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(3, nameof(StoppingHost)),
        "OpcDaServerHost stopping: CLSID={Clsid}");

    private readonly IOpcDaServer _serverImpl;
    private readonly OpcDaServerOptions _options;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly ILogger<OpcDaServerHost> _logger;
    private OpcServerListener? _listener;

    /// <summary>Initializes a new instance of the <see cref="OpcDaServerHost"/> class.</summary>
    public OpcDaServerHost(
        IOpcDaServer serverImpl,
        IOptions<OpcDaServerOptions> options,
        OpcObjectRegistry objectRegistry,
        ILogger<OpcDaServerHost> logger)
    {
        _serverImpl = serverImpl ?? throw new ArgumentNullException(nameof(serverImpl));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string SpecName => "DA";

    /// <inheritdoc />
    public OpcClsidRegistration Registration => new(
        Clsid: _options.Clsid,
        ProgId: _options.ProgId,
        AssemblyName: typeof(IOpcDaServer).Assembly.GetName().Name ?? "Opc.Classic.Da",
        TypeName: _serverImpl.GetType().FullName ?? "Unknown",
        FriendlyName: _options.FriendlyName);

    /// <summary>
    /// Gets the local network endpoint the listener is bound to once
    /// <see cref="StartAsync"/> has completed. Returns <see langword="null"/>
    /// before <see cref="StartAsync"/> or after <see cref="StopAsync"/>.
    /// </summary>
    public EndPoint? LocalEndpoint => _listener?.LocalEndpoint;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartingHost(_logger, _options.Clsid, _options.ProgId, null);

        IPEndPoint listenEndpoint = ListenAddressParser.Parse(_options.ListenAddress ?? "127.0.0.1:0");
        var endpoint = new TcpServerEndpoint(listenEndpoint);
        var dispatcher = new IOPCServerServerDispatcher(_serverImpl);
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCServer.InterfaceId] = dispatcher,
            },
            _objectRegistry,
            _logger);
        _listener = new OpcServerListener(endpoint, processor, _logger);

        Task started = _listener.StartAsync(cancellationToken);
        HostListeningOn(_logger, _options.Clsid, _listener.LocalEndpoint, null);
        return started;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        StoppingHost(_logger, _options.Clsid, null);

        OpcServerListener? listener = _listener;
        _listener = null;
        if (listener is not null)
        {
            await listener.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "IDisposable is synchronous; the underlying StopAsync is async.")]
    public void Dispose()
    {
        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => new(StopAsync(CancellationToken.None));
}
