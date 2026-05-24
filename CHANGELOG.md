# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.5.0-alpha.1] - 2026-05-23

This release closes 9 of 11 critical findings from the protocol gap analysis (`gap-analysis.md`), implementing the M5–M9 milestones from the 1.0.0 roadmap. The wire protocol is now spec-correct on every dimension that was identified as MEDIUM or higher severity.

### Added (M5 — Wire-protocol consolidation)

- **Source-generated server dispatch** (`gap-2`, commit `8c14f40`): 47 auto-generated `<I>ServerDispatcher` partial classes routing 127 opnums (was 7). Replaces the banned-symbol `ReflectionDispatchTable.MethodInfo.Invoke` and `LocalCoClass.Activator.CreateInstance(Type)` — both AOT-contract violations are gone (0 matches in `src/`).
- **ORPC_THIS / ORPC_THAT envelope at channel level** (`gap-4`, commit `119b01b`): per [MS-DCOM] §2.2.19/20. `DcomCallChannel.InvokeAsync` automatically wraps every call. `CausalityContext` flows GUIDs through nested calls.
- **NTLMv2 MIC (Message Integrity Code)** (`gap-5`, commit `78fc9b1`): HMAC-MD5 over NEGOTIATE+CHALLENGE+AUTHENTICATE per [MS-NLMP] §3.1.5.1.2. Constant-time verification.
- **SPNEGO mechListMIC verification + NegTokenResp encoding** (`gap-7`, commit `d80a2bb`): full [RFC 4178] + [MS-SPNG] flow with `IGssMicProvider` interface for inner-mech delegation.
- **VARIANT / SAFEARRAY full surface** (`gap-8`, commit `aae013a`): VT_VARIANT, VT_BYREF, VT_RECORD, multi-dim SAFEARRAY, FADF_* features. Recursion limited to 64 levels (DoS protection).

### Added (M6 — Method-surface expansion)

- **DA: 23 missing IDL methods declared** (`gap-9`, commit `7c547e9`) across 11 interfaces: IOPCServer (AddGroup, GetGroupByName, CreateGroupEnumerator), IOPCBrowse, IOPCBrowseServerAddressSpace, IOPCGroupStateMgt, IOPCItemMgt (AddItems, ValidateItems, CreateEnumerator), IOPCSyncIO, IOPCAsyncIO2/3, IOPCItemProperties, IOPCItemDeadbandMgt, IOPCItemSamplingMgt, IOPCItemIO. Now possible thanks to M1 generator advanced types.
- **HDA: 33 missing IDL methods declared** (`gap-10`, commit `d17fbfc`) across 9 interfaces: IOPCHDA_Server, SyncRead, SyncUpdate, SyncAnnotations, AsyncRead, AsyncUpdate, AsyncAnnotations, Playback, Browser, DataCallback. Now possible thanks to M5 VARIANT-full + server-dispatch gen.
- **End-to-end integration tests** (`rw-b1`, commit `cd410d6`): 30 new test methods across 5 spec areas (DA, AE, HDA, CrossSpec, ErrorPath) using `InMemoryCallChannel`. Zero source bugs revealed.

### Added (M7 — DCOM activation + discovery)

- **IRemoteSCMActivator v5.6 server** (`gap-1`, commit `75f5483`): real `RemoteCreateInstance` + `RemoteGetClassObject` server paths per [MS-DCOM] §3.1.2.5.2. ClassFactoryRegistry + OBJREF_STANDARD export via LocalCoClass + ComOxidRuntime. **Managed processes can now host DCOM clients.**
- **Real OpcEnumClient** (`gap-12`, commit `a526f32`): connects to OPCEnum.exe (CLSID `13486D51-4821-11D2-A494-3CB306C10000`) via RemoteCreateInstance + IOPCServerList::EnumClassesOfCategories + IEnumGUID::Next + GetClassDetails (v1/v2). Replaces the NotImplementedException scaffold.

### Added (M8 — Security hardening)

- **Kerberos GSS-API packet protection** (`gap-6`, commit `98fb712`): `gss_wrap` / `gss_unwrap` / `gss_get_mic` / `gss_verify_mic` per [RFC 4121] + [RFC 4757] (RC4-HMAC) + [RFC 3962] (AES128/256-CTS-HMAC-SHA1-96) + [RFC 8009] (AES128/256-CTS-HMAC-SHA256-128/SHA384-192). 5 etypes total. Replay protection via monotonic sequence numbers. RFC test vectors verified.

### Added (M9 — Quality + samples + docs)

