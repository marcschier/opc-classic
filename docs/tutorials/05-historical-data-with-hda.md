# Historical data with OPC HDA

OPC Historical Data Access is the query side of OPC Classic. Data Access answers "what is the value now?"; HDA answers "what happened over this time range?" A production historian client needs raw reads, processed aggregates, annotations, modified-value audit trails, continuation handling, and careful time semantics. This tutorial shows how to model those scenarios with `Opc.Classic.Hda` and how the sample client and server fit together.

Use the HDA client and HDA server samples as bookends. The client sample builds a loopback call path with `LoopbackHdaClient`, `IOPCHDA_ServerClientProxy`, `IOPCHDA_SyncReadClientProxy`, `IOPCHDA_SyncAnnotationsClientProxy`, and `IOPCHDA_AsyncReadClientProxy`; when `OPC_CLASSIC_SERVER_HOST` and `OPC_CLASSIC_SERVER_PORT` are set it uses `DcomCallChannelFactory.ConnectTcpAsync` instead of the in-process channel. The server sample implements `IOpcHdaServer` over `HistoricalDataStore` and reads `OPC_CLASSIC_SAMPLE_PORT` (default `51302`) or `OPC_CLASSIC_LISTEN_ADDRESS`. The Windows CCW path also covers SyncUpdate, AsyncUpdate, Playback, annotation insert, and async advise vtables for native-client validation. The public application-level contract is `IHdaServer`, which exposes browse, raw reads, processed reads, read-at-time, annotations, and continuation reads.

## Prerequisites

- .NET 10 SDK.
- Familiarity with UTC timestamps and historian bucket queries.
- Optional: a production HDA server or the repository HDA sample.

## What you'll learn

- How to choose absolute and relative HDA times.
- How to read raw values and inspect per-item `OpcResultId` values.
- How to request processed values with built-in aggregates.
- How annotations and modified values differ.
- How to design server-side aggregate behavior.
- How to avoid common historian pitfalls.

## HDA time model

`HdaTime` is either absolute or relative. Absolute times are UTC `DateTimeOffset` values. Relative times use OPC HDA's `NOW` grammar, for example `NOW-10M`, `NOW-1H`, or `NOW-7D+12H`.

```csharp
using Opc.Classic.Hda;

HdaTime start = HdaTime.Relative("NOW-1H");
HdaTime end = HdaTime.Now;
HdaTime fixedStart = HdaTime.Absolute(DateTimeOffset.UtcNow.AddDays(-1));
```

Resolve relative times once per operation, not once per item. If you resolve `NOW` separately for each item, fast queries can still produce slightly different windows. The sample server normalizes start and end and supports simple `NOW±duration` expressions in `SampleHdaServer.ResolveTime`.

## Read raw values

A raw read returns the stored samples in chronological order. The application-level interface returns one `HdaReadResult` per item:

```csharp
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Hda;

public sealed class HistorianReader
{
    private readonly IHdaServer _historian;
    private readonly ILogger<HistorianReader> _logger;

    public HistorianReader(IHdaServer historian, ILogger<HistorianReader> logger)
    {
        _historian = historian;
        _logger = logger;
    }

    public async Task ReadRawAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<HdaReadResult> results = await _historian.ReadRawAsync(
            ["Sensor.Temperature", "Sensor.Pressure"],
            HdaTime.Relative("NOW-10M"),
            HdaTime.Now,
            maxValuesPerItem: 100,
            includeBounds: false,
            cancellationToken).ConfigureAwait(false);

        foreach (HdaReadResult result in results)
        {
            if (result.ResultId.IsFailure)
            {
                _logger.LogWarning("ReadRaw failed for {Item}: {Result}", result.ItemId, result.ResultId);
                continue;
            }

            foreach (HdaItemValue value in result.Values)
            {
                _logger.LogInformation(
                    "{Item} {Timestamp:O} value={Value} quality={Quality}",
                    result.ItemId,
                    value.Timestamp,
                    value.Value,
                    value.Quality);
            }
        }
    }
}
```

`maxValuesPerItem` is a server-side cap. A production client should treat a non-null `ContinuationHandle` as an instruction to call `ReadNextAsync`. Do not increase the cap until the server returns everything; doing so can create huge responses and long lock times on older historians.

