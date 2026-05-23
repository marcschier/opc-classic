# Release readiness — 0.2.0-alpha.1 (2026-05-23)

## Pre-flight checks

- [x] Build: 0 errors solution-wide
- [x] Tests: ~700 tests across 17 test projects
- [x] Coverage: 81%+ line / 70%+ branch on Opc.Classic.Core
- [x] AOT verified: samples/Opc.Classic.Samples.AotCanary publishes clean
- [x] NuGet metadata in place (Phase 16A)
- [x] SourceLink + snupkg (Phase 16C)
- [x] Strong-named (Phase 16B; public key token 3a65daacab9a8cf3)
- [x] LICENSE = MIT
- [x] CHANGELOG.md updated with 0.2.0-alpha.1 section
- [x] Version bumped in src/Directory.Build.props

## Known issues acknowledged in release notes

- SharpCifs.Std (LGPL-2.1) still transitively present pending N7.6 follow-up
- Phase 14B/C/D conformance tests soft-skip without real-server infrastructure

## Ship procedure (manual)

The release workflow at `.github/workflows/release.yml` triggers on tag push:

```bash
# Verify the tag matches the expected format
git tag v0.2.0-alpha.1
git push origin v0.2.0-alpha.1
```

The workflow:
1. Builds + tests (excluding NativeConformance/MatrikonConformance/CompatMatrix categories)
2. Packs every Opc.Classic.* assembly
3. Pushes to nuget.org if `NUGET_API_KEY` secret is set
4. Uploads artifacts unconditionally for review
5. Creates a GitHub Release with the changelog section as the body

This step is intentionally not executed from the orchestrator session (it requires user consent for `git push`). The user manually tags + pushes when ready.

## 1.0.0 gates (still open)

- Compat matrix GREEN: real-server Phase 14B/C/D tests passing
- SharpCifs.Std fully dropped (N7.6 followup)
- Optional: OPC CTT pass via Phase 14E workflow + sample
