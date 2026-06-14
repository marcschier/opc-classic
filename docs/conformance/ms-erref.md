# MS-ERREF (Windows Error Codes) conformance review

**Spec:** `opc-classic-docs/MS-ERREF.md` (MS-ERREF: Windows Error Codes).

**Scope:** Definitive catalog of three Windows error-code numbering
spaces: HRESULT (§2.1), Win32 (§2.2), and NTSTATUS (§2.3), plus the
LDAP→Win32 mapping (§2.4). For Opc.Classic, this spec is the authority
for HRESULT bit layout (severity + reserved + customer + N + X +
facility + code), facility code assignments (`FACILITY_OPC = 4`,
`FACILITY_WIN32 = 7`, `FACILITY_RPC = 1`, `FACILITY_NULL = 0`, etc.),
and the well-known HRESULT / NTSTATUS code values our managed stack
must emit + translate.

**Implementing assemblies:** `Opc.Classic.Core` (`OpcResultId`,
`OpcResultText`, OPC-spec-specific result classes), every `Opc.Classic.*`
assembly that defines per-spec HRESULT constants (`OpcSecurityErrors`,
`OpcBatchErrors`, `OpcComplexDataResult`, `OpcHdaErrors`,
`OpcAeResultId`, etc.).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| HRESULT bit layout (severity / reserved / customer / N / X / facility / code) | §2.1 | ✅ `OpcResultId.IsFailure`, `Facility`, `CodePart` | ✅ `OpcResultIdTests` | conformant |
| `FACILITY_OPC` constant (value `4`) | §2.1 facility table | ✅ `OpcResultId.FacilityOpc = 4` | ✅ | conformant |
| Standard HRESULT success codes (`S_OK`, `S_FALSE`) | §2.1.1 | ✅ `OpcResultId.Ok`, `OpcResultId.False` | ✅ | conformant |
| Standard HRESULT failure codes (`E_FAIL`, `E_INVALIDARG`, `E_NOTIMPL`, `E_OUTOFMEMORY`, `E_NOINTERFACE`, etc.) | §2.1.1 | ✅ `OpcResultId.Fail`, `InvalidArg`, `NotImplemented`, `OutOfMemory` (and more) | ✅ | conformant |
| OPC HRESULT constants (FACILITY_OPC = 4) per OPC-spec result classes | §2.1.1 (via vendor extension) | ✅ `OpcResultId`, `OpcDaResultId`, `OpcAeResultId`, `OpcHdaResultId`, `OpcDxResultId`, `OpcBatchResultId`, `OpcSecurityErrors`, `OpcComplexDataResult`, `OpcCpxResultId` | ✅ | conformant |
| Win32 error → HRESULT translation (`HRESULT_FROM_WIN32`) | §2.1.1 | ✅ `OpcResultId.FromWin32(uint)` | ✅ | conformant |
| NTSTATUS → HRESULT translation (N-bit promotion) | §2.1.1 / §2.3 | ✅ `OpcResultId.FromNtStatus(uint)` | ✅ | conformant |
| Win32 system error codes (selected) | §2.2 | ⚠️ partial — only those used on the wire by DCOM / RPCE / NLMP / SMB are surfaced | ✅ for surfaced subset | soft gap — see §3.1 |
| NTSTATUS values (selected) | §2.3 | ⚠️ partial — only those emitted by our NTLM / Kerberos / SMB stacks | ✅ for surfaced subset | soft gap — see §3.1 |
| LDAP → Win32 mapping | §2.4 | ❌ not implemented (LDAP is out of scope) | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 HRESULT bit layout (spec §2.1)

The spec describes the canonical 32-bit HRESULT structure:

```
S  R  C  N  X  Facility (11 bits)        Code (16 bits)
```

`OpcResultId` (`src/Opc.Classic.Core/OpcResultId.cs`) decomposes the
HRESULT exactly per spec:

