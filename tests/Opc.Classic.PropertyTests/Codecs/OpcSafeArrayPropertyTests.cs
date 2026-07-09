// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using CsCheck;
using Opc.Classic.Ndr;

namespace Opc.Classic.PropertyTests.Codecs;

public sealed class OpcSafeArrayPropertyTests
{
    private static readonly SafeArrayFeatures[] ScalarFeatureBits =
    [
        SafeArrayFeatures.Auto,
        SafeArrayFeatures.Static,
        SafeArrayFeatures.Embedded,
        SafeArrayFeatures.FixedSize,
        SafeArrayFeatures.HaveVartype,
    ];

    [Test]
    public Task OneDimensionalInt32_RoundTrips_WithRandomLowerBounds()
    {
        OneDimensionalInt32Gen.Sample(value => SafeArrayRoundTrips(value), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task TwoDimensionalDouble_RoundTrips_WithRandomBounds()
    {
        TwoDimensionalDoubleGen.Sample(value => SafeArrayRoundTrips(value), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task ThreeDimensionalBstr_RoundTrips_WithRandomBounds()
    {
        ThreeDimensionalBstrGen.Sample(value => SafeArrayRoundTrips(value), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task ScalarFadfFeatureCombinations_RoundTrip()
    {
        ScalarFeatureSafeArrayGen.Sample(value => SafeArrayRoundTrips(value), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task BstrFadfFeatureCombinations_RoundTrip()
    {
        BstrFeatureSafeArrayGen.Sample(value => SafeArrayRoundTrips(value), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VariantAndRecordFadfFeatureCombinations_RoundTrip()
    {
        VariantOrRecordFeatureSafeArrayGen.Sample(value => SafeArrayRoundTrips(value), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task EdgeElementCounts_ZeroOneAndLarge_RoundTrip()
    {
        Gen.Int.Array[1024].Sample(values =>
        {
            var zero = new OpcSafeArray(VarType.VT_I4, Array.Empty<int>(), [0], [0], SafeArrayFeatures.HaveVartype);
            var one = new OpcSafeArray(VarType.VT_I4, new[] { values[0] }, [1], [-1], SafeArrayFeatures.HaveVartype);
            var large = new OpcSafeArray(VarType.VT_I4, values, [values.Length], [int.MinValue], SafeArrayFeatures.HaveVartype);
            return SafeArrayRoundTrips(zero) && SafeArrayRoundTrips(one) && SafeArrayRoundTrips(large, capacity: 16 * 1024);
        }, iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    private static readonly Gen<OpcSafeArray> OneDimensionalInt32Gen =
        from length in Gen.Int[0, 128]
        from lowerBound in Gen.Int[-128, 128]
        from data in Gen.Int.Array[length]
        select new OpcSafeArray(VarType.VT_I4, data, [length], [lowerBound], SafeArrayFeatures.HaveVartype);

    private static readonly Gen<OpcSafeArray> TwoDimensionalDoubleGen =
        from dim0 in Gen.Int[0, 16]
        from dim1 in Gen.Int[0, 16]
        from lower0 in Gen.Int[-32, 32]
        from lower1 in Gen.Int[-32, 32]
        from data in Gen.Double.Array[dim0 * dim1]
        select new OpcSafeArray(VarType.VT_R8, data, [dim0, dim1], [lower0, lower1], SafeArrayFeatures.HaveVartype);

    private static readonly Gen<OpcSafeArray> ThreeDimensionalBstrGen =
        from dim0 in Gen.Int[0, 4]
        from dim1 in Gen.Int[0, 4]
        from dim2 in Gen.Int[0, 4]
        from lower0 in Gen.Int[-8, 8]
        from lower1 in Gen.Int[-8, 8]
        from lower2 in Gen.Int[-8, 8]
        from data in CodecProperty.NullableShortStringGen.Array[dim0 * dim1 * dim2]
        select new OpcSafeArray(
            VarType.VT_BSTR,
            data,
            [dim0, dim1, dim2],
            [lower0, lower1, lower2],
            SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Bstr);

    private static readonly Gen<OpcSafeArray> ScalarFeatureSafeArrayGen =
        from mask in Gen.Int[0, (1 << 5) - 1]
        from data in Gen.Int.Array[0, 32]
        select new OpcSafeArray(VarType.VT_I4, data, features: FeaturesFromMask(mask, ScalarFeatureBits));

    private static readonly Gen<OpcSafeArray> BstrFeatureSafeArrayGen =
        from mask in Gen.Int[0, (1 << 6) - 1]
        from data in CodecProperty.NullableShortStringGen.Array[0, 16]
        select new OpcSafeArray(
            VarType.VT_BSTR,
            data,
            features: FeaturesFromMask(mask, [.. ScalarFeatureBits, SafeArrayFeatures.Bstr]));

    private static readonly Gen<OpcSafeArray> VariantOrRecordFeatureSafeArrayGen =
        from choice in Gen.Int[0, 1]
        from mask in Gen.Int[0, (1 << 6) - 1]
        from variants in CodecProperty.RecursiveVariantGen.Array[0, 8]
        from records in CodecProperty.RecordValueGen.Array[0, 8]
        select choice == 0
            ? new OpcSafeArray(
                VarType.VT_VARIANT,
                variants,
                features: FeaturesFromMask(mask, [.. ScalarFeatureBits, SafeArrayFeatures.Variant]))
            : new OpcSafeArray(
                VarType.VT_RECORD,
                records,
                features: FeaturesFromMask(mask, [.. ScalarFeatureBits, SafeArrayFeatures.Record]));

    private static bool SafeArrayRoundTrips(OpcSafeArray value, int capacity = 4096) => CodecProperty.RoundTrips(
        value,
        static (ref NdrWriter writer, OpcSafeArray array) => writer.WriteSafeArray(array),
        static (ref NdrReader reader) => reader.ReadSafeArray(),
        CodecProperty.SafeArrayEquals,
        capacity);

    private static SafeArrayFeatures FeaturesFromMask(int mask, SafeArrayFeatures[] bits)
    {
        SafeArrayFeatures features = SafeArrayFeatures.None;
        for (int i = 0; i < bits.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                features |= bits[i];
            }
        }
        return features;
    }
}
