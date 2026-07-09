# Opc.Classic Sample AE Client

Console client for the managed OPC AE API. With no environment variables it runs in-process through `InMemoryCallChannel`; when both `OPC_CLASSIC_SERVER_HOST` and `OPC_CLASSIC_SERVER_PORT` are set it connects over TCP with `DcomCallChannelFactory.ConnectTcpAsync(..., NoOpAuthContext.Instance)`.

## Run in-process

```powershell
dotnet run --project samples\Opc.Classic.Samples.AeClient
```

## Run against a TCP sample server

```powershell
# Terminal 1
$env:OPC_CLASSIC_SAMPLE_PORT = "51301"
dotnet run --project samples\Opc.Classic.Samples.AeServer

# Terminal 2
$env:OPC_CLASSIC_SERVER_HOST = "127.0.0.1"
$env:OPC_CLASSIC_SERVER_PORT = "51301"
dotnet run --project samples\Opc.Classic.Samples.AeClient
```

## What it demonstrates

- Creates an in-process AE server and reuses `SampleAeServer` from Opc.Classic.Samples sample for status/filter calls.
- Connects a generated `IOPCEventServer` proxy through `InMemoryCallChannel` + `OpcAeServerDispatcher`, or through a TCP `DcomCallChannel` when the environment variables are set.
- Reads server status, browses the in-process area/source tree, and enables condition monitoring.
- Creates an `IAeSubscription` and iterates its canonical `IAsyncEnumerable<EventNotification>` stream in the in-process path.
- Captures an acknowledgement-required condition event, acknowledges it, disables monitoring, and disconnects cleanly.

The remote TCP path exercises generated AE calls against a hosted server. Event callback delivery over a remote subscription remains minimal until the server-side AE callback work lands.

## Source files

- `Program.cs` — host setup and in-process/TCP selection.
- `AeClientDemo.cs` — status, browse, condition, subscription, and acknowledge flow.
- `LoopbackAeClient.cs` — managed AE client facade over generated proxies.
- `InProcessAeServer.cs` / `InProcessAeSubscription.cs` — in-process area/condition/event model.
- `RemoteAeSubscription.cs` — minimal TCP-path subscription wrapper returned from remote `CreateEventSubscription`.

For the compose-orchestrated container demo, see README.docker sample.
