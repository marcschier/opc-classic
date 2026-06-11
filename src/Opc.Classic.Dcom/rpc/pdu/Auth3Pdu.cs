// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Auth pdu
/// </summary>
public class Auth3Pdu : ConnectionOrientedPdu
{

    public const int AUTH3_TYPE = 0x10;

    /// <inheritdoc/>
    public override int Type => AUTH3_TYPE;

    /// <summary>
    /// Create pdu
    /// </summary>
    public Auth3Pdu() =>
        // Really useless value
        CallId = 0;

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr) =>
        ndr.WriteUnsignedLong(0);
}
