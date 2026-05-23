# Opc.Classic Sample HDA Client

Self-contained console client for the managed OPC HDA API. It runs an in-process `SampleHdaServer` behind an `InMemoryCallChannel`, so no external OPC server or Windows COM registration is required.

## Run

```bash
dotnet run --project samples/Opc.Classic.Samples.HdaClient
```

## Demonstrates

- Microsoft.Extensions.Hosting, DI, and ILogger setup.
- Connecting to an in-process `IOpcHdaServer` through `InMemoryCallChannel`.
- Reading HDA server status and browsing the sample item space.
- `IOPCHDA_SyncRead` raw reads and processed reads using the `Average` aggregate.
- `IOPCHDA_SyncAnnotations.QueryCapabilities`; annotation read is shown through the managed HDA DTO surface because the generated SyncAnnotations proxy currently exposes capability query only.
- Starting an `IOPCHDA_AsyncRead` request and cancelling it with a `CancellationToken`.
- Releasing item handles and disconnecting cleanly.
