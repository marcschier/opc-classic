using System;
using System.Collections;

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

namespace ndr {


	using Encdec = jcifs.util.Encdec;

	public class NdrBuffer {
		internal int Referent;
		internal Hashtable Referents;

		internal class Entry {
			internal int Referent;
			internal object Obj;
		}

		public sbyte[] Buf;
		public int Start;
		public int Index_Renamed;
		public int Length_Renamed;
		public NdrBuffer Deferred;

		public bool IgnoreAlign = false;

		public NdrBuffer(sbyte[] buf, int start) {
			this.Buf = buf;
			this.Start = Index_Renamed = start;
			Length_Renamed = 0;
			Deferred = this;
		}

		public virtual NdrBuffer Derive(int idx) {
			NdrBuffer nb = new NdrBuffer(Buf, Start);
			nb.Index_Renamed = idx;
			nb.Deferred = Deferred;
			nb.IgnoreAlign = IgnoreAlign;
			return nb;
		}



		public virtual void Reset() {
			this.Index_Renamed = Start;
			Length_Renamed = 0;
			Deferred = this;
		}
		public virtual int Index {
			get {
				return Index_Renamed;
			}
			set {
				this.Index_Renamed = value;
			}
		}
		public virtual int Capacity {
			get {
				return Buf.Length - Start;
			}
		}
		public virtual sbyte[] Buffer {
			get {
				return Buf;
			}
		}
		public virtual int Align(int boundary, sbyte value) {
			if (IgnoreAlign) {
				return 0;
			}
			int n = Align(boundary);
			int i = n;
			while (i > 0) {
				Buf[Index_Renamed - i] = value;
				i--;
			}
			return n;
		}
		public virtual void WriteOctetArray(sbyte[] b, int i, int l) {
			Array.Copy(b, i, Buf, Index_Renamed, l);
			Advance(l);
		}
		public virtual void ReadOctetArray(sbyte[] b, int i, int l) {
			Array.Copy(Buf, Index_Renamed, b, i, l);
			Advance(l);
		}


		public virtual int Length {
			get {
				return Deferred.Length_Renamed;
			}
		}
		public virtual void Advance(int n) {
			Index_Renamed += n;
			if ((Index_Renamed - Start) > Deferred.Length_Renamed) {
				Deferred.Length_Renamed = Index_Renamed - Start;
			}
		}
		public virtual int Align(int boundary) {
			if (IgnoreAlign) {
				return 0;
			}
			int m = boundary - 1;
			int i = Index_Renamed - Start;
			int n = ((i + m) & ~m) - i;
			Advance(n);
			return n;
		}
		public virtual void Enc_ndr_small(int s) {
			Buf[Index_Renamed] = unchecked((sbyte)(s & 0xFF));
			Advance(1);
		}
		public virtual int Dec_ndr_small() {
			int val = Buf[Index_Renamed] & 0xFF;
			Advance(1);
			return val;
		}
		public virtual void Enc_ndr_short(int s) {
			Align(2);
			Encdec.enc_uint16le((short)s, Buf, Index_Renamed);
			Advance(2);
		}
		public virtual int Dec_ndr_short() {
			Align(2);
			int val = Encdec.dec_uint16le(Buf, Index_Renamed);
			Advance(2);
			return val;
		}
		public virtual void Enc_ndr_long(int l) {
			Align(4);
			Encdec.enc_uint32le(l, Buf, Index_Renamed);
			Advance(4);
		}
		public virtual int Dec_ndr_long() {
			Align(4);
			int val = Encdec.dec_uint32le(Buf, Index_Renamed);
			Advance(4);
			return val;
		}
		/* float */
		/* double */
		public virtual void Enc_ndr_string(string s) {
			Align(4);
			int i = Index_Renamed;
			int len = s.Length;
			Encdec.enc_uint32le(len + 1, Buf, i);
			i += 4;
			Encdec.enc_uint32le(0, Buf, i);
			i += 4;
			Encdec.enc_uint32le(len + 1, Buf, i);
			i += 4;
			try {
				Array.Copy(s.GetBytes("UnicodeLittleUnmarked"), 0, Buf, i, len * 2);
			}
			catch (UnsupportedEncodingException) {
			}
			i += len * 2;
			Buf[i++] = (sbyte)'\0';
			Buf[i++] = (sbyte)'\0';
			Advance(i - Index_Renamed);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String dec_ndr_string() throws NdrException
		public virtual string Dec_ndr_string() {
			Align(4);
			int i = Index_Renamed;
			string val = null;
			int len = Encdec.dec_uint32le(Buf, i);
			i += 12;
			if (len != 0) {
				len--;
				int size = len * 2;
				try {
					if (size < 0 || size > 0xFFFF) {
						throw new NdrException(NdrException.INVALID_CONFORMANCE);
					}
					val = StringHelperClass.NewString(Buf, i, size, "UnicodeLittle");
					i += size + 2;
				}
				catch (UnsupportedEncodingException) {
				}
			}
			Advance(i - Index_Renamed);
			return val;
		}
		private int GetDceReferent(object obj) {
			Entry e;

			if (Referents == null) {
				Referents = new Hashtable();
				Referent = 1;
			}

			if ((e = (Entry)Referents.GetValueOrNull(obj)) == null) {
				e = new Entry();
				e.Referent = Referent++;
				e.Obj = obj;
				Referents[obj] = e;
			}

			return e.Referent;
		}
		public virtual void Enc_ndr_referent(object obj, int type) {
			if (obj == null) {
				Enc_ndr_long(0);
				return;
			}
			switch (type) {
				case 1: // unique
				case 3: // ref
					Enc_ndr_long(System.identityHashCode(obj));
					return;
				case 2: // ptr
					Enc_ndr_long(GetDceReferent(obj));
					return;
			}
		}

		public override string ToString() {
			return "start=" + Start + ",index=" + Index_Renamed + ",length=" + Length;
		}
	}


}