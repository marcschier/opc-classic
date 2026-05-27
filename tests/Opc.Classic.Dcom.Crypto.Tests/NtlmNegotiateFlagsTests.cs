// SPDX-License-Identifier: MIT

using System;
using System.Buffers.Binary;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Opc.Classic.Dcom.Crypto;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using TUnit.Core;

namespace Opc.Classic.Dcom.Crypto.Tests;

public sealed class NtlmNegotiateFlagsTests
{
    private const string User = "User";
    private const string Domain = "Domain";
    private const string Password = "Password";

    private static readonly byte[] ExportedSessionKey = Convert.FromHexString("55555555555555555555555555555555");
    private static readonly byte[] Payload = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");

    /// <summary>
    /// MS-NLMP §3.4.5.2 SIGNKEY and §3.4.5.3 SEALKEY: extended session security derives
    /// directional signing keys from the exported session key and directional sealing keys from the negotiated strength.
    /// </summary>
    [Test]
    [Arguments(true, false, false, 128, "4788dc861b4782f35d43fd98fe1a2d39", "d04d6f10741041d1d246d64188d7a8ad", "59f600973cc4960a25480a7c196e4c58", "9355f3a957c1583d25c4c2f11e40390e")]
    [Arguments(true, false, true, 128, "4788dc861b4782f35d43fd98fe1a2d39", "d04d6f10741041d1d246d64188d7a8ad", "59f600973cc4960a25480a7c196e4c58", "9355f3a957c1583d25c4c2f11e40390e")]
    [Arguments(true, true, true, 128, "4788dc861b4782f35d43fd98fe1a2d39", "d04d6f10741041d1d246d64188d7a8ad", "59f600973cc4960a25480a7c196e4c58", "9355f3a957c1583d25c4c2f11e40390e")]
    [Arguments(true, true, true, 56, "4788dc861b4782f35d43fd98fe1a2d39", "d04d6f10741041d1d246d64188d7a8ad", "a5f7253c1065e8d3d68642040e71cfe0", "583e2f98959b385cd158f3734b5f5d3f")]
    [Arguments(true, true, true, 40, "4788dc861b4782f35d43fd98fe1a2d39", "d04d6f10741041d1d246d64188d7a8ad", "42f964a471091a02ff4a77455366e4e5", "c5d3853b406b7c1241c595f0ce0750e2")]
    [Arguments(true, true, false, 56, "4788dc861b4782f35d43fd98fe1a2d39", "d04d6f10741041d1d246d64188d7a8ad", "a5f7253c1065e8d3d68642040e71cfe0", "583e2f98959b385cd158f3734b5f5d3f")]
    public async Task MsNlmp3452And3453_ExtendedSessionSecurity_DerivesExpectedDirectionalKeys(
        bool sign,
        bool seal,
        bool keyExchange,
        int keyBits,
        string expectedClientSigningKey,
        string expectedServerSigningKey,
        string expectedClientSealingKey,
        string expectedServerSealingKey)
    {
        var flags = BuildFlags(sign, seal, keyExchange, extendedSessionSecurity: true, keyBits);
        var security = CreateNtlm1(flags, ExportedSessionKey, isServer: false);

        await Assert.That(ToHex(GetSecurityField(security, "_clientSigningKey"))).IsEqualTo(expectedClientSigningKey);
        await Assert.That(ToHex(GetSecurityField(security, "_serverSigningKey"))).IsEqualTo(expectedServerSigningKey);
        await Assert.That(ToHex(GetCipherState(security, "_clientCipher"))).IsEqualTo(ToHex(CreateRc4State(expectedClientSealingKey)));
        await Assert.That(ToHex(GetCipherState(security, "_serverCipher"))).IsEqualTo(ToHex(CreateRc4State(expectedServerSealingKey)));
        await Assert.That(security.Protection).IsEqualTo(seal ? ProtectionLevel.PROTECTION_LEVEL_PRIVACY : ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);
    }

    /// <summary>
    /// MS-NLMP §3.4.5.2 SIGNKEY and §3.4.5.3 SEALKEY: without extended session security,
    /// signing keys are NIL and, absent LMKEY/datagram negotiation, sealing keys are the exported session key.
    /// </summary>
    [Test]
    [Arguments(true, false, false, 128)]
    [Arguments(true, true, false, 56)]
    public async Task MsNlmp3452And3453_WithoutExtendedSessionSecurity_UsesNilSignKeysAndUnmodifiedSealKeys(
        bool sign,
        bool seal,
        bool keyExchange,
        int keyBits)
    {
        var flags = BuildFlags(sign, seal, keyExchange, extendedSessionSecurity: false, keyBits);
        var security = CreateNtlm1(flags, ExportedSessionKey, isServer: false);

        await Assert.That(GetSecurityField(security, "_clientSigningKey").Length).IsEqualTo(0);
        await Assert.That(GetSecurityField(security, "_serverSigningKey").Length).IsEqualTo(0);
        await Assert.That(ToHex(GetCipherState(security, "_clientCipher"))).IsEqualTo(ToHex(CreateRc4State(ToHex(ExportedSessionKey))));
        await Assert.That(ToHex(GetCipherState(security, "_serverCipher"))).IsEqualTo(ToHex(CreateRc4State(ToHex(ExportedSessionKey))));
        await Assert.That(security.Protection).IsEqualTo(seal ? ProtectionLevel.PROTECTION_LEVEL_PRIVACY : ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);
    }

