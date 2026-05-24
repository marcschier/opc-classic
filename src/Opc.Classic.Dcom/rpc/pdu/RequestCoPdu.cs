// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Util.Sharpen;
using System;
using System.IO;

namespace Opc.Classic.Dcom.Rpc.pdu; 
/// <summary>
/// Request pdu
/// </summary>
public class RequestCoPdu : ConnectionOrientedPdu, IFragmentable {

    /// <summary> Type info - TODO - move to PduTypes.cs </summary>
    public const int REQUEST_TYPE = 0x00;

    /// <inheritdoc/>
    public override int Type => REQUEST_TYPE;

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
    /// Op number
    /// </summary>
    public override int Opnum { get; set; }

    /// <summary>
    /// Request object
    /// </summary>
    public UUID Object {
        get => _object;
        set {
            _object = value;
            SetFlag(PFC_OBJECT_UUID, value != null);
        }
    }

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
        UUID oid = null;
        var src = ndr.Buffer;
        AllocationHint = src.Dec_ndr_long();
        ContextId = src.Dec_ndr_short();
        Opnum = src.Dec_ndr_short();
        if (GetFlag(PFC_OBJECT_UUID)) {
            oid = new UUID();
            try {
                oid.Decode(ndr, src);
            }
            catch (NdrException e) {
                Log.Logger.Verbose(e, "Decode error - skip.");
            }
        }
        Object = oid;
    }

    /// <inheritdoc/>
    protected internal override void WriteBody(NdrCodec ndr) {
        var dst = ndr.Buffer;
        dst.Enc_ndr_long(AllocationHint);
        dst.Enc_ndr_short(ContextId);
        dst.Enc_ndr_short(Opnum);
        if (GetFlag(PFC_OBJECT_UUID)) {
            try {
                _object.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Verbose(e, "Encode error - skip.");
            }
        }
    }

    /// <inheritdoc/>
    protected internal void ReadStub(NdrCodec ndr) {
        var src = ndr.Buffer;
        src.Align(8);
        byte[] stub = null;
        var length = FragmentLength - src.Index;
        if (length > 0) {
            stub = new byte[length];
            ndr.ReadOctetArray(stub, 0, length);
        }
        Stub = stub;
    }

    /// <inheritdoc/>
    protected internal void WriteStub(NdrCodec ndr) {
        var dst = ndr.Buffer;
        dst.Align(8, 0);
        var stub = Stub;
        if (stub != null) {
            ndr.WriteOctetArray(stub, 0, stub.Length);
        }
    }

    /// <inheritdoc/>
    public Iterator<ConnectionOrientedPdu> GetFragments(int size) {
        var stub = Stub;
        if (stub == null) {
            return new ConnectionOrientedPdu[] { this }.Iterator();
        }

        // subtracting 8 bytes for authentication header and 16
        // for the authentication verifier size, someone forgot the
        // poor guys..
        var stubSize = size - (GetFlag(PFC_OBJECT_UUID) ? 40 : 24) - 8 - 16;
        if (stub.Length <= stubSize) {
            return new ConnectionOrientedPdu[] { this }.Iterator();
        }
        Log.Logger.Verbose(
            "In fragment of RequestCoPdu, this packet will be fragmented while sending...");
        return new FragmentIterator(this, stubSize);
    }

    /// <inheritdoc/>
    public ConnectionOrientedPdu Reassemble(Iterator<ConnectionOrientedPdu> fragments) {
        if (!fragments.HasNext()) {
            throw new IOException("No fragments available.");
        }
        try {
            var pdu = (RequestCoPdu)fragments.Next();
            var stub = pdu.Stub;
            if (stub == null) {
                stub = Array.Empty<byte>();
            }
            while (fragments.HasNext()) {
                var fragment_Renamed = (RequestCoPdu)fragments.Next();
                var fragmentStub = fragment_Renamed.Stub;
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
            return (ConnectionOrientedPdu)base.MemberwiseClone();
        }
        catch (Exception) {
            throw new InvalidOperationException();
        }
    }

    private sealed class FragmentIterator : Iterator<ConnectionOrientedPdu> {

        public FragmentIterator(RequestCoPdu outerInstance, int stubSize) {
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
            var fragment = (RequestCoPdu)_outerInstance.Clone();
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
            // always use the same callId
            fragment.CallId = _callId;
            Log.Logger.Verbose("In FragementIterator:next(): callIdCounter is " + _callId);
            return fragment;
        }

        /// <inheritdoc/>
        public override void Remove() => throw new NotSupportedException();

        private readonly RequestCoPdu _outerInstance;
        private readonly int _stubSize;
        private int _index;
        private readonly int _callId = AllocateCallId();
    }

    private UUID _object;
}
