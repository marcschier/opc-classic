# Opc.Classic.Samples.LoopbackDemo

Runs an OPC DA client and an OPC DA server in the same .NET process. No Windows COM runtime, DCOM endpoint, registry entry, or external OPC server is required.

## What it demonstrates

The sample wires the normal Opc.Classic layers together with an in-memory transport:

```text
DA client proxies
  -> ICallChannel
  -> InMemoryCallChannel
  -> DA server dispatcher
  -> SampleDaServer + in-memory tag store

SampleDaServer callbacks
  -> IOPCDataCallbackClientProxy
  -> InMemoryCallChannel
  -> client callback sink
```

`InMemoryCallChannel` is the test-double equivalent of the DCOM call channel: generated DA proxies still NDR-encode requests, the channel still dispatches by interface IID and opnum, and the dispatcher returns HRESULT + NDR response payloads. This makes the full client-to-server shape visible without needing a network listener.

The demo keeps COM-pointer and multi-output calls in the same architectural path with loopback shims for `AddGroup`, `AddItems`, `Read`, and browse enumeration, while using generated proxies for implemented calls such as `GetStatus`, `Write`, `Refresh2`, `SetEnable`, and `IOPCDataCallback::OnDataChange`.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.LoopbackDemo\Opc.Classic.Samples.LoopbackDemo.csproj
```

Expected output shows:

- server status from `SampleDaServer`;
- browsed tag names;
- group and item handles;
- initial reads;
- written values;
- `OnDataChange` notifications streamed for about five seconds;
- clean callback unadvise and group removal.

## Source files

- `Program.cs` — host setup.
- `LoopbackDemoService.cs` — orchestrates the client/server demo.
- `LoopbackDaClient.cs` / `LoopbackDaRuntime.cs` — loopback client and runtime plumbing.
- `SampleDaServer.cs` / `LoopbackTagStore.cs` — in-process DA server and tag data.
- `LoopbackModels.cs` / `LoopbackNdr.cs` — DTOs and NDR helpers used by the demo.

For the compose-orchestrated multi-container sample flow, see `samples\README.docker.md`.
