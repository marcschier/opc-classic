# Release process

## Pre-1.0 alpha/beta/rc releases

1. Update `CHANGELOG.md` — move items from `## [Unreleased]` to a new
   `## [0.1.0-alpha.N]` section with today's date.
2. Commit + push the changelog update.
3. Tag the commit: `git tag v0.1.0-alpha.N` (or whatever)
4. Push the tag: `git push origin v0.1.0-alpha.N`
5. The `release` workflow auto-runs on tag push:
   - Validates tag format `v<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`
   - Builds + tests (excluding NativeConformance/MatrikonConformance/CompatMatrix)
   - Packs every `Opc.Classic.*` library to a nupkg + snupkg
   - Pushes to nuget.org if `NUGET_API_KEY` secret is configured
   - Uploads artifacts unconditionally for manual review
   - Creates a GitHub Release with the changelog section as the description

## 1.0.0 release prerequisites

The 1.0.0 release waits for ALL of the following to be GREEN:

- ✅ AOT canary verified (`samples/Opc.Classic.Samples.AotCanary/`)
- ✅ Coverage ≥ 70% line / 60% branch (current ~82%/70% on Core)
- ⏳ Phase 14B native COM conformance (NEEDS prerequisites in workflow)
- ⏳ Phase 14C Matrikon conformance (NEEDS Matrikon installer secret)
- ⏳ Phase 14D compat matrix (NEEDS Phase 14B + 14C green AND Phase 4F server hosting fully built out)
- ⏳ Phase 14E OPC CTT (OPTIONAL; gated on OPC Foundation membership)

Once those are green, the 1.0.0 release is `git tag v1.0.0` + `git push origin v1.0.0`.

## Version history

- `0.2.0-alpha.1` (2026-05-23): cumulative Session 10/11 pre-release; tag/push is a manual user action.
- `0.1.0-alpha.2` (2026-05-22): Phase 16E release workflow and first alpha.2 package prep.

## Manual / workflow_dispatch trigger

If automated tag-based dispatch fails for any reason, the release
workflow accepts a manual trigger:

GitHub → Actions → Release → Run workflow → input `tag: v0.1.0-alpha.N`

## Required secrets

| Secret | Purpose |
|---|---|
| `NUGET_API_KEY` | nuget.org API key for `dotnet nuget push` |
| (optional) `OPC_CTT_INSTALLER_URL` | OPC Foundation CTT installer URL (gates the Phase 14E workflow) |
| (optional) `MATRIKON_INSTALLER_URL` | Matrikon Simulation Server installer URL (gates Phase 14C) |

## Versioning

The project uses simplified SemVer: `<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`.

Pre-1.0 is alpha. The first 1.0.0 release ships when the compat matrix is green.
