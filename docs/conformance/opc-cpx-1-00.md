# OPC Complex Data 1.00 conformance review

**Spec:** `opc-classic-docs/OPC-CPX-1.00.md` (OPC Complex Data Specification 1.00, December 10, 2003).

**Scope:** Vendor-extension COM interfaces for complex DA item discovery (`IOPCComplexDataItem`, `IOPCComplexDataItem2`, `IOPCTypeLibrary`), DA item properties `600-609`, OPC Binary type-dictionary parser + codec, XML Schema parser + complex-value serializer, CPX namespace conventions, type-conversion + data-filter behavior, and CPX-specific HRESULTs.

**Implementing assemblies:** `Opc.Classic.Cpx` (codecs, type model, DA hosting helpers, DCOM interfaces), `Opc.Classic.Core` (cross-cutting).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCComplexDataItem` (4 methods) | vendor extension over §3.3 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IOPCComplexDataItem2` (3 methods) | vendor extension | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IOPCTypeLibrary` (3 methods) | vendor extension | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| DA properties 600-609 | §3.3 | ✅ `OpcComplexDataProperty` constants | ✅ | conformant |
| OPC Binary dictionary parser | §6 | ✅ `OpcBinaryDictionaryParser` | ✅ | conformant |
| OPC Binary encoder | §6 | ✅ `OpcBinaryEncoder` (incl. BitString) | ✅ | conformant |
| OPC Binary decoder | §6 | ✅ `OpcBinaryDecoder` (incl. BitString) | ✅ | conformant |
| XML Schema parser | §5.1 | ✅ `XmlSchemaParser` | ✅ | conformant |
| XML complex-value serializer | §5 | ✅ `XmlComplexValueSerializer` | ✅ | conformant |
| CPX namespace convention | §3.4 | ✅ `CpxNamespaceBuilder` + `OpcCpxAddressSpace` | ✅ | conformant |
| DA item-property publisher | §3.3 | ✅ `OpcCpxItemProperties` | ✅ | conformant |
| CPX HRESULTs (5 codes) | §9 | ✅ `OpcComplexDataResult` | ✅ | conformant |
| Type-conversion runtime | §7 | ⚠️ helpers only; runtime is server-policy | n/a | soft gap — see §3.1 |
| Data-filter runtime | §8 | ⚠️ helpers only; runtime is server-policy | n/a | soft gap — see §3.1 |
| End-to-end CPX sample server | n/a | ❌ not shipped | n/a | soft gap — see §3.1 |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCComplexDataItem` (vendor extension over spec §3.3)

**IID:** `7ECE6649-2C1E-494A-BB99-22D36FB3B0C3`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `GetTypeSystemID` | 3 | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs` line 28 → generated `.OpcProxy.g.cs` / `.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Cpx.Tests/Dcom/IOPCCpxProxyTests.cs` |
| `GetDictionaryID` | 4 | line 34 | same |
| `GetTypeID` | 5 | line 40 | same |
| `GetDictionary` | 6 | line 46 | same |

These accessors expose the values of DA properties 600 / 601 / 602 / 603 on a single DA item via the IOPCComplexDataItem extension interface, mirroring the same data otherwise available through `IOPCItemProperties`.

### 1.2 `IOPCComplexDataItem2` (vendor extension)

**IID:** `44F68398-60AF-4F02-9442-172D058CB16F`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `GetTypeDescription` | 3 | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs` line 61 | `tests/Opc.Classic.Cpx.Tests/Dcom/IOPCCpxProxyTests.cs` |
| `GetConsistencyWindow` | 4 | line 67 | same |
| `GetWriteBehavior` | 5 | line 73 | same |

Exposes properties 604 / 605 / 606.

### 1.3 `IOPCTypeLibrary` (vendor extension)

