using System;
using System.Text;

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

namespace rpc.core
{

	using NdrBuffer = ndr.NdrBuffer;
	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class UUID : NdrObject
	{

		public const string NIL_UUID = "00000000-0000-0000-0000-000000000000";

		private const int TIMELOW_INDEX = 0;

		private const int TIMEMID_INDEX = 1;

		private const int TIMEHIGHANDVERSION_INDEX = 2;

		private const int CLOCKSEQHIGHANDRESERVED_INDEX = 3;

		private const int CLOCKSEQLOW_INDEX = 4;

		private const int NODE_INDEX = 5;

		internal int timeLow, timeMid, timeHighAndVersion, clockSeqHighAndReserved, clockSeqLow;
		internal sbyte[] node = new sbyte[6];

		public UUID()
		{
		}
		public UUID(string uuid)
		{
			parse(uuid);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer dst) throws ndr.NdrException
		public override void encode(NetworkDataRepresentation ndr, NdrBuffer dst)
		{
			dst.enc_ndr_long(timeLow);
			dst.enc_ndr_short(timeMid);
			dst.enc_ndr_short(timeHighAndVersion);
			dst.enc_ndr_small(clockSeqHighAndReserved);
			dst.enc_ndr_small(clockSeqLow);
			Array.Copy(node, 0, dst.buf, dst.index, 6);
			dst.index += 6;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer src) throws ndr.NdrException
		public override void decode(NetworkDataRepresentation ndr, NdrBuffer src)
		{
			timeLow = src.dec_ndr_long();
			timeMid = src.dec_ndr_short();
			timeHighAndVersion = src.dec_ndr_short();
			clockSeqHighAndReserved = src.dec_ndr_small();
			clockSeqLow = src.dec_ndr_small();
			Array.Copy(src.buf, src.index, node, 0, 6);
			src.index += 6;
		}
	/*
	    public long getTimeLow() {
	        return ((UnsignedLongHolder)
	                structure.get(TIMELOW_INDEX)).getUnsignedLong();
	    }
	
	    public void setTimeLow(long timeLow) {
	        ((UnsignedLongHolder) structure.get(TIMELOW_INDEX)).setUnsignedLong(
	                timeLow);
	    }
	
	    public int getTimeMid() {
	        return ((UnsignedShortHolder)
	                structure.get(TIMEMID_INDEX)).getUnsignedShort();
	    }
	
	    public void setTimeMid(int timeMid) {
	        ((UnsignedShortHolder) structure.get(TIMEMID_INDEX)).setUnsignedShort(
	                timeMid);
	    }
	
	    public int getTimeHighAndVersion() {
	        return ((UnsignedShortHolder)
	                structure.get(TIMEHIGHANDVERSION_INDEX)).getUnsignedShort();
	    }
	
	    public void setTimeHighAndVersion(int timeHighAndVersion) {
	        ((UnsignedShortHolder)
	                structure.get(TIMEHIGHANDVERSION_INDEX)).setUnsignedShort(
	                        timeHighAndVersion);
	    }
	
	    public short getClockSeqHighAndReserved() {
	        return ((UnsignedSmallHolder) structure.get(
	                CLOCKSEQHIGHANDRESERVED_INDEX)).getUnsignedSmall();
	    }
	
	    public void setClockSeqHighAndReserved(short clockSeqHighAndReserved) {
	        ((UnsignedSmallHolder) structure.get(
	                CLOCKSEQHIGHANDRESERVED_INDEX)).setUnsignedSmall(
	                        clockSeqHighAndReserved);
	    }
	
	    public short getClockSeqLow() {
	        return ((UnsignedSmallHolder) structure.get(
	                CLOCKSEQLOW_INDEX)).getUnsignedSmall();
	    }
	
	    public void setClockSeqLow(short clockSeqLow) {
	        ((UnsignedSmallHolder) structure.get(
	                CLOCKSEQLOW_INDEX)).setUnsignedSmall(clockSeqLow);
	    }
	
	    public byte[] getNode() {
	        return (byte[]) ((FixedArray) structure.get(NODE_INDEX)).getArray();
	    }
	
	    public void setNode(byte[] node) {
	        ((FixedArray) structure.get(NODE_INDEX)).setArray(node);
	    }
	*/

		public override string ToString()
		{
			var buffer = new StringBuilder();
	//        int timeLow = (int) (getTimeLow() & 0xffffffffl);
			buffer.Append(((timeLow >> 28) & 0x0f).ToString("x"));
			buffer.Append(((timeLow >> 24) & 0x0f).ToString("x"));
			buffer.Append(((timeLow >> 20) & 0x0f).ToString("x"));
			buffer.Append(((timeLow >> 16) & 0x0f).ToString("x"));
			buffer.Append(((timeLow >> 12) & 0x0f).ToString("x"));
			buffer.Append(((timeLow >> 8) & 0x0f).ToString("x"));
			buffer.Append(((timeLow >> 4) & 0x0f).ToString("x"));
			buffer.Append((timeLow & 0x0f).ToString("x"));
			buffer.Append('-');
	//        int timeMid = getTimeMid();
			buffer.Append(((timeMid >> 12) & 0x0f).ToString("x"));
			buffer.Append(((timeMid >> 8) & 0x0f).ToString("x"));
			buffer.Append(((timeMid >> 4) & 0x0f).ToString("x"));
			buffer.Append((timeMid & 0x0f).ToString("x"));
			buffer.Append('-');
	//        int timeHighAndVersion = getTimeHighAndVersion();
			buffer.Append(((timeHighAndVersion >> 12) & 0x0f).ToString("x"));
			buffer.Append(((timeHighAndVersion >> 8) & 0x0f).ToString("x"));
			buffer.Append(((timeHighAndVersion >> 4) & 0x0f).ToString("x"));
			buffer.Append((timeHighAndVersion & 0x0f).ToString("x"));
			buffer.Append('-');
	//        short clockSeqHighAndReserved = getClockSeqHighAndReserved();
			buffer.Append(((clockSeqHighAndReserved >> 4) & 0x0f).ToString("x"));
			buffer.Append((clockSeqHighAndReserved & 0x0f).ToString("x"));
	//        short clockSeqLow = getClockSeqLow();
			buffer.Append(((clockSeqLow >> 4) & 0x0f).ToString("x"));
			buffer.Append((clockSeqLow & 0x0f).ToString("x"));
			buffer.Append('-');
	//        byte[] node = getNode();
			for (var i = 0; i < 6; i++)
			{
				buffer.Append(((node[i] >> 4) & 0x0f).ToString("x"));
				buffer.Append((node[i] & 0x0f).ToString("x"));
			}
			return buffer.ToString();
		}

		public virtual void parse(string uuid)
		{
			var tokenizer = new StringTokenizer(uuid, "-");
			timeLow = (int)long.Parse(tokenizer.nextToken(), 16);
			timeMid = int.Parse(tokenizer.nextToken(), 16);
			timeHighAndVersion = int.Parse(tokenizer.nextToken(), 16);
			string token = tokenizer.nextToken();
			clockSeqHighAndReserved = int.Parse(token.Substring(0, 2), 16);
			clockSeqLow = int.Parse(token.Substring(2), 16);
			token = tokenizer.nextToken();
			node = new sbyte[6];
			for (var i = 0; i < 6; i++)
			{
				var offset = i * 2;
				node[i] = (sbyte)((char.digit(token[offset], 16) << 4) | char.digit(token[offset + 1], 16));
			}
	/*
	        setTimeLow(timeLow);
	        setTimeMid(timeMid);
	        setTimeHighAndVersion(timeHighAndVersion);
	        setClockSeqHighAndReserved(clockSeqHighAndReserved);
	        setClockSeqLow(clockSeqLow);
	        setNode(node);
	*/
		}
	}

}