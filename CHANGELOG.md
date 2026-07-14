# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0-alpha] - 2026-07-14

### Added

- Real DCE/RPC capture decoding and fragment-aware ORPC replay with bidirectional presentation-context/call correlation, malformed-frame diagnostics, cancellation, and NTLM integrity/privacy trailer unwrapping.
- Expanded NativeAOT canary coverage across every shipped runtime and SDK meta-assembly.
- Optional self-hosted Windows workflows for real out-of-process DCOM conformance, native TestServer/OpcEnum validation, and Windows-container publication.
- Package, documentation-consistency, activation retry, OXID fallback, DUALSTRINGARRAY, capture, and metadata regression tests.

### Changed

- `IRemoteSCMActivator::RemoteCreateInstance` preserves the authoritative outer HRESULT and uses typed availability, RPC, presentation-context, and malformed-response exceptions so legacy fallback occurs only when the modern SCM interface is unavailable.
- Capture sessions own, redact, and zero NTLM session-key material across failure and disposal paths.
- Legacy `DUALSTRINGARRAY` decoding enforces declared counts, security offsets, terminators, and truncation contracts while accepting valid empty Windows encodings.
- `Opc.Classic.Mcp` generates one version-synchronized registry manifest for the base package and every RID package.
- Documentation describes the current listener authentication, SMB3/`ncacn_np`, XML-DA client-only scope, TestServer registration, release images, conformance gaps, and MCP tool behavior.

### Fixed

- Corrected destructive metadata for HDA insert/replace operations.
- Corrected Windows native-fleet artifact detection, cache reuse, proxy/stub registration checks, TestServer/OpcEnum image setup, persistent-runner cleanup, and classic Docker image publishing.

## [0.2.0-alpha] - 2026-07-13

### Changed

- **Release workflow:** the Windows `opc-classic-managed` container image is no longer built on
  every tag push. GitHub-hosted Windows runners can no longer build Windows-container images (the
  Docker daemon / `docker_engine` named pipe was removed and buildx does not support Windows
  containers), so the `docker-publish` job is now opt-in via the `publish_windows_image`
  `workflow_dispatch` input and requires a self-hosted Windows runner. The Linux
  `opc-classic-managed-linux` image continues to publish automatically on every release.
- Bumped the `Microsoft.Extensions.*` dependency group.

## [0.1.0-alpha.1] - 2026-07-13

### Added

- MS-DCOM-conformant `IRemoteSCMActivator::RemoteCreateInstance` (opnum 4) activation for DA and
  Discovery: byte-exact `ActivationPropertiesIn` / `ActivationPropertiesOut` marshaling, a shared
  `IObjectExporter` `ResolveOxid2` / `ResolveOxid` client, and a legacy `IActivation` fallback for
  older RPCSS.
- Bounded activation retry on transient "server unavailable" HRESULTs so cold-starting DCOM servers
  are retried rather than failed on the first attempt.
- `interop/tools/build-register-proxystubs.ps1`, which builds and registers the vendored OPC
  interface proxy/stubs (`opcproxy` / `opccomn_ps` / `opcsec_ps` / `opc_aeps` / `opchda_ps`) so the
  real Windows COM runtime can standard-marshal the managed sample servers' OPC interfaces on a CI
  runner.

### Fixed

- cross-impl-matrix `E_NOINTERFACE` (`0x80004002`): sample servers activated by the real Windows SCM
  now marshal their OPC interfaces correctly once the OPC interface proxy/stubs are registered on the
  host (previously only present on machines with OPC Core Components installed).

## [0.1.0-alpha] - 2026-07-09

First public preview of Opc.Classic — a cross-platform, NativeAOT-compatible .NET 10
implementation of OPC Classic. See [docs/ROADMAP.md](docs/ROADMAP.md) for the planned
scope and open gates.

### Added

- Managed OPC Classic stack covering DA, AE, HDA, Batch, Commands, Complex Data, DX,
  Security, Discovery, and XML-DA over a fully managed DCOM/MSRPC transport (no Windows
  COM required at runtime), with self-contained NTLMv2 / Kerberos / SPNEGO authentication
  and channel-binding support.
- Source-generated client proxies and server dispatchers; every runtime assembly is
  NativeAOT- and trim-compatible.
- NuGet packaging: the self-contained `Opc.Classic` SDK meta-package, the
  `Opc.Classic.Windows` Windows DCOM server-hosting add-on, the `Opc.Classic.Generators`
  and `Opc.Classic.MigrationAnalyzer` Roslyn packages, and the `Opc.Classic.Mcp` MCP
  server tool are published to nuget.org; the granular per-spec `Opc.Classic.*`
  assemblies ship to the GitHub Packages feed.

### Changed

- **Build / versioning:** adopt [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
  for build-time version derivation. Versions now come from the repo-root
  [`version.json`](version.json) + git height instead of a hard-coded
  `<Version>` in `src/Directory.Build.props`. Release versions remain
  tag-driven: the release workflow stamps the tag version into
  `version.json` and builds with `-p:PublicRelease=true` so the published
  package version is exactly the tag. Adds the `nbgv` CLI as a local
  dotnet tool and `fetch-depth: 0` to CI checkouts so nbgv can read the
  full git history.
