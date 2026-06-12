//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip tests for the NDR conformant-array writer/reader helpers.
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Tests;

public sealed class NdrArrayTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    [Test]
    public async Task ByteArray_RoundTrips()
    {
        var input = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x42 };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantByteArray(input));
        byte[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantByteArray();
        }
        // 4 (count) + 5 (data) = 9
        await Assert.That(bytes.Length).IsEqualTo(9);
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task ByteArray_Empty_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantByteArray(ReadOnlySpan<byte>.Empty));
        byte[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantByteArray();
        }
        await Assert.That(bytes.Length).IsEqualTo(4);
        await Assert.That(read.Length).IsEqualTo(0);
    }

    [Test]
    public async Task Int16Array_RoundTrips()
    {
        var input = new short[] { -32768, -1, 0, 1, 32767 };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantInt16Array(input));
        short[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantInt16Array();
        }
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task UInt16Array_RoundTrips()
    {
        var input = new ushort[] { 0, 1, 0xCAFE, 0xFFFF };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantUInt16Array(input));
        ushort[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantUInt16Array();
        }
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task Int32Array_RoundTrips()
    {
        var input = new int[] { int.MinValue, -1, 0, 1, int.MaxValue, unchecked((int)0xC0040001u) };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantInt32Array(input));
        int[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantInt32Array();
        }
        // 4 (count) + 6*4 (elements) = 28
        await Assert.That(bytes.Length).IsEqualTo(28);
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task UInt32Array_RoundTrips()
    {
        var input = new uint[] { 0, 1, 0xDEADBEEFu, 0xFFFFFFFFu };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantUInt32Array(input));
        uint[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantUInt32Array();
        }
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task Int64Array_RoundTrips_WithAlignment()
    {
        var input = new long[] { long.MinValue, -1L, 0L, 0x0102030405060708L, long.MaxValue };
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            // Force unaligned start so the array header alignment matters.
            w.WriteByte(0x01);
            w.WriteConformantInt64Array(input);
        });
        byte rByte;
        long[] read;
        {
            var r = new NdrReader(bytes);
            rByte = r.ReadByte();
            read = r.ReadConformantInt64Array();
        }
        await Assert.That(rByte).IsEqualTo((byte)0x01);
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task SingleArray_RoundTrips()
    {
        var input = new float[] { float.MinValue, -0.5f, 0f, 0.5f, float.MaxValue };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantSingleArray(input));
        float[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantSingleArray();
        }
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task DoubleArray_RoundTrips()
    {
        var input = new double[] { double.MinValue, -1.5, 0, 1.5, 3.141592653589793, double.MaxValue };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantDoubleArray(input));
        double[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantDoubleArray();
        }
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task GuidArray_RoundTrips()
    {
        var input = new[]
        {
            new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3"),
            new Guid("85C0B427-2893-4CBC-BD78-E5FC5146F08F"),
            new Guid("1F1217B0-DEE0-11D2-A5E5-000086339399"),
        };
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantGuidArray(input));
        Guid[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantGuidArray();
        }
        // 4 (count) + 3*16 (elements) = 52
        await Assert.That(bytes.Length).IsEqualTo(52);
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task DoubleArray_LargeBuffer_RoundTrips()
    {
        var input = Enumerable.Range(0, 100).Select(i => i * 0.5).ToArray();
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformantDoubleArray(input), capacity: 4096);
        double[] read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformantDoubleArray();
        }
        await Assert.That(read.Length).IsEqualTo(100);
        await Assert.That(read.SequenceEqual(input)).IsTrue();
    }

    [Test]
    public async Task MixedArrays_RoundTrip()
    {
        var ints = new int[] { 1, 2, 3 };
        var doubles = new double[] { 1.1, 2.2, 3.3 };
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteConformantInt32Array(ints);
            w.WriteConformantDoubleArray(doubles);
        });
        int[] rInts;
        double[] rDoubles;
        {
            var r = new NdrReader(bytes);
            rInts = r.ReadConformantInt32Array();
            rDoubles = r.ReadConformantDoubleArray();
        }
        await Assert.That(rInts.SequenceEqual(ints)).IsTrue();
        await Assert.That(rDoubles.SequenceEqual(doubles)).IsTrue();
    }
}
