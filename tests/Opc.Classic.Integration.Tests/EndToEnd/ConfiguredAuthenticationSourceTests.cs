// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Integration.Tests.EndToEnd;

/// <summary>
/// Exercises the production <see cref="ConfiguredAuthenticationSource" /> server path end-to-end
/// against the production <see cref="NtlmAuthentication" /> client engine: a valid credential
/// establishes a matching session key, and a wrong password / missing challenge is rejected.
/// </summary>
public sealed class ConfiguredAuthenticationSourceTests
{
    private const string Domain = "DOMAIN";
    private const string User = "User";
    private const string Password = "Password";
    private const string WrongPassword = "WrongPassword";

    [Test, Category("EndToEnd")]
    public async Task Authenticate_establishes_matching_session_key_for_valid_credentials()
    {
        NtlmAuthentication client = CreateClient(Password);
        var source = new ConfiguredAuthenticationSource(User, Password, Domain);
        var connection = new PropertyBag();

        Type1Message type1 = client.CreateType1();
        byte[] type2Token = source.CreateChallenge(connection, type1);
        var type2 = new Type2Message(type2Token);
        Type3Message type3 = client.CreateType3(type2);

        sbyte[] serverKey = source.Authenticate(connection, type2, type3);
        byte[] serverKeyBytes = FromSigned(serverKey);
        ReadOnlyMemory<byte>? clientKey = client.EstablishedSessionKey;

        await Assert.That(clientKey.HasValue).IsTrue();
        await Assert.That(serverKeyBytes.Length).IsEqualTo(16);
        await Assert.That(serverKeyBytes.SequenceEqual(clientKey.GetValueOrDefault().ToArray())).IsTrue();
        await Assert.That(ConfiguredAuthenticationSource.GetEstablishedContext(connection)).IsNotNull();
    }

    [Test, Category("EndToEnd")]
    public async Task Authenticate_rejects_wrong_password()
    {
        NtlmAuthentication client = CreateClient(WrongPassword);
        var source = new ConfiguredAuthenticationSource(User, Password, Domain);
        var connection = new PropertyBag();

        Type1Message type1 = client.CreateType1();
        byte[] type2Token = source.CreateChallenge(connection, type1);
        var type2 = new Type2Message(type2Token);
        Type3Message type3 = client.CreateType3(type2);

        await Assert.That(() => source.Authenticate(connection, type2, type3)).Throws<SecurityException>();
    }

    [Test, Category("EndToEnd")]
    public async Task Authenticate_without_prior_challenge_throws()
    {
        var source = new ConfiguredAuthenticationSource(User, Password, Domain);

        await Assert.That(() => source.Authenticate(new PropertyBag(), type2: null!, new Type3Message()))
            .Throws<InvalidOperationException>();
    }

    private static NtlmAuthentication CreateClient(string password)
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
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, password);
        return new NtlmAuthentication(properties);
    }

    private static byte[] FromSigned(sbyte[] bytes)
    {
        var unsigned = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            unsigned[i] = unchecked((byte)bytes[i]);
        }

        return unsigned;
    }
}
