# OPC Complex Data 1.00 — Spec Coverage Review

**Spec**: OPC Complex Data Specification Version 1.00 (December 10, 2003)  
**Implementation**: `src/Opc.Classic.Cpx/`  
**Review date**: 2025-01-XX  
**Reviewer**: Spec coverage analysis agent

---

## Summary

**Interfaces**: 3/3 declared (100%)  
**Methods**: 11/11 declared (100%)  
**Core types**: 6/6 implemented (100%)  
**XML/binary codecs**: 0/2 (0% — deferred to Phase 9B)  
**DA Properties**: 0/10 defined (0%)  
**Overall compliance**: **PARTIAL** — DCOM interface projection complete, managed type model complete, but **missing** all XML/OPCBinary codecs, DA property constants (600–609), CPX namespace helpers, and type-system integration with DA servers.

### Severity Breakdown

- **BLOCKER**: 2 (XML/OPCBinary codecs, DA property constants)  
- **HIGH**: 2 (CPX namespace support, type conversion/filter infrastructure)  
- **MEDIUM**: 1 (Error codes not defined)  
- **LOW**: 1 (Documentation/examples)

---

## Gaps in Implementation

### BLOCKER

#### 1. XML Schema and OPCBinary Codecs — Missing

**Spec**: §3.2 Complex Data Type Descriptions, §5.1 XML Schema, §6 OPC Binary Type System  
**Status**: Missing  
**Impact**: 

The spec defines two type systems for complex data:

1. **XMLSchema** (§3.3.1, §5.1): Complex data values transported as XML documents with `xsi:type` attributes. Type ID is the `name` attribute of the `element` or `complexType`. Nested types use slash-delimited names (e.g., `Connection/Status`).

2. **OPCBinary** (§3.3.1, §6): Binary data values transported as `byte[]`. Dictionary and Type Description are XML documents conforming to the OPCBinary schema (Appendix A). Defines primitive types (BitString, CharString, Integer, FloatingPoint) and TypeReference for nested structs.

**Current state**:
- `TypeDictionary`, `TypeDescription`, `TypeField`, `StructType`, `StructField` managed types exist
- `TypeKind` enum defines primitive types (Boolean, Int8–UInt64, Single, Double, String, FileTime, Guid, Blob, StructReference)
- `ByteOrder`, `ComplexValue`, `InstanceDescription` types exist
- **Missing**: XML serializers/deserializers for `TypeDictionary` (OPCBinary schema)
- **Missing**: Binary codecs to encode/decode `byte[]` item values per `TypeDescription`
- **Missing**: XML Schema parser for XMLSchema type system
- **Missing**: XML item value serializer/deserializer for XMLSchema

**Example** (from spec §5.2):

```xml
<TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/"
                DefaultBigEndian="true">
  <TypeDescription TypeID="BlockHeader">
    <CharString Name="Block Tag" xsi:type="Ascii" Length="32" />
    <Integer Name="Execution Time" xsi:type="Int32" />
    <Integer Name="Execution Frequency" xsi:type="Int32" />
    <Integer Name="Number of Parameters" xsi:type="Int16" />
  </TypeDescription>
</TypeDictionary>
```

This must be:
1. Parsed into `TypeDictionary`/`TypeDescription` instances
2. Used to decode `byte[]` item values into `ComplexValue` field bags
3. Used to encode `ComplexValue` instances back to `byte[]` for writes

**Severity**: **BLOCKER** — Without codecs, clients/servers cannot:
- Read/write complex item values
- Parse `Dictionary` property (ID 603)
- Parse `Type Description` property (ID 604)
- Interpret binary payloads from OPCBinary items
- Generate/consume XMLSchema complex values

**Recommendation**: Phase 9B follow-up — implement:
- `OpcBinaryDictionaryParser` (XML → `TypeDictionary`)
- `OpcBinaryEncoder`/`OpcBinaryDecoder` (byte[] ↔ `ComplexValue` per `TypeDescription`)
- `XmlSchemaTypeSystemParser` (XMLSchema → `TypeDictionary`)
- `XmlComplexValueSerializer` (XMLSchema complex values ↔ `ComplexValue`)

