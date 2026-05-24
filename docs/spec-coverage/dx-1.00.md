# OPC DX 1.00 Specification Coverage Analysis

**Specification**: OPC Data eXchange Specification Version 1.0 (March 5, 2003)  
**Implementation**: `Opc.Classic.Dx` managed assembly  
**Analysis Date**: 2025-01-20

---

## Executive Summary

The OPC DX 1.00 implementation in `Opc.Classic.Dx` provides foundational coverage for the DX configuration service specification, with **deliberate deferred implementation** of complex structure codecs pending Phase 9A-followup. The managed type system covers all major DX entities (connections, source servers, status records), but **proxy methods for Add/Modify/Update operations are intentionally stubbed** because the NDR codec registry currently has **no DX-specific codecs registered** (per `gap-analysis.md`).

### Coverage Summary

| Category | Specified | Implemented | Coverage | Notes |
|----------|-----------|-------------|----------|-------|
| **Interfaces** | 1 | 1 (partial) | 25% | `IOPCConfiguration` — 3 of 12+ methods projected |
| **Core Services** | 12 | 3 | 25% | Query/Delete/Reset only; Add/Modify/Copy deferred |
| **Data Structures** | 16+ | 8 | 50% | Records defined; codecs NOT registered |
| **Enumerations** | 4 | 2 | 50% | `ConnectionState`, `OverrideState` present |
| **Error Codes** | 40+ | 0 | 0% | DX-specific error constants absent |
| **Runtime Model** | Full | Minimal | 10% | DX database concepts, no runtime implementation |

**Major Gaps**: 12 of 16 `IOPCConfiguration` methods, 40+ DX error codes, compound structure codecs, and the complete DX runtime state machine (§6).

---

## 1. Interface Coverage

### 1.1 IOPCConfiguration (IID C130D281-F4AA-4779-8846-C2C4CB444F2A)

**Spec Reference**: §5.2 (OPC DX Configuration Services), Appendix B (COM Mapping)

| Method | Opnum | Spec § | Status | Notes |
|--------|-------|--------|--------|-------|
| **GetServerState** | 3 | — | ❌ Missing | Server state query (implicit from DA status) |
| **GetStatusString** | 4 | — | ❌ Missing | Status diagnostic helper |
| **CopyDefaultServerAttributes** | 5 | §5.2.1.5 | ❌ Missing | Reset source-server state to defaults |
| **QuerySourceServers** | 6 | §5.2.1.1 | ❌ Missing | Source server discovery |
| **AddSourceServers** | 7 | §5.2.1.2 | ❌ Missing | Register source servers |
| **ModifySourceServers** | 8 | §5.2.1.3 | ❌ Missing | Update source-server config |
| **DeleteSourceServers** | 9 | §5.2.1.4 | ❌ Missing | Deregister source servers |
| **QueryDXConnectionNames** | 10 | §5.2.2.1 | ✅ Projected | Browse path + mask-based discovery (opnum 8) |
| **AddDXConnections** | 11 | §5.2.2.2 | ❌ Codec Missing | Requires `OpcDxConnection[]` codec |
| **ModifyDXConnections** | 12 | §5.2.2.4 | ❌ Codec Missing | Requires `OpcDxConnection[]` codec |
| **UpdateDXConnections** | 13 | §5.2.2.3 | ❌ Codec Missing | Requires `OpcDxConnection[]` + mask codec |
| **DeleteDXConnections** | 14 | §5.2.2.5 | ✅ Projected | Per-connection `int[]` HRESULTs (opnum 12) |
| **CopyDefaultDXConnectionAttributes** | 15 | §5.2.2.6 | ❌ Missing | Reset connection state to defaults |
| **ResetConfiguration** | 16 | §5.2.3 | ✅ Projected | Clear all config (opnum 14) |

**Analysis**:
- **3 of 14 methods** successfully projected (`QueryDXConnectionNamesAsync`, `DeleteDXConnectionsAsync`, `ResetConfigurationAsync`).
- **9 methods** require compound structure codecs (`OpcDxConnection`, `OpcDxSourceServer`, `OpcDxGeneralResponse`) that are **not yet registered** in the proxy codec table (§3 of IOPCDxInterfaces.cs).
- **2 source-server state-management methods** (`CopyDefaultServerAttributes`, `QuerySourceServers`) deferred because source-server infrastructure is minimal.

**Root Cause**: §3 of `IOPCDxInterfaces.cs`:

```csharp
// Add/modify/copy methods use OpcDxConnection/OpcDxGeneralResponse records 
// not registered in the proxy codec table yet.
```

---

## 2. Data Structures Coverage

