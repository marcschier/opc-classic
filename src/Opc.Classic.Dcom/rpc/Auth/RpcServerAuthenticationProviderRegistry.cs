// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Explicit, AOT-safe authentication provider registry.
/// </summary>
public sealed class RpcServerAuthenticationProviderRegistry : IRpcServerAuthenticationProviderSelector
{
    private readonly Lock _gate = new();
    private readonly Dictionary<int, IRpcServerAuthenticationProvider> _providers = [];

    /// <summary>
    /// Initializes an empty registry.
    /// </summary>
    public RpcServerAuthenticationProviderRegistry()
    {
    }

    /// <summary>
    /// Initializes a registry with explicitly supplied providers.
    /// </summary>
    public RpcServerAuthenticationProviderRegistry(IEnumerable<IRpcServerAuthenticationProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        foreach (IRpcServerAuthenticationProvider provider in providers)
        {
            Register(provider);
        }
    }

    /// <summary>
    /// Gets a value indicating whether any providers are registered.
    /// </summary>
    public bool HasProviders
    {
        get
        {
            lock (_gate)
            {
                return _providers.Count != 0;
            }
        }
    }

    /// <summary>
    /// Registers a provider. Duplicate service identifiers are rejected.
    /// </summary>
    public void Register(IRpcServerAuthenticationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if ((uint)provider.AuthenticationService > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(provider),
                "RPC authentication service identifiers must fit in one byte.");
        }

        lock (_gate)
        {
            if (!_providers.TryAdd(provider.AuthenticationService, provider))
            {
                throw new InvalidOperationException(
                    $"An RPC authentication provider is already registered for service {provider.AuthenticationService}.");
            }
        }
    }

    /// <inheritdoc />
    public bool TryGetProvider(
        int authenticationService,
        [NotNullWhen(true)] out IRpcServerAuthenticationProvider? provider)
    {
        lock (_gate)
        {
            return _providers.TryGetValue(authenticationService, out provider);
        }
    }
}
