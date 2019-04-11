using System;

/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>

namespace rpc {

    using Format = ndr.Format;
    using NdrBuffer = ndr.NdrBuffer;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    public abstract class ConnectionOrientedPdu : NdrObject, ProtocolDataUnit {

        public const int CONNECTION_ORIENTED_MAJOR_VERSION = 5;

        public const int MUST_RECEIVE_FRAGMENT_SIZE = 7160;

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

        public const int MAJOR_VERSION_OFFSET = 0;

        public const int MINOR_VERSION_OFFSET = 1;

        public const int TYPE_OFFSET = 2;

        public const int FLAGS_OFFSET = 3;

        public const int DATA_REPRESENTATION_OFFSET = 4;

        public const int FRAG_LENGTH_OFFSET = 8;

        public const int AUTH_LENGTH_OFFSET = 10;

        public const int CALL_ID_OFFSET = 12;

        public const int HEADER_LENGTH = 16;

        private int MinorVersion_Renamed = 0;

        private int Flags_Renamed = PFC_FIRST_FRAG | PFC_LAST_FRAG;

        protected internal static int CallIdCounter = 0;

        private int CallId_Renamed = CallIdCounter;

        private bool UseCallIdCounter = true;

        private int FragLength = 0;

        private int AuthLength = 0;

        private Format Format_Renamed;

        public virtual int MajorVersion {
            get {
                return CONNECTION_ORIENTED_MAJOR_VERSION;
            }
        }

        public virtual int MinorVersion {
            get {
                return MinorVersion_Renamed;
            }
            set {
                this.MinorVersion_Renamed = value;
            }
        }


        public virtual Format Format {
            get {
                return (Format_Renamed != null) ? Format_Renamed : (Format_Renamed = Format.DEFAULT_FORMAT);
            }
            set {
                this.Format_Renamed = value;
            }
        }


        public virtual int Flags {
            get {
                return Flags_Renamed;
            }
            set {
                this.Flags_Renamed = value;
            }
        }


        public virtual bool GetFlag(int flag) {
            return (Flags & flag) != 0;
        }

        public virtual void SetFlag(int flag, bool value) {
            Flags = value ? (Flags | flag) : (Flags & ~flag);
        }

        public virtual int CallId {
            get {
                return CallId_Renamed;
            }
            set {
                UseCallIdCounter = false;
                this.CallId_Renamed = value;
            }
        }


        public virtual int FragmentLength {
            get {
                return FragLength;
            }
            set {
                this.FragLength = value;
            }
        }


        public virtual int AuthenticatorLength {
            get {
                return AuthLength;
            }
            set {
                this.AuthLength = value;
            }
        }


        public override void Decode(NetworkDataRepresentation ndr, NdrBuffer src) {
            ndr.Buffer = src;
            ReadPdu(ndr);
        }

        public override void Encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
            ndr.Buffer = dst;
            Format = Format;
            WritePdu(ndr);
            NdrBuffer buffer = ndr.Buffer;
            int length = buffer.Length;
            FragmentLength = length;
            // write the header lengths, now that we know them.
            buffer.Index = FRAG_LENGTH_OFFSET;
            ndr.WriteUnsignedShort(length);
            ndr.WriteUnsignedShort(AuthenticatorLength);
            buffer.Index = length;
        }

        public virtual void ReadPdu(NetworkDataRepresentation ndr) {
            ReadHeader(ndr);
            ReadBody(ndr);
        }

        public virtual void WritePdu(NetworkDataRepresentation ndr) {
            WriteHeader(ndr);
            WriteBody(ndr);
        }

        public virtual void ReadHeader(NetworkDataRepresentation ndr) {
            if (ndr.ReadUnsignedSmall() != CONNECTION_ORIENTED_MAJOR_VERSION) {
                throw new System.InvalidOperationException("Version mismatch.");
            }
            // read minor version
            MinorVersion = ndr.ReadUnsignedSmall();
            if (Type != ndr.ReadUnsignedSmall()) {
                throw new System.ArgumentException("Incorrect PDU type.");
            }
            Flags = ndr.ReadUnsignedSmall();
            Format format = ndr.ReadFormat(false);
            Format = format;
            Format = format;
            FragmentLength = ndr.ReadUnsignedShort();
            AuthenticatorLength = ndr.ReadUnsignedShort();
            this.CallId_Renamed = ((int) ndr.ReadUnsignedLong());
        }

        public virtual void WriteHeader(NetworkDataRepresentation ndr) {
            ndr.WriteUnsignedSmall((short) MajorVersion);
            ndr.WriteUnsignedSmall((short) MinorVersion);
            ndr.WriteUnsignedSmall((short) Type);
            ndr.WriteUnsignedSmall((short) Flags);
            ndr.WriteFormat(false);
            // skip the fragment and auth lengths, since we don't have them yet.
            ndr.WriteUnsignedShort(0);
            ndr.WriteUnsignedShort(0);
            ndr.WriteUnsignedLong(UseCallIdCounter ? CallIdCounter++: CallId_Renamed);
        }

        public virtual void ReadBody(NetworkDataRepresentation ndr) {
        }

        public virtual void WriteBody(NetworkDataRepresentation ndr) {
        }

        public abstract int Type { get; }

    }

}