# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-05-27 *(awaiting release-blocker gates to tag)*

First stable release of the cross-platform .NET 10 OPC Classic stack.

### What's in this release

- **Cross-platform DCOM transport** — managed MS-DCOM/MSRPC client and server
  transport over `ncacn_ip_tcp`, with NTLMv2, Kerberos (RC4 + AES), SPNEGO,
  packet integrity/privacy, and RFC 5056/5929 channel binding support.
- **Windows CCW activation** — SCM-launched servers via raw COM vtables with no
  `[ComImport]`. DA covers `IOPCServer`, `IOPCGroupStateMgt(2)`, `IOPCItemMgt`,
  `IOPCSyncIO(2)`, `IOPCAsyncIO2/3`, and `IConnectionPoint(Container)` with
  release-scope per-method vtables and VARIANT/SAFEARRAY/BSTR marshaling.
- **AE + HDA Windows CCWs** — multi-tearoff CCWs for `IOPCEventServer` +
  `IOPCEventSubscriptionMgt` and `IOPCHDA_Server` + Sync/AsyncRead surfaces.
- **Source-generated dispatchers** — Roslyn incremental generators emit static
  client proxies and server dispatchers across the OPC Classic interface set,
  keeping the runtime NativeAOT- and trim-compatible.
- **DA address-space model** — `IOpcAddressSpace`, DA 2.x/3.0 browse,
  `IOPCItemProperties`, `IOPCItemDeadbandMgt`, and `IOPCItemSamplingMgt` backed
  by managed default implementations.
- **OPC CTT integration** — Windows COM registration, a managed CTT sample
  server, vendored CTT installers, and Windows-container Docker fleet wiring
  with managed/native server and client targets.
- **Wire and conformance fixtures** — WINREG PCAP replay coverage, C-built OPC
  reference server/client MVPs, and MS-NLMP/Kerberos/SPNEGO/channel-binding
  vectors for the security-sensitive transport paths.
- **Comprehensive test suite** — the rc.10 sweep has 2113 passed / 12
  skipped / 0 failed across 23 .NET test projects, with 0 build errors and
  0 build warnings.

### Known gaps (deferred to future releases)

See `docs\release-blockers.md` for the 3 remaining quality gates before the
FINAL tag.

## [Unreleased]

Post-`1.0.0-rc.11` work: broad test-coverage sweep, integration suites for the
deferred DCOM paths, two large `external/` tree restructures, a
security-focused fuzz campaign that surfaced and fixed a real bug, the docker
test fleet authoring, and a matrix-driver process-cleanup hardening.

### Added

- **Track CV — broad unit-test coverage sweep** (commit `7f8381a9`). New
  `tests/Opc.Classic.Mcp.Capture.Tests` project (was 0 % / no tests) plus
  ~286 new TUnit tests across Core, Da, Dx, Cpx, Xml, Mcp, Discovery, Ae, Hda,
  Hosting, and Dcom. Gated unit coverage rose from 71 %/51.5 % (line/branch)
  to **75.8 %/58.6 %**; the CI gate floors in `.github/workflows/build.yml`
  raised from 70/50 to 73/55 with conservative cross-OS headroom. New
  per-assembly highlights: Mcp.Capture 0→80.2, Cpx 57.8→77.1, Discovery
  52.1→66.7, Ae 58.7→65.8, Mcp 60.1→65.2, Hda 70.3→74.5, Core 82.5→86.4,
  Dx 85→89.8.
- **Track IT — real-TCP integration suites for the deferred paths**
  (commit `10c9e6f5`). Convert the previously-deferred "integration-only"
  paths into real, cross-platform, non-flaky tests: AE/HDA managed
  client↔server over the real `OpcServerListener` TCP transport (AE
  GetStatus/QueryAvailableFilters/QueryEventCategories; HDA GetStatus/
  GetItemAttributes/ValidateItemIDs), bounded 8×10 concurrency, client-side
  in-flight cancellation (TCS-gated; asserts client OperationCanceledException
  + clean host stop), NTLMv2 handshake protocol coverage (Type1→Type2→Type3
  + MIC + channel-binding incl. tampered/wrong-password negatives),
  Windows-gated `ComClassObjectRegistrar` register/resume/revoke smoke on a
  dedicated MTA thread, and offline pcap-fixture decode through
  `PcapCaptureSource` + `OpcDcomDecoder` (soft-skips only when native
  libpcap/Npcap is unavailable). F4Auth listener-auth tests stay honestly
  skipped (managed listener does not implement server-side NTLM bind
  challenge).
- **Docker conformance fleet authoring** (commit `3a4a7474`). Add
  `external/docker/opc-testserver/` and `external/docker/opc-testclient/`
  Windows-container images built from the vendored CoreComponents CMake
  tree via `external/tools/build-testserver.ps1` (cache-aware multi-stage
  Dockerfile installs VS Build Tools 2022 VCTools+ATL+CMake only on a cold
  cache; testclient reuses the testserver image as artifact source to avoid
  a second ~30-min build). `docker-compose.test.yml` adds the new services
  on `opc-test-net` (10.0.1.12 / 10.0.1.22). `run-matrix.ps1` gains an
  `-IncludeTestServer` switch; `docker-test-fleet.yml` detects
  `external/redist`, caches+primes the CMake build, and soft-skips to
  managed-only when the vendor tree is absent.
- **Track FZ — CsCheck-driven parser-fuzz campaign + deep-run CI**
  (commit `791714cd`). Six phases delivering a shared
  `tests/Opc.Classic.Tests.Fuzz` library (`FuzzHarness` with edge-weighted
  `Gen<byte[]>`, structural `MutateValid` mutator, `AssertParseDoesNotCrash`
  with closed exception set + bounded time/memory, deterministic seeds via
  `OPCCLASSIC_FUZZ_SEED`/`OPCCLASSIC_FUZZ_ITERATIONS`, hex-dump +
  corpus helpers) and ~110 fuzz test cases across 12 attacker-controlled
  parser surfaces: DCE/RPC `PduCodec` + 11 PDU types, NTLM Type1/2/3 +
  AvPairs + MIC, SMB2 message decoders, SPNEGO ASN.1 DER, `NdrReader`
  (incl. `ReadVariant` recursion + length-confusion), `OrpcExtentArrayCodec`,
  OBJREF/`InterfacePointer`, CPX recursive type-dictionary + binary
  decoder, OPCEnum response, XML-DA `SoapEnvelopeReader`, MCP
  `OpcDcomDecoder`. `.github/workflows/fuzz-deep.yml` runs every fuzz
  surface at 10000+ iterations on a workflow_dispatch + weekly schedule.
  `docs/security/THREAT_MODEL.md` §4.1 documents the coverage map.
- **`docs/security/audit-packet/`** (new). Self-contained NTLMSSP
  audit-prep packet (9 docs, 421 lines: README / scope / threat-model
  subset / file inventory / design / KAT references / test-coverage map /
  limitations / reviewer checklist) for the external `rw-e4` review.

### Changed

- **Repository restructure: `ext/` → `external/`** (commits `52773c1c`,
  `f40cf2a5`). Renamed `ext/` → `external/`; flattened
  `ext/redist/CoreComponents/*` up to `external/redist/*` (CoreComponents
  dir removed); consolidated native sample apps under
  `external/redist/samples/` (`OpcTestServer` + `OpcTestClient` from repo
  `samples/`, and the de-spaced `SampleServer`/`SampleClient` +
  `Shared`/README/`regserver.cmd` from `ext/samples/`); moved the
  Windows-container test fleet `docker/` → `external/docker/`. All native
  `.vcxproj` include dirs adjusted (+1 `..` depth, ClCompile + MIDL);
  `OpcTestServer.rc` rewritten to `#include "version.h"`; new
  `OPC_TEST_SAMPLES_DIR` CMake cache variable located the test-app
  sources. Build-tooling scripts moved to `external/tools/` with
  `$PSScriptRoot` math updated. .md citations under `docs/`/`src/`
  rewritten to use filename + "external" provenance phrasing instead of
  hard-coded paths; layout descriptions and runnable command paths
  preserved. New `external/.gitignore` for the CMake build outputs.
- **Vendored CoreComponents pruning** (commit `29d6dc65`). Removed
  `ext/redist/*.msi` and `*.msm`, `ext/CoreComponents/.github`, and the
  upstream WiX/ packaging tree (MSI/MSM packaging removed from `build.ps1`).
  Renamed `Source/` → `src/` throughout the vendored tree.
- **OpcDaServerCcw class-doc refresh + AE opcae_ps.dll waiver** (commit
  `1e482f93`). The CCW class-doc XML comment was refreshed to reflect that
  all 8 tearoffs (IOPCServer/IOPCCommon/IOPCBrowse/IOPCItemProperties/
  IOPCItemIO/IOPCBrowseServerAddressSpace/IOPCSecurityNT/IOPCSecurityPrivate)
  have real managed dispatch (the previous note still claimed
  `E_NOTIMPL` stubs). `docs/CONFORMANCE.md` AE section documents the
  `samples-ae` `get_condition_state`/`ack_condition` `EXPECTED_FAIL` as a
  known external-component limitation: the OPC Foundation `opcae_ps.dll`
  MIDL stub crashes on the `OPCCONDITIONSTATE` round-trip / rejects the
  `AckCondition` `[in]` unmarshal (DR33); the managed proxy↔dispatcher
  round-trip passes.