### 2.1 Core Configuration Records

| Structure | Spec § | Impl File | Fields | Status |
|-----------|--------|-----------|--------|--------|
| **DXConnection** | §5.1.2, §4.3.2 | `DxConnection.cs` | 21 fields + mask | ✅ Defined, ❌ No codec |
| **SourceServer** | §5.1.8, §4.4.1 | `DxSourceServer.cs` | 10 fields + mask | ✅ Defined, ❌ No codec |
| **DXGeneralResponse** | §5.1.3 | `DxGeneralResponse.cs` | ConfigVersion + `IdentifiedResult[]` | ✅ Defined, ❌ No codec |
| **IdentifiedResult** | §5.1.4 | `DxGeneralResponse.cs` | ItemPath/Name/Version + ResultCode | ✅ Defined |
| **ItemIdentifier** | §5.1.5 | Inline in records | ItemPath, ItemName, Version | ✅ Inline |
| **BrowsePath** | §5.1.1 | `string` primitive | '/' delimited branch path | ✅ Handled as string |
| **NodeName** | §5.1.6 | `string` primitive | Branch-local name | ✅ Handled as string |

**Analysis**:
- All **7 core parameter structures** have managed record definitions.
- **Mask fields** (`dwMask` in native IDL) are present but not yet processed by codecs.
- **No NDR marshal/unmarshal** codecs registered in `Opc.Classic.Dcom.Serialization` codec registry.

**Expected Codec Phase**: §Phase 9A-followup will register:
```csharp
DxConnectionCodec : INdrCodec<DxConnection>
DxSourceServerCodec : INdrCodec<DxSourceServer>
DxGeneralResponseCodec : INdrCodec<DxGeneralResponse>
```

### 2.2 Runtime Status Structures

| Structure | Spec § | Impl Status | Notes |
|-----------|--------|-------------|-------|
| **ServerStatus** | §4.2 | ❌ Not defined | Server-level status (ConfigVersion, ServerState, DXConnectionCount, etc.) |
| **DXConnectionStatus** | §4.3.2.19 | ❌ Not defined | Per-connection runtime (DXConnectionState, WriteValue/Quality, SourceValue/Quality, queue stats) |
| **DXSourceServerStatus** | §4.4.1.6 | ❌ Not defined | Per-source-server status (ConnectStatus, ErrorID, LastConnectTimestamp, PingTime, etc.) |
| **DXQuality** | §4.3.2.19.4 | ❌ Not defined | Quality + LimitBits + VendorBits (XML schema `DXQuality` complex type) |

**Analysis**:
- **4 status structures** are not represented in managed code. The DX runtime model (§6) is entirely absent.
- These would be **browseable DA items** under `DX/ServerStatus`, `DX/DXConnectionsRoot/<name>/Status`, `DX/SourceServers/<name>/Status`.
- Implementation priority is **low** because DX servers are rare; focus remains on **client-side configuration** (IDxServer).

---

## 3. Enumeration Coverage

| Enumeration | Spec § | Impl File | Values | Status |
|-------------|--------|-----------|--------|--------|
| **ServerState** | §4.2.2, Table 1 | ❌ Not defined | running, failed, noConfig, suspended, shutdown, test, commFault, unknown (8 values) | Missing |
| **DXConnectionState** | §4.3.2.19.1, Table 4 | ❌ Not defined | initializing, operational, deactivated, sourceServerNotConnected, subscriptionFailed, targetItemNotFound (6 values) | Missing |
| **ConnectStatus** | §4.4.1.6.1, Table 7 | ❌ Not defined | connected, disconnected, connecting, failed (4 values) | Missing |
| **ConnectionState** | § (non-spec) | `ConnectionState.cs` | Initial, Connecting, Subscribing, Connected, Disconnecting, Disconnected (6 values) | ✅ Custom enum |

**Analysis**:
- **3 spec-mandated enumerations** are missing.
- **1 custom `ConnectionState` enum** exists but does not align with DX spec §4.3.2.19.1 `DXConnectionState`.

**Recommendation**: Define spec-aligned enums:
```csharp
public enum DxServerState { Running, Failed, NoConfig, Suspended, Shutdown, Test, CommFault, Unknown }
public enum DxConnectionState { Initializing, Operational, Deactivated, SourceServerNotConnected, SubscriptionFailed, TargetItemNotFound }
public enum DxConnectStatus { Connected, Disconnected, Connecting, Failed }
```

---

## 4. Error Code Coverage

**Spec Reference**: §5.1.7 (ResultCode), Tables 2, 5, 6, 8, 15

