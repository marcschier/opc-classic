// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Principal;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Default authorization mapper that exposes the authenticated principal unchanged.
/// </summary>
public sealed class IdentityRpcServerAuthorizationMapper : IRpcServerAuthorizationMapper
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static IdentityRpcServerAuthorizationMapper Instance { get; } =
        new IdentityRpcServerAuthorizationMapper();

    private IdentityRpcServerAuthorizationMapper()
    {
    }

    /// <inheritdoc />
    public IPrincipal MapPrincipal(IPrincipal authenticatedPrincipal)
    {
        ArgumentNullException.ThrowIfNull(authenticatedPrincipal);
        return authenticatedPrincipal;
    }
}
