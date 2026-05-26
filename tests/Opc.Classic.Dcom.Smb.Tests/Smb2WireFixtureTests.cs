//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using Opc.Classic.Dcom.Smb;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

/// <summary>
/// Wire-format fixtures — byte sequences crafted from the published [MS-SMB2] spec
/// examples + worked-by-hand intermediate values. These regression tests prevent
/// drift in the codec layout away from the published Microsoft Open Specifications.
///
/// Capturing real-server fixtures (Wireshark `.pcapng` → byte arrays) is documented
/// in `src/Opc.Classic.Dcom.Smb/FIXTURES.md`; until that capture is performed against
/// a real Samba/Windows server, the synthetic fixtures below validate that our
/// codec implementations match the spec's documented wire layouts.
/// </summary>
public sealed class Smb2WireFixtureTests
{
    /// <summary>
    /// SMB2 packet header for a NEGOTIATE request with MessageId=1. The exact byte
    /// layout is fixed by [MS-SMB2] §2.2.1.2; verifying the on-the-wire bytes match
    /// the spec example catches any field-ordering or endianness regressions.
    /// </summary>
    [Test]
    public async Task NegotiateHeader_MatchesSpecLayout()
    {
        var header = new Smb2PacketHeader(
            CreditCharge: 0,
            Status: 0,
            Command: Smb2Command.Negotiate,
            CreditRequestResponse: 0,
            Flags: 0,
            NextCommand: 0,
            MessageId: 1,
            ProcessId: 0xfeff,
            TreeId: 0,
            SessionId: 0,
            Signature: ReadOnlyMemory<byte>.Empty);

        byte[] actual = new byte[64];
        header.Write(actual);

        // Expected byte sequence ([MS-SMB2] §2.2.1.2 field layout, little-endian):
        //   00..03  ProtocolId         FE 53 4D 42         "\xFESMB"
        //   04..05  StructureSize       40 00               64
        //   06..07  CreditCharge        00 00               0
        //   08..0B  Status              00 00 00 00         0
        //   0C..0D  Command             00 00               0 (Negotiate)
        //   0E..0F  CreditRequest       00 00               0
        //   10..13  Flags               00 00 00 00         0
        //   14..17  NextCommand         00 00 00 00         0
        //   18..1F  MessageId           01 00 00 00 00 00 00 00   1
        //   20..23  ProcessId           FF FE 00 00         0xfeff
        //   24..27  TreeId              00 00 00 00         0
        //   28..2F  SessionId           00 00 00 00 00 00 00 00   0
        //   30..3F  Signature           16 zero bytes
        byte[] expected =
        [
            0xFE, 0x53, 0x4D, 0x42,
            0x40, 0x00,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0xFE, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ];

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    /// <summary>
    /// The NetBIOS-over-TCP 4-byte length prefix carries a single 24-bit big-endian
    /// length value with the high byte set to 0x00 (Direct TCP). See [MS-CIFS] §2.2.1.
    /// </summary>
    [Test]
    public async Task NetBiosFrameHeader_24BitBigEndian()
    {
        byte[] actual = new byte[4];
        NetBiosFraming.WriteHeader(actual, 0x010203);

        byte[] expected = [0x00, 0x01, 0x02, 0x03];
        await Assert.That(actual).IsEquivalentTo(expected);

        // Round-trip
        int decoded = NetBiosFraming.ReadPayloadLength(expected);
        await Assert.That(decoded).IsEqualTo(0x010203);
    }

    /// <summary>
    /// SMB2 NEGOTIATE request body with two dialects (SMB 2.0.2 + SMB 3.1.1).
    /// Validates field ordering for StructureSize, DialectCount, SecurityMode,
    /// Reserved, Capabilities, ClientGuid, and the trailing dialect array.
    /// </summary>
    [Test]
    public async Task NegotiateRequestBody_TwoDialects()
    {
        var req = new Smb2NegotiateRequest(
            SecurityMode: 0x01,
            Capabilities: 0,
            ClientGuid: Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"),
            Dialects: new[] { Smb2Dialect.Smb202, Smb2Dialect.Smb311 });

        byte[] body = new byte[44];
        int written = req.WriteTo(body);
        await Assert.That(written).IsEqualTo(40);

        // [00..01] StructureSize 36 (decimal)
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(0))).IsEqualTo((ushort)36);
        // [02..03] DialectCount 2
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(2))).IsEqualTo((ushort)2);
        // [04..05] SecurityMode 0x01
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(4))).IsEqualTo((ushort)0x01);
        // [06..07] Reserved 0
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(6))).IsEqualTo((ushort)0);
        // [08..0B] Capabilities 0
        await Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(8))).IsEqualTo(0u);
        // [0C..1B] ClientGuid in System.Guid byte order
        var actualGuid = new Guid(body.AsSpan(12, 16));
        await Assert.That(actualGuid).IsEqualTo(Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"));
        // [24..25] First dialect 0x0202
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(36))).IsEqualTo((ushort)0x0202);
        // [26..27] Second dialect 0x0311
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(38))).IsEqualTo((ushort)0x0311);
    }
}