**IID:** `B8C1B2C6-ACB7-4B7B-87B5-6EAC2CF63C31`

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `QueryTypeSystem` | 3 | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs` line 88 | `tests/Opc.Classic.Cpx.Tests/Dcom/IOPCCpxProxyTests.cs` |
| `GetTypeItemID` | 4 | line 94 | same |
| `GetDictionaryItemID` | 5 | line 100 | same |

### 1.4 DA Properties 600 - 609 (spec §3.3)

| Property ID | Name | Spec § | Constant |
|---|---|---|---|
| 600 | Type System ID | §3.3.1 | `OpcComplexDataProperty.TypeSystemId` |
| 601 | Dictionary ID | §3.3.2 | `OpcComplexDataProperty.DictionaryId` |
| 602 | Type ID | §3.3.3 | `OpcComplexDataProperty.TypeId` |
| 603 | Dictionary | §3.3.4 | `OpcComplexDataProperty.Dictionary` |
| 604 | Type Description | §3.3.5 | `OpcComplexDataProperty.TypeDescription` |
| 605 | Consistency Window | §3.3.6 | `OpcComplexDataProperty.ConsistencyWindow` |
| 606 | Write Behavior | §3.3.7 | `OpcComplexDataProperty.WriteBehavior` |
| 607 | Unconverted Item ID | §7 | `OpcComplexDataProperty.UnconvertedItemId` |
| 608 | Unfiltered Item ID | §8 | `OpcComplexDataProperty.UnfilteredItemId` |
| 609 | Data Filter Value | §8 | `OpcComplexDataProperty.DataFilterValue` |

Source: `src/Opc.Classic.Cpx/OpcComplexDataProperty.cs`. Tests: `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs`.

### 1.5 OPC Binary type system (spec §6)

| Surface | Source | Tests |
|---|---|---|
| Dictionary parser | `src/Opc.Classic.Cpx/OpcBinaryDictionaryParser.cs` | `tests/Opc.Classic.Cpx.Tests/CpxParserAdditionalTests.cs`, `tests/Opc.Classic.Cpx.Tests/TypeDictionaryAdditionalTests.cs`, `tests/Opc.Classic.Cpx.Tests/Fuzz/CpxDictionaryParserFuzzTests.cs` |
| Binary encoder | `src/Opc.Classic.Cpx/OpcBinaryEncoder.cs` | `tests/Opc.Classic.Cpx.Tests/CpxCodecTests.cs`, `tests/Opc.Classic.Cpx.Tests/OpcBinaryBitStringTests.cs`, `tests/Opc.Classic.Cpx.Tests/OpcBinaryCodecAdditionalTests.cs` |
| Binary decoder | `src/Opc.Classic.Cpx/OpcBinaryDecoder.cs` | `tests/Opc.Classic.Cpx.Tests/CpxCodecTests.cs`, `tests/Opc.Classic.Cpx.Tests/OpcBinaryBitStringTests.cs`, `tests/Opc.Classic.Cpx.Tests/Fuzz/CpxBinaryDecoderFuzzTests.cs` |
| Codec utilities (alignment / padding) | `src/Opc.Classic.Cpx/OpcBinaryCodecUtilities.cs` | `tests/Opc.Classic.Cpx.Tests/OpcBinaryCodecAdditionalTests.cs` |

Primitive type coverage (per §6.2.4.2): `Boolean`, `Integer{8,16,32,64}`, `UInteger{8,16,32,64}`, `Floating-Point{32,64}`, `CharString`, `Single`, `Double`, `Date`, `Time`, `DateTime`, `Duration`, `Decimal`, `BitString`, `Reference`. All implemented via `TypeKind` enum (`src/Opc.Classic.Cpx/TypeKind.cs`).

Endianness: explicit `ByteOrder` enum (`src/Opc.Classic.Cpx/ByteOrder.cs`) honours the `BIG_ENDIAN` / `LITTLE_ENDIAN` declarations in the dictionary header (§6.2.1).

### 1.6 XML Schema type system (spec §5)

| Surface | Source | Tests |
|---|---|---|
| Schema parser | `src/Opc.Classic.Cpx/XmlSchemaParser.cs` | `tests/Opc.Classic.Cpx.Tests/CpxParserAdditionalTests.cs` |
| Complex-value serializer | `src/Opc.Classic.Cpx/XmlComplexValueSerializer.cs` | `tests/Opc.Classic.Cpx.Tests/XmlComplexValueSerializerAdditionalTests.cs` |
| Complex value DOM | `src/Opc.Classic.Cpx/ComplexValue.cs` | exercised by both above |

### 1.7 CPX namespace convention (spec §3.4)

| Surface | Source | Tests |
|---|---|---|
| Path builder (`/CPX/<TypeSystem>/<Dictionary>/<TypeID>`) | `src/Opc.Classic.Cpx/CpxNamespaceBuilder.cs` | `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs` |
| DA address-space integration | `src/Opc.Classic.Cpx/Hosting/OpcCpxAddressSpace.cs` | `tests/Opc.Classic.Cpx.Tests/OpcCpxAddressSpaceTests.cs` |
| DA item-property publisher | `src/Opc.Classic.Cpx/Hosting/OpcCpxItemProperties.cs` | `tests/Opc.Classic.Cpx.Tests/OpcCpxItemPropertiesTests.cs` |
| DI registration | `src/Opc.Classic.Cpx/Hosting/ServiceCollectionExtensions.cs` (`AddOpcCpxAddressSpace`) | exercised by hosting tests |
| Options surface | `src/Opc.Classic.Cpx/Hosting/OpcCpxOptions.cs` | exercised by hosting tests |

### 1.8 CPX HRESULTs (spec §9)

| Constant | Value | Source | Tests |
|---|---|---|---|
| `OPCCPX_E_TYPE_CHANGED` | `0xC0040407` | `src/Opc.Classic.Cpx/OpcComplexDataResult.cs` | `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs` |
| `OPCCPX_E_FILTER_DUPLICATE` | `0xC0040408` | same | same |
| `OPCCPX_E_FILTER_INVALID` | `0xC0040409` | same | same |
| `OPCCPX_E_FILTER_ERROR` | `0xC004040A` | same | same |
| `OPCCPX_S_FILTER_NO_DATA` | `0x0004040B` | same | same |

`OpcComplexDataResult.XmlDaNamespace` exposes the `http://opcfoundation.org/ComplexData/1.0/` namespace URI (§5.1) for XML-DA payload tagging.

