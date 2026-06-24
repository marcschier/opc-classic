// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// Hosts modern DCOM <b>cold-activation</b> for the simulation DA server on a single listener:
/// it serves <c>IActivation</c> (via <see cref="ActivationServer" /> + <see cref="SimulationActivationServer" />)
/// and the activated DA object's ORPC calls (routed by IPID through the shared
/// <see cref="OpcObjectRegistry" />) on the same endpoint. A client connects, calls
/// <c>IActivation::RemoteActivation(CLSID)</c>, receives the activated object's IPID, then routes
/// subsequent <c>IOPCServer</c> calls to that IPID — exactly the sequence the modern <c>dcom://</c>
/// client performs (and the foundation for unmodified native explorers once an EPM/135 + NTLM
/// front-end is added).
/// </summary>
public sealed class SimulationActivationHost : IAsyncDisposable
{
    private readonly OpcServerListener _listener;
    private readonly OpcServerListener? _endpointMapperListener;
    private bool _started;

    private SimulationActivationHost(OpcServerListener listener, OpcServerListener? endpointMapperListener, Guid daClsid)
    {
        _listener = listener;
        _endpointMapperListener = endpointMapperListener;
        DaClsid = daClsid;
    }

    /// <summary>The CLSID clients activate to obtain the DA server.</summary>
    public Guid DaClsid { get; }

    /// <summary>The bound activation/object endpoint after <see cref="StartAsync" />.</summary>
    public IPEndPoint? Endpoint => _listener.LocalEndpoint as IPEndPoint;

    /// <summary>The bound endpoint-mapper endpoint after <see cref="StartAsync" />.</summary>
    public IPEndPoint? EndpointMapperEndpoint => _endpointMapperListener?.LocalEndpoint as IPEndPoint;

    /// <summary>Creates an activation host over the given model.</summary>
    public static SimulationActivationHost Create(
        SimulatedPlantModel model,
        Guid daClsid,
        string listenAddress,
        ILoggerFactory loggerFactory,
        string? endpointMapperListenAddress = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(listenAddress);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var objectRegistry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, objectRegistry);
        var aeServer = new SimAeHostServer(model);
        var hdaServer = new SimHdaHostServer(model);
        OpcServerListener? listener = null;
        var objectExporter = new IObjectExporterDispatcher(
            endpointProvider: () => listener?.LocalEndpoint as IPEndPoint,
            objectRegistry: objectRegistry);
        var activationServer = new SimulationActivationServer(
            daClsid,
            daServer,
            objectRegistry,
            aeServer: aeServer,
            hdaServer: hdaServer,
            endpointProvider: () => listener?.LocalEndpoint as IPEndPoint,
            remUnknownIpid: objectExporter.IRemUnknownIpid,
            logger: loggerFactory.CreateLogger<SimulationActivationServer>());

        var rootDispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [ActivationServer.InterfaceId] = new ActivationServer(activationServer, loggerFactory.CreateLogger<ActivationServer>()),
            [RemoteSCMActivatorDispatcher.InterfaceId] = new RemoteSCMActivatorDispatcher(activationServer, loggerFactory.CreateLogger<RemoteSCMActivatorDispatcher>()),
            [IObjectExporterDispatcher.InterfaceId] = objectExporter,
        };
        var processor = new RpcServerConnectionProcessor(
            rootDispatchers,
            objectRegistry,
            loggerFactory.CreateLogger<RpcServerConnectionProcessor>());
        var endpoint = new TcpServerEndpoint(ListenAddressParser.Parse(listenAddress));
        listener = new OpcServerListener(endpoint, processor, loggerFactory.CreateLogger<OpcServerListener>());

        OpcServerListener? endpointMapperListener = null;
        if (!string.IsNullOrWhiteSpace(endpointMapperListenAddress))
        {
            var epmDispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [EndpointMapperDispatcher.InterfaceId] = new EndpointMapperDispatcher(() => listener.LocalEndpoint as IPEndPoint),
            };
            var epmProcessor = new RpcServerConnectionProcessor(
                epmDispatchers,
                loggerFactory.CreateLogger<RpcServerConnectionProcessor>());
            var epmEndpoint = new TcpServerEndpoint(ListenAddressParser.Parse(endpointMapperListenAddress));
            endpointMapperListener = new OpcServerListener(epmEndpoint, epmProcessor, loggerFactory.CreateLogger<OpcServerListener>());
        }

        return new SimulationActivationHost(listener, endpointMapperListener, daClsid);
    }

    /// <summary>Starts the activation/object listener.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _listener.StartAsync(cancellationToken).ConfigureAwait(false);
        if (_endpointMapperListener is not null)
        {
            await _endpointMapperListener.StartAsync(cancellationToken).ConfigureAwait(false);
        }
        _started = true;
    }

    /// <summary>Stops the listener.</summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_started)
        {
            if (_endpointMapperListener is not null)
            {
                await _endpointMapperListener.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            await _listener.StopAsync(cancellationToken).ConfigureAwait(false);
            _started = false;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        if (_endpointMapperListener is not null)
        {
            await _endpointMapperListener.DisposeAsync().ConfigureAwait(false);
        }
        await _listener.DisposeAsync().ConfigureAwait(false);
    }
}
