# OPC DA 2.05a conformance review

**Spec:** `opc-classic-docs/OPC-DA-2.05A.md` (OPC Data Access Custom Interface Specification 2.05a).

**Scope:** DA 2.x server, group, item-management, synchronous I/O, asynchronous I/O 2.0, data-callback, browse, item-property, item-attribute enumerator, public-group optional surfaces, legacy V20 shim, and OPC Common carry-overs (`IOPCCommon`, `IOPCShutdown`, `IConnectionPointContainer`).

**Implementing assemblies:** `Opc.Classic.Da`, `Opc.Classic.Core`, `Opc.Classic.Hosting.Windows`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCServer` (6 methods) | §4.4.4 | ✅ source-generated proxy + dispatcher; managed DCOM and Windows CCW all-scope group enumerators | ✅ | conformant |
| `IOPCCommon` (5 methods) | §4.4.3 / Common §7 | ✅ source-generated proxy + dispatcher; ⚠️ Windows CCW implements only `SetClientName` | ✅ | hard gap (unverified — Phase 2 deep-validation will close) |
| `IOPCServerPublicGroups` (optional, 2 methods) | §4.4.7 | ❌ not projected | n/a | soft gap — optional public groups |
| `IOPCBrowseServerAddressSpace` (5 methods) | §4.4.8 | ✅ source-generated proxy + dispatcher; ✅ Windows CCW tearoff | ✅ | conformant |
| `IOPCItemProperties` (3 methods) | §4.4.6 | ✅ source-generated proxy + dispatcher; ✅ Windows CCW tearoff | ✅ | conformant |
| `IOPCGroupStateMgt` (4 methods) | §4.5.3 | ✅ source-generated proxy + dispatcher + managed group | ✅ | conformant |
| `IOPCPublicGroupStateMgt` (optional, 2 methods) | §4.5.4 | ❌ not projected | n/a | soft gap — optional public groups |
| `IOPCItemMgt` (7 methods) | §4.5.2 | ✅ source-generated proxy + dispatcher + Windows CCW group | ✅ | conformant |
| `IOPCSyncIO` (2 methods) | §4.5.5 | ✅ main DA projection + V20 shim | ✅ | conformant |
| `IOPCAsyncIO2` (6 methods) | §4.5.6 | ✅ source-generated proxy + dispatcher + Windows CCW group | ✅ | conformant |
| `IOPCDataCallback` (4 methods) | §4.6.1 | ✅ outbound proxy + Windows callback proxy | ✅ | conformant |
| `IConnectionPointContainer` / `IConnectionPoint` on OPCGroup | §4.5.7 | ✅ group connection-point container, point, and enumerators | ✅ | conformant |
| `IConnectionPointContainer` on OPCServer for `IOPCShutdown` | §4.4.5 / §4.6.2 | ✅ DCOM interface definitions; ❌ Windows DA server CCW does not expose the container | ⚠️ partial | hard gap (unverified — Phase 2 deep-validation will close) |
| `IEnumOPCItemAttributes` (4 methods) | §4.5.8 | ✅ source-generated proxy + dispatcher + Windows CCW enumerator | ✅ | conformant |
| Legacy `IOPCAsyncIO` / `IDataObject` / `IAdviseSink` | §4.3.4 / §4.5.9 / §4.5.10 / §4.6.3 | ⚠️ minimal V20 shim only (`Refresh`, `Cancel`) | ⚠️ IID/proxy smoke | soft gap — legacy path deferred |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCServer` (spec §4.4.4)

6 wire-level methods.

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---|---|---|---|
| `AddGroup` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaServerCcw.cs` | `tests/Opc.Classic.Da.Tests/OpcMethodOpnumTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `GetErrorString` | 4 | same | same | same |
| `GetGroupByName` | 5 | same | same | same |
| `GetStatus` | 6 | same | same | `tests/Opc.Classic.Da.Tests/NdrOpcServerStatusCodecTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `RemoveGroup` | 7 | same | same | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `CreateGroupEnumerator` | 8 | same | ✅ `IEnumString` name scopes and `IEnumUnknown` connection scopes | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupEnumeratorLoopbackTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerGroupEnumeratorCcwTests.cs` |

The source-generated path declares all required DA 2.05a opnums in `IOPCInterfaces.cs`. `IOpcDaServer` creates immutable private/public snapshots for all six `OPCENUMSCOPE` values. Private groups precede public groups in combined scopes. Name scopes return `IEnumString`; connection scopes return `IEnumUnknown`. Managed DCOM and Windows CCW enumerators implement `Next`, `Skip`, `Reset`, and cursor-preserving `Clone`; partial fetch/skip returns the COM `S_FALSE` result and connection enumerators retain interface references for the snapshot lifetime. Managed connection enumerators reuse each group's registered IPID/OXID/OID identity rather than registering duplicate dispatcher objects for the same group.

