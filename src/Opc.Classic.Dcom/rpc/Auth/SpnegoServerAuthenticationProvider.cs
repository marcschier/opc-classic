// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Formats.Asn1;
using System.Security;
using System.Security.Cryptography;
using Opc.Classic.Dcom.Kerberos.Spnego;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Negotiates Kerberos or policy-controlled NTLMSSP for RPC_C_AUTHN_GSS_NEGOTIATE.
/// </summary>
public sealed class SpnegoServerAuthenticationProvider : IRpcServerAuthenticationProvider
{
    /// <summary>
    /// RPC_C_AUTHN_GSS_NEGOTIATE.
    /// </summary>
    public const int SpnegoAuthenticationService = 9;

    /// <summary>
    /// Initializes a provider from validated SPNEGO policy.
    /// </summary>
    public SpnegoServerAuthenticationProvider(SpnegoServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
    }

    /// <summary>
    /// Initializes a provider with a direct Kerberos provider and optional NTLM fallback.
    /// </summary>
    public SpnegoServerAuthenticationProvider(
        IRpcServerAuthenticationProvider kerberosProvider,
        IRpcServerAuthenticationProvider? ntlmProvider = null,
        bool allowNtlmFallback = false)
        : this(new SpnegoServerOptions(
            kerberosProvider,
            ntlmProvider,
            allowNtlmFallback
                ? SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable
                : SpnegoNtlmFallbackPolicy.Disabled))
    {
    }

    /// <summary>
    /// Gets the validated negotiation policy.
    /// </summary>
    public SpnegoServerOptions Options { get; }

    /// <inheritdoc />
    public int AuthenticationService => SpnegoAuthenticationService;

    /// <inheritdoc />
    public IRpcServerAuthenticationAcceptor CreateAcceptor() =>
        new Acceptor(Options);

    /// <summary>
    /// Per-connection RFC 4178 negotiation state.
    /// </summary>
    public sealed class Acceptor : IRpcServerAuthenticationAcceptor
    {
        private readonly SpnegoServerOptions _options;
        private IRpcServerAuthenticationAcceptor? _innerAcceptor;
        private RpcServerAuthenticationSession? _pendingSession;
        private ReadOnlyMemory<byte>? _pendingPeerMic;
        private byte[]? _mechListBytes;
        private string? _selectedMechanismOid;
        private bool _peerMicRequired;
        private bool _established;
        private bool _receivedInitialToken;

        internal Acceptor(SpnegoServerOptions options) =>
            _options = options;

        /// <summary>
        /// Gets the selected mechanism, or <see langword="null"/> before selection.
        /// </summary>
        public SpnegoMech? SelectedMechanism => _selectedMechanismOid switch
        {
            SpnegoOids.KerberosV5 => SpnegoMech.KerberosV5,
            SpnegoOids.Ntlmssp => SpnegoMech.Ntlmssp,
            _ => null,
        };

        /// <summary>
        /// Gets the selected mechanism OID, or <see langword="null"/> before selection.
        /// </summary>
        public string? SelectedMechanismOid => _selectedMechanismOid;

        /// <summary>
        /// Gets the latest negotiation state emitted by the acceptor.
        /// </summary>
        public SpnegoNegState? NegotiationState { get; private set; }

        /// <inheritdoc />
        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel) =>
            AcceptToken(token, protectionLevel, isFinalLeg: false, CancellationToken.None);

