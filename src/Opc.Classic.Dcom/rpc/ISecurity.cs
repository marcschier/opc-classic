// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Security interface
/// </summary>
public interface ISecurity
{
    /// <summary>
    /// Verifier length
    /// </summary>
    int VerifierLength { get; }

    /// <summary>
    /// Authentication service
    /// </summary>
    int AuthenticationService { get; }

    /// <summary>
    /// Protection level
    /// </summary>
    ProtectionLevel Protection { get; }

    /// <summary>
    /// Process incoming
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="index">Zero-based index at which the read or write operation begins.</param>
    /// <param name="length">Number of bytes or elements to process.</param>
    /// <param name="verifierIndex">Index at which the authentication verifier begins in the PDU buffer.</param>
    /// <param name="isFragmented">Value indicating whether the PDU payload spans additional fragments.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void ProcessIncoming(NdrCodec ndr, int index, int length,
        int verifierIndex, bool isFragmented);

    /// <summary>
    /// Process outgoing
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="index">Zero-based index at which the read or write operation begins.</param>
    /// <param name="length">Number of bytes or elements to process.</param>
    /// <param name="verifierIndex">Index at which the authentication verifier begins in the PDU buffer.</param>
    /// <param name="isFragmented">Value indicating whether the PDU payload spans additional fragments.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void ProcessOutgoing(NdrCodec ndr, int index, int length,
        int verifierIndex, bool isFragmented);
}
