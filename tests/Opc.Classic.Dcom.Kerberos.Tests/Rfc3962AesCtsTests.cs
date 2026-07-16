// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class Rfc3962AesCtsTests
{
    [Test]
    public async Task Rfc3962_AES128_string_to_key_iteration_1_matches_vector()
    {
        byte[] key = DeriveAesSha1Key(EncryptionType.AES128_CTS_HMAC_SHA1_96, iterationCount: 1);

        await Assert.That(key.SequenceEqual(KerberosTestHex.FromHex("42263C6E89F4FC28B8DF68EE09799F15"))).IsTrue();
    }

    [Test]
    public async Task Rfc3962_AES256_string_to_key_iteration_1_matches_vector()
    {
        byte[] key = DeriveAesSha1Key(EncryptionType.AES256_CTS_HMAC_SHA1_96, iterationCount: 1);

        await Assert.That(key.SequenceEqual(KerberosTestHex.FromHex(
            "FE697B52BC0D3CE14432BA036A92E65B" +
            "BB52280990A2FA27883998D72AF30161"))).IsTrue();
    }

    [Test]
    public async Task AES128_CTS_HMAC_SHA1_96_wrap_privacy_round_trips()
    {
        byte[] key = KerberosTestHex.FromHex("00112233445566778899AABBCCDDEEFF");
        var sender = new KerberosSession(key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var receiver = new KerberosSession(
            key,
            EncryptionType.AES128_CTS_HMAC_SHA1_96,
            isAcceptor: true);
        byte[] plaintext = Enumerable.Range(0, 37).Select(i => (byte)i).ToArray();

        byte[] token = sender.WrapMessage(plaintext, confidential: true);
        byte[] unwrapped = receiver.UnwrapMessage(token, out bool wasConfidential);

        await Assert.That(wasConfidential).IsTrue();
        await Assert.That(unwrapped.SequenceEqual(plaintext)).IsTrue();
    }

    [Test]
    public async Task AES256_CTS_HMAC_SHA1_96_wrap_privacy_round_trips()
    {
        byte[] key = KerberosTestHex.FromHex(
            "00112233445566778899AABBCCDDEEFF" +
            "102132435465768798A9BACBDCEDFE0F");
        var sender = new KerberosSession(key, EncryptionType.AES256_CTS_HMAC_SHA1_96);
        var receiver = new KerberosSession(
            key,
            EncryptionType.AES256_CTS_HMAC_SHA1_96,
            isAcceptor: true);
        byte[] plaintext = Enumerable.Range(0, 53).Select(i => (byte)(255 - i)).ToArray();

        byte[] token = sender.WrapMessage(plaintext, confidential: true);
        byte[] unwrapped = receiver.UnwrapMessage(token, out bool wasConfidential);

        await Assert.That(wasConfidential).IsTrue();
        await Assert.That(unwrapped.SequenceEqual(plaintext)).IsTrue();
    }

    private static byte[] DeriveAesSha1Key(EncryptionType etype, int iterationCount)
    {
        var iterations = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(iterations, iterationCount);
        var key = new KerberosKey(
            password: "password",
            salt: "ATHENA.MIT.EDUraeburn",
            etype: etype,
            iterationParams: iterations);

        return key.GetKey().ToArray();
    }
}
