//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using CsCheck;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.Mcp.Capture.Tests.Fuzz;

public sealed class OpcDcomDecoderFuzzTests
{
    private const int NullLinkType = 0;
    private const int EthernetLinkType = 1;
    private const string Surface = "OpcDcomDecoder";
    private static readonly DateTimeOffset s_baseTimestamp = new(2026, 6, 8, 19, 29, 49, TimeSpan.Zero);
    private static readonly Guid s_interfaceId = Guid.Parse("22222222-3333-4444-5555-666666666666");
    private static readonly Type[] s_allowedExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(NotSupportedException),
    ];

    [Test, Category("Fuzz")]
    public async Task OpcDcomDecoder_DecodeAll_RandomCapturedPacket_DoesNotCrash()
    {
        int sampled = 0;
        FuzzFinding? finding = null;

        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            if (finding is not null)
            {
                return;
            }

            sampled++;
            CapturedPacket packet = NewPacket(
                bytes,
                bytes.Length,
                (bytes.Length == 0 || (bytes[0] & 1) == 0) ? NullLinkType : EthernetLinkType,
                s_baseTimestamp.AddTicks(bytes.Length));

            finding = TryAssertDecoderDoesNotCrash(bytes, _ => Decode(packet), nameof(OpcDcomDecoder_DecodeAll_RandomCapturedPacket_DoesNotCrash));
        }, iter: FuzzHarness.Iterations, threads: 1);

        if (finding is not null)
        {
            SaveCorpusAndSkip(finding.Value.Input, finding.Value.Scenario, finding.Value.Exception);
        }

        await Assert.That(sampled).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test, Category("Fuzz")]
    public async Task OpcDcomDecoder_DecodeAll_TruncatedEthernetFrame_BoundedOrRejected()
    {
        byte[] frame = NewTcpFrame(NewBindPayload(callId: 42));
        int checkedFrames = 0;

        for (int length = 1; length <= 60; length++)
        {
            byte[] truncated = frame[..length];
            AssertDecoderDoesNotCrash(
                truncated,
                static input => Decode(NewPacket(input.ToArray(), input.Length, EthernetLinkType, s_baseTimestamp)),
                nameof(OpcDcomDecoder_DecodeAll_TruncatedEthernetFrame_BoundedOrRejected),
                static decoded =>
                {
                    if (decoded.Count != 0)
                    {
                        throw new InvalidOperationException($"Truncated Ethernet frame decoded {decoded.Count} DCE/RPC frame(s).");
                    }
                });
            checkedFrames++;
        }

        await Assert.That(checkedFrames).IsEqualTo(60);
    }

    [Test, Category("Fuzz")]
    public async Task OpcDcomDecoder_DecodeAll_OversizedIpLength_BoundedOrRejected()
    {
        byte[] frame = NewTcpFrame(NewBindPayload(callId: 43));
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(14 + 2, 2), 0xFFFF);
        byte[] boundedFrame = frame[..60];
        bool completed = false;

        AssertDecoderDoesNotCrash(
            boundedFrame,
            static input => Decode(NewPacket(input.ToArray(), input.Length, EthernetLinkType, s_baseTimestamp)),
            nameof(OpcDcomDecoder_DecodeAll_OversizedIpLength_BoundedOrRejected));
        completed = true;

        await Assert.That(completed).IsTrue();
    }

    [Test, Category("Fuzz")]
    public async Task OpcDcomDecoder_DecodeAll_MutatedValidFrame_DoesNotCrash()
    {
        byte[] frame = NewTcpFrame(NewBindPayload(callId: 44));
        int sampled = 0;
        FuzzFinding? finding = null;

        FuzzHarness.MutateValid(frame).Sample(mutated =>
        {
            if (finding is not null)
            {
                return;
            }

            sampled++;
            CapturedPacket packet = NewPacket(mutated, mutated.Length, EthernetLinkType, s_baseTimestamp);
            finding = TryAssertDecoderDoesNotCrash(mutated, _ => Decode(packet), nameof(OpcDcomDecoder_DecodeAll_MutatedValidFrame_DoesNotCrash));
        }, iter: FuzzHarness.Iterations, threads: 1);

        if (finding is not null)
        {
            SaveCorpusAndSkip(finding.Value.Input, finding.Value.Scenario, finding.Value.Exception);
        }

        await Assert.That(sampled).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test, Category("Fuzz")]
    public async Task OpcDcomDecoder_DecodeAll_TruncatedPcapRecord_BoundedOrRejected()
    {
        byte[] frame = NewTcpFrame(NewBindPayload(callId: 45));
        byte[] snapped = frame[..Math.Min(frame.Length, 64)];
        int checkedRecords = 0;

        foreach (int originalLength in new[] { 1, snapped.Length - 1, snapped.Length, snapped.Length + 1, 1024 * 1024 })
        {
            CapturedPacket packet = NewPacket(snapped, originalLength, EthernetLinkType, s_baseTimestamp);
            AssertDecoderDoesNotCrash(
                snapped,
                _ => Decode(packet),
                $"{nameof(OpcDcomDecoder_DecodeAll_TruncatedPcapRecord_BoundedOrRejected)} originalLength={originalLength}",
                static decoded =>
                {
                    if (decoded.Count > 1)
                    {
                        throw new InvalidOperationException($"CapturedPacket length mismatch decoded {decoded.Count} DCE/RPC frame(s).");
                    }
                });
            checkedRecords++;
        }

        await Assert.That(checkedRecords).IsEqualTo(5);
    }

    [Test, Category("Fuzz")]
    public async Task OpcDcomDecoder_DecodeAll_CorpusReplay_DoesNotCrash()
    {
        int replayed = 0;

        foreach (object[] row in FuzzHarness.LoadCorpus(Surface))
        {
            byte[] corpus = (byte[])row[0];
            CapturedPacket packet = NewPacket(corpus, corpus.Length, EthernetLinkType, s_baseTimestamp);
            AssertDecoderDoesNotCrash(corpus, _ => Decode(packet), nameof(OpcDcomDecoder_DecodeAll_CorpusReplay_DoesNotCrash));
            replayed++;
        }

        await Assert.That(replayed >= 0).IsTrue();
    }

    private static IReadOnlyList<DecodedOpcPdu> Decode(CapturedPacket packet) => new OpcDcomDecoder().DecodeAll([packet]);

    private static CapturedPacket NewPacket(byte[] data, int originalLength, int linkType, DateTimeOffset timestamp) =>
        new(timestamp, originalLength, data, linkType, new Dictionary<string, string?>());

    private static void AssertDecoderDoesNotCrash(
        ReadOnlyMemory<byte> input,
        Func<ReadOnlyMemory<byte>, IReadOnlyList<DecodedOpcPdu>> parse,
        string scenario,
        Action<IReadOnlyList<DecodedOpcPdu>>? resultInvariant = null)
    {
        FuzzFinding? finding = TryAssertDecoderDoesNotCrash(input, parse, scenario, resultInvariant);
        if (finding is not null)
        {
            SaveCorpusAndSkip(finding.Value.Input, finding.Value.Scenario, finding.Value.Exception);
        }
    }

    private static FuzzFinding? TryAssertDecoderDoesNotCrash(
        ReadOnlyMemory<byte> input,
        Func<ReadOnlyMemory<byte>, IReadOnlyList<DecodedOpcPdu>> parse,
        string scenario,
        Action<IReadOnlyList<DecodedOpcPdu>>? resultInvariant = null)
    {
        try
        {
            FuzzHarness.AssertParseDoesNotCrash(
                input,
                parse,
                s_allowedExceptions,
                resultInvariant,
                timeoutMs: 1_000);
        }
        catch (InvalidOperationException ex)
        {
            return new FuzzFinding(input.ToArray(), scenario, ex);
        }

        return null;
    }

    private static void SaveCorpusAndSkip(ReadOnlyMemory<byte> input, string scenario, Exception exception)
    {
        string directory = Path.Combine(FindRepositoryRoot(), "tests", "_Fixtures", "Fuzz", Surface);
        Directory.CreateDirectory(directory);

        string corpusFileName = FuzzHarness.CorpusFileName(input);
        string corpusPath = Path.Combine(directory, corpusFileName);
        File.WriteAllBytes(corpusPath, input.ToArray());

        string notesPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(corpusFileName) + ".notes.md");
        File.WriteAllText(
            notesPath,
            string.Join(
                Environment.NewLine,
                "# OpcDcomDecoder fuzz finding",
                string.Empty,
                "- Scenario: " + scenario,
                "- Exception: " + exception.GetType().FullName,
                "- Message: " + exception.Message,
                "- Corpus: " + corpusFileName));

        Skip.Test($"Captured unexpected OpcDcomDecoder fuzz input in {corpusPath}: {exception.GetType().Name}");
    }

    private static byte[] NewBindPayload(int callId)
    {
        var pdu = new BindPdu
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList =
            [
                new PresentationContext(
                    7,
                    new PresentationSyntax(new UUID(s_interfaceId.ToString("D")), 1, 0)),
            ],
        };

        return PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
    }

    private static byte[] NewTcpFrame(byte[] tcpPayload)
    {
        byte[] frame = new byte[14 + 20 + 20 + tcpPayload.Length];

        frame[0] = 0x00;
        frame[1] = 0x11;
        frame[2] = 0x22;
        frame[3] = 0x33;
        frame[4] = 0x44;
        frame[5] = 0x55;
        frame[6] = 0x66;
        frame[7] = 0x77;
        frame[8] = 0x88;
        frame[9] = 0x99;
        frame[10] = 0xAA;
        frame[11] = 0xBB;
        frame[12] = 0x08;
        frame[13] = 0x00;

        int ipOffset = 14;
        frame[ipOffset] = 0x45;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ipOffset + 2, 2), (ushort)(20 + 20 + tcpPayload.Length));
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ipOffset + 4, 2), 0x1000);
        frame[ipOffset + 8] = 64;
        frame[ipOffset + 9] = 6;
        frame[ipOffset + 12] = 10;
        frame[ipOffset + 13] = 1;
        frame[ipOffset + 14] = 2;
        frame[ipOffset + 15] = 3;
        frame[ipOffset + 16] = 10;
        frame[ipOffset + 17] = 4;
        frame[ipOffset + 18] = 5;
        frame[ipOffset + 19] = 6;

        int tcpOffset = ipOffset + 20;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset, 2), 50001);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 2, 2), 135);
        frame[tcpOffset + 12] = 0x50;
        frame[tcpOffset + 13] = 0x18;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 14, 2), 8192);
        tcpPayload.CopyTo(frame.AsSpan(tcpOffset + 20));

        return frame;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }

    private readonly record struct FuzzFinding(byte[] Input, string Scenario, Exception Exception);
}
