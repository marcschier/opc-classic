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

public sealed class NdrOpcHdaAnnotationCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 4096)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcHdaAnnotation ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcHdaAnnotationCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TwoAnnotationSeries_WithUnicodeUsers()
    {
        var input = new OpcHdaAnnotation(
            clientHandle: 42,
            timestamps: new[]
            {
                new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero),
            },
            annotations: new[] { "Started – café", "温度 stable ✅" },
            annotationTimes: new[]
            {
                new DateTimeOffset(2026, 5, 22, 10, 0, 5, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 10, 1, 5, TimeSpan.Zero),
            },
            users: new[] { "José Ångström", "山田太郎" });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaAnnotationCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.ClientHandle).IsEqualTo(42);
        await Assert.That(back.Timestamps.Length).IsEqualTo(2);
        await Assert.That(back.Timestamps[0]).IsEqualTo(input.Timestamps[0]);
        await Assert.That(back.Annotations[0]).IsEqualTo("Started – café");
        await Assert.That(back.Annotations[1]).IsEqualTo("温度 stable ✅");
        await Assert.That(back.AnnotationTimes[1]).IsEqualTo(input.AnnotationTimes[1]);
        await Assert.That(back.Users[0]).IsEqualTo("José Ångström");
        await Assert.That(back.Users[1]).IsEqualTo("山田太郎");
    }

    [Test]
    public async Task RoundTrip_EmptySeries()
    {
        var input = new OpcHdaAnnotation(
            clientHandle: 1,
            timestamps: Array.Empty<DateTimeOffset>(),
            annotations: Array.Empty<string?>(),
            annotationTimes: Array.Empty<DateTimeOffset>(),
            users: Array.Empty<string?>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaAnnotationCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.ClientHandle).IsEqualTo(1);
        await Assert.That(back.Timestamps.Length).IsEqualTo(0);
        await Assert.That(back.Annotations.Length).IsEqualTo(0);
        await Assert.That(back.AnnotationTimes.Length).IsEqualTo(0);
        await Assert.That(back.Users.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_WithNullUserNames()
    {
        var input = new OpcHdaAnnotation(
            clientHandle: 7,
            timestamps: new[]
            {
                new DateTimeOffset(2026, 5, 22, 11, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 11, 1, 0, TimeSpan.Zero),
            },
            annotations: new[] { "operator note", null },
            annotationTimes: new[]
            {
                new DateTimeOffset(2026, 5, 22, 11, 0, 10, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 22, 11, 1, 10, TimeSpan.Zero),
            },
            users: new string?[] { null, "historian" });
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcHdaAnnotationCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Annotations[0]).IsEqualTo("operator note");
        await Assert.That(back.Annotations[1]).IsNull();
        await Assert.That(back.Users[0]).IsNull();
        await Assert.That(back.Users[1]).IsEqualTo("historian");
    }

    [Test]
    public async Task ConstructorRejectsArrayLengthMismatch()
    {
        bool threw = false;
        try
        {
            _ = new OpcHdaAnnotation(
                clientHandle: 1,
                timestamps: new DateTimeOffset[2],
                annotations: new string?[2],
                annotationTimes: new DateTimeOffset[1],
                users: new string?[2]);
        }
        catch (ArgumentException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
