// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.pdu; 
/// <summary>
/// Orphan
/// </summary>
public class OrphanedPdu : ConnectionOrientedPdu {

    /// <summary> Type info - TODO - move to PduTypes.cs </summary>
    public const int ORPHANED_TYPE = 0x13;

    /// <inheritdoc/>
    public override int Type => ORPHANED_TYPE;
}
