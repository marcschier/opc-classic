using System;
using System.Text;

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

namespace rpc.core {

    using NdrBuffer = ndr.NdrBuffer;
    using NdrException = ndr.NdrException;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    public class UUID : NdrObject {

        public const string NIL_UUID = "00000000-0000-0000-0000-000000000000";

        private const int TIMELOW_INDEX = 0;

        private const int TIMEMID_INDEX = 1;

        private const int TIMEHIGHANDVERSION_INDEX = 2;

        private const int CLOCKSEQHIGHANDRESERVED_INDEX = 3;

        private const int CLOCKSEQLOW_INDEX = 4;

        private const int NODE_INDEX = 5;

        internal int TimeLow, TimeMid, TimeHighAndVersion, ClockSeqHighAndReserved, ClockSeqLow;
        internal sbyte[] Node = new sbyte[6];

        public UUID() {
        }
        public UUID(string uuid) {
            Parse(uuid);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer dst) throws ndr.NdrException
        public override void Encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
            dst.Enc_ndr_long(TimeLow);
            dst.Enc_ndr_short(TimeMid);
            dst.Enc_ndr_short(TimeHighAndVersion);
            dst.Enc_ndr_small(ClockSeqHighAndReserved);
            dst.Enc_ndr_small(ClockSeqLow);
            Array.Copy(Node, 0, dst.Buf, dst.Index_Renamed, 6);
            dst.Index_Renamed += 6;
        }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer src) throws ndr.NdrException
        public override void Decode(NetworkDataRepresentation ndr, NdrBuffer src) {
            TimeLow = src.Dec_ndr_long();
            TimeMid = src.Dec_ndr_short();
            TimeHighAndVersion = src.Dec_ndr_short();
            ClockSeqHighAndReserved = src.Dec_ndr_small();
            ClockSeqLow = src.Dec_ndr_small();
            Array.Copy(src.Buf, src.Index_Renamed, Node, 0, 6);
            src.Index_Renamed += 6;
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

        public override string ToString() {
            StringBuilder buffer = new StringBuilder();
    //        int timeLow = (int) (getTimeLow() & 0xffffffffl);
            buffer.Append(((TimeLow >> 28) & 0x0f).ToString("x"));
            buffer.Append(((TimeLow >> 24) & 0x0f).ToString("x"));
            buffer.Append(((TimeLow >> 20) & 0x0f).ToString("x"));
            buffer.Append(((TimeLow >> 16) & 0x0f).ToString("x"));
            buffer.Append(((TimeLow >> 12) & 0x0f).ToString("x"));
            buffer.Append(((TimeLow >> 8) & 0x0f).ToString("x"));
            buffer.Append(((TimeLow >> 4) & 0x0f).ToString("x"));
            buffer.Append((TimeLow & 0x0f).ToString("x"));
            buffer.Append('-');
    //        int timeMid = getTimeMid();
            buffer.Append(((TimeMid >> 12) & 0x0f).ToString("x"));
            buffer.Append(((TimeMid >> 8) & 0x0f).ToString("x"));
            buffer.Append(((TimeMid >> 4) & 0x0f).ToString("x"));
            buffer.Append((TimeMid & 0x0f).ToString("x"));
            buffer.Append('-');
    //        int timeHighAndVersion = getTimeHighAndVersion();
            buffer.Append(((TimeHighAndVersion >> 12) & 0x0f).ToString("x"));
            buffer.Append(((TimeHighAndVersion >> 8) & 0x0f).ToString("x"));
            buffer.Append(((TimeHighAndVersion >> 4) & 0x0f).ToString("x"));
            buffer.Append((TimeHighAndVersion & 0x0f).ToString("x"));
            buffer.Append('-');
    //        short clockSeqHighAndReserved = getClockSeqHighAndReserved();
            buffer.Append(((ClockSeqHighAndReserved >> 4) & 0x0f).ToString("x"));
            buffer.Append((ClockSeqHighAndReserved & 0x0f).ToString("x"));
    //        short clockSeqLow = getClockSeqLow();
            buffer.Append(((ClockSeqLow >> 4) & 0x0f).ToString("x"));
            buffer.Append((ClockSeqLow & 0x0f).ToString("x"));
            buffer.Append('-');
    //        byte[] node = getNode();
            for (int i = 0; i < 6; i++) {
                buffer.Append(((Node[i] >> 4) & 0x0f).ToString("x"));
                buffer.Append((Node[i] & 0x0f).ToString("x"));
            }
            return buffer.ToString();
        }

        public virtual void Parse(string uuid) {
            StringTokenizer tokenizer = new StringTokenizer(uuid, "-");
            TimeLow = (int)long.Parse(tokenizer.nextToken(), 16);
            TimeMid = int.Parse(tokenizer.nextToken(), 16);
            TimeHighAndVersion = int.Parse(tokenizer.nextToken(), 16);
            string token = tokenizer.nextToken();
            ClockSeqHighAndReserved = int.Parse(token.Substring(0, 2), 16);
            ClockSeqLow = int.Parse(token.Substring(2), 16);
            token = tokenizer.nextToken();
            Node = new sbyte[6];
            for (int i = 0; i < 6; i++) {
                int offset = i * 2;
                Node[i] = (sbyte)((char.digit(token[offset], 16) << 4) | char.digit(token[offset + 1], 16));
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