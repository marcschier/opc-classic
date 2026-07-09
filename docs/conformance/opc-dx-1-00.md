# OPC DX 1.00 conformance review

**Spec:** `opc-classic-docs/OPC-DX-1.00.md` (OPC Data eXchange Specification 1.0, March 5, 2003).

**Scope:** DX Database address-space model, `IOPCConfiguration` DCOM configuration services, DX source-server / connection / response structures, DX status structures, DX-specific HRESULTs, component category identifiers, DCOM IDL mapping, Web Services mapping, persistence, and the runtime source-to-target data-transfer model.

**Implementing assemblies:** `Opc.Classic.Dx`, `Opc.Classic.Core`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| DX component category + `IOPCConfiguration` IID | App. B.1.4 | ✅ `OpcGuids.CATID_OPCDXServer10`, `OpcGuids.IID_IOPCConfiguration`, `[OpcInterface]` | ✅ | conformant |
| `IOPCConfiguration` source-server services | §5.2.1, App. B.1.4 | ✅ hand-written client proxy | ✅ partial | conformant for client calls |
| `IOPCConfiguration` DXConnection query/add/update/modify/copy/reset services | §5.2.2 - §5.2.3, App. B.1.4 | ✅ hand-written client proxy | ✅ partial | conformant for implemented calls |
| `IOPCConfiguration::DeleteDXConnections` | §5.2.2.5, App. B.1.4 | ✅ accepts DXConnection masks and returns mask errors plus `DXGeneralResponse` | ✅ proxy + fixture tests | conformant — closed in commit `<future>` |
| DX parameter structures (`ItemIdentifier`, `DXConnection`, `SourceServer`, `DXGeneralResponse`, `IdentifiedResult`) | §5.1 | ✅ records + NDR codecs | ✅ | conformant |
| DX status structures (`ServerStatus`, `DXConnectionStatus`, `DXSourceServerStatus`, `DXQuality`) | §4.2 - §4.4 | ✅ records + NDR codecs | ✅ | conformant as data model/codecs |
| DX enums and masks | §4.2.2, §4.3.2.19, §4.4.1.6, App. B.1.3 | ✅ `DxEnums`, `ConnectionState`, `OverrideState` | ✅ | conformant |
| DX HRESULT table | §5.1.7 | ✅ `OpcDxError` constants | ✅ | conformant |
| DX namespace / reserved branch helpers | §4, App. B.1.4 names | ✅ `DxNamespace` | ✅ | conformant as helpers |
| DX Database DA branch exposure | §4 | ❌ no generic DX server address-space runtime | n/a | waived deferred server runtime (unverified — Phase 2 deep-validation will close) |
| Persistence / DirtyFlag save loop | §5.3 | ❌ no generic DX persistence runtime | n/a | waived deferred server runtime (unverified — Phase 2 deep-validation will close) |
| Runtime source-server bridge, subscriptions, queues, conversion, target-write truth table | §6 | ❌ no generic DX data-transfer state machine | n/a | waived deferred server runtime (unverified — Phase 2 deep-validation will close) |
| Web Services / XML-DA mapping | Appendix A | ❌ no DX XML-DA service endpoint | n/a | waived lower-priority transport mapping (unverified — Phase 2 deep-validation will close) |

---

## 1 Surface-by-surface coverage matrix

### 1.1 Component category and interface identifiers (spec App. B.1.4)

| Identifier | Spec value / role | Source | Tests |
|---|---|---|---|
| `CATID_OPCDXServer10` | `A0C85BB8-4161-4FD6-8655-BB584601C9E0` | `src/Opc.Classic.Core/OpcGuids.cs`, `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs` | `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs` |
| `IOPCConfiguration` | `C130D281-F4AA-4779-8846-C2C4CB444F2A` | `src/Opc.Classic.Core/OpcGuids.cs`, `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs` | `tests/Opc.Classic.Dx.Tests/DcomInterfaceIdTests.cs` |

The Phase 0 interface inventory also flags `IOPCBrowseServerAddressSpace::GetItemID` (§4 and §5.1.5) and `IOPCCommon` (Appendices A/B) as cross-spec dependencies. They are not DX-specific interfaces; DX uses them through DA browsing/error-text conventions when a DX server chooses to expose the runtime database over DA.

