//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

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