- **Cross-impl matrix process-cleanup hardening** (commit `f66012800`).
  `tools/run_cross_impl_matrix.py` now stops leftover
  `Opc.Classic.Samples.*` server EXEs and orphan `Opc.Classic.Mcp` `dotnet`
  hosts before the matrix, before each profile, and (in a finally) after
  each profile. The earlier `security-da` `__fatal__` "initialize timed out
  after 60s" regression was caused by SCM-activated samples accumulating
  across profiles, starving the 6th profile's MCP host startup.
  Windows-only; no-op elsewhere so CI is unaffected.

### Fixed

- **OpcDcomDecoder rejects truncated Ethernet inputs cleanly** (commit
  `3db24c20`). Fuzz finding (Track FZ-5): the MCP capture decoder threw
  raw `IndexOutOfRangeException` on malformed/truncated Ethernet frames.
  New private `ValidateCapturedFrame` helper validates Ethernet / VLAN /
  IPv4 / IPv6 / TCP lengths up-front and throws `InvalidDataException`
  with offset context. 4 previously-skipped fuzz tests un-skipped; 20-file
  corpus at `tests/_Fixtures/Fuzz/OpcDcomDecoder/` retained as regression
  fixtures.

## [1.0.0-rc.11] - 2026-06-09

Post-`1.0.0-rc.10` work focused on Matrikon DA interop completeness, wire-trace diagnostics,
and MCP-side IOPCDataCallback queue plumbing. The 1.0.0 release-blocker gates
(`release-100-tag`, `rw-e1-ntlmv2-realserver`, `rw-e4-ntlm-audit`) remain open.

### Added (Tracks AY+/AY++/BF/BG/BH/BI — 2026-06-04 sweep)

- **Track BG — OPCEnum bind regression closed (Issue D, commit `74eba65d`).**
  New `OpcDiscoverySpecCatalog` declares the full Discovery IID set
  (`IOPCServerList(2)`, `IOPCEnumGUID`, `IEnumGUID`, `IRemUnknown(2)`)
  in the initial DCE bind PDU. `DcomOpcEnumCallChannelFactory` now
  consumes the OXID-level `DUALSTRINGARRAY` from the activation
  response to resolve the actual OPCEnum data-port (the per-interface
  OBJREF ResolverBindings carry only OXID-resolver addresses without
  port info, so dialing them landed on RPCSS / port 135). New
  `DcomCallChannelFactory.ConnectActivatedAsync` overload constructs a
  channel routed to the activated object's IPID. `OpcEnumProxyCodec`
  fixed to emit both the explicit `cImplemented` / `cRequired` ULONGs
  AND the conformant array `max_count` prefix per DCE/RPC §14.3.4.
  `DecodeInterfaceRefResponse` handles the `MInterfacePointer` wrapper
  (`referent + ulCntData + max_count + OBJREF`) per MS-DCOM 2.2.18.7
  for `[out] IUnknown**` returns. `NdrReader.ReadVaryingConformantGuidArray`
  decodes `[out, size_is(N), length_is(*pceltFetched)] GUID* rgelt`
  (`IEnumGUID::Next`). Bind-rejection downgrade catch is now IID-specific
  so downstream IEnumGUID rejections don't trigger spurious
  IOPCServerList fallback.
- **Track BH — TestServer registration script aligned to upstream WiX (commit `9d6ed944`).**
  Audited every file copy, registry entry, COM CLSID/AppID/category,
  and self-registration step in
  `D:\git\marcschier\OPC-Classic-CoreComponents\WiX\Installer.wxs` +
  `MergeModule.wxs` + `MergeModuleSdk.wxs`. Output:
  `docs/interop/testserver-registration-spec.md`. `tools/register-testserver.ps1`
  now registers the full 8-DLL proxy/stub set (`opccomn_ps`,
  `opcproxy`, `opc_aeps`, `opcbc_ps`, `OpcCmdPs`, `OpcDxPs`,
  `opchda_ps`, `opcsec_ps`) in dependency order, copies
  `OpcTestServer_x64.config.xml` alongside the EXE, and runs
  `OpcCategoryManager.exe /RegServer`. Live test identified the actual
  `CO_E_SERVER_EXEC_FAILURE` cause: DCOM AppID has no explicit
  Launch/Access ACL, so SCM denies non-admin callers. New
  `tools/grant-testserver-acl.ps1` (delegates to `grant-opcenum-acl.ps1`
  with the TestServer AppID) automates the SD merge.
- **Track BI — `IObjectExporter` dispatcher + Subscribe Advise/Unadvise wireup (commit `41e30ca7`, AP1/AP2/AP4 closed).**
  Manual `IObjectExporterDispatcher` implements MS-DCOM 3.1.2.5.1.1
  opnums 1-5 (SimplePing, ComplexPing, ServerAlive, ResolveOxid2,
  ServerAlive2). `DaCallbackEndpoint.StartAsync` registers it at the
  well-known IID `99FCFEC4-5260-101B-BBCB-00AA0021347A` so a remote
  OPC server's pre-callback ResolveOxid2 / SimplePing probes resolve
  to the listener's actual TCP endpoint + a synthetic IRemUnknown
  IPID. `DaClientTools.Subscribe` lazy-starts the endpoint, calls
  `RegisterSink` + `BuildSinkObjRef`, invokes
  `IConnectionPoint::Advise(sink)`, and stores the cookie +
  `SinkIpid` on `DaSubscriptionContext`. `RemoveGroup` +
  `DaClientState.DisposeAsync` walk subscriptions and call
  `Unadvise(cookie)` + `UnregisterSink(ipid)` for cleanup
  (best-effort; tolerates server-side teardown races).
- **Track AY++ — `OPCITEMSTATE` `[unique] VARIANT` codec fix (commit `7fce8b45`).**
  `NdrOpcItemStateCodec` refactored to the deferred-pile model:
  20-byte inline part (`hClient + filetime + wQuality + wReserved +
  variantRef`) followed by deferred wireVARIANT body. New
  `WriteConformantArray` / `ReadConformantArray` for N-element bulk.
  `IOPCSyncIOClientProxy.InvokeReadAsync` rewired through the new
  helper. Closes `opcclassic.da.read_sync` + `opcclassic.da.poll_subscription`
  against live Matrikon.
- **Track AY+ — Three stacking NDR VARIANT codec bugs fixed (commit `6a8f32ce`).**
  Closes `opcclassic.da.get_properties` against live Matrikon: embedded
  `VARIANT` is a `[unique]` pointer (FC_USER_MARSHAL flags=0x83 sets
  `USER_MARSHAL_UNIQUE` per MS-OAUT 2.2.29.2 — wireVARIANT body lives
  in the deferred pile AFTER string referents); `wireVARIANT` has a
  4-byte ULONG discriminator before the union body (was being skipped);
  `FLAGGED_WORD_BLOB` (BSTR) needs a `max_count` prefix per MS-OAUT
  2.2.23 (`referent + max_count + cBytes + clSize + WCHAR[clSize]`);
  `clSize` is quadwords per MS-OAUT 2.2.29.1, not bytes.
- **Track BF — Probe refresh + Issue D documentation (commit `24e89b4d`).**
  Stable Matrikon probe baseline restored at **25/95 OK with
  `--da-clsid`** / **26/95 OK with `--da-progid` after Track BG**
  (zero DA failures, `discovery.enumerate_servers` and all DA tools
  pass). Updated `docs/interop/probe-coverage.md` table rows + headline.
  New "Issue D" section documents the OPCEnum data-port bind
  rejection class and its root cause (OXID resolution + IPID routing).
- **Probe headline (post-BG)**: **26/95 tools OK** against live
  Matrikon Simulation Server, up from the 19/95 starting baseline at
  rc.10. The 69 remaining FAILs are: missing matching servers
  (HDA / AE / Batch / Commands / DX / XML-DA = 63 of the 69) +
  CPX/security item-specific args (6).

### Original Unreleased entries

- **Track AK1** — `NdrReader.FormatHexContext` decorates every decode-fail throw with a
  hex window centered on `_position` (16 bytes before/after, `>>` marker on the failing
  byte, ASCII gutter). Every `InvalidOperationException` / `InvalidDataException` in
  `NdrReader.cs` + `NdrVariantExtensions.cs` now appends this context.
- **Track AK2** — Opt-in NDR wire capture: `OpcWireCapture` + `WireCapturingCallChannel`
  decorator write per-call `.hex` dumps when `OPCCLASSIC_WIRE_CAPTURE_DIR` is set.
  `tools/probe_servers.py --save-wire-payloads <dir>` plumbs the env var into the
  spawned MCP server. New `docs/interop/wire-captures/` landing page with `.gitignore`.
- **Track AK3** — `tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs` parses the
  `.hex` format back into a `byte[]` for replay-style regression tests.
