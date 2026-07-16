// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class Rfc4757Rc4HmacTests
{
    private static readonly byte[] Rfc4757FooStringToKey = KerberosTestHex.FromHex("AC8E657F83DF82BEEA5D43BDAF7800CC");

    [Test]
    public async Task Rfc4757_section_2_String2Key_foo_vector_is_accepted_as_RC4_session_key()
    {
        var session = new KerberosSession(Rfc4757FooStringToKey, EncryptionType.RC4_HMAC_NT);

        byte[] token = session.WrapMessage([0x01, 0x02, 0x03], confidential: false);

        await Assert.That(token.Length).IsEqualTo(49);
        await Assert.That(token[0]).IsEqualTo((byte)0x60);
        await Assert.That(token[1]).IsEqualTo((byte)47);
        await Assert.That(token[13]).IsEqualTo((byte)0x02);
        await Assert.That(token[17]).IsEqualTo((byte)0xFF);
    }

    [Test]
    public async Task Rfc4757_HMAC_MD5_MIC_derivation_matches_expected_checksum()
    {
        var session = new KerberosSession(Rfc4757FooStringToKey, EncryptionType.RC4_HMAC_NT);

        byte[] mic = session.GetMic([0x01, 0x02, 0x03]);
        byte[] checksum = mic.AsSpan(mic.Length - 8).ToArray();

        await Assert.That(mic.Length).IsEqualTo(37);
        await Assert.That(mic[0]).IsEqualTo((byte)0x60);
        await Assert.That(mic[13]).IsEqualTo((byte)0x01);
        await Assert.That(checksum.SequenceEqual(
            KerberosTestHex.FromHex("992800C311E4F8F1"))).IsTrue();
    }

    [Test]
    public async Task Rc4Hmac_wrap_privacy_round_trips()
    {
        var sender = new KerberosSession(Rfc4757FooStringToKey, EncryptionType.RC4_HMAC_NT);
        var receiver = new KerberosSession(
            Rfc4757FooStringToKey,
            EncryptionType.RC4_HMAC_NT,
            isAcceptor: true);
        byte[] plaintext = [0x41, 0x42, 0x43, 0x44];

        byte[] token = sender.WrapMessage(plaintext, confidential: true);
        byte[] unwrapped = receiver.UnwrapMessage(token, out bool wasConfidential);

        await Assert.That(wasConfidential).IsTrue();
        await Assert.That(unwrapped.SequenceEqual(plaintext)).IsTrue();
    }

    [Test]
    public async Task MsKile_section_4_5_Rc4_GssWrapEx_known_answer_unwraps()
    {
        byte[] sessionKey = KerberosTestHex.FromHex(
            "81A2CB90AF7FC2D19554A150D8185359");
        byte[] ciphertext = KerberosTestHex.FromHex(
            "8ED63F0AC83815335B72E293BAE1F660");
        byte[] signature = KerberosTestHex.FromHex(
            "603B06092A864886F712010202020111001000FFFF" +
            "E29E8BBC6348E740EBAA619244A156A13B5CF65E3C21B9AA");
        byte[] token = signature.Concat(ciphertext).ToArray();
        var receiver = new KerberosSession(
            sessionKey,
            EncryptionType.RC4_HMAC_NT,
            initialSequenceNumber: 0x60CBACD3,
            isAcceptor: true);

        byte[] plaintext = receiver.UnwrapMessage(
            token,
            out bool wasConfidential);

        await Assert.That(wasConfidential).IsTrue();
        await Assert.That(plaintext).IsEquivalentTo(
            KerberosTestHex.FromHex(
                "112233445566778899AABBCCDDEEFF"));
    }
}
