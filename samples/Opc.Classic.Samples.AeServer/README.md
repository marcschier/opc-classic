# Opc.Classic Sample AE Server

Managed OPC AE sample server mirroring the native `COM\Sample Server\Ae` simulation loop with a small synthetic event tree.

## Event tree

- `Server.Heartbeat` - simple "Heartbeat" event every 5 seconds, severity `Info` (`100`).
- `Server.Errors` - periodic simple simulated error event every 15 seconds, severity `700`.
- `Demo.Conditions` - tracking condition `DemoCondition`, toggling `Active` / `Inactive` every 10 seconds.

The current emitter logs events only. Phase 7F-followup wires these events into `IOpcAeEventPublisher` and AE subscriptions.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.AeServer
```

The server registers as `Opc.Classic.Samples.AeServer.1` (CLSID `C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F`) and listens on `127.0.0.1:0`.

## Status

Scaffold-grade. `SampleAeServer` implements the initial `IOpcAeServer.GetStatusAsync` surface while subscription management, area browsing, condition enablement, acknowledgements, and event fan-out remain Phase 7F-followup work.
