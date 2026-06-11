//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.IO;
using CsCheck;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Ndr;
using Opc.Classic.Tests.Fuzz;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.PropertyTests.Fuzz.Ndr;

public sealed class OrpcExtentFuzzTests
{
    private static readonly Type[] AllowedOrpcExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(InvalidOperationException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task OrpcExtentArrayCodec_Read_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static OrpcThat (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return OrpcThat.Read(ref reader);
                },
                AllowedOrpcExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task OrpcExtentArrayCodec_Read_MutatedValid_DoesNotCrash()
    {
        byte[] valid = WriteOrpcThat(new OrpcThat
        {
            Extensions =
            [
                new OrpcExtent(new Guid("00112233-4455-6677-8899-aabbccddeeff"), new byte[] { 1, 2, 3, 4, 5 }),
            ],
        });

        FuzzHarness.MutateValid(valid).Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static OrpcThat (ReadOnlyMemory<byte> bytes) =>
                {
                    var reader = new NdrReader(bytes.Span);
                    return OrpcThat.Read(ref reader);
                },
                AllowedOrpcExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task OrpcExtentArrayCodec_Read_ExtentCountVsBodyLength_Bounded()
    {
        byte[] input = OrpcThatWithExtentCount(0x10000000u);

        await Assert.That((Action)(() => _ = ReadOrpcThat(input))).Throws<Exception>();
        FuzzHarness.AssertParseDoesNotCrash(
            input,
            static OrpcThat (ReadOnlyMemory<byte> bytes) =>
            {
                var reader = new NdrReader(bytes.Span);
                return OrpcThat.Read(ref reader);
            },
            AllowedOrpcExceptions);
    }

    private static OrpcThat ReadOrpcThat(ReadOnlyMemory<byte> input)
    {
        var reader = new NdrReader(input.Span);
        return OrpcThat.Read(ref reader);
    }

    private static byte[] WriteOrpcThat(OrpcThat value)
    {
        byte[] buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        value.Write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] OrpcThatWithExtentCount(uint extentCount)
    {
        byte[] buffer = new byte[20];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0), 0u);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(4), 0x00020000u);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(8), extentCount);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(12), 0u);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(16), 0x00020004u);
        return buffer;
    }
}
