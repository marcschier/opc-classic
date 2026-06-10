# AE wire-format spec — DR32/DR33 real-fix analysis

> **Phase A of the DR32/DR33 real-fix plan.** This document
> captures the exact byte-level wire format that the OPC Foundation
> native `opcae_ps.dll` MIDL proxy/stub expects for
> `IOPCEventServer::GetConditionState` (opnum 12) and
> `IOPCEventServer::AckCondition` (opnum 17). Source: the
> vendored `external/inc/opc_ae_p.c` (119 KB of MIDL-generated
> proxy/stub C source). Cited byte-by-byte against
> ProcFormatString + TypeFormatString offsets in that file.
>
> **Purpose:** ground truth for Phase B (capture managed-stack wire
> bytes) and Phase C (byte-diff to identify the discrepancies that
> cause `opcae_ps.dll` to reject `AckCondition` requests + crash
> on `GetConditionState` responses).
>
> **Scope:** the 3 problem flows blocking DR32-phase4/phase5 and
> DR33-getconditionstate/ackcondition. Other AE methods marshal
> correctly on the `samples-ae` native-CCW path and are not
> covered here.

## Conventions

- Field offsets in `opc_ae_p.c` use the comment `/* NNN */` prefix
  on each format-string byte. All cited offsets reference those
  comments.
- MIDL format-string opcode reference (subset used here):
  - `FC_RP` (0x11) — reference pointer (NO inline referent ID on
    the wire when `simple_pointer` flag set)
  - `FC_OP` (0x13) — object/unique pointer (4-byte inline
    referent ID; data deferred)
  - `FC_UP` (0x12) — unique pointer (same wire format as FC_OP
    in the conformant-array-element context)
  - `FC_PP` (0x4b) — pointer-rich (struct/array contains pointers
    that need deferred body resolution)
  - `FC_VARIABLE_REPEAT` (0x48) + `FC_FIXED_OFFSET` (0x49) —
    repeat the pointer-resolution pattern for each conformant
    array element
  - `FC_C_WSTRING` (0x25) — conformant varying wide string (max +
    offset + actual + chars[])
  - `FC_BOGUS_STRUCT` (0x1a) — struct with embedded complex types
    (pointers/arrays/unions); requires pre-order traversal for
    deferred body resolution
  - `FC_STRUCT` (0x15) — simple struct (no embedded complex)
  - `FC_CARRAY` (0x1b) — conformant array (max_count + elements)
  - `FC_EMBEDDED_COMPLEX` (0x4c) — embedded complex type
    reference (offset to the embedded type)
  - `FC_LONG` (0x8), `FC_SHORT` (0x6), `FC_ULONG` (0x29 corr desc)
  - `[simple_pointer]` (flag 0x8) — the pointer's referent is a
    "simple" type with no further deferred pointers
- Parameter flag values seen here:
  - `0x10b` = must_size + must_free + in + simple_ref (request side)
  - `0x2013` = must_size + must_free + out + srv_alloc_size=8 (response side)
  - `0x48` = in + base_type (scalar)
  - `0x70` = out + return + base_type (HRESULT return)

## GetConditionState (opnum 12)

### Method signature (from `external/inc/opc_ae.idl:206-212`)

```idl
HRESULT GetConditionState (
    [in]                           LPWSTR              szSource,
    [in]                           LPWSTR              szConditionName,
    [in]                           DWORD               dwNumEventAttrs,
    [in, size_is(dwNumEventAttrs)] DWORD*              pdwAttributeIDs,
    [out]                          OPCCONDITIONSTATE** ppConditionState
);
```

### ProcFormatString (opc_ae_p.c:564-612)

| Param | Stack offset | Flags | Wire type | Notes |
| --- | --- | --- | --- | --- |
| szSource | 4 | `0x10b` simple_ref | `FC_RP [simple_pointer] → FC_C_WSTRING` at TypeFormatString[144] | NO outer referent ID; just conformance + chars |
| szConditionName | 8 | `0x10b` simple_ref | same as szSource (TypeFormatString[144]) | |
| dwNumEventAttrs | 12 | `0x48` in/base | `FC_LONG` | 4-byte DWORD |
| pdwAttributeIDs | 16 | `0x10b` simple_ref | `FC_RP → FC_CARRAY` at TypeFormatString[242]; FC_LONG elements, size from param at stack 12 | NO outer referent ID; conformance DWORD + array data |
| ppConditionState | 20 | `0x2013` out/srv_alloc=8 | `FC_RP [alloced_on_stack] [pointer_deref] → FC_OP → OPCCONDITIONSTATE` at TypeFormatString[252→256→1304] | OUTER referent ID (FC_OP) + deferred struct body |
| (return) | 24 | `0x70` out/return | `FC_LONG` (HRESULT) | |