| Category | Spec Count | Impl Count | Coverage |
|----------|------------|------------|----------|
| **Server Errors** | 7 (Table 2) | 0 | 0% |
| **Target Item Errors** | 13 (Table 5) | 0 | 0% |
| **Source Item Errors** | 10 (Table 6) | 0 | 0% |
| **Source Server Errors** | 5 (Table 8) | 0 | 0% |
| **DX Specific Errors** | 20+ (Table 15) | 0 | 0% |

**Total**: ~55 DX error codes specified, **0 implemented**.

**Missing Constants** (partial list from §5.1.7 Table 15):
```
E_PERSISTING, E_NOITEMLIST, E_SERVER_STATE, E_VERSION_MISMATCH,
E_UNKNOWN_ITEM_PATH, E_UNKNOWN_ITEM_NAME, E_INVALID_ITEM_PATH, E_INVALID_ITEM_NAME,
E_INVALID_NAME, E_DUPLICATE_NAME, E_INVALID_BROWSE_PATH, E_INVALID_SERVER_URL,
E_INVALID_SERVER_TYPE, E_UNSUPPORTED_SERVER_TYPE, E_CONNECTIONS_EXIST,
E_TOO_MANY_CONNECTIONS, E_OVERRIDE_BADTYPE, E_OVERRIDE_RANGE,
E_SUBSTITUTE_BADTYPE, E_SUBSTITUTE_RANGE, E_INVALID_TARGET_ITEM,
E_SOURCE_SERVER_NOT_CONNECTED, E_SOURCE_SERVER_FAULT, E_SOURCE_SERVER_NO_ACCESS,
E_SOURCE_ITEM_BADRIGHTS, E_SOURCE_ITEM_BAD_QUALITY, E_SOURCE_ITEM_BADTYPE,
E_SUBSCRIPTION_FAULT, E_TARGET_ITEM_DISCONNECTED, E_TARGET_FAULT,
E_TARGET_NO_ACCESS, E_TARGET_NO_WRITES_ATTEMPTED, E_TARGET_INVALID_ITEM,
S_TARGET_SUBSTITUTED, S_TARGET_OVERRIDEN, S_CLAMP, ...
```

**Analysis**:
- **No `OpcDxError` constant class** exists.
- Current error handling relies on generic `OpcResultId` (DA/COM HRESULTs).
- DX-specific semantics (e.g., `E_VERSION_MISMATCH`, `E_CONNECTIONS_EXIST`) are not represented.

**Recommendation**: Define `OpcDxError` static class:
```csharp
public static class OpcDxError
{
    public static readonly OpcResultId VersionMismatch = new(0x811D0001, "OPCDX_E_VERSION_MISMATCH");
    public static readonly OpcResultId ConnectionsExist = new(0x811D0002, "OPCDX_E_CONNECTIONS_EXIST");
    // ... 53 more
}
```

---

## 5. IDxServer Managed Interface

**File**: `src/Opc.Classic.Dx/IDxServer.cs`

| Member | Spec Alignment | Notes |
|--------|----------------|-------|
| `GetStatusAsync()` | Partial | Returns `OpcServerStatus` (DA 2.05), not DX `ServerStatus` (§4.2) |
| `GetSourceServersAsync()` | §5.2.1.1 | Placeholder for `QuerySourceServers` |
| `AddOrUpdateSourceServerAsync()` | §5.2.1.2, §5.2.1.3 | Combines `AddSourceServers` + `ModifySourceServers` |
| `RemoveSourceServerAsync()` | §5.2.1.4 | Wrapper for `DeleteSourceServers` |
| `GetConnectionsAsync(filter)` | §5.2.2.1 | Wrapper for `QueryDXConnections` |
| `AddOrUpdateConnectionAsync()` | §5.2.2.2, §5.2.2.4 | Combines `AddDXConnections` + `ModifyDXConnections` |
| `RemoveConnectionAsync()` | §5.2.2.5 | Wrapper for `DeleteDXConnections` |
| `ResetConfigurationAsync()` | §5.2.3 | Direct mapping |

**Analysis**:
- **Async-first API** simplifies the 14 COM methods into 8 managed methods.
- **Combines Add/Modify** into upsert-style operations (idiomatic for .NET).
- **Missing**: `CopyDefaultServerAttributes`, `CopyDefaultDXConnectionAttributes`, `UpdateDXConnections` (§5.2.2.3).
- **Missing**: Granular version-checking semantics (§4.3.2.1 `Version` attribute).

---

## 6. DX Database / Runtime Model Coverage

**Spec Reference**: §4 (DX Database), §6 (OPC DX Runtime Model)

