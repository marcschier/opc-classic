// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using System.Security.Cryptography;

namespace Opc.Classic.Samples.DaServer;

public sealed class TagTree
{
    public IReadOnlyDictionary<string, ITagSource> Tags { get; }

    public TagTree()
    {
        var tags = new Dictionary<string, ITagSource>(StringComparer.Ordinal)
        {
            ["Random.Real4"] = new RandomReal4(),
            ["Random.Real8"] = new RandomReal8(),
            ["Random.Int1"] = new RandomInt1(),
            ["Random.Int2"] = new RandomInt2(),
            ["Random.Int4"] = new RandomInt4(),
            ["Random.UInt1"] = new RandomUInt1(),
            ["Random.UInt2"] = new RandomUInt2(),
            ["Random.UInt4"] = new RandomUInt4(),
            ["Random.Boolean"] = new RandomBool(),
            ["Random.String"] = new RandomString(),

            ["Bucket Brigade.Real4"] = new BucketReal4(),
            ["Bucket Brigade.Real8"] = new BucketReal8(),
            ["Bucket Brigade.Int1"] = new BucketInt1(),
            ["Bucket Brigade.Int2"] = new BucketInt2(),
            ["Bucket Brigade.Int4"] = new BucketInt4(),
            ["Bucket Brigade.UInt1"] = new BucketUInt1(),
            ["Bucket Brigade.UInt2"] = new BucketUInt2(),
            ["Bucket Brigade.UInt4"] = new BucketUInt4(),
            ["Bucket Brigade.Boolean"] = new BucketBool(),
            ["Bucket Brigade.String"] = new BucketString(),

            ["Saw-toothed Waves.Real4"] = new SawtoothReal4(),
            ["Saw-toothed Waves.Real8"] = new SawtoothReal8(),
            ["Square Waves.Real4"] = new SquareReal4(),
            ["Square Waves.Real8"] = new SquareReal8(),
            ["Triangle Waves.Real4"] = new TriangleReal4(),
            ["Triangle Waves.Real8"] = new TriangleReal8(),

            ["Read Error.Int1"] = new ReadErrorTag(),
            ["Read Error.Int2"] = new ReadErrorTag(),
            ["Read Error.Int4"] = new ReadErrorTag(),
            ["Write Error.Int1"] = new WriteErrorTag(),
        };

        Tags = tags;
    }
}

public interface ITagSource
{
    object? Read();
    bool TryWrite(object? value);
}

internal abstract class ReadOnlyTag : ITagSource
{
    public abstract object? Read();
    public bool TryWrite(object? value) => false;
}

internal abstract class BucketTag<T> : ITagSource
{
    private readonly object _gate = new();
    private T _value;

    protected BucketTag(T initialValue)
    {
        _value = initialValue;
    }

    public object? Read()
    {
        lock (_gate)
        {
            return _value;
        }
    }

    public bool TryWrite(object? value)
    {
        if (value is not T typedValue)
        {
            return false;
        }

        lock (_gate)
        {
            _value = typedValue;
        }

        return true;
    }
}

internal sealed class RandomReal4 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextSingle();
}

internal sealed class RandomReal8 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextDouble();
}

internal sealed class RandomInt1 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextSByte();
}

internal sealed class RandomInt2 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextInt16();
}

internal sealed class RandomInt4 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextInt32();
}

internal sealed class RandomUInt1 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextByte();
}

internal sealed class RandomUInt2 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextUInt16();
}

internal sealed class RandomUInt4 : ReadOnlyTag
{
    public override object Read() => RandomValues.NextUInt32();
}

internal sealed class RandomBool : ReadOnlyTag
{
    public override object Read() => RandomValues.NextBoolean();
}

internal sealed class RandomString : ReadOnlyTag
{
    public override object Read() => RandomValues.NextString();
}

internal sealed class BucketReal4 : BucketTag<float>
{
    public BucketReal4() : base(0.0F) { }
}

internal sealed class BucketReal8 : BucketTag<double>
{
    public BucketReal8() : base(0.0D) { }
}

internal sealed class BucketInt1 : BucketTag<sbyte>
{
    public BucketInt1() : base(0) { }
}