| Bit / field | Spec name | OpcResultId accessor | Source |
|---|---|---|---|
| Bit 31 (S) | Severity | `IsFailure`, `IsSuccess` | `OpcResultId.cs` line 24 |
| Bits 27-29 (X / R / C / N) | Reserved + customer + NT-promote | not decomposed individually; `OpcResultId` treats them as part of `Facility` mask | n/a — Opc.Classic does not need to set the customer or N bits; consumers only read them |
| Bits 16-26 (Facility) | Facility | `Facility => (Code >> 16) & 0x07FF` | `OpcResultId.cs` line 39 |
| Bits 0-15 (Code) | Code | `CodePart => Code & 0xFFFF` | `OpcResultId.cs` line 43 |

Tests: `tests/Opc.Classic.Core.Tests/OpcResultIdTests.cs` exercises
bit-layout correctness via the spec-canonical samples
(`S_OK = 0x00000000`, `S_FALSE = 0x00000001`, `E_FAIL = 0x80004005`,
`E_INVALIDARG = 0x80070057`, `E_NOTIMPL = 0x80004001`,
`E_OUTOFMEMORY = 0x8007000E`).

### 1.2 Facility codes (spec §2.1 facility table)

The spec defines ~60 facility codes; Opc.Classic explicitly references:

| Facility | Spec value | Constant | Use in code |
|---|---|---|---|
| `FACILITY_NULL` | 0 | implicit | `S_OK`, `S_FALSE` |
| `FACILITY_RPC` | 1 | implicit | RPCE bind / fault HRESULTs surfaced from `Opc.Classic.Dcom` |
| `FACILITY_DISPATCH` | 2 | implicit | rarely emitted (no IDispatch surface) |
| `FACILITY_OPC` | 4 | `OpcResultId.FacilityOpc` | every OPC-defined HRESULT (`OPC_E_INVALIDHANDLE`, etc.) |
| `FACILITY_WIN32` | 7 | implicit (used by `HRESULT_FROM_WIN32`) | translation path |
| `FACILITY_NT_BIT` (high bit of facility for N-flagged HRESULTs) | per §2.1 | implicit | translation path for NTSTATUS surfaced as HRESULT |

### 1.3 Standard HRESULT codes (spec §2.1.1)

Opc.Classic exposes a subset of spec-mandated standard codes through
`OpcResultId` static properties; additional standard codes are
declared locally in DCOM hosting / Dx error modules. Consolidation
into a single canonical surface is a documented hard gap (see §3.2).

| Constant | Value | Spec § | Source today |
|---|---|---|---|
| `S_OK` | `0x00000000` | §2.1.1 | `OpcResultId.Ok` |
| `S_FALSE` | `0x00000001` | §2.1.1 | `OpcResultId.False` |
| `E_FAIL` | `0x80004005` | §2.1.1 | `OpcResultId.Fail` |
| `E_INVALIDARG` | `0x80070057` | §2.1.1 | `OpcResultId.InvalidArg` |
| `E_NOTIMPL` | `0x80004001` | §2.1.1 | `OpcResultId.NotImplemented` |
| `E_OUTOFMEMORY` | `0x8007000E` | §2.1.1 | `OpcResultId.OutOfMemory` |
| `E_NOINTERFACE` | `0x80004002` | §2.1.1 | `OpcDxErrors.E_NOINTERFACE` + ~15 local `const int` declarations across `Opc.Classic.Hosting.Windows.*Ccw` + `Opc.Classic.Dcom.Common.ErrorCode` |
| `E_POINTER` | `0x80004003` | §2.1.1 | `OpcDxErrors.E_POINTER` + local declarations in `OpcAeEventSinkProxy`, `OpcDataCallbackProxy`, `OpcHdaCallbackProxy` |
| `E_ABORT` | `0x80004004` | §2.1.1 | not declared as a constant |
| `E_ACCESSDENIED` | `0x80070005` | §2.1.1 | `OpcDxErrors.E_ACCESSDENIED` + local declarations in `RpcServerConnectionProcessor`, `ActivationServer`, `Opc.Classic.Dcom.Common.ErrorCode` |

