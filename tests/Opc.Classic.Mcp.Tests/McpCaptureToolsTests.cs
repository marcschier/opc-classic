// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.CompilerServices;
using ModelContextProtocol;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Mcp.Capture;
using Opc.Classic.Mcp.Tools;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Mcp.Tests;

public sealed class McpCaptureToolsTests
{
    [Test]
    public async Task CaptureSessionDto_From_Maps_session_metadata()
    {
        string folder = CreateScratchFolder();
        string rawPcapPath = Path.Combine(folder, "trace.pcap");
        var source = new SyntheticCaptureSource(rawPcapPath)
        {
            PacketCount = 3,
            ByteCount = 512,
        };
        var request = new CaptureStartRequest(
            InterfaceName: "eth0",
            BpfFilter: "tcp port 135",
            Promiscuous: false,
            MaxBytes: 4096,
            MaxPackets: 9,
            MaxDurationSeconds: 60);
        await using var session = new CaptureSession("capture-1", "synthetic", source, folder, request);

        await session.StartAsync(CancellationToken.None);
        await session.StopAsync(CancellationToken.None);
        CaptureSessionDto dto = CaptureSessionDto.From(session);

        await Assert.That(dto.SessionId).IsEqualTo("capture-1");
        await Assert.That(dto.Source).IsEqualTo("synthetic");
        await Assert.That(dto.State).IsEqualTo(CaptureSessionState.Completed);
        await Assert.That(dto.StartedAt.HasValue).IsTrue();
        await Assert.That(dto.StoppedAt.HasValue).IsTrue();
        await Assert.That(dto.PacketCount).IsEqualTo(3);
        await Assert.That(dto.ByteCount).IsEqualTo(512);
        await Assert.That(dto.InterfaceName).IsEqualTo("eth0");
        await Assert.That(dto.Filter).IsEqualTo("tcp port 135");
        await Assert.That(dto.Error).IsNull();
        await Assert.That(dto.RawPcapFilePath).IsEqualTo(rawPcapPath);
    }