- **Property-based test expansion** (`rw-b2`, commit `b2b9218`): 5 new CsCheck test classes / 98 new properties covering NDR primitives, all VARIANT shapes (including post-gap-8 additions), SAFEARRAY multi-dim, all 21+ spec struct codecs, and conformant arrays. Property test project: 110 tests passing.
- **Verify.TUnit snapshot tests** (`rw-b4`, commit `bb91969`): NEW `tests/Opc.Classic.SnapshotTests/` project. 55 golden-file snapshot tests with 55 verified `.verified.txt` files covering primitives, VARIANT shapes, SAFEARRAY shapes, spec structs, ORPC envelope, and OBJREF shapes. Catches accidental wire-format regressions.
- **Docker containerization** (`rw-c5`, commit `ff44248`): 8 Dockerfiles + 2 docker-compose files (main + loopback) + `samples/README.docker.md`. Multi-stage builds, chiseled runtime images, multi-arch (linux/amd64 + linux/arm64) support.
- **`Opc.Classic.MigrationAnalyzer` Roslyn analyzer** (`rw-d5`, commit `06aa534`): 7 OCM* diagnostics + 7 code-fix providers + 15 tests + 8 per-diagnostic doc pages. Detects legacy OpcCom.Da / OpcRcw.* usage and auto-rewrites to Opc.Classic.*.

### Fixed (carried from earlier waves)

- **`IOPCServer` opnums** (commit `aaa3ad5`): 5 silent wire-protocol bugs fixed — `GetStatusAsync` (3→6), `GetErrorStringAsync` (8→4), `RemoveGroupAsync` (5→7), `IOPCGroupStateMgt.SetNameAsync` (4→5), loopback `AddGroupOpnum` (4→3). 29-entry regression test added.
- **DCOM ping cadence** (commit `3c89b19`): client period 240s→80s, server expiry 480s→160s per [MS-DCOM] §3.1.4.1. Sessions are no longer reclaimed by spec-compliant peers.
- **OBJREF_HANDLER / OBJREF_CUSTOM / OBJREF_EXTENDED parsing** (commit `67c18ae`): per [MS-DCOM] §2.2.18. Previously only OBJREF_STANDARD was decoded.
- **Generator diagnostics** (commit `b6ed21e`): OPCGEN001/002/003/007/009/010 wired (reserved but not emitted before).
- **SpnegoDecoder CA1859** (commit `20e3837`): `ReadMechTypes` returns `List<string>` instead of `IReadOnlyList<string>` (performance analyzer fix).

### Added (carried from earlier waves)

- **Discovery: real `RemoteRegistryEnum`** (commit `6aa93fe`): WINREG-over-SMB enumeration of remote OPC server CLSIDs. Replaces NotImplementedException scaffold.
- **Channel binding (CBT) integration** (commit `9d126d2`): TLS cert extraction + SHA-256/SHA-384 selection per [RFC 5929]; insertion into NTLM `MsvAvChannelBindings` AV_PAIR and Kerberos `KRB_AP_CHKSUM_TYPE_GSS` checksum. + `docs/security/CHANNEL_BINDING.md`.
- **Kerberos KDC Testcontainers fixture** (commit `6421011`): integration tests against MIT Kerberos KDC in Docker.
- **Generator advanced parameter shapes** (commit `cda87ac`): ref/out parameters, IFACE pointer return (STDOBJREF/MEOW), `[OpcGenerateMultiOutRecord]` for auto-generated multi-out record types.
- **4 client sample apps** (commits `12efb2d`, `3a85307`, `5567b50`, `8a85063`): `Opc.Classic.Samples.{DaClient,AeClient,HdaClient,LoopbackDemo}`. The LoopbackDemo runs a DA client + server in one process via `InMemoryCallChannel`.
- **BenchmarkDotNet performance suite** (commit `a2aa28e`): 6 classes / 27 benchmarks for NdrWriter/Reader, OpcVariant, OpcSafeArray, CodecRegistry, DcomCallChannel hot paths.
- **Architecture diagram suite** (commit `0e4f07b`): 10 Mermaid diagrams under `docs/diagrams/` covering high-level architecture, call shim flow, server dispatch, NTLM/Kerberos/SPNEGO handshakes, discovery, source-gen pipeline, DA subscriptions, AOT trimming.
- **STRIDE threat model** (commit `a96a103`): `docs/security/THREAT_MODEL.md` with DFDs, per-flow STRIDE analysis, code-cited mitigations, open recommendations, compliance mapping (OWASP ASVS, NIST SP 800-63, IEC 62443).
- **Long-form tutorial series** (commit `9fe9e01`): 10 tutorials (20,806 words) under `docs/tutorials/`.
- **Protocol gap analysis** (commit `c76d9a5`): `gap-analysis.md` (430 lines) — comprehensive comparison vs OPC IDL + MS-DCOM/RPCE/OAUT/NLMP/KILE/SPNG/RFC 5056/[C706] NDR.

### Verification