### 1.2 `IOPCCommon` (spec §4.4.3 / OPC Common §7)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---|---|---|---|
| `SetLocaleID` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaServerDispatcher.cs` | ❌ `E_NOTIMPL` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `GetLocaleID` | 4 | same | ❌ `E_NOTIMPL` | same |
| `QueryAvailableLocaleIDs` | 5 | same | ❌ `E_NOTIMPL` | same |
| `GetErrorString` | 6 | same | ❌ `E_NOTIMPL` on `IOPCCommon` tearoff; `IOPCServer::GetErrorString` works | same |
| `SetClientName` | 7 | same | ✅ records and forwards client name | `tests/Opc.Classic.Hosting.Windows.Tests/Da/SetClientNameTests.cs` |

The managed cross-platform dispatcher honors `IOPCCommon`; the DA Windows CCW still needs locale/error-string method bodies on its `IOPCCommon` tearoff.

### 1.3 `IOPCBrowseServerAddressSpace` (spec §4.4.8)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `QueryOrganization` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/DefaultBrowseServerAddressSpace.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaServerCcw.cs` | `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs` |
| `ChangeBrowsePosition` | 4 | same | same | same |
| `BrowseOPCItemIDs` | 5 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs` |
| `GetItemID` | 6 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `BrowseAccessPaths` | 7 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs` |

Spec §4.4.8 says this interface MUST be implemented as a separate interface on the single underlying Data Access object; `OpcDaServerCcw.SupportsInterface` exposes that DA server tearoff.

### 1.4 `IOPCItemProperties` (spec §4.4.6)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `QueryAvailableProperties` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/DefaultItemProperties.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaServerCcw.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs`, `tests/Opc.Classic.Da.Tests/Wire/GetItemPropertiesStandardSetFixtureTests.cs` |
| `GetItemProperties` | 4 | same | same | same |
| `LookupItemIDs` | 5 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs` |

Canonical DA property IDs and property codecs are implemented in `src/Opc.Classic.Da/OpcItemProperties.cs`, `src/Opc.Classic.Da/PropertyID.cs`, and `src/Opc.Classic.Da/Ndr/NdrOpcItemPropertiesCodec.cs`.

### 1.5 `IOPCGroupStateMgt` (spec §4.5.3)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `GetState` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaGroupCcwMethods.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `SetState` | 4 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs` |
| `SetName` | 5 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `CloneGroup` | 6 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |

### 1.6 `IOPCItemMgt` (spec §4.5.2)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `AddItems` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaGroupCcwMethods.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `ValidateItems` | 4 | same | same | same |
| `RemoveItems` | 5 | same | same | same |
| `SetActiveState` | 6 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `SetClientHandles` | 7 | same | same | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupItemMgtTests.cs` |
| `SetDatatypes` | 8 | same | same | same |
| `CreateEnumerator` | 9 | same | `src/Opc.Classic.Hosting.Windows/Da/OpcEnumOpcItemAttributesCcw.cs` | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaItemAttributesEnumeratorTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcEnumOpcItemAttributesCcwTests.cs` |

### 1.7 `IOPCSyncIO` (spec §4.5.5)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `Read` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/V20/IOPCV20Interfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaGroupCcwSyncIoMethods.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `Write` | 4 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |

### 1.8 `IOPCAsyncIO2` (spec §4.5.6)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `Read` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaGroupCcwAsyncIoMethods.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `Write` | 4 | same | same | same |
| `Refresh2` | 5 | same | same | same |
| `Cancel2` | 6 | same | same | same |
| `SetEnable` | 7 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `GetEnable` | 8 | same | same | same |

### 1.9 `IOPCDataCallback` (spec §4.6.1)

| Method | Opnum | Source proxy | Windows callback proxy | Tests |
|---|---|---|---|---|
| `OnDataChange` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDataCallbackProxy.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDataCallbackProxyTests.cs` |
| `OnReadComplete` | 4 | same | same | same |
| `OnWriteComplete` | 5 | same | same | same |
| `OnCancelComplete` | 6 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |

### 1.10 Group `IConnectionPointContainer` / `IConnectionPoint` (spec §4.5.7)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `EnumConnectionPoints` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcDaGroupCcwConnectionPointMethods.cs`, `src/Opc.Classic.Hosting.Windows/Da/OpcEnumConnectionPointsCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `FindConnectionPoint` | 4 | same | same | same |
| `GetConnectionInterface` | 3 | same | same | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `Advise` | 5 | same | `src/Opc.Classic.Hosting.Windows/Da/OpcDataCallbackProxy.cs` | same |
| `Unadvise` | 6 | same | same | same |
| `EnumConnections` | COM `IConnectionPoint` | n/a | `src/Opc.Classic.Hosting.Windows/Da/OpcEnumConnectionsCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |

The group container supports `IOPCDataCallback`; `IDataObject` is intentionally rejected with `CONNECT_E_NOCONNECTION` as the old DA 1.x callback path is deferred.

### 1.11 `IEnumOPCItemAttributes` (spec §4.5.8)

| Method | Opnum | Source | Windows CCW | Tests |
|---|---|---|---|---|
| `Next` | 3 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Da/Hosting/OpcDaItemAttributesEnumerator.cs` | `src/Opc.Classic.Hosting.Windows/Da/OpcEnumOpcItemAttributesCcwMethods.cs` | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaItemAttributesEnumeratorTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcEnumOpcItemAttributesCcwTests.cs` |
| `Skip` | 4 | same | same | same |
| `Reset` | 5 | same | same | same |
| `Clone` | 6 | same | same | same |

