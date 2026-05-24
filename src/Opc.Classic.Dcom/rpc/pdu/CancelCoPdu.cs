// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.pdu; 

/// <summary>
/// Cancel
/// </summary>
public class CancelCoPdu : ConnectionOrientedPdu {

    /// <summary> Type info - TODO - move to PduTypes.cs </summary>
    public const int CANCEL_TYPE = 0x12;

    /// <inheritdoc/>
    public override int Type => CANCEL_TYPE;
}
