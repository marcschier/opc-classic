// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Security;
using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosRpcPacketProtectionTests
{
    [Test]
    public async Task Rpc_GetMICEx_round_trips_all_supported_encryption_types()
    {
        foreach ((EncryptionType etype, byte[] key) in Cases())
        {
            var sender = new KerberosSession(
                key,
                etype,
                initialSendSequenceNumber: 41,
                initialReceiveSequenceNumber: 900,
                isAcceptor: false,
                usesAcceptorSubkey: false);
            var receiver = new KerberosSession(
                key,
                etype,
                initialSendSequenceNumber: 900,
                initialReceiveSequenceNumber: 41,
                isAcceptor: true,
                usesAcceptorSubkey: false);
            byte[] message = CreateRpcMessage();
            byte[] expected = message.ToArray();

            byte[] verifier = sender.ProtectRpcMessage(
                message,
                confidentialOffset: 16,
                confidentialLength: message.Length - 24,
                confidential: false);
            receiver.UnprotectRpcMessage(
                message,
                confidentialOffset: 16,
                confidentialLength: message.Length - 24,
                verifier,
                confidential: false);

            await Assert.That(message).IsEquivalentTo(expected);
            await Assert.That(verifier.Length)
                .IsEqualTo(sender.GetRpcVerifierLength(confidential: false));
        }
    }

    [Test]
    public async Task Rpc_WrapEx_encrypts_only_body_and_round_trips_all_supported_encryption_types()
    {
        foreach ((EncryptionType etype, byte[] key) in Cases())
        {
            var sender = new KerberosSession(
                key,
                etype,
                initialSendSequenceNumber: 7,
                initialReceiveSequenceNumber: 31,
                isAcceptor: false,
                usesAcceptorSubkey: false);
            var receiver = new KerberosSession(
                key,
                etype,
                initialSendSequenceNumber: 31,
                initialReceiveSequenceNumber: 7,
                isAcceptor: true,
                usesAcceptorSubkey: false);
            byte[] message = CreateRpcMessage();
            byte[] expected = message.ToArray();
            const int confidentialOffset = 16;
            int confidentialLength = message.Length - 24;

            byte[] verifier = sender.ProtectRpcMessage(
                message,
                confidentialOffset,
                confidentialLength,
                confidential: true);

            await Assert.That(message.AsSpan(0, confidentialOffset).ToArray())
                .IsEquivalentTo(expected.AsSpan(0, confidentialOffset).ToArray());
            await Assert.That(
                message.AsSpan(confidentialOffset, confidentialLength)
                    .SequenceEqual(expected.AsSpan(
                        confidentialOffset,
                        confidentialLength)))
                .IsFalse();
            await Assert.That(message.AsSpan(confidentialOffset + confidentialLength).ToArray())
                .IsEquivalentTo(expected.AsSpan(confidentialOffset + confidentialLength).ToArray());
            await Assert.That(verifier.Length)
                .IsEqualTo(sender.GetRpcVerifierLength(confidential: true));

            receiver.UnprotectRpcMessage(
                message,
                confidentialOffset,
                confidentialLength,
                verifier,
                confidential: true);
            await Assert.That(message).IsEquivalentTo(expected);
        }
    }

    [Test]
    public async Task Aes_Rpc_verifiers_use_MS_KILE_DCE_style_EC_RRC_framing()
    {
        var aesSha1 = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var aesSha256 = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA256_128);
        var aesSha384 = new KerberosSession(
            Key256,
            EncryptionType.AES256_CTS_HMAC_SHA384_192);
        var rc4 = new KerberosSession(
            Key128,
            EncryptionType.RC4_HMAC_NT);
        byte[] message = CreateRpcMessage();

        byte[] integrity = aesSha1.ProtectRpcMessage(
            message.ToArray(),
            16,
            message.Length - 24,
            confidential: false);
        byte[] privacySha1 = aesSha1.ProtectRpcMessage(
            message.ToArray(),
            16,
            message.Length - 24,
            confidential: true);
        byte[] privacySha256 = aesSha256.ProtectRpcMessage(
            message.ToArray(),
            16,
            message.Length - 24,
            confidential: true);
        byte[] privacySha384 = aesSha384.ProtectRpcMessage(
            message.ToArray(),
            16,
            message.Length - 24,
            confidential: true);
        byte[] rc4Integrity = rc4.ProtectRpcMessage(
            message.ToArray(),
            16,
            message.Length - 24,
            confidential: false);
        byte[] rc4Privacy = rc4.ProtectRpcMessage(
            message.ToArray(),
            16,
            message.Length - 24,
            confidential: true);

        await Assert.That(integrity.Length).IsEqualTo(28);
        await Assert.That(BinaryPrimitives.ReadUInt16BigEndian(integrity))
            .IsEqualTo((ushort)0x0404);
        await Assert.That(privacySha1.Length).IsEqualTo(76);
        await Assert.That(ReadEc(privacySha1)).IsEqualTo((ushort)16);
        await Assert.That(ReadRrc(privacySha1)).IsEqualTo((ushort)28);
        await Assert.That(privacySha256.Length).IsEqualTo(80);
        await Assert.That(ReadRrc(privacySha256)).IsEqualTo((ushort)32);
        await Assert.That(privacySha384.Length).IsEqualTo(88);
        await Assert.That(ReadRrc(privacySha384)).IsEqualTo((ushort)40);
        await Assert.That(rc4Integrity.Length).IsEqualTo(37);
        await Assert.That(rc4Integrity[0]).IsEqualTo((byte)0x60);
        await Assert.That(rc4Integrity[13]).IsEqualTo((byte)0x01);
        await Assert.That(rc4Privacy.Length).IsEqualTo(45);
        await Assert.That(rc4Privacy[1]).IsEqualTo((byte)0x2B);
        await Assert.That(rc4Privacy[13]).IsEqualTo((byte)0x02);
    }

    [Test]
    public async Task Rpc_protection_rejects_tampered_sign_only_segments()
    {
        var sender = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var receiver = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96,
            isAcceptor: true);
        byte[] message = CreateRpcMessage();
        byte[] verifier = sender.ProtectRpcMessage(
            message,
            16,
            message.Length - 24,
            confidential: true);
        byte[] tampered = message.ToArray();
        tampered[0] ^= 0x01;

        Exception? rejected = CaptureException(() =>
            receiver.UnprotectRpcMessage(
                tampered,
                16,
                tampered.Length - 24,
                verifier,
                confidential: true));

        await Assert.That(rejected).IsTypeOf<SecurityException>();
    }

    [Test]
    public async Task Directional_sequence_state_uses_AP_REQ_and_AP_REP_numbers()
    {
        var initiator = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96,
            initialSendSequenceNumber: 0x10203040,
            initialReceiveSequenceNumber: 0x50607080,
            isAcceptor: false,
            usesAcceptorSubkey: true);
        var acceptor = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96,
            initialSendSequenceNumber: 0x50607080,
            initialReceiveSequenceNumber: 0x10203040,
            isAcceptor: true,
            usesAcceptorSubkey: true);
        byte[] data = [0x01, 0x02, 0x03];

        byte[] initiatorMic = initiator.GetMic(data);
        byte[] acceptorMic = acceptor.GetMic(data);

        await Assert.That(BinaryPrimitives.ReadInt64BigEndian(initiatorMic.AsSpan(8)))
            .IsEqualTo(0x10203040);
        await Assert.That(BinaryPrimitives.ReadInt64BigEndian(acceptorMic.AsSpan(8)))
            .IsEqualTo(0x50607080);
        await Assert.That(acceptor.VerifyMic(data, initiatorMic)).IsTrue();
        await Assert.That(initiator.VerifyMic(data, acceptorMic)).IsTrue();
    }

    [Test]
    public async Task MIC_rejects_reflection_and_acceptor_subkey_flag_mismatch()
    {
        var initiator = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var wrongSubkeyAcceptor = new KerberosSession(
            Key128,
            EncryptionType.AES128_CTS_HMAC_SHA1_96,
            isAcceptor: true,
            usesAcceptorSubkey: true);
        byte[] data = [0x11, 0x22];
        byte[] mic = initiator.GetMic(data);

        await Assert.That(initiator.VerifyMic(data, mic)).IsFalse();
        await Assert.That(wrongSubkeyAcceptor.VerifyMic(data, mic)).IsFalse();
    }

    [Test]
    public async Task Rc4_Rpc_Dce_style_preserves_pre_aligned_body_length()
    {
        var sender = new KerberosSession(
            Key128,
            EncryptionType.RC4_HMAC_NT);
        var receiver = new KerberosSession(
            Key128,
            EncryptionType.RC4_HMAC_NT,
            isAcceptor: true);
        byte[] message =
            Enumerable.Range(0, 56).Select(static value => (byte)value).ToArray();
        byte[] expected = message.ToArray();

        byte[] verifier = sender.ProtectRpcMessage(
            message,
            confidentialOffset: 16,
            confidentialLength: 32,
            confidential: true);

        await Assert.That(message.Length).IsEqualTo(expected.Length);
        await Assert.That(verifier.Length).IsEqualTo(45);
        await Assert.That(verifier[1]).IsEqualTo((byte)0x2B);

        receiver.UnprotectRpcMessage(
            message,
            confidentialOffset: 16,
            confidentialLength: 32,
            verifier,
            confidential: true);
        await Assert.That(message).IsEquivalentTo(expected);
    }

    private static readonly byte[] Key128 =
        KerberosTestHex.FromHex("00112233445566778899AABBCCDDEEFF");
    private static readonly byte[] Key256 = KerberosTestHex.FromHex(
        "00112233445566778899AABBCCDDEEFF" +
        "102132435465768798A9BACBDCEDFE0F");

    private static IEnumerable<(EncryptionType EncryptionType, byte[] Key)> Cases()
    {
        yield return (EncryptionType.AES128_CTS_HMAC_SHA1_96, Key128);
        yield return (EncryptionType.AES256_CTS_HMAC_SHA1_96, Key256);
        yield return (EncryptionType.AES128_CTS_HMAC_SHA256_128, Key128);
        yield return (EncryptionType.AES256_CTS_HMAC_SHA384_192, Key256);
        yield return (EncryptionType.RC4_HMAC_NT, Key128);
    }

    private static byte[] CreateRpcMessage() =>
        Enumerable.Range(0, 43).Select(static value => (byte)value).ToArray();

    private static ushort ReadEc(ReadOnlySpan<byte> token) =>
        BinaryPrimitives.ReadUInt16BigEndian(token[4..]);

    private static ushort ReadRrc(ReadOnlySpan<byte> token) =>
        BinaryPrimitives.ReadUInt16BigEndian(token[6..]);

    private static Exception? CaptureException(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
}
