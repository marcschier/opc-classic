//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Da.Dcom;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Subscriptions;

public sealed class OpcDaSubscriptionContractTests
{
    [Test]
    public async Task OpcDaDataChange_RecordEquality_MatchesPayload()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        IReadOnlyList<OpcDaDataChangeItem> items = new[]
        {
            new OpcDaDataChangeItem(10, OpcVariant.FromInt32(42), OpcQuality.Good, timestamp, 0),
        };

        var expected = new OpcDaDataChange(1, 2, 3, 4, items);
        var actual = new OpcDaDataChange(1, 2, 3, 4, items);

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task OpcDaDataChange_CtorThrows_WhenItemsNull()
    {
        await Assert.That(() => { _ = new OpcDaDataChange(1, 2, 3, 4, null!); })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task OpcDaDataChangeItem_RecordEquality_MatchesPayload()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var expected = new OpcDaDataChangeItem(10, OpcVariant.FromString("running"), OpcQuality.Good, timestamp, 0);
        var actual = expected with { };

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task IConnectionPointContainer_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIConnectionPointContainerInterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIConnectionPointContainerInterfaceId()).IsEqualTo(ReadExpectedIConnectionPointContainerInterfaceId());
    }

    [Test]
    public async Task IConnectionPoint_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIConnectionPointInterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIConnectionPointInterfaceId()).IsEqualTo(ReadExpectedIConnectionPointInterfaceId());
    }

    [Test]
    public async Task IOPCDataCallback_InterfaceId_IsEmitted()
    {
        await Assert.That(ReadIOPCDataCallbackInterfaceId()).IsNotEqualTo(ReadEmptyGuid());
        await Assert.That(ReadIOPCDataCallbackInterfaceId()).IsEqualTo(ReadExpectedIOPCDataCallbackInterfaceId());
    }

    // TUnitAssertions0005 workaround: use non-const indirections for IID assertions.
    private static Guid ReadEmptyGuid() => Guid.Empty;
    private static Guid ReadIConnectionPointContainerInterfaceId() => IConnectionPointContainer.InterfaceId;
    private static Guid ReadIConnectionPointInterfaceId() => IConnectionPoint.InterfaceId;
    private static Guid ReadIOPCDataCallbackInterfaceId() => IOPCDataCallback.InterfaceId;
    private static Guid ReadExpectedIConnectionPointContainerInterfaceId() => OpcGuids.IID_IConnectionPointContainer;
    private static Guid ReadExpectedIConnectionPointInterfaceId() => OpcGuids.IID_IConnectionPoint;
    private static Guid ReadExpectedIOPCDataCallbackInterfaceId() => OpcGuids.IID_IOPCDataCallback;
}