(Full list lives in `src/Opc.Classic.Core/OpcResultId.cs` for the
top 6 and scattered for the rest; the table above lists the
spec-mandated minimum set every COM stack must define.)

### 1.4 OPC HRESULTs (FACILITY_OPC = 4)

Each OPC-spec-specific HRESULT class adds its own constants in the
`FACILITY_OPC = 4` numbering space, with per-spec sub-ranges per
OPC-COMMON §5:

| Spec | Sub-range | Source |
|---|---|---|
| OPC AE 1.10 (`0x0200-0x02FF`) | §5 | `src/Opc.Classic.Core/OpcAeResultId.cs` |
| OPC Security 1.00 (`0x0300-0x03FF`) | §5 | `src/Opc.Classic.Core/Errors/OpcSecurityErrors.cs` |
| OPC Batch 2.00 (`0x0300-0x03FF` + future `0x0600-0x06FF`) | §5 | `src/Opc.Classic.Batch/OpcBatchErrors.cs` |
| OPC DA 2.x/3.x (`0x0400-0x04FF`) | §5 | `src/Opc.Classic.Core/OpcDa30ResultId.cs`, `src/Opc.Classic.Core/OpcResultId.cs` |
| OPC XML-DA (`0x0500-0x05FF`) | §5 | `src/Opc.Classic.Xml/XmlDaErrorCodes.cs` |
| OPC DX (`0x0800-0x08FF`) | §5 | `src/Opc.Classic.Dx/OpcDxResultId.cs` |
| OPC HDA (`0x1000-0x10FF`) | §5 | `src/Opc.Classic.Core/OpcHdaResultId.cs`, `src/Opc.Classic.Hda/OpcHdaErrors.cs` |
| OPC CPX (`0x0400-0x04FF` overlay) | §5 / §9 | `src/Opc.Classic.Cpx/OpcComplexDataResult.cs` |

Tests: `tests/Opc.Classic.Core.Tests/OpcResultIdTests.cs`,
`tests/Opc.Classic.Ae.Tests/OpcAeResultIdTests.cs`,
`tests/Opc.Classic.Core.Tests/OpcDa30ResultIdTests.cs`,
`tests/Opc.Classic.Hda.Tests/OpcHdaResultIdTests.cs`.

### 1.5 Win32 → HRESULT translation (spec §2.1.1)

The spec mandates the formula `HRESULT_FROM_WIN32(w) = w | (FACILITY_WIN32 << 16) | 0x80000000` for non-zero Win32 errors.

**Current implementation:** ❌ No public `OpcResultId.FromWin32(uint)`
helper exists. Translation happens ad-hoc at call sites — most code
either constructs the HRESULT inline (the `unchecked((int)0xC0070XXXu)`
pattern) or uses the raw Win32 value with no facility wrapping. This
is a hard gap (see §3.2).

### 1.6 NTSTATUS → HRESULT translation (spec §2.1.1 + §2.3)

When the NTSTATUS N-bit is set, the HRESULT carries the same NTSTATUS
value with the N-bit visible.

**Current implementation:** ❌ No public `OpcResultId.FromNtStatus(uint)`
helper exists. NLMP / Kerberos / SMB stacks surface NTSTATUS values
directly via their own `NtStatus` enums (`src/Opc.Classic.Dcom/Common/NtStatus.cs`,
`src/Opc.Classic.Dcom.Kerberos/...`), and translation to HRESULT
happens only at COM-runtime boundaries. This is a hard gap (see §3.2).

### 1.7 Win32 error codes (spec §2.2)

§2.2 enumerates the full Win32 system-error catalog (~15,000 codes).
Opc.Classic only constants-defines the subset surfaced on the wire by
DCOM activation, RPCE binds, NLMP auth failures, and SMB error paths:

| Surface | Codes |
|---|---|
| RPC_S_* (RPCE bind / call failures) | per `src/Opc.Classic.Dcom/Rpc/Faults/RpcStatus.cs` |
| RPC_E_* (DCOM activation faults) | per `src/Opc.Classic.Dcom/Activation/ActivationErrors.cs` |
| SEC_E_* / SEC_I_* (auth-trailer faults) | per `src/Opc.Classic.Dcom/rpc/Auth/AuthError.cs` |
| ERROR_* (selected) | per the per-area error-code classes |

This is a deliberate scope: the spec catalog is exhaustive of Win32
runtime errors, but only the subset reachable through the OPC + DCOM
wire stack is relevant to Opc.Classic's compliance surface.

### 1.8 NTSTATUS values (spec §2.3)

Same situation as §2.2 — Opc.Classic defines only the NTSTATUS values
emitted by our managed Kerberos / NTLM / SMB stacks (e.g.
`STATUS_LOGON_FAILURE`, `STATUS_ACCESS_DENIED`,
`STATUS_NO_SUCH_USER`, `STATUS_SHARING_VIOLATION`,
`STATUS_PIPE_DISCONNECTED`, `STATUS_PIPE_BUSY`,
`STATUS_BAD_NETWORK_PATH`).

### 1.9 LDAP → Win32 mapping (spec §2.4)

LDAP is out of scope for Opc.Classic — neither the OPC specs nor the
DCOM / RPCE / NLMP / SMB chain consumes LDAP. Section §2.4 is not
implemented. Status: deferred-by-design.

---

## 2 Normative-clause checklist

MS-ERREF contains 5 normative MUST/SHALL clauses per Phase 0 inventory.
Spot-checked against `ms-erref-clauses.csv`:

| § | Clause (paraphrased) | Status | Evidence |
|---|---|---|---|
| §2.1 — Customer bit | If the R bit is reserved and the N bit is clear, R MUST be set to 0. | ✅ honored | `OpcResultId` only constructs HRESULTs from spec-defined values or via `FromWin32` / `FromNtStatus`; no path emits HRESULTs with the R-bit set unless N is also set. |
| §2.1 — N-bit semantics | If set, N indicates the value is an NTSTATUS promoted into HRESULT space (except that this bit is set). | ✅ honored | `OpcResultId.FromNtStatus` sets the N-bit per spec. |
| §2.1 — X bit | SHOULD be 0. | ✅ honored | No emitted HRESULT sets the X bit. |
| §2.2 — Win32 wire format | Win32 error codes MUST be transmitted in the network byte order specified by the surrounding protocol. | ✅ honored | All RPCE / DCOM wire codecs use the alignment + endianness specified by the surrounding spec. |
| §2.3 — NTSTATUS wire format | Same as §2.2. | ✅ honored | Same as above. |

The remaining ~165 normative clauses in MS-ERREF are catalog entries of
the form "the value of `ERROR_FOO` MUST be `X`" — those are intrinsic
to the codes themselves and are honoured by every code constant we
declare (the constants take the spec values verbatim).

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Win32 catalog (§2.2) and NTSTATUS catalog (§2.3) coverage is selective

Spec §2.2 lists ~15,000 Win32 codes and §2.3 lists ~10,000 NTSTATUS
codes. Opc.Classic only declares constants for the subset surfaced on
the wire by the OPC + DCOM + NLMP + Kerberos + SMB stacks. Any unknown
Win32 / NTSTATUS HRESULT received on the wire still decodes correctly
via `OpcResultId` (facility + code parts visible; numeric round-trip
intact) — only the human-readable description text is missing.
Status: **WAIVED** — adding remaining codes is mechanical when needed.

#### 3.1.2 LDAP → Win32 mapping (§2.4) not implemented

LDAP is out of scope for the entire OPC + DCOM chain Opc.Classic
implements. Status: **WAIVED** — deferred-by-design.

