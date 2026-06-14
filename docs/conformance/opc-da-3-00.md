# OPC DA 3.00 conformance review

**Spec:** `opc-classic-docs/OPC-DA-3.00.md` (OPC Data Access Custom Interface Specification Version 3.0, March 4, 2003).

**Scope:** DA 3.0 server, group, item-management, browse, stateless I/O, synchronous/asynchronous I/O, data callback, item-property, deadband, sampling, connection-point, structure, quality, and HRESULT surfaces. DA 2.x carry-over interfaces required by DA 3.0 version interoperability are included where they are part of the DA 3.0 object model.

**Implementing assemblies:** `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Dcom`, `Opc.Classic.Hosting.Windows`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCServer` (6 methods) | §4.3.4 | ✅ source-generated proxy + dispatcher; managed server contract; Windows CCW bodies | ✅ | partial hardening gap — see §3.2.3 |
| `IOPCCommon` (5 methods) | §4.3.3 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IOPCBrowse` (2 methods) | §4.3.6 | ✅ projection + default browse implementation | ✅ | conformant for managed DCOM; CCW minimal stub |
| `IOPCItemIO` (2 methods) | §4.3.7 | ⚠️ projected and tested; default host registration incomplete | ✅ proxy/dispatcher tests | hard gap — see §3.2.1 |
| `IOPCItemProperties` (3 methods) | §4.2.14 / DA 2.x carry-over | ✅ projection + default standard property set | ✅ | conformant |
| `IOPCItemMgt` (7 methods) | §4.4.2 | ✅ projection + `OpcDaGroup` implementation + Windows CCW | ✅ | conformant |
| `IOPCGroupStateMgt` / `IOPCGroupStateMgt2` | §4.4.3 - §4.4.4 | ✅ group state + keep-alive | ✅ | conformant |
| `IOPCSyncIO` / `IOPCSyncIO2` | §4.4.5 - §4.4.6 | ✅ sync read/write + max-age/VQT paths | ✅ | conformant |
| `IOPCAsyncIO2` / `IOPCAsyncIO3` | §4.4.7 - §4.4.8 | ✅ async transaction/cancel/enable/max-age/VQT projection; callback delivery helpers | ✅ | conformant with soft callback-depth caveat |
| `IOPCItemDeadbandMgt` | §4.4.9 | ✅ per-item deadband on `OpcDaGroup`; default helper returns deterministic unsupported/not-set errors | ✅ | conformant |
| `IOPCItemSamplingMgt` (optional) | §4.4.10 | ✅ per-item sampling/buffering on `OpcDaGroup`; default helper returns deterministic unsupported/not-set errors | ✅ | conformant (optional surface implemented) |
| `IConnectionPointContainer` / `IConnectionPoint` on OPCGroup | §4.4.11 | ✅ managed and Windows CCW callback connection routing | ✅ | conformant |
| `IConnectionPointContainer` on OPCServer / `IOPCShutdown` | §4.3.5 / §4.5.2 | ⚠️ `IOPCShutdown` projected; server-side connection point not verified | ⚠️ partial | hard gap — see §3.2.2 |
| `IEnumOPCItemAttributes` | §4.4.12 | ✅ projection + managed enumerator + Windows CCW | ✅ | conformant |
| Structures, constants, qualities, HRESULTs | §6 - §8 | ✅ DA structs/codecs/result constants | ✅ codec + result tests | conformant; NaN quality rule tracked in §2 |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCServer` (spec §4.3.4)

6 wire-level methods.

| Method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---:|---|---|---|
| `AddGroup` | 3 | `Opc.Classic.Da.Dcom.IOPCServer.OpcProxy.g.cs` (generated from `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`) | `Opc.Classic.Da.Dcom.IOPCServer.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Wire/IOPCServerAddGroupWireFixtures.cs`, `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupRegistrationTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `GetErrorString` | 4 | generated | generated | `tests/Opc.Classic.Da.Tests/Dcom/IOPCServerProxyTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs` |
| `GetGroupByName` | 5 | generated | generated | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupRegistrationTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `GetStatus` | 6 | generated | generated | `tests/Opc.Classic.Da.Tests/NdrOpcServerStatusCodecTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `RemoveGroup` | 7 | generated | generated | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupRegistrationTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaServerCcwTests.cs` |
| `CreateGroupEnumerator` | 8 | generated | generated | `tests/Opc.Classic.Da.Tests/OpcMethodOpnumTests.cs`; Windows CCW hard gap in §3.2.3 |