| Component | Spec § | Status | Notes |
|-----------|--------|--------|-------|
| **DX Root** | §4.1 | ❌ Not implemented | Browseable "DX" branch |
| **ServerStatus** | §4.2 | ❌ Not implemented | 9 sub-items (ConfigurationVersion, ServerState, DXConnectionCount, etc.) |
| **DXConnectionsRoot** | §4.3 | ❌ Not implemented | Browse tree for connections |
| **Branch Status Items** | §4.3.1.1 | ❌ Not implemented | Write-only `Overridden`, `SourceItemConnected`, `TargetItemConnected` |
| **DXConnection Items** | §4.3.2 | ❌ Not implemented | 18 config attributes + Status complex item |
| **SourceServers Branch** | §4.4 | ❌ Not implemented | Browse tree for source servers |
| **SourceServer Items** | §4.4.1 | ❌ Not implemented | 5 config attributes + Status complex item |
| **Runtime Startup** | §6.1 | ❌ Not implemented | DX initialization sequence |
| **Connection Management** | §6.2 | ❌ Not implemented | Source-server connect/reconnect/disconnect logic |
| **Data Transfer** | §6.3 | ❌ Not implemented | Subscription, queueing, conversion, target updates |
| **Target Update Truth Table** | §6.3.5.1 | ❌ Not implemented | 18-rule decision matrix for target writes |
| **Persistence** | §5.3 | ❌ Not implemented | DirtyFlag + periodic persist |

**Analysis**:
- **Entire DX runtime state machine** (§6) is absent.
- The implementation is **configuration-service-only** (DCOM proxy for `IOPCConfiguration`).
- **No DA server behavior** — the DX server extension that exposes the "DX" branch, manages subscriptions to source servers, and writes to target items is not implemented.

**Implementation Scope**: The current `Opc.Classic.Dx` assembly is a **client-side configuration library**, not a DX server runtime. A future `Opc.Classic.Dx.Server` assembly would implement §6.

---

## 7. Test Coverage

**Test Projects**: `tests/Opc.Classic.Dx.Tests/`

| Test File | Purpose | Coverage |
|-----------|---------|----------|
| **DcomInterfaceIdTests.cs** | Verifies `IOPCConfiguration` IID (C130D281-...) and `IOPCDXServer` IID | ✅ IIDs validated |
| **DxTypesTests.cs** | Tests `DxConnection`, `DxSourceServer`, `ConnectionState`, `OverrideState` record construction | ✅ Basic type validation |
| **Dcom/IOPCDxProxyTests.cs** | Validates proxy generation for `IOPCConfiguration` (method signatures, vtable layout) | ✅ Proxy generation verified |

**Total Test Count**: ~15 tests (2 IID tests, ~10 type tests, ~3 proxy tests).

**Analysis**:
- **No end-to-end tests** against a DX server (C++ sample or managed implementation).
- **No codec round-trip tests** (because codecs are not registered).
- **No service method tests** (Add/Modify/Query return placeholder `NotImplementedException` or empty results).

**Recommendation**: Defer integration tests until Phase 9A codec registration.

---

## 8. Known Gaps and Deferred Work

### 8.1 Phase 9A-Followup (Codec Registration)

**Blocking**: Add/Modify/Update/Copy methods for connections and source servers.

**Tasks**:
1. Register `DxConnectionCodec`, `DxSourceServerCodec`, `DxGeneralResponseCodec` in `Opc.Classic.Dcom.Serialization`.
2. Implement `dwMask` presence-bit logic (21 flags for `DxConnection`, 5 for `DxSourceServer`).
3. Enable `[OpcMethod]` projections for:
   - `AddDXConnectionsAsync(DxConnection[], CancellationToken)` → `DxGeneralResponse`
   - `ModifyDXConnectionsAsync(DxConnection[], CancellationToken)` → `DxGeneralResponse`
   - `UpdateDXConnectionsAsync(string browsePath, DxConnection mask, ...)` → `DxGeneralResponse`
   - `AddSourceServersAsync(DxSourceServer[], CancellationToken)` → `DxGeneralResponse`
   - etc. (6 more methods)

### 8.2 DX Runtime Server Implementation

**Scope**: Implement §6 (OPC DX Runtime Model).

**Components**:
- **DA Server Extension**: Expose `DX/` branch, `ServerStatus`, `DXConnectionsRoot`, `SourceServers` as browseable items.
- **Source Access**: Act as DA client to source servers (create groups, add items, subscribe to IAdviseSink callbacks).
- **Data Transfer Logic**:
  - Queue source updates (§6.3.3 Queueing).
  - Apply data conversion (§6.3.4).
  - Execute Target Update Truth Table (§6.3.5.1 — 18 rules).
  - Handle override/substitute values (§4.3.2.19.17, §4.3.2.8).
