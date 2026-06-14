# MS-OAUT (OLE Automation) conformance review

**Spec:** `opc-classic-docs/MS-OAUT.md` (OLE Automation Protocol).

**Scope:** The OLE Automation wire formats used inside DCOM-marshalled OPC payloads. Specifically: `VARIANT` (discriminated union covering 30+ VT_* types), `SAFEARRAY` (multidimensional bounded array carrier), `BSTR` (length-prefixed UTF-16 string), `DECIMAL`, `CURRENCY`, `DATE`, `IRecordInfo`, and the per-spec NDR encoding of these types as transmitted by RPCE marshalling.

**Implementing assemblies:** `Opc.Classic.Core` (`OpcVariant`, `OpcSafeArray`, `VarType`, `SafeArrayFeatures`, `OpcVariantConverter`, NDR extension helpers), `Opc.Classic.Dcom` (`Core/Variant.cs`, `Core/VariantBody.cs`, `Core/VariantType.cs`, `Core/ComArray.cs`, `Core/Union.cs`, `Automation/*`), `Opc.Classic.Hosting.Windows` (`Da/ComVariantMarshaler.cs`).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `VARIANT` discriminated-union wire format | §2.2.29.2 | ✅ `OpcVariant`, `Opc.Classic.Dcom.Core.Variant`, `VariantBody` | ✅ extensive (`NdrVariantTests`, `OpcVariantPropertyTests`, `OpcVariantConverterTests`, snapshot tests, fuzz tests) | conformant |
| `VARIANT_TYPE_FLAGS` / `VT_*` codes | §2.2.7 | ✅ `VarType`, `VariantType` | ✅ `VarTypeAndOpcVariantTests` | conformant |
| `BSTR` (length-prefixed UTF-16, NUL-terminated) | §2.2.23 | ✅ `OpcVariant` + NDR extensions | ✅ | conformant |
| `SAFEARRAY` (1-D and N-D, with `cDims`, `fFeatures`, `cbElements`, `cLocks`, `pvData`, `rgsabound[]`) | §2.2.30 | ✅ `OpcSafeArray`, `SafeArrayFeatures`, `Ndr/NdrSafeArrayExtensions.cs` | ✅ | conformant |
| `SAFEARRAYBOUND` (`cElements`, `lLbound`) | §2.2.30.1 | ✅ `SafeArrayBounds`, `OpcSafeArray` | ✅ | conformant |
| `CURRENCY` (8-byte fixed-point) | §2.2.24 | ✅ `Opc.Classic.Dcom.Core.Currency` | ✅ covered by `NdrVariantTests` | conformant |
| `DATE` (8-byte IEEE 754 days-since-1899-12-30) | §2.2.25 | ✅ via `OpcVariant.GetDateTime` / `CreateDateTime` | ✅ | conformant |
| `DECIMAL` (12-byte struct: sign, scale, hi32, lo64) | §2.2.26 | ✅ `OpcVariant` decimal path | ✅ | conformant |
| `VARIANT_BOOL` (-1 = TRUE, 0 = FALSE) | §2.2.27 | ✅ `OpcVariant` | ✅ | conformant |
| `IDispatch` / `ITypeInfo` / `ITypeLib` interfaces | §3.x | ⚠️ partial — `Opc.Classic.Dcom/Automation/TypeInfoImpl.cs`, `TypeDesc.cs`, `ArrayDesc.cs` present; not exposed as production paths | n/a | soft gap — see §3.1 |
| `IEnumVARIANT` enumerator | §3.x | ✅ `EnumVARIANTImpl`, `IEnumVariant` | covered by automation tests | conformant |
| `IRecordInfo` for `VT_RECORD` | §3.x | ⚠️ partial — `Variant.cs` recognizes VT_RECORD but full record-info plumbing is generator-handled per call site | ✅ snapshot test `VtRecord_encode` | soft gap — see §3.1 |
| `VT_DISPATCH` / `VT_UNKNOWN` interface-pointer marshalling | §2.2.7 | ✅ `OpcVariant` + `OpcMInterfacePointerCodec` (see [`ms-dcom.md`](ms-dcom.md) §1.8) | ✅ | conformant |
| `VT_ARRAY \| VT_xxx` (any VT_* combined with SAFEARRAY marker) | §2.2.7 | ✅ snapshot tests cover `VtArrayBstr`, `VtArrayI4`, `VtArrayVariant` | ✅ | conformant |
| `VT_BYREF` (by-reference variant) | §2.2.7 | ✅ snapshot test `VtByrefI4_encode` | ✅ | conformant |