Managed server hosting flows through `src/Opc.Classic.Da/Hosting/IOpcDaServer.cs`, `src/Opc.Classic.Da/Hosting/OpcDaServerDispatcher.cs`, and the sample/conformance server `samples/Opc.Classic.Samples.CttServer/CttDaServer.cs`.

### 1.2 `IOPCCommon` (spec §4.3.3)

| Method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---:|---|---|---|
| `SetLocaleID` | 3 | `Opc.Classic.Da.Dcom.IOPCCommon.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCCommon.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/SetClientNameTests.cs` |
| `GetLocaleID` | 4 | generated | generated | same |
| `QueryAvailableLocaleIDs` | 5 | generated | generated | same |
| `GetErrorString` | 6 | generated | generated | same |
| `SetClientName` | 7 | generated | generated | `tests/Opc.Classic.Hosting.Windows.Tests/Da/SetClientNameTests.cs` |

`OpcDaServerDispatcher` stores per-connection client diagnostics and delegates locale/error-text methods to `IOPCCommon` or `IOpcDaServer`.

### 1.3 `IOPCBrowse` and `IOPCItemProperties` (spec §4.2.14 / §4.3.6)

| Interface / method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---:|---|---|---|
| `IOPCBrowse::GetProperties` | 3 | `Opc.Classic.Da.Dcom.IOPCBrowse.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCBrowse.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs`, `tests/Opc.Classic.Da.Tests/Wire/GetItemPropertiesStandardSetFixtureTests.cs` |
| `IOPCBrowse::Browse` | 4 | generated | generated | `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs`, `tests/Opc.Classic.Da.Tests/NdrOpcBrowseElementCodecTests.cs` |
| `IOPCItemProperties::QueryAvailableProperties` | 3 | `Opc.Classic.Da.Dcom.IOPCItemProperties.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCItemProperties.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/DefaultDaInterfacesTests.cs`, `tests/Opc.Classic.Da.Tests/OpcItemPropertiesAdditionalTests.cs` |
| `IOPCItemProperties::GetItemProperties` | 4 | generated | generated | same |
| `IOPCItemProperties::LookupItemIDs` | 5 | generated | generated | same |

Implementation evidence: `src/Opc.Classic.Da/Hosting/DefaultBrowse.cs`, `src/Opc.Classic.Da/Hosting/DefaultItemProperties.cs`, `src/Opc.Classic.Da/OpcBrowseElementResult.cs`, `src/Opc.Classic.Da/OpcItemProperties.cs`, and `src/Opc.Classic.Da/Ndr/NdrOpcBrowseElementCodec.cs`.

### 1.4 `IOPCItemIO` and `IOPCSyncIO2` (spec §4.3.7 / §4.4.6)

| Interface / method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---:|---|---|---|
| `IOPCItemIO::Read` | 3 | `Opc.Classic.Da.Dcom.IOPCItemIO.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCItemIO.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCMissingDaMethodRoundTripTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/GeneratedServerDispatcherTests.cs` |
| `IOPCItemIO::WriteVQT` | 4 | generated | generated | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/GeneratedServerDispatcherTests.cs` |
| `IOPCSyncIO2::Read` | 3 | `Opc.Classic.Da.Dcom.IOPCSyncIO2.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCSyncIO2.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupItemStateTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `IOPCSyncIO2::Write` | 4 | generated | generated | same |
| `IOPCSyncIO2::ReadMaxAge` | 5 | generated | generated | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `IOPCSyncIO2::WriteVQT` | 6 | generated | generated | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |

`IOPCSyncIO2` is implemented by `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs`. `IOPCItemIO` is projected and code-generated, but the default host does not yet register an item-IO implementation for the OPCServer object (hard gap §3.2.1).

### 1.5 `IOPCItemMgt`, `IOPCGroupStateMgt`, and `IEnumOPCItemAttributes` (spec §4.4.2 - §4.4.4 / §4.4.12)

