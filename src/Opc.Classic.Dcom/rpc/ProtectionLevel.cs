// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Protection level
/// </summary>
public enum ProtectionLevel
{

    /// <summary>
    /// None
    /// </summary>
    PROTECTION_LEVEL_NONE = 1,

    /// <summary>
    /// Connect
    /// </summary>
    PROTECTION_LEVEL_CONNECT = 2,

    /// <summary>
    /// Call
    /// </summary>
    PROTECTION_LEVEL_CALL = 3,

    /// <summary>
    /// Packet
    /// </summary>
    PROTECTION_LEVEL_PACKET = 4,

    /// <summary>
    /// Integrity
    /// </summary>
    PROTECTION_LEVEL_INTEGRITY = 5,

    /// <summary>
    /// Privacy
    /// </summary>
    PROTECTION_LEVEL_PRIVACY = 6,
}
