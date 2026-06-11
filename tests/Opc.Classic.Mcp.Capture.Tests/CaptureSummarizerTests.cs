//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureSummarizerTests
{
    private static readonly Guid s_interfaceA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid s_interfaceB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid s_ipid = Guid.Parse("cccccccc-0000-0000-0000-000000000003");

    [Test]
    public async Task Summarize_EmptyInput_ReturnsZeroCountDurationAndEmptyBuckets()
    {
        CaptureSummary summary = CaptureSummarizer.Summarize("session-1", Array.Empty<DecodedOpcPdu>());

        await Assert.That(summary.SessionId).IsEqualTo("session-1");
        await Assert.That(summary.PduCount).IsEqualTo(0);
        await Assert.That(summary.DurationSeconds).IsEqualTo(0.0);
        await Assert.That(summary.TopPduTypes.Count).IsEqualTo(0);
        await Assert.That(summary.TopSources.Count).IsEqualTo(0);
        await Assert.That(summary.TopDestinations.Count).IsEqualTo(0);
        await Assert.That(summary.TopInterfaces.Count).IsEqualTo(0);
        await Assert.That(summary.TopOpnums.Count).IsEqualTo(0);
        await Assert.That(summary.TopIpids.Count).IsEqualTo(0);
        await Assert.That(summary.TopFaultCodes.Count).IsEqualTo(0);
        await Assert.That(summary.TopBindRejectReasons.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Summarize_Pdus_CountsAndOrdersTopBucketsWithConcreteFormats()
    {
        var start = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
        DecodedOpcPdu[] pdus =
        [
            NewPdu(start.AddSeconds(10), "request", "client-a:50000", "server:135", s_interfaceA, opnum: 3, objectIpid: s_ipid),
            NewPdu(start.AddSeconds(4), "request", "client-b:50001", "server:135", s_interfaceA, opnum: 3),
            NewPdu(start.AddSeconds(14), "request", "client-a:50000", "server:135", s_interfaceB, opnum: 1),
            NewPdu(start.AddSeconds(1), "fault", "server:135", "client-a:50000", s_interfaceB, faultStatus: unchecked((int)0x80004005)),
            NewPdu(start.AddSeconds(6), "bind_ack", resultList:
            [
                new PresentationResultInfo("ACCEPTANCE", "REASON_NOT_SPECIFIED"),
                new PresentationResultInfo("USER_REJECTION", "ABSTRACT_SYNTAX_NOT_SUPPORTED"),
            ]),
            NewPdu(start.AddSeconds(3), "bind_ack", resultList:
            [
                new PresentationResultInfo("PROVIDER_REJECTION", "LOCAL_LIMIT_EXCEEDED"),
            ]),
        ];

        CaptureSummary summary = CaptureSummarizer.Summarize("session-2", pdus, top: 2);

        await Assert.That(summary.PduCount).IsEqualTo(6);
        await Assert.That(summary.DurationSeconds).IsEqualTo(13.0);
        await Assert.That(summary.TopPduTypes.Count).IsEqualTo(2);
        await Assert.That(summary.TopPduTypes[0]).IsEqualTo(new TopEntry("request", 3));
        await Assert.That(summary.TopPduTypes[1]).IsEqualTo(new TopEntry("bind_ack", 2));
        await Assert.That(summary.TopSources[0]).IsEqualTo(new TopEntry("client-a:50000", 2));
        await Assert.That(summary.TopSources[1]).IsEqualTo(new TopEntry("client-b:50001", 1));
        await Assert.That(summary.TopDestinations[0]).IsEqualTo(new TopEntry("server:135", 3));
        await Assert.That(summary.TopInterfaces[0]).IsEqualTo(new TopEntry(s_interfaceA.ToString("D"), 2));
        await Assert.That(summary.TopInterfaces[1]).IsEqualTo(new TopEntry(s_interfaceB.ToString("D"), 2));
        await Assert.That(summary.TopOpnums[0]).IsEqualTo(new TopEntry($"{s_interfaceA:D}/op3", 2));
        await Assert.That(summary.TopOpnums[1]).IsEqualTo(new TopEntry($"{s_interfaceB:D}/op1", 1));
        await Assert.That(summary.TopIpids[0]).IsEqualTo(new TopEntry(s_ipid.ToString("D"), 1));
        await Assert.That(summary.TopFaultCodes[0]).IsEqualTo(new TopEntry("0x80004005", 1));
        await Assert.That(summary.TopBindRejectReasons.Count).IsEqualTo(2);
        await Assert.That(summary.TopBindRejectReasons[0]).IsEqualTo(new TopEntry("PROVIDER_REJECTION;LOCAL_LIMIT_EXCEEDED", 1));
        await Assert.That(summary.TopBindRejectReasons[1]).IsEqualTo(new TopEntry("USER_REJECTION;ABSTRACT_SYNTAX_NOT_SUPPORTED", 1));
    }

    [Test]
    public async Task Summarize_TiesOrderByKeyOrdinalAscending()
    {
        DecodedOpcPdu[] pdus =
        [
            NewPdu(DateTimeOffset.UnixEpoch, "zeta"),
            NewPdu(DateTimeOffset.UnixEpoch.AddSeconds(1), "alpha"),
            NewPdu(DateTimeOffset.UnixEpoch.AddSeconds(2), "middle"),
        ];

        CaptureSummary summary = CaptureSummarizer.Summarize("tie-session", pdus);

        await Assert.That(summary.TopPduTypes[0]).IsEqualTo(new TopEntry("alpha", 1));
        await Assert.That(summary.TopPduTypes[1]).IsEqualTo(new TopEntry("middle", 1));
        await Assert.That(summary.TopPduTypes[2]).IsEqualTo(new TopEntry("zeta", 1));
    }

    [Test]
    public async Task Summarize_NullOrInvalidArguments_Throw()
    {
        DecodedOpcPdu[] pdus = [NewPdu(DateTimeOffset.UnixEpoch, "request")];

        await Assert.That(() => CaptureSummarizer.Summarize(null!, pdus)).Throws<ArgumentNullException>();
        await Assert.That(() => CaptureSummarizer.Summarize(string.Empty, pdus)).Throws<ArgumentException>();
        await Assert.That(() => CaptureSummarizer.Summarize("session", null!)).Throws<ArgumentNullException>();
        await Assert.That(() => CaptureSummarizer.Summarize("session", pdus, top: 0)).Throws<ArgumentOutOfRangeException>();
    }

    private static DecodedOpcPdu NewPdu(
        DateTimeOffset timestamp,
        string pduType,
        string? source = null,
        string? destination = null,
        Guid? interfaceId = null,
        int? opnum = null,
        Guid? objectIpid = null,
        int? faultStatus = null,
        IReadOnlyList<PresentationResultInfo>? resultList = null)
        => new()
        {
            Timestamp = timestamp,
            PduType = pduType,
            SourceEndpoint = source,
            DestinationEndpoint = destination,
            InterfaceId = interfaceId,
            Opnum = opnum,
            ObjectIpid = objectIpid,
            FaultStatus = faultStatus,
            ResultList = resultList ?? Array.Empty<PresentationResultInfo>(),
        };
}
