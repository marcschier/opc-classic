// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Full DA group-lifecycle smoke test proving an OPC DA client can do everything an explorer
/// needs against the simulation server over a real TCP listener: connect, add a group, add
/// items, sync-read live model values (kept fresh by the transport host's value ticker), write,
/// and remove the group. This exercises the same client/server path a native explorer (e.g.
/// Matrikon) uses, just over the cross-platform managed transport.
/// </summary>
public sealed class DaLifecycleTransportTests
{
    private const int CacheDataSource = 1;

    [Test]
    public async Task Da_group_lifecycle_round_trips_live_values_over_real_tcp_listener()
    {
        var model = new SimulatedPlantModel();
        await using SimulationTransportHost host = SimulationTransportHost.Create(
            model,
            new SimulationTransportOptions { DaListenAddress = "127.0.0.1:0" },
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        IPEndPoint endpoint = host.DaEndpoint ?? throw new InvalidOperationException("No DA endpoint.");

        await using DcomCallChannel rootChannel = await ConnectAsync(endpoint, objectIpid: null).ConfigureAwait(false);
        var server = new IOPCServerClientProxy(rootChannel);

        OpcServerStatus status = await server.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);

        await server.AddGroupAsync(
            name: "explorer-group",
            active: true,
            requestedUpdateRate: 250,
            clientGroupHandle: 99,
            timeBias: 0,
            percentDeadband: 0.0f,
            localeId: 0x409,
            requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
            serverGroupHandle: out int serverGroupHandle,
            revisedUpdateRate: out _,
            group: out IOpcInterfaceRef groupRef,
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(serverGroupHandle).IsGreaterThan(0);
        await Assert.That(groupRef.Ipid).IsNotEqualTo(Guid.Empty);

        await using DcomCallChannel itemMgtChannel = await ConnectAsync(endpoint, groupRef.Ipid).ConfigureAwait(false);
        await using DcomCallChannel syncIoChannel = await ConnectAsync(endpoint, groupRef.Ipid).ConfigureAwait(false);
        var itemMgt = new IOPCItemMgtClientProxy(itemMgtChannel);
        var syncIo = new IOPCSyncIOClientProxy(syncIoChannel);

        OpcItemDef[] itemDefs =
        [
            new(null, "Random.Real8", Active: true, ClientHandle: 11, Blob: [], VarType.VT_EMPTY),
            new(null, "Plant.Reactor1.Temperature", Active: true, ClientHandle: 12, Blob: [], VarType.VT_EMPTY),
            new(null, "Bucket Brigade.Int4", Active: true, ClientHandle: 13, Blob: [], VarType.VT_EMPTY),
        ];
        await itemMgt.AddItemsAsync(itemDefs, out OpcItemResult[] addResults, out int[] addErrors, CancellationToken.None)
            .ConfigureAwait(false);

        await Assert.That(addResults.Length).IsEqualTo(3);
        await Assert.That(addErrors.All(static e => e >= 0)).IsTrue();
        int[] serverHandles = [.. addResults.Select(static r => r.ServerHandle)];
        await Assert.That(serverHandles.All(static h => h > 0)).IsTrue();

        // Let the transport host's value ticker populate item values from the model.
        await Task.Delay(TimeSpan.FromMilliseconds(600)).ConfigureAwait(false);

        OpcItemState[] states = await syncIo.ReadAsync(CacheDataSource, serverHandles, out int[] readErrors, CancellationToken.None)
            .ConfigureAwait(false);
        await Assert.That(states.Length).IsEqualTo(3);
        await Assert.That(readErrors.All(static e => e >= 0)).IsTrue();
        await Assert.That(states.All(static s => !s.Value.IsEmpty)).IsTrue();

        int[] writeErrors = await syncIo.WriteAsync([serverHandles[2]], [OpcVariant.FromInt32(42)], CancellationToken.None)
            .ConfigureAwait(false);
        await Assert.That(writeErrors.All(static e => e >= 0)).IsTrue();

        // The write to the writable Bucket Brigade.Int4 must persist across ticker cycles.
        await Task.Delay(TimeSpan.FromMilliseconds(600)).ConfigureAwait(false);
        OpcItemState[] afterWrite = await syncIo.ReadAsync(CacheDataSource, [serverHandles[2]], out int[] afterErrors, CancellationToken.None)
            .ConfigureAwait(false);
        await Assert.That(afterErrors[0]).IsGreaterThanOrEqualTo(0);
        await Assert.That(afterWrite[0].Value.AsInt32()).IsEqualTo(42);

        await server.RemoveGroupAsync(serverGroupHandle, force: true, CancellationToken.None).ConfigureAwait(false);
    }

    private static async Task<DcomCallChannel> ConnectAsync(IPEndPoint endpoint, Guid? objectIpid)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            CancellationToken.None).ConfigureAwait(false);
        return objectIpid is { } ipid
            ? new DcomCallChannel(transport, new NoOpAuthContext(), ipid)
            : new DcomCallChannel(transport, new NoOpAuthContext());
    }
}
