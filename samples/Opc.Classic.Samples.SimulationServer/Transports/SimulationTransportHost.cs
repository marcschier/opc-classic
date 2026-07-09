// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Samples.SimulationServer.Ae;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// Options controlling which real transports the simulation server exposes.
/// </summary>
public sealed record SimulationTransportOptions
{
    /// <summary>ncacn_ip_tcp bind address for the DA listener (<c>host:port</c>, port 0 = ephemeral).</summary>
    public string DaListenAddress { get; init; } = "127.0.0.1:0";

    /// <summary>ncacn_ip_tcp bind address for the AE listener.</summary>
    public string AeListenAddress { get; init; } = "127.0.0.1:0";

    /// <summary>ncacn_ip_tcp bind address for the HDA listener.</summary>
    public string HdaListenAddress { get; init; } = "127.0.0.1:0";

    /// <summary>CLSID advertised for the DA server.</summary>
    public Guid DaClsid { get; init; } = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001");

    /// <summary>ProgID advertised for the DA server.</summary>
    public string DaProgId { get; init; } = "Opc.Classic.Simulation.DA.1";

    /// <summary>CLSID advertised for the AE server.</summary>
    public Guid AeClsid { get; init; } = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0002");

    /// <summary>ProgID advertised for the AE server.</summary>
    public string AeProgId { get; init; } = "Opc.Classic.Simulation.AE.1";

    /// <summary>CLSID advertised for the HDA server.</summary>
    public Guid HdaClsid { get; init; } = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0003");

    /// <summary>ProgID advertised for the HDA server.</summary>
    public string HdaProgId { get; init; } = "Opc.Classic.Simulation.HDA.1";
}

/// <summary>
/// Hosts the simulation server over real cross-platform transports. It starts the DA, AE, and
/// HDA managed listeners (<see cref="OpcDaServerHost" /> / <see cref="OpcAeServerHost" /> /
/// <see cref="OpcHdaServerHost" />) backed by the shared <see cref="SimulatedPlantModel" />, so
/// managed OPC clients and the Opc.Classic MCP server (via a <c>tcp://host:port</c> /
/// <c>dcom://host/ProgID</c> connection string) can reach the simulated address space over the
/// network, and — when registered on Windows — over native DCOM.
/// </summary>
public sealed class SimulationTransportHost : IAsyncDisposable
{
    private readonly List<ServiceProvider> _providers;
    private readonly SimDaHostServer _daServer;
    private readonly OpcDaServerHost _daHost;
    private readonly OpcAeServerHost _aeHost;
    private readonly OpcHdaServerHost _hdaHost;
    private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(250);
    private CancellationTokenSource? _tickerCts;
    private Task? _tickerTask;
    private bool _started;

    private SimulationTransportHost(
        List<ServiceProvider> providers,
        SimDaHostServer daServer,
        OpcDaServerHost daHost,
        OpcAeServerHost aeHost,
        OpcHdaServerHost hdaHost,
        SimulationTransportOptions options)
    {
        _providers = providers;
        _daServer = daServer;
        _daHost = daHost;
        _aeHost = aeHost;
        _hdaHost = hdaHost;
        Options = options;
    }

    /// <summary>The transport options this host was created with.</summary>
    public SimulationTransportOptions Options { get; }

    /// <summary>The bound DA endpoint after <see cref="StartAsync" />.</summary>
    public IPEndPoint? DaEndpoint => _daHost.LocalEndpoint as IPEndPoint;

    /// <summary>The bound AE endpoint after <see cref="StartAsync" />.</summary>
    public IPEndPoint? AeEndpoint => _aeHost.LocalEndpoint as IPEndPoint;

    /// <summary>The bound HDA endpoint after <see cref="StartAsync" />.</summary>
    public IPEndPoint? HdaEndpoint => _hdaHost.LocalEndpoint as IPEndPoint;

    /// <summary>The CLSID advertised for the DA server.</summary>
    public Guid DaClsid => Options.DaClsid;

    /// <summary>The ProgID advertised for the DA server.</summary>
    public string DaProgId => Options.DaProgId;

    /// <summary>The managed DA server implementation (for Windows CCW / SCM activation).</summary>
    public IOpcDaServer DaServer => _daServer;

