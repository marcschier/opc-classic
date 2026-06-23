// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Opt-in smoke tests proving the simulation server is reachable over a real
/// cross-platform transport (the managed ncacn_ip_tcp listener), not just the
/// in-memory channels. A fully managed OPC DA client connects over TCP and reads
/// status + a simulated item value. The main MCP integration suite stays in-memory;
/// these are tagged so they can be filtered.
/// </summary>
public sealed class TransportSmokeTests
{
    [Test]
    public async Task Da_server_round_trips_status_over_real_tcp_listener()
    {
        var model = new SimulatedPlantModel();
        await using SimulationTransportHost host = SimulationTransportHost.Create(
            model,
            new SimulationTransportOptions { DaListenAddress = "127.0.0.1:0" },
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        await using DcomCallChannel channel = await ConnectAsync(host).ConfigureAwait(false);
        var proxy = new IOPCServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.VendorInfo).Contains(model.VendorInfo);
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Da);
    }

    [Test]
    public async Task Da_server_reads_simulated_item_value_over_real_tcp_listener()
    {
        var model = new SimulatedPlantModel();
        await using SimulationTransportHost host = SimulationTransportHost.Create(
            model,
            new SimulationTransportOptions { DaListenAddress = "127.0.0.1:0" },
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        await using DcomCallChannel channel = await ConnectAsync(host).ConfigureAwait(false);
        var proxy = new IOPCItemIOClientProxy(channel);

        string[] itemIds = ["Plant.Reactor1.Temperature"];
        int[] maxAges = [0];
        await proxy.ReadAsync(
            itemIds,
            maxAges,
            out OpcVariant[] values,
            out ushort[] qualities,
            out long[] timestamps,
            out int[] errors,
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(qualities[0]).IsEqualTo(OpcQuality.Good.RawValue);
        await Assert.That(values[0].Boxed).IsNotNull();
    }

    [Test]
    public async Task Ae_server_round_trips_status_over_real_tcp_listener()
    {
        var model = new SimulatedPlantModel();
        await using SimulationTransportHost host = SimulationTransportHost.Create(
            model,
            new SimulationTransportOptions { AeListenAddress = "127.0.0.1:0" },
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        IPEndPoint bound = host.AeEndpoint ?? throw new InvalidOperationException("No AE endpoint.");
        await using DcomCallChannel channel = await ConnectAsync(bound).ConfigureAwait(false);
        var proxy = new IOPCEventServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Ae);
        await Assert.That(status.VendorInfo).Contains(model.VendorInfo);
    }

    [Test]
    public async Task Hda_server_round_trips_status_over_real_tcp_listener()
    {
        var model = new SimulatedPlantModel();
        await using SimulationTransportHost host = SimulationTransportHost.Create(
            model,
            new SimulationTransportOptions { HdaListenAddress = "127.0.0.1:0" },
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        IPEndPoint bound = host.HdaEndpoint ?? throw new InvalidOperationException("No HDA endpoint.");
        await using DcomCallChannel channel = await ConnectAsync(bound).ConfigureAwait(false);
        var proxy = new IOPCHDA_ServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Hda);
        await Assert.That(status.VendorInfo).Contains(model.VendorInfo);
    }

    private static async Task<DcomCallChannel> ConnectAsync(IPEndPoint bound)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            bound.Address.ToString(),
            bound.Port,
            CancellationToken.None).ConfigureAwait(false);
        return new DcomCallChannel(transport, new NoOpAuthContext());
    }

    private static async Task<DcomCallChannel> ConnectAsync(SimulationTransportHost host)
    {
        IPEndPoint bound = host.DaEndpoint
            ?? throw new InvalidOperationException("Transport host did not expose a bound DA endpoint.");
        return await ConnectAsync(bound).ConfigureAwait(false);
    }
}