### Request wire layout

```
[common header (16)]
[ORPC envelope (orpcthis: 16 bytes minimum)]
[request stub:]
  szSource (FC_C_WSTRING):
    max_count       (4 bytes, DWORD)
    offset          (4 bytes, DWORD, =0)
    actual_count    (4 bytes, DWORD)
    chars[]         (actual_count * 2 bytes, includes null terminator)
    padding to 4-align
  szConditionName (FC_C_WSTRING):
    same layout as szSource
  dwNumEventAttrs (DWORD, 4 bytes)
  pdwAttributeIDs (FC_CARRAY of FC_LONG):
    max_count       (4 bytes, == dwNumEventAttrs)
    dwNumEventAttrs * 4-byte DWORD elements
[auth pad + verifier header + auth value]  (when sign/sealed)
```

**Critical:** szSource and szConditionName are **simple_ref**. There
is NO outer 4-byte referent ID before each string's conformance
DWORD. Applying `[OpcRefString]` to the managed `IOPCEventServer.
GetConditionState` interface declaration is REQUIRED to match this
on the wire.

The existing investigation confirms `[OpcRefString]` makes the
request decode succeed (CCW logs `ENTER → decoded → RETURN S_OK`,
matrix reaches 103/1). This part is solved.

### OPCCONDITIONSTATE struct (TypeFormatString[1304-1346], referenced from response)

```
FC_BOGUS_STRUCT alignment=4, size=96 bytes
```

**On-wire member layout (96 bytes total):**

| Offset | Size | Field | Type/flag |
| --- | --- | --- | --- |
| 0 | 2 | wState | WORD |
| 2 | 2 | wReserved1 | WORD |
| 4 | 4 | szActiveSubCondition | POINTER (referent ID) |
| 8 | 4 | szASCDefinition | POINTER |
| 12 | 4 | dwASCSeverity | DWORD |
| 16 | 4 | szASCDescription | POINTER |
| 20 | 2 | wQuality | WORD |
| 22 | 2 | wReserved2 | WORD |
| 24 | 8 | ftLastAckTime | FILETIME (FC_EMBEDDED_COMPLEX → TypeFormatString[10]) |
| 32 | 8 | ftSubCondLastActive | FILETIME |
| 40 | 8 | ftCondLastActive | FILETIME |
| 48 | 8 | ftCondLastInactive | FILETIME |
| 56 | 4 | szAcknowledgerID | POINTER |
| 60 | 4 | szComment | POINTER |
| 64 | 4 | dwNumSCs | DWORD |
| 68 | 4 | pszSCNames | POINTER (FC_OP → conformant FC_PP LPWSTR array) |
| 72 | 4 | pszSCDefinitions | POINTER (same shape) |
| 76 | 4 | pdwSCSeverities | POINTER (FC_OP → conformant FC_LONG array) |
| 80 | 4 | pszSCDescriptions | POINTER (same as pszSCNames) |
| 84 | 4 | dwNumEventAttrs | DWORD |
| 88 | 4 | pEventAttributes | POINTER (FC_OP → conformant VARIANT array) |
| 92 | 4 | pErrors | POINTER (FC_OP → conformant FC_LONG array) |

**FILETIME marshals as `FC_STRUCT alignment=4, size=8` containing
2 × `FC_LONG`** (dwLowDateTime + dwHighDateTime) — TypeFormatString[10]:

```
/* 10 */ 0x15 FC_STRUCT, 0x3 (alignment=4), 0x8 0x0 (size=8),
         0x8 FC_LONG, 0x8 FC_LONG, FC_PAD, FC_END
```

**The OPCCONDITIONSTATE FILETIMEs land at offsets 24, 32, 40, 48 —
all multiples of 8.** Even though `FC_STRUCT alignment=4` declares
4-byte alignment, the struct layout (after wReserved2 padding) places
FILETIMEs on 8-byte boundaries naturally. The prior investigation's
"FILETIMEs must stay 8-byte aligned (a 4-align change deterministically
crashes the stub)" reflects this *layout-imposed* 8-byte placement —
not a hidden alignment-override rule on FILETIME itself.

