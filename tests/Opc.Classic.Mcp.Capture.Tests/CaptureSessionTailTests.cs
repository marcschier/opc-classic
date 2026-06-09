//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

/// <summary>
/// Coverage for the cursor-based live-tail path on <see cref="CaptureSession"/>.
/// <see cref="CaptureSession.DrainTailAsync"/> is <c>internal</c>; tests call
/// it via reflection through the existing
/// <c>InternalsVisibleTo("Opc.Classic.Mcp.Capture.Tests")</c> attribute on
/// Opc.Classic.Mcp.Capture.
/// </summary>
public sealed class CaptureSessionTailTests {
    private static readonly Guid InterfaceA = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Test]
    public async Task DrainTailAsync_FirstCallWithCursor0_ReturnsAllPdusEmittedSoFar() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a"), HexResponse("b") });

        DrainTailResult result = await harness.DrainAsync(sinceIndex: 0, max: 100);

        await Assert.That(result.Pdus.Count).IsEqualTo(2);
        await Assert.That(result.NextIndex).IsEqualTo(2);
        await Assert.That(result.TotalEmitted).IsEqualTo(2);
        await Assert.That(result.SessionState).IsEqualTo(CaptureSessionState.Running);
        await Assert.That(result.Done).IsFalse();
    }

    [Test]
    public async Task DrainTailAsync_SecondCallWithReturnedCursor_ReturnsZeroNewPdus() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a"), HexResponse("b") });

        DrainTailResult first = await harness.DrainAsync(sinceIndex: 0, max: 100);
        DrainTailResult second = await harness.DrainAsync(sinceIndex: first.NextIndex, max: 100);

        await Assert.That(second.Pdus.Count).IsEqualTo(0);
        await Assert.That(second.NextIndex).IsEqualTo(first.NextIndex);
        await Assert.That(second.TotalEmitted).IsEqualTo(first.TotalEmitted);
        await Assert.That(second.Done).IsFalse();
    }

    [Test]
    public async Task DrainTailAsync_PacketsAppendedBetweenCalls_AreReturnedOnSecondCall() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a"), HexResponse("b") });

        DrainTailResult first = await harness.DrainAsync(sinceIndex: 0, max: 100);

        harness.Source.Packets.Add(HexRequest("c"));
        harness.Source.Packets.Add(HexResponse("d"));

        DrainTailResult second = await harness.DrainAsync(sinceIndex: first.NextIndex, max: 100);

        await Assert.That(second.Pdus.Count).IsEqualTo(2);
        await Assert.That(second.NextIndex).IsEqualTo(4);
        await Assert.That(second.TotalEmitted).IsEqualTo(4);
        // The 3rd and 4th packets get decoded; their annotations roundtrip back.
        await Assert.That(second.Pdus[0].PduType).IsEqualTo("orpc_body");
        await Assert.That(second.Pdus[1].PduType).IsEqualTo("orpc_body");
    }

    [Test]
    public async Task DrainTailAsync_MaxLimitsReturnedWindow_AndPreservesCursorAdvance() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[]
            {
                HexRequest("a"), HexResponse("b"),
                HexRequest("c"), HexResponse("d"),
                HexRequest("e"),
            });

        DrainTailResult first = await harness.DrainAsync(sinceIndex: 0, max: 2);

        await Assert.That(first.Pdus.Count).IsEqualTo(2);
        await Assert.That(first.NextIndex).IsEqualTo(2);
        await Assert.That(first.TotalEmitted).IsEqualTo(5);
        // Subsequent calls walk the rest of the cache without re-decoding.
        DrainTailResult second = await harness.DrainAsync(sinceIndex: first.NextIndex, max: 2);
        await Assert.That(second.Pdus.Count).IsEqualTo(2);
        await Assert.That(second.NextIndex).IsEqualTo(4);
        DrainTailResult third = await harness.DrainAsync(sinceIndex: second.NextIndex, max: 2);
        await Assert.That(third.Pdus.Count).IsEqualTo(1);
        await Assert.That(third.NextIndex).IsEqualTo(5);
    }

    [Test]
    public async Task DrainTailAsync_AfterSessionStopped_ReportsDoneWhenCursorCaughtUp() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a"), HexResponse("b") });

        await harness.Session.StopAsync(TestContext.Current!.CancellationToken);

        DrainTailResult first = await harness.DrainAsync(sinceIndex: 0, max: 100);

        await Assert.That(first.Pdus.Count).IsEqualTo(2);
        await Assert.That(first.SessionState).IsEqualTo(CaptureSessionState.Completed);
        await Assert.That(first.Done).IsTrue();

        DrainTailResult second = await harness.DrainAsync(sinceIndex: first.NextIndex, max: 100);
        await Assert.That(second.Pdus.Count).IsEqualTo(0);
        await Assert.That(second.Done).IsTrue();
    }

    [Test]
    public async Task DrainTailAsync_NegativeSinceIndex_IsClampedToZero() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a") });

        DrainTailResult result = await harness.DrainAsync(sinceIndex: -42, max: 100);

        await Assert.That(result.Pdus.Count).IsEqualTo(1);
        await Assert.That(result.NextIndex).IsEqualTo(1);
    }

    [Test]
    public async Task DrainTailAsync_SinceIndexBeyondCache_ReturnsEmptyWithoutAdvance() {
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a") });

        DrainTailResult result = await harness.DrainAsync(sinceIndex: 99, max: 100);

        await Assert.That(result.Pdus.Count).IsEqualTo(0);
        await Assert.That(result.NextIndex).IsEqualTo(1);
        await Assert.That(result.TotalEmitted).IsEqualTo(1);
    }

    [Test]
    public async Task DrainTailAsync_PreservesPerFlowDecoderStateAcrossPolls() {
        // First poll consumes 1 packet; second poll consumes the next 1; if the
        // long-lived OpcDcomDecoder is preserved, both PDUs decode cleanly. We
        // can't easily prove "preserved" against the public DecodedOpcPdu shape
        // for hex-source packets (which are stateless), but we can prove that
        // re-feeding the already-consumed packets doesn't duplicate output.
        await using TailHarness harness = await TailHarness.StartAsync(
            new[] { HexRequest("a"), HexResponse("b"), HexRequest("c") });

        // First call drains everything.
        DrainTailResult first = await harness.DrainAsync(sinceIndex: 0, max: 100);
        await Assert.That(first.TotalEmitted).IsEqualTo(3);

        // Add one more packet; second call should only emit ONE new PDU,
        // proving the decoder is not re-processing the first 3 packets.
        harness.Source.Packets.Add(HexResponse("d"));

        DrainTailResult second = await harness.DrainAsync(sinceIndex: first.NextIndex, max: 100);
        await Assert.That(second.Pdus.Count).IsEqualTo(1);
        await Assert.That(second.TotalEmitted).IsEqualTo(4);
    }

    private static CapturedPacket HexRequest(string tag) => MakeHexPacket(tag, direction: "request");

    private static CapturedPacket HexResponse(string tag) => MakeHexPacket(tag, direction: "response");

    private static CapturedPacket MakeHexPacket(string tag, string direction) {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(tag);
        return new CapturedPacket(
            Timestamp: new DateTimeOffset(2026, 6, 9, 12, 0, 0, TimeSpan.Zero),
            OriginalLength: data.Length,
            Data: data,
            LinkType: 0,
            Annotations: new Dictionary<string, string?> {
                ["iid"] = InterfaceA.ToString("D"),
                ["opnum"] = "5",
                ["direction"] = direction,
                ["tag"] = tag,
            });
    }

    private sealed class TailHarness : IAsyncDisposable {
        private readonly string _scratchFolder;

        private TailHarness(CaptureSession session, FakeCaptureSource source, string scratchFolder) {
            Session = session;
            Source = source;
            _scratchFolder = scratchFolder;
        }

        public CaptureSession Session { get; }
        public FakeCaptureSource Source { get; }

        public static async Task<TailHarness> StartAsync(IEnumerable<CapturedPacket> initialPackets) {
            string folder = TestDirectories.CreateUniqueTempDirectory();
            var source = new FakeCaptureSource();
            foreach (CapturedPacket pkt in initialPackets) {
                source.Packets.Add(pkt);
            }
            var session = new CaptureSession(
                id: "session-tail-" + Guid.NewGuid().ToString("N")[..8],
                sourceName: "fake",
                source: source,
                sessionFolder: folder,
                request: new CaptureStartRequest(InterfaceName: "lo"));
            await session.StartAsync(TestContext.Current!.CancellationToken);
            return new TailHarness(session, source, folder);
        }

        public Task<DrainTailResult> DrainAsync(long sinceIndex, int max) {
            // DrainTailAsync is internal; invoke via reflection using the
            // assembly's InternalsVisibleTo grant for this test project.
            MethodInfo method = typeof(CaptureSession).GetMethod(
                "DrainTailAsync",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("DrainTailAsync method not found.");
            try {
                return (Task<DrainTailResult>)method.Invoke(Session,
                    new object[] { sinceIndex, max, TestContext.Current!.CancellationToken })!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null) {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        public async ValueTask DisposeAsync() {
            await Session.DisposeAsync();
            TestDirectories.DeleteIfExists(_scratchFolder);
        }
    }
}
