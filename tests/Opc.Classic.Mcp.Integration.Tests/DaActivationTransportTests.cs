// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Verifies the simulation's modern DCOM cold-activation endpoint is hosted and reachable, and
/// that it enforces the activation security gate. A full authenticated cold-activation
/// (RemoteActivation → object call) additionally requires server-side NTLM bind handling on the
/// managed listener, which is not yet implemented (see <c>F4Auth</c> / <c>NtlmHandshakeProtocolTests</c>),
/// so an anonymous activation is correctly denied here. The activation handler itself
/// (<see cref="SimulationActivationServer" />) registers the DA generated dispatchers in the
/// shared object registry and returns the activated IPID — the modern (non-reflection) path that
/// becomes a working cold-activation once authenticated transport lands.
/// </summary>
public sealed class DaActivationTransportTests
{
    private static readonly Guid SimDaClsid = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001");

    [Test]
    public async Task Modern_activation_endpoint_is_hosted_and_denies_anonymous_activation()
    {
        var model = new SimulatedPlantModel();
        await using SimulationActivationHost host = SimulationActivationHost.Create(
            model,
            SimDaClsid,
            "127.0.0.1:0",
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        IPEndPoint endpoint = host.Endpoint ?? throw new InvalidOperationException("No activation endpoint.");

        await using DcomCallChannel activationChannel = await ConnectAsync(endpoint, objectIpid: null, ActivationServer.InterfaceId).ConfigureAwait(false);
        var request = new RemoteActivationRequest(
            Clsid: SimDaClsid,
            RequestedIids: new[] { IOPCServer.InterfaceId },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 0x07 });
        byte[] payload = IActivationCodec.EncodeRemoteActivationRequest(request);

        NdrCallResult result = await activationChannel.InvokeAsync(
            ActivationServer.InterfaceId,
            0,
            payload,
            CancellationToken.None).ConfigureAwait(false);

        // The endpoint is live and the IActivation security gate denies the anonymous call
        // (authenticated activation requires server-side NTLM bind handling — a core follow-up).
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.AccessDenied.Code);
    }

    [Test]
    public async Task Activation_handler_registers_routable_da_object_and_returns_ipid()
    {
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(SimDaClsid, daServer, registry);

        RemoteActivationResponse response = await activationServer.RemoteActivationAsync(
            new RemoteActivationRequest(
                Clsid: SimDaClsid,
                RequestedIids: new[] { IOPCServer.InterfaceId },
                ClientImpLevel: 3,
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }),
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.IpidRemUnknown).IsNotEqualTo(Guid.Empty);
        // The activated IPID is registered for IOPCServer, so object calls route to the DA server.
        await Assert.That(registry.ContainsInterface(IOPCServer.InterfaceId)).IsTrue();
    }

    private static async Task<DcomCallChannel> ConnectAsync(IPEndPoint endpoint, Guid? objectIpid, Guid preBindIid)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            CancellationToken.None).ConfigureAwait(false);
        Guid[] preBind = [preBindIid];
        return objectIpid is { } ipid
            ? new DcomCallChannel(transport, new NoOpAuthContext(), ipid, preBind)
            : new DcomCallChannel(transport, new NoOpAuthContext(), preBind);
    }
}