- **Track AL** — Byte-exact wire fixture tests for request encoding (`SyncIO::Write`,
  `SyncIO::Read`, `ItemMgt::AddItems`), response decoding (the same trio plus a
  null-referent-to-empty-array safety case from Track AG4), and server-side dispatch
  (`ConnectionPoint::GetConnectionInterface`).
- **Track AN** — `tools/grant-opcenum-acl.ps1` reads the OPCEnum AppID's
  `AccessPermission` + `LaunchPermission` REG_BINARY security descriptors, appends a
  `CCDCLCSWRP` ACE for the calling user (or `-Account`), and writes the merged
  descriptor back. Idempotent + `-Unregister` for rollback. Documented in
  `docs/interop/opcenum-auth.md`.
- **Track AP3** — MCP `DaSubscriptionContext` gained a `DaDataCallbackSink` backed by
  a bounded `Channel<DataChangeNotification>` (capacity 1024, drop-oldest with drop
  counter). Implements the full `IOPCDataCallback` interface.
- **Track AP5** — 10 synthetic tests in `DaDataCallbackSinkTests` covering enqueue,
  per-call counters, drain cap + requeue, multi-batch FIFO order, bounded-queue
  drop-oldest, Dispose lifecycle, FILETIME decode, and mismatched-array-length
  rejection.
- **Track AP6** — `docs/interop/da-callbacks.md` documents the architecture (host
  listener + sink OBJREF + Advise + push), the AP status table, the sink contract, the
  production callback-bind path AP1/AP2/AP4 will need, firewall / DCOM ACL prerequisites,
  and the synthetic test coverage.

### Changed

- **Tracks AF/AG/AH/AI** (`dfbf234b`, `3a1ba9c3`) — NDR wire-format completeness sweep
  across ~20 DA/CPX methods: `[OpcEmitArrayCount]` on every sibling-count IDL pattern,
  null-referent-to-empty-array safety on `[out] T**` arrays, relaxed `rpcReserved`
  acceptance, and `OPCITEMPROPERTY` embedded VARIANT shape fixes.
- **Track AJ2** (`d913bb50`) — `IOPCServerList::EnumClassesOfCategories` +
  `GetClassDetails` server-side implementations.
- **Track AM** (`1b059d0a`) — `IEnumOPCItemAttributes::NextAsync` signature changed
  from `Task<OpcItemAttributes[]>` to `Task` + `out OpcItemAttributes[] elements`
  + `out int fetchedCount`. Surfaces the IDL `pceltFetched` correctly so callers can
  detect the last batch (`fetched < celt` or `fetched == 0`) instead of guessing from
  array length. Hosting + CCW + 7 unit tests + integration test updated.
- **Track AP3** — `opcclassic.da.poll_subscription` MCP tool now drains the
  subscription's callback queue first via `Sink.DrainItems(maxNotifications)` before
  falling back to the existing synchronous pull. Client-handle→item-name resolution
  uses a one-shot reverse index over `DaGroupContext.Items` to match the OPC DA wire
  contract (`IOPCDataCallback` delivers values keyed by client handle, not server
  handle). Behavior unchanged when no callbacks are wired (today's default).
- **Track AP3** — `DaSubscriptionContext` changed from positional record to sealed class
  to hold the eagerly-constructed sink. `DaClientState.DisposeAsync` and
  `DaClientTools.RemoveGroup` now dispose orphaned sinks.

### Documentation

- `docs/interop/opcenum-auth.md` gained a "Grant OPCEnum ACLs without dcomcnfg" section
  with usage examples + a "What the script does" subsection explaining the `CCDCLCSWRP`
  rights mask + an audit one-liner.
- `docs/interop/wire-captures/README.md` added with capture format documentation, enable
  steps, and cross-references to the replay parser + da-callbacks + opcenum-auth.
- `docs/interop/da-callbacks.md` documents the AP1-AP6 status table, production
  enablement prerequisites, and the synthetic test coverage.
- `docs/README.md` gained an "Interop with native OPC servers" index section.
- `docs/ROADMAP.md` 1.0.0 checklist reconciled to mark Tracks AC/AE/AF/AG/AH/AI/AK/AL/AM/AN/AP3
  shipped.
- `docs/interop/testserver.md` (Track AJ1) — documents the residual TestServer SCM
  activation blocker as environmental.

### Closed (no user-visible effect; tracked for completeness)

- **Track AC** (`2d96d8f9`) — Pre-declared the full DA IID set in the initial BindPdu;
  unblocked subscribe end-to-end against Matrikon.
- **Track AD** (`ffcc1514`) — `tools/register-testserver.ps1` no-MSI registration of the
  OPC Foundation TestServer; activation still gated by environmental blocker.
- **Track AE** (`ee2425c2`) — `IRemoteSCMActivator::RemoteCreateInstance` raised to
  `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY` (unblocks OPCEnum discovery + ProgID connect).
- **Track AJ1** (`1191e315`) — TestServer activation residual blocker documented.

## [1.0.0-rc.10] - 2026-05-28

Tenth release-candidate. **Spec-coverage gap closure across AE, HDA,
CPX, Security, Common, Batch, and DA**, plus a repo reorganization
landed in `1.0.0-rc.9`.

### Added — AE Windows CCW array-marshaling completion (Track O1)

All 14 previously-`E_NOTIMPL` array-heavy methods now have real bodies
on the Windows CCW path:

- `IOPCEventServer::QueryEventCategories` / `QueryConditionNames` /
  `QuerySubConditionNames` / `QuerySourceConditions` /
  `QueryEventAttributes` — `LPWSTR[]` + `DWORD[]` + `VARTYPE[]` arrays
- `IOPCEventServer::TranslateToItemIDs` — 3 parallel arrays
  (`LPWSTR[]` + `LPWSTR[]` + `CLSID[]`)
- `IOPCEventServer::GetConditionState` — `OPCCONDITIONSTATE*` struct
- `IOPCEventServer::EnableConditionByArea` / `BySource`,
  `DisableConditionByArea` / `BySource`, `AckCondition` — per-item
  `HRESULT[]` arrays
- `IOPCEventSubscriptionMgt::SetReturnedAttributes` /
  `GetReturnedAttributes` — per-category `DWORD[]` attribute IDs

New helper: `src/Opc.Classic.Ae/Hosting/Windows/OpcAeArrayMarshaler.cs`
ships shared COM-task-alloc / free-on-failure utilities for the 14
methods.

### Added — HDA Windows CCW update + playback + advise (Track O2)

- `IOPCHDA_SyncUpdate` raw-vtable CCW (6 methods): `QueryCapabilities`,
  `Insert`, `Replace`, `InsertReplace`, `DeleteRaw`, `DeleteAtTime`.
- `IOPCHDA_AsyncUpdate` raw-vtable CCW (7 methods) firing
  `OnInsert`/`OnReplace`/`OnInsertReplace`/`OnDelete` callbacks.
- `IOPCHDA_Playback` raw-vtable CCW (3 methods): `ReadRawWithUpdate`,
  `ReadProcessedWithUpdate`, `Cancel` — streaming history reads.
- `SyncAnnotationsInsert` + `AsyncAnnotationsInsert` real bodies.
- `AsyncAdviseRaw` + `AsyncAdviseProcessed` periodic-update bodies.

### Added — CPX DA-server integration helpers (Track O3)

- `src/Opc.Classic.Cpx/Hosting/OpcCpxAddressSpace.cs`: managed
  decorator that exposes `/CPX/{TypeSystem}/{Dictionary}/{TypeID}`
  browse branches via the DA hosting path.
- `src/Opc.Classic.Cpx/Hosting/OpcCpxItemProperties.cs`: publishes CPX
  properties 600-609 on complex DA items (TypeSystemId, DictionaryId,
  TypeId, UnconvertedItemId, UnfilteredItemId, DataFilterValue).
- `AddOpcCpxAddressSpace` + `AddOpcCpxItemProperties` DI hooks.
- BitString completion in `OpcBinaryDecoder` / `OpcBinaryEncoder` for
  non-byte-aligned fields per CPX 1.00 §6.2.4.2.1.

### Added — Security reference sample server (Track O4)

- `samples/Opc.Classic.Samples.OpcSecurityServer/` — runnable managed
  sample demonstrating `IOPCSecurityNT` + `IOPCSecurityPrivate` stub
  ACL semantics + cross-platform identity tracking.
- `docs/cookbook/08-implementing-opc-security.md` — DCOM-layer vs
  OPC-layer security distinction + production-replacement guidance.

### Added — Common + Batch convenience helpers (Track O5a + O6)

- `IDaServer.SetClientNameAsync(string, CancellationToken)` —
  high-level wrapper for `IOPCCommon::SetClientName` opnum.
- `OpcBatchPropertyId` typed helper covering 79 spec-defined Batch
  property IDs (400-478) per OPC Batch 2.00 Appendix A; `GetDescription`
  and `GetExpectedVarType` static accessors.

### Added — DA integration test coverage (Track O5b + O5c)

- `DaFullLifecycleTests.cs` — CTT-style end-to-end test against
  `OpcDaServerHost` over loopback TCP: AddGroup → AddItems → sync
  read/write → async callback (Refresh2 → OnDataChange) → RemoveGroup
  → Shutdown.
- `DaEnumOpcItemAttributesVeuInfoTests.cs` — vEUInfo VARIANT round-trip
  for VT_ARRAY|VT_BSTR (discrete enum), VT_ARRAY|VT_R8 (analog range),
  and VT_EMPTY via `CreateEnumerator(IID_IEnumOPCItemAttributes)`.
- `DaBrowseContinuationPointTests.cs` — IOPCBrowse continuation-point
  scenarios across flat + 3-level hierarchical address spaces:
  paged browse, hierarchy-boundary crossing, opaque-token preservation,
  invalid-token rejection (`E_INVALIDCONTINUATIONPOINT`).

### Changed — DA browse continuation tokens are namespaced (Track O5c)

`DefaultBrowse` now emits `"opc-da-browse:N"` style continuation
tokens instead of bare `"N"`; tokens from one browse context can no
longer be confused with tokens from another. The CPX `OpcCpxAddressSpace`
test was updated to match.

### Added — Object-IPID dispatch infrastructure (Track O5c, supporting)

The continuation-point integration test surfaced that the managed DCOM
loopback transport needed object-IPID dispatch support for per-call
object routing. `DcomCallChannel`, `OpcObjectRegistry`,
`RpcServerConnectionProcessor`, and the generator
`OpcServerDispatchGenerator` all gained object-IPID hooks.

### Documentation

- `docs/CONFORMANCE.md` — all 11 docs refreshed for the post-Track-O
  state. The README matrix table now shows the correct "Remaining
  status notes" per spec. 2.0-scope items (XML-DA hosting, SOAP 1.2,
  DX runtime, Batch generic runtime, CPX conversion/filter engines)
  are explicitly preserved as gaps.