        /// <inheritdoc />
        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            CancellationToken cancellationToken) =>
            AcceptToken(token, protectionLevel, isFinalLeg: false, cancellationToken);

        /// <inheritdoc />
        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            bool isFinalLeg,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_established)
            {
                throw Reject("the connection authentication context is already established");
            }
            if (token.IsEmpty)
            {
                throw Reject("the negotiation token is empty");
            }

            try
            {
                return !_receivedInitialToken
                    ? AcceptInitialToken(token, protectionLevel, isFinalLeg, cancellationToken)
                    : AcceptResponseToken(token, protectionLevel, isFinalLeg, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SecurityException)
            {
                NegotiationState = SpnegoNegState.Reject;
                throw;
            }
            catch (Exception exception) when (
                exception is ArgumentException
                    or AsnContentException
                    or CryptographicException
                    or InvalidOperationException)
            {
                throw Reject("the negotiation token failed protocol validation", exception);
            }
        }

        private RpcServerAuthenticationTokenResult AcceptInitialToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            bool isFinalLeg,
            CancellationToken cancellationToken)
        {
            SpnegoNegTokenInit init = SpnegoDecoder.DecodeNegTokenInit(token);
            ValidateMechanismList(init);
            _receivedInitialToken = true;
            _mechListBytes = init.MechListBytes.ToArray();
            _pendingPeerMic = init.MechListMic;

            int selectedIndex = SelectMechanism(init.MechTypes, out IRpcServerAuthenticationProvider provider);
            _selectedMechanismOid = init.MechTypes[selectedIndex];
            _peerMicRequired = selectedIndex != 0
                || StringComparer.Ordinal.Equals(_selectedMechanismOid, SpnegoOids.Ntlmssp)
                    && _options.KerberosProvider is not null;
            _innerAcceptor = provider.CreateAcceptor();

            if (selectedIndex != 0 || init.MechToken.IsEmpty)
            {
                return Continue(SpnegoNegState.AcceptIncomplete, null, null);
            }

            RpcServerAuthenticationTokenResult inner = _innerAcceptor.AcceptToken(
                init.MechToken,
                protectionLevel,
                cancellationToken);
            return ProcessInnerResult(inner, isFinalLeg);
        }

        private RpcServerAuthenticationTokenResult AcceptResponseToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            bool isFinalLeg,
            CancellationToken cancellationToken)
        {
            SpnegoNegTokenResp response = SpnegoDecoder.DecodeNegTokenResp(token);
            if (response.NegState.HasValue
                && !Enum.IsDefined(response.NegState.GetValueOrDefault()))
            {
                throw Reject("the initiator supplied an invalid negotiation state");
            }
            if (response.NegState == SpnegoNegState.Reject)
            {
                throw Reject("the initiator rejected negotiation");
            }
            if (response.SupportedMech is not null
                && !StringComparer.Ordinal.Equals(response.SupportedMech, _selectedMechanismOid))
            {
                throw Reject("the initiator changed the selected mechanism");
            }
            if (response.MechListMic.HasValue)
            {
                if (_pendingPeerMic.HasValue)
                {
                    throw Reject("the initiator supplied more than one mechListMIC");
                }

                _pendingPeerMic = response.MechListMic;
            }

            if (_pendingSession is not null)
            {
                if (response.ResponseToken is { IsEmpty: false })
                {
                    throw Reject("a mechanism token was supplied after the mechanism completed");
                }

                return CompletePendingSession(isFinalLeg);
            }

            if (response.ResponseToken is not { IsEmpty: false } mechanismToken)
            {
                throw Reject("the selected mechanism requires a response token");
            }

            RpcServerAuthenticationTokenResult inner = _innerAcceptor!.AcceptToken(
                mechanismToken,
                protectionLevel,
                cancellationToken);
            return ProcessInnerResult(inner, isFinalLeg);
        }

        private RpcServerAuthenticationTokenResult ProcessInnerResult(
            RpcServerAuthenticationTokenResult inner,
            bool isFinalLeg)
        {
            if (inner.Session is null)
            {
                return Continue(
                    SpnegoNegState.AcceptIncomplete,
                    inner.ResponseToken,
                    null);
            }

            RpcServerAuthenticationSession session = WrapSession(inner.Session);
            IGssMicProvider? micProvider = GetMicProvider(inner.Session);
            if (_pendingPeerMic.HasValue)
            {
                if (micProvider is null
                    || !micProvider.VerifyMic(
                        _mechListBytes!,
                        _pendingPeerMic.Value.Span))
                {
                    throw Reject("mechListMIC verification failed");
                }
            }
            else if (_peerMicRequired)
            {
                if (micProvider is null)
                {
                    throw Reject(
                        "the selected mechanism cannot verify the required mechListMIC");
                }
                if (isFinalLeg)
                {
                    throw Reject("the required mechListMIC is absent from the final RPC authentication leg");
                }

                _pendingSession = session;
                byte[] responseMic = micProvider.GetMic(_mechListBytes!);
                return Continue(
                    SpnegoNegState.RequestMic,
                    inner.ResponseToken,
                    responseMic);
            }

            if (isFinalLeg
                && StringComparer.Ordinal.Equals(_selectedMechanismOid, SpnegoOids.Ntlmssp))
            {
                return CompleteFinalNtlm(session, inner.ResponseToken);
            }

            byte[]? mechListMic = null;
            if (ShouldGenerateMic())
            {
                mechListMic = (micProvider
                    ?? throw Reject(
                        "the selected mechanism cannot generate mechListMIC"))
                    .GetMic(_mechListBytes!);
            }
            return Complete(session, inner.ResponseToken, mechListMic, emitResponseToken: true);
        }

        private RpcServerAuthenticationTokenResult CompleteFinalNtlm(
            RpcServerAuthenticationSession session,
            ReadOnlyMemory<byte> innerResponseToken)
        {
            if (!innerResponseToken.IsEmpty)
            {
                throw Reject(
                    "NTLM completed with an unexpected mechanism response token");
            }

            return Complete(session, null, null, emitResponseToken: false);
        }

        private RpcServerAuthenticationTokenResult CompletePendingSession(bool isFinalLeg)
        {
            if (!_pendingPeerMic.HasValue)
            {
                throw Reject("the required mechListMIC is absent");
            }

            RpcServerAuthenticationSession session = _pendingSession!;
            IGssMicProvider? micProvider = GetMicProvider(session);
            if (micProvider is null
                || !micProvider.VerifyMic(_mechListBytes!, _pendingPeerMic.Value.Span))
            {
                throw Reject("mechListMIC verification failed");
            }

            bool emitResponseToken = !isFinalLeg
                || !StringComparer.Ordinal.Equals(_selectedMechanismOid, SpnegoOids.Ntlmssp);
            return Complete(session, null, null, emitResponseToken);
        }

        private RpcServerAuthenticationTokenResult Continue(
            SpnegoNegState state,
            ReadOnlyMemory<byte>? responseToken,
            ReadOnlyMemory<byte>? mechListMic)
        {
            NegotiationState = state;
            return RpcServerAuthenticationTokenResult.Continue(
                SpnegoEncoder.EncodeNegTokenResp(
                    new SpnegoNegTokenResp(
                        state,
                        _selectedMechanismOid,
                        responseToken,
                        mechListMic)));
        }

        private RpcServerAuthenticationTokenResult Complete(
            RpcServerAuthenticationSession session,
            ReadOnlyMemory<byte>? responseToken,
            ReadOnlyMemory<byte>? mechListMic,
            bool emitResponseToken)
        {
            NegotiationState = SpnegoNegState.AcceptCompleted;
            _established = true;
            return emitResponseToken
                ? RpcServerAuthenticationTokenResult.Complete(
                    session,
                    SpnegoEncoder.EncodeNegTokenResp(
                        new SpnegoNegTokenResp(
                            SpnegoNegState.AcceptCompleted,
                            _selectedMechanismOid,
                            responseToken,
                            mechListMic)))
                : RpcServerAuthenticationTokenResult.Complete(session);
        }

        private int SelectMechanism(
            IReadOnlyList<string> offeredMechanisms,
            out IRpcServerAuthenticationProvider provider)
        {
            int kerberosIndex = IndexOf(offeredMechanisms, SpnegoOids.KerberosV5);
            if (kerberosIndex >= 0 && _options.KerberosProvider is not null)
            {
                provider = _options.KerberosProvider;
                return kerberosIndex;
            }

            int ntlmIndex = IndexOf(offeredMechanisms, SpnegoOids.Ntlmssp);
            if (ntlmIndex >= 0
                && _options.NtlmFallbackPolicy
                    == SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable
                && _options.NtlmProvider is not null)
            {
                provider = _options.NtlmProvider;
                return ntlmIndex;
            }

            throw Reject("no offered mechanism is permitted by server policy");
        }

        private static void ValidateMechanismList(SpnegoNegTokenInit init)
        {
            if (init.MechTypes.Count == 0 || init.MechListBytes.IsEmpty)
            {
                throw new SecurityException(
                    "SPNEGO authentication rejected: the mechanism list is absent.");
            }
            if (init.MechTypes.Any(string.IsNullOrWhiteSpace)
                || init.MechTypes.Distinct(StringComparer.Ordinal).Count()
                    != init.MechTypes.Count)
            {
                throw new SecurityException(
                    "SPNEGO authentication rejected: the mechanism list is invalid.");
            }
        }

        private RpcServerAuthenticationSession WrapSession(
            RpcServerAuthenticationSession inner)
        {
            IRpcServerProtectionContext? protectionContext =
                inner.ProtectionContext is null
                    ? null
                    : new SpnegoProtectionContext(inner.ProtectionContext);
            return new RpcServerAuthenticationSession(
                SpnegoServerAuthenticationProvider.SpnegoAuthenticationService,
                inner.Principal,
                inner.ProtectionLevel,
                protectionContext);
        }

        private bool ShouldGenerateMic() =>
            _mechListBytes is not null
            && (_peerMicRequired || _pendingPeerMic.HasValue || CountMechanisms(_mechListBytes) > 1);

        private static int CountMechanisms(ReadOnlyMemory<byte> mechListBytes)
        {
            var reader = new AsnReader(mechListBytes, AsnEncodingRules.DER);
            AsnReader sequence = reader.ReadSequence();
            int count = 0;
            while (sequence.HasData)
            {
                _ = sequence.ReadObjectIdentifier();
                count++;
            }

            reader.ThrowIfNotEmpty();
            return count;
        }

        private static IGssMicProvider? GetMicProvider(
            RpcServerAuthenticationSession session) =>
            session.ProtectionContext as IGssMicProvider;

        private static int IndexOf(IReadOnlyList<string> values, string value)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (StringComparer.Ordinal.Equals(values[index], value))
                {
                    return index;
                }
            }

            return -1;
        }

        private SecurityException Reject(string reason, Exception? innerException = null)
        {
            NegotiationState = SpnegoNegState.Reject;
            return new SecurityException(
                $"SPNEGO authentication rejected: {reason}.",
                innerException);
        }

        private sealed class SpnegoProtectionContext :
            IRpcServerProtectionContext,
            IGssMicProvider
        {
            private readonly IRpcServerProtectionContext _inner;
            private readonly IGssMicProvider? _micProvider;

            public SpnegoProtectionContext(IRpcServerProtectionContext inner)
            {
                _inner = inner;
                _micProvider = inner as IGssMicProvider;
            }

            public int AuthenticationService =>
                SpnegoServerAuthenticationProvider.SpnegoAuthenticationService;

            public OpcProtectionLevel ProtectionLevel => _inner.ProtectionLevel;

            public int VerifierLength => _inner.VerifierLength;

            public int GetVerifierLength(int signedRegionLength, int confidentialLength) =>
                _inner.GetVerifierLength(signedRegionLength, confidentialLength);

            public void Protect(
                Span<byte> signedRegion,
                int confidentialOffset,
                int confidentialLength,
                out byte[] verifier) =>
                _inner.Protect(
                    signedRegion,
                    confidentialOffset,
                    confidentialLength,
                    out verifier);

            public bool Unprotect(
                Span<byte> signedRegion,
                int confidentialOffset,
                int confidentialLength,
                ReadOnlyMemory<byte> verifier) =>
                _inner.Unprotect(
                    signedRegion,
                    confidentialOffset,
                    confidentialLength,
                    verifier);

            public byte[] GetMic(ReadOnlySpan<byte> data) =>
                (_micProvider
                    ?? throw new InvalidOperationException(
                        "The selected mechanism does not expose GSS MIC services."))
                .GetMic(data);

            public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic) =>
                _micProvider?.VerifyMic(data, mic) == true;
        }
    }
}
