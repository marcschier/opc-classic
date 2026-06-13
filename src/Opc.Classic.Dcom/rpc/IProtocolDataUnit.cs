// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Defines a PDU
/// </summary>
public interface IProtocolDataUnit
{
    /// <summary>
    /// Major version
    /// </summary>
    int MajorVersion { get; }

    /// <summary>
    /// Type
    /// </summary>
    int Type { get; }

    /// <summary>
    /// Format to use
    /// </summary>
    NdrFormat Format { get; set; }
}