internal sealed class BucketInt2 : BucketTag<short>
{
    public BucketInt2() : base(0) { }
}

internal sealed class BucketInt4 : BucketTag<int>
{
    public BucketInt4() : base(0) { }
}

internal sealed class BucketUInt1 : BucketTag<byte>
{
    public BucketUInt1() : base(0) { }
}

internal sealed class BucketUInt2 : BucketTag<ushort>
{
    public BucketUInt2() : base(0) { }
}

internal sealed class BucketUInt4 : BucketTag<uint>
{
    public BucketUInt4() : base(0U) { }
}

internal sealed class BucketBool : BucketTag<bool>
{
    public BucketBool() : base(false) { }
}

internal sealed class BucketString : BucketTag<string>
{
    public BucketString() : base(string.Empty) { }
}

internal abstract class WaveReal4Tag : ReadOnlyTag
{
    public sealed override object Read() => (float)Sample();
    protected abstract double Sample();
}

internal abstract class WaveReal8Tag : ReadOnlyTag
{
    public sealed override object Read() => Sample();
    protected abstract double Sample();
}

internal sealed class SawtoothReal4 : WaveReal4Tag
{
    protected override double Sample() => WaveMath.Sawtooth();
}

internal sealed class SawtoothReal8 : WaveReal8Tag
{
    protected override double Sample() => WaveMath.Sawtooth();
}

internal sealed class SquareReal4 : WaveReal4Tag
{
    protected override double Sample() => WaveMath.Square();
}

internal sealed class SquareReal8 : WaveReal8Tag
{
    protected override double Sample() => WaveMath.Square();
}

internal sealed class TriangleReal4 : WaveReal4Tag
{
    protected override double Sample() => WaveMath.Triangle();
}

internal sealed class TriangleReal8 : WaveReal8Tag
{
    protected override double Sample() => WaveMath.Triangle();
}

internal sealed class ReadErrorTag : ITagSource
{
    public object? Read() => throw new OpcException(
        OpcResultId.BadRights,
        "Read Error tag returns OPC_E_BADRIGHTS");

    public bool TryWrite(object? value) => true;
}

internal sealed class WriteErrorTag : ITagSource
{
    public object Read() => 0;

    public bool TryWrite(object? value) => throw new OpcException(
        OpcResultId.BadRights,
        "Write Error tag returns OPC_E_BADRIGHTS");
}

internal static class RandomValues
{
    public static float NextSingle() => RandomNumberGenerator.GetInt32(0, 1_000_000) / 1_000_000.0F;
    public static double NextDouble() => RandomNumberGenerator.GetInt32(0, 1_000_000) / 1_000_000.0D;
    public static sbyte NextSByte() => (sbyte)RandomNumberGenerator.GetInt32(sbyte.MinValue, sbyte.MaxValue + 1);
    public static short NextInt16() => (short)RandomNumberGenerator.GetInt32(short.MinValue, short.MaxValue + 1);
    public static int NextInt32() => RandomNumberGenerator.GetInt32(0, int.MaxValue);
    public static byte NextByte() => (byte)RandomNumberGenerator.GetInt32(byte.MinValue, byte.MaxValue + 1);
    public static ushort NextUInt16() => (ushort)RandomNumberGenerator.GetInt32(ushort.MinValue, ushort.MaxValue + 1);

    public static uint NextUInt32()
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        RandomNumberGenerator.Fill(bytes);
        return BitConverter.ToUInt32(bytes);
    }

    public static bool NextBoolean() => RandomNumberGenerator.GetInt32(0, 2) == 1;

    public static string NextString() => string.Create(
        CultureInfo.InvariantCulture,
        $"Random-{Guid.NewGuid():N}");
}

internal static class WaveMath
{
    private const long PeriodMilliseconds = 10_000;

    public static double Sawtooth() => Phase() * 100.0D;
    public static double Square() => Phase() < 0.5D ? 100.0D : 0.0D;

    public static double Triangle()
    {
        var phase = Phase();
        return phase < 0.5D
            ? phase * 200.0D
            : (1.0D - phase) * 200.0D;
    }

    private static double Phase()
    {
        var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % PeriodMilliseconds;
        return elapsed / (double)PeriodMilliseconds;
    }
}
