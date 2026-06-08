# `opc-classic/ctt` — OPC Compliance Test Tool container

Bakes the six vendored OPC CTT MSIs (`external/private/ctt/`, ~13 MB total) into a
Windows Server Core 2022 image. The container's `ENTRYPOINT` is a small
PowerShell shim (`run-ctt.ps1`) that invokes `OpcCtt.exe` against a target
ProgID + remote host and emits an XML conformance report.

## Build

```pwsh
cd <repo-root>
docker build --file external/docker/opc-ctt/Dockerfile --tag opc-classic/ctt .
```

The build copies `external/private/ctt/` into the image (~13 MB MSIs), runs six
`msiexec /quiet /norestart` installs in spec-mandated order (Common Modules
first, then DA 2.05a → DA 3.0 → AE → HDA → XML-DA), imports
[`dcom-test-acls.reg`](dcom-test-acls.reg) to relax DCOM ACLs for anonymous
cross-container access, and applies the OPCEnum-auto-start tweak.

## Run

### Against the managed sample server in a peer container

```pwsh
# (Bring the fleet up first; see external/docker/docker-compose.test.yml)
docker run --rm --network opc-test-net opc-classic/ctt `
    -ProgId Opc.Classic.DaSample.1 `
    -TargetHost opc-classic-managed `
    -OutputPath C:/results/managed-run.xml
```

### Against the native (C-built) sample server

```pwsh
docker run --rm --network opc-test-net opc-classic/ctt `
    -ProgId OPC.SampleServer.1 `
    -TargetHost opc-classic-c-server `
    -OutputPath C:/results/native-run.xml
```

### Interactive PowerShell shell (debug / ad-hoc)

```pwsh
docker run --rm -it --entrypoint powershell --network opc-test-net opc-classic/ctt
# inside the container:
#   Get-Service OpcEnum
#   & "${env:ProgramFiles(x86)}\OPC Foundation\OPC Compliance Test Tool\OpcCtt.exe" /?
```

### Dump the CTT CLI help

```pwsh
docker run --rm opc-classic/ctt -Help
```

This prints `OpcCtt.exe /?` to stdout — useful while we verify the canonical
headless invocation flags for CTT v2.0.15 (`/AUTO /Output: /ServerProgId: /TargetHost:`
in the shim is currently the best-guess invocation).

## Configuration knobs

| Parameter | Default | Notes |
| --- | --- | --- |
| `-ProgId` | _required_ | e.g. `Opc.Classic.DaSample.1` |
| `-TargetHost` | (local) | Hostname or IP of the container hosting the server |
| `-ScriptPath` | (none) | Path to a CTT script bundle (XML) |
| `-OutputPath` | `C:/results/ctt-results.xml` | Volume-mount this for host-side access |
| `-TimeoutSeconds` | `1800` | Kill OpcCtt.exe if it hangs |
| `-Help` | `false` | Dump CLI help and exit |

## Networking

The image expects to be attached to a Docker network that bridges to the
peer containers running the SUT (system under test). Recommended:

```pwsh
docker network create --driver l2bridge --subnet 10.0.1.0/24 opc-test-net
```

NAT networking (Docker default) WILL NOT WORK end-to-end because DCOM's
OXID resolver announces the server's internal IP to the client, which the
client cannot route back to without `l2bridge` (or `transparent`).

## Open issues

1. **`OpcCtt.exe /?` flags unverified**: the `/AUTO /Output:` syntax in the
   shim is a best-guess. First real CI run dumps the actual help text as
   `opcctt-help.txt`; updating the shim is a known follow-up.
2. **CTT MSI EULA**: if any of the six MSIs fail with EULA-related errors
   under `/quiet`, the build can be amended with `ACCEPT_EULA=1` or by
   transforming the MSI via WiX `dark`.
3. **CTT licensing**: the vendored MSIs are intended for OPC Foundation
   member-only use. The container image is **for internal testing only**;
   do not publish to a public registry without OPC Foundation sign-off.

## Related files

- `external/docker/opc-ctt/Dockerfile` — image definition
- `external/docker/opc-ctt/dcom-test-acls.reg` — relaxed DCOM ACL imports
- `external/docker/opc-ctt/run-ctt.ps1` — ENTRYPOINT shim
- `external/docker/docker-compose.test.yml` — orchestrates the full test fleet
- `external/docker/README.md` — top-level fleet documentation
- `docs/test-fleet.md` — adopter cookbook
- `docs/architecture/dcom-container-networking.md` — l2bridge vs transparent
