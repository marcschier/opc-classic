// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

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

    [Test]
    public async Task Serializer_ConvertedNestedStructureArray_RoundTripsRequestedShape()
    {
        var sourceItem = CreateType("SourceItem", "source:item", new TypeField("Code", TypeKind.UInt8));
        var requestedItem = CreateType("RequestedItem", "requested:item", new TypeField("Code", TypeKind.UInt16));
        var sourceBatch = CreateType(
            "SourceBatch",
            "source:batch",
            new TypeField("Items", TypeKind.StructReference, sourceItem.TypeId, ElementCount: 2));
        var requestedBatch = CreateType(
            "RequestedBatch",
            "requested:batch",
            new TypeField("Items", TypeKind.StructReference, requestedItem.TypeId, ElementCount: 2));
        var requestedDictionary = new TypeDictionary("http://example.com/requested", [requestedBatch, requestedItem]);
        var source = CreateValue(sourceBatch, new Dictionary<string, object?>
        {
            ["Items"] = new[]
            {
                CreateValue(sourceItem, new Dictionary<string, object?> { ["Code"] = (byte)7 }),
                CreateValue(sourceItem, new Dictionary<string, object?> { ["Code"] = (byte)9 }),
            },
        });
        var converted = OpcCpxTypeConverter.Convert(
            source,
            sourceBatch,
            requestedBatch,
            TypeDictionary.FromTypes(sourceBatch, sourceItem),
            requestedDictionary);

        var xml = XmlComplexValueSerializer.Serialize((ComplexValue)converted.Value!, requestedBatch, requestedDictionary);
        var decoded = XmlComplexValueSerializer.Deserialize(xml, requestedBatch, requestedDictionary);

        await Assert.That(converted.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(decoded.TryGet<object?[]>("Items", out var items)).IsTrue();
        await Assert.That(((ComplexValue)items![0]!).Fields["Code"]).IsEqualTo((ushort)7);
        await Assert.That(((ComplexValue)items[1]!).Fields["Code"]).IsEqualTo((ushort)9);
    }

    [Test]
    public async Task Serializer_NestedRequiredFieldMissing_NoOptionalFieldInference_ThrowsExpectedExceptions()
    {
        var child = CreateType("Child", "child", new TypeField("Required", TypeKind.UInt8));
        var root = CreateType("Root", "root", new TypeField("Child", TypeKind.StructReference, child.TypeId));
        var dictionary = TypeDictionary.FromTypes(root, child);
        var missing = CreateValue(root, new Dictionary<string, object?>
        {
            ["Child"] = CreateValue(child, new Dictionary<string, object?>()),
        });

        await Assert.That(() => XmlComplexValueSerializer.Serialize(missing, root, dictionary))
            .Throws<KeyNotFoundException>();
        await Assert.That(() => XmlComplexValueSerializer.Deserialize("<Root><Child /></Root>", root, dictionary))
            .Throws<FormatException>();
    }

    [Test]
    public async Task Serializer_PreservesOptionalAndMinMaxOccurrenceConstraints()
    {
        var type = CreateType(
            "Envelope",
            "envelope",
            new TypeField("Required", TypeKind.String, MinOccurs: 1),
            new TypeField("Optional", TypeKind.String, MinOccurs: 0),
            new TypeField("Values", TypeKind.UInt8, ElementCount: 2, MinOccurs: 1));
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["Required"] = "present",
            ["Values"] = new byte[] { 7 },
        });

        var xml = XmlComplexValueSerializer.Serialize(value, type);
        var decoded = XmlComplexValueSerializer.Deserialize(xml, type);

        await Assert.That(xml).DoesNotContain("<Optional>");
        await Assert.That(xml).Contains("<Values>7</Values>");
        await Assert.That(decoded.Fields.ContainsKey("Optional")).IsFalse();
        await Assert.That(((object?[])decoded.Fields["Values"]!).Length).IsEqualTo(1);
    }

    [Test]
    public async Task Serializer_ExtraElementsAndFields_AreIgnored()
    {
        var type = CreateType("Payload", "payload", new TypeField("Value", TypeKind.UInt8));
        var value = CreateValue(type, new Dictionary<string, object?> { ["Value"] = (byte)7, ["Ignored"] = 99 });
        var xml = XmlComplexValueSerializer.Serialize(value, type);
        var decoded = XmlComplexValueSerializer.Deserialize(
            "<Payload><Ignored>99</Ignored><Value>7</Value><Extra>100</Extra></Payload>",
            type);

        await Assert.That(xml).IsEqualTo("<Payload xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:type=\"payload\"><Value>7</Value></Payload>");
        await Assert.That(decoded.Fields.Count).IsEqualTo(1);
        await Assert.That(decoded.Fields["Value"]).IsEqualTo((byte)7);
    }

    private static TypeDescription CreateType(string name, string typeId, params TypeField[] fields) =>
        new(name, typeId, TypeKind.StructReference, true, fields);

    private static ComplexValue CreateValue(TypeDescription type, IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };
}