### Test results

- **2113 passed / 12 skipped / 0 failed** across 23 .NET test projects.
- 0 build warnings / 0 build errors.
- Cumulative gain from rc.9 baseline (1971 passed): **+142 net new tests**.

## [1.0.0-rc.9] - 2026-05-28

Ninth release-candidate. Repository reorganization (no functional code
changes):

- `External/` → `ext/` (with `ref/docs` and `redist/` subfolders)
- `docs/diagrams/` merged into `docs/architecture/`
- `docs/decisions/` removed (single ADR file)
- Samples section split from root `README.md` to `samples/README.md`
- `ARCHITECTURE.md` refreshed for full `src/` assembly coverage + SMB
- `Phase N` labels replaced with concrete feature wording across all
  repo markdown
- `COM/` → `ext/samples/` (OPC Foundation native C++ sample servers)
- `ext/Include/` → `ext/inc/`; `COM/Include/` consolidated into `ext/inc/`
- Unused folders (OPC Batch Sample Code, OPC Security Sample Code) and
  unused proxy/stub DLLs deleted (~3 MB freed)

Test results unchanged: 1971 passed / 12 skipped / 0 failed.

## [1.0.0-rc.8] - 2026-05-27

Eighth release-candidate. **Spec-coverage completion + transport hardening.**
Closes the four largest open work-tracks from `docs/ROADMAP.md`:
remaining AE/HDA Windows-CCW gaps, SMB signing+encryption+ncacn_np
wire-up, legacy `IActivation` interop, and the threat-model hardening
items called out in `docs/security/THREAT_MODEL.md`.

### Added — AE Windows CCW completion (Track G)

- `OpcAeAreaBrowserCcw` + real `IOPCEventServer::CreateAreaBrowser` body
  per OPC AE 1.10 §5.3.2 — `ChangeBrowsePosition`, `BrowseOPCAreas`,
  `GetQualifiedAreaName`, `GetQualifiedSourceName` (cap-g2b).
- `OpcAeSubscriptionCcw` + real `CreateEventSubscription` body for the
  full ~12 methods of `IOPCEventSubscriptionMgt` per §5.4 (cap-g2a).
- Full `OPCEVENTFILTER` marshaling (event types, categories, severity
  range, area + source BSTR arrays) + `OpcAeEventSinkProxy` delivering
  `ONEVENTSTRUCT[]` to client `IOPCEventSink` per §5.5; Refresh
  fragment fan-out with `bLastRefresh` semantics (cap-g2c).
- Reusable `OpcEnumStringCcw` raw-vtable CCW for `IEnumString` returns.

### Added — HDA Windows CCW completion (Track G)

- `OpcHdaBrowserCcw` + real `IOPCHDA_Server::CreateBrowse` body per
  HDA 1.20 §5.4 (cap-g3a).
- `IOPCHDA_SyncRead` real bodies — `ReadRaw`, `ReadProcessed`,
  `ReadAttribute`, `ReadModified`, `ReadAtTime`, `ReadAnnotations`
  with `OPCHDA_ITEM[]` array + VARIANT + FILETIME marshaling (cap-g3b).
- `IOPCHDA_AsyncRead` real bodies with `OpcHdaCallbackProxy` firing
  `IOPCHDA_DataCallback::OnDataChange` (cap-g3c).
- `OpcHdaItemMarshaler` helper.

### Added — DA Windows CCW completion (Track G)

- `IEnumConnections` + `IEnumConnectionPoints` raw-vtable CCW
  infrastructure with `Next`/`Skip`/`Reset`/`Clone` independent
  cursors, snapshot semantics; wired into existing connection-point
  Advise/Unadvise machinery (cap-g4a + cap-g4b).
- `IOPCAsyncIO3::WriteVqt` real body with `OPCITEMVQT` marshaling,
  async dispatch, cancellation via `pdwCancelID`, and
  `OnWriteComplete` callback firing (cap-g5a).

### Added — SMB2 signing, encryption, and `ncacn_np` transport (Track H)

- `Smb2Signer` — HMAC-SHA256 for SMB 2.0.2/2.1; AES-128-CMAC for
  SMB 3.x with SMB3KDF-derived signing keys per MS-SMB2 §3.1.4.1 +
  SP800-108 §5.1; inbound verification with constant-time mismatch
  rejection (cap-h1).
- `Smb2Crypter` + `Smb2TransformHeader` — AES-128-CCM and AES-128-GCM
  per MS-SMB2 §3.1.4.3 + §2.2.41; encryption-key derivation per
  §3.1.5.2; `SMB2_ENCRYPTION_CAPABILITIES` negotiate context for
  SMB 3.1.1 cipher selection (cap-h2).
- `NcacnNpTransport` + `NcacnNpTransportFactory` — `IAsyncTransport`
  backed by `Smb2RpcTransportAdapter`; opens SMB2 connection,
  negotiates dialect + auth, opens IPC$ tree, opens the named pipe
  via SMB2 CREATE, routes RPC reads/writes through the adapter; the
  `RpcTransport` system now routes `protseq=ncacn_np` automatically
  (cap-h3).
- `IActivation::RemoteActivation` client over TCP for legacy XP /
  Server-2003 server interop, plus matching server-side dispatcher
  for accepting legacy clients with the same authentication policy
  as `IRemoteSCMActivator` (cap-h4 + cap-h5).
- `WinRegClient` over `ncacn_np` exercising the new transport against
  a Samba Linux container fixture (cap-h7).

### Added — wire-fixture infrastructure (Track H)

- PCAP replay harness (`PcapFileReader` + `Smb2PcapReplayer` +
  `PcapFixtureBase`) supporting both libpcap binary and human-readable
  hex-dump TXT golden fixtures (cap-h6).
- Samba container fixture under `docker/samba/` with `Dockerfile`,
  `smb.conf`, `docker-compose.yml`, and a CI workflow that brings
  the container up + runs the WINREG smoke (cap-h7).

### Added — XML-DA polish (Track I)

- `XmlDaErrorCode.IsSuccess()` + `XmlDaErrorCodes.IsSuccessResultId()`
  extensions so callers can distinguish the 3 OPC XML-DA success
  codes (`S_CLAMP`, `S_DATAQUEUEOVERFLOW`, `S_UNSUPPORTEDRATE`) from
  `E_*` faults without manual parsing (cap-g1a).
- `docs/cookbook/06-xmlda-client-flows.md` — concise XML-DA client
  cookbook covering GetStatus / Read / Write / Subscribe flows
  (cap-i1).
- `docs/CONFORMANCE.md#opc-xml-da-101` "Error and quality codes" section with
  full enum + quality bit tables verified against source (cap-i2).
- `docs/cookbook/07-enabling-packet-privacy.md` — opt-in privacy
  mode cookbook for DCOM/TCP, DCOM/SMB, XML-DA/HTTP (cap-j2).

### Security — threat-model hardening (Track J)

- Password lifetime + zeroization sweep across NTLMv2 / NTLMv1 / LM
  derivations + Kerberos auth info; `SensitiveBufferPool` helper for
  pooled buffers with `CryptographicOperations.ZeroMemory` on release;
  closes the `THREAT_MODEL.md` 6.2 PARTIAL on derived material
  (cap-j1).
- Direct `SIGNATURE_BLOCK` formation + mismatch tests against
  MS-NLMP §3.4.4 / §3.4.5 vectors — 9 cases covering tampered SeqNum,
  tampered checksum, wrong key, replay, UInt32 wrap (cap-g1d).
- `Ntlm1` negotiated-flag matrix tests against MS-NLMP §3.4.5 —
  ESS SIGN/SEAL × KEY_EXCH × 128/56/40 (cap-g1e).
