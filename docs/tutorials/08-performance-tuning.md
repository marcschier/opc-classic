# Performance tuning Opc.Classic applications

Applies to Opc.Classic 0.6.0-alpha.1; the public API shape targets 1.0.0-rc.1.

OPC Classic performance problems often appear in layers: tag batches that are too small, callbacks that allocate per item, historians returning too many samples, authentication signing overhead, and NDR codecs on hot paths. Opc.Classic gives you AOT-friendly primitives, generated proxies, span-based NDR readers/writers, and explicit DTOs. This tutorial shows how to use those primitives without fighting the runtime.

The most important rule is to measure. Use Release builds, representative fixtures, application-level metrics, and focused microbenchmarks as the gate for changes to NDR, generated proxies, `OpcVariant`, and dispatcher code. Do not tune by intuition alone.

## Prerequisites

- A working DA, AE, or HDA client/server built on Opc.Classic.
- Familiarity with `ArrayPool<T>`, spans, async I/O, and allocation profiling.
- A representative test server or replay fixture.
- Release builds; Debug numbers are misleading.

## What you'll learn

- Where the hot paths are in generated DCOM calls.
- How to use `NdrWriter` and `NdrReader` efficiently.
- How `ArrayPool<byte>` is used by generated proxies and dispatchers.
- How to reduce `OpcVariant` boxing in application code.
- How to choose DA batch sizes for `AddItems`, `Read`, and `Write`.
- How to pipeline asynchronous operations safely.

## The hot path sequence

A generated proxy call follows the same core sequence described in [../ARCHITECTURE.md](../ARCHITECTURE.md):

1. rent a scratch buffer;
2. encode parameters with `NdrWriter` and static codecs;
3. call `ICallChannel.InvokeAsync(interfaceId, opnum, payload, ct)`;
4. check the returned HRESULT;
5. decode the response with `NdrReader` and static codecs;
6. return the buffer to `ArrayPool<byte>`.

Server dispatchers mirror the same pattern. DA, AE, and HDA hosting dispatchers rent response buffers and return them in `finally` blocks. The generator also emits `ArrayPool<byte>.Shared.Rent(1024)` for generated proxies. That means the library already avoids per-call request-buffer allocation for common paths. Your job is not to add another abstraction on top that copies payloads three times.

## NdrWriter and NdrReader basics

`NdrWriter` and `NdrReader` are `ref struct` types over spans. They are forward-only, little-endian, and self-aligning. Keep them local to synchronous encoding/decoding blocks; do not store them in fields or capture them across `await`.

```csharp
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

Span<byte> buffer = stackalloc byte[256];
var writer = new NdrWriter(buffer);

var state = new OpcItemState(
    ClientHandle: 7,
    Timestamp: DateTimeOffset.UtcNow,
    Quality: OpcQuality.Good,
    Value: OpcVariant.FromDouble(42.5));

NdrOpcItemStateCodec.Write(ref writer, state);
int length = writer.Position;

var reader = new NdrReader(buffer[..length]);
OpcItemState decoded = NdrOpcItemStateCodec.Read(ref reader);
Console.WriteLine(decoded.Value.AsDouble());
```

Use `stackalloc` only for small, bounded payloads. For generated calls or variable-length arrays, prefer pooled arrays.

## Use ArrayPool correctly

When you need a temporary byte buffer, rent, slice, and return it in `finally`:

```csharp
using System.Buffers;
using Opc.Classic.Ndr;

byte[] rented = ArrayPool<byte>.Shared.Rent(4096);
try
{
    var writer = new NdrWriter(rented.AsSpan());
    writer.WriteInt32(123);
    writer.WriteUnicodeStringPtr("Random.Real8");
    ReadOnlyMemory<byte> payload = rented.AsMemory(0, writer.Position);

    // Pass payload to the next layer before returning the buffer, or copy if it must outlive this scope.
    Console.WriteLine($"Encoded {payload.Length} bytes");
}
finally
{
    ArrayPool<byte>.Shared.Return(rented);
}
```

Never return a rented buffer while another asynchronous operation still references it. If the payload must outlive the method, copy the used slice to a new array. That allocation is cheaper than a data-corruption bug.

Also avoid clearing every returned buffer unless it contains secrets. Clearing large buffers can dominate CPU time. OPC values are usually process data, not credentials, but authentication tokens and passwords should never be stored in reusable buffers outside the auth layer.

## Codec choice and generated proxies

Prefer generated proxies and static codecs over reflection-based dispatch. The repository source generator emits interface IDs, opnum tables, and client proxy classes. Static codecs exist for core DA, AE, HDA, and Batch structs. That is faster and trimming-safe because the runtime does not need to discover methods or build expression trees.

