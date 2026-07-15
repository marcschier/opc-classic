// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security;
using Kerberos.NET;
using Kerberos.NET.Crypto;
using Opc.Classic;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Dcom.Rpc.Auth;

namespace Opc.Classic.Dcom.Kerberos.Tests;

[ClassDataSource<KdcFixture>(Shared = SharedType.PerAssembly)]
[Category("Kerberos")]
[NotInParallel]
public sealed class KerberosServerAuthenticationProviderIntegrationTests
{
    private readonly KdcFixture _kdc;

    public KerberosServerAuthenticationProviderIntegrationTests(KdcFixture kdc)
    {
        _kdc = kdc;
    }

    [Test]
    public async Task CreateAcceptor_returns_distinct_per_connection_acceptors()
    {
        using var credentials = new PasswordKerberosServerCredentialProvider(
            KdcFixture.ServerSpn,
            KdcFixture.RealmName,
            "placeholder-secret");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper()));

        IRpcServerAuthenticationAcceptor first = provider.CreateAcceptor();
        IRpcServerAuthenticationAcceptor second = provider.CreateAcceptor();

        await Assert.That(provider.AuthenticationService)
            .IsEqualTo(KerberosServerAuthenticationProvider.KerberosAuthenticationService);
        await Assert.That(ReferenceEquals(first, second)).IsFalse();
    }

    [Test]
    public async Task Acceptor_honors_pre_canceled_authentication()
    {
        using var credentials = new PasswordKerberosServerCredentialProvider(
            KdcFixture.ServerSpn,
            KdcFixture.RealmName,
            "placeholder-secret");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper()));
        IRpcServerAuthenticationAcceptor acceptor = provider.CreateAcceptor();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Exception? thrown = CaptureException(() => acceptor.AcceptToken(
            new byte[] { 0x01 },
            OpcProtectionLevel.Integrity,
            cancellation.Token));

        await Assert.That(thrown).IsTypeOf<OperationCanceledException>();
    }

    [Test]
    [Arguments(OpcProtectionLevel.Integrity)]
    [Arguments(OpcProtectionLevel.Privacy)]
    public async Task Acceptor_establishes_mutual_auth_maps_principal_and_round_trips_rpc_protection(
        OpcProtectionLevel protectionLevel)
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(KdcFixture.ServerSpn, "server.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper(), minimumProtectionLevel: protectionLevel));
        IRpcServerAuthenticationAcceptor acceptor = provider.CreateAcceptor();
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());

        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);
        RpcServerAuthenticationTokenResult result = acceptor.AcceptToken(apReq, protectionLevel);
        byte[] sessionKey = await client.ProcessApResponseAsync(result.ResponseToken).ConfigureAwait(false);
        KerberosSession clientSession = CreateClientSession(client);

        await Assert.That(result.Session).IsNotNull();
        await Assert.That(result.ResponseToken.IsEmpty).IsFalse();
        await Assert.That(result.Session!.AuthenticationService)
            .IsEqualTo(KerberosServerAuthenticationProvider.KerberosAuthenticationService);
        await Assert.That(result.Session.ProtectionLevel).IsEqualTo(protectionLevel);
        await Assert.That(result.Session.ProtectionContext).IsNotNull();
        await Assert.That(result.Session.Principal.Identity!.AuthenticationType).IsEqualTo("Kerberos");
        await Assert.That(result.Session.Principal.Identity!.Name).IsEqualTo("operators/testuser");
        await Assert.That(result.Session.Principal.IsInRole("operator")).IsTrue();
        await Assert.That(sessionKey.SequenceEqual(client.EstablishedSessionKey!.Key.ToArray())).IsTrue();

        await AssertClientToServerProtectionAsync(result.Session.ProtectionContext!, clientSession, protectionLevel);
        await AssertServerToClientProtectionAsync(result.Session.ProtectionContext!, clientSession, protectionLevel);
    }

    [Test]
    public async Task Spnego_acceptor_completes_ap_req_ap_rep_mic_and_protected_calls()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        byte[] channelBinding = Enumerable.Repeat((byte)0x5A, 16).ToArray();
        using var credentials = CreateKeytabProvider(
            KdcFixture.ServerSpn,
            "server.keytab");
        var kerberos = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                CreateAllowMapper(),
                channelBindingPolicy: KerberosChannelBindingPolicy.Required,
                channelBindingsHash: channelBinding));
        var provider = new SpnegoServerAuthenticationProvider(kerberos);
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync(
            channelBinding).ConfigureAwait(false);
        byte[] init = SpnegoTokenBuilder.BuildInitToken(
            apReq,
            out byte[] mechListBytes);

        RpcServerAuthenticationTokenResult result = provider.CreateAcceptor()
            .AcceptToken(init, OpcProtectionLevel.Integrity);
        SpnegoNegTokenResp response =
            SpnegoDecoder.DecodeNegTokenResp(result.ResponseToken);
        _ = await client.ProcessApResponseAsync(
            response.ResponseToken.GetValueOrDefault()).ConfigureAwait(false);
        KerberosSession clientSession = CreateClientSession(client);
        bool micVerified = response.VerifyMechListMic(
            mechListBytes,
            new KerberosMicProvider(clientSession));

        await Assert.That(response.NegState)
            .IsEqualTo(SpnegoNegState.AcceptCompleted);
        await Assert.That(response.SupportedMech)
            .IsEqualTo(SpnegoOids.KerberosV5);
        await Assert.That(micVerified).IsTrue();
        await Assert.That(result.Session).IsNotNull();
        await Assert.That(result.Session!.AuthenticationService)
            .IsEqualTo(SpnegoServerAuthenticationProvider.SpnegoAuthenticationService);
        await AssertClientToServerProtectionAsync(
            result.Session.ProtectionContext!,
            clientSession,
            OpcProtectionLevel.Integrity);
        await AssertServerToClientProtectionAsync(
            result.Session.ProtectionContext!,
            clientSession,
            OpcProtectionLevel.Integrity);
    }

    [Test]
    public async Task Acceptor_rejects_ap_req_for_the_wrong_service_principal()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(KdcFixture.ClientSpn, "client.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper(), servicePrincipals: [KdcFixture.ClientSpn]));
        IRpcServerAuthenticationAcceptor acceptor = provider.CreateAcceptor();
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);

        Exception? thrown = CaptureException(() => acceptor.AcceptToken(apReq, OpcProtectionLevel.Integrity));

        await Assert.That(ContainsException<KerberosValidationException>(thrown)
            || ContainsException<SecurityException>(thrown))
            .IsTrue();
    }

    [Test]
    public async Task Acceptor_rejects_ap_req_decrypted_with_the_wrong_service_key()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = new PasswordKerberosServerCredentialProvider(
            KdcFixture.ServerSpn,
            KdcFixture.RealmName,
            "definitely-not-the-service-password");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper()));
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);

        Exception? thrown = CaptureException(() =>
            provider.CreateAcceptor().AcceptToken(
                apReq,
                OpcProtectionLevel.Integrity));

        await Assert.That(ContainsException<SecurityException>(thrown)).IsTrue();
    }

    [Test]
    public async Task Acceptor_rejects_expired_service_ticket_using_configured_clock()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(
            KdcFixture.ServerSpn,
            "server.keytab");
        var clock = new ManualTimeProvider(DateTimeOffset.UtcNow.AddDays(2));
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                CreateAllowMapper(),
                timeProvider: clock));
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);

        Exception? thrown = CaptureException(() =>
            provider.CreateAcceptor().AcceptToken(
                apReq,
                OpcProtectionLevel.Integrity));

        await Assert.That(ContainsException<SecurityException>(thrown)).IsTrue();
    }

    [Test]
    public async Task Acceptor_rejects_authenticator_outside_future_clock_skew()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(
            KdcFixture.ServerSpn,
            "server.keytab");
        var clock = new ManualTimeProvider(DateTimeOffset.UtcNow.AddHours(-1));
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                CreateAllowMapper(),
                timeProvider: clock));
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);

        Exception? thrown = CaptureException(() =>
            provider.CreateAcceptor().AcceptToken(
                apReq,
                OpcProtectionLevel.Integrity));

        await Assert.That(ContainsException<SecurityException>(thrown)).IsTrue();
    }

    [Test]
    public async Task Acceptor_accepts_matching_required_channel_binding()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        byte[] channelBinding = Enumerable.Repeat((byte)0x3C, 16).ToArray();
        using var credentials = CreateKeytabProvider(
            KdcFixture.ServerSpn,
            "server.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                CreateAllowMapper(),
                channelBindingPolicy: KerberosChannelBindingPolicy.Required,
                channelBindingsHash: channelBinding));
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync(
            channelBinding).ConfigureAwait(false);

        RpcServerAuthenticationTokenResult result = provider.CreateAcceptor()
            .AcceptToken(apReq, OpcProtectionLevel.Integrity);

        await Assert.That(result.Session).IsNotNull();
        await Assert.That(result.ResponseToken.IsEmpty).IsFalse();
    }

    [Test]
    public async Task Acceptor_rejects_channel_binding_checksum_mismatch()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        byte[] expectedBinding = Enumerable.Repeat((byte)0x11, 16).ToArray();
        byte[] suppliedBinding = Enumerable.Repeat((byte)0x22, 16).ToArray();
        using var credentials = CreateKeytabProvider(
            KdcFixture.ServerSpn,
            "server.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(
                credentials,
                CreateAllowMapper(),
                channelBindingPolicy: KerberosChannelBindingPolicy.Required,
                channelBindingsHash: expectedBinding));
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync(
            suppliedBinding).ConfigureAwait(false);

        Exception? thrown = CaptureException(() =>
            provider.CreateAcceptor().AcceptToken(
                apReq,
                OpcProtectionLevel.Integrity));

        await Assert.That(ContainsException<SecurityException>(thrown)).IsTrue();
    }

    [Test]
    public async Task Acceptor_rejects_tampered_ap_req()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(KdcFixture.ServerSpn, "server.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper()));
        IRpcServerAuthenticationAcceptor acceptor = provider.CreateAcceptor();
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);
        byte[] tampered = apReq.ToArray();
        tampered[^1] ^= 0x01;

        Exception? thrown = CaptureException(() => acceptor.AcceptToken(tampered, OpcProtectionLevel.Integrity));

        await Assert.That(ContainsException<KerberosValidationException>(thrown)
            || ContainsException<SecurityException>(thrown))
            .IsTrue();
    }

    [Test]
    public async Task Acceptor_rejects_replayed_ap_req_across_connections()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(KdcFixture.ServerSpn, "server.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper()));
        IRpcServerAuthenticationAcceptor first = provider.CreateAcceptor();
        IRpcServerAuthenticationAcceptor second = provider.CreateAcceptor();
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);

        RpcServerAuthenticationTokenResult initial = first.AcceptToken(apReq, OpcProtectionLevel.Integrity);
        Exception? replay = CaptureException(() => second.AcceptToken(apReq, OpcProtectionLevel.Integrity));

        await Assert.That(initial.Session).IsNotNull();
        await Assert.That(initial.ResponseToken.IsEmpty).IsFalse();
        await Assert.That(ContainsException<ReplayException>(replay)
            || ContainsException<KerberosValidationException>(replay)
            || ContainsException<SecurityException>(replay))
            .IsTrue();
    }

    [Test]
    public async Task Acceptor_applies_normalized_principal_allow_and_deny_policy()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        var client = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);

        using var allowCredentials = CreateKeytabProvider(KdcFixture.ServerSpn, "server.keytab");
        var allowProvider = new KerberosServerAuthenticationProvider(
            CreateOptions(
                allowCredentials,
                new KerberosPrincipalMappingPolicy(
                    KerberosPrincipalNormalization.LowercaseNameAndCanonicalRealm,
                    [new KerberosPrincipalMapping("TESTUSER@opcclassic.local", "operators/testuser", ["operator"])])));
        RpcServerAuthenticationTokenResult allowResult =
            allowProvider.CreateAcceptor().AcceptToken(apReq, OpcProtectionLevel.Integrity);

        using var denyCredentials = CreateKeytabProvider(KdcFixture.ServerSpn, "server.keytab");
        var denyProvider = new KerberosServerAuthenticationProvider(
            CreateOptions(
                denyCredentials,
                new KerberosPrincipalMappingPolicy(
                    KerberosPrincipalNormalization.LowercaseNameAndCanonicalRealm,
                    [new KerberosPrincipalMapping("mallory@OPCCLASSIC.LOCAL", "operators/mallory")])));
        Exception? deny = CaptureException(() => denyProvider.CreateAcceptor().AcceptToken(apReq, OpcProtectionLevel.Integrity));

        await Assert.That(allowResult.Session).IsNotNull();
        await Assert.That(allowResult.Session!.Principal.Identity!.Name).IsEqualTo("operators/testuser");
        await Assert.That(allowResult.Session.Principal.IsInRole("operator")).IsTrue();
        await Assert.That(ContainsException<SecurityException>(deny)
            || ContainsException<InvalidOperationException>(deny))
            .IsTrue();
    }

    [Test]
    public async Task CreateAcceptor_supports_parallel_handshakes_without_cross_connection_state()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        using var credentials = CreateKeytabProvider(KdcFixture.ServerSpn, "server.keytab");
        var provider = new KerberosServerAuthenticationProvider(
            CreateOptions(credentials, CreateAllowMapper()));
        IRpcServerAuthenticationAcceptor first = provider.CreateAcceptor();
        IRpcServerAuthenticationAcceptor second = provider.CreateAcceptor();
        var firstClient = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        var secondClient = new KerberosConnectionContext(_kdc.CreateUserKeytabAuthInfo());

        RpcServerAuthenticationTokenResult[] results = await Task.WhenAll(
            CompleteHandshakeAsync(first, firstClient, OpcProtectionLevel.Integrity),
            CompleteHandshakeAsync(second, secondClient, OpcProtectionLevel.Integrity)).ConfigureAwait(false);

        await Assert.That(results.Length).IsEqualTo(2);
        await Assert.That(results[0].Session).IsNotNull();
        await Assert.That(results[1].Session).IsNotNull();
        await Assert.That(ReferenceEquals(results[0].Session, results[1].Session)).IsFalse();
        await Assert.That(results[0].Session!.Principal.Identity!.Name).IsEqualTo("operators/testuser");
        await Assert.That(results[1].Session!.Principal.Identity!.Name).IsEqualTo("operators/testuser");
    }

    private void SkipWhenKdcUnavailable()
    {
        if (!_kdc.IsAvailable)
        {
            Skip.Test(_kdc.SkipReason ?? $"Requires Docker — set {KdcFixture.RunEnvironmentVariable}=1 to enable.");
        }
    }

    private FileKerberosKeytabCredentialProvider CreateKeytabProvider(string principal, string keytabFileName)
    {
        _ = _kdc.ServerKeyTable;
        string path = Path.Combine(AppContext.BaseDirectory, "kerberos-kdc", keytabFileName);
        return new FileKerberosKeytabCredentialProvider(principal, _kdc.Realm, path);
    }

    private KerberosServerOptions CreateOptions(
        IKerberosServerCredentialProvider credentials,
        IKerberosPrincipalMapper mapper,
        IEnumerable<string>? servicePrincipals = null,
        KerberosChannelBindingPolicy channelBindingPolicy = KerberosChannelBindingPolicy.Disabled,
        OpcProtectionLevel minimumProtectionLevel = OpcProtectionLevel.Integrity,
        ReadOnlyMemory<byte>? channelBindingsHash = null,
        TimeProvider? timeProvider = null)
    {
        return new KerberosServerOptions(
            servicePrincipals ?? [KdcFixture.ServerSpn],
            _kdc.Realm,
            credentials,
            [EncryptionType.AES256_CTS_HMAC_SHA1_96, EncryptionType.AES128_CTS_HMAC_SHA1_96],
            TimeSpan.FromMinutes(5),
            channelBindingPolicy,
            minimumProtectionLevel,
            mapper,
            channelBindingsHash,
            timeProvider);
    }

    private static IKerberosPrincipalMapper CreateAllowMapper() =>
        new KerberosPrincipalMappingPolicy(
            KerberosPrincipalNormalization.LowercaseNameAndCanonicalRealm,
            [new KerberosPrincipalMapping("testuser@OPCCLASSIC.LOCAL", "operators/testuser", ["operator"])]);

    private static async Task<RpcServerAuthenticationTokenResult> CompleteHandshakeAsync(
        IRpcServerAuthenticationAcceptor acceptor,
        KerberosConnectionContext client,
        OpcProtectionLevel protectionLevel)
    {
        byte[] apReq = await client.AcquireApRequestAsync().ConfigureAwait(false);
        RpcServerAuthenticationTokenResult result = acceptor.AcceptToken(apReq, protectionLevel);
        _ = await client.ProcessApResponseAsync(result.ResponseToken).ConfigureAwait(false);
        return result;
    }

    private static KerberosSession CreateClientSession(KerberosConnectionContext client)
    {
        KerberosSessionKey sessionKey = client.EstablishedSessionKey
            ?? throw new InvalidOperationException("Kerberos client session key was not established.");
        return new KerberosSession(
            sessionKey.Key.Span,
            sessionKey.EncryptionType,
            sessionKey.SendSequenceNumber,
            sessionKey.ReceiveSequenceNumber,
            isAcceptor: false,
            usesAcceptorSubkey: sessionKey.UsesAcceptorSubkey);
    }

    private static async Task AssertClientToServerProtectionAsync(
        IRpcServerProtectionContext serverProtectionContext,
        KerberosSession clientSession,
        OpcProtectionLevel protectionLevel)
    {
        byte[] originalFirst = [0x10, 0x21, 0x32, 0x43, 0x54, 0x65];
        byte[] wireFirst = originalFirst.ToArray();
        byte[] verifierFirst = ProtectClientPacket(clientSession, wireFirst, confidentialOffset: 2, confidentialLength: 3, protectionLevel);
        byte[] tamperedVerifier = verifierFirst.ToArray();
        tamperedVerifier[^1] ^= 0x01;
        byte[] tamperedFirst = wireFirst.ToArray();
        bool tamperedAccepted = serverProtectionContext.Unprotect(
            tamperedFirst,
            2,
            3,
            tamperedVerifier);
        byte[] receivedFirst = wireFirst.ToArray();
        bool acceptedFirst = serverProtectionContext.Unprotect(receivedFirst, 2, 3, verifierFirst);
        byte[] replayedFirst = wireFirst.ToArray();
        bool replayAccepted = serverProtectionContext.Unprotect(replayedFirst, 2, 3, verifierFirst);

        byte[] originalSecond = [0x70, 0x81, 0x92, 0xA3, 0xB4, 0xC5];
        byte[] wireSecond = originalSecond.ToArray();
        byte[] verifierSecond = ProtectClientPacket(clientSession, wireSecond, confidentialOffset: 1, confidentialLength: 4, protectionLevel);
        byte[] receivedSecond = wireSecond.ToArray();
        bool acceptedSecond = serverProtectionContext.Unprotect(receivedSecond, 1, 4, verifierSecond);

        await Assert.That(tamperedAccepted).IsFalse();
        await Assert.That(acceptedFirst).IsTrue();
        await Assert.That(receivedFirst.SequenceEqual(originalFirst)).IsTrue();
        await Assert.That(replayAccepted).IsFalse();
        await Assert.That(acceptedSecond).IsTrue();
        await Assert.That(receivedSecond.SequenceEqual(originalSecond)).IsTrue();
    }

    private static async Task AssertServerToClientProtectionAsync(
        IRpcServerProtectionContext serverProtectionContext,
        KerberosSession clientSession,
        OpcProtectionLevel protectionLevel)
    {
        byte[] original = [0x01, 0x13, 0x25, 0x37, 0x49, 0x5B];
        byte[] wire = original.ToArray();
        serverProtectionContext.Protect(wire, confidentialOffset: 1, confidentialLength: 4, out byte[] verifier);
        bool accepted = UnprotectForClient(clientSession, wire, 1, 4, verifier, protectionLevel);

        await Assert.That(serverProtectionContext.AuthenticationService)
            .IsEqualTo(KerberosServerAuthenticationProvider.KerberosAuthenticationService);
        await Assert.That(serverProtectionContext.ProtectionLevel).IsEqualTo(protectionLevel);
        await Assert.That(accepted).IsTrue();
        await Assert.That(wire.SequenceEqual(original)).IsTrue();
    }

    private static byte[] ProtectClientPacket(
        KerberosSession clientSession,
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        OpcProtectionLevel protectionLevel)
    {
        if (protectionLevel >= OpcProtectionLevel.Privacy)
        {
            return clientSession.ProtectRpcMessage(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                confidential: true);
        }

        return clientSession.ProtectRpcMessage(
            signedRegion,
            confidentialOffset,
            confidentialLength,
            confidential: false);
    }

    private static bool UnprotectForClient(
        KerberosSession clientSession,
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlyMemory<byte> verifier,
        OpcProtectionLevel protectionLevel)
    {
        try
        {
            clientSession.UnprotectRpcMessage(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                verifier.Span,
                protectionLevel >= OpcProtectionLevel.Privacy);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (SecurityException)
        {
            return false;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return false;
        }
    }

    private static Exception? CaptureException(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private static bool ContainsException<TException>(Exception? exception)
        where TException : Exception =>
        exception is TException
        || exception?.InnerException is not null && ContainsException<TException>(exception.InnerException);

    private sealed class ManualTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public ManualTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
