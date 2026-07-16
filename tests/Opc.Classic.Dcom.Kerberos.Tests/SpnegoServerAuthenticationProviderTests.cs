// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Dcom.Rpc.Auth;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class SpnegoServerAuthenticationProviderTests
{
    private const string NtlmDomain = "DOMAIN";
    private const string NtlmPassword = "Password";
    private const string NtlmUser = "User";

    private static readonly byte[] MicKey =
    [
        0x00, 0x11, 0x22, 0x33,
        0x44, 0x55, 0x66, 0x77,
        0x88, 0x99, 0xAA, 0xBB,
        0xCC, 0xDD, 0xEE, 0xFF,
    ];

    [Test]
    public async Task Kerberos_first_completes_with_selected_mechanism_response_mic_and_outer_protection()
    {
        var protection = new TestProtectionContext(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            MicKey);
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x01],
            [0x02],
            protection);
        var provider = new SpnegoServerAuthenticationProvider(kerberos);
        var acceptor = (SpnegoServerAuthenticationProvider.Acceptor)provider.CreateAcceptor();
        byte[] token = EncodeInit(
            [SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp],
            [0x01]);

        RpcServerAuthenticationTokenResult result =
            acceptor.AcceptToken(token, OpcProtectionLevel.Integrity);
        SpnegoNegTokenResp response =
            SpnegoDecoder.DecodeNegTokenResp(result.ResponseToken);

        await Assert.That(acceptor.SelectedMechanism)
            .IsEqualTo(SpnegoMech.KerberosV5);
        await Assert.That(acceptor.NegotiationState)
            .IsEqualTo(SpnegoNegState.AcceptCompleted);
        await Assert.That(response.NegState)
            .IsEqualTo(SpnegoNegState.AcceptCompleted);
        await Assert.That(response.SupportedMech)
            .IsEqualTo(SpnegoOids.KerberosV5);
        await Assert.That(response.ResponseToken.GetValueOrDefault().Span.SequenceEqual(new byte[] { 0x02 }))
            .IsTrue();
        await Assert.That(response.MechListMic.HasValue).IsTrue();
        await Assert.That(result.Session).IsNotNull();
        await Assert.That(result.Session!.AuthenticationService)
            .IsEqualTo(SpnegoServerAuthenticationProvider.SpnegoAuthenticationService);
        await Assert.That(result.Session.ProtectionContext!.AuthenticationService)
            .IsEqualTo(SpnegoServerAuthenticationProvider.SpnegoAuthenticationService);

        byte[] payload = [0x10, 0x20, 0x30];
        result.Session.ProtectionContext.Protect(payload, 0, payload.Length, out byte[] verifier);
        bool verified = result.Session.ProtectionContext.Unprotect(
            payload,
            0,
            payload.Length,
            verifier);

        await Assert.That(verified).IsTrue();
        await Assert.That(protection.ProtectCalls).IsEqualTo(1);
        await Assert.That(protection.UnprotectCalls).IsEqualTo(1);
    }

    [Test]
    public async Task Kerberos_is_selected_over_an_optimistic_ntlm_token_and_requires_valid_mic()
    {
        var protection = new TestProtectionContext(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            MicKey);
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x33],
            [0x44],
            protection);
        var ntlm = FakeAuthenticationProvider.Completes(
            10,
            [0x11],
            [],
            new TestProtectionContext(10, MicKey));
        var provider = new SpnegoServerAuthenticationProvider(
            kerberos,
            ntlm,
            allowNtlmFallback: true);
        var acceptor = (SpnegoServerAuthenticationProvider.Acceptor)provider.CreateAcceptor();
        string[] mechanisms = [SpnegoOids.Ntlmssp, SpnegoOids.KerberosV5];
        byte[] mechList = SpnegoEncoder.EncodeMechTypeList(mechanisms);

        RpcServerAuthenticationTokenResult first = acceptor.AcceptToken(
            EncodeInit(mechanisms, [0x11]),
            OpcProtectionLevel.Integrity);
        SpnegoNegTokenResp firstResponse =
            SpnegoDecoder.DecodeNegTokenResp(first.ResponseToken);
        var micProvider = new TestMicProvider(MicKey);
        byte[] peerMic = micProvider.GetMic(mechList);
        RpcServerAuthenticationTokenResult second = acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.KerberosV5,
                    new byte[] { 0x33 },
                    peerMic)),
            OpcProtectionLevel.Integrity);

        await Assert.That(first.Session).IsNull();
        await Assert.That(firstResponse.NegState)
            .IsEqualTo(SpnegoNegState.AcceptIncomplete);
        await Assert.That(firstResponse.SupportedMech)
            .IsEqualTo(SpnegoOids.KerberosV5);
        await Assert.That(kerberos.AcceptCalls).IsEqualTo(1);
        await Assert.That(ntlm.AcceptCalls).IsEqualTo(0);
        await Assert.That(second.Session).IsNotNull();
        await Assert.That(
            SpnegoDecoder.DecodeNegTokenResp(second.ResponseToken).NegState)
            .IsEqualTo(SpnegoNegState.AcceptCompleted);
    }

    [Test]
    public async Task Changed_selection_with_tampered_mic_is_rejected()
    {
        var protection = new TestProtectionContext(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            MicKey);
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x33],
            [],
            protection);
        var provider = new SpnegoServerAuthenticationProvider(kerberos);
        var acceptor = (SpnegoServerAuthenticationProvider.Acceptor)provider.CreateAcceptor();
        string[] mechanisms = ["1.2.3", SpnegoOids.KerberosV5];
        byte[] mechList = SpnegoEncoder.EncodeMechTypeList(mechanisms);
        _ = acceptor.AcceptToken(
            EncodeInit(mechanisms, [0x7F]),
            OpcProtectionLevel.Integrity);
        byte[] mic = new TestMicProvider(MicKey).GetMic(mechList);
        mic[^1] ^= 0x01;

        Exception? thrown = CaptureException(() => acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.KerberosV5,
                    new byte[] { 0x33 },
                    mic)),
            OpcProtectionLevel.Integrity));

        await Assert.That(thrown).IsTypeOf<SecurityException>();
        await Assert.That(acceptor.NegotiationState)
            .IsEqualTo(SpnegoNegState.Reject);
    }

    [Test]
    public async Task Ntlm_fallback_is_policy_controlled_and_preserves_multi_step_tokens()
    {
        var ntlm = FakeAuthenticationProvider.ContinuesThenCompletes(
            10,
            [0x01],
            [0x02],
            [0x03],
            new TestProtectionContext(10, MicKey));
        var options = new SpnegoServerOptions(
            kerberosProvider: null,
            ntlm,
            SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable);
        var provider = new SpnegoServerAuthenticationProvider(options);
        var acceptor = (SpnegoServerAuthenticationProvider.Acceptor)provider.CreateAcceptor();

        RpcServerAuthenticationTokenResult first = acceptor.AcceptToken(
            EncodeInit([SpnegoOids.Ntlmssp], [0x01]),
            OpcProtectionLevel.Integrity);
        SpnegoNegTokenResp challenge =
            SpnegoDecoder.DecodeNegTokenResp(first.ResponseToken);
        RpcServerAuthenticationTokenResult second = acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.Ntlmssp,
                    new byte[] { 0x03 },
                    null)),
            OpcProtectionLevel.Integrity);

        await Assert.That(challenge.NegState)
            .IsEqualTo(SpnegoNegState.AcceptIncomplete);
        await Assert.That(challenge.SupportedMech)
            .IsEqualTo(SpnegoOids.Ntlmssp);
        await Assert.That(challenge.ResponseToken.GetValueOrDefault().Span.SequenceEqual(new byte[] { 0x02 }))
            .IsTrue();
        await Assert.That(second.Session).IsNotNull();
        await Assert.That(second.Session!.AuthenticationService)
            .IsEqualTo(SpnegoServerAuthenticationProvider.SpnegoAuthenticationService);
        await Assert.That(ntlm.AcceptCalls).IsEqualTo(2);
    }

    [Test]
    public async Task Configured_Kerberos_fallback_requires_MIC_even_when_NTLM_is_first()
    {
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x70],
            [],
            new TestProtectionContext(
                KerberosServerAuthenticationProvider.KerberosAuthenticationService,
                MicKey));
        var ntlm = FakeAuthenticationProvider.ContinuesThenCompletes(
            10,
            [0x01],
            [0x02],
            [0x03],
            new TestProtectionContext(10, MicKey));
        var provider = new SpnegoServerAuthenticationProvider(
            kerberos,
            ntlm,
            allowNtlmFallback: true);
        var acceptor =
            (SpnegoServerAuthenticationProvider.Acceptor)provider.CreateAcceptor();
        string[] mechanisms = [SpnegoOids.Ntlmssp];

        _ = acceptor.AcceptToken(
            EncodeInit(mechanisms, [0x01]),
            OpcProtectionLevel.Integrity);
        Exception? missingMic = CaptureException(() => acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.Ntlmssp,
                    new byte[] { 0x03 },
                    null)),
            OpcProtectionLevel.Integrity,
            isFinalLeg: true,
            CancellationToken.None));

        await Assert.That(missingMic).IsTypeOf<SecurityException>();
        await Assert.That(kerberos.AcceptCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Ntlm_over_SPNEGO_final_auth3_completes_without_outbound_token()
    {
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x70],
            [],
            new TestProtectionContext(
                KerberosServerAuthenticationProvider.KerberosAuthenticationService,
                MicKey));
        var ntlm = FakeAuthenticationProvider.ContinuesThenCompletes(
            10,
            [0x01],
            [0x02],
            [0x03],
            new TestProtectionContext(10, MicKey));
        var provider = new SpnegoServerAuthenticationProvider(
            kerberos,
            ntlm,
            allowNtlmFallback: true);
        var acceptor =
            (SpnegoServerAuthenticationProvider.Acceptor)provider.CreateAcceptor();
        string[] mechanisms = [SpnegoOids.Ntlmssp];
        byte[] mechList = SpnegoEncoder.EncodeMechTypeList(mechanisms);
        byte[] clientMic = new TestMicProvider(MicKey).GetMic(mechList);

        _ = acceptor.AcceptToken(
            EncodeInit(mechanisms, [0x01]),
            OpcProtectionLevel.Integrity);
        RpcServerAuthenticationTokenResult completed = acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.Ntlmssp,
                    new byte[] { 0x03 },
                    clientMic)),
            OpcProtectionLevel.Integrity,
            isFinalLeg: true,
            CancellationToken.None);

        await Assert.That(completed.Session).IsNotNull();
        await Assert.That(completed.ResponseToken.IsEmpty).IsTrue();
        await Assert.That(acceptor.NegotiationState)
            .IsEqualTo(SpnegoNegState.AcceptCompleted);
    }

    [Test]
    public async Task Ntlm_fallback_verifies_and_generates_directional_mech_list_mic()
    {
        var client = CreateNtlmClient();
        var source = new ConfiguredAuthenticationSource(
            NtlmUser,
            NtlmPassword,
            NtlmDomain);
        var provider = new SpnegoServerAuthenticationProvider(
            new SpnegoServerOptions(
                kerberosProvider: null,
                source,
                SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable));
        IRpcServerAuthenticationAcceptor acceptor = provider.CreateAcceptor();
        string[] mechanisms = ["1.2.3", SpnegoOids.Ntlmssp];
        byte[] mechList = SpnegoEncoder.EncodeMechTypeList(mechanisms);

        RpcServerAuthenticationTokenResult selection = acceptor.AcceptToken(
            EncodeInit(mechanisms, [0x7F]),
            OpcProtectionLevel.Integrity);
        Type1Message type1 = client.CreateType1();
        RpcServerAuthenticationTokenResult challenge = acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.Ntlmssp,
                    type1.ToByteArray(),
                    null)),
            OpcProtectionLevel.Integrity);
        var type2 = new Type2Message(
            SpnegoDecoder.DecodeNegTokenResp(challenge.ResponseToken)
                .ResponseToken.GetValueOrDefault().ToArray());
        Type3Message type3 = client.CreateType3(type2);
        byte[] exportedSessionKey = client.EstablishedSessionKey
            .GetValueOrDefault().ToArray();
        var clientMic = new NtlmMicProvider(
            DeriveNtlmSigningKey(
                exportedSessionKey,
                "session key to client-to-server signing key magic constant"));
        RpcServerAuthenticationTokenResult completed = acceptor.AcceptToken(
            SpnegoEncoder.EncodeNegTokenResp(
                new SpnegoNegTokenResp(
                    SpnegoNegState.AcceptIncomplete,
                    SpnegoOids.Ntlmssp,
                    type3.ToByteArray(),
                    clientMic.GetMic(mechList))),
            OpcProtectionLevel.Integrity);
        SpnegoNegTokenResp completedResponse =
            SpnegoDecoder.DecodeNegTokenResp(completed.ResponseToken);
        var serverMic = new NtlmMicProvider(
            DeriveNtlmSigningKey(
                exportedSessionKey,
                "session key to server-to-client signing key magic constant"));

        await Assert.That(
            SpnegoDecoder.DecodeNegTokenResp(selection.ResponseToken).SupportedMech)
            .IsEqualTo(SpnegoOids.Ntlmssp);
        await Assert.That(completed.Session).IsNotNull();
        await Assert.That(completed.Session!.Principal.Identity!.Name)
            .IsEqualTo($"{NtlmDomain}\\{NtlmUser}");
        await Assert.That(completedResponse.VerifyMechListMic(mechList, serverMic))
            .IsTrue();
    }

    [Test]
    public async Task Kerberos_only_mode_rejects_ntlm_and_unsupported_mechanisms()
    {
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x01],
            [],
            new TestProtectionContext(
                KerberosServerAuthenticationProvider.KerberosAuthenticationService,
                MicKey));
        var provider = new SpnegoServerAuthenticationProvider(kerberos);

        Exception? ntlm = CaptureException(() => provider.CreateAcceptor().AcceptToken(
            EncodeInit([SpnegoOids.Ntlmssp], [0x01]),
            OpcProtectionLevel.Integrity));
        Exception? unsupported = CaptureException(() => provider.CreateAcceptor().AcceptToken(
            EncodeInit(["1.2.3.4"], [0x01]),
            OpcProtectionLevel.Integrity));

        await Assert.That(ntlm).IsTypeOf<SecurityException>();
        await Assert.That(unsupported).IsTypeOf<SecurityException>();
        await Assert.That(kerberos.AcceptCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Kerberos_failure_never_falls_back_to_ntlm()
    {
        var kerberos = FakeAuthenticationProvider.Rejects(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService);
        var ntlm = FakeAuthenticationProvider.Completes(
            10,
            [0x02],
            [],
            new TestProtectionContext(10, MicKey));
        var provider = new SpnegoServerAuthenticationProvider(
            kerberos,
            ntlm,
            allowNtlmFallback: true);

        Exception? thrown = CaptureException(() => provider.CreateAcceptor().AcceptToken(
            EncodeInit(
                [SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp],
                [0x01]),
            OpcProtectionLevel.Integrity));

        await Assert.That(thrown).IsTypeOf<SecurityException>();
        await Assert.That(kerberos.AcceptCalls).IsEqualTo(1);
        await Assert.That(ntlm.AcceptCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Malformed_asn1_and_pre_cancellation_fail_closed()
    {
        var kerberos = FakeAuthenticationProvider.Completes(
            KerberosServerAuthenticationProvider.KerberosAuthenticationService,
            [0x01],
            [],
            new TestProtectionContext(
                KerberosServerAuthenticationProvider.KerberosAuthenticationService,
                MicKey));
        var provider = new SpnegoServerAuthenticationProvider(kerberos);
        Exception? malformed = CaptureException(() => provider.CreateAcceptor().AcceptToken(
            new byte[] { 0x60, 0x01, 0x00 },
            OpcProtectionLevel.Integrity));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Exception? canceled = CaptureException(() => provider.CreateAcceptor().AcceptToken(
            EncodeInit([SpnegoOids.KerberosV5], [0x01]),
            OpcProtectionLevel.Integrity,
            cancellation.Token));

        await Assert.That(malformed).IsTypeOf<SecurityException>();
        await Assert.That(canceled).IsTypeOf<OperationCanceledException>();
        await Assert.That(kerberos.AcceptCalls).IsEqualTo(0);
    }

    private static byte[] EncodeInit(
        IReadOnlyList<string> mechanisms,
        byte[] optimisticToken) =>
        SpnegoEncoder.EncodeNegTokenInit(
            new SpnegoNegTokenInit(
                mechanisms,
                optimisticToken,
                null,
                SpnegoEncoder.EncodeMechTypeList(mechanisms)));

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

    private static NtlmAuthentication CreateNtlmClient()
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
        properties.SetProperty("rpc.ntlm.domain", NtlmDomain);
        properties.SetProperty(
            Opc.Classic.Dcom.Rpc.Security.USERNAME,
            NtlmUser);
        properties.SetProperty(
            Opc.Classic.Dcom.Rpc.Security.PASSWORD,
            NtlmPassword);
        return new NtlmAuthentication(properties);
    }

    private static byte[] DeriveNtlmSigningKey(
        ReadOnlySpan<byte> exportedSessionKey,
        string magicConstant)
    {
        byte[] magic = Encoding.ASCII.GetBytes(magicConstant + '\0');
        byte[] input = new byte[exportedSessionKey.Length + magic.Length];
        exportedSessionKey.CopyTo(input);
        magic.CopyTo(input.AsSpan(exportedSessionKey.Length));
#pragma warning disable CA5351 // NTLM extended-session signing keys are specified with MD5.
        return MD5.HashData(input);
#pragma warning restore CA5351
    }

    private sealed class FakeAuthenticationProvider : IRpcServerAuthenticationProvider
    {
        private readonly Func<IRpcServerAuthenticationAcceptor> _factory;
        private int _acceptCalls;

        private FakeAuthenticationProvider(
            int authenticationService,
            Func<IRpcServerAuthenticationAcceptor> factory)
        {
            AuthenticationService = authenticationService;
            _factory = factory;
        }

        public int AuthenticationService { get; }

        public int AcceptCalls => Volatile.Read(ref _acceptCalls);

        public IRpcServerAuthenticationAcceptor CreateAcceptor() =>
            new CountingAcceptor(_factory(), () => Interlocked.Increment(ref _acceptCalls));

        public static FakeAuthenticationProvider Completes(
            int authenticationService,
            byte[] expectedToken,
            byte[] responseToken,
            TestProtectionContext protectionContext) =>
            new(
                authenticationService,
                () => new ScriptedAcceptor(
                    [
                        new Step(
                            expectedToken,
                            RpcServerAuthenticationTokenResult.Complete(
                                CreateSession(
                                    authenticationService,
                                    protectionContext),
                                responseToken)),
                    ]));

        public static FakeAuthenticationProvider ContinuesThenCompletes(
            int authenticationService,
            byte[] initialToken,
            byte[] challenge,
            byte[] responseToken,
            TestProtectionContext protectionContext) =>
            new(
                authenticationService,
                () => new ScriptedAcceptor(
                    [
                        new Step(
                            initialToken,
                            RpcServerAuthenticationTokenResult.Continue(challenge)),
                        new Step(
                            responseToken,
                            RpcServerAuthenticationTokenResult.Complete(
                                CreateSession(
                                    authenticationService,
                                    protectionContext))),
                    ]));

        public static FakeAuthenticationProvider Rejects(int authenticationService) =>
            new(authenticationService, static () => new RejectingAcceptor());

        private static RpcServerAuthenticationSession CreateSession(
            int authenticationService,
            TestProtectionContext protectionContext) =>
            new(
                authenticationService,
                new GenericPrincipal(
                    new GenericIdentity("test-user", "test"),
                    ["operator"]),
                OpcProtectionLevel.Integrity,
                protectionContext);

        private sealed class CountingAcceptor : IRpcServerAuthenticationAcceptor
        {
            private readonly IRpcServerAuthenticationAcceptor _inner;
            private readonly Action _onAccept;

            public CountingAcceptor(
                IRpcServerAuthenticationAcceptor inner,
                Action onAccept)
            {
                _inner = inner;
                _onAccept = onAccept;
            }

            public RpcServerAuthenticationTokenResult AcceptToken(
                ReadOnlyMemory<byte> token,
                OpcProtectionLevel protectionLevel)
            {
                _onAccept();
                return _inner.AcceptToken(token, protectionLevel);
            }

            public RpcServerAuthenticationTokenResult AcceptToken(
                ReadOnlyMemory<byte> token,
                OpcProtectionLevel protectionLevel,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _onAccept();
                return _inner.AcceptToken(token, protectionLevel, cancellationToken);
            }
        }

        private sealed class ScriptedAcceptor : IRpcServerAuthenticationAcceptor
        {
            private readonly Queue<Step> _steps;

            public ScriptedAcceptor(IEnumerable<Step> steps) =>
                _steps = new Queue<Step>(steps);

            public RpcServerAuthenticationTokenResult AcceptToken(
                ReadOnlyMemory<byte> token,
                OpcProtectionLevel protectionLevel)
            {
                if (!_steps.TryDequeue(out Step? step)
                    || !token.Span.SequenceEqual(step.ExpectedToken.Span))
                {
                    throw new SecurityException(
                        "The fake mechanism received an unexpected token.");
                }

                return step.Result;
            }
        }

        private sealed class RejectingAcceptor : IRpcServerAuthenticationAcceptor
        {
            public RpcServerAuthenticationTokenResult AcceptToken(
                ReadOnlyMemory<byte> token,
                OpcProtectionLevel protectionLevel) =>
                throw new SecurityException("The fake mechanism rejected authentication.");
        }

        private sealed record Step(
            ReadOnlyMemory<byte> ExpectedToken,
            RpcServerAuthenticationTokenResult Result);
    }

    private sealed class TestProtectionContext :
        IRpcServerProtectionContext,
        IGssMicProvider
    {
        private readonly TestMicProvider _micProvider;

        public TestProtectionContext(int authenticationService, ReadOnlySpan<byte> micKey)
        {
            AuthenticationService = authenticationService;
            _micProvider = new TestMicProvider(micKey);
        }

        public int AuthenticationService { get; }

        public OpcProtectionLevel ProtectionLevel => OpcProtectionLevel.Integrity;

        public int VerifierLength => 32;

        public int ProtectCalls { get; private set; }

        public int UnprotectCalls { get; private set; }

        public void Protect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            out byte[] verifier)
        {
            ProtectCalls++;
            verifier = SHA256.HashData(signedRegion);
        }

        public bool Unprotect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            ReadOnlyMemory<byte> verifier)
        {
            UnprotectCalls++;
            return CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(signedRegion),
                verifier.Span);
        }

        public byte[] GetMic(ReadOnlySpan<byte> data) =>
            _micProvider.GetMic(data);

        public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic) =>
            _micProvider.VerifyMic(data, mic);
    }

    private sealed class TestMicProvider : IGssMicProvider
    {
        private readonly byte[] _key;

        public TestMicProvider(ReadOnlySpan<byte> key) =>
            _key = key.ToArray();

        public byte[] GetMic(ReadOnlySpan<byte> data) =>
            HMACSHA256.HashData(_key, data);

        public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic) =>
            CryptographicOperations.FixedTimeEquals(GetMic(data), mic);
    }
}
