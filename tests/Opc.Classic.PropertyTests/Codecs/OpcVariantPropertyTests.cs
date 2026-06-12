//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using CsCheck;
using Opc.Classic.Ndr;

namespace Opc.Classic.PropertyTests.Codecs;

public sealed class OpcVariantPropertyTests
{
    [Test]
    public Task VtI4_RoundTrips()
    {
        Gen.Int.Sample(value => VariantRoundTrips(OpcVariant.FromInt32(value)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtR8_RoundTrips()
    {
        Gen.Double.Sample(value => VariantRoundTrips(OpcVariant.FromDouble(value)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtBool_RoundTrips()
    {
        Gen.Bool.Sample(value => VariantRoundTrips(OpcVariant.FromBoolean(value)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtBstr_RoundTrips_NullAndUnicode()
    {
        CodecProperty.NullableShortStringGen.Sample(
            value => VariantRoundTrips(new OpcVariant(VarType.VT_BSTR, value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtDate_RoundTrips()
    {
        CodecProperty.OleDateTimeGen.Sample(value => VariantRoundTrips(OpcVariant.FromDate(value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtFileTime_RoundTrips()
    {
        Gen.Long.Sample(value => VariantRoundTrips(OpcVariant.FromFileTime(value)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtCy_IsExplicitlyUnsupportedByCurrentCoreCodec()
    {
        Gen.Long.Sample(value => CodecProperty.VariantWriteIsUnsupported(new OpcVariant(VarType.VT_CY, value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtDecimal_IsExplicitlyUnsupportedByCurrentCoreCodec()
    {
        Gen.Decimal.Sample(value => CodecProperty.VariantWriteIsUnsupported(new OpcVariant(VarType.VT_DECIMAL, value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtArrayI4_RoundTrips()
    {
        CodecProperty.Int32SafeArrayGen.Sample(array => VariantRoundTrips(OpcVariant.FromSafeArray(array)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtArrayR8_RoundTrips()
    {
        CodecProperty.DoubleSafeArrayGen.Sample(array => VariantRoundTrips(OpcVariant.FromSafeArray(array)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtArrayBstr_RoundTrips_NullAndUnicodeElements()
    {
        CodecProperty.BstrSafeArrayGen.Sample(array => VariantRoundTrips(OpcVariant.FromSafeArray(array)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtVariant_RoundTrips_WithBoundedDepth()
    {
        CodecProperty.RecursiveVariantGen.Sample(value =>
            CodecProperty.VariantDepth(value) < NdrVariantExtensions.MaxVariantRecursionDepth && VariantRoundTrips(value),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtArrayVariant_RoundTrips_WithBoundedElementDepth()
    {
        CodecProperty.VariantSafeArrayGen.Sample(array =>
        {
            var value = OpcVariant.FromSafeArray(array);
            return CodecProperty.VariantDepth(value) < NdrVariantExtensions.MaxVariantRecursionDepth && VariantRoundTrips(value);
        }, iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtByRefI4_RoundTrips()
    {
        Gen.Int.Sample(value => VariantRoundTrips(OpcVariant.FromByRef(VarType.VT_I4, value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtByRefBstr_RoundTrips_NullAndUnicode()
    {
        CodecProperty.NullableShortStringGen.Sample(
            value => VariantRoundTrips(OpcVariant.FromByRef(VarType.VT_BSTR, value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task VtRecord_RoundTrips_WithRegisteredRecordInfo()
    {
        CodecProperty.RecordValueGen.Sample(value => VariantRoundTrips(OpcVariant.FromRecord(value)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    private static bool VariantRoundTrips(OpcVariant value) => CodecProperty.RoundTrips(
        value,
        static (ref NdrWriter writer, OpcVariant variant) => writer.WriteVariant(variant),
        static (ref NdrReader reader) => reader.ReadVariant(),
        CodecProperty.VariantEquals);
}
