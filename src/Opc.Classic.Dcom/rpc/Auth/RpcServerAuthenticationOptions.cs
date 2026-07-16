// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Authentication configuration for <see cref="Opc.Classic.Dcom.Transport.RpcServerConnectionProcessor"/>.
/// </summary>
public sealed class RpcServerAuthenticationOptions
{
    /// <summary>
    /// Initializes authentication configuration.
    /// </summary>
    public RpcServerAuthenticationOptions(
        IRpcServerAuthenticationProviderSelector providerSelector,
        IRpcServerAuthorizationMapper? authorizationMapper = null,
        bool requireAuthentication = true)
    {
        ArgumentNullException.ThrowIfNull(providerSelector);
        ProviderSelector = providerSelector;
        AuthorizationMapper = authorizationMapper ?? IdentityRpcServerAuthorizationMapper.Instance;
        RequireAuthentication = requireAuthentication;
    }

    /// <summary>
    /// Gets the provider selector.
    /// </summary>
    public IRpcServerAuthenticationProviderSelector ProviderSelector { get; }

    /// <summary>
    /// Gets the principal mapper.
    /// </summary>
    public IRpcServerAuthorizationMapper AuthorizationMapper { get; }

    /// <summary>
    /// Gets a value indicating whether requests require an established authenticated session.
    /// </summary>
    public bool RequireAuthentication { get; }
}
