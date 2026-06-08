//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.IO;
using CsCheck;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Tests.Fuzz;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.PropertyTests.Fuzz.Ndr;

public sealed class NdrReaderFuzzTests
{
    private static readonly Type[] AllowedNdrExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(InvalidOperationException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadUnicodeString_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static string (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return reader.ReadUnicodeString();
                },
                AllowedNdrExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadBstr_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static string? (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return reader.ReadBstr();
                },
                AllowedNdrExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadConformanceHeader_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static int (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return reader.ReadConformanceHeader();
                },
                AllowedNdrExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadInterfacePointer_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static IOpcInterfaceRef? (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return OpcMInterfacePointerCodec.Read(ref reader);
                },
                AllowedNdrExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadGuid_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static Guid (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return reader.ReadGuid();
                },
                AllowedNdrExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_ReadUnicodeString_MutatedValid_DoesNotCrash()
    {
        byte[] valid = WriteUnicodeString("FZ-2 \u2713");

        FuzzHarness.MutateValid(valid).Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static string (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return reader.ReadUnicodeString();
                },
                AllowedNdrExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_LengthHeader_OverlargeConformance_RejectedOrBounded()
    {
        byte[] input = UInt32s(uint.MaxValue / 2, 0, uint.MaxValue / 2);
        await Assert.That((Action)(() => _ = ReadUnicodeString(input))).Throws<Exception>();
        AssertDocumentedRejection(input, ReadUnicodeString);
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_LengthHeader_NegativeOffset_Rejected()
    {
        byte[] intMaxOffset = UInt32s(1, int.MaxValue, 1);
        byte[] overflowingOffset = UInt32s(uint.MaxValue, uint.MaxValue - 1, 4);

        await Assert.That((Action)(() => _ = ReadUnicodeString(intMaxOffset))).Throws<Exception>();
        await Assert.That((Action)(() => _ = ReadUnicodeString(overflowingOffset))).Throws<Exception>();
        AssertDocumentedRejection(intMaxOffset, ReadUnicodeString);
        AssertDocumentedRejection(overflowingOffset, ReadUnicodeString);
    }

    [Test]
    [Category("Fuzz")]
    public async Task NdrReader_LengthHeader_ActualExceedsMax_Rejected()
    {
        byte[] input = UInt32s(1, 0, 2);

        await Assert.That((Action)(() => _ = ReadUnicodeString(input))).Throws<Exception>();
        AssertDocumentedRejection(input, ReadUnicodeString);
    }

    private static string ReadUnicodeString(ReadOnlyMemory<byte> input)
    {
        var reader = new NdrReader(input.Span);
        return reader.ReadUnicodeString();
    }

    private static void AssertDocumentedRejection<T>(ReadOnlyMemory<byte> input, Func<ReadOnlyMemory<byte>, T> parse) =>
        FuzzHarness.AssertParseDoesNotCrash(input, parse, AllowedNdrExceptions);

    private static byte[] WriteUnicodeString(string value)
    {
        byte[] buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        writer.WriteUnicodeString(value);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] UInt32s(params uint[] values)
    {
        byte[] buffer = new byte[values.Length * sizeof(uint)];
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(i * sizeof(uint)), values[i]);
        }

        return buffer;
    }

}
