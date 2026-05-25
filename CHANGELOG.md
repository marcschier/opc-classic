# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added `Opc.Classic.Mcp` documentation, sample configuration, and AI-agent integration snippets for Claude Desktop, Cursor, VS Code Copilot Chat, and GitHub Copilot CLI.

### Changed

- Reworked the documentation set as plain Markdown with an audience-oriented hub and a consolidated roadmap.

## [0.6.0-alpha.1] - 2026-05-24

### Changed

- `Opc.Classic.Dcom` now inherits the strict AOT, trimming, analyzer, package, and warning-as-error settings used by the other source projects.
- Analyzer cleanup brought the full solution to **0 build warnings / 0 build errors**.
- Source formatting, namespace usage, nullable annotations, exception handling, logging patterns, collection usage, and culture-aware formatting were normalized across the DCOM stack.
- NativeAOT fixes replaced runtime-type array construction with closed, tag-based allocation paths for COM arrays, VARIANT arrays, and SAFEARRAY payloads.
- Assembly names, package IDs, namespaces, docs, tests, and samples consistently use the `Opc.Classic.*` dotted form.

### Verification

- Build: **0 errors / 0 warnings**.
- Tests: **1253 passed / 24 skipped / 0 failed**.
- Runtime source projects are AOT/trim compatible under the shared `src/Directory.Build.props` settings.

## [0.5.0-alpha.1] - 2026-05-24

### Added

- DCOM wire coverage for ORPC envelopes, OBJREF variants, NTLMv2 MIC, SPNEGO mechListMIC, channel binding, and Kerberos packet protection.
- Source-generated server dispatchers for 47 interfaces and 127 opnums.
- Expanded DA and HDA IDL method declarations, including advanced multi-out and complex-array shapes.
- Managed `IRemoteSCMActivator` v5.6 server support and real `OpcEnumClient` discovery.
- Property, snapshot, loopback, generator, and conformance-oriented test coverage, plus DA/AE/HDA client samples and the loopback demo.

### Changed

- VARIANT and SAFEARRAY support covers nested variants, by-ref values, records, multidimensional arrays, and common OPC property/HDA payload shapes.
- Generator diagnostics and migration analyzer diagnostics are documented under `docs/generators/` and `docs/migration/`.

## [0.4.0-alpha.1] - 2026-05-23

### Added

- Managed DA, AE, and HDA server samples with realistic tag/event/history data.
- Comprehensive adoption guide and cookbook-oriented documentation for clients, servers, deployment, security, and migration.
- MIT licensing and repository metadata for package consumers.

### Changed

- Project identity standardized on `Opc.Classic.*` assemblies, namespaces, package IDs, and folder names.
- Generator-emitted proxy names use idiomatic class names while preserving IDL wire identifiers where required.

## [0.3.0-alpha.1] - 2026-05-23

### Changed

- Authentication message handling became self-contained with in-tree NTLMSSP message encoding and test-vector coverage.
- Native COM conformance documentation and registration scripts were prepared for DA, AE, and HDA sample servers.
- Windows conformance scaffolding was aligned with verified native CLSIDs and ProgIDs.

## [0.2.0-alpha.1] - 2026-05-23

### Added

- `DcomCallChannel` over `IAsyncTransport` with bind, request, response, fragmentation, and authentication seams.
- Local server hosting primitives with `LocalCoClass`, `IOpcServerHost`, class registration, and dispatch-table expansion.
- Codec registry support for primitives, conformant arrays, OPC structures, VARIANT, and SAFEARRAY payloads.
- Generated client and server call paths across DA, AE, HDA, Cpx, DX, Batch, Commands, and Security interfaces.
- `Opc.Classic.Samples.CttServer` for CTT-oriented managed DA server validation.

## [0.1.0-alpha.2] - 2026-05-22

### Added

- Source-generated call shims for `[OpcInterface]` and `[OpcMethod]` declarations.
- Hosting, discovery, Kerberos, SPNEGO, channel binding, and async transport foundations.
- Windows CI and native conformance scaffolding.

### Changed

- DCOM defaults use packet integrity, NTLMv2, and NTLM2 session security.
- NTLMv1 is obsolete and requires explicit opt-in.
- Logging uses Microsoft.Extensions.Logging.
