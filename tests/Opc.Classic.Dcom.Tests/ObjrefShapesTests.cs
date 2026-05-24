//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Core;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class ObjrefShapesTests
{
    private const string Iid = "11111111-2222-3333-4455-66778899aabb";
    private const string Ipid = "aaaaaaaa-bbbb-cccc-ddee-ff0011223344";
    private const string HandlerClsid = "12345678-1234-5678-90ab-cdef01234567";
    private const string CustomClsid = "87654321-4321-8765-0fed-cba987654321";
    private const int ObjRefStandard = 0x1;
    private const int ObjRefHandler = 0x2;
    private const int ObjRefCustom = 0x4;
    private const int ObjRefExtended = 0x8;
    private const int SorfNoping = 0x1000;
    private const int ComAuthzNone = 0xffff;
    private const int ExtendedSignature = unchecked((int)0x4e535956);
    private static readonly byte[] ObjRefSignature = [0x4d, 0x45, 0x4f, 0x57];

    [Test]
    public async Task RoundTrip_standard_objref()
    {
        byte[] encoded = BuildObjRef(ObjRefStandard, ndr =>
        {
            WriteStdObjRef(ndr);
            WriteDualStringArray(ndr);
        });

        InterfacePointerBody decoded = Decode(encoded);

        await Assert.That(decoded.ObjectType).IsEqualTo(ObjRefStandard);
        await Assert.That(decoded.GetObjectReference(ObjRefStandard) is not null).IsTrue();
        await Assert.That(Encode(decoded)).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task RoundTrip_handler_objref()
    {
        byte[] encoded = BuildObjRef(ObjRefHandler, ndr =>
        {
            WriteStdObjRef(ndr);
            InterfacePointerBody.WriteUuid(ndr, HandlerClsid, "test handler clsid");
            WriteDualStringArray(ndr);
        });

        InterfacePointerBody decoded = Decode(encoded);
        var handler = decoded as HandlerInterfacePointerBody;

        await Assert.That(handler is not null).IsTrue();
        await Assert.That(decoded.ObjectType).IsEqualTo(ObjRefHandler);
        await Assert.That(string.Equals(handler!.HandlerCLSID, HandlerClsid, StringComparison.OrdinalIgnoreCase)).IsTrue();
        await Assert.That(Encode(decoded)).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task RoundTrip_custom_objref()
    {
        byte[] payload = [0xde, 0xad, 0xbe, 0xef, 0x01, 0x02];
        const int cbExtension = 0x12345678;
        const int reserved = unchecked((int)0xaabbccdd);
        byte[] encoded = BuildObjRef(ObjRefCustom, ndr =>
        {
            InterfacePointerBody.WriteUuid(ndr, CustomClsid, "test custom clsid");
            ndr.WriteUnsignedLong(cbExtension);
            ndr.WriteUnsignedLong(reserved);
            ndr.WriteOctetArray(payload, 0, payload.Length);
        });

        InterfacePointerBody decoded = Decode(encoded);
        var custom = decoded as CustomInterfacePointerBody;

        await Assert.That(custom is not null).IsTrue();
        await Assert.That(decoded.ObjectType).IsEqualTo(ObjRefCustom);
        await Assert.That(string.Equals(custom!.CLSID, CustomClsid, StringComparison.OrdinalIgnoreCase)).IsTrue();
        await Assert.That(custom.ExtensionSize).IsEqualTo(cbExtension);
        await Assert.That(custom.Reserved).IsEqualTo(reserved);
        await Assert.That(custom.ObjectData).IsEquivalentTo(payload);
        await Assert.That(Encode(decoded)).IsEquivalentTo(encoded);
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    public async Task RoundTrip_extended_objref(int extensionCount)
    {
        byte[] encoded = BuildObjRef(ObjRefExtended, ndr =>
        {
            WriteStdObjRef(ndr);
            ndr.WriteUnsignedLong(ExtendedSignature);
            WriteDualStringArray(ndr);
            ndr.WriteUnsignedLong(extensionCount);
            ndr.WriteUnsignedLong(ExtendedSignature);
            for (int i = 0; i < extensionCount; i++)
            {
                WriteExtension(ndr, i);
            }
        });

        InterfacePointerBody decoded = Decode(encoded);
        var extended = decoded as ExtendedInterfacePointerBody;

        await Assert.That(extended is not null).IsTrue();
        await Assert.That(decoded.ObjectType).IsEqualTo(ObjRefExtended);
        await Assert.That(extended!.Extensions.Count).IsEqualTo(extensionCount);
        byte[] roundTrip = Encode(decoded);
        await Assert.That(roundTrip.Length).IsEqualTo(encoded.Length);
        await Assert.That(roundTrip).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task Unknown_flag_returns_unknown_body_with_raw_bytes()
    {
        byte[] raw = [0x10, 0x20, 0x30, 0x40, 0x50];
        byte[] encoded = BuildObjRef(0x10, ndr => ndr.WriteOctetArray(raw, 0, raw.Length));

        InterfacePointerBody decoded = Decode(encoded);
        var unknown = decoded as UnknownInterfacePointerBody;

        await Assert.That(unknown is not null).IsTrue();
        await Assert.That(decoded.ObjectType).IsEqualTo(0x10);
        await Assert.That(unknown!.RawBytes).IsEquivalentTo(raw);
        await Assert.That(Encode(decoded)).IsEquivalentTo(encoded);
    }

    private static InterfacePointerBody Decode(byte[] encoded)
    {
        var ndr = CreateReader(encoded);
        return InterfacePointerBody.Decode(ndr, 0);
    }

    private static byte[] Encode(InterfacePointerBody body)
    {
        var ndr = CreateWriter();
        body.Encode(ndr, 0);
        return ToArray(ndr);
    }

    private static byte[] BuildObjRef(int objectType, Action<NdrCodec> writeBody)
    {
        var ndr = CreateWriter();
        ndr.WriteOctetArray(ObjRefSignature, 0, 4);
        ndr.WriteUnsignedLong(objectType);
        InterfacePointerBody.WriteUuid(ndr, Iid, "test iid");
        writeBody(ndr);
        return WithLengthPrefix(ToArray(ndr));
    }

    private static void WriteStdObjRef(NdrCodec ndr)
    {
        ndr.WriteUnsignedLong(SorfNoping);
        ndr.WriteUnsignedLong(5);
        byte[] oxid = [0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef];
        byte[] oid = [0x10, 0x32, 0x54, 0x76, 0x98, 0xba, 0xdc, 0xfe];
        ndr.WriteOctetArray(oxid, 0, oxid.Length);
        ndr.WriteOctetArray(oid, 0, oid.Length);
        InterfacePointerBody.WriteUuid(ndr, Ipid, "test ipid");
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
        ndr.WriteUnsignedShort(0x0a);
        ndr.WriteUnsignedShort(ComAuthzNone);
        ndr.WriteUnsignedShort(0);
        ndr.WriteUnsignedShort(0);
    }

    private static void WriteExtension(NdrCodec ndr, int index)
    {
        string id = index == 0
            ? "aaaaaaaa-0000-0000-0000-000000000001"
            : "bbbbbbbb-0000-0000-0000-000000000002";
        byte[] payload = index == 0 ? [0x01, 0x02, 0x03] : [0x10, 0x20, 0x30, 0x40, 0x50];
        int roundedSize = (payload.Length + 7) & ~7;
        byte[] rounded = new byte[roundedSize];
        payload.CopyTo(rounded.AsSpan());

        InterfacePointerBody.WriteUuid(ndr, id, "test extension id");
        ndr.WriteUnsignedLong(payload.Length);
        ndr.WriteUnsignedLong(roundedSize);
        ndr.WriteOctetArray(rounded, 0, rounded.Length);
    }

    private static byte[] WithLengthPrefix(byte[] objRef)
    {
        byte[] encoded = new byte[8 + objRef.Length];
        BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(0, 4), objRef.Length);
        BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(4, 4), objRef.Length);
        objRef.CopyTo(encoded.AsSpan(8));
        return encoded;
    }

    private static NdrCodec CreateReader(byte[] bytes)
    {
        var buffer = new NdrBuffer(bytes, 0)
        {
            Length = bytes.Length,
        };
        return new NdrCodec { Buffer = buffer };
    }

    private static NdrCodec CreateWriter() =>
        new() { Buffer = new NdrBuffer(new byte[1024], 0) };

    private static byte[] ToArray(NdrCodec ndr) =>
        ndr.Buffer.Buf.AsSpan(0, ndr.Buffer.Length).ToArray();
}
