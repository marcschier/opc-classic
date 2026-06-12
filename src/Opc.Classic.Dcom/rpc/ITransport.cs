// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Transport interface
/// </summary>
public interface ITransport
{

    /// <summary>
    /// Protocol name
    /// </summary>
    string Protocol { get; }

    /// <summary>
    /// Configuration
    /// </summary>
    PropertyBag Properties { get; }

    /// <summary>
    /// Attach
    /// </summary>
    /// <param name="syntax">Presentation syntax negotiated for the RPC context.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The bound RPC endpoint attached to the transport.</returns>
    IEndpoint Attach(PresentationSyntax syntax);

    /// <summary>
    /// Send
    /// </summary>
    /// <param name="buffer">Buffer containing the bytes or fields being processed.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void Send(NdrBuffer buffer);

    /// <summary>
    /// Receive
    /// </summary>
    /// <param name="buffer">Buffer containing the bytes or fields being processed.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void Receive(NdrBuffer buffer);

    /// <summary>
    /// Close
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void Close();
}
