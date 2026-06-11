//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.IO;
using CsCheck;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Tests.Fuzz;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Fuzz.Objref;

public sealed class ObjrefFuzzTests
{
    private const string Iid = "11111111-2222-3333-4455-66778899aabb";
    private const string Ipid = "aaaaaaaa-bbbb-cccc-ddee-ff0011223344";
    private const int ObjRefStandard = 0x1;
    private const int SorfNoping = 0x1000;
    private const int ComAuthzNone = 0xFFFF;
    private static readonly Type[] AllowedObjRefExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(FormatException),
    ];

    private static readonly byte[] ObjRefSignature = [0x4D, 0x45, 0x4F, 0x57];

    [Test]
    [Category("Fuzz")]
    public void InterfacePointer_Parse_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static bytes => FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                ParseObjRef,
                AllowedObjRefExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);
    }

    [Test]
    [Category("Fuzz")]
    public void InterfacePointer_Parse_MutatedValid_DoesNotCrash()
    {
        byte[] valid = BuildObjRef(static ndr =>
        {
            WriteStdObjRef(ndr, publicRefs: 5);
            WriteDualStringArray(ndr);
        });

        FuzzHarness.MutateValid(valid).Sample(
            static bytes => FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                ParseObjRef,
                AllowedObjRefExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);
    }

    [Test]
    [Category("Fuzz")]
    public async Task InterfacePointer_Parse_DualStringArrayOverflow_Bounded()
    {
        byte[] objRef = BuildObjRef(static ndr =>
        {
            WriteStdObjRef(ndr, publicRefs: 5);
            ndr.WriteUnsignedShort(0xFFFF);
            ndr.WriteUnsignedShort(1);
            byte[] tail = new byte[16];
            Array.Fill(tail, (byte)0xFF);
            ndr.WriteOctetArray(tail, 0, tail.Length);
        });

        await Assert.That(() => ParseObjRef(objRef)).Throws<EndOfStreamException>();
    }

    [Test]
    [Category("Fuzz")]
    public async Task InterfacePointer_Parse_BadSignature_Rejected()
    {
        byte[] objRef = BuildObjRef(static ndr =>
        {
            WriteStdObjRef(ndr, publicRefs: 5);
            WriteDualStringArray(ndr);
        });
        objRef[8 + 3] = (byte)'X';

        await Assert.That(() => ParseObjRef(objRef)).Throws<InvalidDataException>();
    }

    [Test]
    [Category("Fuzz")]
    public async Task InterfacePointer_Parse_StandardFlagWithBadCount_Rejected()
    {
        byte[] objRef = BuildObjRef(static ndr =>
        {
            WriteStdObjRef(ndr, publicRefs: unchecked((int)0xFFFFFFFF));
            WriteDualStringArray(ndr);
        });

        await Assert.That(() => ParseObjRef(objRef)).Throws<InvalidDataException>();
    }

    [Test]
    [Category("Fuzz")]
    [Skip("OxidResolver is internal and exposes no public decode surface reachable without changing production InternalsVisibleTo.")]
    public void OxidResolver_RemoteActivationResult_RandomBytes_DoesNotCrash()
    {
    }

    [Test]
    [Category("Fuzz")]
    public void InterfacePointer_Parse_Corpus_DoesNotCrash()
    {
        foreach (object[] row in FuzzHarness.LoadCorpus("Objref"))
        {
            byte[] bytes = (byte[])row[0];
            FuzzHarness.AssertParseDoesNotCrash(bytes, ParseObjRef, AllowedObjRefExceptions);
        }
    }

    private static InterfacePointerBody ParseObjRef(ReadOnlyMemory<byte> input)
    {
        byte[] bytes = input.ToArray();
        var ndr = new NdrCodec { Buffer = new NdrBuffer(bytes, 0) };
        ndr.Buffer.SetLength(bytes.Length);
        InterfacePointerBody? body = InterfacePointerBody.Decode(ndr, 0);
        if (body is null)
        {
            throw new InvalidDataException("OBJREF signature was not recognized.");
        }

        object? stdObjRef = body.GetObjectReference(ObjRefStandard);
        if (stdObjRef is not null)
        {
            int publicRefs = (int)stdObjRef.GetType().GetProperty("PublicRefs")!.GetValue(stdObjRef)!;
            if (publicRefs < 0)
            {
                throw new InvalidDataException("STDOBJREF public reference count was negative.");
            }
        }

        return body;
    }

    private static byte[] BuildObjRef(Action<NdrCodec> writeBody)
    {
        var ndr = CreateWriter();
        ndr.WriteOctetArray(ObjRefSignature, 0, 4);
        ndr.WriteUnsignedLong(ObjRefStandard);
        InterfacePointerBody.WriteUuid(ndr, Iid, "objref fuzz iid");
        writeBody(ndr);
        return WithLengthPrefix(ToArray(ndr));
    }

    private static void WriteStdObjRef(NdrCodec ndr, int publicRefs)
    {
        ndr.WriteUnsignedLong(SorfNoping);
        ndr.WriteUnsignedLong(publicRefs);
        byte[] oxid = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF];
        byte[] oid = [0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE];
        ndr.WriteOctetArray(oxid, 0, oxid.Length);
        ndr.WriteOctetArray(oid, 0, oid.Length);
        InterfacePointerBody.WriteUuid(ndr, Ipid, "objref fuzz ipid");
    }

    private static void WriteDualStringArray(NdrCodec ndr)
    {
        const string networkAddress = "127.0.0.1[13579]";
        int stringBindingLength = 2 + (networkAddress.Length * 2) + 2;
        int securityOffsetBytes = stringBindingLength + 2;
        int securityBindingLength = 2 + 2 + 2;
        int entryBytes = stringBindingLength + 2 + securityBindingLength + 2;

        ndr.WriteUnsignedShort(entryBytes / 2);
        ndr.WriteUnsignedShort(securityOffsetBytes / 2);
        ndr.WriteUnsignedShort(0x07);
        foreach (char ch in networkAddress)
        {
            ndr.WriteUnsignedShort(ch);
        }

        ndr.WriteUnsignedShort(0);
        ndr.WriteUnsignedShort(0);
        ndr.WriteUnsignedShort(0x0A);
        ndr.WriteUnsignedShort(ComAuthzNone);
        ndr.WriteUnsignedShort(0);
        ndr.WriteUnsignedShort(0);
    }

    private static byte[] WithLengthPrefix(byte[] objRef)
    {
        byte[] encoded = new byte[8 + objRef.Length];
        BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(0, 4), objRef.Length);
        BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(4, 4), objRef.Length);
        objRef.CopyTo(encoded.AsSpan(8));
        return encoded;
    }

    private static NdrCodec CreateWriter() =>
        new() { Buffer = new NdrBuffer(new byte[1024], 0) };

    private static byte[] ToArray(NdrCodec ndr) =>
        ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length).ToArray();
}
