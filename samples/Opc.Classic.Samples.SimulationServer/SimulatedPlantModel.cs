// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Globalization;

namespace Opc.Classic.Samples.SimulationServer;

/// <summary>
/// Canonical data types the simulated plant exposes. Feature-area modules map these
/// onto spec-specific representations (DA <c>VARTYPE</c>, HDA values, XML-DA types).
/// </summary>
public enum SimulatedDataType
{
    /// <summary>Boolean (VT_BOOL).</summary>
    Boolean,

    /// <summary>16-bit signed integer (VT_I2).</summary>
    Int16,

    /// <summary>32-bit signed integer (VT_I4).</summary>
    Int32,

    /// <summary>32-bit IEEE float (VT_R4).</summary>
    Single,

    /// <summary>64-bit IEEE float (VT_R8).</summary>
    Double,

    /// <summary>Unicode string (VT_BSTR).</summary>
    String,
}

/// <summary>
/// Deterministic signal shapes used to compute a tag's value from elapsed time.
/// </summary>
public enum SimulatedSignal
{
    /// <summary>Holds the last written or seeded constant value.</summary>
    Constant,

    /// <summary>Sine wave around a midpoint.</summary>
    Sine,

    /// <summary>Rising sawtooth that wraps at the period.</summary>
    Sawtooth,

    /// <summary>Square wave alternating between low and high each half-period.</summary>
    Square,

    /// <summary>Symmetric triangle wave.</summary>
    Triangle,

    /// <summary>Monotonic ramp that wraps at the configured maximum.</summary>
    Ramp,

    /// <summary>Seeded, time-quantized pseudo-random walk (deterministic for a given second).</summary>
    Random,
}

/// <summary>
/// A single addressable item in the simulated address space.
/// </summary>
public sealed record SimulatedTag
{
    /// <summary>Fully-qualified item id (e.g. <c>Plant.Reactor1.Temperature</c>).</summary>
    public required string ItemId { get; init; }

    /// <summary>Dotted browse-branch path of the parent (empty for root items).</summary>
    public required string BranchPath { get; init; }

    /// <summary>Leaf display name.</summary>
    public required string Name { get; init; }

    /// <summary>Canonical data type.</summary>
    public required SimulatedDataType DataType { get; init; }

    /// <summary>Signal shape used to derive the value over time.</summary>
    public required SimulatedSignal Signal { get; init; }

    /// <summary>Whether clients may write the item (writes override the generated value).</summary>
    public bool Writable { get; init; }

    /// <summary>Engineering low / signal minimum.</summary>
    public double Minimum { get; init; }

    /// <summary>Engineering high / signal maximum.</summary>
    public double Maximum { get; init; } = 100.0;

    /// <summary>Period of the periodic signals, in seconds.</summary>
    public double PeriodSeconds { get; init; } = 30.0;

    /// <summary>High-alarm threshold for analog tags that participate in the AE model (NaN = none).</summary>
    public double HighAlarm { get; init; } = double.NaN;

    /// <summary>Low-alarm threshold for analog tags that participate in the AE model (NaN = none).</summary>
    public double LowAlarm { get; init; } = double.NaN;

    /// <summary>Engineering units label, when meaningful.</summary>
    public string? Units { get; init; }
}

/// <summary>
/// The single deterministic world that every feature area projects. DA reads current
/// values, HDA replays the same value function over a time window, and AE derives
/// conditions from analog thresholds — so all specs stay mutually consistent.
/// </summary>
/// <remarks>
/// All generators are pure functions of (tag, timestamp), quantized to whole seconds,
/// so repeated reads at the same instant return identical values and tests are stable.
/// Writes to <see cref="SimulatedTag.Writable" /> tags are stored as overrides.
/// </remarks>
public sealed class SimulatedPlantModel
{
    private readonly List<SimulatedTag> _tags = [];
    private readonly Dictionary<string, SimulatedTag> _byId = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, object> _overrides = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates the model and seeds the canonical sample address space.</summary>
    public SimulatedPlantModel()
        : this(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
    {
    }

    /// <summary>Creates the model with an explicit deterministic epoch.</summary>
    public SimulatedPlantModel(DateTimeOffset startTimeUtc)
    {
        StartTimeUtc = startTimeUtc;
        SeedAddressSpace();
    }

    /// <summary>Deterministic epoch used as the origin for all time-based signals.</summary>
    public DateTimeOffset StartTimeUtc { get; }

    /// <summary>Vendor banner reported by feature-area status calls.</summary>
    public string VendorInfo { get; } = "Opc.Classic Full-Feature Simulation Server";

    /// <summary>Server version reported by feature-area status calls.</summary>
    public Version ServerVersion { get; } = new(1, 0, 0);

    /// <summary>All tags in deterministic declaration order.</summary>
    public IReadOnlyList<SimulatedTag> Tags => _tags;

    /// <summary>Looks up a tag by item id.</summary>
    public bool TryGetTag(string itemId, out SimulatedTag tag)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _byId.TryGetValue(itemId, out tag!);
    }

