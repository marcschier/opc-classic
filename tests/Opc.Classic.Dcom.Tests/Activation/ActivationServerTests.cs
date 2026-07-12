// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;

using LegacyRemoteActivationRequest = Opc.Classic.Dcom.Activation.RemoteActivationRequest;
using LegacyRemoteActivationResponse = Opc.Classic.Dcom.Activation.RemoteActivationResponse;

namespace Opc.Classic.Dcom.Tests.Activation;

public sealed class ActivationServerTests
{
    private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    private const int E_ACCESSDENIED = unchecked((int)0x80070005u);
    private const int E_INVALIDARG = unchecked((int)0x80070057u);
    private static readonly Guid TestClsid = new("11111111-2222-3333-4444-555555555555");
    private static readonly Guid IidIUnknown = OpcGuids.IID_IUnknown;

    [Test]
    public async Task RemoteActivation_via_rpc_processor_rejects_forged_verifier_without_established_session()
    {
        // Security regression: a forged sec_trailer (auth_length > 0, auth_level = PKT_INTEGRITY)
        // on a connection that never completed an NTLM handshake must NOT satisfy the
        // authenticated-and-integrity activation gate. The processor derives authorization from the
        // established NTLM session, not the spoofable per-packet trailer, so this request is rejected
        // with E_ACCESSDENIED before any class factory runs.
        LegacyActivationServer legacy = CreateLegacyServer(TestClsid, IidIUnknown);
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [ActivationServer.InterfaceId] = new ActivationServer(legacy),
            });
        await using var transport = new InMemoryAsyncTransport();
        byte[] requestPayload = IActivationCodec.EncodeRemoteActivationRequest(new LegacyRemoteActivationRequest(
            TestClsid,
            new[] { IidIUnknown },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 7 }));

        await WriteFrameWithAuthVerifier(transport, NewBindForInterface(ActivationServer.InterfaceId, contextId: 0, callId: 1));
        await WriteFrameWithAuthVerifier(transport, NewRequest(contextId: 0, opnum: 0, callId: 2, requestPayload));
        await RunProcessorAndShutdown(processor, transport);

        BindAcknowledgePdu ack = await ReadOutboundPduAs<BindAcknowledgePdu>(transport);
        FaultCoPdu fault = await ReadOutboundPduAs<FaultCoPdu>(transport);

        await Assert.That(ack.ResultList[0].Result).IsEqualTo(PresentationResultCode.ACCEPTANCE);
        await Assert.That(fault.CallId).IsEqualTo(2);
        await Assert.That(unchecked((int)fault.Status)).IsEqualTo(E_ACCESSDENIED);
    }

    [Test]
    public async Task RemoteActivation_with_integrity_session_activates_known_clsid_and_returns_objref()
    {
        // Happy path: an integrity-protected (authenticated) session activating a known CLSID
        // yields a well-formed OBJREF_STANDARD for the requested interface. The RPC-processor
        // gate is exercised by the rejection test above; here the dispatch path is driven with an
        // established integrity level so the class factory runs and the OBJREF is produced.
        LegacyActivationServer legacy = CreateLegacyServer(TestClsid, IidIUnknown);
        byte[] requestPayload = IActivationCodec.EncodeRemoteActivationRequest(new LegacyRemoteActivationRequest(
            TestClsid,
            new[] { IidIUnknown },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 7 }));

        DispatchResult result = await ActivationServer.DispatchRemoteActivationAsync(
            legacy,
            requestPayload,
            OpcProtectionLevel.Integrity);

        await Assert.That(result.IsSuccess).IsTrue();
        LegacyRemoteActivationResponse response = IActivationCodec.DecodeRemoteActivationResponse(
            result.Payload.Span,
            expectedInterfaceCount: 1);

        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.AuthnHint).IsEqualTo(5u);
        await Assert.That(response.Oxid).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.IpidRemUnknown).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.OxidBindings.Length).IsGreaterThan(0);
        await Assert.That(response.InterfaceResults[0].Hresult).IsEqualTo(0);
        byte[] objRef = response.InterfaceResults[0].ObjRef.ToArray();
        await Assert.That(ReadObjRefSignature(objRef)).IsEqualTo(0x574F454Du);
        await Assert.That(ReadObjRefIid(objRef)).IsEqualTo(IidIUnknown);
        await Assert.That(ReadObjRefIpid(objRef)).IsNotEqualTo(response.IpidRemUnknown);
    }

    [Test]
    public async Task RemoteActivation_unknown_clsid_returns_class_not_registered_in_body()
    {
        var legacy = new LegacyActivationServer(new RemoteSCMActivatorServer(new InMemoryClsidRegistry()));
        var request = new LegacyRemoteActivationRequest(
            Guid.NewGuid(),
            new[] { IidIUnknown },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 7 });

        DispatchResult result = await ActivationServer.DispatchRemoteActivationAsync(
            legacy,
            IActivationCodec.EncodeRemoteActivationRequest(request),
            OpcProtectionLevel.Integrity);
        LegacyRemoteActivationResponse response = IActivationCodec.DecodeRemoteActivationResponse(result.Payload.Span, 1);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(response.Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
        await Assert.That(response.InterfaceResults[0].Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
    }

    [Test]
    public async Task RemoteActivation_malformed_body_returns_fault_and_logs_error()
    {
        var logger = new RecordingLogger();
        LegacyActivationServer legacy = CreateLegacyServer(TestClsid, IidIUnknown);

        DispatchResult result = await ActivationServer.DispatchRemoteActivationAsync(
            legacy,
            new byte[] { 0x01, 0x02, 0x03 },
            OpcProtectionLevel.Integrity,
            logger);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Hresult).IsEqualTo(E_INVALIDARG);
        await Assert.That(logger.Contains("malformed")).IsTrue();
    }

    [Test]
    public async Task RemoteActivation_anonymous_dispatch_is_rejected_before_activation()
    {
        var server = new ThrowingActivationServer();
        byte[] requestPayload = IActivationCodec.EncodeRemoteActivationRequest(new LegacyRemoteActivationRequest(
            TestClsid,
            new[] { IidIUnknown },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 7 }));

        DispatchResult result = await ActivationServer.DispatchRemoteActivationAsync(
            server,
            requestPayload,
            isAuthenticated: false,
            OpcProtectionLevel.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Hresult).IsEqualTo(E_ACCESSDENIED);
        await Assert.That(server.WasCalled).IsFalse();
    }

    [Test]
    public async Task ActivationClient_and_ActivationServer_agree_on_wire_payload_layout()
    {
        LegacyRemoteActivationRequest? received = null;
        var server = new CapturingActivationServer(request =>
        {
            received = request;
            return new LegacyRemoteActivationResponse(
                0,
                new Guid("08070605-0403-0201-0000-000000000000"),
                new Guid("22222222-3333-4444-5555-666666666666"),
                5,
                (5, 1),
                new[] { new RemoteActivationInterfaceResult(0, new byte[] { 0x4d, 0x45, 0x4f, 0x57 }) })
            {
                OxidBindings = new byte[] { 0x02, 0x00, 0x01, 0x00, 0x07, 0x00, 0x00, 0x00 },
            };
        });
        var channel = new InMemoryCallChannelBuilder()
            .Register(ActivationServer.InterfaceId, 0, async (_, _, payload, cancellationToken) =>
            {
                DispatchResult dispatch = await ActivationServer.DispatchRemoteActivationAsync(
                    server,
                    payload,
                    OpcProtectionLevel.Integrity,
                    cancellationToken: cancellationToken);
                return dispatch.ToNdrCallResult();
            })
            .Build();
        var client = new ActivationClient(channel);

        LegacyRemoteActivationResponse response = await client.RemoteActivationAsync(
            TestClsid,
            new[] { "ncacn_ip_tcp" },
            "legacy.moniker",
            new[] { IidIUnknown });

        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Clsid).IsEqualTo(TestClsid);
        await Assert.That(received.ObjectName).IsEqualTo("legacy.moniker");
        await Assert.That(received.RequestedProtocolSequences[0]).IsEqualTo((ushort)7);
        await Assert.That(channel.CallLog[0].InterfaceId).IsEqualTo(ActivationServer.InterfaceId);
        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(0);
        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.AuthnHint).IsEqualTo(5u);
    }

    private static LegacyActivationServer CreateLegacyServer(Guid clsid, Guid iid)
    {
        var factories = new ClassFactoryRegistry();
        factories.Register(
            clsid,
            _ => new ClassFactoryActivationResult(
                new TestComObject(),
                new LocalInterfaceDefinition(iid.ToString("D"), isDispInterface: false)));
        return new LegacyActivationServer(new RemoteSCMActivatorServer(factories));
    }

    private static async Task RunProcessorAndShutdown(
        RpcServerConnectionProcessor processor,
        InMemoryAsyncTransport transport)
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
        InMemoryAsyncTransport transport,
        ConnectionOrientedPdu pdu)
    {
        const int authBodyLength = 16;
        byte[] frame = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        int padding = PaddingTo(frame.Length, 4);
        int verifierStart = frame.Length + padding;
        int totalLength = verifierStart + 8 + authBodyLength;
        byte[] forged = new byte[totalLength];
        frame.AsSpan().CopyTo(forged);
        forged[verifierStart] = 0;
        forged[verifierStart + 1] = (byte)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY;
        forged[verifierStart + 2] = (byte)padding;
        BinaryPrimitives.WriteUInt16LittleEndian(forged.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)totalLength);
        BinaryPrimitives.WriteUInt16LittleEndian(forged.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), authBodyLength);
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
            ContextList = [new PresentationContext(contextId, new PresentationSyntax(new UUID(interfaceId.ToString("D")), 0, 0))],
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

    private static int PaddingTo(int length, int alignment)
    {
        int remainder = length % alignment;
        return remainder == 0 ? 0 : alignment - remainder;
    }

    private static uint ReadObjRefSignature(ReadOnlySpan<byte> objRef) =>
        BinaryPrimitives.ReadUInt32LittleEndian(objRef[..4]);

    private static Guid ReadObjRefIid(ReadOnlySpan<byte> objRef) =>
        new(objRef.Slice(8, 16));

    private static Guid ReadObjRefIpid(ReadOnlySpan<byte> objRef) =>
        new(objRef.Slice(48, 16));

    private sealed class TestComObject
    {
    }

    private sealed class ThrowingActivationServer : IActivationServer
    {
        public bool WasCalled { get; private set; }

        public Task<int> RemoteActivationAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            throw new InvalidOperationException("Activation should have been rejected before this method was called.");
        }

        public Task<LegacyRemoteActivationResponse> RemoteActivationAsync(
            LegacyRemoteActivationRequest request,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            throw new InvalidOperationException("Activation should have been rejected before this method was called.");
        }
    }

    private sealed class CapturingActivationServer : IActivationServer
    {
        private readonly Func<LegacyRemoteActivationRequest, LegacyRemoteActivationResponse> _handler;

        public CapturingActivationServer(Func<LegacyRemoteActivationRequest, LegacyRemoteActivationResponse> handler) =>
            _handler = handler;

        public Task<int> RemoteActivationAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task<LegacyRemoteActivationResponse> RemoteActivationAsync(
            LegacyRemoteActivationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_handler(request));
    }

    private sealed class RecordingLogger : ILogger
    {
        private readonly List<string> _messages = new();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _messages.Add(formatter(state, exception));
        }

        public bool Contains(string value) =>
            _messages.Exists(message => message.Contains(value, StringComparison.OrdinalIgnoreCase));

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
