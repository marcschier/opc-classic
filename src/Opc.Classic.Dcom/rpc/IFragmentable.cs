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
    /// <param name="size"></param>
    /// <returns></returns>
    IEnumerable<ConnectionOrientedPdu> GetFragments(int size);

    /// <summary>
    /// Reassemble
    /// </summary>
    /// <param name="fragments"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    ConnectionOrientedPdu Reassemble(
        IEnumerable<ConnectionOrientedPdu> fragments);

    /// <summary>
    /// Clone
    /// </summary>
    /// <returns></returns>
    ConnectionOrientedPdu Clone();
}
