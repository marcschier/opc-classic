//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using BenchmarkDotNet.Attributes;
using Opc.Classic.Ndr;

namespace Opc.Classic.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class OpcVariantBenchmarks
{
    private byte[] _buffer = [];
    private byte[] _payload = [];
    private OpcVariant _variant;

    [Params(VariantShape.Int32, VariantShape.Double, VariantShape.Bstr, VariantShape.Boolean, VariantShape.Date, VariantShape.Int32Array)]
    public VariantShape Shape { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _variant = CreateVariant(Shape);
        _buffer = new byte[EstimateCapacity(Shape)];
        _payload = Encode(_variant, _buffer.Length);
    }

    [Benchmark]
    public int EncodeVariant()
    {
        var writer = new NdrWriter(_buffer);
        writer.WriteVariant(_variant);
        return writer.Position;
    }

    [Benchmark]
    public OpcVariant DecodeVariant()
    {
        var reader = new NdrReader(_payload);
        return reader.ReadVariant();
    }

    [Benchmark]
    public OpcVariant RoundTripVariant()
    {
        var writer = new NdrWriter(_buffer);
        writer.WriteVariant(_variant);
        var reader = new NdrReader(_buffer.AsSpan(0, writer.Position));
        return reader.ReadVariant();
    }

    private static byte[] Encode(OpcVariant value, int capacity)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        writer.WriteVariant(value);
        return buffer[..writer.Position];
    }

    private static OpcVariant CreateVariant(VariantShape shape) => shape switch
    {
        VariantShape.Int32 => OpcVariant.FromInt32(42_424_242),
        VariantShape.Double => OpcVariant.FromDouble(Math.E),
        VariantShape.Bstr => OpcVariant.FromString("OPC Classic Benchmark Variant"),
        VariantShape.Boolean => OpcVariant.FromBoolean(true),
        VariantShape.Date => OpcVariant.FromDate(new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc)),
        VariantShape.Int32Array => OpcVariant.FromSafeArray(OpcSafeArray.OfInt32(CreateInt32Values(100))),
        _ => throw new InvalidOperationException($"Unsupported variant shape {shape}."),
    };

    private static int[] CreateInt32Values(int count)
    {
        var values = new int[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = i * 17 - 3;
        }

        return values;
    }

    private static int EstimateCapacity(VariantShape shape) => shape switch
    {
        VariantShape.Int32 or VariantShape.Double or VariantShape.Boolean or VariantShape.Date => 128,
        VariantShape.Bstr => 512,
        VariantShape.Int32Array => 1_024,
        _ => 1_024,
    };

    public enum VariantShape
    {
        Int32,
        Double,
        Bstr,
        Boolean,
        Date,
        Int32Array,
    }
}
