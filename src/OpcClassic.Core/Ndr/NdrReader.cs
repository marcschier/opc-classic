//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// Network Data Representation (NDR) primitive reader.
// Symmetric to NdrWriter — same alignment rules, same little-endian wire
// format. See NdrWriter.cs for the spec reference and design notes.
//

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpcClassic.Ndr;

/// <summary>
/// A forward-only span-based NDR reader.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public ref struct NdrReader
{
    private readonly ReadOnlySpan<byte> _buffer;
    private int _position;

    /// <summary>Creates a new reader over the supplied buffer.</summary>
    public NdrReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    /// <summary>Current byte position in the buffer (also the number of bytes consumed).</summary>
    public int Position => _position;

    /// <summary>Total length of the underlying buffer.</summary>
    public int Length => _buffer.Length;

    /// <summary>Remaining readable bytes.</summary>
    public int RemainingBytes => _buffer.Length - _position;

    /// <summary>
    /// Aligns the position to the given power-of-two boundary by consuming
    /// (but not validating) padding bytes. <paramref name="boundary"/> must
    /// be 1, 2, 4, or 8.
    /// </summary>
    public void AlignTo(int boundary)
    {
        if (boundary != 1 && boundary != 2 && boundary != 4 && boundary != 8)
        {
            throw new ArgumentOutOfRangeException(nameof(boundary), boundary, "Alignment must be 1, 2, 4, or 8.");
        }

        int misaligned = _position & (boundary - 1);
        if (misaligned == 0)
        {
            return;
        }

        int padding = boundary - misaligned;
        EnsureAvailable(padding);
        _position += padding;
    }

    public byte ReadByte()
    {
        EnsureAvailable(1);
        byte value = _buffer[_position];
        _position += 1;
        return value;
    }

    public bool ReadBoolean() => ReadByte() != 0;

    public sbyte ReadInt8() => unchecked((sbyte)ReadByte());

    public byte ReadUInt8() => ReadByte();

    public short ReadInt16()
    {
        AlignTo(2);
        EnsureAvailable(2);
        short value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(_position, 2));
        _position += 2;
        return value;
    }

    public ushort ReadUInt16()
    {
        AlignTo(2);
        EnsureAvailable(2);
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(_position, 2));
        _position += 2;
        return value;
    }

    public int ReadInt32()
    {
        AlignTo(4);
        EnsureAvailable(4);
        int value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(_position, 4));
        _position += 4;
        return value;
    }

    public uint ReadUInt32()
    {
        AlignTo(4);
        EnsureAvailable(4);
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position, 4));
        _position += 4;
        return value;
    }

    public long ReadInt64()
    {
        AlignTo(8);
        EnsureAvailable(8);
        long value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(_position, 8));
        _position += 8;
        return value;
    }

    public ulong ReadUInt64()
    {
        AlignTo(8);
        EnsureAvailable(8);
        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(_position, 8));
        _position += 8;
        return value;
    }

    public float ReadSingle()
    {
        AlignTo(4);
        EnsureAvailable(4);
        float value = BinaryPrimitives.ReadSingleLittleEndian(_buffer.Slice(_position, 4));
        _position += 4;
        return value;
    }

    public double ReadDouble()
    {
        AlignTo(8);
        EnsureAvailable(8);
        double value = BinaryPrimitives.ReadDoubleLittleEndian(_buffer.Slice(_position, 8));
        _position += 8;
        return value;
    }

    public Guid ReadGuid()
    {
        AlignTo(4);
        EnsureAvailable(16);
        var guid = new Guid(_buffer.Slice(_position, 16));
        _position += 16;
        return guid;
    }

    /// <summary>
    /// Reads a FILETIME (two little-endian uint halves, low first). Returns
    /// the value as Int64 100-nanosecond intervals since 1601-01-01 UTC.
    /// </summary>
    public long ReadFileTime()
    {
        AlignTo(4);
        EnsureAvailable(8);
        uint low = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position, 4));
        uint high = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position + 4, 4));
        _position += 8;
        return unchecked((long)(((ulong)high << 32) | low));
    }

    /// <summary>Reads a conformance header (a single uint, aligned to 4).</summary>
    public int ReadConformanceHeader()
    {
        uint value = ReadUInt32();
        if (value > int.MaxValue)
        {
            throw new InvalidOperationException($"NDR conformance header {value} exceeds Int32.MaxValue.");
        }
        return unchecked((int)value);
    }

    /// <summary>
    /// Reads a 4-byte referent ID. Returns true if the referent is non-null
    /// (out parameter contains the ID); false if null (referent = 0).
    /// </summary>
    public bool TryReadReferentId(out uint referentId)
    {
        referentId = ReadUInt32();
        return referentId != 0u;
    }

    /// <summary>
    /// Reads a conformant + variant Unicode string per the LPWSTR convention:
    ///   uint max_count
    ///   uint offset (validated to be 0)
    ///   uint actual_count
    ///   wchar[actual_count]
    /// The trailing null terminator IS counted in actual_count per DCOM
    /// convention; this method strips it from the returned string. If the
    /// terminator is missing the entire buffer is returned as-is.
    /// </summary>
    public string ReadUnicodeString()
    {
        AlignTo(4);
        _ = ReadUInt32();                  // max_count — discarded; we trust actual
        uint offset = ReadUInt32();        // offset
        if (offset != 0u)
        {
            throw new InvalidOperationException($"NDR LPWSTR offset must be 0 but was {offset}.");
        }
        uint actualCount = ReadUInt32();   // actual_count (includes the NUL)
        if (actualCount > (uint)int.MaxValue / 2)
        {
            throw new InvalidOperationException($"NDR LPWSTR actual_count {actualCount} too large.");
        }

        int charCount = (int)actualCount;
        EnsureAvailable(charCount * 2);

        // Drop the trailing NUL if present.
        int effective = charCount;
        if (effective > 0)
        {
            ushort last = BinaryPrimitives.ReadUInt16LittleEndian(
                _buffer.Slice(_position + (effective - 1) * 2, 2));
            if (last == 0)
            {
                effective -= 1;
            }
        }

        var chars = new char[effective];
        for (int i = 0; i < effective; i++)
        {
            chars[i] = (char)BinaryPrimitives.ReadUInt16LittleEndian(
                _buffer.Slice(_position + i * 2, 2));
        }
        _position += charCount * 2;
        return new string(chars);
    }

    /// <summary>Reads a span of raw bytes verbatim (no alignment, no length prefix).</summary>
    public ReadOnlySpan<byte> ReadRawBytes(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        EnsureAvailable(count);
        var span = _buffer.Slice(_position, count);
        _position += count;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureAvailable(int requiredBytes)
    {
        if (_position + requiredBytes > _buffer.Length)
        {
            throw new InvalidOperationException(
                $"NdrReader past end-of-buffer: need {requiredBytes} bytes at position {_position} but only {_buffer.Length - _position} remain.");
        }
    }
}
