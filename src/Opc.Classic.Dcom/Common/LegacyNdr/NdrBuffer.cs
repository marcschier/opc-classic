// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Opc.Classic.Dcom.Internal.LegacyNdr;

public class NdrBuffer
{
    public byte[] Buf;
    public int Start;
    public int Index;
    public int Length;
    public NdrBuffer Deferred;

    public NdrBuffer(byte[] buf, int start)
    {
        Buf = buf ?? throw new ArgumentNullException(nameof(buf));
        if ((uint)start > (uint)buf.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        Start = start;
        Index = start;
        Length = 0;
        Deferred = this;
    }

    private NdrBuffer(byte[] buf, int start, int index, int length, NdrBuffer deferred)
    {
        Buf = buf;
        Start = start;
        Index = index;
        Length = length;
        Deferred = deferred;
    }

    public NdrBuffer Derive(int index) => new(Buf, Start, index, Length, Deferred ?? this);

    public void Reset()
    {
        Index = Start;
        Length = 0;
        Deferred = this;
    }

    public int GetIndex() => Index;

    public void SetIndex(int index) => Index = index;

    public int GetCapacity() => Buf.Length - Start;

    public int GetTailSpace() => Buf.Length - Index;

    public byte[] GetBuffer() => Buf;

    public int GetLength() => Length;

    public void SetLength(int length) => Length = length;

    public void Advance(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        EnsureWritable(n);
        Index += n;
        UpdateLength();
    }

    public int Align(int boundary) => Align(boundary, 0);

    public int Align(int boundary, byte value)
    {
        if (boundary <= 0 || (boundary & (boundary - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(boundary), "Alignment must be a positive power of two.");
        }

        int misaligned = Index & (boundary - 1);
        if (misaligned == 0)
        {
            return 0;
        }

        int padding = boundary - misaligned;
        EnsureWritable(padding);
        Array.Fill(Buf, value, Index, padding);
        Index += padding;
        UpdateLength();
        return padding;
    }

    public void WriteOctetArray(byte[] b, int i, int l)
    {
        ValidateArrayRange(b, i, l);
        EnsureWritable(l);
        Buffer.BlockCopy(b, i, Buf, Index, l);
        Index += l;
        UpdateLength();
    }

    public void ReadOctetArray(byte[] b, int i, int l)
    {
        ValidateArrayRange(b, i, l);
        EnsureReadable(l);
        Buffer.BlockCopy(Buf, Index, b, i, l);
        Index += l;
    }

    public void Enc_ndr_small(int s)
    {
        EnsureWritable(1);
        Buf[Index++] = unchecked((byte)(s & 0xff));
        UpdateLength();
    }

    public int Dec_ndr_small()
    {
        EnsureReadable(1);
        return Buf[Index++] & 0xff;
    }

    public void Enc_ndr_short(int s)
    {
        Align(2);
        EnsureWritable(2);
        BinaryPrimitives.WriteUInt16LittleEndian(Buf.AsSpan(Index, 2), unchecked((ushort)s));
        Index += 2;
        UpdateLength();
    }

    public int Dec_ndr_short()
    {
        Align(2);
        EnsureReadable(2);
        int value = BinaryPrimitives.ReadUInt16LittleEndian(Buf.AsSpan(Index, 2));
        Index += 2;
        return value;
    }

    public void Enc_ndr_long(int l)
    {
        Align(4);
        EnsureWritable(4);
        BinaryPrimitives.WriteInt32LittleEndian(Buf.AsSpan(Index, 4), l);
        Index += 4;
        UpdateLength();
    }

    public int Dec_ndr_long()
    {
        Align(4);
        EnsureReadable(4);
        int value = BinaryPrimitives.ReadInt32LittleEndian(Buf.AsSpan(Index, 4));
        Index += 4;
        return value;
    }

    public void Enc_ndr_hyper(long h)
    {
        Align(8);
        EnsureWritable(8);
        BinaryPrimitives.WriteInt64LittleEndian(Buf.AsSpan(Index, 8), h);
        Index += 8;
        UpdateLength();
    }

    public long Dec_ndr_hyper()
    {
        Align(8);
        EnsureReadable(8);
        long value = BinaryPrimitives.ReadInt64LittleEndian(Buf.AsSpan(Index, 8));
        Index += 8;
        return value;
    }

    public void Enc_ndr_string(string value)
    {
        if (value is null)
        {
            Enc_ndr_long(0);
            return;
        }

        Align(4);
        int count = value.Length + 1;
        Enc_ndr_long(count);
        Enc_ndr_long(0);
        Enc_ndr_long(count);
        EnsureWritable(count * 2);
        for (int i = 0; i < value.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(Buf.AsSpan(Index, 2), value[i]);
            Index += 2;
        }

        BinaryPrimitives.WriteUInt16LittleEndian(Buf.AsSpan(Index, 2), 0);
        Index += 2;
        UpdateLength();
    }

    public string Dec_ndr_string()
    {
        Align(4);
        int maxCount = Dec_ndr_long();
        int offset = Dec_ndr_long();
        int actualCount = Dec_ndr_long();
        if (maxCount < 0 || offset < 0 || actualCount < 0 || offset + actualCount > maxCount)
        {
            throw new NdrException(NdrException.InvalidConformance);
        }

        EnsureReadable(actualCount * 2);
        int effectiveCount = actualCount;
        if (effectiveCount > 0 && BinaryPrimitives.ReadUInt16LittleEndian(Buf.AsSpan(Index + ((effectiveCount - 1) * 2), 2)) == 0)
        {
            effectiveCount--;
        }

        var chars = new char[effectiveCount];
        for (int i = 0; i < effectiveCount; i++)
        {
            chars[i] = (char)BinaryPrimitives.ReadUInt16LittleEndian(Buf.AsSpan(Index + (i * 2), 2));
        }

        Index += actualCount * 2;
        return new string(chars);
    }

    public void Enc_ndr_referent(object obj, int type)
    {
        if (obj is null)
        {
            Enc_ndr_long(0);
            return;
        }

        Enc_ndr_long(RuntimeHelpers.GetHashCode(obj));
    }

    public override string ToString() => $"start={Start},index={Index},length={Length}";

    private void EnsureWritable(int count)
    {
        if (Index < 0 || count < 0 || Index + count > Buf.Length)
        {
            throw new NdrException("NDR buffer overflow.");
        }
    }

    private void EnsureReadable(int count)
    {
        int readableLimit = Length > 0 ? Math.Min(Buf.Length, Start + Length) : Buf.Length;
        if (Index < 0 || count < 0 || Index + count > readableLimit)
        {
            throw new EndOfStreamException("NDR buffer underflow.");
        }
    }

    private void UpdateLength()
    {
        int used = Index - Start;
        if (used > Length)
        {
            Length = used;
        }

        if (Deferred is not null && !ReferenceEquals(Deferred, this) && used > Deferred.Length)
        {
            Deferred.Length = used;
        }
    }

    private static void ValidateArrayRange(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
    }
}
