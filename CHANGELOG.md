# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
