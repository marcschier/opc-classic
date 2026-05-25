// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Util.Sharpen;
using System;
using System.Collections.Generic;
using System.IO;

namespace Opc.Classic.Dcom.Rpc.pdu; 
/// <summary>
/// Fault pdu
/// </summary>
public class FaultCoPdu : ConnectionOrientedPdu, IFragmentable {

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
    protected internal override void ReadPdu(NdrCodec ndr) {
        ReadHeader(ndr);
        ReadBody(ndr);
        ReadStub(ndr);
    }

    /// <inheritdoc/>
    protected internal override void WritePdu(NdrCodec ndr) {
        WriteHeader(ndr);
        WriteBody(ndr);
        WriteStub(ndr);
    }

    /// <inheritdoc/>
    protected internal override void ReadBody(NdrCodec ndr) {
        AllocationHint = ndr.ReadUnsignedLong();
        ContextId = ndr.ReadUnsignedShort();
        CancelCount = ndr.ReadUnsignedSmall();
        Status = (FaultCode)ndr.ReadUnsignedLong();
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr) {
        ndr.WriteUnsignedLong(AllocationHint);
        ndr.WriteUnsignedShort(ContextId);
        ndr.WriteUnsignedSmall((short)CancelCount);
        ndr.WriteUnsignedLong((int)Status);
    }

    /// <inheritdoc/>
    protected internal void ReadStub(NdrCodec ndr) {
        var buf = ndr.Buffer;
        buf.Align(8);
        byte[] stub = null;
        var length = FragmentLength - buf.Index;
        if (length > 0) {
            stub = new byte[length];
            ndr.ReadOctetArray(stub, 0, length);
        }
        Stub = stub;
    }

    /// <inheritdoc/>
    protected internal void WriteStub(NdrCodec ndr) {
        var buf = ndr.Buffer;
        buf.Align(8, 0);
        var stub = Stub;
        if (stub != null) {
            ndr.WriteOctetArray(stub, 0, stub.Length);
        }
    }

    /// <inheritdoc/>
    public Iterator<ConnectionOrientedPdu> GetFragments(int size) {
        var stub = Stub;
        if (stub == null) {
            return new List<ConnectionOrientedPdu> { this }.Iterator();
        }
        var stubSize = size - 24;
        if (stub.Length <= stubSize) {
            return new List<ConnectionOrientedPdu> { this }.Iterator();
        }
        return new FragmentIterator(this, stubSize);
    }

    /// <inheritdoc/>
    public ConnectionOrientedPdu Reassemble(Iterator<ConnectionOrientedPdu> fragments) {
        if (!fragments.HasNext()) {
            throw new IOException("No fragments available.");
        }
        try {
            var pdu = (FaultCoPdu)fragments.Next();
            var stub = pdu.Stub;
            if (stub == null) {
                stub = Array.Empty<byte>();
            }
            while (fragments.HasNext()) {
                var fragment = (FaultCoPdu)fragments.Next();
                var fragmentStub = fragment.Stub;
                if (fragmentStub != null && fragmentStub.Length > 0) {
                    var tmp = new byte[stub.Length + fragmentStub.Length];
                    Array.Copy(stub, 0, tmp, 0, stub.Length);
                    Array.Copy(fragmentStub, 0, tmp, stub.Length, fragmentStub.Length);
                    stub = tmp;
                }
            }
            var length = stub.Length;
            if (length > 0) {
                pdu.Stub = stub;
                pdu.AllocationHint = length;
            }
            else {
                pdu.Stub = null;
                pdu.AllocationHint = 0;
            }
            pdu.SetFlag(PFC_FIRST_FRAG, true);
            pdu.SetFlag(PFC_LAST_FRAG, true);
            return pdu;
        }
        catch (Exception) {
            throw new IOException("Unable to assemble PDU fragments.");
        }
    }

    /// <inheritdoc/>
    public ConnectionOrientedPdu Clone() {
        try {
            return (ConnectionOrientedPdu)base.MemberwiseClone(); // TODO : Deep clone
        }
        catch (Exception) {
            throw new InvalidOperationException();
        }
    }

    private sealed class FragmentIterator : Iterator<ConnectionOrientedPdu> {

        public FragmentIterator(FaultCoPdu outerInstance, int stubSize) {
            _outerInstance = outerInstance;
            _stubSize = stubSize;
        }

        /// <inheritdoc/>
        public override bool HasNext() => _index < _outerInstance.Stub.Length;

        /// <inheritdoc/>
        public override ConnectionOrientedPdu Next() {
            if (_index >= _outerInstance.Stub.Length) {
                throw new NoSuchElementException();
            }
            var fragment = (FaultCoPdu)_outerInstance.Clone();
            var allocation = _outerInstance.Stub.Length - _index;
            fragment.AllocationHint = allocation;
            if (_stubSize < allocation) {
                allocation = _stubSize;
            }
            var fragmentStub = new byte[allocation];
            Array.Copy(_outerInstance.Stub, _index, fragmentStub, 0, allocation);
            fragment.Stub = fragmentStub;
            var flags = _outerInstance.Flags & ~(PFC_FIRST_FRAG | PFC_LAST_FRAG);
            if (_index == 0) {
                flags |= PFC_FIRST_FRAG;
            }
            _index += allocation;
            if (_index >= _outerInstance.Stub.Length) {
                flags |= PFC_LAST_FRAG;
            }
            fragment.Flags = flags;
            return fragment;
        }

        public override void Remove() => throw new NotSupportedException();

        private readonly FaultCoPdu _outerInstance;
        private readonly int _stubSize;
        private int _index;
    }
}
