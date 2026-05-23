//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip tests for the NDR SAFEARRAY (1-D scalar subset) codec.
//

using System;
using System.Linq;
using Opc.Classic;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class NdrSafeArrayTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcSafeArray ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return r.ReadSafeArray();
    }

    [Test]
    public async Task Int32Array_RoundTrips()
    {
        var input = OpcSafeArray.OfInt32(new[] { 1, 2, 3, 4, 5 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.ElementType).IsEqualTo(VarType.VT_I4);
        await Assert.That(read.TotalElements).IsEqualTo(5);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task DoubleArray_RoundTrips()
    {
        var input = OpcSafeArray.OfDouble(new[] { 1.5, -2.5, 3.14159265358979 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.ElementType).IsEqualTo(VarType.VT_R8);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task ByteArray_RoundTrips()
    {
        var input = OpcSafeArray.OfUInt8(new byte[] { 0xCA, 0xFE, 0xBA, 0xBE });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task Int16Array_RoundTrips()
    {
        var input = OpcSafeArray.OfInt16(new short[] { -1, 0, 1, short.MaxValue, short.MinValue });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task SingleArray_RoundTrips()
    {
        var input = OpcSafeArray.OfSingle(new[] { 0.5f, 1.5f, -2.5f });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task Int64Array_RoundTrips()
    {
        var input = OpcSafeArray.OfInt64(new[] { long.MinValue, 0L, long.MaxValue });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task BooleanArray_RoundTrips()
    {
        var input = OpcSafeArray.OfBoolean(new[] { true, false, true, true, false });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task StringArray_RoundTrips()
    {
        var input = OpcSafeArray.OfString(new[] { "Tag1", "Tag2", "Tag3" });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input), capacity: 256);
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task StringArray_WithNulls_RoundTrips()
    {
        var input = new OpcSafeArray(VarType.VT_BSTR, new string?[] { "ok", null, "also" });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input), capacity: 256);
        var read = ReadOne(bytes);
        var data = (string?[])read.Data;
        await Assert.That(data[0]).IsEqualTo("ok");
        await Assert.That(data[1]).IsNull();
        await Assert.That(data[2]).IsEqualTo("also");
    }

    [Test]
    public async Task EmptyArray_RoundTrips()
    {
        var input = OpcSafeArray.OfDouble(Array.Empty<double>());
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.TotalElements).IsEqualTo(0);
        await Assert.That(read.ElementType).IsEqualTo(VarType.VT_R8);
    }

    [Test]
    public async Task LowerBound_RoundTrips()
    {
        var input = new OpcSafeArray(
            VarType.VT_I4,
            new int[] { 10, 20, 30 },
            lengths: new[] { 3 },
            lowerBounds: new[] { 7 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.LowerBounds[0]).IsEqualTo(7);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task LargeDoubleArray_RoundTrips()
    {
        var input = OpcSafeArray.OfDouble(Enumerable.Range(0, 100).Select(i => i * 0.25).ToArray());
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input), capacity: 2048);
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task MultiDimensional_ThrowsOnWrite()
    {
        bool threw = false;
        try
        {
            var input = new OpcSafeArray(
                VarType.VT_I4,
                new int[] { 1, 2, 3, 4 },
                lengths: new[] { 2, 2 });
            WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Header_HasExpectedFlags()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
            w.WriteSafeArray(OpcSafeArray.OfInt32(new[] { 1, 2, 3 })));
        // cDims=1 (UInt16), fFeatures=0x0080 (FADF_HAVEVARTYPE)
        await Assert.That(BitConverter.ToUInt16(bytes, 0)).IsEqualTo((ushort)1);
        await Assert.That(BitConverter.ToUInt16(bytes, 2)).IsEqualTo((ushort)0x0080);
        // cbElements=4 for VT_I4
        await Assert.That(BitConverter.ToUInt32(bytes, 4)).IsEqualTo(4u);
        // cLocks=0
        await Assert.That(BitConverter.ToUInt32(bytes, 8)).IsEqualTo(0u);
    }
}
