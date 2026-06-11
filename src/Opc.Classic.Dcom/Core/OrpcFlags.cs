// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Core;

internal enum OrpcFlags
{

    /// <summary>
    /// no additional info in packet
    /// </summary>
    ORPCF_NULL = 0,

    /// <summary>
    /// call is local to this machine
    /// </summary>
    ORPCF_LOCAL = 1,

    /// <summary>
    /// reserved for local use
    /// </summary>
    ORPCF_RESERVED1 = 2,

    /// <summary>
    /// reserved for local use
    /// </summary>
    ORPCF_RESERVED2 = 4,

    /// <summary>
    /// reserved for local use
    /// </summary>
    ORPCF_RESERVED3 = 8,

    /// <summary>
    /// reserved for local use
    /// </summary>
    ORPCF_RESERVED4 = 16
}
