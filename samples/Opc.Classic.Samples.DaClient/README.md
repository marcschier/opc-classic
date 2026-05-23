# Opc.Classic.Samples.DaClient

Self-contained OPC DA client sample. It uses `InMemoryCallChannel` wired to an in-process `IOpcDaServer` so no external OPC/DCOM server is required.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.DaClient\Opc.Classic.Samples.DaClient.csproj
```

## Demonstrates

- generated `IOPCServerClientProxy` over an `ICallChannel` (replace with `DcomCallChannelFactory.ConnectAsync(...)` for a real DCOM endpoint);
- browsing a DA tag tree;
- creating a group/subscription and adding items;
- synchronous reads;
- async data-change callbacks;
- clean group removal/disconnect.