    /// <summary>
    /// MS-NLMP §3.4.5.2 SIGNKEY and §3.4.5.3 SEALKEY with §3.4.4.2 MAC: SIGN preserves the payload,
    /// while SEAL uses the negotiated sealing key to encrypt it and both directions verify the signature.
    /// </summary>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task MsNlmp3452And3453_SignOnlyAndSealModes_ProtectPayloadDifferently(bool keyExchange)
    {
        var signOnlyFlags = BuildFlags(sign: true, seal: false, keyExchange, extendedSessionSecurity: true, keyBits: 128);
        var sealFlags = BuildFlags(sign: true, seal: true, keyExchange, extendedSessionSecurity: true, keyBits: 128);

        var signOnly = Protect(signOnlyFlags);
        var sealedMessage = Protect(sealFlags);

        await Assert.That(signOnly.ProtectedPayload.SequenceEqual(Payload)).IsTrue();
        await Assert.That(sealedMessage.ProtectedPayload.SequenceEqual(Payload)).IsFalse();
        await Assert.That(Unprotect(signOnlyFlags, signOnly.Buffer).SequenceEqual(Payload)).IsTrue();
        await Assert.That(Unprotect(sealFlags, sealedMessage.Buffer).SequenceEqual(Payload)).IsTrue();
    }

    /// <summary>
    /// MS-NLMP §3.4.5.1 KXKEY and §3.4.5.2 SIGNKEY: the KEY_EXCH flag selects a secondary exported
    /// session key during negotiation and controls RC4 wrapping of the extended-session-security checksum.
    /// </summary>
    [Test]
    public async Task MsNlmp3451And3452_KeyExchangeFlag_SelectsSecondaryKeyAndChecksumWrapping()
    {
        var withoutKeyExchange = BuildFlags(sign: true, seal: false, keyExchange: false, extendedSessionSecurity: true, keyBits: 128);
        var withKeyExchange = BuildFlags(sign: true, seal: false, keyExchange: true, extendedSessionSecurity: true, keyBits: 128);
        var unwrappedSignature = CreateUnwrappedEssSignature(GetSecurityField(CreateNtlm1(withoutKeyExchange, ExportedSessionKey, isServer: false), "_clientSigningKey"));

        var withoutKeyExchangeMessage = Protect(withoutKeyExchange);
        var withKeyExchangeMessage = Protect(withKeyExchange);

        await Assert.That(ToHex(withoutKeyExchangeMessage.Verifier)).IsEqualTo(ToHex(unwrappedSignature));
        await Assert.That(ToHex(withKeyExchangeMessage.Verifier)).IsNotEqualTo(ToHex(unwrappedSignature));
    }

    /// <summary>
    /// MS-NLMP §3.4.5.1 KXKEY: negotiated KEY_EXCH encrypts a random secondary key into the AUTHENTICATE
    /// message; without KEY_EXCH the NTLM2-session KXKEY is used directly by the Ntlm1 security context.
    /// </summary>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task MsNlmp3451_Ntlm1Handshake_UsesSecondarySessionKeyOnlyWhenKeyExchangeNegotiated(bool keyExchange)
    {
        var client = CreateAuthentication(keyExchange);
        var server = CreateAuthentication(keyExchange);

        var type1 = client.CreateType1();
        var type2 = server.CreateType2(type1);
        var type3 = client.CreateType3(type2);
        InvokeCreateSecurityWhenServer(server, type3);

        await Assert.That(type3.GetFlag(NtlmFlags.NtlmsspNegotiateKeyExch)).IsEqualTo(keyExchange);
        await Assert.That(type3.GetSessionKey().Length).IsEqualTo(keyExchange ? 16 : 0);
        await Assert.That(client.Security.GetType().Name).IsEqualTo("Ntlm1");
        await Assert.That(server.Security.GetType().Name).IsEqualTo("Ntlm1");
        await Assert.That(ToHex(GetSecurityField(client.Security, "_clientSigningKey")))
            .IsEqualTo(ToHex(GetSecurityField(server.Security, "_clientSigningKey")));
        await Assert.That(ToHex(GetSecurityField(client.Security, "_serverSigningKey")))
            .IsEqualTo(ToHex(GetSecurityField(server.Security, "_serverSigningKey")));
    }

