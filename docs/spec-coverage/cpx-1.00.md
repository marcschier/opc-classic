# OPC Complex Data 1.00 — Spec Coverage Review

**Spec**: OPC Complex Data Specification Version 1.00 (December 10, 2003)
**Implementation**: `src/Opc.Classic.Cpx/`
**Review target**: `1.0.0-rc.7`

---

## Summary

**Interfaces**: 3/3 extension interfaces declared (100%)
**Methods**: 11/11 declared (100%)
**Core types**: implemented
**XML/OPCBinary support**: implemented for dictionary parsing, XML complex value serialization, and OPCBinary encode/decode
**DA Properties**: IDs 600-609 defined
**Error codes**: `OPCCPX_*` constants defined

**Overall compliance**: **Projection and codec infrastructure complete; DA server runtime integration remains partial**. Earlier claims that codecs, property constants, namespace helpers, and CPX HRESULTs were missing are stale.

---

## Implementation Status

### 1. DCOM Interface Projection

| Interface | Methods | Status | Source |
|---|---:|---|---|
| `IOPCComplexDataItem` | 4/4 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs:20-41` |
| `IOPCComplexDataItem2` | 3/3 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs:43-60` |
| `IOPCTypeLibrary` | 3/3 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs:62-79` |

These are vendor/industry extension interfaces around the CPX property model; the spec itself primarily standardizes DA properties, type dictionaries, namespace conventions, conversion, and filtering.

### 2. CPX Property and Error Constants

| Feature | Status | Source |
|---|---|---|
| Property IDs 600-609 | ✅ | `src/Opc.Classic.Cpx/OpcComplexDataProperty.cs:8-42` |
| CPX HRESULT constants | ✅ | `src/Opc.Classic.Cpx/OpcComplexDataResult.cs:11-48` |

### 3. Type Systems and Codecs

| Feature | Status | Source |
|---|---|---|
| OPCBinary dictionary parser | ✅ | `src/Opc.Classic.Cpx/OpcBinaryDictionaryParser.cs:15-80` |
| OPCBinary decoder | ✅ | `src/Opc.Classic.Cpx/OpcBinaryDecoder.cs:12-60` |
| OPCBinary encoder | ✅ | `src/Opc.Classic.Cpx/OpcBinaryEncoder.cs` |
| XML Schema parser | ✅ | `src/Opc.Classic.Cpx/XmlSchemaParser.cs:15-60` |
| XML complex value serializer | ✅ | `src/Opc.Classic.Cpx/XmlComplexValueSerializer.cs` |
| CPX namespace helper | ✅ | `src/Opc.Classic.Cpx/CpxNamespaceBuilder.cs:11-80` |

---

## Remaining Gaps in Implementation

### HIGH

#### 1. DA server runtime integration

The CPX assembly provides types, parsers, codecs, properties, errors, and namespace helpers, but it does not automatically wire those into a running DA server's address space.

A CPX-capable server still needs to:

1. Expose properties 600-609 on complex DA items.
2. Expose `/CPX/{TypeSystem}/{Dictionary}/{TypeID}` browse branches where desired.
3. Return CPX dictionaries/type descriptions through DA property reads.
4. Apply CPX timestamp, quality, deadband, and write-behavior rules to complex item values.

**Source helpers**: `src/Opc.Classic.Cpx/OpcComplexDataProperty.cs:8-42`, `src/Opc.Classic.Cpx/CpxNamespaceBuilder.cs:11-80`.

---

#### 2. Type conversion and data filter execution

The spec's §7 type-conversion and §8 data-filter behavior require server-side state and policy. Helpers exist for paths and the core data representation, but the runtime does not provide a generic conversion/filter engine.

**Status**: helper infrastructure present; semantic execution is server-specific.
**Priority**: High for a CPX sample server, lower for client-side dictionary/value decoding.

---

### LOW

#### 3. BitString support

`TypeKind` covers the common OPCBinary primitive and structured types, but non-byte-aligned BitString fields remain a completeness gap.

**Priority**: Low; rare in modern server payloads.

#### 4. End-to-end DA/CPX samples

No sample server/client currently demonstrates property-based type discovery, CPX namespace browsing, conversion branches, and data filters.

---

## Test Coverage Assessment

| Test File | Scope |
|---|---|
| `tests/Opc.Classic.Cpx.Tests/CpxCodecTests.cs:1-216` | OPCBinary/XML codec behavior |
| `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs:1-190` | Type model, properties, namespace helpers, errors |
| `tests/Opc.Classic.Cpx.Tests/Dcom/IOPCCpxProxyTests.cs:1-66` | DCOM proxy/interface projection |

Missing tests are now mostly end-to-end DA integration tests rather than core codec/property tests.

---

## Compliance Summary

| Feature | Spec § | Status | Priority |
|---|---|---|---|
| DCOM extension interfaces | vendor extension | ✅ 3/3 declared | N/A |
| Managed type model | §6 | ✅ Complete | N/A |
| XMLSchema parser | §5 | ✅ Implemented | N/A |
| OPCBinary parser/codec | §6 | ✅ Implemented | N/A |
| DA property constants | §3.3 | ✅ Implemented | N/A |
| CPX namespace helpers | §3.4 | ✅ Implemented | N/A |
| Type conversions | §7 | ⚠️ Server-specific runtime work | High |
| Data filters | §8 | ⚠️ Server-specific runtime work | High |
| Error codes | §9 | ✅ Implemented | N/A |
| BitString support | §6.2.4.2.1 | ⚠️ Partial | Low |

---

## Conclusion

The CPX review should no longer describe the implementation as codec-blocked. Core CPX parsing, encoding/decoding, property IDs, HRESULTs, and namespace helpers are present and tested. Remaining work is to integrate those building blocks into a DA server runtime and sample workflows.
