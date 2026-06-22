// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using CsCheck;
using Opc.Classic.Ndr;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.PropertyTests.Fuzz.Ndr;

public sealed class NdrVariantRecursionFuzzTests
{
    private static readonly Type[] AllowedVariantExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(InvalidOperationException),
    ];

    [Test]
    [Category("Fuzz")]
    [Arguments(8)]
    [Arguments(16)]
    [Arguments(32)]
    [Arguments(64)]
    [Arguments(128)]
    [Arguments(256)]
    [Arguments(1024)]
    public async Task NdrReader_ReadVariant_DeepNestedVariant_BoundedRecursionOrRejected(int depth)
    {
        byte[] input = BuildNestedVariant(depth);

        FuzzHarness.AssertParseDoesNotCrash(
            input,
            static OpcVariant (ReadOnlyMemory<byte> bytes) =>
            {
                var reader = new NdrReader(bytes.Span);
                return reader.ReadVariant();
            },
            AllowedVariantExceptions);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadVariant_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static OpcVariant (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return reader.ReadVariant();
                },
                AllowedVariantExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadVariant_RandomVtTag_DoesNotCrash()
    {
        Gen.Select(
            Gen.Int[0, 0x9000],
            FuzzHarness.BytesEdgeWeighted,
            static (vt, payload) => BuildVariantWithTag((ushort)vt, payload)).Sample(
                static input => FuzzHarness.AssertParseDoesNotCrash(
                    input,
                    static OpcVariant (ReadOnlyMemory<byte> bytes) =>
                    {
                        var reader = new NdrReader(bytes.Span);
                        return reader.ReadVariant();
                    },
                    AllowedVariantExceptions),
                iter: FuzzHarness.Iterations,
                threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    private static byte[] BuildNestedVariant(int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);

        byte[] buffer = new byte[Math.Max(32, (depth + 1) * 24)];
        int position = 0;
        for (int i = 0; i < depth; i++)
        {
            Align(ref position, 8);
            WriteVariantHeader(buffer, ref position, VarType.VT_VARIANT);
        }

        Align(ref position, 8);
        WriteVariantHeader(buffer, ref position, VarType.VT_EMPTY);
        return buffer.AsSpan(0, position).ToArray();
    }

    private static byte[] BuildVariantWithTag(ushort vt, byte[] payload)
    {
        byte[] buffer = new byte[24 + payload.Length];
        int position = 0;
        WriteVariantHeader(buffer, ref position, (VarType)vt);
        payload.CopyTo(buffer.AsSpan(position));
        return buffer;
    }

    private static void WriteVariantHeader(byte[] buffer, ref int position, VarType vt)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(position), 3u);
        position += sizeof(uint);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(position), 0u);
        position += sizeof(uint);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(position), (ushort)vt);
        position += sizeof(ushort);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(position), 0);
        position += sizeof(ushort);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(position), 0);
        position += sizeof(ushort);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(position), 0);
        position += sizeof(ushort);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(position), (uint)vt);
        position += sizeof(uint);
    }

    private static void Align(ref int position, int boundary)
    {
        int misaligned = position & (boundary - 1);
        if (misaligned != 0)
        {
            position += boundary - misaligned;
        }
    }
}
