// SPDX-License-Identifier: MIT

using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Crypto.Tests;

public sealed class NtlmMicTests
{
    private const string User = "User";
    private const string Domain = "Domain";
    private const string Password = "Password";

    [Test]
    public async Task Compute_MatchesFixedHmacMd5Vector()
    {
        byte[] sessionKey = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] negotiate = System.Text.Encoding.ASCII.GetBytes("NEGOTIATE_MESSAGE");
        byte[] challenge = System.Text.Encoding.ASCII.GetBytes("CHALLENGE_MESSAGE");
        byte[] authenticate = Enumerable.Range(0, 96).Select(static i => (byte)i).ToArray();
        Array.Clear(authenticate, Type3Message.MicOffset, Type3Message.MicLength);

        byte[] mic = NtlmMic.Compute(sessionKey, negotiate, challenge, authenticate);

        await Assert.That(Convert.ToHexString(mic).ToLowerInvariant())
            .IsEqualTo("e31f7c21386a1b03ded5cb66998fefe3");
    }

    [Test]
    public async Task Verify_ReturnsTrue_WhenAuthenticateContainsComputedMic()
    {
        byte[] sessionKey = Convert.FromHexString("101112131415161718191A1B1C1D1E1F");
        byte[] negotiate = System.Text.Encoding.ASCII.GetBytes("NEGOTIATE_MESSAGE");
        byte[] challenge = System.Text.Encoding.ASCII.GetBytes("CHALLENGE_MESSAGE");
        byte[] authenticate = Enumerable.Range(0, 96).Select(static i => (byte)(255 - i)).ToArray();
        Array.Clear(authenticate, Type3Message.MicOffset, Type3Message.MicLength);
        byte[] mic = NtlmMic.Compute(sessionKey, negotiate, challenge, authenticate);
        mic.CopyTo(authenticate.AsSpan(Type3Message.MicOffset, Type3Message.MicLength));

        bool verified = NtlmMic.Verify(sessionKey, negotiate, challenge, authenticate, Type3Message.MicOffset);

        await Assert.That(verified).IsTrue();
    }

    [Test]
    public async Task ClientServerRoundTrip_IncludesMicAtOffset72_AndServerVerifies()
    {
        var client = CreateAuthentication();
        var server = CreateAuthentication();
        Type1Message type1 = client.CreateType1();
        Type2Message type2 = server.CreateType2(type1);

        Type3Message type3 = client.CreateType3(type2);
        byte[] authenticate = type3.ToByteArray();
        InvokeCreateSecurityWhenServer(server, type3, authenticate);

        await Assert.That(type2.GetFlag(NtlmFlags.NtlmsspNegotiateVersion)).IsTrue();
        await Assert.That(NtlmAvPairs.HasMicFlag(type2.GetTargetInformation())).IsTrue();
        await Assert.That(type3.HasMic).IsTrue();
        await Assert.That(type3.GetMic().SequenceEqual(authenticate.AsSpan(Type3Message.MicOffset, Type3Message.MicLength).ToArray()))
            .IsTrue();
        await Assert.That(server.Security).IsNotNull();
    }

    [Test]
    public async Task ServerVerification_ThrowsSecurityException_WhenAuthenticateIsTampered()
    {
        var client = CreateAuthentication();
        var server = CreateAuthentication();
        Type1Message type1 = client.CreateType1();
        Type2Message type2 = server.CreateType2(type1);
        Type3Message type3 = client.CreateType3(type2);
        byte[] authenticate = type3.ToByteArray();
        FlipFirstWorkstationByte(authenticate);
        var tamperedType3 = new Type3Message(authenticate);

        await Assert.That(() => InvokeCreateSecurityWhenServer(server, tamperedType3, authenticate))
            .Throws<SecurityException>();
    }

    [Test]
    public async Task Verify_ReturnsFalse_WhenSessionKeyIsTampered()
    {
        byte[] sessionKey = Convert.FromHexString("202122232425262728292A2B2C2D2E2F");
        byte[] negotiate = System.Text.Encoding.ASCII.GetBytes("NEGOTIATE_MESSAGE");
        byte[] challenge = System.Text.Encoding.ASCII.GetBytes("CHALLENGE_MESSAGE");
        byte[] authenticate = Enumerable.Range(0, 96).Select(static i => (byte)(i ^ 0xA5)).ToArray();
        Array.Clear(authenticate, Type3Message.MicOffset, Type3Message.MicLength);
        byte[] mic = NtlmMic.Compute(sessionKey, negotiate, challenge, authenticate);
        mic.CopyTo(authenticate.AsSpan(Type3Message.MicOffset, Type3Message.MicLength));
        byte[] wrongSessionKey = (byte[])sessionKey.Clone();
        wrongSessionKey[0] ^= 0x80;

        bool verified = NtlmMic.Verify(wrongSessionKey, negotiate, challenge, authenticate, Type3Message.MicOffset);

        await Assert.That(verified).IsFalse();
    }

    [Test]
    public async Task Verify_UsesFixedTimeEqualsForMicComparison()
    {
        string sourcePath = Path.Combine(FindRepositoryRoot(), "src", "Opc.Classic.Dcom", "Common", "Ntlm", "NtlmMic.cs");
        string source = File.ReadAllText(sourcePath);

        await Assert.That(source.Contains("CryptographicOperations.FixedTimeEquals", StringComparison.Ordinal)).IsTrue();
    }

    private static NtlmAuthentication CreateAuthentication()
    {
        var properties = new PropertyBag();
        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", "true");
        properties.SetProperty("rpc.ntlm.seal", "true");
        properties.SetProperty("rpc.ntlm.keyExchange", "true");
        properties.SetProperty("rpc.ntlm.keyLength", "128");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");
        properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        properties.SetProperty("rpc.ntlm.allowV1", "false");
        properties.SetProperty("rpc.ntlm.sso", "false");
        properties.SetProperty("rpc.ntlm.domain", Domain);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, User);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, Password);
        return new NtlmAuthentication(properties);
    }

    private static void FlipFirstWorkstationByte(byte[] authenticate)
    {
        const int workstationFieldsOffset = 44;
        ushort length = BinaryPrimitives.ReadUInt16LittleEndian(authenticate.AsSpan(workstationFieldsOffset, sizeof(ushort)));
        uint offset = BinaryPrimitives.ReadUInt32LittleEndian(authenticate.AsSpan(workstationFieldsOffset + 4, sizeof(uint)));
        if (length == 0 || offset >= authenticate.Length)
        {
            throw new InvalidOperationException("Test authenticate message did not contain a workstation payload.");
        }

        authenticate[(int)offset] ^= 0x20;
    }

    private static void InvokeCreateSecurityWhenServer(NtlmAuthentication authentication, Type3Message type3, byte[] authenticate)
    {
        MethodInfo method = typeof(NtlmAuthentication).GetMethod(
            "CreateSecurityWhenServerWithMic",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(object), typeof(byte[]) },
            modifiers: null)!;
        try
        {
            method.Invoke(authentication, new object[] { type3, authenticate });
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
