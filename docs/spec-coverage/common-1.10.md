# OPC Common 1.10 Specification Coverage

**Spec version**: OPC Common Definitions and Interfaces 1.10 (December 13, 2002)
**Spec file**: `External/Docs/opc-common-1.10-specification.md`
**Reviewed assemblies**: `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Discovery`, `Opc.Classic.Dcom`

---

## Executive Summary

OPC Common 1.10 defines shared infrastructure used by DA, AE, HDA, DX, Batch, Commands, Security, and XML-DA:

- `IOPCCommon` for locale, error text, and client-name metadata
- `IOPCShutdown` for server-to-client shutdown notifications
- `IOPCServerList` / `IOPCServerList2` and `IOPCEnumGUID` for OPCEnum discovery
- Component-category GUIDs and standard OPC HRESULT codes
- The Appendix B string-filter function

**Coverage**: ✅ **High**. The DCOM interfaces are projected and the discovery client uses real hand-written proxy paths for OPCEnum enumeration. Remaining items are convenience/server-policy gaps rather than missing mandatory wire declarations.

---

## 1. Interfaces

### 1.1 IOPCCommon (§7)

**Spec**: 5 methods — `SetLocaleID`, `GetLocaleID`, `QueryAvailableLocaleIDs`, `GetErrorString`, `SetClientName`

**Implementation status**: ✅ **5/5 DCOM methods declared with source-generated proxy and dispatcher**

| Method | Status | Source |
|---|---|---|
| `SetLocaleID` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `GetLocaleID` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `QueryAvailableLocaleIDs` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `GetErrorString` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `SetClientName` | ✅ DCOM; not a high-level `IDaServer` convenience member | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:116-120` |

`IDaServer` still exposes the common high-value locale/error members (`SetLocaleAsync`, `LocaleId`, `GetSupportedLocalesAsync`, `GetErrorTextAsync`) and treats `SetClientName` as optional diagnostic metadata (`src/Opc.Classic.Da/IDaServer.cs`).

---

### 1.2 IOPCShutdown (§6)

**Spec**: 1 method — `ShutdownRequest(szReason)`

**Implementation status**: ✅ **1/1 DCOM method declared with source-generated proxy and dispatcher**

- Source: `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:729-740`
- Managed DA facade: `IDaServer.ServerShutdown` remains the high-level event pattern for clients.

---

### 1.3 IOPCServerList / IOPCServerList2 (§9)

**Implementation status**: ✅ **Functional OPCEnum discovery client**

`OpcEnumClient` remote-activates `CLSID_OpcEnum`, queries `IOPCServerList2` where available, falls back to `IOPCServerList`, enumerates category matches, and merges descriptors.

**Files**:

- `src/Opc.Classic.Discovery/OpcEnumClient.cs:1-497`
- `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:17-97`
- `src/Opc.Classic.Core/OpcGuids.cs:1-415`

**Tests**:

- `tests/Opc.Classic.Discovery.Tests/OpcEnumClientTests.cs:1-325`
- `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs:1-154`

---

### 1.4 IOPCEnumGUID (§9.6)

**Spec**: `Next`, `Skip`, `Reset`, `Clone`

**Implementation status**: ✅ **4/4 supported in the OPCEnum discovery proxy/dispatcher; generated DA projection declares `Next`, `Skip`, and `Reset`**

| Method | Status | Source |
|---|---|---|
| `Next` | ✅ | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:127-145` |
| `Skip` | ✅ | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:147-160` |
| `Reset` | ✅ | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:162-170` |
| `Clone` | ✅ hand-written interface-ref path | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:172-180`, `258-270` |

The generated declaration in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:799-824` intentionally omits `Clone` because it returns an enumerator interface pointer; the Discovery proxy covers that pattern for real OPCEnum usage.

---

## 2. Component Categories (CATIDs)

**Implementation status**: ✅ **All standard CATIDs defined**

`OpcGuids` contains DA, AE, HDA, DX, Batch, Commands, Security, and XML-DA category IDs and category arrays used by discovery (`src/Opc.Classic.Core/OpcGuids.cs:1-415`). Tests verify canonical values and duplicate safety (`tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs:1-154`).

---

## 3. Error Codes (HRESULTs)

**Implementation status**: ✅ **Common OPC HRESULTs are represented by `OpcResultId`**

`OpcResultId` defines the standard OPC facility and shared DA/Common result IDs, plus helper properties such as `IsFailure`, `IsSuccess`, `Facility`, and `CodePart` (`src/Opc.Classic.Core/OpcResultId.cs:1-136`). Spec-specific errors live in their owning assemblies, for example HDA, CPX, Batch, DX, Security, and XML-DA.

---

## 4. Gap Summary & Recommendations

| # | Feature | Status | Priority | Recommendation |
|---|---|---|---|---|
| 1 | `SetClientName` high-level convenience API | ✅ DCOM, ⚠️ not on `IDaServer` | Low | Add only if application code needs typed access outside raw DCOM. |
| 2 | Appendix B string filter utility | ⚠️ No shared public helper | Medium | Consider a reusable `OpcStringFilter.MatchPattern(...)` helper for server implementers. |
| 3 | Property ID range enforcement | Informational | Low | Keep as documentation; runtime enforcement is not required by OPC Common. |

---

## 5. Test Coverage

| Test File | Scope |
|---|---|
| `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs:1-154` | IID/CLSID/CATID registry and category arrays |
| `tests/Opc.Classic.Discovery.Tests/OpcEnumClientTests.cs:1-325` | OPCEnum enumeration, category merging, and error mapping |
| `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs:1-244` | Server dispatcher coverage that includes common DA hosting paths |

---

## 6. Conclusion

OPC Common coverage is high for the wire-visible functionality required by modern OPC Classic clients. The important stale caveats from earlier reviews — missing `IOPCCommon` and missing `IOPCEnumGUID::Skip/Reset/Clone` support — no longer apply to the current DCOM/discovery implementation.
