//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// MS-NLMP §4.2.4.1 NTLMv2 sample handshake fixture replay tests.
//

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Opc.Classic.Dcom.Crypto;
using Opc.Classic.Dcom.Internal.Ntlm;
using TUnit.Core;

namespace Opc.Classic.Dcom.Crypto.Tests.Fixtures.Ntlm;

public sealed class NtlmHandshakeFixtureTests {
    private const string User = "User";
    private const string Domain = "Domain";
    private const string Password = "Password";
    private const string Workstation = "COMPUTER";
    private const ushort MsvAvNbComputerName = 0x0001;
    private const ushort MsvAvNbDomainName = 0x0002;

    private static readonly byte[] ServerChallenge = Convert.FromHexString("0123456789abcdef");
    private static readonly byte[] RandomSessionKey = Convert.FromHexString("55555555555555555555555555555555");
    private static readonly byte[] EncryptedRandomSessionKey = Convert.FromHexString("c5dad2544fc9799094ce1ce90bc9d03e");
    private static readonly byte[] AuthenticateVersion = Convert.FromHexString("0501280a0000000f");
    private static readonly byte[] LmChallengeResponse = Convert.FromHexString(
        "86c35097ac9cec102554764a57cccc19aaaaaaaaaaaaaaaa");
    private static readonly byte[] NtChallengeResponse = Convert.FromHexString(
        "68cd0ab851e51c96aabc927bebef6a1c" +
        "01010000000000000000000000000000" +
        "aaaaaaaaaaaaaaaa" +
        "00000000" +
        "02000c0044006f006d00610069006e00" +
        "01000c00530065007200760065007200" +
        "0000000000000000");

    [Test]
    public async Task Negotiate_request_marshals_to_fixture_bytes() {
        byte[] expected = ReadFixture("negotiate.bin");
        byte[] actual = BuildNegotiateMessage();

        await Assert.That(actual.SequenceEqual(expected)).IsTrue();
    }

    [Test]
    public async Task Challenge_response_unmarshals_to_sample_ServerChallenge() {
        var challenge = new Type2Message(ReadFixture("challenge.bin"));
        byte[] targetInformation = challenge.GetTargetInformation();

        await Assert.That(challenge.GetChallenge().SequenceEqual(ServerChallenge)).IsTrue();
        await Assert.That(challenge.GetTarget()).IsEqualTo("Server");
        await Assert.That(ReadAvPair(targetInformation, MsvAvNbDomainName)).IsEqualTo(Domain);
        await Assert.That(ReadAvPair(targetInformation, MsvAvNbComputerName)).IsEqualTo("Server");
    }

    [Test]
    public async Task Authenticate_marshals_with_session_key_to_fixture_bytes() {
        byte[] expected = ReadFixture("authenticate.bin");
        byte[] actual = BuildAuthenticateMessage();

        await Assert.That(actual.SequenceEqual(expected)).IsTrue();
    }

    [Test]
    public async Task Authenticate_unmarshals_to_expected_NtChallengeResponse() {
        var authenticate = new Type3Message(ReadFixture("authenticate.bin"));

        await Assert.That(authenticate.GetNTResponse().SequenceEqual(NtChallengeResponse)).IsTrue();
        await Assert.That(authenticate.GetSessionKey().SequenceEqual(EncryptedRandomSessionKey)).IsTrue();
        await Assert.That(authenticate.GetLMResponse().SequenceEqual(LmChallengeResponse)).IsTrue();
        await Assert.That(authenticate.GetDomain()).IsEqualTo(Domain);
        await Assert.That(authenticate.GetUser()).IsEqualTo(User);
        await Assert.That(authenticate.GetWorkstation()).IsEqualTo(Workstation);
    }

    [Test]
    public async Task Negotiate_round_trip_preserves_flags() {
        byte[] encoded = BuildNegotiateMessage();
        var parsed = new Type1Message(encoded);
        byte[] roundTrip = parsed.ToByteArray();

        await Assert.That(parsed.GetFlag(NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity)).IsTrue();
        await Assert.That(parsed.GetFlag(NtlmFlags.NtlmsspNegotiateDatagram)).IsTrue();
        await Assert.That(parsed.GetFlag(NtlmFlags.NtlmsspNegotiateIdentify)).IsTrue();
        await Assert.That(roundTrip.SequenceEqual(encoded)).IsTrue();
    }

    private static byte[] BuildNegotiateMessage() =>
        new Type1Message(GetNegotiateFlags(), Domain, Workstation).ToByteArray();

