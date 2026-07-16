// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Security.Principal;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Dcom.Rpc.Auth;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosServerOptionsTests
{
    private const string Principal = "RPCSS/server.example.com";
    private const string Realm = "EXAMPLE.COM";
    private const string Password = "not-a-real-password";

    [Test]
    public async Task Options_validate_required_and_bounded_policy()
    {
        using var credentials = CreatePasswordProvider();
        IKerberosPrincipalMapper mapper = CreateMapper();

        await Assert.That(() => CreateOptions(credentials, mapper, servicePrincipals: []))
            .Throws<ArgumentException>();
        await Assert.That(() => CreateOptions(credentials, mapper, realm: "OTHER.COM"))
            .Throws<ArgumentException>();
        await Assert.That(() => CreateOptions(
                credentials,
                mapper,
                encryptionTypes: [EncryptionType.DES_CBC_MD5]))
            .Throws<ArgumentException>();
        await Assert.That(() => CreateOptions(credentials, mapper, clockSkew: TimeSpan.Zero))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => CreateOptions(
                credentials,
                mapper,
                minimumProtection: OpcProtectionLevel.None))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => CreateOptions(
                credentials,
                mapper,
                channelBinding: (KerberosChannelBindingPolicy)999))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => CreateOptions(
                credentials,
                mapper,
                channelBindingsHash: new byte[15]))
            .Throws<ArgumentException>();
        await Assert.That(() => CreateOptions(
                credentials,
                mapper,
                channelBinding: KerberosChannelBindingPolicy.WhenPresent))
            .Throws<ArgumentException>();
        await Assert.That(() => CreateOptions(
                credentials,
                mapper,
                channelBinding: KerberosChannelBindingPolicy.Required))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Password_source_rotates_snapshots_and_redacts_secrets()
    {
        using var provider = CreatePasswordProvider();
        using KerberosServerCredential first = provider.AcquireCredential();
        var firstPassword = new char[Password.Length];
        ((KerberosPasswordCredential)first).CopyPasswordTo(firstPassword);

        provider.Rotate("replacement-secret");
        using KerberosServerCredential second = provider.AcquireCredential();
        var secondPassword = new char[((KerberosPasswordCredential)second).SecretLength];
        ((KerberosPasswordCredential)second).CopyPasswordTo(secondPassword);

        await Assert.That(new string(firstPassword)).IsEqualTo(Password);
        await Assert.That(new string(secondPassword)).IsEqualTo("replacement-secret");
        await Assert.That(provider.Version).IsEqualTo(2);
        await Assert.That(provider.ToString()).DoesNotContain(Password);
        await Assert.That(provider.ToString()).DoesNotContain("replacement-secret");
        await Assert.That(second.ToString()).DoesNotContain("replacement-secret");
        Array.Clear(firstPassword);
        Array.Clear(secondPassword);
    }

    [Test]
    public async Task Credential_snapshots_reject_secret_access_after_disposal()
    {
        using var provider = CreatePasswordProvider();
        KerberosServerCredential credential = provider.AcquireCredential();
        credential.Dispose();

        await Assert.That(credential.IsDisposed).IsTrue();
        await Assert.That(() =>
            ((KerberosPasswordCredential)credential).CopyPasswordTo(new char[Password.Length]))
            .Throws<ObjectDisposedException>();

        provider.Dispose();
        await Assert.That(() => provider.AcquireCredential())
            .Throws<ObjectDisposedException>();
    }

    [Test]
    public async Task Keytab_source_loads_rotates_and_preserves_existing_snapshots()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            $"kerberos-options-{Guid.NewGuid():N}.keytab");
        byte[] firstBytes = CreateKeytab(0x11);
        byte[] secondBytes = CreateKeytab(0x22);
        try
        {
            await File.WriteAllBytesAsync(path, firstBytes);
            using var provider = new FileKerberosKeytabCredentialProvider(Principal, Realm, path);
            using var first = (KerberosKeytabCredential)provider.AcquireCredential();

            await File.WriteAllBytesAsync(path, secondBytes);
            using var second = (KerberosKeytabCredential)provider.AcquireCredential();
            var firstCopy = new byte[first.SecretLength];
            var secondCopy = new byte[second.SecretLength];
            first.CopyKeytabTo(firstCopy);
            second.CopyKeytabTo(secondCopy);

            await Assert.That(firstCopy).IsEquivalentTo(firstBytes);
            await Assert.That(secondCopy).IsEquivalentTo(secondBytes);
            await Assert.That(provider.Version).IsEqualTo(2);
            await Assert.That(provider.ToString()).DoesNotContain(Convert.ToHexString(secondBytes));
            Array.Clear(firstCopy);
            Array.Clear(secondCopy);
        }
        finally
        {
            File.Delete(path);
            Array.Clear(firstBytes);
            Array.Clear(secondBytes);
        }
    }

    [Test]
    public async Task Keytab_source_rejects_missing_and_malformed_files()
    {
        string missing = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.keytab");
        await Assert.That(() =>
                new FileKerberosKeytabCredentialProvider(Principal, Realm, missing))
            .Throws<FileNotFoundException>();

        string malformed = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.keytab");
        try
        {
            await File.WriteAllBytesAsync(malformed, [5, 2, 0, 0, 0, 1, 10]);
            await Assert.That(() =>
                    new FileKerberosKeytabCredentialProvider(Principal, Realm, malformed))
                .Throws<InvalidDataException>();
        }
        finally
        {
            File.Delete(malformed);
        }
    }

    [Test]
    public async Task Keytab_source_rejects_oversized_and_torn_rotation_without_swapping()
    {
        string path = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.keytab");
        byte[] firstBytes = CreateKeytab(0x31);
        byte[] secondBytes = CreateKeytab(0x32);
        try
        {
            await File.WriteAllBytesAsync(path, firstBytes);
            using var provider = new FileKerberosKeytabCredentialProvider(Principal, Realm, path);

            await File.WriteAllBytesAsync(path, secondBytes.AsMemory(0, secondBytes.Length / 2));
            await Assert.That(() => provider.Reload()).Throws<InvalidDataException>();
            await Assert.That(provider.Version).IsEqualTo(1);

            await ReplaceAtomicallyAsync(path, secondBytes);
            await Assert.That(provider.Reload()).IsTrue();
            await Assert.That(provider.Version).IsEqualTo(2);

            provider.Dispose();
            using (var oversized = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                oversized.SetLength((16L * 1024 * 1024) + 1);
            }

            await Assert.That(() =>
                    new FileKerberosKeytabCredentialProvider(Principal, Realm, path))
                .Throws<InvalidDataException>();
        }
        finally
        {
            File.Delete(path);
            Array.Clear(firstBytes);
            Array.Clear(secondBytes);
        }
    }

    [Test]
    public async Task Keytab_source_concurrent_atomic_rotation_never_returns_torn_snapshot()
    {
        string path = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.keytab");
        byte[] firstBytes = CreateKeytab(0x41);
        byte[] secondBytes = CreateKeytab(0x42);
        var errors = new ConcurrentQueue<Exception>();
        int invalidSnapshots = 0;
        try
        {
            await File.WriteAllBytesAsync(path, firstBytes);
            using var provider = new FileKerberosKeytabCredentialProvider(Principal, Realm, path);

            Task writer = Task.Run(async () =>
            {
                for (int iteration = 0; iteration < 12; iteration++)
                {
                    await ReplaceAtomicallyAsync(
                        path,
                        iteration % 2 == 0 ? secondBytes : firstBytes);
                }
            });
            Task[] readers = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
            {
                try
                {
                    for (int iteration = 0; iteration < 20; iteration++)
                    {
                        using var credential = (KerberosKeytabCredential)provider.AcquireCredential();
                        var snapshot = new byte[credential.SecretLength];
                        try
                        {
                            credential.CopyKeytabTo(snapshot);
                            if (!snapshot.AsSpan().SequenceEqual(firstBytes)
                                && !snapshot.AsSpan().SequenceEqual(secondBytes))
                            {
                                Interlocked.Increment(ref invalidSnapshots);
                            }
                        }
                        finally
                        {
                            Array.Clear(snapshot);
                        }
                    }
                }
                catch (Exception exception)
                {
                    errors.Enqueue(exception);
                }
            })).ToArray();

            await Task.WhenAll(readers.Append(writer));
            await ReplaceAtomicallyAsync(path, secondBytes);
            provider.Reload();

            await Assert.That(errors).IsEmpty();
            await Assert.That(invalidSnapshots).IsEqualTo(0);
            await Assert.That(provider.Version).IsGreaterThanOrEqualTo(2);
        }
        finally
        {
            File.Delete(path);
            Array.Clear(firstBytes);
            Array.Clear(secondBytes);
        }
    }

    [Test]
    public async Task Principal_policy_normalizes_maps_roles_and_rejects_unmapped()
    {
        var policy = new KerberosPrincipalMappingPolicy(
            KerberosPrincipalNormalization.LowercaseNameAndCanonicalRealm,
            [new KerberosPrincipalMapping("Alice@example.com", "operators/alice", ["operator"])]);

        bool mapped = policy.TryMapPrincipal("ALICE@EXAMPLE.COM", out IPrincipal? principal);
        bool rejected = policy.TryMapPrincipal("mallory@EXAMPLE.COM", out IPrincipal? rejectedPrincipal);

        await Assert.That(mapped).IsTrue();
        await Assert.That(principal!.Identity!.Name).IsEqualTo("operators/alice");
        await Assert.That(principal.IsInRole("operator")).IsTrue();
        await Assert.That(rejected).IsFalse();
        await Assert.That(rejectedPrincipal).IsNull();
    }

    [Test]
    public async Task Principal_policy_requires_explicit_opt_in_for_unmapped_principals()
    {
        await Assert.That(() => new KerberosPrincipalMappingPolicy(
                KerberosPrincipalNormalization.CanonicalRealm,
                []))
            .Throws<ArgumentException>();

        var policy = new KerberosPrincipalMappingPolicy(
            KerberosPrincipalNormalization.CanonicalRealm,
            [],
            allowUnmappedPrincipals: true);

        bool mapped = policy.TryMapPrincipal("alice@example.com", out IPrincipal? principal);
        await Assert.That(mapped).IsTrue();
        await Assert.That(principal!.Identity!.Name).IsEqualTo("alice@EXAMPLE.COM");
        await Assert.That(principal.IsInRole("Administrators")).IsFalse();
    }

    [Test]
    public async Task Explicit_and_DI_registration_construct_without_reflection()
    {
        using var credentials = CreatePasswordProvider();
        KerberosServerOptions options = CreateOptions(credentials, CreateMapper());
        var registry = new RpcServerAuthenticationProviderRegistry();

        KerberosServerAuthenticationProvider explicitProvider = registry.RegisterKerberos(options);
        bool found = registry.TryGetProvider(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            out IRpcServerAuthenticationProvider? selected);

        var services = new ServiceCollection();
        IServiceCollection returned = services.AddKerberosRpcServerAuthentication(options);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        var diRegistry = serviceProvider.GetRequiredService<RpcServerAuthenticationProviderRegistry>();
        bool diFound = diRegistry.TryGetProvider(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            out IRpcServerAuthenticationProvider? diSelected);

        await Assert.That(found).IsTrue();
        await Assert.That(selected).IsSameReferenceAs(explicitProvider);
        await Assert.That(returned).IsSameReferenceAs(services);
        await Assert.That(diFound).IsTrue();
        await Assert.That(diSelected).IsTypeOf<KerberosServerAuthenticationProvider>();
        await Assert.That(options.ToString()).DoesNotContain(Password);
    }

    [Test]
    public async Task DI_registration_composes_existing_registry_and_multiple_providers()
    {
        using var credentials = CreatePasswordProvider();
        KerberosServerOptions options = CreateOptions(credentials, CreateMapper());
        var firstProvider = new StubAuthenticationProvider(41);
        var secondProvider = new StubAuthenticationProvider(42);
        var existingRegistry = new RpcServerAuthenticationProviderRegistry([firstProvider]);
        var services = new ServiceCollection();
        services.AddSingleton(existingRegistry);
        services.AddSingleton<IRpcServerAuthenticationProvider>(secondProvider);
        services.AddSingleton<IRpcServerAuthenticationProviderSelector>(existingRegistry);

        services.AddKerberosRpcServerAuthentication(options);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<RpcServerAuthenticationProviderRegistry>();
        var selector = serviceProvider.GetRequiredService<IRpcServerAuthenticationProviderSelector>();

        bool firstFound = registry.TryGetProvider(41, out IRpcServerAuthenticationProvider? first);
        bool secondFound = registry.TryGetProvider(42, out IRpcServerAuthenticationProvider? second);
        bool kerberosFound = selector.TryGetProvider(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            out IRpcServerAuthenticationProvider? kerberos);

        await Assert.That(registry).IsSameReferenceAs(existingRegistry);
        await Assert.That(selector).IsSameReferenceAs(existingRegistry);
        await Assert.That(firstFound).IsTrue();
        await Assert.That(first).IsSameReferenceAs(firstProvider);
        await Assert.That(secondFound).IsTrue();
        await Assert.That(second).IsSameReferenceAs(secondProvider);
        await Assert.That(kerberosFound).IsTrue();
        await Assert.That(kerberos).IsTypeOf<KerberosServerAuthenticationProvider>();
    }

    [Test]
    public async Task Options_accept_an_explicit_test_credential_provider()
    {
        using var provider = new TestCredentialProvider();
        KerberosServerOptions options = CreateOptions(provider, CreateMapper());
        using KerberosServerCredential credential = options.CredentialProvider.AcquireCredential();

        await Assert.That(options.CredentialProvider).IsSameReferenceAs(provider);
        await Assert.That(credential.Kind).IsEqualTo(KerberosServerCredentialKind.Password);
        await Assert.That(provider.AcquireCount).IsEqualTo(1);
    }

    private static PasswordKerberosServerCredentialProvider CreatePasswordProvider() =>
        new(Principal, Realm, Password);

    private static IKerberosPrincipalMapper CreateMapper() =>
        new KerberosPrincipalMappingPolicy(
            KerberosPrincipalNormalization.CanonicalRealm,
            [new KerberosPrincipalMapping("alice@EXAMPLE.COM", "alice")]);

    private static byte[] CreateKeytab(byte marker)
    {
        byte[] keyBytes = Enumerable.Repeat(marker, 32).ToArray();
        try
        {
            var principal = new PrincipalName(
                PrincipalNameType.NT_SRV_INST,
                Realm,
                Principal.Split('/'));
            var key = new KerberosKey(
                key: keyBytes,
                principal: principal,
                etype: EncryptionType.AES256_CTS_HMAC_SHA1_96,
                kvno: 1);
            var keyTable = new KeyTable([key]);
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                keyTable.Write(writer);
            }

            return stream.ToArray();
        }
        finally
        {
            Array.Clear(keyBytes);
        }
    }

    private static async Task ReplaceAtomicallyAsync(string path, byte[] bytes)
    {
        string replacement = $"{path}.{Guid.NewGuid():N}.replacement";
        try
        {
            await File.WriteAllBytesAsync(replacement, bytes);
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    File.Move(replacement, path, overwrite: true);
                    break;
                }
                catch (Exception exception) when (
                    attempt < 99
                    && exception is IOException or UnauthorizedAccessException)
                {
                    await Task.Delay(1);
                }
            }
        }
        finally
        {
            File.Delete(replacement);
        }
    }

    private static KerberosServerOptions CreateOptions(
        IKerberosServerCredentialProvider credentials,
        IKerberosPrincipalMapper mapper,
        IEnumerable<string>? servicePrincipals = null,
        string realm = Realm,
        IEnumerable<EncryptionType>? encryptionTypes = null,
        TimeSpan? clockSkew = null,
        KerberosChannelBindingPolicy channelBinding = KerberosChannelBindingPolicy.Disabled,
        OpcProtectionLevel minimumProtection = OpcProtectionLevel.Integrity,
        ReadOnlyMemory<byte>? channelBindingsHash = null) =>
        new(
            servicePrincipals ?? [Principal],
            realm,
            credentials,
            encryptionTypes ?? [EncryptionType.AES256_CTS_HMAC_SHA1_96],
            clockSkew ?? TimeSpan.FromMinutes(5),
            channelBinding,
            minimumProtection,
            mapper,
            channelBindingsHash);

    private sealed class TestCredentialProvider : IKerberosServerCredentialProvider
    {
        public string Principal => KerberosServerOptionsTests.Principal;

        public string Realm => KerberosServerOptionsTests.Realm;

        public long Version => 1;

        public int AcquireCount { get; private set; }

        public KerberosServerCredential AcquireCredential()
        {
            AcquireCount++;
            return new KerberosPasswordCredential(Principal, Realm, "test-secret");
        }

        public void Dispose()
        {
        }
    }

    private sealed class StubAuthenticationProvider : IRpcServerAuthenticationProvider
    {
        public StubAuthenticationProvider(int authenticationService) =>
            AuthenticationService = authenticationService;

        public int AuthenticationService { get; }

        public IRpcServerAuthenticationAcceptor CreateAcceptor() =>
            throw new InvalidOperationException("Not used by registration tests.");
    }
}