### 1.2 `IOPCConfiguration` source-server services (spec §5.2.1 / App. B.1.4)

| Service | Opnum | Spec request/response | Source proxy | Tests |
|---|---:|---|---|---|
| `GetServers` / `QuerySourceServersAsync` | 3 | no request parameters; returns `SourceServer[0:N]` | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs`, `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs` | `tests/Opc.Classic.Dx.Tests/Dcom/IOPCDxProxyTests.cs` |
| `AddServers` / `AddSourceServersAsync` | 4 | `SourceServer[1:N]` -> `DXGeneralResponse` | same | proxy coverage by shared source-server array/general-response codecs |
| `ModifyServers` / `ModifySourceServersAsync` | 5 | `ServerDefinitions[1:N]` -> `DXGeneralResponse` | same | codec coverage |
| `DeleteServers` / `DeleteSourceServersAsync` | 6 | `ItemIdentifier[0:N]` -> `DXGeneralResponse` | same | codec coverage |
| `CopyDefaultServerAttributes` | 7 | `ConfigToStatus`, `ItemIdentifier[0:N]` -> `DXGeneralResponse` | same | codec coverage |

`DxSourceServer` carries the spec attributes `Name`, `Description`, `ServerType`, `ServerURL`, `ItemIdentifier`, `Version`, `DefaultSourceServerConnected`, `dwMask`, and reserved DWORD. The codec registry includes `OPCDX_SOURCE_SERVER` and `OPCDX_SOURCE_SERVER_ARRAY`.

### 1.3 `IOPCConfiguration` DXConnection services (spec §5.2.2 / App. B.1.4)

| Service | Opnum | Spec request/response | Source proxy | Tests | Status |
|---|---:|---|---|---|---|
| `QueryDXConnections` | 8 | `BrowsePath`, `DXConnectionMasks[1:N]`, `Recursive` -> either mask errors or `DXConnection[0:N]` | `IOPCDxInterfaces.cs`, `IOPCConfigurationClientProxy.cs` | `IOPCDxProxyTests.cs` | ✅ |
| `AddDXConnections` | 9 | `DXConnection[1:N]` -> `DXGeneralResponse` | same | `IOPCDxProxyTests.cs` | ✅ |
| `UpdateDXConnections` | 10 | `BrowsePath`, masks, `Recursive`, definition -> mask errors or `DXGeneralResponse` | same | `IOPCDxProxyTests.cs` | ✅ |
| `ModifyDXConnections` | 11 | `DXConnectionDefinition[1:N]` -> `DXGeneralResponse` | same | codec coverage | ✅ |
| `DeleteDXConnections` | 12 | `BrowsePath`, `DXConnectionMasks[0:M]`, `Recursive` -> mask errors and/or `DXGeneralResponse` | same | `IOPCDxProxyTests.cs`, `DxNdrCodecTests.cs` | ✅ |
| `CopyDefaultDXConnectionAttributes` | 13 | `ConfigToStatus`, `BrowsePath`, `Recursive`, masks -> mask errors or `DXGeneralResponse` | same | codec coverage | ✅ |
| `ResetConfiguration` | 14 | old configuration version -> new configuration version | same | `IOPCDxProxyTests.cs` | ✅ |

`DxConnection` covers the configurable attributes in §5.1.2, including browse paths, connection name, description, keyword, default runtime controls, override/substitute values, target/source item identifiers, queue size, update rate, deadband, vendor data, and the native mask. Tests round-trip masks, variants, arrays, and representative status fields.

### 1.4 DX parameter and status codecs (spec §4, §5.1, App. B.1.4)

| Structure / area | Source | Tests | Status |
|---|---|---|---|
| `OpcDxItemIdentifier` | `src/Opc.Classic.Dx/DxItemIdentifier.cs`, `src/Opc.Classic.Dx/Ndr/NdrOpcDxCodecs.cs` | `tests/Opc.Classic.Dx.Tests/DxItemIdentifierNdrCodecTests.cs`, `DxItemIdentifierAdditionalTests.cs` | ✅ |
| `OpcDxIdentifiedResult` | `src/Opc.Classic.Dx/DxGeneralResponse.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs` | ✅ |
| `OpcDxGeneralResponse` | `src/Opc.Classic.Dx/DxGeneralResponse.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs`, `IOPCDxProxyTests.cs` | ✅ |
| `OpcDxSourceServer` | `src/Opc.Classic.Dx/DxSourceServer.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs`, `DxTypesTests.cs` | ✅ |
| `OpcDxConnection` | `src/Opc.Classic.Dx/DxConnection.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs`, `DxTypesTests.cs` | ✅ |
| `DXServerStatus` | `src/Opc.Classic.Dx/DxStatusRecords.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs` | ✅ data model/codecs |
| `DXConnectionStatus` | `DxStatusRecords.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs` | ✅ data model/codecs |
| `DXSourceServerStatus` | `DxStatusRecords.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs` | ✅ data model/codecs |
| `DXQuality` / `OPCError` | `DxStatusRecords.cs`, `NdrOpcDxCodecs.cs` | `DxNdrCodecTests.cs` | ✅ |
| Codec registry | `NdrOpcDxCodecRegistry.RegisteredCodecNames` | `DxNdrCodecTests.cs` | ✅ 16 registered entries |

### 1.5 Enums, masks, HRESULTs, and namespace names

| Surface | Spec § | Source | Tests | Status |
|---|---|---|---|---|
| Source-server types | §4.2.8 / §4.4.1.3 | `src/Opc.Classic.Dx/DxEnums.cs` | `DxTypesTests.cs` | ✅ |
| Server state | §4.2.2 | `DxEnums.cs` | `DxNdrCodecTests.cs` | ✅ |
| DXConnection state | §4.3.2.19.1 | `DxEnums.cs`, `ConnectionState.cs` | `DxNdrCodecTests.cs`, `DxTypesTests.cs` | ✅ |
| Source-server connect status | §4.4.1.6.1 | `DxEnums.cs` | `DxNdrCodecTests.cs` | ✅ |
| Quality/limit status | §4.3.2.19.4 | `DxEnums.cs` | `DxNdrCodecTests.cs` | ✅ |
| Optional-field masks | §5.1.2, §5.1.8, App. B.1.3 | `DxEnums.cs`, `DxConnection.cs`, `DxSourceServer.cs` | `DxNdrCodecTests.cs` | ✅ |
| DX HRESULTs | §5.1.7 | `src/Opc.Classic.Dx/OpcDxErrors.cs` | `DxNdrCodecTests.cs` | ✅ |
| Reserved DX branch names and paths | §4, App. B.1.4 names | `src/Opc.Classic.Dx/DxNamespace.cs` | `DxNdrCodecTests.cs` | ✅ helpers |

### 1.6 DX Database and runtime model (spec §4, §5.3, §6)

The spec requires a DX server to expose a reserved DA subtree rooted at `DX`, including `ServerStatus`, `DXConnectionsRoot`, and `SourceServers`; to persist source servers and connections; and to run a live bridge from source OPC DA/XML-DA servers into local target items. `Opc.Classic.Dx` deliberately stops at configuration-client and codec support today. The model objects and namespace helpers are present, but no generic server runtime populates the DA address space, runs the source-server connection lifecycle, owns source queues, executes conversion, or applies the target-update truth table.

This is classified as a documented waiver rather than a hard conformance gap for the current package scope because the existing aggregate conformance review defines DX server runtime/DA bridge, persistence, and live data transfer as deferred-by-design product work.

---

## 2 Normative-clause checklist

OPC-DX-1.00 contains 1 Phase 0 `SHALL` entry in `opc-dx-1-00-clauses.csv`. The inventory entry is the OPC Foundation license/liability disclaimer and does not impose an implementation behavior on `Opc.Classic`.

| § | Clause | Status | Evidence |
|---|---|---|---|
| front matter | "IN NO EVENT SHALL THE OPC FOUNDATION, ITS MEMBERS, OR ANY THIRD PARTY BE LIABLE" | n/a | legal disclaimer in the spec front matter; no product conformance action |

Implementation-affecting DX requirements are therefore tracked by surface and behavior rather than by the Phase 0 clause CSV:

| Requirement area | Spec § | Status | Evidence |
|---|---|---|---|
| DCOM IDL masks identify optional fields in DX structures | App. B.1.4 | ✅ implemented | `DxConnection.Mask`, `DxSourceServer.Mask`, `NdrOpcDxCodecs.cs`, `DxNdrCodecTests.cs` |
| Configuration services update/return the parameter shapes defined by §5.1/§5.2 | §5.1 - §5.2 | ✅ except delete hard gap | `IOPCDxInterfaces.cs`, `IOPCConfigurationClientProxy.cs`, `IOPCDxProxyTests.cs` |
| DX server request semantics update ConfigurationVersion/DirtyFlag and runtime status | §5.2 - §5.3 | ⚠️ waived | requires generic DX server database/persistence runtime |
| DX runtime source connections, subscriptions, queues, conversion, and target writes | §6 | ⚠️ waived | deferred server-runtime state machine |

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 DX Database DA address-space exposure is not a generic runtime

Spec §4 requires every DX server to expose the reserved DA subtree rooted at `DX`, with standardized `ServerStatus`, `DXConnectionsRoot`, and `SourceServers` branches. `DxNamespace` provides canonical names and path helpers, and the status/configuration records/codecs exist, but `Opc.Classic.Dx` does not ship a generic DX server that populates those branches through DA browsing and item access. Status: **WAIVED** as deferred DX server-runtime work.

#### 3.1.2 Persistence and DirtyFlag save loop are not implemented generically

Spec §5.3 requires a saved DX Database, DirtyFlag handling, save within one minute, `E_PERSISTING` behavior while saving, startup restore, and shutdown save. `OpcDxError` contains the persistence HRESULTs and status records contain `DirtyFlag`, but no persistence engine is included. Status: **WAIVED** as deferred DX server-runtime work.

#### 3.1.3 Live source-to-target transfer state machine is not implemented generically

Spec §6 defines startup restore, source-server connection/recovery, DA/XML-DA subscriptions, queue flushing/high-water accounting, conversion, runtime controls, and target-write truth-table behavior. The current implementation provides data shapes and constants only. Status: **WAIVED** because the aggregate conformance review explicitly classifies the DA bridge and live transfer loop as future runtime product work, not a client-proxy/codec blocker.

#### 3.1.4 Appendix A Web Services / XML-DA mapping is not implemented

Appendix A maps DX services to Web Services/XML-DA. The package has DCOM configuration-client coverage but no DX XML-DA service endpoint. Status: **WAIVED** as a lower-priority transport mapping until a generic DX server runtime exists.

### 3.2 Closed hard gaps

#### 3.2.1 Closed: `IOPCConfiguration::DeleteDXConnections` DCOM projection

Spec §5.2.2.5 and App. B.1.4 define `DeleteDXConnections` as `BrowsePath`, `DXConnectionMasks[0:M]`, `Recursive`, returning per-mask errors and a `DXGeneralResponse`. Status: **CLOSED** in commit `<future>`. The projection now exposes `DeleteDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive)`, encodes `NdrOpcDxConnectionArrayCodec`, returns `DxDeleteConnectionsResult`, and has direct proxy/fixture coverage for opnum 12 payload and response decoding.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC DX 1.00](../CONFORMANCE.md#opc-dx-100)
- Architecture overview: [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md)
- Roadmap: [`docs/ROADMAP.md`](../ROADMAP.md)
- DA conformance for address-space/browse dependencies: [`docs/CONFORMANCE.md` § OPC DA 2.05a](../CONFORMANCE.md#opc-da-205a), [`docs/CONFORMANCE.md` § OPC DA 3.00](../CONFORMANCE.md#opc-da-300)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-DX-1.00.md` (OPC Data eXchange Specification Version 1.0, March 5, 2003).

Phase 0 inventory:

- `files/conformance/inventory/opc-dx-1-00-headings.csv` (193 entries)
- `files/conformance/inventory/opc-dx-1-00-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-dx-1-00-interfaces.csv` (3 interface references + 1 method reference)
