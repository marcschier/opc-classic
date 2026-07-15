// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Normalizes principals and applies only explicitly configured application authorization.
/// </summary>
/// <remarks>
/// This policy does not inspect PAC data, operating-system groups, or platform identities.
/// </remarks>
public sealed class KerberosPrincipalMappingPolicy : IKerberosPrincipalMapper
{
    private readonly Dictionary<string, KerberosPrincipalMapping> _mappings;

    /// <summary>
    /// Initializes an explicit principal mapping policy.
    /// </summary>
    public KerberosPrincipalMappingPolicy(
        KerberosPrincipalNormalization normalization,
        IEnumerable<KerberosPrincipalMapping> mappings,
        bool allowUnmappedPrincipals = false)
    {
        if (!Enum.IsDefined(normalization))
        {
            throw new ArgumentOutOfRangeException(nameof(normalization));
        }

        ArgumentNullException.ThrowIfNull(mappings);
        Normalization = normalization;
        AllowUnmappedPrincipals = allowUnmappedPrincipals;
        _mappings = new Dictionary<string, KerberosPrincipalMapping>(StringComparer.Ordinal);
        foreach (KerberosPrincipalMapping mapping in mappings)
        {
            ArgumentNullException.ThrowIfNull(mapping);
            string normalized = Normalize(mapping.AuthenticatedPrincipal);
            if (!_mappings.TryAdd(normalized, mapping))
            {
                throw new ArgumentException(
                    $"Duplicate normalized Kerberos principal mapping '{normalized}'.",
                    nameof(mappings));
            }
        }

        if (_mappings.Count == 0 && !allowUnmappedPrincipals)
        {
            throw new ArgumentException(
                "At least one explicit mapping is required when unmapped principals are rejected.",
                nameof(mappings));
        }
    }

    /// <summary>
    /// Gets the configured normalization mode.
    /// </summary>
    public KerberosPrincipalNormalization Normalization { get; }

    /// <summary>
    /// Gets a value indicating whether normalized, unmapped principals are authorized without roles.
    /// </summary>
    public bool AllowUnmappedPrincipals { get; }

    /// <inheritdoc />
    public bool TryMapPrincipal(
        string authenticatedPrincipal,
        [NotNullWhen(true)] out IPrincipal? applicationPrincipal)
    {
        string normalized;
        try
        {
            normalized = Normalize(authenticatedPrincipal);
        }
        catch (ArgumentException)
        {
            applicationPrincipal = null;
            return false;
        }

        if (_mappings.TryGetValue(normalized, out KerberosPrincipalMapping? mapping))
        {
            applicationPrincipal = new GenericPrincipal(
                new GenericIdentity(mapping.ApplicationPrincipal, "Kerberos"),
                mapping.Roles.ToArray());
            return true;
        }

        if (AllowUnmappedPrincipals)
        {
            applicationPrincipal = new GenericPrincipal(
                new GenericIdentity(normalized, "Kerberos"),
                []);
            return true;
        }

        applicationPrincipal = null;
        return false;
    }

    /// <summary>
    /// Normalizes and validates a Kerberos principal according to this policy.
    /// </summary>
    public string Normalize(string principal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        int separator = principal.LastIndexOf('@');
        if (separator <= 0 || separator == principal.Length - 1)
        {
            throw new ArgumentException(
                "Kerberos principals must include a non-empty name and realm.",
                nameof(principal));
        }

        string name = principal[..separator];
        string realm = principal[(separator + 1)..];
        if (name.Any(char.IsWhiteSpace) || realm.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Kerberos principals cannot contain whitespace.", nameof(principal));
        }

        return Normalization switch
        {
            KerberosPrincipalNormalization.None => principal,
            KerberosPrincipalNormalization.CanonicalRealm =>
                $"{name}@{realm.ToUpperInvariant()}",
            KerberosPrincipalNormalization.LowercaseNameAndCanonicalRealm =>
                $"{name.ToLowerInvariant()}@{realm.ToUpperInvariant()}",
            _ => throw new InvalidOperationException("Unsupported principal normalization policy."),
        };
    }
}
