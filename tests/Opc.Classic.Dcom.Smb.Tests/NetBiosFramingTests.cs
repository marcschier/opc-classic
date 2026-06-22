// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class NetBiosFramingTests
{
    [Test]
    public async Task WriteHeader_EncodesPayloadLengthBigEndian()
    {
        byte[] frame = new byte[4];
        NetBiosFraming.WriteHeader(frame, 0x010203);
        await Assert.That(frame[0]).IsEqualTo((byte)0);
        await Assert.That(frame[1]).IsEqualTo((byte)0x01);
        await Assert.That(frame[2]).IsEqualTo((byte)0x02);
        await Assert.That(frame[3]).IsEqualTo((byte)0x03);
    }

    [Test]
    public async Task ReadPayloadLength_DecodesBigEndian()
    {
        byte[] frame = [0x00, 0x00, 0x01, 0x00];
        int len = NetBiosFraming.ReadPayloadLength(frame);
        await Assert.That(len).IsEqualTo(256);
    }

    [Test]
    public async Task ReadPayloadLength_RejectsNonSmbDirectFrameType()
    {
        byte[] frame = [0x81, 0x00, 0x00, 0x00];
        bool threw = false;
        try { NetBiosFraming.ReadPayloadLength(frame); }
        catch (Smb2ProtocolException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task WriteHeader_RejectsOversizedPayload()
    {
        byte[] frame = new byte[4];
        bool threw = false;
        try { NetBiosFraming.WriteHeader(frame, 0x20_0000); }
        catch (ArgumentOutOfRangeException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
