//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using BenchmarkDotNet.Attributes;
using Opc.Classic.Ndr;

namespace Opc.Classic.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class NdrWriterBenchmarks
{
    private const long FileTimeSeed = 133_485_408_000_000_000L;

    private byte[] _buffer = [];
    private byte[] _byteArray = [];
    private double[] _doubleValues = [];
    private long[] _fileTimeValues = [];
    private string _stringValue = string.Empty;
    private uint[] _uint32Values = [];

    [Params(1, 100, 10_000)]
    public int Scale { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _uint32Values = CreateUInt32Values(Scale);
        _doubleValues = CreateDoubleValues(Scale);
        _fileTimeValues = CreateFileTimeValues(Scale);
        _stringValue = CreateString(MapStringLength(Scale));
        _byteArray = CreateByteArray(MapByteArrayLength(Scale));

        int capacity = Math.Max(Scale * 8 + 32, Math.Max(_byteArray.Length + 16, 12 + (_stringValue.Length + 1) * 2));
        _buffer = new byte[capacity];
    }

    [Benchmark(Baseline = true)]
    public int WriteUInt32Naive()
    {
        int position = 0;
        Span<byte> span = _buffer;
        for (int i = 0; i < _uint32Values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(position, 4), _uint32Values[i]);
            position += 4;
        }

        return position;
    }

    [Benchmark]
    public int WriteUInt32()
    {
        var writer = new NdrWriter(_buffer);
        for (int i = 0; i < _uint32Values.Length; i++)
        {
            writer.WriteUInt32(_uint32Values[i]);
        }

        return writer.Position;
    }

    [Benchmark]
    public int WriteStringNaive()
    {
        int position = 0;
        Span<byte> span = _buffer;
        int countWithNul = _stringValue.Length + 1;
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(position, 4), unchecked((uint)countWithNul));
        position += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(position, 4), 0u);
        position += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(position, 4), unchecked((uint)countWithNul));
        position += 4;

        for (int i = 0; i < _stringValue.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(position, 2), _stringValue[i]);
            position += 2;
        }

        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(position, 2), 0);
        return position + 2;
    }

    [Benchmark]
    public int WriteString()
    {
        var writer = new NdrWriter(_buffer);
        writer.WriteUnicodeString(_stringValue);
        return writer.Position;
    }

    [Benchmark]
    public int WriteByteArrayNaive()
    {
        int position = 0;
        Span<byte> span = _buffer;
        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(position, 4), unchecked((uint)_byteArray.Length));
        position += 4;
        for (int i = 0; i < _byteArray.Length; i++)
        {
            span[position++] = _byteArray[i];
        }

        return position;
    }

    [Benchmark]
    public int WriteByteArray()
    {
        var writer = new NdrWriter(_buffer);
        writer.WriteConformantByteArray(_byteArray);
        return writer.Position;
    }

    [Benchmark]
    public int WriteDouble()
    {
        var writer = new NdrWriter(_buffer);
        for (int i = 0; i < _doubleValues.Length; i++)
        {
            writer.WriteDouble(_doubleValues[i]);
        }

        return writer.Position;
    }

    [Benchmark]
    public int WriteFileTime()
    {
        var writer = new NdrWriter(_buffer);
        for (int i = 0; i < _fileTimeValues.Length; i++)
        {
            writer.WriteFileTime(_fileTimeValues[i]);
        }

        return writer.Position;
    }

    private static uint[] CreateUInt32Values(int count)
    {
        var values = new uint[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = unchecked(0x9E37_79B9u + (uint)i);
        }

        return values;
    }

    private static double[] CreateDoubleValues(int count)
    {
        var values = new double[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Math.PI * (i + 1) / 17.0;
        }

        return values;
    }

    private static long[] CreateFileTimeValues(int count)
    {
        var values = new long[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = FileTimeSeed + i * 10_000L;
        }

        return values;
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
