// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Result of accepting one authentication token.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct RpcServerAuthenticationTokenResult(
    ReadOnlyMemory<byte> ResponseToken,
    RpcServerAuthenticationSession? Session)
{
    /// <summary>
    /// Creates a continuation result containing a token for the peer.
    /// </summary>
    public static RpcServerAuthenticationTokenResult Continue(ReadOnlyMemory<byte> responseToken) =>
        new(responseToken, null);

    /// <summary>
    /// Creates a completed result containing the established session.
    /// </summary>
    public static RpcServerAuthenticationTokenResult Complete(RpcServerAuthenticationSession session)
        => Complete(session, ReadOnlyMemory<byte>.Empty);

    /// <summary>
    /// Creates a completed result containing the established session and a final token for the peer.
    /// </summary>
    public static RpcServerAuthenticationTokenResult Complete(
        RpcServerAuthenticationSession session,
        ReadOnlyMemory<byte> responseToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        return new RpcServerAuthenticationTokenResult(responseToken, session);
    }
}
