// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using CsCheck;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Tests.Fuzz;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests;

public sealed class DualStringArrayCodecTests
{
    private const string Iid = "11111111-2222-3333-4455-66778899aabb";
    private const string Ipid = "aaaaaaaa-bbbb-cccc-ddee-ff0011223344";
    private const int ObjRefStandard = 0x1;
    private const int SorfNoping = 0x1000;
    private static readonly byte[] ObjRefSignature = [0x4D, 0x45, 0x4F, 0x57];
    private static readonly Type[] AllowedFuzzExceptions =
        [typeof(InvalidDataException), typeof(EndOfStreamException)];

    [Test]
    public async Task Decode_KnownWindowsTcpEncoding_RoundTrips()
    {
        byte[] encoded = CreateEncoding(
            [(0x07, "127.0.0.1[13579]")],
            [(0x0A, 0xFFFF, "")]);

        InterfacePointerBody decoded = Decode(WrapInStandardObjRef(encoded));

        await Assert.That(Encode(decoded)).IsEquivalentTo(WrapInStandardObjRef(encoded));
    }

    [Test]
    public async Task Decode_MultipleBindings_RoundTrips()
    {
        byte[] encoded = CreateEncoding(
            [(0x07, "server.example[135]"), (0x0F, "server")],
            [(0x0A, 0xFFFF, ""), (0x10, 0xFFFF, "HOST/server.example")]);

        InterfacePointerBody decoded = Decode(WrapInStandardObjRef(encoded));

        await Assert.That(Encode(decoded)).IsEquivalentTo(WrapInStandardObjRef(encoded));
    }

    [Test]
    public async Task Decode_EmptyBindingLists_RoundTrips()
    {
        byte[] encoded = CreateEncoding([], []);

        InterfacePointerBody decoded = Decode(WrapInStandardObjRef(encoded));

        await Assert.That(Encode(decoded)).IsEquivalentTo(WrapInStandardObjRef(encoded));
    }

    [Test]
    public async Task Decode_EmptyDeclaredArray_RoundTrips()
    {
        byte[] encoded = CreateEmptyDeclaredEncoding();

        InterfacePointerBody decoded = Decode(WrapInStandardObjRef(encoded));

        await Assert.That(Encode(decoded)).IsEquivalentTo(WrapInStandardObjRef(encoded));
    }

    [Test]
    [Arguments("security-offset-zero")]
    [Arguments("security-offset-at-end")]
    [Arguments("security-offset-too-large")]
    [Arguments("missing-string-list-terminator")]
    [Arguments("unterminated-string-binding")]
    [Arguments("early-string-list-terminator")]
    [Arguments("missing-security-list-terminator")]
    [Arguments("unterminated-security-binding")]
    [Arguments("early-security-list-terminator")]
    public async Task Decode_MalformedEncoding_Rejects(string caseName)
    {
        byte[] encoded = CreateMalformedEncoding(caseName);

        await Assert.That(() => Decode(WrapInStandardObjRef(encoded))).Throws<InvalidDataException>();
    }

    [Test]
    [Arguments("truncated-header")]
    [Arguments("truncated-payload")]
    public async Task Decode_TruncatedEncoding_PreservesEndOfStreamException(string caseName)
    {
        byte[] encoded = CreateMalformedEncoding(caseName);

        await Assert.That(() => Decode(WrapInStandardObjRef(encoded))).Throws<EndOfStreamException>();
    }

