//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class OpcServerListenerTests
{
    private static readonly Guid InterfaceId = Guid.Parse("aa111111-2222-3333-4444-555555555555");

    [Test]
    public async Task LocalEndpoint_exposes_bound_port_before_start()
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(EmptyDispatchers());

        await using var listener = new OpcServerListener(endpoint, processor);

        var bound = listener.LocalEndpoint as IPEndPoint;
        await Assert.That(bound).IsNotNull();
        await Assert.That(bound!.Port).IsGreaterThan(0);
    }

    [Test]
    public async Task End_to_end_real_TCP_client_round_trip()
    {
        // A real TcpClient connects, sends a bind + request, and receives
        // a response. Proves the full ocom-1 stack (TcpServerEndpoint ->
        // accept loop -> processor -> dispatcher) works end-to-end over
        // a real network socket.
        var dispatcher = new RecordingDispatcher(payload: [0x10, 0x20, 0x30]);
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var listener = new OpcServerListener(endpoint, processor);

        await listener.StartAsync(TestContext.Current!.CancellationToken);
        var bound = (IPEndPoint)listener.LocalEndpoint;

        using var client = new TcpClient();
        await client.ConnectAsync(bound.Address, bound.Port, TestContext.Current!.CancellationToken);
        var stream = client.GetStream();
        PipeReader reader = PipeReader.Create(stream);
        PipeWriter writer = PipeWriter.Create(stream);

        BindPdu bind = NewBind(InterfaceId, contextId: 0, callId: 1);
        await WriteAndFlush(writer, PduCodec.EncodePdu(bind, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE));

        byte[] ackFrame = await PduCodec.ReadPduFrameAsync(reader, TestContext.Current!.CancellationToken);
        var ack = (BindAcknowledgePdu)PduCodec.DecodePdu(ackFrame);
        await Assert.That(ack.ResultList[0].Result).IsEqualTo(PresentationResultCode.ACCEPTANCE);

        RequestCoPdu request = NewRequest(contextId: 0, opnum: 7, callId: 2);
        await WriteAndFlush(writer, PduCodec.EncodePdu(request, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE));

        byte[] responseFrame = await PduCodec.ReadPduFrameAsync(reader, TestContext.Current!.CancellationToken);
        var response = (ResponseCoPdu)PduCodec.DecodePdu(responseFrame);
        await Assert.That(response.CallId).IsEqualTo(2);
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractResponseBody(response.Stub);
        await Assert.That(body.ToArray()).IsEquivalentTo(new byte[] { 0x10, 0x20, 0x30 });
        await Assert.That(dispatcher.LastOpnum).IsEqualTo(7);
    }

    [Test]
    public async Task StartAsync_then_StopAsync_drains_in_flight_connections()
    {
        var dispatcher = new RecordingDispatcher(payload: []);
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);
        var bound = (IPEndPoint)listener.LocalEndpoint;

        // Open a client to create an in-flight connection
        using var client = new TcpClient();
        await client.ConnectAsync(bound.Address, bound.Port, TestContext.Current!.CancellationToken);
        // Give the accept loop a moment to register the connection
        for (int i = 0; i < 50 && listener.InFlightConnectionCount == 0; i++)
        {
            await Task.Delay(20, TestContext.Current!.CancellationToken);
        }
        await Assert.That(listener.InFlightConnectionCount).IsGreaterThanOrEqualTo(1);

        // Close client side to let the connection processor's read loop exit cleanly
        client.Close();
        await listener.StopAsync(TestContext.Current!.CancellationToken);

        await Assert.That(listener.InFlightConnectionCount).IsEqualTo(0);
    }

    [Test]
    public async Task StartAsync_twice_throws()
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(EmptyDispatchers());
        await using var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);

        await TUnit.Assertions.Assert.That(async () => await listener.StartAsync(TestContext.Current!.CancellationToken))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Concurrent_StartAsync_yields_exactly_one_started_listener()
    {
        // The Start path must be serialized — otherwise two concurrent StartAsync
        // calls could both pass the "_acceptLoop is null" check and both spin up
        // their own accept loops, leaking one.
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(EmptyDispatchers());
        await using var listener = new OpcServerListener(endpoint, processor);

        const int parallelism = 16;
        var starts = new Task[parallelism];
        var startGate = new SemaphoreSlim(0, parallelism);
        for (int i = 0; i < parallelism; i++)
        {
            starts[i] = Task.Run(async () =>
            {
                await startGate.WaitAsync().ConfigureAwait(false);
                await listener.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
            });
        }

        startGate.Release(parallelism);

        int succeeded = 0;
        int alreadyStartedFaults = 0;
        await Task.WhenAll(starts.Select(async t =>
        {
            try { await t.ConfigureAwait(false); Interlocked.Increment(ref succeeded); }
            catch (InvalidOperationException) { Interlocked.Increment(ref alreadyStartedFaults); }
        })).ConfigureAwait(false);

        await Assert.That(succeeded).IsEqualTo(1);
        await Assert.That(alreadyStartedFaults).IsEqualTo(parallelism - 1);

        var bound = listener.LocalEndpoint as IPEndPoint;
        await Assert.That(bound).IsNotNull();
        await Assert.That(bound!.Port).IsGreaterThan(0);
    }

    [Test]
    public async Task Parallel_Start_and_Stop_cycles_leave_listener_in_clean_state()
    {
        // Drive Start/Stop cycles in parallel against the same listener; the
        // lifecycle lock must ensure each cycle's mutations are atomic so the
        // final state is either fully stopped (no orphaned accept loop) or fully
        // started exactly once.
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(EmptyDispatchers());
        var listener = new OpcServerListener(endpoint, processor);
        try
        {
            const int cycles = 6;
            for (int i = 0; i < cycles; i++)
            {
                await listener.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
                Task stop = listener.StopAsync(TestContext.Current!.CancellationToken);

                // Issue a parallel "second stop" while the first one drains —
                // exercises the disposed-or-already-null branch.
                Task secondStop = listener.StopAsync(TestContext.Current!.CancellationToken);
                await Task.WhenAll(stop, secondStop).ConfigureAwait(false);
            }

            await Assert.That(listener.InFlightConnectionCount).IsEqualTo(0);

            // After the loop the listener must be startable again without leaking.
            await listener.StartAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
            await Assert.That((listener.LocalEndpoint as IPEndPoint)!.Port).IsGreaterThan(0);
        }
        finally
        {
            await listener.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static IReadOnlyDictionary<Guid, IOpcServerDispatcher> EmptyDispatchers() =>
        new Dictionary<Guid, IOpcServerDispatcher>();

    private static async Task WriteAndFlush(PipeWriter writer, byte[] bytes)
    {
        Memory<byte> dest = writer.GetMemory(bytes.Length);
        bytes.AsSpan().CopyTo(dest.Span);
        writer.Advance(bytes.Length);
        await writer.FlushAsync(TestContext.Current!.CancellationToken);
    }

    private static BindPdu NewBind(Guid interfaceId, int contextId, int callId) =>
        new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList = [new(contextId, new PresentationSyntax(new UUID(interfaceId.ToString("D")), 0, 0))],
        };

    private static RequestCoPdu NewRequest(int contextId, int opnum, int callId)
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(Array.Empty<byte>(), Guid.NewGuid());
        return new RequestCoPdu
        {
            CallId = callId,
            ContextId = contextId,
            Opnum = opnum,
            AllocationHint = stub.Length,
            Stub = stub,
        };
    }

    private sealed class RecordingDispatcher : IOpcServerDispatcher
    {
        private readonly byte[] _payload;

        public RecordingDispatcher(byte[] payload) { _payload = payload; }

        public int LastOpnum { get; private set; } = -1;

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
        {
            LastOpnum = opnum;
            return ValueTask.FromResult(DispatchResult.Success(_payload));
        }
    }
}
