// SPDX-License-Identifier: MIT

using System.IO;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc; 
/// <summary>
/// Security interface
/// </summary>
public interface ISecurity {

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
    /// <param name="ndr"></param>
    /// <param name="index"></param>
    /// <param name="length"></param>
    /// <param name="verifierIndex"></param>
    /// <param name="isFragmented"></param>
    /// <exception cref="IOException"></exception>
    void ProcessIncoming(NdrCodec ndr, int index, int length,
        int verifierIndex, bool isFragmented);

    /// <summary>
    /// Process outgoing
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="index"></param>
    /// <param name="length"></param>
    /// <param name="verifierIndex"></param>
    /// <param name="isFragmented"></param>
    /// <exception cref="IOException"></exception>
    void ProcessOutgoing(NdrCodec ndr, int index, int length,
        int verifierIndex, bool isFragmented);
}
