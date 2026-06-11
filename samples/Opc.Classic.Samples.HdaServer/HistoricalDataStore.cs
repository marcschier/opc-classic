// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

namespace Opc.Classic.Samples.HdaServer;

public sealed class HistoricalDataStore
{
    private static readonly string[] s_tagIds = ["Sensor.Temperature", "Sensor.Pressure", "Sensor.FlowRate"];

    // Lazily seed each tag on first access so the SCM-launched HDA EXE
    // can complete CoRegisterClassObject without waiting on the seed
    // loop. Constructing the store itself is now effectively free (the
    // dictionary is empty until the first ReadRaw / ItemIds / Contains
    // call). This prevents the SCM activation race that previously
    // returned RPC_S_SERVER_UNAVAILABLE for HDA but not for AE.
    private readonly Lazy<Dictionary<string, List<(DateTimeOffset Time, double Value)>>> _data;

    public HistoricalDataStore()
    {
        EndTime = DateTimeOffset.UtcNow;
        StartTime = EndTime.AddDays(-1);
        _data = new Lazy<Dictionary<string, List<(DateTimeOffset, double)>>>(
            BuildData,
            System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public DateTimeOffset StartTime { get; }

    public DateTimeOffset EndTime { get; }

    public IReadOnlyCollection<string> ItemIds => _data.Value.Keys;

    public bool Contains(string tagId) => _data.Value.ContainsKey(tagId);

    private Dictionary<string, List<(DateTimeOffset Time, double Value)>> BuildData()
    {
        var data = new Dictionary<string, List<(DateTimeOffset, double)>>(StringComparer.OrdinalIgnoreCase);
        Seed(data, "Sensor.Temperature", StartTime, EndTime, t => 20.0 + 5.0 * Math.Sin(t.TotalHours / 12.0 * Math.PI));
        Seed(data, "Sensor.Pressure", StartTime, EndTime, t => 101.3 + 0.5 * Math.Cos(t.TotalHours / 6.0 * Math.PI));
        Seed(data, "Sensor.FlowRate", StartTime, EndTime, t => 50.0 + 10.0 * Math.Sin(t.TotalHours * Math.PI));
        // OPC Foundation TestServer-compatible Random.* item names so
        // probes targeting default Matrikon/TestServer item IDs work.
        Seed(data, "Random.Int4", StartTime, EndTime, t => 1000.0 * Math.Sin(t.TotalHours));
        Seed(data, "Random.Real8", StartTime, EndTime, t => 3.14 * Math.Cos(t.TotalHours));
        Seed(data, "Random.String", StartTime, EndTime, t => (int)t.TotalSeconds);
        return data;
    }

    private static void Seed(
        Dictionary<string, List<(DateTimeOffset Time, double Value)>> data,
        string tagId,
        DateTimeOffset start,
        DateTimeOffset end,
        Func<TimeSpan, double> generator)
    {
        // Sample every 10 seconds (was 1 second) to keep the seed loop under
        // ~10k entries per tag. The original 1-second cadence over a 24-hour
        // window produced 86,400 entries per tag, pushing startup past 2s.
        var list = new List<(DateTimeOffset, double)>(capacity: 8700);
        for (var t = start; t <= end; t = t.AddSeconds(10))
        {
            list.Add((t, generator(t - start)));
        }

        data[tagId] = list;
    }

    public IEnumerable<(DateTimeOffset Time, double Value)> ReadRaw(string tagId, DateTimeOffset start, DateTimeOffset end, int maxValues)
    {
        if (!_data.Value.TryGetValue(tagId, out var samples))
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
