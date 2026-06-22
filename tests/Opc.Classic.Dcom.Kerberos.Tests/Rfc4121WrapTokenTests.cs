// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class Rfc4121WrapTokenTests
{
    private static readonly byte[] Aes128Key = KerberosTestHex.FromHex("00112233445566778899AABBCCDDEEFF");

    [Test]
    public async Task Wrap_integrity_token_has_RFC4121_header_fields()
    {
        var session = new KerberosSession(Aes128Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        byte[] token = session.WrapMessage([0x01, 0x02, 0x03], confidential: false);

        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(token)).IsEqualTo((ushort)0x0504);
        await Assert.That(token[2]).IsEqualTo((byte)0x00);
        await Assert.That(token[3]).IsEqualTo((byte)0xFF);
        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(token.AsSpan(4))).IsEqualTo((ushort)12);
        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(token.AsSpan(6))).IsEqualTo((ushort)0);
        await Assert.That(BinaryPrimitives.ReadInt64BigEndian(token.AsSpan(8))).IsEqualTo(0);
    }

    [Test]
    public async Task Wrap_privacy_token_has_sealed_flag_and_round_trips()
    {
        var session = new KerberosSession(Aes128Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        byte[] plaintext = [0x10, 0x20, 0x30, 0x40];

        byte[] token = session.WrapMessage(plaintext, confidential: true);
        byte[] unwrapped = session.UnwrapMessage(token, out bool wasConfidential);

        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(token)).IsEqualTo((ushort)0x0504);
        await Assert.That((token[2] & 0x02) != 0).IsTrue();
        await Assert.That(wasConfidential).IsTrue();
        await Assert.That(unwrapped.SequenceEqual(plaintext)).IsTrue();
    }

    [Test]
    public async Task Wrap_integrity_only_round_trips_without_confidentiality()
    {
        var session = new KerberosSession(Aes128Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        byte[] plaintext = [0x41, 0x42, 0x43, 0x44, 0x45];

        byte[] token = session.WrapMessage(plaintext, confidential: false);
        byte[] unwrapped = session.UnwrapMessage(token, out bool wasConfidential);

        await Assert.That(wasConfidential).IsFalse();
        await Assert.That(unwrapped.SequenceEqual(plaintext)).IsTrue();
    }

    [Test]
    public async Task Wrap_and_mic_tokens_use_distinct_RFC4121_token_ids()
    {
        var session = new KerberosSession(Aes128Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);

        byte[] wrap = session.WrapMessage([0x01], confidential: false);
        byte[] mic = session.GetMic([0x01]);

        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(wrap)).IsEqualTo((ushort)0x0504);
        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(mic)).IsEqualTo((ushort)0x0404);
    }
}
