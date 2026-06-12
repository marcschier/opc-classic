//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// AE-specific <see cref="IOpcServerHost"/> implementation for managed in-process servers.
/// </summary>
public sealed class OpcAeServerHost : IOpcServerHost, IDisposable, IAsyncDisposable
{
    private static readonly Action<ILogger, Guid, string, Exception?> StartingHost = LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(1, nameof(StartingHost)),
        "OpcAeServerHost starting: CLSID={Clsid}, ProgId={ProgId}");

    private static readonly Action<ILogger, Guid, EndPoint, Exception?> HostListeningOn = LoggerMessage.Define<Guid, EndPoint>(
        LogLevel.Information,
        new EventId(2, nameof(HostListeningOn)),
        "OpcAeServerHost listening: CLSID={Clsid}, endpoint={Endpoint}");

    private static readonly Action<ILogger, Guid, Exception?> StoppingHost = LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(3, nameof(StoppingHost)),
        "OpcAeServerHost stopping: CLSID={Clsid}");

    private readonly IOpcAeServer _serverImpl;
    private readonly OpcAeServerOptions _options;
    private readonly ILogger<OpcAeServerHost> _logger;
    private OpcServerListener? _listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcAeServerHost"/> class.
    /// </summary>
    public OpcAeServerHost(
        IOpcAeServer serverImpl,
        IOptions<OpcAeServerOptions> options,
        ILogger<OpcAeServerHost> logger)
    {
        _serverImpl = serverImpl ?? throw new ArgumentNullException(nameof(serverImpl));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string SpecName => "AE";

    /// <inheritdoc />
    public OpcClsidRegistration Registration => new(
        Clsid: _options.Clsid,
        ProgId: _options.ProgId,
        AssemblyName: typeof(IOpcAeServer).Assembly.GetName().Name ?? "Opc.Classic.Ae",
        TypeName: _serverImpl.GetType().FullName ?? "Unknown",
        FriendlyName: _options.FriendlyName);

    /// <summary>
    /// Gets the local network endpoint the listener is bound to once
    /// <see cref="StartAsync"/> has completed.
    /// </summary>
    public EndPoint? LocalEndpoint => _listener?.LocalEndpoint;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartingHost(_logger, _options.Clsid, _options.ProgId, null);

        IPEndPoint listenEndpoint = ListenAddressParser.Parse(_options.ListenAddress ?? "127.0.0.1:0");
        var endpoint = new TcpServerEndpoint(listenEndpoint);
        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCEventServer.InterfaceId] = new IOPCEventServerServerDispatcher(_serverImpl),
        };

        // Register additional AE interface dispatchers when the impl provides
        // them (AE 1.10 extensions + subscription / browser tearoffs).
        if (_serverImpl is IOPCEventServer2 eventServer2)
        {
            dispatchers[IOPCEventServer2.InterfaceId] = new IOPCEventServer2ServerDispatcher(eventServer2);
        }
        if (_serverImpl is IOPCEventSubscriptionMgt subscriptionMgt)
        {
            dispatchers[IOPCEventSubscriptionMgt.InterfaceId] = new IOPCEventSubscriptionMgtServerDispatcher(subscriptionMgt);
        }
        if (_serverImpl is IOPCEventSubscriptionMgt2 subscriptionMgt2)
        {
            dispatchers[IOPCEventSubscriptionMgt2.InterfaceId] = new IOPCEventSubscriptionMgt2ServerDispatcher(subscriptionMgt2);
        }
        if (_serverImpl is IOPCEventAreaBrowser areaBrowser)
        {
            dispatchers[IOPCEventAreaBrowser.InterfaceId] = new IOPCEventAreaBrowserServerDispatcher(areaBrowser);
        }

        var processor = new RpcServerConnectionProcessor(dispatchers, _logger);
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
