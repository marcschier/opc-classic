# OPC Batch 2.00 — Spec Coverage Review

**Spec**: OPC Batch Custom Interface Specification Version 2.0 (July 19, 2001)  
**Implementation**: `src/Opc.Classic.Batch/`  
**Review date**: 2025-01-XX  
**Reviewer**: Spec coverage analysis agent

---

## Summary

**Interfaces**: 2/4 covered (50%)  
**Methods**: 6/11 declared (55%)  
**Structs**: 2/2 codecs registered (100%)  
**Overall compliance**: **PARTIAL** — Core enumeration and query methods missing; `IOPCBatchServer2` partially declared but missing `CreateFilteredEnumerator` body.

### Severity Breakdown

- **BLOCKER**: 1 (CreateEnumerator returns interface pointer — deferred)  
- **HIGH**: 3 (IOPCEnumerationSets methods all missing)  
- **MEDIUM**: 1 (CreateFilteredEnumerator deferred)  
- **LOW**: 1 (IEnumOPCBatchSummary::Clone deferred)

---

## Gaps in Implementation

### BLOCKER

#### 1. `IOPCBatchServer::CreateEnumerator` — Interface Pointer Return

**Spec**: §5.2.4.2 (`opcbc.idl` line 5055–5058)

```idl
HRESULT CreateEnumerator(
  [in]  REFIID      riid,
  [out, iid_is(riid)] LPUNKNOWN * ppUnk
);
```

**Status**: Deferred (comment: "returns an interface pointer and remains deferred")  
**Source**: `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:29`  
**Impact**: Clients cannot enumerate batches via `IEnumOPCBatchSummary` through `IOPCBatchServer`. Without this, batch discovery is incomplete.  
**Severity**: **BLOCKER** — Required for §3.2.4 Batch List enumeration workflow (spec §5.2.6 depends on this).

---

### HIGH

#### 2. `IOPCEnumerationSets::QueryEnumerationSets` — Missing

**Spec**: §5.2.7.1 (`opcbc.idl` line 5123–5127)

```idl
HRESULT QueryEnumerationSets(
  [out]                              DWORD * pdwCount,
  [out, size_is(,*pdwCount)]         DWORD ** ppdwEnumSetId,
  [out, string, size_is(,*pdwCount)] LPWSTR ** ppszEnumSetName
);
```

**Status**: Missing — interface `IOPCEnumerationSets` declared (line 66–81) but method missing.  
**Source**: `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:66–81`  
**Impact**: Clients cannot discover available enumeration sets (§3.4 Enumeration Concept). Enumeration sets 0–6 are standard (PHYS, PROC, STATE, MODE, PARAM, MR_PROC, RE_USE), plus vendor-specific 100+. Without this, clients cannot translate enumeration values to localized strings (e.g., state "OPCB_STATE_RUNNING" → "Running").  
**Severity**: **HIGH** — Required for multi-vendor interoperability and runtime translation of enumerations.

**Workaround**: Clients may hard-code standard enumeration sets 0–6 but cannot discover vendor extensions.

---

#### 3. `IOPCEnumerationSets::QueryEnumeration` — Missing

**Spec**: §5.2.7.2 (`opcbc.idl` line 5129–5133)

```idl
HRESULT QueryEnumeration(
  [in]  DWORD   dwEnumSetId,
  [in]  DWORD   dwEnumValue,
  [out, string] LPWSTR * pszEnumName
);
```

**Status**: Missing — method declared at line 72–74 but body missing.  
**Source**: `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:72–74`  
**Impact**: Clients cannot translate enumeration (set, value) pairs to localized strings. Example: `QueryEnumeration(OPCB_ENUM_STATE=2, OPCB_STATE_RUNNING=1)` → "Running" (localized).  
**Severity**: **HIGH** — Core enumeration lookup required by §3.4, §3.8 Typical Use.

---

#### 4. `IOPCEnumerationSets::QueryEnumerationList` — Missing

**Spec**: §5.2.7.3 (`opcbc.idl` line 5135–5140)

```idl
HRESULT QueryEnumerationList(
  [in]  DWORD   dwEnumSetId,
  [out] DWORD * pdwCount,
  [out, size_is(,*pdwCount)]         DWORD ** ppdwEnumValue,
  [out, string, size_is(,*pdwCount)] LPWSTR ** ppszEnumName
);
```