---

#### 2. DA Property Constants (IDs 600–609) — Missing

**Spec**: §3.3 Complex Data Type Item Properties (Table on line 755–798)  
**Status**: Missing  
**Impact**: 

The spec defines 10 DA item properties (IDs 600–609) for complex data:

| Prop ID | Name | Data Type | Usage |
|---------|------|-----------|-------|
| 600 | Type System ID | string | Mandatory — identifies type system ("XMLSchema" or "OPCBinary") |
| 601 | Dictionary ID | string | Mandatory — identifies dictionary version |
| 602 | Type ID | string | Mandatory — identifies type within dictionary |
| 603 | Dictionary | BLOB | Optional — full dictionary (XML Schema or OPCBinary XML) |
| 604 | Type Description | BLOB | Optional — single type description |
| 605 | Consistency Window | string | Optional — time consistency ("0", "100", "Unknown", "Not Consistent") |
| 606 | Write Behavior | string | Optional — "All or Nothing" (default) or "Best Effort" |
| 607 | Unconverted Item ID | string | Mandatory for type conversions (§7) |
| 608 | Unfiltered Item ID | string | Mandatory for data filters (§8) |
| 609 | Data Filter Value | string | Mandatory for data filters (§8) |

**Current state**:
- No `OpcComplexDataProperty` constants class in `src/Opc.Classic.Cpx/`
- No integration with DA property mechanism (compare to `OpcDataAccessProperty` in `src/Opc.Classic.Da/`)
- Managed types (`InstanceDescription`, `TypeDictionary`) exist but are not wired to DA properties

**Example usage** (from spec §5.1):

A server exposes `Connection` item with properties:

```csharp
// Property 600 (Type System ID)
"XMLSchema"

// Property 601 (Dictionary ID)
"http://opcfoundation.org/ComplexData/Sample1.xsd"

// Property 602 (Type ID)
"Connection"

// Property 603 (Dictionary)
<?xml version="1.0" encoding="utf-8" ?>
<xsd:schema xmlns=http://opcfoundation.org/ComplexData/Sample1.xsd
            targetNamespace=http://opcfoundation.org/ComplexData/Sample1.xsd>
  <xs:element name="Connection">...</xs:element>
</xsd:schema>
```

Clients use these properties to:
1. Identify type system (property 600)
2. Cache dictionaries (property 601 + 603)
3. Decode item value (property 602 + dictionary)

**Severity**: **BLOCKER** — Without property constants:
- Servers cannot expose complex data metadata via DA properties
- Clients cannot discover type descriptions
- No integration with standard OPC DA 2.0/3.0 property mechanism

**Recommendation**: Add `OpcComplexDataProperty.cs`:

```csharp
public static class OpcComplexDataProperty
{
    public const int TypeSystemId = 600;
    public const int DictionaryId = 601;
    public const int TypeId = 602;
    public const int Dictionary = 603;
    public const int TypeDescription = 604;
    public const int ConsistencyWindow = 605;
    public const int WriteBehavior = 606;
    public const int UnconvertedItemId = 607;
    public const int UnfilteredItemId = 608;
    public const int DataFilterValue = 609;
}
```

---

### HIGH

#### 3. CPX Namespace Support — Missing

**Spec**: §3.4 Complex Data Namespace  
**Status**: Missing  
**Impact**: 

Servers may expose a `/CPX` branch in the DA address space for browsing supported type systems, dictionaries, and type IDs:

```
/CPX
  /XMLSchema
    /Sample1 (Dictionary ID item)
      /Connection (Type ID item)
      /Connection/Status (Nested type)
  /OPCBinary
    /PlantTypes (Dictionary ID item)
      /MotorStatus (Type ID item)
```

**Structure** (from spec §3.4, Figure 6):

