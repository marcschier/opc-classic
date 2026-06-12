// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Framework Internal.
/// </summary>
public interface IComRuntimeWorker
{

    /// <summary>
    /// Set op number
    /// </summary>
    int Opnum { get; set; }

    /// <summary>
    /// Current iid
    /// </summary>
    string CurrentIID { get; set; }

    /// <summary>
    /// Current object
    /// </summary>
    UUID CurrentObjectID { get; set; }

#pragma warning disable MA0016 // Implementations in Core expose List<string>; keep interface shape stable for this wave.
    /// <summary>
    /// Query interface ids
    /// </summary>
    List<string> QIedIIDs { get; }
#pragma warning restore MA0016

    /// <summary>
    /// Resolver
    /// </summary>
    bool Resolver { get; }

    /// <summary>
    /// Worker
    /// </summary>
    /// <returns><c>true</c> when worker over is satisfied; otherwise <c>false</c>.</returns>
    bool WorkerOver();
}
