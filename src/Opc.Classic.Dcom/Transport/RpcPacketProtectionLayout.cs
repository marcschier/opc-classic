// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Transport;

internal static class RpcPacketProtectionLayout
{
    public static int GetConfidentialOffset(ReadOnlySpan<byte> pdu)
    {
        if (pdu.Length < ConnectionOrientedPdu.HEADER_LENGTH)
        {
            throw new InvalidOperationException(
                "DCE/RPC PDU is shorter than the common header.");
        }

        int offset = pdu[ConnectionOrientedPdu.TYPE_OFFSET] switch
        {
            RequestCoPdu.REQUEST_TYPE =>
                ConnectionOrientedPdu.HEADER_LENGTH
                + 8
                + (HasObjectUuid(pdu) ? 16 : 0),
            ResponseCoPdu.RESPONSE_TYPE =>
                ConnectionOrientedPdu.HEADER_LENGTH + 8,
            FaultCoPdu.FAULT_TYPE =>
                ConnectionOrientedPdu.HEADER_LENGTH + 16,
            // Bind and alter-context authentication tokens retain the existing
            // body-relative protection layout.
            _ => ConnectionOrientedPdu.HEADER_LENGTH,
        };

        if (offset > pdu.Length)
        {
            throw new InvalidOperationException(
                "DCE/RPC PDU is shorter than its fixed packet header.");
        }

        return offset;
    }

    private static bool HasObjectUuid(ReadOnlySpan<byte> pdu) =>
        (pdu[ConnectionOrientedPdu.FLAGS_OFFSET]
            & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0;
}
