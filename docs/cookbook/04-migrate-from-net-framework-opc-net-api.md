# Adopt Opc.Classic in OPC NET API projects

## What this covers

Map common OPC Foundation .NET Framework OPC NET API concepts to current .NET 10 `Opc.Classic.*` APIs. 

Core types:

- `OpcUrl` and `OpcConnectData` are in `Opc.Classic`.
- `IDaServer`, `IDaSubscription`, `Item`, `ItemValueResult`, `DataChange`, and `SubscriptionState` are in `Opc.Classic.Da`.
- `OpcBatchPropertyId` in `Opc.Classic.Batch` provides typed OPC Batch property IDs 400-478 for Batch property porting.
- Generated DCOM projections such as `IOPCServerClientProxy` are in `Opc.Classic.Da.Dcom`.

## Project file

Use a current target framework and opt into AOT validation where your application supports it.

```xml
<TargetFramework>net10.0</TargetFramework>
<IsAotCompatible>true</IsAotCompatible>
```

Reference the areas you need:

```bash
dotnet add package Opc.Classic.Core
dotnet add package Opc.Classic.Da
dotnet add package Opc.Classic.Dcom
```

## API mapping

| OPC NET API concept | Opc.Classic concept |
| --- | --- |
| `Opc.URL` | `OpcUrl` |
| `Opc.ConnectData` | `OpcConnectData` |
| custom OPC credentials | `System.Net.NetworkCredential` |
| `Opc.Da.Server` | `IDaServer` or generated `IOPCServerClientProxy` |
| `Opc.Da.Subscription` | `IDaSubscription` |
| `Opc.Da.Item` | `Item` |
| `Opc.Da.ItemValue` | `ItemValue` / `ItemValueResult` |
| synchronous callbacks | `IAsyncEnumerable<DataChange>` |
| COM activation defaults | explicit `OpcConnectData` authentication and protection policy |

## Connection setup

```csharp
using System.Net;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", password, "CORP"),
    OpcProtectionLevel.Integrity);
```

Use `OpcConnectData.WithKerberos(...)` for Active Directory environments with a configured `RPCSS/<fqdn>` service principal.

## DA read and subscription shape

```csharp
using Opc.Classic.Da;

await using IDaSubscription subscription = await server.CreateSubscriptionAsync(
    new SubscriptionState { Name = "process", Active = true, UpdateRateMs = 1000 },
    cancellationToken);

var items = new[] { new Item("Random.Int1") { ClientHandle = 1 } };
await subscription.AddItemsAsync(items, cancellationToken);

await foreach (DataChange change in subscription.DataChanges.WithCancellation(cancellationToken))
{
    foreach (ItemValueResult item in change.Items)
    {
        Console.WriteLine($"{item.ItemName}: {item.Value} {item.Quality} {item.Timestamp:O}");
    }
}
```

## Important differences

- Async-first: `Task`, `IAsyncEnumerable<T>`, and `CancellationToken` are the default shape.
- Packet integrity is the default for cross-machine DCOM authentication.
- Values are represented by DA result records and `OpcVariant` at the wire layer.
- Source-generated proxies and dispatchers provide static AOT-safe DCOM bindings.
- Generated client proxy names use the interface name plus `ClientProxy`, for example `IOPCServerClientProxy`.
- OPC IDL-defined identifiers keep their spec spelling, including underscores where present.

## Analyzer support

`Opc.Classic.MigrationAnalyzer` provides Roslyn diagnostics and code fixes for common OPC NET API usage patterns. Use it when you want automated guidance while updating a larger application.

## Validation path

1. Start with a known ProgID or CLSID and `OpcUrl.Parse(...)`.
2. Use NTLMv2 with `OpcProtectionLevel.Integrity`.
3. Verify `GetStatusAsync` before adding item/group logic.
4. Add reads and subscriptions.
5. Enable Kerberos/SPNEGO where Active Directory policy requires it.
6. Publish the application with trimming or NativeAOT if that is part of your deployment model.
