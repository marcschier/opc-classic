// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Fragmantable tag
/// </summary>
public interface IFragmentable
{

    /// <summary>
    /// Create fragments
    /// </summary>
    /// <param name="size">Number of bytes or elements represented by the field.</param>
    /// <returns>The requested fragments value.</returns>
    IEnumerable<ConnectionOrientedPdu> GetFragments(int size);

    /// <summary>
    /// Reassemble
    /// </summary>
    /// <param name="fragments">PDU fragments that make up the complete RPC message.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The complete PDU reconstructed from the supplied fragments.</returns>
    ConnectionOrientedPdu Reassemble(
        IEnumerable<ConnectionOrientedPdu> fragments);

    /// <summary>
    /// Clone
    /// </summary>
    /// <returns>A copy of the PDU that can be transmitted or modified independently.</returns>
    ConnectionOrientedPdu Clone();
}
