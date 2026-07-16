// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Immutable, AOT-safe configuration for an inbound Kerberos authentication provider.
/// </summary>
public sealed class KerberosServerOptions
{
    private static readonly HashSet<EncryptionType> SupportedEncryptionTypes =
    [
        EncryptionType.AES128_CTS_HMAC_SHA1_96,
        EncryptionType.AES256_CTS_HMAC_SHA1_96,
        EncryptionType.AES128_CTS_HMAC_SHA256_128,
        EncryptionType.AES256_CTS_HMAC_SHA384_192,
        EncryptionType.RC4_HMAC_NT,
    ];

    /// <summary>
    /// Initializes and validates inbound Kerberos policy.
    /// </summary>
    public KerberosServerOptions(
        IEnumerable<string> servicePrincipals,
        string realm,
        IKerberosServerCredentialProvider credentialProvider,
        IEnumerable<EncryptionType> allowedEncryptionTypes,
        TimeSpan clockSkew,
        KerberosChannelBindingPolicy channelBindingPolicy,
        OpcProtectionLevel minimumProtectionLevel,
        IKerberosPrincipalMapper principalMapper,
        ReadOnlyMemory<byte>? channelBindingsHash = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(servicePrincipals);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        ArgumentNullException.ThrowIfNull(credentialProvider);
        ArgumentNullException.ThrowIfNull(allowedEncryptionTypes);
        ArgumentNullException.ThrowIfNull(principalMapper);

        string normalizedRealm = realm.Trim().ToUpperInvariant();
        string[] principals = ValidateServicePrincipals(servicePrincipals);
        EncryptionType[] encryptionTypes = ValidateEncryptionTypes(allowedEncryptionTypes);
        ValidateProtocolPolicy(clockSkew, channelBindingPolicy, minimumProtectionLevel);
        byte[]? validatedChannelBindingsHash =
            ValidateChannelBindingsHash(channelBindingsHash);
        if (channelBindingPolicy != KerberosChannelBindingPolicy.Disabled
            && validatedChannelBindingsHash is null)
        {
            throw new ArgumentException(
                "WhenPresent and Required channel-binding policies require an expected 16-byte hash.",
                nameof(channelBindingsHash));
        }
        if (!string.Equals(credentialProvider.Realm, normalizedRealm, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The credential provider realm must match the configured realm.",
                nameof(credentialProvider));
        }
        if (!principals.Contains(credentialProvider.Principal, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The credential provider principal must be one of the configured service principals.",
                nameof(credentialProvider));
        }

        ServicePrincipals = Array.AsReadOnly(principals);
        Realm = normalizedRealm;
        CredentialProvider = credentialProvider;
        AllowedEncryptionTypes = Array.AsReadOnly(encryptionTypes);
        ClockSkew = clockSkew;
        ChannelBindingPolicy = channelBindingPolicy;
        MinimumProtectionLevel = minimumProtectionLevel;
        PrincipalMapper = principalMapper;
        ChannelBindingsHash = validatedChannelBindingsHash;
        TimeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Gets accepted service principals.
    /// </summary>
    public IReadOnlyList<string> ServicePrincipals { get; }

    /// <summary>
    /// Gets the canonical Kerberos realm.
    /// </summary>
    public string Realm { get; }

    /// <summary>
    /// Gets the credential source.
    /// </summary>
    public IKerberosServerCredentialProvider CredentialProvider { get; }

    /// <summary>
    /// Gets accepted ticket encryption types.
    /// </summary>
    public IReadOnlyList<EncryptionType> AllowedEncryptionTypes { get; }

    /// <summary>
    /// Gets the maximum permitted authenticator clock skew.
    /// </summary>
    public TimeSpan ClockSkew { get; }

    /// <summary>
    /// Gets channel-binding enforcement policy.
    /// </summary>
    public KerberosChannelBindingPolicy ChannelBindingPolicy { get; }

    /// <summary>
    /// Gets the minimum accepted RPC protection level.
    /// </summary>
    public OpcProtectionLevel MinimumProtectionLevel { get; }

    /// <summary>
    /// Gets explicit principal mapping and authorization policy.
    /// </summary>
    public IKerberosPrincipalMapper PrincipalMapper { get; }

    /// <summary>
    /// Gets the expected RFC 2744 channel-bindings hash, when configured.
    /// </summary>
    public ReadOnlyMemory<byte>? ChannelBindingsHash { get; }

    /// <summary>
    /// Gets the clock used for ticket and authenticator lifetime validation.
    /// </summary>
    public TimeProvider TimeProvider { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{nameof(KerberosServerOptions)} {{ Realm = {Realm}, ServicePrincipals = {ServicePrincipals.Count}, Credential = [REDACTED], EncryptionTypes = {AllowedEncryptionTypes.Count}, ClockSkew = {ClockSkew}, ChannelBinding = {ChannelBindingPolicy}, MinimumProtection = {MinimumProtectionLevel} }}";

    private static string ValidateServicePrincipal(string principal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        string value = principal.Trim();
        int separator = value.IndexOf('/');
        if (separator <= 0
            || separator == value.Length - 1
            || value.IndexOf('/', separator + 1) >= 0
            || value.Contains('@', StringComparison.Ordinal)
            || value.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException(
                "Service principals must use the unqualified 'service/host' form without whitespace.",
                nameof(principal));
        }

        return value;
    }

    private static string[] ValidateServicePrincipals(IEnumerable<string> servicePrincipals)
    {
        string[] principals = servicePrincipals.Select(ValidateServicePrincipal).ToArray();
        if (principals.Length == 0)
        {
            throw new ArgumentException("At least one service principal is required.", nameof(servicePrincipals));
        }
        if (principals.Distinct(StringComparer.OrdinalIgnoreCase).Count() != principals.Length)
        {
            throw new ArgumentException("Service principals must be unique.", nameof(servicePrincipals));
        }

        return principals;
    }

    private static EncryptionType[] ValidateEncryptionTypes(
        IEnumerable<EncryptionType> allowedEncryptionTypes)
    {
        EncryptionType[] encryptionTypes = allowedEncryptionTypes.ToArray();
        if (encryptionTypes.Length == 0)
        {
            throw new ArgumentException("At least one encryption type is required.", nameof(allowedEncryptionTypes));
        }
        if (encryptionTypes.Distinct().Count() != encryptionTypes.Length
            || encryptionTypes.Any(static encryptionType => !SupportedEncryptionTypes.Contains(encryptionType)))
        {
            throw new ArgumentException(
                "Encryption types must be unique and limited to supported AES or RC4-HMAC values.",
                nameof(allowedEncryptionTypes));
        }

        return encryptionTypes;
    }

    private static void ValidateProtocolPolicy(
        TimeSpan clockSkew,
        KerberosChannelBindingPolicy channelBindingPolicy,
        OpcProtectionLevel minimumProtectionLevel)
    {
        if (clockSkew <= TimeSpan.Zero || clockSkew > TimeSpan.FromHours(1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(clockSkew),
                "Clock skew must be greater than zero and no more than one hour.");
        }
        if (!Enum.IsDefined(channelBindingPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(channelBindingPolicy));
        }
        if (minimumProtectionLevel is < OpcProtectionLevel.Connect or > OpcProtectionLevel.Privacy)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumProtectionLevel),
                "Minimum protection must be Connect, Call, Packet, Integrity, or Privacy.");
        }
    }

    private static byte[]? ValidateChannelBindingsHash(
        ReadOnlyMemory<byte>? channelBindingsHash)
    {
        if (!channelBindingsHash.HasValue || channelBindingsHash.Value.IsEmpty)
        {
            return null;
        }

        if (channelBindingsHash.Value.Length != 16)
        {
            throw new ArgumentException(
                "The Kerberos channel-bindings hash must be exactly 16 bytes.",
                nameof(channelBindingsHash));
        }

        return channelBindingsHash.Value.ToArray();
    }
}