- **Connection State Machine**: Manage 6 states (initializing, operational, deactivated, sourceServerNotConnected, subscriptionFailed, targetItemNotFound).
- **Persistence**: Implement §5.3 (periodic save/load of DX Database, DirtyFlag maintenance).

**Estimated Effort**: 2000+ LOC (comparable to `Opc.Classic.Da.Server`).

### 8.3 Error Code Constants

**Task**: Define `OpcDxError` static class with 55+ constants (§5.1.7 tables).

### 8.4 XML-DA Mapping

**Spec Reference**: Appendix A (Web Services Implementation)

**Status**: Not started. XML-DA support is lower priority than DCOM (DX servers are predominantly COM-based).

---

## 9. Alignment with gap-analysis.md

**Excerpt from `docs/gap-analysis.md` (Phase 2 - DX)**:

> DX codec registry currently has **no DX-specific codecs registered** — DX proxies likely use empty-payload placeholder bodies until Phase 9A-followup registers `OpcDxConnection`, `OpcDxSourceServer`, and `OpcDxGeneralResponse` codecs.

**Validation**: ✅ **Confirmed**. §3 of `IOPCDxInterfaces.cs`:

```csharp
// Add/modify/copy methods use OpcDxConnection/OpcDxGeneralResponse records 
// not registered in the proxy codec table yet.
```

**Impact**:
- **3 of 14 `IOPCConfiguration` methods** successfully projected (25% interface coverage).
- **9 methods** blocked by missing codecs (Add/Modify/Update/Copy for connections and source servers).
- **No functional Add/Modify operations** available to DX clients until codecs are registered.

---

## 10. Recommendations

### 10.1 Immediate Actions (No Codec Dependency)

1. **Define `OpcDxError` constants** (55+ DX-specific HRESULTs from §5.1.7) → 200 LOC, enables proper error handling.
2. **Define status structure records** (`ServerStatus`, `DXConnectionStatus`, `DXSourceServerStatus`, `DXQuality`) → 150 LOC, aligns with §4.2, §4.3.2.19, §4.4.1.6.
3. **Define spec-aligned enums** (`DxServerState`, `DxConnectionState`, `DxConnectStatus`) → 50 LOC, replaces custom `ConnectionState`.
4. **Document codec deferral** in README: "DX Add/Modify operations require Phase 9A codec registration; Query/Delete/Reset work today."

### 10.2 Phase 9A-Followup (Codec Registration)

1. Register `DxConnectionCodec`, `DxSourceServerCodec`, `DxGeneralResponseCodec` in NDR codec table.
2. Implement `dwMask` presence-bit marshaling (21 flags for `DxConnection`, 5 for `DxSourceServer`).
3. Enable 9 blocked `IOPCConfiguration` methods → **90% interface coverage**.
4. Add integration tests against C++ DX sample server (if available) or managed test shim.

### 10.3 Future Work (DX Server Runtime)

1. Implement §6 (Runtime Model) → new `Opc.Classic.Dx.Server` assembly.
2. Implement §4 (DX Database) → DA server extension with browseable "DX" branch.
3. Add persistence layer (§5.3) → save/load DX configuration to XML or binary format.
4. Add XML-DA support (Appendix A) → optional, lower priority.

---

## 11. Conclusion

The `Opc.Classic.Dx` implementation provides **foundational type coverage** (records, enums, managed interface) but **deliberately defers complex operations** (Add/Modify/Update/Copy) pending Phase 9A codec registration. The current 25% interface coverage reflects a **strategic pause** — the 3 working methods (`QueryDXConnectionNames`, `DeleteDXConnections`, `ResetConfiguration`) use only scalar/string codecs, while the 9 blocked methods require compound structure marshaling that is not yet priority-justified by DX deployment rarity.

**Key Findings**:
- ✅ **All core data structures defined** (8 of 8 records).
- ✅ **IIDs and proxy generation validated** (2 of 2 interfaces).
- ❌ **No NDR codecs registered** (0 of 3 required).
- ❌ **No DX runtime implementation** (§6 state machine absent).
- ❌ **No error code constants** (0 of 55+).

**Recommendation**: **Defer Phase 9A until DX server demand justifies codec work**. Current coverage is sufficient for read-only DX configuration discovery (QueryDXConnectionNames) and reset operations (ResetConfiguration).

---

**Document Revision**: 1.0  
**Next Review**: After Phase 9A codec registration