**Status**: Missing — method declared at line 76–78 but body missing (comment: "has two parallel output arrays and waits for an explicit record type").  
**Source**: `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:76–78`  
**Impact**: Clients cannot retrieve all enumerations for a set at once (bulk lookup). Example use: startup caching of all state names for set `OPCB_ENUM_STATE` (§3.8).  
**Severity**: **HIGH** — Optional pattern for efficient bulk enumeration translation, but commonly used by clients to avoid repeated single-value queries.

---

### MEDIUM

#### 5. `IOPCBatchServer2::CreateFilteredEnumerator` — Deferred

**Spec**: §5.2.5.1 (`opcbc.idl` line 5069–5075)

```idl
HRESULT CreateFilteredEnumerator(
  [in]         REFIID                   riid,
  [in, ptr]    OPCBATCHSUMMARYFILTER *  pFilter,
  [in, string] LPCWSTR                  szModel,
  [out, iid_is(riid)] LPUNKNOWN *      ppUnk
);
```

**Status**: Deferred (comment: "returns interface pointer and remains deferred")  
**Source**: `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:32–38`  
**Impact**: Clients cannot filter batch lists by ID substring, size range, state, mode, or time windows. Spec §3.2.4 Batch List, §6.1.2 OPCBATCHSUMMARYFILTER. `IOPCBatchServer2` is marked optional in spec but is the only way to query archived batches (`OPCBBatchArchiveModel`).  
**Severity**: **MEDIUM** — Optional interface (§5.2.5) but commonly implemented. Loss: filtered enumeration for `OPCBBatchModel` + `OPCBBatchArchiveModel`.

---

### LOW

#### 6. `IEnumOPCBatchSummary::Clone` — Deferred

**Spec**: §5.2.6.4 (`opcbc.idl` line 5106–5108)

```idl
HRESULT Clone(
  [out] IEnumOPCBatchSummary ** ppEnumBatchSummary
);
```

**Status**: Deferred (comment: "returns IEnumOPCBatchSummary and remains deferred")  
**Source**: `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:63`  
**Impact**: Enumerators cannot be cloned. Standard COM `IEnum*` pattern (MS documentation). Clients can workaround by re-calling `CreateEnumerator` (once that is implemented).  
**Severity**: **LOW** — Convenience; rarely used in practice.

---

## Coverage Gaps (Integration Tests Recommended)

### 1. **Batch Namespace Models** — No server-side implementation visible

**Spec**: §3.2.1 Batch Namespace Models, §3.2.2 Browsing the OPC Batch Namespace  
**Well-known item IDs**:

- `OPCBPhysicalModel`  
- `OPCBMasterRecipeModel`  
- `OPCBBatchModel`  
- `OPCBBatchArchiveModel`  
- `OPCBBatchIDList`

**Gaps**:

- No server-side browse implementation for these models detected in `src/Opc.Classic.Batch/`.
- No `IOPCBrowseServerAddressSpace` implementation visible (required per §3.5 Compliance).
- No property support (§3.3 OPC Batch Properties, 79 properties ID 400–478).

**Recommendation**:

- Server-side namespace construction belongs in a separate runtime or sample server (e.g., `samples/Opc.Classic.Samples.BatchServer`).
- Integration tests should validate:
  - Delimiter retrieval via `GetDelimiter`
  - Browsing `OPCBBatchModel` → batches → unit procedures → operations → phases
  - Property queries via `IOPCItemProperties::QueryAvailableProperties`
  - Dynamic namespace (batches added/removed, browse position invalidation)

---

### 2. **Enumeration Sets 0–6** — No constants or types visible

**Spec**: §3.4 Enumeration Concept, Table 12 – Enumerations  
**Standard enumeration sets**:

| Set ID | Name                    | Values                                                                                                                     |
|--------|-------------------------|----------------------------------------------------------------------------------------------------------------------------|
| 0      | `OPCB_ENUM_PHYS`        | 0=ENTERPRISE, 1=SITE, 2=AREA, 3=PROCESSCELL, 4=UNIT, 5=EQUIPMENTMODULE, 6=CONTROLMODULE, 7=EPE                           |
| 1      | `OPCB_ENUM_PROC`        | 0=PROCEDURE, 1=UNITPROCEDURE, 2=OPERATION, 3=PHASE, 4=PARAMETER_COLLECTION, 5=PARAMETER, 6=RESULT_COLLECTION, 7=RESULT, 8=BATCH, 9=CAMPAIGN, 10=MASTER_RECIPE |
| 2      | `OPCB_ENUM_STATE`       | 0=IDLE, 1=RUNNING, 2=COMPLETE, 3=PAUSING, 4=PAUSED, 5=HOLDING, 6=HELD, 7=RESTARTING, 8=STOPPING, 9=STOPPED, 10=ABORTING, 11=ABORTED, 12=UNKNOWN |
| 3      | `OPCB_ENUM_MODE`        | 0=AUTOMATIC, 1=SEMIAUTOMATIC, 2=MANUAL, 3=UNKNOWN                                                                         |
| 4      | `OPCB_ENUM_PARAM`       | 0=PROCESSINPUT, 1=PROCESSPARAMETER, 2=PROCESSOUTPUT                                                                       |
| 5      | `OPCB_ENUM_MR_PROC`     | 0=PROCEDURE, 1=UNITPROCEDURE, 2=OPERATION, 3=PHASE, 4=PARAMETER_COLLECTION, 5=PARAMETER, 6=RESULT_COLLECTION, 7=RESULT   |
| 6      | `OPCB_ENUM_RE_USE`      | 0=INVALID, 1=LINKED, 2=EMBEDDED, 3=COPIED                                                                                 |

**Recommendation**: Add managed enums for these sets in `src/Opc.Classic.Batch/` for server/client reference (e.g., `OpcBatchPhysicalLevel.cs`, `OpcBatchState.cs`, `OpcBatchMode.cs`). Not a blocker for DCOM projection (NDR + interfaces) but helpful for server implementors.

---

### 3. **Property IDs 400–478** — No property constants visible

**Spec**: §3.3 OPC Batch Properties, Table 11  
**Sample required properties** (batch level):

- 400 `ID` (VT_BSTR) — R/R/R/R across all models
- 401 `Value` (varies) — O/O/O/O
- 410 `OPCBBatchModelLevel` (VT_I4) — R/R/-- (batch/RPE only)
- 433 `ControlRecipeID` (VT_BSTR) — --/O/--/--
- 441 `ExecutionState` (VT_BSTR) — --/R/R/--
- 446 `ActualStartTime` (VT_DATE) — --/R/O/--

**Recommendation**: Add property ID constants (e.g., `OpcBatchPropertyId.cs`) if server implementors are expected to use this library. Not required for proxy generation.

---

### 4. **`OPCBATCHSUMMARY` and `OPCBATCHSUMMARYFILTER` Codecs** — ✅ COVERED

**Spec**: §6.1.1 OPCBATCHSUMMARY, §6.1.2 OPCBATCHSUMMARYFILTER (`opcbc.idl` line 5007–5041)  
**Implementation**:

- `src/Opc.Classic.Batch/OpcBatchSummary.cs` — record type
- `src/Opc.Classic.Batch/OpcBatchSummaryFilter.cs` — record type
- `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryCodec.cs` — NDR encoder/decoder
- `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryFilterCodec.cs` — NDR encoder/decoder

**Status**: ✅ **COMPLETE** — All 10 fields of `OPCBATCHSUMMARY` and 13 fields of `OPCBATCHSUMMARYFILTER` mapped. FILETIME ↔ DateTimeOffset conversion correct (1601-01-01 epoch).

---

### 5. **Error Codes** — No Batch-specific error handling visible

**Spec**: Appendix B (`OPCBatchError.h`)  
**Batch-specific HRESULT**:

- `OPCB_E_NOT_MEANINGFUL = 0xC0040300L` — "The data is not meaningful at the present time" (§3.6 OPC Data Access, used when e.g., `ActualStartTime` is queried before batch starts).

**Recommendation**: Add constant to `src/Opc.Classic.Batch/OpcBatchErrors.cs` for server implementors.

---

### 6. **Component Category Registration** — Documentation only

**Spec**: §7 Installation Issues

- **Version 1.0**: `CATID_OPCBatchServer10 = {a8080da0-e23e-11d2-afa7-00c04f539421}`  
  Descriptor: "OPC Batch Server Version 1.0"
- **Version 2.0**: `CATID_OPCBatchServer20 = {843DE67B-B0C9-11d4-A0B7-000102A980B1}`  
  Descriptor: "OPC Batch Server Version 2.0"

**Recommendation**: Add constants + COM registration helpers (similar to DA/AE) in `src/Opc.Classic.Discovery/` for server registration.

---

## Compliance Checklist (§3.5)

A fully compliant OPC Batch 2.0 server must:

| Requirement                                                      | Status    | Notes                                                                 |
|------------------------------------------------------------------|-----------|-----------------------------------------------------------------------|
| OPC Data Access 2.04 (all required interfaces)                  | ⚠️ PARTIAL | Depends on DA runtime; not in Batch project scope.                   |
| `IOPCBrowseServerAddressSpace` (DA optional, Batch required)     | ❌ MISSING | No implementation visible.                                            |
| `IOPCBatchServer`                                                | ⚠️ PARTIAL | `GetDelimiter` ✅, `CreateEnumerator` ❌ (deferred).                  |
| `IEnumOPCBatchSummary`                                           | ⚠️ PARTIAL | `Next`, `Skip`, `Reset`, `Count` ✅; `Clone` ❌ (deferred).           |
| `IOPCEnumerationSets`                                            | ❌ MISSING | Interface declared, all 3 methods missing.                           |
| Batch namespace (5 well-known item IDs)                          | ❌ MISSING | Server-side implementation not in scope.                             |
| Required properties (79 properties ID 400–478)                   | ❌ MISSING | No property constants or DA integration.                             |

**Overall**: ⚠️ **PARTIAL COMPLIANCE** — Interface projections exist but key methods deferred; no server-side namespace or property support.

---

## Recommendations for Next Phase

### High Priority

1. **Implement `IOPCEnumerationSets` methods** (HIGH)  
   - `QueryEnumerationSets`, `QueryEnumeration`, `QueryEnumerationList`  
   - Block: Requires two-level-out `DWORD**` + `LPWSTR**` NDR marshalling (post-M5 confirmed available).  
   - Impact: Enables client-side enumeration translation for states, modes, levels.

2. **Implement `IOPCBatchServer::CreateEnumerator`** (BLOCKER)  
   - Returns `IEnumOPCBatchSummary` interface pointer.  
   - Block: Requires IFACE-pointer-out NDR support (confirmed available post-M5).  
   - Impact: Enables batch enumeration workflow (§5.2.6).

### Medium Priority

3. **Implement `IOPCBatchServer2::CreateFilteredEnumerator`** (MEDIUM)  
   - Returns filtered `IEnumOPCBatchSummary` for `OPCBBatchModel` or `OPCBBatchArchiveModel`.  
   - Block: Same as #2 (IFACE-pointer-out).  
   - Impact: Enables filtered batch queries (archive scenarios).

4. **Add managed enumeration types** (LOW)  
   - `OpcBatchPhysicalLevel`, `OpcBatchState`, `OpcBatchMode`, etc.  
   - Impact: Helper types for server implementors (not required for DCOM proxy).

### Low Priority

5. **Add property ID constants** (LOW)  
   - `OpcBatchPropertyId.cs` with 79 property IDs 400–478.  
   - Impact: Server-side convenience; not needed for proxy generation.

6. **Add error code constants** (LOW)  
   - `OPCB_E_NOT_MEANINGFUL = 0xC0040300`.  
   - Impact: Server-side error handling.

7. **Implement `IEnumOPCBatchSummary::Clone`** (LOW)  
   - Returns cloned enumerator.  
   - Block: Same as #2.  
   - Impact: Convenience; rarely used.

---

## Conclusion

The **Opc.Classic.Batch** project provides **foundational NDR codec support** for the two key structures (`OPCBATCHSUMMARY`, `OPCBATCHSUMMARYFILTER`) and **partial interface projections** for the four OPC Batch 2.0 interfaces. However, **3 of 4 interfaces are incomplete**:

- **IOPCBatchServer**: 50% (1/2 methods)  
- **IOPCBatchServer2**: 0% (0/1 methods, deferred)  
- **IEnumOPCBatchSummary**: 80% (4/5 methods)  
- **IOPCEnumerationSets**: 0% (0/3 methods)

**Critical path** for full compliance:

1. Unblock `CreateEnumerator` / `CreateFilteredEnumerator` (IFACE-pointer-out NDR — confirmed ready).  
2. Implement `IOPCEnumerationSets` (two-level-out arrays — confirmed ready).  
3. Build server-side namespace support (separate effort, likely in `samples/` or runtime).

**Readiness for M6 (Batch support milestone)**: 🟡 **Partial** — NDR codecs production-ready; interface methods need implementation once IFACE-pointer-out patterns are validated in generator tests.

---

**Reviewed by**: Spec coverage analysis agent  
**Spec source**: `External/Docs/opc-batch-2.00-specification.md`  
**Implementation source**: `src/Opc.Classic.Batch/`  
**Commit**: (to be added after review)
