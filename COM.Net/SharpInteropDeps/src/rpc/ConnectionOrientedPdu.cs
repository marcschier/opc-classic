using System;

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

namespace rpc
{

	using Format = ndr.Format;
	using NdrBuffer = ndr.NdrBuffer;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public abstract class ConnectionOrientedPdu : NdrObject, ProtocolDataUnit
	{

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

		private int minorVersion;

		private int flags = PFC_FIRST_FRAG | PFC_LAST_FRAG;

		protected internal static int callIdCounter;

		private int callId = callIdCounter;

		private bool useCallIdCounter = true;

		private int fragLength;

		private int authLength;

		private Format format;

        public virtual int MajorVersion => CONNECTION_ORIENTED_MAJOR_VERSION;

        public virtual int MinorVersion {
            get => minorVersion;
            set => minorVersion = value;
        }


        public virtual Format Format {
            get => format ?? (format = Format.DEFAULT_FORMAT);
            set => format = value;
        }


        public virtual int Flags {
            get => flags;
            set => flags = value;
        }


        public virtual bool getFlag(int flag)
		{
			return (Flags & flag) != 0;
		}

		public virtual void setFlag(int flag, bool value)
		{
			Flags = value ? (Flags | flag) : (Flags & ~flag);
		}

		public virtual int CallId {
            get => callId;
            set {
                useCallIdCounter = false;
                callId = value;
            }
        }


        public virtual int FragmentLength {
            get => fragLength;
            set => fragLength = value;
        }


        public virtual int AuthenticatorLength {
            get => authLength;
            set => authLength = value;
        }


        public override void decode(NetworkDataRepresentation ndr, NdrBuffer src)
		{
			ndr.Buffer = src;
			readPdu(ndr);
		}

		public override void encode(NetworkDataRepresentation ndr, NdrBuffer dst)
		{
			ndr.Buffer = dst;
			Format = Format;
			writePdu(ndr);
			var buffer = ndr.Buffer;
			var length = buffer.Length;
			FragmentLength = length;
			// write the header lengths, now that we know them.
			buffer.Index = FRAG_LENGTH_OFFSET;
			ndr.writeUnsignedShort(length);
			ndr.writeUnsignedShort(AuthenticatorLength);
			buffer.Index = length;
		}

		protected internal virtual void readPdu(NetworkDataRepresentation ndr)
		{
			readHeader(ndr);
			readBody(ndr);
		}

		protected internal virtual void writePdu(NetworkDataRepresentation ndr)
		{
			writeHeader(ndr);
			writeBody(ndr);
		}

		protected internal virtual void readHeader(NetworkDataRepresentation ndr)
		{
			if (ndr.readUnsignedSmall() != CONNECTION_ORIENTED_MAJOR_VERSION)
			{
				throw new InvalidOperationException("Version mismatch.");
			}
			// read minor version
			MinorVersion = ndr.readUnsignedSmall();
			if (Type != ndr.readUnsignedSmall())
			{
				throw new ArgumentException("Incorrect PDU type.");
			}
			Flags = ndr.readUnsignedSmall();
			var format = ndr.readFormat(false);
			Format = format;
			Format = format;
			FragmentLength = ndr.readUnsignedShort();
			AuthenticatorLength = ndr.readUnsignedShort();
			callId = (int) ndr.readUnsignedLong();
		}

		protected internal virtual void writeHeader(NetworkDataRepresentation ndr)
		{
			ndr.writeUnsignedSmall((short) MajorVersion);
			ndr.writeUnsignedSmall((short) MinorVersion);
			ndr.writeUnsignedSmall((short) Type);
			ndr.writeUnsignedSmall((short) Flags);
			ndr.writeFormat(false);
			// skip the fragment and auth lengths, since we don't have them yet.
			ndr.writeUnsignedShort(0);
			ndr.writeUnsignedShort(0);
			ndr.writeUnsignedLong(useCallIdCounter ? callIdCounter++: callId);
		}

		protected internal virtual void readBody(NetworkDataRepresentation ndr)
		{
		}

		protected internal virtual void writeBody(NetworkDataRepresentation ndr)
		{
		}

		public abstract int Type {get;}

	}

}