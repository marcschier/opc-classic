// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using BenchmarkDotNet.Attributes;

namespace Opc.Classic.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class CodecRegistryBenchmarks
{
    private LocalCodecRegistry _warmRegistry = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _warmRegistry = new LocalCodecRegistry();
        _ = _warmRegistry.Get(typeof(int));
        _ = _warmRegistry.Get(typeof(OpcVariant));
    }

    [Benchmark]
    public CodecEntry ColdPrimitiveLookup()
    {
        var registry = new LocalCodecRegistry();
        return registry.Get(typeof(int));
    }

    [Benchmark(Baseline = true)]
    public CodecEntry WarmPrimitiveLookup() => _warmRegistry.Get(typeof(int));

    [Benchmark]
    public CodecEntry ColdStructLookup()
    {
        var registry = new LocalCodecRegistry();
        return registry.Get(typeof(OpcVariant));
    }

    [Benchmark]
    public CodecEntry WarmStructLookup() => _warmRegistry.Get(typeof(OpcVariant));

    public readonly record struct CodecEntry(string Encoder, string Decoder);

    private sealed class LocalCodecRegistry
    {
        private readonly Dictionary<Type, CodecEntry> _cache = [];

        public CodecEntry Get(Type type)
        {
            if (_cache.TryGetValue(type, out CodecEntry entry))
            {
                return entry;
            }

            entry = CreateEntry(type);
            _cache.Add(type, entry);
            return entry;
        }

        private static CodecEntry CreateEntry(Type type)
        {
            if (type == typeof(int))
            {
                return new CodecEntry("NdrWriter.WriteInt32", "NdrReader.ReadInt32");
            }

            if (type == typeof(OpcVariant))
            {
                return new CodecEntry("NdrVariantExtensions.WriteVariant", "NdrVariantExtensions.ReadVariant");
            }

            throw new InvalidOperationException($"No benchmark codec entry registered for {type.FullName}.");
        }
    }
}
