// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
//
// replaced the original Java SPI / META-INF/services service-loader
// (incompatible with NativeAOT because it uses runtime ClassLoader / Type.GetType
// reflection) with a simple register-at-startup pattern. Consumers — typically a
// Microsoft.Extensions.DependencyInjection container — register their custom
// AuthenticationSource via SetDefaultInstance(...) once during host startup.
//

using System.Buffers.Binary;
using System.Security.Principal;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Dcom.Rpc.Auth;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Server-side NTLM authentication source. Consumer-pluggable: an
/// implementation validates incoming NTLM credentials against the
/// consumer's credential store (e.g. an in-memory user table, LDAP, an
/// Active Directory bind, etc.).
/// </summary>
/// <remarks>
/// This contract is only exercised when the managed process is acting as
/// an inbound DCOM server (the <see cref="Opc.Classic.Dcom.Core.LocalCoClass"/>
/// + <see cref="Opc.Classic.Dcom.Core.ComOxidRuntime"/> path receiving callback
/// PDUs). Pure client scenarios never construct an
/// <see cref="AuthenticationSource"/>.
/// <para>
/// Register an implementation at host startup via
/// <see cref="SetDefaultInstance(AuthenticationSource?)"/>. If no
/// implementation is registered, <see cref="DefaultInstance"/> returns
/// <see cref="NullAuthenticationSource.Instance"/> which throws
/// <see cref="InvalidOperationException"/> on any auth attempt.
/// </para>
/// </remarks>
public abstract class AuthenticationSource : IRpcServerAuthenticationProvider
{
    private static AuthenticationSource? s_default;

    /// <summary>
    /// The current default authentication source. Never returns
    /// <see langword="null"/>; returns <see cref="NullAuthenticationSource.Instance"/>
    /// (which throws on auth attempts) until an implementation is registered
    /// via <see cref="SetDefaultInstance(AuthenticationSource?)"/>.
    /// </summary>
    public static AuthenticationSource DefaultInstance =>
        Volatile.Read(ref s_default) ?? NullAuthenticationSource.Instance;

    /// <summary>
    /// Register the global default authentication source. Intended to be
    /// called once at host startup by the DI container or test fixture.
    /// </summary>
    /// <param name="source">
    /// The implementation to use, or <see langword="null"/> to reset to
    /// the no-op default (subsequent auth attempts will throw).
    /// </param>
    public static void SetDefaultInstance(AuthenticationSource? source)
    {
        Interlocked.Exchange(ref s_default, source);
    }

    /// <inheritdoc />
    public int AuthenticationService => NtlmAuthentication.AUTHENTICATIONSERVICENTLM;

    /// <inheritdoc />
    public virtual IRpcServerAuthenticationAcceptor CreateAcceptor() =>
        new NtlmAuthenticationSourceAcceptor(this);

    /// <summary>
    /// Produce an 8-byte NTLM challenge for the given Type-1 message and
    /// session properties.
    /// </summary>
    /// <exception cref="IOException">Underlying credential store I/O failure.</exception>
    public abstract byte[] CreateChallenge(PropertyBag properties,
        Type1Message type1);

    /// <summary>
    /// Validate the client's Type-3 response against the (Type-2 challenge,
    /// session properties) and return the session key bytes used for
    /// integrity / privacy on subsequent traffic.
    /// </summary>
    /// <exception cref="IOException">Underlying credential store I/O failure.</exception>
    public abstract sbyte[] Authenticate(PropertyBag properties,
        Type2Message type2, Type3Message type3);

    internal virtual NtlmAuthentication? GetEstablishedNtlmContext(PropertyBag properties) => null;

    private sealed class NtlmAuthenticationSourceAcceptor : IRpcServerAuthenticationAcceptor
    {
        private readonly AuthenticationSource _source;
        private readonly PropertyBag _properties = new();
        private Type2Message? _type2;
        private bool _established;

        public NtlmAuthenticationSourceAcceptor(AuthenticationSource source) =>
            _source = source;

