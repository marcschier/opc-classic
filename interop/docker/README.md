# opc-classic Docker test fleet

Windows containers for end-to-end DCOM interop testing of the managed
implementation against:

- A **native (C-built) OPC DA smoke server** — hand-rolled in `interop\docker\opc-c-server\build\` with OPC Foundation headers.
- A **native (C-built) OPC DA smoke client** — hand-rolled in `interop\docker\opc-c-client\build\` with OPC Foundation headers.
- The **OPC Foundation TestServer/TestClient x64 pair** — built from the
  vendored `external` CMake tree and gated behind
  `interop\docker\run-matrix.ps1 -IncludeTestServer`.

The managed `Opc.Classic.Samples.CttServer` runs beside the native C and OPC
Foundation reference containers so cross-implementation client/server pairs can
be tested on a single Windows host. The rc.10 baseline is 0 build warnings/errors and
2113 passed / 12 skipped / 0 failed across 23 .NET test projects.

## Status

| Container | Status |
| --- | --- |
| `opc-classic/managed` | ✅ Ready — publishes `Opc.Classic.Samples.CttServer` and registers `Opc.Classic.DaSample.1` |
| `opc-classic/c-server` | ✅ Ready — builds the hand-rolled native DA smoke server (`opc_exe.exe`) from `opc-sample-server.cpp` |
| `opc-classic/c-client` | ✅ Ready — builds the hand-rolled native DA smoke client (`opc-test.exe`) from `opc-test.cpp` |
| `opc-classic/testserver` | 🧱 Scaffolded — builds OPC Foundation `OpcTestServer_x64.exe` from `external`; validate on a Windows Docker host |
| `opc-classic/testclient` | 🧱 Scaffolded — copies `OpcTestClient_x64.exe` from the testserver image; validate on a Windows Docker host |
| `docker-compose.test.yml` | ✅ Ready — orchestrates the five images on `opc-test-net` |
| `.github/workflows/docker-test-fleet.yml` | ✅ Ready — CI entry point for the fleet |

The native C server/client images now build real MVP binaries. Their entrypoint scripts still retain missing-binary checks so failed local builds are easy to debug with `docker exec`.

## Sample roster

The repository carries 10 sample apps: DaServer, DaClient, AeServer, AeClient, HdaServer, HdaClient, LoopbackDemo, CttServer, AotCanary, and OpcSecurityServer. The Docker fleet uses CttServer for managed DA coverage; the Linux sample Compose files cover the DA/AE/HDA pairs plus LoopbackDemo, while OpcSecurityServer runs from source on port 51304.

## Managed-server state

The `opc-classic/managed` container runs `Opc.Classic.Samples.CttServer` with the current managed DCOM server stack:

| Piece | What ships |
| --- | --- |
| Cross-platform RPC listener (`OpcServerListener` + `TcpServerEndpoint` + `RpcServerConnectionProcessor`) | Real TCP accept + DCE/RPC bind/alter/request/shutdown PDU handling |
| DA/AE/HDA hosts wired to the listener | Managed servers physically accept inbound DCOM calls |
| IPID per-object dispatch routing (`OpcObjectRegistry`) | `RequestCoPdu.Object` UUIDs resolve to per-group dispatchers |
| `CttDaServer` group tracking | `AddGroup` creates an `OpcDaGroup`, returns a real IPID, and unregisters it on `RemoveGroup` |
| Group dispatchers | `IOPCGroupStateMgt(2)`, `IOPCItemMgt`, `IOPCSyncIO(2)`, `IOPCAsyncIO2/3`, `IConnectionPoint`, deadband, and sampling dispatch through `OpcDaGroup` |
| Windows CCW factory + SCM wireup | `IClassFactory::CreateInstance` returns a NativeAOT-friendly `IOPCServer` CCW backed by `CttDaServer`; AE array CCWs and HDA Update/Playback/Annotations CCWs are covered by the rc.10 Windows tests |
| Outbound callback infrastructure | Listener + generated `IOPCDataCallback` proxy/dispatcher paths are available for callback composition |

## Prerequisites

- **Windows 10/11 Pro/Enterprise** or **Windows Server 2022+** host (Linux
  containers can't host DCOM).
- **Docker Desktop on Windows** with the daemon in **"Switch to Windows
  containers"** mode. Linux mode CAN NOT run these images.
- **Hyper-V isolation** (default on Win11) or **process isolation** (Windows
  Server hosts) — required for `windowsservercore:ltsc2022` to start.
- **~10 GB free disk** for the layered images during build (build stage uses
  the ~10 GB `dotnet/framework/sdk` image; the TestServer cold build also
  installs VS Build Tools in a Server Core layer).
- **One `l2bridge` Docker network** named `opc-test-net` (created on first
  `interop/docker/run-matrix.ps1` invocation).
- **Optional `external` vendor tree** for
  `opc-classic/testserver` and `opc-classic/testclient`. OPERATOR: if the tree
  is omitted locally, restore `interop\build\x64\Release` from CI or
  build the TestServer image on a machine with the vendored sources.

## Quick start

### Build everything

```pwsh
docker compose --file interop/docker/docker-compose.test.yml --profile interactive build
```

This includes `opc-classic/testserver` and `opc-classic/testclient`, so it
requires `external`. To keep the historical three-image smoke path,
use `interop\docker\run-matrix.ps1` without `-IncludeTestServer`.

### Bring the server fleet up

```pwsh
interop/docker/run-matrix.ps1
# Builds + starts the c-server + managed-server containers (and optionally
# testserver with -IncludeTestServer). Servers stay up so an interactive
# client can attach via the c-client / testclient profiles.
```

### Open an interactive c-client shell

```pwsh
interop/docker/run-interactive.ps1
# Drops you into a PowerShell session inside the c-client container; the
# c-server and managed-server containers are running and reachable.
```

## Containers

### `opc-classic/managed`

The managed `Opc.Classic.Samples.CttServer`, registered under `HKLM\Software\Classes\CLSID\{...}` on container startup via `--register --registry-hive=hklm`. When SCM launches the EXE with `-Embedding`, `ComClassObjectRegistrar` delegates `IClassFactory::CreateInstance` to `OpcDaServerCcw.Create`, so supported activations receive a real `IOPCServer` CCW backed by `CttDaServer`.

### `opc-classic/c-server`

Builds and runs the hand-rolled native OPC DA smoke server (`opc_exe.exe`) from `interop\docker\opc-c-server\build\opc-sample-server.cpp`. It self-registers `OPC.SampleServer.1`, exposes `Sin`, `Square`, and `Random`, and implements the DA root/group interfaces.

### `opc-classic/c-client`

Builds and runs the hand-rolled native OPC DA smoke client (`opc-test.exe`) from `interop\docker\opc-c-client\build\opc-test.cpp`. It resolves a ProgID on a target host, calls `AddGroup`, `AddItems`, `Read`, then removes the group.

### `opc-classic/testserver`

Builds the OPC Foundation CoreComponents CMake targets for
`OpcTestServer_x64.exe`, `OpcTestClient_x64.exe`, `OpcCategoryManager.exe`, and
the eight proxy/stub DLLs. `server-init.ps1` invokes
`interop\tools\register-testserver.ps1`, starts `OpcTestServer_x64.exe`, and unregisters
on shutdown. See `interop\docker\opc-testserver\README.md`.

### `opc-classic/testclient`

Copies `OpcTestClient_x64.exe` and the proxy/stub DLLs from the
`opc-classic/testserver` image to avoid a second CoreComponents build. Its
entrypoint redirects local OpcEnum activation to `opc-classic-testserver` via
DCOM `RemoteServerName`, then verifies `OpcTestServer_x64.1` appears in the
enumeration output. See `interop\docker\opc-testclient\README.md`.

## Networking

DCOM in containers **requires** an `l2bridge` (or `transparent`) network.
The default Docker NAT will break the OXID resolver: the server announces
its internal container IP in the `DUALSTRINGARRAY` returned from activation,
and the client cannot route to that IP through NAT.

`interop/docker/run-matrix.ps1` creates the network idempotently with:

```pwsh
docker network create --driver l2bridge `
    --subnet 10.0.1.0/24 --gateway 10.0.1.1 opc-test-net
```

