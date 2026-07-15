// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Defines one explicit Kerberos-to-application principal authorization mapping.
/// </summary>
public sealed class KerberosPrincipalMapping
{
    /// <summary>
    /// Initializes a principal mapping.
    /// </summary>
    public KerberosPrincipalMapping(
        string authenticatedPrincipal,
        string applicationPrincipal,
        IEnumerable<string>? roles = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticatedPrincipal);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationPrincipal);
        AuthenticatedPrincipal = authenticatedPrincipal;
        ApplicationPrincipal = applicationPrincipal;
        Roles = Array.AsReadOnly(roles?.Select(ValidateRole).ToArray() ?? []);
    }

    /// <summary>
    /// Gets the authenticated Kerberos principal.
    /// </summary>
    public string AuthenticatedPrincipal { get; }

    /// <summary>
    /// Gets the application-visible principal name.
    /// </summary>
    public string ApplicationPrincipal { get; }

    /// <summary>
    /// Gets the explicitly assigned application roles.
    /// </summary>
    public IReadOnlyList<string> Roles { get; }

    private static string ValidateRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        return role;
    }
}
