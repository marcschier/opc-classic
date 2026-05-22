//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Linq;
using OpcClassic;
using OpcClassic.Da.Ndr;
using OpcClassic.Ndr;
using TUnit.Core;

namespace OpcClassic.Da.Tests;

public sealed class NdrOpcItemAttributesCodecTests
{
    private const int OpcEuTypeNone = 0;
    private const int OpcEuTypeAnalog = 1;

    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemAttributes ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcItemAttributesCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TypicalActiveDoubleItem()
    {
        var input = new OpcItemAttributes(
            AccessPath: string.Empty,
            ItemId: "Channel1.Device1.Tag1",
            Active: true,
            ClientHandle: 42,
            ServerHandle: 12345,
            AccessRights: 3,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_EMPTY,
            CanonicalDataType: VarType.VT_R8,
            EUType: OpcEuTypeNone,
            EUInfo: OpcVariant.Empty);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemAttributesCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back.AccessPath).IsEqualTo(string.Empty);
        await Assert.That(back.ItemId).IsEqualTo("Channel1.Device1.Tag1");
        await Assert.That(back.Active).IsTrue();
        await Assert.That(back.ClientHandle).IsEqualTo(42);
        await Assert.That(back.ServerHandle).IsEqualTo(12345);
        await Assert.That(back.AccessRights).IsEqualTo(3);
        await Assert.That(back.RequestedDataType).IsEqualTo(VarType.VT_EMPTY);
        await Assert.That(back.CanonicalDataType).IsEqualTo(VarType.VT_R8);
        await Assert.That(back.EUType).IsEqualTo(OpcEuTypeNone);
        await Assert.That(back.EUInfo).IsEqualTo(OpcVariant.Empty);
    }

    [Test]
    public async Task RoundTrip_AnalogEuRangeSafeArray()
    {
        var range = OpcSafeArray.OfDouble(new[] { 0.0, 100.0 });
        var euInfo = OpcVariant.FromSafeArray(range);
        var input = new OpcItemAttributes(
            AccessPath: string.Empty,
            ItemId: "Analog.Tag",
            Active: true,
            ClientHandle: 7,
            ServerHandle: 70,
            AccessRights: 1,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_R8,
            CanonicalDataType: VarType.VT_R8,
            EUType: OpcEuTypeAnalog,
            EUInfo: euInfo);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemAttributesCodec.Write(ref w, input), capacity: 2048);
        var back = ReadOne(bytes);
        var backRange = back.EUInfo.AsSafeArray();

        await Assert.That(back.EUType).IsEqualTo(OpcEuTypeAnalog);
        await Assert.That(back.EUInfo.Type).IsEqualTo((VarType)((ushort)VarType.VT_ARRAY | (ushort)VarType.VT_R8));
        await Assert.That(backRange is not null).IsTrue();
        await Assert.That(backRange!.ElementType).IsEqualTo(VarType.VT_R8);
        await Assert.That(((double[])backRange.Data).SequenceEqual(new[] { 0.0, 100.0 })).IsTrue();
    }

    [Test]
    public async Task RoundTrip_NullStringsAndEmptyBlob()
    {
        var input = new OpcItemAttributes(
            AccessPath: null,
            ItemId: null,
            Active: false,
            ClientHandle: 1,
            ServerHandle: 2,
            AccessRights: 0,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_EMPTY,
            CanonicalDataType: VarType.VT_I4,
            EUType: OpcEuTypeNone,
            EUInfo: OpcVariant.Empty);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemAttributesCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back.AccessPath).IsNull();
        await Assert.That(back.ItemId).IsNull();
        await Assert.That(back.Active).IsFalse();
        await Assert.That(back.Blob.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_LargeBlobAndAccessRightsBitmask()
    {
        byte[] blob = Enumerable.Range(0, 256)
            .Select(i => unchecked((byte)((i * 37 + 13) & 0xFF)))
            .ToArray();
        const int accessRights = unchecked((int)0x80000003u);
        var input = new OpcItemAttributes(
            AccessPath: "Root",
            ItemId: "Blob.Tag",
            Active: true,
            ClientHandle: 10,
            ServerHandle: 20,
            AccessRights: accessRights,
            Blob: blob,
            RequestedDataType: VarType.VT_BSTR,
            CanonicalDataType: VarType.VT_BSTR,
            EUType: OpcEuTypeNone,
            EUInfo: OpcVariant.Empty);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemAttributesCodec.Write(ref w, input), capacity: 4096);
        var back = ReadOne(bytes);

        await Assert.That(back.AccessRights).IsEqualTo(accessRights);
        await Assert.That(back.Blob.SequenceEqual(blob)).IsTrue();
    }

    [Test]
    public async Task ClientHandle_LayoutFollowsTwoLpwstrsAndWin32Bool()
    {
        var input = new OpcItemAttributes(
            AccessPath: "AP",
            ItemId: "ID",
            Active: true,
            ClientHandle: unchecked((int)0xCAFEBABEu),
            ServerHandle: 0,
            AccessRights: 0,
            Blob: Array.Empty<byte>(),
            RequestedDataType: VarType.VT_EMPTY,
            CanonicalDataType: VarType.VT_I4,
            EUType: OpcEuTypeNone,
            EUInfo: OpcVariant.Empty);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemAttributesCodec.Write(ref w, input));
        (int activeWire, uint clientHandleWire) = ReadActiveAndClientHandleAfterStrings(bytes);

        await Assert.That(activeWire).IsEqualTo(-1);
        await Assert.That(clientHandleWire).IsEqualTo(0xCAFEBABEu);
    }

    [Test]
    public async Task DataTypeVtypes_AreConsecutiveUInt16sWithoutReservedPadding()
    {
        var input = new OpcItemAttributes(
            AccessPath: null,
            ItemId: null,
            Active: false,
            ClientHandle: 0,
            ServerHandle: 0,
            AccessRights: 0,
            Blob: new byte[] { 0xAA },
            RequestedDataType: VarType.VT_I2,
            CanonicalDataType: VarType.VT_R8,
            EUType: OpcEuTypeNone,
            EUInfo: OpcVariant.Empty);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemAttributesCodec.Write(ref w, input));
        (ushort requested, ushort canonical, int requestedOffset, int canonicalOffset) = ReadDataTypeVtypes(bytes);

        await Assert.That(requested).IsEqualTo((ushort)VarType.VT_I2);
        await Assert.That(canonical).IsEqualTo((ushort)VarType.VT_R8);
        await Assert.That(canonicalOffset - requestedOffset).IsEqualTo(2);
    }

    private static (int ActiveWire, uint ClientHandleWire) ReadActiveAndClientHandleAfterStrings(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadUnicodeStringPtr();
        int activeWire = r.ReadInt32();
        uint clientHandleWire = r.ReadUInt32();
        return (activeWire, clientHandleWire);
    }

    private static (ushort Requested, ushort Canonical, int RequestedOffset, int CanonicalOffset) ReadDataTypeVtypes(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadInt32();
        _ = r.ReadUInt32();
        _ = r.ReadUInt32();
        _ = r.ReadUInt32();
        _ = r.ReadConformantByteArray();
        r.AlignTo(2);
        int requestedOffset = r.Position;
        ushort requested = r.ReadUInt16();
        int canonicalOffset = r.Position;
        ushort canonical = r.ReadUInt16();
        return (requested, canonical, requestedOffset, canonicalOffset);
    }
}
