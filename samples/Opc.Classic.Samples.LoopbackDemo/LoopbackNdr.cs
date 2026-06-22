// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers;
using Opc.Classic.Ndr;

namespace Opc.Classic.Samples.LoopbackDemo;

internal static class LoopbackNdr
{
    private const int InitialBufferSize = 16 * 1024;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    public delegate void WriteAction(ref NdrWriter writer);

    public static ReadOnlyMemory<byte> Write(WriteAction write)
    {
        ArgumentNullException.ThrowIfNull(write);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(InitialBufferSize);
        try
        {
            var writer = new NdrWriter(buffer);
            write(ref writer);
            return buffer.AsMemory(0, writer.Position).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static void WriteStringArray(ref NdrWriter writer, IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        writer.WriteUInt32((uint)values.Count);
        foreach (string value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    public static string[] ReadStringArray(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new string[count];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadUnicodeStringPtr() ?? string.Empty;
        }

        return values;
    }

    public static void WriteInt32Array(ref NdrWriter writer, IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        writer.WriteUInt32((uint)values.Count);
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    public static int[] ReadInt32Array(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new int[count];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadInt32();
        }

        return values;
    }

    public static ushort[] ReadUInt16Array(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new ushort[count];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadUInt16();
        }

        return values;
    }

    public static long[] ReadInt64Array(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new long[count];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadInt64();
        }

        return values;
    }

    public static void WriteVariantArray(ref NdrWriter writer, IReadOnlyList<OpcVariant> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        writer.WriteUInt32((uint)values.Count);
        foreach (OpcVariant value in values)
        {
            writer.WriteVariant(value);
        }
    }

    public static OpcVariant[] ReadVariantArray(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new OpcVariant[count];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadVariant();
        }

        return values;
    }

    public static long ToFileTime(DateTimeOffset value) => value.UtcTicks - FileTimeEpochOffsetTicks;
    public static DateTimeOffset FromFileTime(long fileTimeTicks) => new(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
}
