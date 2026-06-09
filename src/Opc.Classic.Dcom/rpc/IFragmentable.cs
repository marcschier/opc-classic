// SPDX-License-Identifier: MIT

using SharpCifs.Util.Sharpen;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Fragmantable tag
/// </summary>
public interface IFragmentable {

    /// <summary>
    /// Create fragments
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    Iterator<ConnectionOrientedPdu> GetFragments(int size);

    /// <summary>
    /// Reassemble
    /// </summary>
    /// <param name="fragments"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    ConnectionOrientedPdu Reassemble(
        Iterator<ConnectionOrientedPdu> fragments);

    /// <summary>
    /// Clone
    /// </summary>
    /// <returns></returns>
    ConnectionOrientedPdu Clone();
}
