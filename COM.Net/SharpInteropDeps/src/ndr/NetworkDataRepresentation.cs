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
// Contributors:
// Vikram Roopchand  - Moving to EPL from LGPL v1.
// 

namespace ndr
{

	using Hexdump = jcifs.util.Hexdump;

	public class NetworkDataRepresentation
	{

		public const string NDR_UUID = "8a885d04-1ceb-11c9-9fe8-08002b104860";

		public const int NDR_MAJOR_VERSION = 2;

		public const int NDR_MINOR_VERSION = 0;

		public static readonly string NDR_SYNTAX = NDR_UUID + ":" + NDR_MAJOR_VERSION + "." + NDR_MINOR_VERSION;

		public int ptr;
		public NdrBuffer buf;
		public Format format;

		public NetworkDataRepresentation()
		{
		}

		public virtual NdrBuffer Buffer {
            set => buf = value;
            get => buf;
        }

        public virtual void hexdump(int count)
		{
			Hexdump.hexdump(System.err, buf.buf, buf.index, count);
		}
		public virtual bool readBoolean()
		{
			return buf.dec_ndr_small() == 0 ? false : true;
		}
		public virtual void writeBoolean(bool value)
		{
			buf.enc_ndr_small(value ? 1 : 0);
		}
		public virtual int readUnsignedSmall()
		{
			return buf.dec_ndr_small();
		}
		public virtual int readUnsignedShort()
		{
			return buf.dec_ndr_short();
		}
		public virtual int readUnsignedLong()
		{
			return buf.dec_ndr_long();
		}
		public virtual void writeUnsignedSmall(int value)
		{
			buf.enc_ndr_small(value);
		}
		public virtual void writeUnsignedShort(int value)
		{
			buf.enc_ndr_short(value);
		}
		public virtual void writeUnsignedLong(int value)
		{
			buf.enc_ndr_long(value);
		}

		public virtual Format Format {
            set => format = value;
            get => format;
        }
        public virtual Format readFormat(bool connectionless)
		{
			var format = Format.readFormat(buf.buf, buf.index, connectionless);
			buf.index += 4;
			return format;
		}
		public virtual void writeFormat(Format format)
		{
			format.writeFormat(buf.buf, buf.index, false);
			buf.index += 4;
		}
		public virtual void writeFormat(bool connectionless)
		{
			var index = buf.Index;
			buf.index += connectionless ? 3 : 4;
			format.writeFormat(buf.buf, index, connectionless);
		}

		public virtual void readCharacterArray(char[] array, int offset, int length)
		{
			if (array == null || length == 0)
			{
				return;
			}
			length += offset;
			// won't work for EBCDIC
			for (var i = offset; i < length; i++)
			{
				array[i] = (char) buf.buf[buf.index++];
			}
		}
		public virtual void writeCharacterArray(char[] array, int offset, int length)
		{
			if (array == null || length == 0)
			{
				return;
			}
			length += offset;
			// won't work for EBCDIC
			for (var i = offset; i < length; i++)
			{
				buf.buf[buf.index++] = (sbyte) array[i];
			}
		}
		public virtual void writeOctetArray(sbyte[] b, int i, int l)
		{
			buf.writeOctetArray(b, i, l);
		}
		public virtual void readOctetArray(sbyte[] b, int i, int l)
		{
			buf.readOctetArray(b, i, l);
		}
	}

}