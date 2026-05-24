# Release process

This repository publishes MIT-licensed `Opc.Classic.*` NuGet packages from plain Markdown documentation and the .NET 10 XML solution. The current package line is `0.6.0-alpha.1`; the next pre-1.0 promotion target is `1.0.0-rc.1`.

## Versioning and cadence

- Use SemVer: `<MAJOR>.<MINOR>.<PATCH>[-<prerelease>.<N>]`.
- Use prerelease labels in the order `alpha`, `beta`, then `rc`.
- Do not reuse release tags. If a package must be replaced, cut a higher version.
- Package IDs and namespaces remain under `Opc.Classic.*`.
- Stable `1.0.0` follows the release-candidate soak only after CI, package install, and conformance gates are green.

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
- All nine OPC Classic sub-spec packages are included in the release set.
- The 127 routed server opnums are covered by the generated dispatch path.
- Conformance gates required for the release line have completed or are explicitly waived by maintainers.
- `CHANGELOG.md` has a dated section for the exact release version.

## Prepare the release commit

1. Move the relevant `CHANGELOG.md` entries from `Unreleased` into a section named for the release version.
2. Confirm `src\Directory.Build.props` contains the intended default package version.
3. Confirm the release workflow can derive the same version from the `v<version>` tag.
4. Commit the release-prep change on the release branch.

## Tag and publish

Use the exact version string, including any prerelease suffix:

```powershell
$version = "1.0.0-rc.1"
git tag "v$version"
git push origin "v$version"
```

The `release` workflow runs on tag push and manual dispatch. It:

- validates the tag format;
- restores, builds, and tests the solution in Release configuration;
- packs every `Opc.Classic.*` library into `.nupkg` and `.snupkg` artifacts;
- pushes packages to nuget.org when `NUGET_API_KEY` is configured;
- uploads package artifacts for review even when publishing is skipped;
- creates a GitHub Release with the matching changelog section.

## Manual workflow dispatch

If tag-triggered automation needs to be re-run, use:

GitHub -> Actions -> Release -> Run workflow -> input `tag: v1.0.0-rc.1`

The manual input must match an existing release tag.

## Required secrets

| Secret | Purpose |
|---|---|
| `NUGET_API_KEY` | nuget.org API key used by `dotnet nuget push`. |
| `OPC_CTT_INSTALLER_URL` | Optional OPC Foundation CTT installer URL for the CTT conformance workflow. |
| `MATRIKON_INSTALLER_URL` | Optional Matrikon Simulation Server installer URL for vendor conformance runs. |

## Package install smoke checks

After packages are available, verify install and build with the published version:

```powershell
$version = "0.6.0-alpha.1"
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
- Record CTT and other conformance report locations in the release notes when applicable.
