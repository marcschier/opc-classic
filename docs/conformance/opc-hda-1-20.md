# OPC HDA 1.20 conformance review

**Spec:** `opc-classic-docs/OPC-HDA-1.20.md` (OPC Historical Data Access Specification Version 1.20, January 30, 2004; errata 2.0-7.0 applied inline).

**Scope:** HDA custom COM/DCOM server, browser, synchronous read/update/annotation, asynchronous read/update/annotation, playback, callback, connection-point, data-structure, aggregate, quality, HRESULT, and server-registration surfaces. The spec explicitly leaves historian storage, aggregate algorithms, update policy, and source-system architecture to server implementations.

**Implementing assemblies:** `Opc.Classic.Hda`, `Opc.Classic.Core`, `Opc.Classic.Hosting.Windows`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCHDA_Server` (7 methods) | §4.4.1 | ✅ source-generated projection + dispatcher; Windows CCW implements metadata, handles, validation, and `CreateBrowse` | ✅ | conformant |
| `IOPCHDA_Browser` (4 methods) | §4.4.2 | ✅ source-generated proxy; Windows browser CCW implements enumeration/cursor/item-id methods | ✅ | conformant |
| `IOPCHDA_SyncRead` (5 methods) | §4.4.3 | ✅ source-generated projection + dispatcher; Windows CCW marshals `OPCHDA_ITEM`, `OPCHDA_MODIFIEDITEM`, and `OPCHDA_ATTRIBUTE` arrays | ✅ | conformant |
| `IOPCHDA_SyncUpdate` (6 methods) | §4.4.4 | ✅ source-generated projection + dispatcher; Windows CCW delegates update/delete operations to dispatcher/server policy | ✅ | conformant |
| `IOPCHDA_SyncAnnotations` (3 methods) | §4.4.5 | ✅ source-generated projection + dispatcher; Windows CCW covers read and insert annotation arrays | ✅ | conformant |
| `IOPCHDA_AsyncRead` (8 methods) | §4.5.1 | ✅ source-generated projection + dispatcher; Windows CCW returns cancel IDs/errors and fires callbacks | ✅ | conformant |
| `IOPCHDA_AsyncUpdate` (7 methods) | §4.5.2 | ✅ source-generated projection + dispatcher; Windows CCW returns cancel IDs/errors and fires `OnUpdateComplete` | ✅ | conformant |
| `IOPCHDA_AsyncAnnotations` (4 methods) | §4.5.3 | ✅ source-generated projection + dispatcher; Windows CCW covers read/insert/cancel callback flow | ✅ | conformant |
| `IOPCHDA_Playback` (3 methods) | §4.6.1 | ✅ source-generated projection + dispatcher; Windows CCW streams `OnPlayback` and supports cancel | ✅ | conformant |
| `IOPCHDA_DataCallback` (9 methods) | §4.8.1 | ✅ source-generated callback projection; Windows `OpcHdaCallbackProxy` invokes all callback slots | ✅ | conformant |
| `IConnectionPointContainer` / `IConnectionPoint` | §4.7 | ⚠️ `FindConnectionPoint(IID_IOPCHDA_DataCallback)` and Advise/Unadvise work; `EnumConnectionPoints` and `EnumConnections` return `E_NOTIMPL` | ✅ for implemented paths | hard gap (unverified — Phase 2 deep-validation will close) |
| `IOPCCommon` / shutdown carry-over | §3.1.1, §2.4 | ⚠️ HDA-specific host/CCW evidence not found; common conformance is covered cross-spec for DA/AE paths | ⚠️ | hard gap (unverified — Phase 2 deep-validation will close) |
| HDA structures/codecs | §5.3 | ✅ `OPCHDA_TIME`, `OPCHDA_ITEM`, `OPCHDA_MODIFIEDITEM`, `OPCHDA_ATTRIBUTE`, `OPCHDA_ANNOTATION` codecs/marshalers | ✅ | conformant |
| HDA HRESULTs | App. B | ✅ `OpcHdaErrors` constants | ✅ | conformant |
| Aggregate identifiers / semantics | §2.9, §5.3.3 | ⚠️ common aggregate enum/helper coverage appears partial; algorithm semantics remain server-policy | ⚠️ | hard/soft split — see §3 |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCHDA_Server` (spec §4.4.1)

7 wire-level methods.

