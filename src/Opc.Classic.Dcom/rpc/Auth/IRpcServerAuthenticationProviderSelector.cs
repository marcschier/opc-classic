// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Selects a server authentication provider for an RPC authentication service.
/// </summary>
public interface IRpcServerAuthenticationProviderSelector
{
    /// <summary>
    /// Attempts to select the provider registered for <paramref name="authenticationService"/>.
    /// </summary>
    bool TryGetProvider(
        int authenticationService,
        [NotNullWhen(true)] out IRpcServerAuthenticationProvider? provider);
}
