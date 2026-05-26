# OPC Compliance Test Tool (CTT) conformance

The OPC Foundation's Compliance Test Tool (CTT) is the canonical conformance
suite for OPC Classic servers. This document describes how to validate an
`Opc.Classic`-based server against the CTT, both in CI and on a local Windows
workstation.

For the CI workflow's internal architecture (install order, registry hive
choice, scope-boundary rationale), see `docs/ctt/CI_DESIGN.md`.

## Scope of this gate

| Server under test | `samples/Opc.Classic.Samples.CttServer/` |
|---|---|
| ProgID | `Opc.Classic.DaSample.1` |
| CLSID | `{8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A}` |
| CI workflow | `.github/workflows/opc-ctt.yml` |
| Artifact | `opc-ctt-results` |

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

## CI flow

`.github/workflows/opc-ctt.yml` runs on `windows-2022` and:

1. Installs the six vendored CTT MSIs via `msiexec /i ... /quiet /norestart`,
   Common Modules first, then plugins in spec order
2. Starts the OPCEnum service (the OPC server enumerator)
3. Publishes `Opc.Classic.Samples.CttServer` Release
4. Registers the managed server under HKLM via the `--register` CLI
5. Locates `OpcCtt.exe` and dumps its `/?` help output as a diagnostic
6. Runs CTT against `Opc.Classic.DaSample.1` (`continue-on-error: true`)
7. Unregisters the server
8. Uploads `ctt-results.xml` + the CLI help dump as `opc-ctt-results`

The workflow runs weekly on Sundays at 03:00 UTC and on `workflow_dispatch`.

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

`Opc.Classic.Samples.CttServer`'s `IClassFactory.CreateInstance` returns
`E_NOINTERFACE` for any IID besides `IID_IUnknown`. This means:

- ✅ The CTT **discovers** the server through OPCEnum or direct CLSID lookup
- ✅ The CTT **launches** the server EXE through `CoCreateInstance` / SCM
- ❌ Every CTT conformance test that binds to `IOPCServer`, `IOPCBrowse`,
  `IOPCItemMgt`, etc. **fails immediately** with `E_NOINTERFACE`

The CI workflow's `continue-on-error: true` reflects this: the workflow today
proves the **registration plumbing** (install → register → launch → unregister),
not the conformance result. Full IOPCServer dispatch via the managed DCOM
listener (`OpcDaServerHost`) is the next workstream — once it lands the
`continue-on-error` is dropped and a passing `ctt-results.xml` becomes a
release gate.

## Release gates

| Tag | Gate |
|---|---|
| `1.0.0-rc.1` | Build 0/0 + tests passing + CTT plumbing smoke green |
| `1.0.0` (FINAL) | CTT conformance PASS on `Opc.Classic.DaSample.1` (no failing tests in `ctt-results.xml`) |

The XML report is kept alongside the release artifacts for auditability.

## Related documentation

- `docs/ctt/CI_DESIGN.md` — CI workflow internals
- `samples/Opc.Classic.Samples.CttServer/README.md` — sample-level CLI documentation
- `src/Opc.Classic.Hosting/Windows/README.md` — Windows COM registration internals
- `External/CTT/readme.txt` — original OPC Foundation CTT inventory