Containers get fixed IPs:

| Container | Static IP |
| --- | --- |
| `opc-classic-c-server` | 10.0.1.10 |
| `opc-classic-managed` | 10.0.1.11 |
| `opc-classic-testserver` | 10.0.1.12 |
| `opc-classic-c-client` | 10.0.1.21 |
| `opc-classic-testclient` | 10.0.1.22 |

DCOM dynamic ports are pinned to `49152-49200` (via
`HKLM\SOFTWARE\Microsoft\Rpc\Internet\Ports` in `dcom-test-acls.reg`) so
the `EXPOSE` directive is bounded.

## Security caveats

These containers are **for sandboxed testing only**. They:

- Disable DCOM authentication (`LegacyAuthenticationLevel = RPC_C_AUTHN_LEVEL_NONE`).
- Grant `Everyone` + `ANONYMOUS LOGON` full DCOM access via `DefaultAccessPermission`
  and `DefaultLaunchPermission`.
- Pin a wide dynamic port range.

Per the [DcomContainerSample reference](https://github.com/wazzzaatosh/DcomContainerSample),
making DCOM work in containers is fundamentally about loosening the security
posture. The relaxed ACLs in `dcom-test-acls.reg` are appropriate for a
disposable test rig but **must never be applied to a production host**.

## Known gaps

- **TestServer/TestClient scaffold**: CoreComponents builds and DCOM
  `RemoteServerName` redirection cannot be validated without a Windows Docker
  host. OPERATOR: run `interop\docker\run-matrix.ps1 -IncludeTestServer` and tune the
  VS Build Tools component IDs, OpcEnum availability, or port pinning if needed.
- **DcomContainerSample's open issue**: even the simplest reference example
  in [DcomContainerSample](https://github.com/wazzzaatosh/DcomContainerSample)
  has unresolved cross-container `access denied` errors. If the fleet hits the
  same condition, mitigation paths are documented in
  `docs\architecture\dcom-container-networking.md`.

## Related documentation

- `docs\test-fleet.md` — adopter cookbook (debugging, capture, common errors)
- `docs\architecture\dcom-container-networking.md` — l2bridge / transparent / NAT trade-offs
