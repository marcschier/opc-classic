// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Ndr;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Fuzz;

public sealed class DecoderBoundsFuzzTests
{
    private static readonly byte[] NtlmSignature = "NTLMSSP\0"u8.ToArray();

    [Test]
    [Arguments("unicode-offset-nonzero")]
    [Arguments("unicode-actual-exceeds-max")]
    [Arguments("unicode-actual-negative")]
    [Arguments("unicode-oversized-default")]
    [Arguments("unicode-truncated-header")]
    [Arguments("unicode-truncated-body")]
    [Arguments("bstr-flags")]
    [Arguments("bstr-oversized")]
    [Arguments("bstr-negative")]
    [Arguments("bstr-truncated-body")]
    [Arguments("raw-over-quota")]
    [Arguments("byte-array-over-quota")]
    [Arguments("int16-array-over-quota")]
    [Arguments("uint16-array-over-quota")]
    [Arguments("int32-array-over-quota")]
    [Arguments("uint32-array-over-quota")]
    [Arguments("int64-array-over-quota")]
    [Arguments("single-array-over-quota")]
    [Arguments("double-array-over-quota")]
    [Arguments("guid-array-over-quota")]
    public async Task NdrReader_RejectsMalformedBoundedInputs(string caseName)
    {
        await Assert.That(() => ExecuteNdrCase(caseName)).Throws<Exception>();
    }

    [Test]
    [Arguments("type1-truncated")]
    [Arguments("type1-bad-signature")]
    [Arguments("type1-wrong-type")]
    [Arguments("type1-oversized")]
    [Arguments("type1-domain-out-of-range")]
    [Arguments("type1-domain-overlaps-header")]
    [Arguments("type1-fields-overlap")]
    [Arguments("type1-version-truncated")]
    [Arguments("type2-truncated")]
    [Arguments("type2-wrong-type")]
    [Arguments("type2-oversized")]
    [Arguments("type2-target-out-of-range")]
    [Arguments("type2-target-info-out-of-range")]
    [Arguments("type2-fields-overlap")]
    [Arguments("type3-truncated")]
    [Arguments("type3-wrong-type")]
    [Arguments("type3-oversized")]
    [Arguments("type3-lm-out-of-range")]
    [Arguments("type3-user-overlaps-header")]
    [Arguments("type3-fields-overlap")]
    [Arguments("type3-negative-offset")]
    public async Task NtlmMessages_RejectMalformedBoundedInputs(string caseName)
    {
        await Assert.That(() => ExecuteNtlmCase(caseName)).Throws<Exception>();
    }

    private static void ExecuteNdrCase(string caseName)
    {
        switch (caseName)
        {
            case "unicode-offset-nonzero":
                _ = new NdrReader(UInt32s(1, 1, 1), 64).ReadUnicodeString();
                break;
            case "unicode-actual-exceeds-max":
                _ = new NdrReader(UInt32s(1, 0, 2), 64).ReadUnicodeString();
                break;
            case "unicode-actual-negative":
                _ = new NdrReader(UInt32s(uint.MaxValue, 0, uint.MaxValue), 64).ReadUnicodeString();
                break;
            case "unicode-oversized-default":
                _ = new NdrReader(UInt32s(8_388_609, 0, 8_388_609)).ReadUnicodeString();
                break;
            case "unicode-truncated-header":
                _ = new NdrReader(new byte[8], 64).ReadUnicodeString();
                break;
            case "unicode-truncated-body":
                _ = new NdrReader(UInt32s(2, 0, 2), 64).ReadUnicodeString();
                break;
            case "bstr-flags":
                _ = new NdrReader(UInt32s(1, 1, 0), 64).ReadBstr();
                break;
            case "bstr-oversized":
                _ = new NdrReader(UInt32s(1, 0, 33), 64).ReadBstr();
                break;
            case "bstr-negative":
                _ = new NdrReader(UInt32s(1, 0, uint.MaxValue), 64).ReadBstr();
                break;
            case "bstr-truncated-body":
                _ = new NdrReader(UInt32s(1, 0, 1), 64).ReadBstr();
                break;
            case "raw-over-quota":
                _ = new NdrReader(new byte[64], 64).ReadRawBytes(65);
                break;
            case "byte-array-over-quota":
                _ = new NdrReader(UInt32s(65), 64).ReadConformantByteArray();
                break;
            case "int16-array-over-quota":
                _ = new NdrReader(UInt32s(33), 64).ReadConformantInt16Array();
                break;
            case "uint16-array-over-quota":
                _ = new NdrReader(UInt32s(33), 64).ReadConformantUInt16Array();
                break;
            case "int32-array-over-quota":
                _ = new NdrReader(UInt32s(17), 64).ReadConformantInt32Array();
                break;
            case "uint32-array-over-quota":
                _ = new NdrReader(UInt32s(17), 64).ReadConformantUInt32Array();
                break;
            case "int64-array-over-quota":
                _ = new NdrReader(UInt32s(9), 64).ReadConformantInt64Array();
                break;
            case "single-array-over-quota":
                _ = new NdrReader(UInt32s(17), 64).ReadConformantSingleArray();
                break;
            case "double-array-over-quota":
                _ = new NdrReader(UInt32s(9), 64).ReadConformantDoubleArray();
                break;
            case "guid-array-over-quota":
                _ = new NdrReader(UInt32s(5), 64).ReadConformantGuidArray();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unknown NDR fuzz case.");
        }
    }

