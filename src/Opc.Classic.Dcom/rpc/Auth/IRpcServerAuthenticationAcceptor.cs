// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Accepts mechanism tokens for one RPC connection.
/// </summary>
public interface IRpcServerAuthenticationAcceptor
{
    /// <summary>
    /// Accepts the next token from the peer and optionally returns a response token or established session.
    /// </summary>
    RpcServerAuthenticationTokenResult AcceptToken(
        ReadOnlyMemory<byte> token,
        OpcProtectionLevel protectionLevel);

    /// <summary>
    /// Accepts the next token with cooperative cancellation.
    /// </summary>
    RpcServerAuthenticationTokenResult AcceptToken(
        ReadOnlyMemory<byte> token,
        OpcProtectionLevel protectionLevel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return AcceptToken(token, protectionLevel);
    }
}
