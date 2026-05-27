# OPC Batch 2.00 — Spec Coverage Review

**Spec**: OPC Batch Custom Interface Specification Version 2.0 (July 19, 2001)
**Implementation**: `src/Opc.Classic.Batch/`
**Review target**: `1.0.0-rc.7`

---

## Summary

**Interfaces**: 4/4 projected
**Methods**: 11/11 declared/projected
**Structs**: 2/2 codecs registered (`OPCBATCHSUMMARY`, `OPCBATCHSUMMARYFILTER`)
**Error constants**: Batch-specific `OPCB_E_NOT_MEANINGFUL` is present

**Overall compliance**: **Projection complete; server semantics remain implementation-specific**. Earlier claims that `CreateEnumerator`, `CreateFilteredEnumerator`, `IOPCEnumerationSets::*`, and `IEnumOPCBatchSummary::Clone` were missing are stale.

---

## Implementation Status

| Interface | Methods | Status | Source |
|---|---:|---|---|
| `IOPCBatchServer` | 2/2 | ✅ `GetDelimiter`, `CreateEnumerator` | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:20-31` |
| `IOPCBatchServer2` | 1/1 | ✅ `CreateFilteredEnumerator` | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:33-40` |
| `IEnumOPCBatchSummary` | 5/5 | ✅ `Next`, `Skip`, `Reset`, `Clone`, `Count` | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:42-65` |
| `IOPCEnumerationSets` | 3/3 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:67-86` |

Interface-pointer methods use hand-written proxy/dispatcher paths where needed:

- `IOPCBatchServerClientProxy` and dispatcher: `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs:19-184`
- `IOPCBatchServer2ClientProxy` and dispatcher: `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs:50-223`
- `IEnumOPCBatchSummaryClientProxy` and dispatcher: `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs:76-311`

---

## Structures and Error Codes

| Feature | Status | Source/Test |
|---|---|---|
| `OPCBATCHSUMMARY` | ✅ codec | `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryCodec.cs`; `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryCodecTests.cs:1-116` |
| `OPCBATCHSUMMARYFILTER` | ✅ codec | `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryFilterCodec.cs`; `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryFilterCodecTests.cs` |
| Batch HRESULTs | ✅ constants/tests | `src/Opc.Classic.Batch/OpcBatchErrors.cs`; `tests/Opc.Classic.Batch.Tests/OpcBatchErrorsTests.cs:1-21` |

---

## Remaining Implementation Work

### 1. Batch namespace models

The library projects Batch interfaces and codecs but does not provide a complete vendor-neutral Batch server namespace. A compliant server still needs to expose and maintain the standard Batch namespace models such as `OPCBPhysicalModel`, `OPCBMasterRecipeModel`, `OPCBBatchModel`, `OPCBBatchArchiveModel`, and `OPCBBatchIDList`.

**Status**: server-implementation responsibility.
**Priority**: Medium for a sample/reference Batch server; not a DCOM projection gap.

### 2. Batch property IDs and DA integration

Batch property IDs 400-478 are spec-defined DA item properties. The Batch projection does not yet provide a type-safe `OpcBatchPropertyId` helper or a reference DA namespace integration.

**Priority**: Low/Medium convenience for server authors.

### 3. Enumeration-set data source

`IOPCEnumerationSets` is now projected, but a server implementation must still supply localized enumeration set names and values for standard and vendor-defined sets.

---

## Test Coverage

| Test File | Scope |
|---|---|
| `tests/Opc.Classic.Batch.Tests/Dcom/IOPCBatchProxyTests.cs:1-286` | Batch proxy and dispatcher payloads, including interface-reference returns |
| `tests/Opc.Classic.Batch.Tests/DcomInterfaceIdTests.cs:1-52` | IIDs |
| `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryCodecTests.cs:1-116` | Summary codec round trips |
| `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryFilterCodecTests.cs` | Filter codec round trips |
| `tests/Opc.Classic.Batch.Tests/OpcBatchErrorsTests.cs:1-21` | Batch HRESULT constants |

---

## Compliance Checklist (§3.5)

| Requirement | Current status | Notes |
|---|---|---|
| OPC Data Access dependency | ✅ via DA runtime | Batch project depends on the DA surface for item/property semantics. |
| `IOPCBrowseServerAddressSpace` | ✅ in DA runtime | Batch namespace population remains server-specific. |
| `IOPCBatchServer` | ✅ projected | Server must implement delimiter/enumerator behavior. |
| `IOPCBatchServer2` | ✅ projected | Server must implement filter semantics. |
| `IEnumOPCBatchSummary` | ✅ projected | Clone is no longer deferred. |
| `IOPCEnumerationSets` | ✅ projected | Server must provide enumeration-set content. |
| Batch namespace/properties | ⚠️ not provided as a generic runtime | Suitable for a future sample/reference server. |

---

## Conclusion

`Opc.Classic.Batch` now provides complete Batch DCOM projection coverage for the spec interfaces and the two required structures. The remaining work is not missing wire declarations; it is server-side semantics: maintaining the Batch namespace, property metadata, enumeration-set catalogs, and real batch summary data.
