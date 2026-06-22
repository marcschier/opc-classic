// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using CsCheck;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.Cpx.Tests.Fuzz;

public sealed class CpxBinaryDecoderFuzzTests
{
    private static readonly Type[] AllowedBinaryDecodeExceptions =
    [
        typeof(InvalidDataException),
        typeof(FormatException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(EndOfStreamException),
        typeof(NotSupportedException),
        typeof(InvalidOperationException),
        typeof(KeyNotFoundException),
    ];

    private static readonly TypeDictionary Dictionary = OpcBinaryDictionaryParser.Parse("""
        <TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/"
                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        DefaultBigEndian="true">
          <TypeDescription TypeID="FunctionBlockHeader">
            <CharString Name="Block Tag" xsi:type="Ascii" Length="8" />
            <Integer Name="Execution Time" xsi:type="Int32" />
            <Integer Name="Execution Frequency" xsi:type="Int32" />
            <Integer Name="Number of Parameters" xsi:type="Int16" />
          </TypeDescription>
        </TypeDictionary>
        """);

    private static readonly TypeDescription Type = Dictionary.TryGetByTypeId("FunctionBlockHeader")!;

    private static readonly byte[] ValidPayload =
    [
        0x46, 0x42, 0x2d, 0x31, 0x30, 0x30, 0x00, 0x00,
        0x00, 0x00, 0x03, 0xe8,
        0x00, 0x00, 0x00, 0x32,
        0x00, 0x03,
    ];

    [Test]
    [Category("Fuzz")]
    public async Task OpcBinaryDecoder_Decode_RandomBytes_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static input => OpcBinaryDecoder.Decode(input.ToArray(), Type, Dictionary),
                AllowedBinaryDecodeExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task OpcBinaryDecoder_Decode_MutatedValidPayload_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.MutateValid(ValidPayload).Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static input => OpcBinaryDecoder.Decode(input.ToArray(), Type, Dictionary),
                AllowedBinaryDecodeExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task OpcBinaryDecoder_Decode_Corpus_DoesNotCrash()
    {
        int exercised = 0;
        foreach (object[] row in FuzzHarness.LoadCorpus("CpxBinary"))
        {
            exercised++;
            var bytes = (byte[])row[0];
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static input => OpcBinaryDecoder.Decode(input.ToArray(), Type, Dictionary),
                AllowedBinaryDecodeExceptions);
        }

        await Assert.That(exercised).IsGreaterThanOrEqualTo(0);
    }
}
