# Migrating from the OPC Foundation .NET API

Updated for Opc.Classic 0.4.0-alpha.1.

Many OPC Classic applications still depend on the OPC Foundation .NET API for .NET Framework. Those applications usually run only on Windows, use synchronous calls, rely on COM registration, and carry types such as `Opc.Da.Server`, `Opc.Da.Subscription`, `Opc.Da.Item`, `Opc.Da.ItemValueResult`, and `Opc.ConnectData`. Opc.Classic keeps the OPC concepts but changes the platform assumptions: namespaces are `Opc.Classic.*`, APIs are async, cancellation is explicit, DCOM is pure managed on the portable path, and NativeAOT/trimming shape the design.

This guide maps the old patterns to the new ones. It is not an automated migration yet. The planned `rw-d5-migration-tooling` work is in progress and is expected to provide scripts/analyzers that catch common namespace and synchronous-call changes. Until then, migrate feature by feature and keep behavior tests around every OPC operation.

For the short version see [../cookbook/04-migrate-from-net-framework-opc-net-api.md](../cookbook/04-migrate-from-net-framework-opc-net-api.md). For architecture, read [../ARCHITECTURE.md](../ARCHITECTURE.md).

## Prerequisites

- A project using the legacy OPC Foundation .NET API or a similar .NET Framework OPC Classic wrapper.
- .NET 10 SDK.
- An inventory of DA, AE, HDA, security, discovery, and hosting features your application uses.
- Access to a test OPC server with known tags/events/history.

## What you'll learn

- How namespaces and packages map.
- How connection data maps to `OpcUrl` and `OpcConnectData`.
- How synchronous DA calls map to async `IDaServer` methods.
- How subscriptions map to `IAsyncEnumerable` streams.
- Which migration gotchas matter most.
- How to prepare for automated migration tooling.

## Migration strategy

Do not start with a search-and-replace across the whole solution. First identify integration boundaries. Most legacy applications have one or two classes that own `Opc.Da.Server` connections and expose domain-specific methods such as `ReadPumpStatus` or `WriteSetpoint`. Migrate those adapter classes first, keep the rest of the application unchanged, and add tests around the adapter behavior.

Recommended phases:

1. **Compile on .NET 10 without OPC changes** where possible.
2. **Introduce domain interfaces** that hide the legacy OPC API.
3. **Replace connection configuration** with `OpcUrl` and `OpcConnectData`.
4. **Migrate one-shot reads/writes**.
5. **Migrate subscriptions and callbacks**.
6. **Migrate AE/HDA feature areas**.
7. **Remove Windows-only COM assumptions**.
8. **Enable trimming/AOT gates** if the app is a service or container.

## Namespace and package translations

| Legacy concept | Opc.Classic replacement | Notes |
| --- | --- | --- |
| `Opc` | `Opc.Classic` | Core types such as `OpcUrl`, `OpcConnectData`, `OpcResultId`. |
| `Opc.Da` | `Opc.Classic.Da` | `IDaServer`, `IDaSubscription`, `Item`, `ItemValueResult`, `SubscriptionState`. |
| `Opc.Ae` | `Opc.Classic.Ae` | `IAeServer`, `IAeSubscription`, `EventNotification`, `SubscriptionFilter`. |
| `Opc.Hda` | `Opc.Classic.Hda` | `IHdaServer`, `HdaTime`, `HdaReadResult`, `HdaAggregate`. |
| `OpcCom` / COM RCW helpers | `Opc.Classic.Dcom` generated proxies | Portable path avoids Windows COM RCWs. |
| `Opc.ConnectData` | `OpcConnectData` | Immutable, includes auth mode and protection level. |
| `Opc.URL` | `OpcUrl` | Parses `opcda://host/ProgId` and related schemes. |
| `Opc.Da.Server` | `IDaServer` | Async interface; implementation supplied by DCOM adapter, tests, or loopback. |
| `Opc.Da.Subscription` | `IDaSubscription` | Async disposal and `IAsyncEnumerable<DataChange>`. |

## Connection mapping

Legacy code often looked like this:

```csharp
// Legacy OPC Foundation .NET API style.
var url = new Opc.URL("opcda://opc01/Matrikon.OPC.Simulation.1");
var connectData = new Opc.ConnectData(new NetworkCredential("opc-reader", password, "PLANT"));
var server = new Opc.Da.Server(new OpcCom.Factory(), null);
server.Connect(url, connectData);
```

In Opc.Classic, parse the URL and construct explicit connection data:

```csharp
using System.Net;
using Opc.Classic;

OpcUrl url = OpcUrl.Parse("opcda://opc01/Matrikon.OPC.Simulation.1");
var credentials = new NetworkCredential("opc-reader", password, "PLANT");
OpcConnectData connectData = OpcConnectData.WithNtlmV2(
    url,
    credentials,
    OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));
```

