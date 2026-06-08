# OPC CTT CI design

This document describes how the `.github\workflows\opc-ctt.yml` workflow is
wired up to run the OPC Compliance Test Tool (CTT) against
`samples\Opc.Classic.Samples.CttServer` on a Windows GitHub Actions runner.
It also records how that diagnostic runner relates to the Windows Docker test
fleet used for the release smoke gate.

rc.10 validation baseline: `dotnet build Opc.Classic.slnx` is 0 warnings / 0 errors, and the 23 .NET test projects are green at 2113 passed / 12 skipped / 0 failed.

## CTT version + provenance

| Component | Version | Source |
| --- | --- | --- |
| OPC CTT Common Modules (Test Tool shell) | v2.0.15 | `external/private/ctt/` |
| OPC CTT DataAccess 2.05a Plugin | v2.0.22 | `external/private/ctt/` |
| OPC CTT DataAccess 3.0 Plugin | v1.0.18 | `external/private/ctt/` |
| OPC CTT Alarms & Events Plugin | v1.0.14 | `external/private/ctt/` |
| OPC CTT Historical Data Plugin | v1.0.8 | `external/private/ctt/` |
| OPC CTT XML-DA Plugin | v1.0.8 | `external/private/ctt/` |

All six MSIs are vendored in the repository (`external/private/ctt/`, ~13 MB total)
and tracked in git. No external download is required at CI time.

## Install order

The Common Modules MSI must be installed first; it provides:

- `OpcCtt.exe` (the test runner)
- The OPCEnum service (`OpcEnum.exe`), which CTT uses to browse for OPC servers
- Shared COM proxy/stub registrations consumed by every plugin

Plugin MSIs may be installed in any order after Common Modules. The workflow
installs them in the spec-ordered sequence (DA 2.05a → DA 3.0 → AE → HDA → XML-DA)
for readability.

Each `msiexec /i ... /quiet /norestart` step accepts return codes 0 (success)
and 3010 (reboot pending; harmless in CI).

## Workflow architecture

```
checkout
  └─ setup-dotnet (global.json)
     └─ install 6 MSIs (Common first)
        └─ Start-Service OpcEnum (idempotent)
           └─ dotnet publish samples/Opc.Classic.Samples.CttServer -c Release
              └─ Opc.Classic.Samples.CttServer.exe --register  (HKLM, both views)
                 └─ Run CTT smoke (continue-on-error)
                    └─ Opc.Classic.Samples.CttServer.exe --unregister
                       └─ upload-artifact opc-ctt-results
```

### Why HKLM (not HKCU)

The GitHub Actions Windows runners execute the workflow under an
administrative user. The CTT (and the OPCEnum service) inspect the merged
`HKCR` view; HKCU registrations would only be visible to the calling user
session, while HKLM is system-wide. Writing under HKLM with both `Registry32`
and `Registry64` views ensures both the 32-bit OPC CTT v2.0.15 binary and
any 64-bit OPC client can discover the server.

For local developer runs the trade-off inverts:
`--register --registry-hive=hkcu` does not require elevation but only the
calling user's session can use the registration. See
`docs\CONFORMANCE.md#opc-ctt-conformance` for the local-run cookbook.

## Current scope: diagnostic runner vs release gate

The standalone `.github\workflows\opc-ctt.yml` workflow remains a non-blocking
diagnostic runner. It installs CTT, publishes and registers the sample server,
launches `OpcCtt.exe`, uploads the help/results artifacts, and always
unregisters the server.

`CttServer` no longer uses the original IUnknown-only class-factory smoke. On
`-Embedding`, it registers `ComClassObjectRegistrar.RegisterClassObject` with a
`CreateInstance` callback that returns `OpcDaServerCcw` for `IID_IUnknown` and
`IOPCServer`; DA group, callback, and enumerator CCWs cover the expanded
Windows activation path. rc.10 DA integration coverage includes full lifecycle,
VEU-info enumerator attributes, and browse continuation points, so remaining
CTT gaps should be treated as diagnostic findings rather than the original stub
limitation.

## Related release gates

`.github\workflows\build.yml` is the primary build/test gate. It restores and builds `Opc.Classic.slnx` on Ubuntu, macOS, and Windows for Debug and Release, runs each `tests\**\*.csproj` with coverage, verifies coverage thresholds, runs `dotnet format --verify-no-changes`, publishes the NativeAOT canary on Ubuntu and Windows, and runs the Windows conformance job on `main` or manual dispatch.

`.github\workflows\docker-test-fleet.yml` is a Windows-container smoke. It runs manually or monthly, switches the runner to Windows containers, executes `external\docker\run-matrix.ps1 -SkipBuild:$false -OnlyManaged`, and uploads `external\docker\results\*.xml`.

## Unknowns / TBDs

- **OpcCtt.exe CLI**: the `/AUTO /Output: /ServerProgId:` syntax used in the
  workflow is speculative. The first real CI run uploads the output of
  `OpcCtt.exe /?` as `opcctt-help.txt`, which will let us verify the canonical
  headless invocation against the v2.0.15 build.
- **MSI EULA prompts**: if any of the six MSIs fail with EULA-related errors
  under `/quiet`, the workflow step can be amended with `ACCEPT_EULA=1` (or
  the EULA-bypass property name documented by the OPC Foundation).
- **OPCEnum boot order**: the workflow explicitly `Start-Service OpcEnum`s
  the enumerator service after install in case the MSI does not auto-start it
  on first boot.

## Artifacts

The `opc-ctt-results` artifact uploaded by the workflow contains:

- `ctt-results.xml` — the CTT-emitted conformance report (when the CTT runs)
- `opcctt-help.txt` — the `OpcCtt.exe /?` dump from the locate step (for CLI
  discovery on the first runs)

## Triggers

- `workflow_dispatch` — manual run from the GitHub UI
- `schedule: '0 3 * * 0'` — weekly on Sundays at 03:00 UTC

The previous `OPC_CTT_INSTALLER_URL` secret gating has been removed since the
MSIs are vendored.

## Related files

- `.github\workflows\opc-ctt.yml` — standalone diagnostic workflow
- `.github\workflows\build.yml` — cross-platform build/test/coverage, format, AOT canary, and Windows conformance gate
- `.github\workflows\docker-test-fleet.yml` — Windows-container CTT smoke matrix
- `external\docker\docker-compose.test.yml` and `external\docker\run-matrix.ps1` — four-container fleet orchestration
- `samples\Opc.Classic.Samples.CttServer\README.md` — sample-level CLI docs
- `src\Opc.Classic.Hosting\Windows\README.md` — registration plumbing reference
- `docs\CONFORMANCE.md#opc-ctt-conformance` — adopter-facing usage docs
