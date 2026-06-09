//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip tests for the NDR SAFEARRAY (1-D scalar subset) codec.
//

using System;
using System.IO;
using System.Linq;
using Opc.Classic;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class NdrSafeArrayTests {
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256) {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcSafeArray ReadOne(byte[] bytes) {
        var r = new NdrReader(bytes);
        return r.ReadSafeArray();
    }

    [Test]
    public async Task Int32Array_RoundTrips() {
        var input = OpcSafeArray.OfInt32(new[] { 1, 2, 3, 4, 5 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.ElementType).IsEqualTo(VarType.VT_I4);
        await Assert.That(read.TotalElements).IsEqualTo(5);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task DoubleArray_RoundTrips() {
        var input = OpcSafeArray.OfDouble(new[] { 1.5, -2.5, 3.14159265358979 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.ElementType).IsEqualTo(VarType.VT_R8);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task ByteArray_RoundTrips() {
        var input = OpcSafeArray.OfUInt8(new byte[] { 0xCA, 0xFE, 0xBA, 0xBE });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task Int16Array_RoundTrips() {
        var input = OpcSafeArray.OfInt16(new short[] { -1, 0, 1, short.MaxValue, short.MinValue });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task SingleArray_RoundTrips() {
        var input = OpcSafeArray.OfSingle(new[] { 0.5f, 1.5f, -2.5f });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task Int64Array_RoundTrips() {
        var input = OpcSafeArray.OfInt64(new[] { long.MinValue, 0L, long.MaxValue });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task BooleanArray_RoundTrips() {
        var input = OpcSafeArray.OfBoolean(new[] { true, false, true, true, false });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task StringArray_RoundTrips() {
        var input = OpcSafeArray.OfString(new[] { "Tag1", "Tag2", "Tag3" });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input), capacity: 256);
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task StringArray_WithNulls_RoundTrips() {
        var input = new OpcSafeArray(VarType.VT_BSTR, new string?[] { "ok", null, "also" });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input), capacity: 256);
        var read = ReadOne(bytes);
        var data = (string?[])read.Data;
        await Assert.That(data[0]).IsEqualTo("ok");
        await Assert.That(data[1]).IsNull();
        await Assert.That(data[2]).IsEqualTo("also");
    }

    [Test]
    public async Task EmptyArray_RoundTrips() {
        var input = OpcSafeArray.OfDouble(Array.Empty<double>());
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.TotalElements).IsEqualTo(0);
        await Assert.That(read.ElementType).IsEqualTo(VarType.VT_R8);
    }

    [Test]
    public async Task LowerBound_RoundTrips() {
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
    public async Task LargeDoubleArray_RoundTrips() {
        var input = OpcSafeArray.OfDouble(Enumerable.Range(0, 100).Select(i => i * 0.25).ToArray());
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input), capacity: 2048);
        await Assert.That(ReadOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task TwoDimensional_Int32Array_RoundTrips() {
        var input = new OpcSafeArray(
            VarType.VT_I4,
            new int[] { 1, 2, 3, 4, 5, 6 },
            lengths: new[] { 2, 3 },
            lowerBounds: new[] { 0, 1 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.Rank).IsEqualTo(2);
        await Assert.That(read.Lengths[0]).IsEqualTo(2);
        await Assert.That(read.Lengths[1]).IsEqualTo(3);
        await Assert.That(read.LowerBounds[1]).IsEqualTo(1);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task ThreeDimensional_DoubleArray_RoundTrips() {
        var input = new OpcSafeArray(
            VarType.VT_R8,
            new[] { 1.0d, 2.0d, 3.0d, 4.0d, 5.0d, 6.0d, 7.0d, 8.0d },
            lengths: new[] { 2, 2, 2 },
            lowerBounds: new[] { 0, -1, 4 });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.Rank).IsEqualTo(3);
        await Assert.That(read.LowerBounds[1]).IsEqualTo(-1);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task FeatureFlags_ArePreserved() {
        var input = new OpcSafeArray(
            VarType.VT_BSTR,
            new string?[] { "A", "B" },
            features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Bstr | SafeArrayFeatures.FixedSize);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSafeArray(input));
        var read = ReadOne(bytes);
        await Assert.That(read.Features).IsEqualTo(SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Bstr | SafeArrayFeatures.FixedSize);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task Reader_RejectsTooManyDimensions() {
        var bytes = new byte[4];
        BitConverter.GetBytes((ushort)257).CopyTo(bytes, 0);
        bool threw = false;
        try {
            _ = ReadOne(bytes);
        }
        catch (InvalidDataException) {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Reader_RejectsPayloadLargerThanTwoGiB() {
        const uint elementCount = 536_870_913u;
        var bytes = WriteOne((ref NdrWriter w) => {
            w.WriteUInt16(1);
            w.WriteUInt16((ushort)SafeArrayFeatures.HaveVartype);
            w.WriteUInt32(4);
            w.WriteUInt32(0);
            w.WriteUInt32(elementCount);
            w.WriteUInt16((ushort)VarType.VT_I4);
            w.WriteUInt16(0);
            w.WriteUInt32(elementCount);
            w.WriteInt32(0);
        });

        bool threw = false;
        try {
            _ = ReadOne(bytes);
        }
        catch (InvalidDataException) {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Header_HasExpectedFlags() {
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
