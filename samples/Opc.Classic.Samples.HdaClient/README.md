# Opc.Classic Sample HDA Client

Console client for the managed OPC HDA API. With no environment variables it runs an in-process `SampleHdaServer` behind `InMemoryCallChannel`; when both `OPC_CLASSIC_SERVER_HOST` and `OPC_CLASSIC_SERVER_PORT` are set it connects over TCP with `DcomCallChannelFactory.ConnectTcpAsync(..., NoOpAuthContext.Instance)`.

## Run in-process

```powershell
dotnet run --project samples\Opc.Classic.Samples.HdaClient
```

## Run against a TCP sample server

```powershell
# Terminal 1
$env:OPC_CLASSIC_SAMPLE_PORT = "51302"
dotnet run --project samples\Opc.Classic.Samples.HdaServer

# Terminal 2
$env:OPC_CLASSIC_SERVER_HOST = "127.0.0.1"
$env:OPC_CLASSIC_SERVER_PORT = "51302"
dotnet run --project samples\Opc.Classic.Samples.HdaClient
```

## Demonstrates

- Microsoft.Extensions.Hosting, DI, and ILogger setup.
- Connecting to an in-process `IOpcHdaServer` through `InMemoryCallChannel`, or to a hosted server through TCP `DcomCallChannel`.
- Reading HDA server status and browsing the sample item space (the TCP path uses a local browse fallback until generated HDA browse coverage is complete).
- `IOPCHDA_SyncRead` raw reads and processed reads using the `Average` aggregate.
- `IOPCHDA_SyncAnnotations.QueryCapabilities`; annotation read is shown through the managed HDA DTO surface because the generated SyncAnnotations proxy currently exposes capability query only.
- Starting an `IOPCHDA_AsyncRead` request and cancelling it with a `CancellationToken`.
- Releasing item handles and disconnecting cleanly.

## Source files

- `Program.cs` — host setup and in-process/TCP selection.
- `HdaClientDemo.cs` — status, browse, handle, raw/processed read, annotation, and cancellation flow.
- `LoopbackHdaClient.cs` — HDA client facade over generated proxies.
- `LoopbackHdaCallRouter.cs` — in-process NDR router used by the default path.

For the compose-orchestrated container demo, see `samples\README.docker.md`.
