// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Internal.LegacyNdr;

public abstract class NdrOp
{
    public object Value { get; set; }

    public virtual int Opnum { get; set; } = -1;

    public virtual void Encode(NdrCodec ndr, NdrBuffer dst)
    {
        ndr.Buffer = dst;
        Write(ndr);
    }

    public virtual void Decode(NdrCodec ndr, NdrBuffer src)
    {
        ndr.Buffer = src;
        Read(ndr);
    }

    public virtual void Write(NdrCodec ndr)
    {
    }

    public virtual void Read(NdrCodec ndr)
    {
    }
}
