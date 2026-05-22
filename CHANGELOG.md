# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

This project is pre-1.0 and has no released versions yet.

The Unreleased section accumulates repository state until the first 1.0.0 release line is cut.

### Added

- Added 13+ OPC NDR spec struct codecs across the DA, AE, HDA, and Batch implementation workstreams.
- Verified OPC NDR codec coverage currently lives under these source paths:
  - `src\OpcClassic.Da\Ndr\`
  - `src\OpcClassic.Ae\Ndr\`
  - `src\OpcClassic.Hda\Ndr\`
- Added DA NDR codecs for server status and item-related structures.
- Added AE NDR codecs for event notification and event server status structures.
- Added HDA NDR codecs for time, item, modified item, attribute, and annotation structures.
- Added NDR primitive and compound marshalling coverage for:
  - conformant arrays
  - `VARIANT`
  - `SAFEARRAY`
  - `LPWSTR` and LPWSTR pointer forms
  - `BSTR`
  - `FILETIME`
  - `Guid`
- Added XML-DA support in `src\OpcClassic.Xml\` with 100% spec-complete coverage for the eight XML-DA operations.
- Added `OpcInterfaceGenerator` in `src\OpcClassic.Generators\`.
- Added generator output for `InterfaceId` metadata from `[OpcInterface]` declarations.
- Added generator output for opnums from `[OpcMethod]` declarations.
- Added source-generated interface metadata coverage for 38 partial interfaces migrated to the generator model.
- Added 642+ tests across 13 test projects under `tests\`.
- Added test coverage for protocol codecs, XML-DA, generator behavior, crypto, logging, and core compatibility paths.
- Added repository governance documentation for security reporting, contribution workflow, and changelog maintenance.

### Changed

- Replaced the Serilog static logging API in `src\OpcClassic.Dcom\` with a Microsoft.Extensions.Logging-based shim as part of Phase 2G.
- Migrated 38 partial interfaces to use `OpcInterfaceGenerator` output for InterfaceId and method opnum metadata instead of hand-maintained declarations.
- Continued tightening source-project quality gates around NativeAOT compatibility and analyzer enforcement.
- Continued the shift from runtime reflection dispatch toward generated metadata and generated dispatch paths.
- Documented that `OpcClassic.slnx` is the .NET 10 XML solution format used for build and test entry points.

### Security

- Phase 3B: DCOM activation path now defaults to `PROTECTION_LEVEL_INTEGRITY` per KB5004442. Set `OpcProtectionLevel.Connect` explicitly on `OpcConnectData` to opt back to the legacy level for unhardened servers.
- Phase 3C: NTLMv2 + extended session security are now enabled by default
  (`rpc.ntlm.ntlmv2=true`, `rpc.ntlm.ntlm2=true`). The legacy NTLMv1
  `Ntlm1` class is marked `[Obsolete]` and is gated behind an explicit
  `rpc.ntlm.allowV1=true` opt-in. Callers that need NTLMv1 for very
  old servers must opt back in (not recommended).
- NTLMv2 and Kerberos/SPNEGO hardening remains in active design and implementation across Phase 2 and Phase 3.
- DCOM authentication behavior remains a security-sensitive review area until the authentication stack reaches its stable pre-1.0 baseline.
- In-tree NTLMv2 behavior remains a target for responsible cryptanalysis reports.
- In-tree RC4 and MD4 behavior in `src\OpcClassic.Dcom\Crypto\` remains a target for deterministic test-vector review.

### Removed

- Removed the Serilog package dependency from the DCOM logging path.
- Replaced that dependency with Microsoft.Extensions.Logging.Abstractions-based infrastructure.
- No stable released versions have been removed because the project has not shipped 1.0.0.
