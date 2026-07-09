// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ndr;
using Opc.Classic.SnapshotTests.Support;

namespace Opc.Classic.SnapshotTests.Codecs;

public sealed class PrimitivesSnapshotTests
{
    [Test]
    public async Task Boolean_true_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrBoolean", "true", static (ref NdrWriter writer) => writer.WriteBoolean(true));

    [Test]
    public async Task Int8_minus_one_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrInt8", "-1", static (ref NdrWriter writer) => writer.WriteInt8(-1));

    [Test]
    public async Task UInt8_255_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrUInt8", "255", static (ref NdrWriter writer) => writer.WriteUInt8(255));

    [Test]
    public async Task Int16_minus_1234_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrInt16", "-1234", static (ref NdrWriter writer) => writer.WriteInt16(-1234));

    [Test]
    public async Task UInt16_cafe_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrUInt16", "0xCAFE", static (ref NdrWriter writer) => writer.WriteUInt16(0xCAFE));

    [Test]
    public async Task Int32_42_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrInt32", "42", static (ref NdrWriter writer) => writer.WriteInt32(42));

    [Test]
    public async Task UInt32_deadbeef_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrUInt32", "0xDEADBEEF", static (ref NdrWriter writer) => writer.WriteUInt32(0xDEADBEEF));

    [Test]
    public async Task Int64_canonical_value_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrInt64", "0x0102030405060708", static (ref NdrWriter writer) => writer.WriteInt64(0x0102030405060708L));

    [Test]
    public async Task UInt64_canonical_value_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrUInt64", "0x8877665544332211", static (ref NdrWriter writer) => writer.WriteUInt64(0x8877665544332211UL));

    [Test]
    public async Task Single_pi_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrSingle", "3.14159", static (ref NdrWriter writer) => writer.WriteSingle(3.14159f));

    [Test]
    public async Task Double_pi_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrDouble", "3.14159", static (ref NdrWriter writer) => writer.WriteDouble(3.14159d));

    [Test]
    public async Task Guid_canonical_value_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrGuid", "00000000-1111-2222-3333-444444444444", static (ref NdrWriter writer) => writer.WriteGuid(new Guid("00000000-1111-2222-3333-444444444444")));

    [Test]
    public async Task FileTime_unix_epoch_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrFileTime", "1970-01-01T00:00:00Z", static (ref NdrWriter writer) => writer.WriteFileTime(116444736000000000L));

    [Test]
    public async Task Unicode_string_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrUnicodeString", "OPC Classic", static (ref NdrWriter writer) => writer.WriteUnicodeString("OPC Classic"));

    [Test]
    public async Task Bstr_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrBstr", "OPC Classic", static (ref NdrWriter writer) => writer.WriteBstr("OPC Classic"));

    [Test]
    public async Task Conformant_byte_array_encodes_to_stable_ndr_bytes() =>
        await Verify("NdrConformantByteArray", "CA FE BA BE", static (ref NdrWriter writer) => writer.WriteConformantByteArray([0xCA, 0xFE, 0xBA, 0xBE]));

    private static Task Verify(string codecName, string sampleDescription, NdrWriteAction write) =>
        SnapshotVerifier.VerifyBytes(codecName, sampleDescription, NdrSnapshotWriter.Write(write, capacity: 256));
}
