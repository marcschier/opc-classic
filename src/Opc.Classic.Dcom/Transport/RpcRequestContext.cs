// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using System.Security.Principal;

namespace Opc.Classic.Dcom.Transport;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct RpcRequestContext(
    bool IsAuthenticated,
    bool IsEstablished,
    OpcProtectionLevel ProtectionLevel,
    EndPoint RemoteEndpoint)
{
    /// <summary>
    /// Gets the established RPC authentication service identifier.
    /// </summary>
    public int AuthenticationService { get; init; }

    /// <summary>
    /// Gets the authenticated and authorization-mapped principal.
    /// </summary>
    public IPrincipal? Principal { get; init; }
}
