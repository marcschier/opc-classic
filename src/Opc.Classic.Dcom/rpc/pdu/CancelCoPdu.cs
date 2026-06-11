// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Cancel
/// </summary>
public class CancelCoPdu : ConnectionOrientedPdu
{

    public const int CANCEL_TYPE = 0x12;

    /// <inheritdoc/>
    public override int Type => CANCEL_TYPE;
}