| Interface / method group | Opnums | Source proxy | Source dispatcher | Tests |
|---|---|---|---|---|
| `IOPCItemMgt` (`AddItems`, `ValidateItems`, `RemoveItems`, `SetActiveState`, `SetClientHandles`, `SetDatatypes`, `CreateEnumerator`) | 3-9 | `Opc.Classic.Da.Dcom.IOPCItemMgt.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCItemMgt.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupItemMgtTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `IOPCGroupStateMgt` (`GetState`, `SetState`, `SetName`, `CloneGroup`) | 3-6 | `Opc.Classic.Da.Dcom.IOPCGroupStateMgt.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCGroupStateMgt.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupTests.cs` |
| `IOPCGroupStateMgt2` (`SetKeepAlive`, `GetKeepAlive`) | 7-8 | `Opc.Classic.Da.Dcom.IOPCGroupStateMgt2.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCGroupStateMgt2.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `IEnumOPCItemAttributes` (`Next`, `Skip`, `Reset`, `Clone`) | 3-6 | `Opc.Classic.Da.Dcom.IEnumOPCItemAttributes.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IEnumOPCItemAttributes.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaItemAttributesEnumeratorTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcEnumOpcItemAttributesCcwTests.cs` |

Implementation evidence: `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs`, `src/Opc.Classic.Da/Hosting/OpcDaItemAttributesEnumerator.cs`, `src/Opc.Classic.Da/OpcItemDef.cs`, `src/Opc.Classic.Da/OpcItemResult.cs`, and `src/Opc.Classic.Da/OpcItemAttributes.cs`.

### 1.6 `IOPCAsyncIO2`, `IOPCAsyncIO3`, `IConnectionPoint`, and `IOPCDataCallback` (spec §4.4.7 - §4.5.1)

