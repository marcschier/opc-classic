//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

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