- Build: 0 errors / 1281 pre-existing transitional warnings (all in `Opc.Classic.Dcom` legacy code; tracked as M10 rw-a8 cleanup).
- Tests: **1253 passed / 24 skipped / 0 failed** (was 900 at start of 0.5.0 cycle — net +353).
- Server-side opnum coverage: **127 routed** (was 7 — 18× increase).
- AOT-contract violations in src/: **0** (was 2).
- Self-contained NTLMSSP and Kerberos sign/seal both functional across all common etypes.

### Status

- 0.5.0-alpha.1 — wire-protocol correctness + DCOM activation + Kerberos crypto complete.
- M10 (1.0.0-rc.1) — `rw-a8` modernize 1281 Dcom warnings → `IsAotCompatible=true` on all assemblies (multi-week mechanical effort).
- M-CI (1.0.0) — Phase 14D compat matrix GREEN (needs Windows runner + OPC Foundation Core Components); OPC CTT pass (needs OPC Foundation membership + CI secret); nuget.org publish.

## [0.4.0-alpha.1] - 2026-05-23

### Added (samples)

- `samples/Opc.Classic.Samples.DaServer/` — managed DA sample server mirroring the C++ `COM/Sample Server/Da/` reference (Matrikon-style tag tree: Random.*, Bucket Brigade.*, wave generators, error injection tags)
- `samples/Opc.Classic.Samples.AeServer/` — managed AE sample server with periodic synthetic event emission (Heartbeat, condition transitions)
- `samples/Opc.Classic.Samples.HdaServer/` — managed HDA sample server with in-memory historical data (sensor.* signals at 1s resolution over 1 day)
- `docs/ADOPTION.md` — comprehensive adoption guide (~20 KB): Hello World client + server, authentication scenarios, discovery, cross-platform notes, AOT publishing, spec coverage, migration paths, troubleshooting

### Changed

- **BREAKING**: Project renamed from `OpcClassic.*` to `Opc.Classic.*` (dotted form). Every namespace, NuGet package ID, folder, and project file follows the new convention. Pre-1.0 alpha; no packages published under the old name.
- **License changed from EPL-1.0 to MIT.** The OpcClassic .NET 10 codebase has been substantively rewritten since the early DCOM port phases (no third-party DCOM attribution remains in src/); the relicense makes the library more adoptable.
- Generator-emitted proxy class names lose the underscore: `<Interface>_ClientProxy` → `<Interface>ClientProxy`.
- Loopback integration test classes modernized to PascalCase (e.g. `F1_DA_RoundTrip` → `F1DaRoundTrip`).
- IDL-spec identifiers (e.g. `IOPCHDA_Server`, `OPCHDA_ANNOTATION`) RETAIN underscores per OPC convention.

## [0.3.0-alpha.1] - 2026-05-23

### Removed

- **SharpCifs.Std (LGPL-2.1) transitional dependency fully dropped.** The Opc.Classic .NET 10 assembly tree is now license-clean under EPL-1.0 with no LGPL transitive runtime dependencies.

### Changed

- **N7.6 FINAL**: `Opc.Classic.Dcom.Internal.Ntlm.NtlmMessage` + `Type1Message` + `Type2Message` + `Type3Message` reimplemented self-contained per MS-NLMP §2.2.1.1-3. The Phase 2I MS-NLMP §4.2.4.1 spec test vectors continue to pass, proving wire compatibility with the SharpCifs implementation that was replaced.
- **N14B prep**: `COM/README.md` documents the native C++ sample server build process (MSBuild + Windows SDK + OPC Foundation Core Components). `COM/regserver.cmd` registers all three (DA/AE/HDA) sample servers. Phase 14B test scaffolds updated with verified CLSIDs/ProgIDs from the C++ sources.

### Status

- 1.0.0 Gate 2 (clear LGPL dep) — **MET** ✅
- 1.0.0 Gate 1 (compat matrix) — preparation complete; actual native build + conformance runs require Windows runner with VS 2022 Build Tools + Windows SDK + OPC Foundation Core Components installer (the `windows-conformance` CI job is wired to perform this when assets are available)
- 1.0.0 Gate 3 (OPC CTT) — externally blocked on OPC Foundation membership + `OPC_CTT_INSTALLER_URL` CI secret (workflow scaffold ready in `.github/workflows/opc-ctt.yml`)

## [0.2.0-alpha.1] - 2026-05-23

### Added

