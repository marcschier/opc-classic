# Compatibility matrix (Phase 14D)

The cross-platform DCOM story is validated via a four-cell matrix:

|                        | Server side                |                               |
|------------------------|----------------------------|-------------------------------|
| **Client side**        | net10 managed              | Windows COM (legacy)          |
| **net10 managed**      | Phase 13 (loopback)        | Phase 14B + 14C               |
| **Windows COM**        | **Phase 14D-B**            | Out of scope (legacy native)  |

Phase 14D's specific contribution is the **bottom-left cell**:
**Windows COM clients consuming net10 managed servers**. Test scaffolds
under this folder use `COM/Sample Client/Da/Simple Client/OpcDaSimpleClient.vcxproj`
which builds `OpcDaSimpleClient.exe` under `COM/BuildOutput/bin/clients/Win32/`,
as the native client and an in-process `OpcDaServerHost` as the managed server.

## Loopback equivalent

`Category=CompatMatrix.Loopback` tests use a managed client as the stand-in for
the future native client process. They route the generated `IOPCServer` proxy
through `InMemoryCallChannel` into `OpcDaServerDispatcher` with a `StubDaServer`.
This proves the net10 server-side pipeline and assertions work in-process.

## Real Phase 14D-B path

The Windows COM client -> net10 server tests remain scaffold-only for the real
native-client launch because they require a native executable, class
registration, OBJREF/listener plumbing, and process-level coordination. Those
scaffolds now assert the managed server/proxy/category plumbing instead of a
placeholder assertion and soft-skip when prerequisites are missing.

## Status

The folder now has loopback-backed assertions for the net10 managed server
pipeline. Full Phase 14D-B end-to-end coverage still needs:
  - Phase 4A LocalCoClass modernization
  - Phase 4C RemoteSCMActivatorServer (real OBJREF encoding)
  - Phase 4F hosted runtime with bound ncacn_ip_tcp listener
  - Phase 6F per-method DA server dispatch exposed to native clients

The matrix is the **gate for the 1.0 release** (Phase 16E). Until all
four scenarios show GREEN, the project is not ready for general
availability.