---

## 2 Normative-clause checklist

OPC-CPX-1.00 contains 1 normative clause per the Phase 0 inventory:

| § | Clause (paraphrased) | Status | Evidence |
|---|---|---|---|
| §3.3 | A server SHALL expose properties 600 (Type System ID) and 602 (Type ID) for any DA item whose canonical type is complex; the other CPX properties (601, 603-609) are optional and conditional on server capabilities. | ✅ honored when hosting via `OpcCpxItemProperties` | `tests/Opc.Classic.Cpx.Tests/OpcCpxItemPropertiesTests.cs` exercises the mandatory 600 + 602 path; the property publisher only emits optional 601/603-609 when the host registers a backing dictionary / type description. |

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 §7 type-conversion runtime is server-policy

Spec §7 specifies a generic type-conversion engine that lets clients
request converted-into-native-type readings via `UnconvertedItemId`
(property 607) plus per-item conversion expressions. Opc.Classic ships
the property + ItemID plumbing, but the actual conversion-engine
implementation is server-specific and not provided as a generic
library. Status: **WAIVED** — server authors implement conversion in
their own `IOpcAddressSpace` / IOPCItemMgt hooks; the property plumbing
documents the contract for them. See [ROADMAP §Open conformance
follow-ups](../ROADMAP.md).

#### 3.1.2 §8 data-filter runtime is server-policy

Same situation as §7. Spec §8 defines the data-filter language for
"return only field N of the complex value" style filters using
`UnfilteredItemId` (608) + `DataFilterValue` (609). Opc.Classic ships
the property plumbing + HRESULT codes (`OPCCPX_E_FILTER_*`) but the
filter-expression evaluation is server-specific. Status: **WAIVED**.

#### 3.1.3 No end-to-end CPX sample server

Opc.Classic ships sample servers for DA / AE / HDA / OpcEnum /
OpcSecurity but no Complex Data sample. A reference sample would
demonstrate property-based type discovery, CPX namespace browsing, and
how to wire `OpcCpxAddressSpace` + `OpcCpxItemProperties` into an
existing `OpcDaServerHost`. Status: **WAIVED** (deferred) — the unit
tests under `tests/Opc.Classic.Cpx.Tests/Hosting/` cover the hosting
helpers in isolation, but no end-to-end probe-server scenario exists.

### 3.2 Hard gaps

None at present. Codecs, type model, property IDs, HRESULTs, namespace
helpers, and DA hosting helpers all conform to the spec. The
soft-gap §7 / §8 runtimes are explicitly server-policy per the spec text
itself (the spec only standardises the *representation* of conversion
expressions and filters, not the *execution* engine).

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC Complex Data 1.00](../CONFORMANCE.md#opc-complex-data-100)
- Related spec: [`docs/conformance/opc-da-3-00.md`](opc-da-3-00.md) (DA 3.0 carries `IOPCBrowse` and `IOPCItemProperties` which the CPX namespace + properties extend)
- Related spec: [`docs/conformance/opc-xmlda-1-01.md`](opc-xmlda-1-01.md) (XML-DA also references the `http://opcfoundation.org/ComplexData/1.0/` namespace for complex values in SOAP responses)
- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC DA 3.00 (IOPCItemProperties)](../CONFORMANCE.md#opc-da-300)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-CPX-1.00.md` (OPC Complex Data
Specification 1.00, December 10, 2003).

Phase 0 inventory:

- `files/conformance/inventory/opc-cpx-1-00-headings.csv` (46 entries)
- `files/conformance/inventory/opc-cpx-1-00-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-cpx-1-00-interfaces.csv` (0 entries — CPX spec text does not name COM interfaces using the `IOPC*` token form; surface mapping is by spec section above)
