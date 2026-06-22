// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class OpcDcomDecoderTests
{
    private const int EthernetLinkType = 1;
    private static readonly Guid s_interfaceId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid s_objectIpid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Test]
    public async Task Decode_NullArguments_Throw()
    {
        var decoder = new OpcDcomDecoder();

        await Assert.That(() => decoder.Decode(null!).ToArray()).Throws<ArgumentNullException>();
        await Assert.That(() => decoder.DecodeAll(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Decode_HexSourceRequestAndResponse_MapsAnnotationsToOrpcBodyPdus()
    {
        var decoder = new OpcDcomDecoder();
        var timestamp = new DateTimeOffset(2026, 6, 7, 11, 0, 0, TimeSpan.Zero);
        var request = new CapturedPacket(
            timestamp,
            3,
            new byte[] { 0xAA, 0xBB, 0xCC },
            LinkType: 0,
            Annotations: new Dictionary<string, string?>
            {
                ["iid"] = s_interfaceId.ToString("D"),
                ["opnum"] = "9",
                ["direction"] = "request",
                ["hresult"] = "0x80004005",
            });
        var response = request with
        {
            Data = new byte[] { 0x01, 0x02 },
            OriginalLength = 2,
            Annotations = new Dictionary<string, string?>
            {
                ["iid"] = s_interfaceId.ToString("D"),
                ["opnum"] = "9",
                ["direction"] = "response",
                ["hresult"] = "0x80004005",
            },
        };

        IReadOnlyList<DecodedOpcPdu> decoded = decoder.DecodeAll([request, response]);

        await Assert.That(decoded.Count).IsEqualTo(2);
        await Assert.That(decoded[0].PduType).IsEqualTo("orpc_body");
        await Assert.That(decoded[0].Timestamp).IsEqualTo(timestamp);
        await Assert.That(decoded[0].CallId).IsEqualTo(-1);
        await Assert.That(decoded[0].InterfaceId).IsEqualTo(s_interfaceId);
        await Assert.That(decoded[0].Opnum).IsEqualTo(9);
        await Assert.That(decoded[0].Hresult).IsNull();
        await Assert.That(decoded[0].RequestStubLength).IsEqualTo(3);
        await Assert.That(decoded[0].ResponseStubLength).IsNull();
        await Assert.That(decoded[1].Hresult).IsEqualTo(unchecked((int)0x80004005));
        await Assert.That(decoded[1].RequestStubLength).IsNull();
        await Assert.That(decoded[1].ResponseStubLength).IsEqualTo(2);
        await Assert.That(decoded[1].Annotations!["direction"]).IsEqualTo("response");
    }

    [Test]
    public async Task Decode_MissingOrInvalidHexAnnotations_LeavesOptionalFieldsNull()
    {
        var decoder = new OpcDcomDecoder();
        var packet = new CapturedPacket(
            DateTimeOffset.UnixEpoch,
            1,
            new byte[] { 0x01 },
            LinkType: 0,
            Annotations: new Dictionary<string, string?>
            {
                ["iid"] = "not-a-guid",
                ["opnum"] = "not-an-int",
                ["direction"] = "response",
                ["hresult"] = "not-hex",
            });

        DecodedOpcPdu decoded = decoder.Decode(packet).Single();

        await Assert.That(decoded.InterfaceId).IsNull();
        await Assert.That(decoded.Opnum).IsNull();
        await Assert.That(decoded.Hresult).IsNull();
        await Assert.That(decoded.ResponseStubLength).IsEqualTo(1);
    }

    [Test]
    public async Task Decode_EmptyLinkLayerPacket_YieldsNoPdus()
    {
        var decoder = new OpcDcomDecoder();
        var empty = new CapturedPacket(DateTimeOffset.UnixEpoch, 0, ReadOnlyMemory<byte>.Empty, EthernetLinkType, new Dictionary<string, string?>());

        await Assert.That(decoder.Decode(empty).Count()).IsEqualTo(0);
    }

    [Test]
    public async Task Decode_EthernetTcpDceRpcFrames_ReassemblesAndProjectsConcretePduFields()
    {
        var decoder = new OpcDcomDecoder();
        var timestamp = new DateTimeOffset(2026, 6, 7, 12, 0, 0, TimeSpan.Zero);
        var decoded = new List<DecodedOpcPdu>();

        decoded.AddRange(decoder.Decode(NewTcpPacket(NewBindPdu(callId: 10), timestamp)));
        decoded.AddRange(decoder.Decode(NewTcpPacket(NewBindAckPdu(callId: 11), timestamp.AddMilliseconds(1))));
        byte[] requestFrame = Encode(NewRequestPdu(callId: 12));
        decoded.AddRange(decoder.Decode(NewTcpPacket(requestFrame.AsSpan(0, 8).ToArray(), timestamp.AddMilliseconds(2))));
        await Assert.That(decoded.Count).IsEqualTo(2);
        decoded.AddRange(decoder.Decode(NewTcpPacket(requestFrame.AsSpan(8).ToArray(), timestamp.AddMilliseconds(3))));
        decoded.AddRange(decoder.Decode(NewTcpPacket(NewResponsePdu(callId: 13), timestamp.AddMilliseconds(4))));
        decoded.AddRange(decoder.Decode(NewTcpPacket(NewFaultPdu(callId: 14), timestamp.AddMilliseconds(5))));

        await Assert.That(decoded.Count).IsEqualTo(5);
        await Assert.That(decoded[0].PduType).IsEqualTo("bind");
        await Assert.That(decoded[0].SourceEndpoint).IsEqualTo("10.0.0.1:50000");
        await Assert.That(decoded[0].DestinationEndpoint).IsEqualTo("10.0.0.2:135");
        await Assert.That(decoded[0].CallId).IsEqualTo(10);
        await Assert.That(decoded[0].ContextList.Count).IsEqualTo(1);
        await Assert.That(decoded[0].ContextList[0].ContextId).IsEqualTo(3);
        await Assert.That(decoded[0].ContextList[0].AbstractSyntaxIid).IsEqualTo(s_interfaceId);
        await Assert.That(decoded[0].ContextList[0].MajorVersion).IsEqualTo(1);
        await Assert.That(decoded[0].ContextList[0].MinorVersion).IsEqualTo(2);
        await Assert.That(decoded[1].PduType).IsEqualTo("bind_ack");
        await Assert.That(decoded[1].ResultList.Count).IsEqualTo(1);
        await Assert.That(decoded[1].ResultList[0]).IsEqualTo(new PresentationResultInfo("ACCEPTANCE", "REASON_NOT_SPECIFIED"));
        await Assert.That(decoded[2].PduType).IsEqualTo("request");
        await Assert.That(decoded[2].Timestamp).IsEqualTo(timestamp.AddMilliseconds(3));
        await Assert.That(decoded[2].CallId).IsEqualTo(12);
        await Assert.That(decoded[2].ContextId).IsEqualTo(3);
        await Assert.That(decoded[2].Opnum).IsEqualTo(7);
        await Assert.That(decoded[2].InterfaceId).IsEqualTo(s_interfaceId);
        await Assert.That(decoded[2].ObjectIpid).IsEqualTo(s_objectIpid);
        await Assert.That(decoded[2].RequestStubLength.HasValue).IsTrue();
        await Assert.That(decoded[2].RequestStubLength.GetValueOrDefault()).IsGreaterThan(0);
        await Assert.That(decoded[3].PduType).IsEqualTo("response");
        await Assert.That(decoded[3].CallId).IsEqualTo(13);
        await Assert.That(decoded[3].ContextId).IsEqualTo(3);
        await Assert.That(decoded[3].InterfaceId).IsEqualTo(s_interfaceId);
        await Assert.That(decoded[3].Hresult).IsEqualTo(0x12345678);
        await Assert.That(decoded[3].ResponseStubLength).IsEqualTo(8);
        await Assert.That(decoded[4].PduType).IsEqualTo("fault");
        await Assert.That(decoded[4].CallId).IsEqualTo(14);
        await Assert.That(decoded[4].FaultStatus).IsEqualTo((int)FaultCode.OPERATION_RANGE_ERROR);
    }

    [Test]
    public async Task Decode_SimpleShutdownAuthCancelAndOrphanedPdus_ProjectEndpointAndType()
    {
        var decoder = new OpcDcomDecoder();
        DecodedOpcPdu[] decoded =
        [
            .. decoder.Decode(NewTcpPacket(new ShutdownPdu { CallId = 21 }, DateTimeOffset.UnixEpoch)),
            .. decoder.Decode(NewTcpPacket(new Auth3Pdu { CallId = 22 }, DateTimeOffset.UnixEpoch)),
            .. decoder.Decode(NewTcpPacket(new CancelCoPdu { CallId = 23 }, DateTimeOffset.UnixEpoch)),
            .. decoder.Decode(NewTcpPacket(new OrphanedPdu { CallId = 24 }, DateTimeOffset.UnixEpoch)),
        ];

        await Assert.That(decoded.Length).IsEqualTo(4);
        await Assert.That(decoded[0].PduType).IsEqualTo("shutdown");
        await Assert.That(decoded[1].PduType).IsEqualTo("auth3");
        await Assert.That(decoded[2].PduType).IsEqualTo("cancel");
        await Assert.That(decoded[3].PduType).IsEqualTo("orphaned");
        await Assert.That(decoded[0].CallId).IsEqualTo(-1);
        await Assert.That(decoded[0].SourceEndpoint).IsEqualTo("10.0.0.1:50000");
    }

    [Test]
    public async Task Decode_AlterContextAndBindNakFrames_ProjectExpectedPduTypes()
    {
        var decoder = new OpcDcomDecoder();
        Guid alterIid = Guid.Parse("99999999-8888-7777-6666-555555555555");

        DecodedOpcPdu alter = decoder.Decode(NewTcpPacket(new AlterContextPdu
        {
            CallId = 31,
            AssociationGroupId = 9,
            ContextList = [NewPresentationContext(4, alterIid)],
        }, DateTimeOffset.UnixEpoch)).Single();
        DecodedOpcPdu alterResponse = decoder.Decode(NewTcpPacket(new AlterContextResponsePdu
        {
            CallId = 32,
            AssociationGroupId = 9,
            SecondaryAddress = new Port("135"),
            ResultList =
            [
                new PresentationResult(),
            ],
        }, DateTimeOffset.UnixEpoch.AddMilliseconds(1))).Single();
        DecodedOpcPdu bindNak = decoder.Decode(NewTcpPacket(new BindNoAcknowledgePdu
        {
            CallId = 33,
            RejectReason = BindNoAcknowledgeReason.LOCAL_LIMIT_EXCEEDED,
        }, DateTimeOffset.UnixEpoch.AddMilliseconds(2))).Single();

        await Assert.That(alter.PduType).IsEqualTo("alter_context");
        await Assert.That(alter.CallId).IsEqualTo(31);
        await Assert.That(alter.ContextList.Count).IsEqualTo(1);
        await Assert.That(alter.ContextList[0].ContextId).IsEqualTo(4);
        await Assert.That(alter.ContextList[0].AbstractSyntaxIid).IsEqualTo(alterIid);
        await Assert.That(alterResponse.PduType).IsEqualTo("alter_context_resp");
        await Assert.That(alterResponse.CallId).IsEqualTo(32);
        await Assert.That(alterResponse.ResultList[0]).IsEqualTo(
            new PresentationResultInfo("ACCEPTANCE", "REASON_NOT_SPECIFIED"));
        await Assert.That(bindNak.PduType).IsEqualTo("bind_nak");
        await Assert.That(bindNak.CallId).IsEqualTo(33);
    }

    private static BindPdu NewBindPdu(int callId)
        => new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList =
            [
                NewPresentationContext(3, s_interfaceId),
            ],
        };

    private static PresentationContext NewPresentationContext(int contextId, Guid interfaceId)
        => new(
            contextId,
            new PresentationSyntax(new UUID(interfaceId.ToString("D")), 1, 2));

    private static BindAcknowledgePdu NewBindAckPdu(int callId)
        => new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            SecondaryAddress = new Port("135"),
            ResultList = [new PresentationResult()],
        };

    private static RequestCoPdu NewRequestPdu(int callId)
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(new byte[] { 0x44, 0x55 }, Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"));
        return new RequestCoPdu
        {
            CallId = callId,
            ContextId = 3,
            Opnum = 7,
            AllocationHint = stub.Length,
            Stub = stub,
            Object = new UUID(s_objectIpid.ToString("D")),
        };
    }

    private static ResponseCoPdu NewResponsePdu(int callId)
    {
        byte[] stub = [0x11, 0x22, 0x33, 0x44, 0x78, 0x56, 0x34, 0x12];
        return new ResponseCoPdu
        {
            CallId = callId,
            ContextId = 3,
            AllocationHint = stub.Length,
            Stub = stub,
        };
    }

    private static FaultCoPdu NewFaultPdu(int callId)
        => new()
        {
            CallId = callId,
            ContextId = 3,
            AllocationHint = 0,
            Status = FaultCode.OPERATION_RANGE_ERROR,
            Stub = [],
        };

    private static CapturedPacket NewTcpPacket(ConnectionOrientedPdu pdu, DateTimeOffset timestamp)
        => NewTcpPacket(Encode(pdu), timestamp);

    private static byte[] Encode(ConnectionOrientedPdu pdu)
        => PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

    // ====================================================================
    // CA9-c step 3: in-decoder NTLM auth-trailer unwrap integration tests
    // ====================================================================

    // 16-byte deterministic session key shared across the unwrap tests.
    private static readonly byte[] s_testSessionKey = new byte[]
    {
        0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
        0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
    };
    private const int NtlmAuthValueLength = 16;
    private const int AuthVerifierHeaderLength = 8;
    private const byte AuthTypeNtlm = 0x0A;
    private const byte AuthLevelPrivacy = 0x06;
    private static readonly System.Net.IPAddress s_clientIp = System.Net.IPAddress.Parse("10.0.0.1");
    private static readonly System.Net.IPAddress s_serverIp = System.Net.IPAddress.Parse("10.0.0.2");
    private const int ClientPort = 50000;
    private const int ServerPort = 135;

    [Test]
    public async Task Decode_BindPdu_SetsClientToServerOnFlow_AndReverseFlowGetsServerToClient()
    {
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey);
        var decoder = new OpcDcomDecoder(unwrapper);
        var ts = new DateTimeOffset(2026, 6, 9, 14, 0, 0, TimeSpan.Zero);

        IEnumerable<DecodedOpcPdu> bindProjections = decoder.DecodeRawDcomFrame(
            Encode(NewBindPdu(callId: 100)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts);
        await Assert.That(bindProjections.Count()).IsEqualTo(1);

        // Feed a sealed Response on the reverse flow (server-side) and assert
        // that the unwrap identified the direction as ServerToClient via the
        // Bind we just processed (would have returned 'direction unknown'
        // otherwise).
        byte[] sealedResponse = BuildSealedFramePerCodebase(
            ptype: 0x02, callId: 101, contextId: 0, opnum: 0,
            plaintextStub: new byte[] { 0xAA, 0xBB, 0xCC, 0xDD },
            isServerSide: true);

        DecodedOpcPdu response = decoder.DecodeRawDcomFrame(
            sealedResponse,
            s_serverIp, ServerPort, s_clientIp, ClientPort, ts.AddMilliseconds(1)).Single();
        await Assert.That(response.PduType).IsEqualTo("response");
        await Assert.That(response.AuthUnwrapStatus).IsEqualTo("Decrypted");
        await Assert.That(response.AuthUnwrapReason).IsNull();
    }

    [Test]
    public async Task Decode_RequestPduWithSealedBody_DecryptsAndProjectsPlaintext()
    {
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey);
        var decoder = new OpcDcomDecoder(unwrapper);
        var ts = new DateTimeOffset(2026, 6, 9, 14, 0, 0, TimeSpan.Zero);

        _ = decoder.DecodeRawDcomFrame(
            Encode(NewBindPdu(callId: 200)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts).ToList();

        byte[] sealedRequest = BuildSealedFramePerCodebase(
            ptype: 0x00, callId: 201, contextId: 3, opnum: 7,
            plaintextStub: new byte[] { 0x11, 0x22, 0x33, 0x44 },
            isServerSide: false);

        DecodedOpcPdu request = decoder.DecodeRawDcomFrame(
            sealedRequest, s_clientIp, ClientPort, s_serverIp, ServerPort, ts.AddMilliseconds(1)).Single();

        await Assert.That(request.PduType).IsEqualTo("request");
        await Assert.That(request.CallId).IsEqualTo(201);
        await Assert.That(request.ContextId).IsEqualTo(3);
        await Assert.That(request.Opnum).IsEqualTo(7);
        await Assert.That(request.AuthUnwrapStatus).IsEqualTo("Decrypted");
        await Assert.That(request.AuthUnwrapReason).IsNull();
    }

    [Test]
    public async Task Decode_RequestPduNoAuthLength_DoesNotInvokeUnwrapper()
    {
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey);
        var decoder = new OpcDcomDecoder(unwrapper);
        var ts = new DateTimeOffset(2026, 6, 9, 14, 0, 0, TimeSpan.Zero);

        _ = decoder.DecodeRawDcomFrame(
            Encode(NewBindPdu(callId: 300)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts).ToList();

        DecodedOpcPdu plain = decoder.DecodeRawDcomFrame(
            Encode(NewRequestPdu(callId: 301)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts.AddMilliseconds(1)).Single();

        await Assert.That(plain.PduType).IsEqualTo("request");
        await Assert.That(plain.CallId).IsEqualTo(301);
        await Assert.That(plain.AuthUnwrapStatus).IsNull();
        await Assert.That(plain.AuthUnwrapReason).IsNull();
    }

    [Test]
    public async Task Decode_RequestPduBeforeBind_AnnotatesUnknownDirection()
    {
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey);
        var decoder = new OpcDcomDecoder(unwrapper);
        var ts = new DateTimeOffset(2026, 6, 9, 14, 0, 0, TimeSpan.Zero);

        byte[] sealedRequest = BuildSealedFramePerCodebase(
            ptype: 0x00, callId: 401, contextId: 3, opnum: 7,
            plaintextStub: new byte[] { 0x11, 0x22, 0x33, 0x44 },
            isServerSide: false);

        DecodedOpcPdu request = decoder.DecodeRawDcomFrame(
            sealedRequest, s_clientIp, ClientPort, s_serverIp, ServerPort, ts).Single();

        await Assert.That(request.AuthUnwrapStatus).IsEqualTo("SignatureMismatch");
        await Assert.That(request.AuthUnwrapReason).IsNotNull();
        await Assert.That(request.AuthUnwrapReason!).Contains("Direction unknown");
    }

    [Test]
    public async Task Decode_AuthSchemeNotNtlm_SkipsUnwrapSilently()
    {
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey);
        var decoder = new OpcDcomDecoder(unwrapper);
        var ts = new DateTimeOffset(2026, 6, 9, 14, 0, 0, TimeSpan.Zero);

        _ = decoder.DecodeRawDcomFrame(
            Encode(NewBindPdu(callId: 500)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts).ToList();

        byte[] plain = Encode(NewRequestPdu(callId: 501));
        byte[] withSpnegoTrailer = AppendBogusAuthTrailer(plain, authType: 0x09);

        DecodedOpcPdu request = decoder.DecodeRawDcomFrame(
            withSpnegoTrailer, s_clientIp, ClientPort, s_serverIp, ServerPort, ts.AddMilliseconds(1)).Single();

        await Assert.That(request.PduType).IsEqualTo("request");
        await Assert.That(request.AuthUnwrapStatus).IsNull();
        await Assert.That(request.AuthUnwrapReason).IsNull();
    }

    [Test]
    public async Task Decode_NoUnwrapperConfigured_PreservesExistingBehavior()
    {
        var decoder = new OpcDcomDecoder(); // no unwrapper
        var ts = new DateTimeOffset(2026, 6, 9, 14, 0, 0, TimeSpan.Zero);

        _ = decoder.DecodeRawDcomFrame(
            Encode(NewBindPdu(callId: 600)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts).ToList();

        // Plain Request (no auth trailer). The default decoder behaves
        // identically to today: no unwrap, no annotation.
        DecodedOpcPdu plainRequest = decoder.DecodeRawDcomFrame(
            Encode(NewRequestPdu(callId: 601)),
            s_clientIp, ClientPort, s_serverIp, ServerPort, ts.AddMilliseconds(1)).Single();

        await Assert.That(plainRequest.PduType).IsEqualTo("request");
        await Assert.That(plainRequest.AuthUnwrapStatus).IsNull();
        await Assert.That(plainRequest.AuthUnwrapReason).IsNull();
    }

    /// <summary>
    /// Builds a sign+seal-protected DCE/RPC frame matching this codebase's
    /// <c>DcomCallChannel.ApplyPacketProtectionCore</c> layout: starts from a
    /// plain Request/Response/Fault PDU bytes, pads to 4-byte alignment,
    /// appends the auth verifier header, then RC4-encrypts the signed region
    /// + writes the verifier into the trailing 16-byte slot using production
    /// <c>Ntlm1.ProcessOutgoing</c>.
    /// </summary>
    private static byte[] BuildSealedFramePerCodebase(
        byte ptype, int callId, int contextId, int opnum, byte[] plaintextStub, bool isServerSide)
    {
        ConnectionOrientedPdu basePdu = ptype switch
        {
            0x00 => (ConnectionOrientedPdu)new RequestCoPdu
            {
                CallId = callId,
                ContextId = contextId,
                Opnum = opnum,
                AllocationHint = plaintextStub.Length,
                Stub = plaintextStub,
            },
            0x02 => new ResponseCoPdu
            {
                CallId = callId,
                ContextId = contextId,
                AllocationHint = plaintextStub.Length,
                Stub = plaintextStub,
            },
            0x03 => new FaultCoPdu
            {
                CallId = callId,
                ContextId = contextId,
                AllocationHint = 0,
                Status = FaultCode.OPERATION_RANGE_ERROR,
                Stub = plaintextStub,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(ptype)),
        };
        byte[] pduBytes = PduCodec.EncodePdu(basePdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

        int padding = (4 - (pduBytes.Length % 4)) % 4;
        int verifierStart = pduBytes.Length + padding;
        int fragmentLength = verifierStart + AuthVerifierHeaderLength + NtlmAuthValueLength;

        byte[] protectedPdu = new byte[fragmentLength];
        Array.Copy(pduBytes, 0, protectedPdu, 0, pduBytes.Length);

        protectedPdu[verifierStart] = AuthTypeNtlm;
        protectedPdu[verifierStart + 1] = AuthLevelPrivacy;
        protectedPdu[verifierStart + 2] = (byte)padding;
        protectedPdu[verifierStart + 3] = 0;

        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 2), (ushort)NtlmAuthValueLength);

        // Encrypt ONLY the body portion (offsets 16..verifierStart) — the
        // common header MUST stay plaintext on the wire so the receiver can
        // parse fragLength + authLength + ptype. Matches the production
        // receiver's VerifyPacketProtection which verifies pduBody starting
        // at offset 16. The verifier (16 bytes) lands in the trailing slot.
        int bodyStart = ConnectionOrientedPdu.HEADER_LENGTH;
        int bodyLength = verifierStart - bodyStart;
        byte[] bodyAndVerifierSlot = new byte[bodyLength + NtlmAuthValueLength];
        Array.Copy(protectedPdu, bodyStart, bodyAndVerifierSlot, 0, bodyLength);
        // bodyAndVerifierSlot[bodyLength..bodyLength+16] starts zeroed (verifier slot)
#pragma warning disable CS0618 // Ntlm1 [Obsolete] is intentional in this passive-unwrap test
        var producer = new Opc.Classic.Dcom.Rpc.Auth.ntlm.Ntlm1(NtlmPassiveUnwrapper.DefaultFlags, (byte[])s_testSessionKey.Clone(), isServerSide);
#pragma warning restore CS0618
        var ndrBuffer = new Opc.Classic.Dcom.Internal.LegacyNdr.NdrBuffer(bodyAndVerifierSlot, 0);
        ndrBuffer.SetLength(bodyAndVerifierSlot.Length);
        var ndr = new Opc.Classic.Dcom.Internal.LegacyNdr.NdrCodec { Buffer = ndrBuffer, Format = Opc.Classic.Dcom.Internal.LegacyNdr.NdrFormat.DEFAULT_FORMAT };
        producer.ProcessOutgoing(ndr, index: 0, length: bodyLength, verifierIndex: bodyLength, isFragmented: false);

        // Copy encrypted body back into the frame body slot, and the
        // verifier into the trailing 16-byte auth-value slot.
        Array.Copy(bodyAndVerifierSlot, 0, protectedPdu, bodyStart, bodyLength);
        Array.Copy(bodyAndVerifierSlot, bodyLength, protectedPdu, verifierStart + AuthVerifierHeaderLength, NtlmAuthValueLength);

        return protectedPdu;
    }

    private static byte[] AppendBogusAuthTrailer(byte[] plainPdu, byte authType)
    {
        int padding = (4 - (plainPdu.Length % 4)) % 4;
        int verifierStart = plainPdu.Length + padding;
        int fragmentLength = verifierStart + AuthVerifierHeaderLength + NtlmAuthValueLength;
        byte[] frame = new byte[fragmentLength];
        Array.Copy(plainPdu, 0, frame, 0, plainPdu.Length);
        frame[verifierStart] = authType;
        frame[verifierStart + 1] = AuthLevelPrivacy;
        frame[verifierStart + 2] = (byte)padding;
        frame[verifierStart + 3] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 2), (ushort)NtlmAuthValueLength);
        return frame;
    }

    private static CapturedPacket NewTcpPacket(byte[] tcpPayload, DateTimeOffset timestamp)
    {
        byte[] frame = new byte[14 + 20 + 20 + tcpPayload.Length];

        // Ethernet header.
        frame[0] = 0x00;
        frame[1] = 0x11;
        frame[2] = 0x22;
        frame[3] = 0x33;
        frame[4] = 0x44;
        frame[5] = 0x55;
        frame[6] = 0x66;
        frame[7] = 0x77;
        frame[8] = 0x88;
        frame[9] = 0x99;
        frame[10] = 0xAA;
        frame[11] = 0xBB;
        frame[12] = 0x08;
        frame[13] = 0x00;

        int ipOffset = 14;
        frame[ipOffset] = 0x45;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ipOffset + 2, 2), (ushort)(20 + 20 + tcpPayload.Length));
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ipOffset + 4, 2), 0x1000);
        frame[ipOffset + 8] = 64;
        frame[ipOffset + 9] = 6;
        frame[ipOffset + 12] = 10;
        frame[ipOffset + 15] = 1;
        frame[ipOffset + 16] = 10;
        frame[ipOffset + 19] = 2;

        int tcpOffset = ipOffset + 20;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset, 2), 50000);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 2, 2), 135);
        frame[tcpOffset + 12] = 0x50;
        frame[tcpOffset + 13] = 0x18;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 14, 2), 8192);
        tcpPayload.CopyTo(frame.AsSpan(tcpOffset + 20));

        return new CapturedPacket(
            timestamp,
            frame.Length,
            frame,
            EthernetLinkType,
            new Dictionary<string, string?>());
    }
}
