//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Linq;
using Kerberos.NET.Crypto;
using TUnit.Core;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class Rfc4757Rc4HmacTests
{
    private static readonly byte[] Rfc4757FooStringToKey = KerberosTestHex.FromHex("AC8E657F83DF82BEEA5D43BDAF7800CC");

    [Test]
    public async Task Rfc4757_section_2_String2Key_foo_vector_is_accepted_as_RC4_session_key()
    {
        var session = new KerberosSession(Rfc4757FooStringToKey, EncryptionType.RC4_HMAC_NT);

        byte[] token = session.WrapMessage([0x01, 0x02, 0x03], confidential: false);

        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(token)).IsEqualTo((ushort)0x0504);
        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(token.AsSpan(4))).IsEqualTo((ushort)16);
    }

    [Test]
    public async Task Rfc4757_HMAC_MD5_MIC_derivation_matches_expected_checksum()
    {
        var session = new KerberosSession(Rfc4757FooStringToKey, EncryptionType.RC4_HMAC_NT);

        byte[] mic = session.GetMic([0x01, 0x02, 0x03]);
        byte[] checksum = mic.AsSpan(16).ToArray();

        await Assert.That(checksum.SequenceEqual(KerberosTestHex.FromHex("26B37360A873E2CB358614EE513E82E1"))).IsTrue();
    }

    [Test]
    public async Task Rc4Hmac_wrap_privacy_round_trips()
    {
        var session = new KerberosSession(Rfc4757FooStringToKey, EncryptionType.RC4_HMAC_NT);
        byte[] plaintext = [0x41, 0x42, 0x43, 0x44];

        byte[] token = session.WrapMessage(plaintext, confidential: true);
        byte[] unwrapped = session.UnwrapMessage(token, out bool wasConfidential);

        await Assert.That(wasConfidential).IsTrue();
        await Assert.That(unwrapped.SequenceEqual(plaintext)).IsTrue();
    }
}
