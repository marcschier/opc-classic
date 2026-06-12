//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Cpx.Hosting;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcCpxItemPropertiesTests
{
    private const string ItemId = "Plant.Motor01.Status";
    private const string DictionaryId = "http://example.com/PlantTypes.v1";
    private const string DictionaryXml = "<TypeDictionary Name=\"PlantTypes\" />";
    private const string TypeDescriptionXml = "<TypeDescription TypeID=\"MotorStatus\" />";

    [Test]
    public async Task DefaultItemProperties_PublishesCpxPropertiesForComplexItem()
    {
        var properties = new DefaultItemProperties(new OpcCpxItemProperties(CreateOptions()));

        await properties.QueryAvailablePropertiesAsync(
            ItemId,
            out var propertyIds,
            out var descriptions,
            out var dataTypes);

        await Assert.That(propertyIds).Contains(OpcComplexDataProperty.TypeSystemId);
        await Assert.That(propertyIds).Contains(OpcComplexDataProperty.DataFilterValue);
        await Assert.That(descriptions[propertyIds.IndexOf(OpcComplexDataProperty.TypeId)]).IsEqualTo("Type ID");
        await Assert.That(dataTypes[propertyIds.IndexOf(OpcComplexDataProperty.DictionaryId)]).IsEqualTo((ushort)VarType.VT_BSTR);
    }

    [Test]
    public async Task GetItemProperties_ReturnsCpxPropertyValuesAndLookupItemIds()
    {
        var properties = new DefaultItemProperties(new OpcCpxItemProperties(CreateOptions()));
        var propertyIds = new[]
        {
            OpcComplexDataProperty.TypeSystemId,
            OpcComplexDataProperty.DictionaryId,
            OpcComplexDataProperty.TypeId,
            OpcComplexDataProperty.Dictionary,
            OpcComplexDataProperty.TypeDescription,
            OpcComplexDataProperty.ConsistencyWindow,
            OpcComplexDataProperty.WriteBehavior,
            OpcComplexDataProperty.UnconvertedItemId,
            OpcComplexDataProperty.UnfilteredItemId,
            OpcComplexDataProperty.DataFilterValue,
        };

        await properties.GetItemPropertiesAsync(ItemId, propertyIds, out var values, out var errors);
        await properties.LookupItemIdsAsync(
            ItemId,
            new[]
            {
                OpcComplexDataProperty.DictionaryId,
                OpcComplexDataProperty.TypeId,
                OpcComplexDataProperty.UnconvertedItemId,
                OpcComplexDataProperty.UnfilteredItemId,
            },
            out var itemIds,
            out var lookupErrors);

        await Assert.That(errors).IsEquivalentTo(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        await Assert.That(values[0].AsString()).IsEqualTo(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(values[1].AsString()).IsEqualTo(DictionaryId);
        await Assert.That(values[2].AsString()).IsEqualTo("MotorStatus");
        await Assert.That(values[3].AsString()).IsEqualTo(DictionaryXml);
        await Assert.That(values[4].AsString()).IsEqualTo(TypeDescriptionXml);
        await Assert.That(values[5].AsString()).IsEqualTo("Unknown");
        await Assert.That(values[6].AsString()).IsEqualTo("Best Effort");
        await Assert.That(values[7].AsString()).IsEqualTo("Plant.Motor01.NativeStatus");
        await Assert.That(values[8].AsString()).IsEqualTo("Plant.Motor01.UnfilteredStatus");
        await Assert.That(values[9].AsString()).IsEqualTo("Severity >= Good");
        await Assert.That(lookupErrors).IsEquivalentTo(new[] { 0, 0, 0, 0 });
        await Assert.That(itemIds).IsEquivalentTo(new[]
        {
            "/CPX/OPCBinary/PlantTypes.v1",
            "/CPX/OPCBinary/PlantTypes.v1/MotorStatus",
            "Plant.Motor01.NativeStatus",
            "Plant.Motor01.UnfilteredStatus",
        });
    }

    private static OpcCpxOptions CreateOptions()
    {
        var dictionary = new TypeDictionary(
            "PlantTypes",
            new[]
            {
                new TypeDescription("MotorStatus", "MotorStatus", TypeKind.StructReference, true, new[] { new TypeField("Running", TypeKind.Boolean) }),
            });

        return new OpcCpxOptions()
            .AddDictionary(
                TypeDictionary.OpcBinaryTypeSystemId,
                DictionaryId,
                dictionary,
                DictionaryXml,
                new Dictionary<string, string>
                {
                    ["MotorStatus"] = TypeDescriptionXml,
                })
            .AddComplexItem(
                ItemId,
                TypeDictionary.OpcBinaryTypeSystemId,
                DictionaryId,
                "MotorStatus",
                consistencyWindow: "Unknown",
                writeBehavior: "Best Effort",
                unconvertedItemId: "Plant.Motor01.NativeStatus",
                unfilteredItemId: "Plant.Motor01.UnfilteredStatus",
                dataFilterValue: "Severity >= Good");
    }
}

internal static class PropertyIdTestExtensions
{
    public static int IndexOf(this int[] values, int value)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] == value)
            {
                return i;
            }
        }

        return -1;
    }
}