- `RpcTransportQuotas` — tunable `MaxNdrPayloadSize` (16 MB),
  `MaxNtlmMessageSize` (64 KB-1), `MaxSmb2MessageSize` (128 KB-1)
  with property-bag accessors; bounded-input enforcement across
  NDR + NTLMSSP + SMB2 parsers with 68 new fuzz-test cases (cap-j3).
- Privacy-mode default review + per-transport opt-in documentation
  (cap-j2).

### Fixed — NTLMv1 spec violations uncovered while writing tests (cap-g1e)

- `Ntlm1` `ProtectionLevel` now correctly resolves to
  `CONNECT` (no protection) when neither `NTLMSSP_NEGOTIATE_SIGN` nor
  `NTLMSSP_NEGOTIATE_SEAL` is negotiated. Previously always returned
  `INTEGRITY` even with no protection negotiated.
- `NtlmAuthentication` now only generates + transmits the
  `EncryptedRandomSessionKey` when `NTLMSSP_NEGOTIATE_KEY_EXCH` is
  set; otherwise the user session key IS the exported session key
  per MS-NLMP §3.4.3. Previously always generated a secondary key.
- `Responses.cs` LM session hash now correctly extracts the first
  8 bytes of the MD5 digest. Previously passed an undersized buffer
  to `DoFinal()`.
- `NTLMKeyFactory` adds `NtlmFlags`-aware key-derivation overloads
  so the correct derivation function is selected per MS-NLMP §3.4.5.

### Documentation

- Per-spec coverage docs (`docs/CONFORMANCE.md`) refreshed across
  AE / HDA / DA / Common / XML-DA / Security for the rc.8 surface.
- `docs/architecture/smb-transport.md`: signing + encryption,
  ncacn_np wire-up, WINREG smoke, and PCAP fixtures all marked
  ✅ Landed.
- `docs/architecture/activation-transports.md`: legacy IActivation
  row now ✅ Client + server.
- `docs/security/THREAT_MODEL.md`: 6.2 (auth secrets), SR 4.1
  (confidentiality), SR 7.1 (DoS protection) all refreshed.
- `docs/security/NTLMSSP_AUDIT_GUIDE.md`: test coverage matrix
  refreshed; `NtlmSignatureBlockTests.cs` and
  `NtlmNegotiateFlagsTests.cs` are now direct (no longer indirect).
- `docs/cookbook/`: 2 new entries (06 XML-DA flows, 07 packet privacy).

### Test results

- 1971 passed / 12 skipped / 0 failed across 23 .NET test projects.
- 0 build warnings / 0 build errors.
- Cumulative gain from rc.7 (1253 passed): **+718 net new tests**.

## [1.0.0-rc.7] - 2026-05-27

Seventh release-candidate. **DCOM-over-IP between sample containers is
now functional.** Track E closes the long-standing "not functional yet"
caveat in `samples/README.docker.md` — `docker compose -f samples/docker-compose.yml up`
now exchanges real OPC calls over TCP between the daserver/aeserver/hdaserver
and their dialing clients.

### Added — public `TcpClientTransport` + `ConnectTcpAsync` helper (cap-e1)

- New `src/Opc.Classic.Dcom/Transport/TcpClientTransport.cs` — public
  `IAsyncTransport` over a connected `TcpClient`, with the static
  `ConnectAsync(host, port, ct)` convenience helper. Lifted from
  test-private code in `ManagedClientOverTransportTests`.
- New `DcomCallChannelFactory.ConnectTcpAsync(host, port, authContext, ct)`
  static helper that dials a host:port and wraps the transport in a
  `DcomCallChannel`. The most common adopter entry point for managed
  DCOM-over-IP clients.

### Changed — sample servers bind env port on `0.0.0.0` (cap-e2)

- `samples/Opc.Classic.Samples.DaServer`, `AeServer`, `HdaServer`,
  `CttServer` each read `OPC_CLASSIC_SAMPLE_PORT` (defaults
  DA=51300 / AE=51301 / HDA=51302 / CTT=51303) and bind
  `0.0.0.0:<port>` instead of the previous `127.0.0.1:0`.
- Optional `OPC_CLASSIC_LISTEN_ADDRESS` env var fully overrides the bind
  address (e.g. `192.168.1.10:51300`).
- **Behavior change**: `dotnet run --project samples/...DaServer` now
  binds to `0.0.0.0:51300` instead of `127.0.0.1` ephemeral. Documented
  in each sample's README.md as a release note.

### Added — sample clients dial TCP when env vars are set (cap-e3)

- `samples/Opc.Classic.Samples.DaClient`, `AeClient`, `HdaClient` each
  detect `OPC_CLASSIC_SERVER_HOST` + `OPC_CLASSIC_SERVER_PORT`. When
  both are set, the call channel becomes a `DcomCallChannel` built from
  `ConnectTcpAsync(...)`; the proxies wire to it transparently.
- When env vars are unset, the existing in-process
  `InMemoryCallChannel` + `Loopback*Server` path is preserved for
  local-dev (`dotnet run` outside Docker).
- Each client logs which path is active at startup (visible via
  `docker compose logs`).
- DA + AE clients gain new `DcomDaSubscription.cs` /
  `RemoteAeSubscription.cs` files; HDA client's `LoopbackHdaClient`
  refactored to accept either an in-process or proxy-supplied
  `IOpcHdaServer`.

### Added — public-API unit + smoke tests (cap-e5)

- `tests/Opc.Classic.Dcom.Tests/Tests/TcpClientTransportTests.cs` (+8 tests):
  argument validation, loopback round-trip through a `TcpListener`,
  `ConnectTcpAsync` returns a `DcomCallChannel`, `DisposeAsync`
  idempotency.

### Changed — `samples/README.docker.md` (cap-e4)

- "Not functional yet" caveat removed.
- Mermaid diagram updated: dashed "future DCOM-over-IP" arrows are now
  solid "DCOM-over-IP" connections. In-process channel boxes removed.
- Image table updated: notes column now states the real bind / dial
  addresses instead of "documentary".
- New optional overrides section documenting `OPC_CLASSIC_LISTEN_ADDRESS`
  and the no-env-vars local-dev fallback.
- New implementation-references section linking to the relevant source
  files.

### Tests

- Dcom: 123 (was 115, +8 from cap-e1/e5).
- Integration: 94 passing (existing
  `ManagedClientOverTransportTests` now routed through the new public
  surface; behavior unchanged).
- Solution-wide: all 17 test projects green; 0 build errors / 0
  warnings.

### Notes

- The sample compose deployment uses a `NoOpAuthContext` — no NTLM /
  Kerberos handshake. Documented in the updated README; production
  deployments would layer real auth on top of the same transport.
- The Windows CCW activation path (`OpcDaServerCcw`,
  `OpcDataCallbackProxy`) is unaffected by this track; CCW remains the
  SCM-launched-server path.

## [1.0.0-rc.6] - 2026-05-27

Sixth release-candidate. Closes the last three sandbox-feasible Track D
prep items (NTLMSSP wire fixtures + audit-prep guide + 1.0.0 release-prep
package). The remaining 3 todos all formally require external/
environment-dependent steps; see [docs/release-blockers.md](docs/release-blockers.md).

### Added — NTLMSSP wire-fixture replay (cap-d1)

- 3 binary fixtures in `tests/Opc.Classic.Dcom.Crypto.Tests/Fixtures/Ntlm/`
  capturing NEGOTIATE_MESSAGE (46 B), CHALLENGE_MESSAGE (104 B), and
  AUTHENTICATE_MESSAGE (232 B) bytes anchored to MS-NLMP §4.2.4 sample
  inputs.
- `NtlmHandshakeFixtureTests`: 5 replay tests covering encode-to-fixture +
  decode-from-fixture round-trips. Sandbox-feasible coverage for the
  encoder/decoder; the live-AD round-trip remains the
  `rw-e1-ntlmv2-realserver` gate.

### Added — NTLMSSP audit-prep guide (cap-d2)

- `docs/security/NTLMSSP_AUDIT_GUIDE.md` (572 lines, 10 sections):
  comprehensive enumeration of NTLMSSP code surface, cryptographic
  primitives in use (with RFC references), test coverage map,
  threat-model addendum, known limitations, and audit-scope
  recommendations. Enables external `rw-e4-ntlm-audit` engagement
  to start without further dev-side prep work.

### Added — 1.0.0 release-prep package (cap-d3)

- Consolidated `[1.0.0]` CHANGELOG section narrating the rc.1..rc.5
  delivery as a single coherent release with an "awaiting CTT smoke
  green" marker.
- `docs/release-blockers.md` (107 lines): one-page document naming the
  three remaining gates with owner + status + estimated-effort lines.
- Root `README.md` refreshed: version badge bump (0.6.0-alpha.1 →
  1.0.0-rc.5), honest "release candidate" status, hub-style links to
  subfolder READMEs, and the trademark disclaimer.

### Tests

- Crypto: 36 passing (was 31, +5).
- Solution-wide: all 17 test projects green; 0 build errors / 0 warnings.

### Remaining open todos (3, all environment-blocked)

