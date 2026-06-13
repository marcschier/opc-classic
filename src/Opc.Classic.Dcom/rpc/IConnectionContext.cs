// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Connection context
/// </summary>
public interface IConnectionContext
{
    /// <summary>
    /// Connectrion
    /// </summary>
    IConnection Connection { get; }

    /// <summary>
    /// Whether it is established
    /// </summary>
    bool Established { get; }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <param name="properties">Property values used to initialize the COM descriptor.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The initial authentication token produced for the connection.</returns>
    ConnectionOrientedPdu Init(PresentationContext context,
        PropertyBag properties);

    /// <summary>
    /// Alter
    /// </summary>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The authentication token produced for an alter-context exchange.</returns>
    ConnectionOrientedPdu Alter(PresentationContext context);

    /// <summary>
    /// Accept
    /// </summary>
    /// <param name="pdu">DCE/RPC PDU instance being encoded, decoded, or transmitted.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The authentication token produced while accepting the remote context.</returns>
    ConnectionOrientedPdu Accept(ConnectionOrientedPdu pdu);
}
