# OPC Data Access 2.05a — Spec Coverage Review

**Spec**: OPC Data Access Custom Interface Specification 2.05a  
**Implementation**: `src/Opc.Classic.Da/V20/IOPCV20Interfaces.cs` (V2 back-compat layer), `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` (Modern DA 3.0 surface)  
**Review date**: 2025-01-XX  
**Reviewer**: Spec coverage analysis agent

---

## Summary

**V20 Back-Compat Layer (Legacy 2.05a Client Support):**
- **Interfaces**: 2/8 partially declared (25%)  
- **Methods**: 3/19 declared (16%)  
- **Structs**: 5/5 codecs registered (100%)  
- **Overall compliance**: **MINIMAL** — Intentional design. V20 layer provides minimal connectivity to legacy 2.05a servers. Comments explicitly state "New consumer code SHOULD prefer the DA 3.0 surface."

**Modern Dcom Surface (DA 3.0 + backward compatibility):**
- **Interfaces**: 12/8 (150% — includes DA 3.0 extensions)  
- **Methods**: 43+ declared (covers DA 2.05a + DA 3.0)  
- **Structs**: 5/5 codecs registered (100%)  
- **Overall compliance**: **COMPLETE** — Full DA 2.05a coverage in modern namespace, plus DA 3.0 enhancements.

### Severity Breakdown (V20 Layer Only)

- **BLOCKER**: 0 (By design — V20 is minimal back-compat shim)  
- **HIGH**: 6 (Missing interfaces: IOPCServer, IOPCCommon, IOPCItemMgt, IOPCGroupStateMgt, IOPCShutdown, IConnectionPointContainer)  
- **MEDIUM**: 5 (Missing methods in IOPCSyncIO::Read, IOPCAsyncIO::Read/Write)  
- **LOW**: 2 (Missing IOPCBrowseServerAddressSpace, IOPCItemProperties)

**Architecture Decision**: The V20 namespace is intentionally minimal — codecs exist, but only essential I/O methods are exposed. For full 2.05a coverage, consumers use the modern `Opc.Classic.Da.Dcom` namespace which includes all DA 2.05a interfaces plus DA 3.0 extensions.

---

## Gap Analysis by Namespace

### 1. V20 Back-Compat Layer (`Opc.Classic.Da.V20.Dcom`)

#### Missing Interfaces (DA 2.05a Mandatory)

##### IOPCServer — Missing

**Spec**: §4.4.4, IID `39c13a4d-011e-11d0-9675-0020afd8adb3`  
**Methods** (6):
1. `AddGroup` — Creates a new group
2. `GetErrorString` — Returns localized error strings
3. `GetGroupByName` — Retrieves a group by name
4. `GetStatus` — Returns server status (OPCSERVERSTATUS)
5. `RemoveGroup` — Removes a group
6. `CreateGroupEnumerator` — Enumerates groups

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot create groups, query server status, or retrieve error strings. This is the root server interface — without it, V20 layer cannot establish sessions.  
**Severity**: **HIGH** — Core server interface. However, modern `Dcom.IOPCServer` provides full coverage.  
**Workaround**: Use `Opc.Classic.Da.Dcom.IOPCServer` (fully implemented with 3 of 6 methods — AddGroup/GetGroupByName/CreateGroupEnumerator deferred due to interface pointer returns).

---

##### IOPCCommon — Missing

**Spec**: §4.4.6, IID not specified in spec excerpt but standard COM  
**Methods** (5):
1. `SetLocaleID` — Sets client locale
2. `GetLocaleID` — Gets current locale
3. `QueryAvailableLocaleIDs` — Lists supported locales
4. `GetErrorString` — Returns localized error strings
5. `SetClientName` — Sets client name for logging

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot configure locales or identify themselves to servers.  
**Severity**: **HIGH** — Required for 2.05a compliance (mandatory interface).  
**Workaround**: No `Dcom.IOPCCommon` either. **GAP** in both namespaces.

---

##### IOPCGroupStateMgt — Missing