- `release-100-tag` — blocked on CTT smoke green (Windows Docker host CI).
- `rw-e1-ntlmv2-realserver` — needs live Windows Server with domain creds.
- `rw-e4-ntlm-audit` — external third-party crypto/security audit.

See [docs/release-blockers.md](docs/release-blockers.md) for owner +
remediation details on each gate.

## [1.0.0-rc.5] - 2026-05-27

Fifth release-candidate. Closes the last 3 sandbox-feasible items
(smb-3 WINREG PCAP, docker-2 + docker-3 hand-rolled C MVPs). All
remaining 3 todos are external/environment-blocked (live Win Server,
third-party audit, CTT smoke on Windows Docker).

### Added — WINREG end-to-end coverage (smb-3-winreg-e2e)

- 5 binary PCAP fixtures in `tests/Opc.Classic.Dcom.Smb.Tests/Fixtures/Winreg/`
  capturing real MS-RPCE WINREG bind + OPNUM 2 (OpenLocalMachine) + OPNUM
  15 (BaseRegEnumKey) request/response bytes.
- `MockWinregServer`: in-memory RPC transport that replays the captured
  bytes and validates the inbound bytes match the canonical fixture.
- `WinregFixtureReplayTests`: 5 wire-replay tests covering OpenHKLM
  request/response and EnumKey response decode paths.
- SMB test project: 22 tests (was 17, +5).

### Added — C-built reference server + client Dockerfiles (docker-2 + docker-3)

- `docker/opc-c-server/build/opc-sample-server.cpp` (NEW, ~300 lines):
  hand-rolled MVP native OPC DA server implementing IOPCServer +
  IOPCCommon + IOPCGroupStateMgt + IOPCItemMgt + IOPCSyncIO. Exposes 3
  sample tags (Sin VT_R8, Square VT_BOOL, Random VT_I4) with a 100ms
  background-thread tag-update loop. Real bodies for activation,
  /RegServer + /UnregServer, group state, item add/validate/remove,
  sync read/write. E_NOTIMPL for clone / browse / async / subscriptions.
- `docker/opc-c-client/build/opc-test.cpp` (NEW, ~190 lines): hand-rolled
  MVP OPC DA client. CoCreateInstanceEx remote activation +
  IOPCItemMgt.AddItems + IOPCSyncIO.Read. Exit codes 2-6 identify the
  failing stage; HRESULT printed to stderr.
- Matching `.vcxproj` + `.sln` files for both targets.
- Dockerfiles for both containers updated to wire the MSBuild step
  (no longer placeholders).

### Status

Validation deferred to CI: the C++ artifacts can only be compiled +
containerized on a Windows host with Docker Desktop in Windows-container
mode. Source-level checks (cl.exe + standard COM headers) clean.

`dotnet build Opc.Classic.slnx`: 0 errors / 0 warnings. Solution-wide
all 17 .NET test projects green; DA 385 + SMB 22 + AE 86 + HDA 123 +
the rest.

### Remaining open todos (3, all environment-blocked)

- `release-100-tag` — blocked on CTT smoke green (Windows Docker host
  CI execution).
- `rw-e1-ntlmv2-realserver` — needs live Windows Server with domain
  credentials.
- `rw-e4-ntlm-audit` — external third-party crypto/security audit.

## [1.0.0-rc.4] - 2026-05-27

Fourth release-candidate. Track A (VARIANT marshaling + data path
completion) of the post-rc.3 plan is **complete**. Windows CCW now
carries data via real OPCITEMSTATE / VARIANT / OPCITEMVQT marshaling on
the inbound + outbound paths.

DA tests: **385 passing** (was 346 at rc.3, +39); solution-wide all
17 test projects green; build 0/0.

### Added — VARIANT + SAFEARRAY + BSTR marshaling foundation (cap-c1)

- `Opc.Classic.Da.Hosting.Windows.ComVariantMarshaler` — read/write the
  COM VARIANT 16/24-byte tagged-union struct in native memory. Covers
  every scalar VARTYPE (VT_I1-VT_UI8, VT_R4/R8, VT_DATE, VT_ERROR,
  VT_BOOL, VT_BSTR) plus 1-D SAFEARRAY (VT_ARRAY|*) with proper x86/x64
  descriptor alignment. BSTR alloc/free helpers; VariantClear-equivalent
  to release heap allocations.

### Added — IOPCSyncIO / IOPCSyncIO2 / IOPCAsyncIO2 real method bodies (cap-c2 + cap-c3)

- `IOPCSyncIO.Read` (slot 3): allocates OPCITEMSTATE[] OUT via
  CoTaskMemAlloc with per-item VARIANT marshaling.
- `IOPCSyncIO.Write` (slot 4): reads VARIANT[] IN.
- `IOPCSyncIO2.ReadMaxAge` (slot 5): separate VARIANT[] / WORD[] /
  FILETIME[] / HRESULT[] OUT arrays.
- `IOPCSyncIO2.WriteVqt` (slot 6): reads OPCITEMVQT[] via offset
  arithmetic.
- `IOPCAsyncIO2.Write` (slot 4): VARIANT[] IN + cancel ID OUT.
- Deferred (documented): `IOPCAsyncIO3.WriteVqt` remains E_NOTIMPL.

### Added — Outbound IOPCDataCallback payloads (cap-c4)

- `OpcDataCallbackProxy.OnDataChange` (vtable slot 3): allocates
  OPCHANDLE[] + VARIANT[] + WORD[] + FILETIME[] + HRESULT[] arrays via
  CoTaskMemAlloc, marshals payload fields through, invokes client's
  vtable slot, and frees all allocations (ClearVariant per-element for
  BSTR/SAFEARRAY cleanup) after the call returns.
- `OpcDataCallbackProxy.OnReadComplete` (slot 4): same shape.
- `OpcDataCallbackProxy.OnWriteComplete` (slot 5): handle + HRESULT
  array marshaling.

### Added — OPCITEMATTRIBUTES.vEUInfo real VARIANT marshaling (cap-c5)

- `OpcEnumOpcItemAttributesCcw.Next` writes real VARIANT vEUInfo (was
  VT_EMPTY). Enumerated item attributes now carry actual EU info
  (VT_BSTR for enum labels; VT_R8 SAFEARRAY for analog bounds) to COM
  clients.

### Added — IOpcDataCallbackSink abstraction (cap-c8)

- New `IOpcDataCallbackSink` interface unifies callback delivery between
  the cross-platform DCOM transport path (IOpcInterfaceRef-based) and
  the Windows SCM-activated CCW path. `OpcDataCallbackProxy` now
  implements the interface.
- New `OpcDaGroup.AdviseAsync(IOpcDataCallbackSink)` overload stores
  sinks in a parallel `_directSinks` dictionary; `UnadviseAsync` removes
  from both dictionaries.
- `TriggerDataChangeAsync` and `TriggerCancelCompleteAsync` fan-out
  iterates both dictionaries so Windows-CCW clients receive the same
  callbacks as cross-platform-transport clients.
- Windows CCW `IConnectionPoint::Advise` now also registers the proxy
  with `OpcDaGroup.AdviseAsync(IOpcDataCallbackSink)`; shared cookie
  space between `_directSinks` (managed) and `CcwSession.ScmSinks`
  (CCW lifecycle).

### Tests: +39 in DA (now 385)

- ComVariantMarshalerTests (+21): scalar round-trips for all VARTYPEs,
  BSTR round-trip with FreeBSTR, SAFEARRAY of I4/R8/BSTR, ClearVariant.
- OpcDaGroupCcwTests (+6): SyncIO Read returns OPCITEMSTATE matching
  managed group, Write through VT_I4/VT_BSTR, ReadMaxAge separate
  output arrays, WriteVqt timestamp override, AsyncIO2 Write cancel id.
- OpcDataCallbackProxyTests (+7): OnDataChange/OnReadComplete/
  OnWriteComplete payload marshaling against stub native CCWs.
- OpcEnumOpcItemAttributesCcwTests (+1): VT_BSTR vEUInfo round-trip.
- OpcDaGroupSubscriptionTests (+4): IOpcDataCallbackSink Advise overload,
  TriggerDataChange/CancelComplete fan-out to direct sinks,
  Unadvise removes from direct-sinks, null-sink guard.

### Known gaps still deferred to future releases

- IOPCAsyncIO3.WriteVqt: VQT marshaling (deferred from cap-c3).
- IOPCEventServer.CreateEventSubscription + EVENTFILTER marshaling
  (AE; not yet a cap-c-* todo).
- IOPCHDA_SyncRead/AsyncRead.ReadRaw/ReadProcessed: OPCHDA_ITEM[] +
  DATE[] + VARIANT[] marshaling (HDA; not yet a cap-c-* todo).
- OPC CTT smoke pass: still requires Windows Docker host.

## [1.0.0-rc.3] - 2026-05-27

Third release-candidate. Completes the Windows CCW DA path and the
Windows CCW AE/HDA per-method vtables.
Build green (0/0); DA tests **346 passing** (was 314 at rc.2, +32);
solution-wide all 17 test projects green.

### Added — Windows CCW DA per-interface vtables

