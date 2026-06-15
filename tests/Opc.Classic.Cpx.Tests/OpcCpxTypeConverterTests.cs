//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcCpxTypeConverterTests
{
    [Test]
    public async Task Convert_BooleanToInt32_ReturnsOneOrZero()
    {
        var trueResult = OpcCpxTypeConverter.Convert(true, TypeKind.Boolean, TypeKind.Int32);
        var falseResult = OpcCpxTypeConverter.Convert(false, TypeKind.Boolean, TypeKind.Int32);

        await Assert.That(trueResult.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(trueResult.Value).IsEqualTo(1);
        await Assert.That(falseResult.Value).IsEqualTo(0);
    }

    [Test]
    public async Task Convert_Int8ToInt32_PreservesSignedValue()
    {
        var result = OpcCpxTypeConverter.Convert((sbyte)-12, TypeKind.Int8, TypeKind.Int32);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(-12);
    }

    [Test]
    public async Task Convert_UInt8ToInt32_PreservesUnsignedValue()
    {
        var result = OpcCpxTypeConverter.Convert((byte)250, TypeKind.UInt8, TypeKind.Int32);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(250);
    }

    [Test]
    public async Task Convert_Int32ToInt64_PreservesValue()
    {
        var result = OpcCpxTypeConverter.Convert(123456, TypeKind.Int32, TypeKind.Int64);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(123456L);
    }

    [Test]
    public async Task Convert_SingleToDouble_PreservesValue()
    {
        var result = OpcCpxTypeConverter.Convert(12.5f, TypeKind.Single, TypeKind.Double);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(12.5d);
    }

    [Test]
    public async Task Convert_StringToInt32_ParsesNumericText()
    {
        var result = OpcCpxTypeConverter.Convert("42", TypeKind.String, TypeKind.Int32);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_UnsupportedConversion_ReturnsTypeChanged()
    {
        var result = OpcCpxTypeConverter.Convert(Guid.Empty, TypeKind.Guid, TypeKind.Int32);

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);
        await Assert.That(result.Value).IsNull();
    }

    [Test]
    public async Task Convert_ComplexValue_ConvertsMatchingFields()
    {
        var sourceType = new TypeDescription(
            "SourceStatus",
            "SourceStatus",
            TypeKind.StructReference,
            true,
            new[]
            {
                new TypeField("Running", TypeKind.Boolean),
                new TypeField("Code", TypeKind.UInt8),
            });
        var requestedType = new TypeDescription(
            "RequestedStatus",
            "RequestedStatus",
            TypeKind.StructReference,
            true,
            new[]
            {
                new TypeField("Running", TypeKind.Int32),
                new TypeField("Code", TypeKind.Int32),
            });
        var sourceValue = new ComplexValue
        {
            Type = new StructType { Name = "SourceStatus" },
            Fields = new Dictionary<string, object?>
            {
                ["Running"] = true,
                ["Code"] = (byte)7,
            },
        };

        var result = OpcCpxTypeConverter.Convert(sourceValue, sourceType, requestedType);
        var converted = (ComplexValue)result.Value!;

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(converted.Fields["Running"]).IsEqualTo(1);
        await Assert.That(converted.Fields["Code"]).IsEqualTo(7);
    }

    [Test]
    public async Task Convert_ComplexValue_MissingField_ReturnsTypeChanged()
    {
        var sourceType = new TypeDescription("Source", "Source", TypeKind.StructReference, true, new[] { new TypeField("A", TypeKind.Int8) });
        var requestedType = new TypeDescription("Requested", "Requested", TypeKind.StructReference, true, new[] { new TypeField("B", TypeKind.Int32) });
        var sourceValue = new ComplexValue
        {
            Type = new StructType { Name = "Source" },
            Fields = new Dictionary<string, object?> { ["A"] = (sbyte)1 },
        };

        var result = OpcCpxTypeConverter.Convert(sourceValue, sourceType, requestedType);

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);
    }
}