### Pointer layout (TypeFormatString[1348-1390]) — deferred bodies, pre-order

For each `POINTER` field in the OPCCONDITIONSTATE struct body, the
deferred body appears AFTER the entire struct body, in pre-order:

| Order | Field | Deferred body type |
| --- | --- | --- |
| 1 | szActiveSubCondition | `FC_OP [simple_pointer] → FC_C_WSTRING` (TypeFormatString[1348]) |
| 2 | szASCDefinition | same (TypeFormatString[1352]) |
| 3 | szASCDescription | same (TypeFormatString[1356]) |
| 4 | szAcknowledgerID | same (TypeFormatString[1360]) |
| 5 | szComment | same (TypeFormatString[1364]) |
| 6 | pszSCNames | `FC_OP → FC_CARRAY of LPWSTR (FC_PP)` (TypeFormatString[1368→260]) |
| 7 | pszSCDefinitions | same (TypeFormatString[1372→260]) |
| 8 | pdwSCSeverities | `FC_OP → FC_CARRAY of FC_LONG` (TypeFormatString[1376→290]) |
| 9 | pszSCDescriptions | same as pszSCNames (TypeFormatString[1380→260]) |
| 10 | pEventAttributes | `FC_OP → FC_CARRAY of VARIANT (FC_BOGUS_ARRAY)` (TypeFormatString[1384→1276]) |
| 11 | pErrors | `FC_OP → FC_CARRAY of FC_LONG` (TypeFormatString[1388→1294]) |

**On-wire response stub layout (after the outer ppConditionState
referent ID):**

```
[outer FC_OP referent ID (4 bytes, non-zero)]
[OPCCONDITIONSTATE struct body (96 bytes per the layout table above —
 every POINTER field is a 4-byte referent ID, non-zero for non-null)]
[deferred bodies in pre-order:]
  szActiveSubCondition body: FC_C_WSTRING (max + offset + actual + chars + pad)
  szASCDefinition body: FC_C_WSTRING
  szASCDescription body: FC_C_WSTRING
  szAcknowledgerID body: FC_C_WSTRING
  szComment body: FC_C_WSTRING
  pszSCNames body: conformance-DWORD (==dwNumSCs)
                   + dwNumSCs × 4-byte referent IDs (FC_PP inline)
                   + dwNumSCs × FC_C_WSTRING deferred bodies
  pszSCDefinitions body: same structure
  pdwSCSeverities body: conformance-DWORD (==dwNumSCs)
                      + dwNumSCs × 4-byte DWORD values
  pszSCDescriptions body: same structure as pszSCNames
  pEventAttributes body: conformance-DWORD (==dwNumEventAttrs)
                       + dwNumEventAttrs × VARIANT (16 bytes inline + deferred pointer bodies)
  pErrors body: conformance-DWORD (==dwNumEventAttrs)
              + dwNumEventAttrs × 4-byte HRESULT values
```

**Hypothesis for the intermittent `opcae_ps.dll` crash on response
(DR33-getconditionstate):** the prior investigation found
`[OpcRefString]` fixes the request decode but the response then
crashes intermittently with TCP RST. Candidate root causes (to
validate in Phase C against actual managed-encoder output):

1. **Deferred-pointer ordering** — managed encoder may walk the
   POINTER fields in a different order than the MIDL pre-order
   table above (specifically: are szAcknowledgerID/szComment
   emitted BEFORE the 4 deferred arrays?).
2. **VARIANT array marshaling** — pEventAttributes is the most
   complex deferred body in OPCCONDITIONSTATE. If the managed
   VARIANT encoder produces a slightly different shape (extra
   padding, wrong vt field, missing inline-vs-deferred split),
   the receiver could read past the end and crash.
3. **Empty/null string handling** — for fields like
   szActiveSubCondition that may legitimately be NULL: does the
   managed encoder emit referent ID 0 (skip body) or referent ID
   non-zero with empty body (max=0, actual=0)? `opcae_ps.dll`
   likely expects the former.

---

## AckCondition (opnum 17)

### Method signature (from `external/inc/opc_ae.idl:235-244`)