- `OpcDaGroupCcw` now exposes nine tearoffs (was three at rc.2):
  IUnknown + IOPCGroupStateMgt(2) + IOPCItemMgt + IOPCSyncIO +
  IOPCSyncIO2 + IOPCAsyncIO2 + IOPCAsyncIO3 + IConnectionPoint +
  IConnectionPointContainer. Shared `CcwSession` holds the refcount and
  all tearoff pointers; QI for IID_IUnknown on any tearoff returns the
  canonical identity pointer (MS-DCOM §3.2.6). (cap-a3 + cap-a3b + cap-a4)
- New file `OpcDaGroupCcwMethods.cs` — `AddItems` + `ValidateItems` now
  have real OPCITEMDEF[] → OPCITEMRESULT[] marshaling (BSTR + DWORD +
  VARTYPE + BLOB ptr+size fields). `CloneGroup` allocates a fresh
  `OpcDaGroup` + copies items + wraps in a new CCW. `CreateEnumerator`
  wraps the existing managed `OpcDaItemAttributesEnumerator` in an
  `OpcEnumOpcItemAttributesCcw`. (cap-a1 + cap-a2)
- New file `OpcDaGroupCcwSyncIoMethods.cs` — IOPCSyncIO + IOPCSyncIO2
  vtables wired with E_NOTIMPL stubs documenting deferred VARIANT[]
  marshaling. QI succeeds; data path stays cross-platform-only.
- New file `OpcDaGroupCcwAsyncIoMethods.cs` — IOPCAsyncIO2 + IOPCAsyncIO3
  real bodies: Read, Refresh2, Cancel2, SetEnable, GetEnable, ReadMaxAge
  (DA 3.0), RefreshMaxAge (DA 3.0). Write/WriteVqt remain E_NOTIMPL
  (VARIANT marshaling).
- New file `OpcDaGroupCcwConnectionPointMethods.cs` — IConnectionPoint
  Advise/Unadvise wires a per-session `_scmSinks` `ConcurrentDictionary`
  + `OpcDataCallbackProxy`; CONNECT_E_NOCONNECTION on unknown cookies.
  FindConnectionPoint for IID_IOPCDataCallback returns the tearoff.
- New file `OpcEnumOpcItemAttributesCcw.cs` + companion methods file —
  single-tearoff CCW for IEnumOPCItemAttributes (Next/Skip/Reset/Clone).
  Real bodies for Skip/Reset/Clone; Next allocates OPCITEMATTRIBUTES[]
  with VT_EMPTY vEUInfo (full VARIANT marshaling deferred).
- New file `OpcDataCallbackProxy.cs` — server-side proxy class wrapping a
  client-supplied IUnknown for outbound IOPCDataCallback callbacks.
  OnCancelComplete real body (the simplest, no VARIANT marshaling);
  OnDataChange/OnReadComplete/OnWriteComplete signatures with
  TODO(cap-a8-followup) marshaling sketches. (cap-a8)

### Added — Windows CCW AE/HDA per-method vtables

- `OpcAeServerCcw` now multi-tearoff: IUnknown + IOPCEventServer +
  IOPCEventSubscriptionMgt. Real bodies: GetStatus (allocates
  OPCEVENTSERVERSTATUS_NATIVE), QueryAvailableFilters, subscription
  Refresh/CancelRefresh/GetState/SetState. E_NOTIMPL for
  CreateEventSubscription (interface ptr return) + complex EVENTFILTER
  marshaling. (cap-a7a + cap-a7b)
- `OpcHdaServerCcw` now multi-tearoff: IUnknown + IOPCHDA_Server +
  IOPCHDA_SyncRead + IOPCHDA_AsyncRead. Real bodies on IOPCHDA_Server:
  GetItemAttributes, GetAggregates, GetHistorianStatus, ValidateItemIDs,
  GetItemHandles, ReleaseItemHandles. E_NOTIMPL for CreateBrowse +
  Sync/AsyncRead methods (OPCHDA_ITEM/VARIANT marshaling deferred).
  (cap-a7c + cap-a7d)

### Tests: +32 in DA, +8 in Ae, +8 in Hda

- DA: 346 passing (was 314 at rc.2). New tests cover every new tearoff's
  QI / refcount / dispatch behaviour, plus stub-server integration for
  AddItems/ValidateItems/CloneGroup/CreateEnumerator round-trips.
- AE: 86 passing (was 78 at rc.2).
- HDA: 123 passing (was 115 at rc.2).
- Solution-wide: all 17 test projects green.

### Known gaps deferred to future releases

- Windows CCW IOPCSyncIO/IOPCSyncIO2/IOPCAsyncIO2 Write: VARIANT[] IN
  marshaling (BSTR + SAFEARRAY + 16/24-byte tagged union).
- Windows CCW IOPCSyncIO Read: VARIANT[] OUT marshaling for OPCITEMSTATE.
- Windows CCW IOPCDataCallback.OnDataChange/OnReadComplete/OnWriteComplete
  outbound: VARIANT[] + FILETIME[] + WORD[] OUT marshaling.
- Windows CCW IEnumOPCItemAttributes.Next vEUInfo: VARIANT marshaling
  (currently VT_EMPTY).
- Windows CCW IOPCEventServer.CreateEventSubscription + EVENTFILTER
  marshaling.
- Windows CCW IOPCHDA_SyncRead/AsyncRead.ReadRaw/ReadProcessed:
  OPCHDA_ITEM[] OUT marshaling (DATE[] + VARIANT[] + QUALITY[]).
- Cross-platform DCOM ↔ Windows-CCW sink unification: today
  `OpcDaGroup._sinks` (managed `IOpcInterfaceRef` for cross-platform) and
  the CCW's `_scmSinks` (`OpcDataCallbackProxy` for Windows SCM) are
  parallel. A future `IOpcDataCallbackSink` abstraction can unify them.
- OPC CTT smoke pass (`ocom-9`) — still requires Windows Docker host.

## [1.0.0-rc.2] - 2026-05-27

Second release-candidate. Substantial wire-server + Windows-CCW work since
rc.1; build green (0/0); DA tests 314 passing (was 247 at rc.1, +67 net new).
Solution-wide test sweep: all 17 test projects green.

### Added — wire-server: cross-platform DCOM listener

- `Opc.Classic.Dcom.Transport.PduCodec` + `OrpcEnvelope` — extracted shared
  RPC PDU framing primitives. (ocom-1a)
- `TcpServerEndpoint` + `RpcServerConnectionProcessor` + `OpcServerListener` —
  cross-platform `ncacn_ip_tcp` listener; binds, accepts, dispatches incoming
  DCOM PDUs to per-IID `IOpcServerDispatcher`. (ocom-1b)
- `OpcDaServerHost` / `OpcAeServerHost` / `OpcHdaServerHost` — replaced empty
  `AcceptConnectionsAsync` stubs with real listener wireup. (ocom-2)
- `Opc.Classic.Dcom.Transport.OpcObjectRegistry` — per-IPID per-object
  dispatcher routing so calls on a server-allocated IPID land at the right
  managed instance (groups, enumerators, subscriptions). (ocom-3a)

### Added — Windows CCW DA path

- `Opc.Classic.Da.Hosting.Windows.OpcDaServerCcw` — AOT-friendly raw COM-vtable
  CCW for `IOPCServer` with 12-slot vtable (IUnknown + 9 IOPCServer methods).
  Real method bodies for `GetErrorString`, `GetStatus`, `RemoveGroup`,
  `AddGroup`, `GetGroupByName`; `CreateGroupEnumerator` returns E_NOTIMPL.
  (ocom-6 + ocom-6b + ocom-6c + ocom-6d + cap-a5)
- `Opc.Classic.Da.Hosting.Windows.OpcDaGroupCcw` + `OpcDaGroupCcwMethods` —
  multi-tearoff CCW for OPC DA groups exposing IUnknown + IOPCGroupStateMgt(2)
  + IOPCItemMgt. Real method bodies for `GetState/SetState/SetName/SetKeepAlive
  /GetKeepAlive/RemoveItems/SetActiveState/SetClientHandles/SetDatatypes`.
  `CloneGroup/AddItems/ValidateItems/CreateEnumerator` return E_NOTIMPL
  pending OPCITEMDEF/VARIANT/SAFEARRAY marshaling (cap-a1/a2/a3 deferred).
  Release-to-zero properly frees all tearoffs, vtables, and GCHandle.
  (ocom-6d + rev-1 + rev-2)

### Added — OPC DA group managed surface

- `Opc.Classic.Da.Hosting.OpcDaGroup` — full managed group implementing:
  `IOPCGroupStateMgt` + `IOPCGroupStateMgt2` (state, keep-alive),
  `IOPCItemMgt` (items + enumerator), `IOPCSyncIO` + `IOPCSyncIO2`
  (Read/Write/MaxAge), `IOPCAsyncIO2` + `IOPCAsyncIO3` (async with cancel),
  `IConnectionPoint` + `IConnectionPointContainer` (data-callback
  subscriptions), `IOPCItemDeadbandMgt` (per-item deadband),
  `IOPCItemSamplingMgt` (per-item sampling rate + buffering).
  (ocom-3c + ocom-3d + ocom-7b + ocom-8 + ocom-8b + ocom-8d + cap-b4 + cap-b5)
