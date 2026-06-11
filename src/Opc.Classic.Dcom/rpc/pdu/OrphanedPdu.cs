// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Orphan
/// </summary>
public class OrphanedPdu : ConnectionOrientedPdu
{

    public const int ORPHANED_TYPE = 0x13;

    /// <inheritdoc/>
    public override int Type => ORPHANED_TYPE;
}