```idl
HRESULT AckCondition(
    [in]                     DWORD     dwCount,
    [in, string]             LPWSTR    szAcknowledgerID,
    [in, string]             LPWSTR    szComment,
    [in, size_is(dwCount)]   LPWSTR*   pszSource,
    [in, size_is(dwCount)]   LPWSTR*   pszConditionName,
    [in, size_is(dwCount)]   FILETIME* pftActiveTime,
    [in, size_is(dwCount)]   DWORD*    pdwCookie,
    [out, size_is(,dwCount)] HRESULT** ppErrors
);
```

### ProcFormatString (opc_ae_p.c:742-808)

| Param | Stack offset | Flags | Wire type | Notes |
| --- | --- | --- | --- | --- |
| dwCount | 4 | `0x48` in/base | `FC_LONG` | conformance source for all 5 array params |
| szAcknowledgerID | 8 | `0x10b` simple_ref | `FC_RP [simple_pointer] → FC_C_WSTRING` at TypeFormatString[144] | NO outer referent |
| szComment | 12 | `0x10b` simple_ref | same as szAcknowledgerID | |
| pszSource | 16 | `0x10b` simple_ref | `FC_RP → FC_CARRAY of LPWSTR (FC_PP)` at TypeFormatString[1396]; element type `FC_UP [simple_pointer] → FC_C_WSTRING` | NO outer referent on the array, BUT each element is a unique pointer with referent ID |
| pszConditionName | 20 | `0x10b` simple_ref | same shape as pszSource (TypeFormatString[1396]) | |
| pftActiveTime | 24 | `0x10b` simple_ref | `FC_RP → FC_CARRAY of FC_EMBEDDED_COMPLEX FILETIME` at TypeFormatString[1430] | NO referents per element — FILETIMEs are inline value types |
| pdwCookie | 28 | `0x10b` simple_ref | `FC_RP → FC_CARRAY of FC_LONG` at TypeFormatString[1448] | inline DWORDs |
| ppErrors | 32 | `0x2013` out/srv_alloc=8 | `FC_RP [alloced_on_stack] [pointer_deref] → FC_OP → FC_CARRAY of FC_LONG` at TypeFormatString[1458] | outer FC_OP referent + deferred conformant DWORD array |
| (return) | 36 | `0x70` out/return | `FC_LONG` (HRESULT) | |

### Request wire layout

```
[common header (16)]
[ORPC envelope]
[request stub:]
  dwCount (DWORD, 4 bytes)
  szAcknowledgerID (FC_C_WSTRING simple_ref):
    max_count, offset, actual_count, chars[], pad to 4-align
  szComment (FC_C_WSTRING simple_ref): same layout
  pszSource (FC_CARRAY of LPWSTR, FC_PP):
    max_count (DWORD, == dwCount)
    [dwCount × 4-byte FC_UP referent IDs — inline part of the array body]
    [DEFERRED: dwCount × FC_C_WSTRING bodies, one per non-null referent]
  pszConditionName (same shape as pszSource):
    max_count + dwCount referent IDs
    [DEFERRED: dwCount × FC_C_WSTRING bodies]
  pftActiveTime (FC_CARRAY of inline FILETIMEs):
    max_count (DWORD, == dwCount)
    dwCount × 8-byte FILETIME values (NO referent IDs — values inline)
    NO deferred bodies
  pdwCookie (FC_CARRAY of inline DWORDs):
    max_count (DWORD, == dwCount)
    dwCount × 4-byte DWORD values
    NO deferred bodies
[auth pad + verifier header + auth value]  (when sign/sealed)
```

**The "TWO deferred FC_PP wstring arrays + an [in] FILETIME array"
structure from DR33's description matches the table above:**

- `pszSource` + `pszConditionName` are both FC_PP arrays of FC_UP
  LPWSTR — each contributes inline referent IDs in the array body
  PLUS deferred FC_C_WSTRING bodies later.
- `pftActiveTime` is an FC_CARRAY of FC_EMBEDDED_COMPLEX FILETIMEs
  — purely inline, no FC_PP / no deferred bodies.
- `pdwCookie` is an FC_CARRAY of FC_LONG — purely inline.

**Deferred body ORDERING (the critical question):** the MIDL
pre-order rule says deferred bodies follow inline data in the
order their pointers appear. So:

1. ALL of pszSource's inline (max + referents) is on the wire
2. ALL of pszConditionName's inline (max + referents) is on the wire
3. ALL of pftActiveTime's inline (max + 8-byte FILETIMEs) is on the wire
4. ALL of pdwCookie's inline (max + DWORD values) is on the wire
5. THEN pszSource[0] FC_C_WSTRING body, pszSource[1] body, …,
   pszSource[dwCount-1] body
