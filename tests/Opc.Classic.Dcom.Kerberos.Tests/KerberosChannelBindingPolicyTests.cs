// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Reflection;
using System.Security;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;
using Opc.Classic.Dcom.Rpc.Auth;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosChannelBindingPolicyTests
{
    private const string Principal = "RPCSS/server.example.com";
    private const string Realm = "EXAMPLE.COM";
    private static readonly byte[] ExpectedHash =
        Convert.FromHexString("00112233445566778899AABBCCDDEEFF");

    [Test]
    public async Task Disabled_ignores_supplied_channel_binding_bytes()
    {
        using var credentials = new PasswordKerberosServerCredentialProvider(
            Principal,
            Realm,
            "not-a-real-password");
        KerberosServerOptions options = CreateOptions(
            credentials,
            KerberosChannelBindingPolicy.Disabled,
            expectedHash: null);
        object acceptor =
            new KerberosServerAuthenticationProvider(options).CreateAcceptor();
        var checksum = new DelegationInfo
        {
            ChannelBinding = new byte[] { 0x01, 0x02, 0x03 },
        };

        Exception? thrown = InvokeValidation(acceptor, checksum);

        await Assert.That(thrown).IsNull();
    }

    [Test]
    public async Task WhenPresent_and_Required_compare_against_configured_hash()
    {
        using var credentials = new PasswordKerberosServerCredentialProvider(
            Principal,
            Realm,
            "not-a-real-password");
        object whenPresent = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                KerberosChannelBindingPolicy.WhenPresent,
                ExpectedHash)).CreateAcceptor();
        object required = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                KerberosChannelBindingPolicy.Required,
                ExpectedHash)).CreateAcceptor();

        Exception? matching = InvokeValidation(
            whenPresent,
            new DelegationInfo { ChannelBinding = ExpectedHash });
        Exception? absentOptional = InvokeValidation(
            whenPresent,
            new DelegationInfo { ChannelBinding = ReadOnlyMemory<byte>.Empty });
        byte[] mismatchedHash = new byte[16];
        mismatchedHash[0] = 0xFF;
        Exception? mismatch = InvokeValidation(
            whenPresent,
            new DelegationInfo { ChannelBinding = mismatchedHash });
        Exception? absentRequired = InvokeValidation(
            required,
            new DelegationInfo { ChannelBinding = ReadOnlyMemory<byte>.Empty });

        await Assert.That(matching).IsNull();
        await Assert.That(absentOptional).IsNull();
        await Assert.That(mismatch).IsTypeOf<SecurityException>();
        await Assert.That(absentRequired).IsTypeOf<SecurityException>();
    }

    private static KerberosServerOptions CreateOptions(
        IKerberosServerCredentialProvider credentials,
        KerberosChannelBindingPolicy policy,
        ReadOnlyMemory<byte>? expectedHash) =>
        new(
            [Principal],
            Realm,
            credentials,
            [EncryptionType.AES128_CTS_HMAC_SHA1_96],
            TimeSpan.FromMinutes(5),
            policy,
            OpcProtectionLevel.Integrity,
            new KerberosPrincipalMappingPolicy(
                KerberosPrincipalNormalization.CanonicalRealm,
                [],
                allowUnmappedPrincipals: true),
            expectedHash);

    private static Exception? InvokeValidation(
        object acceptor,
        DelegationInfo checksum)
    {
        MethodInfo method = acceptor.GetType().GetMethod(
            "ValidateChannelBinding",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            _ = method.Invoke(acceptor, [checksum]);
            return null;
        }
        catch (TargetInvocationException exception)
        {
            return exception.InnerException;
        }
    }
}
