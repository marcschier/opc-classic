// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Production server-side NTLMv2 <see cref="AuthenticationSource" /> backed by a single configured
/// credential (username / password / optional domain). It validates an inbound NTLMv2 handshake
/// against that credential and establishes the per-connection session-security context used for
/// per-PDU integrity / privacy.
/// </summary>
/// <remarks>
/// <para>
/// All NTLMv2 cryptography (Type-2 challenge generation, NT-proof verification, channel-binding and
/// MIC validation, session-key derivation, and the sign/seal context) is performed by the vetted
/// <see cref="NtlmAuthentication" /> engine; this type only supplies the configured credential and
/// wires the two-step <c>CreateChallenge</c> / <c>Authenticate</c> conversation to it.
/// </para>
/// <para>
/// Per-connection state (the server <see cref="NtlmAuthentication" /> instance that holds the saved
/// challenge and, after authentication, the established <c>Security</c>) is stashed in the
/// per-connection <see cref="PropertyBag" /> passed to both calls — so a single shared
/// <see cref="ConfiguredAuthenticationSource" /> instance is safe across concurrent connections.
/// Retrieve the established context with <see cref="GetEstablishedContext(PropertyBag)" /> to
/// sign/seal subsequent PDUs.
/// </para>
/// </remarks>
public sealed class ConfiguredAuthenticationSource : AuthenticationSource
{
    /// <summary>Environment variable supplying the configured DCOM username.</summary>
    public const string UserEnvironmentVariable = "OPC_CLASSIC_DCOM_USER";

    /// <summary>Environment variable supplying the configured DCOM password.</summary>
    public const string PasswordEnvironmentVariable = "OPC_CLASSIC_DCOM_PASSWORD";

    /// <summary>Environment variable supplying the configured DCOM domain (optional).</summary>
    public const string DomainEnvironmentVariable = "OPC_CLASSIC_DCOM_DOMAIN";

    private const string ServerContextKey = "opc.classic.server.ntlm.context";

    private readonly string _user;
    private readonly string _password;
    private readonly string _domain;
    private readonly byte[]? _channelBindingsHash;

    /// <summary>
    /// Initializes a new <see cref="ConfiguredAuthenticationSource" /> for a single credential.
    /// </summary>
    /// <param name="user">Configured username the client must present.</param>
    /// <param name="password">Configured password used to verify the NTLMv2 proof.</param>
    /// <param name="domain">Configured domain (optional; empty for a workgroup credential).</param>
    /// <param name="channelBindingsHash">
    /// Optional expected channel-binding token (CBT) hash. When supplied, the client's NTLMv2 blob
    /// must carry a matching channel-binding AV pair or authentication is rejected.
    /// </param>
    public ConfiguredAuthenticationSource(
        string user,
        string password,
        string domain = "",
        byte[]? channelBindingsHash = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(user);
        ArgumentNullException.ThrowIfNull(password);
        _user = user;
        _password = password;
        _domain = domain ?? string.Empty;
        _channelBindingsHash = channelBindingsHash is null ? null : (byte[])channelBindingsHash.Clone();
    }

    /// <summary>
    /// Builds a <see cref="ConfiguredAuthenticationSource" /> from the
    /// <c>OPC_CLASSIC_DCOM_USER</c> / <c>OPC_CLASSIC_DCOM_PASSWORD</c> / <c>OPC_CLASSIC_DCOM_DOMAIN</c>
    /// environment variables, or <see langword="null" /> when no username/password pair is configured.
    /// </summary>
    public static ConfiguredAuthenticationSource? FromEnvironment()
    {
        string? user = Environment.GetEnvironmentVariable(UserEnvironmentVariable);
        string? password = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable);
        if (string.IsNullOrEmpty(user) || password is null)
        {
            return null;
        }

        string domain = Environment.GetEnvironmentVariable(DomainEnvironmentVariable) ?? string.Empty;
        return new ConfiguredAuthenticationSource(user, password, domain);
    }

    /// <inheritdoc />
    public override byte[] CreateChallenge(PropertyBag properties, Type1Message type1)
    {
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(type1);

        var server = new NtlmAuthentication(BuildServerProperties());
        properties.SetProperty(ServerContextKey, server);
        return server.CreateType2(type1).ToByteArray();
    }

    /// <inheritdoc />
    public override sbyte[] Authenticate(PropertyBag properties, Type2Message type2, Type3Message type3)
    {
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(type3);

        if (properties.GetProperty(ServerContextKey) is not NtlmAuthentication server)
        {
            throw new InvalidOperationException(
                "CreateChallenge must be called on the same connection PropertyBag before Authenticate.");
        }

        server.CreateSecurityWhenServerWithMic(type3, type3.ToByteArray());
        return ToSignedBytes(server.EstablishedSessionKey);
    }

    /// <summary>
    /// Returns the established server NTLM context for a connection whose handshake has completed,
    /// or <see langword="null" /> if authentication has not (yet) succeeded on this connection. The
    /// context's <see cref="NtlmAuthentication.Security" /> performs per-PDU sign/seal.
    /// </summary>
    public static NtlmAuthentication? GetEstablishedContext(PropertyBag properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return properties.GetProperty(ServerContextKey) is NtlmAuthentication server && server.Security is not null
            ? server
            : null;
    }

    internal override NtlmAuthentication? GetEstablishedNtlmContext(PropertyBag properties) =>
        GetEstablishedContext(properties);

    private PropertyBag BuildServerProperties()
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
        properties.SetProperty("rpc.ntlm.domain", _domain);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, _user);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, _password);
        if (_channelBindingsHash is not null)
        {
            properties.SetProperty("rpc.ntlm.channelBindingsHash", _channelBindingsHash);
        }

        return properties;
    }

    private static sbyte[] ToSignedBytes(ReadOnlyMemory<byte>? sessionKey)
    {
        ReadOnlySpan<byte> bytes = sessionKey.GetValueOrDefault().Span;
        var signed = new sbyte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            signed[i] = unchecked((sbyte)bytes[i]);
        }

        return signed;
    }
}
