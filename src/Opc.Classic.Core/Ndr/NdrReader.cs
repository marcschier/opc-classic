//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Network Data Representation (NDR) primitive reader.
// Symmetric to NdrWriter — same alignment rules, same little-endian wire
// format. See NdrWriter.cs for the spec reference and design notes.
//

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Opc.Classic.Ndr;

/// <summary>
/// A forward-only span-based NDR reader.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public ref struct NdrReader
{
    /// <summary>Default maximum decoded payload, in bytes.</summary>
    public const int DefaultMaxPayloadSize = 16 * 1024 * 1024;

    private readonly ReadOnlySpan<byte> _buffer;
    private readonly int _maxPayloadSize;
    private int _position;

    /// <summary>Creates a new reader over the supplied buffer.</summary>
    public NdrReader(ReadOnlySpan<byte> buffer)
        : this(buffer, DefaultMaxPayloadSize)
    {
    }

    /// <summary>Creates a new reader over the supplied buffer with a decoded-payload quota.</summary>
    public NdrReader(ReadOnlySpan<byte> buffer, int maxPayloadSize)
    {
        if (maxPayloadSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadSize), maxPayloadSize, "NDR payload quota must be positive.");
        }

        if (buffer.Length > maxPayloadSize)
        {
            throw new InvalidOperationException($"NDR buffer length {buffer.Length} exceeds the configured quota of {maxPayloadSize} bytes.");
        }

        _buffer = buffer;
        _maxPayloadSize = maxPayloadSize;
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
            throw new InvalidOperationException($"NDR conformance header {value} exceeds Int32.MaxValue." + FormatContext());
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
        uint maxCount = ReadUInt32();
        uint offset = ReadUInt32();        // offset
        if (offset != 0u)
        {
            throw new InvalidOperationException($"NDR LPWSTR offset must be 0 but was {offset}." + FormatContext());
        }
        uint actualCount = ReadUInt32();   // actual_count (includes the NUL)
        if (actualCount > maxCount)
        {
            throw new InvalidOperationException($"NDR LPWSTR actual_count {actualCount} exceeds max_count {maxCount}." + FormatContext());
        }
        EnsureBoundedPayloadBytes(actualCount, sizeof(char), "NDR LPWSTR actual_count");

        int charCount = (int)actualCount;
        EnsureAvailable(charCount * sizeof(char));

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
        _position += charCount * sizeof(char);
        return new string(chars);
    }

    /// <summary>
    /// Reads an OLE Automation BSTR per [MS-OAUT] §2.2.23 — a referent
    /// followed by a FLAGGED_WORD_BLOB. Returns <see langword="null"/>
    /// when the referent is zero (null BSTR).
    /// </summary>
    /// <summary>Reads a [unique] BSTR (FLAGGED_WORD_BLOB) per MS-OAUT 2.2.23.</summary>
    /// <remarks>
    /// Wire layout: <c>referent + max_count + cBytes + clSize + char[clSize]</c>.
    /// max_count is the implicit conformant-array prefix (always equals clSize
    /// here per <c>[size_is(clSize)] asData[]</c>); cBytes is the byte count
    /// including any 4-byte padding; clSize is the character count without the
    /// trailing NUL. Older versions of this method omitted the max_count read,
    /// which broke interop with real OPC servers (Matrikon emits the spec
    /// layout). Symmetric loopback continued to work because the writer had the
    /// same bug — see Track AY+ NDR variant codec sweep.
    /// </remarks>
    public string? ReadBstr()
    {
        if (!TryReadReferentId(out _))
        {
            return null;
        }
        uint maxCount = ReadUInt32();
        uint cBytes = ReadUInt32();
        uint clSize = ReadUInt32();
        if (clSize > maxCount)
        {
            throw new InvalidOperationException(
                $"NDR BSTR clSize ({clSize}) exceeds max_count ({maxCount})." + FormatContext());
        }
        _ = cBytes; // informational; bounds enforced via clSize
        EnsureBoundedPayloadBytes(clSize, sizeof(char), "NDR BSTR clSize");
        int charCount = (int)clSize;
        EnsureAvailable(charCount * sizeof(char));
        var chars = new char[charCount];
        for (int i = 0; i < charCount; i++)
        {
            chars[i] = (char)BinaryPrimitives.ReadUInt16LittleEndian(
                _buffer.Slice(_position + i * 2, 2));
        }
        _position += charCount * sizeof(char);
        return new string(chars);
    }

    /// <summary>
    /// Reads a unique-pointer LPWSTR — a referent followed by the
    /// conformant-variant string body. Returns <see langword="null"/>
    /// when the referent is zero.
    /// </summary>
    public string? ReadUnicodeStringPtr()
    {
        if (!TryReadReferentId(out _))
        {
            return null;
        }
        return ReadUnicodeString();
    }

    /// <summary>Reads a span of raw bytes verbatim (no alignment, no length prefix).</summary>
    public ReadOnlySpan<byte> ReadRawBytes(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        EnsureBoundedPayloadBytes((uint)count, 1, "NDR raw byte read");
        EnsureAvailable(count);
        var span = _buffer.Slice(_position, count);
        _position += count;
        return span;
    }

    // -------- Conformant arrays of primitive types --------

    /// <summary>Reads a conformant array of bytes (uint count + raw bytes).</summary>
    public byte[] ReadConformantByteArray()
    {
        int count = ReadBoundedConformanceCount(1, "NDR conformant byte array");
        EnsureAvailable(count);
        var result = new byte[count];
        _buffer.Slice(_position, count).CopyTo(result);
        _position += count;
        return result;
    }

    /// <summary>Reads a conformant array of Int16 values.</summary>
    public short[] ReadConformantInt16Array()
    {
        int count = ReadBoundedConformanceCount(sizeof(short), "NDR conformant Int16 array");
        AlignTo(2);
        EnsureAvailable(count * sizeof(short));
        var result = new short[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(_position, 2));
            _position += 2;
        }
        return result;
    }

    /// <summary>Reads a conformant array of UInt16 values.</summary>
    public ushort[] ReadConformantUInt16Array()
    {
        int count = ReadBoundedConformanceCount(sizeof(ushort), "NDR conformant UInt16 array");
        AlignTo(2);
        EnsureAvailable(count * sizeof(ushort));
        var result = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(_position, 2));
            _position += 2;
        }
        return result;
    }

    /// <summary>Reads a conformant array of Int32 values.</summary>
    public int[] ReadConformantInt32Array()
    {
        int count = ReadBoundedConformanceCount(sizeof(int), "NDR conformant Int32 array");
        EnsureAvailable(count * sizeof(int));
        var result = new int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(_position, 4));
            _position += 4;
        }
        return result;
    }

    /// <summary>Reads a conformant array of UInt32 values.</summary>
    public uint[] ReadConformantUInt32Array()
    {
        int count = ReadBoundedConformanceCount(sizeof(uint), "NDR conformant UInt32 array");
        EnsureAvailable(count * sizeof(uint));
        var result = new uint[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position, 4));
            _position += 4;
        }
        return result;
    }

    /// <summary>Reads a conformant array of Int64 values.</summary>
    public long[] ReadConformantInt64Array()
    {
        int count = ReadBoundedConformanceCount(sizeof(long), "NDR conformant Int64 array");
        AlignTo(8);
        EnsureAvailable(count * sizeof(long));
        var result = new long[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(_position, 8));
            _position += 8;
        }
        return result;
    }

    /// <summary>
    /// Reads a conformant array of Windows FILETIME values. Each element is
    /// two consecutive 32-bit halves (low, high) with 4-byte alignment — the
    /// element layout matches <see cref="ReadFileTime"/>, NOT 8-byte-aligned
    /// Int64. Used for IDL <c>[out, size_is(N)] FILETIME *p</c> arrays.
    /// </summary>
    public long[] ReadConformantFileTimeArray()
    {
        int count = ReadBoundedConformanceCount(sizeof(long), "NDR conformant FILETIME array");
        EnsureAvailable(count * sizeof(long));
        var result = new long[count];
        for (int i = 0; i < count; i++)
        {
            uint low = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position, 4));
            uint high = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position + 4, 4));
            result[i] = unchecked((long)(((ulong)high << 32) | low));
            _position += 8;
        }
        return result;
    }

    /// <summary>Reads a conformant array of Single (float) values.</summary>
    public float[] ReadConformantSingleArray()
    {
        int count = ReadBoundedConformanceCount(sizeof(float), "NDR conformant Single array");
        EnsureAvailable(count * sizeof(float));
        var result = new float[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadSingleLittleEndian(_buffer.Slice(_position, 4));
            _position += 4;
        }
        return result;
    }

    /// <summary>Reads a conformant array of Double values.</summary>
    public double[] ReadConformantDoubleArray()
    {
        int count = ReadBoundedConformanceCount(sizeof(double), "NDR conformant Double array");
        AlignTo(8);
        EnsureAvailable(count * sizeof(double));
        var result = new double[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = BinaryPrimitives.ReadDoubleLittleEndian(_buffer.Slice(_position, 8));
            _position += 8;
        }
        return result;
    }

    /// <summary>Reads a conformant array of Guid values.</summary>
    public Guid[] ReadConformantGuidArray()
    {
        int count = ReadBoundedConformanceCount(16, "NDR conformant Guid array");
        AlignTo(4);
        EnsureAvailable(count * 16);
        var result = new Guid[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = new Guid(_buffer.Slice(_position, 16));
            _position += 16;
        }
        return result;
    }

    private int ReadBoundedConformanceCount(int elementSize, string context)
    {
        int count = ReadConformanceHeader();
        EnsureBoundedPayloadBytes((uint)count, elementSize, context);
        return count;
    }

    private void EnsureBoundedPayloadBytes(uint count, int elementSize, string context)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(elementSize);
        ulong byteCount = (ulong)count * (uint)elementSize;
        if (byteCount > (uint)_maxPayloadSize)
        {
            throw new InvalidOperationException(
                $"{context} requires {byteCount} bytes, which exceeds the configured NDR quota of {_maxPayloadSize} bytes." + FormatContext());
        }

        if (byteCount > int.MaxValue)
        {
            throw new InvalidOperationException($"{context} requires {byteCount} bytes, which exceeds Int32.MaxValue." + FormatContext());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureAvailable(int requiredBytes)
    {
        if (requiredBytes < 0 || _position > _buffer.Length - requiredBytes)
        {
            throw new InvalidOperationException(
                $"NdrReader past end-of-buffer: need {requiredBytes} bytes at position {_position} but only {_buffer.Length - _position} remain." + FormatContext());
        }
    }

    /// <summary>
    /// Formats a hex window around <see cref="Position"/> for use in decode-failure
    /// exception messages. Renders up to <paramref name="contextBytes"/> bytes on either
    /// side, marks the failure offset with <c>>></c>, and includes an ASCII gutter so a
    /// developer can eyeball the bytes when comparing against a Wireshark capture or a
    /// canonical MIDL layout.
    /// </summary>
    /// <remarks>
    /// Returns a leading newline + multi-line block when context is available, or an
    /// empty string when the buffer is empty / quota-violating writes have not yet
    /// produced wire bytes. Callers concatenate the result onto the base failure
    /// message so single-line tooling output still renders cleanly when no context
    /// is available.
    /// </remarks>
    public string FormatContext(int contextBytes = 16) =>
        FormatHexContext(_buffer, _position, contextBytes);

    /// <summary>
    /// Static helper that formats a hex window around the supplied offset. Exposed so
    /// codecs (e.g. <c>NdrVariantExtensions</c>) that hold their own reference to the
    /// buffer can produce identically-shaped diagnostic blocks.
    /// </summary>
    public static string FormatHexContext(ReadOnlySpan<byte> buffer, int position, int contextBytes = 16)
    {
        if (buffer.IsEmpty || contextBytes <= 0)
        {
            return string.Empty;
        }
        int start = Math.Max(0, position - contextBytes);
        int end = Math.Min(buffer.Length, position + contextBytes);
        if (end <= start)
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder(160);
        sb.Append("\nWire context (bytes ").Append(start).Append("..").Append(end)
            .Append(", >> marks position ").Append(position).AppendLine("):");
        const int RowBytes = 16;
        int rowStart = start - (start % RowBytes);
        for (int row = rowStart; row < end; row += RowBytes)
        {
            sb.Append("  ").Append(row.ToString("X4", System.Globalization.CultureInfo.InvariantCulture)).Append(": ");
            var hex = new System.Text.StringBuilder(48);
            var ascii = new System.Text.StringBuilder(16);
            for (int col = 0; col < RowBytes; col++)
            {
                int idx = row + col;
                if (idx < start || idx >= end || idx >= buffer.Length)
                {
                    hex.Append("   ");
                    ascii.Append(' ');
                    continue;
                }
                if (idx == position)
                {
                    hex.Append(">>");
                }
                else
                {
                    hex.Append(' ');
                }
                byte b = buffer[idx];
                hex.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
                ascii.Append(b >= 0x20 && b < 0x7F ? (char)b : '.');
            }
            sb.Append(hex).Append("  ").Append(ascii).AppendLine();
        }
        return sb.ToString();
    }
}
