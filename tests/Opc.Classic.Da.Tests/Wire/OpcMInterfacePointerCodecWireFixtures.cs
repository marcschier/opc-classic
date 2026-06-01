//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Wire-format fixture tests for OpcMInterfacePointerCodec (Track Y9).
// Pins the MS-DCOM §2.2.1.10 MInterfacePointer wrapping shape:
//   uint  referent_id;
//   {if non-null:}
//     uint ulCntData;
//     ulCntData bytes of OBJREF
//

using System;
using System.Collections.Generic;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class OpcMInterfacePointerCodecWireFixtures
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static readonly Guid SampleIid = new("39C13A4E-011E-11D0-9675-0020AFD8ADB3");   // IID_IOPCGroupStateMgt
    private static readonly Guid SampleIpid = new("00000001-0000-0000-c000-000000000046");

    private static IOpcInterfaceRef BuildFixture() =>
        new OpcInterfaceRef(
            iid: SampleIid,
            flags: 0u,
            publicRefs: 5u,
            oxid: 0x0123_4567_89AB_CDEFul,
            oid: 0xFEDC_BA98_7654_3210ul,
            ipid: SampleIpid,
            securityOffset: 0,
            resolverBindings: new List<ushort>());

    [Test]
    public async Task Encode_non_null_emits_referent_plus_cbData_plus_OBJREF()
    {
        IOpcInterfaceRef iref = BuildFixture();
        byte[] wire = WriteOne((ref NdrWriter w) => OpcMInterfacePointerCodec.Write(ref w, iref));

        // [0..3]   referent_id     = 0x00020000 (matches Windows DCOM convention).
        // [4..7]   ulCntData       = length of OBJREF payload (Y9 sentinel: non-zero).
        // [8..11]  OBJREF.MEOW     = 0x574F454D ("MEOW" little-endian).
        // [12..15] OBJREF.objref_type = 0x00000001 (OBJREF_STANDARD).
        // [16..31] OBJREF.iid      = SampleIid (in NDR layout).
        // [32..]   OBJREF.flags + publicRefs + oxid + oid + ipid + DUALSTRINGARRAY.
        await Assert.That(WireAssert.ReadUInt32At(wire, 0)).IsEqualTo(0x00020000u);
        uint cbData = WireAssert.ReadUInt32At(wire, 4);
        await Assert.That(cbData).IsGreaterThan(0u);
        // Total wire length = 4 (referent) + 4 (cbData) + cbData bytes of OBJREF.
        await Assert.That(wire.Length).IsEqualTo(8 + (int)cbData);
        // MEOW magic at start of OBJREF body.
        await Assert.That(WireAssert.ReadUInt32At(wire, 8)).IsEqualTo(0x574F454Du);
        // OBJREF_STANDARD discriminator.
        await Assert.That(WireAssert.ReadUInt32At(wire, 12)).IsEqualTo(0x00000001u);
    }

    [Test]
    public async Task Encode_null_emits_only_zero_referent()
    {
        byte[] wire = WriteOne((ref NdrWriter w) => OpcMInterfacePointerCodec.Write(ref w, null));

        await Assert.That(wire.Length).IsEqualTo(4);
        await Assert.That(WireAssert.ReadUInt32At(wire, 0)).IsEqualTo(0u);
    }

    [Test]
    public async Task RoundTrip_preserves_oxid_oid_ipid_iid()
    {
        IOpcInterfaceRef expected = BuildFixture();
        byte[] wire = WriteOne((ref NdrWriter w) => OpcMInterfacePointerCodec.Write(ref w, expected));

        var reader = new NdrReader(wire);
        IOpcInterfaceRef? actual = OpcMInterfacePointerCodec.Read(ref reader);

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.Iid).IsEqualTo(expected.Iid);
        await Assert.That(actual.Oxid).IsEqualTo(expected.Oxid);
        await Assert.That(actual.Oid).IsEqualTo(expected.Oid);
        await Assert.That(actual.Ipid).IsEqualTo(expected.Ipid);
        await Assert.That(actual.PublicRefs).IsEqualTo(expected.PublicRefs);
    }

    [Test]
    public async Task RoundTrip_null_decodes_back_to_null()
    {
        byte[] wire = WriteOne((ref NdrWriter w) => OpcMInterfacePointerCodec.Write(ref w, null));

        var reader = new NdrReader(wire);
        IOpcInterfaceRef? actual = OpcMInterfacePointerCodec.Read(ref reader);

        await Assert.That(actual).IsNull();
    }

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }
}
