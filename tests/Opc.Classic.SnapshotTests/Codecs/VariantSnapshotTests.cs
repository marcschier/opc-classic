//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading.Tasks;
using Opc.Classic.Ndr;
using Opc.Classic.SnapshotTests.Support;
using TUnit.Core;

namespace Opc.Classic.SnapshotTests.Codecs;

public sealed class VariantSnapshotTests {
    [Test]
    public async Task VtI4_42_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_I4(42)", OpcVariant.FromInt32(42));

    [Test]
    public async Task VtR8_3_14_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_R8(3.14)", OpcVariant.FromDouble(3.14d));

    [Test]
    public async Task VtBool_true_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_BOOL(true)", OpcVariant.FromBoolean(true));

    [Test]
    public async Task VtBstr_hello_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_BSTR(\"hello\")", OpcVariant.FromString("hello"));

    [Test]
    public async Task VtDate_2024_01_01_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_DATE(2024-01-01)", OpcVariant.FromDate(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

    [Test]
    public async Task VtArrayI4_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_ARRAY|VT_I4([1,2,3,4,5])", OpcVariant.FromSafeArray(OpcSafeArray.OfInt32([1, 2, 3, 4, 5])));

    [Test]
    public async Task VtArrayBstr_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_ARRAY|VT_BSTR([\"a\",\"b\",\"c\"])", OpcVariant.FromSafeArray(OpcSafeArray.OfString(["a", "b", "c"])));

    [Test]
    public async Task VtArrayVariant_encodes_to_stable_bytes() =>
        await VerifyVariant(
            "OpcVariant",
            "VT_ARRAY|VT_VARIANT([VT_I4(1),VT_R8(2.0),VT_BSTR(\"3\")])",
            OpcVariant.FromSafeArray(OpcSafeArray.OfVariant(
            [
                OpcVariant.FromInt32(1),
                OpcVariant.FromDouble(2.0d),
                OpcVariant.FromString("3"),
            ])));

    [Test]
    public async Task VtByrefI4_encodes_to_stable_bytes() =>
        await VerifyVariant("OpcVariant", "VT_BYREF|VT_I4(42)", OpcVariant.FromByRef(VarType.VT_I4, 42));

    [Test]
    public async Task VtRecord_encodes_to_stable_bytes() {
        RecordInfoRegistry.Register(CodecFixtures.SampleRecordInfo);
        try {
            await VerifyVariant("OpcVariant", "VT_RECORD(SampleRecord)", OpcVariant.FromRecord(CodecFixtures.SampleRecordValue()));
        }
        finally {
            _ = RecordInfoRegistry.Unregister(CodecFixtures.SampleRecordInfo.Id);
        }
    }

    private static Task VerifyVariant(string codecName, string sampleDescription, OpcVariant value) =>
        SnapshotVerifier.VerifyBytes(codecName, sampleDescription, NdrSnapshotWriter.Write((ref NdrWriter writer) => writer.WriteVariant(value), capacity: 2048));
}
