//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class OrpcReplayToolTests {
    private static readonly Guid s_iid = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Test]
    public async Task Replay_NullPduSequence_Throws() {
        var tool = new OrpcReplayTool();

        await Assert.That(() => tool.Replay(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Replay_MixedPdus_CountsSucceededSkippedAndBuildsConcreteKeys() {
        var tool = new OrpcReplayTool();
        DecodedOpcPdu[] pdus =
        [
            NewPdu("request", requestStubLength: 8, opnum: 3, interfaceId: s_iid),
            NewPdu("request", requestStubLength: 4, opnum: 3, interfaceId: s_iid),
            NewPdu("response", responseStubLength: 0, opnum: 3, interfaceId: s_iid),
            NewPdu("orpc_body", responseStubLength: 2, opnum: null, interfaceId: null),
            NewPdu("bind", requestStubLength: 99, opnum: 99, interfaceId: s_iid),
        ];

        ReplayReport report = tool.Replay(pdus);

        await Assert.That(report.TotalSucceeded).IsEqualTo(3);
        await Assert.That(report.TotalFailed).IsEqualTo(0);
        await Assert.That(report.TotalSkipped).IsEqualTo(1);
        await Assert.That(report.Buckets.Count).IsEqualTo(3);
        await Assert.That(report.Buckets[0].Key).IsEqualTo($"{s_iid:D}/op3/request");
        await Assert.That(report.Buckets[0].SucceededCount).IsEqualTo(2);
        await Assert.That(report.Buckets[0].SkippedCount).IsEqualTo(0);
        await Assert.That(report.Buckets[1].Key).IsEqualTo("<unbound>/op-/orpc_body");
        await Assert.That(report.Buckets[1].SucceededCount).IsEqualTo(1);
        await Assert.That(report.Buckets[2].Key).IsEqualTo($"{s_iid:D}/op3/response");
        await Assert.That(report.Buckets[2].SkippedCount).IsEqualTo(1);
    }

    [Test]
    public async Task TryReplayBody_EmptyAndNonEmptyBodies_ReturnExpectedResults() {
        var tool = new OrpcReplayTool();

        bool emptyOk = tool.TryReplayBody(ReadOnlyMemory<byte>.Empty, out string? emptyError);
        bool nonEmptyOk = tool.TryReplayBody(new byte[] { 0x01, 0x02, 0x03, 0x04 }, out string? nonEmptyError);

        await Assert.That(emptyOk).IsFalse();
        await Assert.That(emptyError).IsEqualTo("Empty payload");
        await Assert.That(nonEmptyOk).IsTrue();
        await Assert.That(nonEmptyError).IsNull();
    }

    [Test]
    public async Task ReplayKeyStats_DefaultsAndMutableCounters_Work() {
        var stats = new ReplayKeyStats("iid/op1/request") {
            SucceededCount = 2,
            FailedCount = 1,
            SkippedCount = 3,
            FirstFailureMessage = "bad ndr",
        };

        await Assert.That(stats.Key).IsEqualTo("iid/op1/request");
        await Assert.That(stats.SucceededCount).IsEqualTo(2);
        await Assert.That(stats.FailedCount).IsEqualTo(1);
        await Assert.That(stats.SkippedCount).IsEqualTo(3);
        await Assert.That(stats.FirstFailureMessage).IsEqualTo("bad ndr");
    }

    private static DecodedOpcPdu NewPdu(
        string pduType,
        int? requestStubLength = null,
        int? responseStubLength = null,
        int? opnum = null,
        Guid? interfaceId = null)
        => new() {
            Timestamp = DateTimeOffset.UnixEpoch,
            PduType = pduType,
            RequestStubLength = requestStubLength,
            ResponseStubLength = responseStubLength,
            Opnum = opnum,
            InterfaceId = interfaceId,
        };
}
