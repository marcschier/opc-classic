# Opc.Classic Sample AE Server

Managed OPC AE sample server mirroring the native `external\samples\SampleServer\Ae` simulation loop with a small synthetic event tree.

## Event tree

- `Server.Heartbeat` - simple "Heartbeat" event every 5 seconds, severity `Info` (`100`).
- `Server.Errors` - periodic simple simulated error event every 15 seconds, severity `700`.
- `Demo.Conditions` - tracking condition `DemoCondition`, toggling `Active` / `Inactive` every 10 seconds.

The current emitter logs events only. Follow-up server work wires these events into remote AE subscriptions.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.AeServer
```

The server registers as `Opc.Classic.Samples.AeServer.1` (CLSID `C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F`) and listens on `0.0.0.0:51301` by default.

Set `OPC_CLASSIC_SAMPLE_PORT` to change the default port or `OPC_CLASSIC_LISTEN_ADDRESS` to override the full bind address.

## Status

Scaffold-grade. `SampleAeServer` implements `GetStatusAsync` and `QueryAvailableFiltersAsync`; remote subscription management, area browsing, condition enablement, acknowledgements, and event fan-out remain follow-up work. The AE client sample's default in-process path uses its own in-process area/condition model to demonstrate those flows today.

## Source files

- `Program.cs` — host setup, registry options, and listen-address selection.
- `SampleAeServer.cs` — managed AE server status/filter surface.
- `EventEmitter.cs` — logged synthetic event loop.

For the compose-orchestrated container demo, see `samples\README.docker.md`.
