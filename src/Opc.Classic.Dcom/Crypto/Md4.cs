//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Pure-managed implementation of MD4 per RFC 1320.
// In-tree replacement for BouncyCastle's MD4Digest. BCL has no MD4.
//
// MD4 is cryptographically broken (collisions known since 2007) but is still
// required for NTLM password hashing (NT-Hash = MD4(UTF16LE password)) per
// MS-NLMP §3.3.1. Use ONLY for protocol compatibility — never for new
// security primitives.
//

using System;

namespace Opc.Classic.Dcom.Crypto;

/// <summary>
/// Pure-managed MD4 (RFC 1320). Hash size = 16 bytes; block size = 64 bytes.
/// </summary>
public static class Md4 {
    /// <summary>Size of the produced hash, in bytes.</summary>
    public const int HashSizeInBytes = 16;

    /// <summary>Compression-function block size, in bytes.</summary>
    public const int BlockSizeInBytes = 64;

    /// <summary>One-shot hashing: returns a 16-byte MD4 of <paramref name="source"/>.</summary>
    public static byte[] HashData(ReadOnlySpan<byte> source) {
        var result = new byte[HashSizeInBytes];
        HashData(source, result);
        return result;
    }

    /// <summary>One-shot hashing into a caller-provided 16-byte destination.</summary>
    public static void HashData(ReadOnlySpan<byte> source, Span<byte> destination) {
        if (destination.Length < HashSizeInBytes) {
            throw new ArgumentException(
                $"Destination must be at least {HashSizeInBytes} bytes.", nameof(destination));
        }

        var state = new Md4State();
        state.Initialize();
        state.AppendData(source);
        state.GetHashAndReset(destination);
    }
}
