//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class Rfc4121MicTokenTests
{
    private static readonly byte[] Aes256Key = KerberosTestHex.FromHex(
        "00112233445566778899AABBCCDDEEFF" +
        "102132435465768798A9BACBDCEDFE0F");

    [Test]
    public async Task GetMic_and_VerifyMic_round_trip()
    {
        var session = new KerberosSession(Aes256Key, EncryptionType.AES256_CTS_HMAC_SHA1_96);
        byte[] data = [0xDE, 0xAD, 0xBE, 0xEF];

        byte[] mic = session.GetMic(data);
        bool verified = session.VerifyMic(data, mic);

        await Assert.That(verified).IsTrue();
    }

    [Test]
    public async Task GetMic_token_has_RFC4121_MIC_token_id_and_filler()
    {
        var session = new KerberosSession(Aes256Key, EncryptionType.AES256_CTS_HMAC_SHA1_96);

        byte[] mic = session.GetMic([0x01, 0x02]);

        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(mic)).IsEqualTo((ushort)0x0404);
        await Assert.That(mic[2]).IsEqualTo((byte)0x00);
        await Assert.That(mic[3]).IsEqualTo((byte)0xFF);
        await Assert.That(mic[4]).IsEqualTo((byte)0xFF);
        await Assert.That(mic[5]).IsEqualTo((byte)0xFF);
        await Assert.That(mic[6]).IsEqualTo((byte)0xFF);
        await Assert.That(mic[7]).IsEqualTo((byte)0xFF);
        await Assert.That(BinaryPrimitives.ReadInt64BigEndian(mic.AsSpan(8))).IsEqualTo(0);
    }

    [Test]
    public async Task VerifyMic_returns_false_for_tampered_data()
    {
        var session = new KerberosSession(Aes256Key, EncryptionType.AES256_CTS_HMAC_SHA1_96);
        byte[] mic = session.GetMic([0x01, 0x02, 0x03]);

        bool verified = session.VerifyMic([0x01, 0x02, 0x7F], mic);

        await Assert.That(verified).IsFalse();
    }
}