    [Test]
    public async Task CaptureTools_ListCaptures_Filters_active_and_completed_sessions()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => new SyntheticCaptureSource(rawPcapPath: null),
            new CaptureStartRequest(InterfaceName: "lo", BpfFilter: "tcp"),
            CancellationToken.None);

        IReadOnlyList<CaptureSessionDto> active = tools.ListCaptures("active");
        CaptureSessionDto stopped = await tools.StopCapture(session.Id, CancellationToken.None);
        IReadOnlyList<CaptureSessionDto> activeAfterStop = tools.ListCaptures("active");
        IReadOnlyList<CaptureSessionDto> completed = tools.ListCaptures("completed");
        IReadOnlyList<CaptureSessionDto> all = tools.ListCaptures("all");

        await Assert.That(active.Count).IsEqualTo(1);
        await Assert.That(active[0].SessionId).IsEqualTo(session.Id);
        await Assert.That(active[0].State).IsEqualTo(CaptureSessionState.Running);
        await Assert.That(stopped.State).IsEqualTo(CaptureSessionState.Completed);
        await Assert.That(activeAfterStop.Count).IsEqualTo(0);
        await Assert.That(completed.Count).IsEqualTo(1);
        await Assert.That(completed[0].SessionId).IsEqualTo(session.Id);
        await Assert.That(all.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CaptureTools_TailCapture_RunningSessionWithNoPackets_ReturnsEmptyWindow_NotDone()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => new SyntheticCaptureSource(rawPcapPath: null),
            new CaptureStartRequest(InterfaceName: "lo"),
            CancellationToken.None);

        CaptureTailResultDto result = await tools.TailCapture(session.Id, max: 200, sinceIndex: 0, cancellationToken: CancellationToken.None);

        await Assert.That(result.SessionId).IsEqualTo(session.Id);
        await Assert.That(result.Pdus.Count).IsEqualTo(0);
        await Assert.That(result.NextIndex).IsEqualTo(0);
        await Assert.That(result.TotalEmitted).IsEqualTo(0);
        await Assert.That(result.SessionState).IsEqualTo(CaptureSessionState.Running);
        // Session is still running so Done MUST be false even though the cache
        // is empty - the caller should keep polling.
        await Assert.That(result.Done).IsFalse();
    }

    [Test]
    public async Task CaptureTools_TailCapture_AfterStop_ReportsDoneTrue()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => new SyntheticCaptureSource(rawPcapPath: null),
            new CaptureStartRequest(InterfaceName: "lo"),
            CancellationToken.None);
        await tools.StopCapture(session.Id, CancellationToken.None);

        CaptureTailResultDto result = await tools.TailCapture(session.Id, max: 200, sinceIndex: 0, cancellationToken: CancellationToken.None);

        await Assert.That(result.SessionState).IsEqualTo(CaptureSessionState.Completed);
        // Session ended + cache fully drained (no packets) → caller should stop polling.
        await Assert.That(result.Done).IsTrue();
    }

    [Test]
    public async Task CaptureTools_TailCapture_UnknownSession_ThrowsMcpException()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);

        await Assert.That(async () =>
            await tools.TailCapture("missing", max: 10, sinceIndex: 0, cancellationToken: CancellationToken.None))
            .Throws<McpException>();
    }

    [Test]
    public async Task CaptureTools_StartCapture_NtlmSessionKeyHex_ValidatesLength()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);

        // 31 hex chars = 15.5 bytes (invalid length)
        await Assert.That(async () => await tools.StartCapture(
            interfaceName: "lo",
            ntlmSessionKeyHex: "0123456789ABCDEF0123456789ABCDE",
            cancellationToken: CancellationToken.None)).Throws<McpException>();

        // Non-hex char in payload
        await Assert.That(async () => await tools.StartCapture(
            interfaceName: "lo",
            ntlmSessionKeyHex: "ZZZZ56789ABCDEF0123456789ABCDEF0",
            cancellationToken: CancellationToken.None)).Throws<McpException>();
    }

    [Test]
    public async Task CaptureTools_GetCapture_Returns_pcap_path_and_empty_summaries_without_decoding_live_traffic()
    {
        await using CaptureSessionManager manager = CreateManager();
        string rawPcapPath = Path.Combine(AppContext.BaseDirectory, "synthetic-capture.pcap");
        var tools = new CaptureTools(manager);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => new SyntheticCaptureSource(rawPcapPath),
            new CaptureStartRequest(InterfaceName: "lo"),
            CancellationToken.None);

        string pcapPath = await tools.GetCapture(session.Id, "pcap-path", cancellationToken: CancellationToken.None);
        string json = await tools.GetCapture(session.Id, "json", cancellationToken: CancellationToken.None);
        string dcom = await tools.GetCapture(session.Id, "dcom", cancellationToken: CancellationToken.None);
        CaptureSummary summary = await tools.SummarizeCapture(session.Id, cancellationToken: CancellationToken.None);
        bool removed = await tools.RemoveCapture(session.Id, CancellationToken.None);

        await Assert.That(pcapPath).IsEqualTo(rawPcapPath);
        await Assert.That(json).IsEqualTo("[]");
        await Assert.That(dcom).Contains($"# Opc.Classic capture session {session.Id} — 0 PDUs");
        await Assert.That(summary.SessionId).IsEqualTo(session.Id);
        await Assert.That(summary.PduCount).IsEqualTo(0);
        await Assert.That(summary.DurationSeconds).IsEqualTo(0d);
        await Assert.That(removed).IsTrue();
        await Assert.That(manager.Count).IsEqualTo(0);
    }

    [Test]
    public async Task CaptureTools_Invalid_session_or_state_inputs_throw_mcp_exceptions()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);

        await Assert.That(() => tools.ListCaptures("paused")).Throws<McpException>();
        await Assert.That(async () => await tools.StopCapture("missing", CancellationToken.None)).Throws<McpException>();
        await Assert.That(async () => await tools.GetCapture("missing", "pcap-path", cancellationToken: CancellationToken.None)).Throws<McpException>();
    }

    [Test]
    public async Task CaptureTools_DecodePdu_DecodesKnownRequestResponseAndFaultFrames()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);

        string request = tools.DecodePdu(FormatHex(EncodeRequest()));
        string response = tools.DecodePdu(Convert.ToHexString(EncodeResponse()));
        string fault = tools.DecodePdu(Convert.ToHexString(EncodeFault()));

        await Assert.That(request).Contains("\"PduType\": \"request\"");
        await Assert.That(request).Contains("\"CallId\": 21");
        await Assert.That(request).Contains("\"Opnum\": 7");
        await Assert.That(request).DoesNotContain("RawFrame");
        await Assert.That(request).DoesNotContain("StubBytes");
        await Assert.That(response).Contains("\"PduType\": \"response\"");
        await Assert.That(response).Contains("\"CallId\": 21");
        await Assert.That(fault).Contains("\"PduType\": \"fault\"");
        await Assert.That(fault).Contains("\"FaultStatus\":");
    }

    [Test]
    public async Task CaptureTools_DecodePdu_RejectsEmptyTruncatedAndUndecodableInputWithStructuredErrors()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        byte[] truncated = EncodeRequest()[..^1];
        byte[] undecodable = new byte[ConnectionOrientedPdu.HEADER_LENGTH];
        undecodable[0] = 5;
        undecodable[2] = 0xEF;
        undecodable[8] = (byte)undecodable.Length;

        var empty = await Assert.That(() => tools.DecodePdu("0x, ; :"))
            .Throws<McpException>();
        var shortFrame = await Assert.That(() => tools.DecodePdu(Convert.ToHexString(truncated)))
            .Throws<McpException>();
        var badType = await Assert.That(() => tools.DecodePdu(Convert.ToHexString(undecodable)))
            .Throws<McpException>();
        var odd = await Assert.That(() => tools.DecodePdu("0x0"))
            .Throws<McpException>();

        await Assert.That(empty!.Message).Contains("\"code\":\"capture_decode_failed\"");
        await Assert.That(empty.Message).Contains("\"reason\":\"empty_frame\"");
        await Assert.That(shortFrame!.Message).Contains("\"reason\":\"fragment_length_mismatch\"");
        await Assert.That(badType!.Message).Contains("\"stage\":\"pdu_codec\"");
        await Assert.That(odd!.Message).Contains("\"reason\":\"odd_nibble_count\"");
    }

    [Test]
    public async Task CaptureTools_DecodePdu_ParsesFramesLargerThanFormer8192ByteLimit()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        byte[] payload = Enumerable.Range(0, 9000).Select(i => (byte)i).ToArray();
        byte[] stub = OrpcEnvelope.BuildRequestStub(payload, Guid.Empty);
        byte[] frame = PduCodec.EncodePdu(
            new RequestCoPdu
            {
                AllocationHint = stub.Length,
                ContextId = 0,
                Opnum = 7,
                Stub = stub,
                CallId = 22,
            },
            ushort.MaxValue);

        string decoded = tools.DecodePdu(Convert.ToHexString(frame));

        await Assert.That(frame.Length).IsGreaterThan(8192);
        await Assert.That(decoded).Contains("\"PduType\": \"request\"");
        await Assert.That(decoded).Contains($"\"RequestStubLength\": {stub.Length}");
    }

    [Test]
    public async Task CaptureTools_DecodePdu_RejectsTooLargeEmbeddedPrefixAndTrailingBytes()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        string tooLarge = new('0', (ushort.MaxValue + 1) * 2);
        byte[] trailing = [.. EncodeRequest(), 0x00];

        var oversized = await Assert.That(() => tools.DecodePdu(tooLarge)).Throws<McpException>();
        var embeddedPrefix = await Assert.That(() => tools.DecodePdu("05 0x00")).Throws<McpException>();
        var extraBytes = await Assert.That(() => tools.DecodePdu(Convert.ToHexString(trailing))).Throws<McpException>();

        await Assert.That(oversized!.Message).Contains("\"reason\":\"input_too_large\"");
        await Assert.That(embeddedPrefix!.Message).Contains("\"reason\":\"invalid_character\"");
        await Assert.That(extraBytes!.Message).Contains("\"reason\":\"fragment_length_mismatch\"");
    }

    private static CaptureSessionManager CreateManager()
    {
        string root = CreateScratchFolder();
        return new CaptureSessionManager(root, maxActiveSessions: 2, maxRetainedSessions: 4);
    }

    private static string CreateScratchFolder()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "McpCaptureToolsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string FormatHex(byte[] bytes)
        => "0x" + string.Join(" ", bytes.Select(b => b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));

    private static byte[] EncodeRequest()
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(new byte[] { 0x10, 0x20 }, Guid.Empty);
        return PduCodec.EncodePdu(
            new RequestCoPdu
            {
                AllocationHint = stub.Length,
                ContextId = 0,
                Opnum = 7,
                Stub = stub,
                CallId = 21,
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
                CallId = 21,
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
                CallId = 21,
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

    private sealed class SyntheticCaptureSource : ICaptureSource
    {
        private readonly string? _rawPcapPath;

        public SyntheticCaptureSource(string? rawPcapPath) => _rawPcapPath = rawPcapPath;

        public long PacketCount { get; init; }
        public long ByteCount { get; init; }
        public int LinkType => 0;
        public CaptureStartRequest? StartRequest { get; private set; }

        public Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartRequest = request;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<CapturedPacket> ReadAllAsync(
            long? maxPackets,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _ = maxPackets;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield break;
        }

        public string? GetRawPcapFilePath() => _rawPcapPath;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
