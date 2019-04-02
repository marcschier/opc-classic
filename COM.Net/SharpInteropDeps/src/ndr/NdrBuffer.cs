using System;
using System.Collections;

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


	using Encdec = jcifs.util.Encdec;

	public class NdrBuffer
	{
		internal int referent;
		internal Hashtable referents;

		internal class Entry
		{
			internal int referent;
			internal object obj;
		}

		public sbyte[] buf;
		public int start;
		public int index;
		public int length;
		public NdrBuffer deferred;

		public bool ignoreAlign;

		public NdrBuffer(sbyte[] buf, int start)
		{
			this.buf = buf;
			this.start = index = start;
			length = 0;
			deferred = this;
		}

		public virtual NdrBuffer derive(int idx)
		{
            var nb = new NdrBuffer(buf, start) {
                index = idx,
                deferred = deferred,
                ignoreAlign = ignoreAlign
            };
            return nb;
		}



		public virtual void reset()
		{
			index = start;
			length = 0;
			deferred = this;
		}
		public virtual int Index {
            get => index;
            set => index = value;
        }
        public virtual int Capacity => buf.Length - start;
        public virtual sbyte[] Buffer => buf;
        public virtual int align(int boundary, sbyte value)
		{
			if (ignoreAlign)
			{
				return 0;
			}
			var n = align(boundary);
			var i = n;
			while (i > 0)
			{
				buf[index - i] = value;
				i--;
			}
			return n;
		}
		public virtual void writeOctetArray(sbyte[] b, int i, int l)
		{
			Array.Copy(b, i, buf, index, l);
			advance(l);
		}
		public virtual void readOctetArray(sbyte[] b, int i, int l)
		{
			Array.Copy(buf, index, b, i, l);
			advance(l);
		}


        public virtual int Length => deferred.length;
        public virtual void advance(int n)
		{
			index += n;
			if ((index - start) > deferred.length)
			{
				deferred.length = index - start;
			}
		}
		public virtual int align(int boundary)
		{
			if (ignoreAlign)
			{
				return 0;
			}
			var m = boundary - 1;
			var i = index - start;
			var n = ((i + m) & ~m) - i;
			advance(n);
			return n;
		}
		public virtual void enc_ndr_small(int s)
		{
			buf[index] = unchecked((sbyte)(s & 0xFF));
			advance(1);
		}
		public virtual int dec_ndr_small()
		{
			var val = buf[index] & 0xFF;
			advance(1);
			return val;
		}
		public virtual void enc_ndr_short(int s)
		{
			align(2);
			Encdec.enc_uint16le((short)s, buf, index);
			advance(2);
		}
		public virtual int dec_ndr_short()
		{
			align(2);
			int val = Encdec.dec_uint16le(buf, index);
			advance(2);
			return val;
		}
		public virtual void enc_ndr_long(int l)
		{
			align(4);
			Encdec.enc_uint32le(l, buf, index);
			advance(4);
		}
		public virtual int dec_ndr_long()
		{
			align(4);
			int val = Encdec.dec_uint32le(buf, index);
			advance(4);
			return val;
		}
		/* float */
		/* double */
		public virtual void enc_ndr_string(string s)
		{
			align(4);
			var i = index;
			var len = s.Length;
			Encdec.enc_uint32le(len + 1, buf, i);
			i += 4;
			Encdec.enc_uint32le(0, buf, i);
			i += 4;
			Encdec.enc_uint32le(len + 1, buf, i);
			i += 4;
			try
			{
				Array.Copy(s.GetBytes("UnicodeLittleUnmarked"), 0, buf, i, len * 2);
			}
			catch (UnsupportedEncodingException)
			{
			}
			i += len * 2;
			buf[i++] = (sbyte)'\0';
			buf[i++] = (sbyte)'\0';
			advance(i - index);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String dec_ndr_string() throws NdrException
		public virtual string dec_ndr_string()
		{
			align(4);
			var i = index;
			string val = null;
			int len = Encdec.dec_uint32le(buf, i);
			i += 12;
			if (len != 0)
			{
				len--;
				var size = len * 2;
				try
				{
					if (size < 0 || size > 0xFFFF)
					{
						throw new NdrException(NdrException.INVALID_CONFORMANCE);
					}
					val = StringHelperClass.NewString(buf, i, size, "UnicodeLittle");
					i += size + 2;
				}
				catch (UnsupportedEncodingException)
				{
				}
			}
			advance(i - index);
			return val;
		}
		private int getDceReferent(object obj)
		{
			Entry e;

			if (referents == null)
			{
				referents = new Hashtable();
				referent = 1;
			}

			if ((e = (Entry)referents[obj]) == null)
			{
                e = new Entry {
                    referent = referent++,
                    obj = obj
                };
                referents[obj] = e;
			}

			return e.referent;
		}
		public virtual void enc_ndr_referent(object obj, int type)
		{
			if (obj == null)
			{
				enc_ndr_long(0);
				return;
			}
			switch (type)
			{
				case 1: // unique
				case 3: // ref
					enc_ndr_long(System.identityHashCode(obj));
					return;
				case 2: // ptr
					enc_ndr_long(getDceReferent(obj));
					return;
			}
		}

		public override string ToString()
		{
			return "start=" + start + ",index=" + index + ",length=" + Length;
		}
	}


}