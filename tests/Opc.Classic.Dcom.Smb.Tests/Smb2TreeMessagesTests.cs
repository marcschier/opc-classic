//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Text;
using Opc.Classic.Dcom.Smb;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class Smb2TreeMessagesTests
{
    [Test]
    public async Task TreeConnectRequest_WriteTo_EncodesUnicodePath()
    {
        const string Path = @"\\server\IPC$";
        var req = new Smb2TreeConnectRequest(Path);
        byte[] buf = new byte[128];
        int written = req.WriteTo(buf);

        ushort structureSize = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(0));
        await Assert.That(structureSize).IsEqualTo((ushort)9);

        ushort pathOffset = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(4));
        await Assert.That(pathOffset).IsEqualTo((ushort)(64 + 8));

        ushort pathLength = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(6));
        await Assert.That(pathLength).IsEqualTo((ushort)(Path.Length * 2));

        string decoded = Encoding.Unicode.GetString(buf, 8, pathLength);
        await Assert.That(decoded).IsEqualTo(Path);

        await Assert.That(written).IsEqualTo(8 + (Path.Length * 2));
    }

    [Test]
    public async Task TreeConnectResponse_Read_ParsesShareTypeAndMaximalAccess()
    {
        byte[] buf = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(0), 16);
        buf[2] = 0x02;
        buf[3] = 0;
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(4), 0x40);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(8), 0x800);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(12), 0x001F01FF);

        var resp = Smb2TreeConnectResponse.Read(buf);
        await Assert.That(resp.ShareType).IsEqualTo((byte)0x02);
        await Assert.That(resp.ShareFlags).IsEqualTo(0x40u);
        await Assert.That(resp.Capabilities).IsEqualTo(0x800u);
        await Assert.That(resp.MaximalAccess).IsEqualTo(0x001F01FFu);
    }
}