- `/CPX` — reserved branch for complex data metadata
  - `/{TypeSystemID}` — branch per type system ("XMLSchema", "OPCBinary")
    - `/{DictionaryName}` — branch + item per dictionary
      - Value: current Dictionary ID (for change detection via subscription)
      - Property 603 (Dictionary): full dictionary BLOB
    - `/{DictionaryName}/{TypeID}` — branch + item per type
      - Value: Type ID (never changes)
      - Property 604 (Type Description): type-specific BLOB

**Benefits**:
- Clients can browse all available type systems and dictionaries
- Clients can subscribe to Dictionary ID items to detect dictionary changes
- Clients can fetch Type Descriptions on-demand (property 604) vs. full dictionary (property 603)

**Current state**:
- No `CpxNamespaceBuilder` or similar helper to construct CPX branches
- No DA server integration
- Comment in `IOPCCpxInterfaces.cs` mentions "CPX codecs" as deferred

**Severity**: **HIGH** — Optional per spec §3.4, but commonly implemented for type discovery. Without this:
- Clients must already know type IDs
- Clients must fetch full dictionary (property 603) vs. targeted type (property 604)
- No standard browse path for type metadata

**Recommendation**: Add CPX namespace builder helpers in Phase 9B for server-side construction.

---

#### 4. Type Conversion and Data Filter Infrastructure — Missing

**Spec**: §7 Type Conversions, §8 Data Filters and Queries  
**Status**: Missing (framework exists but no runtime integration)  
**Impact**: 

**Type Conversions** (§7): Servers expose alternate formats under `/ItemID/CPX/{Format}` branch:

```
/Sample/Connections/Device00 (native OPCBinary item)
  /CPX
    /XML (XMLSchema conversion)
```

- Property 607 (Unconverted Item ID) = `/Sample/Connections/Device00`
- Clients can read/write in alternate formats
- Server performs conversion

**Data Filters** (§8): Servers accept write-only filter parameters under `/ItemID/CPX/DataFilters`:

```
/Sample/Connections/Device00/CPX/XML/DataFilters (write-only branch)
```

Client writes filter XML:

```xml
<DataFilter Name="Filter01">
  /*/ConnectFailCount
</DataFilter>
```

Server creates:

```
/Sample/Connections/Device00/CPX/XML/DataFilters/Filter01 (read-only filtered item)
```

- Property 608 (Unfiltered Item ID) = parent item
- Property 609 (Data Filter Value) = filter string

**Benefits**:
- Native/XML conversions for interoperability
- XPath/SQL-style queries for partial reads
- Field masking for OnDataChange optimization

**Current state**:
- Properties 607–609 not defined
- No type-conversion helpers
- No filter/query parser
- Comments indicate deferred to Phase 9B

**Severity**: **HIGH** — Optional advanced features, but §7 type conversions are common (e.g., exposing binary data as XML for web clients). §8 data filters are less common.

**Recommendation**: Phase 9B — implement after codecs.

---

### MEDIUM

#### 5. Error Codes (OPCCPX_*) — Not Defined

**Spec**: §9 Error Codes  
**Status**: Missing  
**Impact**: 

The spec defines 5 CPX-specific error codes:

| Name | Value | Description |
|------|-------|-------------|
| `OPCCPX_E_TYPE_CHANGED` | 0x809A0000 | Dictionary and/or type description changed |
| `OPCCPX_E_FILTER_DUPLICATE` | 0x809A0001 | Data filter with same name exists |
| `OPCCPX_E_FILTER_INVALID` | 0x809A0002 | Data filter syntax invalid |
| `OPCCPX_E_FILTER_ERROR` | 0x809A0003 | Error applying filter to data |
| `OPCCPX_S_FILTER_NO_DATA` | 0x009A0004 | Data filter excluded all fields (success with empty result) |

**Current state**:
- No `OpcComplexDataResult` or `CpxHResult` constants
- Compare to `OpcDataAccessResult` in `src/Opc.Classic.Da/`

**Usage**:

1. **E_TYPE_CHANGED** (§3.3.2): Server detects dictionary ID change after client added item to group → all reads/writes return this error until client re-fetches metadata

2. **E_FILTER_DUPLICATE** (§8.2): Client writes filter with name that already exists

3. **E_FILTER_INVALID** (§8.2): XPath syntax error, unknown field name, etc.

4. **E_FILTER_ERROR** (§8.2): Runtime error applying filter (e.g., database query failed)

5. **S_FILTER_NO_DATA** (§8.2): Filter matched no fields (e.g., XPath returned empty set)

**Severity**: **MEDIUM** — Error codes are critical for robust implementations but can use generic DA errors initially.

**Recommendation**: Add `OpcComplexDataResult.cs`:

```csharp
public static class OpcComplexDataResult
{
    public const int E_TYPE_CHANGED = unchecked((int)0x809A0000);
    public const int E_FILTER_DUPLICATE = unchecked((int)0x809A0001);
    public const int E_FILTER_INVALID = unchecked((int)0x809A0002);
    public const int E_FILTER_ERROR = unchecked((int)0x809A0003);
    public const int S_FILTER_NO_DATA = 0x009A0004;
}
```

---

### LOW

#### 6. Spec Examples and Documentation — Minimal

**Spec**: §5 Complex Data Examples (XML Schema, Function Block)  
**Status**: No example server/client demonstrating:
- Property-based type discovery
- OPCBinary codec usage
- CPX namespace browsing
- Type conversions
- Data filters

**Current state**:
- Tests in `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs` validate managed types
- No end-to-end DA integration tests
- No sample server exposing complex items

**Severity**: **LOW** — Implementation exists, examples are helpful but not blocking.

**Recommendation**: Add to `samples/` after Phase 9B codecs:
- `Opc.Classic.Samples.CpxServer` — exposes motor status, function block header (OPCBinary)
- `Opc.Classic.Samples.CpxClient` — browses CPX namespace, fetches dictionaries, decodes values

---

## Coverage Gaps (Spec Compliance Checklist)

### 1. **DCOM Interface Projection** ✅ COMPLETE

**Spec**: Custom interfaces not in standard IDL, but commonly implemented as extensions.  
**Status**: 3/3 interfaces declared in `IOPCCpxInterfaces.cs`:

- `IOPCComplexDataItem` (IID `7ECE6649-2C1E-494A-BB99-22D36FB3B0C3`)  
  - `GetTypeItemID` (opnum 3) ✅  
  - `GetUnconvertedItemID` (opnum 4) ✅  
  - `GetDataFilter` (opnum 5) ✅  
  - `SetDataFilter` (opnum 6) ✅  

- `IOPCComplexDataItem2` (IID `44F68398-60AF-4F02-9442-172D058CB16F`)  
  - `GetTypeID` (opnum 3) ✅ — returns Guid instead of string (extension)
  - `GetDictionaryID` (opnum 4) ✅  
  - `GetAvailableFilters` (opnum 5) ✅  

- `IOPCTypeLibrary` (IID `B8C1B2C6-ACB7-4B7B-87B5-6EAC2CF63C31`)  
  - `GetDictionary` (opnum 3) ✅  
  - `GetTypeID` (opnum 4) ✅  
  - `GetTypeItemID` (opnum 5) ✅  

**Note**: These interfaces are not in the official spec §6 (which only defines item properties and namespace conventions), but they are industry-standard extensions found in vendor implementations. The spec intentionally avoids defining custom interfaces, relying on properties alone. Implementation includes these for compatibility with real-world servers.

---

### 2. **Managed Type Model** ✅ COMPLETE

**Spec**: §6 OPC Binary Type System  
**Status**: Core types implemented:

- `TypeDictionary` ✅ — OPCBinary dictionary with defaults (§6.2.1)
- `TypeDescription` ✅ — Type entry with fields (§6.2.2)
- `TypeField` ✅ — Field descriptor (§6.2.3)
- `TypeKind` ✅ — Primitive types (§6.2.4.2)
- `StructType` ✅ — Struct definition
- `StructField` ✅ — Struct field
- `ComplexValue` ✅ — Decoded value bag
- `InstanceDescription` ✅ — Item metadata + field values
- `ByteOrder` ✅ — Big/Little endian

