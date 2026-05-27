# OPC HDA 1.20 — Spec Coverage Review

**Spec**: OPC Historical Data Access Specification Version 1.20 (January 30, 2004)
**Implementation**: `src/Opc.Classic.Hda/`
**Review target**: `1.0.0-rc.7`

---

## Summary

**Interfaces**: 10 projected interfaces (9 HDA service interfaces plus `IOPCHDA_DataCallback`)
**Methods**: 56/56 declared
**Structs**: 5/5 codecs registered
**Aggregates**: 27 standard aggregate IDs declared
**Quality flags**: HDA-specific flags defined
**Error constants**: HDA Appendix C constants present

**Overall compliance**: **Full DCOM declaration/proxy/dispatcher coverage; Windows CCW now covers browser creation plus HDA sync/async read payloads; update/playback CCWs remain partial**.

Earlier claims that HDA was fully production-ready through all paths are too broad. The DCOM projection is complete, and the Windows CCW now has native `OPCHDA_ITEM[]`/`OPCHDA_ATTRIBUTE[]`/`OPCHDA_MODIFIEDITEM[]`/`OPCHDA_ANNOTATION[]` marshaling for history reads; update and playback CCW paths remain future work.

---

## Implementation Status

### Interface Coverage

All HDA service and callback interfaces are declared with opnums matching the spec in `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs:21-309`.

| Interface | Methods | Cross-platform DCOM status | Windows CCW status |
|---|---:|---|---|
| `IOPCHDA_Server` | 6/6 | ✅ generated proxy + dispatcher | ✅ real `GetItemAttributes`, `GetAggregates`, `GetHistorianStatus`, `ValidateItemIDs`, `GetItemHandles`, `ReleaseItemHandles`, and `CreateBrowse` |
| `IOPCHDA_Browser` | 4/4 | ✅ generated proxy declaration | ✅ `CreateBrowse` returns a raw-vtable browser CCW with `GetEnum`, `ChangeBrowsePosition`, `GetItemID`, and `GetBranchPosition` |
| `IOPCHDA_SyncRead` | 5/5 | ✅ generated proxy + dispatcher | ✅ `ReadRaw`, `ReadProcessed`, `ReadAtTime`, `ReadModified`, and `ReadAttribute` marshal native HDA arrays |
| `IOPCHDA_SyncUpdate` | 6/6 | ✅ generated proxy + dispatcher | not yet a full CCW runtime path |
| `IOPCHDA_SyncAnnotations` | 3/3 | ✅ generated proxy + dispatcher | ✅ `Read` marshals native annotation arrays; query capabilities wired when implemented by the server; insert remains future work |
| `IOPCHDA_AsyncRead` | 8/8 | ✅ generated proxy + dispatcher | ✅ read methods return cancel IDs/errors and fire `IOPCHDA_DataCallback`; advise methods remain future work |
| `IOPCHDA_AsyncUpdate` | 7/7 | ✅ generated proxy + dispatcher | not yet a full CCW runtime path |
| `IOPCHDA_AsyncAnnotations` | 4/4 | ✅ generated proxy + dispatcher | ✅ `Read` returns cancel IDs/errors and fires `OnReadAnnotations`; insert remains future work |
| `IOPCHDA_Playback` | 3/3 | ✅ generated proxy + dispatcher | not yet a full CCW runtime path |
| `IOPCHDA_DataCallback` | 9/9 | ✅ callback projection | native callback lifecycle tests still recommended |

**Key CCW sources**:

- Real `IOPCHDA_Server` methods and `CreateBrowse`: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaServerCcwMethods.cs`
- Sync read and annotation-read CCW bodies: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaServerCcwMethods.cs`
- Async read callback bridge: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaCallbackProxy.cs`
- Native HDA array/VARIANT marshaling: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaItemMarshaler.cs`

---

## Data Structure Coverage

| Structure | Status |
|---|---|
| `OPCHDA_TIME` | ✅ codec |
| `OPCHDA_ITEM` | ✅ codec |
| `OPCHDA_MODIFIEDITEM` | ✅ codec |
| `OPCHDA_ANNOTATION` | ✅ codec |
| `OPCHDA_ATTRIBUTE` | ✅ codec |

These codecs support the generated DCOM path. The Windows CCW now also has native memory allocation and OAUT `VARIANT` array marshaling for read results.

---

## Aggregate, Quality, and Error Coverage

- Standard aggregate IDs are represented by `OpcHdaAggregateId` / aggregate support types.
- HDA quality flags are represented in `OpcHdaQuality`.
- HDA Appendix C error constants are present in `src/Opc.Classic.Hda/OpcHdaErrors.cs`.

The older recommendation to “add HDA error constants” is stale.

---

## Gaps in Implementation

### HIGH

#### 1. Windows CCW update/playback bodies

HDA sync/async update, insert-annotation, advise, and playback methods are projected and tested through DCOM, but the Windows CCW still needs native method bodies for those non-read paths.

### MEDIUM

#### 2. Server-side aggregate/update/annotation semantics

The interfaces and codecs are present, but aggregate calculations, update policies, annotation storage, relative time parsing, and playback behavior are server-specific. A sample/reference HDA server would make those semantics testable.

---

## Test Coverage

| Test File | Scope |
|---|---|
| `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaServerCcwMethodsTests.cs:1-524` | Windows CCW implemented methods and deferred read/browser behavior |
| `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs:1-455` | DCOM proxy round trips for HDA methods |
| `tests/Opc.Classic.Hda.Tests/Hosting/OpcHdaServerDispatcherTests.cs:1-110` | Server dispatcher routing |

Recommended additions:

1. Native CCW read tests once OPCHDA item/VARIANT allocation is implemented.
2. Browser CCW tests once `CreateBrowse` returns a browser object.
3. Aggregate calculation, annotation, update, playback, and relative time parser tests for any sample/reference HDA server.

---

## Compliance Checklist (§3 Compliance)

| Requirement | Current status | Notes |
|---|---|---|
| `IOPCHDA_Server` | ✅ DCOM, ✅ partial CCW real bodies | `CreateBrowse` remains deferred in CCW |
| `IOPCHDA_SyncRead` | ✅ DCOM, ⚠️ CCW deferred bodies | Managed DCOM declaration complete |
| Async callback pattern | ✅ DCOM projection | Native lifecycle integration tests recommended |
| `IOPCHDA_Browser` or DA browse | ✅ DCOM declaration | Native browser CCW pending |
| Optional update/annotation/playback interfaces | ✅ DCOM declarations | Server-specific semantics pending |

---

## Conclusion

HDA should be described as declaration- and codec-complete for the managed DCOM path, with targeted Windows CCW gaps. The remaining work is to implement native browser and history-read marshaling plus server-specific behavior tests, not to add missing HDA interface declarations.
