// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class OrpcReplayToolTests
{
    [Test]
    public async Task Replay_NullPduSequence_Throws()
    {
        var tool = new OrpcReplayTool();

        await Assert.That(() => tool.Replay(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ReplayDetailed_ValidRequestResponseAndFault_Succeed()
    {
        var tool = new OrpcReplayTool();
        DecodedDcomFrame[] frames =
        [
            Decode(EncodeRequest()),
            Decode(EncodeResponse()),
            Decode(EncodeFault()),
        ];

        ReplayReport report = tool.ReplayDetailed(frames);

        await Assert.That(report.TotalSucceeded).IsEqualTo(3);
        await Assert.That(report.TotalFailed).IsEqualTo(0);
        await Assert.That(report.TotalSkipped).IsEqualTo(0);
    }

    [Test]
    public async Task ReplayDetailed_TruncatedFrame_FailsWithHexContext()
    {
        byte[] valid = EncodeRequest();
        byte[] truncated = valid[..^1];
        var frame = new DecodedDcomFrame(
            Pdu: null,
            PduType: "request",
            RawFrame: truncated,
            StubBytes: null,
            Failure: null);

        ReplayReport report = new OrpcReplayTool().ReplayDetailed([frame]);

        await Assert.That(report.TotalFailed).IsEqualTo(1);
        await Assert.That(report.Buckets[0].Key).IsEqualTo("<unknown>");
        await Assert.That(report.Buckets[0].FirstFailureMessage).Contains("frag_length mismatch");
        await Assert.That(report.Buckets[0].FirstFailureHexContext).IsNotNull();
        await Assert.That(report.Buckets[0].FirstFailureHexContext).IsNotEqualTo(string.Empty);
    }

    [Test]
    public async Task ReplayDetailed_UnknownStructuredFailure_IsCountedBeforeTypeFiltering()
    {
        var frame = new DecodedDcomFrame(
            Pdu: null,
            PduType: "unknown",
            RawFrame: [0x05, 0x00],
            StubBytes: null,
            Failure: new DcomDecodeFailure("framing", "truncated_header", "header truncated"));

        ReplayReport report = new OrpcReplayTool().ReplayDetailed([frame]);

        await Assert.That(report.TotalFailed).IsEqualTo(1);
        await Assert.That(report.Buckets.Count).IsEqualTo(1);
        await Assert.That(report.Buckets[0].Key).IsEqualTo("<unknown>");
        await Assert.That(report.Buckets[0].FailedCount).IsEqualTo(1);
    }

    [Test]
    public async Task ReplayDetailed_MalformedFragmentLength_Fails()
    {
        byte[] malformed = EncodeResponse();
        BinaryPrimitives.WriteUInt16LittleEndian(
            malformed.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2),
            checked((ushort)(malformed.Length + 8)));
        var frame = new DecodedDcomFrame(null, "response", malformed, null, null);

        ReplayReport report = new OrpcReplayTool().ReplayDetailed([frame]);

        await Assert.That(report.TotalFailed).IsEqualTo(1);
        await Assert.That(report.Buckets[0].FirstFailureMessage).Contains("frag_length mismatch");
    }

    [Test]
    public async Task ReplayDetailed_InvalidOrpcEnvelope_Fails()
    {
        var request = new RequestCoPdu
        {
            AllocationHint = 4,
            ContextId = 0,
            Opnum = 3,
            Stub = [0x01, 0x02, 0x03, 0x04],
            CallId = 17,
        };
        byte[] raw = PduCodec.EncodePdu(request, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

        ReplayReport report = new OrpcReplayTool().ReplayDetailed([Decode(raw)]);

        await Assert.That(report.TotalFailed).IsEqualTo(1);
        await Assert.That(report.Buckets[0].FirstFailureMessage).IsNotNull();
        await Assert.That(report.Buckets[0].FirstFailureHexContext).Contains("01");
    }

    [Test]
    public async Task ReplayDetailed_FragmentedRequest_ReassemblesEnvelopeSplitAcrossFragments()
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(
            Enumerable.Range(0, 32).Select(i => (byte)i).ToArray(),
            Guid.Parse("11111111-2222-3333-4444-555555555555"));
        DecodedDcomFrame[] frames = CreateRequestFragments(stub, splitAt: 5, callId: 51);

        ReplayReport report = new OrpcReplayTool().ReplayDetailed(frames);

        await Assert.That(report.TotalSucceeded).IsEqualTo(1);
        await Assert.That(report.TotalFailed).IsEqualTo(0);
        await Assert.That(report.Buckets.Single().SucceededCount).IsEqualTo(1);
    }

    [Test]
    public async Task ReplayDetailed_FragmentedResponse_ReassemblesEnvelopeSplitAcrossFragments()
    {
        byte[] stub = OrpcEnvelope.BuildResponseStub(
            Enumerable.Range(0, 32).Select(i => (byte)(0x80 + i)).ToArray());
        DecodedDcomFrame[] frames = CreateResponseFragments(stub, splitAt: 3, callId: 52);

        ReplayReport report = new OrpcReplayTool().ReplayDetailed(frames);

        await Assert.That(report.TotalSucceeded).IsEqualTo(1);
        await Assert.That(report.TotalFailed).IsEqualTo(0);
        await Assert.That(report.Buckets.Single().SucceededCount).IsEqualTo(1);
    }

    [Test]
    public async Task ReplayDetailed_FragmentedRequestAndResponse_ZeroAllocationHintsAreAccepted()
    {
        byte[] requestStub = OrpcEnvelope.BuildRequestStub(
            Enumerable.Range(0, 24).Select(i => (byte)i).ToArray(),
            Guid.Empty);
        byte[] responseStub = OrpcEnvelope.BuildResponseStub(
            Enumerable.Range(0, 24).Select(i => (byte)(0x40 + i)).ToArray());

        ReplayReport requestReport = new OrpcReplayTool().ReplayDetailed(
            CreateRequestFragments(requestStub, splitAt: 4, callId: 56, zeroAllocationHints: true));
        ReplayReport responseReport = new OrpcReplayTool().ReplayDetailed(
            CreateResponseFragments(responseStub, splitAt: 2, callId: 57, zeroAllocationHints: true));

        await Assert.That(requestReport.TotalSucceeded).IsEqualTo(1);
        await Assert.That(requestReport.TotalFailed).IsEqualTo(0);
        await Assert.That(responseReport.TotalSucceeded).IsEqualTo(1);
        await Assert.That(responseReport.TotalFailed).IsEqualTo(0);
    }

    [Test]
    public async Task ReplayDetailed_FragmentedRequest_MissingMiddleFailsAllocationValidation()
    {
        byte[] completeEnvelope = OrpcEnvelope.BuildRequestStub(
            new byte[] { 0x10, 0x20 },
            Guid.Empty);
        var first = new RequestCoPdu
        {
            AllocationHint = completeEnvelope.Length + 8,
            ContextId = 0,
            Opnum = 3,
            Stub = completeEnvelope,
            CallId = 53,
            Flags = ConnectionOrientedPdu.PFC_FIRST_FRAG,
        };
        var last = new RequestCoPdu
        {
            AllocationHint = 0,
            ContextId = 0,
            Opnum = 3,
            Stub = [],
            CallId = 53,
            Flags = ConnectionOrientedPdu.PFC_LAST_FRAG,
        };

        ReplayReport report = new OrpcReplayTool().ReplayDetailed(
        [
            Decode(PduCodec.EncodePdu(first, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)),
            Decode(PduCodec.EncodePdu(last, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)),
        ]);

        await Assert.That(report.TotalFailed).IsEqualTo(1);
        await Assert.That(report.TotalSucceeded).IsEqualTo(0);
        await Assert.That(report.Buckets.Single().FirstFailureMessage).Contains("First fragment allocation hint");
    }

    [Test]
    public async Task ReplayDetailed_FragmentedRequest_InconsistentContextOrOpnumFails()
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(new byte[] { 0x10, 0x20 }, Guid.Empty);
        DecodedDcomFrame[] inconsistentContext = CreateRequestFragments(
            stub,
            splitAt: 5,
            callId: 54,
            lastContextId: 1);
        DecodedDcomFrame[] inconsistentOpnum = CreateRequestFragments(
            stub,
            splitAt: 5,
            callId: 55,
            lastOpnum: 4);

        ReplayReport contextReport = new OrpcReplayTool().ReplayDetailed(inconsistentContext);
        ReplayReport opnumReport = new OrpcReplayTool().ReplayDetailed(inconsistentOpnum);

        await Assert.That(contextReport.TotalFailed).IsEqualTo(1);
        await Assert.That(contextReport.Buckets.Single().FirstFailureMessage).Contains("context id");
        await Assert.That(opnumReport.TotalFailed).IsEqualTo(1);
        await Assert.That(opnumReport.Buckets.Single().FirstFailureMessage).Contains("opnum");
    }

    [Test]
    public async Task ReplayDetailed_MixedFrames_PopulatesAllBuckets()
    {
        DecodedDcomFrame validRequest = Decode(EncodeRequest());
        DecodedDcomFrame validFault = Decode(EncodeFault());
        byte[] invalidOrpc = PduCodec.EncodePdu(
            new RequestCoPdu
            {
                AllocationHint = 1,
                ContextId = 0,
                Opnum = 3,
                Stub = [0xFF],
                CallId = 19,
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        var skipped = new DecodedDcomFrame(
            new DecodedOpcPdu
            {
                Timestamp = DateTimeOffset.UnixEpoch,
                PduType = "request",
                Opnum = 3,
                RequestStubLength = 8,
            },
            "request",
            RawFrame: null,
            StubBytes: null,
            Failure: null);

        ReplayReport report = new OrpcReplayTool().ReplayDetailed(
            [validRequest, validFault, Decode(invalidOrpc), skipped]);

        await Assert.That(report.TotalSucceeded).IsEqualTo(2);
        await Assert.That(report.TotalFailed).IsEqualTo(1);
        await Assert.That(report.TotalSkipped).IsEqualTo(1);
        await Assert.That(report.Buckets.Sum(b => b.SucceededCount)).IsEqualTo(2);
        await Assert.That(report.Buckets.Sum(b => b.FailedCount)).IsEqualTo(1);
        await Assert.That(report.Buckets.Sum(b => b.SkippedCount)).IsEqualTo(1);
    }

    [Test]
    public async Task ReplayDetailed_CancellationDuringEnumeration_ThrowsWithoutReport()
    {
        using var cts = new CancellationTokenSource();

        IEnumerable<DecodedDcomFrame> Frames()
        {
            yield return Decode(EncodeRequest());
            cts.Cancel();
            yield return Decode(EncodeResponse());
        }

        await Assert.That(() => new OrpcReplayTool().ReplayDetailed(Frames(), cts.Token))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task TryReplayBody_RequiresValidOrpcEnvelope()
    {
        var tool = new OrpcReplayTool();
        byte[] valid = OrpcEnvelope.BuildRequestStub(new byte[] { 0x10, 0x20 }, Guid.Empty);

        bool emptyOk = tool.TryReplayBody(ReadOnlyMemory<byte>.Empty, out string? emptyError);
        bool validOk = tool.TryReplayBody(valid, out string? validError);
        bool invalidOk = tool.TryReplayBody(new byte[] { 0x01, 0x02, 0x03, 0x04 }, out string? invalidError);

        await Assert.That(emptyOk).IsFalse();
        await Assert.That(emptyError).IsEqualTo("Empty payload");
        await Assert.That(validOk).IsTrue();
        await Assert.That(validError).IsNull();
        await Assert.That(invalidOk).IsFalse();
        await Assert.That(invalidError).IsNotNull();
    }

    [Test]
    public async Task ReplayKeyStats_DefaultsAndMutableCounters_Work()
    {
        var stats = new ReplayKeyStats("iid/op1/request")
        {
            SucceededCount = 2,
            FailedCount = 1,
            SkippedCount = 3,
            FirstFailureMessage = "bad ndr",
            FirstFailureHexContext = "0000: 01 02",
        };

        await Assert.That(stats.Key).IsEqualTo("iid/op1/request");
        await Assert.That(stats.SucceededCount).IsEqualTo(2);
        await Assert.That(stats.FailedCount).IsEqualTo(1);
        await Assert.That(stats.SkippedCount).IsEqualTo(3);
        await Assert.That(stats.FirstFailureMessage).IsEqualTo("bad ndr");
        await Assert.That(stats.FirstFailureHexContext).Contains("01 02");
    }

    private static DecodedDcomFrame Decode(byte[] frame, bool reverse = false)
    {
        var decoder = new OpcDcomDecoder();
        return decoder.DecodeRawDcomFrameStrict(
            frame,
            reverse ? IPAddress.Parse("10.0.0.2") : IPAddress.Parse("10.0.0.1"),
            reverse ? 135 : 49152,
            reverse ? IPAddress.Parse("10.0.0.1") : IPAddress.Parse("10.0.0.2"),
            reverse ? 49152 : 135,
            DateTimeOffset.UnixEpoch);
    }

    private static DecodedDcomFrame[] CreateRequestFragments(
        byte[] stub,
        int splitAt,
        int callId,
        int lastContextId = 0,
        int lastOpnum = 3,
        bool zeroAllocationHints = false)
    {
        var first = new RequestCoPdu
        {
            AllocationHint = zeroAllocationHints ? 0 : stub.Length,
            ContextId = 0,
            Opnum = 3,
            Stub = stub[..splitAt],
            CallId = callId,
            Flags = ConnectionOrientedPdu.PFC_FIRST_FRAG,
        };
        var last = new RequestCoPdu
        {
            AllocationHint = zeroAllocationHints ? 0 : stub.Length - splitAt,
            ContextId = lastContextId,
            Opnum = lastOpnum,
            Stub = stub[splitAt..],
            CallId = callId,
            Flags = ConnectionOrientedPdu.PFC_LAST_FRAG,
        };
        return
        [
            Decode(PduCodec.EncodePdu(first, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)),
            Decode(PduCodec.EncodePdu(last, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)),
        ];
    }

    private static DecodedDcomFrame[] CreateResponseFragments(
        byte[] stub,
        int splitAt,
        int callId,
        bool zeroAllocationHints = false)
    {
        var first = new ResponseCoPdu
        {
            AllocationHint = zeroAllocationHints ? 0 : stub.Length,
            ContextId = 0,
            Stub = stub[..splitAt],
            CallId = callId,
            Flags = ConnectionOrientedPdu.PFC_FIRST_FRAG,
        };
        var last = new ResponseCoPdu
        {
            AllocationHint = zeroAllocationHints ? 0 : stub.Length - splitAt,
            ContextId = 0,
            Stub = stub[splitAt..],
            CallId = callId,
            Flags = ConnectionOrientedPdu.PFC_LAST_FRAG,
        };
        return
        [
            Decode(PduCodec.EncodePdu(first, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE), reverse: true),
            Decode(PduCodec.EncodePdu(last, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE), reverse: true),
        ];
    }

    private static byte[] EncodeRequest()
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(new byte[] { 0x10, 0x20 }, Guid.Empty);
        return PduCodec.EncodePdu(
            new RequestCoPdu
            {
                AllocationHint = stub.Length,
                ContextId = 0,
                Opnum = 3,
                Stub = stub,
                CallId = 11,
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
    }

    private static byte[] EncodeResponse()
    {
        byte[] stub = OrpcEnvelope.BuildResponseStub(new byte[] { 0x30, 0x40 });
        return PduCodec.EncodePdu(
            new ResponseCoPdu
            {
                AllocationHint = stub.Length,
                ContextId = 0,
                Stub = stub,
                CallId = 11,
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
    }

    private static byte[] EncodeFault()
        => PduCodec.EncodePdu(
            new FaultCoPdu
            {
                AllocationHint = 0,
                ContextId = 0,
                Status = (FaultCode)0x1C010003,
                Stub = [],
                CallId = 11,
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
}
