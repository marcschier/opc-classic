// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;

namespace Opc.Classic.Da.Tests.Wire;

/// <summary>
/// Test helpers for wire-fixture assertions: peek at little-endian scalars
/// at known offsets without re-decoding the entire payload, and dump byte
/// ranges as hex strings for human-readable failure messages.
/// </summary>
internal static class WireAssert
{
    public static uint ReadUInt32At(byte[] bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));

    public static uint ReadUInt32At(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, 4));

    public static ushort ReadUInt16At(byte[] bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));

    public static ushort ReadUInt16At(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(offset, 2));

    public static string HexDump(ReadOnlySpan<byte> bytes)
    {
        char[] chars = new char[bytes.Length * 2];
        const string Hex = "0123456789ABCDEF";
        for (int i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = Hex[bytes[i] >> 4];
            chars[i * 2 + 1] = Hex[bytes[i] & 0x0F];
        }
        return new string(chars);
    }
}
