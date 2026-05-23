# Release readiness — 0.4.0-alpha.1 (2026-05-23)

## Pre-flight checks

- [x] Build command documented as `dotnet build Opc.Classic.slnx`
- [x] Tests baseline retained: 757 tests across the Opc.Classic test inventory
- [x] Coverage baseline retained: 81.69% line / 69.73% branch
- [x] AOT canary sample present: `samples\Opc.Classic.Samples.AotCanary`
- [x] NuGet package IDs renamed to `Opc.Classic.*`
- [x] Generated proxy class names updated from `<Interface>_ClientProxy` to `<Interface>ClientProxy`
- [x] LICENSE = MIT and package metadata should publish MIT
- [x] SharpCifs.Std external LGPL dependency dropped (N7.6 FINAL)
- [x] CHANGELOG.md contains the 0.4.0-alpha.1 section

## Release highlights

- **Breaking rename:** namespaces, package IDs, folders, and project files now use the `Opc.Classic.*` dotted form.
- **MIT relicense:** the repository and distributable packages are license-clean under MIT after the SharpCifs.Std package drop.
- **Sample/conformance servers:** three managed sample servers now cover DA (`Opc.Classic.Samples.DaServer.1`), AE (`Opc.Classic.Samples.AeServer.1`), and HDA (`Opc.Classic.Samples.HdaServer.1`); `samples\Opc.Classic.Samples.CttServer` registers the managed DA CTT target as `Opc.Classic.DaSample.1`; the preserved native OPC Foundation DA, AE, and HDA sample servers under `COM\Sample Server\` back the Windows compatibility matrix.

## Known issues acknowledged in release notes

- Gate 1 compatibility matrix still needs a Windows runner with Visual Studio 2022 Build Tools, Windows SDK, and OPC Foundation Core Components.
- Gate 3 OPC CTT remains externally blocked on OPC Foundation membership and `OPC_CTT_INSTALLER_URL`.
- Matrikon conformance still needs `MATRIKON_INSTALLER_URL`.

## Ship procedure (manual)

The release workflow at `.github\workflows\release.yml` triggers on tag push:

```bash
# Verify the tag matches the expected format
git tag v0.4.0-alpha.1
git push origin v0.4.0-alpha.1
```

The workflow:

1. Builds + tests (excluding NativeConformance/MatrikonConformance/CompatMatrix categories)
2. Packs every `Opc.Classic.*` assembly
3. Pushes to nuget.org if `NUGET_API_KEY` secret is set
4. Uploads artifacts unconditionally for review
5. Creates a GitHub Release with the changelog section as the body

This step is intentionally not executed from the orchestrator session because it requires user consent for `git push`. The user manually tags + pushes when ready.

## 1.0.0 gates

- ✅ **Gate 2 (LGPL): MET** — `SharpCifs.Std` is dropped, N7.6 FINAL is confirmed, and the repository/package license is MIT.
- 🟡 **Gate 1 (compat matrix): prep complete** — workflow, managed and native DA/AE/HDA sample-server references, and ProgID/CLSID inventory are ready; needs Windows runner + OPC Foundation Core Components.
- 🔒 **Gate 3 (CTT): externally blocked** — managed CTT sample server exists and registers `Opc.Classic.DaSample.1`, but the official CTT requires OPC Foundation membership and the installer secret.
