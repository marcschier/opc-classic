//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Pure-managed implementation of MD4 per RFC 1320.
//

using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Opc.Classic.Dcom.Crypto;

/// <summary>
/// Incremental MD4 state. Reusable after <see cref="GetHashAndReset"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Md4State {
    // RFC 1320 §3.3 — initial chaining values, little-endian.
    private uint _a, _b, _c, _d;
    private ulong _bitsProcessed;
    private uint _bufferedBytes;

    // 64-byte working buffer for incomplete blocks.
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Mutated through ref")]
    private Buffer64 _buffer;

    [InlineArray(64)]
    private struct Buffer64 {
#pragma warning disable IDE0051, CS0169
        private byte _element0;
#pragma warning restore IDE0051, CS0169
    }

    /// <summary>Reset to RFC 1320 §3.3 initial values.</summary>
    public void Initialize() {
        _a = 0x67452301u;
        _b = 0xefcdab89u;
        _c = 0x98badcfeu;
        _d = 0x10325476u;
        _bitsProcessed = 0;
        _bufferedBytes = 0;
    }

    /// <summary>Append <paramref name="data"/> to the running hash.</summary>
    public void AppendData(ReadOnlySpan<byte> data) {
        _bitsProcessed += (ulong)data.Length * 8;

        // If we have a partial block buffered, fill it first.
        Span<byte> buf = _buffer;
        if (_bufferedBytes > 0) {
            var need = Md4.BlockSizeInBytes - (int)_bufferedBytes;
            if (data.Length < need) {
                data.CopyTo(buf[(int)_bufferedBytes..]);
                _bufferedBytes += (uint)data.Length;
                return;
            }
            data[..need].CopyTo(buf[(int)_bufferedBytes..]);
            data = data[need..];
            ProcessBlock(buf);
            _bufferedBytes = 0;
        }

        // Crunch any full 64-byte blocks directly from the input.
        while (data.Length >= Md4.BlockSizeInBytes) {
            ProcessBlock(data[..Md4.BlockSizeInBytes]);
            data = data[Md4.BlockSizeInBytes..];
        }

        // Buffer the tail.
        if (!data.IsEmpty) {
            data.CopyTo(buf);
            _bufferedBytes = (uint)data.Length;
        }
    }

    /// <summary>Finalize, write the 16-byte hash, and reset state for reuse.</summary>
    public void GetHashAndReset(Span<byte> destination) {
        if (destination.Length < Md4.HashSizeInBytes) {
            throw new ArgumentException(
                $"Destination must be at least {Md4.HashSizeInBytes} bytes.", nameof(destination));
        }

        // RFC 1320 §3.1 — pad with 0x80 + zeros to (mod 64 == 56), then 8-byte
        // little-endian bit length.
        Span<byte> buf = _buffer;
        buf[(int)_bufferedBytes] = 0x80;
        var padded = (int)_bufferedBytes + 1;

        if (padded > Md4.BlockSizeInBytes - 8) {
            // Not enough room for length; pad to end of this block, process, start fresh.
            buf[padded..].Clear();
            ProcessBlock(buf);
            buf.Clear();
        }
        else {
            buf[padded..(Md4.BlockSizeInBytes - 8)].Clear();
        }

        BinaryPrimitives.WriteUInt64LittleEndian(buf[(Md4.BlockSizeInBytes - 8)..], _bitsProcessed);
        ProcessBlock(buf);

        BinaryPrimitives.WriteUInt32LittleEndian(destination[..4], _a);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(4, 4), _b);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(8, 4), _c);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(12, 4), _d);

        Initialize();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint G(uint x, uint y, uint z) => (x & y) | (x & z) | (y & z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint H(uint x, uint y, uint z) => x ^ y ^ z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Rol(uint v, int s) => (v << s) | (v >> (32 - s));

#pragma warning disable MA0051 // MD4 compression round is intentionally kept literal to RFC 1320.
    private void ProcessBlock(ReadOnlySpan<byte> block) {
        // Decode 16 little-endian 32-bit words.
        Span<uint> x = stackalloc uint[16];
        for (var i = 0; i < 16; i++) {
            x[i] = BinaryPrimitives.ReadUInt32LittleEndian(block.Slice(i * 4, 4));
        }

        var a = _a;
        var b = _b;
        var c = _c;
        var d = _d;

        // Round 1: a = ROL(a + F(b,c,d) + X[k], s) for k in 0..15, s in {3,7,11,19}
        a = Rol(a + F(b, c, d) + x[0], 3);
        d = Rol(d + F(a, b, c) + x[1], 7);
        c = Rol(c + F(d, a, b) + x[2], 11);
        b = Rol(b + F(c, d, a) + x[3], 19);
        a = Rol(a + F(b, c, d) + x[4], 3);
        d = Rol(d + F(a, b, c) + x[5], 7);
        c = Rol(c + F(d, a, b) + x[6], 11);
        b = Rol(b + F(c, d, a) + x[7], 19);
        a = Rol(a + F(b, c, d) + x[8], 3);
        d = Rol(d + F(a, b, c) + x[9], 7);
        c = Rol(c + F(d, a, b) + x[10], 11);
        b = Rol(b + F(c, d, a) + x[11], 19);
        a = Rol(a + F(b, c, d) + x[12], 3);
        d = Rol(d + F(a, b, c) + x[13], 7);
        c = Rol(c + F(d, a, b) + x[14], 11);
        b = Rol(b + F(c, d, a) + x[15], 19);

        // Round 2: a = ROL(a + G(b,c,d) + X[k] + 0x5A827999, s) for k {0,4,8,12,1,5,...}, s {3,5,9,13}
        const uint K2 = 0x5A827999u;
        a = Rol(a + G(b, c, d) + x[0] + K2, 3);
        d = Rol(d + G(a, b, c) + x[4] + K2, 5);
        c = Rol(c + G(d, a, b) + x[8] + K2, 9);
        b = Rol(b + G(c, d, a) + x[12] + K2, 13);
        a = Rol(a + G(b, c, d) + x[1] + K2, 3);
        d = Rol(d + G(a, b, c) + x[5] + K2, 5);
        c = Rol(c + G(d, a, b) + x[9] + K2, 9);
        b = Rol(b + G(c, d, a) + x[13] + K2, 13);
        a = Rol(a + G(b, c, d) + x[2] + K2, 3);
        d = Rol(d + G(a, b, c) + x[6] + K2, 5);
        c = Rol(c + G(d, a, b) + x[10] + K2, 9);
        b = Rol(b + G(c, d, a) + x[14] + K2, 13);
        a = Rol(a + G(b, c, d) + x[3] + K2, 3);
        d = Rol(d + G(a, b, c) + x[7] + K2, 5);
        c = Rol(c + G(d, a, b) + x[11] + K2, 9);
        b = Rol(b + G(c, d, a) + x[15] + K2, 13);

        // Round 3: a = ROL(a + H(b,c,d) + X[k] + 0x6ED9EBA1, s) for k {0,8,4,12,2,10,...}, s {3,9,11,15}
        const uint K3 = 0x6ED9EBA1u;
        a = Rol(a + H(b, c, d) + x[0] + K3, 3);
        d = Rol(d + H(a, b, c) + x[8] + K3, 9);
        c = Rol(c + H(d, a, b) + x[4] + K3, 11);
        b = Rol(b + H(c, d, a) + x[12] + K3, 15);
        a = Rol(a + H(b, c, d) + x[2] + K3, 3);
        d = Rol(d + H(a, b, c) + x[10] + K3, 9);
        c = Rol(c + H(d, a, b) + x[6] + K3, 11);
        b = Rol(b + H(c, d, a) + x[14] + K3, 15);
        a = Rol(a + H(b, c, d) + x[1] + K3, 3);
        d = Rol(d + H(a, b, c) + x[9] + K3, 9);
        c = Rol(c + H(d, a, b) + x[5] + K3, 11);
        b = Rol(b + H(c, d, a) + x[13] + K3, 15);
        a = Rol(a + H(b, c, d) + x[3] + K3, 3);
        d = Rol(d + H(a, b, c) + x[11] + K3, 9);
        c = Rol(c + H(d, a, b) + x[7] + K3, 11);
        b = Rol(b + H(c, d, a) + x[15] + K3, 15);

        _a += a;
        _b += b;
        _c += c;
        _d += d;
    }
#pragma warning restore MA0051
}
