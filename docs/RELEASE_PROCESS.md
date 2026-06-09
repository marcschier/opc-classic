# Release process

This repository publishes MIT-licensed `Opc.Classic.*` NuGet packages and the
distributable managed OPC DA server container image from plain Markdown
documentation and the .NET 10 XML solution. Releases go to **three targets**
in parallel:

- **nuget.org** — `Opc.Classic.*` NuGet packages (conditional on `NUGET_API_KEY`).
- **GitHub Packages NuGet feed** — same packages mirrored to `https://nuget.pkg.github.com/marcschier/index.json` (always-on, uses `GITHUB_TOKEN`).
- **GHCR (`ghcr.io`)** — Docker image `ghcr.io/marcschier/opc-classic-managed:<version>` (always-on, uses `GITHUB_TOKEN`). The C-built reference images (`opc-c-server`, `opc-c-client`) are publishable on demand via the `publish_reference_images` workflow_dispatch input.

## Versioning and cadence

- Use SemVer: `<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`.
- Use prerelease labels in the order `alpha`, `beta`, then `rc`.
- Tags going forward should use the canonical `v` prefix (e.g. `v1.0.0-rc.11`); the workflow tolerates bare tags (`1.0.0-rc.11`) for compatibility with the existing `1.0.0-rc.1`..`1.0.0-rc.10` history.
- Do not reuse release tags. If a package must be replaced, cut a higher version.
- Package IDs and namespaces remain under `Opc.Classic.*`.
- The Docker image tag tracks the release version. `:latest` moves only on **stable** releases (no `-<prerelease>` suffix).
- Stable `1.0.0` follows the release-candidate soak only after CI, package install, OPC CTT, live NTLMv2, and external audit gates are green or explicitly waived by maintainers.

## Release readiness checklist

Run the release checks from a clean working tree:

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx --configuration Release
dotnet test Opc.Classic.slnx --configuration Release --filter "Category!=NativeConformance&Category!=MatrikonConformance&Category!=CompatMatrix"
dotnet publish samples\Opc.Classic.Samples.AotCanary --configuration Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

Before tagging, verify:

- Build completes with 0 errors and 0 warnings.
- Test results are green, with only expected skipped tests.
- Package metadata carries `PackageLicenseExpression=MIT`.
- Public APIs and package IDs use the `Opc.Classic.*` namespace family.
- NativeAOT and trimming checks remain clean.
- All OPC Classic sub-spec packages are included in the release set.
- The 127 routed server opnums are covered by the generated dispatch path.
- Conformance gates required for the release line have completed or are explicitly waived by maintainers.
- `CHANGELOG.md` has a dated section for the exact release version, and `[Unreleased]` contains only intentional unreleased work.

## Prepare the release change

1. Move the relevant `CHANGELOG.md` entries from `Unreleased` into a section named for the release version.
2. Confirm `src\Directory.Build.props` contains the intended default package version for package builds or pass `-p:Version=<version>` consistently.
3. Confirm the release workflow can derive the same package version from the tag you intend to publish.
4. Create the release-prep Git change on the release branch.

## Tag and publish

Use the exact version string, including any prerelease suffix. `v` prefix is preferred going forward; bare tags are tolerated:

```powershell
$version = "1.0.0-rc.11"
git tag -a "v$version" -m "Opc.Classic $version"
```

Do **not** push tags automatically. Push only after explicit maintainer approval:

```powershell
git push origin "v$version"
```

The workflow trigger `.github\workflows\release.yml` accepts tag patterns `v*`, `1.*`, and `2.*` and validates that the tag matches `[v]<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`. A leading `v` is stripped from the version derived for package and image tags, so a `v1.0.0-rc.11` tag still produces `Opc.Classic.Core.1.0.0-rc.11.nupkg` and `ghcr.io/marcschier/opc-classic-managed:1.0.0-rc.11`.

When the `release` workflow runs, it:

- validates the tag format;
- restores, builds, and tests the solution in Release configuration;
- packs every `Opc.Classic.*` library into `.nupkg` and `.snupkg` artifacts;
- pushes packages to **nuget.org** when `NUGET_API_KEY` is configured;
- pushes packages to the **GitHub Packages NuGet feed** at `https://nuget.pkg.github.com/marcschier/index.json` (always — uses the auto-issued `GITHUB_TOKEN`);
- uploads package artifacts for review even when publishing is skipped;
- creates a GitHub Release with the matching changelog section, the package files attached, and consumer install instructions.

Then the `docker-publish` job (gated on `release` succeeding, runs on `windows-2022`):

- logs in to `ghcr.io` with `GITHUB_TOKEN`;
- builds and pushes `ghcr.io/marcschier/opc-classic-managed:<version>` (plus `:latest` only on stable releases);
- builds and pushes `ghcr.io/marcschier/opc-classic-c-server` and `opc-classic-c-client` **only** when `workflow_dispatch` is used with `publish_reference_images: true` (default is opc-managed only for tag-push releases — avoids surprising the registry with interop test infrastructure on every release).