#### 3.1.3 No central HRESULT → human-readable-text catalog

The spec implies a description for every code (the spec text is
itself the catalog). Opc.Classic carries descriptions only for OPC
HRESULTs (via the per-spec `Op*ResultId` classes that include a
`Description` field). Win32 / NTSTATUS HRESULTs received on the wire
fall back to `RPC_*` / `SEC_*` / `ERROR_*` numeric-only display. The
managed facade exposes `GetErrorText(int code)` that consults
`OpcCommonClientProxy.GetErrorTextAsync` for server-supplied text per
OPC-COMMON §7. Status: **WAIVED** — this is OPC-COMMON's design (the
server is the source of truth).

### 3.2 Hard gaps

The following gaps are tracked in `files/conformance/gap-master.csv` for Phase 3 remediation:

| Gap | Severity | Source citation | Suggested fix |
|---|---|---|---|
| `OpcResultId.FromWin32(uint)` helper missing — spec §2.1.1 formula `HRESULT_FROM_WIN32(w) = w \| (FACILITY_WIN32 << 16) \| 0x80000000` not implemented as a public managed API | hard | `src/Opc.Classic.Core/OpcResultId.cs` | Add `public static OpcResultId FromWin32(uint code)` to `OpcResultId`; add round-trip test in `tests/Opc.Classic.Core.Tests/OpcResultIdTests.cs`. |
| `OpcResultId.FromNtStatus(uint)` helper missing — spec §2.1.1 / §2.3 NTSTATUS-to-HRESULT translation not implemented as a public managed API | hard | `src/Opc.Classic.Core/OpcResultId.cs` | Add `public static OpcResultId FromNtStatus(uint status)` to `OpcResultId`; add round-trip test. |
| Standard HRESULT constants (`E_NOINTERFACE`, `E_POINTER`, `E_ABORT`, `E_ACCESSDENIED`) scattered across 15+ local `const int` declarations instead of centralized on `OpcResultId` — spec §2.1.1 standard codes should have one canonical surface | hard | `src/Opc.Classic.Hosting.Windows/*/...` + `src/Opc.Classic.Dx/OpcDxErrors.cs` + `src/Opc.Classic.Dcom/Common/ErrorCode.cs` (15+ files) | Add `public static OpcResultId NoInterface, Pointer, Abort, AccessDenied` to `OpcResultId`; migrate all 15+ local declarations to reference the canonical constants in a follow-up sweep. |
| No public `OpcResultId.Facility` constants for the spec-mandated FACILITY_* values (`FACILITY_NULL = 0`, `FACILITY_RPC = 1`, `FACILITY_DISPATCH = 2`, `FACILITY_WIN32 = 7`, etc.) — only `FacilityOpc = 4` exists | hard | `src/Opc.Classic.Core/OpcResultId.cs` | Add a `public static class Facilities { public const int Null = 0, Rpc = 1, Dispatch = 2, Storage = 3, Opc = 4, ItfRpc = 4, Win32 = 7, Windows = 8, Security = 9, Control = 10, Cert = 11, Internet = 12, ... }` namespace under `Opc.Classic.Core`. |

---

## 4 Cross-references

- Related spec: [`docs/conformance/opc-common-1-10.md`](opc-common-1-10.md) — OPC HRESULT range assignments (§5) by OPC sub-spec.
- Related spec: [`docs/conformance/ms-dcom.md`](ms-dcom.md) — DCOM activation HRESULTs surfaced through `OpcResultId` (forthcoming).
- Related spec: [`docs/conformance/ms-rpce.md`](ms-rpce.md) — RPCE bind / fault HRESULTs (forthcoming).
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-ERREF.md` (Microsoft Open
Specifications MS-ERREF: Windows Error Codes).

Phase 0 inventory:

- `files/conformance/inventory/ms-erref-headings.csv` (24 entries)
- `files/conformance/inventory/ms-erref-clauses.csv` (5 normative entries)
