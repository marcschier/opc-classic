// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Creates per-connection acceptors for one RPC authentication service.
/// </summary>
public interface IRpcServerAuthenticationProvider
{
    /// <summary>
    /// Gets the RPC authentication service identifier handled by this provider.
    /// </summary>
    int AuthenticationService { get; }

    /// <summary>
    /// Creates a fresh authentication acceptor for one RPC connection.
    /// </summary>
    IRpcServerAuthenticationAcceptor CreateAcceptor();
}
