//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Tests;

public sealed class NdrOpcHdaAttributeCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcHdaAttribute ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcHdaAttributeCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TwoIntValues()
    {
        var input = new OpcHdaAttribute(
            clientHandle: 42,
            attributeId: 1,
            timestamps: new[]
            {
                new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero),
            },
            values: new[] { OpcVariant.FromInt32(100), OpcVariant.FromInt32(200) });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaAttributeCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.AttributeId).IsEqualTo(1);
        await Assert.That(back.Timestamps.Length).IsEqualTo(2);
        await Assert.That(back.Values[0].AsInt32()).IsEqualTo(100);
        await Assert.That(back.Values[1].AsInt32()).IsEqualTo(200);
    }

    [Test]
    public async Task RoundTrip_Empty()
    {
        var input = new OpcHdaAttribute(
            clientHandle: 1,
            attributeId: 2,
            timestamps: Array.Empty<DateTimeOffset>(),
            values: Array.Empty<OpcVariant>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaAttributeCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Timestamps.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_WithBstrValues()
    {
        var input = new OpcHdaAttribute(
            clientHandle: 1,
            attributeId: 2,
            timestamps: new[] { new DateTimeOffset(2026, 5, 22, 0, 0, 0, TimeSpan.Zero) },
            values: new[] { OpcVariant.FromString("running") });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaAttributeCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Values[0].AsString()).IsEqualTo("running");
    }

    [Test]
    public async Task ConstructorRejectsArrayLengthMismatch()
    {
        bool threw = false;
        try
        {
            _ = new OpcHdaAttribute(
                clientHandle: 1,
                attributeId: 1,
                timestamps: new DateTimeOffset[3],
                values: new OpcVariant[2]);
        }
        catch (ArgumentException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