- `OpcDaItem` — gains `PercentDeadband`, `SamplingRate`, `BufferEnabled`
  per-item state for DA 3.0 management interfaces. (cap-b4 + cap-b5)
- `OpcDaItemAttributesEnumerator` — stateful per-cursor enumerator for
  `IEnumOPCItemAttributes`. Snapshot-at-create semantics per OPC DA 2.05a
  §4.4.7.2. (ocom-8d)
- `TriggerDataChangeAsync` + `TriggerCancelCompleteAsync` — caller-supplied
  outbound callback fan-out for `IOPCDataCallback.OnDataChange` /
  `OnCancelComplete`. Honors SetEnable. (ocom-7b + rev-11)

### Added — DA address space + DA 3.0 interfaces

- `IOpcAddressSpace` abstraction + `FlatHierarchicalNamespace` +
  `InMemoryAddressSpace` — hierarchical browse model with empty-flat fallback.
  (cap-b1)
- `DefaultBrowseServerAddressSpace` — DA 2.x browse backed by an
  `IOpcAddressSpace`. Supports ChangeBrowsePosition (UP/DOWN/TO), GetItemID,
  per-server browse position tracking. (cap-b1)
- `DefaultBrowse` — DA 3.0 unified browse returning OPCBROWSEELEMENT records
  with proper branch/item flags and maxElementsReturned pagination. (cap-b3)
- `DefaultItemProperties` + `OpcStandardProperties` + `IOpcItemPropertyProvider`
  — DA 2.x item properties publishing the OPC-standard ID set (1-8:
  CanonicalDataType / Value / Quality / Timestamp / AccessRights / ScanRate
  / EuType / EuInfo) with pluggable per-item value provider. (cap-b2)
- `DefaultItemDeadbandMgt` + `DefaultItemSamplingMgt` — DA 3.0 default impls
  returning OPC_E_DEADBANDNOTSET / OPC_E_RATENOTSET / OPC_E_NOBUFFERING when
  no per-item override is configured. (cap-b4 + cap-b5)

### Added — Windows CCW AE / HDA parity

- `Opc.Classic.Ae.Hosting.Windows.OpcAeServerCcw` — IUnknown-identity CCW for
  AE servers (parity with DA SCM activation). Per-method `IOPCEventServer`
  vtable deferred. (rev-13)
- `Opc.Classic.Hda.Hosting.Windows.OpcHdaServerCcw` — IUnknown-identity CCW
  for HDA servers. Per-method `IOPCHDA_Server` vtable deferred. (rev-13)

### Added — Tests (+67 net new in DA)

- 314 DA tests (was 247 at rc.1). New test files:
  `OpcDaServerListenerTests`, `OpcObjectRegistryTests`, `OpcDaServerDispatcherTests`
  (and 14 more per-interface dispatcher test files), `OpcDaGroupItemMgtTests`,
  `OpcDaGroupSubscriptionTests`, `OpcDaGroupAsyncIoTests`,
  `OpcDaItemAttributesEnumeratorTests`, `OpcDaGroupConcurrencyTests`,
  `OpcDaGroupItemStateTests`, `OpcAddressSpaceTests`, `DefaultDaInterfacesTests`,
  Windows-only `OpcDaServerCcwTests` + `OpcDaGroupCcwTests` +
  `OpcAeServerCcwTests` + `OpcHdaServerCcwTests`.

### Fixed — code-review findings (16 / 16)

- **CRITICAL**: `OpcDaGroupCcw` exposed only IUnknown — real DCOM clients
  saw E_NOINTERFACE on QI for IOPC* IIDs. Now multi-tearoff with real
  vtables. (rev-1)
- **HIGH**: CCW Release-to-zero leaked GCHandle + native memory.
  Now properly frees on refcount → 0. (rev-2)
- **HIGH**: CCW used generic E_FAIL where OPC_E_* codes apply.
  ArgumentException → E_INVALIDARG mapping added across all CCW catch
  blocks. (rev-4)
- **HIGH**: Missing `IOPCBrowseServerAddressSpace` + `IOPCItemProperties`
  managed impls. Now Default* classes auto-wired by OpcDaServerHost. (rev-5)
- **MED**: `IOPCItemDeadbandMgt`, `IOPCItemSamplingMgt`, `IOPCBrowse` had
  no host impl. Now wired via OpcDaGroup state + DefaultBrowse. (rev-9)
- **MED**: `OpcDaServerCcw.AddGroup` could AV on null OUT params.
  Now validates `phServerGroup` / `pRevisedUpdateRate` / `ppUnk`. (rev-6)
- **MED**: `GetGroupByName` returned E_NOTIMPL; `dwGroupCount` unwired.
  Now resolves via `IOpcDaServer.ResolveGroupByNameAsync`. (rev-7 + rev-8)
- **MED**: `Cancel2Async` was a no-op; no OnCancelComplete delivery.
  Now records last cancel id; `TriggerCancelCompleteAsync` mirrors
  TriggerDataChangeAsync for sink fan-out. (rev-11)
- **MED**: No concurrency tests for OpcDaGroup item collection.
  New `OpcDaGroupConcurrencyTests` covers enumerator + read under
  concurrent AddItems/RemoveItems load. (rev-10)
- **MED**: `TriggerDataChangeAsync` short-circuit on `!_callbacksEnabled`
  untested. (rev-12)
- **MED**: AE + HDA had no CCW parity. Now `OpcAeServerCcw` +
  `OpcHdaServerCcw`. (rev-13)
- **LOW**: `UnadviseAsync` silently succeeded on unknown cookie. Now
  throws CONNECT_E_NOCONNECTION (0x80040200) per COM convention. (rev-14)
- **LOW**: Enumerator snapshot semantics undocumented. (rev-15)
- **LOW**: `IDataObject` advise IID unhandled. (rev-16)
- All 16 review findings closed.

### Changed

- `IOpcDaServer` gains default-implemented `ResolveGroupAsync(handle)` +
  `ResolveGroupByNameAsync(name)` returning null. Implementations that
  track groups in-process (the reference `CttDaServer`) override these so
  Windows CCW and cross-platform DCOM paths share the same lookup.
- `CttDaServer.CreateGroup` registers 11 per-group dispatchers (added
  IOPCItemDeadbandMgt + IOPCItemSamplingMgt).
- `OpcDaServerHost` auto-detects `IOPCBrowse` / `IOPCBrowseServerAddressSpace`
  / `IOPCItemProperties` / `IOPCItemDeadbandMgt` / `IOPCItemSamplingMgt` on
  the user's `IOpcDaServer` and falls back to default impls when absent.

### Known gaps deferred to future releases

- Windows CCW: `IOPCSyncIO` + `IOPCAsyncIO2/3` per-method vtables
  (VARIANT marshaling).
- Windows CCW: `IConnectionPoint` per-method vtable (client-IUnknown
  sink proxy).
- Windows CCW: OPCITEMDEF array marshaling for `AddItems` / `ValidateItems`.
- Windows CCW: Interface-pointer marshaling for `CloneGroup` /
  `CreateEnumerator` / outbound `IOPCDataCallback` proxy.
- AE + HDA: per-method vtables beyond IUnknown identity.
- OPC CTT smoke pass (`ocom-9`) — requires Windows Docker host.

## [1.0.0-rc.1] - 2026-05-26

Release-candidate cut for `1.0.0`. Build green (0/0); tests green (1418+ passing, 24 skipped, 0 failed); Windows COM registration plumbing for the CTT integration in place.

### Added

- `Opc.Classic.Hosting.Windows.WindowsComRegistration` — Windows COM registration shim that writes the full out-of-process server tree (`HKCR\CLSID\{x}` with `LocalServer32`, `AppID` as a named value, `ProgID`, `VersionIndependentProgID`, `Implemented Categories`, `Component Categories\{catid}\409` for LCID 1033) under HKLM or HKCU, in both `Registry32` and `Registry64` views by default.
- `Opc.Classic.Hosting.OpcComponentCategories` — the nine standard OPC Classic CATIDs (DA 1.0 / 2.0 / 3.0, AE 1.0, HDA 1.0, XML-DA 1.0, DX 1.0, Batch 1.0 / 2.0) sourced from the OPC Foundation IDL headers vendored in `ext/inc/`.
- `Opc.Classic.Hosting.Windows.ComClassObjectRegistrar` — AOT-friendly raw COM-vtable bridge that registers a managed `IClassFactory` with `ole32!CoRegisterClassObject` so Windows COM SCM can launch the sample EXE via `LocalServer32`.
- `samples/Opc.Classic.Samples.CttServer` — `--register` / `--unregister` / `--registry-hive=hklm|hkcu` / `--registry-view=32|64|both` / `-Embedding` (case-insensitive) CLI for OPC CTT integration.
- `tests/Opc.Classic.Hosting.Tests/Windows/WindowsComRegistrationTests.cs` — 7 HKCU-isolated, parallel-serialized tests covering every documented registry shape including an explicit AppID-as-named-value-not-subkey guard.
- `ext/private/ctt/` — six OPC Compliance Test Tool MSIs (~13 MB total) vendored into the repository for the CI workflow.
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
