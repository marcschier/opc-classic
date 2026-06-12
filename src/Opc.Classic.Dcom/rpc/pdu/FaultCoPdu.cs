// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Fault pdu
/// </summary>
public class FaultCoPdu : ConnectionOrientedPdu, IFragmentable
{

    public const int FAULT_TYPE = 0x03;

    /// <inheritdoc/>
    public override int Type => FAULT_TYPE;

    /// <summary>
    /// Stub
    /// </summary>
    public byte[] Stub { get; set; }

    /// <summary>
    /// Hint
    /// </summary>
    public int AllocationHint { get; set; }

    /// <summary>
    /// Context
    /// </summary>
    public int ContextId { get; set; }

    /// <summary>
    /// Cancel counter
    /// </summary>
    public int CancelCount { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public FaultCode Status { get; set; } = FaultCode.UNSPECIFIED_REJECTION;

    /// <inheritdoc/>
    protected internal override void ReadPdu(NdrCodec ndr)
    {
        ReadHeader(ndr);
        ReadBody(ndr);
        ReadStub(ndr);
    }

    /// <inheritdoc/>
    protected internal override void WritePdu(NdrCodec ndr)
    {
        WriteHeader(ndr);
        WriteBody(ndr);
        WriteStub(ndr);
    }

    /// <inheritdoc/>
    protected internal override void ReadBody(NdrCodec ndr)
    {
        AllocationHint = ndr.ReadUnsignedLong();
        ContextId = ndr.ReadUnsignedShort();
        CancelCount = ndr.ReadUnsignedSmall();
        Status = (FaultCode)ndr.ReadUnsignedLong();
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr)
    {
        ndr.WriteUnsignedLong(AllocationHint);
        ndr.WriteUnsignedShort(ContextId);
        ndr.WriteUnsignedSmall((short)CancelCount);
        ndr.WriteUnsignedLong((int)Status);
    }

    /// <inheritdoc/>
    protected internal void ReadStub(NdrCodec ndr)
    {
        var buf = ndr.Buffer;
        buf.Align(8);
        byte[] stub = null;
        var length = FragmentLength - buf.Index;
        if (length > 0)
        {
            stub = new byte[length];
            ndr.ReadOctetArray(stub, 0, length);
        }
        Stub = stub;
    }

    /// <inheritdoc/>
    protected internal void WriteStub(NdrCodec ndr)
    {
        var buf = ndr.Buffer;
        buf.Align(8, 0);
        var stub = Stub;
        if (stub != null)
        {
            ndr.WriteOctetArray(stub, 0, stub.Length);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<ConnectionOrientedPdu> GetFragments(int size)
    {
        var stub = Stub;
        if (stub == null || stub.Length <= size - 24)
        {
            yield return this;
            yield break;
        }

        var stubSize = size - 24;
        var index = 0;
        while (index < stub.Length)
        {
            var fragment = (FaultCoPdu)Clone();
            var allocation = stub.Length - index;
            fragment.AllocationHint = allocation;
            if (stubSize < allocation)
            {
                allocation = stubSize;
            }
            var fragmentStub = new byte[allocation];
            Array.Copy(stub, index, fragmentStub, 0, allocation);
            fragment.Stub = fragmentStub;
            var flags = Flags & ~(PFC_FIRST_FRAG | PFC_LAST_FRAG);
            if (index == 0)
            {
                flags |= PFC_FIRST_FRAG;
            }
            index += allocation;
            if (index >= stub.Length)
            {
                flags |= PFC_LAST_FRAG;
            }
            fragment.Flags = flags;
            yield return fragment;
        }
    }

    /// <inheritdoc/>
    public ConnectionOrientedPdu Reassemble(IEnumerable<ConnectionOrientedPdu> fragments)
    {
        using var iter = fragments.GetEnumerator();
        if (!iter.MoveNext())
        {
            throw new IOException("No fragments available.");
        }
        try
        {
            var pdu = (FaultCoPdu)iter.Current;
            var stub = pdu.Stub ?? Array.Empty<byte>();
            while (iter.MoveNext())
            {
                var fragment = (FaultCoPdu)iter.Current;
                var fragmentStub = fragment.Stub;
                if (fragmentStub != null && fragmentStub.Length > 0)
                {
                    var tmp = new byte[stub.Length + fragmentStub.Length];
                    Array.Copy(stub, 0, tmp, 0, stub.Length);
                    Array.Copy(fragmentStub, 0, tmp, stub.Length, fragmentStub.Length);
                    stub = tmp;
                }
            }
            var length = stub.Length;
            if (length > 0)
            {
                pdu.Stub = stub;
                pdu.AllocationHint = length;
            }
            else
            {
                pdu.Stub = null;
                pdu.AllocationHint = 0;
            }
            pdu.SetFlag(PFC_FIRST_FRAG, true);
            pdu.SetFlag(PFC_LAST_FRAG, true);
            return pdu;
        }
        catch (Exception)
        {
            throw new IOException("Unable to assemble PDU fragments.");
        }
    }

    /// <inheritdoc/>
    public ConnectionOrientedPdu Clone()
    {
        try
        {
            return (ConnectionOrientedPdu)base.MemberwiseClone(); // TODO : Deep clone
        }
        catch (Exception)
        {
            throw new InvalidOperationException();
        }
    }
}
