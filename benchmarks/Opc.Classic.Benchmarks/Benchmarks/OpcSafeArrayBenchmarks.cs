//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using BenchmarkDotNet.Attributes;
using Opc.Classic.Ndr;

namespace Opc.Classic.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class OpcSafeArrayBenchmarks {
    private const string Int32_100 = "Int32_100";
    private const string Int32_1000 = "Int32_1000";
    private const string Int32_10000 = "Int32_10000";
    private const string Double_100 = "Double_100";
    private const string Double_1000 = "Double_1000";
    private const string Bstr_100 = "Bstr_100";

    private OpcSafeArray _array = OpcSafeArray.OfInt32([]);
    private byte[] _buffer = [];
    private byte[] _payload = [];

    [Params(Int32_100, Int32_1000, Int32_10000, Double_100, Double_1000, Bstr_100)]
    public string Case { get; set; } = Int32_100;

    [GlobalSetup]
    public void GlobalSetup() {
        _array = CreateArray(Case);
        _buffer = new byte[EstimateCapacity(_array)];
        _payload = Encode(_array, _buffer.Length);
    }

    [Benchmark]
    public int EncodeSafeArray() {
        var writer = new NdrWriter(_buffer);
        writer.WriteSafeArray(_array);
        return writer.Position;
    }

    [Benchmark]
    public OpcSafeArray DecodeSafeArray() {
        var reader = new NdrReader(_payload);
        return reader.ReadSafeArray();
    }

    [Benchmark]
    public OpcSafeArray RoundTripSafeArray() {
        var writer = new NdrWriter(_buffer);
        writer.WriteSafeArray(_array);
        var reader = new NdrReader(_buffer.AsSpan(0, writer.Position));
        return reader.ReadSafeArray();
    }

    private static byte[] Encode(OpcSafeArray array, int capacity) {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        writer.WriteSafeArray(array);
        return buffer[..writer.Position];
    }

    private static OpcSafeArray CreateArray(string benchmarkCase) => benchmarkCase switch {
        Int32_100 => OpcSafeArray.OfInt32(CreateInt32Values(100)),
        Int32_1000 => OpcSafeArray.OfInt32(CreateInt32Values(1_000)),
        Int32_10000 => OpcSafeArray.OfInt32(CreateInt32Values(10_000)),
        Double_100 => OpcSafeArray.OfDouble(CreateDoubleValues(100)),
        Double_1000 => OpcSafeArray.OfDouble(CreateDoubleValues(1_000)),
        Bstr_100 => OpcSafeArray.OfString(CreateStringValues(100)),
        _ => throw new InvalidOperationException($"Unsupported SAFEARRAY benchmark case '{benchmarkCase}'."),
    };

    private static int[] CreateInt32Values(int count) {
        var values = new int[count];
        for (int i = 0; i < values.Length; i++) {
            values[i] = i * 17 - 3;
        }

        return values;
    }

    private static double[] CreateDoubleValues(int count) {
        var values = new double[count];
        for (int i = 0; i < values.Length; i++) {
            values[i] = Math.Sqrt(i + 1);
        }

        return values;
    }

    private static string[] CreateStringValues(int count) {
        var values = new string[count];
        for (int i = 0; i < values.Length; i++) {
            values[i] = FormattableString.Invariant($"Tag-{i:0000}-Benchmark");
        }

        return values;
    }

    private static int EstimateCapacity(OpcSafeArray array) => array.ElementType switch {
        VarType.VT_I4 => 64 + array.TotalElements * 4,
        VarType.VT_R8 => 64 + array.TotalElements * 8,
        VarType.VT_BSTR => 256 + array.TotalElements * 64,
        _ => 1024 + array.TotalElements * 16,
    };
}