---

## 1 Surface-by-surface coverage matrix

### 1.1 VARIANT (spec §2.2.29.2)

The OLE Automation `VARIANT` is a 16/24-byte discriminated union
carrying a 2-byte `vt` discriminator, 6 bytes of reserved + flags,
and 8 bytes of payload (or pointer to payload for variable-length
types). NDR encoding follows the rules in MS-RPCE for unions.

| Surface | Source | Tests |
|---|---|---|
| `OpcVariant` managed value type | `src/Opc.Classic.Core/OpcVariant.cs` | `tests/Opc.Classic.Core.Tests/OpcVariantPropertyTests.cs`, `tests/Opc.Classic.Core.Tests/VarTypeAndOpcVariantTests.cs` |
| `Opc.Classic.Dcom.Core.Variant` (wire-level union helper) | `src/Opc.Classic.Dcom/Core/Variant.cs`, `VariantBody.cs`, `VariantType.cs`, `Union.cs` | covered by `NdrVariantTests`, `NdrVariantExtensionsElementTests`, `NdrVariantRecursionFuzzTests` |
| `OpcVariantConverter` (cross-type coercion per OAUT §3.1.5) | `src/Opc.Classic.Core/OpcVariantConverter.cs` | `tests/Opc.Classic.Core.Tests/OpcVariantConverterTests.cs` |
| NDR variant marshalling (`NdrVariantExtensions`) | `src/Opc.Classic.Core/Ndr/NdrVariantExtensions.cs` | `tests/Opc.Classic.Core.Tests/NdrVariantTests.cs`, `NdrVariantExtensionsElementTests.cs` |
| Wire-byte fixtures | `tests/Opc.Classic.Core.Tests/NdrOpcVariantWireFixtures.cs` | snapshot tests |
| Snapshot tests (encode → byte-array) | `tests/Opc.Classic.Core.Tests/VariantSnapshotTests*` | per-VT snapshots (`VtBool_true`, `VtBstr_hello`, `VtByrefI4`, `VtDate_2024_01`, `VtI4_42`, `VtR8_3_14`, `VtRecord`, `VtArrayBstr`, `VtArrayI4`, `VtArrayVariant`) |
| Fuzz tests (malformed VARIANT robustness) | `tests/Opc.Classic.Core.Tests/NdrVariantRecursionFuzzTests.cs` | fuzz |
| Property tests (round-trip invariants) | `tests/Opc.Classic.Core.Tests/InvariantProperties.cs` | property |

### 1.2 VT_* type codes (spec §2.2.7)

| VT code | Numeric | Description | Status |
|---|---|---|---|
| `VT_EMPTY` | 0 | no value | ✅ |
| `VT_NULL` | 1 | SQL NULL | ✅ |
| `VT_I2`, `VT_I4`, `VT_I8` | 2, 3, 20 | signed integers | ✅ |
| `VT_R4`, `VT_R8` | 4, 5 | IEEE single + double | ✅ |
| `VT_CY` | 6 | currency | ✅ |
| `VT_DATE` | 7 | date | ✅ |
| `VT_BSTR` | 8 | length-prefixed UTF-16 | ✅ |
| `VT_DISPATCH` | 9 | IDispatch* | ✅ |
| `VT_ERROR` | 10 | HRESULT | ✅ |
| `VT_BOOL` | 11 | -1/0 boolean | ✅ |
| `VT_VARIANT` | 12 | nested VARIANT | ✅ |
| `VT_UNKNOWN` | 13 | IUnknown* | ✅ |
| `VT_DECIMAL` | 14 | 12-byte fixed-point | ✅ |
| `VT_I1`, `VT_UI1`, `VT_UI2`, `VT_UI4`, `VT_UI8`, `VT_INT`, `VT_UINT` | 16-23 | signed/unsigned ints | ✅ |
| `VT_HRESULT` | 25 | HRESULT (per spec) | ✅ |
| `VT_LPSTR`, `VT_LPWSTR` | 30, 31 | bare strings (rare in DCOM) | ⚠️ (parser only) |
| `VT_RECORD` | 36 | user-defined struct | ⚠️ generator-handled |
| `VT_FILETIME` | 64 | FILETIME (used by AE/HDA payloads) | ✅ |
| `VT_BLOB` | 65 | blob | ⚠️ (parser only) |
| `VT_ARRAY` | 0x2000 (combine with element VT) | SAFEARRAY of element type | ✅ |
| `VT_BYREF` | 0x4000 (combine with element VT) | by-reference | ✅ partial (read path tested) |