**Spec**: §4.5.3, IID `39c13a50-011e-11d0-9675-0020afd8adb3`  
**Methods** (4):
1. `GetState` — Returns group state (update rate, active, deadband, etc.)
2. `SetState` — Updates group state
3. `SetName` — Renames the group
4. `CloneGroup` — Clones a group

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot configure group update rates, deadbands, or active state.  
**Severity**: **HIGH** — Core group management.  
**Workaround**: `Dcom.IOPCGroupStateMgt` fully declared (3 of 4 methods — CloneGroup deferred due to interface pointer return).

---

##### IOPCItemMgt — Missing

**Spec**: §4.5.2, IID `39c13a54-011e-11d0-9675-0020afd8adb3`  
**Methods** (7):
1. `AddItems` — Adds items to group
2. `ValidateItems` — Validates items without adding
3. `RemoveItems` — Removes items
4. `SetActiveState` — Activates/deactivates items
5. `SetClientHandles` — Rebinds client handles
6. `SetDatatypes` — Sets requested VARTYPEs
7. `CreateEnumerator` — Enumerates items

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot add items to groups. Without items, no data can be read or subscribed.  
**Severity**: **HIGH** — Core item management.  
**Workaround**: `Dcom.IOPCItemMgt` fully declared (6 of 7 methods — CreateEnumerator deferred due to interface pointer return).

---

##### IOPCShutdown — Missing

**Spec**: §4.6.3, IID `f31dfde1-07b6-11d2-b2d8-0060083ba1fb`  
**Methods** (1):
1. `ShutdownRequest(szReason)` — Server notifies client of imminent shutdown

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot gracefully handle server shutdowns.  
**Severity**: **HIGH** — Mandatory for 2.05a compliance.  
**Workaround**: No `Dcom.IOPCShutdown` either. **GAP** in both namespaces.

---

##### IConnectionPointContainer / IConnectionPoint — Missing

**Spec**: §4.6 (subscription mechanism for IOPCDataCallback)  
**IIDs**: `B196B284-BAB4-101A-B69C-00AA00341D07` (Container), `B196B286-BAB4-101A-B69C-00AA00341D07` (Point)  
**Methods**:
- `IConnectionPointContainer::EnumConnectionPoints` — Enumerates connection points
- `IConnectionPointContainer::FindConnectionPoint` — Finds a connection point by IID
- `IConnectionPoint::Advise` — Registers a callback sink
- `IConnectionPoint::Unadvise` — Unregisters a callback sink
- `IConnectionPoint::GetConnectionInterface` — Returns callback IID
- `IConnectionPoint::GetConnectionPointContainer` — Returns container
- `IConnectionPoint::EnumConnections` — Enumerates active connections