`OpcConnectData` does not open the connection by itself. Your application should receive an `IDaServer`, `IAeServer`, or `IHdaServer` from dependency injection. That keeps the business logic testable and lets you swap loopback, in-memory, DCOM, or future adapters without changing reads and writes.

## DA read mapping

Legacy synchronous DA reads often used arrays and returned mixed result/value objects:

```csharp
// Legacy style.
var items = new[]
{
    new Opc.Da.Item { ItemName = "Random.Real8", ClientHandle = 1 },
};
Opc.Da.ItemValueResult[] values = server.Read(items);
Console.WriteLine(values[0].Value);
```

The new high-level shape is async and cancellation-aware:

```csharp
using Opc.Classic.Da;

public static async Task ReadDaAsync(IDaServer server, CancellationToken cancellationToken)
{
    var items = new[]
    {
        new Item("Random.Real8") { ClientHandle = 1 },
    };

    IReadOnlyList<ItemValueResult> values = await server.ReadAsync(items, cancellationToken).ConfigureAwait(false);
    foreach (ItemValueResult value in values)
    {
        if (value.ResultId.IsFailure)
        {
            string text = await server.GetErrorTextAsync(value.ResultId, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"{value.ItemName} failed: {text}");
            continue;
        }

        Console.WriteLine($"{value.ItemName}={value.Value} quality={value.Quality} timestamp={value.Timestamp:O}");
    }
}
```

The new code forces you to decide what to do with partial failure. That is a good thing: OPC bulk operations routinely return mixed results.

## Subscription mapping

Legacy code usually subscribed with event handlers:

```csharp
// Legacy style.
var groupState = new Opc.Da.SubscriptionState
{
    Name = "fast",
    Active = true,
    UpdateRate = 1000,
};
Opc.Da.Subscription subscription = (Opc.Da.Subscription)server.CreateSubscription(groupState);
subscription.DataChanged += OnDataChanged;
subscription.AddItems(items);
```

Opc.Classic exposes the callback stream as `IAsyncEnumerable<DataChange>`:

```csharp
using Opc.Classic.Da;

public static async Task SubscribeAsync(IDaServer server, CancellationToken cancellationToken)
{
    await using IDaSubscription subscription = await server.CreateSubscriptionAsync(
        new SubscriptionState
        {
            Name = "fast",
            Active = true,
            UpdateRateMs = 1000,
            KeepAliveMs = 10_000,
        },
        cancellationToken).ConfigureAwait(false);

    await subscription.AddItemsAsync(
        [new Item("Random.Real8") { ClientHandle = 1 }],
        cancellationToken).ConfigureAwait(false);

    await foreach (DataChange change in subscription.DataChanges.WithCancellation(cancellationToken).ConfigureAwait(false))
    {
        foreach (ItemValueResult item in change.Items)
        {
            Console.WriteLine($"tx={change.TransactionId} {item.ItemName}={item.Value}");
        }
    }
}
```

`await using` is important. Disposing the subscription removes the server-side group and releases callback state.

## Write mapping

Legacy writes typically passed `ItemValue` arrays and ignored per-item result codes. Preserve those result codes in the migration:

```csharp
using Opc.Classic.Da;

public static async Task WriteSetpointAsync(IDaServer server, double setpoint, CancellationToken cancellationToken)
{
    IReadOnlyList<IdentifiedResult> results = await server.WriteAsync(
        [
            new ItemValue("Controller.Setpoint")
            {
                ClientHandle = 10,
                Value = setpoint,
                Quality = OpcQuality.Good,
                Timestamp = DateTimeOffset.UtcNow,
            },
        ],
        cancellationToken).ConfigureAwait(false);

    foreach (IdentifiedResult result in results)
    {
        OpcException.ThrowIfFailed(result.ResultId, $"Write {result.ItemName}");
    }
}
```

Some servers require `WriteVQT` semantics with value, quality, and timestamp; others ignore quality/timestamp. Keep those differences in your adapter tests.

## AE mapping

Legacy AE code often used callback events. The new surface uses `IAeSubscription.Events`:

```csharp
using Opc.Classic.Ae;

public static async Task MonitorAlarmsAsync(IAeServer server, CancellationToken cancellationToken)
{
    await using IAeSubscription subscription = await server.CreateSubscriptionAsync(
        active: true,
        bufferTimeMs: 100,
        maxBufferSize: 100,
        cancellationToken).ConfigureAwait(false);

    await subscription.SetFilterAsync(
        new SubscriptionFilter { EventTypes = EventType.Condition, MinSeverity = 250 },
        cancellationToken).ConfigureAwait(false);

    await foreach (EventNotification notification in subscription.Events.WithCancellation(cancellationToken))
    {
        Console.WriteLine($"{notification.Source}: {notification.Message}");
    }
}
```

