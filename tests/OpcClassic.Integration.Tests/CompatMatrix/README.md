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

## Status

Scaffold-only today. The full Phase 4 stack must land first:
  - Phase 4A LocalCoClass modernization
  - Phase 4C RemoteSCMActivatorServer (real OBJREF encoding)
  - Phase 4F hosted runtime with bound ncacn_ip_tcp listener
  - Phase 6F per-method DA server dispatch

The matrix is the **gate for the 1.0 release** (Phase 16E). Until all
four scenarios show GREEN, the project is not ready for general
availability.
