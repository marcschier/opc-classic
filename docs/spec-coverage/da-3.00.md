# OPC DA 3.00 — Spec Coverage Review

**Spec**: OPC Data Access Custom Interface Specification Version 3.0 (March 4, 2003)
**Implementation**: `src/Opc.Classic.Da/`
**Review target**: `1.0.0-rc.7`

---

## Summary

**Interfaces**: DA 3.0 server, group, item, browse, I/O, callback, and connection-point surfaces are projected.
**Overall compliance**: ✅ **Modern DCOM projection complete with substantial Windows CCW support**.

The earlier DA 3.0 review listed `AddGroup`, `AddItems`, `SetState`, `ReadMaxAge`, keep-alive, `CreateEnumerator`, and callback/connection point methods as missing or stubbed. Those claims are stale for the current implementation.

---

## Implementation Status by Area

### OPCServer interfaces

| Interface | Status | Source |
|---|---|---|
| `IOPCServer` | ✅ all 6 methods declared with interface-ref returns where required; Windows CCW has real method bodies | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:30-84`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaServerCcw.cs:1-556` |
| `IOPCCommon` | ✅ all 5 methods generated | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `IOPCBrowse` | ✅ DA 3.0 unified browse (`GetProperties`, `Browse`) | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:123-157`; `src/Opc.Classic.Da/Hosting/DefaultBrowse.cs:1-123` |
| `IOPCBrowseServerAddressSpace` | ✅ DA 2.x browse compatibility with `IEnumString` interface refs | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:159-199`; `src/Opc.Classic.Da/Hosting/DefaultBrowseServerAddressSpace.cs:1-167` |
| `IOPCItemProperties` | ✅ property query/read/lookup; default canonical property IDs 1-8 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:201-242`; `src/Opc.Classic.Da/Hosting/DefaultItemProperties.cs:1-106` |
| `IOPCItemIO` | ✅ stateless DA 3.0 read/write VQT | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:327-352` |

### OPCGroup interfaces

| Interface | Status | Source |
|---|---|---|
| `IOPCItemMgt` | ✅ `AddItems`, `ValidateItems`, `RemoveItems`, active/client/datatype setters, `CreateEnumerator`; Windows CCW includes OPCITEMDEF/OPCITEMRESULT marshaling | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:354-412`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcw.cs:1-512` |
| `IOPCGroupStateMgt` | ✅ `GetState`, `SetState`, `SetName`, `CloneGroup`; Windows CCW clone copies items | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:414-452`; `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs:1-1225` |
| `IOPCGroupStateMgt2` | ✅ `SetKeepAlive`, `GetKeepAlive` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:454-471` |
| `IOPCSyncIO` | ✅ DA 2.x sync `Read`/`Write`; Windows CCW has real VARIANT bodies | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:473-494` |
| `IOPCSyncIO2` | ✅ `Read`, `Write`, `ReadMaxAge`, `WriteVQT` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:496-537` |
| `IOPCAsyncIO2` | ✅ `Read`, `Write`, `Refresh2`, `Cancel2`, `SetEnable`, `GetEnable` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:539-590` |
| `IOPCAsyncIO3` | ✅ DA 3.0 async max-age/VQT extensions | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:592-649` |
| `IOPCItemDeadbandMgt` | ✅ 3/3; default returns per-handle not-set/not-supported policy errors | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:244-272`; `src/Opc.Classic.Da/Hosting/DefaultItemDeadbandMgt.cs:1-54` |
| `IOPCItemSamplingMgt` | ✅ 5/5; default returns per-handle rate/buffering policy errors | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:274-325`; `src/Opc.Classic.Da/Hosting/DefaultItemSamplingMgt.cs:1-83` |
| `IEnumOPCItemAttributes` | ✅ dispatcher and Windows CCW `Next`/`Skip`/`Reset`/`Clone` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:651-679`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumOpcItemAttributesCcw.cs:1-153` |
| `IConnectionPointContainer` / `IConnectionPoint` | ✅ callback connection routing; `Advise`/`Unadvise` wired | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:681-727`; `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs:1-1225` |
| `IOPCDataCallback` | ✅ callback proxy/dispatcher; Windows CCW outbound proxy handles VARIANT arrays | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:742-797`; `src/Opc.Classic.Da/Hosting/Windows/OpcDataCallbackProxy.cs:1-395` |

---

## Current Gaps and Deferred Work

### MEDIUM

1. **Policy-specific implementations for deadband/sampling**
   The default implementations deliberately return per-handle `OPC_E_DEADBANDNOTSET`, `OPC_E_DEADBANDNOTSUPPORTED`, `OPC_E_RATENOTSET`, or `OPC_E_NOBUFFERING` until a server supplies deadband/sampling policy.

2. **Windows CCW/native interop breadth**
   DA has broad CCW coverage, but continued CTT testing is still needed for uncommon edge cases such as public groups, alternate access paths, and complex `vEUInfo` values.

3. **Higher-level convenience APIs**
   Some low-level DCOM methods are projected directly rather than wrapped by higher-level abstractions. This is intentional to preserve spec fidelity.

---

## Coverage Gaps (Integration Tests Recommended)

Current tests cover the major areas that were formerly listed as missing:

- `tests/Opc.Classic.Da.Tests/Dcom/IOPCServerProxyTests.cs:1-103`
- `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs:1-244`
- `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs:1-200`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaServerCcwTests.cs:1-559`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaGroupCcwTests.cs:1-1815`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDataCallbackProxyTests.cs:1-781`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcEnumOpcItemAttributesCcwTests.cs:1-247`

Recommended next tests:

1. Full CTT-style group lifecycle: `AddGroup` → `AddItems` → sync read/write → async callback → `RemoveGroup`.
2. DA 3.0 `IOPCBrowse` continuation-point scenarios against hierarchical and flat namespaces.
3. Item deadband/sampling custom policy implementations beyond the default unsupported/not-set responses.
4. Native interop tests for `vEUInfo` engineering-unit arrays and uncommon VARIANT types.

---

## Compliance Checklist (DA 3.0 vs Implementation)

| Interface | DA 3.0 Requirement | Current Status |
|---|---|---|
| `IOPCServer` | Required | ✅ Full DCOM; Windows CCW real bodies |
| `IOPCCommon` | Required | ✅ Full DCOM |
| `IConnectionPointContainer` | Required for callbacks | ✅ Wired for DA group callbacks |
| `IOPCBrowse` | Required | ✅ Full DCOM and default browse implementation |
| `IOPCItemIO` | Required | ✅ Full DCOM |
| `IOPCItemMgt` | Required | ✅ Full DCOM and Windows CCW |
| `IOPCGroupStateMgt` | Required | ✅ Full DCOM and Windows CCW |
| `IOPCGroupStateMgt2` | DA 3.0 | ✅ Keep-alive methods present |
| `IOPCSyncIO` / `IOPCSyncIO2` | Required | ✅ Full DCOM; CCW sync I/O bodies present |
| `IOPCAsyncIO2` / `IOPCAsyncIO3` | Required | ✅ Full DCOM; CCW async bodies present for DA paths described in source |
| `IOPCItemDeadbandMgt` | Required | ✅ Projection and default policy helper |
| `IOPCItemSamplingMgt` | Optional | ✅ Projection and default policy helper |

---

## Conclusion

The DA 3.0 coverage document should be read as a current implementation-status document, not as the older gap list. The flagship DA surface now has full DCOM declarations and broad Windows CCW support for practical DA server/client workflows. Remaining work is targeted conformance hardening and policy/sample coverage.
