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
- `DcomCallChannelFactory.ConnectTcpAsync` and `TcpClientTransport` are in `Opc.Classic.Dcom.Transport`.

## Connection policy

```csharp
using System.Net;
using Opc.Classic;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;

var url = OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", password, "CORP"),
    protectionLevel: OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));

IAuthContext authContext = NtlmAuthentication.CreateAuthContext(connectData);
int endpointPort = 51300; // sample container default; use the resolved or constrained Matrikon RPC endpoint in production.
await using DcomCallChannel channel = await DcomCallChannelFactory.ConnectTcpAsync(
    url.Host,
    endpointPort,
    authContext,
    cancellationToken).ConfigureAwait(false);
```

`endpointPort` is the TCP endpoint your deployment resolved or constrained for the DCOM object. In the repository sample containers it is `51300` for DA; with a Windows Matrikon host, coordinate endpoint mapper (`135/tcp`) and dynamic RPC port range with the Windows administrators.

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

- Opc.Classic.Samples sample shows the managed DA client shape.
- Opc.Classic.Samples sample validates generated proxy/dispatcher flow without remote networking.
- Opc.Classic.Samples sample validates NativeAOT publish behavior.
- [../../samples/README.docker.md](../../samples/README.docker.md) documents the sample `OPC_CLASSIC_SERVER_HOST`, `OPC_CLASSIC_SERVER_PORT`, and server port defaults.

For packet-integrity rationale, see [DCOM hardening and packet integrity](05-dcom-hardening-pkt-integrity-explainer.md).
