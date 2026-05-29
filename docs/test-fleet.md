# Docker test fleet — adopter cookbook

How to use the `docker/` fleet for end-to-end DCOM testing of the managed
implementation. For the architectural overview see [`docker/README.md`](../docker/README.md).
The fleet contains four Windows-container targets from `docker/docker-compose.test.yml`:
`c-server`, `managed-server`, `ctt`, and `c-client`.

## Common workflows

### 1. Smoke the managed CttServer against the CTT

```pwsh
# From the repo root, on a Windows host with Docker Desktop in Windows mode:
docker network create --driver l2bridge --subnet 10.0.1.0/24 --gateway 10.0.1.1 opc-test-net
docker compose --file docker\docker-compose.test.yml --profile interactive build
docker\run-matrix.ps1 -OnlyManaged
```

Result: `docker/results/ctt-managed.xml` — open in a text viewer or the CTT
report viewer.

### 2. Drive the managed server from a native C client

The `opc-c-client` image builds the hand-rolled DA client MVP from
`docker/opc-c-client/build/opc-test.cpp` and can target the managed server:

```pwsh
docker compose --file docker\docker-compose.test.yml up -d managed-server
docker compose --file docker\docker-compose.test.yml run --rm c-client `
    -ProgId Opc.Classic.DaSample.1 `
    -TargetHost opc-classic-managed
```

### 3. Smoke the native C server/client MVPs

The `opc-c-server` image builds the hand-rolled DA server MVP from
`docker/opc-c-server/build/opc-sample-server.cpp`; the `opc-c-client` image can
be pointed at it on the same `opc-test-net` l2bridge network.

```pwsh
docker compose --file docker\docker-compose.test.yml up -d c-server
docker compose --file docker\docker-compose.test.yml run --rm c-client `
    -ProgId Opc.SampleServer.1 `
    -TargetHost opc-classic-c-server
```

## Debugging

### "Access denied" from the CTT or c-client against the managed server

Symptoms: `OpcCtt.exe` reports `0x80070005 (E_ACCESSDENIED)`; the c-client
fails its `CoCreateInstanceEx` call.

Causes / fixes:

1. **Registry view mismatch**: the CTT is 32-bit on a 64-bit Windows host,
   so it reads from `HKLM\Software\Wow6432Node\Classes\CLSID`. Confirm the
   managed server was registered with `--registry-view=both` (the default)
   so both views see the CLSID.
2. **OPCEnum not running**: `docker exec opc-classic-managed Get-Service OpcEnum`
   should show `Running`. If not, restart the container.
3. **Network isolation**: the containers must be on the SAME `l2bridge`
   network. NAT networking will break OXID resolution.
4. **DCOM ACLs not applied**: `docker exec opc-classic-managed reg query
   "HKLM\SOFTWARE\Microsoft\Ole" /v EnableDCOM` should print `Y`. If not,
   the `dcom-test-acls.reg` import failed during build.

### CTT hangs / times out

The PowerShell shim has a `-TimeoutSeconds 1800` default. Override with
`-TimeoutSeconds 3600` for slow conformance suites. If the CTT consistently
hangs at startup, dump the help output to verify the CLI flags:

```pwsh
docker run --rm opc-classic/ctt -Help
```

### Capturing DCOM wire traffic

The containers don't include Wireshark. Capture on the host using the
container's named network adapter:

```pwsh
# Find the network's adapter
Get-NetAdapter -Name 'vEthernet (opc-test-net)' | Select Name,InterfaceDescription
# Start a packet capture (PowerShell 7+)
pktmon start --comp 'vEthernet (opc-test-net)' --capture --pkt-size 0
# ... run your test ...
pktmon stop
pktmon etl2pcap PktMon.etl
# Open PktMon.pcap in Wireshark
```

### Inspecting the registered CLSIDs in a container

```pwsh
docker exec opc-classic-managed reg query "HKLM\Software\Classes\CLSID\{8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A}" /s
```

### Inspecting RPC endpoints

```pwsh
docker exec opc-classic-managed netstat -ano | findstr LISTEN
# Should show 0.0.0.0:135 (RPC EPMapper) and a port in 49152-49200
```

## CI integration

The rc.10 repository baseline outside the Windows-container gate is **0 build warnings / 0 build errors** and **2113 passed / 12 skipped / 0 failed** across 23 .NET test projects.

`.github/workflows/docker-test-fleet.yml` runs the matrix monthly on
`windows-2022` and can also be started manually with `workflow_dispatch`. Inspect
runs via:

```pwsh
gh run list --workflow=docker-test-fleet.yml
gh run view <run-id> --log
gh run download <run-id> --name docker-test-fleet-results
```

## Known limitations

- **Cannot run on Linux Docker**: Windows containers require a Windows
  kernel host. Use GitHub Actions' `windows-2022` runner for CI.
- **CTT MSI redistribution**: the vendored CTT installers are OPC Foundation
  member-only. The `opc-classic/ctt` image bakes them in; don't publish to
  a public registry without OPC Foundation approval.
- **Build artifacts are not cached**: each CI run rebuilds the multi-GB images
  from scratch. Mitigation paths include GHCR layer caching, but this is
  not yet wired.
- **Validation is environment-blocked**: the managed CTT smoke and native C server/client interop paths have source, project files, and Dockerfiles wired, but compiling/running them still requires a Windows Docker host.
