// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Testing;
using Opc.Classic.Transport;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Tests;

public sealed class DcomCallChannelTests
{
    private static readonly IReadOnlyList<Guid> PreBindIids = OpcSpecCatalog.Da;
    private static readonly Guid FirstInterfaceId = PreBindIids[0];
    private static readonly Guid SecondInterfaceId = PreBindIids[1];
    private static readonly Guid RejectedOptionalInterfaceId = IOPCAsyncIO3.InterfaceId;

    [Test]
    public async Task InvokeAsync_via_InMemoryAsyncTransport_round_trips()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] responsePayload = [0x21, 0x22, 0x23, 0x24];
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes(responsePayload));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 3, new byte[] { 0x10, 0x11 });

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray()).IsEquivalentTo(responsePayload);
    }

    [Test]
    public async Task InvokeAsync_FaultPdu_returns_hresult()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateFaultBytes(ReadEFail()));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 7, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(ReadEFail());
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
    }

    [Test]
    public async Task InvokeAsync_normal_response_decodes_S_FALSE_and_removes_hresult()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] responsePayload = [0x31, 0x32];
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(
            CreateResponseBytes(responsePayload, OpcResultId.False.Code));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(
            Guid.NewGuid(),
            3,
            ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.IsFault).IsFalse();
        await Assert.That(result.ResponsePayload.ToArray())
            .IsEquivalentTo(responsePayload);
    }

    [Test]
    public async Task Bind_authentication_token_uses_body_relative_16_byte_alignment()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            new TokenAuthContext());

        _ = await channel.InvokeAsync(
            Guid.NewGuid(),
            3,
            ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<byte[]> frames =
            await ReadOutboundFramesAsync(transport);
        byte[] bind = frames[0];
        AssertAuthenticationAlignment(
            bind,
            expectedAuthLength: 1,
            requirePadding: true);
    }

    [Test]
    public async Task Alter_context_empty_verifier_uses_body_relative_16_byte_alignment()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        await transport.WriteInboundAsync(CreateAlterAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            new RecordingIntegrityAuthContext());

        _ = await channel.InvokeAsync(
            Guid.NewGuid(),
            3,
            ReadOnlyMemory<byte>.Empty);
        _ = await channel.InvokeAsync(
            Guid.NewGuid(),
            4,
            ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<byte[]> frames =
            await ReadOutboundFramesAsync(transport);
        byte[] alter = frames.Single(
            static frame =>
                frame[ConnectionOrientedPdu.TYPE_OFFSET]
                    == AlterContextPdu.ALTER_CONTEXT_TYPE);
        AssertAuthenticationAlignment(
            alter,
            expectedAuthLength: 0,
            requirePadding: true);
    }

    [Test]
    public async Task InvokeAsync_normal_response_preserves_negative_application_hresult()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(
            CreateResponseBytes([], ReadEFail()));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(
            Guid.NewGuid(),
            3,
            ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(ReadEFail());
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.IsFault).IsFalse();
    }

    [Test]
    public async Task InvokeAsync_rejects_normal_response_without_trailing_hresult()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseFragmentBytes(
            new byte[8],
            ConnectionOrientedPdu.PFC_FIRST_FRAG
                | ConnectionOrientedPdu.PFC_LAST_FRAG));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance);

        Exception? thrown = null;
        try
        {
            _ = await channel.InvokeAsync(
                Guid.NewGuid(),
                3,
                ReadOnlyMemory<byte>.Empty);
        }
        catch (Exception exception)
        {
            thrown = exception;
        }

        await Assert.That(thrown).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task InvokeRawAsync_preserves_plain_NDR_response_without_hresult_decoding()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] rawPayload = [0x41, 0x42, 0x43];
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateRawResponseBytes(rawPayload));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeRawAsync(
            Guid.NewGuid(),
            3,
            ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray())
            .IsEquivalentTo(rawPayload);
    }

    // Regression guard for the NTLM RPC signing fix: at Integrity the channel must sign the
    // ENTIRE PDU except the trailing auth_value (common header + body + auth pad + sec_trailer
    // header), per MS-RPCE §3.3.1.5.2.2 — NOT just the post-header body. Real Windows RPCSS
    // rejects a body-only signature with RPC_S_SEC_PKG_ERROR (0x721).
    [Test]
    public async Task InvokeAsync_at_Integrity_signs_full_pdu_including_header_and_sec_trailer()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([0x55]));
        var authContext = new RecordingIntegrityAuthContext();
        var channel = new DcomCallChannel(transport, authContext);

        _ = await channel.InvokeAsync(Guid.NewGuid(), 3, new byte[] { 0x10, 0x11, 0x12 });

        byte[] region = authContext.CapturedRegion
            ?? throw new InvalidOperationException("SignAndSeal was not invoked.");
        // Common header is included: byte 0 is the DCE/RPC major version (5) and TYPE_OFFSET
        // carries the REQUEST ptype.
        await Assert.That(region[0]).IsEqualTo((byte)5);
        await Assert.That(region[ConnectionOrientedPdu.TYPE_OFFSET]).IsEqualTo((byte)RequestCoPdu.REQUEST_TYPE);
        // The confidential (sealed) sub-range starts after the common header.
        await Assert.That(authContext.CapturedConfidentialOffset).IsEqualTo(ConnectionOrientedPdu.HEADER_LENGTH);
        // The signed region ends with the 8-byte sec_trailer header (auth_type 0x0A = NTLM).
        await Assert.That(region[^8]).IsEqualTo((byte)0x0A);
        // region == header + confidential body + 8-byte sec_trailer header (auth_value excluded).
        await Assert.That(region.Length)
            .IsEqualTo(authContext.CapturedConfidentialOffset + authContext.CapturedConfidentialLength + 8);
        int verifierStart = region.Length - 8;
        int authPadding = region[verifierStart + 2];
        int unpaddedPduLength = verifierStart - authPadding;
        int unpaddedBodyLength =
            unpaddedPduLength - ConnectionOrientedPdu.HEADER_LENGTH;
        int expectedPadding = (16 - (unpaddedBodyLength % 16)) % 16;
        await Assert.That(
            (verifierStart - ConnectionOrientedPdu.HEADER_LENGTH) % 16)
            .IsEqualTo(0);
        await Assert.That(authPadding).IsEqualTo(expectedPadding);
        await Assert.That(authPadding).IsGreaterThan(0);
    }

    // The channel must surface the DCE/RPC bind_nak reject_reason (MS-RPCE §2.2.2.10) as a
    // typed BindException so callers can adapt (e.g. retry with fewer presentation contexts
    // when a server answers LOCAL_LIMIT_EXCEEDED), rather than a generic "received type 13".
    [Test]
    public async Task InvokeAsync_BindNak_throws_BindException_carrying_reject_reason()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindNakBytes(BindNoAcknowledgeReason.LOCAL_LIMIT_EXCEEDED));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        BindException? caught = null;
        try
        {
            _ = await channel.InvokeAsync(Guid.NewGuid(), 3, ReadOnlyMemory<byte>.Empty);
        }
        catch (BindException ex)
        {
            caught = ex;
        }

        await Assert.That(caught).IsNotNull();
        await Assert.That(caught!.RejectReason).IsEqualTo(BindNoAcknowledgeReason.LOCAL_LIMIT_EXCEEDED);
        await Assert.That(caught.Message).Contains("LOCAL_LIMIT_EXCEEDED");
    }

    [Test]
    public async Task InvokeAsync_fragmented_response_assembles_correctly()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] responsePayload = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07];
        byte[] responseStub = CreateResponseStub(responsePayload);
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseFragmentBytes(
            responseStub[0..5],
            ConnectionOrientedPdu.PFC_FIRST_FRAG));
        await transport.WriteInboundAsync(CreateResponseFragmentBytes(
            responseStub[5..],
            ConnectionOrientedPdu.PFC_LAST_FRAG));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 9, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray()).IsEquivalentTo(responsePayload);
    }

    [Test]
    public async Task InvokeAsync_predeclares_Da_contexts_in_initial_bind_order()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes(PreBindIids.Count));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        _ = await channel.InvokeAsync(FirstInterfaceId, 6, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<ConnectionOrientedPdu> outbound = await ReadOutboundPdusAsync(transport);
        var bind = (BindPdu)outbound[0];
        await Assert.That(bind.ContextList.Length).IsEqualTo(PreBindIids.Count);
        for (int i = 0; i < PreBindIids.Count; i++)
        {
            await Assert.That(bind.ContextList[i].ContextId).IsEqualTo(i);
            Guid actualInterfaceId = Guid.Parse(bind.ContextList[i].AbstractSyntax.Uuid.ToString());
            await Assert.That(actualInterfaceId).IsEqualTo(PreBindIids[i]);
        }
    }

    [Test]
    public async Task InvokeAsync_keeps_first_call_iid_at_context_zero()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes(PreBindIids.Count));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        _ = await channel.InvokeAsync(SecondInterfaceId, 3, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<ConnectionOrientedPdu> outbound = await ReadOutboundPdusAsync(transport);
        var bind = (BindPdu)outbound[0];
        await Assert.That(bind.ContextList[0].ContextId).IsEqualTo(0);
        Guid actualInterfaceId = Guid.Parse(bind.ContextList[0].AbstractSyntax.Uuid.ToString());
        await Assert.That(actualInterfaceId).IsEqualTo(SecondInterfaceId);
    }

    [Test]
    public async Task InvokeAsync_allows_optional_predeclared_context_rejection()
    {
        await using var transport = new InMemoryAsyncTransport();
        int rejectedIndex = IndexOf(PreBindIids, RejectedOptionalInterfaceId);
        await transport.WriteInboundAsync(CreateBindAckBytes(
            PreBindIids.Count,
            rejectedIndex));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        NdrCallResult result = await channel.InvokeAsync(FirstInterfaceId, 6, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
    }

    [Test]
    public async Task InvokeAsync_reuses_predeclared_context_without_alter_context()
    {
        await using var transport = new InMemoryAsyncTransport();
        Guid routedIpid = new("44444444-4444-4444-4444-444444444444");
        await transport.WriteInboundAsync(CreateBindAckBytes(PreBindIids.Count));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        _ = await channel.InvokeAsync(FirstInterfaceId, 6, ReadOnlyMemory<byte>.Empty);
        channel.RegisterInterfaceIpid(SecondInterfaceId, routedIpid);
        _ = await channel.InvokeAsync(SecondInterfaceId, 3, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<ConnectionOrientedPdu> outbound = await ReadOutboundPdusAsync(transport);
        await Assert.That(ContainsPdu<AlterContextPdu>(outbound)).IsFalse();
        var secondRequest = (RequestCoPdu)outbound[2];
        await Assert.That(secondRequest.ContextId).IsEqualTo(IndexOf(PreBindIids, SecondInterfaceId));
        await Assert.That(Guid.Parse(secondRequest.Object!.ToString())).IsEqualTo(routedIpid);
    }

    [Test]
    public async Task InvokeAsync_cancellation_token_propagates()
    {
        await using var transport = new InMemoryAsyncTransport();
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var canceled = false;
        try
        {
            _ = await channel.InvokeAsync(Guid.NewGuid(), 1, ReadOnlyMemory<byte>.Empty, cts.Token);
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        await Assert.That(canceled).IsTrue();
    }

    [Test]
    public async Task DcomCallChannelFactory_connects_then_disposes_transport()
    {
        var transportFactory = new RecordingTransportFactory();
        var channelFactory = new DcomCallChannelFactory(transportFactory);
        var endpoint = new IPEndPoint(IPAddress.Loopback, 135);

        ICallChannel channel = await channelFactory.ConnectAsync(endpoint, Guid.Empty, NoOpAuthContext.Instance);

        await Assert.That(transportFactory.Endpoint).IsEqualTo(endpoint);
        await ((IAsyncDisposable)channel).DisposeAsync();
        await Assert.That(transportFactory.Transport.IsDisposed).IsTrue();
    }

    private static byte[] CreateBindAckBytes() => CreateBindAckBytes(resultCount: 1);

    private static byte[] CreateAlterAckBytes()
    {
        var alterAck = new AlterContextResponsePdu
        {
            AssociationGroupId = 1,
            CallId = 3,
            MaxReceiveFragment =
                ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxTransmitFragment =
                ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ResultList = [new PresentationResult()],
            SecondaryAddress = new Port(),
        };
        return EncodePdu(alterAck);
    }

    private static byte[] CreateBindAckBytes(int resultCount, int rejectedIndex = -1)
    {
        var bindAck = new BindAcknowledgePdu
        {
            AssociationGroupId = 1,
            CallId = 1,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ResultList = CreatePresentationResults(resultCount, rejectedIndex),
            SecondaryAddress = new Port(),
        };

        return EncodePdu(bindAck);
    }

    private static PresentationResult[] CreatePresentationResults(int count, int rejectedIndex)
    {
        var results = new PresentationResult[count];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = i == rejectedIndex
                ? new PresentationResult(
                    PresentationResultCode.PROVIDER_REJECTION,
                    PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED,
                    new PresentationSyntax(NdrCodec.NDR_SYNTAX))
                : new PresentationResult();
        }

        return results;
    }

    private static byte[] CreateResponseBytes(
        byte[] responsePayload,
        int hresult = 0) =>
        CreateResponseFragmentBytes(
            CreateResponseStub(responsePayload, hresult),
            ConnectionOrientedPdu.PFC_FIRST_FRAG | ConnectionOrientedPdu.PFC_LAST_FRAG);

    private static byte[] CreateRawResponseBytes(byte[] responsePayload) =>
        CreateResponseFragmentBytes(
            responsePayload,
            ConnectionOrientedPdu.PFC_FIRST_FRAG
                | ConnectionOrientedPdu.PFC_LAST_FRAG);

    private static byte[] CreateResponseFragmentBytes(byte[] responseStub, int flags)
    {
        var response = new ResponseCoPdu
        {
            AllocationHint = responseStub.Length,
            CallId = 2,
            ContextId = 0,
            Flags = flags,
            Stub = responseStub,
        };

        return EncodePdu(response);
    }

    private static byte[] CreateFaultBytes(int hresult)
    {
        var fault = new FaultCoPdu
        {
            AllocationHint = 0,
            CallId = 2,
            ContextId = 0,
            Status = (FaultCode)hresult,
        };

        return EncodePdu(fault);
    }

    private static byte[] CreateBindNakBytes(BindNoAcknowledgeReason reason)
    {
        var nak = new BindNoAcknowledgePdu
        {
            CallId = 1,
            RejectReason = reason,
        };

        return EncodePdu(nak);
    }

    private static byte[] CreateResponseStub(
        byte[] responsePayload,
        int hresult = 0)
    {
        byte[] stub = new byte[8 + responsePayload.Length + sizeof(int)];
        responsePayload.CopyTo(stub.AsSpan(8));
        BinaryPrimitives.WriteInt32LittleEndian(
            stub.AsSpan(8 + responsePayload.Length),
            hresult);
        return stub;
    }

    private static async Task<IReadOnlyList<ConnectionOrientedPdu>> ReadOutboundPdusAsync(InMemoryAsyncTransport transport)
    {
        IReadOnlyList<byte[]> frames =
            await ReadOutboundFramesAsync(transport);
        return frames.Select(PduCodec.DecodePdu).ToArray();
    }

    private static async Task<IReadOnlyList<byte[]>> ReadOutboundFramesAsync(
        InMemoryAsyncTransport transport)
    {
        ReadResult result = await transport.ReadOutbound.ReadAsync();
        byte[] outbound = result.Buffer.ToArray();
        transport.ReadOutbound.AdvanceTo(result.Buffer.End);

        var frames = new List<byte[]>();
        int offset = 0;
        while (offset < outbound.Length)
        {
            int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
                outbound.AsSpan(offset + ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, sizeof(ushort)));
            byte[] frame = outbound.AsSpan(offset, fragmentLength).ToArray();
            frames.Add(frame);
            offset += fragmentLength;
        }

        return frames;
    }

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

    private static bool ContainsPdu<T>(IReadOnlyList<ConnectionOrientedPdu> pdus)
        where T : ConnectionOrientedPdu
    {
        for (int i = 0; i < pdus.Count; i++)
        {
            if (pdus[i] is T)
            {
                return true;
            }
        }

        return false;
    }

    private static int IndexOf(IReadOnlyList<Guid> values, Guid value)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == value)
            {
                return i;
            }
        }

        return -1;
    }

    private static byte[] EncodePdu(ConnectionOrientedPdu pdu)
    {
        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(new byte[ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE], 0);
        pdu.Encode(ndr, buffer);
        return buffer.Buf.AsSpan(0, buffer.Length).ToArray();
    }

    // TUnitAssertions0005 workaround: use a method call for the E_FAIL constant.
    private static int ReadEFail() => unchecked((int)0x80004005u);

    private sealed class RecordingTransportFactory : IAsyncTransportFactory
    {
        public RecordingTransport Transport { get; } = new();
        public EndPoint? Endpoint { get; private set; }

        public ValueTask<IAsyncTransport> ConnectAsync(
            EndPoint endpoint,
            CancellationToken cancellationToken = default)
        {
            Endpoint = endpoint;
            return ValueTask.FromResult<IAsyncTransport>(Transport);
        }
    }

    private sealed class RecordingTransport : IAsyncTransport
    {
        private readonly InMemoryAsyncTransport _inner = new();

        public bool IsDisposed { get; private set; }
        public EndPoint RemoteEndpoint => _inner.RemoteEndpoint;
        public System.IO.Pipelines.PipeReader Input => _inner.Input;
        public System.IO.Pipelines.PipeWriter Output => _inner.Output;

        public ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            _inner.FlushAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            await _inner.DisposeAsync().ConfigureAwait(false);
        }
    }

    private sealed class RecordingIntegrityAuthContext : IAuthContext
    {
        public byte[]? CapturedRegion { get; private set; }

        public int CapturedConfidentialOffset { get; private set; } = -1;

        public int CapturedConfidentialLength { get; private set; } = -1;

        public OpcProtectionLevel ProtectionLevel => OpcProtectionLevel.Integrity;

        public byte AuthenticationServiceCode => 0x0A;

        public byte[] BuildInitialToken() => [];

        public byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken) => [];

        public void SignAndSeal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, out byte[] signature)
        {
            CapturedRegion = signedRegion.ToArray();
            CapturedConfidentialOffset = confidentialOffset;
            CapturedConfidentialLength = confidentialLength;
            signature = new byte[16];
        }

        public bool VerifyAndUnseal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, ReadOnlyMemory<byte> signature) => true;
    }

    private sealed class TokenAuthContext : IAuthContext
    {
        public OpcProtectionLevel ProtectionLevel =>
            OpcProtectionLevel.Connect;

        public byte AuthenticationServiceCode => 42;

        public byte[] BuildInitialToken() => [0x01];

        public byte[] ProcessChallengeToken(
            ReadOnlyMemory<byte> serverToken)
        {
            _ = serverToken;
            return [];
        }

        public void SignAndSeal(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            out byte[] signature) =>
            signature = [];

        public bool VerifyAndUnseal(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            ReadOnlyMemory<byte> signature) => signature.IsEmpty;
    }
}