    [Test]
    [Category("Fuzz")]
    public void Decode_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static bytes => FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static input => Decode(WrapInStandardObjRef(input.Span)),
                AllowedFuzzExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);
    }

    [Test]
    [Category("Fuzz")]
    public void Decode_MutatedValidEncoding_DoesNotCrash()
    {
        byte[] valid = CreateEncoding(
            [(0x07, "127.0.0.1[13579]")],
            [(0x0A, 0xFFFF, "")]);

        FuzzHarness.MutateValid(valid).Sample(
            static bytes => FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static input => Decode(WrapInStandardObjRef(input.Span)),
                AllowedFuzzExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);
    }

    private static InterfacePointerBody Decode(byte[] encoded)
    {
        var buffer = new NdrBuffer(encoded, 0);
        buffer.SetLength(encoded.Length);
        return InterfacePointerBody.Decode(new NdrCodec { Buffer = buffer }, 0);
    }

    private static byte[] Encode(InterfacePointerBody value)
    {
        var ndr = CreateWriter(4096);
        value.Encode(ndr, 0);
        return ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length).ToArray();
    }

    private static byte[] CreateEncoding(
        (int TowerId, string NetworkAddress)[] stringBindings,
        (int AuthnSvc, int AuthzSvc, string PrincipalName)[] securityBindings)
    {
        var stringWords = new List<int>();
        foreach ((int towerId, string networkAddress) in stringBindings)
        {
            stringWords.Add(towerId);
            AddString(stringWords, networkAddress);
        }
        stringWords.Add(0);

        var securityWords = new List<int>();
        foreach ((int authnSvc, int authzSvc, string principalName) in securityBindings)
        {
            securityWords.Add(authnSvc);
            securityWords.Add(authzSvc);
            AddString(securityWords, principalName);
        }
        securityWords.Add(0);

        var ndr = CreateWriter(4096);
        ndr.WriteUnsignedShort(stringWords.Count + securityWords.Count);
        ndr.WriteUnsignedShort(stringWords.Count);
        foreach (int word in stringWords)
        {
            ndr.WriteUnsignedShort(word);
        }
        foreach (int word in securityWords)
        {
            ndr.WriteUnsignedShort(word);
        }
        return ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length).ToArray();
    }

    private static byte[] CreateMalformedEncoding(string caseName)
    {
        var ndr = CreateWriter(4096);
        switch (caseName)
        {
            case "truncated-header":
                ndr.WriteUnsignedShort(2);
                break;
            case "truncated-payload":
                WriteWords(ndr, 4, 2, 0, 0, 0);
                break;
            case "security-offset-zero":
                WriteWords(ndr, 2, 0, 0, 0);
                break;
            case "security-offset-at-end":
                WriteWords(ndr, 2, 2, 0, 0);
                break;
            case "security-offset-too-large":
                WriteWords(ndr, 2, 3, 0, 0);
                break;
            case "missing-string-list-terminator":
                WriteWords(ndr, 4, 2, 0x07, 0, 0x0A, 0);
                break;
            case "unterminated-string-binding":
                WriteWords(ndr, 5, 3, 0x07, 'a', 'b', 0, 0);
                break;
            case "early-string-list-terminator":
                WriteWords(ndr, 4, 2, 0, 0x07, 0, 0);
                break;
            case "missing-security-list-terminator":
                WriteWords(ndr, 5, 1, 0, 0x0A, 0xFFFF, 0, 0x10);
                break;
            case "unterminated-security-binding":
                WriteWords(ndr, 5, 1, 0, 0x0A, 0xFFFF, 'a', 'b');
                break;
            case "early-security-list-terminator":
                WriteWords(ndr, 3, 1, 0, 0, 0x0A);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unknown malformed encoding.");
        }
        return ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length).ToArray();
    }

    private static byte[] CreateEmptyDeclaredEncoding()
    {
        var ndr = CreateWriter(4);
        WriteWords(ndr, 0, 0);
        return ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length).ToArray();
    }

    private static void AddString(List<int> words, string value)
    {
        foreach (char ch in value)
        {
            words.Add(ch);
        }
        words.Add(0);
    }

    private static void WriteWords(NdrCodec ndr, params int[] words)
    {
        foreach (int word in words)
        {
            ndr.WriteUnsignedShort(word);
        }
    }

    private static byte[] WrapInStandardObjRef(ReadOnlySpan<byte> dualStringArray)
    {
        var ndr = CreateWriter(dualStringArray.Length + 128);
        ndr.WriteOctetArray(ObjRefSignature, 0, ObjRefSignature.Length);
        ndr.WriteUnsignedLong(ObjRefStandard);
        InterfacePointerBody.WriteUuid(ndr, Iid, "DUALSTRINGARRAY test IID");
        ndr.WriteUnsignedLong(SorfNoping);
        ndr.WriteUnsignedLong(5);
        ndr.WriteOctetArray([0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF], 0, 8);
        ndr.WriteOctetArray([0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE], 0, 8);
        InterfacePointerBody.WriteUuid(ndr, Ipid, "DUALSTRINGARRAY test IPID");
        byte[] payload = dualStringArray.ToArray();
        ndr.WriteOctetArray(payload, 0, payload.Length);
        return WithLengthPrefix(ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length));
    }

    private static byte[] WithLengthPrefix(ReadOnlySpan<byte> objRef)
    {
        byte[] encoded = new byte[8 + objRef.Length];
        BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(0, 4), objRef.Length);
        BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(4, 4), objRef.Length);
        objRef.CopyTo(encoded.AsSpan(8));
        return encoded;
    }

    private static NdrCodec CreateWriter(int capacity) =>
        new() { Buffer = new NdrBuffer(new byte[capacity], 0) };
}
