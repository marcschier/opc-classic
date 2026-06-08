//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class DecodedOpcPduTests
{
    [Test]
    public async Task Constructor_DefaultOptionalProperties_AreUnsetOrEmpty()
    {
        var timestamp = new DateTimeOffset(2026, 6, 7, 13, 30, 0, TimeSpan.Zero);
        var pdu = new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = "bind",
        };

        await Assert.That(pdu.Timestamp).IsEqualTo(timestamp);
        await Assert.That(pdu.PduType).IsEqualTo("bind");
        await Assert.That(pdu.SourceEndpoint).IsNull();
        await Assert.That(pdu.DestinationEndpoint).IsNull();
        await Assert.That(pdu.CallId).IsEqualTo(-1);
        await Assert.That(pdu.ContextId).IsNull();
        await Assert.That(pdu.Opnum).IsNull();
        await Assert.That(pdu.InterfaceId).IsNull();
        await Assert.That(pdu.ObjectIpid).IsNull();
        await Assert.That(pdu.Hresult).IsNull();
        await Assert.That(pdu.FaultStatus).IsNull();
        await Assert.That(pdu.RequestStubLength).IsNull();
        await Assert.That(pdu.ResponseStubLength).IsNull();
        await Assert.That(pdu.ContextList.Count).IsEqualTo(0);
        await Assert.That(pdu.ResultList.Count).IsEqualTo(0);
        await Assert.That(pdu.Annotations).IsNull();
    }

    [Test]
    public async Task WithExpression_PreservesRecordValueEqualityForConcreteMetadata()
    {
        Guid iid = Guid.Parse("11111111-2222-3333-4444-555555555555");
        IReadOnlyDictionary<string, string?> annotations = new Dictionary<string, string?>
        {
            ["direction"] = "request",
        };
        var original = new DecodedOpcPdu
        {
            Timestamp = new DateTimeOffset(2026, 6, 7, 13, 31, 0, TimeSpan.Zero),
            PduType = "request",
            CallId = 5,
            InterfaceId = iid,
            Opnum = 3,
            RequestStubLength = 12,
            Annotations = annotations,
        };

        DecodedOpcPdu changed = original with { CallId = 6, RequestStubLength = 16 };

        await Assert.That(changed.PduType).IsEqualTo("request");
        await Assert.That(changed.CallId).IsEqualTo(6);
        await Assert.That(changed.InterfaceId).IsEqualTo(iid);
        await Assert.That(changed.Opnum).IsEqualTo(3);
        await Assert.That(changed.RequestStubLength).IsEqualTo(16);
        await Assert.That(changed.Annotations).IsEqualTo(annotations);
        await Assert.That(original.CallId).IsEqualTo(5);
    }

    [Test]
    public async Task PresentationRecords_ValueEqualityAndProperties_Work()
    {
        Guid iid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var context = new PresentationContextInfo(2, iid, 1, 0);
        var sameContext = new PresentationContextInfo(2, iid, 1, 0);
        var result = new PresentationResultInfo("USER_REJECTION", "ABSTRACT_SYNTAX_NOT_SUPPORTED");

        await Assert.That(context).IsEqualTo(sameContext);
        await Assert.That(context.ContextId).IsEqualTo(2);
        await Assert.That(context.AbstractSyntaxIid).IsEqualTo(iid);
        await Assert.That(context.MajorVersion).IsEqualTo(1);
        await Assert.That(context.MinorVersion).IsEqualTo(0);
        await Assert.That(result.Result).IsEqualTo("USER_REJECTION");
        await Assert.That(result.Reason).IsEqualTo("ABSTRACT_SYNTAX_NOT_SUPPORTED");
    }
}