### 1.12 `IOPCServerPublicGroups` and `IOPCPublicGroupStateMgt` (optional, spec §4.4.7 / §4.5.4)

| Interface | Methods | Source | Tests | Status |
|---|---|---|---|---|
| `IOPCServerPublicGroups` | `GetPublicGroupByName`, `RemovePublicGroup` | ❌ no projection found in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` | n/a | soft gap — optional |
| `IOPCPublicGroupStateMgt` | `GetState`, `MoveToPublic` | ❌ no projection found in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` | n/a | soft gap — optional |

### 1.13 Legacy V20 compatibility shim (old `IOPCAsyncIO`, old `IDataObject`/`IAdviseSink` paths)

`src/Opc.Classic.Da/V20/IOPCV20Interfaces.cs` intentionally exposes only `IOPCSyncIO` plus the old `IOPCAsyncIO::Refresh` and `Cancel` declarations. `tests/Opc.Classic.Da.Tests/V20/IOPCV20InterfaceIdTests.cs` and `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` smoke the shim. New DA 2.x clients should use the main `Opc.Classic.Da.Dcom` surface with `IOPCAsyncIO2` and `IOPCDataCallback`.

---

## 2 Normative-clause checklist

OPC-DA-2.05a contains 2 normative MUST/SHALL clauses in the Phase 0 inventory (`opc-da-2-05a-clauses.csv`):

| § | Clause | Status | Evidence |
|---|---|---|---|
| Front matter | "IN NO EVENT SHALL THE OPC FOUNDATION, ITS MEMBERS, OR ANY THIRD PARTY BE..." | n/a | License/legal text; not an implementation requirement. |
| §4.4.8 | "In practice, this interface MUST be implemented (like any other interface) as a separate interface on the single underlying Data Access Object." | ✅ honored | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` declares `IOPCBrowseServerAddressSpace`; `src/Opc.Classic.Hosting.Windows/Da/OpcDaServerCcw.cs` exposes a DA server tearoff; `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs` and `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs` exercise it. |

The Version Interoperability table (§4.2.1) is covered by §1: the required DA 2.0 interfaces are projected in the modern surface, with Windows CCW hard gaps called out below.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Optional public groups are not implemented

`IOPCServerPublicGroups` and `IOPCPublicGroupStateMgt` are optional in §4.4.7 and §4.5.4. Opc.Classic currently treats all managed groups as private and does not project the public-group management interfaces. Status: **WAIVED** until a product scenario requires public shared groups.

#### 3.1.2 Legacy DA 1.x async / `IDataObject` callback path is minimal

The old `IOPCAsyncIO`, `IDataObject`, and `IAdviseSink` paths are superseded by `IOPCAsyncIO2` + `IOPCDataCallback`. `src/Opc.Classic.Da/V20/IOPCV20Interfaces.cs` is intentionally a compatibility shim rather than the primary DA 2.05a surface. Status: **WAIVED** for modern DA 2.x conformance.

### 3.2 Hard gaps

#### 3.2.1 Windows DA server CCW `IOPCCommon` locale methods are stubs

`IOPCCommon::SetLocaleID`, `GetLocaleID`, `QueryAvailableLocaleIDs`, and `GetErrorString` return `E_NOTIMPL` on the Windows CCW `IOPCCommon` tearoff, while the cross-platform generated dispatcher implements the methods. Status: **OPEN** (unverified — Phase 2 deep-validation will close).

#### 3.2.2 Windows DA server CCW does not expose server-level shutdown connection point

The DA server object should expose `IConnectionPointContainer` for `IOPCShutdown` carry-over behavior. The group CCW supports connection points for `IOPCDataCallback`, but `src/Opc.Classic.Hosting.Windows/Da/OpcDaServerCcw.cs` does not advertise `IConnectionPointContainer` on the root DA server. Status: **OPEN** (unverified — Phase 2 deep-validation will close).

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC DA 2.05a](../CONFORMANCE.md#opc-da-205a)
- OPC Common carry-overs: [`opc-common-1-10.md`](opc-common-1-10.md)
- DA 3.0 follow-up matrix: [`opc-da-3-00.md`](opc-da-3-00.md) once it exists; DA 3.0 carries forward most DA 2.0 group, item, browse, and callback surface.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-DA-2.05A.md` (OPC Data Access Custom Interface Specification 2.05a).

Phase 0 inventory:

- `files/conformance/inventory/opc-da-2-05a-headings.csv` (92 entries)
- `files/conformance/inventory/opc-da-2-05a-clauses.csv` (2 normative entries)
- `files/conformance/inventory/opc-da-2-05a-interfaces.csv` (21 interfaces + 46 methods)

Validation context: cross-implementation profiles `ctt-da`, `matrikon`, `testserver`, `samples-da`, and `security-da` are green; OPC Foundation native `OpcTestClient_x64.exe` drives the DA 2.x lifecycle against the managed samples server end-to-end.
