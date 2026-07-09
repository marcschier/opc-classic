// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Connection interface
/// </summary>
public interface IConnection
{
    /// <summary>
    /// Transmit
    /// </summary>
    /// <param name="pdu">DCE/RPC PDU instance being encoded, decoded, or transmitted.</param>
    /// <param name="transport">Underlying RPC transport handle, such as a TCP socket or SMB named pipe.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void Transmit(ConnectionOrientedPdu pdu, ITransport transport);

    /// <summary>
    /// Receive
    /// </summary>
    /// <param name="transport">Underlying RPC transport handle, such as a TCP socket or SMB named pipe.</param>
    /// <returns>The next complete DCE/RPC connection-oriented PDU received from the transport.</returns>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    ConnectionOrientedPdu Receive(ITransport transport);
}
