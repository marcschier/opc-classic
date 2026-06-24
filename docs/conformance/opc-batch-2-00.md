# OPC Batch 2.00 conformance review

**Spec:** `opc-classic-docs/OPC-BATCH-2.00.md` (OPC Batch Custom Interface Specification 2.00).

**Scope:** 4 DCOM interfaces (`IOPCBatchServer`, `IOPCBatchServer2`, `IEnumOPCBatchSummary`, `IOPCEnumerationSets`), 4 NDR-encoded structures (`OPCBATCHSUMMARY`, `OPCBATCHSUMMARYFILTER`, enumeration set lists), the `OPC.Batch.1` ProgID + CATID, and 478 Batch DA property IDs (`OPCBATCH_PROP_*`, 400-478).

**Implementing assemblies:** `Opc.Classic.Batch` (DCOM projections, codecs, property IDs, error constants), `Opc.Classic.Core` (cross-cutting types).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCBatchServer` (2 methods) | §3.2 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IOPCBatchServer2` (1 method) | §3.3 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IEnumOPCBatchSummary` (5 methods) | §3.4 | ✅ source-generated proxy + dispatcher (hand-written `Clone` interface-ref path) | ✅ | conformant |
| `IOPCEnumerationSets` (3 methods) | §3.5 | ⚠️ projected; multi-out record generation is server-policy | ⚠️ | soft gap — see §3.1 |
| `OPCBATCHSUMMARY` codec | App. B IDL | ✅ `NdrOpcBatchSummaryCodec` | ✅ | conformant |
| `OPCBATCHSUMMARYFILTER` codec | App. B IDL | ✅ `NdrOpcBatchSummaryFilterCodec` | ✅ | conformant |
| Batch HRESULTs | App. C | ✅ `OpcBatchErrors` | ✅ | conformant |
| Batch DA property IDs 400 - 478 | §4 | ✅ `OpcBatchPropertyId` | ✅ | conformant |
| `CATID_OPCBatchServer10` | OPC-COMMON §8.1 | ✅ `OpcGuids.CATID_OPCBatchServer10` | ✅ | conformant |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCBatchServer` (spec §3.2)

**IID:** `8BB4ED50-B314-11D3-B3EA-00C04F8ECEAA`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `GetDelimiter` | 3 | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs` line 25 → generated `.OpcProxy.g.cs` / `.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Batch.Tests/Dcom/IOPCBatchProxyTests.cs` |
| `CreateEnumerator` | 4 | line 31 | same |

### 1.2 `IOPCBatchServer2` (spec §3.3)

**IID:** `895A78CF-B0C5-11D4-A0B7-000102A980B1`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `CreateFilteredEnumerator` | 3 | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs` line 44 | `tests/Opc.Classic.Batch.Tests/Dcom/IOPCBatchProxyTests.cs` |

### 1.3 `IEnumOPCBatchSummary` (spec §3.4)

**IID:** `A8080DA2-E23E-11D2-AFA7-00C04F539421`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `Next` | 3 | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs` line 57 | `tests/Opc.Classic.Batch.Tests/Dcom/IOPCBatchProxyTests.cs` |
| `Skip` | 4 | line 63 | same |
| `Reset` | 5 | line 69 | same |
| `Clone` | 6 | line 75 (hand-written interface-ref path in `IOPCBatchClientProxies.cs`) | same |
| `Count` | 7 | line 81 | same |

### 1.4 `IOPCEnumerationSets` (spec §3.5)

**IID:** `A8080DA3-E23E-11D2-AFA7-00C04F539421`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `QueryEnumerationSets` | 3 | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs` line 96 | `tests/Opc.Classic.Batch.Tests/Dcom/IOPCBatchProxyTests.cs` |
| `QueryEnumeration` | 4 | line 103 | same |
| `QueryEnumerationDescription` | 5 | line 109 | same |

**Note:** `QueryEnumerationSets` returns multi-out arrays of dynamically-sized records. The generator currently projects the wire shape; populating runtime values requires per-server logic and is server-policy (see ROADMAP — multi-out record generation).