    private static void ExecuteNtlmCase(string caseName)
    {
        switch (caseName)
        {
            case "type1-truncated":
                _ = new Type1Message(new byte[8]);
                break;
            case "type1-bad-signature":
                _ = new Type1Message(new byte[32]);
                break;
            case "type1-wrong-type":
                _ = new Type1Message(NtlmMessage(2, 32));
                break;
            case "type1-oversized":
                _ = new Type1Message(NtlmMessage(1, 64 * 1024));
                break;
            case "type1-domain-out-of-range":
                _ = new Type1Message(WithSecurityBuffer(NtlmMessage(1, 32), 16, 1, 32));
                break;
            case "type1-domain-overlaps-header":
                _ = new Type1Message(WithSecurityBuffer(NtlmMessage(1, 32), 16, 1, 12));
                break;
            case "type1-fields-overlap":
                _ = new Type1Message(WithSecurityBuffers(NtlmMessage(1, 40), (16, 4, 32u), (24, 4, 34u)));
                break;
            case "type1-version-truncated":
                _ = new Type1Message(WithUInt32(NtlmMessage(1, 32), 12, (uint)NtlmFlags.NtlmsspNegotiateVersion));
                break;
            case "type2-truncated":
                _ = new Type2Message(new byte[12]);
                break;
            case "type2-wrong-type":
                _ = new Type2Message(NtlmMessage(1, 48));
                break;
            case "type2-oversized":
                _ = new Type2Message(NtlmMessage(2, 64 * 1024));
                break;
            case "type2-target-out-of-range":
                _ = new Type2Message(WithSecurityBuffer(NtlmMessage(2, 48), 12, 1, 48));
                break;
            case "type2-target-info-out-of-range":
                _ = new Type2Message(WithSecurityBuffer(NtlmMessage(2, 48), 40, 1, 48));
                break;
            case "type2-fields-overlap":
                _ = new Type2Message(WithSecurityBuffers(NtlmMessage(2, 64), (12, 8, 48u), (40, 8, 52u)));
                break;
            case "type3-truncated":
                _ = new Type3Message(new byte[32]);
                break;
            case "type3-wrong-type":
                _ = new Type3Message(NtlmMessage(2, 64));
                break;
            case "type3-oversized":
                _ = new Type3Message(NtlmMessage(3, 64 * 1024));
                break;
            case "type3-lm-out-of-range":
                _ = new Type3Message(WithSecurityBuffer(NtlmMessage(3, 64), 12, 1, 64));
                break;
            case "type3-user-overlaps-header":
                _ = new Type3Message(WithSecurityBuffer(NtlmMessage(3, 64), 36, 1, 60));
                break;
            case "type3-fields-overlap":
                _ = new Type3Message(WithSecurityBuffers(NtlmMessage(3, 80), (12, 8, 64u), (20, 8, 68u)));
                break;
            case "type3-negative-offset":
                _ = new Type3Message(WithSecurityBuffer(NtlmMessage(3, 64), 12, 1, uint.MaxValue));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unknown NTLM fuzz case.");
        }
    }

    private static byte[] UInt32s(params uint[] values)
    {
        var buffer = new byte[values.Length * sizeof(uint)];
        for (var i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(i * sizeof(uint)), values[i]);
        }
        return buffer;
    }

    private static byte[] NtlmMessage(int messageType, int length)
    {
        var raw = new byte[length];
        NtlmSignature.CopyTo(raw, 0);
        BinaryPrimitives.WriteInt32LittleEndian(raw.AsSpan(8), messageType);
        return raw;
    }

    private static byte[] WithUInt32(byte[] raw, int offset, uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(raw.AsSpan(offset), value);
        return raw;
    }

    private static byte[] WithSecurityBuffer(byte[] raw, int fieldOffset, ushort length, uint bufferOffset) =>
        WithSecurityBuffers(raw, (fieldOffset, length, bufferOffset));

    private static byte[] WithSecurityBuffers(byte[] raw, params (int FieldOffset, ushort Length, uint BufferOffset)[] fields)
    {
        foreach (var field in fields)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(field.FieldOffset), field.Length);
            BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(field.FieldOffset + sizeof(ushort)), field.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(raw.AsSpan(field.FieldOffset + 4), field.BufferOffset);
        }
        return raw;
    }
}
