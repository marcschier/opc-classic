// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcBinaryBitStringTests
{
    [Test]
    [Arguments(3)]
    [Arguments(7)]
    [Arguments(11)]
    [Arguments(17)]
    [Arguments(33)]
    public async Task OpcBinaryEncoderDecoder_RoundTripsNonByteAlignedBitString(int bitCount)
    {
        var type = new TypeDescription(
            "BitStringContainer",
            "BitStringContainer",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Prefix", TypeKind.UInt8),
                new TypeField("Bits", TypeKind.BitString, Length: bitCount),
                new TypeField("Suffix", TypeKind.UInt8),
            });
        var bits = CreatePattern(bitCount);
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["Prefix"] = (byte)0xA5,
            ["Bits"] = bits,
            ["Suffix"] = (byte)0x5A,
        });

        var encoded = OpcBinaryEncoder.Encode(value, type);
        var decoded = OpcBinaryDecoder.Decode(encoded, type);
        var reencoded = OpcBinaryEncoder.Encode(decoded, type);

        await Assert.That(encoded.Length).IsEqualTo(2 + ((bitCount + 7) / 8));
        await Assert.That(encoded[0]).IsEqualTo((byte)0xA5);
        await Assert.That(encoded[^1]).IsEqualTo((byte)0x5A);
        await Assert.That(decoded.TryGet<byte[]>("Bits", out var decodedBits)).IsTrue();
        await Assert.That(decodedBits).IsEquivalentTo(bits);
        await Assert.That(reencoded).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task OpcBinaryEncoderDecoder_AccumulatesConsecutiveBitStringsBeforePadding()
    {
        var type = new TypeDescription(
            "SplitBits",
            "SplitBits",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("First", TypeKind.BitString, Length: 3),
                new TypeField("Second", TypeKind.BitString, Length: 5),
                new TypeField("Trailing", TypeKind.UInt8),
            });
        var first = new byte[] { 0b1010_0000 };
        var second = new byte[] { 0b1101_1000 };
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["First"] = first,
            ["Second"] = second,
            ["Trailing"] = (byte)0x42,
        });

        var encoded = OpcBinaryEncoder.Encode(value, type);
        var decoded = OpcBinaryDecoder.Decode(encoded, type);

        await Assert.That(encoded).IsEquivalentTo(new byte[] { 0b1011_1011, 0x42 });
        await Assert.That(decoded.TryGet<byte[]>("First", out var decodedFirst)).IsTrue();
        await Assert.That(decoded.TryGet<byte[]>("Second", out var decodedSecond)).IsTrue();
        await Assert.That(decodedFirst).IsEquivalentTo(first);
        await Assert.That(decodedSecond).IsEquivalentTo(second);
    }

    private static byte[] CreatePattern(int bitCount)
    {
        var bytes = new byte[(bitCount + 7) / 8];
        for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
        {
            if (bitIndex % 3 != 1)
            {
                bytes[bitIndex / 8] |= (byte)(1 << (7 - (bitIndex % 8)));
            }
        }

        return bytes;
    }

    private static ComplexValue CreateValue(TypeDescription type, IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };
}
