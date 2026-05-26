# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0-rc.1] - 2026-05-26

Release-candidate cut for `1.0.0`. Build green (0/0); tests green (1418+ passing, 24 skipped, 0 failed); Windows COM registration plumbing for the CTT integration in place.

### Added

- `Opc.Classic.Hosting.Windows.WindowsComRegistration` — Windows COM registration shim that writes the full out-of-process server tree (`HKCR\CLSID\{x}` with `LocalServer32`, `AppID` as a named value, `ProgID`, `VersionIndependentProgID`, `Implemented Categories`, `Component Categories\{catid}\409` for LCID 1033) under HKLM or HKCU, in both `Registry32` and `Registry64` views by default.
- `Opc.Classic.Hosting.OpcComponentCategories` — the nine standard OPC Classic CATIDs (DA 1.0 / 2.0 / 3.0, AE 1.0, HDA 1.0, XML-DA 1.0, DX 1.0, Batch 1.0 / 2.0) sourced from the OPC Foundation IDL headers vendored in `External/Include/`.
- `Opc.Classic.Hosting.Windows.ComClassObjectRegistrar` — AOT-friendly raw COM-vtable bridge that registers a managed `IClassFactory` with `ole32!CoRegisterClassObject` so Windows COM SCM can launch the sample EXE via `LocalServer32`.
- `samples/Opc.Classic.Samples.CttServer` — `--register` / `--unregister` / `--registry-hive=hklm|hkcu` / `--registry-view=32|64|both` / `-Embedding` (case-insensitive) CLI for OPC CTT integration.
- `tests/Opc.Classic.Hosting.Tests/Windows/WindowsComRegistrationTests.cs` — 7 HKCU-isolated, parallel-serialized tests covering every documented registry shape including an explicit AppID-as-named-value-not-subkey guard.
- `External/CTT/` — six OPC Compliance Test Tool MSIs (~13 MB total) vendored into the repository for the CI workflow.
- `docs/ctt/CI_DESIGN.md` — CI flow architecture for the OPC CTT workflow (install order, hive choice rationale, scope boundary, unknowns).
- `samples/Opc.Classic.Samples.CttServer/README.md` + `src/Opc.Classic.Hosting/Windows/README.md` — CLI and registration-plumbing usage docs.
- Added `Opc.Classic.Mcp` documentation, sample configuration, and AI-agent integration snippets for Claude Desktop, Cursor, VS Code Copilot Chat, and GitHub Copilot CLI.

### Changed

- `.editorconfig` now drives `AnalysisLevel=latest-all` + `AnalysisMode=All` repo-wide; intentional design choices (CA1034 on proxy `Opnums`, CA1054/CA1056 on custom OPC URL schemes, CA1508 on Kerberos.NET defensive guards) are documented via per-site `[SuppressMessage]` attributes rather than project-wide silencing.
- `OpcStringFilter` memoization table converted from multi-dimensional `bool?[,]` to jagged `bool?[][]` (CA1814) without observable behaviour change.
- `OpcSafeArray.{Lengths,LowerBounds}` exposed as `ReadOnlySpan<int>`, `DispatchResult.Payload` as `ReadOnlyMemory<byte>` (CA1819).
- Test helper methods migrated from `buffer[..writer.Position]` to `buffer.AsMemory(0, writer.Position)` (CA1832) and from synchronous `cts.Cancel()` to `await cts.CancelAsync()` (CA1849) across 9 test files.
- `.github/workflows/opc-ctt.yml` rewritten to install the six vendored OPC CTT MSIs via `msiexec /quiet /norestart`, start the OPCEnum service, publish the sample CttServer, register it under HKLM (both registry views), run the CTT smoke (`continue-on-error: true` while the IClassFactory stub returns `E_NOINTERFACE`), and unregister cleanly. The `OPC_CTT_INSTALLER_URL` secret gating is removed.
- `docs/OPC_CTT_CONFORMANCE.md` rewritten as an adopter-facing usage doc with a local-run cookbook and an explicit scope-boundary note.
- Reworked the documentation set as plain Markdown with an audience-oriented hub and a consolidated roadmap.
- Stripped 35 obsolete `TODO` markers from `src/Opc.Classic.Dcom/` (legacy ported library) without any behaviour change.

### Known gaps

- The Windows-only `IClassFactory.CreateInstance` in `ComClassObjectRegistrar` returns `E_NOINTERFACE` for any IID other than `IID_IUnknown`. This is enough for COM SCM and the OPC Compliance Test Tool to discover and launch the server; full `IOPCServer` / `IOPCBrowse` / `IOPCItemMgt` dispatch via the managed DCOM listener (`OpcDaServerHost`) is the next workstream and is required before `1.0.0` (final).
- Real-server NTLMv2 wire testing (against a live Windows Server) and an external third-party NTLMSSP security audit remain tracked for post-1.0 maintenance.

### Verification

- Build: **0 errors / 0 warnings** across `Opc.Classic.slnx`.
- Tests: **1418+ passed / 24 skipped / 0 failed** with `Category!=NativeConformance&Category!=MatrikonConformance&Category!=CompatMatrix&Category!=Kerberos&Category!=Timing`.
- All `src/` projects are AOT-clean and trim-clean under the shared `src/Directory.Build.props` settings.

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
