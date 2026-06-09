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

public sealed class NdrOpcBrowseElementCodecTests {
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 2048) {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcBrowseElementResult ReadOne(byte[] bytes) {
        var r = new NdrReader(bytes);
        return NdrOpcBrowseElementCodec.Read(ref r);
    }

    private static (uint FlagValue, uint Reserved) ReadWireFlagAndReserved(byte[] bytes) {
        var r = new NdrReader(bytes);
        _ = r.ReadUnicodeStringPtr();
        _ = r.ReadUnicodeStringPtr();
        uint flagValue = r.ReadUInt32();
        uint reserved = r.ReadUInt32();
        return (flagValue, reserved);
    }

    private static OpcItemProperties EmptyProperties() =>
        new(ErrorId: 0, Properties: Array.Empty<OpcItemPropertyResult>());

    [Test]
    public async Task RoundTrip_LeafItem_EmptyProperties() {
        var input = new OpcBrowseElementResult(
            Name: "Tag1",
            ItemId: "Group1.Tag1",
            FlagValue: 2,
            Properties: EmptyProperties());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBrowseElementCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        (uint wireFlagValue, uint wireReserved) = ReadWireFlagAndReserved(bytes);

        string? name = back.Name;
        string? itemId = back.ItemId;
        int flagValue = back.FlagValue;
        bool isItem = back.IsItem;
        bool isBranch = back.IsBranch;
        int errorId = back.Properties.ErrorId;
        int propertyCount = back.Properties.Properties.Length;

        await Assert.That(name).IsEqualTo("Tag1");
        await Assert.That(itemId).IsEqualTo("Group1.Tag1");
        await Assert.That(flagValue).IsEqualTo(2);
        await Assert.That(isItem).IsEqualTo(true);
        await Assert.That(isBranch).IsEqualTo(false);
        await Assert.That(errorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(0);
        await Assert.That(wireFlagValue).IsEqualTo(2u);
        await Assert.That(wireReserved).IsEqualTo(0u);
    }

    [Test]
    public async Task RoundTrip_Branch_EmptyProperties() {
        var input = new OpcBrowseElementResult(
            Name: "Group1",
            ItemId: "Group1",
            FlagValue: 1,
            Properties: EmptyProperties());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBrowseElementCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        (uint wireFlagValue, uint wireReserved) = ReadWireFlagAndReserved(bytes);

        string? name = back.Name;
        string? itemId = back.ItemId;
        int flagValue = back.FlagValue;
        bool isBranch = back.IsBranch;
        bool isItem = back.IsItem;
        int errorId = back.Properties.ErrorId;
        int propertyCount = back.Properties.Properties.Length;

        await Assert.That(name).IsEqualTo("Group1");
        await Assert.That(itemId).IsEqualTo("Group1");
        await Assert.That(flagValue).IsEqualTo(1);
        await Assert.That(isBranch).IsEqualTo(true);
        await Assert.That(isItem).IsEqualTo(false);
        await Assert.That(errorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(0);
        await Assert.That(wireFlagValue).IsEqualTo(1u);
        await Assert.That(wireReserved).IsEqualTo(0u);
    }

    [Test]
    public async Task RoundTrip_ItemWithTwoProperties() {
        var input = new OpcBrowseElementResult(
            Name: "Tag1",
            ItemId: "Group1.Tag1",
            FlagValue: 2,
            Properties: new OpcItemProperties(
                ErrorId: 0,
                Properties:
                [
                    new OpcItemPropertyResult(
                        DataType: VarType.VT_R8,
                        PropertyId: 100,
                        ItemId: null,
                        Description: "Item Value",
                        Value: OpcVariant.FromDouble(42.5),
                        ErrorId: 0),
                    new OpcItemPropertyResult(
                        DataType: VarType.VT_I4,
                        PropertyId: 101,
                        ItemId: "Group1.Tag1.Quality",
                        Description: "Quality Code",
                        Value: OpcVariant.FromInt32(192),
                        ErrorId: 0),
                ]));
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBrowseElementCodec.Write(ref w, input), capacity: 4096);
        var back = ReadOne(bytes);
        (uint wireFlagValue, uint wireReserved) = ReadWireFlagAndReserved(bytes);

        bool isItem = back.IsItem;
        int errorId = back.Properties.ErrorId;
        int propertyCount = back.Properties.Properties.Length;
        OpcItemPropertyResult first = back.Properties.Properties[0];
        OpcItemPropertyResult second = back.Properties.Properties[1];
        int firstPropertyId = first.PropertyId;
        double? firstValue = first.Value.AsDouble();
        int secondPropertyId = second.PropertyId;
        int? secondValue = second.Value.AsInt32();
        string? secondItemId = second.ItemId;

        await Assert.That(isItem).IsEqualTo(true);
        await Assert.That(errorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(2);
        await Assert.That(firstPropertyId).IsEqualTo(100);
        await Assert.That(firstValue).IsEqualTo(42.5);
        await Assert.That(secondPropertyId).IsEqualTo(101);
        await Assert.That(secondValue).IsEqualTo(192);
        await Assert.That(secondItemId).IsEqualTo("Group1.Tag1.Quality");
        await Assert.That(wireFlagValue).IsEqualTo(2u);
        await Assert.That(wireReserved).IsEqualTo(0u);
    }

    [Test]
    public async Task RoundTrip_NullName_EmptyItemId() {
        var input = new OpcBrowseElementResult(
            Name: null,
            ItemId: string.Empty,
            FlagValue: 2,
            Properties: EmptyProperties());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBrowseElementCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        (uint wireFlagValue, uint wireReserved) = ReadWireFlagAndReserved(bytes);

        string? name = back.Name;
        string? itemId = back.ItemId;
        bool isItem = back.IsItem;
        uint wireNameReferent = BitConverter.ToUInt32(bytes, 0);
        int propertyCount = back.Properties.Properties.Length;

        await Assert.That(name).IsEqualTo(null);
        await Assert.That(itemId).IsEqualTo(string.Empty);
        await Assert.That(isItem).IsEqualTo(true);
        await Assert.That(propertyCount).IsEqualTo(0);
        await Assert.That(wireNameReferent).IsEqualTo(0u);
        await Assert.That(wireFlagValue).IsEqualTo(2u);
        await Assert.That(wireReserved).IsEqualTo(0u);
    }
}