### 1.5 NDR codecs (spec App. B IDL)

| Codec | Source | Tests |
|---|---|---|
| `OPCBATCHSUMMARY` (per spec §6.2 struct) | `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryCodec.cs` | `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryCodecTests.cs`, `tests/Opc.Classic.Batch.Tests/BatchFileTimeFuzzTests.cs` |
| `OPCBATCHSUMMARYFILTER` (per spec §6.2 struct) | `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryFilterCodec.cs` | `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryFilterCodecTests.cs` |

`OPCBATCHSUMMARY` carries `szID`, `szDescription`, `szOPCItemID`, `szMasterRecipeID`, `szMasterRecipeVersion`, `dblBatchSize`, `szEngineeringUnits`, `ftStartTime`, `ftEndTime`, `ftExecutionTime` per spec.

### 1.6 Batch HRESULTs (spec App. C)

| Constant | Source | Tests |
|---|---|---|
| `OPCB_E_INVALIDCONFIGFILE` etc. (spec App. C) | `src/Opc.Classic.Batch/OpcBatchErrors.cs` | `tests/Opc.Classic.Batch.Tests/OpcBatchErrorsTests.cs` |

### 1.7 Batch DA property IDs 400 - 478 (spec §4)

| Surface | Source | Tests |
|---|---|---|
| `OpcBatchPropertyId.*` constants (per spec §4) | `src/Opc.Classic.Batch/OpcBatchPropertyId.cs` | `tests/Opc.Classic.Batch.Tests/OpcBatchPropertyIdTests.cs` |

All 79 Batch-property IDs (400 - 478) are declared with their spec
canonical names and types.

---

## 2 Normative-clause checklist

OPC-BATCH-2.00 contains 1 normative SHALL clause per the Phase 0
inventory:

| § | Clause (paraphrased) | Status | Evidence |
|---|---|---|---|
| §3.x — `IOPCBatchServer::GetDelimiter` | The server SHALL return a NUL-terminated wide-string delimiter consistent with the delimiter used in the address-space hierarchy. | ✅ honored at the wire layer | `GetDelimiterAsync` returns a string; the server-policy aspect (consistency with namespace) is implementer-controlled. |

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 `IOPCEnumerationSets` multi-out record generation

The generator currently emits the wire-shape projection for the
3 methods but does not yet auto-generate the multi-out record arrays
(arrays of arrays of variable-length strings). Server implementers must
hand-write the dispatcher for these methods. Status: **WAIVED**
(deferred-by-design) — see ROADMAP entry. Affects Batch servers only.

#### 3.1.2 No reference Batch server

Opc.Classic ships no `samples/Opc.Classic.Samples.OpcBatchServer`
(unlike DA / AE / HDA / Security / OpcEnum). Status: **WAIVED**
(deferred) — Batch servers are rare in industry; reference sample is a
backlog item.

#### 3.1.3 No matrix profile against a third-party Batch server

The cross-impl matrix is green for the DA / AE / HDA profiles
but no `batch-*` profile. Same rationale as §3.1.2 — third-party
Batch servers are rare. Status: **WAIVED**.

### 3.2 Hard gaps

None at present. All 4 interfaces, 11 wire methods, 2 codecs, error
constants, and property IDs are implemented and tested.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC Batch 2.00](../CONFORMANCE.md#opc-batch-200)
- Related spec: [`docs/conformance/opc-common-1-10.md`](opc-common-1-10.md) — CATID + ProgID conventions.
- Related spec: [`docs/conformance/opc-da-2-05a.md`](opc-da-2-05a.md) — Batch property IDs extend DA property semantics.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-BATCH-2.00.md` (OPC Batch Custom
Interface Specification 2.00).

Phase 0 inventory:

- `files/conformance/inventory/opc-batch-2-00-headings.csv` (69 entries)
- `files/conformance/inventory/opc-batch-2-00-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-batch-2-00-interfaces.csv` (17 interface + 15 method references)
