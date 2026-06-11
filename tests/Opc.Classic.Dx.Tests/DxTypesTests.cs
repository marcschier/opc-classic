//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dx.Tests;

public sealed class DxConnectionTests
{
    [Test]
    public async Task RecordEquality_MatchesEquivalentConnection()
    {
        var connection = new DxConnection(
            "Tank1_to_HMI",
            description: "Tank level mirror",
            browsePaths: new[] { "Area1", "Area1/Tank1" },
            sourceServerName: "PLC1",
            sourceItemName: "PLC1.Tank1.Level",
            targetItemName: "HMI.Tank1.Level",
            updateRateMilliseconds: 1000,
            deadbandPercent: 0.5f);

        var copy = connection with { };

        await Assert.That(copy).IsEqualTo(connection);
    }

    [Test]
    public async Task Constructor_NegativeUpdateRate_Throws()
    {
        await Assert.That(() => new DxConnection("BadRate", updateRateMilliseconds: -1))
            .Throws<ArgumentOutOfRangeException>();
    }
}

public sealed class DxGeneralResponseTests
{
    [Test]
    public async Task Constructor_WithNoErrors_UsesEmptyErrorsArray()
    {
        var response = new DxGeneralResponse("cfg-1");
        var errorCount = response.Errors.Length;

        await Assert.That(errorCount).IsEqualTo(0);
        await Assert.That(response.ConfigurationVersion).IsEqualTo("cfg-1");
    }
}

public sealed class DxSourceServerTests
{
    [Test]
    public async Task RecordRoundTrip_PreservesSourceServerFields()
    {
        var source = new DxSourceServer(
            "PLC1",
            "opcda://plc1.plant1/Vendor.OPC.1",
            description: "Main process PLC",
            serverType: "OPC DA",
            defaultConnected: true);

        var roundTripped = source with { };

        await Assert.That(roundTripped).IsEqualTo(source);
        await Assert.That(roundTripped.DefaultConnected).IsEqualTo(true);
    }
}

public sealed class DxQueryParametersTests
{
    [Test]
    public async Task Constructor_AssignsBrowseQueryFields()
    {
        var query = new DxQueryParameters(
            new DxBrowsePath("Area1/Tank1"),
            recursive: true,
            browseFilter: DxBrowseFilter.Connections,
            itemQuery: new DxItemQuery(ItemName: "Tank1"));

        await Assert.That(query.BrowsePath?.Path).IsEqualTo("Area1/Tank1");
        await Assert.That(query.Recursive).IsTrue();
        await Assert.That(query.BrowseFilter).IsEqualTo(DxBrowseFilter.Connections);
        await Assert.That(query.ItemQuery?.ItemName).IsEqualTo("Tank1");
    }
}
