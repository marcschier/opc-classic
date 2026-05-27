# OPC Data Access 2.05a — Spec Coverage Review

**Spec**: OPC Data Access Custom Interface Specification 2.05a
**Implementation**: `src/Opc.Classic.Da/V20/IOPCV20Interfaces.cs` (minimal V2 back-compat layer), `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` (modern DA surface)
**Review target**: `1.0.0-rc.7`

---

## Summary

### V20 Back-Compat Layer

- **Status**: intentionally minimal.
- **Purpose**: compatibility shims for older clients/servers.
- **Guidance**: new code should use the modern `Opc.Classic.Da.Dcom` surface.

### Modern DCOM Surface

- **Status**: ✅ full DA 2.05a projection coverage, plus DA 3.0 extensions used for shared implementation.
- **Cross-platform path**: source-generated proxies and server dispatchers cover the DA 2.05a interfaces in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:30-797`.
- **Windows CCW path**: full vtables with real bodies for server/group lifecycle, item management, sync/async I/O, callbacks, connection points, and item-attribute enumeration.
- **Major stale caveats removed**: `IOPCServer`, `IOPCCommon`, `IOPCGroupStateMgt`, `IOPCItemMgt`, `IOPCSyncIO`, `IOPCAsyncIO2`, `IOPCDataCallback`, `IConnectionPoint`, `IOPCBrowseServerAddressSpace`, and `IOPCItemProperties` are no longer “missing” or “stub-only” in the modern surface.

---

## Gap Analysis by Namespace

### 1. V20 Back-Compat Layer (`Opc.Classic.Da.V20.Dcom`)

The V20 namespace remains deliberately narrow. Missing V20 declarations are not treated as modern DA 2.05a gaps because the current supported surface is `Opc.Classic.Da.Dcom`.

**Recommendation**: keep V20 documentation clear: use it only for legacy compatibility and use modern DCOM for full DA 2.05a/3.0 coverage.

---

### 2. Modern DCOM Surface (`Opc.Classic.Da.Dcom`)

| Spec interface | Cross-platform status | Windows CCW status | Source |
|---|---|---|---|
| `IOPCServer` | ✅ 6/6 methods declared; `AddGroup`, `GetGroupByName`, `CreateGroupEnumerator` return interface refs | ✅ full vtable, real `AddGroup`, `GetErrorString`, `GetStatus`, `GetGroupByName`, `RemoveGroup`, etc. | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:30-84`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaServerCcw.cs:1-556` |
| `IOPCCommon` | ✅ 5/5 generated proxy + dispatcher | routed through hosting where implemented | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `IOPCGroupStateMgt` | ✅ 4/4 | ✅ full group CCW; `CloneGroup` copies items | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:414-452`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcw.cs:1-512` |
| `IOPCItemMgt` | ✅ 7/7 including `AddItems`, `ValidateItems`, `CreateEnumerator` | ✅ full vtable; `OPCITEMDEF[]`/`OPCITEMRESULT[]` marshaling and item enumerator CCW | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:354-412`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcw.cs:1-512` |
| `IOPCSyncIO` | ✅ 2/2 | ✅ `Read`/`Write` real bodies with VARIANT marshaling | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:473-494` |
| `IOPCAsyncIO2` | ✅ 6/6 | ✅ `Read`, `Write`, `Refresh2`, `Cancel2`, `SetEnable`, `GetEnable` real bodies | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:539-590` |
| `IOPCBrowseServerAddressSpace` | ✅ 5/5, backed by address-space abstractions | usable through default browse services | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:159-199`; `src/Opc.Classic.Da/Hosting/DefaultBrowseServerAddressSpace.cs:1-167` |
| `IOPCItemProperties` | ✅ 3/3 | default properties include canonical IDs 1-8 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:201-242`; `src/Opc.Classic.Da/Hosting/DefaultItemProperties.cs:1-106` |
| `IConnectionPointContainer` / `IConnectionPoint` | ✅ dispatcher coverage; `Advise`/`Unadvise` wired | ✅ group CCW participates in callback fan-out | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:681-727`; `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs:1-1225` |
| `IOPCDataCallback` | ✅ 4/4 outbound callback projection | ✅ `OpcDataCallbackProxy` marshals `OnDataChange`, `OnReadComplete`, `OnWriteComplete`, `OnCancelComplete` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:742-797`; `src/Opc.Classic.Da/Hosting/Windows/OpcDataCallbackProxy.cs:1-395` |
| `IEnumOPCItemAttributes` | ✅ dispatcher + stateful enumerator | ✅ `Next`, `Skip`, `Reset`, `Clone`, including `vEUInfo` VARIANT marshaling | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:651-679`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumOpcItemAttributesCcw.cs:1-153` |

`IOpcDaServer` also includes `ResolveGroupAsync` and `ResolveGroupByNameAsync` helper defaults for server/group lookup (`src/Opc.Classic.Da/Hosting/IOpcDaServer.cs:1-131`).

---

## Structure Coverage

DA 2.05a structures and VARIANT-heavy payloads are implemented through the shared DA/NDR code and Windows CCW paths:

- `OPCITEMSTATE` / `OpcItemState`
- `OPCITEMDEF` / `OpcItemDef`
- `OPCITEMRESULT` / `OpcItemResult`
- `OPCITEMATTRIBUTES` / `OpcItemAttributes`
- `OPCSERVERSTATUS` / `OpcServerStatus`
- `OPC_QUALITY` / `OpcQuality`

Current Windows CCW item and I/O tests exercise the marshaling paths that were previously listed as deferred.

---

## Test Coverage

| Test File | Scope |
|---|---|
| `tests/Opc.Classic.Da.Tests/V20/IOPCV20InterfaceIdTests.cs` | V20 IID compatibility |
| `tests/Opc.Classic.Da.Tests/Dcom/IOPCServerProxyTests.cs:1-103` | `IOPCServer` proxy calls |
| `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs:1-244` | Server dispatcher routing |
| `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs:1-200` | Browse/property defaults |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaServerCcwTests.cs:1-559` | Windows server CCW |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaGroupCcwTests.cs:1-1815` | Windows group CCW item/I/O/state/callback behavior |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDataCallbackProxyTests.cs:1-781` | Outbound `IOPCDataCallback` VARIANT marshaling |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcEnumOpcItemAttributesCcwTests.cs:1-247` | Item attribute enumerator CCW |

---

## Recommendations

1. Keep V20 docs explicit about its intentionally minimal scope.
2. Continue adding CTT/native interop coverage for Windows CCW edge cases.
3. Add more end-to-end DA 2.x client/server scenarios that combine group creation, item addition, sync reads, async callbacks, and shutdown.
4. Add public convenience wrappers only where adoption feedback shows raw DCOM projections are too low-level.

---

## Conclusion

The modern DA DCOM surface now provides full DA 2.05a coverage. The older findings that `IOPCCommon`, `IOPCShutdown`, group/item management, synchronous reads, connection points, and item enumerators were missing or deferred no longer apply. Remaining caveats should be framed around V20 compatibility scope and integration/CTT hardening.
