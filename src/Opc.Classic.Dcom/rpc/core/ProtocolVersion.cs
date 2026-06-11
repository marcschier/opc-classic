// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Protocol version
/// </summary>
public class ProtocolVersion : NdrOp
{

    /// <summary>
    /// Major version
    /// </summary>
    public int MajorVersion { get; set; }

    /// <summary>
    /// Minor version
    /// </summary>
    public int MinorVersion { get; set; }

    /// <inheritdoc/>
    public override void Encode(NdrCodec ndr, NdrBuffer dst)
    {
        dst.Enc_ndr_small(MajorVersion);
        dst.Enc_ndr_small(MinorVersion);
    }

    /// <inheritdoc/>
    public override void Decode(NdrCodec ndr, NdrBuffer src)
    {
        MajorVersion = src.Dec_ndr_small();
        MinorVersion = src.Dec_ndr_small();
    }
}
