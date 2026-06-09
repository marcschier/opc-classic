//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Da;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class BrowseFiltersTests {
    private static int ValueOf(BrowseFilters f) => (int)f;

    [Test]
    public async Task FilterValues_MatchOpcDaWireValues() {
        // OPC DA 3.0 OPCBROWSEFILTER enum.
        await Assert.That(ValueOf(BrowseFilters.All)).IsEqualTo(1);
        await Assert.That(ValueOf(BrowseFilters.Branch)).IsEqualTo(2);
        await Assert.That(ValueOf(BrowseFilters.Leaf)).IsEqualTo(3);
    }
}

public sealed class BrowseElementTests {
    [Test]
    public async Task Default_HasEmptyNameAndItemName() {
        var e = new BrowseElement();
        await Assert.That(e.Name).IsEqualTo(string.Empty);
        await Assert.That(e.ItemName).IsEqualTo(string.Empty);
        await Assert.That(e.IsItem).IsFalse();
        await Assert.That(e.HasChildren).IsFalse();
        await Assert.That(e.Properties.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ItemAndBranch_BothFlagsHonoredByToString() {
        var e = new BrowseElement {
            Name = "Tank",
            ItemName = "Plant.Tank",
            IsItem = true,
            HasChildren = true,
        };
        await Assert.That(e.ToString()).Contains("item+branch");
    }

    [Test]
    public async Task LeafOnly_ToStringHasItemTag() {
        var e = new BrowseElement { Name = "Temp", IsItem = true, HasChildren = false };
        await Assert.That(e.ToString()).Contains("item");
        await Assert.That(e.ToString()).DoesNotContain("branch");
    }

    [Test]
    public async Task BranchOnly_ToStringHasBranchTag() {
        var e = new BrowseElement { Name = "Plant", IsItem = false, HasChildren = true };
        await Assert.That(e.ToString()).Contains("branch");
        await Assert.That(e.ToString()).DoesNotContain("item+");
    }
}

public sealed class BrowsePositionTests {
    [Test]
    public async Task Completed_IsTerminal() {
        await Assert.That(BrowsePosition.Completed.IsCompleted).IsTrue();
        await Assert.That(BrowsePosition.Completed.IsTerminal).IsTrue();
    }

    [Test]
    public async Task EmptyContinuationPoint_IsTerminalEvenWithoutCompletedFlag() {
        var p = new BrowsePosition { ContinuationPoint = string.Empty };
        await Assert.That(p.IsTerminal).IsTrue();
    }

    [Test]
    public async Task NonEmptyContinuationPoint_NotTerminal() {
        var p = new BrowsePosition { ContinuationPoint = "next-page-token" };
        await Assert.That(p.IsTerminal).IsFalse();
        await Assert.That(p.IsCompleted).IsFalse();
        await Assert.That(p.ContinuationPoint).IsEqualTo("next-page-token");
    }
}

public sealed class PropertyIdTests {
    [Test]
    public async Task MandatoryProperties_HaveExpectedCodes_1Through8() {
        await Assert.That(PropertyID.DataType.Code).IsEqualTo(1);
        await Assert.That(PropertyID.Value.Code).IsEqualTo(2);
        await Assert.That(PropertyID.Quality.Code).IsEqualTo(3);
        await Assert.That(PropertyID.Timestamp.Code).IsEqualTo(4);
        await Assert.That(PropertyID.AccessRights.Code).IsEqualTo(5);
        await Assert.That(PropertyID.ScanRate.Code).IsEqualTo(6);
        await Assert.That(PropertyID.EuType.Code).IsEqualTo(7);
        await Assert.That(PropertyID.EuInfo.Code).IsEqualTo(8);
    }

    [Test]
    public async Task RecommendedProperties_HaveExpectedCodes_100Plus() {
        await Assert.That(PropertyID.EuUnits.Code).IsEqualTo(100);
        await Assert.That(PropertyID.Description.Code).IsEqualTo(101);
        await Assert.That(PropertyID.HighEu.Code).IsEqualTo(102);
        await Assert.That(PropertyID.LowEu.Code).IsEqualTo(103);
        await Assert.That(PropertyID.HighInstrumentRange.Code).IsEqualTo(104);
        await Assert.That(PropertyID.LowInstrumentRange.Code).IsEqualTo(105);
        await Assert.That(PropertyID.TimeZone.Code).IsEqualTo(108);
    }

    [Test]
    public async Task RecordEquality_IsByCode() {
        var a = new PropertyID(2);
        var b = new PropertyID(2);
        var c = new PropertyID(3);

        // Equality includes Name; two PropertyIDs with same code but different names are NOT equal.
        await Assert.That(a == b).IsTrue();
        await Assert.That(a == c).IsFalse();
    }

    [Test]
    public async Task ToString_FormatsCodeAndName() {
        await Assert.That(PropertyID.Value.ToString()).IsEqualTo("2: Item Value");
        await Assert.That(new PropertyID(9999).ToString()).IsEqualTo("9999");
    }
}

public sealed class ItemPropertyTests {
    [Test]
    public async Task Default_ResultIdIsOk() {
        var p = new ItemProperty();
        await Assert.That(p.ResultId).IsEqualTo(OpcResultId.Ok);
        await Assert.That(p.Description).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Initializer_AssignsAllFields() {
        var p = new ItemProperty {
            PropertyId = PropertyID.EuUnits,
            Description = "Engineering units",
            DataType = typeof(string),
            Value = "DEGC",
            ResultId = OpcResultId.Ok,
            ItemName = "Tank.Temperature.EuUnits",
        };
        await Assert.That(p.PropertyId).IsEqualTo(PropertyID.EuUnits);
        await Assert.That(p.Description).IsEqualTo("Engineering units");
        await Assert.That(p.Value).IsEqualTo("DEGC");
        await Assert.That(p.ItemName).IsEqualTo("Tank.Temperature.EuUnits");
    }
}

public sealed class ItemPropertyResultTests {
    [Test]
    public async Task Default_HasEmptyProperties() {
        var c = new ItemPropertyResult();
        await Assert.That(c.Properties.Count).IsEqualTo(0);
        await Assert.That(c.ResultId).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    public async Task Initializer_AssignsAllFields() {
        var c = new ItemPropertyResult {
            ItemName = "Tank.Temp",
            ItemPath = "Plant1",
            ResultId = OpcResultId.Ok,
            Properties = new[]
            {
                new ItemProperty { PropertyId = PropertyID.Value, Value = 42.0 },
                new ItemProperty { PropertyId = PropertyID.Quality, Value = (ushort)0xC0 },
            },
        };
        await Assert.That(c.ItemName).IsEqualTo("Tank.Temp");
        await Assert.That(c.ItemPath).IsEqualTo("Plant1");
        await Assert.That(c.Properties.Count).IsEqualTo(2);
    }
}
