//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Linq;
using OpcClassic;
using OpcClassic.Hda;
using OpcClassic.Hda.Ndr;
using OpcClassic.Ndr;
using TUnit.Core;

namespace OpcClassic.Hda.Tests;

public sealed class NdrOpcHdaItemCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcHdaItem ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcHdaItemCodec.Read(ref r);
    }

    private static OpcHdaItem MakeSample(int count = 3)
    {
        var timestamps = new DateTimeOffset[count];
        var qualities = new uint[count];
        var values = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            timestamps[i] = new DateTimeOffset(2026, 5, 22, 10, i, 0, TimeSpan.Zero);
            qualities[i] = 192;  // HDA quality "Good"
            values[i] = OpcVariant.FromDouble(100.0 + i);
        }
        return new OpcHdaItem(42, 0, timestamps, qualities, values);
    }

    [Test]
    public async Task RoundTrip_ThreeDoubleValues()
    {
        var input = MakeSample(3);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.ClientHandle).IsEqualTo(42);
        await Assert.That(back.Timestamps.Length).IsEqualTo(3);
        await Assert.That(back.Values[0].AsDouble()).IsEqualTo(100.0);
        await Assert.That(back.Values[2].AsDouble()).IsEqualTo(102.0);
    }

    [Test]
    public async Task RoundTrip_SingleValue()
    {
        var input = MakeSample(1);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Timestamps.Length).IsEqualTo(1);
        await Assert.That(back.Values[0].AsDouble()).IsEqualTo(100.0);
    }

    [Test]
    public async Task RoundTrip_EmptySeries()
    {
        var input = new OpcHdaItem(
            clientHandle: 1,
            aggregateHandle: 0,
            timestamps: Array.Empty<DateTimeOffset>(),
            qualities: Array.Empty<uint>(),
            values: Array.Empty<OpcVariant>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Timestamps.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ConstructorRejectsArrayLengthMismatch()
    {
        bool threw = false;
        try
        {
            _ = new OpcHdaItem(
                clientHandle: 1,
                aggregateHandle: 0,
                timestamps: new DateTimeOffset[3],
                qualities: new uint[3],
                values: new OpcVariant[2]);
        }
        catch (ArgumentException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task RoundTrip_MixedVariantTypes()
    {
        var input = new OpcHdaItem(
            clientHandle: 7,
            aggregateHandle: 0,
            timestamps: new[]
            {
                new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero),
            },
            qualities: new uint[] { 192, 192 },
            values: new[] { OpcVariant.FromDouble(3.14), OpcVariant.FromInt32(42) });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Values[0].AsDouble()).IsEqualTo(3.14);
        await Assert.That(back.Values[1].AsInt32()).IsEqualTo(42);
    }

    [Test]
    public async Task RoundTrip_AggregateHandlePreserved()
    {
        var input = new OpcHdaItem(
            clientHandle: 1,
            aggregateHandle: 17,
            timestamps: Array.Empty<DateTimeOffset>(),
            qualities: Array.Empty<uint>(),
            values: Array.Empty<OpcVariant>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaItemCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.AggregateHandle).IsEqualTo(17);
    }
}
