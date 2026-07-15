// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Explicitly maps an authenticated Kerberos principal to application authorization.
/// </summary>
public interface IKerberosPrincipalMapper
{
    /// <summary>
    /// Attempts to map and authorize an authenticated Kerberos principal.
    /// </summary>
    bool TryMapPrincipal(
        string authenticatedPrincipal,
        [NotNullWhen(true)] out IPrincipal? applicationPrincipal);
}
