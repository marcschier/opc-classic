// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Endpoint type
/// </summary>
[Flags]
public enum Semantics
{

    /// <summary>
    /// No special call semantics.
    /// </summary>
    None = 0,

    /// <summary>
    /// Maybe
    /// </summary>
    MAYBE = 0x01,

    /// <summary>
    /// Idempotent
    /// </summary>
    IDEMPOTENT = 0x02,

    /// <summary>
    /// Broadcast
    /// </summary>
    BROADCAST = 0x04,
}
