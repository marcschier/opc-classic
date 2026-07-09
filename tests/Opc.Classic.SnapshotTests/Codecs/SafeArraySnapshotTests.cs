// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ndr;
using Opc.Classic.SnapshotTests.Support;

namespace Opc.Classic.SnapshotTests.Codecs;

public sealed class SafeArraySnapshotTests
{
    [Test]
    public async Task One_dimensional_i4_array_encodes_to_stable_bytes() =>
        await VerifySafeArray("OpcSafeArray", "1-D VT_I4[5]", OpcSafeArray.OfInt32([1, 2, 3, 4, 5]));

    [Test]
    public async Task Two_dimensional_r8_array_encodes_to_stable_bytes() =>
        await VerifySafeArray(
            "OpcSafeArray",
            "2-D VT_R8[3,3]",
            new OpcSafeArray(
                VarType.VT_R8,
                new[] { 1.0d, 2.0d, 3.0d, 4.0d, 5.0d, 6.0d, 7.0d, 8.0d, 9.0d },
                lengths: [3, 3],
                lowerBounds: [0, 0]));

    [Test]
    public async Task Three_dimensional_i4_array_encodes_to_stable_bytes() =>
        await VerifySafeArray(
            "OpcSafeArray",
            "3-D VT_I4[2,2,2]",
            new OpcSafeArray(
                VarType.VT_I4,
                new[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                lengths: [2, 2, 2],
                lowerBounds: [0, 0, 0]));

    [Test]
    public async Task Fadf_bstr_flagged_array_encodes_to_stable_bytes() =>
        await VerifySafeArray(
            "OpcSafeArray",
            "FADF_BSTR VT_BSTR[3]",
            new OpcSafeArray(
                VarType.VT_BSTR,
                new string?[] { "a", "b", "c" },
                features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Bstr));

    [Test]
    public async Task Fadf_fixedsize_i4_array_encodes_to_stable_bytes() =>
        await VerifySafeArray(
            "OpcSafeArray",
            "FADF_FIXEDSIZE VT_I4[5]",
            new OpcSafeArray(
                VarType.VT_I4,
                new[] { 10, 20, 30, 40, 50 },
                features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.FixedSize));

    private static Task VerifySafeArray(string codecName, string sampleDescription, OpcSafeArray value) =>
        SnapshotVerifier.VerifyBytes(codecName, sampleDescription, NdrSnapshotWriter.Write((ref NdrWriter writer) => writer.WriteSafeArray(value), capacity: 2048));
}
