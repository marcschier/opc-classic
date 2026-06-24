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
    private bool _started;

    private SimulationActivationHost(OpcServerListener listener, Guid daClsid)
    {
        _listener = listener;
        DaClsid = daClsid;
    }

    /// <summary>The CLSID clients activate to obtain the DA server.</summary>
    public Guid DaClsid { get; }

    /// <summary>The bound activation/object endpoint after <see cref="StartAsync" />.</summary>
    public IPEndPoint? Endpoint => _listener.LocalEndpoint as IPEndPoint;

    /// <summary>Creates an activation host over the given model.</summary>
    public static SimulationActivationHost Create(
        SimulatedPlantModel model,
        Guid daClsid,
        string listenAddress,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(listenAddress);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var objectRegistry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, objectRegistry);
        var activationServer = new SimulationActivationServer(
            daClsid,
            daServer,
            objectRegistry,
            loggerFactory.CreateLogger<SimulationActivationServer>());

        var rootDispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [ActivationServer.InterfaceId] = new ActivationServer(activationServer, loggerFactory.CreateLogger<ActivationServer>()),
        };
        var processor = new RpcServerConnectionProcessor(
            rootDispatchers,
            objectRegistry,
            loggerFactory.CreateLogger<RpcServerConnectionProcessor>());
        var endpoint = new TcpServerEndpoint(ListenAddressParser.Parse(listenAddress));
        var listener = new OpcServerListener(endpoint, processor, loggerFactory.CreateLogger<OpcServerListener>());
        return new SimulationActivationHost(listener, daClsid);
    }

    /// <summary>Starts the activation/object listener.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _listener.StartAsync(cancellationToken).ConfigureAwait(false);
        _started = true;
    }

    /// <summary>Stops the listener.</summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_started)
        {
            await _listener.StopAsync(cancellationToken).ConfigureAwait(false);
            _started = false;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        await _listener.DisposeAsync().ConfigureAwait(false);
    }
}
