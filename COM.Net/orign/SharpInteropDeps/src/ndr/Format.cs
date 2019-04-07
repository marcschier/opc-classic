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

	public class Format {

		public const int LITTLE_ENDIAN = 0x10000000;

		public const int BIG_ENDIAN = 0x00000000;

		public const int ASCII_CHARACTER = 0x00000000;

		public const int EBCDIC_CHARACTER = 0x01000000;

		public const int IEEE_FLOATING_POINT = 0x00000000;

		public const int VAX_FLOATING_POINT = 0x00010000;

		public const int CRAY_FLOATING_POINT = 0x00100000;

		public const int IBM_FLOATING_POINT = 0x00110000;

		public static readonly int DEFAULT_DATA_REPRESENTATION = LITTLE_ENDIAN | ASCII_CHARACTER | IEEE_FLOATING_POINT;

		public static readonly Format DEFAULT_FORMAT = new Format(DEFAULT_DATA_REPRESENTATION);

		internal const int BYTE_ORDER_MASK = unchecked((int)0xf0000000);

		internal const int CHARACTER_MASK = 0x0f000000;

		internal const int FLOATING_POINT_MASK = 0x00ff0000;

		private readonly int DataRepresentation_Renamed;

		public Format(int dataRepresentation) {
			this.DataRepresentation_Renamed = dataRepresentation;
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

		public virtual int DataRepresentation {
			get {
				return DataRepresentation_Renamed;
			}
		}

		public static Format ReadFormat(sbyte[] src, int index, bool connectionless) {
			int value = src[index++] << 24;
			value |= (src[index++] & 0xff) << 16;
			value |= (src[index++] & 0xff) << 8;
			if (!connectionless) {
				value |= src[index] & 0xff;
			}
			return new Format(value);
		}

		public virtual void WriteFormat(sbyte[] dest, int index, bool connectionless) {
			int val = DataRepresentation;
			dest[index++] = unchecked((sbyte)((val >> 24) & 0xff));
			dest[index++] = unchecked((sbyte)((val >> 16) & 0xff));
			dest[index] = (sbyte) 0x00;
			if (!connectionless) {
				dest[++index] = (sbyte) 0x00;
			}
		}

	}

}