| Interface / method group | Opnums | Source proxy | Source dispatcher | Tests |
|---|---|---|---|---|
| `IOPCAsyncIO2` (`Read`, `Write`, `Refresh2`, `Cancel2`, `SetEnable`, `GetEnable`) | 3-8 | `Opc.Classic.Da.Dcom.IOPCAsyncIO2.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCAsyncIO2.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupAsyncIoTests.cs`, `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `IOPCAsyncIO3` (`ReadMaxAge`, `WriteVQT`, `RefreshMaxAge` plus inherited async controls) | 5-11 | `Opc.Classic.Da.Dcom.IOPCAsyncIO3.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCAsyncIO3.OpcServerDispatch.g.cs` | same, plus `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs` |
| `IConnectionPointContainer` / `IConnectionPoint` on OPCGroup | 3-6 | generated where supported; hand-written client proxy for `IConnectionPoint` | `Opc.Classic.Da.Dcom.IConnectionPoint*.OpcServerDispatch.g.cs`; Windows CCW methods in `OpcDaGroupCcwConnectionPointMethods.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDaGroupCcwTests.cs`, `tests/Opc.Classic.Da.Tests/Dcom/IOPCAdditionalDaProxyTests.cs` |
| `IOPCDataCallback` (`OnDataChange`, `OnReadComplete`, `OnWriteComplete`, `OnCancelComplete`) | 3-6 | `Opc.Classic.Da.Dcom.IOPCDataCallback.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCDataCallback.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Da/OpcDataCallbackProxyTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/OpcDaDataChangePublisherTests.cs` |

`OpcDaGroup` maintains callback enable state, sink registration, data-change fan-out, and cancel-complete fan-out. The Windows CCW path exposes `Advise`/`Unadvise`, `EnumConnections`, and `EnumConnectionPoints` over native COM.

### 1.7 `IOPCItemDeadbandMgt` and `IOPCItemSamplingMgt` (spec §4.4.9 - §4.4.10)

| Interface / method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---:|---|---|---|
| `SetItemDeadband` | 3 | `Opc.Classic.Da.Dcom.IOPCItemDeadbandMgt.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCItemDeadbandMgt.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/DefaultItemDeadbandMgtAdditionalTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupTests.cs` |
| `GetItemDeadband` | 4 | generated | generated | same |
| `ClearItemDeadband` | 5 | generated | generated | same |
| `SetItemSamplingRate` | 3 | `Opc.Classic.Da.Dcom.IOPCItemSamplingMgt.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCItemSamplingMgt.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/DefaultItemSamplingMgtAdditionalTests.cs`, `tests/Opc.Classic.Da.Tests/Hosting/OpcDaGroupTests.cs` |
| `GetItemSamplingRate` | 4 | generated | generated | same |
| `ClearItemSamplingRate` | 5 | generated | generated | same |
| `SetItemBufferEnable` | 6 | generated | generated | same |
| `GetItemBufferEnable` | 7 | generated | generated | same |

Per-item implementations live in `OpcDaGroup`; default server-object helpers live in `DefaultItemDeadbandMgt` and `DefaultItemSamplingMgt` and return spec HRESULTs such as `OPC_E_DEADBANDNOTSUPPORTED`, `OPC_E_DEADBANDNOTSET`, `OPC_E_RATENOTSET`, and `OPC_E_NOBUFFERING`.

### 1.8 Structures, quality, and HRESULT coverage (spec §6 - §8)

| Surface | Source | Tests |
|---|---|---|
| `OPCSERVERSTATUS` | `src/Opc.Classic.Core/OpcServerStatus.cs`, `src/Opc.Classic.Da/Ndr/NdrOpcServerStatusCodec.cs` | `tests/Opc.Classic.Da.Tests/NdrOpcServerStatusCodecTests.cs`, `tests/Opc.Classic.Da.Tests/Wire/NdrOpcServerStatusWireFixtures.cs` |
| `OPCITEMSTATE` | `src/Opc.Classic.Da/OpcItemState.cs`, `src/Opc.Classic.Da/Ndr/NdrOpcItemStateCodec.cs` | `tests/Opc.Classic.Da.Tests/NdrOpcItemStateCodecTests.cs`, snapshots |
| `OPCITEMDEF` / `OPCITEMRESULT` / `OPCITEMATTRIBUTES` | `src/Opc.Classic.Da/OpcItemDef.cs`, `OpcItemResult.cs`, `OpcItemAttributes.cs` | `tests/Opc.Classic.Da.Tests/NdrOpcItemDefCodecTests.cs`, `NdrOpcItemResultCodecTests.cs`, `NdrOpcItemAttributesCodecTests.cs` |
| `OPCITEMPROPERTY` / `OPCITEMPROPERTIES` / `OPCBROWSEELEMENT` | `src/Opc.Classic.Da/OpcItemPropertyResult.cs`, `OpcItemProperties.cs`, `OpcBrowseElementResult.cs` | `tests/Opc.Classic.Da.Tests/NdrOpcItemPropertyCodecTests.cs`, `NdrOpcItemPropertiesCodecTests.cs`, `NdrOpcBrowseElementCodecTests.cs` |
| `OPCITEMVQT` | `src/Opc.Classic.Da/OpcItemVqt.cs`, `src/Opc.Classic.Da/Ndr/NdrOpcItemVqtCodec.cs` | `tests/Opc.Classic.Da.Tests/NdrOpcItemVqtCodecTests.cs` |
| OPC DA HRESULTs | `src/Opc.Classic.Core/OpcResultId.cs` | `tests/Opc.Classic.Da.Tests/IdentifiedResultAdditionalTests.cs`, `tests/Opc.Classic.Core.Tests/OpcResultIdTests.cs` |
| DA 3.0 CATID/IIDs | `src/Opc.Classic.Core/OpcGuids.cs` | `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs`, `tests/Opc.Classic.Da.Tests/DcomInterfaceIdTests.cs` |

The cross-implementation matrix reported in the aggregate conformance notes is 8/8 GREEN at 105/0 per implementation, providing empirical wire-level conformance evidence for the generated proxy/dispatcher and codec paths.

---

## 2 Normative-clause checklist

OPC-DA-3.00 contains 6 normative MUST/SHALL entries in the Phase 0 inventory (`opc-da-3-00-clauses.csv`); the boilerplate warranty disclaimer entry is not an implementation requirement.

| § | Clause | Status | Evidence |
|---|---|---|---|
| §4.2.12 | NaN values read as native `VT_R4`/`VT_R8` must be returned with `OPC_QUALITY_BAD`; NaN writes must supply `OPC_QUALITY_BAD`. | ⚠️ unverified — Phase 2 deep-validation will close | Quality constants exist in `src/Opc.Classic.Da/Hosting/OpcDaItemQuality.cs`; no specific NaN-quality test was verified in this pass. |
| §4.3.6.1 | Final `IOPCBrowse::Browse` continuation response must reset the continuation point to a NULL string when no larger than `dwMaxElementsReturned`. | ✅ honored | `src/Opc.Classic.Da/Hosting/DefaultBrowse.cs` sets `continuationPoint` to `string.Empty` when `moreElements` is false; `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs` and `DaBrowseContinuationPointTests` coverage are cited in aggregate conformance notes. |
| §6.5 | Variant data-item NaN quality rule repeats §4.2.12. | ⚠️ unverified — Phase 2 deep-validation will close | Same evidence as §4.2.12. |
| §6.7.1 | `OPCITEMSTATE` NaN quality rule repeats §4.2.12. | ⚠️ unverified — Phase 2 deep-validation will close | `src/Opc.Classic.Da/OpcItemState.cs` and `src/Opc.Classic.Da/Ndr/NdrOpcItemStateCodec.cs`; no specific NaN-quality test was verified in this pass. |
| §6.7.9 | For a browse hint without valid `ItemID`, `ItemProperties.hrError` must be `OPC_E_INVALIDITEMID`. | ⚠️ unverified — Phase 2 deep-validation will close | `src/Opc.Classic.Da/OpcBrowseElementResult.cs` and `src/Opc.Classic.Da/OpcItemProperties.cs` can represent this; default browse currently emits branch/item elements only. |

The remaining conformance evidence is interface-shape, opnum, wire-format, and behavior coverage and is covered in §1.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 `IOPCItemSamplingMgt` policy depth is implementation-specific

The interface is optional in the DA 3.0 object model and is implemented. The default helper returns deterministic `OPC_E_RATENOTSET` / `OPC_E_NOBUFFERING` responses when a server does not supply custom sampling policy. Status: **WAIVED** for default helper behavior; server authors can opt into richer sampling through `OpcDaGroup` or custom implementations.

#### 3.1.2 Windows CCW top-level browse/property/item-IO bodies are intentionally minimal

The native Windows CCW exposes top-level DA 3.0 browse, property, and item-IO vtables so native probes can bind. Some bodies return empty success payloads rather than translating the full managed address-space/property/value model. This is a cross-platform-first design compromise; the managed DCOM path carries the complete source-generated wire format. Status: **WAIVED** as interop hardening work unless a required method returns `E_NOTIMPL` (see §3.2.3).

#### 3.1.3 NaN quality and browse-hint rules need targeted tests

The structures/codecs can represent the required values, but this pass did not verify dedicated tests for the NaN quality MUSTs or browse-hint `OPC_E_INVALIDITEMID`. Status: **unverified — Phase 2 deep-validation will close**.

### 3.2 Hard gaps

#### 3.2.1 Top-level `IOPCItemIO` is projected but not registered by the default managed DA host

Spec §4.3.1 and §4.3.7 put `IOPCItemIO` on the OPCServer object. The interface, proxy, and dispatcher exist in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, and dispatcher tests exist in `tests/Opc.Classic.Da.Tests/Hosting/GeneratedServerDispatcherTests.cs`; however `src/Opc.Classic.Da/Hosting/OpcDaServerHost.cs` registers `IOPCBrowse`, `IOPCBrowseServerAddressSpace`, `IOPCItemProperties`, `IOPCItemDeadbandMgt`, and `IOPCItemSamplingMgt`, but not `IOPCItemIO`. Status: **HARD GAP**.

#### 3.2.2 OPCServer `IConnectionPointContainer` for `IOPCShutdown` is not verified as wired

Spec §4.3.5 requires DA 2.0+ compliant servers to support `IConnectionPointContainer` on the OPCServer object, and `FindConnectionPoint` must support `IID_IOPCShutdown`. The `IOPCShutdown` sink is projected in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`, but this pass only verified group-level data-callback connection points in `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs` and `src/Opc.Classic.Hosting.Windows/Da/OpcDaGroupCcwConnectionPointMethods.cs`. Status: **HARD GAP** until the OPCServer-level shutdown connection point is explicitly wired and tested.

