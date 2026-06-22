// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcItemPropertiesCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemProperties ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcItemPropertiesCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_ZeroProperties_Success()
    {
        var input = new OpcItemProperties(
            ErrorId: 0,
            Properties: Array.Empty<OpcItemPropertyResult>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertiesCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        int errorId = back.ErrorId;
        int propertyCount = back.Properties.Length;
        int wireLength = bytes.Length;
        uint wireErrorId = BitConverter.ToUInt32(bytes, 0);
        uint wireNumProperties = BitConverter.ToUInt32(bytes, 4);
        uint wireArrayCount = BitConverter.ToUInt32(bytes, 8);
        uint wireReserved = BitConverter.ToUInt32(bytes, 12);

        await Assert.That(errorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(0);
        await Assert.That(wireLength).IsEqualTo(16);
        await Assert.That(wireErrorId).IsEqualTo(0u);
        await Assert.That(wireNumProperties).IsEqualTo(0u);
        await Assert.That(wireArrayCount).IsEqualTo(0u);
        await Assert.That(wireReserved).IsEqualTo(0u);
    }

    [Test]
    public async Task RoundTrip_TwoSuccessProperties()
    {
        var input = new OpcItemProperties(
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
                    ItemId: "Sensor.Temperature.Quality",
                    Description: "Quality Code",
                    Value: OpcVariant.FromInt32(192),
                    ErrorId: 0),
            ]);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertiesCodec.Write(ref w, input), capacity: 2048);
        var back = ReadOne(bytes);

        int errorId = back.ErrorId;
        int propertyCount = back.Properties.Length;
        uint wireNumProperties = BitConverter.ToUInt32(bytes, 4);
        uint wireArrayCount = BitConverter.ToUInt32(bytes, 8);
        OpcItemPropertyResult first = back.Properties[0];
        OpcItemPropertyResult second = back.Properties[1];
        int firstPropertyId = first.PropertyId;
        double? firstValue = first.Value.AsDouble();
        int secondPropertyId = second.PropertyId;
        int? secondValue = second.Value.AsInt32();
        string? secondItemId = second.ItemId;

        await Assert.That(errorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(2);
        await Assert.That(wireNumProperties).IsEqualTo(2u);
        await Assert.That(wireArrayCount).IsEqualTo(2u);
        await Assert.That(firstPropertyId).IsEqualTo(100);
        await Assert.That(firstValue).IsEqualTo(42.5);
        await Assert.That(secondPropertyId).IsEqualTo(101);
        await Assert.That(secondValue).IsEqualTo(192);
        await Assert.That(secondItemId).IsEqualTo("Sensor.Temperature.Quality");
    }

    [Test]
    public async Task Read_UsesPItemPropertiesConformanceCount_WhenDwNumPropertiesDiffers()
    {
        OpcItemPropertyResult property = new(
            DataType: VarType.VT_I4,
            PropertyId: 100,
            ItemId: null,
            Description: null,
            Value: OpcVariant.FromInt32(88),
            ErrorId: 0);
        byte[] bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteInt32(0);
            w.WriteUInt32(0);
            w.WriteUInt32(1);
            NdrOpcItemPropertyCodec.Write(ref w, property);
            w.WriteUInt32(0);
        }, capacity: 2048);

        OpcItemProperties back = ReadOne(bytes);

        await Assert.That(back.Properties.Length).IsEqualTo(1);
        await Assert.That(back.Properties[0].PropertyId).IsEqualTo(100);
        await Assert.That(back.Properties[0].Value.AsInt32()).IsEqualTo(88);
    }

    [Test]
    public async Task RoundTrip_MixedPerElementErrorCodes()
    {
        int invalidPropertyId = unchecked((int)0xC0040203u);
        var input = new OpcItemProperties(
            ErrorId: 0,
            Properties:
            [
                new OpcItemPropertyResult(
                    DataType: VarType.VT_BSTR,
                    PropertyId: 600,
                    ItemId: null,
                    Description: "Engineering Units",
                    Value: OpcVariant.FromString("degC"),
                    ErrorId: 0),
                new OpcItemPropertyResult(
                    DataType: VarType.VT_EMPTY,
                    PropertyId: 9999,
                    ItemId: null,
                    Description: null,
                    Value: OpcVariant.Empty,
                    ErrorId: invalidPropertyId),
                new OpcItemPropertyResult(
                    DataType: VarType.VT_I4,
                    PropertyId: 102,
                    ItemId: null,
                    Description: "Timestamp Substatus",
                    Value: OpcVariant.FromInt32(7),
                    ErrorId: 0),
            ]);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertiesCodec.Write(ref w, input), capacity: 2048);
        var back = ReadOne(bytes);

        int errorId = back.ErrorId;
        int propertyCount = back.Properties.Length;
        string? firstValue = back.Properties[0].Value.AsString();
        int secondErrorId = back.Properties[1].ErrorId;
        bool secondIsEmpty = back.Properties[1].Value.IsEmpty;
        int? thirdValue = back.Properties[2].Value.AsInt32();

        await Assert.That(errorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(3);
        await Assert.That(firstValue).IsEqualTo("degC");
        await Assert.That(secondErrorId).IsEqualTo(invalidPropertyId);
        await Assert.That(secondIsEmpty).IsEqualTo(true);
        await Assert.That(thirdValue).IsEqualTo(7);
    }

    [Test]
    public async Task RoundTrip_UnknownItemId_WithEmptyProperties()
    {
        int unknownItemId = unchecked((int)0xC0040007u);
        var input = new OpcItemProperties(
            ErrorId: unknownItemId,
            Properties: Array.Empty<OpcItemPropertyResult>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemPropertiesCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        int errorId = back.ErrorId;
        int propertyCount = back.Properties.Length;
        uint wireErrorId = BitConverter.ToUInt32(bytes, 0);
        uint wireReserved = BitConverter.ToUInt32(bytes, 12);

        await Assert.That(errorId).IsEqualTo(unknownItemId);
        await Assert.That(propertyCount).IsEqualTo(0);
        await Assert.That(wireErrorId).IsEqualTo(0xC0040007u);
        await Assert.That(wireReserved).IsEqualTo(0u);
    }
}
