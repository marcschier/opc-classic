//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using Opc.Classic;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcItemDefCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemDef ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcItemDefCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TypicalActiveItem()
    {
        var input = new OpcItemDef(
            AccessPath: string.Empty,
            ItemId: "Channel1.Device1.Tag1",
            Active: true,
            ClientHandle: 42,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_R8);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.ItemId).IsEqualTo("Channel1.Device1.Tag1");
        await Assert.That(back.Active).IsTrue();
        await Assert.That(back.ClientHandle).IsEqualTo(42);
        await Assert.That(back.RequestedDataType).IsEqualTo(VarType.VT_R8);
    }

    [Test]
    public async Task ConformantArray_RoundTripsDeferredPointerPile()
    {
        OpcItemDef[] input =
        [
            new OpcItemDef(null, "Bucket Brigade.Int4", true, 1, [], VarType.VT_EMPTY),
            new OpcItemDef(null, "Bucket Brigade.String", true, 2, [], VarType.VT_EMPTY),
        ];
        byte[] bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.WriteConformantArray(ref w, input), capacity: 1024);
        var reader = new NdrReader(bytes);

        OpcItemDef[] back = NdrOpcItemDefCodec.ReadConformantArray(ref reader);

        await Assert.That(back.Length).IsEqualTo(2);
        await Assert.That(back[0].ItemId).IsEqualTo("Bucket Brigade.Int4");
        await Assert.That(back[1].ItemId).IsEqualTo("Bucket Brigade.String");
        await Assert.That(back[1].ClientHandle).IsEqualTo(2);
    }

    [Test]
    public async Task RoundTrip_NullAccessPath()
    {
        var input = new OpcItemDef(
            AccessPath: null,
            ItemId: "Tag1",
            Active: false,
            ClientHandle: 1,
            Blob: null,
            RequestedDataType: VarType.VT_EMPTY);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.AccessPath).IsNull();
        await Assert.That(back.Active).IsFalse();
        await Assert.That(back.Blob!.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_NonAsciiItemId()
    {
        var input = new OpcItemDef(
            AccessPath: string.Empty,
            ItemId: "Müller.Device.温度",
            Active: true,
            ClientHandle: 7,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_BSTR);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.ItemId).IsEqualTo("Müller.Device.温度");
    }

    [Test]
    public async Task ActiveTrue_EmitsWin32BoolOne_OnWire()
    {
        var input = new OpcItemDef(
            AccessPath: string.Empty,
            ItemId: "T",
            Active: true,
            ClientHandle: 0,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_I4);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.Write(ref w, input), capacity: 256);
        var reader = new NdrReader(bytes);
        _ = reader.ReadUnicodeStringPtr();
        _ = reader.ReadUnicodeStringPtr();

        await Assert.That(reader.ReadInt32()).IsEqualTo(1);
    }

    [Test]
    public async Task ActiveFalse_RoundTrip()
    {
        var input = new OpcItemDef(
            AccessPath: string.Empty,
            ItemId: "T",
            Active: false,
            ClientHandle: 0,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_I4);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.Write(ref w, input), capacity: 256);
        await Assert.That(ReadOne(bytes).Active).IsFalse();
    }

    [Test]
    public async Task RoundTrip_WithBlobPayload()
    {
        var blob = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var input = new OpcItemDef(
            AccessPath: string.Empty,
            ItemId: "Tag",
            Active: true,
            ClientHandle: 1,
            Blob: blob,
            RequestedDataType: VarType.VT_I4);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemDefCodec.Write(ref w, input), capacity: 256);
        var back = ReadOne(bytes);
        await Assert.That(back.Blob!.SequenceEqual(blob)).IsTrue();
    }
}
