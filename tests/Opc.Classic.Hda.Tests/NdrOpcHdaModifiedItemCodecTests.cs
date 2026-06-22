// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Tests;

public sealed class NdrOpcHdaModifiedItemCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 2048)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcHdaModifiedItem ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcHdaModifiedItemCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TwoModifiedItems()
    {
        var input = new OpcHdaModifiedItem(
            clientHandle: 42,
            timestamps: new[]
            {
                new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero),
            },
            qualities: new uint[] { 192, 216 },
            values: new[] { OpcVariant.FromDouble(12.5), OpcVariant.FromDouble(13.75) },
            modificationTimes: new[]
            {
                new DateTimeOffset(2026, 5, 23, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 23, 8, 5, 0, TimeSpan.Zero),
            },
            editTypes: new uint[] { 1, 2 },
            users: new string?[] { "operator-a", "operator-b" });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaModifiedItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.ClientHandle).IsEqualTo(42);
        await Assert.That(back.Timestamps.Length).IsEqualTo(2);
        await Assert.That(back.Values[0].AsDouble()).IsEqualTo(12.5);
        await Assert.That(back.Values[1].AsDouble()).IsEqualTo(13.75);
        await Assert.That(back.EditTypes[0]).IsEqualTo(1u);
        await Assert.That(back.EditTypes[1]).IsEqualTo(2u);
        await Assert.That(back.Users[0]).IsEqualTo("operator-a");
        await Assert.That(back.Users[1]).IsEqualTo("operator-b");
    }

    [Test]
    public async Task RoundTrip_Empty()
    {
        var input = new OpcHdaModifiedItem(
            clientHandle: 1,
            timestamps: Array.Empty<DateTimeOffset>(),
            qualities: Array.Empty<uint>(),
            values: Array.Empty<OpcVariant>(),
            modificationTimes: Array.Empty<DateTimeOffset>(),
            editTypes: Array.Empty<uint>(),
            users: Array.Empty<string?>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaModifiedItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Timestamps.Length).IsEqualTo(0);
        await Assert.That(back.Users.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_NullUserNames()
    {
        var input = new OpcHdaModifiedItem(
            clientHandle: 7,
            timestamps: new[]
            {
                new DateTimeOffset(2026, 5, 22, 11, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 11, 1, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 11, 2, 0, TimeSpan.Zero),
            },
            qualities: new uint[] { 192, 192, 192 },
            values: new[] { OpcVariant.FromDouble(1.0), OpcVariant.FromDouble(2.0), OpcVariant.FromDouble(3.0) },
            modificationTimes: new[]
            {
                new DateTimeOffset(2026, 5, 23, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 23, 9, 1, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 23, 9, 2, 0, TimeSpan.Zero),
            },
            editTypes: new uint[] { 1, 3, 4 },
            users: new string?[] { null, "operator", null });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaModifiedItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Users[0]).IsNull();
        await Assert.That(back.Users[1]).IsEqualTo("operator");
        await Assert.That(back.Users[2]).IsNull();
    }

    [Test]
    public async Task ConstructorRejectsParallelArrayLengthMismatch()
    {
        bool threw = false;
        try
        {
            _ = new OpcHdaModifiedItem(
                clientHandle: 1,
                timestamps: new DateTimeOffset[2],
                qualities: new uint[2],
                values: new OpcVariant[1],
                modificationTimes: new DateTimeOffset[2],
                editTypes: new uint[2],
                users: new string?[2]);
        }
        catch (ArgumentException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task ByteLayout_HClientAtOffsetZero()
    {
        var input = new OpcHdaModifiedItem(
            clientHandle: unchecked((int)0xAABBCCDDu),
            timestamps: Array.Empty<DateTimeOffset>(),
            qualities: Array.Empty<uint>(),
            values: Array.Empty<OpcVariant>(),
            modificationTimes: Array.Empty<DateTimeOffset>(),
            editTypes: Array.Empty<uint>(),
            users: Array.Empty<string?>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaModifiedItemCodec.Write(ref w, input));
        await Assert.That(BitConverter.ToUInt32(bytes, 0)).IsEqualTo(0xAABBCCDDu);
    }
}