## Processed values and aggregates

Processed reads resample the time range into fixed intervals and ask the server to compute an aggregate for each interval. Opc.Classic includes `HdaAggregate` values matching OPC HDA built-ins: `Average`, `TimeAverage`, `Minimum`, `Maximum`, `Total`, `Count`, `StandardDeviation`, `Variance`, `Delta`, `Range`, and others.

```csharp
IReadOnlyList<HdaReadResult> processed = await _historian.ReadProcessedAsync(
    requests:
    [
        new AggregateRequest("Sensor.Temperature", HdaAggregate.Average),
        new AggregateRequest("Sensor.Pressure", HdaAggregate.Maximum),
        new AggregateRequest("Sensor.FlowRate", HdaAggregate.Total),
    ],
    startTime: HdaTime.Relative("NOW-1H"),
    endTime: HdaTime.Now,
    resampleInterval: TimeSpan.FromMinutes(5),
    cancellationToken).ConfigureAwait(false);
```

The sample server implements aggregate calculation in `SampleHdaServer.Aggregate`. It maps `Count` to sample count, `Minimum` and `Maximum` to extrema, `Total` to sum, `StandardDeviation` and `Variance` to statistical functions, and defaults to average. A production historian should follow OPC HDA 1.20 aggregate definitions precisely, especially for `TimeAverage` and quality-weighted calculations. `Average` and `TimeAverage` are not always interchangeable: a simple average weights each sample equally, while time average weights by duration.

## Server-side raw and processed implementation

The hosting interface `IOpcHdaServer` is lower-level and uses wire-oriented DTOs. The sample server shows the pattern:

```csharp
using Opc.Classic;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Hosting;

public sealed class MinimalHdaServer : IOpcHdaServer
{
    private readonly Dictionary<string, List<(DateTimeOffset Time, double Value)>> _data = new(StringComparer.Ordinal)
    {
        ["Sensor.Temperature"] = Enumerable.Range(0, 60)
            .Select(i => (DateTimeOffset.UtcNow.AddMinutes(-60 + i), 20.0 + i / 10.0))
            .ToList(),
    };

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = now.AddHours(-1),
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            MaxReturnValues = 10_000,
            VendorInfo = "Minimal HDA Server",
        });
    }

    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        int[] results = itemIds
            .Select(item => _data.ContainsKey(item) ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code)
            .ToArray();
        return Task.FromResult(results);
    }

    public Task<OpcHdaItem[]> ReadRawAsync(
        string[] itemIds,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        CancellationToken ct = default)
    {
        DateTimeOffset start = startTime.IsStringExpression ? DateTimeOffset.UtcNow.AddHours(-1) : startTime.Timestamp;
        DateTimeOffset end = endTime.IsStringExpression ? DateTimeOffset.UtcNow : endTime.Timestamp;
        OpcHdaItem[] items = itemIds.Select((itemId, index) => CreateItem(index + 1, itemId, start, end, maxValues)).ToArray();
        return Task.FromResult(items);
    }

    private OpcHdaItem CreateItem(int clientHandle, string itemId, DateTimeOffset start, DateTimeOffset end, int maxValues)
    {
        (DateTimeOffset Time, double Value)[] samples = _data.TryGetValue(itemId, out var values)
            ? values.Where(v => v.Time >= start && v.Time <= end).Take(maxValues <= 0 ? int.MaxValue : maxValues).ToArray()
            : [];

        return new OpcHdaItem(
            clientHandle,
            AggregateHandle: 0,
            Timestamps: samples.Select(static s => s.Time).ToArray(),
            Qualities: samples.Select(static _ => OpcQuality.Good.RawValue).ToArray(),
            Values: samples.Select(static s => OpcVariant.FromDouble(s.Value)).ToArray());
    }
}
```

That snippet is intentionally minimal. The repository sample has more complete time parsing and aggregate handling. In production, validate item IDs before reading, enforce server maximums, preserve quality, and return per-item failures rather than throwing for a single missing item.

## Annotations

Annotations are notes attached to historical timestamps. They are not the value itself and should not be displayed as samples. The public DTOs are `HdaAnnotationResult` and `HdaAnnotation`:

