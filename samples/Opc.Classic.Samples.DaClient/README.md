# Opc.Classic.Samples.DaClient

OPC DA console client sample. With no environment variables it runs in-process through `InMemoryCallChannel`; when both `OPC_CLASSIC_SERVER_HOST` and `OPC_CLASSIC_SERVER_PORT` are set it connects over TCP with `DcomCallChannelFactory.ConnectTcpAsync(..., NoOpAuthContext.Instance)`.

## Run in-process

```powershell
dotnet run --project samples\Opc.Classic.Samples.DaClient\Opc.Classic.Samples.DaClient.csproj
```

## Run against a TCP sample server

```powershell
# Terminal 1
$env:OPC_CLASSIC_SAMPLE_PORT = "51300"
dotnet run --project samples\Opc.Classic.Samples.DaServer\Opc.Classic.Samples.DaServer.csproj

# Terminal 2
$env:OPC_CLASSIC_SERVER_HOST = "127.0.0.1"
$env:OPC_CLASSIC_SERVER_PORT = "51300"
dotnet run --project samples\Opc.Classic.Samples.DaClient\Opc.Classic.Samples.DaClient.csproj
```

## Demonstrates

- generated DA client proxies over `ICallChannel`;
- in-process `LoopbackDaServer` + `InMemoryCallChannel` for local demos and tests;
- TCP `DcomCallChannel` for container or separately hosted sample servers;
- browsing a DA tag tree;
- creating a group/subscription and adding items;
- synchronous reads and refresh-driven data-change callbacks;
- `DcomDaSubscription` for the TCP path's item-management and sync-I/O calls;
- clean group removal/disconnect.

## Source files

- `Program.cs` — host setup, in-process/TCP selection, demo worker, loopback server and subscription.
- `DcomDaSubscription.cs` — DA subscription adapter used when the client connects over TCP.

For the compose-orchestrated container demo, see `samples\README.docker.md`.