        public RpcServerAuthenticationTokenResult AcceptToken(
            ReadOnlyMemory<byte> token,
            OpcProtectionLevel protectionLevel)
        {
            if (token.IsEmpty)
            {
                throw new ArgumentException("NTLM authentication tokens cannot be empty.", nameof(token));
            }

            byte[] tokenBytes = token.ToArray();
            int messageType = ReadMessageType(tokenBytes);
            if (messageType == 1)
            {
                var type1 = new Type1Message(tokenBytes);
                byte[] challenge = _source.CreateChallenge(_properties, type1);
                _type2 = new Type2Message(challenge);
                _established = false;
                return RpcServerAuthenticationTokenResult.Continue(challenge);
            }

            if (messageType != 3)
            {
                throw new InvalidOperationException($"Unexpected NTLM message type {messageType}.");
            }
            if (_type2 is null || _established)
            {
                throw new InvalidOperationException("NTLM Type3 received before a Type2 challenge was created.");
            }

            var type3 = new Type3Message(tokenBytes);
            _source.Authenticate(_properties, _type2, type3);
            NtlmAuthentication context = _source.GetEstablishedNtlmContext(_properties)
                ?? throw new InvalidOperationException(
                    "Authentication source did not establish an NTLM security context.");
            _established = true;

            string user = type3.User ?? string.Empty;
            string domain = type3.Domain ?? string.Empty;
            string name = string.IsNullOrEmpty(domain) ? user : $"{domain}\\{user}";
            var principal = new GenericPrincipal(new GenericIdentity(name, "NTLM"), []);
            var protectionContext = new NtlmServerProtectionContext(
                context.Security,
                protectionLevel,
                type3.Flags,
                context.EstablishedSessionKey);
            var session = new RpcServerAuthenticationSession(
                NtlmAuthentication.AUTHENTICATIONSERVICENTLM,
                principal,
                protectionLevel,
                protectionContext);
            return RpcServerAuthenticationTokenResult.Complete(session);
        }

        private static int ReadMessageType(ReadOnlySpan<byte> token)
        {
            if (token.Length < 12)
            {
                throw new ArgumentException("NTLM token is too short.", nameof(token));
            }

            return BinaryPrimitives.ReadInt32LittleEndian(token[8..12]);
        }
    }

    private sealed class NtlmServerProtectionContext :
        IRpcServerProtectionContext,
        IGssMicProvider
    {
        private readonly ISecurity _security;
        private readonly NtlmMicProvider _incomingMicProvider;
        private readonly NtlmMicProvider _outgoingMicProvider;

        public NtlmServerProtectionContext(
            ISecurity security,
            OpcProtectionLevel protectionLevel,
            NtlmFlags flags,
            ReadOnlyMemory<byte>? exportedSessionKey)
        {
            ArgumentNullException.ThrowIfNull(security);
            if (!exportedSessionKey.HasValue || exportedSessionKey.Value.IsEmpty)
            {
                throw new InvalidOperationException(
                    "NTLM session security did not expose an exported session key.");
            }

            _security = security;
            ProtectionLevel = protectionLevel;
            byte[] key = exportedSessionKey.Value.ToArray();
            try
            {
                var keyFactory = new NTLMKeyFactory();
                _incomingMicProvider = new NtlmMicProvider(
                    keyFactory.GenerateClientSigningKey(flags, key));
                _outgoingMicProvider = new NtlmMicProvider(
                    keyFactory.GenerateServerSigningKey(flags, key));
            }
            finally
            {
                System.Security.Cryptography.CryptographicOperations.ZeroMemory(key);
            }
        }

        public int AuthenticationService => NtlmAuthentication.AUTHENTICATIONSERVICENTLM;

        public OpcProtectionLevel ProtectionLevel { get; }

        public int VerifierLength => _security.VerifierLength;

        public void Protect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            out byte[] verifier)
        {
            var buffer = new byte[signedRegion.Length + _security.VerifierLength];
            signedRegion.CopyTo(buffer);
            NdrCodec ndr = CreateNdrCodec(buffer);
            _security.ProcessOutgoing(
                ndr,
                confidentialOffset,
                confidentialLength,
                signedRegion.Length,
                isFragmented: false);
            buffer.AsSpan(0, signedRegion.Length).CopyTo(signedRegion);
            verifier = buffer.AsSpan(signedRegion.Length, _security.VerifierLength).ToArray();
        }

        public bool Unprotect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            ReadOnlyMemory<byte> verifier)
        {
            if (verifier.Length != _security.VerifierLength)
            {
                return false;
            }

            var buffer = new byte[signedRegion.Length + _security.VerifierLength];
            signedRegion.CopyTo(buffer);
            verifier.Span.CopyTo(buffer.AsSpan(signedRegion.Length));
            NdrCodec ndr = CreateNdrCodec(buffer);
            try
            {
                _security.ProcessIncoming(
                    ndr,
                    confidentialOffset,
                    confidentialLength,
                    signedRegion.Length,
                    isFragmented: false);
            }
            catch (IntegrityException)
            {
                return false;
            }

            buffer.AsSpan(0, signedRegion.Length).CopyTo(signedRegion);
            return true;
        }

        public byte[] GetMic(ReadOnlySpan<byte> data) =>
            _outgoingMicProvider.GetMic(data);

        public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic) =>
            _incomingMicProvider.VerifyMic(data, mic);

        private static NdrCodec CreateNdrCodec(byte[] buffer)
        {
            var ndrBuffer = new NdrBuffer(buffer, 0);
            ndrBuffer.SetLength(buffer.Length);
            return new NdrCodec { Buffer = ndrBuffer, Format = NdrFormat.DEFAULT_FORMAT };
        }
    }
}
