# Release process

## Pre-1.0 alpha/beta/rc releases

1. Update `CHANGELOG.md` — move items from `## [Unreleased]` to a new section such as `## [0.4.0-alpha.N]` with today's date.
2. Verify the release build command uses the .NET 10 XML solution:

   ```powershell
   dotnet restore Opc.Classic.slnx
   dotnet build Opc.Classic.slnx
   dotnet test Opc.Classic.slnx --configuration Release --filter "Category!=NativeConformance&Category!=MatrikonConformance&Category!=CompatMatrix"
   ```

3. Confirm package metadata carries the MIT license expression and the public license badge/reference says MIT.
4. Commit + push the release-prep update.
5. Tag the commit: `git tag v0.4.0-alpha.N` (or the exact release version).
6. Push the tag: `git push origin v0.4.0-alpha.N`.
7. The `release` workflow auto-runs on tag push:
   - Validates tag format `v<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`
   - Builds + tests (excluding NativeConformance/MatrikonConformance/CompatMatrix)
   - Packs every `Opc.Classic.*` library to a nupkg + snupkg
   - Pushes to nuget.org if `NUGET_API_KEY` secret is configured
   - Uploads artifacts unconditionally for manual review
   - Creates a GitHub Release with the changelog section as the description

## Package install smoke examples

Use the renamed `Opc.Classic.*` package IDs:

```powershell
dotnet add package Opc.Classic.Core --version 0.4.0-alpha.1
dotnet add package Opc.Classic.Da --version 0.4.0-alpha.1
dotnet add package Opc.Classic.Dcom --version 0.4.0-alpha.1
dotnet add package Opc.Classic.Hosting --version 0.4.0-alpha.1
dotnet add package Opc.Classic.Xml --version 0.4.0-alpha.1
```

## 1.0.0 release prerequisites

The 1.0.0 release waits for ALL blocking gates to be GREEN:

- ✅ AOT canary verified (`samples\Opc.Classic.Samples.AotCanary\`)
- ✅ Coverage ≥ 70% line / 60% branch (baseline 81.69% / 69.73%)
- ✅ Gate 2: LGPL dependency cleared — `SharpCifs.Std` dropped and repository relicensed under MIT
- 🟡 Gate 1: compatibility matrix prep complete — requires a Windows runner with VS 2022 Build Tools, Windows SDK, and OPC Foundation Core Components before native DA/AE/HDA runs can be green
- ⏳ Phase 14C Matrikon conformance — needs `MATRIKON_INSTALLER_URL`
- 🔒 Gate 3: OPC CTT — externally blocked on OPC Foundation membership and `OPC_CTT_INSTALLER_URL`; workflow and managed DA sample are scaffolded

Once those are green, the 1.0.0 release is `git tag v1.0.0` + `git push origin v1.0.0`.

## Version history

- `0.1.0-alpha.1` (2026-05-22): first pre-1.0 package/release workflow preparation.
- `0.2.0-alpha.1` (2026-05-23): DCOM call channel, generated shims, server dispatch, Kerberos seam, and initial managed CTT sample.
- `0.3.0-alpha.1` (2026-05-23): N7.6 FINAL SharpCifs.Std drop; 1.0.0 Gate 2 met.
- `0.4.0-alpha.1` (2026-05-23): rename to `Opc.Classic.*`, generator proxy class rename, MIT relicense, three managed DA/AE/HDA sample servers, and refreshed sample/conformance docs.

## Manual / workflow_dispatch trigger

If automated tag-based dispatch fails for any reason, the release workflow accepts a manual trigger:

GitHub → Actions → Release → Run workflow → input `tag: v0.4.0-alpha.N`

## Required secrets

| Secret | Purpose |
|---|---|
| `NUGET_API_KEY` | nuget.org API key for `dotnet nuget push` |
| (optional) `OPC_CTT_INSTALLER_URL` | OPC Foundation CTT installer URL (gates the Phase 14E workflow) |
| (optional) `MATRIKON_INSTALLER_URL` | Matrikon Simulation Server installer URL (gates Phase 14C) |

## Versioning

The project uses simplified SemVer: `<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`.

Pre-1.0 is alpha. The first 1.0.0 release ships when the compatibility matrix and external conformance gates are green.