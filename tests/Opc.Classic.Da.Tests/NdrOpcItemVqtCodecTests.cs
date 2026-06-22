// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcItemVqtCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemVqt ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcItemVqtCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_ValueOnly_NoQualityNoTimestamp()
    {
        var input = new OpcItemVqt(OpcVariant.FromDouble(42.5));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemVqtCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Value.AsDouble()).IsEqualTo(42.5);
        await Assert.That(back.Quality).IsNull();
        await Assert.That(back.Timestamp).IsNull();
    }

    [Test]
    public async Task RoundTrip_WithQuality()
    {
        var input = new OpcItemVqt(
            OpcVariant.FromInt32(123),
            Quality: OpcQuality.Good);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemVqtCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Value.AsInt32()).IsEqualTo(123);
        await Assert.That(back.Quality).IsNotNull();
        await Assert.That(back.Quality!.Value).IsEqualTo(OpcQuality.Good);
        await Assert.That(back.Timestamp).IsNull();
    }

    [Test]
    public async Task RoundTrip_WithTimestamp()
    {
        var ts = new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero);
        var input = new OpcItemVqt(
            OpcVariant.FromInt32(7),
            Timestamp: ts);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemVqtCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Timestamp).IsNotNull();
        await Assert.That(back.Timestamp!.Value.UtcDateTime).IsEqualTo(ts.UtcDateTime);
    }

    [Test]
    public async Task RoundTrip_WithQualityAndTimestamp()
    {
        var ts = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero);
        var input = new OpcItemVqt(
            OpcVariant.FromString("hello"),
            Quality: OpcQuality.Uncertain,
            Timestamp: ts);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemVqtCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.Value.AsString()).IsEqualTo("hello");
        await Assert.That(back.Quality!.Value).IsEqualTo(OpcQuality.Uncertain);
        await Assert.That(back.Timestamp!.Value.UtcDateTime).IsEqualTo(ts.UtcDateTime);
    }

    [Test]
    public async Task QualitySpecified_WireUsesWin32BoolMinusOne()
    {
        var input = new OpcItemVqt(OpcVariant.FromInt32(1), Quality: OpcQuality.Good);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemVqtCodec.Write(ref w, input));
        // wireVARIANT for VT_I4 = 16 hdr + 4 ULONG discriminator + 4 body = 24 bytes.
        // After 24 bytes, the bQualitySpecified Int32 begins. It should be 0xFFFFFFFF (-1).
        await Assert.That(bytes[24]).IsEqualTo((byte)0xFF);
        await Assert.That(bytes[25]).IsEqualTo((byte)0xFF);
        await Assert.That(bytes[26]).IsEqualTo((byte)0xFF);
        await Assert.That(bytes[27]).IsEqualTo((byte)0xFF);
    }

    [Test]
    public async Task QualityNotSpecified_WireUsesZeroBool_AndZeroQuality()
    {
        var input = new OpcItemVqt(OpcVariant.FromInt32(1));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemVqtCodec.Write(ref w, input));
        // wireVARIANT for VT_I4 = 24 bytes (header + discriminator + body).
        // bQualitySpecified at byte 24 should be 0
        await Assert.That(bytes[24]).IsEqualTo((byte)0);
        await Assert.That(bytes[25]).IsEqualTo((byte)0);
        await Assert.That(bytes[26]).IsEqualTo((byte)0);
        await Assert.That(bytes[27]).IsEqualTo((byte)0);
        // wQuality (bytes 28-29) also 0
        await Assert.That(bytes[28]).IsEqualTo((byte)0);
        await Assert.That(bytes[25]).IsEqualTo((byte)0);
    }
}
