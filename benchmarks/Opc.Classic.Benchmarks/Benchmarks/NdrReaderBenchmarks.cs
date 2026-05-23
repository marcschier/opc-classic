//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using BenchmarkDotNet.Attributes;
using Opc.Classic.Ndr;

namespace Opc.Classic.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class NdrReaderBenchmarks
{
    private const long FileTimeSeed = 133_485_408_000_000_000L;

    private byte[] _byteArrayPayload = [];
    private byte[] _doublePayload = [];
    private byte[] _fileTimePayload = [];
    private byte[] _stringPayload = [];
    private byte[] _uint32Payload = [];

    [Params(1, 100, 10_000)]
    public int Scale { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _uint32Payload = WritePayload(Scale * 4 + 16, (ref NdrWriter writer) =>
        {
            for (int i = 0; i < Scale; i++)
            {
                writer.WriteUInt32(unchecked(0x9E37_79B9u + (uint)i));
            }
        });
        _stringPayload = WritePayload(12 + (MapStringLength(Scale) + 1) * 2, (ref NdrWriter writer) =>
            writer.WriteUnicodeString(CreateString(MapStringLength(Scale))));
        _byteArrayPayload = WritePayload(MapByteArrayLength(Scale) + 16, (ref NdrWriter writer) =>
            writer.WriteConformantByteArray(CreateByteArray(MapByteArrayLength(Scale))));
        _doublePayload = WritePayload(Scale * 8 + 32, (ref NdrWriter writer) =>
        {
            for (int i = 0; i < Scale; i++)
            {
                writer.WriteDouble(Math.PI * (i + 1) / 17.0);
            }
        });
        _fileTimePayload = WritePayload(Scale * 8 + 32, (ref NdrWriter writer) =>
        {
            for (int i = 0; i < Scale; i++)
            {
                writer.WriteFileTime(FileTimeSeed + i * 10_000L);
            }
        });
    }

    [Benchmark(Baseline = true)]
    public uint ReadUInt32Naive()
    {
        uint checksum = 0;
        ReadOnlySpan<byte> span = _uint32Payload;
        for (int position = 0; position < span.Length; position += 4)
        {
            checksum ^= BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(position, 4));
        }

        return checksum;
    }

    [Benchmark]
    public uint ReadUInt32()
    {
        uint checksum = 0;
        var reader = new NdrReader(_uint32Payload);
        for (int i = 0; i < Scale; i++)
        {
            checksum ^= reader.ReadUInt32();
        }

        return checksum;
    }

    [Benchmark]
    public int ReadStringNaive()
    {
        ReadOnlySpan<byte> span = _stringPayload;
        uint offset = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(4, 4));
        if (offset != 0u)
        {
            throw new InvalidOperationException("NDR LPWSTR offset must be zero.");
        }

        int charCount = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(8, 4)));
        int effective = charCount;
        if (effective > 0 && BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(12 + (effective - 1) * 2, 2)) == 0)
        {
            effective--;
        }

        var chars = new char[effective];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(12 + i * 2, 2));
        }

        return new string(chars).Length;
    }

    [Benchmark]
    public int ReadString()
    {
        var reader = new NdrReader(_stringPayload);
        return reader.ReadUnicodeString().Length;
    }

    [Benchmark]
    public int ReadByteArrayNaive()
    {
        ReadOnlySpan<byte> span = _byteArrayPayload;
        int count = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(0, 4)));
        var bytes = new byte[count];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = span[4 + i];
        }

        return bytes.Length;
    }

    [Benchmark]
    public int ReadByteArray()
    {
        var reader = new NdrReader(_byteArrayPayload);
        return reader.ReadConformantByteArray().Length;
    }

    [Benchmark]
    public double ReadDouble()
    {
        double sum = 0;
        var reader = new NdrReader(_doublePayload);
        for (int i = 0; i < Scale; i++)
        {
            sum += reader.ReadDouble();
        }

        return sum;
    }

    [Benchmark]
    public long ReadFileTime()
    {
        long checksum = 0;
        var reader = new NdrReader(_fileTimePayload);
        for (int i = 0; i < Scale; i++)
        {
            checksum ^= reader.ReadFileTime();
        }

        return checksum;
    }

    private delegate void PayloadWriter(ref NdrWriter writer);

    private static byte[] WritePayload(int capacity, PayloadWriter write)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static byte[] CreateByteArray(int count)
    {
        var values = new byte[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = unchecked((byte)(i * 31 + 7));
        }

        return values;
    }

    private static string CreateString(int length)
    {
        var chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)('A' + i % 26);
        }

        return new string(chars);
    }

    private static int MapByteArrayLength(int scale) => scale switch
    {
        1 => 16,
        100 => 1_024,
        _ => 64 * 1_024,
    };

    private static int MapStringLength(int scale) => scale switch
    {
        1 => 16,
        100 => 256,
        _ => 4_096,
    };
}
