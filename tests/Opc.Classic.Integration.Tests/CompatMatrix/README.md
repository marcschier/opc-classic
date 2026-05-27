# Compatibility matrix (Phase 14D)

The cross-platform DCOM story is validated via a four-cell matrix:

|                        | Server side                |                               |
|------------------------|----------------------------|-------------------------------|
| **Client side**        | net10 managed              | Windows COM (legacy)          |
| **net10 managed**      | Phase 13 loopback + managed TCP transport | Phase 14B + 14C |
| **Windows COM**        | **Phase 14D-B**            | Out of scope (legacy native)  |

Phase 14D's specific contribution is the **bottom-left cell**: **Windows COM clients consuming net10 managed servers**. Tests under this folder also include managed stand-ins that exercise the same server/listener plumbing before a native client process is available.

## Current test groups

- `ManagedClientOverTransportTests` — real managed client -> managed `OpcDaServerHost` over `TcpServerEndpoint` / `DcomCallChannel`. These are not scaffold-only; they bind a loopback TCP listener and round-trip generated `IOPCServer` calls through the transport.
- `OutboundCallbackOverTransportTests` — real listener/proxy proof for server-to-client `IOPCDataCallback` calls over the same transport model.
- `Net10ServerToNativeClientTests` — native-client-launch readiness checks for `COM\Sample Client\Da\Simple Client\OpcDaSimpleClient.vcxproj` / `OpcDaSimpleClient.exe`. These soft-skip when the native executable or Windows prerequisites are missing.
- `CompatMatrixSummaryTests` — structural guard that the four matrix cells remain represented.

## Loopback equivalent

`Category=CompatMatrix.Loopback` tests use in-process or loopback TCP infrastructure so they run without a registered native COM client. The older in-memory route uses `InMemoryCallChannel` into `OpcDaServerDispatcher` with `StubDaServer`; the transport route uses `OpcDaServerHost` + `TcpServerEndpoint` + `DcomCallChannel`.

## Native Windows COM client path

The Windows COM client -> net10 server process-launch tests still require a native executable, class registration, OBJREF/listener plumbing, and process-level coordination. Those tests assert the managed server/proxy/category plumbing and soft-skip when prerequisites are missing rather than reporting placeholder success.

## Status

The folder now has real loopback TCP coverage for managed client -> managed server, plus native-client readiness scaffolds. Full Phase 14D-B end-to-end coverage still needs the remaining release-blocker work tracked in `docs\release-blockers.md`, especially the native-client activation/OBJREF handoff and final CTT/native-client orchestration.

The matrix is the **gate for the 1.0 release**. Until all four scenarios show GREEN, the project is not ready for general availability.
