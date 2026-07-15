// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Immutable policy for inbound SPNEGO authentication.
/// </summary>
public sealed class SpnegoServerOptions
{
    /// <summary>
    /// Initializes SPNEGO mechanism policy with a Boolean NTLM fallback switch.
    /// </summary>
    public SpnegoServerOptions(
        IRpcServerAuthenticationProvider? kerberosProvider,
        IRpcServerAuthenticationProvider? ntlmProvider,
        bool allowNtlmFallback)
        : this(
            kerberosProvider,
            ntlmProvider,
            allowNtlmFallback
                ? SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable
                : SpnegoNtlmFallbackPolicy.Disabled)
    {
    }

    /// <summary>
    /// Initializes SPNEGO mechanism and fallback policy.
    /// </summary>
    public SpnegoServerOptions(
        IRpcServerAuthenticationProvider? kerberosProvider,
        IRpcServerAuthenticationProvider? ntlmProvider = null,
        SpnegoNtlmFallbackPolicy ntlmFallbackPolicy = SpnegoNtlmFallbackPolicy.Disabled)
    {
        if (!Enum.IsDefined(ntlmFallbackPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(ntlmFallbackPolicy));
        }
        if (kerberosProvider is null
            && (ntlmProvider is null
                || ntlmFallbackPolicy == SpnegoNtlmFallbackPolicy.Disabled))
        {
            throw new ArgumentException(
                "SPNEGO requires a Kerberos provider or an enabled NTLM fallback provider.",
                nameof(kerberosProvider));
        }
        if (kerberosProvider is not null
            && kerberosProvider.AuthenticationService
                != KerberosServerAuthenticationProvider.KerberosAuthenticationService)
        {
            throw new ArgumentException(
                "The Kerberos provider must handle RPC_C_AUTHN_GSS_KERBEROS.",
                nameof(kerberosProvider));
        }
        if (ntlmProvider is not null
            && ntlmProvider.AuthenticationService
                != ntlm.NtlmAuthentication.AUTHENTICATIONSERVICENTLM)
        {
            throw new ArgumentException(
                "The NTLM provider must handle RPC_C_AUTHN_WINNT.",
                nameof(ntlmProvider));
        }
        if (ntlmFallbackPolicy != SpnegoNtlmFallbackPolicy.Disabled
            && ntlmProvider is null)
        {
            throw new ArgumentException(
                "An NTLM provider is required when fallback is enabled.",
                nameof(ntlmProvider));
        }

        KerberosProvider = kerberosProvider;
        NtlmProvider = ntlmProvider;
        NtlmFallbackPolicy = ntlmFallbackPolicy;
    }

    /// <summary>
    /// Gets the direct Kerberos provider, when configured.
    /// </summary>
    public IRpcServerAuthenticationProvider? KerberosProvider { get; }

    /// <summary>
    /// Gets the NTLM provider used only by explicit fallback policy.
    /// </summary>
    public IRpcServerAuthenticationProvider? NtlmProvider { get; }

    /// <summary>
    /// Gets the NTLM fallback policy.
    /// </summary>
    public SpnegoNtlmFallbackPolicy NtlmFallbackPolicy { get; }

    /// <summary>
    /// Gets whether policy permits NTLM fallback.
    /// </summary>
    public bool AllowNtlmFallback =>
        NtlmFallbackPolicy == SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable;

    /// <summary>
    /// Gets whether the acceptor is restricted to Kerberos.
    /// </summary>
    public bool KerberosOnly => !AllowNtlmFallback;
}
