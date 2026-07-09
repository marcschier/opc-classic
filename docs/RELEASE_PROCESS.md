# Release process

This repository publishes MIT-licensed `Opc.Classic.*` NuGet packages and the
distributable managed OPC DA server container image from plain Markdown
documentation and the .NET 10 XML solution. Releases go to **three targets**
in parallel:

- **nuget.org** — the curated consumer set only: `Opc.Classic` (self-contained SDK meta-package bundling the cross-platform runtime), `Opc.Classic.Windows` (Windows DCOM server-hosting add-on), `Opc.Classic.Generators` and `Opc.Classic.MigrationAnalyzer` (Roslyn analyzer packages), and `Opc.Classic.Mcp` (MCP server tool). Conditional on `NUGET_API_KEY`.
- **GitHub Packages NuGet feed** — the full granular `Opc.Classic.*` per-spec set (every runtime assembly as its own package) mirrored to `https://nuget.pkg.github.com/marcschier/index.json` (always-on, uses `GITHUB_TOKEN`).
- **GHCR (`ghcr.io`)** — Docker images (always-on, uses `GITHUB_TOKEN`, all images cosign-signed keyless):
  - `ghcr.io/marcschier/opc-classic-managed:<version>` — **Windows** container variant (`windowsservercore:ltsc2022`); built for users registering the managed server through the Windows SCM.
  - `ghcr.io/marcschier/opc-classic-managed-linux:<version>` — **Linux** multi-arch (`linux/amd64`+`linux/arm64`, `noble-chiseled`) variant; the preferred choice when the consumer only needs the managed TCP RPC listener (the managed code is fully cross-platform; no Windows COM dependency at runtime).
  - The C-built reference images (`opc-c-server`, `opc-c-client`) are publishable on demand via the `publish_reference_images` workflow_dispatch input.

## nuget.org package layout

The nuget.org set is deliberately small so consumers get a curated, self-contained experience, while the granular per-spec packages stay on the GitHub feed for advanced use.

- **`Opc.Classic`** is a self-contained meta-package: it physically bundles the 15 cross-platform runtime assemblies (Core, Hosting, managed DCOM/MSRPC + Kerberos + SMB, DA, AE, HDA, Discovery, Security, XML-DA, Batch, Commands, Complex Data, DX) into `lib/`, and declares only its 3rd-party dependencies (the `Microsoft.Extensions.*` set and `Kerberos.NET`). It carries no `Opc.Classic.*` package dependencies, so a nuget.org consumer never needs the GitHub feed. Bundling uses `BuildOutputInPackage` (see `src/Opc.Classic/Opc.Classic.csproj`), which preserves each assembly's strong name, `InternalsVisibleTo`, and NativeAOT/trim compatibility — there is no IL merging.
- **`Opc.Classic.Windows`** bundles only the Windows COM host (`Opc.Classic.Hosting.Windows`) and depends on `Opc.Classic` for the shared runtime, rather than re-bundling it. Its distinct id avoids clashing with the granular `Opc.Classic.Hosting.Windows` package on the GitHub feed.
- **`Opc.Classic.Generators`** and **`Opc.Classic.MigrationAnalyzer`** are Roslyn analyzer packages (the source generators and the migration analyzer/code-fixes). They ship the analyzer assembly under `analyzers/dotnet/cs` with no `lib/` output and no runtime dependencies, per NuGet analyzer conventions.
- **`Opc.Classic.Mcp`** is the MCP server, packaged as a .NET tool (`PackAsTool`, `PackageType=McpServer`) with per-RID runtime variants; it versions in lockstep with the libraries via Nerdbank.GitVersioning.

> Follow-up (independent of nuget.org publishing): the MCP package's `.mcp/server.json` is still the template scaffold. Wiring it up for the Model Context Protocol registry — including keeping its `version` in sync with the package version — is tracked separately.

## Versioning and cadence

