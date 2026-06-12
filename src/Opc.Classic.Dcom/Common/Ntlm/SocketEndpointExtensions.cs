// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Sockets;

namespace Opc.Classic.Dcom.Common.Ntlm;

/// <summary>
/// Small <see cref="Socket"/> port-access helpers used by the legacy DCOM
/// transport plumbing. The accessors return 0 when the underlying endpoint
/// is not an <see cref="IPEndPoint"/> (e.g. unbound sockets) so callers can
/// treat them as nullable-free.
/// </summary>
public static class SocketEndpointExtensions
{
    /// <summary>
    /// Gets the bound local TCP/UDP port for <paramref name="socket"/>, or 0 if unbound.
    /// </summary>
    public static int GetLocalPort(this Socket socket) =>
        socket.LocalEndPoint is IPEndPoint endpoint ? endpoint.Port : 0;

    /// <summary>
    /// Gets the connected remote TCP/UDP port for <paramref name="socket"/>, or 0 if unconnected.
    /// </summary>
    public static int GetPort(this Socket socket) =>
        socket.RemoteEndPoint is IPEndPoint endpoint ? endpoint.Port : 0;
}