```csharp
IReadOnlyList<HdaAnnotationResult> annotations = await _historian.ReadAnnotationsAsync(
    ["Sensor.Temperature"],
    HdaTime.Relative("NOW-1D"),
    HdaTime.Now,
    cancellationToken).ConfigureAwait(false);

foreach (HdaAnnotationResult result in annotations)
{
    foreach (HdaAnnotation annotation in result.Annotations)
    {
        Console.WriteLine($"{result.ItemId} {annotation.Timestamp:O}: {annotation.AnnotationText} by {annotation.User}");
    }
}
```

Opc.Classic keeps HDA wire interfaces and application DTOs separate. Generated HDA proxies preserve protocol shapes such as `IOPCHDA_SyncAnnotations`, while the managed DTO surface presents annotations through `HdaAnnotation` and `HdaAnnotationResult`. Keep that distinction clear when writing docs or UIs so wire-level capability names do not leak into operator workflows.

## Modified values

OPC HDA can expose modified historical values through `IOPCHDA_SyncRead::ReadModified`. Opc.Classic models the wire shape with `OpcHdaModifiedItem`: parallel arrays for timestamps, qualities, values, modification times, edit types, and users. Modified values answer audit questions: who inserted, replaced, or deleted a historical value, and when?

```csharp
var modified = new OpcHdaModifiedItem(
    clientHandle: 1,
    timestamps: [DateTimeOffset.UtcNow.AddMinutes(-30)],
    qualities: [OpcQuality.Good.RawValue],
    values: [OpcVariant.FromDouble(21.5)],
    modificationTimes: [DateTimeOffset.UtcNow],
    editTypes: [1u],
    users: ["historian-admin"]);
```

Do not merge modified-value audit rows into raw history without marking them. Operators need to know whether a displayed value is original, inserted, replaced, or deleted. OPC HDA 1.20 defines edit types; preserve them even if your UI initially shows only raw values.

## Time-aggregated query design

For dashboards, prefer processed reads over raw reads followed by client-side aggregation. Server-side aggregation keeps network payloads small and lets the historian apply its quality and interpolation rules. Pick bucket sizes based on the question:

- one-minute buckets for recent troubleshooting;
- five- or fifteen-minute buckets for shift dashboards;
- one-hour buckets for daily reporting;
- one-day buckets for compliance summaries.

Always log the exact start, end, aggregate, and interval used. A trend labeled "average temperature" is not reproducible unless the interval and time zone are explicit.

## Pitfalls

- Treat all times as UTC in code; convert only at UI boundaries.
- `includeBounds` may add values at start/end that were not stored samples.
- `Total` and `TimeAverage` require domain-specific interpretation for units.
- Empty result sets are not always errors; they may mean no samples in the interval.
- Do not assume every server supports every aggregate. Call `GetSupportedAggregatesAsync` on `IHdaServer` when using the high-level client shape.
- Huge raw reads can starve older historians. Page with continuation handles.

## Continuations and pagination

Historian reads should be designed for pagination from the beginning. `HdaReadResult.ContinuationHandle` exists because servers may cap responses even when the client asks for more. A UI that requests a month of one-second data should not block the server until every value is returned. Instead, request a bounded range, render partial results, and ask for more only when the operator zooms or exports.

A continuation loop around the high-level interface looks like this:

```csharp
IReadOnlyList<HdaReadResult> firstPage = await historian.ReadRawAsync(
    ["Sensor.Temperature"],
    HdaTime.Relative("NOW-1D"),
    HdaTime.Now,
    maxValuesPerItem: 1000,
    includeBounds: false,
    cancellationToken).ConfigureAwait(false);

int? handle = firstPage[0].ContinuationHandle;
while (handle is int continuation)
{
    IReadOnlyList<HdaReadResult> next = await historian.ReadNextAsync(
        ["Sensor.Temperature"],
        [continuation],
        maxValuesPerItem: 1000,
        cancellationToken).ConfigureAwait(false);

    handle = next[0].ContinuationHandle;
}
```

Always provide a cancellation token and an export timeout. Long historian exports should be background jobs with progress, not web request handlers.

## Quality-aware aggregation

