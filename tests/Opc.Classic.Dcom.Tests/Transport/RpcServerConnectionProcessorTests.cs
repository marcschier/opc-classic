// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;
using System.Security.Principal;
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
        (byte[] body, int hresult) = DecodeComResponse(response);
        await Assert.That(body).IsEquivalentTo(responseBody);
        await Assert.That(hresult).IsEqualTo(0);
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
    public async Task DispatcherFailure_returns_normal_response_with_trailing_hresult()
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
        ResponseCoPdu response = await ReadOutboundPduAs<ResponseCoPdu>(transport);

        (byte[] payload, int actualHresult) = DecodeComResponse(response);
        await Assert.That(response.CallId).IsEqualTo(2);
        await Assert.That(actualHresult).IsEqualTo(hresult);
        await Assert.That(payload).IsEmpty();
    }

    [Test]
    public async Task Dispatcher_S_FALSE_returns_payload_and_trailing_hresult()
    {
        byte[] responseBody = [0x10, 0x20];
        var dispatcher = new StubDispatcher(
            _ => DispatchResult.Success(
                responseBody,
                OpcResultId.False.Code));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = dispatcher,
            });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
        await WritePduToInbound(
            transport,
            NewRequest(contextId: 0, opnum: 5, callId: 2, payload: []));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        ResponseCoPdu response =
            await ReadOutboundPduAs<ResponseCoPdu>(transport);
        (byte[] payload, int hresult) = DecodeComResponse(response);
        await Assert.That(payload).IsEquivalentTo(responseBody);
        await Assert.That(hresult).IsEqualTo(OpcResultId.False.Code);
    }

    [Test]
    public async Task Raw_dispatcher_preserves_plain_NDR_and_uses_fault_for_RPC_status()
    {
        var success = new RawStubDispatcher(
            _ => DispatchResult.Success([0x51, 0x52]));
        var failure = new RawStubDispatcher(
            _ => DispatchResult.Fault(unchecked((int)0x800706D1u)));
        Guid successInterface = Guid.NewGuid();
        Guid failureInterface = Guid.NewGuid();
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [successInterface] = success,
                [failureInterface] = failure,
            });
        await using var transport = new InMemoryAsyncTransport();
        await WritePduToInbound(
            transport,
            NewBindForInterface(successInterface, contextId: 0, callId: 1));
        await WritePduToInbound(
            transport,
            NewAlterContextForInterface(
                failureInterface,
                contextId: 1,
                callId: 2));
        await WritePduToInbound(
            transport,
            NewRawRequest(contextId: 0, opnum: 3, callId: 3));
        await WritePduToInbound(
            transport,
            NewRawRequest(contextId: 1, opnum: 4, callId: 4));

        await RunProcessorAndShutdown(processor, transport);

        await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        await ReadOutboundPduAs<AlterContextResponsePdu>(transport);
        ResponseCoPdu response =
            await ReadOutboundPduAs<ResponseCoPdu>(transport);
        FaultCoPdu fault = await ReadOutboundPduAs<FaultCoPdu>(transport);
        await Assert.That(response.Stub).IsEquivalentTo(
            new byte[] { 0x51, 0x52 });
        await Assert.That(unchecked((int)fault.Status))
            .IsEqualTo(unchecked((int)0x800706D1u));
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
    public async Task Continuation_provider_is_selected_by_service_and_maps_principal()
    {
        const byte authenticationService = 42;
        var provider = new StubAuthenticationProvider(authenticationService);
        var mapper = new PrefixAuthorizationMapper();
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]),
            mapper);
        var dispatcher = new RecordingContextDispatcher();
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();

        await WriteFrameWithAuthVerifier(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
            authenticationService,
            authValue: [1],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);
        await WriteFrameWithAuthVerifier(
            transport,
            new Auth3Pdu { CallId = 2 },
            authenticationService,
            authValue: [3],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);
        await WriteFrameWithAuthVerifier(
            transport,
            NewRequest(contextId: 0, opnum: 5, callId: 3, payload: []),
            authenticationService,
            authValue: [9],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);

        await RunProcessorAndShutdown(processor, transport);

        byte[] bindAck = await ReadOutboundFrame(transport);
        AssertAuthenticationAlignment(
            bindAck,
            expectedAuthLength: 1,
            requirePadding: true);
        RpcPduAuthenticationData bindAuthentication = ReadAuthenticationData(bindAck);
        await Assert.That(bindAuthentication.AuthenticationService).IsEqualTo(authenticationService);
        await Assert.That(bindAuthentication.AuthValue).IsEquivalentTo(new byte[] { 2 });
        await ReadOutboundPduAs<ResponseCoPdu>(transport);
        await Assert.That(provider.CreateAcceptorCount).IsEqualTo(1);
        await Assert.That(provider.LastAcceptor!.AcceptedTokens.Count).IsEqualTo(2);
        await Assert.That(provider.LastAcceptor.FinalLegFlags)
            .IsEquivalentTo(new[] { false, true });
        await Assert.That(dispatcher.LastContext.HasValue).IsTrue();
        await Assert.That(dispatcher.LastContext!.Value.AuthenticationService).IsEqualTo(authenticationService);
        await Assert.That(dispatcher.LastContext.Value.Principal?.Identity?.Name)
            .IsEqualTo("mapped:mechanism-user");
    }

    [Test]
    [Arguments(43, 7, (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT)]
    [Arguments(42, 8, (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT)]
    [Arguments(42, 7, (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY)]
    [Arguments(42, 7, (byte)ProtectionLevel.PROTECTION_LEVEL_NONE)]
    public async Task Authentication_continuation_requires_exact_pinned_fields(
        int continuationService,
        int continuationContextId,
        byte continuationProtectionLevel)
    {
        const byte authenticationService = 42;
        const int contextId = 7;
        var provider = new StubAuthenticationProvider(
            authenticationService);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]));
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = new RecordingContextDispatcher(),
            });
        await using var transport = new InMemoryAsyncTransport();
        await WriteFrameWithAuthVerifier(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
            authenticationService,
            authValue: [1],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT,
            contextId: contextId);
        await WriteFrameWithAuthVerifier(
            transport,
            new Auth3Pdu { CallId = 2 },
            checked((byte)continuationService),
            authValue: [3],
            authLevel: continuationProtectionLevel,
            contextId: continuationContextId);

        await processor.ProcessConnectionAsync(
                transport,
                TestContext.Current!.CancellationToken)
            .AsTask()
            .WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current!.CancellationToken);

        _ = await ReadOutboundFrame(transport);
        await Assert.That(provider.CreateAcceptorCount).IsEqualTo(1);
        await Assert.That(provider.LastAcceptor!.AcceptedTokens.Count)
            .IsEqualTo(1);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Single_step_provider_completion_on_bind_or_alter_establishes_session(
        bool useAlterContext)
    {
        const byte authenticationService = 42;
        var provider = new StubAuthenticationProvider(
            authenticationService,
            completeOnFirstToken: true);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]));
        var dispatcher = new RecordingContextDispatcher();
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();
        int requestContextId = useAlterContext ? 1 : 0;

        if (useAlterContext)
        {
            await WritePduToInbound(
                transport,
                NewBindForInterface(InterfaceId, contextId: 0, callId: 1));
            await WriteFrameWithAuthVerifier(
                transport,
                NewAlterContextForInterface(InterfaceId, contextId: 1, callId: 2),
                authenticationService,
                authValue: [1],
                authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);
        }
        else
        {
            await WriteFrameWithAuthVerifier(
                transport,
                NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
                authenticationService,
                authValue: [1],
                authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);
        }

        await WriteFrameWithAuthVerifier(
            transport,
            NewRequest(requestContextId, opnum: 5, callId: 3, payload: []),
            authenticationService,
            authValue: [9],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);
        await RunProcessorAndShutdown(processor, transport);

        if (useAlterContext)
        {
            await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        }
        byte[] authenticationResponse = await ReadOutboundFrame(transport);
        byte expectedResponseType = (byte)(useAlterContext
            ? AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE
            : BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE);
        await Assert.That(authenticationResponse[ConnectionOrientedPdu.TYPE_OFFSET])
            .IsEqualTo(expectedResponseType);
        await Assert.That(ReadAuthenticationLength(authenticationResponse)).IsEqualTo(0);
        await ReadOutboundPduAs<ResponseCoPdu>(transport);
        await Assert.That(provider.LastAcceptor!.AcceptedTokens.Count).IsEqualTo(1);
        await Assert.That(dispatcher.LastContext!.Value.IsAuthenticated).IsTrue();
        await Assert.That(dispatcher.LastContext.Value.IsEstablished).IsTrue();
        await Assert.That(dispatcher.LastContext.Value.Principal?.Identity?.Name)
            .IsEqualTo("mechanism-user");
    }

    [Test]
    public async Task Optional_auth_unknown_verifier_bind_is_acknowledged_without_session()
    {
        const byte registeredService = 42;
        const byte unknownService = 43;
        var provider = new StubAuthenticationProvider(registeredService);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]),
            requireAuthentication: false);
        var dispatcher = new AuthenticatedOnlyContextDispatcher();
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher> { [InterfaceId] = dispatcher });
        await using var transport = new InMemoryAsyncTransport();

        await WriteFrameWithAuthVerifier(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
            unknownService,
            authValue: [1],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);
        await WriteFrameWithAuthVerifier(
            transport,
            NewRequest(contextId: 0, opnum: 5, callId: 2, payload: []),
            unknownService,
            authValue: [9],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);
        await RunProcessorAndShutdown(processor, transport);

        BindAcknowledgePdu ack = await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        ResponseCoPdu response = await ReadOutboundPduAs<ResponseCoPdu>(transport);
        await Assert.That(ack.ResultList[0].Result).IsEqualTo(PresentationResultCode.ACCEPTANCE);
        (_, int hresult) = DecodeComResponse(response);
        await Assert.That(hresult)
            .IsEqualTo(global::Opc.Classic.OpcResultId.AccessDenied.Code);
        await Assert.That(provider.CreateAcceptorCount).IsEqualTo(0);
        await Assert.That(dispatcher.LastContext!.Value.IsAuthenticated).IsFalse();
        await Assert.That(dispatcher.LastContext.Value.IsEstablished).IsFalse();
    }

    [Test]
    public async Task Required_auth_unknown_verifier_bind_is_rejected()
    {
        const byte registeredService = 42;
        const byte unknownService = 43;
        var provider = new StubAuthenticationProvider(registeredService);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]));
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = new RecordingContextDispatcher(),
            });
        await using var transport = new InMemoryAsyncTransport();

        await WriteFrameWithAuthVerifier(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
            unknownService,
            authValue: [1],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);

        await processor.ProcessConnectionAsync(
                transport,
                TestContext.Current!.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);

        BindNoAcknowledgePdu nak = await ReadOutboundPduAs<BindNoAcknowledgePdu>(transport);
        await Assert.That(nak.CallId).IsEqualTo(1);
        await Assert.That(provider.CreateAcceptorCount).IsEqualTo(0);
    }

    [Test]
    public async Task Continuation_provider_without_response_token_is_rejected()
    {
        const byte authenticationService = 42;
        var provider = new StubAuthenticationProvider(
            authenticationService,
            returnEmptyContinuation: true);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]));
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = new RecordingContextDispatcher(),
            });
        await using var transport = new InMemoryAsyncTransport();

        await WriteFrameWithAuthVerifier(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
            authenticationService,
            authValue: [1],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_CONNECT);

        await processor.ProcessConnectionAsync(
                transport,
                TestContext.Current!.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);

        BindNoAcknowledgePdu nak = await ReadOutboundPduAs<BindNoAcknowledgePdu>(transport);
        await Assert.That(nak.CallId).IsEqualTo(1);
        await Assert.That(provider.LastAcceptor!.AcceptedTokens.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Integrity_session_without_protection_context_is_rejected()
    {
        const byte authenticationService = 42;
        var provider = new StubAuthenticationProvider(
            authenticationService,
            completeOnFirstToken: true);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]));
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = new RecordingContextDispatcher(),
            });
        await using var transport = new InMemoryAsyncTransport();

        await WriteFrameWithAuthVerifier(
            transport,
            NewBindForInterface(InterfaceId, contextId: 0, callId: 1),
            authenticationService,
            authValue: [1],
            authLevel: (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);

        await processor.ProcessConnectionAsync(
                transport,
                TestContext.Current!.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);

        BindNoAcknowledgePdu nak = await ReadOutboundPduAs<BindNoAcknowledgePdu>(transport);
        await Assert.That(nak.CallId).IsEqualTo(1);
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

        (byte[] rootBody, int rootHresult) = DecodeComResponse(rootResponse);
        (byte[] objectBody, int objectHresult) = DecodeComResponse(objectResponse);
        await Assert.That(rootBody).IsEquivalentTo(rootPayload);
        await Assert.That(objectBody).IsEquivalentTo(objectPayload);
        await Assert.That(rootHresult).IsEqualTo(0);
        await Assert.That(objectHresult).IsEqualTo(0);
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
        (byte[] body, int hresult) = DecodeComResponse(response);
        await Assert.That(body).IsEquivalentTo(rootPayload);
        await Assert.That(hresult).IsEqualTo(0);
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
        await WriteFrameWithAuthVerifier(
            transport,
            pdu,
            authenticationService: 0,
            authValue: new byte[authBodyLength],
            authLevel);
    }

    private static async Task WriteFrameWithAuthVerifier(
        InMemoryAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        byte authenticationService,
        byte[] authValue,
        byte authLevel = 0,
        int contextId = 0)
    {
        byte[] frame = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        int authBodyLength = authValue.Length;
        int padding = PaddingTo(
            frame.Length - ConnectionOrientedPdu.HEADER_LENGTH,
            16);
        int verifierStart = frame.Length + padding;
        int totalLength = verifierStart + 8 + authBodyLength;
        byte[] forged = new byte[totalLength];
        frame.AsSpan().CopyTo(forged);
        forged[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET] = (byte)(totalLength & 0xFF);
        forged[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET + 1] = (byte)((totalLength >> 8) & 0xFF);
        forged[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET] = (byte)(authBodyLength & 0xFF);
        forged[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET + 1] = (byte)((authBodyLength >> 8) & 0xFF);
        // The sec_trailer follows body-relative 16-byte authentication padding:
        // [auth_type, auth_level, pad_length, reserved, context_id(4)].
        forged[verifierStart] = authenticationService;
        forged[verifierStart + 1] = authLevel;
        forged[verifierStart + 2] = checked((byte)padding);
        BinaryPrimitives.WriteInt32LittleEndian(
            forged.AsSpan(verifierStart + 4),
            contextId);
        authValue.CopyTo(forged.AsSpan(verifierStart + 8));
        await transport.WriteInboundAsync(forged);
    }

    private static int PaddingTo(int length, int alignment)
    {
        int remainder = length % alignment;
        return remainder == 0 ? 0 : alignment - remainder;
    }

    private static async Task<byte[]> ReadOutboundFrame(InMemoryAsyncTransport transport) =>
        await PduCodec.ReadPduFrameAsync(
            transport.ReadOutbound,
            TestContext.Current!.CancellationToken);

    private static RpcPduAuthenticationData ReadAuthenticationData(byte[] frame)
    {
        int authLength = frame[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET]
            | (frame[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET + 1] << 8);
        int fragmentLength = frame[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET]
            | (frame[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET + 1] << 8);
        int verifierStart = fragmentLength - authLength - 8;
        return new RpcPduAuthenticationData(
            frame[verifierStart],
            frame.AsSpan(verifierStart + 8, authLength).ToArray());
    }

    private static int ReadAuthenticationLength(byte[] frame) =>
        frame[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET]
        | (frame[ConnectionOrientedPdu.AUTH_LENGTH_OFFSET + 1] << 8);

    private static void AssertAuthenticationAlignment(
        byte[] frame,
        int expectedAuthLength,
        bool requirePadding)
    {
        int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET));
        int authLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET));
        int verifierStart = fragmentLength - authLength - 8;
        int padding = frame[verifierStart + 2];
        int unpaddedPduLength = verifierStart - padding;
        int bodyLength =
            unpaddedPduLength - ConnectionOrientedPdu.HEADER_LENGTH;
        int expectedPadding = (16 - (bodyLength % 16)) % 16;
        if (authLength != expectedAuthLength
            || (verifierStart - ConnectionOrientedPdu.HEADER_LENGTH) % 16 != 0
            || padding != expectedPadding
            || requirePadding && padding == 0)
        {
            throw new InvalidOperationException(
                "Authentication verifier alignment did not match MS-RPCE.");
        }
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

    private static (byte[] Payload, int Hresult) DecodeComResponse(
        ResponseCoPdu response)
    {
        ReadOnlyMemory<byte> body =
            OrpcEnvelope.ExtractResponseBody(response.Stub);
        int payloadLength = body.Length - sizeof(int);
        return (
            body[..payloadLength].ToArray(),
            BinaryPrimitives.ReadInt32LittleEndian(body.Span[payloadLength..]));
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

    private static RequestCoPdu NewRawRequest(
        int contextId,
        int opnum,
        int callId) =>
        new()
        {
            CallId = callId,
            ContextId = contextId,
            Opnum = opnum,
            AllocationHint = 1,
            Stub = [0x01],
        };

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

    private sealed class RawStubDispatcher : IRpcRawNdrDispatcher
    {
        private readonly Func<int, DispatchResult> _handler;

        public RawStubDispatcher(Func<int, DispatchResult> handler) =>
            _handler = handler;

        public ValueTask<DispatchResult> DispatchAsync(
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
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

    private sealed class AuthenticatedOnlyContextDispatcher : IRpcRequestContextDispatcher
    {
        public RpcRequestContext? LastContext { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(DispatchResult.Fault(
                global::Opc.Classic.OpcResultId.AccessDenied.Code));

        public ValueTask<DispatchResult> DispatchAsync(
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            RpcRequestContext requestContext,
            CancellationToken cancellationToken = default)
        {
            LastContext = requestContext;
            return ValueTask.FromResult(
                requestContext.IsAuthenticated && requestContext.IsEstablished
                    ? DispatchResult.Success([])
                    : DispatchResult.Fault(global::Opc.Classic.OpcResultId.AccessDenied.Code));
        }
    }

    private readonly record struct RpcPduAuthenticationData(
        byte AuthenticationService,
        byte[] AuthValue);

    private sealed class StubAuthenticationProvider : IRpcServerAuthenticationProvider
    {
        private readonly bool _completeOnFirstToken;
        private readonly bool _returnEmptyContinuation;

        public StubAuthenticationProvider(
            int authenticationService,
            bool completeOnFirstToken = false,
            bool returnEmptyContinuation = false)
        {
            AuthenticationService = authenticationService;
            _completeOnFirstToken = completeOnFirstToken;
            _returnEmptyContinuation = returnEmptyContinuation;
        }

        public int AuthenticationService { get; }

        public StubAuthenticationAcceptor? LastAcceptor { get; private set; }

        public int CreateAcceptorCount { get; private set; }

        public IRpcServerAuthenticationAcceptor CreateAcceptor()
        {
            CreateAcceptorCount++;
            LastAcceptor = new StubAuthenticationAcceptor(
                AuthenticationService,
                _completeOnFirstToken,
                _returnEmptyContinuation);
            return LastAcceptor;
        }
    }

    private sealed class StubAuthenticationAcceptor : IRpcServerAuthenticationAcceptor
    {
        private readonly int _authenticationService;
        private readonly bool _completeOnFirstToken;
        private readonly bool _returnEmptyContinuation;

        public StubAuthenticationAcceptor(
            int authenticationService,
            bool completeOnFirstToken,
            bool returnEmptyContinuation)
        {
            _authenticationService = authenticationService;
            _completeOnFirstToken = completeOnFirstToken;
            _returnEmptyContinuation = returnEmptyContinuation;
        }

        public List<byte[]> AcceptedTokens { get; } = [];

        public List<bool> FinalLegFlags { get; } = [];

        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel) =>
            AcceptCore(token, protectionLevel);

        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            bool isFinalLeg,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FinalLegFlags.Add(isFinalLeg);
            return AcceptCore(token, protectionLevel);
        }

        private RpcServerAuthenticationTokenResult AcceptCore(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel)
        {
            byte[] tokenBytes = token.ToArray();
            AcceptedTokens.Add(tokenBytes);
            if (tokenBytes.SequenceEqual(new byte[] { 1 }))
            {
                if (_completeOnFirstToken)
                {
                    return Complete(protectionLevel);
                }
                if (_returnEmptyContinuation)
                {
                    return default;
                }

                return RpcServerAuthenticationTokenResult.Continue(new byte[] { 2 });
            }
            if (!tokenBytes.SequenceEqual(new byte[] { 3 }))
            {
                throw new InvalidOperationException("Unexpected test token.");
            }

            return Complete(protectionLevel);
        }

        private RpcServerAuthenticationTokenResult Complete(
            OpcProtectionLevel protectionLevel)
        {
            var principal = new GenericPrincipal(
                new GenericIdentity("mechanism-user", "TEST"),
                []);
            return RpcServerAuthenticationTokenResult.Complete(
                new RpcServerAuthenticationSession(
                    _authenticationService,
                    principal,
                    protectionLevel));
        }
    }

    private sealed class PrefixAuthorizationMapper : IRpcServerAuthorizationMapper
    {
        public IPrincipal MapPrincipal(IPrincipal authenticatedPrincipal) =>
            new GenericPrincipal(
                new GenericIdentity(
                    $"mapped:{authenticatedPrincipal.Identity?.Name}",
                    authenticatedPrincipal.Identity?.AuthenticationType),
                []);
    }
}
