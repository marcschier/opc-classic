//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests;

public sealed class NdrOpcHdaTimeCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 128)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcHdaTime ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcHdaTimeCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_StringExpression()
    {
        var input = OpcHdaTime.FromString("NOW-1H");
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaTimeCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.IsStringExpression).IsTrue();
        await Assert.That(back.StringExpression).IsEqualTo("NOW-1H");
    }

    [Test]
    public async Task RoundTrip_AbsoluteTimestamp()
    {
        var ts = new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero);
        var input = OpcHdaTime.FromTimestamp(ts);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaTimeCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.IsStringExpression).IsFalse();
        await Assert.That(back.Timestamp.UtcDateTime).IsEqualTo(ts.UtcDateTime);
    }

    [Test]
    public async Task RoundTrip_EmptyStringExpression()
    {
        var input = OpcHdaTime.FromString(string.Empty);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaTimeCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.IsStringExpression).IsTrue();
        await Assert.That(back.StringExpression).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task BString_True_EmitsMinusOne()
    {
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaTimeCodec.Write(ref w, OpcHdaTime.FromString("NOW")));
        // bString is the leading Int32 — should be 0xFFFFFFFF for TRUE.
        await Assert.That(BitConverter.ToInt32(bytes, 0)).IsEqualTo(-1);
    }

    [Test]
    public async Task BString_False_EmitsZero()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
            NdrOpcHdaTimeCodec.Write(ref w, OpcHdaTime.FromTimestamp(DateTimeOffset.UnixEpoch)));
        await Assert.That(BitConverter.ToInt32(bytes, 0)).IsEqualTo(0);
    }
}
