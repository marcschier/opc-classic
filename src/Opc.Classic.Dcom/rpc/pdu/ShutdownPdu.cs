// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.pdu; 

/// <summary>
/// Shutdown
/// </summary>
public class ShutdownPdu : ConnectionOrientedPdu {

    /// <summary> Type info - TODO - move to PduTypes.cs </summary>
    public const int SHUTDOWN_TYPE = 0x11;

    /// <inheritdoc/>
    public override int Type => SHUTDOWN_TYPE;
}