    /// <summary>Returns the immediate child branch names under <paramref name="branchPath" />.</summary>
    public IReadOnlyList<string> BrowseBranches(string branchPath)
    {
        ArgumentNullException.ThrowIfNull(branchPath);
        var prefix = branchPath.Length == 0 ? string.Empty : branchPath + ".";
        var children = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SimulatedTag tag in _tags)
        {
            if (tag.BranchPath.Length <= branchPath.Length)
            {
                continue;
            }

            if (!tag.BranchPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                !(branchPath.Length == 0 && tag.BranchPath.Length > 0))
            {
                continue;
            }

            var remainder = tag.BranchPath[prefix.Length..];
            var dot = remainder.IndexOf('.', StringComparison.Ordinal);
            children.Add(dot < 0 ? remainder : remainder[..dot]);
        }

        return [.. children];
    }

    /// <summary>Returns the leaf item ids directly under <paramref name="branchPath" />.</summary>
    public IReadOnlyList<SimulatedTag> BrowseLeaves(string branchPath)
    {
        ArgumentNullException.ThrowIfNull(branchPath);
        var leaves = new List<SimulatedTag>();
        foreach (SimulatedTag tag in _tags)
        {
            if (string.Equals(tag.BranchPath, branchPath, StringComparison.OrdinalIgnoreCase))
            {
                leaves.Add(tag);
            }
        }

        return leaves;
    }

    /// <summary>Records a client write to a writable tag. Returns false for unknown or read-only tags.</summary>
    public bool TryWrite(string itemId, object value)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(value);
        if (!_byId.TryGetValue(itemId, out SimulatedTag? tag) || !tag.Writable)
        {
            return false;
        }

        _overrides[itemId] = Coerce(tag.DataType, value);
        return true;
    }

    /// <summary>Computes the value of a tag at a timestamp (overrides win for writable tags).</summary>
    public object ValueAt(SimulatedTag tag, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (tag.Writable && _overrides.TryGetValue(tag.ItemId, out object? overridden))
        {
            return overridden;
        }

        return Coerce(tag.DataType, RawSignal(tag, timestamp));
    }

    /// <summary>Computes the current value of a tag at <paramref name="now" />.</summary>
    public object CurrentValue(SimulatedTag tag, DateTimeOffset now) => ValueAt(tag, now);

    /// <summary>
    /// Replays a deterministic historical series for a tag at the given sampling interval.
    /// </summary>
    public IReadOnlyList<(DateTimeOffset Timestamp, object Value)> History(
        SimulatedTag tag,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (interval <= TimeSpan.Zero)
        {
            interval = TimeSpan.FromSeconds(1);
        }

        var series = new List<(DateTimeOffset, object)>();
        for (DateTimeOffset t = start; t <= end; t = t.Add(interval))
        {
            series.Add((t, Coerce(tag.DataType, RawSignal(tag, t))));
        }

        return series;
    }

    private double RawSignal(SimulatedTag tag, DateTimeOffset timestamp)
    {
        double seconds = Math.Floor((timestamp - StartTimeUtc).TotalSeconds);
        double span = tag.Maximum - tag.Minimum;
        double period = tag.PeriodSeconds <= 0 ? 30.0 : tag.PeriodSeconds;
        double phase = seconds % period / period;

        return tag.Signal switch
        {
            SimulatedSignal.Constant => tag.Minimum,
            SimulatedSignal.Sine => tag.Minimum + span * 0.5 * (1.0 + Math.Sin(2.0 * Math.PI * phase)),
            SimulatedSignal.Sawtooth => tag.Minimum + span * phase,
            SimulatedSignal.Square => phase < 0.5 ? tag.Minimum : tag.Maximum,
            SimulatedSignal.Triangle => tag.Minimum + span * (phase < 0.5 ? phase * 2.0 : (1.0 - phase) * 2.0),
            SimulatedSignal.Ramp => tag.Minimum + span * phase,
            SimulatedSignal.Random => tag.Minimum + span * DeterministicUnit(tag.ItemId, (long)seconds),
            _ => tag.Minimum,
        };
    }

    private static double DeterministicUnit(string itemId, long second)
    {
        unchecked
        {
            ulong hash = 1469598103934665603UL;
            foreach (char c in itemId)
            {
                hash = (hash ^ c) * 1099511628211UL;
            }

            hash ^= (ulong)second + 0x9E3779B97F4A7C15UL;
            hash *= 1099511628211UL;
            hash ^= hash >> 29;
            return (hash >> 11) / (double)(1UL << 53);
        }
    }

    private static object Coerce(SimulatedDataType type, object value)
    {
        double d = value switch
        {
            bool b => b ? 1.0 : 0.0,
            string s => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed) ? parsed : 0.0,
            IConvertible convertible => convertible.ToDouble(CultureInfo.InvariantCulture),
            _ => 0.0,
        };

        return type switch
        {
            SimulatedDataType.Boolean => value is bool b ? b : d >= 0.5,
            SimulatedDataType.Int16 => (short)Math.Round(d),
            SimulatedDataType.Int32 => (int)Math.Round(d),
            SimulatedDataType.Single => (float)d,
            SimulatedDataType.Double => d,
            SimulatedDataType.String => value as string ?? d.ToString("0.###", CultureInfo.InvariantCulture),
            _ => d,
        };
    }

    private void Add(SimulatedTag tag)
    {
        _tags.Add(tag);
        _byId[tag.ItemId] = tag;
    }

    private void SeedAddressSpace()
    {
        AddSimple("Random", "Real8", SimulatedDataType.Double, SimulatedSignal.Random, 0, 100);
        AddSimple("Random", "Real4", SimulatedDataType.Single, SimulatedSignal.Random, 0, 100);
        AddSimple("Random", "Int4", SimulatedDataType.Int32, SimulatedSignal.Random, 0, 1000);

        AddSimple("Signals", "Sine", SimulatedDataType.Double, SimulatedSignal.Sine, -100, 100, period: 30);
        AddSimple("Signals", "Sawtooth", SimulatedDataType.Double, SimulatedSignal.Sawtooth, 0, 100, period: 20);
        AddSimple("Signals", "Square", SimulatedDataType.Double, SimulatedSignal.Square, 0, 100, period: 16);
        AddSimple("Signals", "Triangle", SimulatedDataType.Double, SimulatedSignal.Triangle, 0, 100, period: 24);
        AddSimple("Signals", "Ramp", SimulatedDataType.Double, SimulatedSignal.Ramp, 0, 100, period: 60);

        AddWritable("Bucket Brigade", "Int4", SimulatedDataType.Int32, 0);
        AddWritable("Bucket Brigade", "Real8", SimulatedDataType.Double, 0);
        AddWritable("Bucket Brigade", "Boolean", SimulatedDataType.Boolean, 0);
        AddWritable("Bucket Brigade", "String", SimulatedDataType.String, 0);

        AddProcess("Plant.Reactor1", "Temperature", -20, 200, period: 45, high: 160, low: 5, units: "degC");
        AddProcess("Plant.Reactor1", "Pressure", 0, 50, period: 35, high: 42, low: 2, units: "bar");
        AddProcess("Plant.Reactor1", "Level", 0, 100, period: 50, high: 90, low: 10, units: "%");
        AddProcess("Plant.Reactor2", "Temperature", -20, 200, period: 40, high: 160, low: 5, units: "degC");
        AddProcess("Plant.Reactor2", "Pressure", 0, 50, period: 30, high: 42, low: 2, units: "bar");
        AddProcess("Plant.Reactor2", "Flow", 0, 500, period: 25, high: 450, low: 20, units: "l/min");
    }

    private void AddSimple(
        string branch,
        string name,
        SimulatedDataType type,
        SimulatedSignal signal,
        double min,
        double max,
        double period = 30)
        => Add(new SimulatedTag
        {
            ItemId = branch + "." + name,
            BranchPath = branch,
            Name = name,
            DataType = type,
            Signal = signal,
            Minimum = min,
            Maximum = max,
            PeriodSeconds = period,
        });

    private void AddWritable(string branch, string name, SimulatedDataType type, double seed)
        => Add(new SimulatedTag
        {
            ItemId = branch + "." + name,
            BranchPath = branch,
            Name = name,
            DataType = type,
            Signal = SimulatedSignal.Constant,
            Writable = true,
            Minimum = seed,
            Maximum = seed,
        });

    private void AddProcess(
        string branch,
        string name,
        double min,
        double max,
        double period,
        double high,
        double low,
        string units)
        => Add(new SimulatedTag
        {
            ItemId = branch + "." + name,
            BranchPath = branch,
            Name = name,
            DataType = SimulatedDataType.Double,
            Signal = SimulatedSignal.Sine,
            Minimum = min,
            Maximum = max,
            PeriodSeconds = period,
            HighAlarm = high,
            LowAlarm = low,
            Units = units,
        });
}