| Method | Opnum | Source proxy / dispatcher | Windows CCW / host | Tests |
|---|---:|---|---|---|
| `GetItemAttributes` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaServerCcwMethods.cs` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs`, `tests/Opc.Classic.Hda.Tests/Hosting/OpcHdaServerDispatcherRoundTripAdditionalTests.cs` |
| `GetAggregates` | 4 | same | same | same |
| `GetHistorianStatus` | 5 | same | same | `tests/Opc.Classic.Hda.Tests/Hosting/OpcHdaServerDispatcherRoundTripAdditionalTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwMethodsTests.cs` |
| `GetItemHandles` | 6 | same | same | same |
| `ReleaseItemHandles` | 7 | same | same | same |
| `ValidateItemIDs` | 8 | same | same | `tests/Opc.Classic.Hda.Tests/Hosting/OpcHdaServerDispatcherTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwMethodsTests.cs` |
| `CreateBrowse` | 9 | ⚠️ Windows CCW sub-object pattern; root source-generated interface currently lists server opnums 3-8 only | `OpcHdaServerCcwMethods.CreateBrowse`, `OpcHdaBrowserCcw` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaBrowserCcwTests.cs` |

The managed host (`src/Opc.Classic.Hda/Hosting/OpcHdaServerHost.cs`) registers generated dispatchers for HDA server/read/update/annotation/playback interfaces when the server implementation supports them. Windows native clients use `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaServerCcw.cs` tearoffs.

### 1.2 `IOPCHDA_Browser` (spec §4.4.2)

| Method | Opnum | Source proxy | Windows CCW | Tests |
|---|---:|---|---|---|
| `GetEnum` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaBrowserCcw.cs` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaBrowserCcwTests.cs` |
| `ChangeBrowsePosition` | 4 | same | same | same |
| `GetItemID` | 5 | same | same | same |
| `GetBranchPosition` | 6 | same | same | same |

`CreateBrowse` validates filter arrays and returns a per-instance browser CCW. The browser stores branch position and delegates filtering/navigation to `IOpcHdaServerDispatcher`.

### 1.3 `IOPCHDA_SyncRead` (spec §4.4.3)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `ReadRaw` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `OpcHdaServerCcwMethods.SyncReadRaw` | `tests/Opc.Classic.Hda.Tests/Dcom/IOPCHdaProxyTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |
| `ReadProcessed` | 4 | same | `OpcHdaServerCcwMethods.SyncReadProcessed` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |
| `ReadAtTime` | 5 | same | `OpcHdaServerCcwMethods.SyncReadAtTime` | same |
| `ReadModified` | 6 | same | `OpcHdaServerCcwMethods.SyncReadModified` | same |
| `ReadAttribute` | 7 | same | `OpcHdaServerCcwMethods.SyncReadAttribute` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |

### 1.4 `IOPCHDA_SyncUpdate` (spec §4.4.4)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `QueryCapabilities` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaSyncUpdateCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaSyncUpdateCcwTests.cs` |
| `Insert` | 4 | same | same | same; `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs` |
| `Replace` | 5 | same | same | same |
| `InsertReplace` | 6 | same | same | same |
| `DeleteRaw` | 7 | same | same | same |
| `DeleteAtTime` | 8 | same | same | same |

### 1.5 `IOPCHDA_SyncAnnotations` (spec §4.4.5)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `QueryCapabilities` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `OpcHdaServerCcwMethods.SyncAnnotationsQueryCapabilities` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |
| `Read` | 4 | same | `OpcHdaServerCcwMethods.SyncReadAnnotations` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |
| `Insert` | 5 | same | `OpcHdaServerCcwMethods.SyncAnnotationsInsert` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |

### 1.6 `IOPCHDA_AsyncRead` (spec §4.5.1)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `ReadRaw` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `OpcHdaServerCcwMethods.AsyncReadRaw` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |
| `AdviseRaw` | 4 | same | `OpcHdaServerCcwMethods.AsyncAdviseRaw` | `tests/Opc.Classic.Hda.Tests/Dcom/IOPCHdaProxyTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |
| `ReadProcessed` | 5 | same | `OpcHdaServerCcwMethods.AsyncReadProcessed` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |
| `AdviseProcessed` | 6 | same | `OpcHdaServerCcwMethods.AsyncAdviseProcessed` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |
| `ReadAtTime` | 7 | same | `OpcHdaServerCcwMethods.AsyncReadAtTime` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs` |
| `ReadModified` | 8 | same | `OpcHdaServerCcwMethods.AsyncReadModified` | same |
| `ReadAttribute` | 9 | same | `OpcHdaServerCcwMethods.AsyncReadAttribute` | same |
| `Cancel` | 10 | same | `OpcHdaServerCcwMethods.AsyncCancel` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwReadTests.cs` |

### 1.7 `IOPCHDA_AsyncUpdate` (spec §4.5.2)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `QueryCapabilities` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaAsyncUpdateCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaAsyncUpdateCcwTests.cs` |
| `Insert` | 4 | same | same | same; `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs` |
| `Replace` | 5 | same | same | same |
| `InsertReplace` | 6 | same | same | same |
| `DeleteRaw` | 7 | same | same | same |
| `DeleteAtTime` | 8 | same | same | same |
| `Cancel` | 9 | same | same | same |