#### 3.2.3 Windows CCW `IOPCServer::CreateGroupEnumerator` returns `E_NOTIMPL`

Spec §4.3.4.6 requires `CreateGroupEnumerator`. The source-generated managed DCOM interface exists and the sample CTT server returns an interface reference, but the Windows COM-callable wrapper implementation in `src/Opc.Classic.Hosting.Windows/Da/OpcDaServerCcw.cs` currently returns `E_NOTIMPL`. Status: **HARD GAP** for native Windows COM interop.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC DA 3.00](../CONFORMANCE.md#opc-da-300)
- OPC Common callback/error/locale baseline: [`docs/conformance/opc-common-1-10.md`](opc-common-1-10.md)
- DA 2.05a carry-over surfaces: aggregate [`docs/CONFORMANCE.md` § OPC DA 2.05a](../CONFORMANCE.md#opc-da-205a)
- Architecture overview: [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-DA-3.00.md` (OPC Data Access Custom Interface Specification Version 3.0, March 4, 2003).

Phase 0 inventory:

- `files/conformance/inventory/opc-da-3-00-headings.csv` (154 entries)
- `files/conformance/inventory/opc-da-3-00-clauses.csv` (6 normative entries)
- `files/conformance/inventory/opc-da-3-00-interfaces.csv` (36 interface + 45 method references)
