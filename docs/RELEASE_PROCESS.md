# Release process

This repository publishes MIT-licensed `Opc.Classic.*` NuGet packages from plain Markdown documentation and the .NET 10 XML solution. 

## Versioning and cadence

- Use SemVer: `<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`.
- Use prerelease labels in the order `alpha`, `beta`, then `rc`.
- Current rc tags are bare version tags such as `1.0.0-rc.10` (no leading `v`) and lowercase prerelease labels.
- Do not reuse release tags. If a package must be replaced, cut a higher version.
- Package IDs and namespaces remain under `Opc.Classic.*`.
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

Use the exact version string, including any prerelease suffix. The rc.1..rc.10 tags in this checkout are annotated and local:

```powershell
$version = "1.0.0-rc.10"
git tag -a $version -m "Opc.Classic $version"
```

Do **not** push tags automatically. Push only after explicit maintainer approval:

```powershell
git push origin $version
```

The current `.github\workflows\release.yml` still triggers on `v*` and validates `v<version>` tags. Before the first remote package publish, either align that workflow with the bare local tag convention or intentionally create/push a matching `v<version>` release tag and document the decision in the release notes.

When the `release` workflow runs, it:

- validates the tag format it is configured to accept;
- restores, builds, and tests the solution in Release configuration;
- packs every `Opc.Classic.*` library into `.nupkg` and `.snupkg` artifacts;
- pushes packages to nuget.org when `NUGET_API_KEY` is configured;
- uploads package artifacts for review even when publishing is skipped;
- creates a GitHub Release with the matching changelog section.

## Manual workflow dispatch

If tag-triggered automation needs to be re-run, use:

GitHub -> Actions -> Release -> Run workflow -> input `tag: <existing-release-tag>`

The manual input must match an existing release tag and the tag format accepted by `.github\workflows\release.yml`.

## Required secrets

| Secret | Purpose |
| --- | --- |
| `NUGET_API_KEY` | nuget.org API key used by `dotnet nuget push`; when absent, package artifacts are still uploaded for review. |

The CTT workflows use vendored installers from `external\private\ctt\`; no `OPC_CTT_INSTALLER_URL` secret is required in the current tree.

## Package install smoke checks

After packages are available, verify install and build with the published version:

```powershell
$version = "1.0.0-rc.10"
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
```

## Post-publish verification

- Confirm the GitHub Release links to the expected tag and artifacts.
- Confirm nuget.org lists all expected `Opc.Classic.*` packages and symbol packages.
- Confirm the package install smoke project restores and builds.
- Record CTT, Docker test fleet, live NTLMv2, and audit report locations in the release notes when applicable.
