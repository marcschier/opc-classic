// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// Network Data Representation (NDR) primitive writers.
//
// NDR is the wire-format spec used by DCE/RPC and (transitively) DCOM.
// Reference: DCE 1.1 RPC, "Transfer Syntax NDR" (Appendix N of the OpenGroup
// CAE specification), and [MS-RPCE] / [MS-DCOM] for Microsoft extensions.
//
// Key invariants:
//   - Little-endian on the wire (DCOM default). Big-endian NDR exists but is
//     not used by DCOM in practice; we hard-code little-endian here for AOT
//     safety and reduced branching.
//   - Every scalar is aligned to its natural size: byte=1, short=2, int=4,
//     long=8, single=4, double=8, Guid=4 (it's a struct of {uint, ushort,
//     ushort, byte[8]} — outer alignment is 4).
//   - Conformant arrays carry their length prefix as a uint at the
//     ConformanceHeader site, with the elements following at their alignment.
//   - Unique pointers carry a 4-byte referent ID (non-zero) followed by the
//     pointee payload; nullable as 0.
//
// This is the managed-side companion to the legacy Opc.Classic.Dcom NDR layer
// in Opc.Classic.Dcom. The writer here is span-based, no-allocation, and
// AOT-clean by construction. The opaque ref tracker is a simple monotonically
// increasing uint.
//

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Opc.Classic.Ndr;

