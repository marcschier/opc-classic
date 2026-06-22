// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Bind nack
/// </summary>
public class BindNoAcknowledgePdu : ConnectionOrientedPdu
{
    public const int BIND_NO_ACKNOWLEDGE_TYPE = 0x0d;

    /// <inheritdoc/>
    public override int Type => BIND_NO_ACKNOWLEDGE_TYPE;

    /// <summary>
    /// Reject reason
    /// </summary>
    public BindNoAcknowledgeReason RejectReason { get; set; }

    /// <summary>
    /// Version list
    /// </summary>
    public ProtocolVersion[] VersionList { get; set; }

    /// <inheritdoc/>
    protected internal override void ReadBody(NdrCodec ndr)
    {
        var reason = ndr.ReadUnsignedSmall();
        RejectReason = (BindNoAcknowledgeReason)reason;
        ProtocolVersion[] versionList = null;
        if (RejectReason == BindNoAcknowledgeReason.PROTOCOL_VERSION_NOT_SUPPORTED)
        {
            var count = ndr.ReadUnsignedSmall();
            versionList = new ProtocolVersion[count];
            for (var i = 0; i < count; i++)
            {
                versionList[i] = new ProtocolVersion();
                versionList[i].Read(ndr);
            }
        }
        VersionList = versionList;
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr)
    {
        var reason = (short)RejectReason;
        ndr.WriteUnsignedSmall(reason);
        if (RejectReason != BindNoAcknowledgeReason.PROTOCOL_VERSION_NOT_SUPPORTED)
        {
            return;
        }
        var versionList = VersionList;
        var count = (versionList != null) ? versionList.Length : 0;
        for (var i = 0; i < count; i++)
        {
            versionList[i].Write(ndr);
        }
    }
}
