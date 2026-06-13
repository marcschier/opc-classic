// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Bind ack
/// </summary>
public class BindAcknowledgePdu : ConnectionOrientedPdu
{
    public const int BIND_ACKNOWLEDGE_TYPE = 0x0c;

    /// <inheritdoc/>
    public override int Type => BIND_ACKNOWLEDGE_TYPE;

    /// <summary>
    /// Max transmit fragment
    /// </summary>
    public int MaxTransmitFragment { get; set; } = MUST_RECEIVE_FRAGMENT_SIZE;

    /// <summary>
    /// Max receive fragment
    /// </summary>
    public int MaxReceiveFragment { get; set; } = MUST_RECEIVE_FRAGMENT_SIZE;

    /// <summary>
    /// Association group id
    /// </summary>
    public int AssociationGroupId { get; set; }

    /// <summary>
    /// Secondary address
    /// </summary>
    public Port SecondaryAddress { get; set; }

    /// <summary>
    /// Result list
    /// </summary>
    public PresentationResult[] ResultList { get; set; }

    /// <inheritdoc/>
    protected internal override void ReadBody(NdrCodec ndr)
    {
        MaxTransmitFragment = ndr.ReadUnsignedShort();
        MaxReceiveFragment = ndr.ReadUnsignedShort();
        AssociationGroupId = ndr.ReadUnsignedLong();
        var secondaryAddress = new Port();
        secondaryAddress.Read(ndr);
        SecondaryAddress = secondaryAddress;
        ndr.Buffer.Align(4);
        var count = ndr.ReadUnsignedSmall();
        var resultList = new PresentationResult[count];
        for (var i = 0; i < count; i++)
        {
            resultList[i] = new PresentationResult();
            resultList[i].Read(ndr);
        }
        ResultList = resultList;
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr)
    {
        ndr.WriteUnsignedShort(MaxTransmitFragment);
        ndr.WriteUnsignedShort(MaxReceiveFragment);
        ndr.WriteUnsignedLong(AssociationGroupId);
        var secondaryAddress = SecondaryAddress;
        if (secondaryAddress == null)
        {
            secondaryAddress = new Port();
        }
        secondaryAddress.Write(ndr);
        ndr.Buffer.Align(4);
        var resultList = ResultList;
        var count = resultList.Length;
        ndr.WriteUnsignedSmall((short)count);
        for (var i = 0; i < count; i++)
        {
            resultList[i].Write(ndr);
        }
    }
}
