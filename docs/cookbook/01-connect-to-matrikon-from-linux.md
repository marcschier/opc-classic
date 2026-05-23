# Connect to Matrikon OPC Simulation Server from Linux

Updated for Opc.Classic 0.4.0-alpha.1.

## What this covers

A .NET 10 Linux client connects to a Windows Matrikon OPC DA Simulation Server, creates a DA group, and reads values.

## Status / availability

`OpcUrl` and `OpcConnectData` are in `src\Opc.Classic.Core`; `IDaServer`, `IDaSubscription`, `SubscriptionState`, and `Item` are in `src\Opc.Classic.Da`. DCOM call-shim emission for `IOPCServer` / `IOPCGroupStateMgt` is wired through generated `IOPCServerClientProxy`-style classes; Phase 14C remains the real Matrikon connectivity gate.

For in-repository samples, `samples\Opc.Classic.Samples.DaServer` mirrors a Matrikon-style DA tag tree, `samples\Opc.Classic.Samples.AotCanary` exercises the AOT-safe core/DA surface, and `samples\Opc.Classic.Samples.CttServer` exposes the CTT DA target registered as `Opc.Classic.DaSample.1`.

## Install

```bash
dotnet add package Opc.Classic.Core
dotnet add package Opc.Classic.Da
```

## Connect

```csharp
using System.Net;
using Opc.Classic;
using Opc.Classic.Da;

var url = OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", password, "CORP"),
    protectionLevel: OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));

await using IDaServer server =
    await DaClient.ConnectAsync(connectData, cancellationToken);
```

For the hardening rationale, see [05-dcom-hardening-pkt-integrity-explainer.md](05-dcom-hardening-pkt-integrity-explainer.md).

## Subscribe and read

```csharp
var items = new[]
{
    new Item("Random.Int1") { ClientHandle = 1001 },
    new Item("Random.Real8") { ClientHandle = 1002 },
};

await using var subscription = await server.CreateSubscriptionAsync(
    new SubscriptionState { Name = "linux-matrikon-sim", UpdateRateMs = 1000, Active = true },
    cancellationToken);

await subscription.AddItemsAsync(items, cancellationToken);
var snapshot = await server.ReadAsync(items, cancellationToken);

foreach (ItemValueResult value in snapshot)
    Console.WriteLine($"{value.ItemName}: {value.Value} {value.Quality} {value.Timestamp:O}");
```

For continuous updates, iterate `subscription.DataChanges`; each `DataChange` is one `IOPCDataCallback::OnDataChange` batch.

## Network requirements

Open TCP/135 for the Endpoint Mapper plus the dynamic TCP range used by the Matrikon COM endpoint. Prefer constraining the dynamic range on Windows, then scope firewall rules to the Linux client IP and server process.