    private static byte[] BuildAuthenticateMessage() {
        // MS-NLMP §4.2.4.3 publishes the same field values with payloads ordered
        // Domain/User/Workstation/LM/NT/SessionKey. Type3Message writes LM/NT first;
        // the security-buffer offsets keep both encodings semantically equivalent.
        var message = new Type3Message(
            GetAuthenticateFlags(),
            LmChallengeResponse,
            NtChallengeResponse,
            Domain,
            User,
            Workstation);
        SetVersion(message, AuthenticateVersion);
        message.SetSessionKey(ComputeEncryptedRandomSessionKey());
        return message.ToByteArray();
    }

    private static byte[] ComputeEncryptedRandomSessionKey() {
        byte[] ntowfv2 = ComputeNTOWFv2();
        byte[] ntProofStr = NtChallengeResponse.AsSpan(0, 16).ToArray();
        byte[] sessionBaseKey = HmacMd5(ntowfv2, ntProofStr);
        byte[] encrypted = new Rc4(sessionBaseKey).Process(RandomSessionKey);
        if (!encrypted.SequenceEqual(EncryptedRandomSessionKey)) {
            throw new InvalidDataException("MS-NLMP §4.2.4.2.3 encrypted session key vector changed.");
        }

        return encrypted;
    }

    private static byte[] ComputeNTOWFv2() {
        byte[] ntHash = Md4.HashData(Encoding.Unicode.GetBytes(Password));
        return HmacMd5(ntHash, Encoding.Unicode.GetBytes(User.ToUpperInvariant() + Domain));
    }

    private static byte[] HmacMd5(byte[] key, byte[] input) {
        using var hmac = new HMACMD5(key);
        return hmac.ComputeHash(input);
    }

    private static void SetVersion(Type3Message message, byte[] version) {
        FieldInfo field = typeof(Type3Message).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(message, (byte[])version.Clone());
    }

    private static NtlmFlags GetNegotiateFlags() =>
        NtlmFlags.NtlmsspNegotiateUnicode |
        NtlmFlags.NtlmsspNegotiateOem |
        NtlmFlags.NtlmsspNegotiateSign |
        NtlmFlags.NtlmsspNegotiateSeal |
        NtlmFlags.NtlmsspNegotiateDatagram |
        NtlmFlags.NtlmsspNegotiateLmKey |
        NtlmFlags.NtlmsspNegotiateNtlm |
        NtlmFlags.NtlmsspNegotiateAlwaysSign |
        NtlmFlags.NtlmsspTargetTypeDomain |
        NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity |
        NtlmFlags.NtlmsspNegotiateIdentify;

    private static NtlmFlags GetAuthenticateFlags() =>
        NtlmFlags.NtlmsspNegotiateUnicode |
        NtlmFlags.NtlmsspRequestTarget |
        NtlmFlags.NtlmsspNegotiateSign |
        NtlmFlags.NtlmsspNegotiateSeal |
        NtlmFlags.NtlmsspNegotiateNtlm |
        NtlmFlags.NtlmsspNegotiateAlwaysSign |
        NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity |
        NtlmFlags.NtlmsspNegotiateTargetInfo |
        NtlmFlags.NtlmsspNegotiateVersion |
        NtlmFlags.NtlmsspNegotiate128 |
        NtlmFlags.NtlmsspNegotiateKeyExch |
        NtlmFlags.NtlmsspNegotiate56;

    private static string ReadAvPair(byte[] targetInformation, ushort avId) {
        if (!NtlmAvPairs.TryGet(targetInformation, avId, out ReadOnlySpan<byte> value)) {
            throw new InvalidDataException($"NTLM target info did not contain AV pair 0x{avId:X4}.");
        }

        return Encoding.Unicode.GetString(value);
    }

    private static byte[] ReadFixture(string fileName) => File.ReadAllBytes(GetFixturePath(fileName));

    private static string GetFixturePath(string fileName) {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent) {
            string localPath = Path.Combine(directory.FullName, "Fixtures", "Ntlm", fileName);
            if (File.Exists(localPath)) {
                return localPath;
            }

            string repoPath = Path.Combine(
                directory.FullName,
                "tests",
                "Opc.Classic.Dcom.Crypto.Tests",
                "Fixtures",
                "Ntlm",
                fileName);
            if (File.Exists(repoPath)) {
                return repoPath;
            }
        }

        throw new FileNotFoundException("Could not locate NTLM fixture.", fileName);
    }
}
