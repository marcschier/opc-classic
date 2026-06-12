//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class Smb2NegotiateMessagesTests
{
    [Test]
    public async Task NegotiateRequest_WriteTo_ProducesExpectedLayout()
    {
        var req = new Smb2NegotiateRequest(
            SecurityMode: 0x01,
            Capabilities: 0,
            ClientGuid: Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"),
            Dialects: new[] { Smb2Dialect.Smb202, Smb2Dialect.Smb210, Smb2Dialect.Smb300, Smb2Dialect.Smb311 });

        byte[] buf = new byte[256];
        int written = req.WriteTo(buf);

        // StructureSize = 36
        ushort structSize = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(0));
        await Assert.That(structSize).IsEqualTo((ushort)36);

        // DialectCount = 4
        ushort dialectCount = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(2));
        await Assert.That(dialectCount).IsEqualTo((ushort)4);

        // SecurityMode = 0x01
        ushort secMode = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(4));
        await Assert.That(secMode).IsEqualTo((ushort)0x01);

        // First dialect = 0x0202
        ushort firstDialect = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(36));
        await Assert.That(firstDialect).IsEqualTo((ushort)0x0202);

        // Last dialect = 0x0311
        ushort lastDialect = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(36 + 6));
        await Assert.That(lastDialect).IsEqualTo((ushort)0x0311);

        // Total = 36 + 4*2 = 44
        await Assert.That(written).IsEqualTo(44);
    }

    [Test]
    public async Task NegotiateResponse_Read_ParsesDialectAndServerGuid()
    {
        // Body layout per [MS-SMB2] §2.2.4. Offsets in the body are computed RELATIVE
        // to the SMB2 packet header start (i.e. 64 bytes before the body).
        byte[] buf = new byte[72];
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(0), 65);
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(2), 0x01);
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(4), 0x0300);
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(6), 0);
        var serverGuid = Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00");
        serverGuid.TryWriteBytes(buf.AsSpan(8, 16));
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(24), 0x00000007);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(28), 0x10000);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(32), 0x10000);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(36), 0x10000);
        // SecurityBufferOffset = 64 (header) + 64 (fixed body) = 128 = 0x80
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(56), 128);
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(58), 8);
        // Inline 8-byte SPNEGO sentinel at body offset 64
        byte[] spnego = [0x60, 0x82, 0x00, 0x44, 0x06, 0x06, 0x2b, 0x06];
        Array.Copy(spnego, 0, buf, 64, 8);

        var resp = Smb2NegotiateResponse.Read(buf);
        await Assert.That(resp.Dialect).IsEqualTo(Smb2Dialect.Smb300);
        await Assert.That(resp.ServerGuid).IsEqualTo(serverGuid);
        await Assert.That(resp.SecurityBuffer.Length).IsEqualTo(8);
        await Assert.That(resp.SecurityBuffer.Span[0]).IsEqualTo((byte)0x60);
    }
}
