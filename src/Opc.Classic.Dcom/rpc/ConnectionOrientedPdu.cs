//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc {
    using Opc.Classic.Dcom.Internal.LegacyNdr;
    using System;

    /// <summary>
    /// A connection oriented pdu
    /// </summary>
    public abstract class ConnectionOrientedPdu : NdrOp, IProtocolDataUnit {

        /// <summary>
        /// Flag indicating the PDU is the first fragment.
        /// </summary>
        public const int PFC_FIRST_FRAG = 0x01;

        /// <summary>
        /// Flag indicating the PDU is the last fragment.
        /// </summary>
        public const int PFC_LAST_FRAG = 0x02;

        /// <summary>
        /// Flag indicating a cancel was pending at the sender.
        /// </summary>
        public const int PFC_PENDING_CANCEL = 0x04;

        /// <summary>
        /// Flag indicating support for concurrent multiplexing of a
        /// single connection.
        /// </summary>
        public const int PFC_CONC_MPX = 0x10;

        /// <summary>
        /// Flag for fault PDUs; if set, indicates that the call definitely
        /// did not execute.
        /// </summary>
        public const int PFC_DID_NOT_EXECUTE = 0x20;

        /// <summary>
        /// Flag for request PDU indicating oneway call semantics; no response will
        /// be provided by the server.
        /// </summary>
        public const int PFC_MAYBE = 0x40;

        /// <summary>
        /// Flag indicating a valid object UUID was specified and is present
        /// in the optional object field.  If not set, the object field is
        /// omitted.
        /// </summary>
        public const int PFC_OBJECT_UUID = 0x80;

        /// <summary>
        /// Type
        /// </summary>
        public abstract int Type { get; }

        /// <summary>
        /// Major version
        /// </summary>
        public int MajorVersion => CONNECTION_ORIENTED_MAJOR_VERSION;

        /// <summary>
        /// Minor version
        /// </summary>
        public int MinorVersion { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public NdrFormat Format {
            get => _format ?? (_format = NdrFormat.DEFAULT_FORMAT);
            set => _format = value;
        }

        /// <summary>
        /// Get flags
        /// </summary>
        public int Flags { get; set; } = PFC_FIRST_FRAG | PFC_LAST_FRAG;

        /// <summary>
        /// Test flag
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool GetFlag(int flag) =>
            (Flags & flag) != 0;

        /// <summary>
        /// Set flag
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public void SetFlag(int flag, bool value) =>
            Flags = value ? (Flags | flag) : (Flags & ~flag);

        /// <summary>
        /// Call id
        /// </summary>
        public int CallId {
            get => _callId;
            set {
                _useCallIdCounter = false;
                _callId = value;
            }
        }

        /// <summary>
        /// Fragment length
        /// </summary>
        public int FragmentLength { get; set; }

        /// <summary>
        /// Auth length
        /// </summary>
        public int AuthenticatorLength { get; set; }


        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="src"></param>
        public override void Decode(NdrCodec ndr, NdrBuffer src) {
            ndr.Buffer = src;
            ReadPdu(ndr);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="dst"></param>
        public override void Encode(NdrCodec ndr, NdrBuffer dst) {
            ndr.Buffer = dst;
            Format = Format;
            WritePdu(ndr);
            var buffer = ndr.Buffer;
            var length = buffer.Length;
            FragmentLength = length;
            // write the header lengths, now that we know them.
            buffer.Index = FRAG_LENGTH_OFFSET;
            ndr.WriteUnsignedShort(length);
            ndr.WriteUnsignedShort(AuthenticatorLength);
            buffer.Index = length;
        }

        /// <summary>
        /// Read pdu
        /// </summary>
        /// <param name="ndr"></param>
        protected internal virtual void ReadPdu(NdrCodec ndr) {
            ReadHeader(ndr);
            ReadBody(ndr);
        }

        /// <summary>
        /// Write pdu
        /// </summary>
        /// <param name="ndr"></param>
        protected internal virtual void WritePdu(NdrCodec ndr) {
            WriteHeader(ndr);
            WriteBody(ndr);
        }

        /// <summary>
        /// Read header
        /// </summary>
        /// <param name="ndr"></param>
        protected internal void ReadHeader(NdrCodec ndr) {
            if (ndr.ReadUnsignedSmall() != CONNECTION_ORIENTED_MAJOR_VERSION) {
                throw new InvalidOperationException("Version mismatch.");
            }
            // read minor version
            MinorVersion = ndr.ReadUnsignedSmall();
            if (Type != ndr.ReadUnsignedSmall()) {
                throw new ArgumentException("Incorrect PDU type.");
            }
            Flags = ndr.ReadUnsignedSmall();
            var format = ndr.ReadFormat(false);
            Format = format;
            Format = format;
            FragmentLength = ndr.ReadUnsignedShort();
            AuthenticatorLength = ndr.ReadUnsignedShort();
            _callId = ndr.ReadUnsignedLong();
        }

        /// <summary>
        /// Write header
        /// </summary>
        /// <param name="ndr"></param>
        protected internal void WriteHeader(NdrCodec ndr) {
            ndr.WriteUnsignedSmall((short)MajorVersion);
            ndr.WriteUnsignedSmall((short)MinorVersion);
            ndr.WriteUnsignedSmall((short)Type);
            ndr.WriteUnsignedSmall((short)Flags);
            ndr.WriteFormat(false);
            // skip the fragment and auth lengths, since we don't have them yet.
            ndr.WriteUnsignedShort(0);
            ndr.WriteUnsignedShort(0);
            ndr.WriteUnsignedLong(_useCallIdCounter ? s_callIdCounter++ : _callId);
        }

        /// <summary>
        /// Read body
        /// </summary>
        /// <param name="ndr"></param>
        protected internal virtual void ReadBody(NdrCodec ndr) {
            // override
        }

        /// <summary>
        /// Write body
        /// </summary>
        /// <param name="ndr"></param>
        protected internal virtual void WriteBody(NdrCodec ndr) {
            // override
        }

        /// <private/>
        public const int CONNECTION_ORIENTED_MAJOR_VERSION = 5;
        /// <private/>
        public const int MUST_RECEIVE_FRAGMENT_SIZE = 7160;
        /// <private/>
        public const int MAJOR_VERSION_OFFSET = 0;
        /// <private/>
        public const int MINOR_VERSION_OFFSET = 1;
        /// <private/>
        public const int TYPE_OFFSET = 2;
        /// <private/>
        public const int FLAGS_OFFSET = 3;
        /// <private/>
        public const int DATA_REPRESENTATION_OFFSET = 4;
        /// <private/>
        public const int FRAG_LENGTH_OFFSET = 8;
        /// <private/>
        public const int AUTH_LENGTH_OFFSET = 10;
        /// <private/>
        public const int CALL_ID_OFFSET = 12;
        /// <private/>
        public const int HEADER_LENGTH = 16;

        /// <summary> Call id counter </summary>
        protected internal static int s_callIdCounter;
        private int _callId = s_callIdCounter;
        private bool _useCallIdCounter = true;
        private NdrFormat _format;
    }
}