Acknowledgement maps to `AcknowledgeAsync(actor, comment, IReadOnlyList<ConditionRef>)` and returns per-condition `AckResult` rows.

## HDA mapping

Legacy HDA APIs often exposed raw and processed reads as synchronous arrays. Opc.Classic uses `IHdaServer`:

```csharp
using Opc.Classic.Hda;

public static async Task ReadHistoryAsync(IHdaServer historian, CancellationToken cancellationToken)
{
    IReadOnlyList<HdaReadResult> raw = await historian.ReadRawAsync(
        ["Sensor.Temperature"],
        HdaTime.Relative("NOW-1H"),
        HdaTime.Now,
        maxValuesPerItem: 100,
        includeBounds: false,
        cancellationToken).ConfigureAwait(false);

    IReadOnlyList<HdaReadResult> average = await historian.ReadProcessedAsync(
        [new AggregateRequest("Sensor.Temperature", HdaAggregate.Average)],
        HdaTime.Relative("NOW-1H"),
        HdaTime.Now,
        TimeSpan.FromMinutes(5),
        cancellationToken).ConfigureAwait(false);
}
```

Use `HdaTime.Relative` instead of hand-parsing `NOW` expressions. Keep all internal timestamps in UTC.

## API equivalence table

| Legacy operation | New operation |
| --- | --- |
| `Server.GetStatus()` | `await IDaServer.GetStatusAsync(ct)` |
| `Server.Browse(...)` | `await foreach (BrowseElement in IDaServer.BrowseAsync(...))` |
| `Server.Read(Item[])` | `await IDaServer.ReadAsync(IReadOnlyList<Item>, ct)` |
| `Server.Write(ItemValue[])` | `await IDaServer.WriteAsync(IReadOnlyList<ItemValue>, ct)` |
| `Server.CreateSubscription(...)` | `await IDaServer.CreateSubscriptionAsync(SubscriptionState, ct)` |
| `Subscription.AddItems(...)` | `await IDaSubscription.AddItemsAsync(...)` |
| `Subscription.DataChanged += ...` | `await foreach (DataChange in subscription.DataChanges)` |
| `Subscription.Refresh()` | `await IDaSubscription.RefreshAsync(fromCache, ct)` |
| `AeServer.AcknowledgeCondition(...)` | `await IAeServer.AcknowledgeAsync(...)` |
| `HdaServer.ReadRaw(...)` | `await IHdaServer.ReadRawAsync(...)` |
| `HdaServer.ReadProcessed(...)` | `await IHdaServer.ReadProcessedAsync(...)` |

## Gotchas

- Async all the way: do not call `.Result` or `.Wait()` around OPC calls.
- Cancellation is now part of the API. Thread-abort style timeouts should be removed.
- `ClientHandle` and server handles are not interchangeable.
- OPC success warnings are not exceptions. Preserve `S_FALSE` and `OPC_S_*` values.
- The portable path does not use `[ComImport]` or Windows RCWs.
- Reflection-heavy configuration may break trimming. Prefer explicit options and source-generated code.
- Namespace changes are simple; behavior changes are not. Test every browse/read/write/subscription path.

## Migration scripts while rw-d5 tooling is in progress

Until dedicated tooling exists, use conservative scripts that only report likely edits. For example, a PowerShell inventory:

```powershell
Get-ChildItem -Recurse -Filter *.cs |
  Select-String -Pattern 'Opc\.Da|Opc\.Ae|Opc\.Hda|Opc\.ConnectData|Opc\.URL' |
  Select-Object Path, LineNumber, Line
```

And a dry-run namespace map:

```powershell
$map = @{
  'using Opc.Da;' = 'using Opc.Classic.Da;'
  'using Opc.Ae;' = 'using Opc.Classic.Ae;'
  'using Opc.Hda;' = 'using Opc.Classic.Hda;'
}

Get-ChildItem -Recurse -Filter *.cs | ForEach-Object {
  $text = Get-Content $_.FullName -Raw
  foreach ($key in $map.Keys) {
    if ($text.Contains($key)) {
      [pscustomobject]@{ File = $_.FullName; From = $key; To = $map[$key] }
    }
  }
}
```

Do not run blind replacements on generated files, vendored code, or old samples. Let `rw-d5-migration-tooling` own mechanical rewrites once it lands.

## Refactoring patterns that reduce risk

The safest migration introduces an anti-corruption layer. Define interfaces in your application language, not in OPC language. For example, `IPumpTelemetry.ReadAsync` can return `PumpSnapshot` while the implementation uses `IDaServer`. Once the rest of the app depends on `IPumpTelemetry`, you can swap legacy OPC Foundation code for Opc.Classic without touching UI, business rules, or database code.

