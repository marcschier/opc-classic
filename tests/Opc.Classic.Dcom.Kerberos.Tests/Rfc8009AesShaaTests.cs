//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Linq;
using Kerberos.NET.Crypto;
using TUnit.Core;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class Rfc8009AesShaaTests
{
    private static readonly byte[] Rfc8009Salt = KerberosTestHex.FromHex(
        "10DF9DD783E5BC8ACEA1730E74355F61" +
        "415448454E412E4D49542E4544557261656275726E");

    [Test]
    public async Task Rfc8009_AES128_SHA256_string_to_key_matches_vector()
    {
        byte[] key = new KerberosKey(
            password: "password",
            saltBytes: Rfc8009Salt,
            etype: EncryptionType.AES128_CTS_HMAC_SHA256_128).GetKey().ToArray();

        await Assert.That(key.SequenceEqual(KerberosTestHex.FromHex("089BCA48B105EA6EA77CA5D2F39DC5E7"))).IsTrue();
    }

    [Test]
    public async Task Rfc8009_AES256_SHA384_string_to_key_matches_vector()
    {
        byte[] key = new KerberosKey(
            password: "password",
            saltBytes: Rfc8009Salt,
            etype: EncryptionType.AES256_CTS_HMAC_SHA384_192).GetKey().ToArray();

        await Assert.That(key.SequenceEqual(KerberosTestHex.FromHex(
            "45BD806DBF6A833A9CFFC1C94589A222" +
            "367A79BC21C413718906E9F578A78467"))).IsTrue();
    }

    [Test]
    public async Task Rfc8009_AES128_SHA256_checksum_matches_vector()
    {
        byte[] baseKey = KerberosTestHex.FromHex("3705D96080C17728A0E800EAB6E0D23C");
        byte[] plaintext = KerberosTestHex.FromHex("000102030405060708090A0B0C0D0E0F1011121314");
        var transform = CryptoService.CreateTransform(EncryptionType.AES128_CTS_HMAC_SHA256_128);

        byte[] checksum = transform.MakeChecksum(
            plaintext,
            new KerberosKey(key: baseKey, etype: EncryptionType.AES128_CTS_HMAC_SHA256_128),
            KeyUsage.Ticket,
            KeyDerivationMode.Kc,
            transform.ChecksumSize).ToArray();

        await Assert.That(checksum.SequenceEqual(KerberosTestHex.FromHex("D78367186643D67B411CBA9139FC1DEE"))).IsTrue();
    }

    [Test]
    public async Task Rfc8009_AES256_SHA384_checksum_matches_vector()
    {
        byte[] baseKey = KerberosTestHex.FromHex(
            "6D404D37FAF79F9DF0D33568D3206698" +
            "00EB4836472EA8A026D16B7182460C52");
        byte[] plaintext = KerberosTestHex.FromHex("000102030405060708090A0B0C0D0E0F1011121314");
        var transform = CryptoService.CreateTransform(EncryptionType.AES256_CTS_HMAC_SHA384_192);

        byte[] checksum = transform.MakeChecksum(
            plaintext,
            new KerberosKey(key: baseKey, etype: EncryptionType.AES256_CTS_HMAC_SHA384_192),
            KeyUsage.Ticket,
            KeyDerivationMode.Kc,
            transform.ChecksumSize).ToArray();

        await Assert.That(checksum.SequenceEqual(KerberosTestHex.FromHex(
            "45EE791567EEFCA37F4AC1E0222DE80D" +
            "43C3BFA06699672A"))).IsTrue();
    }

    [Test]
    public async Task AES_SHA2_session_supports_wrap_round_trip_for_both_etypes()
    {
        byte[] plaintext = [0xCA, 0xFE, 0xBA, 0xBE, 0x00, 0x01];
        var aes128 = new KerberosSession(KerberosTestHex.FromHex("3705D96080C17728A0E800EAB6E0D23C"), EncryptionType.AES128_CTS_HMAC_SHA256_128);
        var aes256 = new KerberosSession(
            KerberosTestHex.FromHex("6D404D37FAF79F9DF0D33568D320669800EB4836472EA8A026D16B7182460C52"),
            EncryptionType.AES256_CTS_HMAC_SHA384_192);

        byte[] aes128RoundTrip = aes128.UnwrapMessage(aes128.WrapMessage(plaintext, confidential: true), out bool aes128Confidential);
        byte[] aes256RoundTrip = aes256.UnwrapMessage(aes256.WrapMessage(plaintext, confidential: true), out bool aes256Confidential);

        await Assert.That(aes128Confidential).IsTrue();
        await Assert.That(aes256Confidential).IsTrue();
        await Assert.That(aes128RoundTrip.SequenceEqual(plaintext)).IsTrue();
        await Assert.That(aes256RoundTrip.SequenceEqual(plaintext)).IsTrue();
    }
}