    private static NtlmFlags BuildFlags(bool sign, bool seal, bool keyExchange, bool extendedSessionSecurity, int keyBits)
    {
        var flags = NtlmFlags.NtlmsspNegotiateUnicode | NtlmFlags.NtlmsspNegotiateNtlm;
        if (sign || seal)
        {
            flags |= NtlmFlags.NtlmsspNegotiateSign;
        }

        if (seal)
        {
            flags |= NtlmFlags.NtlmsspNegotiateSeal;
        }

        if (keyExchange)
        {
            flags |= NtlmFlags.NtlmsspNegotiateKeyExch;
        }

        if (extendedSessionSecurity)
        {
            flags |= NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity;
        }

        if (keyBits >= 56)
        {
            flags |= NtlmFlags.NtlmsspNegotiate56;
        }

        if (keyBits >= 128)
        {
            flags |= NtlmFlags.NtlmsspNegotiate128;
        }

        return flags;
    }

    private static ISecurity CreateNtlm1(NtlmFlags flags, byte[] exportedSessionKey, bool isServer)
    {
#pragma warning disable CS0618 // NTLMv1 fallback is intentionally covered by these compatibility tests.
        return new Ntlm1(flags, exportedSessionKey, isServer);
#pragma warning restore CS0618
    }

    private static (byte[] ProtectedPayload, byte[] Verifier, byte[] Buffer) Protect(NtlmFlags flags)
    {
        var security = CreateNtlm1(flags, ExportedSessionKey, isServer: false);
        var buffer = new byte[Payload.Length + security.VerifierLength];
        Payload.CopyTo(buffer.AsSpan());
        var ndr = new NdrCodec { Buffer = new NdrBuffer(buffer, 0) };

        security.ProcessOutgoing(ndr, 0, Payload.Length, Payload.Length, isFragmented: false);

        return (buffer.AsSpan(0, Payload.Length).ToArray(), buffer.AsSpan(Payload.Length, security.VerifierLength).ToArray(), buffer);
    }

    private static byte[] Unprotect(NtlmFlags flags, byte[] protectedBuffer)
    {
        var security = CreateNtlm1(flags, ExportedSessionKey, isServer: true);
        var buffer = (byte[])protectedBuffer.Clone();
        var ndr = new NdrCodec { Buffer = new NdrBuffer(buffer, 0) };

        security.ProcessIncoming(ndr, 0, Payload.Length, Payload.Length, isFragmented: false);

        return buffer.AsSpan(0, Payload.Length).ToArray();
    }

    private static byte[] CreateUnwrappedEssSignature(byte[] signingKey)
    {
        var seqNumPlusData = new byte[sizeof(int) + Payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(seqNumPlusData.AsSpan(0, sizeof(int)), 0);
        Payload.CopyTo(seqNumPlusData.AsSpan(sizeof(int)));

        using var hmac = new HMACMD5(signingKey);
        var checksum = hmac.ComputeHash(seqNumPlusData);
        var signature = new byte[16];
        signature[0] = 0x01;
        Array.Copy(checksum, 0, signature, 4, 8);
        return signature;
    }

    private static NtlmAuthentication CreateAuthentication(bool keyExchange)
    {
        var properties = new PropertyBag();
        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", "true");
        properties.SetProperty("rpc.ntlm.seal", "false");
        properties.SetProperty("rpc.ntlm.keyExchange", keyExchange.ToString());
        properties.SetProperty("rpc.ntlm.keyLength", "128");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");
        properties.SetProperty("rpc.ntlm.ntlmv2", "false");
        properties.SetProperty("rpc.ntlm.allowV1", "true");
        properties.SetProperty("rpc.ntlm.sso", "false");
        properties.SetProperty("rpc.ntlm.domain", Domain);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, User);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, Password);
        return new NtlmAuthentication(properties);
    }

    private static void InvokeCreateSecurityWhenServer(NtlmAuthentication authentication, Type3Message type3)
    {
        var method = typeof(NtlmAuthentication).GetMethod(
            "CreateSecurityWhenServer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            method.Invoke(authentication, new object[] { type3 });
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static byte[] GetSecurityField(ISecurity security, string fieldName)
    {
        var field = security.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (byte[])field.GetValue(security)!;
    }

    private static byte[] GetCipherState(ISecurity security, string fieldName)
    {
        var field = security.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        var cipher = field.GetValue(security)!;
        var rc4Field = cipher.GetType().GetField("_cipher", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var rc4 = rc4Field.GetValue(cipher)!;
        return GetRc4State(rc4);
    }

    private static byte[] CreateRc4State(string keyHex) => GetRc4State(new Rc4(Convert.FromHexString(keyHex)));

    private static byte[] GetRc4State(object rc4)
    {
        var stateField = rc4.GetType().GetField("_s", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return ((byte[])stateField.GetValue(rc4)!).ToArray();
    }

    private static string ToHex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
}
