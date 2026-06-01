//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Wire-format fixture tests for OPC DA NDR codecs. Pins the on-the-wire
// layout at specific byte offsets so that codec refactors (e.g. the
// Y9b deferred-pointer migration) must explicitly update the fixture.
//
// The fixtures focus on shape (offsets, sizes, referent IDs, count
// fields) rather than full-payload hex equality so that the FILETIME
// epoch arithmetic doesn't have to be hand-computed in test source.
//

using System;
using Opc.Classic;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class NdrOpcServerStatusWireFixtures
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static OpcServerStatus BuildFixture() => new()
    {
        Spec = OpcStatusSpec.Da,
        StartTime = new DateTimeOffset(2020, 6, 10, 16, 53, 16, TimeSpan.Zero),
        CurrentTime = new DateTimeOffset(2020, 6, 10, 16, 54, 16, TimeSpan.Zero),
        LastUpdateTime = new DateTimeOffset(2020, 6, 10, 16, 54, 06, TimeSpan.Zero),
        State = OpcServerState.Running,         // 0x00000001
        GroupCount = 0x11,                      // 0x00000011
        BandWidth = 0x22,                       // 0x00000022
        ServerVersion = new Version(1, 2, 3),
        VendorInfo = "Op",                      // 2 chars + NUL terminator
    };

    [Test]
    public async Task OpcServerStatus_wire_layout_matches_OPC_DA_spec_offsets()
    {
        // Per opcda.h OPCSERVERSTATUS / MS-RPCE NDR:
        //   [0..7]   FILETIME ftStartTime         (FILETIME is 2x UInt32, 4-aligned)
        //   [8..15]  FILETIME ftCurrentTime
        //   [16..23] FILETIME ftLastUpdateTime
        //   [24..27] dwServerState (OPCSERVERSTATE)
        //   [28..31] dwGroupCount
        //   [32..35] dwBandWidth
        //   [36..37] wMajorVersion
        //   [38..39] wMinorVersion
        //   [40..41] wBuildNumber
        //   [42..43] wReserved
        //   [44..47] LPWSTR referent_id (non-zero for non-NULL)
        //   [48..51] string max_count (incl. NUL terminator)
        //   [52..55] string offset
        //   [56..59] string actual_count
        //   [60..]   WCHAR[actual_count]
        OpcServerStatus status = BuildFixture();
        byte[] wire = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, status));

        await Assert.That(wire.Length).IsEqualTo(66);
        // Scalars after the FILETIME block.
        await Assert.That(WireAssert.ReadUInt32At(wire, 24)).IsEqualTo(1u);   // state
        await Assert.That(WireAssert.ReadUInt32At(wire, 28)).IsEqualTo(0x11u); // groupCount
        await Assert.That(WireAssert.ReadUInt32At(wire, 32)).IsEqualTo(0x22u); // bandWidth
        await Assert.That(WireAssert.ReadUInt16At(wire, 36)).IsEqualTo((ushort)1);
        await Assert.That(WireAssert.ReadUInt16At(wire, 38)).IsEqualTo((ushort)2);
        await Assert.That(WireAssert.ReadUInt16At(wire, 40)).IsEqualTo((ushort)3);
        await Assert.That(WireAssert.ReadUInt16At(wire, 42)).IsEqualTo((ushort)0);
        // VendorInfo: unique-pointer LPWSTR.
        await Assert.That(WireAssert.ReadUInt32At(wire, 44)).IsEqualTo(0x00020000u); // referent
        await Assert.That(WireAssert.ReadUInt32At(wire, 48)).IsEqualTo(3u); // max_count = 2 chars + NUL
        await Assert.That(WireAssert.ReadUInt32At(wire, 52)).IsEqualTo(0u); // offset
        await Assert.That(WireAssert.ReadUInt32At(wire, 56)).IsEqualTo(3u); // actual_count
        // WCHAR[3] = 'O' 'p' '\0'
        await Assert.That(wire[60]).IsEqualTo((byte)'O');
        await Assert.That(wire[61]).IsEqualTo((byte)0);
        await Assert.That(wire[62]).IsEqualTo((byte)'p');
        await Assert.That(wire[63]).IsEqualTo((byte)0);
        await Assert.That(wire[64]).IsEqualTo((byte)0);
        await Assert.That(wire[65]).IsEqualTo((byte)0);
    }

    [Test]
    public async Task OpcServerStatus_round_trips_through_pinned_wire_layout()
    {
        OpcServerStatus expected = BuildFixture();
        byte[] wire = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, expected));

        var reader = new NdrReader(wire);
        OpcServerStatus actual = NdrOpcServerStatusCodec.Read(ref reader);

        await Assert.That(actual.State).IsEqualTo(expected.State);
        await Assert.That(actual.GroupCount).IsEqualTo(expected.GroupCount);
        await Assert.That(actual.BandWidth).IsEqualTo(expected.BandWidth);
        await Assert.That(actual.ServerVersion).IsEqualTo(expected.ServerVersion);
        await Assert.That(actual.VendorInfo).IsEqualTo(expected.VendorInfo);
        await Assert.That(actual.StartTime.UtcDateTime).IsEqualTo(expected.StartTime.UtcDateTime);
        await Assert.That(actual.CurrentTime.UtcDateTime).IsEqualTo(expected.CurrentTime.UtcDateTime);
        await Assert.That(actual.LastUpdateTime.UtcDateTime).IsEqualTo(expected.LastUpdateTime.UtcDateTime);
    }

    [Test]
    public async Task OpcServerStatus_null_VendorInfo_is_zero_referent()
    {
        OpcServerStatus expected = new()
        {
            Spec = OpcStatusSpec.Da,
            StartTime = new DateTimeOffset(2020, 6, 10, 16, 53, 16, TimeSpan.Zero),
            CurrentTime = new DateTimeOffset(2020, 6, 10, 16, 54, 16, TimeSpan.Zero),
            LastUpdateTime = new DateTimeOffset(2020, 6, 10, 16, 54, 06, TimeSpan.Zero),
            State = OpcServerState.Running,
            GroupCount = 0x11,
            BandWidth = 0x22,
            ServerVersion = new Version(1, 2, 3),
            VendorInfo = null!,
        };
        byte[] wire = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, expected));

        // Without a string body, the wire ends at the LPWSTR referent (offset 44).
        await Assert.That(wire.Length).IsEqualTo(48);
        await Assert.That(WireAssert.ReadUInt32At(wire, 44)).IsEqualTo(0u);
    }

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }
}