    /// <summary>Creates a transport host over the given model and options.</summary>
    public static SimulationTransportHost Create(
        SimulatedPlantModel model,
        SimulationTransportOptions options,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var providers = new List<ServiceProvider>(3);

        var objectRegistry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, objectRegistry);
        ServiceProvider daProvider = BuildDaProvider(daServer, objectRegistry, options, loggerFactory);
        providers.Add(daProvider);
        var daHost = daProvider.GetRequiredService<OpcDaServerHost>();

        ServiceProvider aeProvider = BuildAeProvider(new SimAeServer(model, loggerFactory), options, loggerFactory);
        providers.Add(aeProvider);
        var aeHost = aeProvider.GetRequiredService<OpcAeServerHost>();

        ServiceProvider hdaProvider = BuildHdaProvider(new SimHdaHostServer(model), options, loggerFactory);
        providers.Add(hdaProvider);
        var hdaHost = hdaProvider.GetRequiredService<OpcHdaServerHost>();

        return new SimulationTransportHost(providers, daServer, daHost, aeHost, hdaHost, options);
    }

    /// <summary>Starts the DA, AE, and HDA transport listeners and the model value ticker.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _daHost.StartAsync(cancellationToken).ConfigureAwait(false);
        await _aeHost.StartAsync(cancellationToken).ConfigureAwait(false);
        await _hdaHost.StartAsync(cancellationToken).ConfigureAwait(false);

        _tickerCts = new CancellationTokenSource();
        _tickerTask = RunTickerAsync(_tickerCts.Token);
        _started = true;
    }

    /// <summary>Stops the value ticker and the transport listeners.</summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_started)
        {
            return;
        }

        if (_tickerCts is not null)
        {
            await _tickerCts.CancelAsync().ConfigureAwait(false);
        }

        if (_tickerTask is not null)
        {
            try
            {
                await _tickerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }

        _tickerCts?.Dispose();
        _tickerCts = null;
        _tickerTask = null;

        await _daHost.StopAsync(cancellationToken).ConfigureAwait(false);
        await _aeHost.StopAsync(cancellationToken).ConfigureAwait(false);
        await _hdaHost.StopAsync(cancellationToken).ConfigureAwait(false);
        _started = false;
    }

    private async Task RunTickerAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_tickInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            await _daServer.RefreshFromModelAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        await _daHost.DisposeAsync().ConfigureAwait(false);
        foreach (ServiceProvider provider in _providers)
        {
            await provider.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static ServiceProvider BuildDaProvider(
        SimDaHostServer daServer,
        OpcObjectRegistry objectRegistry,
        SimulationTransportOptions options,
        ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        services.AddLogging();
        services.AddSingleton<IOpcDaServer>(daServer);
        services.AddSingleton<IOpcAddressSpace>(daServer.BuildAddressSpace());
        services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
        services.AddSingleton(objectRegistry);
        services.AddSingleton<OpcDaServerHost>();
        services.Configure<OpcDaServerOptions>(o =>
        {
            o.Clsid = options.DaClsid;
            o.ProgId = options.DaProgId;
            o.FriendlyName = "Opc.Classic Full-Feature Simulation Server (DA)";
            o.ListenAddress = options.DaListenAddress;
        });
        return services.BuildServiceProvider();
    }

    private static ServiceProvider BuildAeProvider(
        IOpcAeServer aeServer,
        SimulationTransportOptions options,
        ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        services.AddLogging();
        services.AddSingleton<IOpcAeServer>(aeServer);
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcAeServerHost>();
        services.Configure<OpcAeServerOptions>(o =>
        {
            o.Clsid = options.AeClsid;
            o.ProgId = options.AeProgId;
            o.FriendlyName = "Opc.Classic Full-Feature Simulation Server (AE)";
            o.ListenAddress = options.AeListenAddress;
        });
        return services.BuildServiceProvider();
    }

    private static ServiceProvider BuildHdaProvider(
        SimHdaHostServer hdaServer,
        SimulationTransportOptions options,
        ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        services.AddLogging();
        services.AddSingleton<IOpcHdaServer>(hdaServer);
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcHdaServerHost>();
        services.Configure<OpcHdaServerOptions>(o =>
        {
            o.Clsid = options.HdaClsid;
            o.ProgId = options.HdaProgId;
            o.FriendlyName = "Opc.Classic Full-Feature Simulation Server (HDA)";
            o.ListenAddress = options.HdaListenAddress;
        });
        return services.BuildServiceProvider();
    }
}
