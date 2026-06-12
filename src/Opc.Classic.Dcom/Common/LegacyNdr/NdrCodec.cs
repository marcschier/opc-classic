// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Internal.LegacyNdr;

public class NdrCodec
{
    public const string NDR_UUID = "8a885d04-1ceb-11c9-9fe8-08002b104860";
    public const int NDR_MAJOR_VERSION = 2;
    public const int NDR_MINOR_VERSION = 0;
    public static readonly string NDR_SYNTAX = NDR_UUID + ":" + NDR_MAJOR_VERSION + "." + NDR_MINOR_VERSION;

    public NdrBuffer Buffer { get; set; }
    public NdrFormat Format { get; set; }
    public int Ptr { get; set; }

    public bool ReadBoolean() => Buffer.Dec_ndr_small() != 0;
    public void WriteBoolean(bool value) => Buffer.Enc_ndr_small(value ? 1 : 0);
    public int ReadUnsignedSmall() => Buffer.Dec_ndr_small();
    public int ReadUnsignedShort() => Buffer.Dec_ndr_short();
    public int ReadUnsignedLong() => Buffer.Dec_ndr_long();
    public void WriteUnsignedSmall(int value) => Buffer.Enc_ndr_small(value);
    public void WriteUnsignedShort(int value) => Buffer.Enc_ndr_short(value);
    public void WriteUnsignedLong(int value) => Buffer.Enc_ndr_long(value);

    public NdrFormat ReadFormat(bool connectionless)
    {
        var format = NdrFormat.ReadFormat(Buffer.Buf, Buffer.Index, connectionless);
        Buffer.Index += 4;
        return format;
    }

    public void WriteFormat(NdrFormat format)
    {
        format.WriteFormat(Buffer.Buf, Buffer.Index, false);
        Buffer.Index += 4;
        Buffer.SetLength(System.Math.Max(Buffer.Length, Buffer.Index - Buffer.Start));
    }

    public void WriteFormat(bool connectionless)
    {
        int index = Buffer.Index;
        Buffer.Index += connectionless ? 3 : 4;
        Format.WriteFormat(Buffer.Buf, index, connectionless);
        Buffer.SetLength(System.Math.Max(Buffer.Length, Buffer.Index - Buffer.Start));
    }

    public void ReadCharacterArray(char[] array, int offset, int length)
    {
        if (array == null || length == 0)
        {
            return;
        }

        length += offset;
        for (int i = offset; i < length; i++)
        {
            array[i] = (char)Buffer.Buf[Buffer.Index++];
        }
    }

    public void WriteCharacterArray(char[] array, int offset, int length)
    {
        if (array == null || length == 0)
        {
            return;
        }

        length += offset;
        for (int i = offset; i < length; i++)
        {
            Buffer.Buf[Buffer.Index++] = (byte)array[i];
        }

        Buffer.SetLength(System.Math.Max(Buffer.Length, Buffer.Index - Buffer.Start));
    }

    public void WriteOctetArray(byte[] b, int i, int l) => Buffer.WriteOctetArray(b, i, l);
    public void ReadOctetArray(byte[] b, int i, int l) => Buffer.ReadOctetArray(b, i, l);

    public void SkipAligned(int alignment)
    {
        int index = Buffer.Index;
        int skip = index % alignment;
        if (skip == 0)
        {
            return;
        }

        skip = alignment - skip;
        ReadOctetArray(new byte[skip], 0, skip);
    }

    public void FillAligned(int alignment)
    {
        int index = Buffer.Index;
        int skip = index % alignment;
        if (skip == 0)
        {
            return;
        }

        skip = alignment - skip;
        WriteOctetArray(new byte[skip], 0, skip);
    }
}
