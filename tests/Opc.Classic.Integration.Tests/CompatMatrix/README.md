# Compatibility matrix

The cross-platform DCOM story is validated via a four-cell matrix:

|                        | Server side                |                               |
| ---------------------- | -------------------------- | ----------------------------- |
| **Client side**        | net10 managed              | Windows COM (legacy)          |
| **net10 managed**      | loopback + managed TCP transport | native-COM server consumption |
| **Windows COM**        | **Windows COM client → net10 server** | Out of scope (legacy native)  |

The bottom-left cell — **Windows COM clients consuming net10 managed servers** — is the focus of this folder. Tests here also include managed stand-ins that exercise the same server/listener plumbing before a native client process is available.

## Current test groups

- `ManagedClientOverTransportTests` — real managed client -> managed `OpcDaServerHost` over `TcpServerEndpoint` / `DcomCallChannel`. These are not scaffold-only; they bind a loopback TCP listener and round-trip generated `IOPCServer` calls through the transport.
- `OutboundCallbackOverTransportTests` — real listener/proxy proof for server-to-client `IOPCDataCallback` calls over the same transport model.
- `Net10ServerToNativeClientTests` — native-client-launch readiness checks for `OpcDaSimpleClient.vcxproj` (vendored OPC Foundation SampleClient) / `OpcDaSimpleClient.exe`. These soft-skip when the native executable or Windows prerequisites are missing.
- DA loopback TCP tests in Da tests cover the same object-IPID dispatch path for group objects, item enumerators, callbacks, and `opc-da-browse:N` continuation tokens.
- `CompatMatrixSummaryTests` — structural guard that the four matrix cells remain represented.

## Loopback equivalent

`Category=CompatMatrix.Loopback` tests use in-process or loopback TCP infrastructure so they run without a registered native COM client. The older in-memory route uses `InMemoryCallChannel` into `OpcDaServerDispatcher` with `StubDaServer`; the transport route uses `OpcDaServerHost` + `TcpServerEndpoint` + `DcomCallChannel`.

## Native Windows COM client path

The Windows COM client -> net10 server process-launch tests still require a native executable, class registration, activation handoff, and process-level coordination. Those tests assert the managed server/proxy/category plumbing and soft-skip when prerequisites are missing rather than reporting placeholder success.

## Status

The folder now has real loopback TCP coverage for managed client -> managed server, plus native-client readiness scaffolds. rc.10 also closes the Windows CCW array/update/playback gaps in the AE and HDA hosting test suites. Still need validation of native-client activation handoff and final native-client orchestration.
