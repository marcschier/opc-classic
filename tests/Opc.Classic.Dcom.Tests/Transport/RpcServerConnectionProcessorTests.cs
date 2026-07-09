// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class RpcServerConnectionProcessorTests
{
    private static readonly Guid InterfaceId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Test]
    public async Task BindPdu_for_known_interface_returns_acceptance()
    {
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = new StubDispatcher() });
        await using var transport = new InMemoryAsyncTransport();
        BindPdu bind = NewBindForInterface(InterfaceId, contextId: 0, callId: 1);
        await WritePduToInbound(transport, bind);

        await RunProcessorAndShutdown(processor, transport);

        BindAcknowledgePdu ack = await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        await Assert.That(ack.ResultList.Length).IsEqualTo(1);
        await Assert.That(ack.ResultList[0].Result).IsEqualTo(PresentationResultCode.ACCEPTANCE);
        await Assert.That(ack.CallId).IsEqualTo(1);
    }

    [Test]
    public async Task BindPdu_for_unknown_interface_returns_rejection()
    {
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = new StubDispatcher() });
        await using var transport = new InMemoryAsyncTransport();
        BindPdu bind = NewBindForInterface(Guid.NewGuid(), contextId: 0, callId: 1);
        await WritePduToInbound(transport, bind);

        await RunProcessorAndShutdown(processor, transport);

        BindAcknowledgePdu ack = await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        await Assert.That(ack.ResultList[0].Result).IsEqualTo(PresentationResultCode.PROVIDER_REJECTION);
        await Assert.That(ack.ResultList[0].Reason).IsEqualTo(PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED);
    }

    [Test]
    public async Task RequestPdu_routes_to_dispatcher_and_returns_response()
    {
        byte[] responseBody = [0xAB, 0xCD, 0xEF];
        var dispatcher = new StubDispatcher(opnum => DispatchResult.Success(responseBody));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        await WritePduToInbound(transport, NewRequest(contextId: 0, opnum: 5, callId: 2, payload: [0x01, 0x02]));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        ResponseCoPdu response = await ReadOutboundPduAs<ResponseCoPdu>(transport);

        await Assert.That(response.CallId).IsEqualTo(2);
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractResponseBody(response.Stub);
        await Assert.That(body.ToArray()).IsEquivalentTo(responseBody);
        await Assert.That(dispatcher.LastOpnum).IsEqualTo(5);
    }

    [Test]
    public async Task RequestPdu_with_unknown_context_returns_fault()
    {
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = new StubDispatcher() });
        await using var transport = new InMemoryAsyncTransport();
        // Request without prior bind - context-id 7 is unknown
        await WritePduToInbound(transport, NewRequest(contextId: 7, opnum: 3, callId: 1, payload: []));

        await RunProcessorAndShutdown(processor, transport);

        FaultCoPdu fault = await ReadOutboundPduAs<FaultCoPdu>(transport);
        await Assert.That(fault.CallId).IsEqualTo(1);
        await Assert.That(fault.GetFlag(ConnectionOrientedPdu.PFC_DID_NOT_EXECUTE)).IsTrue();
    }

    [Test]
    public async Task DispatcherFailure_returns_fault_with_hresult_status()
    {
        const int hresult = unchecked((int)0x80004005);
        var dispatcher = new StubDispatcher(opnum => DispatchResult.Fault(hresult));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        await WritePduToInbound(transport, NewRequest(contextId: 0, opnum: 5, callId: 2, payload: []));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        FaultCoPdu fault = await ReadOutboundPduAs<FaultCoPdu>(transport);

        await Assert.That(fault.CallId).IsEqualTo(2);
        await Assert.That(unchecked((int)fault.Status)).IsEqualTo(hresult);
    }

    [Test]
    public async Task DispatcherThrowing_returns_fault_without_propagating_exception()
    {
        var dispatcher = new StubDispatcher(_ => throw new InvalidOperationException("boom"));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        await WritePduToInbound(transport, NewRequest(contextId: 0, opnum: 5, callId: 2, payload: []));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        FaultCoPdu fault = await ReadOutboundPduAs<FaultCoPdu>(transport);
        await Assert.That(fault.CallId).IsEqualTo(2);
    }

    [Test]
    public async Task ShutdownPdu_terminates_loop_cleanly()
    {
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = new StubDispatcher() });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        await WritePduToInbound(transport, new ShutdownPdu { CallId = 2 });

        // Don't complete inbound here — Shutdown should be enough to exit the loop
        Task processing = processor.ProcessConnectionAsync(transport, TestContext.Current!.CancellationToken).AsTask();
        await processing.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);
        await Assert.That(processing.IsCompletedSuccessfully).IsTrue();
    }

    [Test]
    public async Task AlterContextPdu_adds_new_context_mapping()
    {
        Guid secondInterface = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa");
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = new StubDispatcher(),
                [secondInterface] = new StubDispatcher(opnum => DispatchResult.Success([0xAA])),
            });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        await WritePduToInbound(transport, NewAlterContextForInterface(secondInterface, contextId: 1, callId: 2));
        await WritePduToInbound(transport, NewRequest(contextId: 1, opnum: 3, callId: 3, payload: []));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        AlterContextResponsePdu alterAck = await ReadOutboundPduAs<AlterContextResponsePdu>(transport);
        ResponseCoPdu response = await ReadOutboundPduAs<ResponseCoPdu>(transport);

        await Assert.That(alterAck.ResultList[0].Result).IsEqualTo(PresentationResultCode.ACCEPTANCE);
        await Assert.That(response.CallId).IsEqualTo(3);
    }

    [Test]
    public async Task AuthenticatedBind_is_rejected_with_bind_nak()
    {
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = new StubDispatcher() });
        await using var transport = new InMemoryAsyncTransport();
        BindPdu bind = NewBindForInterface(InterfaceId, contextId: 0, callId: 1);
        await WriteFrameWithAuthVerifier(transport, bind, authBodyLength: 16);

        // Processor exits after the rejection; no need for explicit shutdown
        await processor.ProcessConnectionAsync(transport, TestContext.Current!.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);

        BindNoAcknowledgePdu nak = await ReadOutboundPduAs<BindNoAcknowledgePdu>(transport);
        await Assert.That(nak.CallId).IsEqualTo(1);
        await Assert.That(nak.RejectReason).IsEqualTo(BindNoAcknowledgeReason.REASON_NOT_SPECIFIED);
    }

    [Test]
    public async Task ForgedAuthVerifier_on_unestablished_session_is_not_reported_as_authenticated()
    {
        // Regression (security): a request PDU can carry a forged sec_trailer
        // (auth_length > 0, attacker-chosen auth_level) that is never verified
        // when no NTLM context is established (ShouldProtectPackets is false).
        // The context reported to an IRpcRequestContextDispatcher must reflect
        // the (absent) established session — not the spoofable per-packet trailer
        // — otherwise an unauthenticated client could satisfy the activation
        // authorization gate (authenticated + integrity) and reach class-factory
        // creation. See RpcServerConnectionProcessor.TryDispatchAsync.
        var dispatcher = new RecordingContextDispatcher();
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        // Forge PKT_INTEGRITY in the trailer on a connection that never authenticated.
        await WriteFrameWithAuthVerifier(
            transport,
            NewRequest(contextId: 0, opnum: 5, callId: 2, payload: []),
            authBodyLength: 16,
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);

        await RunProcessorAndShutdown(processor, transport);

        await Assert.That(dispatcher.LastContext.HasValue).IsTrue();
        RpcRequestContext context = dispatcher.LastContext!.Value;
        await Assert.That(context.IsAuthenticated).IsFalse();
        await Assert.That(context.IsEstablished).IsFalse();
        await Assert.That(context.ProtectionLevel).IsNotEqualTo(OpcProtectionLevel.Integrity);
    }

    [Test]
    public async Task Constructor_throws_on_null_dispatcher_map()
    {
        await TUnit.Assertions.Assert.That(() => { _ = new RpcServerConnectionProcessor(null!); })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Request_with_object_uuid_routes_through_object_registry()
    {
        // Demonstrates the per-object IPID routing path. The root
        // dispatcher returns one payload; the per-object dispatcher
        // (registered with a specific IPID) returns a different one.
        // A request with PFC_OBJECT_UUID + matching Object UUID must
        // hit the per-object dispatcher; a request without it must
        // fall back to the root.
        byte[] rootPayload = [0x52, 0x4F, 0x4F, 0x54];
        byte[] objectPayload = [0x4F, 0x42, 0x4A, 0x21];

        var rootDispatcher = new StubDispatcher(_ => DispatchResult.Success(rootPayload));
        var objectDispatcher = new StubDispatcher(_ => DispatchResult.Success(objectPayload));
        var registry = new OpcObjectRegistry();
        Guid objectIpid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = objectDispatcher });

        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = rootDispatcher },
            registry);

        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        // Request 1: no Object UUID -> root dispatcher
        await WritePduToInbound(transport, NewRequest(contextId: 0, opnum: 3, callId: 2, payload: []));
        // Request 2: with Object UUID -> object dispatcher
        await WritePduToInbound(transport, NewRequestWithObject(contextId: 0, opnum: 3, callId: 3, ipid: objectIpid));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        ResponseCoPdu rootResponse = await ReadOutboundPduAs<ResponseCoPdu>(transport);
        ResponseCoPdu objectResponse = await ReadOutboundPduAs<ResponseCoPdu>(transport);

        ReadOnlyMemory<byte> rootBody = OrpcEnvelope.ExtractResponseBody(rootResponse.Stub);
        ReadOnlyMemory<byte> objectBody = OrpcEnvelope.ExtractResponseBody(objectResponse.Stub);
        await Assert.That(rootBody.ToArray()).IsEquivalentTo(rootPayload);
        await Assert.That(objectBody.ToArray()).IsEquivalentTo(objectPayload);
    }

    [Test]
    public async Task Request_with_unknown_object_uuid_falls_back_to_root_dispatcher()
    {
        byte[] rootPayload = [0xFA, 0x11, 0xBA, 0xCC];
        var rootDispatcher = new StubDispatcher(_ => DispatchResult.Success(rootPayload));
        var registry = new OpcObjectRegistry();

        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = rootDispatcher },
            registry);

        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(transport, NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        // Unknown IPID — registry has nothing — must fall back to root
        await WritePduToInbound(transport, NewRequestWithObject(contextId: 0, opnum: 3, callId: 2, ipid: Guid.NewGuid()));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        ResponseCoPdu response = await ReadOutboundPduAs<ResponseCoPdu>(transport);
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractResponseBody(response.Stub);
        await Assert.That(body.ToArray()).IsEquivalentTo(rootPayload);
    }

    private static RequestCoPdu NewRequestWithObject(int contextId, int opnum, int callId, Guid ipid)
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(Array.Empty<byte>(), Guid.NewGuid());
        var request = new RequestCoPdu
        {
            CallId = callId,
            ContextId = contextId,
            Opnum = opnum,
            AllocationHint = stub.Length,
            Stub = stub,
            Object = new UUID(ipid.ToString("D")),
        };
        return request;
    }

    private static async Task RunProcessorAndShutdown(
        RpcServerConnectionProcessor processor, InMemoryAsyncTransport transport)
    {
        await WritePduToInbound(transport, new ShutdownPdu { CallId = int.MaxValue });
        await processor.ProcessConnectionAsync(transport, TestContext.Current!.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);
    }

    private static async Task WritePduToInbound(InMemoryAsyncTransport transport, ConnectionOrientedPdu pdu)
    {
        byte[] frame = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        await transport.WriteInboundAsync(frame);
    }

    private static async Task WriteFrameWithAuthVerifier(
        InMemoryAsyncTransport transport, ConnectionOrientedPdu pdu, int authBodyLength, byte authLevel = 0)
    {
        byte[] frame = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        // Stamp the auth length so the processor's auth check triggers.
        // The actual auth verifier bytes do not need to be valid for the
        // rejection path — the processor decides before decoding.
        int totalLength = frame.Length + 8 + authBodyLength;
        byte[] forged = new byte[totalLength];
        frame.AsSpan().CopyTo(forged);
        forged[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET] = (byte)(totalLength & 0xFF);
        forged[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET + 1] = (byte)((totalLength >> 8) & 0xFF);
        forged[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET] = (byte)(authBodyLength & 0xFF);
        forged[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET + 1] = (byte)((authBodyLength >> 8) & 0xFF);
        // The sec_trailer header begins immediately after the original frame:
        // [auth_type, auth_level, pad_length, reserved, context_id(4)].
        forged[frame.Length + 1] = authLevel;
        await transport.WriteInboundAsync(forged);
    }

    private static async Task<T> ReadOutboundPduAs<T>(InMemoryAsyncTransport transport)
        where T : ConnectionOrientedPdu
    {
        byte[] frame = await PduCodec.ReadPduFrameAsync(transport.ReadOutbound, TestContext.Current!.CancellationToken);
        ConnectionOrientedPdu pdu = PduCodec.DecodePdu(frame);
        if (pdu is T typed)
        {
            return typed;
        }
        throw new InvalidOperationException($"Expected {typeof(T).Name} but read {pdu.GetType().Name}.");
    }

    private static BindPdu NewBindForInterface(Guid interfaceId, int contextId, int callId) =>
        new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList = [BuildContext(interfaceId, contextId)],
        };

    private static AlterContextPdu NewAlterContextForInterface(Guid interfaceId, int contextId, int callId) =>
        new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList = [BuildContext(interfaceId, contextId)],
        };

    private static RequestCoPdu NewRequest(int contextId, int opnum, int callId, byte[] payload)
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(payload, Guid.NewGuid());
        return new RequestCoPdu
        {
            CallId = callId,
            ContextId = contextId,
            Opnum = opnum,
            AllocationHint = stub.Length,
            Stub = stub,
        };
    }

    private static PresentationContext BuildContext(Guid interfaceId, int contextId) =>
        new(contextId, new PresentationSyntax(new UUID(interfaceId.ToString("D")), 0, 0));

    private sealed class StubDispatcher : IOpcServerDispatcher
    {
        private readonly Func<int, DispatchResult> _handler;

        public StubDispatcher() : this(_ => DispatchResult.Success(Array.Empty<byte>())) { }

        public StubDispatcher(Func<int, DispatchResult> handler)
        {
            _handler = handler;
        }

        public int LastOpnum { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
        {
            LastOpnum = opnum;
            return ValueTask.FromResult(_handler(opnum));
        }
    }

    private sealed class RecordingContextDispatcher : IRpcRequestContextDispatcher
    {
        public RpcRequestContext? LastContext { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            ValueTask.FromResult(DispatchResult.Success(Array.Empty<byte>()));

        public ValueTask<DispatchResult> DispatchAsync(
            int opnum, ReadOnlyMemory<byte> requestPayload, RpcRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            LastContext = requestContext;
            return ValueTask.FromResult(DispatchResult.Success(Array.Empty<byte>()));
        }
    }
}