## Manual workflow dispatch

If tag-triggered automation needs to be re-run, use:

GitHub → Actions → Release → Run workflow → input `tag: <existing-release-tag>` (and optionally `publish_reference_images: true` to broaden the Docker push to the C-built reference images).

The manual input must match an existing release tag and the tag format accepted by `.github\workflows\release.yml`.

## Required secrets

| Secret | Purpose | Required? |
| --- | --- | --- |
| `GITHUB_TOKEN` | Auto-issued per workflow; used to push to **GitHub Packages NuGet feed** and **GHCR Docker registry**. No setup needed. | Auto |
| `NUGET_API_KEY` | nuget.org API key used by `dotnet nuget push`. When absent, the nuget.org push step is skipped (GitHub Packages and GHCR pushes still proceed). | Optional |

The CTT workflows use vendored installers from `external\private\ctt\`; no `OPC_CTT_INSTALLER_URL` secret is required in the current tree.

## Consuming the published packages and image

### NuGet packages (nuget.org)

```powershell
dotnet add package Opc.Classic.Core --version 1.0.0-rc.11
```

### NuGet packages (GitHub Packages NuGet feed)

GitHub Packages NuGet requires **authenticated reads even for public packages**. Configure a `nuget.config` in the consuming repo:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github-marcschier" value="https://nuget.pkg.github.com/marcschier/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-marcschier>
      <add key="Username" value="USERNAME" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_TOKEN%" />
    </github-marcschier>
  </packageSourceCredentials>
</configuration>
```

Where `%GITHUB_PACKAGES_TOKEN%` is a Personal Access Token with `read:packages` scope. Then:

```powershell
dotnet add package Opc.Classic.Core --version 1.0.0-rc.11 --source github-marcschier
```

### Docker image (GHCR)

GHCR allows anonymous reads for public images (no PAT needed for `docker pull`):

```powershell
docker pull ghcr.io/marcschier/opc-classic-managed:1.0.0-rc.11
docker pull ghcr.io/marcschier/opc-classic-managed:latest  # stable releases only
```

## Package install smoke checks

After packages are available, verify install and build with the published version:

```powershell
$version = "1.0.0-rc.11"
dotnet new console -n PackageSmoke
Set-Location PackageSmoke
dotnet add package Opc.Classic.Core --version $version
dotnet add package Opc.Classic.Da --version $version
dotnet add package Opc.Classic.Dcom --version $version
dotnet add package Opc.Classic.Hosting --version $version
dotnet add package Opc.Classic.Xml --version $version
dotnet build
```

For manual package publication from workflow artifacts, use the same package files produced by the release workflow:

```powershell
dotnet nuget push .\.nupkg\*.nupkg --source https://api.nuget.org/v3/index.json --api-key <key> --skip-duplicate
dotnet nuget push .\.nupkg\*.nupkg --source https://nuget.pkg.github.com/marcschier/index.json --api-key <GITHUB_TOKEN> --skip-duplicate
```

## Post-publish verification

- Confirm the GitHub Release links to the expected tag and artifacts.
- Confirm nuget.org lists all expected `Opc.Classic.*` packages and symbol packages.
- Confirm the GitHub Packages NuGet feed lists the same packages (visit `https://github.com/marcschier/opc-classic/packages` or query the feed directly).
- Confirm `ghcr.io/marcschier/opc-classic-managed:<version>` is pullable (and `:latest` for stable releases).
- Confirm the package install smoke project restores and builds.
- Record CTT, Docker test fleet, live NTLMv2, and audit report locations in the release notes when applicable.

## Future enhancements (tracked separately)

- **Cosign keyless image signing** — the release workflow already has `id-token: write`, so adding `cosign sign --yes ghcr.io/marcschier/opc-classic-managed@<digest>` after the push is a small follow-up. Tracks transparency-log entry per release. Not implemented yet.
- **NuGet package code-signing** — would require a code-signing cert; out of scope for the current public-release flow. Strong-name assembly identity via `build/Opc.Classic.snk` is already in place.
- **Broader Docker image set** — `opc-managed` is the only distributable today. The fleet's C-built reference images (`opc-c-server`, `opc-c-client`) ship on demand via `publish_reference_images: true`. The test/fixture images (`opc-ctt`, `opc-testserver`, `opc-testclient`, `samba`) are intentionally not published.
- **Linux variant of `opc-managed`** — the managed code is fully cross-platform, so a Linux-based image would dramatically cheapen the Docker build runner. The current Windows-container choice mirrors the legacy expectation that OPC Classic servers run on Windows DCOM hosts.