A historian is not just a time-series database. OPC quality affects aggregate meaning. A simple average over bad-quality samples can create a plausible but wrong number. Decide how your application treats bad or uncertain quality before presenting aggregates. Some reports should exclude bad-quality samples; others should show a separate quality coverage metric such as "93% good samples".

For time-weighted aggregates, sample spacing matters. If one value persists for ten minutes and another for ten seconds, a time average should weight the first more heavily. The sample server's aggregate implementation is intentionally simple for readability; production historians should follow OPC HDA 1.20 aggregate rules and vendor documentation.

## Annotation governance

Annotations are operational records. Decide who can create, modify, and delete them. Include annotation text, annotation time, user, and the timestamp the note refers to. If your application displays raw values and annotations together, make the visual distinction clear. Operators should not confuse a note saying "sensor recalibrated" with a process value.

When exporting data for compliance, include annotations and modified-value audit rows alongside the raw or processed values. A report that omits modifications may appear inconsistent with the historian's audit trail.

## Testing HDA behavior

Create deterministic historian fixtures. Seed a store with known values at fixed timestamps, then assert raw reads, processed reads, boundary inclusion, and empty intervals. Test reversed start/end ranges because many users choose ranges from UI controls in either direction. Test relative times by injecting an evaluation clock in your application layer, even if the server ultimately resolves `NOW`.

For performance tests, separate server computation time from network time. A slow processed read may be caused by aggregate calculation, huge payloads, authentication signing, or client-side rendering. Measure each layer before changing bucket sizes.

## Client UX for historical data

Historian clients should make query cost visible. A trend screen can start with processed five-minute averages, then fetch raw values only for the visible zoom window. An export workflow can estimate sample count before running and warn the user when a query spans millions of values. For operator displays, show the time zone clearly and keep UTC in logs so support can correlate with server traces.

When a query returns partial data, do not silently render it as complete. If `ContinuationHandle` is present, show a loading indicator or "more data available" state. If per-item `ResultId` indicates failure, display that row separately from empty-but-successful results. Empty success means no samples; failure means the item or query could not be served.

## Server retention and compaction

HDA behavior depends on retention policy. A server may store one-second data for seven days, one-minute aggregates for a year, and daily summaries forever. Your application should not assume the same resolution is available for every range. Query design, UI defaults, and export limits should reflect retention tiers. If the server exposes aggregate metadata, use it; otherwise document expected behavior per historian.

Compaction can also affect annotations and modified values. A value may be aggregated away while its audit trail remains. Preserve item IDs and timestamps in audit exports so users can reconcile summary reports with detailed history.

## Data validation after migration

When onboarding a historian, run known-window validation. Pick a time range where operators know the process behavior, read raw values, read processed values, and compare with the vendor's native historian UI. Differences should be explained before users trust reports. Common explanations include different time zones, inclusive versus exclusive end times, bounds insertion, quality filtering, and different definitions of average.

Keep validation scripts in source control. A future server upgrade, retention-policy change, or adapter update should rerun the same reads. Historical data is often used for compliance; reproducibility matters as much as raw connectivity.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Next steps

- Run `Opc.Classic.Samples.HdaServer` and `Opc.Classic.Samples.HdaClient`; for container ports and `OPC_CLASSIC_SERVER_HOST` / `OPC_CLASSIC_SERVER_PORT`, see [../../samples/README.docker.md](../../samples/README.docker.md).
- Compare server hosting with [02-host-an-opc-server.md](02-host-an-opc-server.md).
- Read [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md) for HRESULT and NDR diagnostics.
- Review [../ARCHITECTURE.md](../ARCHITECTURE.md) for NDR codec and generated proxy flow.

## References

- OPC HDA 1.20: `IOPCHDA_Server`, `IOPCHDA_SyncRead`, `IOPCHDA_SyncUpdate`, `IOPCHDA_SyncAnnotations`, `IOPCHDA_AsyncRead`, `IOPCHDA_AsyncUpdate`, and `IOPCHDA_Playback`.
- OPC HDA aggregate definitions for Average, TimeAverage, Minimum, Maximum, Total, Count, and StandardDeviation.
- Repository: `Opc.Classic.Hda`, `Opc.Classic.Samples.HdaServer`, and `Opc.Classic.Samples.HdaClient`.



