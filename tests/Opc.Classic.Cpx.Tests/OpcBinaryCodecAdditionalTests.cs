// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcBinaryCodecAdditionalTests
{
    [Test]
    public async Task EncoderDecoder_RoundTripsPrimitiveMatrixWithExplicitByteOrders()
    {
        var timestamp = new DateTime(2026, 6, 7, 12, 34, 56, DateTimeKind.Utc);
        var guid = new Guid("00112233-4455-6677-8899-AABBCCDDEEFF");
        var type = new TypeDescription(
            "PrimitiveMatrix",
            "PrimitiveMatrix",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Flag", TypeKind.Boolean),
                new TypeField("Signed8", TypeKind.Int8),
                new TypeField("Unsigned8", TypeKind.UInt8),
                new TypeField("LittleInt16", TypeKind.Int16),
                new TypeField("BigUInt16", TypeKind.UInt16, ByteOrder: ByteOrder.BigEndian),
                new TypeField("LittleInt32", TypeKind.Int32),
                new TypeField("BigUInt32", TypeKind.UInt32, ByteOrder: ByteOrder.BigEndian),
                new TypeField("LittleInt64", TypeKind.Int64),
                new TypeField("BigUInt64", TypeKind.UInt64, ByteOrder: ByteOrder.BigEndian),
                new TypeField("Float", TypeKind.Single),
                new TypeField("BigDouble", TypeKind.Double, ByteOrder: ByteOrder.BigEndian),
                new TypeField("Stamp", TypeKind.FileTime),
                new TypeField("Id", TypeKind.Guid),
            },
            defaultBigEndian: false);
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["Flag"] = "true",
            ["Signed8"] = (sbyte)-2,
            ["Unsigned8"] = (byte)250,
            ["LittleInt16"] = (short)-1234,
            ["BigUInt16"] = (ushort)0xABCD,
            ["LittleInt32"] = 0x01020304,
            ["BigUInt32"] = 0x01020304u,
            ["LittleInt64"] = 0x0102030405060708L,
            ["BigUInt64"] = 0x0102030405060708UL,
            ["Float"] = 1.25f,
            ["BigDouble"] = -2.5d,
            ["Stamp"] = timestamp,
            ["Id"] = guid.ToString("D"),
        });

        byte[] encoded = OpcBinaryEncoder.Encode(value, type);
        ComplexValue decoded = OpcBinaryDecoder.Decode(encoded, type);

        await Assert.That(encoded[0]).IsEqualTo((byte)1);
        await Assert.That(encoded[1]).IsEqualTo((byte)0xFE);
        await Assert.That(encoded[2]).IsEqualTo((byte)250);
        await Assert.That(encoded[3]).IsEqualTo((byte)0x2E);
        await Assert.That(encoded[4]).IsEqualTo((byte)0xFB);
        await Assert.That(encoded[5]).IsEqualTo((byte)0xAB);
        await Assert.That(encoded[6]).IsEqualTo((byte)0xCD);
        await Assert.That(encoded[7]).IsEqualTo((byte)0x04);
        await Assert.That(encoded[8]).IsEqualTo((byte)0x03);
        await Assert.That(encoded[11]).IsEqualTo((byte)0x01);
        await Assert.That(decoded.TryGet<bool>("Flag", out bool flag)).IsTrue();
        await Assert.That(flag).IsTrue();
        await Assert.That(decoded.TryGet<sbyte>("Signed8", out sbyte signed8)).IsTrue();
        await Assert.That(signed8).IsEqualTo((sbyte)-2);
        await Assert.That(decoded.TryGet<ushort>("BigUInt16", out ushort bigUInt16)).IsTrue();
        await Assert.That(bigUInt16).IsEqualTo((ushort)0xABCD);
        await Assert.That(decoded.TryGet<uint>("BigUInt32", out uint bigUInt32)).IsTrue();
        await Assert.That(bigUInt32).IsEqualTo(0x01020304u);
        await Assert.That(decoded.TryGet<ulong>("BigUInt64", out ulong bigUInt64)).IsTrue();
        await Assert.That(bigUInt64).IsEqualTo(0x0102030405060708UL);
        await Assert.That(decoded.TryGet<float>("Float", out float floatValue)).IsTrue();
        await Assert.That(floatValue).IsEqualTo(1.25f);
        await Assert.That(decoded.TryGet<double>("BigDouble", out double doubleValue)).IsTrue();
        await Assert.That(doubleValue).IsEqualTo(-2.5d);
        await Assert.That(decoded.TryGet<DateTime>("Stamp", out DateTime decodedTimestamp)).IsTrue();
        await Assert.That(decodedTimestamp).IsEqualTo(timestamp);
        await Assert.That(decoded.TryGet<Guid>("Id", out Guid decodedGuid)).IsTrue();
        await Assert.That(decodedGuid).IsEqualTo(guid);
    }

    [Test]
    public async Task EncoderDecoder_RoundTripsFixedCountedTerminatedAndLengthPrefixedFields()
    {
        var type = new TypeDescription(
            "MixedBinary",
            "MixedBinary",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Count", TypeKind.UInt8),
                new TypeField("FixedAscii", TypeKind.String, Length: 4, StringEncoding: "ASCII", CharWidth: 1),
                new TypeField("CountedUtf8", TypeKind.String, ElementCountFieldName: "Count", StringEncoding: "UTF-8", CharWidth: 1),
                new TypeField("TerminatedString", TypeKind.String, FieldTerminator: "00", StringEncoding: "ASCII", CharWidth: 1),
                new TypeField("FixedBlob", TypeKind.Blob, Length: 3),
                new TypeField("CountedBlob", TypeKind.Blob, ElementCountFieldName: "Count"),
                new TypeField("TerminatedBlob", TypeKind.Blob, FieldTerminator: "DE AD"),
                new TypeField("LengthBlob", TypeKind.Blob),
            },
            defaultBigEndian: false);
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["Count"] = (byte)3,
            ["FixedAscii"] = "AB",
            ["CountedUtf8"] = "XYZ",
            ["TerminatedString"] = "END",
            ["FixedBlob"] = new byte[] { 0x01, 0x02 },
            ["CountedBlob"] = new byte[] { 0x03, 0x04, 0x05 },
            ["TerminatedBlob"] = new byte[] { 0x06, 0x07 },
            ["LengthBlob"] = new byte[] { 0x08, 0x09 },
        });

        byte[] encoded = OpcBinaryEncoder.Encode(value, type);
        ComplexValue decoded = OpcBinaryDecoder.Decode(encoded, type);

        await Assert.That(encoded).IsEquivalentTo(new byte[]
        {
            0x03,
            0x41, 0x42, 0x00, 0x00,
            0x58, 0x59, 0x5A,
            0x45, 0x4E, 0x44, 0x00,
            0x01, 0x02, 0x00,
            0x03, 0x04, 0x05,
            0x06, 0x07, 0xDE, 0xAD,
            0x02, 0x00, 0x00, 0x00,
            0x08, 0x09,
        });
        await Assert.That(decoded.TryGet<string>("FixedAscii", out string? fixedAscii)).IsTrue();
        await Assert.That(fixedAscii).IsEqualTo("AB");
        await Assert.That(decoded.TryGet<string>("CountedUtf8", out string? countedUtf8)).IsTrue();
        await Assert.That(countedUtf8).IsEqualTo("XYZ");
        await Assert.That(decoded.TryGet<string>("TerminatedString", out string? terminatedString)).IsTrue();
        await Assert.That(terminatedString).IsEqualTo("END");
        await Assert.That(decoded.TryGet<byte[]>("FixedBlob", out byte[]? fixedBlob)).IsTrue();
        await Assert.That(fixedBlob).IsEquivalentTo(new byte[] { 0x01, 0x02, 0x00 });
        await Assert.That(decoded.TryGet<byte[]>("CountedBlob", out byte[]? countedBlob)).IsTrue();
        await Assert.That(countedBlob).IsEquivalentTo(new byte[] { 0x03, 0x04, 0x05 });
        await Assert.That(decoded.TryGet<byte[]>("TerminatedBlob", out byte[]? terminatedBlob)).IsTrue();
        await Assert.That(terminatedBlob).IsEquivalentTo(new byte[] { 0x06, 0x07 });
        await Assert.That(decoded.TryGet<byte[]>("LengthBlob", out byte[]? lengthBlob)).IsTrue();
        await Assert.That(lengthBlob).IsEquivalentTo(new byte[] { 0x08, 0x09 });
    }

    [Test]
    public async Task EncoderDecoder_RoundTripsNestedStructArrayUsingCountField()
    {
        var sampleType = new TypeDescription(
            "Sample",
            "SampleType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Code", TypeKind.UInt16) },
            defaultBigEndian: false);
        var batchType = new TypeDescription(
            "Batch",
            "BatchType",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Count", TypeKind.UInt8),
                new TypeField("Samples", TypeKind.StructReference, "SampleType", ElementCountFieldName: "Count"),
            },
            defaultBigEndian: false);
        var dictionary = new TypeDictionary("PlantBinary", new[] { sampleType, batchType }, defaultBigEndian: false);
        var first = CreateValue(sampleType, new Dictionary<string, object?> { ["Code"] = (ushort)0x1234 });
        var second = CreateValue(sampleType, new Dictionary<string, object?> { ["Code"] = (ushort)0xABCD });
        var batch = CreateValue(batchType, new Dictionary<string, object?>
        {
            ["Count"] = (byte)2,
            ["Samples"] = new[] { first, second },
        });

        byte[] encoded = OpcBinaryEncoder.Encode(batch, dictionary, "BatchType");
        ComplexValue decoded = OpcBinaryDecoder.Decode(encoded, dictionary, "BatchType");

        await Assert.That(encoded).IsEquivalentTo(new byte[] { 0x02, 0x34, 0x12, 0xCD, 0xAB });
        await Assert.That(decoded.TryGet<ComplexValue[]>("Samples", out ComplexValue[]? samples)).IsTrue();
        await Assert.That(samples!.Length).IsEqualTo(2);
        await Assert.That(samples[0].TryGet<ushort>("Code", out ushort firstCode)).IsTrue();
        await Assert.That(firstCode).IsEqualTo((ushort)0x1234);
        await Assert.That(samples[1].TryGet<ushort>("Code", out ushort secondCode)).IsTrue();
        await Assert.That(secondCode).IsEqualTo((ushort)0xABCD);
    }

    [Test]
    public async Task EncoderDecoder_ErrorBranches_ThrowSpecificExceptions()
    {
        var type = new TypeDescription(
            "OneField",
            "OneFieldType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Value", TypeKind.UInt8) });
        var missingField = CreateValue(type, new Dictionary<string, object?>());
        var fixedString = new TypeDescription(
            "FixedString",
            "FixedStringType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Name", TypeKind.String, Length: 2, StringEncoding: "ASCII", CharWidth: 1) });
        var longString = CreateValue(fixedString, new Dictionary<string, object?> { ["Name"] = "ABC" });
        var bitStringWithoutLength = new TypeDescription(
            "Bits",
            "BitsType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Bits", TypeKind.BitString) });
        var bitStringValue = CreateValue(bitStringWithoutLength, new Dictionary<string, object?> { ["Bits"] = new byte[] { 0x80 } });
        var dictionary = TypeDictionary.FromTypes(type);

        await Assert.That(() => OpcBinaryEncoder.Encode(missingField, type))
            .Throws<KeyNotFoundException>();
        await Assert.That(() => OpcBinaryEncoder.Encode(longString, fixedString))
            .Throws<InvalidOperationException>();
        await Assert.That(() => OpcBinaryEncoder.Encode(bitStringValue, bitStringWithoutLength))
            .Throws<InvalidOperationException>();
        await Assert.That(() => OpcBinaryEncoder.Encode(CreateValue(type, new Dictionary<string, object?> { ["Value"] = 1 }), dictionary, "MissingType"))
            .Throws<KeyNotFoundException>();
        await Assert.That(() => OpcBinaryDecoder.Decode(new byte[] { 0x01, 0x02 }, type))
            .Throws<FormatException>();
        await Assert.That(() => OpcBinaryDecoder.Decode(new byte[] { 0x01 }, dictionary, "MissingType"))
            .Throws<KeyNotFoundException>();
    }

    private static ComplexValue CreateValue(TypeDescription type, IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };
}
