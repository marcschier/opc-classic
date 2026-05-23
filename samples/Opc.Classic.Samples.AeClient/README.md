# Opc.Classic Sample AE Client

Self-contained console client for the managed OPC AE API. It does not require an external OPC server.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.AeClient
```

## What it demonstrates

- Creates an in-process AE server and reuses `SampleAeServer` from `samples\Opc.Classic.Samples.AeServer` for status/filter calls.
- Connects a generated `IOPCEventServer` proxy through `InMemoryCallChannel` and `OpcAeServerDispatcher`.
- Reads server status, browses the area/source tree, and enables condition monitoring.
- Creates an `IAeSubscription` and iterates its canonical `IAsyncEnumerable<EventNotification>` stream.
- Captures an acknowledgement-required condition event, acknowledges it, disables monitoring, and disconnects cleanly.

The sample uses `Microsoft.Extensions.Hosting`, DI, and `ILogger` to mirror the server samples while keeping the whole client/server exchange in-process.
