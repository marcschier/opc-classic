// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class OpcQualityTests
{
    [Test]
    public async Task Bad_HasKindBad()
    {
        await Assert.That(OpcQuality.Bad.Quality).IsEqualTo(OpcQualityKind.Bad);
    }

    [Test]
    public async Task Good_HasKindGood()
    {
        await Assert.That(OpcQuality.Good.Quality).IsEqualTo(OpcQualityKind.Good);
    }

    [Test]
    public async Task Uncertain_HasKindUncertain()
    {
        await Assert.That(OpcQuality.Uncertain.Quality).IsEqualTo(OpcQualityKind.Uncertain);
    }

    [Test]
    [Arguments(OpcQualityKind.Bad, 0, OpcQualityLimit.NotLimited)]
    [Arguments(OpcQualityKind.Bad, 4, OpcQualityLimit.NotLimited)]      // OPC_QUALITY_CONFIG_ERROR
    [Arguments(OpcQualityKind.Uncertain, 4, OpcQualityLimit.High)]      // OPC_QUALITY_SUB_NORMAL + high-limited
    [Arguments(OpcQualityKind.Good, 6, OpcQualityLimit.Constant)]       // OPC_QUALITY_LOCAL_OVERRIDE + constant
    [Arguments(OpcQualityKind.Good, 15, OpcQualityLimit.Constant)]
    public async Task Compose_RoundTripsAllSubFields(
        OpcQualityKind kind, int substatus, OpcQualityLimit limit)
    {
        var q = OpcQuality.Compose(kind, substatus, limit, vendorExtension: 0xAB);

        await Assert.That(q.Quality).IsEqualTo(kind);
        await Assert.That(q.Substatus).IsEqualTo(substatus);
        await Assert.That(q.Limit).IsEqualTo(limit);
        await Assert.That(q.VendorExtension).IsEqualTo((byte)0xAB);
    }

    [Test]
    public async Task RawValue_PackedAccordingToOpcDaSpec()
    {
        // Quality=Good (3), substatus=4 (Local Override), limit=Constant (3), vendor=0xAB
        // Expected raw layout:
        //   bits 0-1:  quality   = 11 (Good)
        //   bits 2-5:  substatus = 0100
        //   bits 6-7:  limit     = 11 (Constant)
        //   bits 8-15: vendor    = 10101011
        // = 0xAB << 8 | (0b11 << 6) | (0b0100 << 2) | 0b11
        // = 0xAB00     | 0xC0       | 0x10          | 0x03
        // = 0xABD3
        var q = OpcQuality.Compose(OpcQualityKind.Good, substatus: 4, limit: OpcQualityLimit.Constant, vendorExtension: 0xAB);
        await Assert.That(q.RawValue).IsEqualTo((ushort)0xABD3);
    }

    [Test]
    public async Task WithSubstatus_PreservesOtherFields()
    {
        var original = OpcQuality.Compose(OpcQualityKind.Good, substatus: 0, limit: OpcQualityLimit.High, vendorExtension: 0x55);
        var modified = original.WithSubstatus(7);

        await Assert.That(modified.Quality).IsEqualTo(OpcQualityKind.Good);
        await Assert.That(modified.Substatus).IsEqualTo(7);
        await Assert.That(modified.Limit).IsEqualTo(OpcQualityLimit.High);
        await Assert.That(modified.VendorExtension).IsEqualTo((byte)0x55);
    }

    [Test]
    public async Task Compose_NegativeSubstatus_Throws()
    {
        await Assert.That(() => { OpcQuality.Compose(OpcQualityKind.Good, substatus: -1); })
            .Throws<System.ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Compose_OverflowingSubstatus_Throws()
    {
        await Assert.That(() => { OpcQuality.Compose(OpcQualityKind.Good, substatus: 16); })
            .Throws<System.ArgumentOutOfRangeException>();
    }
}
