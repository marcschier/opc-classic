//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests;

public sealed class NdrOpcEventNotificationCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcEventNotification ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcEventNotificationCodec.Read(ref r);
    }

    private static OpcEventNotification MakeNotification(
        bool ackRequired = true,
        OpcVariant[]? eventAttributes = null,
        string? source = "Boiler.Area1.Pump7",
        string? message = "Pump discharge pressure high",
        string? conditionName = "PressureHigh",
        string? subconditionName = "HighHigh",
        string? actorId = "operator1") =>
        new(
            changeMask: 0x0003,
            newState: 0x0005,
            source: source,
            time: new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero),
            message: message,
            eventType: 0x0004,
            eventCategory: 17,
            severity: 900,
            conditionName: conditionName,
            subconditionName: subconditionName,
            quality: new OpcQuality(0x00C0),
            ackRequired: ackRequired,
            activeTime: new DateTimeOffset(2026, 5, 22, 10, 25, 0, TimeSpan.Zero),
            cookie: 0x12345678,
            eventAttributes: eventAttributes ?? new[]
            {
                OpcVariant.FromInt32(42),
                OpcVariant.FromString("high-high"),
                OpcVariant.FromBoolean(true),
            },
            actorId: actorId);

    [Test]
    public async Task RoundTrip_TypicalAlarmEventWithAttributes()
    {
        var input = MakeNotification();
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcEventNotificationCodec.Write(ref w, input), 2048);
        var back = ReadOne(bytes);

        await Assert.That(back.ChangeMask).IsEqualTo((ushort)0x0003);
        await Assert.That(back.NewState).IsEqualTo((ushort)0x0005);
        await Assert.That(back.Source).IsEqualTo("Boiler.Area1.Pump7");
        await Assert.That(back.Message).IsEqualTo("Pump discharge pressure high");
        await Assert.That(back.EventType).IsEqualTo(0x0004u);
        await Assert.That(back.EventCategory).IsEqualTo(17u);
        await Assert.That(back.Severity).IsEqualTo(900u);
        await Assert.That(back.ConditionName).IsEqualTo("PressureHigh");
        await Assert.That(back.SubconditionName).IsEqualTo("HighHigh");
        await Assert.That(back.Quality.RawValue).IsEqualTo((ushort)0x00C0);
        await Assert.That(back.AckRequired).IsTrue();
        await Assert.That(back.Cookie).IsEqualTo(0x12345678u);
        await Assert.That(back.ActorId).IsEqualTo("operator1");
        await Assert.That(back.EventAttributes.Length).IsEqualTo(3);
        await Assert.That(back.EventAttributes[0].AsInt32()).IsEqualTo(42);
        await Assert.That(back.EventAttributes[1].AsString()).IsEqualTo("high-high");
        await Assert.That(back.EventAttributes[2].AsBoolean()).IsEqualTo(true);
    }

    [Test]
    public async Task RoundTrip_EmptyAttributes()
    {
        var input = MakeNotification(eventAttributes: Array.Empty<OpcVariant>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcEventNotificationCodec.Write(ref w, input), 2048);
        var back = ReadOne(bytes);

        await Assert.That(back.EventAttributes.Length).IsEqualTo(0);
        await Assert.That(back.Source).IsEqualTo(input.Source);
        await Assert.That(back.ActorId).IsEqualTo(input.ActorId);
    }

    [Test]
    public async Task RoundTrip_NullOptionalStrings()
    {
        var input = MakeNotification(
            eventAttributes: new[] { OpcVariant.Empty },
            source: null,
            message: null,
            conditionName: null,
            subconditionName: null,
            actorId: null);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcEventNotificationCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back.Source).IsNull();
        await Assert.That(back.Message).IsNull();
        await Assert.That(back.ConditionName).IsNull();
        await Assert.That(back.SubconditionName).IsNull();
        await Assert.That(back.ActorId).IsNull();
        await Assert.That(back.EventAttributes.Length).IsEqualTo(1);
        await Assert.That(back.EventAttributes[0]).IsEqualTo(OpcVariant.Empty);
    }

    [Test]
    public async Task RoundTrip_AckRequiredFalse()
    {
        var input = MakeNotification(ackRequired: false);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcEventNotificationCodec.Write(ref w, input), 2048);
        var back = ReadOne(bytes);

        await Assert.That(back.AckRequired).IsFalse();
    }

    [Test]
    [Arguments(true, 0xFF)]
    [Arguments(false, 0x00)]
    public async Task AckRequired_WireUsesWin32BoolMinusOneOrZero(bool ackRequired, int expectedByte)
    {
        var input = MakeNotification(ackRequired: ackRequired, eventAttributes: Array.Empty<OpcVariant>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcEventNotificationCodec.Write(ref w, input), 2048);
        int offset = FindAckRequiredOffset(bytes);
        byte expected = unchecked((byte)expectedByte);

        await Assert.That(bytes[offset]).IsEqualTo(expected);
        await Assert.That(bytes[offset + 1]).IsEqualTo(expected);
        await Assert.That(bytes[offset + 2]).IsEqualTo(expected);
        await Assert.That(bytes[offset + 3]).IsEqualTo(expected);
    }

    private static int FindAckRequiredOffset(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        _ = r.ReadUInt16();
        _ = r.ReadUInt16();
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadFileTime();
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadUInt32();
        _ = r.ReadUInt32();
        _ = r.ReadUInt32();
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadUInt16();
        _ = r.ReadUInt16();
        r.AlignTo(4);
        return r.Position;
    }
}
