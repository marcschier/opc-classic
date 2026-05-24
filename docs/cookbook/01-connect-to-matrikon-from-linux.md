# Connect to Matrikon OPC Simulation Server from Linux

## What this covers

A .NET 10 Linux client connects to a Windows Matrikon OPC DA Simulation Server, authenticates with NTLMv2 or Kerberos, uses packet integrity, and reads DA values.

## Packages and namespaces

Use the `Opc.Classic.*` packages for the current source tree:

```bash
dotnet add package Opc.Classic.Core
dotnet add package Opc.Classic.Da
dotnet add package Opc.Classic.Dcom
```

Core types:

- `OpcUrl` and `OpcConnectData` are in `Opc.Classic`.
- `IDaServer`, `IDaSubscription`, `SubscriptionState`, `Item`, and `ItemValueResult` are in `Opc.Classic.Da`.
- Generated DA DCOM proxies, such as `IOPCServerClientProxy`, are in `Opc.Classic.Da.Dcom`.

## Connection policy

```csharp
using System.Net;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", password, "CORP"),
    protectionLevel: OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));
```

Use `OpcConnectData.WithKerberos(...)` when the Linux host has access to the domain KDC and the Matrikon host has a matching `RPCSS/<fqdn>` service principal. See [Kerberos in Active Directory](03-kerberos-in-active-directory.md).

## Read and subscribe through `IDaServer`

```csharp
using Opc.Classic.Da;

var items = new[]
{
    new Item("Random.Int1") { ClientHandle = 1001 },
    new Item("Random.Real8") { ClientHandle = 1002 },
};

IReadOnlyList<ItemValueResult> snapshot = await server.ReadAsync(items, cancellationToken);
foreach (ItemValueResult value in snapshot)
{
    Console.WriteLine($"{value.ItemName}: {value.Value} {value.Quality} {value.Timestamp:O}");
}

await using IDaSubscription subscription = await server.CreateSubscriptionAsync(
    new SubscriptionState { Name = "linux-matrikon-sim", UpdateRateMs = 1000, Active = true },
    cancellationToken);

await subscription.AddItemsAsync(items, cancellationToken);
await foreach (DataChange change in subscription.DataChanges.WithCancellation(cancellationToken))
{
    foreach (ItemValueResult item in change.Items)
    {
        Console.WriteLine($"{item.ItemName}: {item.Value} {item.Quality}");
    }
}
```

`DataChanges` yields one managed batch per `IOPCDataCallback::OnDataChange` callback.

## Network requirements

Open TCP/135 for the Endpoint Mapper plus the dynamic TCP range used by the Matrikon COM endpoint. Prefer constraining the dynamic range on Windows, then scope firewall rules to the Linux client IP and server process.

## Validation aids

- `samples\Opc.Classic.Samples.DaClient` shows the managed DA client shape.
- `samples\Opc.Classic.Samples.LoopbackDemo` validates generated proxy/dispatcher flow without remote networking.
- `samples\Opc.Classic.Samples.AotCanary` validates NativeAOT publish behavior.

For packet-integrity rationale, see [DCOM hardening and packet integrity](05-dcom-hardening-pkt-integrity-explainer.md).
