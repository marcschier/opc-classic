// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Endpoint
/// </summary>
public sealed class ComEndpoint : ConnectionOrientedEndpoint
{

    /// <summary>
    /// Create endpoint
    /// </summary>
    /// <param name="transport">Underlying RPC transport handle, such as a TCP socket or SMB named pipe.</param>
    /// <param name="syntax">Presentation syntax negotiated for the RPC context.</param>
    internal ComEndpoint(ITransport transport, PresentationSyntax syntax) :
        base(transport, syntax)
    {
    }

    /// <summary>
    /// Rebind
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    public void RebindEndPoint() => Rebind();
}