If you add a new wire DTO, write a small codec with explicit read/write methods:

```csharp
public static class MyStructCodec
{
    public static void Write(ref NdrWriter writer, MyStruct value)
    {
        writer.WriteInt32(value.Handle);
        writer.WriteUnicodeStringPtr(value.Name);
    }

    public static MyStruct Read(ref NdrReader reader)
    {
        int handle = reader.ReadInt32();
        string name = reader.ReadUnicodeStringPtr() ?? string.Empty;
        return new MyStruct(handle, name);
    }
}

public readonly record struct MyStruct(int Handle, string Name);
```

Do not use `MethodInfo.Invoke`, `Activator.CreateInstance(Type)`, or expression compilation in `src` code. Those APIs are banned by the repository's AOT rules and are slow on hot paths.

## OpcVariant boxing avoidance

`OpcVariant` is a value type that carries a `VarType` and a boxed value for scalar types. That shape is simple, AOT-friendly, and correct for COM `VARIANT`, but repeated boxing can show up in high-frequency DA loops.

Recommendations:

- Convert to `OpcVariant` at the boundary, not repeatedly inside business logic.
- Keep numeric calculations in native `double`, `int`, or `bool` arrays until you write to OPC.
- Avoid converting `OpcVariant` to `object` and back.
- Use typed accessors (`AsDouble`, `AsInt32`, `AsBoolean`) instead of pattern matching on `Boxed` everywhere.

```csharp
static OpcVariant ToOpcValue(double engineeringValue) => OpcVariant.FromDouble(engineeringValue);

static double ReadDoubleOrNaN(OpcVariant value) => value.AsDouble() ?? double.NaN;
```

For large arrays, prefer SAFEARRAY-aware paths as they land rather than one boxed `OpcVariant` per element.

## DA batch sizing

Batch size has a bigger impact than most micro-optimizations. OPC DA calls have fixed overhead: DCOM call setup, authentication verifier, NDR headers, per-call HRESULTs, and network latency. Reading one item per call is almost always slower than reading a batch.

Suggested starting points:

| Operation | Starting batch size | Tune by |
| --- | ---: | --- |
| `AddItems` | 100-500 items | server max item count, item validation latency |
| `Read` from cache | 100-1000 items | response size, callback latency, UI needs |
| `Read` from device | 20-200 items | device scan cost, PLC load |
| `Write` | 20-200 items | criticality, error isolation, audit requirements |
| HDA raw read | time-window cap + continuation | max values per item, historian load |

For DA subscriptions, group tags by update rate and operational importance. Do not put 10 ms control-loop values and 30 second status values in the same group. The server will sample at the group cadence or maintain extra per-item scheduling.

## Async I/O pipelining

Async does not mean unlimited parallelism. Pipeline independent calls, but cap concurrency per server. A simple pattern uses `SemaphoreSlim`:

```csharp
public sealed class OpcCallLimiter
{
    private readonly SemaphoreSlim _gate = new(initialCount: 4);

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
```

Use one limiter per server connection. Unlimited concurrent reads can create head-of-line blocking inside the server, overwhelm authentication signing, or trigger DCOM call timeouts.

## Authentication overhead

`OpcProtectionLevel.Integrity` signs packets. `Privacy` signs and encrypts. Both add CPU work and bytes on the wire. Do not disable integrity to gain speed; hardened DCOM servers require it and security risk is not worth the small gain. Instead:

- increase batch size;
- reduce callback chatter with deadbands;
- avoid unnecessary refresh calls;
- keep connections warm;
- place gateways near servers to reduce latency.

## Server-side publishing

For managed DA servers, `IOpcDaDataChangePublisher` fans out `OpcDaDataChange` batches. Publish batches, not individual item notifications. Use one batch per group scan when possible. Keep slow subscribers isolated and make queue limits explicit. The default publisher catches subscriber exceptions and continues fan-out, but your producer still needs backpressure policy.

## HDA query performance

Historians can return millions of samples. Favor processed reads for dashboards and raw reads only for drill-down. Use `maxValuesPerItem` and continuation handles. For daily reports, request hourly aggregates rather than raw one-second samples and client-side aggregation.

## Measurement checklist

Track these metrics before changing code:

- calls per second by OPC interface/method;
- bytes encoded/decoded per call;
- allocation rate and Gen0 collections;
- DCOM round-trip latency;
- per-item HRESULT distribution;
- callback queue length;
- HDA samples returned per query;
- CPU split between auth, NDR, and application processing.

Once the repository benchmarks project lands, add microbenchmarks for `NdrWriter`, `NdrReader`, `OpcVariant`, generated proxy encode/decode, and dispatcher response encoding. Keep those benchmarks in CI for regression detection.

