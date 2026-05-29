//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Generic;
using System.Linq;
using Opc.Classic.Cpx.Hosting;
using Opc.Classic.Da.Hosting;
using TUnit.Core;

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcCpxAddressSpaceTests
{
    [Test]
    public async Task BrowseAsync_ExposesCpxDictionaryTreeAlongsideInnerNamespace()
    {
        var inner = new InMemoryAddressSpace("Plant");
        inner.AddItem("Plant", "Temperature");
        var addressSpace = new OpcCpxAddressSpace(inner, CreateOptions());

        var root = await addressSpace.BrowseAsync(null, OpcBrowseElementKind.All);
        var typeSystems = await addressSpace.BrowseAsync(CpxNamespaceBuilder.RootPath, OpcBrowseElementKind.Branches);
        var dictionaries = await addressSpace.BrowseAsync("/CPX/OPCBinary", OpcBrowseElementKind.Branches);
        var types = await addressSpace.BrowseAsync("/CPX/OPCBinary/PlantTypes.v1", OpcBrowseElementKind.Items);
        var delegated = await addressSpace.BrowseAsync("Plant", OpcBrowseElementKind.Items);

        await Assert.That(root.Branches).Contains("Plant");
        await Assert.That(root.Branches).Contains("CPX");
        await Assert.That(typeSystems.Branches).Contains(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(dictionaries.Branches).Contains("PlantTypes.v1");
        await Assert.That(types.Items).Contains("MotorStatus");
        await Assert.That(types.Items).Contains("MotorCommand");
        await Assert.That(delegated.Items).Contains("Temperature");
    }

    [Test]
    public async Task DefaultBrowse_WalksCpxTypeItemsWithContinuationPoints()
    {
        var browse = new DefaultBrowse(new OpcCpxAddressSpace(new InMemoryAddressSpace(), CreateOptions()));
        string? continuationPoint = null;

        await browse.BrowseAsync(
            "/CPX/OPCBinary/PlantTypes.v1",
            ref continuationPoint,
            maxElementsReturned: 2,
            browseFilter: 3,
            elementNameFilter: string.Empty,
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: [],
            out var moreElements,
            out var firstPage);

        await Assert.That(moreElements).IsTrue();
        await Assert.That(continuationPoint).IsEqualTo("opc-da-browse:2");
        await Assert.That(firstPage.Select(static element => element.Name).ToArray())
            .IsEquivalentTo(new[] { "MotorStatus", "MotorCommand" });

        await browse.BrowseAsync(
            "/CPX/OPCBinary/PlantTypes.v1",
            ref continuationPoint,
            maxElementsReturned: 2,
            browseFilter: 3,
            elementNameFilter: string.Empty,
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: [],
            out moreElements,
            out var secondPage);

        await Assert.That(moreElements).IsFalse();
        await Assert.That(string.IsNullOrEmpty(continuationPoint)).IsTrue();
        await Assert.That(secondPage.Select(static element => element.Name).ToArray())
            .IsEquivalentTo(new[] { "MotorDiagnostics" });
    }

    private static OpcCpxOptions CreateOptions()
    {
        var types = new[]
        {
            new TypeDescription("MotorStatus", "MotorStatus", TypeKind.StructReference, true, new[] { new TypeField("Running", TypeKind.Boolean) }),
            new TypeDescription("MotorCommand", "MotorCommand", TypeKind.StructReference, true, new[] { new TypeField("Start", TypeKind.Boolean) }),
            new TypeDescription("MotorDiagnostics", "MotorDiagnostics", TypeKind.StructReference, true, new[] { new TypeField("Code", TypeKind.UInt16) }),
        };
        var dictionary = new TypeDictionary("http://example.com/PlantTypes.v1", types, defaultBigEndian: false);
        return new OpcCpxOptions().AddDictionary(
            TypeDictionary.OpcBinaryTypeSystemId,
            "http://example.com/PlantTypes.v1",
            dictionary);
    }
}
