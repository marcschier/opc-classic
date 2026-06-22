// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Internal.LegacyNdr;

public class NdrFormat
{
    public const int LITTLE_ENDIAN = 0x10000000;
    public const int BIG_ENDIAN = 0x00000000;
    public const int ASCII_CHARACTER = 0x00000000;
    public const int EBCDIC_CHARACTER = 0x01000000;
    public const int IEEE_FLOATING_POINT = 0x00000000;
    public const int VAX_FLOATING_POINT = 0x00010000;
    public const int CRAY_FLOATING_POINT = 0x00100000;
    public const int IBM_FLOATING_POINT = 0x00110000;

    public static readonly int DEFAULT_DATA_REPRESENTATION = LITTLE_ENDIAN | ASCII_CHARACTER | IEEE_FLOATING_POINT;
    public static readonly NdrFormat DEFAULT_FORMAT = new(DEFAULT_DATA_REPRESENTATION);

    public NdrFormat(int dataRepresentation)
    {
        DataRepresentation = dataRepresentation;
        if ((dataRepresentation & BYTE_ORDER_MASK) != LITTLE_ENDIAN)
        {
            throw new ArgumentException("Only little-endian byte order is currently supported.", nameof(dataRepresentation));
        }

        if ((dataRepresentation & CHARACTER_MASK) != ASCII_CHARACTER)
        {
            throw new ArgumentException("Only ASCII character set is currently supported.", nameof(dataRepresentation));
        }

        if ((dataRepresentation & FLOATING_POINT_MASK) != IEEE_FLOATING_POINT)
        {
            throw new ArgumentException("Only IEEE floating point is currently supported.", nameof(dataRepresentation));
        }
    }

    public int DataRepresentation { get; }

    public static NdrFormat ReadFormat(byte[] src, int index, bool connectionless)
    {
        int value = src[index++] << 24;
        value |= (src[index++] & 0xff) << 16;
        value |= (src[index++] & 0xff) << 8;
        if (!connectionless)
        {
            value |= src[index] & 0xff;
        }

        return new NdrFormat(value);
    }

    public void WriteFormat(byte[] dest, int index, bool connectionless)
    {
        int value = DataRepresentation;
        dest[index++] = unchecked((byte)((value >> 24) & 0xff));
        dest[index++] = unchecked((byte)((value >> 16) & 0xff));
        dest[index] = 0x00;
        if (!connectionless)
        {
            dest[++index] = 0x00;
        }
    }

    internal const int BYTE_ORDER_MASK = unchecked((int)0xf0000000);
    internal const int CHARACTER_MASK = 0x0f000000;
    internal const int FLOATING_POINT_MASK = 0x00ff0000;
}