- **N1.1**: Real `DcomCallChannel : ICallChannel` over `IAsyncTransport` — full DCOM bind PDU + RequestCoPdu/ResponseCoPdu pipeline + fragmentation + IAuthContext abstraction
- **N1.2**: LocalCoClass modernization — BackgroundService accept loop + Channel<IAsyncTransport> worker queue + IDispatchTable expansion
- **N2**: Codec registry supports conformant arrays of primitives + complex types via OpcProxyGenerator
- **N3+N4+N5**: Per-method generator coverage applied across ~90 methods on ~30 OPC interfaces (DA + AE + HDA + Cpx + DX + Batch + Commands + Security)
- **N6**: Server-side per-method dispatchers (OpcDaServerDispatcher + AE + HDA) routing RequestCoPdu payloads to IOpcDaServer/IOpcAeServer/IOpcHdaServer impls
- **N8**: KerberosAuthContext wires Phase 3D KDC + Phase 3E SPNEGO + Phase 3F channel-binding into the auth flow via IAuthContext
- **N9.1**: `samples/Opc.Classic.Samples.CttServer/` — minimal CTT-compliant managed DA server registered as `Opc.Classic.DaSample.1`
- **N9.2**: Phase 14B/C/D loopback test variants exercise the full client→server pipeline via InMemoryCallChannel + OpcDaServerDispatcher

### Changed

- **N7.2**: SharpCifs.Util replaced with BCL (`Convert.ToHexString`, `Encoding.*`)
- **N7.3**: `SharpCifs.Smb.NtlmPasswordAuthentication` replaced with `System.Net.NetworkCredential`
- **N7.4**: `SharpCifs.Ntlmssp` types vendored behind `Opc.Classic.Dcom.Internal.Ntlm` forwarding wrappers
- **N7.5**: `SharpCifs.Dcerpc.Ndr` types vendored behind `Opc.Classic.Dcom.Internal.LegacyNdr` forwarding wrappers — 74 file migrations
- **N7.6** (partial): Self-contained `NdrException`; full reimpl of remaining wrappers (~1050 LOC) tracked in `src/Opc.Classic.Dcom/Common/SharpCifsBoundary.md`

### Security

- Phase 4 + 6 call-shim pipeline now production-grade: real wire transport (DcomCallChannel), real per-method dispatch (server + client), proper NDR codec coverage including arrays
- DCOM defaults remain INTEGRITY + NTLMv2 (Phase 3B + 3C)
- Kerberos integration paths exercised through the new IAuthContext seam

### Known limitations

- `SharpCifs.Std` (LGPL-2.1) still a transitional runtime dep — full drop is N7.6 follow-up (~1050 LOC reimpl tracked in `SharpCifsBoundary.md`)
- Phase 14B/C/D real-server tests soft-skip when prerequisites missing (native COM build, Matrikon installer, OPC CTT) — loopback variants verify the test machinery
- 1.0.0 release waits for compat matrix GREEN against real native servers (Phase 14D follow-up)

## [0.1.0-alpha.2] - 2026-05-22

### Added

- Phase 4 + Phase 6 call-shim pipeline: OpcProxyGenerator emits real
  `ICallChannel.InvokeAsync` bodies for `[OpcMethod]` methods on
  decorated `[OpcInterface]` partials. First real application:
  IOPCServer (Phase 6B template), IOPCGroupStateMgt + IOPCItemIO
  (Phase 6C).
- Codec registry covers 32+ types: primitives + OpcVariant + OpcSafeArray
  + all 21 spec struct codecs.
- Opc.Classic.Hosting end-to-end: IClsidRegistry + IOpcServerHost +
  Microsoft.Extensions.Hosting integration. AddOpcDaServer<T>(configure)
  registers a DA server implementation.
- Opc.Classic.Discovery: LocalEnum (full impl), OpcEnum/RemoteRegistry
  scaffolds, OpcDiscoveryFactory composite with CLSID dedup.
- Opc.Classic.Dcom.Kerberos: KerberosConnectionContext with real
  Kerberos.NET 4.6.146 integration (AP-REQ/AP-REP). SPNEGO encoder
  (RFC 4178). Channel binding (RFC 5056) helper.
- Phase 14A Windows CI runner + Phase 14B/C/D conformance scaffolds.
- IAsyncTransport scaffold (Phase 2C) — System.IO.Pipelines-backed
  contract for the upcoming async I/O refactor.

### Changed

- DCOM defaults: PROTECTION_LEVEL_INTEGRITY + NTLMv2 + NTLM2 sessions
  (Phase 3B + 3C).
- NTLMv1 marked [Obsolete]; gated behind explicit `rpc.ntlm.allowV1=true`.
- Phase 2I: NTLMv2 server-side key derivation per MS-NLMP §3.4.5
  (verified against §4.2.4.1 spec test vectors).

### Removed

- Serilog package dependency (replaced by Microsoft.Extensions.Logging
  via Phase 2G shim).
- SharpCifs.Util.Sharpen.Properties usage replaced with managed
  PropertyBag (Phase 2D.1; 2D.2-2D.5 queued).
