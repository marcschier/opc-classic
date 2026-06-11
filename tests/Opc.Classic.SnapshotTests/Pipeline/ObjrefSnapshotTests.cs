//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.SnapshotTests.Support;
using Opc.Classic.Dcom.Core;
using TUnit.Core;

namespace Opc.Classic.SnapshotTests.Pipeline;

public sealed class ObjrefSnapshotTests
{
    private const string Iid = "11111111-2222-3333-4455-66778899aabb";
    private const string Ipid = "aaaaaaaa-bbbb-cccc-ddee-ff0011223344";
    private const string HandlerClsid = "12345678-1234-5678-90ab-cdef01234567";
    private const string CustomClsid = "87654321-4321-8765-0fed-cba987654321";
    private const int ObjRefStandard = 0x1;
    private const int ObjRefHandler = 0x2;
    private const int ObjRefCustom = 0x4;
    private const int SorfNoping = 0x1000;
    private const int ComAuthzNone = 0xFFFF;
    private static readonly byte[] ObjRefSignature = [0x4D, 0x45, 0x4F, 0x57];

    [Test]
    public async Task ObjrefStandard_with_single_tcp_binding_encodes_to_stable_bytes() =>
        await SnapshotVerifier.VerifyBytes(
            "OBJREF_STANDARD",
            "known IID/OXID/OID/IPID with one TCP binding",
            BuildObjRef(ObjRefStandard, static ndr =>
            {
                WriteStdObjRef(ndr);
                WriteDualStringArray(ndr);
            }));

    [Test]
    public async Task ObjrefHandler_with_single_tcp_binding_encodes_to_stable_bytes() =>
        await SnapshotVerifier.VerifyBytes(
            "OBJREF_HANDLER",
            "known IID/stdobjref/handler CLSID with one TCP binding",
            BuildObjRef(ObjRefHandler, static ndr =>
            {
                WriteStdObjRef(ndr);
                InterfacePointerBody.WriteUuid(ndr, HandlerClsid, "snapshot handler clsid");
                WriteDualStringArray(ndr);
            }));

    [Test]
    public async Task ObjrefCustom_encodes_to_stable_bytes() =>
        await SnapshotVerifier.VerifyBytes(
            "OBJREF_CUSTOM",
            "known IID/custom CLSID and opaque extension payload",
            BuildObjRef(ObjRefCustom, static ndr =>
            {
                byte[] payload = [0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x02];
                InterfacePointerBody.WriteUuid(ndr, CustomClsid, "snapshot custom clsid");
                ndr.WriteUnsignedLong(0x12345678);
                ndr.WriteUnsignedLong(unchecked((int)0xAABBCCDD));
                ndr.WriteOctetArray(payload, 0, payload.Length);
            }));

    private static byte[] BuildObjRef(int objectType, Action<NdrCodec> writeBody)
    {
        var ndr = CreateWriter();
        ndr.WriteOctetArray(ObjRefSignature, 0, 4);
        ndr.WriteUnsignedLong(objectType);
        InterfacePointerBody.WriteUuid(ndr, Iid, "snapshot iid");
        writeBody(ndr);
        return WithLengthPrefix(ToArray(ndr));
    }

    private static void WriteStdObjRef(NdrCodec ndr)
    {
        ndr.WriteUnsignedLong(SorfNoping);
        ndr.WriteUnsignedLong(5);
        byte[] oxid = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF];
        byte[] oid = [0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE];
        ndr.WriteOctetArray(oxid, 0, oxid.Length);
        ndr.WriteOctetArray(oid, 0, oid.Length);
        InterfacePointerBody.WriteUuid(ndr, Ipid, "snapshot ipid");
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
