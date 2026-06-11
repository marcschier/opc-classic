// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common.Ntlm;
using System;
using System.IO;

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Response pdu
/// </summary>
public class ResponseCoPdu : ConnectionOrientedPdu, IFragmentable
{

    public const int RESPONSE_TYPE = 0x02;

    /// <inheritdoc/>
    public override int Type => RESPONSE_TYPE;

    /// <summary>
    /// Stub
    /// </summary>
    public byte[] Stub { get; set; }

    /// <summary>
    /// Allocation hint
    /// </summary>
    public int AllocationHint { get; set; }

    /// <summary>
    /// Context
    /// </summary>
    public int ContextId { get; set; }

    /// <summary>
    /// Cancel count
    /// </summary>
    public int CancelCount { get; set; }


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
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr)
    {
        ndr.WriteUnsignedLong(AllocationHint);
        ndr.WriteUnsignedShort(ContextId);
        ndr.WriteUnsignedSmall((short)CancelCount);
    }

    /// <inheritdoc/>
    public IEnumerable<ConnectionOrientedPdu> GetFragments(int size)
    {
        var stub = Stub;
        // subtracting 8 bytes for authentication header and 16 for the
        // authentication verifier size, someone forgot the poor guys..
        var stubSize = size - 24 - 8 - 16;
        if (stub == null || stub.Length <= stubSize)
        {
            yield return this;
            yield break;
        }

        var index = 0;
        while (index < stub.Length)
        {
            var fragment = (ResponseCoPdu)Clone();
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
        Log.Logger.Verbose("[DURING RECIEVE IN ASSEMBLE]\n");
        using var iter = fragments.GetEnumerator();
        if (!iter.MoveNext())
        {
            throw new IOException("No fragments available.");
        }
        try
        {
            var pdu = (ResponseCoPdu)iter.Current;
            var stub = pdu.Stub ?? Array.Empty<byte>();
            var i = 0;
            while (iter.MoveNext())
            {
                Log.Logger.Verbose("[IN ASSEMBLE] Fragment { " + i + " }\n");
                var fragment_Renamed = (ResponseCoPdu)iter.Current;
                var fragmentStub = fragment_Renamed.Stub;
                if (fragmentStub != null && fragmentStub.Length > 0)
                {
                    Log.Logger.Verbose("[FRAGMENT'S STUB (new one)] Length is = " +
                        fragmentStub.Length);
                    var tmp = new byte[stub.Length + fragmentStub.Length];
                    Array.Copy(stub, 0, tmp, 0, stub.Length);
                    Array.Copy(fragmentStub, 0, tmp, stub.Length, fragmentStub.Length);
                    stub = tmp;
                    Log.Logger.Verbose(
                        "[ADDED THIS STUB (previous stub + new one) into OLD STUB] " +
                        "Current Length of pieces assembled so far = " + stub.Length +
                        "\n" + Utils.HexString(stub, 0, stub.Length));
                }
            }
            var length = stub.Length;
            if (length > 0)
            {
                pdu.Stub = stub;
                pdu.AllocationHint = length;
                Log.Logger.Verbose("[FULL AND FINAL STUB AFTER ASSEMBLY]\n" +
                    Utils.HexString(stub, 0, stub.Length));
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
            return (ConnectionOrientedPdu)base.MemberwiseClone();
        }
        catch (Exception)
        {
            throw new InvalidOperationException();
        }
    }
    /// <summary>
    /// Read stub
    /// </summary>
    /// <param name="ndr"></param>
    private void ReadStub(NdrCodec ndr)
    {
        ndr.Buffer.Align(8);
        byte[] stub = null;
        var length = FragmentLength - ndr.Buffer.Index;
        if (length > 0)
        {
            stub = new byte[length];
            ndr.ReadOctetArray(stub, 0, length);
        }
        Stub = stub;
    }

    /// <summary>
    /// Write stub
    /// </summary>
    /// <param name="ndr"></param>
    private void WriteStub(NdrCodec ndr)
    {
        ndr.Buffer.Align(8, 0);
        var stub = Stub;
        if (stub != null)
        {
            ndr.WriteOctetArray(stub, 0, stub.Length);
        }
    }
}