### 1.8 `IOPCHDA_AsyncAnnotations` (spec §4.5.3)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `QueryCapabilities` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `OpcHdaServerCcwMethods.AsyncAnnotationsQueryCapabilities` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |
| `Read` | 4 | same | `OpcHdaServerCcwMethods.AsyncReadAnnotations` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs` |
| `Insert` | 5 | same | `OpcHdaServerCcwMethods.AsyncAnnotationsInsert` | same; `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |
| `Cancel` | 6 | same | `OpcHdaServerCcwMethods.AsyncCancel` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |

### 1.9 `IOPCHDA_Playback` (spec §4.6.1)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---:|---|---|---|
| `ReadRawWithUpdate` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaPlaybackCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaPlaybackCcwTests.cs` |
| `ReadProcessedWithUpdate` | 4 | same | same | same |
| `Cancel` | 5 | same | same | same |

### 1.10 `IOPCHDA_DataCallback` (spec §4.8.1)

| Method | Opnum | Source projection | Windows callback proxy | Tests |
|---|---:|---|---|---|
| `OnDataChange` | 3 | `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaCallbackProxy.cs` | `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` |
| `OnReadComplete` | 4 | same | same | same |
| `OnReadModifiedComplete` | 5 | same | same | same |
| `OnReadAttributeComplete` | 6 | same | same | same |
| `OnReadAnnotations` | 7 | same | same | same |
| `OnInsertAnnotations` | 8 | same | same | same |
| `OnPlayback` | 9 | same | same | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaPlaybackCcwTests.cs` |
| `OnUpdateComplete` | 10 | same | same | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaAsyncUpdateCcwTests.cs` |
| `OnCancelComplete` | 11 | same | same | same |

### 1.11 Connection points (spec §4.7)

| Method | Source | Tests | Outcome |
|---|---|---|---|
| `IConnectionPointContainer::FindConnectionPoint` | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaServerCcwConnectionPointMethods.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Hda/OpcHdaServerCcwAnnotationAdviseTests.cs` | ✅ returns `IID_IOPCHDA_DataCallback` connection point |
| `IConnectionPointContainer::EnumConnectionPoints` | same | same | ❌ returns `E_NOTIMPL` (unverified — Phase 2 deep-validation will close) |
| `IConnectionPoint::Advise` / `Unadvise` | same | same | ✅ registers and removes `IOPCHDA_DataCallback` sink |
| `IConnectionPoint::EnumConnections` | same | same | ❌ returns `E_NOTIMPL` (unverified — Phase 2 deep-validation will close) |

### 1.12 Data types, structures, constants, and registration

| Surface | Spec § | Source | Tests | Outcome |
|---|---|---|---|---|
| Interface IDs + HDA CATID | §6 / IDL appendix | `src/Opc.Classic.Core/OpcGuids.cs` | `tests/Opc.Classic.Hda.Tests/DcomInterfaceIdTests.cs` | conformant |
| `OPCHDA_TIME` | §5.3.4 | `src/Opc.Classic.Hda/OpcHdaTime.cs`, `src/Opc.Classic.Hda/Ndr/NdrOpcHdaTimeCodec.cs` | `tests/Opc.Classic.Hda.Tests/NdrOpcHdaTimeCodecTests.cs`, `tests/Opc.Classic.Hda.Tests/HdaFileTimeFuzzTests.cs` | codec conformant; relative-time evaluation is server-policy |
| `OPCHDA_ITEM` | §5.3.1 | `src/Opc.Classic.Hda/OpcHdaItem.cs`, `src/Opc.Classic.Hda/Ndr/NdrOpcHdaItemCodec.cs` | `tests/Opc.Classic.Hda.Tests/NdrOpcHdaItemCodecTests.cs` | conformant |
| `OPCHDA_MODIFIEDITEM` | §5.3.6 | `src/Opc.Classic.Hda/OpcHdaModifiedItem.cs`, `src/Opc.Classic.Hda/Ndr/NdrOpcHdaModifiedItemCodec.cs` | `tests/Opc.Classic.Hda.Tests/NdrOpcHdaModifiedItemCodecTests.cs` | conformant |
| `OPCHDA_ATTRIBUTE` | §5.3.5 | `src/Opc.Classic.Hda/OpcHdaAttribute.cs`, `src/Opc.Classic.Hda/Ndr/NdrOpcHdaAttributeCodec.cs` | `tests/Opc.Classic.Hda.Tests/NdrOpcHdaAttributeCodecTests.cs` | conformant |
| `OPCHDA_ANNOTATION` | §5.3.7 | `src/Opc.Classic.Hda/OpcHdaAnnotation.cs`, `src/Opc.Classic.Hda/Ndr/NdrOpcHdaAnnotationCodec.cs` | `tests/Opc.Classic.Hda.Tests/NdrOpcHdaAnnotationCodecTests.cs` | conformant |
| HDA HRESULTs | App. B | `src/Opc.Classic.Hda/OpcHdaErrors.cs` | `tests/Opc.Classic.Hda.Tests/OpcHdaErrorsTests.cs`, `tests/Opc.Classic.Hda.Tests/OpcHdaResultIdTests.cs` | conformant |
| HDA aggregate helper enum | §5.3.3 | `src/Opc.Classic.Hda/HdaAggregate.cs` | `tests/Opc.Classic.Hda.Tests/InterfaceContractTests.cs` | partial hard gap (unverified — Phase 2 deep-validation will close) |

