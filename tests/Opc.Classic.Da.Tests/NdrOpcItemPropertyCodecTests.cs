//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcItemPropertyCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemPropertyResult ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcItemPropertyCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_DoubleValueProperty()
    {
        var input = new OpcItemPropertyResult(
            DataType: VarType.VT_R8,
            PropertyId: 100,
            ItemId: null,
            Description: "Item Value",
            Value: OpcVariant.FromDouble(42.5),
            ErrorId: 0);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertyCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.PropertyId).IsEqualTo(100);
        await Assert.That(back.DataType).IsEqualTo(VarType.VT_R8);
        await Assert.That(back.Value.AsDouble()).IsEqualTo(42.5);
        await Assert.That(back.Description).IsEqualTo("Item Value");
        await Assert.That(back.ErrorId).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_BstrValueProperty()
    {
        var input = new OpcItemPropertyResult(
            DataType: VarType.VT_BSTR,
            PropertyId: 600,
            ItemId: null,
            Description: "Engineering Units",
            Value: OpcVariant.FromString("degC"),
            ErrorId: 0);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertyCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.Value.AsString()).IsEqualTo("degC");
    }

    [Test]
    public async Task RoundTrip_ErrorId_InvalidPid()
    {
        var input = new OpcItemPropertyResult(
            DataType: VarType.VT_EMPTY,
            PropertyId: 9999,
            ItemId: null,
            Description: null,
            Value: OpcVariant.Empty,
            ErrorId: unchecked((int)0xC0040203u));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertyCodec.Write(ref w, input), capacity: 256);
        var back = ReadOne(bytes);
        await Assert.That(back.ErrorId).IsEqualTo(unchecked((int)0xC0040203u));
        await Assert.That(back.Value.IsEmpty).IsTrue();
    }

    [Test]
    public async Task RoundTrip_EmptyDescription()
    {
        var input = new OpcItemPropertyResult(
            DataType: VarType.VT_I4,
            PropertyId: 5,
            ItemId: null,
            Description: string.Empty,
            Value: OpcVariant.FromInt32(7),
            ErrorId: 0);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertyCodec.Write(ref w, input), capacity: 256);
        var back = ReadOne(bytes);
        await Assert.That(back.Description).IsEqualTo(string.Empty);
        await Assert.That(back.Value.AsInt32()).IsEqualTo(7);
    }

    [Test]
    public async Task RoundTrip_WithIndirectItemId()
    {
        var input = new OpcItemPropertyResult(
            DataType: VarType.VT_BSTR,
            PropertyId: 0x12345,
            ItemId: "Sensor.Temperature.HighLimit",
            Description: "High limit indirect",
            Value: OpcVariant.FromString("ref"),
            ErrorId: 0);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertyCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.ItemId).IsEqualTo("Sensor.Temperature.HighLimit");
        await Assert.That(back.PropertyId).IsEqualTo(0x12345);
    }
}
