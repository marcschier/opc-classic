//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcItemStateCodecTests {
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256) {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemState ReadOne(byte[] bytes) {
        var r = new NdrReader(bytes);
        return NdrOpcItemStateCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_DoubleValue() {
        var input = new OpcItemState(
            ClientHandle: 42,
            Timestamp: new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero),
            Quality: OpcQuality.Good,
            Value: OpcVariant.FromDouble(123.456));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemStateCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.ClientHandle).IsEqualTo(42);
        await Assert.That(back.Quality).IsEqualTo(OpcQuality.Good);
        await Assert.That(back.Value.AsDouble()).IsEqualTo(123.456);
        await Assert.That(back.Timestamp.UtcDateTime).IsEqualTo(input.Timestamp.UtcDateTime);
    }

    [Test]
    public async Task RoundTrip_StringValue() {
        var input = new OpcItemState(
            ClientHandle: 7,
            Timestamp: new DateTimeOffset(2026, 5, 22, 12, 30, 0, TimeSpan.Zero),
            Quality: OpcQuality.Uncertain,
            Value: OpcVariant.FromString("running"));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemStateCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.Value.AsString()).IsEqualTo("running");
        await Assert.That(back.Quality).IsEqualTo(OpcQuality.Uncertain);
    }

    [Test]
    public async Task RoundTrip_BadQuality_NoValue() {
        var input = new OpcItemState(
            ClientHandle: 99,
            Timestamp: new DateTimeOffset(2026, 5, 22, 0, 0, 0, TimeSpan.Zero),
            Quality: OpcQuality.Bad,
            Value: OpcVariant.Empty);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemStateCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Quality).IsEqualTo(OpcQuality.Bad);
        await Assert.That(back.Value.IsEmpty).IsTrue();
    }

    [Test]
    public async Task ClientHandle_Layout_AtOffsetZero() {
        var input = new OpcItemState(
            ClientHandle: unchecked((int)0xCAFEBABE),
            Timestamp: new DateTimeOffset(2026, 5, 22, 0, 0, 0, TimeSpan.Zero),
            Quality: OpcQuality.Good,
            Value: OpcVariant.FromInt32(1));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemStateCodec.Write(ref w, input));
        // hClient is little-endian uint at offset 0
        await Assert.That(BitConverter.ToUInt32(bytes, 0)).IsEqualTo(0xCAFEBABEu);
    }

    [Test]
    public async Task FileTimeFields_OccupyEightBytesAfterHandle() {
        var input = new OpcItemState(
            ClientHandle: 1,
            Timestamp: DateTimeOffset.UnixEpoch,
            Quality: OpcQuality.Good,
            Value: OpcVariant.FromInt32(1));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemStateCodec.Write(ref w, input));
        // bytes 4..11 are the FILETIME pair (two UInt32s for the unix epoch)
        // 1970-01-01 UTC = 11644473600 seconds since 1601 = 116444736000000000 100-ns ticks
        long expected = 116444736000000000L;
        uint low = (uint)(expected & 0xFFFFFFFFu);
        uint high = (uint)(expected >> 32);
        await Assert.That(BitConverter.ToUInt32(bytes, 4)).IsEqualTo(low);
        await Assert.That(BitConverter.ToUInt32(bytes, 8)).IsEqualTo(high);
    }

    [Test]
    public async Task Quality_LowWordOnly_OnWire() {
        var quality = new OpcQuality(0xAB_C0);  // vendor=0xAB in high byte, kind=Good in low byte
        var input = new OpcItemState(
            ClientHandle: 0,
            Timestamp: DateTimeOffset.UnixEpoch,
            Quality: quality,
            Value: OpcVariant.FromInt32(1));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemStateCodec.Write(ref w, input));
        // wQuality at offset 12 (4 hClient + 8 FILETIME)
        ushort wireQuality = BitConverter.ToUInt16(bytes, 12);
        await Assert.That((int)wireQuality).IsEqualTo(0xABC0);
    }
}