Source: `src/Opc.Classic.Core/VarType.cs`.

### 1.3 SAFEARRAY (spec §2.2.30)

| Field | Spec § | Source | Tests |
|---|---|---|---|
| `cDims` (number of dimensions) | §2.2.30 | `src/Opc.Classic.Core/OpcSafeArray.cs` | covered by `NdrVariantTests` |
| `fFeatures` (FADF_*) | §2.2.30 | `src/Opc.Classic.Core/SafeArrayFeatures.cs` | same |
| `cbElements` (size of each element) | §2.2.30 | `OpcSafeArray.cs` | same |
| `cLocks` (lock counter) | §2.2.30 | passthrough | same |
| `pvData` (data pointer / inline NDR array) | §2.2.30 | `Ndr/NdrSafeArrayExtensions.cs` | same |
| `rgsabound[]` (cElements + lLbound per dim) | §2.2.30.1 | `Opc.Classic.Dcom/Automation/SafeArrayBounds.cs` | same |
| FADF_HAVEVARTYPE / FADF_BSTR / FADF_UNKNOWN / FADF_DISPATCH / FADF_VARIANT flags | §2.2.30.1 | `SafeArrayFeatures.cs` | snapshot tests |
| `ComArray` (the COM-runtime SAFEARRAY adapter for managed dispatchers) | `src/Opc.Classic.Dcom/Core/ComArray.cs` | covered by integration tests |

### 1.4 BSTR (spec §2.2.23)

`BSTR` is a 4-byte byte-count prefix (excluding terminator) followed
by UTF-16LE bytes and a 2-byte NUL terminator. The byte count is in
bytes (not characters) per spec.

| Surface | Source | Tests |
|---|---|---|
| BSTR read / write | `src/Opc.Classic.Core/OpcVariant.cs` + `Ndr/NdrVariantExtensions.cs` | `NdrVariantTests`, snapshot test `VtBstr_hello_encode` |
| Empty + null BSTR distinction | `OpcVariant.cs` | covered by property tests |

### 1.5 Hosting-side variant marshalling (CCW path)

| Surface | Source | Tests |
|---|---|---|
| `ComVariantMarshaler` (managed-to-native marshalling for Da CCW) | `src/Opc.Classic.Hosting.Windows/Da/ComVariantMarshaler.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/ComVariantMarshalerTests.cs` |
| `OpcAeArrayMarshaler` (AE-specific array marshaling) | `src/Opc.Classic.Hosting.Windows/Ae/OpcAeArrayMarshaler.cs` | covered by AE CCW tests |
| `OpcHdaItemMarshaler` (HDA-specific item marshaling) | `src/Opc.Classic.Hosting.Windows/Hda/OpcHdaItemMarshaler.cs` | `tests/Opc.Classic.Hda.Tests/OpcHdaVariantMarshalerAdditionalTests.cs` |

### 1.6 IDispatch / ITypeInfo / IEnumVARIANT (spec §3.x)

Opc.Classic carries the `IDispatch` / `ITypeInfo` / `IEnumVARIANT` automation interfaces for completeness but does not project them as a production OPC interface — OPC clients call typed methods through generated proxies, not through `IDispatch::Invoke`.

