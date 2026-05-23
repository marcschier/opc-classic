# Migrate from the .NET Framework OPC NET API to Opc.Classic.*

## What this covers

Move an application from `OpcCom.Da.Server` to .NET 10 and `Opc.Classic.Da`.

## Status / availability

`OpcUrl` / `OpcConnectData` are in `src\Opc.Classic.Core`; `IDaServer`, `IDaSubscription`, `Item`, `ItemValueResult`, and `SubscriptionState` are in `src\Opc.Classic.Da`. DCOM call shims are Phase 6B, so final factory names may change.

## Project file

```xml
<!-- old -->
<TargetFramework>net462</TargetFramework>
```

```xml
<!-- new -->
<TargetFramework>net10.0</TargetFramework>
<IsAotCompatible>true</IsAotCompatible>
```

Remove WCF and Windows-only COM dependencies:

```bash
dotnet add package Opc.Classic.Da
```

## Connection side-by-side

```csharp
// Old
var server = new Opc.Da.Server(new OpcCom.Factory(), null);
server.Connect(
    new Opc.URL("opcda://win-opc01/Matrikon.OPC.Simulation.1"),
    new Opc.ConnectData(new NetworkCredential("opc-reader", password, "CORP")));
```

```csharp
// New
var url = OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", password, "CORP"),
    OpcProtectionLevel.Integrity);

await using IDaServer server =
    await DaClient.ConnectAsync(connectData, cancellationToken); // planned Phase 6B factory
```

## Subscription side-by-side

```csharp
// Old: synchronous callbacks and boxed values
var group = (Opc.Da.Subscription)server.CreateSubscription(new Opc.Da.SubscriptionState
{
    Name = "process", Active = true, UpdateRate = 1000,
});
group.AddItems(new[] { new Opc.Da.Item { ItemName = "Random.Int1" } });
group.DataChanged += (_, _, values) => { /* Value, Quality, Timestamp */ };
```

```csharp
// New: async value-quality-timestamp triples
await using var subscription = await server.CreateSubscriptionAsync(
    new SubscriptionState { Name = "process", Active = true, UpdateRateMs = 1000 },
    cancellationToken);

await subscription.AddItemsAsync(new[] { new Item("Random.Int1") { ClientHandle = 1 } }, cancellationToken);

await foreach (DataChange change in subscription.DataChanges.WithCancellation(cancellationToken))
    foreach (ItemValueResult item in change.Items)
        Console.WriteLine($"{item.ItemName}: {item.Value} {item.Quality} {item.Timestamp:O}");
```

## Important differences

- Async-first: `Task`, `IAsyncEnumerable<T>`, and `CancellationToken`.
- Public DA values are `object?`; wire/NDR code uses `OpcVariant`.
- `CoCreateInstance` becomes `OpcUrl` plus `OpcConnectData`.
- Reflection-based marshaling becomes NDR codecs plus source-generated proxies. See `docs\ARCHITECTURE.md`.

