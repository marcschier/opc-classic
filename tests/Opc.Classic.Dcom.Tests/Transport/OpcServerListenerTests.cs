// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth;
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
        await Assert.That(body[..^sizeof(int)].ToArray())
            .IsEquivalentTo(new byte[] { 0x10, 0x20, 0x30 });
        await Assert.That(
            BinaryPrimitives.ReadInt32LittleEndian(body.Span[^sizeof(int)..]))
            .IsEqualTo(0);
        await Assert.That(dispatcher.LastOpnum).IsEqualTo(7);
    }

    [Test]
    public async Task Enumerator_end_of_sequence_preserves_S_FALSE_over_real_TCP()
    {
        byte[] enumeratedValue = [0xA1, 0xA2];
        var dispatcher = new RecordingDispatcher(
            enumeratedValue,
            OpcResultId.False.Code);
        var endpoint = new TcpServerEndpoint(
            new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = dispatcher,
            });
        await using var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);

        var client = new TcpClient();
        IPEndPoint bound = (IPEndPoint)listener.LocalEndpoint;
        await client.ConnectAsync(
            bound.Address,
            bound.Port,
            TestContext.Current!.CancellationToken);
        await using var channel = new DcomCallChannel(
            new TcpClientTransport(client),
            NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(
            InterfaceId,
            opnum: 3,
            ReadOnlyMemory<byte>.Empty,
            TestContext.Current!.CancellationToken);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.ResponsePayload.ToArray())
            .IsEquivalentTo(enumeratedValue);
    }

    [Test]
    public async Task Integrity_round_trip_uses_body_relative_16_byte_sec_trailer_alignment()
    {
        const byte authenticationService = 42;
        byte[] key = Convert.FromHexString(
            "00112233445566778899AABBCCDDEEFF");
        var serverProtection = new HmacServerProtectionContext(
            authenticationService,
            key);
        var provider = new SingleStepAuthenticationProvider(
            authenticationService,
            serverProtection);
        var options = new RpcServerAuthenticationOptions(
            new RpcServerAuthenticationProviderRegistry([provider]));
        var dispatcher = new RecordingDispatcher(
            [0x61, 0x62, 0x63, 0x64, 0x65]);
        Guid secondInterfaceId = Guid.NewGuid();
        var endpoint = new TcpServerEndpoint(
            new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            options,
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [InterfaceId] = dispatcher,
                [secondInterfaceId] = dispatcher,
            });
        await using var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);

        var client = new TcpClient();
        IPEndPoint bound = (IPEndPoint)listener.LocalEndpoint;
        await client.ConnectAsync(
            bound.Address,
            bound.Port,
            TestContext.Current!.CancellationToken);
        var clientAuth = new HmacClientAuthContext(
            authenticationService,
            key);
        await using var channel = new DcomCallChannel(
            new TcpClientTransport(client),
            clientAuth);

        NdrCallResult result = await channel.InvokeAsync(
            InterfaceId,
            7,
            new byte[] { 0x10, 0x20, 0x30 },
            TestContext.Current!.CancellationToken);
        NdrCallResult alteredResult = await channel.InvokeAsync(
            secondInterfaceId,
            8,
            new byte[] { 0x40, 0x50 },
            TestContext.Current!.CancellationToken);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(alteredResult.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray())
            .IsEquivalentTo(new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 });
        await Assert.That(clientAuth.Outbound.Count).IsEqualTo(2);
        await Assert.That(clientAuth.Inbound.Count).IsEqualTo(2);
        await Assert.That(serverProtection.Inbound.Count).IsEqualTo(2);
        await Assert.That(serverProtection.Outbound.Count).IsEqualTo(3);

        foreach (AlignmentRecord record in clientAuth.Outbound
            .Concat(clientAuth.Inbound)
            .Concat(serverProtection.Inbound)
            .Concat(serverProtection.Outbound))
        {
            await Assert.That(
                (record.VerifierStart - ConnectionOrientedPdu.HEADER_LENGTH)
                    % 16)
                .IsEqualTo(0);
            await Assert.That(record.Padding)
                .IsEqualTo(record.ExpectedPadding);
            await Assert.That(record.Padding).IsGreaterThan(0);
        }
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
        private readonly int _hresult;

        public RecordingDispatcher(byte[] payload, int hresult = 0)
        {
            _payload = payload;
            _hresult = hresult;
        }

        public int LastOpnum { get; private set; } = -1;

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
        {
            LastOpnum = opnum;
            return ValueTask.FromResult(
                DispatchResult.Success(_payload, _hresult));
        }
    }

    private sealed class SingleStepAuthenticationProvider :
        IRpcServerAuthenticationProvider
    {
        private readonly IRpcServerProtectionContext _protectionContext;

        public SingleStepAuthenticationProvider(
            int authenticationService,
            IRpcServerProtectionContext protectionContext)
        {
            AuthenticationService = authenticationService;
            _protectionContext = protectionContext;
        }

        public int AuthenticationService { get; }

        public IRpcServerAuthenticationAcceptor CreateAcceptor() =>
            new Acceptor(AuthenticationService, _protectionContext);

        private sealed class Acceptor : IRpcServerAuthenticationAcceptor
        {
            private readonly int _authenticationService;
            private readonly IRpcServerProtectionContext _protectionContext;

            public Acceptor(
                int authenticationService,
                IRpcServerProtectionContext protectionContext)
            {
                _authenticationService = authenticationService;
                _protectionContext = protectionContext;
            }

            public RpcServerAuthenticationTokenResult AcceptToken(
                ReadOnlyMemory<byte> token,
                OpcProtectionLevel protectionLevel)
            {
                if (!token.Span.SequenceEqual(new byte[] { 0x01 }))
                {
                    throw new InvalidOperationException(
                        "Unexpected authentication token.");
                }

                var principal = new GenericPrincipal(
                    new GenericIdentity("alignment-user", "TEST"),
                    []);
                return RpcServerAuthenticationTokenResult.Complete(
                    new RpcServerAuthenticationSession(
                        _authenticationService,
                        principal,
                        protectionLevel,
                        _protectionContext));
            }
        }
    }

    private sealed class HmacClientAuthContext : IAuthContext
    {
        private readonly byte[] _key;

        public HmacClientAuthContext(
            byte authenticationService,
            byte[] key)
        {
            AuthenticationServiceCode = authenticationService;
            _key = key;
        }

        public List<AlignmentRecord> Outbound { get; } = [];

        public List<AlignmentRecord> Inbound { get; } = [];

        public OpcProtectionLevel ProtectionLevel =>
            OpcProtectionLevel.Integrity;

        public byte AuthenticationServiceCode { get; }

        public byte[] BuildInitialToken() => [0x01];

        public byte[] ProcessChallengeToken(
            ReadOnlyMemory<byte> serverToken) => [];

        public int GetVerifierLength(
            int signedRegionLength,
            int confidentialLength) => 16;

        public void SignAndSeal(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            out byte[] signature)
        {
            _ = confidentialOffset;
            _ = confidentialLength;
            Outbound.Add(ReadAlignment(signedRegion));
            signature = ComputeVerifier(_key, signedRegion);
        }

        public bool VerifyAndUnseal(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            ReadOnlyMemory<byte> signature)
        {
            _ = confidentialOffset;
            _ = confidentialLength;
            Inbound.Add(ReadAlignment(signedRegion));
            return CryptographicOperations.FixedTimeEquals(
                signature.Span,
                ComputeVerifier(_key, signedRegion));
        }
    }

    private sealed class HmacServerProtectionContext :
        IRpcServerProtectionContext
    {
        private readonly byte[] _key;

        public HmacServerProtectionContext(
            int authenticationService,
            byte[] key)
        {
            AuthenticationService = authenticationService;
            _key = key;
        }

        public List<AlignmentRecord> Outbound { get; } = [];

        public List<AlignmentRecord> Inbound { get; } = [];

        public int AuthenticationService { get; }

        public OpcProtectionLevel ProtectionLevel =>
            OpcProtectionLevel.Integrity;

        public int VerifierLength => 16;

        public void Protect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            out byte[] verifier)
        {
            _ = confidentialOffset;
            _ = confidentialLength;
            Outbound.Add(ReadAlignment(signedRegion));
            verifier = ComputeVerifier(_key, signedRegion);
        }

        public bool Unprotect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            ReadOnlyMemory<byte> verifier)
        {
            _ = confidentialOffset;
            _ = confidentialLength;
            Inbound.Add(ReadAlignment(signedRegion));
            return CryptographicOperations.FixedTimeEquals(
                verifier.Span,
                ComputeVerifier(_key, signedRegion));
        }
    }

    private static AlignmentRecord ReadAlignment(
        ReadOnlySpan<byte> signedRegion)
    {
        int verifierStart = signedRegion.Length - 8;
        int padding = signedRegion[verifierStart + 2];
        int unpaddedPduLength = verifierStart - padding;
        int bodyLength =
            unpaddedPduLength - ConnectionOrientedPdu.HEADER_LENGTH;
        int expectedPadding = (16 - (bodyLength % 16)) % 16;
        return new AlignmentRecord(
            verifierStart,
            padding,
            expectedPadding);
    }

    private static byte[] ComputeVerifier(
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> signedRegion) =>
        HMACSHA256.HashData(key, signedRegion)[..16];

    private readonly record struct AlignmentRecord(
        int VerifierStart,
        int Padding,
        int ExpectedPadding);
}