---

## 2 Normative-clause checklist

OPC-HDA-1.20 contains 1 normative MUST/SHALL entry in the Phase 0 inventory, but that entry is a legal disclaimer line rather than an implementation requirement:

| § | Clause | Status | Evidence |
|---|---|---|---|
| n/a | "IN NO EVENT SHALL THE OPC FOUNDATION, ITS MEMBERS, OR ANY THIRD PARTY BE" | n/a | Legal/disclaimer text only; no implementation action. |

Implementation-relevant normative behavior is therefore covered by interface-shape, method, callback, structure, and HRESULT fidelity in §1.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Historian aggregate semantics, relative-time evaluation, update policy, annotation persistence, and storage durability

The spec defines required wire shapes and the expected meaning of historian operations, but the physical historian, aggregate algorithm choices, timestamp resolution, update authorization, annotation persistence, and durable storage model are intentionally server-specific. Opc.Classic provides the DCOM projection, codecs, host plumbing, and CCW marshalers. Individual HDA servers must implement their own policy and storage semantics. Status: **WAIVED as server-policy**, but Phase 2 should deep-validate any reference/sample historian.

### 3.2 Hard gaps

#### 3.2.1 `IConnectionPointContainer::EnumConnectionPoints` does not return an enumerator

Spec §4.7.1.1 says OPCHDA servers must return an enumerator that includes `IOPCHDA_DataCallback`. The Windows HDA CCW currently returns `E_NOTIMPL` for `EnumConnectionPoints`, although `FindConnectionPoint(IID_IOPCHDA_DataCallback)` works. Status: **OPEN** (unverified — Phase 2 deep-validation will close).

#### 3.2.2 `IConnectionPoint::EnumConnections` does not enumerate active callback sinks

The OLE connection-point surface is exposed and `Advise`/`Unadvise` work, but `EnumConnections` returns `E_NOTIMPL`. Status: **OPEN** (unverified — Phase 2 deep-validation will close).

#### 3.2.3 HDA-specific `IOPCCommon` / shutdown carry-over evidence is incomplete

The HDA spec model includes `IOPCCommon`, and client shutdown callback behavior is discussed in §2.4. Current HDA host/CCW evidence reviewed here did not show an HDA root tearoff/dispatcher for `IOPCCommon` or `IOPCShutdown` carry-over, although OPC Common has its own cross-spec conformance review. Status: **OPEN** (unverified — Phase 2 deep-validation will close).

#### 3.2.4 HDA aggregate helper enum appears incomplete

Spec §5.3.3 lists aggregate identifiers through `OPCHDA_ANNOTATIONS`, while `src/Opc.Classic.Hda/HdaAggregate.cs` currently exposes helper values only through `Duration = 19`. The DCOM interfaces still accept raw aggregate IDs as `int[]`, so wire compatibility is not blocked, but the managed helper surface is incomplete. Status: **OPEN** (unverified — Phase 2 deep-validation will close).

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC HDA 1.20](../CONFORMANCE.md#opc-hda-120)
- OPC Common carry-over review: [`docs/conformance/opc-common-1-10.md`](opc-common-1-10.md)
- Managed HDA host: `src/Opc.Classic.Hda/Hosting/OpcHdaServerHost.cs`
- HDA DCOM projections: `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs`
- Windows HDA CCW: `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaServerCcw.cs`
- HDA tests: `tests/Opc.Classic.Hda.Tests/`, `tests/Opc.Classic.Hosting.Windows.Tests/Hda/`
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-HDA-1.20.md` (OPC Historical Data Access Specification Version 1.20, January 30, 2004; errata 2.0-7.0 applied inline).

Phase 0 inventory:

- `files/conformance/inventory/opc-hda-1-20-headings.csv` (174 entries)
- `files/conformance/inventory/opc-hda-1-20-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-hda-1-20-interfaces.csv` (17 interface + 53 method references)
