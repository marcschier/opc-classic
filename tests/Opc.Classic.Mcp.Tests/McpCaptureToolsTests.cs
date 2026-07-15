// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
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
    public async Task CaptureTools_TailCapture_NamedCursorLostResponseRetryReturnsSameWindow()
    {
        await using CaptureSessionManager manager = CreateManager();
        var source = new SyntheticCaptureSource(rawPcapPath: null)
        {
            PacketCount = 4,
        };
        source.Packets.Add(NewAnnotatedPacket("a"));
        source.Packets.Add(NewAnnotatedPacket("b"));
        source.Packets.Add(NewAnnotatedPacket("c"));
        source.Packets.Add(NewAnnotatedPacket("d"));
        var tools = new CaptureTools(manager);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => source,
            new CaptureStartRequest(InterfaceName: "lo"),
            CancellationToken.None);

        CaptureTailResultDto first = await tools.TailCapture(
            session.Id, max: 2, sinceIndex: 0, cancellationToken: CancellationToken.None,
            subscriberId: "retry-client", subscriberCapacity: 4);
        CaptureTailResultDto retry = await tools.TailCapture(
            session.Id, max: 2, sinceIndex: 0, cancellationToken: CancellationToken.None,
            subscriberId: "retry-client", subscriberCapacity: 4);
        CaptureTailResultDto acknowledged = await tools.TailCapture(
            session.Id, max: 2, sinceIndex: first.NextIndex, cancellationToken: CancellationToken.None,
            subscriberId: "retry-client", subscriberCapacity: 4);

        await Assert.That(first.NextIndex).IsEqualTo(2);
        await Assert.That(retry.NextIndex).IsEqualTo(first.NextIndex);
        await Assert.That(retry.Pdus.SequenceEqual(first.Pdus)).IsTrue();
        await Assert.That(acknowledged.Pdus.Count).IsEqualTo(2);
        await Assert.That(acknowledged.NextIndex).IsEqualTo(4);
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
    public async Task CaptureTools_StartCapture_IsOpenWorldAndAmbientSsoDefaultsToFalse()
    {
        System.Reflection.MethodInfo method = typeof(CaptureTools).GetMethod(nameof(CaptureTools.StartCapture))
            ?? throw new InvalidOperationException("StartCapture method was not found.");
        System.Reflection.CustomAttributeData attribute = method.CustomAttributes.Single(
            candidate => candidate.AttributeType == typeof(McpServerToolAttribute));
        System.Reflection.CustomAttributeNamedArgument openWorld = attribute.NamedArguments.Single(
            argument => argument.MemberName == nameof(McpServerToolAttribute.OpenWorld));
        System.Reflection.ParameterInfo ambientSso = method.GetParameters().Single(
            parameter => parameter.Name == "ambientSso");

        await Assert.That(openWorld.TypedValue.Value).IsEqualTo(true);
        await Assert.That(ambientSso.HasDefaultValue).IsTrue();
        await Assert.That(ambientSso.DefaultValue).IsEqualTo(false);
    }

    [Test]
    public async Task CaptureTools_StartCapture_StartsBroadBeforeTargetResolutionThenNarrowsFilter()
    {
        await using CaptureSessionManager manager = CreateManager();
        var source = new SyntheticCaptureSource(rawPcapPath: null);
        var resolver = new ObservingTargetResolver(
            () => source.StartRequest is not null,
            new CaptureTargetMetadata
            {
                Host = "opc-host",
                Status = "resolved",
                Bindings = ["ncacn_ip_tcp:opc-host[51234]"],
                Ports = [135, 51234],
            });
        var tools = new CaptureTools(manager, resolver, _ => source);

        CaptureSessionDto result = await tools.StartCapture(
            interfaceName: "lo",
            cancellationToken: CancellationToken.None,
            targetHost: "opc-host",
            progId: "Vendor.Server.1");

        await Assert.That(resolver.CaptureWasRunning).IsTrue();
        await Assert.That(resolver.AmbientSso).IsFalse();
        await Assert.That(source.StartRequest!.BpfFilter).IsNull();
        await Assert.That(source.StartRequest.ServerPorts).IsNull();
        await Assert.That(source.EffectiveFilter).IsEqualTo("tcp and (port 135 or port 51234)");
        await Assert.That(result.Target!.Status).IsEqualTo("resolved");
        await Assert.That(result.Target.Ports).Contains(51234);
        await Assert.That(result.EffectiveFilter).IsEqualTo(source.EffectiveFilter);
        await Assert.That(result.FilterTransition!.Status)
            .IsEqualTo(CaptureFilterTransitionStatus.LiveUpdated);
    }

    [Test]
    public async Task CaptureTools_SetCaptureFilter_ReturnsExplicitLiveTransition()
    {
        await using CaptureSessionManager manager = CreateManager();
        var source = new SyntheticCaptureSource(rawPcapPath: null);
        var tools = new CaptureTools(
            manager,
            new CaptureTargetResolver(),
            _ => source);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => source,
            new CaptureStartRequest(InterfaceName: "lo", BpfFilter: "tcp"),
            CancellationToken.None);

        CaptureFilterTransitionResult result = await tools.SetCaptureFilter(
            session.Id,
            "tcp port 51234",
            CancellationToken.None);

        await Assert.That(result.SessionId).IsEqualTo(session.Id);
        await Assert.That(result.Status).IsEqualTo(CaptureFilterTransitionStatus.LiveUpdated);
        await Assert.That(result.PreviousFilter).IsEqualTo("tcp");
        await Assert.That(result.EffectiveFilter).IsEqualTo("tcp port 51234");
        await Assert.That(source.EffectiveFilter).IsEqualTo("tcp port 51234");
    }

    [Test]
    public async Task CaptureTargetResolver_TcpConnectionString_ReusesConnectionNormalization()
    {
        var resolver = new CaptureTargetResolver();

        CaptureTargetMetadata result = await resolver.ResolveAsync(
            targetHost: "fallback",
            progId: "  Vendor.Server.1  ",
            clsid: null,
            connectionString: "tcp://opc-host:51001",
            CancellationToken.None);

        await Assert.That(result.Host).IsEqualTo("opc-host");
        await Assert.That(result.ProgId).IsEqualTo("Vendor.Server.1");
        await Assert.That(result.Status).IsEqualTo("resolved");
        await Assert.That(result.Ports).Contains(51001);
    }

    [Test]
    public async Task CaptureTargetResolver_DefaultDoesNotConnectWithAmbientCredentials()
    {
        var factory = new FakeActivationSessionFactory(CreateActivationResponse());
        var resolver = new CaptureTargetResolver(factory);

        CaptureTargetMetadata result = await resolver.ResolveAsync(
            "opc-host",
            null,
            "10138C2C-0000-0000-0000-00000000C000",
            null,
            CancellationToken.None);

        await Assert.That(result.Status).IsEqualTo("ambient_sso_required");
        await Assert.That(result.Error!).Contains("ambientSso=true");
        await Assert.That(factory.CreatedCount).IsEqualTo(0);
    }

    [Test]
    public async Task CaptureTargetResolver_RepeatedActivation_ReleasesAndDisposesEverySession()
    {
        var factory = new FakeActivationSessionFactory(CreateActivationResponse());
        var resolver = new CaptureTargetResolver(factory);
        const string clsid = "10138C2C-0000-0000-0000-00000000C001";

        CaptureTargetMetadata first = await resolver.ResolveAsync(
            "opc-host", null, clsid, null, CancellationToken.None, ambientSso: true);
        CaptureTargetMetadata second = await resolver.ResolveAsync(
            "opc-host", null, clsid, null, CancellationToken.None, ambientSso: true);

        await Assert.That(first.Status).IsEqualTo("activated");
        await Assert.That(second.Status).IsEqualTo("activated");
        await Assert.That(factory.CreatedCount).IsEqualTo(2);
        await Assert.That(factory.ReleaseCount).IsEqualTo(2);
        await Assert.That(factory.DisposeCount).IsEqualTo(2);
    }

    [Test]
    public async Task CaptureTargetResolver_ActivationFailure_StillDisposesSession()
    {
        var factory = new FakeActivationSessionFactory(
            CreateActivationResponse(),
            activationException: new InvalidOperationException("activation failed"));
        var resolver = new CaptureTargetResolver(factory);

        CaptureTargetMetadata result = await resolver.ResolveAsync(
            "opc-host",
            null,
            "10138C2C-0000-0000-0000-00000000C002",
            null,
            CancellationToken.None,
            ambientSso: true);

        await Assert.That(result.Status).IsEqualTo("failed");
        await Assert.That(result.Error!).Contains("activation failed");
        await Assert.That(factory.ReleaseCount).IsEqualTo(0);
        await Assert.That(factory.DisposeCount).IsEqualTo(1);
    }

    [Test]
    public async Task CaptureTargetResolver_ReleaseFailure_IsSurfacedAfterDisposal()
    {
        var factory = new FakeActivationSessionFactory(
            CreateActivationResponse(),
            releaseException: new InvalidOperationException("release failed"));
        var resolver = new CaptureTargetResolver(factory);

        CaptureTargetMetadata result = await resolver.ResolveAsync(
            "opc-host",
            null,
            "10138C2C-0000-0000-0000-00000000C003",
            null,
            CancellationToken.None,
            ambientSso: true);

        await Assert.That(result.Status).IsEqualTo("activated_release_failed");
        await Assert.That(result.Error!).Contains("release failed");
        await Assert.That(factory.DisposeCount).IsEqualTo(1);
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
    public async Task CaptureTools_GetAndSummarize_FinalizePendingGapRecovery()
    {
        await using CaptureSessionManager manager = CreateManager();
        byte[] firstFrame = EncodeRequest(callId: 301);
        byte[] interruptedFrame = EncodeRequest(callId: 302);
        byte[] recoveredFrame = EncodeRequest(callId: 303);
        uint firstSequence = 20_000;
        uint interruptedSequence = firstSequence + (uint)firstFrame.Length;
        uint recoveredSequence = interruptedSequence + (uint)interruptedFrame.Length;
        var source = new SyntheticCaptureSource(rawPcapPath: null)
        {
            PacketCount = 4,
        };
        source.Packets.Add(NewTcpPacket(firstFrame, firstSequence));
        source.Packets.Add(NewTcpPacket(interruptedFrame[..8], interruptedSequence));
        source.Packets.Add(NewTcpPacket(recoveredFrame[..6], recoveredSequence));
        source.Packets.Add(NewTcpPacket(recoveredFrame[6..], recoveredSequence + 6));
        var tools = new CaptureTools(manager);
        CaptureSession session = await manager.CreateAndStartAsync(
            "synthetic",
            _ => source,
            new CaptureStartRequest(InterfaceName: "lo"),
            CancellationToken.None);
        await session.StopAsync(CancellationToken.None);

        string json = await tools.GetCapture(
            session.Id,
            "json",
            maxPdus: 10,
            cancellationToken: CancellationToken.None);
        CaptureSummary summary = await tools.SummarizeCapture(
            session.Id,
            cancellationToken: CancellationToken.None);

        await Assert.That(json).Contains("\"CallId\": 301");
        await Assert.That(json).Contains("\"CallId\": 303");
        await Assert.That(json).DoesNotContain("\"CallId\": 302");
        await Assert.That(summary.PduCount).IsEqualTo(2);
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
    public async Task CaptureTools_DecodePdu_AcceptsOwnedNtlmKeyWithoutEchoingIt()
    {
        await using CaptureSessionManager manager = CreateManager();
        var tools = new CaptureTools(manager);
        const string key = "00112233445566778899AABBCCDDEEFF";

        string decoded = tools.DecodePdu(Convert.ToHexString(EncodeRequest()), key);

        await Assert.That(decoded).Contains("\"PduType\": \"request\"");
        await Assert.That(decoded).DoesNotContain(key);
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

    [Test]
    public async Task CaptureNotificationParams_SerializesOnlyLightweightIndexStateAndDropMetadata()
    {
        const string secret = "00112233445566778899AABBCCDDEEFF";
        var notification = new CaptureNotification(
            "capture-notification",
            "subscription-notification",
            "client-notification",
            CaptureSessionState.Running,
            FirstIndex: 4,
            NextIndex: 7,
            TotalEmitted: 9,
            Done: false,
            CursorDroppedRanges: [new CaptureDropRange(1, 3)],
            NotificationDropCount: 2,
            RecoveryFromIndex: 1,
            RecoveryToIndex: 6);

        CaptureNotificationParams parameters = CaptureNotificationParams.From(notification);
        string json = JsonSerializer.Serialize(parameters);
        string notificationMethod = McpCaptureNotificationPublisher.NotificationMethod;

        await Assert.That(notificationMethod)
            .IsEqualTo("notifications/opcclassic/capture");
        await Assert.That(json).Contains("\"SessionId\":\"capture-notification\"");
        await Assert.That(json).Contains("\"NextIndex\":7");
        await Assert.That(json).Contains("\"NotificationDropCount\":2");
        await Assert.That(json).DoesNotContain("Pdus");
        await Assert.That(json).DoesNotContain("Data");
        await Assert.That(json).DoesNotContain("Raw");
        await Assert.That(json).DoesNotContain("Packet");
        await Assert.That(json).DoesNotContain("NtlmSessionKey");
        await Assert.That(json).DoesNotContain(secret);
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

    private static CapturedPacket NewAnnotatedPacket(string tag)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(tag);
        return new CapturedPacket(
            DateTimeOffset.UtcNow,
            data.Length,
            data,
            LinkType: 0,
            Annotations: new Dictionary<string, string?>
            {
                ["iid"] = "11111111-2222-3333-4444-555555555555",
                ["opnum"] = "5",
                ["direction"] = "request",
            });
    }

    private static byte[] EncodeRequest(int callId = 21)
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(new byte[] { 0x10, 0x20 }, Guid.Empty);
        return PduCodec.EncodePdu(
            new RequestCoPdu
            {
                AllocationHint = stub.Length,
                ContextId = 0,
                Opnum = 7,
                Stub = stub,
                CallId = callId,
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

    private static CapturedPacket NewTcpPacket(byte[] payload, uint sequenceNumber)
    {
        byte[] frame = new byte[14 + 20 + 20 + payload.Length];
        frame[12] = 0x08;
        frame[13] = 0x00;
        int ipOffset = 14;
        frame[ipOffset] = 0x45;
        BinaryPrimitives.WriteUInt16BigEndian(
            frame.AsSpan(ipOffset + 2, 2),
            (ushort)(20 + 20 + payload.Length));
        frame[ipOffset + 8] = 64;
        frame[ipOffset + 9] = 6;
        frame[ipOffset + 12] = 10;
        frame[ipOffset + 15] = 1;
        frame[ipOffset + 16] = 10;
        frame[ipOffset + 19] = 2;
        int tcpOffset = ipOffset + 20;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset, 2), 50_000);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 2, 2), 135);
        BinaryPrimitives.WriteUInt32BigEndian(frame.AsSpan(tcpOffset + 4, 4), sequenceNumber);
        frame[tcpOffset + 12] = 0x50;
        frame[tcpOffset + 13] = 0x18;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 14, 2), 8192);
        payload.CopyTo(frame.AsSpan(tcpOffset + 20));
        return new CapturedPacket(
            DateTimeOffset.UtcNow,
            frame.Length,
            frame,
            LinkType: 1,
            Annotations: new Dictionary<string, string?>());
    }

    private static Opc.Classic.Dcom.Activation.RemoteActivationResponse CreateActivationResponse() =>
        new(
            Hresult: 0,
            Oxid: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            IpidRemUnknown: Guid.Parse("11111111-2222-3333-4444-555555555555"),
            AuthnHint: 5,
            ServerVersion: (5, 7),
            InterfaceResults: []);

    private sealed class SyntheticCaptureSource : ICaptureSource, ICaptureFilterController
    {
        private readonly string? _rawPcapPath;

        public SyntheticCaptureSource(string? rawPcapPath) => _rawPcapPath = rawPcapPath;

        public List<CapturedPacket> Packets { get; } = [];
        public long PacketCount { get; init; }
        public long ByteCount { get; init; }
        public int LinkType => 0;
        public CaptureStartRequest? StartRequest { get; private set; }
        public string? EffectiveFilter { get; private set; }

        public Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartRequest = request;
            EffectiveFilter = string.IsNullOrWhiteSpace(request.BpfFilter)
                ? PcapCaptureSource.BuildServerPortBpfFilter(request.ServerPorts)
                : request.BpfFilter;
            return Task.CompletedTask;
        }

        public CaptureSourceFilterUpdateResult TryUpdateFilter(
            string filter,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EffectiveFilter = filter;
            return CaptureSourceFilterUpdateResult.Updated;
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
            long remaining = maxPackets ?? long.MaxValue;
            foreach (CapturedPacket packet in Packets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (remaining-- <= 0)
                {
                    yield break;
                }
                yield return packet;
                await Task.Yield();
            }
        }

        public string? GetRawPcapFilePath() => _rawPcapPath;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class ObservingTargetResolver : ICaptureTargetResolver
    {
        private readonly Func<bool> _isCaptureRunning;
        private readonly CaptureTargetMetadata _result;

        public ObservingTargetResolver(Func<bool> isCaptureRunning, CaptureTargetMetadata result)
        {
            _isCaptureRunning = isCaptureRunning;
            _result = result;
        }

        public bool CaptureWasRunning { get; private set; }
        public bool AmbientSso { get; private set; }

        public Task<CaptureTargetMetadata> ResolveAsync(
            string? targetHost,
            string? progId,
            string? clsid,
            string? connectionString,
            CancellationToken cancellationToken,
            bool ambientSso = false)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CaptureWasRunning = _isCaptureRunning();
            AmbientSso = ambientSso;
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeActivationSessionFactory : ICaptureTargetActivationSessionFactory
    {
        private readonly Opc.Classic.Dcom.Activation.RemoteActivationResponse _response;
        private readonly Exception? _activationException;
        private readonly Exception? _releaseException;

        public FakeActivationSessionFactory(
            Opc.Classic.Dcom.Activation.RemoteActivationResponse response,
            Exception? activationException = null,
            Exception? releaseException = null)
        {
            _response = response;
            _activationException = activationException;
            _releaseException = releaseException;
        }

        public int CreatedCount { get; private set; }
        public int ReleaseCount { get; private set; }
        public int DisposeCount { get; private set; }

        public Task<ICaptureTargetActivationSession> CreateAsync(
            OpcMcpDcomConnectionRequest request,
            Guid clsid,
            string opcScheme,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreatedCount++;
            return Task.FromResult<ICaptureTargetActivationSession>(
                new FakeActivationSession(this, _response, _activationException, _releaseException));
        }

        private sealed class FakeActivationSession : ICaptureTargetActivationSession
        {
            private readonly FakeActivationSessionFactory _owner;
            private readonly Opc.Classic.Dcom.Activation.RemoteActivationResponse _response;
            private readonly Exception? _activationException;
            private readonly Exception? _releaseException;

            public FakeActivationSession(
                FakeActivationSessionFactory owner,
                Opc.Classic.Dcom.Activation.RemoteActivationResponse response,
                Exception? activationException,
                Exception? releaseException)
            {
                _owner = owner;
                _response = response;
                _activationException = activationException;
                _releaseException = releaseException;
            }

            public Task<Opc.Classic.Dcom.Activation.RemoteActivationResponse> ActivateAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _activationException is null
                    ? Task.FromResult(_response)
                    : Task.FromException<Opc.Classic.Dcom.Activation.RemoteActivationResponse>(_activationException);
            }

            public Task ReleaseAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _owner.ReleaseCount++;
                return _releaseException is null
                    ? Task.CompletedTask
                    : Task.FromException(_releaseException);
            }

            public ValueTask DisposeAsync()
            {
                _owner.DisposeCount++;
                return ValueTask.CompletedTask;
            }
        }
    }
}
