// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// UUID - TODO: Replace entirely everything with sharpcif ndr/uuid
/// </summary>
public class UUID : NdrOp
{

    /// <summary>
    /// null uuid
    /// </summary>
    public const string NIL_UUID = "00000000-0000-0000-0000-000000000000";

    /// <summary>
    /// Create uuid
    /// </summary>
    public UUID() :
        this(NIL_UUID)
    {
    }

#pragma warning disable RECS0154 // Parameter is never used
    /// <summary>
    /// Create from string
    /// </summary>
    /// <param name="str"></param>
    public UUID(string str) => _internal = new SharpCifs.Dcerpc.Uuid(str);
#pragma warning restore RECS0154 // Parameter is never used

    /// <inheritdoc/>
    public override void Encode(NdrCodec ndr, NdrBuffer dst)
    {
        dst.Enc_ndr_long(_internal.TimeLow);
        dst.Enc_ndr_short(_internal.TimeMid);
        dst.Enc_ndr_short(_internal.TimeHiAndVersion);
        dst.Enc_ndr_small(_internal.ClockSeqHiAndReserved);
        dst.Enc_ndr_small(_internal.ClockSeqLow);
        Array.Copy(_internal.Node, 0, dst.Buf, dst.Index, _internal.Node.Length);
        dst.Index += _internal.Node.Length;
    }

    /// <inheritdoc/>
    public override void Decode(NdrCodec ndr, NdrBuffer src)
    {
        _internal.TimeLow = src.Dec_ndr_long();
        _internal.TimeMid = (short)src.Dec_ndr_short();
        _internal.TimeHiAndVersion = (short)src.Dec_ndr_short();
        _internal.ClockSeqHiAndReserved = (byte)src.Dec_ndr_small();
        _internal.ClockSeqLow = (byte)src.Dec_ndr_small();
        _internal.Node = new byte[6];
        Array.Copy(src.Buf, src.Index, _internal.Node, 0, _internal.Node.Length);
        src.Index += _internal.Node.Length;
    }

    /// <inheritdoc/>
    public override string ToString() => _internal.ToString();

    /// <summary>
    /// Parse uuid
    /// </summary>
    /// <param name="uuid"></param>
    public void Parse(string uuid) => _internal = new SharpCifs.Dcerpc.Uuid(uuid);

    private SharpCifs.Dcerpc.Uuid _internal;
}
