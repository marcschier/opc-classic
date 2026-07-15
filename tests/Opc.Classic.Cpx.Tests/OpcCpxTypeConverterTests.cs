// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Numerics;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcCpxTypeConverterTests
{
    private static readonly TypeKind[] s_integralKinds =
    [
        TypeKind.Int8,
        TypeKind.UInt8,
        TypeKind.Int16,
        TypeKind.UInt16,
        TypeKind.Int32,
        TypeKind.UInt32,
        TypeKind.Int64,
        TypeKind.UInt64,
    ];

    [Test]
    public async Task ReferenceConverter_ImplementsPublicContractAndExposesBounds()
    {
        IOpcCpxTypeConverter converter = new OpcCpxReferenceTypeConverter();

        var result = converter.Convert((byte)7, TypeKind.UInt8, TypeKind.Int16);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo((short)7);
        await Assert.That(ReadMaxNestingDepth()).IsEqualTo(32);
        await Assert.That(ReadMaxArrayElements()).IsEqualTo(65_536);
    }

    [Test]
    public async Task Convert_IdentityForEveryKind_ValidatesRuntimeType()
    {
        (TypeKind Kind, object Value)[] cases =
        [
            (TypeKind.Boolean, true),
            (TypeKind.Int8, (sbyte)-1),
            (TypeKind.UInt8, (byte)1),
            (TypeKind.Int16, (short)-2),
            (TypeKind.UInt16, (ushort)2),
            (TypeKind.Int32, -3),
            (TypeKind.UInt32, 3U),
            (TypeKind.Int64, -4L),
            (TypeKind.UInt64, 4UL),
            (TypeKind.Single, 1.5F),
            (TypeKind.Double, 2.5D),
            (TypeKind.String, "value"),
            (TypeKind.FileTime, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            (TypeKind.Guid, Guid.Empty),
            (TypeKind.Blob, new byte[] { 1 }),
            (TypeKind.BitString, new byte[] { 0x80 }),
            (TypeKind.StructReference, new ComplexValue { Type = new StructType { Name = "Value" } }),
        ];

        foreach (var (kind, value) in cases)
        {
            var result = OpcCpxTypeConverter.Convert(value, kind, kind);

            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(ReferenceEquals(result.Value, value)).IsTrue();
        }

        await AssertBadType(OpcCpxTypeConverter.Convert(null, TypeKind.Int32, TypeKind.Int32));
        await AssertBadType(OpcCpxTypeConverter.Convert((short)1, TypeKind.Int32, TypeKind.Int32));
        await AssertBadType(OpcCpxTypeConverter.Convert(1, TypeKind.Unknown, TypeKind.Unknown));
    }

    [Test]
    [Arguments(false, 0)]
    [Arguments(true, 1)]
    public async Task Convert_BooleanToInt32_ReturnsOneOrZero(bool value, int expected)
    {
        var result = OpcCpxTypeConverter.Convert(value, TypeKind.Boolean, TypeKind.Int32);

        await AssertSuccess(result, expected, typeof(int));
    }

    [Test]
    [Arguments("0", 0)]
    [Arguments("-2147483648", int.MinValue)]
    [Arguments("2147483647", int.MaxValue)]
    [Arguments("  +42  ", 42)]
    public async Task Convert_StringToInt32_UsesInvariantIntegerSyntax(string value, int expected)
    {
        var result = OpcCpxTypeConverter.Convert(value, TypeKind.String, TypeKind.Int32);

        await AssertSuccess(result, expected, typeof(int));
    }

    [Test]
    [Arguments("")]
    [Arguments("1.0")]
    [Arguments("1,000")]
    [Arguments("0x10")]
    [Arguments("NaN")]
    public async Task Convert_MalformedStringToInt32_ReturnsBadType(string value)
    {
        var result = OpcCpxTypeConverter.Convert(value, TypeKind.String, TypeKind.Int32);

        await AssertBadType(result);
    }

    [Test]
    [Arguments("2147483648")]
    [Arguments("-2147483649")]
    public async Task Convert_StringToInt32OutsideRange_ReturnsRange(string value)
    {
        await AssertRange(OpcCpxTypeConverter.Convert(value, TypeKind.String, TypeKind.Int32));
    }

    [Test]
    [NotInParallel]
    public async Task Convert_StringToInt32_IsUnaffectedByCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            var invariantResult = OpcCpxTypeConverter.Convert("1234", TypeKind.String, TypeKind.Int32);
            var cultureFormattedResult = OpcCpxTypeConverter.Convert(
                1234.ToString("N0", CultureInfo.CurrentCulture),
                TypeKind.String,
                TypeKind.Int32);

            await AssertSuccess(invariantResult, 1234, typeof(int));
            await AssertBadType(cultureFormattedResult);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Test]
    public async Task Convert_EveryIntegralPair_UsesExactTargetTypeAndMathematicalValue()
    {
        foreach (var sourceKind in s_integralKinds)
        {
            var (sourceMinimum, sourceMaximum) = GetRange(sourceKind);

            foreach (var requestedKind in s_integralKinds)
            {
                var (targetMinimum, targetMaximum) = GetRange(requestedKind);
                var lower = BigInteger.Max(sourceMinimum, targetMinimum);
                var upper = BigInteger.Min(sourceMaximum, targetMaximum);

                foreach (var value in GetRepresentativeValues(lower, upper))
                {
                    var result = OpcCpxTypeConverter.Convert(
                        CreateIntegral(sourceKind, value),
                        sourceKind,
                        requestedKind);

                    await AssertSuccess(result, CreateIntegral(requestedKind, value), GetClrType(requestedKind));
                }
            }
        }
    }

    [Test]
    public async Task Convert_IntegralPairs_RoundTripRepresentativeSharedValues()
    {
        foreach (var sourceKind in s_integralKinds)
        {
            var (sourceMinimum, sourceMaximum) = GetRange(sourceKind);

            foreach (var requestedKind in s_integralKinds)
            {
                var (targetMinimum, targetMaximum) = GetRange(requestedKind);
                var lower = BigInteger.Max(sourceMinimum, targetMinimum);
                var upper = BigInteger.Min(sourceMaximum, targetMaximum);

                foreach (var value in GetRepresentativeValues(lower, upper))
                {
                    var original = CreateIntegral(sourceKind, value);
                    var converted = OpcCpxTypeConverter.Convert(original, sourceKind, requestedKind);
                    var roundTripped = OpcCpxTypeConverter.Convert(converted.Value, requestedKind, sourceKind);

                    await AssertSuccess(roundTripped, original, GetClrType(sourceKind));
                }
            }
        }
    }

    [Test]
    public async Task Convert_EveryIntegralNarrowing_RejectsJustOutsideTargetRange()
    {
        foreach (var sourceKind in s_integralKinds)
        {
            var (sourceMinimum, sourceMaximum) = GetRange(sourceKind);

            foreach (var requestedKind in s_integralKinds)
            {
                var (targetMinimum, targetMaximum) = GetRange(requestedKind);

                if (sourceMinimum < targetMinimum)
                {
                    var below = targetMinimum - BigInteger.One;
                    var result = OpcCpxTypeConverter.Convert(
                        CreateIntegral(sourceKind, below),
                        sourceKind,
                        requestedKind);
                    await AssertRange(result);
                }

                if (sourceMaximum > targetMaximum)
                {
                    var above = targetMaximum + BigInteger.One;
                    var result = OpcCpxTypeConverter.Convert(
                        CreateIntegral(sourceKind, above),
                        sourceKind,
                        requestedKind);
                    await AssertRange(result);
                }
            }
        }
    }

    [Test]
    [Arguments(TypeKind.Int16, TypeKind.Int8, "-129")]
    [Arguments(TypeKind.Int16, TypeKind.Int8, "128")]
    [Arguments(TypeKind.Int32, TypeKind.UInt8, "-1")]
    [Arguments(TypeKind.UInt16, TypeKind.UInt8, "256")]
    [Arguments(TypeKind.Int64, TypeKind.UInt64, "-1")]
    [Arguments(TypeKind.UInt64, TypeKind.Int64, "18446744073709551615")]
    [Arguments(TypeKind.UInt64, TypeKind.UInt32, "4294967296")]
    public async Task Convert_IntegralExtremeOutsideRange_ReturnsRange(
        TypeKind sourceKind,
        TypeKind requestedKind,
        string value)
    {
        var result = OpcCpxTypeConverter.Convert(
            CreateIntegral(sourceKind, BigInteger.Parse(value, CultureInfo.InvariantCulture)),
            sourceKind,
            requestedKind);

        await AssertRange(result);
    }

    [Test]
    public async Task Convert_SingleToDouble_PreservesFiniteSpecialAndSignedZeroValues()
    {
        float[] values =
        [
            float.MinValue,
            -1.5f,
            BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),
            0.0f,
            1.5f,
            float.MaxValue,
            float.NegativeInfinity,
            float.PositiveInfinity,
            float.NaN,
        ];

        foreach (var value in values)
        {
            var result = OpcCpxTypeConverter.Convert(value, TypeKind.Single, TypeKind.Double);

            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(result.Value?.GetType()).IsEqualTo(typeof(double));

            var converted = (double)result.Value!;
            if (float.IsNaN(value))
            {
                await Assert.That(double.IsNaN(converted)).IsTrue();
            }
            else if (value == 0)
            {
                await Assert.That(BitConverter.DoubleToInt64Bits(converted))
                    .IsEqualTo(value < 0 || BitConverter.SingleToInt32Bits(value) < 0
                        ? long.MinValue
                        : 0L);
            }
            else
            {
                await Assert.That(converted).IsEqualTo((double)value);
            }
        }
    }

    [Test]
    public async Task Convert_DoubleToSingle_PreservesInRangeSpecialAndSignedZeroValues()
    {
        double[] values =
        [
            -float.MaxValue,
            -1.5d,
            BitConverter.Int64BitsToDouble(long.MinValue),
            0.0d,
            1.5d,
            float.MaxValue,
            double.NegativeInfinity,
            double.PositiveInfinity,
            double.NaN,
        ];

        foreach (var value in values)
        {
            var result = OpcCpxTypeConverter.Convert(value, TypeKind.Double, TypeKind.Single);

            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(result.Value?.GetType()).IsEqualTo(typeof(float));

            var converted = (float)result.Value!;
            if (double.IsNaN(value))
            {
                await Assert.That(float.IsNaN(converted)).IsTrue();
            }
            else if (value == 0)
            {
                await Assert.That(BitConverter.SingleToInt32Bits(converted))
                    .IsEqualTo(BitConverter.DoubleToInt64Bits(value) < 0 ? int.MinValue : 0);
            }
            else
            {
                await Assert.That(converted).IsEqualTo((float)value);
            }
        }
    }

    [Test]
    public async Task Convert_DoubleOutsideSingleFiniteRange_ReturnsRange()
    {
        double[] values =
        [
            Math.BitIncrement((double)float.MaxValue),
            Math.BitDecrement(-(double)float.MaxValue),
            double.MaxValue,
            double.MinValue,
        ];

        foreach (var value in values)
        {
            await AssertRange(OpcCpxTypeConverter.Convert(value, TypeKind.Double, TypeKind.Single));
        }
    }

    [Test]
    public async Task Convert_Int32ToDouble_PreservesExistingCompatibility()
    {
        foreach (var value in new[] { int.MinValue, -1, 0, 1, int.MaxValue })
        {
            var result = OpcCpxTypeConverter.Convert(value, TypeKind.Int32, TypeKind.Double);

            await AssertSuccess(result, (double)value, typeof(double));
        }
    }

    [Test]
    public async Task Convert_FloatingToIntegral_RemainsUnsupported()
    {
        foreach (var requestedKind in s_integralKinds)
        {
            await AssertBadType(OpcCpxTypeConverter.Convert(1.0f, TypeKind.Single, requestedKind));
            await AssertBadType(OpcCpxTypeConverter.Convert(1.0d, TypeKind.Double, requestedKind));
        }
    }

    [Test]
    public async Task Convert_WrongClrRuntimeTypes_ReturnBadTypeWithoutThrowing()
    {
        (object? Value, TypeKind Source, TypeKind Requested)[] cases =
        [
            ((short)1, TypeKind.Int32, TypeKind.Int64),
            (1, TypeKind.UInt32, TypeKind.UInt64),
            ("true", TypeKind.Boolean, TypeKind.Int32),
            (1.0d, TypeKind.Single, TypeKind.Double),
            (null, TypeKind.Int8, TypeKind.Int16),
        ];

        foreach (var testCase in cases)
        {
            await AssertBadType(OpcCpxTypeConverter.Convert(
                testCase.Value,
                testCase.Source,
                testCase.Requested));
        }
    }

    [Test]
    public async Task Convert_UnsupportedAndVendorStylePairs_ReturnBadType()
    {
        var complex = new ComplexValue { Type = new StructType { Name = "Vendor" } };
        (object Value, TypeKind Source, TypeKind Requested)[] cases =
        [
            (Guid.Empty, TypeKind.Guid, TypeKind.String),
            ("00000000-0000-0000-0000-000000000000", TypeKind.String, TypeKind.Guid),
            (new byte[] { 1, 2 }, TypeKind.Blob, TypeKind.Int32),
            (new byte[] { 0x80 }, TypeKind.BitString, TypeKind.UInt8),
            (complex, TypeKind.StructReference, TypeKind.Int32),
            (1, TypeKind.Unknown, TypeKind.Int32),
            (new VendorValue(7), TypeKind.Int32, TypeKind.Int64),
        ];

        foreach (var testCase in cases)
        {
            await AssertBadType(OpcCpxTypeConverter.Convert(
                testCase.Value,
                testCase.Source,
                testCase.Requested));
        }
    }

    [Test]
    public async Task Convert_ComplexValue_ConvertsMatchingFieldsAndPreservesStaticApi()
    {
        var (sourceType, requestedType, sourceValue) = CreateDirectComplexConversion();

        var result = OpcCpxTypeConverter.Convert(sourceValue, sourceType, requestedType);
        var converted = (ComplexValue)result.Value!;

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(converted.Type.Name).IsEqualTo("RequestedStatus");
        await Assert.That(converted.Fields["Running"]).IsEqualTo(1);
        await Assert.That(converted.Fields["Code"]).IsEqualTo(7);
    }

    [Test]
    public async Task Convert_DictionaryAwareOverload_ConvertsDirectFields()
    {
        var (sourceType, requestedType, sourceValue) = CreateDirectComplexConversion();
        var sourceDictionary = TypeDictionary.FromTypes(sourceType);
        var requestedDictionary = TypeDictionary.FromTypes(requestedType);

        var result = OpcCpxTypeConverter.Convert(
            sourceValue,
            sourceType,
            requestedType,
            sourceDictionary,
            requestedDictionary);
        var converted = (ComplexValue)result.Value!;

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(converted.Fields["Running"]).IsEqualTo(1);
        await Assert.That(converted.Fields["Code"]).IsEqualTo(7);
    }

    [Test]
    public async Task Convert_ComplexValue_MissingMetadataOrMalformedRuntime_ReturnsSpecificErrors()
    {
        var sourceType = new TypeDescription(
            "Source",
            "Source",
            TypeKind.StructReference,
            true,
            [new TypeField("A", TypeKind.Int8)]);
        var requestedType = new TypeDescription(
            "Requested",
            "Requested",
            TypeKind.StructReference,
            true,
            [new TypeField("B", TypeKind.Int32)]);
        var missing = new ComplexValue
        {
            Type = new StructType { Name = "Source" },
            Fields = new Dictionary<string, object?> { ["A"] = (sbyte)1 },
        };
        var malformed = new ComplexValue
        {
            Type = new StructType { Name = "Source" },
            Fields = null!,
        };

        await AssertTypeChanged(OpcCpxTypeConverter.Convert(missing, sourceType, requestedType));
        await AssertBadType(OpcCpxTypeConverter.Convert(malformed, sourceType, requestedType));
    }

    [Test]
    public async Task Convert_ComplexOverloads_ValidateRequiredArguments()
    {
        var type = new TypeDescription("Value", "Value", TypeKind.StructReference, true);
        var value = new ComplexValue { Type = new StructType { Name = "Value" } };
        var dictionary = TypeDictionary.FromTypes(type);
        var converter = new OpcCpxReferenceTypeConverter();

        await Assert.That(() => converter.Convert(null!, type, type)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(value, null!, type)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(value, type, null!)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(null!, type, type, dictionary, dictionary)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(value, null!, type, dictionary, dictionary)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(value, type, null!, dictionary, dictionary)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(value, type, type, null!, dictionary)).Throws<ArgumentNullException>();
        await Assert.That(() => converter.Convert(value, type, type, dictionary, null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Convert_RecursiveStructuresAndCountedArrays_UseSeparateDictionaries()
    {
        var sourceLeaf = CreateType("SourceLeaf", "source:leaf", new TypeField("Code", TypeKind.UInt8));
        var requestedLeaf = CreateType("RequestedLeaf", "requested:leaf", new TypeField("Code", TypeKind.Int32));
        var sourceMiddle = CreateType(
            "SourceMiddle",
            "source:middle",
            new TypeField("Count", TypeKind.UInt8),
            new TypeField("Items", TypeKind.StructReference, sourceLeaf.TypeId, ElementCountFieldName: "Count"));
        var requestedMiddle = CreateType(
            "RequestedMiddle",
            "requested:middle",
            new TypeField("Count", TypeKind.Int32),
            new TypeField("Items", TypeKind.StructReference, requestedLeaf.TypeId, ElementCountFieldName: "Count"));
        var sourceRoot = CreateType(
            "SourceRoot",
            "source:root",
            new TypeField("Child", TypeKind.StructReference, sourceMiddle.TypeId));
        var requestedRoot = new TypeDescription(
            "RequestedRoot",
            "requested:root",
            TypeKind.StructReference,
            true,
            [new TypeField("Child", TypeKind.StructReference, requestedMiddle.TypeId, ByteOrder: ByteOrder.BigEndian)],
            defaultBigEndian: true);
        var source = CreateValue(
            sourceRoot,
            ("Child", CreateValue(
                sourceMiddle,
                ("Count", (byte)2),
                ("Items", new[]
                {
                    CreateValue(sourceLeaf, ("Code", (byte)7)),
                    CreateValue(sourceLeaf, ("Code", (byte)9)),
                }))));

        var result = OpcCpxTypeConverter.Convert(
            source,
            sourceRoot,
            requestedRoot,
            TypeDictionary.FromTypes(sourceRoot, sourceMiddle, sourceLeaf),
            TypeDictionary.FromTypes(requestedRoot, requestedMiddle, requestedLeaf));

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        var root = (ComplexValue)result.Value!;
        var middle = (ComplexValue)root.Fields["Child"]!;
        var items = (ComplexValue[])middle.Fields["Items"]!;
        await Assert.That(root.Type.Name).IsEqualTo("RequestedRoot");
        await Assert.That(root.Type.DefaultByteOrder).IsEqualTo(ByteOrder.BigEndian);
        await Assert.That(root.Type.Fields[0].TypeReference).IsEqualTo(requestedMiddle.TypeId);
        await Assert.That(root.Type.Fields[0].ByteOrder).IsEqualTo(ByteOrder.BigEndian);
        await Assert.That(middle.Fields.Keys.ElementAt(0)).IsEqualTo("Count");
        await Assert.That(middle.Fields.Keys.ElementAt(1)).IsEqualTo("Items");
        await Assert.That(middle.Fields["Count"]).IsEqualTo(2);
        await Assert.That(items[0].Fields["Code"]).IsEqualTo(7);
        await Assert.That(items[1].Fields["Code"]).IsEqualTo(9);
        await AssertBadType(OpcCpxTypeConverter.Convert(source, sourceRoot, requestedRoot));
    }

    [Test]
    [Arguments(TypeKind.UInt8, TypeKind.Int32)]
    [Arguments(TypeKind.Int32, TypeKind.Int16)]
    public async Task Convert_FixedPrimitiveArray_ConvertsEachElement(TypeKind sourceKind, TypeKind requestedKind)
    {
        var sourceType = CreateType("Source", "source", new TypeField("Values", sourceKind, ElementCount: 3));
        var requestedType = CreateType("Requested", "requested", new TypeField("Values", requestedKind, ElementCount: 3));
        object?[] values = sourceKind == TypeKind.UInt8 ? [(byte)1, (byte)2, (byte)3] : [1, 2, 3];

        var result = OpcCpxTypeConverter.Convert(
            CreateValue(sourceType, ("Values", values)),
            sourceType,
            requestedType);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        var converted = (object?[])((ComplexValue)result.Value!).Fields["Values"]!;
        await Assert.That(converted.Length).IsEqualTo(3);
        await Assert.That(converted[0]?.GetType()).IsEqualTo(GetClrType(requestedKind));
        await Assert.That(Convert.ToInt32(converted[2], CultureInfo.InvariantCulture)).IsEqualTo(3);
    }

    [Test]
    public async Task Convert_MalformedNestedFieldsCountsAndShapes_ReturnSpecificErrors()
    {
        var sourceLeaf = CreateType("SourceLeaf", "source:leaf", new TypeField("Value", TypeKind.UInt16));
        var requestedLeaf = CreateType("RequestedLeaf", "requested:leaf", new TypeField("Value", TypeKind.UInt8));
        var sourceRoot = CreateType("SourceRoot", "source:root", new TypeField("Child", TypeKind.StructReference, sourceLeaf.TypeId));
        var requestedRoot = CreateType("RequestedRoot", "requested:root", new TypeField("Child", TypeKind.StructReference, requestedLeaf.TypeId));
        var dictionaries = (
            Source: TypeDictionary.FromTypes(sourceRoot, sourceLeaf),
            Requested: TypeDictionary.FromTypes(requestedRoot, requestedLeaf));
        await AssertRange(OpcCpxTypeConverter.Convert(
            CreateValue(sourceRoot, ("Child", CreateValue(sourceLeaf, ("Value", (ushort)256)))),
            sourceRoot,
            requestedRoot,
            dictionaries.Source,
            dictionaries.Requested));

        var missingField = CreateType("Missing", "missing", new TypeField("Other", TypeKind.UInt8));
        await AssertTypeChanged(OpcCpxTypeConverter.Convert(
            CreateValue(sourceRoot, ("Child", CreateValue(sourceLeaf, ("Value", (ushort)1)))),
            sourceRoot,
            missingField));

        var missingReference = CreateType("MissingRef", "missing:ref", new TypeField("Child", TypeKind.StructReference, "unknown"));
        await AssertBadType(OpcCpxTypeConverter.Convert(
            CreateValue(sourceRoot, ("Child", CreateValue(sourceLeaf, ("Value", (ushort)1)))),
            sourceRoot,
            missingReference,
            dictionaries.Source,
            TypeDictionary.FromTypes(missingReference)));

        var fixedSource = CreateType("Fixed", "fixed", new TypeField("Values", TypeKind.UInt8, ElementCount: 2));
        var fixedRequested = CreateType("FixedRequested", "fixed:requested", new TypeField("Values", TypeKind.Int32, ElementCount: 2));
        await AssertRange(OpcCpxTypeConverter.Convert(
            CreateValue(fixedSource, ("Values", new byte[] { 1 })),
            fixedSource,
            fixedRequested));

        var countedSource = CreateType(
            "Counted",
            "counted",
            new TypeField("Count", TypeKind.UInt8),
            new TypeField("Values", TypeKind.UInt8, ElementCount: 2));
        var countedRequested = CreateType(
            "CountedRequested",
            "counted:requested",
            new TypeField("Count", TypeKind.Int32),
            new TypeField("Values", TypeKind.Int32, ElementCountFieldName: "Count"));
        await AssertRange(OpcCpxTypeConverter.Convert(
            CreateValue(countedSource, ("Count", (byte)3), ("Values", new byte[] { 1, 2 })),
            countedSource,
            countedRequested));

        var scalarRequested = CreateType("Scalar", "scalar", new TypeField("Values", TypeKind.Int32));
        await AssertBadType(OpcCpxTypeConverter.Convert(
            CreateValue(fixedSource, ("Values", new byte[] { 1, 2 })),
            fixedSource,
            scalarRequested));
    }

    [Test]
    [Arguments(32, true)]
    [Arguments(33, false)]
    public async Task Convert_NestingDepthBoundary_IsEnforced(int nestedLevels, bool succeeds)
    {
        var graph = CreateNestedGraph(nestedLevels);
        var result = OpcCpxTypeConverter.Convert(
            graph.Value,
            graph.SourceRoot,
            graph.RequestedRoot,
            graph.SourceDictionary,
            graph.RequestedDictionary);

        if (succeeds)
        {
            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        }
        else
        {
            await AssertRange(result);
        }
    }

    [Test]
    [Arguments(65_536, true)]
    [Arguments(65_537, false)]
    public async Task Convert_ArrayElementBoundary_IsEnforced(int count, bool succeeds)
    {
        var sourceType = CreateType("Source", "source", new TypeField("Values", TypeKind.UInt8, ElementCount: count));
        var requestedType = CreateType("Requested", "requested", new TypeField("Values", TypeKind.Int32, ElementCount: count));
        var result = OpcCpxTypeConverter.Convert(
            CreateValue(sourceType, ("Values", new byte[count])),
            sourceType,
            requestedType);

        if (succeeds)
        {
            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(((object?[])((ComplexValue)result.Value!).Fields["Values"]!).Length).IsEqualTo(count);
        }
        else
        {
            await AssertRange(result);
        }
    }

    [Test]
    public async Task Convert_NestedBitStringIdentityPreservesBytes_AndNumericPairsRemainUnsupported()
    {
        var bits = new byte[] { 0xA5, 0x80 };
        var sourceType = CreateType("SourceBits", "source:bits", new TypeField("Bits", TypeKind.BitString, Length: 9));
        var requestedType = CreateType("RequestedBits", "requested:bits", new TypeField("Bits", TypeKind.BitString, Length: 9));
        var result = OpcCpxTypeConverter.Convert(CreateValue(sourceType, ("Bits", bits)), sourceType, requestedType);

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(ReferenceEquals(bits, ((ComplexValue)result.Value!).Fields["Bits"])).IsTrue();
        await AssertBadType(OpcCpxTypeConverter.Convert(bits, TypeKind.BitString, TypeKind.UInt16));
        await AssertBadType(OpcCpxTypeConverter.Convert((ushort)1, TypeKind.UInt16, TypeKind.BitString));
    }

    [Test]
    public async Task Convert_SameKindFields_ValidateClrTypeLengthAndBitShape()
    {
        var source = CreateType(
            "Source",
            "source",
            new TypeField("Code", TypeKind.String, Length: 4, StringEncoding: "ASCII", CharWidth: 1),
            new TypeField("Bits", TypeKind.BitString, Length: 9));
        var requested = CreateType(
            "Requested",
            "requested",
            new TypeField("Code", TypeKind.String, Length: 3, StringEncoding: "ASCII", CharWidth: 1),
            new TypeField("Bits", TypeKind.BitString, Length: 8));

        var stringRange = OpcCpxTypeConverter.Convert(
            CreateValue(source, ("Code", "ABCD"), ("Bits", new byte[] { 0x80, 0x00 })),
            source,
            requested);
        var malformedBits = OpcCpxTypeConverter.Convert(
            CreateValue(source, ("Code", "ABC"), ("Bits", new byte[] { 0x80, 0x01 })),
            source,
            source);
        var wrongClr = OpcCpxTypeConverter.Convert(
            CreateValue(source, ("Code", 7), ("Bits", new byte[] { 0x80, 0x00 })),
            source,
            source);

        await AssertRange(stringRange);
        await AssertBadType(malformedBits);
        await AssertBadType(wrongClr);
    }

    private static async Task AssertSuccess(OpcCpxConversionResult result, object expected, Type expectedType)
    {
        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value?.GetType()).IsEqualTo(expectedType);
        await Assert.That(result.Value).IsEqualTo(expected);
    }

    private static async Task AssertTypeChanged(OpcCpxConversionResult result)
    {
        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);
        await Assert.That(result.Value).IsNull();
    }

    private static async Task AssertBadType(OpcCpxConversionResult result)
    {
        await Assert.That(result.Error).IsEqualTo(OpcResultId.BadType.Code);
        await Assert.That(result.Value).IsNull();
    }

    private static async Task AssertRange(OpcCpxConversionResult result)
    {
        await Assert.That(result.Error).IsEqualTo(OpcResultId.Range.Code);
        await Assert.That(result.Value).IsNull();
    }

    // TUnitAssertions0005 workaround: Assert.That(const) is rejected by the analyzer.
    private static int ReadMaxNestingDepth() => OpcCpxReferenceTypeConverter.MaxNestingDepth;
    private static int ReadMaxArrayElements() => OpcCpxReferenceTypeConverter.MaxArrayElements;

    private static IEnumerable<BigInteger> GetRepresentativeValues(BigInteger minimum, BigInteger maximum)
    {
        var values = new HashSet<BigInteger>
        {
            minimum,
            maximum,
            BigInteger.Zero,
            BigInteger.One,
            -BigInteger.One,
        };

        if (minimum < maximum)
        {
            values.Add(minimum + BigInteger.One);
            values.Add(maximum - BigInteger.One);
        }

        foreach (var value in values.Order())
        {
            if (value >= minimum && value <= maximum)
            {
                yield return value;
            }
        }
    }

    private static (BigInteger Minimum, BigInteger Maximum) GetRange(TypeKind kind) =>
        kind switch
        {
            TypeKind.Int8 => (sbyte.MinValue, sbyte.MaxValue),
            TypeKind.UInt8 => (byte.MinValue, byte.MaxValue),
            TypeKind.Int16 => (short.MinValue, short.MaxValue),
            TypeKind.UInt16 => (ushort.MinValue, ushort.MaxValue),
            TypeKind.Int32 => (int.MinValue, int.MaxValue),
            TypeKind.UInt32 => (uint.MinValue, uint.MaxValue),
            TypeKind.Int64 => (long.MinValue, long.MaxValue),
            TypeKind.UInt64 => (ulong.MinValue, ulong.MaxValue),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Expected an integral kind."),
        };

    private static object CreateIntegral(TypeKind kind, BigInteger value) =>
        kind switch
        {
            TypeKind.Int8 => (sbyte)value,
            TypeKind.UInt8 => (byte)value,
            TypeKind.Int16 => (short)value,
            TypeKind.UInt16 => (ushort)value,
            TypeKind.Int32 => (int)value,
            TypeKind.UInt32 => (uint)value,
            TypeKind.Int64 => (long)value,
            TypeKind.UInt64 => (ulong)value,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Expected an integral kind."),
        };

    private static Type GetClrType(TypeKind kind) =>
        kind switch
        {
            TypeKind.Int8 => typeof(sbyte),
            TypeKind.UInt8 => typeof(byte),
            TypeKind.Int16 => typeof(short),
            TypeKind.UInt16 => typeof(ushort),
            TypeKind.Int32 => typeof(int),
            TypeKind.UInt32 => typeof(uint),
            TypeKind.Int64 => typeof(long),
            TypeKind.UInt64 => typeof(ulong),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Expected an integral kind."),
        };

    private static (TypeDescription SourceType, TypeDescription RequestedType, ComplexValue Value)
        CreateDirectComplexConversion()
    {
        var sourceType = new TypeDescription(
            "SourceStatus",
            "SourceStatus",
            TypeKind.StructReference,
            true,
            [
                new TypeField("Running", TypeKind.Boolean),
                new TypeField("Code", TypeKind.UInt8),
            ]);
        var requestedType = new TypeDescription(
            "RequestedStatus",
            "RequestedStatus",
            TypeKind.StructReference,
            true,
            [
                new TypeField("Running", TypeKind.Int32),
                new TypeField("Code", TypeKind.Int32),
            ]);
        var value = new ComplexValue
        {
            Type = new StructType { Name = "SourceStatus" },
            Fields = new Dictionary<string, object?>
            {
                ["Running"] = true,
                ["Code"] = (byte)7,
            },
        };

        return (sourceType, requestedType, value);
    }

    private static TypeDescription CreateType(string name, string typeId, params TypeField[] fields) =>
        new(name, typeId, TypeKind.StructReference, true, fields);

    private static ComplexValue CreateValue(
        TypeDescription type,
        params (string Name, object? Value)[] fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields.ToDictionary(static field => field.Name, static field => field.Value, StringComparer.Ordinal),
        };

    private static NestedGraph CreateNestedGraph(int nestedLevels)
    {
        var sourceTypes = new TypeDescription[nestedLevels + 1];
        var requestedTypes = new TypeDescription[nestedLevels + 1];
        sourceTypes[nestedLevels] = CreateType($"Source{nestedLevels}", $"source:{nestedLevels}", new TypeField("Value", TypeKind.UInt8));
        requestedTypes[nestedLevels] = CreateType($"Requested{nestedLevels}", $"requested:{nestedLevels}", new TypeField("Value", TypeKind.Int32));
        for (var level = nestedLevels - 1; level >= 0; level--)
        {
            sourceTypes[level] = CreateType(
                $"Source{level}",
                $"source:{level}",
                new TypeField("Next", TypeKind.StructReference, sourceTypes[level + 1].TypeId));
            requestedTypes[level] = CreateType(
                $"Requested{level}",
                $"requested:{level}",
                new TypeField("Next", TypeKind.StructReference, requestedTypes[level + 1].TypeId));
        }

        var value = CreateValue(sourceTypes[nestedLevels], ("Value", (byte)1));
        for (var level = nestedLevels - 1; level >= 0; level--)
        {
            value = CreateValue(sourceTypes[level], ("Next", value));
        }

        return new NestedGraph(
            sourceTypes[0],
            requestedTypes[0],
            new TypeDictionary(sourceTypes),
            new TypeDictionary(requestedTypes),
            value);
    }

    private sealed record VendorValue(int Value);

    private sealed record NestedGraph(
        TypeDescription SourceRoot,
        TypeDescription RequestedRoot,
        TypeDictionary SourceDictionary,
        TypeDictionary RequestedDictionary,
        ComplexValue Value);
}
