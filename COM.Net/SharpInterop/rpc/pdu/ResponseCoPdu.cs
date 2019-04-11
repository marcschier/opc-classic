//
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
//
// j-Interop (Pure Java implementation of DCOM protocol)
//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace rpc.pdu {
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    /// <summary>
    /// Response pdu
    /// </summary>
    public class ResponseCoPdu : ConnectionOrientedPdu, IFragmentable {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
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
        }

        /// <inheritdoc/>
        protected internal override void WriteBody(NdrCodec ndr) {
            ndr.WriteUnsignedLong(AllocationHint);
            ndr.WriteUnsignedShort(ContextId);
            ndr.WriteUnsignedSmall((short)CancelCount);
        }

        /// <inheritdoc/>
        public Iterator<ConnectionOrientedPdu> GetFragments(int size) {
            var stub = Stub;
            if (stub == null) {
                return new ConnectionOrientedPdu[] { this }.Iterator();
            }

            // subtracting 8 bytes for authentication header and 16 for the
            // authentication verifier size, someone forgot the poor guys..
            var stubSize = size - 24 - 8 - 16;
            if (stub.Length <= stubSize) {
                return new ConnectionOrientedPdu[] { this }.Iterator();
            }
            return new FragmentIterator(this, stubSize);
        }

        /// <inheritdoc/>
        public ConnectionOrientedPdu Reassemble(Iterator<ConnectionOrientedPdu> fragments) {
            Log.Logger.Verbose("[DURING RECIEVE IN ASSEMBLE]\n");
            if (!fragments.HasNext()) {
                throw new IOException("No fragments available.");
            }
            try {
                var pdu = (ResponseCoPdu)fragments.Next();
                var stub = pdu.Stub;
                if (stub == null) {
                    stub = new byte[0];
                }
                var i = 0;
                while (fragments.HasNext()) {
                    Log.Logger.Verbose("[IN ASSEMBLE] Fragment { " + i + " }\n");
                    var fragment_Renamed = (ResponseCoPdu)fragments.Next();
                    var fragmentStub = fragment_Renamed.Stub;
                    if (fragmentStub != null && fragmentStub.Length > 0) {
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
                if (length > 0) {
                    pdu.Stub = stub;
                    pdu.AllocationHint = length;
                    Log.Logger.Verbose("[FULL AND FINAL STUB AFTER ASSEMBLY]\n" +
                        Utils.HexString(stub, 0, stub.Length));
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
        /// <summary>
        /// Read stub
        /// </summary>
        /// <param name="ndr"></param>
        private void ReadStub(NdrCodec ndr) {
            ndr.Buffer.Align(8);
            byte[] stub = null;
            var length = FragmentLength - ndr.Buffer.Index;
            if (length > 0) {
                stub = new byte[length];
                ndr.ReadOctetArray(stub, 0, length);
            }
            Stub = stub;
        }

        /// <summary>
        /// Write stub
        /// </summary>
        /// <param name="ndr"></param>
        private void WriteStub(NdrCodec ndr) {
            ndr.Buffer.Align(8, 0);
            var stub = Stub;
            if (stub != null) {
                ndr.WriteOctetArray(stub, 0, stub.Length);
            }
        }

        private class FragmentIterator : Iterator<ConnectionOrientedPdu> {

            public FragmentIterator(ResponseCoPdu outerInstance, int stubSize) {
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
                var fragment = (ResponseCoPdu)_outerInstance.Clone();
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

            /// <inheritdoc/>
            public override void Remove() => throw new NotSupportedException();

            private readonly ResponseCoPdu _outerInstance;
            private readonly int _stubSize;
            private int _index;
        }
    }
}