Keep behavior tests at that boundary. Use the old implementation to capture expected behavior for known fixtures: missing tag, bad quality, write denied, unsupported rate, callback keep-alive, and reconnect. Then run the same tests against the Opc.Classic implementation. Namespace changes are easy; subtle behavior differences are where migrations fail.

## Handling synchronous callers

Many .NET Framework applications are synchronous. Do not wrap Opc.Classic async calls with `.Result` on UI threads. Instead, move OPC work into background services or async command handlers. If a synchronous boundary is unavoidable during transition, isolate it in one adapter and use `Task.Run` with a clear timeout, then remove it later. Treat that adapter as technical debt.

For Windows Forms or WPF migrations, push OPC reads into an async service and marshal results back to the UI thread. The old API often hid blocking COM calls inside event handlers; the new API makes latency visible. Use that visibility to improve responsiveness.

## Data type and quality differences

Legacy wrappers sometimes converted COM variants directly to .NET objects and hid canonical types. Opc.Classic exposes `OpcVariant` in wire DTOs and plain `object?` in higher-level DA values. Be explicit about conversions at your boundary. For example, convert `double`, `int`, and `bool` with validation and log unexpected types. Do not assume every numeric server returns `double`; many DA servers use `short`, `int`, `float`, or arrays.

Quality handling also deserves attention. Some legacy applications checked only whether a value existed. Migrate those checks to inspect `OpcQuality.Quality`, substatus, and per-item `ResultId`. A value with bad quality should not drive control decisions even if it has the right .NET type.

## Deployment changes after migration

Moving from a Windows-only API to Opc.Classic often changes deployment. Linux containers need Kerberos files, DNS, NTP, firewall rules, and health checks. NativeAOT binaries need trimming-safe configuration. Managed servers need stable listen addresses and ProgID/CLSID registration strategy. Include deployment work in the migration plan, not as a final packaging task.

A good acceptance test is cross-platform: run the same domain-level read and subscription tests on Windows and Linux. If behavior differs, inspect authentication and callback reachability before changing business code.

## Code review checklist for migrated files

During review, look for synchronous blocking, lost cancellation tokens, missing per-item result handling, and accidental Windows-only assumptions. Every new OPC call should pass a `CancellationToken`. Every bulk operation should inspect each row. Every subscription should use `await using`. Every catch block for `Exception` should be justified; prefer `OpcException`, `OperationCanceledException`, and domain-specific exceptions.

Check logging too. Old code often logged only `ex.Message`. New code should include `OpcResultId`, item count, server URL, operation name, and whether the operation was read, write, browse, or subscription. Do not log credentials from the old `ConnectData` object when porting configuration.

## Parallel run technique

For high-risk migrations, run old and new adapters side by side in read-only mode. Read the same tags from both clients, compare values, quality, timestamps, and result IDs, and publish differences to a diagnostic log. Do not write through both adapters. For subscriptions, compare callback rates and final values over a fixed window. Differences may be caused by update-rate negotiation, deadband, or server-specific COM behavior; document them before switching production traffic.

## Deleting legacy dependencies

Remove legacy OPC packages only after all call sites are gone and deployment artifacts no longer need COM registration from the old stack. Search project files, installer scripts, service startup scripts, and documentation. A stale installer step can re-register old COM components and confuse client discovery even after code migration is complete.

## Training the team

A migration changes developer habits. Hold a short workshop on async cancellation, per-item HRESULTs, `OpcQuality`, NativeAOT restrictions, and the difference between client handles and server handles. Review one converted read, one write, one subscription, and one error path together. This investment prevents repeated review comments and helps operations understand why logs look different after the migration.

Update runbooks and support dashboards at the same time as code. If support staff search for legacy exception names or old ProgIDs, they will miss the new failure evidence. Include a mapping from old logs to new structured fields in the release notes.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Next steps

- Build a fresh DA client with [01-build-your-first-da-client.md](01-build-your-first-da-client.md).
- Review hosting changes in [02-host-an-opc-server.md](02-host-an-opc-server.md).
- Prepare deployment and AOT gates with [03-cross-platform-deployment.md](03-cross-platform-deployment.md) and [10-aot-and-trimming.md](10-aot-and-trimming.md).

## References

- OPC DA 3.00, AE 1.10, and HDA 1.20.
- [MS-DCOM] for the transport assumptions that replace Windows COM RCWs on the portable path.
- Repository docs: [../ADOPTION.md](../ADOPTION.md), [../ARCHITECTURE.md](../ARCHITECTURE.md), and [../cookbook/04-migrate-from-net-framework-opc-net-api.md](../cookbook/04-migrate-from-net-framework-opc-net-api.md).




