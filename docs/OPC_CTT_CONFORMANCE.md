# OPC Compliance Test Tool (CTT) conformance

The OPC Foundation's Compliance Test Tool (CTT) is the canonical conformance
suite for OPC Classic servers. This document describes how to validate an
`Opc.Classic`-based server against the CTT, both in CI and on a local Windows
workstation.

For the CI workflow's internal architecture (install order, registry hive
choice, scope-boundary rationale), see `docs/ctt/CI_DESIGN.md`. For the
Windows-container fleet that runs managed/native combinations, see
[`docker/README.md`](../docker/README.md) and [test-fleet.md](test-fleet.md).

## Scope of this gate

| Server under test | `samples/Opc.Classic.Samples.CttServer/` |
|---|---|
| ProgID | `Opc.Classic.DaSample.1` |
| CLSID | `{8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A}` |
| CI workflows | `.github/workflows/opc-ctt.yml`, `.github/workflows/docker-test-fleet.yml` |
| Artifacts | `opc-ctt-results`, `docker-test-fleet-results` |

## Prerequisites

1. **Windows**. The CTT is a 32-bit native COM application; it has no Linux
   or macOS equivalent.
2. **The vendored CTT installers**, already tracked in `External/CTT/` as six
   MSIs (~13 MB total). The CI workflow consumes these directly; local runs
   install them with the same commands documented below.
3. **A published Release build** of `Opc.Classic.Samples.CttServer`.
4. **Administrative privileges** for an HKLM registration (recommended for
   CTT runs). Per-user HKCU registration is supported for developer
   workflows without elevation but only the calling user can then discover
   the server.
5. **Windows Docker host** for the release-gating fleet smoke. Create the
   `opc-test-net` l2bridge network as described in `docker/README.md`.

## CI flow

`.github/workflows/opc-ctt.yml` runs on `windows-2022` and:

1. Installs the six vendored CTT MSIs via `msiexec /i ... /quiet /norestart`,
   Common Modules first, then plugins in spec order
2. Starts the OPCEnum service (the OPC server enumerator)
3. Publishes `Opc.Classic.Samples.CttServer` Release
4. Registers the managed server under HKLM via the `--register` CLI
5. Locates `OpcCtt.exe` and dumps its `/?` help output as a diagnostic
6. Runs CTT against `Opc.Classic.DaSample.1`
7. Unregisters the server
8. Uploads `ctt-results.xml` + the CLI help dump as `opc-ctt-results`

`.github/workflows/docker-test-fleet.yml` builds the Windows-container fleet and
runs the managed CTT smoke through `docker/run-matrix.ps1 -OnlyManaged`. The CTT
release gate remains open until that report is green on a Windows Docker host;
see [release-blockers.md](release-blockers.md).

## Local-run cookbook

### One-time setup

Install the six CTT MSIs from `External/CTT/` (run elevated PowerShell):

```pwsh
$installers = @(
  'External/CTT/Test Tool v2.0.15/OPC CTT Common Modules 2.0.15.msi',
  'External/CTT/Data Access 2.05a Plugin v2.0.22/OPC CTT DataAccess2 Modules 2.0.22.msi',
  'External/CTT/Data Access 3.0 Plugin v1.0.18/OPC CTT DataAccess3 Modules 1.0.18.msi',
  'External/CTT/Alarm and Events Plugin v1.0.14/OPC CTT AlarmEvents Modules 1.0.14.msi',
  'External/CTT/Historical Data Plugin v1.0.8/OPC CTT Historical DA Modules 1.0.8.msi',
  'External/CTT/XML-DA Plugin v1.0.8/OPC CTT XMLDA Modules 1.0.8.msi'
)
foreach ($msi in $installers) {
  Start-Process msiexec -ArgumentList '/i', "`"$msi`"", '/quiet', '/norestart' -Wait
}
```

### Per-run cycle

```pwsh
# Build the managed sample server
dotnet publish samples/Opc.Classic.Samples.CttServer -c Release

$exe = 'samples/Opc.Classic.Samples.CttServer/bin/Release/net10.0/publish/Opc.Classic.Samples.CttServer.exe'

# Register the server. For a local dev box without admin rights, swap
# to --registry-hive=hkcu.
& $exe --register

# Launch the CTT and target Opc.Classic.DaSample.1 (interactive GUI; or pass
# the headless flags documented by the OPC Foundation for v2.0.15)
& "${env:ProgramFiles(x86)}\OPC Foundation\OPC Compliance Test Tool\OpcCtt.exe"

# Clean up
& $exe --unregister
```

### Trade-offs: HKLM vs HKCU

| Hive | Privilege | Visible to | Recommended use |
|---|---|---|---|
| `HKLM` (default) | Administrator | All users, including services | CI, production deployments |
| `HKCU` (`--registry-hive=hkcu`) | None | The calling user only | Local dev, when admin is unavailable |

## Current state

`Opc.Classic.Samples.CttServer` now has the release-scope Windows CCW and managed
DCOM paths wired:

- ✅ OPCEnum/direct CLSID discovery and SCM launch/registration plumbing
- ✅ `IClassFactory` + `IOPCServer` raw-vtable CCW (`OpcDaServerCcw`) with real
  per-method bodies for the release scope
- ✅ `OpcDaGroupCcw` multi-tearoff coverage for group state, item management,
  sync/async I/O, and connection points
- ✅ real `OPCITEMDEF` / `OPCITEMRESULT`, `VARIANT`, `SAFEARRAY`, `BSTR`,
  `FILETIME`, and callback marshaling where needed for the DA path
- ✅ DA address-space browse/properties/default deadband/sampling support through
  `IOpcAddressSpace` and default implementations

The remaining blocker is not a known `E_NOINTERFACE` implementation stub; it is
Windows Docker / CTT execution and triage. The final tag requires a green CTT
smoke report archived with the release artifacts.

## Release gates

| Tag | Gate |
|---|---|
| `1.0.0-rc.7` | Build 0/0 + all 17 .NET test projects green + Docker/native C/server-client wiring present |
| `1.0.0` (FINAL) | CTT smoke green for `Opc.Classic.DaSample.1` on the Windows Docker fleet, plus the other gates in `docs/release-blockers.md` |

The XML report is kept alongside the release artifacts for auditability.

## Related documentation

- `docs/ctt/CI_DESIGN.md` — CI workflow internals
- `docker/README.md` — Windows Docker test fleet overview
- `docs/test-fleet.md` — adopter cookbook for the Docker fleet
- `docs/release-blockers.md` — remaining 1.0.0 FINAL gates
- `samples/Opc.Classic.Samples.CttServer/README.md` — sample-level CLI documentation
- `src/Opc.Classic.Hosting/Windows/README.md` — Windows COM registration internals
- `External/CTT/readme.txt` — original OPC Foundation CTT inventory
