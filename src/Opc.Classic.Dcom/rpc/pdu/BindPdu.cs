// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Bind pdu
/// </summary>
public class BindPdu : ConnectionOrientedPdu
{
    public const int BIND_TYPE = 0x0b;

    // Concurrency audit (date: 2026-05-22): PFC_CONC_MPX (0x10) is parsed and
    // emitted through ConnectionOrientedPdu.Flags for every PDU, including bind.
    // BindPdu itself does not negotiate concurrent call routing, and the server
    // request loop remains single-request/single-response per connection until
    // the Channel<>-backed worker queue lands.

    /// <inheritdoc/>
    public override int Type => BIND_TYPE;

    /// <summary>
    /// max transmit
    /// </summary>
    public int MaxTransmitFragment { get; set; } = MUST_RECEIVE_FRAGMENT_SIZE;

    /// <summary>
    /// Max receive
    /// </summary>
    public int MaxReceiveFragment { get; set; } = MUST_RECEIVE_FRAGMENT_SIZE;

    /// <summary>
    /// Association group
    /// </summary>
    public int AssociationGroupId { get; set; }

    /// <summary>
    /// Context list
    /// </summary>
    public PresentationContext[] ContextList { get; set; }

    /// <inheritdoc/>
    protected internal override void ReadBody(NdrCodec ndr)
    {
        MaxTransmitFragment = ndr.ReadUnsignedShort();
        MaxReceiveFragment = ndr.ReadUnsignedShort();
        AssociationGroupId = ndr.ReadUnsignedLong();
        var count = ndr.ReadUnsignedSmall();
        var contextList = new PresentationContext[count];
        for (var i = 0; i < count; i++)
        {
            contextList[i] = new PresentationContext();
            contextList[i].Read(ndr);
        }
        ContextList = contextList;
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr)
    {
        ndr.WriteUnsignedShort(MaxTransmitFragment);
        ndr.WriteUnsignedShort(MaxReceiveFragment);
        ndr.WriteUnsignedLong(AssociationGroupId);
        var contextList = ContextList;
        var count = contextList.Length;
        ndr.WriteUnsignedSmall((short)count);
        for (var i = 0; i < count; i++)
        {
            contextList[i].Write(ndr);
        }
    }

    /// <summary>
    /// Helper
    /// </summary>
    public void ResetCallIdCounter() => ResetCallIdCounterValue();
}
