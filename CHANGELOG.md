# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

Third release-candidate. Completes Phase 1 (Windows CCW DA path) and
Phase 2 (Windows CCW AE/HDA per-method vtables) of the post-rc.2 plan.
Build green (0/0); DA tests **346 passing** (was 314 at rc.2, +32);
solution-wide all 17 test projects green.

### Added — Windows CCW DA per-interface vtables (Phase 1)

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

### Added — Windows CCW AE/HDA per-method vtables (Phase 2)

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