## Pitfalls

- Returning a pooled buffer too early corrupts payloads.
- Capturing spans or ref structs across `await` does not compile; redesign the scope.
- One OPC call per tag is simple but slow.
- Huge batches hide partial failures and can exceed server limits.
- Client-side aggregation of large HDA raw ranges wastes network and server time.
- Performance fixes that introduce reflection can break AOT.

## Building a repeatable benchmark harness

Until the repository benchmark project lands, create a small local harness that uses the same payloads every run. Benchmark three layers separately: pure codec encode/decode, in-memory call-channel round trips, and real server calls. Mixing them together makes results hard to interpret. A codec benchmark should not open sockets; a network benchmark should not allocate random item lists every iteration.

Representative payloads are more important than synthetic extremes. Include a DA read of 10 items, 100 items, and 1000 items; an HDA raw response with 100 and 10,000 samples; an AE event burst with simple, tracking, and condition events; and a callback batch with good and failed item rows. Keep payloads in source-controlled fixtures so regressions are comparable.

Use release builds and pin CPU settings where possible. Container CPU throttling, laptop power saving, and debugger attachment can dwarf codec changes. Record runtime version, OS, architecture, and commit hash with every benchmark result.

## Allocation investigation workflow

When allocation rate is high, find the owner before changing APIs. Use `dotnet-counters`, `dotnet-trace`, or your profiler of choice to identify whether allocations come from application LINQ, logging message formatting, NDR string decoding, `OpcVariant` boxing, callback queueing, or serialization outside Opc.Classic. Optimize the biggest proven source first.

Common application-level allocation traps include calling `.ToArray()` repeatedly in polling loops, constructing new item lists every scan, logging interpolated strings at disabled levels, and converting values to strings before checking whether logs are enabled. Prefer static `LoggerMessage` delegates in very hot paths, as the samples do.

## Backpressure and freshness

High throughput is useless if values are stale. Track freshness separately from throughput. For DA subscriptions, record callback timestamp, processing start, processing end, and latest value timestamp. If processing delay exceeds the update rate, reduce item count, increase deadband, split groups, or slow the scan. For HDA, track query duration and returned sample count. For AE, track event lag and dropped-event policy.

Backpressure should be explicit. If downstream storage is slow, either buffer with a bounded queue and visible lag metric, shed low-priority data, or fail readiness. An unbounded queue can make memory look fine during testing and then fail catastrophically during an alarm flood.

## Security and performance trade-offs

Do not present packet signing as optional performance tuning. Integrity is the security baseline. If signing cost is visible, compensate with batching, local gateways, and reduced chatter. If encryption with `Privacy` is required, benchmark it under real batch sizes and CPU limits. Document the decision so future maintainers do not "optimize" by weakening authentication.

## Tuning deployment topology

Sometimes the fastest code change is moving the gateway. A client running in a distant cloud region will not beat the latency of a small gateway VM near the OPC server. Place latency-sensitive DA and AE gateways close to the plant network, then forward normalized data to cloud services asynchronously. For HDA exports, schedule work near the historian and upload compressed results rather than streaming raw samples over high-latency links.

Topology also affects callback reliability. A DA subscription with server-to-client callbacks across NAT, service meshes, or container overlays can be fragile. If callbacks are critical, test the exact network path and prefer stable callback endpoints. Performance tuning that ignores topology will plateau quickly.

## Performance budgets

Set budgets before tuning. Examples: a status call under one second, cache reads under 500 milliseconds for 500 items, callback processing lag under two update periods, and HDA dashboard queries under five seconds. Budgets turn performance from an endless optimization exercise into an engineering contract. They also help decide when to split groups, add a gateway, or reject an oversized query.

Budgets should include failure behavior. A timeout that fires in thirty seconds may be acceptable for manual exports but unacceptable for a control-room display. Use different timeouts and batch sizes for interactive, background, and audit workloads.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Next steps

- Read [10-aot-and-trimming.md](10-aot-and-trimming.md) before adding dynamic code.
- Use [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md) to add logs and traces around measured bottlenecks.
- Review [../ARCHITECTURE.md](../ARCHITECTURE.md) for source-generator and NDR flow.

## References

- OPC DA 3.00 for group, item, and callback batch semantics.
- OPC HDA 1.20 for aggregate and continuation behavior.
- [MS-RPCE] for packet fragmentation and authentication verifier overhead.
- Repository files: `src\Opc.Classic.Core\Ndr\NdrWriter.cs`, `NdrReader.cs`, `src\Opc.Classic.Generators\OpcProxyGenerator.cs`, and per-spec hosting dispatchers.




