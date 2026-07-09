// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using CsCheck;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.PropertyTests.Fuzz.Network;

public sealed class NtlmFuzzTests
{
    private static readonly Type[] AllowedNtlmExceptions =
    [
        typeof(InvalidDataException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(FormatException),
        typeof(EndOfStreamException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task Type1_Parse_RandomBytes_DoesNotCrash()
    {
        SampleRandom(ParseType1);
        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task Type2_Parse_RandomBytes_DoesNotCrash()
    {
        SampleRandom(ParseType2);
        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task Type3_Parse_RandomBytes_DoesNotCrash()
    {
        SampleRandom(ParseType3);
        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task Type1_Parse_MutatedValidMessage_DoesNotCrash()
    {
        SampleMutated(new Type1Message().ToByteArray(), ParseType1);
        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task Type2_Parse_MutatedValidMessage_DoesNotCrash()
    {
        Type1Message type1 = new();
        SampleMutated(new Type2Message(type1).ToByteArray(), ParseType2);
        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task Type3_Parse_MutatedValidMessage_DoesNotCrash()
    {
        Type1Message type1 = new();
        Type2Message type2 = new(type1);
        SampleMutated(new Type3Message(type2).ToByteArray(), ParseType3);
        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NtlmAvPairs_Scan_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static bool (ReadOnlyMemory<byte> bytes) =>
                    NtlmAvPairs.HasMicFlag(bytes.Span) || NtlmAvPairs.TryGet(bytes.Span, NtlmAvPairs.MsvAvChannelBindings, out _),
                AllowedNtlmExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NtlmMic_Verify_RandomBytes_DoesNotCrash()
    {
        byte[] sessionKey = new byte[16];
        byte[] negotiate = new Type1Message().ToByteArray();
        byte[] challenge = new Type2Message(new Type1Message()).ToByteArray();

        FuzzHarness.BytesEdgeWeighted.Sample(
            input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                bytes => NtlmMic.Verify(sessionKey, negotiate, challenge, bytes.ToArray(), Type3Message.MicOffset),
                AllowedNtlmExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    [Arguments("NTLM-Type1")]
    [Arguments("NTLM-Type2")]
    [Arguments("NTLM-Type3")]
    [Arguments("NTLM-AvPairs")]
    [Arguments("NTLM-MIC")]
    public async Task Ntlm_CorpusReplay_DoesNotCrash(string surface)
    {
        foreach (object[] row in FuzzHarness.LoadCorpus(surface))
        {
            byte[] input = (byte[])row[0];
            switch (surface)
            {
                case "NTLM-Type1":
                    AssertNtlmParseDoesNotCrash(input, ParseType1);
                    break;
                case "NTLM-Type2":
                    AssertNtlmParseDoesNotCrash(input, ParseType2);
                    break;
                case "NTLM-Type3":
                    AssertNtlmParseDoesNotCrash(input, ParseType3);
                    break;
                case "NTLM-AvPairs":
                    FuzzHarness.AssertParseDoesNotCrash(
                        input,
                        static bool (ReadOnlyMemory<byte> bytes) => NtlmAvPairs.HasMicFlag(bytes.Span),
                        AllowedNtlmExceptions);
                    break;
                case "NTLM-MIC":
                    FuzzHarness.AssertParseDoesNotCrash(
                        input,
                        static bool (ReadOnlyMemory<byte> bytes) => NtlmMic.Verify(new byte[16], [], [], bytes.ToArray(), 0),
                        AllowedNtlmExceptions);
                    break;
            }
        }

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    private static void SampleRandom(Func<ReadOnlyMemory<byte>, NtlmMessage> parse) =>
        FuzzHarness.BytesEdgeWeighted.Sample(
            input => AssertNtlmParseDoesNotCrash(input, parse),
            iter: FuzzHarness.Iterations,
            threads: 1);

    private static void SampleMutated(byte[] valid, Func<ReadOnlyMemory<byte>, NtlmMessage> parse) =>
        FuzzHarness.MutateValid(valid).Sample(
            input => AssertNtlmParseDoesNotCrash(input, parse),
            iter: FuzzHarness.Iterations,
            threads: 1);

    private static void AssertNtlmParseDoesNotCrash(byte[] input, Func<ReadOnlyMemory<byte>, NtlmMessage> parse) =>
        FuzzHarness.AssertParseDoesNotCrash(input, parse, AllowedNtlmExceptions);

    private static NtlmMessage ParseType1(ReadOnlyMemory<byte> bytes) => new Type1Message(bytes.ToArray());
    private static NtlmMessage ParseType2(ReadOnlyMemory<byte> bytes) => new Type2Message(bytes.ToArray());
    private static NtlmMessage ParseType3(ReadOnlyMemory<byte> bytes) => new Type3Message(bytes.ToArray());
}