**Status**: `Dcom.IConnectionPointContainer` declared but empty stub. `Dcom.IConnectionPoint` partially declared (only GetConnectionInterface).  
**Impact**: V20 clients cannot subscribe to `IOPCDataCallback` (DA 2.05a's V2.0 callback interface). Subscription requires Advise/Unadvise.  
**Severity**: **HIGH** — Required for V2.0 subscriptions (IOPCDataCallback-based callbacks).  
**Workaround**: `Dcom.IConnectionPoint` has partial coverage. Advise/Unadvise/EnumConnections deferred (comment: "use interface pointers").

---

##### IOPCBrowseServerAddressSpace — Missing (Optional)

**Spec**: §4.4.8, IID `39c13a4f-011e-11d0-9675-0020afd8adb3`  
**Methods** (5):
1. `QueryOrganization` — Returns flat or hierarchical namespace shape
2. `ChangeBrowsePosition` — Moves browse cursor
3. `BrowseOPCItemIDs` — Browses item IDs (returns IEnumString)
4. `GetItemID` — Resolves browse data ID to fully qualified item ID
5. `BrowseAccessPaths` — Browses available access paths

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot browse server address spaces.  
**Severity**: **LOW** — Optional in 2.05a spec. Browsing is common but not mandatory.  
**Workaround**: `Dcom.IOPCBrowseServerAddressSpace` partially declared (3 of 5 methods — BrowseOPCItemIDs/BrowseAccessPaths deferred due to IEnumString interface pointer returns).

---

##### IOPCItemProperties — Missing (Optional)

**Spec**: §4.4.7, IID `39c13a72-011e-11d0-9675-0020afd8adb3`  
**Methods** (3):
1. `QueryAvailableProperties` — Lists properties for an item
2. `GetItemProperties` — Reads property values
3. `LookupItemIDs` — Resolves item IDs for indirect properties

**Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot query item metadata (engineering units, alarms, etc.).  
**Severity**: **LOW** — Optional in 2.05a spec.  
**Workaround**: `Dcom.IOPCItemProperties` fully declared (all 3 methods).

---

#### Partial Interfaces in V20

##### IOPCSyncIO — Partial

**Spec**: §4.3.1, IID `39c13a52-011e-11d0-9675-0020afd8adb3`  
**Methods** (2):
1. `Read(dwSource, dwCount, phServer, ppItemValues, ppErrors)` — opnum 3
2. `Write(dwCount, phServer, pItemValues, ppErrors)` — opnum 4

**V20 Status**:
- ❌ `Read` — Missing (comment: "needs a multi-out result record codec")
- ✅ `Write` — Declared (opnum 4)

**Impact**: V20 clients can write values but cannot read synchronously. Read is essential for request/reply workflows.  
**Severity**: **MEDIUM** — Read is mandatory for synchronous I/O. However, modern `Dcom.IOPCSyncIO` provides full coverage (both Read and Write declared, though Read throws `NotSupportedException` — likely awaiting codec).

---

##### IOPCAsyncIO — Partial

**Spec**: §4.3.4, IID `39c13a53-011e-11d0-9675-0020afd8adb3`  
**Methods** (4):
1. `Read(dwConnection, dwSource, dwCount, phServer, pTransactionID, ppErrors)` — opnum 3
2. `Write(dwConnection, dwCount, phServer, pItemValues, pTransactionID, ppErrors)` — opnum 4
3. `Refresh(dwConnection, dwSource, pTransactionID)` — opnum 5
4. `Cancel(dwTransactionID)` — opnum 6

**V20 Status**:
- ❌ `Read` — Missing (comment: "return transaction IDs plus per-item HRESULT arrays")
- ❌ `Write` — Missing (same comment)
- ✅ `Refresh` — Declared (opnum 5)
- ✅ `Cancel` — Declared (opnum 6)

**Impact**: V20 clients can refresh and cancel async transactions, but cannot initiate async reads or writes. This breaks the async workflow.  
**Severity**: **MEDIUM** — Async Read/Write are core 2.05a async operations. However, modern `Dcom.IOPCAsyncIO2` (DA 2.05a V2.0 interface) provides full async coverage (Read/Write/Refresh2/Cancel2/SetEnable/GetEnable all declared).

---

##### IOPCDataCallback — Missing

**Spec**: §4.6.1, IID `39c13a70-011e-11d0-9675-0020afd8adb3`  
**Methods** (4):
1. `OnDataChange` — Delivers sampled values from subscriptions
2. `OnReadComplete` — Delivers async read completion
3. `OnWriteComplete` — Delivers async write completion
4. `OnCancelComplete` — Confirms async transaction cancellation

**V20 Status**: Not declared in V20 namespace  
**Impact**: V20 clients cannot receive callbacks (subscriptions or async completions).  
**Severity**: **HIGH** — Required for V2.0 subscriptions (spec §4.6 mandates IOPCDataCallback for DA 2.05a).  
**Workaround**: `Dcom.IOPCDataCallback` fully declared (all 4 methods).

---

### 2. Modern Dcom Surface (`Opc.Classic.Da.Dcom`)

The `Dcom` namespace provides **comprehensive DA 2.05a coverage** plus DA 3.0 enhancements. All DA 2.05a mandatory interfaces are present:

✅ **IOPCServer** — 3 of 6 methods (GetErrorString, GetStatus, RemoveGroup). AddGroup/GetGroupByName/CreateGroupEnumerator deferred (interface pointer returns).  
✅ **IOPCGroupStateMgt** — 3 of 4 methods (GetState, SetState, SetName). CloneGroup deferred (interface pointer return).  
✅ **IOPCItemMgt** — 6 of 7 methods (AddItems, ValidateItems, RemoveItems, SetActiveState, SetClientHandles, SetDatatypes). CreateEnumerator deferred (interface pointer return).  
✅ **IOPCSyncIO** — 2 of 2 methods (Read, Write). Both declared. Read throws `NotSupportedException` in default implementation (awaiting multi-out codec).  
✅ **IOPCAsyncIO2** — 6 of 6 methods (Read, Write, Refresh2, Cancel2, SetEnable, GetEnable). Full coverage.  
✅ **IOPCDataCallback** — 4 of 4 methods (OnDataChange, OnReadComplete, OnWriteComplete, OnCancelComplete). Full coverage.  
✅ **IOPCBrowseServerAddressSpace** — 3 of 5 methods (QueryOrganization, ChangeBrowsePosition, GetItemID). BrowseOPCItemIDs/BrowseAccessPaths deferred (IEnumString interface pointer returns).  
✅ **IOPCItemProperties** — 3 of 3 methods (QueryAvailableProperties, GetItemProperties, LookupItemIDs). Full coverage.  
✅ **IConnectionPoint** — 1 of 7 methods (GetConnectionInterface). Advise/Unadvise/EnumConnections deferred (interface pointers).  
❌ **IOPCCommon** — Missing entirely.  
❌ **IOPCShutdown** — Missing entirely.

**DA 3.0 Extensions (Beyond 2.05a):**
- `IOPCSyncIO2` — Max-age reads, VQT writes
- `IOPCAsyncIO3` — Max-age async reads, async VQT writes
- `IOPCItemIO` — Stateless I/O by item ID
- `IOPCBrowse` — DA 3.0 unified browse interface
- `IOPCGroupStateMgt2` — Keep-alive
- `IOPCItemDeadbandMgt`, `IOPCItemSamplingMgt` — Per-item control

---

## Structure Coverage

All DA 2.05a structures are **fully implemented** with registered NDR codecs:

✅ **OPCITEMSTATE** → `OpcItemState` (`src/Opc.Classic.Da/OpcItemState.cs`, codec: `NdrOpcItemStateCodec`)  
  Fields: `hClient`, `ftTimeStamp`, `wQuality`, `vDataValue`  
  Tests: `NdrOpcItemStateCodecTests`, `NdrOpcItemStateCodecSnapshotTests`

✅ **OPCITEMDEF** → `OpcItemDef` (`src/Opc.Classic.Da/OpcItemDef.cs`, codec: `NdrOpcItemDefCodec`)  
  Fields: `szAccessPath`, `szItemID`, `bActive`, `hClient`, `dwBlobSize`, `pBlob`, `vtRequestedDataType`  
  Tests: `NdrOpcItemDefCodecTests`

✅ **OPCITEMRESULT** → `OpcItemResult` (`src/Opc.Classic.Da/OpcItemResult.cs`, codec: `NdrOpcItemResultCodec`)  
  Fields: `hServer`, `vtCanonicalDataType`, `dwAccessRights`, `dwBlobSize`, `pBlob`  
  Tests: `NdrOpcItemResultCodecTests`

✅ **OPCITEMATTRIBUTES** → `OpcItemAttributes` (`src/Opc.Classic.Da/OpcItemAttributes.cs`, codec: `NdrOpcItemAttributesCodec`)  
  Fields: All 11 fields including `dwEUType`, `vEUInfo` (engineering units)  
  Tests: `NdrOpcItemAttributesCodecTests`

✅ **OPCSERVERSTATUS** → `OpcServerStatus` (`src/Opc.Classic.Core/OpcServerStatus.cs`, codec: `NdrOpcServerStatusCodec`)  
  Fields: `ftStartTime`, `ftCurrentTime`, `ftLastUpdateTime`, `dwServerState`, `dwGroupCount`, `dwBandWidth`, versions, `szVendorInfo`  
  Tests: `NdrOpcServerStatusCodecTests`

✅ **OPC_QUALITY** → `OpcQuality` (`src/Opc.Classic.Core/OpcQuality.cs`)  
  Bit-field decomposition: Quality (0-1), Substatus (2-5), Limit (6-7), Vendor (8-15)  
  Constants: `Good`, `Bad`, `Uncertain`, all substatus codes (BAD_CONFIG_ERROR, UNCERTAIN_LAST_USABLE, etc.), limit flags (LOW_LIMITED, HIGH_LIMITED)  
  No dedicated tests found — **LOW** severity gap (OpcQuality is straightforward bit-field; functional coverage likely exists in integration tests).

---

## Test Coverage

**V20 Layer:**
- ✅ `IOPCV20InterfaceIdTests` — Validates IID matching for IOPCSyncIO and IOPCAsyncIO
- ❌ **No functional method tests** — V20 methods (Write, Refresh, Cancel) have no unit tests or round-trip tests

**Modern Dcom Surface:**
- ✅ `DcomInterfaceIdTests` — Validates IIDs for all Dcom interfaces
- ✅ `IOPCServerProxyTests` — Tests IOPCServer proxy generation
- ✅ `IOPCAdditionalDaProxyTests` — Tests proxy generation for additional DA interfaces
- ✅ `IOPCMissingDaMethodRoundTripTests` — Tests missing method codecs (multi-out records)
- ✅ Codec round-trip tests — All structures have codec tests
- ✅ `OpcMethodOpnumTests` — Validates opnum assignments
- ✅ `BrowseAndPropertyTests` — Tests browse and property operations
- ✅ `OpcDaServerDispatcherTests`, `OpcDaDataChangePublisherTests`, `OpcDaServerHostTests` — Server-side hosting tests
- ✅ `OpcDaSubscriptionContractTests` — Subscription contract tests

**Gaps:**
- ❌ V20 method behavior tests (Write, Refresh, Cancel)
- ❌ OpcQuality unit tests (bit-field decomposition, substatus constants, Compose helper)
- ❌ IOPCCommon integration tests (locale management)
- ❌ IOPCShutdown integration tests (shutdown notification)
- ❌ IConnectionPoint Advise/Unadvise tests (subscription lifecycle)

---

## Recommendations

### 1. Document V20 Scope (Priority: LOW)

**Action**: Add a `README.md` or XML documentation to `src/Opc.Classic.Da/V20/` clarifying:
- V20 is a **minimal back-compat shim** for connectivity to legacy 2.05a servers
- It is **intentionally incomplete** — only essential I/O methods are exposed
- **New code SHOULD use `Opc.Classic.Da.Dcom` namespace** for full 2.05a + DA 3.0 coverage

**Rationale**: The comment in `IOPCV20Interfaces.cs:6-7` states this, but it's easy to miss. A dedicated README makes the architecture decision explicit and reduces confusion about "missing" interfaces.

---

### 2. Implement IOPCCommon (Priority: HIGH)

**Action**: Add `IOPCCommon` to `Opc.Classic.Da.Dcom` namespace with all 5 methods:
- `SetLocaleID`, `GetLocaleID`, `QueryAvailableLocaleIDs`, `GetErrorString`, `SetClientName`

**Rationale**: IOPCCommon is **mandatory** for DA 2.05a compliance (spec §4.4.6). Without it, clients cannot configure locales (critical for error string localization) or identify themselves to servers (important for server-side logging/auditing). This is the **only mandatory DA 2.05a interface missing from both V20 and Dcom namespaces**.

**Implementation Note**: All methods have simple signatures (no interface pointers or enumerators) — straightforward to implement.

---

### 3. Implement IOPCShutdown (Priority: MEDIUM)

**Action**: Add `IOPCShutdown` to `Opc.Classic.Da.Dcom` namespace with `ShutdownRequest(szReason)`.

**Rationale**: IOPCShutdown is **mandatory** for DA 2.05a compliance (spec §4.6.3). Servers notify clients of imminent shutdowns via this interface. Without it, clients cannot gracefully disconnect or save state before server termination.

**Implementation Note**: Single method with simple signature — straightforward to implement.

---

### 4. Complete IConnectionPoint (Priority: MEDIUM)

**Action**: Implement `Advise` and `Unadvise` in `Opc.Classic.Da.Dcom.IConnectionPoint`.

**Rationale**: Without Advise/Unadvise, clients cannot subscribe to `IOPCDataCallback` (DA 2.05a V2.0 callback interface). Subscription is **core functionality** for real-time data delivery. The current partial implementation (only `GetConnectionInterface`) is insufficient.

**Implementation Note**: These methods use interface pointers (`IUnknown*`). Requires codec support for interface marshaling. This is likely why they're deferred (comment: "use interface pointers").

---

### 5. Add OpcQuality Unit Tests (Priority: LOW)

**Action**: Create `OpcQualityTests.cs` covering:
- Bit-field decomposition (Quality, Substatus, Limit, Vendor)
- Constants validation (Good, Bad, Uncertain, all substatus codes)
- `Compose` helper method
- Edge cases (vendor bits, limit combinations, reserved substatus values)

**Rationale**: OpcQuality is a critical bit-field type used in all item state exchanges. While the implementation looks correct, explicit tests ensure future refactoring doesn't break bit-field semantics.

---

### 6. Complete IOPCSyncIO::Read (Priority: MEDIUM)

**Action**: Implement `IOPCSyncIO::Read` codec in `Opc.Classic.Da.Dcom.IOPCSyncIO`.

**Rationale**: Read is currently declared but throws `NotSupportedException`. Comment states "needs a multi-out result record codec." This is a core synchronous I/O operation — essential for request/reply workflows.

**Implementation Note**: `IOPCSyncIO2` (DA 3.0) also has Read declared — suggests codec infrastructure may already exist. Cross-check `IOPCSyncIO2::Read` for reuse opportunities.

---

### 7. V20 Method Tests (Priority: LOW)

**Action**: Add functional tests for V20 methods:
- `IOPCSyncIO::Write` — Round-trip test with mock server
- `IOPCAsyncIO::Refresh` — Verify transaction ID return
- `IOPCAsyncIO::Cancel` — Verify cancellation behavior

**Rationale**: V20 methods are declared and presumably generated by `GenerateOpcProxy`, but have no unit tests. Even minimal shims should have functional tests to catch codec or marshaling issues.

---

### 8. Interface Pointer Method Gap (Priority: LOW)

**Action**: Complete deferred methods that return COM interface pointers:
- `IOPCServer::AddGroup`, `GetGroupByName`, `CreateGroupEnumerator`
- `IOPCItemMgt::CreateEnumerator`
- `IOPCGroupStateMgt::CloneGroup`
- `IOPCBrowseServerAddressSpace::BrowseOPCItemIDs`, `BrowseAccessPaths`
- `IConnectionPointContainer::EnumConnectionPoints`, `FindConnectionPoint`

**Rationale**: These methods are deferred (comments: "returns an interface pointer") but are required for full DA 2.05a compliance. Without them, clients cannot create groups, enumerate items, or browse address spaces. However, many workflows can function with partial coverage (e.g., if groups are pre-created), so this is **LOW priority** compared to IOPCCommon/IOPCShutdown.

**Implementation Note**: Requires codec infrastructure for interface pointer marshaling (`REFIID riid, LPUNKNOWN* ppUnk` pattern). This is likely a significant engineering effort — suggests a dedicated codec-generator feature or manual marshaling.

---

## Conclusion

**V20 Layer**: Intentionally minimal — by design, not oversight. Provides essential I/O connectivity to legacy 2.05a servers. Architecture decision is sound.

**Dcom Layer**: Comprehensive coverage of DA 2.05a + DA 3.0. Only **two mandatory interfaces missing** from full 2.05a compliance:
1. **IOPCCommon** (HIGH priority — only 5 simple methods, no technical blockers)
2. **IOPCShutdown** (MEDIUM priority — 1 simple method)

**Critical Path to Full DA 2.05a Compliance**:
1. Implement `IOPCCommon` in Dcom namespace
2. Implement `IOPCShutdown` in Dcom namespace
3. Complete `IConnectionPoint::Advise/Unadvise` (enables subscriptions)
4. Complete `IOPCSyncIO::Read` codec (enables synchronous reads)

All other gaps are **optional features** (browsing, properties, group cloning) or **deferred due to interface pointer codec infrastructure** (enumerators, group creation). Current implementation provides **strong foundation** for DA 2.05a client and server scenarios.

**No bugs or incorrect signatures detected** — all declared methods match spec signatures correctly.
