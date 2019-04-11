//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpCifs.Dcerpc.Ndr {

    /// <summary>
    /// Format
    /// </summary>
    public class NdrFormat {

        /// <summary>
        /// LE
        /// </summary>
        public const int LITTLE_ENDIAN = 0x10000000;

        /// <summary>
        /// BE
        /// </summary>
        public const int BIG_ENDIAN = 0x00000000;

        /// <summary>
        /// Ascii
        /// </summary>
        public const int ASCII_CHARACTER = 0x00000000;

        /// <summary>
        /// Ebcdic
        /// </summary>
        public const int EBCDIC_CHARACTER = 0x01000000;

        /// <summary>
        /// Ieee float
        /// </summary>
        public const int IEEE_FLOATING_POINT = 0x00000000;

        /// <summary>
        /// Vax float
        /// </summary>
        public const int VAX_FLOATING_POINT = 0x00010000;

        /// <summary>
        /// Cray float
        /// </summary>
        public const int CRAY_FLOATING_POINT = 0x00100000;

        /// <summary>
        /// Ibm float
        /// </summary>
        public const int IBM_FLOATING_POINT = 0x00110000;

        /// <summary>
        /// Default
        /// </summary>
        public static readonly int DEFAULT_DATA_REPRESENTATION =
            LITTLE_ENDIAN | ASCII_CHARACTER | IEEE_FLOATING_POINT;

        /// <summary>
        /// Default format
        /// </summary>
        public static readonly NdrFormat DEFAULT_FORMAT = new NdrFormat(DEFAULT_DATA_REPRESENTATION);

        /// <summary>
        /// Create new format
        /// </summary>
        /// <param name="dataRepresentation"></param>
        public NdrFormat(int dataRepresentation) {
            DataRepresentation = dataRepresentation;
            if ((dataRepresentation & BYTE_ORDER_MASK) != LITTLE_ENDIAN) {
                throw new System.ArgumentException("Only little-endian byte order is currently supported.");
            }
            if ((dataRepresentation & CHARACTER_MASK) != ASCII_CHARACTER) {
                throw new System.ArgumentException("Only ASCII character set is currently supported.");
            }
            if ((dataRepresentation & FLOATING_POINT_MASK) != IEEE_FLOATING_POINT) {
                throw new System.ArgumentException("Only IEEE floating point is currently supported.");
            }
        }

        /// <summary>
        /// Representation
        /// </summary>
        public int DataRepresentation { get; }

        /// <summary>
        /// read
        /// </summary>
        /// <param name="src"></param>
        /// <param name="index"></param>
        /// <param name="connectionless"></param>
        /// <returns></returns>
        public static NdrFormat ReadFormat(byte[] src, int index, bool connectionless) {
            var value = src[index++] << 24;
            value |= (src[index++] & 0xff) << 16;
            value |= (src[index++] & 0xff) << 8;
            if (!connectionless) {
                value |= src[index] & 0xff;
            }
            return new NdrFormat(value);
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="index"></param>
        /// <param name="connectionless"></param>
        public void WriteFormat(byte[] dest, int index, bool connectionless) {
            var val = DataRepresentation;
            dest[index++] = unchecked((byte)((val >> 24) & 0xff));
            dest[index++] = unchecked((byte)((val >> 16) & 0xff));
            dest[index] = 0x00;
            if (!connectionless) {
                dest[++index] = 0x00;
            }
        }

        /// <summary>
        /// Byte order
        /// </summary>
        internal const int BYTE_ORDER_MASK = unchecked((int)0xf0000000);

        /// <summary>
        /// Character
        /// </summary>
        internal const int CHARACTER_MASK = 0x0f000000;

        /// <summary>
        /// Float
        /// </summary>
        internal const int FLOATING_POINT_MASK = 0x00ff0000;
    }
}