// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Globalization;
using Microsoft.Extensions.Logging;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Hosting;

namespace Opc.Classic.Samples.HdaServer;

public sealed partial class SampleHdaServer : IOpcHdaServer
{
    private static readonly Action<ILogger, Exception?> GetStatusMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(GetStatusAsync)),
        "GetStatus");

    private static readonly Action<ILogger, int, int, Exception?> ReadRawMessage = LoggerMessage.Define<int, int>(
        LogLevel.Information,
        new EventId(2, nameof(ReadRawAsync)),
        "ReadRaw: itemCount={ItemCount}, maxValues={MaxValues}");

    private static readonly Action<ILogger, int, HdaAggregate, Exception?> ReadProcessedMessage = LoggerMessage.Define<int, HdaAggregate>(
        LogLevel.Information,
        new EventId(3, nameof(ReadProcessedAsync)),
        "ReadProcessed: itemCount={ItemCount}, aggregate={Aggregate}");

    private static readonly DateTimeOffset StartupTime = DateTimeOffset.UtcNow;
    private static readonly uint GoodQuality = OpcQuality.Good.RawValue;

    private readonly HistoricalDataStore _store;
    private readonly ILogger<SampleHdaServer> _logger;

    public SampleHdaServer(HistoricalDataStore store, ILogger<SampleHdaServer> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetStatusMessage(_logger, null);
        var now = DateTimeOffset.UtcNow;
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = StartupTime,
            CurrentTime = now,
            LastUpdateTime = _store.EndTime,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            MaxReturnValues = 10_000,
            VendorInfo = "Opc.Classic .NET HDA Sample",
        };

        return Task.FromResult(status);
    }

    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();

        var results = new int[itemIds.Length];
        for (var index = 0; index < itemIds.Length; index++)
        {
            results[index] = _store.Contains(itemIds[index])
                ? OpcResultId.Ok.Code
                : OpcResultId.UnknownItemId.Code;
        }

        return Task.FromResult(results);
    }

    public Task<OpcHdaItem[]> ReadRawAsync(
        string[] itemIds,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ct.ThrowIfCancellationRequested();
        ReadRawMessage(_logger, itemIds.Length, maxValues, null);

        (DateTimeOffset start, DateTimeOffset end) = NormalizeRange(ResolveTime(startTime), ResolveTime(endTime));
        var items = new OpcHdaItem[itemIds.Length];
        for (var index = 0; index < itemIds.Length; index++)
        {
            var samples = _store.ReadRaw(itemIds[index], start, end, maxValues).ToArray();
            items[index] = CreateItem(index + 1, 0, samples);
        }

        return Task.FromResult(items);
    }

    public Task<OpcHdaItem[]> ReadProcessedAsync(
        string[] itemIds,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        TimeSpan resampleInterval,
        HdaAggregate aggregate,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        if (resampleInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(resampleInterval), resampleInterval, "Resample interval must be greater than zero.");
        }

        ct.ThrowIfCancellationRequested();
        ReadProcessedMessage(_logger, itemIds.Length, aggregate, null);

        (DateTimeOffset start, DateTimeOffset end) = NormalizeRange(ResolveTime(startTime), ResolveTime(endTime));
        var items = new OpcHdaItem[itemIds.Length];
        for (var index = 0; index < itemIds.Length; index++)
        {
            var processed = ProcessItem(itemIds[index], start, end, resampleInterval, aggregate, ct);
            items[index] = CreateItem(index + 1, (int)aggregate, processed);
        }

        return Task.FromResult(items);
    }

    private static OpcHdaItem CreateItem(
        int clientHandle,
        int aggregateHandle,
        (DateTimeOffset Time, double Value)[] samples)
    {
        var timestamps = new DateTimeOffset[samples.Length];
        var qualities = new uint[samples.Length];
        var values = new OpcVariant[samples.Length];

        for (var index = 0; index < samples.Length; index++)
        {
            timestamps[index] = samples[index].Time;
            qualities[index] = GoodQuality;
            values[index] = OpcVariant.FromDouble(samples[index].Value);
        }

        return new OpcHdaItem(clientHandle, aggregateHandle, timestamps, qualities, values);
    }

    private (DateTimeOffset Time, double Value)[] ProcessItem(
        string itemId,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan resampleInterval,
        HdaAggregate aggregate,
        CancellationToken cancellationToken)
    {
        var processed = new List<(DateTimeOffset, double)>();
        for (var bucketStart = start; bucketStart < end; bucketStart = bucketStart.Add(resampleInterval))
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset bucketEnd = Min(bucketStart.Add(resampleInterval), end);
            var samples = _store.ReadRaw(itemId, bucketStart, bucketEnd, 0).ToArray();
            if (samples.Length == 0)
            {
                continue;
            }

            processed.Add((bucketStart, Aggregate(samples, aggregate)));
        }

        return processed.ToArray();
    }

    private static double Aggregate(
        (DateTimeOffset Time, double Value)[] samples,
        HdaAggregate aggregate) =>
        aggregate switch
        {
            HdaAggregate.Count => samples.Length,
            HdaAggregate.Minimum or HdaAggregate.MinimumActualTime => samples.Min(static sample => sample.Value),
            HdaAggregate.Maximum or HdaAggregate.MaximumActualTime => samples.Max(static sample => sample.Value),
            HdaAggregate.Total => samples.Sum(static sample => sample.Value),
            HdaAggregate.Start => samples[0].Value,
            HdaAggregate.End => samples[^1].Value,
            HdaAggregate.Delta => samples[^1].Value - samples[0].Value,
            HdaAggregate.Range => samples.Max(static sample => sample.Value) - samples.Min(static sample => sample.Value),
            HdaAggregate.StandardDeviation => StandardDeviation(samples),
            HdaAggregate.Variance => Variance(samples),
            _ => samples.Average(static sample => sample.Value),
        };

    private static double StandardDeviation((DateTimeOffset Time, double Value)[] samples) =>
        Math.Sqrt(Variance(samples));

    private static double Variance((DateTimeOffset Time, double Value)[] samples)
    {
        double average = samples.Average(static sample => sample.Value);
        return samples.Average(sample => Math.Pow(sample.Value - average, 2.0));
    }

    private static (DateTimeOffset Start, DateTimeOffset End) NormalizeRange(DateTimeOffset start, DateTimeOffset end) =>
        start <= end ? (start, end) : (end, start);

    private static DateTimeOffset Min(DateTimeOffset first, DateTimeOffset second) =>
        first <= second ? first : second;

    private static DateTimeOffset ResolveTime(OpcHdaTime time)
    {
        if (!time.IsStringExpression)
        {
            return time.Timestamp.ToUniversalTime();
        }

        string expression = time.StringExpression?.Trim() ?? string.Empty;
        var now = DateTimeOffset.UtcNow;
        if (expression.Length == 0 || expression.Equals("NOW", StringComparison.OrdinalIgnoreCase))
        {
            return now;
        }

        if (TryParseNowOffset(expression, now, out DateTimeOffset resolved))
        {
            return resolved;
        }

        return DateTimeOffset.TryParse(
            expression,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out DateTimeOffset parsed)
            ? parsed
            : now;
    }

    private static bool TryParseNowOffset(string expression, DateTimeOffset now, out DateTimeOffset resolved)
    {
        resolved = now;
        if (!expression.StartsWith("NOW", StringComparison.OrdinalIgnoreCase) || expression.Length < 6)
        {
            return false;
        }

        char sign = expression[3];
        if (sign is not ('-' or '+'))
        {
            return false;
        }

        string magnitude = expression[4..^1];
        char unit = char.ToUpperInvariant(expression[^1]);
        if (!double.TryParse(magnitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return false;
        }

        TimeSpan offset = unit switch
        {
            'S' => TimeSpan.FromSeconds(value),
            'M' => TimeSpan.FromMinutes(value),
            'H' => TimeSpan.FromHours(value),
            'D' => TimeSpan.FromDays(value),
            _ => TimeSpan.Zero,
        };

        if (offset == TimeSpan.Zero && value != 0.0)
        {
            return false;
        }

        resolved = sign == '-' ? now.Subtract(offset) : now.Add(offset);
        return true;
    }
}
