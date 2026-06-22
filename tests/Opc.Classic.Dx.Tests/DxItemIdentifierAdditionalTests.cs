// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dx.Tests;

public sealed class DxItemIdentifierAdditionalTests
{
    [Test]
    public async Task DefaultConstructor_UsesNullStringsAndZeroReserved()
    {
        var identifier = new DxItemIdentifier();

        await Assert.That(identifier.ItemPath).IsNull();
        await Assert.That(identifier.ItemName).IsNull();
        await Assert.That(identifier.Version).IsNull();
        await Assert.That(identifier.Reserved).IsEqualTo(0);
    }

    [Test]
    public async Task Constructor_AssignsAllRecordFields()
    {
        var identifier = new DxItemIdentifier(
            ItemPath: "DX/SourceServers",
            ItemName: "PLC1",
            Version: "cfg-42",
            Reserved: unchecked((int)0xA5A50001u));

        await Assert.That(identifier.ItemPath).IsEqualTo("DX/SourceServers");
        await Assert.That(identifier.ItemName).IsEqualTo("PLC1");
        await Assert.That(identifier.Version).IsEqualTo("cfg-42");
        await Assert.That(identifier.Reserved).IsEqualTo(unchecked((int)0xA5A50001u));
    }

    [Test]
    public async Task FromName_CreatesBranchLocalIdentifier()
    {
        DxItemIdentifier identifier = DxItemIdentifier.FromName("Pump.Speed", "v7");

        await Assert.That(identifier.ItemPath).IsNull();
        await Assert.That(identifier.ItemName).IsEqualTo("Pump.Speed");
        await Assert.That(identifier.Version).IsEqualTo("v7");
        await Assert.That(identifier.Reserved).IsEqualTo(0);
    }

    [Test]
    public async Task RecordEquality_UsesPathNameVersionAndReserved()
    {
        var left = new DxItemIdentifier("Path", "Name", "v1", 1);
        var same = new DxItemIdentifier("Path", "Name", "v1", 1);
        var differentReserved = left with { Reserved = 2 };
        var differentVersion = left with { Version = "v2" };

        await Assert.That(left).IsEqualTo(same);
        await Assert.That(left.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(left).IsNotEqualTo(differentReserved);
        await Assert.That(left).IsNotEqualTo(differentVersion);
    }
}
