//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Wire-format fixture tests for OpcVariant (Track Y10). Pins the
// MS-OAUT §2.2.29 _wireVARIANT layout for the common VARTYPEs used by
// OPC DA reads (VT_I4, VT_R4, VT_R8, VT_BOOL, VT_BSTR, VT_UI1) so the
// codec doesn't silently drift from the Microsoft RPC reference shape.
//

using System;
using Opc.Classic;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class NdrOpcVariantWireFixtures
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    [Test]
    public async Task VT_I4_emits_16_byte_header_plus_4_byte_int_payload()
    {
        OpcVariant v = OpcVariant.FromInt32(42);
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(v));

        // Per MS-OAUT §2.2.29.1 _wireVARIANT plus the [switch_type(ULONG),
        // switch_is(vt)] non-encapsulated union rule from C706 §14.4.1:
        //   [0..3]   clSize (DWORD) — quadwords (8-byte units)
        //   [4..7]   rpcReserved (DWORD) = 0
        //   [8..9]   vt (WORD)
        //   [10..15] wReserved1/2/3 (3x WORD = 0)
        //   [16..19] switch_type(ULONG) discriminator = vt as ULONG
        //   [20..]   union body for vt
        await Assert.That(wire.Length).IsEqualTo(24);   // 16 hdr + 4 disc + 4 int32
        await Assert.That(WireAssert.ReadUInt16At(wire, 8)).IsEqualTo((ushort)VarType.VT_I4);
        await Assert.That(WireAssert.ReadUInt32At(wire, 16)).IsEqualTo((uint)VarType.VT_I4); // discriminator
        await Assert.That(WireAssert.ReadUInt32At(wire, 20)).IsEqualTo(42u);                  // body
        // clSize ceil((16 hdr + 4 disc + 4 body) / 8) = 3 quadwords
        await Assert.That(WireAssert.ReadUInt32At(wire, 0)).IsEqualTo(3u);
        await Assert.That(WireAssert.ReadUInt32At(wire, 4)).IsEqualTo(0u);
    }

    [Test]
    public async Task VT_R4_emits_4_byte_single_payload()
    {
        OpcVariant v = OpcVariant.FromSingle(1.5f);
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(v));

        await Assert.That(wire.Length).IsEqualTo(24);   // 16 hdr + 4 disc + 4 float
        await Assert.That(WireAssert.ReadUInt16At(wire, 8)).IsEqualTo((ushort)VarType.VT_R4);
        await Assert.That(WireAssert.ReadUInt32At(wire, 16)).IsEqualTo((uint)VarType.VT_R4); // discriminator
        // 1.5f IEEE-754 single = 0x3FC00000.
        await Assert.That(WireAssert.ReadUInt32At(wire, 20)).IsEqualTo(0x3FC00000u);
    }

    [Test]
    public async Task VT_R8_emits_8_byte_double_payload_8_aligned()
    {
        OpcVariant v = OpcVariant.FromDouble(2.5);
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(v));

        // 16 hdr + 4 disc + 4 align-pad + 8 double = 32 bytes (clSize = 4 quadwords).
        await Assert.That(wire.Length).IsEqualTo(32);
        await Assert.That(WireAssert.ReadUInt16At(wire, 8)).IsEqualTo((ushort)VarType.VT_R8);
        await Assert.That(WireAssert.ReadUInt32At(wire, 16)).IsEqualTo((uint)VarType.VT_R8); // discriminator
        // Body needs 8-byte alignment, so 4 bytes of pad before it at offset 24.
        // 2.5 IEEE-754 double = 0x4004000000000000.
        await Assert.That(WireAssert.ReadUInt32At(wire, 24)).IsEqualTo(0x00000000u);
        await Assert.That(WireAssert.ReadUInt32At(wire, 28)).IsEqualTo(0x40040000u);
        await Assert.That(WireAssert.ReadUInt32At(wire, 0)).IsEqualTo(4u); // clSize
    }

    [Test]
    public async Task VT_BOOL_TRUE_emits_minus_one_word()
    {
        OpcVariant v = OpcVariant.FromBoolean(true);
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(v));

        await Assert.That(wire.Length).IsEqualTo(22);  // 16 hdr + 4 disc + 2 bool
        await Assert.That(WireAssert.ReadUInt16At(wire, 8)).IsEqualTo((ushort)VarType.VT_BOOL);
        await Assert.That(WireAssert.ReadUInt32At(wire, 16)).IsEqualTo((uint)VarType.VT_BOOL); // discriminator
        // Per MS-OAUT VARIANT_BOOL: TRUE = 0xFFFF, FALSE = 0x0000.
        await Assert.That(WireAssert.ReadUInt16At(wire, 20)).IsEqualTo((ushort)0xFFFFu);
    }

    [Test]
    public async Task VT_BSTR_emits_referent_plus_flagged_word_blob()
    {
        OpcVariant v = OpcVariant.FromString("X");
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(v));

        await Assert.That(WireAssert.ReadUInt16At(wire, 8)).IsEqualTo((ushort)VarType.VT_BSTR);
        await Assert.That(WireAssert.ReadUInt32At(wire, 16)).IsEqualTo((uint)VarType.VT_BSTR); // discriminator
        // BSTR FLAGGED_WORD_BLOB (MS-OAUT §2.2.23):
        //   uint referent_id  (20..23)
        //   uint max_count    (24..27) — conformant-array size prefix
        //   uint cBytes       (28..31)
        //   uint clSize       (32..35) — count of WCHARs (no NUL terminator)
        //   WCHAR[clSize]     (36..)
        await Assert.That(WireAssert.ReadUInt32At(wire, 20)).IsEqualTo(0x00020000u);
        await Assert.That(WireAssert.ReadUInt32At(wire, 24)).IsEqualTo(1u);  // max_count
        await Assert.That(WireAssert.ReadUInt32At(wire, 28)).IsEqualTo(2u);  // cBytes
        await Assert.That(WireAssert.ReadUInt32At(wire, 32)).IsEqualTo(1u);  // clSize
        await Assert.That(wire[36]).IsEqualTo((byte)'X');
        await Assert.That(wire[37]).IsEqualTo((byte)0);
    }

    [Test]
    public async Task VT_UI1_emits_1_byte_payload()
    {
        OpcVariant v = OpcVariant.FromUInt8(0xFE);
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(v));

        await Assert.That(WireAssert.ReadUInt16At(wire, 8)).IsEqualTo((ushort)VarType.VT_UI1);
        await Assert.That(WireAssert.ReadUInt32At(wire, 16)).IsEqualTo((uint)VarType.VT_UI1); // discriminator
        await Assert.That(wire[20]).IsEqualTo((byte)0xFE);
    }

    [Test]
    public async Task VT_I4_round_trips_through_pinned_layout()
    {
        OpcVariant expected = OpcVariant.FromInt32(0x12345678);
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(expected));

        var reader = new NdrReader(wire);
        OpcVariant actual = reader.ReadVariant();
        await Assert.That(actual.Type).IsEqualTo(VarType.VT_I4);
        await Assert.That((int)actual.Boxed!).IsEqualTo(0x12345678);
    }

    [Test]
    public async Task VT_BSTR_round_trips_through_pinned_layout()
    {
        OpcVariant expected = OpcVariant.FromString("Random.String");
        byte[] wire = WriteOne((ref NdrWriter w) => w.WriteVariant(expected));

        var reader = new NdrReader(wire);
        OpcVariant actual = reader.ReadVariant();
        await Assert.That(actual.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That((string)actual.Boxed!).IsEqualTo("Random.String");
    }

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }
}