Build-time versions are computed by [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
(nbgv) from the repo-root [`version.json`](../version.json) plus git
height. `src/Directory.Build.props` no longer hard-codes a `<Version>`;
nbgv supplies `Version` / `AssemblyVersion` / `FileVersion` /
`AssemblyInformationalVersion` / `PackageVersion` automatically.

- **Dev / CI builds.** On a GitHub Actions build of `main`
  (`GITHUB_REF=refs/heads/main` matches `publicReleaseRefs`) nbgv emits
  a clean `0.1.0-alpha.<height>`. Local builds and non-`main` refs are not
  public releases, so they append a `-g<commit>` suffix
  (`0.1.0-alpha.<height>-g<commit>`). Query the current value with
  `dotnet tool restore` then `dotnet nbgv get-version`.
- **Tagged releases are authoritative.** nbgv intentionally ignores
  `-p:Version`, so the release workflow makes the tag authoritative by
  stamping the exact tag version into `version.json` (ephemeral — the CI
  workspace only, never committed) and building with
  `-p:PublicRelease=true`. The published package version is therefore
  exactly the tag (minus any leading `v`), for both stable (`1.0.0`) and
  prerelease (`1.0.0-rc.11`) tags. `version.json` sets
  `nuGetPackageVersion.semVer: 2` so prerelease dots are preserved.
- nbgv needs full git history. CI checkouts use `fetch-depth: 0`; the
  managed Docker image build relies on `.git` being present in the build
  context (there is intentionally **no** root `.dockerignore` excluding
  it — if one is ever added, the Dockerfile must instead stamp the
  version via the same `version.json` rewrite or a build-arg). Assemblies
  built inside the Docker image carry the nbgv dev version
  (`1.0.<height>`); the image is identified by its registry tag, which is
  the release version.
- Use SemVer: `<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`.
- Use prerelease labels in the order `alpha`, `beta`, then `rc`.
- Tags going forward should use the canonical `v` prefix (e.g. `v1.0.0`); the workflow tolerates bare tags (`1.0.0`) for compatibility with non-prefixed tag history.
- Do not reuse release tags. If a package must be replaced, cut a higher version.
- Package IDs and namespaces remain under `Opc.Classic.*`.
- The Docker image tag tracks the release version. `:latest` moves only on **stable** releases (no `-<prerelease>` suffix).
- Stable `1.0.0` follows the release-candidate soak only after CI, package install, live NTLMv2, and external audit gates are green or explicitly waived by maintainers.

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
- The generated dispatch path covers the routed OPC server opnums.
- Conformance gates required for the release line have completed or are explicitly waived by maintainers.
- `CHANGELOG.md` has a dated section for the exact release version, and `[Unreleased]` contains only intentional unreleased work.

## Prepare the release change

1. Move the relevant `CHANGELOG.md` entries from `Unreleased` into a section named for the release version.
2. The package version is derived by the release workflow from the Git tag: it stamps the tag version into `version.json` and builds with `-p:PublicRelease=true`, so Nerdbank.GitVersioning emits exactly the tag version. There is no per-release `Directory.Build.props` or `version.json` edit to commit; confirm only that the intended tag string is correct.
3. Confirm the tag string matches the `[v]<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]` format the workflow validates. (`dotnet nbgv get-version` shows the nbgv *dev* default for the current commit — the release stamps the tag over it.)
4. Create the release-prep Git change on the release branch.

## Tag and publish

Use the exact version string, including any prerelease suffix. `v` prefix is preferred going forward; bare tags are tolerated:

```powershell
$version = "1.0.0"
git tag -a "v$version" -m "Opc.Classic $version"
```

Do **not** push tags automatically. Push only after explicit maintainer approval:

```powershell
git push origin "v$version"
```

The workflow trigger `.github\workflows\release.yml` accepts tag patterns `v*`, `1.*`, and `2.*` and validates that the tag matches `[v]<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`. A leading `v` is stripped from the version derived for package and image tags, so a `v1.0.0` tag still produces `Opc.Classic.Core.1.0.0.nupkg` and `ghcr.io/marcschier/opc-classic-managed:1.0.0`.

When the `release` workflow runs, it:

- validates the tag format;
- restores, builds, and tests the solution in Release configuration;
- packs every `Opc.Classic.*` library plus the `Opc.Classic` SDK meta-package, the `Opc.Classic.Windows` add-on, the `Opc.Classic.Generators` / `Opc.Classic.MigrationAnalyzer` analyzer packages, and the `Opc.Classic.Mcp` tool into `.nupkg`/`.snupkg` artifacts;
- pushes **only the curated set** — `Opc.Classic`, `Opc.Classic.Windows`, `Opc.Classic.Generators`, `Opc.Classic.MigrationAnalyzer`, and `Opc.Classic.Mcp` (including its per-RID variants) — to **nuget.org** when `NUGET_API_KEY` is configured;
- pushes **all** packages to the **GitHub Packages NuGet feed** at `https://nuget.pkg.github.com/marcschier/index.json` (always — uses the auto-issued `GITHUB_TOKEN`);
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

## Consuming the published packages and image

### NuGet packages (nuget.org)

nuget.org carries the curated, self-contained set. Reference the SDK meta-package — it bundles the full cross-platform runtime (client and managed server) and declares only its 3rd-party dependencies:

```powershell
dotnet add package Opc.Classic --version 1.0.0
```

On Windows, add the DCOM server-hosting add-on (it pulls in `Opc.Classic` automatically):

```powershell
dotnet add package Opc.Classic.Windows --version 1.0.0
```

Install the MCP server as a .NET tool:

```powershell
dotnet tool install --global Opc.Classic.Mcp --version 1.0.0
```

The Roslyn analyzer packages install like any analyzer (build-time only):

```powershell
dotnet add package Opc.Classic.Generators --version 1.0.0
dotnet add package Opc.Classic.MigrationAnalyzer --version 1.0.0
```

### NuGet packages (GitHub Packages NuGet feed)

The GitHub Packages feed carries the full granular `Opc.Classic.*` per-spec set (each runtime assembly as its own package) for consumers who prefer to reference individual specs rather than the bundled SDK. GitHub Packages NuGet requires **authenticated reads even for public packages**. Configure a `nuget.config` in the consuming repo:

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
dotnet add package Opc.Classic.Core --version 1.0.0 --source github-marcschier
```

### Docker images (GHCR)

GHCR allows anonymous reads for public images (no PAT needed for `docker pull`).

**Linux (preferred for non-Windows consumers; multi-arch amd64+arm64):**

```bash
docker pull ghcr.io/marcschier/opc-classic-managed-linux:1.0.0
docker pull ghcr.io/marcschier/opc-classic-managed-linux:latest  # stable releases only
```

**Windows (when the consumer needs Windows SCM-style registration):**

```powershell
docker pull ghcr.io/marcschier/opc-classic-managed:1.0.0
docker pull ghcr.io/marcschier/opc-classic-managed:latest  # stable releases only
```

#### Verifying cosign signatures

Every Docker image push from the release workflow is signed via cosign keyless (Sigstore Rekor transparency log + GHCR `.sig` co-located artifact). Verify before deploying production:

```bash
cosign verify \
  --certificate-identity-regexp 'https://github.com/marcschier/opc-classic/\.github/workflows/release\.yml@.*' \
  --certificate-oidc-issuer 'https://token.actions.githubusercontent.com' \
  ghcr.io/marcschier/opc-classic-managed-linux:1.0.0
```

Same command shape verifies the Windows variant (`opc-classic-managed`) and the optional reference images (`opc-classic-c-server`, `opc-classic-c-client`). On success, cosign prints the signing certificate and Rekor transparency-log entry; on signature failure it exits non-zero.

## Package install smoke checks

After packages are available, verify install and build with the published version:

```powershell
$version = "1.0.0"
dotnet new console -n PackageSmoke
Set-Location PackageSmoke
dotnet add package Opc.Classic --version $version
# On Windows, also validate the DCOM server-hosting add-on:
dotnet add package Opc.Classic.Windows --version $version
dotnet build
```

For manual package publication from workflow artifacts, use the same package files produced by the release workflow:

```powershell
$version = "1.0.0"
$nugetOrg = "https://api.nuget.org/v3/index.json"
# nuget.org: only the curated set. Match exact ids -- a "Opc.Classic.Mcp*" glob would
# wrongly catch Opc.Classic.Mcp.Capture, which stays GitHub-feed-only. Keep the RID
# list in sync with <RuntimeIdentifiers> in mcp/Opc.Classic.Mcp.csproj.
$curated = @(
  "Opc.Classic", "Opc.Classic.Windows", "Opc.Classic.Generators", "Opc.Classic.MigrationAnalyzer",
  "Opc.Classic.Mcp",
  "Opc.Classic.Mcp.win-x64", "Opc.Classic.Mcp.win-arm64", "Opc.Classic.Mcp.osx-arm64",
  "Opc.Classic.Mcp.linux-x64", "Opc.Classic.Mcp.linux-arm64", "Opc.Classic.Mcp.linux-musl-x64")
foreach ($id in $curated) {
  $f = ".\.nupkg\$id.$version.nupkg"
  if (Test-Path $f) { dotnet nuget push $f --source $nugetOrg --api-key <key> --skip-duplicate }
}
# GitHub Packages feed: every package.
dotnet nuget push ".\.nupkg\*.nupkg" --source https://nuget.pkg.github.com/marcschier/index.json --api-key <GITHUB_TOKEN> --skip-duplicate
```

## Post-publish verification

- Confirm the GitHub Release links to the expected tag and artifacts.
- Confirm nuget.org lists the curated set (`Opc.Classic`, `Opc.Classic.Windows`, `Opc.Classic.Generators`, `Opc.Classic.MigrationAnalyzer`, and `Opc.Classic.Mcp` + its per-RID variants) and that the granular per-spec packages are **not** on nuget.org.
- Confirm the GitHub Packages NuGet feed lists the same packages (visit `https://github.com/marcschier/opc-classic/packages` or query the feed directly).
- Confirm `ghcr.io/marcschier/opc-classic-managed:<version>` (Windows) and `ghcr.io/marcschier/opc-classic-managed-linux:<version>` (Linux multi-arch) are pullable (and `:latest` for stable releases).
- Verify the cosign keyless signature on each pushed image with the command shape documented under "Docker images (GHCR)". A signature-verification failure blocks promotion.
- Confirm the package install smoke project restores and builds.
- Record Docker test fleet, live NTLMv2, and audit report locations in the release notes when applicable.

## Future enhancements (tracked separately)

- **NuGet package code-signing** — would require a code-signing cert; out of scope for the current public-release flow. Strong-name assembly identity via `build/Opc.Classic.snk` is already in place.
- **Broader Docker image set** — `opc-managed` (Windows + Linux variants) is the only distributable today. The fleet's C-built reference images (`opc-c-server`, `opc-c-client`) ship on demand via `publish_reference_images: true`. The test/fixture images (`opc-testserver`, `opc-testclient`, `samba`) are intentionally not published.

