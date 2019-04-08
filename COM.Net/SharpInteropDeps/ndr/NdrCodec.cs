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

namespace SharpCifs.Dcerpc.Ndr {

    /// <summary>
    /// Encoder
    /// </summary>
    public class NdrCodec {

        /// <summary>
        /// Uuid
        /// </summary>
        public const string NDR_UUID = "8a885d04-1ceb-11c9-9fe8-08002b104860";

        /// <summary>
        /// Major version
        /// </summary>
        public const int NDR_MAJOR_VERSION = 2;

        /// <summary>
        /// Minor
        /// </summary>
        public const int NDR_MINOR_VERSION = 0;

        /// <summary>
        /// Syntax
        /// </summary>
        public static readonly string NDR_SYNTAX =
            NDR_UUID + ":" + NDR_MAJOR_VERSION + "." + NDR_MINOR_VERSION;

        /// <summary>
        /// Buffer
        /// </summary>
        public NdrBuffer Buffer { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public NdrFormat Format { get; set; }

        /// <summary>
        /// Current pointer
        /// </summary>
        public int Ptr { get; set; }

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean() => Buffer.Dec_ndr_small() != 0;

        /// <summary>
        /// Write boolean
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(bool value) => Buffer.Enc_ndr_small(value ? 1 : 0);

        /// <summary>
        /// Read unsigned small
        /// </summary>
        /// <returns></returns>
        public int ReadUnsignedSmall() => Buffer.Dec_ndr_small();

        /// <summary>
        /// read unsigned short
        /// </summary>
        /// <returns></returns>
        public int ReadUnsignedShort() => Buffer.Dec_ndr_short();

        /// <summary>
        /// Read unsigned long
        /// </summary>
        /// <returns></returns>
        public int ReadUnsignedLong() => Buffer.Dec_ndr_long();

        /// <summary>
        /// Write unsigned small
        /// </summary>
        /// <param name="value"></param>
        public void WriteUnsignedSmall(int value) => Buffer.Enc_ndr_small(value);

        /// <summary>
        /// Write unsigned short
        /// </summary>
        /// <param name="value"></param>
        public void WriteUnsignedShort(int value) => Buffer.Enc_ndr_short(value);

        /// <summary>
        /// Write unsigned long
        /// </summary>
        /// <param name="value"></param>
        public void WriteUnsignedLong(int value) => Buffer.Enc_ndr_long(value);

        /// <summary>
        /// Read format
        /// </summary>
        /// <param name="connectionless"></param>
        /// <returns></returns>
        public NdrFormat ReadFormat(bool connectionless) {
            var format = NdrFormat.ReadFormat(Buffer.Buf, Buffer.Index, connectionless);
            Buffer.Index += 4;
            return format;
        }

        /// <summary>
        /// Write format
        /// </summary>
        /// <param name="format"></param>
        public void WriteFormat(NdrFormat format) {
            format.WriteFormat(Buffer.Buf, Buffer.Index, false);
            Buffer.Index += 4;
        }

        /// <summary>
        /// Write format
        /// </summary>
        /// <param name="connectionless"></param>
        public void WriteFormat(bool connectionless) {
            var index = Buffer.Index;
            Buffer.Index += connectionless ? 3 : 4;
            Format.WriteFormat(Buffer.Buf, index, connectionless);
        }

        /// <summary>
        /// Read char array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void ReadCharacterArray(char[] array, int offset, int length) {
            if (array == null || length == 0) {
                return;
            }
            length += offset;
            // won't work for EBCDIC
            for (var i = offset; i < length; i++) {
                array[i] = (char)Buffer.Buf[Buffer.Index++];
            }
        }

        /// <summary>
        /// Write char array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteCharacterArray(char[] array, int offset, int length) {
            if (array == null || length == 0) {
                return;
            }
            length += offset;
            // won't work for EBCDIC
            for (var i = offset; i < length; i++) {
                Buffer.Buf[Buffer.Index++] = (byte)array[i];
            }
        }

        /// <summary>
        /// Write octet array
        /// </summary>
        /// <param name="b"></param>
        /// <param name="i"></param>
        /// <param name="l"></param>
        public void WriteOctetArray(byte[] b, int i, int l) => 
            Buffer.WriteOctetArray(b, i, l);

        /// <summary>
        /// Read octet array
        /// </summary>
        /// <param name="b"></param>
        /// <param name="i"></param>
        /// <param name="l"></param>
        public void ReadOctetArray(byte[] b, int i, int l) => 
            Buffer.ReadOctetArray(b, i, l);


        /// <summary>
        /// Skip to alignment boundary
        /// </summary>
        /// <param name="alignment"></param>
        public void SkipAligned(int alignment) {
            var index = Buffer.Index;
            var skip = index % alignment;
            if (skip == 0) {
                return;
            }
            // Skip remainder
            skip = alignment - skip;
            ReadOctetArray(new byte[skip], 0, skip);
        }

        /// <summary>
        /// Fill to alignment boundary
        /// </summary>
        /// <param name="alignment"></param>
        public void FillAligned(int alignment) {
            var index = Buffer.Index;
            var skip = index % alignment;
            if (skip == 0) {
                return;
            }
            // Skip remainder
            skip = alignment - skip;
            WriteOctetArray(new byte[skip], 0, skip);
        }
    }
}