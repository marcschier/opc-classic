// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

namespace Opc.Classic.Samples.HdaServer;

public sealed class HistoricalDataStore
{
    private readonly Dictionary<string, List<(DateTimeOffset Time, double Value)>> _data;

    public HistoricalDataStore()
    {
        _data = new Dictionary<string, List<(DateTimeOffset, double)>>(StringComparer.OrdinalIgnoreCase);
        EndTime = DateTimeOffset.UtcNow;
        StartTime = EndTime.AddDays(-1);
        Seed("Sensor.Temperature", StartTime, EndTime, t => 20.0 + 5.0 * Math.Sin(t.TotalHours / 12.0 * Math.PI));
        Seed("Sensor.Pressure", StartTime, EndTime, t => 101.3 + 0.5 * Math.Cos(t.TotalHours / 6.0 * Math.PI));
        Seed("Sensor.FlowRate", StartTime, EndTime, t => 50.0 + 10.0 * Math.Sin(t.TotalHours * Math.PI));
    }

    public DateTimeOffset StartTime { get; }

    public DateTimeOffset EndTime { get; }

    public IReadOnlyCollection<string> ItemIds => _data.Keys;

    public bool Contains(string tagId) => _data.ContainsKey(tagId);

    private void Seed(string tagId, DateTimeOffset start, DateTimeOffset end, Func<TimeSpan, double> generator)
    {
        // Sample every 10 seconds (was 1 second) to keep the seed loop under
        // ~10k entries per tag. The original 1-second cadence over a 24-hour
        // window produced 86,400 entries per tag, pushing startup past 2s
        // and contributing to SCM activation timing flakiness on some hosts.
        var list = new List<(DateTimeOffset, double)>(capacity: 8700);
        for (var t = start; t <= end; t = t.AddSeconds(10))
        {
            list.Add((t, generator(t - start)));
        }

        _data[tagId] = list;
    }

    public IEnumerable<(DateTimeOffset Time, double Value)> ReadRaw(string tagId, DateTimeOffset start, DateTimeOffset end, int maxValues)
    {
        if (!_data.TryGetValue(tagId, out var samples))
        {
            yield break;
        }

        var count = 0;
        foreach (var (t, v) in samples)
        {
            if (t < start)
            {
                continue;
            }

            if (t > end)
            {
                yield break;
            }

            yield return (t, v);
            if (maxValues > 0 && ++count >= maxValues)
            {
                yield break;
            }
        }
    }
}
