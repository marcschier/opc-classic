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

    using Hexdump = jcifs.util.Hexdump;

    public class NetworkDataRepresentation {

        public const string NDR_UUID = "8a885d04-1ceb-11c9-9fe8-08002b104860";

        public const int NDR_MAJOR_VERSION = 2;

        public const int NDR_MINOR_VERSION = 0;

        public static readonly string NDR_SYNTAX = NDR_UUID + ":" + NDR_MAJOR_VERSION + "." + NDR_MINOR_VERSION;

        public int Ptr;
        public NdrBuffer Buf;
        public Format Format_Renamed;

        public NetworkDataRepresentation() {
        }

        public virtual NdrBuffer Buffer {
            set {
                this.Buf = value;
            }
            get {
                return Buf;
            }
        }

        public virtual void Hexdump(int count) {
            Hexdump.hexdump(System.err, Buf.Buf, Buf.Index_Renamed, count);
        }
        public virtual bool ReadBoolean() {
            return Buf.Dec_ndr_small() == 0 ? false : true;
        }
        public virtual void WriteBoolean(bool value) {
            Buf.Enc_ndr_small(value ? 1 : 0);
        }
        public virtual int ReadUnsignedSmall() {
            return Buf.Dec_ndr_small();
        }
        public virtual int ReadUnsignedShort() {
            return Buf.Dec_ndr_short();
        }
        public virtual int ReadUnsignedLong() {
            return Buf.Dec_ndr_long();
        }
        public virtual void WriteUnsignedSmall(int value) {
            Buf.Enc_ndr_small(value);
        }
        public virtual void WriteUnsignedShort(int value) {
            Buf.Enc_ndr_short(value);
        }
        public virtual void WriteUnsignedLong(int value) {
            Buf.Enc_ndr_long(value);
        }

        public virtual Format Format {
            set {
                this.Format_Renamed = value;
            }
            get {
                return Format_Renamed;
            }
        }
        public virtual Format ReadFormat(bool connectionless) {
            Format format = Format.ReadFormat(Buf.Buf, Buf.Index_Renamed, connectionless);
            Buf.Index_Renamed += 4;
            return format;
        }
        public virtual void WriteFormat(Format format) {
            format.WriteFormat(Buf.Buf, Buf.Index_Renamed, false);
            Buf.Index_Renamed += 4;
        }
        public virtual void WriteFormat(bool connectionless) {
            int index = Buf.Index;
            Buf.Index_Renamed += connectionless ? 3 : 4;
            Format_Renamed.WriteFormat(Buf.Buf, index, connectionless);
        }

        public virtual void ReadCharacterArray(char[] array, int offset, int length) {
            if (array == null || length == 0) {
                return;
            }
            length += offset;
            // won't work for EBCDIC
            for (int i = offset; i < length; i++) {
                array[i] = (char) Buf.Buf[Buf.Index_Renamed++];
            }
        }
        public virtual void WriteCharacterArray(char[] array, int offset, int length) {
            if (array == null || length == 0) {
                return;
            }
            length += offset;
            // won't work for EBCDIC
            for (int i = offset; i < length; i++) {
                Buf.Buf[Buf.Index_Renamed++] = (sbyte) array[i];
            }
        }
        public virtual void WriteOctetArray(sbyte[] b, int i, int l) {
            Buf.WriteOctetArray(b, i, l);
        }
        public virtual void ReadOctetArray(sbyte[] b, int i, int l) {
            Buf.ReadOctetArray(b, i, l);
        }
    }

}