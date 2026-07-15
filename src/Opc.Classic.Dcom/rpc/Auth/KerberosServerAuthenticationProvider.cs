// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using Kerberos.NET;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Kerberos;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Validates direct Kerberos AP-REQ tokens and establishes RPC packet protection.
/// </summary>
public sealed class KerberosServerAuthenticationProvider : IRpcServerAuthenticationProvider
{
    private static readonly Action<ILogger, string, string, EncryptionType, OpcProtectionLevel, bool, Exception?>
        AuthenticationAccepted = LoggerMessage.Define<string, string, EncryptionType, OpcProtectionLevel, bool>(
            LogLevel.Information,
            new EventId(100, nameof(AuthenticationAccepted)),
            "Kerberos RPC authentication accepted principal {Principal} for {ServicePrincipal} using {EncryptionType}, protection {ProtectionLevel}, mutual {MutualAuthentication}");

    private static readonly Action<ILogger, string, Exception?> AuthenticationRejected =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(101, nameof(AuthenticationRejected)),
            "Kerberos RPC authentication rejected: {Reason}");

    private readonly ConcurrentDictionary<string, DateTimeOffset> _replayCache =
        new(StringComparer.Ordinal);
    private readonly ILogger _logger;

    /// <summary>
    /// RPC_C_AUTHN_GSS_KERBEROS.
    /// </summary>
    public const int KerberosAuthenticationService = 16;

    /// <summary>
    /// Initializes a Kerberos provider registration.
    /// </summary>
    public KerberosServerAuthenticationProvider(
        KerberosServerOptions options,
        ILogger<KerberosServerAuthenticationProvider>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
        _logger = logger ?? NullLogger<KerberosServerAuthenticationProvider>.Instance;
    }

    /// <summary>
    /// Gets the validated Kerberos policy.
    /// </summary>
    public KerberosServerOptions Options { get; }

    /// <inheritdoc />
    public int AuthenticationService => KerberosAuthenticationService;

    /// <inheritdoc />
    public IRpcServerAuthenticationAcceptor CreateAcceptor() =>
        new KerberosAcceptor(Options, _replayCache, _logger);

    private sealed class KerberosAcceptor : IRpcServerAuthenticationAcceptor
    {
        private const byte GssInitialContextTokenTag = 0x60;
        private const byte KerberosApRequestApplicationTag = 0x6e;
        private const byte GssApRequestTokenId0 = 0x01;
        private const byte GssApRequestTokenId1 = 0x00;
        private const byte SentByAcceptorFlag = 0x01;
        private const byte AcceptorSubkeyFlag = 0x04;
        private const int Rfc4121HeaderLength = 16;

        private readonly KerberosServerOptions _options;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _replayCache;
        private readonly ILogger _logger;
        private bool _established;

        public KerberosAcceptor(
            KerberosServerOptions options,
            ConcurrentDictionary<string, DateTimeOffset> replayCache,
            ILogger logger)
        {
            _options = options;
            _replayCache = replayCache;
            _logger = logger;
        }

        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel) =>
            AcceptToken(token, protectionLevel, CancellationToken.None);

        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_established)
            {
                throw Reject("the connection authentication context is already established");
            }
            if (token.IsEmpty)
            {
                throw Reject("the AP-REQ token is empty");
            }
            if (protectionLevel < _options.MinimumProtectionLevel
                || protectionLevel > OpcProtectionLevel.Privacy)
            {
                throw Reject("the requested RPC protection level is not permitted");
            }

            try
            {
                return ValidateAndEstablish(
                    token,
                    protectionLevel,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is ArgumentException
                    or CryptographicException
                    or InvalidOperationException
                    or KerberosProtocolException
                    or KerberosValidationException)
            {
                throw Reject("the AP-REQ failed cryptographic or protocol validation", exception);
            }
        }

        private RpcServerAuthenticationTokenResult ValidateAndEstablish(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel,
            CancellationToken cancellationToken)
        {
            KrbApReq apRequest = DecodeApRequest(token);
            ValidateServiceAndEncryptionPolicy(apRequest);
            cancellationToken.ThrowIfCancellationRequested();

            using KerberosServerCredential credential =
                _options.CredentialProvider.AcquireCredential();
            KeyTable keyTable = CreateKeyTable(credential);
            var decrypted = new DecryptedKrbApReq(apRequest, MessageType.KRB_AP_REQ);
            decrypted.Decrypt(keyTable);
            decrypted.Validate(
                ValidationActions.ClientPrincipalIdentifier
                | ValidationActions.Realm);
            ValidateSessionEncryptionPolicy(decrypted);

            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset now = _options.TimeProvider.GetUtcNow();
            ValidateTimes(decrypted, now);
            DelegationInfo checksum =
                ValidateAuthenticatorChecksum(decrypted.Authenticator);
            ValidateChannelBinding(checksum);
            ValidateReplay(decrypted, now);
            return EstablishSession(
                apRequest,
                decrypted,
                protectionLevel,
                cancellationToken);
        }

        private RpcServerAuthenticationTokenResult EstablishSession(
            KrbApReq apRequest,
            DecryptedKrbApReq decrypted,
            OpcProtectionLevel protectionLevel,
            CancellationToken cancellationToken)
        {
            string clientPrincipal = GetClientPrincipal(decrypted);
            if (!_options.PrincipalMapper.TryMapPrincipal(
                    clientPrincipal,
                    out IPrincipal? mappedPrincipal))
            {
                throw Reject("the authenticated principal is not authorized");
            }

            bool mutualAuthentication =
                (decrypted.Options & ApOptions.MutualRequired)
                == ApOptions.MutualRequired;
            KerberosKey contextKey = decrypted.SessionKey;
            bool usesAcceptorSubkey = false;
            byte[] responseToken = [];
            if (mutualAuthentication)
            {
                KrbApRep apReply = decrypted.CreateResponseMessage();
                responseToken = apReply.EncodeApplication().ToArray();
                var decryptedReply = new DecryptedKrbApRep(apReply);
                decryptedReply.Decrypt(decrypted.SessionKey);
                if (decryptedReply.Response.SubSessionKey is not null)
                {
                    contextKey = decryptedReply.Response.SubSessionKey.AsKey();
                    usesAcceptorSubkey = true;
                }
            }
            if (!_options.AllowedEncryptionTypes.Contains(
                    contextKey.EncryptionType))
            {
                throw Reject(
                    "the negotiated context key uses an encryption type prohibited by policy");
            }

            cancellationToken.ThrowIfCancellationRequested();
            RpcServerAuthenticationSession rpcSession = CreateRpcSession(
                contextKey,
                mappedPrincipal,
                protectionLevel,
                usesAcceptorSubkey);
            _established = true;
            AuthenticationAccepted(
                _logger,
                clientPrincipal,
                GetServicePrincipal(apRequest),
                contextKey.EncryptionType,
                protectionLevel,
                mutualAuthentication,
                null);
            return RpcServerAuthenticationTokenResult.Complete(
                rpcSession,
                responseToken);
        }

        private static RpcServerAuthenticationSession CreateRpcSession(
            KerberosKey contextKey,
            IPrincipal mappedPrincipal,
            OpcProtectionLevel protectionLevel,
            bool usesAcceptorSubkey)
        {
            var session = new KerberosSession(
                contextKey,
                contextKey.EncryptionType,
                isAcceptor: true,
                usesAcceptorSubkey: usesAcceptorSubkey);
            var protectionContext = new KerberosServerProtectionContext(
                session,
                protectionLevel,
                usesAcceptorSubkey);
            return new RpcServerAuthenticationSession(
                KerberosAuthenticationService,
                mappedPrincipal,
                protectionLevel,
                protectionContext);
        }

        private KrbApReq DecodeApRequest(ReadOnlyMemory<byte> token)
        {
            ReadOnlyMemory<byte> applicationToken = token;
            ReadOnlySpan<byte> span = applicationToken.Span;
            if (span[0] == GssInitialContextTokenTag)
            {
                applicationToken = GssApiToken.Decode(applicationToken).Token;
                span = applicationToken.Span;
            }
            if (span.Length >= 2
                && span[0] == GssApRequestTokenId0
                && span[1] == GssApRequestTokenId1)
            {
                applicationToken = applicationToken[2..];
                span = applicationToken.Span;
            }
            if (span.IsEmpty || span[0] != KerberosApRequestApplicationTag)
            {
                throw Reject("the token is not a direct Kerberos AP-REQ");
            }

            return KrbApReq.DecodeApplication(applicationToken);
        }

        private void ValidateServiceAndEncryptionPolicy(KrbApReq apRequest)
        {
            string servicePrincipal = GetServicePrincipal(apRequest);
            if (!string.Equals(
                    apRequest.Ticket.Realm,
                    _options.Realm,
                    StringComparison.OrdinalIgnoreCase)
                || !_options.ServicePrincipals.Contains(
                    servicePrincipal,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw Reject("the ticket service principal is not configured");
            }

            if (!_options.AllowedEncryptionTypes.Contains(apRequest.Ticket.EncryptedPart.EType)
                || !_options.AllowedEncryptionTypes.Contains(apRequest.Authenticator.EType))
            {
                throw Reject("the AP-REQ uses an encryption type prohibited by policy");
            }
        }

        private void ValidateTimes(DecryptedKrbApReq request, DateTimeOffset now)
        {
            DateTimeOffset startTime = request.Ticket.StartTime ?? request.Ticket.AuthTime;
            if ((request.Ticket.Flags & TicketFlags.Invalid) == TicketFlags.Invalid
                || now + _options.ClockSkew < startTime
                || now - _options.ClockSkew > request.Ticket.EndTime)
            {
                throw Reject("the service ticket is not valid at the current time");
            }

            DateTimeOffset authenticatorTime =
                request.Authenticator.CTime.AddTicks(request.Authenticator.CuSec * 10L);
            if (request.Authenticator.CuSec is < 0 or > 999999
                || (now - authenticatorTime).Duration() > _options.ClockSkew)
            {
                throw Reject("the authenticator timestamp is outside the permitted clock skew");
            }
        }

        private void ValidateSessionEncryptionPolicy(DecryptedKrbApReq request)
        {
            if (!_options.AllowedEncryptionTypes.Contains(
                    request.SessionKey.EncryptionType)
                || !_options.AllowedEncryptionTypes.Contains(
                    request.Ticket.Key.EType)
                || request.Authenticator.Subkey is not null
                && !_options.AllowedEncryptionTypes.Contains(
                    request.Authenticator.Subkey.EType))
            {
                throw Reject(
                    "the decrypted Kerberos context uses an encryption type prohibited by policy");
            }
        }

        private DelegationInfo ValidateAuthenticatorChecksum(KrbAuthenticator authenticator)
        {
            KrbChecksum? checksum = authenticator.Checksum;
            if (checksum is null
                || (int)checksum.Type != KerberosChannelBindingChecksum.KrbApChecksumTypeGss)
            {
                throw Reject("the authenticator is missing the required GSS checksum");
            }

            DelegationInfo delegation = checksum.DecodeDelegation();
            if (delegation.Flags == GssContextEstablishmentFlag.GSS_C_NONE)
            {
                throw Reject("the authenticator GSS flags are invalid");
            }

            return delegation;
        }

        private void ValidateChannelBinding(DelegationInfo checksum)
        {
            ReadOnlySpan<byte> supplied = checksum.ChannelBinding.Span;
            bool suppliedBinding = supplied.Length != 0 && !IsAllZero(supplied);
            if (supplied.Length != 0 && supplied.Length != 16)
            {
                throw Reject("the authenticator channel-binding checksum has an invalid length");
            }

            ReadOnlyMemory<byte>? expectedMemory = _options.ChannelBindingsHash;
            if (!suppliedBinding)
            {
                if (_options.ChannelBindingPolicy == KerberosChannelBindingPolicy.Required)
                {
                    throw Reject("the required channel binding is absent");
                }

                return;
            }

            if (!expectedMemory.HasValue
                || !CryptographicOperations.FixedTimeEquals(
                    supplied,
                    expectedMemory.Value.Span))
            {
                throw Reject("the channel-binding checksum does not match this transport");
            }
        }

        private void ValidateReplay(DecryptedKrbApReq request, DateTimeOffset now)
        {
            foreach ((string key, DateTimeOffset expires) in _replayCache)
            {
                if (expires < now)
                {
                    _replayCache.TryRemove(key, out _);
                }
            }

            string replayKey = string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"{GetClientPrincipal(request)}|{request.Authenticator.CTime.UtcTicks}|{request.Authenticator.CuSec}|{GetServicePrincipal(request.EncryptedTicket)}");
            DateTimeOffset expiresAt = request.Ticket.EndTime + _options.ClockSkew;
            if (!_replayCache.TryAdd(replayKey, expiresAt))
            {
                throw Reject("the authenticator was replayed");
            }
        }

        private KeyTable CreateKeyTable(KerberosServerCredential credential)
        {
            if (credential is KerberosKeytabCredential keytabCredential)
            {
                byte[] keytab = new byte[keytabCredential.SecretLength];
                try
                {
                    keytabCredential.CopyKeytabTo(keytab);
                    return new KeyTable(keytab);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(keytab);
                }
            }

            if (credential is KerberosPasswordCredential passwordCredential)
            {
                char[] password = new char[passwordCredential.SecretLength];
                try
                {
                    passwordCredential.CopyPasswordTo(password);
                    var principal = new PrincipalName(
                        PrincipalNameType.NT_SRV_INST,
                        credential.Realm,
                        credential.Principal.Split('/'));
                    string passwordValue = new(password);
                    KerberosKey[] keys = _options.AllowedEncryptionTypes
                        .Select(etype => new KerberosKey(
                            passwordValue,
                            principalName: principal,
                            etype: etype))
                        .ToArray();
                    return new KeyTable(keys);
                }
                finally
                {
                    Array.Clear(password);
                }
            }

            throw new NotSupportedException(
                $"Kerberos credential kind {credential.Kind} is not supported.");
        }

        private static string GetClientPrincipal(DecryptedKrbApReq request) =>
            $"{request.Ticket.CName.FullyQualifiedName}@{request.Ticket.CRealm}";

        private static string GetServicePrincipal(KrbApReq request) =>
            request.Ticket.SName.FullyQualifiedName;

        private static string GetServicePrincipal(KrbTicket ticket) =>
            ticket.SName.FullyQualifiedName;

        private static bool IsAllZero(ReadOnlySpan<byte> value)
        {
            byte combined = 0;
            foreach (byte item in value)
            {
                combined |= item;
            }
            return combined == 0;
        }

        private SecurityException Reject(string reason, Exception? innerException = null)
        {
            AuthenticationRejected(_logger, reason, null);
            return new SecurityException(
                $"Kerberos authentication rejected: {reason}.",
                innerException);
        }

        private sealed class KerberosServerProtectionContext : IRpcServerProtectionContext
        {
            private readonly KerberosSession _session;
            private readonly bool _usesAcceptorSubkey;

            public KerberosServerProtectionContext(
                KerberosSession session,
                OpcProtectionLevel protectionLevel,
                bool usesAcceptorSubkey)
            {
                _session = session;
                ProtectionLevel = protectionLevel;
                _usesAcceptorSubkey = usesAcceptorSubkey;
            }

            public int AuthenticationService => KerberosAuthenticationService;

            public OpcProtectionLevel ProtectionLevel { get; }

            public int VerifierLength =>
                _session.GetWrapTokenLength(0, ProtectionLevel >= OpcProtectionLevel.Privacy);

            public int GetVerifierLength(int signedRegionLength, int confidentialLength) =>
                _session.GetWrapTokenLength(
                    ProtectionLevel >= OpcProtectionLevel.Privacy
                        ? confidentialLength
                        : signedRegionLength,
                    ProtectionLevel >= OpcProtectionLevel.Privacy);

            public void Protect(
                Span<byte> signedRegion,
                int confidentialOffset,
                int confidentialLength,
                out byte[] verifier)
            {
                if (ProtectionLevel >= OpcProtectionLevel.Privacy)
                {
                    Span<byte> confidential = signedRegion.Slice(
                        confidentialOffset,
                        confidentialLength);
                    verifier = _session.WrapMessage(confidential, confidential: true);
                    verifier.AsSpan(Rfc4121HeaderLength, confidential.Length)
                        .CopyTo(confidential);
                    return;
                }

                verifier = _session.WrapMessage(signedRegion, confidential: false);
            }

            public bool Unprotect(
                Span<byte> signedRegion,
                int confidentialOffset,
                int confidentialLength,
                ReadOnlyMemory<byte> verifier)
            {
                bool privacy = ProtectionLevel >= OpcProtectionLevel.Privacy;
                if (!HasExpectedPeerFlags(verifier.Span, privacy))
                {
                    return false;
                }

                Span<byte> target = privacy
                    ? signedRegion.Slice(confidentialOffset, confidentialLength)
                    : signedRegion;
                try
                {
                    byte[] plaintext = _session.UnwrapMessage(
                        verifier.Span,
                        out bool wasConfidential);
                    if (wasConfidential != privacy || plaintext.Length != target.Length)
                    {
                        return false;
                    }
                    if (!wasConfidential && !plaintext.AsSpan().SequenceEqual(target))
                    {
                        return false;
                    }

                    plaintext.CopyTo(target);
                    return true;
                }
                catch (Exception exception) when (
                    exception is ArgumentException
                        or CryptographicException
                        or InvalidOperationException
                        or SecurityException)
                {
                    return false;
                }
            }

            private bool HasExpectedPeerFlags(
                ReadOnlySpan<byte> verifier,
                bool privacy)
            {
                if (verifier.Length < Rfc4121HeaderLength)
                {
                    return false;
                }

                byte flags = verifier[2];
                bool sentByAcceptor = (flags & SentByAcceptorFlag) != 0;
                bool acceptorSubkey = (flags & AcceptorSubkeyFlag) != 0;
                bool sealedToken = (flags & 0x02) != 0;
                return !sentByAcceptor
                    && acceptorSubkey == _usesAcceptorSubkey
                    && sealedToken == privacy;
            }
        }
    }
}