**Missing XML/binary codecs** (see BLOCKER #1).

---

### 3. **Complex Data Behavior** ⚠️ PARTIAL

**Spec**: §4 Complex Data Behavior  
**Status**:

- **Type Conversion** (§4): ❌ Servers must reject non-VT_EMPTY requested types → Not implemented
- **OnDataChange** (§4): ⚠️ Deadband applies to any element exceeding threshold → Deferred to DA server runtime
- **Quality** (§4): ⚠️ Complex item quality = poorest element quality → Deferred to DA server runtime
- **Timestamp** (§4): ⚠️ Complex item timestamp = latest element timestamp → Deferred to DA server runtime
- **Time Consistency Window** (§4): ❌ Property 605 not defined (see BLOCKER #2)
- **Write** (§4): ❌ All or Nothing / Best Effort (property 606 not defined)

**Recommendation**: DA server runtime must implement these rules when reading/writing complex items.

---

### 4. **Well-known Type System IDs** ✅ DOCUMENTED

**Spec**: §3.3.1 Type System ID Property  
**Status**: Constants defined:

- `TypeDictionary.OpcBinaryTypeSystemId = "OPCBinary"` ✅
- XMLSchema = `"XMLSchema"` (not constant, but used in `InstanceDescription` default) ✅

**Spec values**:
- `XMLSchema` — W3C XML Schema (§3.3.1)
- `OPCBinary` — OPCBinary XML dictionary (§3.3.1)

---

### 5. **Standard Field Types** ✅ MAPPED

**Spec**: §6.2.4 Standard Field Types  
**Status**: `TypeKind` enum maps to OPCBinary primitives:

| OPCBinary Type | TypeKind | Spec § |
|----------------|----------|--------|
| BitString | (not mapped) | 6.2.4.2.1 |
| CharString | String | 6.2.4.2.2 |
| Ascii | String | 6.2.4.2.2 |
| Unicode | String | 6.2.4.2.2 |
| Integer | Int8–Int64, UInt8–UInt64 | 6.2.4.2.3 |
| FloatingPoint | Single, Double | 6.2.4.2.4 |
| TypeReference | StructReference | 6.2.4.1 |
| FILETIME | FileTime | §6.2.3 Format |
| GUID | Guid | (extension) |
| BLOB | Blob | (extension) |

**Gap**: `BitString` not represented — no managed equivalent for bit fields with non-byte-aligned offsets. Spec §6.2.4.2.1 allows bit fields that don't align to byte boundaries.

**Recommendation**: Add `TypeKind.BitString` + bit-offset field to `StructField` for completeness (LOW priority — rare in practice).

---

### 6. **Dictionary Defaults** ✅ IMPLEMENTED

**Spec**: §6.2.1 OPC Binary Dictionary attributes  
**Status**: `TypeDictionary` properties match spec defaults:

| Attribute | Default | Impl |
|-----------|---------|------|
| DefaultBigEndian | true | ✅ (constructor default) |
| DefaultStringEncoding | "UCS-2" | ✅ const |
| DefaultCharWidth | 2 | ✅ (constructor default) |
| DefaultFloatFormat | "IEEE-754" | ✅ const |

---

## Recommendations by Priority

### Phase 9B (Blockers)

1. **XML/OPCBinary Codecs** — Implement serializers/deserializers
   - `OpcBinaryDictionaryParser.Parse(string xml)` → `TypeDictionary`
   - `OpcBinaryDecoder.Decode(byte[] data, TypeDescription type)` → `ComplexValue`
   - `OpcBinaryEncoder.Encode(ComplexValue value, TypeDescription type)` → `byte[]`
   - `XmlSchemaParser.Parse(string xsd)` → `TypeDictionary` (simplified)
   - `XmlComplexValueSerializer.Deserialize(string xml, TypeDescription type)` → `ComplexValue`
   - `XmlComplexValueSerializer.Serialize(ComplexValue value, TypeDescription type)` → `string`

2. **DA Property Constants** — Add `OpcComplexDataProperty.cs` with IDs 600–609

3. **CPX Namespace Helpers** — Add server-side builder for `/CPX/{TypeSystem}/{Dictionary}/{TypeID}` branches

4. **Error Codes** — Add `OpcComplexDataResult.cs` with OPCCPX_* codes

### Phase 9C (Enhancements)

5. **Type Conversion Framework** — Implement §7 infrastructure (post-codec)

6. **Data Filter Framework** — Implement §8 infrastructure (post-codec)

7. **Sample Server/Client** — Add to `samples/` demonstrating full workflow

### Future (Low Priority)

8. **BitString Support** — Add `TypeKind.BitString` for non-byte-aligned fields

9. **Integration Tests** — End-to-end DA property queries, codec round-trips, CPX browse

---

## Test Coverage Assessment

**Current tests** (`tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs`):

✅ `TypeDescription` value equality and field sequence  
✅ Constructor validation (empty names/IDs, unknown type kind)  
✅ `TypeField` normalization and negative count rejection  
✅ `InstanceDescription` value equality and field lookup  
✅ `TypeDictionary` lookup by name/type ID, case sensitivity, duplicate rejection  
✅ DCOM interface IID validation (3/3 interfaces)

**Missing integration tests**:

❌ DA property queries (600–609) on complex items  
❌ OPCBinary dictionary XML parsing  
❌ OPCBinary codec round-trip (encode + decode = identity)  
❌ XMLSchema type system parsing  
❌ XML complex value serialization  
❌ CPX namespace browsing  
❌ Type change detection (E_TYPE_CHANGED error)  
❌ Data filter creation/deletion  
❌ Consistency window writes  
❌ Write behavior (All or Nothing vs. Best Effort)

**Recommendation**: Add integration tests after Phase 9B codec implementation.

---

## Compliance Summary

| Feature | Spec § | Status | Priority |
|---------|--------|--------|----------|
| DCOM interfaces | (extension) | ✅ 3/3 declared | N/A |
| Managed type model | §6 | ✅ Complete | N/A |
| XML/OPCBinary codecs | §6, §5.1 | ❌ Missing | BLOCKER |
| DA property constants | §3.3 | ❌ Missing | BLOCKER |
| CPX namespace | §3.4 | ❌ Missing | HIGH |
| Type conversions | §7 | ❌ Missing | HIGH |
| Data filters | §8 | ❌ Missing | HIGH |
| Error codes | §9 | ❌ Missing | MEDIUM |
| BitString support | §6.2.4.2.1 | ⚠️ Partial | LOW |
| Examples/docs | §5 | ⚠️ Minimal | LOW |

**Overall**: Infrastructure complete, runtime integration blocked on codecs and property constants. Phase 9B delivery will enable full OPC CPX 1.00 compliance.

---

## References

- **OPC Complex Data Specification 1.00** (December 10, 2003)  
  - `External/Docs/opc-cpx-1.00-specification.md` (lines 1–3200+)

- **Implementation**:  
  - `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs` (DCOM interfaces)  
  - `src/Opc.Classic.Cpx/TypeDictionary.cs` (managed dictionary)  
  - `src/Opc.Classic.Cpx/TypeDescription.cs` (managed type)  
  - `src/Opc.Classic.Cpx/ComplexValue.cs` (decoded value bag)  
  - `src/Opc.Classic.Cpx/InstanceDescription.cs` (item metadata)  
  - `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs` (unit tests)

- **Related specs**:  
  - OPC DA 2.05a (Property mechanism — IDs 600+ are extension range)  
  - W3C XML Schema 1.0 (XMLSchema type system)  
  - S88.01 Batch Control (example complex data in §5.2)