/// <summary>
/// A forward-only span-based NDR writer.
/// </summary>
/// <remarks>
/// Wraps a caller-provided <see cref="Span{T}"/> and tracks the current write
/// position. All writes are little-endian and self-aligning per NDR rules.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public ref struct NdrWriter
{
    private readonly Span<byte> _buffer;
    private int _position;
    private uint _nextReferentId;

    /// <summary>
    /// Creates a new writer over the supplied buffer.
    /// </summary>
    public NdrWriter(Span<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
        _nextReferentId = 0x00020000u;  // matches DCE/RPC conventional starting referent
    }

    /// <summary>
    /// Current byte position in the buffer (also the number of bytes written).
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Capacity of the underlying buffer.
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Remaining writable bytes.
    /// </summary>
    public int RemainingBytes => _buffer.Length - _position;

    /// <summary>
    /// Aligns the position to the given power-of-two boundary by padding with
    /// zero bytes. <paramref name="boundary"/> must be one of {1, 2, 4, 8}.
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
        EnsureCapacity(padding);
        _buffer.Slice(_position, padding).Clear();
        _position += padding;
    }

    /// <summary>
    /// Writes a single byte (no alignment required — byte is 1-aligned).
    /// </summary>
    public void WriteByte(byte value)
    {
        EnsureCapacity(1);
        _buffer[_position] = value;
        _position += 1;
    }

    /// <summary>
    /// Writes a boolean as a 1-byte 0/1.
    /// </summary>
    public void WriteBoolean(bool value) => WriteByte(value ? (byte)1 : (byte)0);

    /// <summary>
    /// Writes a signed 8-bit integer.
    /// </summary>
    public void WriteInt8(sbyte value) => WriteByte(unchecked((byte)value));

    /// <summary>
    /// Writes an unsigned 8-bit integer (alias for <see cref="WriteByte"/>).
    /// </summary>
    public void WriteUInt8(byte value) => WriteByte(value);

    /// <summary>
    /// Writes a little-endian signed 16-bit integer, aligned to 2.
    /// </summary>
    public void WriteInt16(short value)
    {
        AlignTo(2);
        EnsureCapacity(2);
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_position, 2), value);
        _position += 2;
    }

    /// <summary>
    /// Writes a little-endian unsigned 16-bit integer, aligned to 2.
    /// </summary>
    public void WriteUInt16(ushort value)
    {
        AlignTo(2);
        EnsureCapacity(2);
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_position, 2), value);
        _position += 2;
    }

    /// <summary>
    /// Writes a little-endian signed 32-bit integer, aligned to 4.
    /// </summary>
    public void WriteInt32(int value)
    {
        AlignTo(4);
        EnsureCapacity(4);
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_position, 4), value);
        _position += 4;
    }

    /// <summary>
    /// Writes a little-endian unsigned 32-bit integer, aligned to 4.
    /// </summary>
    public void WriteUInt32(uint value)
    {
        AlignTo(4);
        EnsureCapacity(4);
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position, 4), value);
        _position += 4;
    }

    /// <summary>
    /// Writes a little-endian signed 64-bit integer, aligned to 8.
    /// </summary>
    public void WriteInt64(long value)
    {
        AlignTo(8);
        EnsureCapacity(8);
        BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_position, 8), value);
        _position += 8;
    }

    /// <summary>
    /// Writes a little-endian unsigned 64-bit integer, aligned to 8.
    /// </summary>
    public void WriteUInt64(ulong value)
    {
        AlignTo(8);
        EnsureCapacity(8);
        BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Slice(_position, 8), value);
        _position += 8;
    }

    /// <summary>
    /// Writes a single-precision IEEE-754 float, aligned to 4.
    /// </summary>
    public void WriteSingle(float value)
    {
        AlignTo(4);
        EnsureCapacity(4);
        BinaryPrimitives.WriteSingleLittleEndian(_buffer.Slice(_position, 4), value);
        _position += 4;
    }

    /// <summary>
    /// Writes a double-precision IEEE-754 float, aligned to 8.
    /// </summary>
    public void WriteDouble(double value)
    {
        AlignTo(8);
        EnsureCapacity(8);
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer.Slice(_position, 8), value);
        _position += 8;
    }

    /// <summary>
    /// Writes a <see cref="Guid"/> in DCE/NDR layout: {uint Data1, ushort Data2,
    /// ushort Data3, byte[8] Data4}. Outer alignment is 4 (the largest interior
    /// scalar). The first 8 bytes are the little-endian-encoded Data1/Data2/Data3;
    /// the Data4 byte array follows in declaration order.
    /// </summary>
    public void WriteGuid(Guid value)
    {
        AlignTo(4);
        EnsureCapacity(16);
        // Guid.TryWriteBytes already emits the GUID in the DCE/NDR-compatible
        // little-endian layout when TryWriteBytes(Span<byte>) is used (it
        // serializes Data1/Data2/Data3 in host endianness on LE platforms,
        // which matches NDR's little-endian wire format).
        bool ok = value.TryWriteBytes(_buffer.Slice(_position, 16));
        if (!ok)
        {
            throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
        }
        _position += 16;
    }

    /// <summary>
    /// Writes a Windows FILETIME (64-bit count of 100-nanosecond intervals
    /// since 1601-01-01 UTC) as two little-endian 32-bit halves: low half
    /// first, high half second. Outer alignment is 4.
    /// </summary>
    /// <remarks>
    /// FILETIME on the NDR wire is NOT an Int64 — it is two consecutive
    /// 32-bit fields per the FILETIME struct, so the alignment is 4 not 8.
    /// </remarks>
    public void WriteFileTime(long fileTime100ns)
    {
        AlignTo(4);
        EnsureCapacity(8);
        uint low = unchecked((uint)(fileTime100ns & 0xFFFFFFFFu));
        uint high = unchecked((uint)((fileTime100ns >> 32) & 0xFFFFFFFFu));
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position, 4), low);
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position + 4, 4), high);
        _position += 8;
    }

    /// <summary>
    /// Writes the conformance header for a conformant array — a single uint
    /// equal to the maximum element count. Conformant arrays embed this
    /// header at the array's start. Aligned to 4.
    /// </summary>
    public void WriteConformanceHeader(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Conformance count must be non-negative.");
        }
        WriteUInt32(unchecked((uint)count));
    }

    /// <summary>
    /// Writes a referent ID for a unique pointer. Returns the ID assigned.
    /// Zero referent IDs encode a null pointer.
    /// </summary>
    public uint WriteReferentId()
    {
        uint id = _nextReferentId;
        _nextReferentId += 4;
        WriteUInt32(id);
        return id;
    }

    /// <summary>
    /// Writes a null referent (encoded as 4 zero bytes, aligned to 4).
    /// </summary>
    public void WriteNullReferent() => WriteUInt32(0u);

    /// <summary>
    /// Writes a 4-byte unique-pointer referent: 0 when <paramref name="nonNull"/>
    /// is false (NULL pointer), otherwise an auto-incremented non-zero referent
    /// ID per DCE 1.1 §14.3.10. This is the preferred entry point for generator
    /// emission so that multiple sibling unique pointers don't share the same
    /// referent ID (the receiver would otherwise treat them as aliases).
    /// </summary>
    public void WriteUniquePointerReferent(bool nonNull)
    {
        if (nonNull)
        {
            _ = WriteReferentId();
        }
        else
        {
            WriteUInt32(0u);
        }
    }

    /// <summary>
    /// Writes a span of raw bytes verbatim (no alignment, no length prefix).
    /// </summary>
    public void WriteRawBytes(ReadOnlySpan<byte> source)
    {
        EnsureCapacity(source.Length);
        source.CopyTo(_buffer.Slice(_position, source.Length));
        _position += source.Length;
    }

    /// <summary>
    /// Writes a conformant + variant Unicode string per the LPWSTR convention:
    ///   uint max_count
    ///   uint offset (always 0)
    ///   uint actual_count
    ///   wchar[actual_count]   (each character is 2 little-endian bytes)
    /// The trailing null terminator IS included in max_count and actual_count
    /// per DCOM convention; pass the string without a terminator and we add
    /// the +1.
    /// </summary>
    public void WriteUnicodeString(ReadOnlySpan<char> value)
    {
        AlignTo(4);
        int countWithNul = value.Length + 1;
        WriteUInt32(unchecked((uint)countWithNul)); // max_count
        WriteUInt32(0u);                             // offset
        WriteUInt32(unchecked((uint)countWithNul)); // actual_count

        EnsureCapacity(countWithNul * 2);
        for (int i = 0; i < value.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_position, 2), value[i]);
            _position += 2;
        }
        // null terminator
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_position, 2), 0);
        _position += 2;
    }

    /// <summary>
    /// Writes an OLE Automation BSTR as a referent followed by a
    /// FLAGGED_WORD_BLOB per [MS-OAUT] §2.2.23:
    ///   uint referent       (non-zero for non-null; 0 for null)
    ///   {if non-null:}
    ///     uint fFlags       (0)
    ///     uint clSize       (count of UInt16 elements — char count, no terminator)
    ///     ushort[clSize] chars
    /// </summary>
    public void WriteBstr(ReadOnlySpan<char> value)
    {
        uint referent = WriteReferentId();
        _ = referent;
        // FLAGGED_WORD_BLOB per MS-OAUT 2.2.23: max_count (conformant array
        // prefix) + cBytes + clSize + WCHAR[clSize]. max_count and clSize
        // both equal the char count; cBytes is the byte count.
        uint clSize = unchecked((uint)value.Length);
        WriteUInt32(clSize);
        WriteUInt32(clSize * 2u);  // cBytes — informational byte count
        WriteUInt32(clSize);
        EnsureCapacity(value.Length * 2);
        for (int i = 0; i < value.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_position, 2), value[i]);
            _position += 2;
        }
    }

    /// <summary>
    /// Writes a null BSTR (a single zero referent UInt32).
    /// </summary>
    public void WriteNullBstr() => WriteNullReferent();

    /// <summary>
    /// Writes a unique-pointer LPWSTR — a referent ID followed by the
    /// conformant-variant string body (or a single zero referent for null).
    /// </summary>
    public void WriteUnicodeStringPtr(string? value)
    {
        if (value is null)
        {
            WriteNullReferent();
            return;
        }
        _ = WriteReferentId();
        WriteUnicodeString(value);
    }

    // -------- Conformant arrays of primitive types --------

    /// <summary>
    /// Writes a conformant array of bytes: 4-byte count + raw bytes
    /// (no element alignment since bytes are 1-aligned).
    /// </summary>
    public void WriteConformantByteArray(ReadOnlySpan<byte> values)
    {
        WriteConformanceHeader(values.Length);
        WriteRawBytes(values);
    }

    /// <summary>
    /// Writes a conformant array of Int16 values.
    /// </summary>
    public void WriteConformantInt16Array(ReadOnlySpan<short> values)
    {
        WriteConformanceHeader(values.Length);
        AlignTo(2);
        EnsureCapacity(values.Length * 2);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_position, 2), values[i]);
            _position += 2;
        }
    }

    /// <summary>
    /// Writes a conformant array of UInt16 values.
    /// </summary>
    public void WriteConformantUInt16Array(ReadOnlySpan<ushort> values)
    {
        WriteConformanceHeader(values.Length);
        AlignTo(2);
        EnsureCapacity(values.Length * 2);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_position, 2), values[i]);
            _position += 2;
        }
    }

    /// <summary>
    /// Writes a conformant array of Int32 values.
    /// </summary>
    public void WriteConformantInt32Array(ReadOnlySpan<int> values)
    {
        WriteConformanceHeader(values.Length);
        EnsureCapacity(values.Length * 4);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_position, 4), values[i]);
            _position += 4;
        }
    }

    /// <summary>
    /// Writes a conformant array of UInt32 values.
    /// </summary>
    public void WriteConformantUInt32Array(ReadOnlySpan<uint> values)
    {
        WriteConformanceHeader(values.Length);
        EnsureCapacity(values.Length * 4);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position, 4), values[i]);
            _position += 4;
        }
    }

    /// <summary>
    /// Writes a conformant array of Int64 values.
    /// </summary>
    public void WriteConformantInt64Array(ReadOnlySpan<long> values)
    {
        WriteConformanceHeader(values.Length);
        AlignTo(8);
        EnsureCapacity(values.Length * 8);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_position, 8), values[i]);
            _position += 8;
        }
    }

    /// <summary>
    /// Writes a conformant array of Windows FILETIME values. Unlike Int64,
    /// FILETIME is two consecutive 32-bit halves (low, high) per element with
    /// 4-byte alignment — NOT 8-byte-aligned Int64. Used for IDL
    /// <c>[size_is(N)] FILETIME *p</c> arrays in IOPCItemIO::Read,
    /// IOPCSyncIO::Read, etc.
    /// </summary>
    public void WriteConformantFileTimeArray(ReadOnlySpan<long> values)
    {
        WriteConformanceHeader(values.Length);
        EnsureCapacity(values.Length * 8);
        for (int i = 0; i < values.Length; i++)
        {
            uint low = unchecked((uint)(values[i] & 0xFFFFFFFFu));
            uint high = unchecked((uint)((values[i] >> 32) & 0xFFFFFFFFu));
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position, 4), low);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position + 4, 4), high);
            _position += 8;
        }
    }

    /// <summary>
    /// Writes a conformant array of Single (float) values.
    /// </summary>
    public void WriteConformantSingleArray(ReadOnlySpan<float> values)
    {
        WriteConformanceHeader(values.Length);
        EnsureCapacity(values.Length * 4);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(_buffer.Slice(_position, 4), values[i]);
            _position += 4;
        }
    }

    /// <summary>
    /// Writes a conformant array of Double values.
    /// </summary>
    public void WriteConformantDoubleArray(ReadOnlySpan<double> values)
    {
        WriteConformanceHeader(values.Length);
        AlignTo(8);
        EnsureCapacity(values.Length * 8);
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(_buffer.Slice(_position, 8), values[i]);
            _position += 8;
        }
    }

    /// <summary>
    /// Writes a conformant array of Guid values (each 16 bytes, aligned to 4).
    /// </summary>
    public void WriteConformantGuidArray(ReadOnlySpan<Guid> values)
    {
        WriteConformanceHeader(values.Length);
        AlignTo(4);
        EnsureCapacity(values.Length * 16);
        for (int i = 0; i < values.Length; i++)
        {
            bool ok = values[i].TryWriteBytes(_buffer.Slice(_position, 16));
            if (!ok)
            {
                throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
            }
            _position += 16;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int additionalBytes)
    {
        if (_position + additionalBytes > _buffer.Length)
        {
            throw new NdrBufferOverflowException(
                _position,
                additionalBytes,
                _buffer.Length - _position);
        }
    }
}