6. THEN pszConditionName[0] body, …, pszConditionName[dwCount-1] body

If the managed encoder INTERLEAVES the deferred bodies (e.g.,
emits pszSource[i] body immediately after the inline FC_PP block,
then pszConditionName's inline + bodies after), the wire layout
will not match what `opcae_ps.dll` expects.

**Hypothesis for the `opcae_ps.dll` `[in]` rejection on
AckCondition (DR33-ackcondition-array-investigate):** candidate
root causes (to validate in Phase C):

1. **Deferred-body ordering between pszSource and pszConditionName**
   — managed encoder may emit pszSource's deferred bodies BEFORE
   pszConditionName's inline FC_PP block, instead of after both
   arrays' inline data.
2. **pftActiveTime placement** — managed encoder may treat the
   FILETIME array as deferred (emitting it AFTER the wstring
   bodies) instead of inline immediately after pszConditionName.
   Since FC_CARRAY of FC_EMBEDDED_COMPLEX is inline by spec, any
   deferral would mis-align everything.
3. **pdwCookie placement** — same hypothesis as pftActiveTime.
4. **Conformance DWORD alignment** — every FC_CARRAY starts with
   a 4-byte conformance DWORD. If the managed encoder skips a
   pad-to-4 before the conformance DWORD (after the previous
   variable-length wstring body), the offset shifts.

---

## Cross-references for Phase B (capture managed-stack wire bytes)

When Phase B captures managed-encoder output for these flows,
hex-diff against the layouts above. Specifically look for:

- **GetConditionState request**: does the managed encoder emit
  szSource/szConditionName WITHOUT outer referent IDs (simple_ref)?
- **GetConditionState response**: does the OPCCONDITIONSTATE
  struct body land at the right offsets (96 bytes, FILETIMEs at
  24/32/40/48)? Are deferred bodies in pre-order (5 strings, then
  4 deferred arrays, then 2 more arrays)?
- **AckCondition request**: are the 4 conformant arrays' inline
  data fully written BEFORE any deferred wstring bodies? Are
  pszSource's deferred bodies (dwCount of them) emitted CONTIGUOUSLY
  before pszConditionName's deferred bodies?

## Generator-architecture risk markers

If Phase C reveals any of the following, we hit the **Phase C hard
gate** and stop for operator decision:

- The source-generator emits deferred bodies interleaved with
  inline data (i.e. doesn't model pre-order traversal at all).
  Fixing this requires changing how the generator orchestrates
  multi-parameter marshaling — a generator-architecture change.
- The source-generator does not model FC_PP (pointer-rich array
  with deferred element bodies) and instead emits each LPWSTR
  array element as an FC_OP with referent inline next to its
  body. Fixing this requires a new generator wire-pattern.
- The VARIANT encoder produces a structurally different layout
  (e.g. always inlines VARIANTs even in the deferred context),
  affecting OPCCONDITIONSTATE.pEventAttributes.

If Phase C reveals NONE of the above and the discrepancies are
limited to parameter flag annotations (`[OpcRefString]`-style),
Phase D can proceed with confidence.

---

## Source citations

- `external/inc/opc_ae.idl:206-212` — GetConditionState IDL signature
- `external/inc/opc_ae.idl:235-244` — AckCondition IDL signature
- `external/inc/opc_ae.idl:106-131` — OPCCONDITIONSTATE struct IDL
- `external/inc/opc_ae_p.c:564-612` — GetConditionState ProcFormatString
- `external/inc/opc_ae_p.c:742-808` — AckCondition ProcFormatString
- `external/inc/opc_ae_p.c:1734-1888` — TypeFormatString offsets 0-200
  (FILETIME at 10, szSource/szConditionName at 144, pdwAttributeIDs
  carray at 242, ppConditionState marshal at 252→256)
- `external/inc/opc_ae_p.c:1888-2010` — TypeFormatString offsets
  240-340 (pdwCookie+pftActiveTime+pszSource/pszConditionName carrays
  at 1396/1430/1448, OPCCONDITIONSTATE outer at 1304)
- `external/inc/opc_ae_p.c:2680-2750` — OPCCONDITIONSTATE struct body
  + pointer layout (offsets 1304-1390)
- `external/inc/opc_ae_p.c:2762-2820` — AckCondition array type
  defs (offsets 1396-1458)