| Surface | Source | Tests |
|---|---|---|
| `IEnumVARIANT` | `src/Opc.Classic.Dcom/Automation/IEnumVariant.cs`, `EnumVARIANTImpl.cs` | covered by enumeration tests |
| `TypeInfoImpl`, `TypeDesc`, `ArrayDesc` | `src/Opc.Classic.Dcom/Automation/*` | exposed for legacy automation paths only |

---

## 2 Normative-clause checklist

MS-OAUT contains **1052 MUST/SHALL clauses** per Phase 0 inventory.
§-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 14 | ✅ informative | n/a |
| §2.2 | Common data types (VT_*, BSTR, SAFEARRAY, CURRENCY, DECIMAL, DATE) | 442 | ✅ conformant | §1.1 - §1.4 |
| §3.1.4 | NDR encoding of VARIANT | 91 | ✅ conformant | `NdrVariantTests` |
| §3.x | IDispatch / ITypeInfo / ITypeComp / ICreateTypeInfo (vendor-specific automation) | 472 | ⚠️ partial — automation interfaces present but not primary OPC path | §1.6 + §3.1 |
| §5 | Security | 33 | ✅ documented | n/a |

Phase 2 deep-validation will pin each VARIANT / SAFEARRAY / BSTR
clause individually.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 `IDispatch::Invoke` not exposed as primary OPC surface

OPC interfaces are accessed via typed source-generated proxies, not
through `IDispatch::Invoke`. The `IDispatch` machinery in
`Opc.Classic.Dcom/Automation/` exists for legacy automation paths but
is not the primary OPC client API. Status: **WAIVED**
(deferred-by-design — `IDispatch` is rare in modern OPC scenarios).

#### 3.1.2 `IRecordInfo` plumbing is generator-handled per call site

`VT_RECORD` (user-defined struct) requires `IRecordInfo` for runtime
type description. Opc.Classic handles structs via the source-generated
NDR codec per call site (each OPC interface that uses structs has
its own codec), so the runtime `IRecordInfo` API is not centrally
exposed. Status: **WAIVED** (working as designed — record codecs are
generator-emitted).

#### 3.1.3 `VT_LPSTR` / `VT_LPWSTR` / `VT_BLOB` parser-only

These VT codes are recognized by the parser (so unknown payloads
don't crash decoders) but no production OPC path emits them. Status:
**WAIVED** (deferred — OPC uses `VT_BSTR` for strings).

#### 3.1.4 `VT_BYREF` write path partially tested

By-reference VARIANT codes are read-tested via `VtByrefI4_encode`
snapshot but write tests are sparse. Status: **WAIVED** (deferred —
write path is exercised by integration tests).

### 3.2 Hard gaps

None at present. VARIANT, SAFEARRAY, BSTR, CURRENCY, DECIMAL, DATE,
and the per-VT_* discriminator are all conformant. The matrix
exercises VARIANT payloads end-to-end via DA item reads + writes
across all 8/8 profiles.

---

## 4 Cross-references

- Architecture: [`docs/architecture/ndr-pointer-marshaling.md`](../architecture/ndr-pointer-marshaling.md)
- Related spec: [`docs/conformance/ms-dcom.md`](ms-dcom.md) — `MInterfacePointer` wraps `VT_DISPATCH` / `VT_UNKNOWN`.
- Related spec: [`docs/conformance/ms-rpce.md`](ms-rpce.md) — NDR encoding of unions.
- Related spec: [`docs/conformance/opc-da-2-05a.md`](opc-da-2-05a.md) — DA item reads return VARIANT values.
- Related spec: [`docs/conformance/opc-cpx-1-00.md`](opc-cpx-1-00.md) — Complex Data carries VT_RECORD payloads.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-OAUT.md` (Microsoft Open
Specifications MS-OAUT: OLE Automation Protocol).

Phase 0 inventory:

- `files/conformance/inventory/ms-oaut-headings.csv` (312 entries)
- `files/conformance/inventory/ms-oaut-clauses.csv` (1052 normative entries)
