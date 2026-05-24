//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// MS-NLMP §4.2.4.1 NTLMv2 sample vectors and server-side key derivation tests.
//

using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Crypto;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Crypto.Tests;

public sealed class NtlmV2ServerKeyDerivationTests
{
    private const string User = "User";
    private const string Domain = "Domain";
    private const string Password = "Password";
    private const string Workstation = "COMPUTER";

    private static readonly byte[] ServerChallenge = Convert.FromHexString("0123456789abcdef");
    private static readonly byte[] RandomSessionKey = Convert.FromHexString("55555555555555555555555555555555");
    private static readonly byte[] EncryptedRandomSessionKey = Convert.FromHexString("c5dad2544fc9799094ce1ce90bc9d03e");
    private static readonly byte[] Temp = Convert.FromHexString(
        "01010000000000000000000000000000" +
        "aaaaaaaaaaaaaaaa" +
        "00000000" +
        "02000c0044006f006d00610069006e00" +
        "01000c00530065007200760065007200" +
        "0000000000000000");
    private static readonly byte[] NtChallengeResponse = Convert.FromHexString(
        "68cd0ab851e51c96aabc927bebef6a1c" +
        "01010000000000000000000000000000" +
        "aaaaaaaaaaaaaaaa" +
        "00000000" +
        "02000c0044006f006d00610069006e00" +
        "01000c00530065007200760065007200" +
        "0000000000000000");
    private static readonly byte[] LmChallengeResponse = Convert.FromHexString(
        "86c35097ac9cec102554764a57cccc19aaaaaaaaaaaaaaaa");

    [Test]
    public async Task MsNlmp4241_Vectors_DeriveExpectedKeys()
    {
        var ntowfv2 = ComputeNTOWFv2(Password, User, Domain);
        await Assert.That(ToHex(ntowfv2)).IsEqualTo("0c868a403bfd7a93a3001ef22ef02e3f");

        var ntProofStr = ComputeNtProofStr(ntowfv2, ServerChallenge, Temp);
        await Assert.That(ToHex(ntProofStr)).IsEqualTo("68cd0ab851e51c96aabc927bebef6a1c");

        var sessionBaseKey = HmacMd5(ntowfv2, ntProofStr);
        await Assert.That(ToHex(sessionBaseKey)).IsEqualTo("8de40ccadbc14a82f15cb0ad0de95ca3");

        var decryptedRandomSessionKey = new Rc4(sessionBaseKey).Process(EncryptedRandomSessionKey);
        await Assert.That(ToHex(decryptedRandomSessionKey)).IsEqualTo(ToHex(RandomSessionKey));
    }

    [Test]
    public async Task CreateSecurityWhenServer_VerifiesValidNtProof_AndDecryptsRandomSessionKey()
    {
        var authentication = CreateAuthentication(Password);
        SetSavedServerChallenge(authentication, ServerChallenge);
        var type3 = CreateMsNlmpType3();

        InvokeCreateSecurityWhenServer(authentication, type3);

        await Assert.That(authentication.Security).IsNotNull();
        await Assert.That(ToHex(GetSecurityField(authentication, "_clientSigningKey")))
            .IsEqualTo("4788dc861b4782f35d43fd98fe1a2d39");
    }

    [Test]
    public async Task CreateSecurityWhenServer_ThrowsSecurityException_WhenNtProofDoesNotMatchPassword()
    {
        var authentication = CreateAuthentication("WrongPassword");
        SetSavedServerChallenge(authentication, ServerChallenge);
        var type3 = CreateMsNlmpType3();

        await Assert.That(() => InvokeCreateSecurityWhenServer(authentication, type3))
            .Throws<SecurityException>();
    }

    [Test]
    public async Task ClientServerRoundTrip_DerivesMatchingSigningKeys()
    {
        var client = CreateAuthentication(Password);
        var server = CreateAuthentication(Password);

        var type1 = client.CreateType1();
        var type2 = server.CreateType2(type1);
        var type3 = client.CreateType3(type2);

        InvokeCreateSecurityWhenServer(server, type3);

        await Assert.That(type3.GetFlag(NtlmFlags.NtlmsspNegotiateKeyExch)).IsTrue();
        await Assert.That(ToHex(GetSecurityField(server, "_clientSigningKey")))
            .IsEqualTo(ToHex(GetSecurityField(client, "_clientSigningKey")));
        await Assert.That(ToHex(GetSecurityField(server, "_serverSigningKey")))
            .IsEqualTo(ToHex(GetSecurityField(client, "_serverSigningKey")));
    }

    private static Type3Message CreateMsNlmpType3()
    {
        var flags = NtlmFlags.NtlmsspNegotiateUnicode |
            NtlmFlags.NtlmsspNegotiateNtlm |
            NtlmFlags.NtlmsspNegotiateNtlm2 |
            NtlmFlags.NtlmsspNegotiateKeyExch |
            NtlmFlags.NtlmsspNegotiateSign |
            NtlmFlags.NtlmsspNegotiateSeal |
            NtlmFlags.NtlmsspNegotiateTargetInfo |
            NtlmFlags.NtlmsspNegotiate128 |
            NtlmFlags.NtlmsspNegotiate56;
        var type3 = new Type3Message(flags, LmChallengeResponse, NtChallengeResponse, Domain, User, Workstation);
        type3.SetSessionKey(EncryptedRandomSessionKey);
        return type3;
    }

    private static NtlmAuthentication CreateAuthentication(string password)
    {
        var properties = new PropertyBag();
        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", "true");
        properties.SetProperty("rpc.ntlm.seal", "true");
        properties.SetProperty("rpc.ntlm.keyExchange", "true");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");
        properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        properties.SetProperty("rpc.ntlm.domain", Domain);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, User);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, password);
        return new NtlmAuthentication(properties);
    }

    private static void SetSavedServerChallenge(NtlmAuthentication authentication, byte[] challenge)
    {
        var field = typeof(NtlmAuthentication).GetField("_serverChallenge", BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(authentication, (byte[])challenge.Clone());
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

    private static byte[] GetSecurityField(NtlmAuthentication authentication, string fieldName)
    {
        var field = authentication.Security.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (byte[])field.GetValue(authentication.Security)!;
    }

    private static byte[] ComputeNTOWFv2(string password, string user, string domain)
    {
        var ntHash = Md4.HashData(Encoding.Unicode.GetBytes(password));
        return HmacMd5(ntHash, Encoding.Unicode.GetBytes(user.ToUpperInvariant() + domain));
    }

    private static byte[] ComputeNtProofStr(byte[] ntowfv2, byte[] serverChallenge, byte[] temp)
    {
        var input = new byte[serverChallenge.Length + temp.Length];
        Array.Copy(serverChallenge, 0, input, 0, serverChallenge.Length);
        Array.Copy(temp, 0, input, serverChallenge.Length, temp.Length);
        return HmacMd5(ntowfv2, input);
    }

    private static byte[] HmacMd5(byte[] key, byte[] input)
    {
        using var hmac = new HMACMD5(key);
        return hmac.ComputeHash(input);
    }

    private static string ToHex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
}
