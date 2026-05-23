//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Batch.Dcom;
using TUnit.Core;

namespace Opc.Classic.Batch.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCBatchServer_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIOPCBatchServerInterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIOPCBatchServerInterfaceId()).IsEqualTo(ReadExpectedIOPCBatchServerInterfaceId());
    }

    [Test]
    public async Task IOPCBatchServer2_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIOPCBatchServer2InterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIOPCBatchServer2InterfaceId()).IsEqualTo(ReadExpectedIOPCBatchServer2InterfaceId());
    }

    [Test]
    public async Task IEnumOPCBatchSummary_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIEnumOPCBatchSummaryInterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIEnumOPCBatchSummaryInterfaceId()).IsEqualTo(ReadExpectedIEnumOPCBatchSummaryInterfaceId());
    }

    [Test]
    public async Task IOPCEnumerationSets_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIOPCEnumerationSetsInterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIOPCEnumerationSetsInterfaceId()).IsEqualTo(ReadExpectedIOPCEnumerationSetsInterfaceId());
    }

    // TUnitAssertions0005 workaround: use non-const indirections for IID assertions.
    private static Guid ReadEmptyGuid() => Guid.Empty;
    private static Guid ReadIOPCBatchServerInterfaceId() => IOPCBatchServer.InterfaceId;
    private static Guid ReadIOPCBatchServer2InterfaceId() => IOPCBatchServer2.InterfaceId;
    private static Guid ReadIEnumOPCBatchSummaryInterfaceId() => IEnumOPCBatchSummary.InterfaceId;
    private static Guid ReadIOPCEnumerationSetsInterfaceId() => IOPCEnumerationSets.InterfaceId;
    private static Guid ReadExpectedIOPCBatchServerInterfaceId() => OpcGuids.IID_IOPCBatchServer;
    private static Guid ReadExpectedIOPCBatchServer2InterfaceId() => OpcGuids.IID_IOPCBatchServer2;
    private static Guid ReadExpectedIEnumOPCBatchSummaryInterfaceId() => OpcGuids.IID_IEnumOPCBatchSummary;
    private static Guid ReadExpectedIOPCEnumerationSetsInterfaceId() => OpcGuids.IID_IOPCEnumerationSets;
}
