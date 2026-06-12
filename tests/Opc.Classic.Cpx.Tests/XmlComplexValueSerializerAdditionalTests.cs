//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Cpx.Tests;

public sealed class XmlComplexValueSerializerAdditionalTests
{
    [Test]
    public async Task Serializer_RoundTripsPrimitiveArraysBlobGuidFileTimeAndNamespace()
    {
        var id = new Guid("4D40350B-0D22-4B98-9E4F-7A09D61802D5");
        var stamp = new DateTime(2026, 6, 7, 10, 20, 30, DateTimeKind.Utc);
        var type = new TypeDescription(
            "Batch",
            "BatchType",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Name", TypeKind.String),
                new TypeField("Counts", TypeKind.UInt16, ElementCount: 2),
                new TypeField("Payload", TypeKind.Blob),
                new TypeField("Id", TypeKind.Guid),
                new TypeField("Stamp", TypeKind.FileTime),
            });
        var dictionary = new TypeDictionary("http://example.com/cpx", new[] { type });
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["Name"] = "Batch-1",
            ["Counts"] = new ushort[] { 7, 9 },
            ["Payload"] = new byte[] { 0x01, 0x02, 0x03 },
            ["Id"] = id,
            ["Stamp"] = stamp,
        });

        string xml = XmlComplexValueSerializer.Serialize(value, type, dictionary);
        ComplexValue decoded = XmlComplexValueSerializer.Deserialize(xml, type, dictionary);

        await Assert.That(xml).Contains("xmlns=\"http://example.com/cpx\"");
        await Assert.That(xml).Contains("xsi:type=\"BatchType\"");
        await Assert.That(xml).Contains("<Counts>7</Counts><Counts>9</Counts>");
        await Assert.That(xml).Contains("<Payload>AQID</Payload>");
        await Assert.That(decoded.TryGet<string>("Name", out string? name)).IsTrue();
        await Assert.That(name).IsEqualTo("Batch-1");
        await Assert.That(decoded.TryGet<object?[]>("Counts", out object?[]? counts)).IsTrue();
        await Assert.That(counts![0]).IsEqualTo((ushort)7);
        await Assert.That(counts[1]).IsEqualTo((ushort)9);
        await Assert.That(decoded.TryGet<byte[]>("Payload", out byte[]? payload)).IsTrue();
        await Assert.That(payload).IsEquivalentTo(new byte[] { 0x01, 0x02, 0x03 });
        await Assert.That(decoded.TryGet<Guid>("Id", out Guid decodedId)).IsTrue();
        await Assert.That(decodedId).IsEqualTo(id);
        await Assert.That(decoded.TryGet<DateTime>("Stamp", out DateTime decodedStamp)).IsTrue();
        await Assert.That(decodedStamp).IsEqualTo(stamp);
    }

    [Test]
    public async Task Serializer_RoundTripsFixedNestedStructArray()
    {
        var motorType = new TypeDescription(
            "Motor",
            "MotorType",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Running", TypeKind.Boolean),
                new TypeField("Speed", TypeKind.Single),
            });
        var plantType = new TypeDescription(
            "Plant",
            "PlantType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Motor", TypeKind.StructReference, "MotorType", ElementCount: 2) });
        var dictionary = new TypeDictionary("http://example.com/cpx", new[] { motorType, plantType });
        var first = CreateValue(motorType, new Dictionary<string, object?> { ["Running"] = true, ["Speed"] = 12.5f });
        var second = CreateValue(motorType, new Dictionary<string, object?> { ["Running"] = false, ["Speed"] = 0.5f });
        var plant = CreateValue(plantType, new Dictionary<string, object?> { ["Motor"] = new[] { first, second } });

        string xml = XmlComplexValueSerializer.Serialize(plant, plantType, dictionary);
        ComplexValue decoded = XmlComplexValueSerializer.Deserialize(xml, plantType, dictionary);

        await Assert.That(xml).Contains("<Motor><Running>true</Running><Speed>12.5</Speed></Motor>");
        await Assert.That(decoded.TryGet<object?[]>("Motor", out object?[]? motors)).IsTrue();
        var decodedFirst = (ComplexValue)motors![0]!;
        var decodedSecond = (ComplexValue)motors[1]!;
        await Assert.That(decodedFirst.TryGet<bool>("Running", out bool firstRunning)).IsTrue();
        await Assert.That(firstRunning).IsTrue();
        await Assert.That(decodedFirst.TryGet<float>("Speed", out float firstSpeed)).IsTrue();
        await Assert.That(firstSpeed).IsEqualTo(12.5f);
        await Assert.That(decodedSecond.TryGet<bool>("Running", out bool secondRunning)).IsTrue();
        await Assert.That(secondRunning).IsFalse();
    }

    [Test]
    public async Task Serializer_ErrorBranches_ThrowSpecificExceptions()
    {
        var type = new TypeDescription(
            "Payload",
            "PayloadType",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Name", TypeKind.String),
                new TypeField("Values", TypeKind.UInt8, ElementCount: 2),
            });
        var missingName = CreateValue(type, new Dictionary<string, object?> { ["Values"] = new byte[] { 1, 2 } });
        var wrongCount = CreateValue(type, new Dictionary<string, object?> { ["Name"] = "X", ["Values"] = new byte[] { 1 } });
        const string missingValueXml = "<Payload><Name>X</Name></Payload>";
        const string wrongCountXml = "<Payload><Name>X</Name><Values>1</Values></Payload>";
        var unsupportedType = new TypeDescription(
            "Bits",
            "BitsType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Bits", TypeKind.BitString, Length: 8) });
        var unsupportedValue = CreateValue(unsupportedType, new Dictionary<string, object?> { ["Bits"] = new byte[] { 0x80 } });

        await Assert.That(() => XmlComplexValueSerializer.Serialize(missingName, type))
            .Throws<KeyNotFoundException>();
        await Assert.That(() => XmlComplexValueSerializer.Serialize(wrongCount, type))
            .Throws<InvalidOperationException>();
        await Assert.That(() => XmlComplexValueSerializer.Deserialize(missingValueXml, type))
            .Throws<FormatException>();
        await Assert.That(() => XmlComplexValueSerializer.Deserialize(wrongCountXml, type))
            .Throws<FormatException>();
        await Assert.That(() => XmlComplexValueSerializer.Serialize(unsupportedValue, unsupportedType))
            .Throws<NotSupportedException>();
    }

    private static ComplexValue CreateValue(TypeDescription type, IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };
}
