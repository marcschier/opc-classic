// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// Loopback integration: proves the loopback IOPCDataCallback scaffold lights up
// end-to-end inside one process:
//
//   DaCallbackEndpoint (TCP server, loopback bind)
//     → IOPCDataCallbackServerDispatcher (auto-generated)
//       → DaDataCallbackSink (AP3 bounded queue)
//
//   IOPCDataCallbackClientProxy (managed outbound)
//     → DcomCallChannel (RegisterInterfaceIpid binds the channel to the sink IPID)
//       → TCP transport back to the endpoint
//
// What this DOES NOT cover: the AP1/AP2/AP4 production flow where a real OPC
// server calls IObjectExporter::ResolveOxid2 against our listener and only
// THEN dials the callback transport. Real-Matrikon callback bring-up needs
// the OXID resolver wired up, which is deferred. The scaffold proven here is
// the wire-and-dispatch path that the future production flow will reuse.
//

using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Mcp.Tests;

public sealed class DaCallbackEndpointIntegrationTests
{
    [Test]
    public async Task StartAsync_BindsToLoopback_ByDefault()
    {
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);

        IPEndPoint? local = endpoint.LocalEndpoint;
        await Assert.That(local).IsNotNull();
        await Assert.That(local!.Address).IsEqualTo(IPAddress.Loopback);
        await Assert.That(local.Port).IsGreaterThan(0);
        await Assert.That(endpoint.IsRunning).IsTrue();
    }

    [Test]
    public async Task RegisterSink_AssignsFreshIpidPerSink_AndCountIncrements()
    {
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);

        var sinkA = new DaDataCallbackSink();
        var sinkB = new DaDataCallbackSink();

        Guid ipidA = endpoint.RegisterSink(sinkA);
        Guid ipidB = endpoint.RegisterSink(sinkB);

        await Assert.That(ipidA).IsNotEqualTo(Guid.Empty);
        await Assert.That(ipidB).IsNotEqualTo(Guid.Empty);
        await Assert.That(ipidA).IsNotEqualTo(ipidB);
        await Assert.That(endpoint.RegisteredSinkCount).IsEqualTo(2);

        await Assert.That(endpoint.UnregisterSink(ipidA)).IsTrue();
        await Assert.That(endpoint.RegisteredSinkCount).IsEqualTo(1);
        await Assert.That(endpoint.UnregisterSink(ipidA)).IsFalse();   // already gone
    }

    [Test]
    public async Task RegisterSink_BeforeStart_Throws()
    {
        await using var endpoint = new DaCallbackEndpoint();
        var sink = new DaDataCallbackSink();
        await Assert.That(() => { _ = endpoint.RegisterSink(sink); })
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task BuildSinkObjRef_BeforeStart_Throws()
    {
        await using var endpoint = new DaCallbackEndpoint();
        await Assert.That(() => { _ = endpoint.BuildSinkObjRef(Guid.NewGuid()); })
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task BuildSinkObjRef_UnknownIpid_Throws()
    {
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
        await Assert.That(() => { _ = endpoint.BuildSinkObjRef(Guid.NewGuid()); })
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task BuildSinkObjRef_EncodesIidAndIpidAndTcpBinding()
    {
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);

        var sink = new DaDataCallbackSink();
        Guid ipid = endpoint.RegisterSink(sink);
        IOpcInterfaceRef objref = endpoint.BuildSinkObjRef(ipid);

        await Assert.That(objref.Iid).IsEqualTo(IOPCDataCallback.InterfaceId);
        await Assert.That(objref.Ipid).IsEqualTo(ipid);
        await Assert.That(objref.PublicRefs).IsEqualTo(1u);
        await Assert.That(objref.Oxid).IsNotEqualTo(0UL);
        await Assert.That(objref.Oid).IsNotEqualTo(0UL);
        await Assert.That(objref.ResolverBindings.Count).IsGreaterThan(0);

        // First ushort is the TCP tower id (0x07); subsequent ushorts encode the
        // "127.0.0.1[<port>]" string with a NUL terminator, followed by the
        // string-binding terminator (0), then the security binding.
        await Assert.That(objref.ResolverBindings[0]).IsEqualTo((ushort)0x07);
        await Assert.That(objref.SecurityOffset).IsGreaterThan((ushort)0);
        await Assert.That(objref.SecurityOffset).IsLessThan((ushort)objref.ResolverBindings.Count);
    }

    [Test]
    public async Task InboundOnDataChange_RoutedByIpid_ReachesRegisteredSink()
    {
        // 1. Stand up the loopback callback endpoint.
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);

        var targetSink = new DaDataCallbackSink();
        var unrelatedSink = new DaDataCallbackSink();
        Guid targetIpid = endpoint.RegisterSink(targetSink);
        Guid unrelatedIpid = endpoint.RegisterSink(unrelatedSink);

        // 2. Open a managed client channel to the endpoint and register the
        //    target IPID so subsequent IOPCDataCallback calls route to it.
        IPEndPoint listenerEndpoint = endpoint.LocalEndpoint!;
        await using var channel = await ConnectChannelAsync(listenerEndpoint).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(IOPCDataCallback.InterfaceId, targetIpid);

        var proxy = new IOPCDataCallbackClientProxy(channel);

        // 3. Fire OnCancelComplete via the proxy. Use OnCancelComplete because
        //    it's the simplest no-payload IOPCDataCallback method - perfect for
        //    isolating "did the dispatch reach the right sink?".
        await proxy.OnCancelCompleteAsync(
            transactionId: 0xABCD,
            groupHandle: 0xDEF,
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        // 4. Verify the targetSink saw the call, the unrelated sink did not.
        await Assert.That(targetSink.OnCancelCompleteCount).IsEqualTo(1L);
        await Assert.That(unrelatedSink.OnCancelCompleteCount).IsEqualTo(0L);

        // Cleanup: prove Unregister disposes the registry mapping.
        await Assert.That(endpoint.UnregisterSink(targetIpid)).IsTrue();
        await Assert.That(endpoint.UnregisterSink(unrelatedIpid)).IsTrue();
        await Assert.That(endpoint.RegisteredSinkCount).IsEqualTo(0);
    }

    [Test]
    public async Task InboundOnDataChange_DeliversPayloadToSinkAndDrains()
    {
        // Exercises the payload-heavy IOPCDataCallback::OnDataChange path
        // (opnum 3) end-to-end: per-item VARIANT array, qualities, FILETIMEs,
        // errors. Verifies the sink enqueues a DataChangeNotification that
        // DrainItems can then read back with all fields preserved.
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
        var sink = new DaDataCallbackSink();
        Guid sinkIpid = endpoint.RegisterSink(sink);

        await using var channel = await ConnectChannelAsync(endpoint.LocalEndpoint!).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(IOPCDataCallback.InterfaceId, sinkIpid);
        var proxy = new IOPCDataCallbackClientProxy(channel);

        DateTimeOffset expectedTimestamp = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
        long ts = expectedTimestamp.ToFileTime();
        const ushort GoodQuality = 0xC0;
        const ushort BadQuality = 0x00;
        const int Success = 0;
        int Failure = unchecked((int)0x80070005u);

        await proxy.OnDataChangeAsync(
            transactionId: 0x42,
            groupHandle: 0xCAFE,
            masterQuality: 0,
            masterError: 0,
            clientHandles: [10, 20],
            values: [OpcVariant.FromInt32(123), OpcVariant.FromDouble(2.71)],
            qualities: [GoodQuality, BadQuality],
            timestamps: [ts, ts],
            errors: [Success, Failure],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        IReadOnlyList<DataChangeItem> drained = sink.DrainItems(maxItems: 0);
        await Assert.That(drained.Count).IsEqualTo(2);

        // Per-item ClientHandle / Value already covered above; G6 extends to
        // assert Quality, HResult, and Timestamp also survive the wire round-trip.
        await Assert.That(drained[0].ClientHandle).IsEqualTo(10);
        await Assert.That(drained[0].Value.Boxed).IsEqualTo(123);
        await Assert.That(drained[0].Quality.RawValue).IsEqualTo(GoodQuality);
        await Assert.That(drained[0].HResult).IsEqualTo(Success);
        await Assert.That(drained[0].Timestamp).IsEqualTo(expectedTimestamp);

        await Assert.That(drained[1].ClientHandle).IsEqualTo(20);
        await Assert.That(drained[1].Value.Boxed).IsEqualTo(2.71);
        await Assert.That(drained[1].Quality.RawValue).IsEqualTo(BadQuality);
        await Assert.That(drained[1].HResult).IsEqualTo(Failure);
        await Assert.That(drained[1].Timestamp).IsEqualTo(expectedTimestamp);

        await Assert.That(endpoint.UnregisterSink(sinkIpid)).IsTrue();
    }

    [Test]
    public async Task StopAsync_IsIdempotent()
    {
        var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
        await endpoint.StopAsync(CancellationToken.None).ConfigureAwait(false);
        await endpoint.StopAsync(CancellationToken.None).ConfigureAwait(false);
        await Assert.That(endpoint.IsRunning).IsFalse();
        await endpoint.DisposeAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task StartAsync_IsIdempotent()
    {
        await using var endpoint = new DaCallbackEndpoint();
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
        IPEndPoint first = endpoint.LocalEndpoint!;
        await endpoint.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
        IPEndPoint second = endpoint.LocalEndpoint!;
        await Assert.That(second.Port).IsEqualTo(first.Port);
    }

    private static async Task<DcomCallChannel> ConnectChannelAsync(IPEndPoint endpoint)
    {
        var client = new TcpClient();
        await client.ConnectAsync(endpoint.Address, endpoint.Port, TestContext.Current!.CancellationToken).ConfigureAwait(false);
        return new DcomCallChannel(new TcpClientTransport(client), new NoOpAuthContext());
    }

    private sealed class TcpClientTransport : IAsyncTransport
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TcpClientTransport(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            Input = PipeReader.Create(_stream);
            Output = PipeWriter.Create(_stream);
            RemoteEndpoint = client.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
        }

        public EndPoint RemoteEndpoint { get; }
        public PipeReader Input { get; }
        public PipeWriter Output { get; }

        public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

        public async ValueTask DisposeAsync()
        {
            await Input.CompleteAsync().ConfigureAwait(false);
            await Output.CompleteAsync().ConfigureAwait(false);
            await _stream.DisposeAsync().ConfigureAwait(false);
            _client.Dispose();
        }
    }
}
