# OPC AE 1.10 conformance review

**Spec:** `opc-classic-docs/OPC-AE-1.10.md` (OPC Alarms and Events Custom Interface 1.10, Final Release).

**Scope:** OPC Event Server, Event Subscription, Area Browser, client callback, AE-specific data structures, AE HRESULTs, component category registration, and OPC Common carry-over surfaces referenced by AE 1.10.

**Implementing assemblies:** `Opc.Classic.Ae`, `Opc.Classic.Core`, `Opc.Classic.Dcom`, `Opc.Classic.Hosting.Windows`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCEventServer` (16 methods) | §5.3.4 | ✅ source-generated proxy + dispatcher; ✅ Windows CCW method bodies | ✅ | conformant; native `opcae_ps.dll` waiver for two calls — see §3.1 |
| `IOPCEventServer2` (6 optional methods) | §5.3.5 | ✅ source-generated proxy + dispatcher; ⚠️ Windows CCW exposure unverified | ✅ managed round-trip | conformant for managed DCOM; CCW unverified — Phase 2 deep-validation will close |
| Event-server `IConnectionPointContainer` for `IOPCShutdown` | §5.3.6 | ❌ not found on AE Windows CCW during this pass | ❌ | hard gap — see §3.2 |
| `IOPCEventAreaBrowser` (4 optional methods) | §5.4.1 | ✅ source-generated proxy + dispatcher; ✅ Windows CCW | ✅ | conformant where browser is provided |
| `IOPCEventSubscriptionMgt` (8 methods) | §5.5.1 | ✅ source-generated proxy + dispatcher; ✅ Windows CCW | ✅ | conformant |
| `IOPCEventSubscriptionMgt2` (2 optional methods) | §5.5.2 | ✅ source-generated proxy + dispatcher; ⚠️ Windows CCW exposure unverified | ✅ proxy/opnum | conformant for managed DCOM; CCW unverified — Phase 2 deep-validation will close |
| Subscription `IConnectionPointContainer` / `IConnectionPoint` for `IOPCEventSink` | §5.5.3 - §5.5.4 | ✅ `FindConnectionPoint` + `Advise`/`Unadvise`; ⚠️ enumeration stubs | ✅ | soft gap for enumeration — see §3.1 |
| `IOPCEventSink::OnEvent` | §5.6.1 | ✅ source-generated callback + Windows outbound sink proxy | ✅ | conformant |
| `IOPCShutdown` carry-over | §5.6.2 / OPC Common §6.2 | ✅ common DA projection exists; ⚠️ AE server connection point missing | partial | hard gap for AE server object — see §3.2 |
| `IOPCCommon` carry-over | §5.3.3 / OPC Common §7 | ✅ common projection exists in core/DA; ❌ AE EventServer exposure not found | ❌ AE-specific | hard gap — see §3.2 |
| AE structs (`OPCEVENTSERVERSTATUS`, `OPCCONDITIONSTATE`, `ONEVENTSTRUCT`) | §5.3.4.1.1, App. D | ✅ NDR codecs + Windows native structs | ✅ | conformant |
| AE component category | §6.2 | ✅ `OpcGuids.CATID_OPCAEServer10` | ✅ core GUID tests | conformant |
| AE HRESULTs | §7 / App. F | ✅ `OpcAeResultId` | ✅ | conformant |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCEventServer` (spec §5.3.4)

16 wire-level methods, opnums 3-18.

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---|---|---|---|
| `GetStatus` | 3 | `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Ae/OpcAeServerCcwMethods.cs` | `tests/Opc.Classic.Ae.Tests/NdrOpcEventServerStatusCodecTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeServerCcwMethodsTests.cs` |
| `CreateEventSubscription` | 4 | `IOPCInterfaces.cs`; `src/Opc.Classic.Ae/Hosting/AeEventServerDispatcherInterceptor.cs` registers the tearoff | `OpcAeServerCcwMethods.cs` + `OpcAeSubscriptionCcw.cs` | `tests/Opc.Classic.Ae.Tests/AeEndToEndDispatchTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeSubscriptionCcwTests.cs` |
| `QueryAvailableFilters` | 5 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwMethodsTests.cs` |
| `QueryEventCategories` | 6 | generated | `OpcAeServerCcwMethods.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeServerCcwArrayTests.cs` |
| `QueryConditionNames` | 7 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `QuerySubConditionNames` | 8 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `QuerySourceConditions` | 9 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `QueryEventAttributes` | 10 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `TranslateToItemIDs` | 11 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `GetConditionState` | 12 | generated; `[OpcRefString]` simple-ref strings | `OpcAeServerCcwMethods.cs` | `tests/Opc.Classic.Ae.Tests/NdrOpcConditionStateCodecTests.cs`, `tests/Opc.Classic.Ae.Tests/Wire/Dr3233/Dr3233WireCaptureTests.cs`, `OpcAeServerCcwArrayTests.cs` |
| `EnableConditionByArea` | 13 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `EnableConditionBySource` | 14 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `DisableConditionByArea` | 15 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `DisableConditionBySource` | 16 | generated | `OpcAeServerCcwMethods.cs` | `OpcAeServerCcwArrayTests.cs` |
| `AckCondition` | 17 | generated; `[OpcRefString]` + deferred string arrays | `OpcAeServerCcwMethods.cs` | `Dr3233WireCaptureTests.cs`, `OpcAeServerCcwArrayTests.cs` |
| `CreateAreaBrowser` | 18 | generated | `OpcAeServerCcwMethods.cs` + `OpcAeAreaBrowserCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeAreaBrowserCcwTests.cs` |

`tests/Opc.Classic.Ae.Tests/OpcAeMethodOpnumTests.cs` verifies all AE opnums, and `tests/Opc.Classic.Ae.Tests/DcomInterfaceIdTests.cs` verifies AE IIDs.

### 1.2 `IOPCEventServer2` (spec §5.3.5, optional)

| Method | Opnum | Source | Tests | Outcome |
|---|---|---|---|---|
| `EnableConditionByArea2` | 19 | `IOPCInterfaces.cs` | `OpcAeMethodOpnumTests.cs`, `tests/Opc.Classic.Ae.Tests/Dcom/IOPCAeDeferredMethodRoundTripTests.cs` | managed DCOM conformant |
| `EnableConditionBySource2` | 20 | same | same | managed DCOM conformant |
| `DisableConditionByArea2` | 21 | same | same | managed DCOM conformant |
| `DisableConditionBySource2` | 22 | same | same | managed DCOM conformant |
| `GetEnableStateByArea` | 23 | same | same | managed DCOM conformant |
| `GetEnableStateBySource` | 24 | same | same | managed DCOM conformant |

Windows CCW exposure was not validated before the stop request; mark as **unverified — Phase 2 deep-validation will close**. The interface is optional by §5.3.5.

### 1.3 Event-server `IConnectionPointContainer` / `IOPCShutdown` (spec §5.3.6, §5.6.2)

Spec requires the OPCEventServer object to support an `IConnectionPointContainer` whose enumerator includes `IOPCShutdown`, and `FindConnectionPoint` must support `IID_IOPCShutdown`.

Current evidence found only the managed shutdown event on `src/Opc.Classic.Ae/IAeServer.cs`; the AE Windows CCW (`src/Opc.Classic.Hosting.Windows/Ae/OpcAeServerCcw.cs`) supports `IUnknown`, `IOPCEventServer`, and a legacy direct `IOPCEventSubscriptionMgt` tearoff, but no server-level `IConnectionPointContainer` for `IOPCShutdown`. This is a hard gap in the native Windows AE server-object surface.

### 1.4 `IOPCEventAreaBrowser` (spec §5.4.1)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---|---|---|---|
| `ChangeBrowsePosition` | 3 | `IOPCInterfaces.cs` | `src/Opc.Classic.Hosting.Windows/Ae/OpcAeAreaBrowserCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeAreaBrowserCcwTests.cs` |
| `BrowseOPCAreas` | 4 | generated; returns `IEnumString` | `OpcAeAreaBrowserCcw.cs` + `OpcEnumStringCcw.cs` | same |
| `GetQualifiedAreaName` | 5 | generated | `OpcAeAreaBrowserCcw.cs` | same |
| `GetQualifiedSourceName` | 6 | generated | `OpcAeAreaBrowserCcw.cs` | same |

Area browsing is optional for simple AE servers (§5.4), but where a browser is returned by `CreateAreaBrowser`, the implemented surface matches the IDL shape.

### 1.5 `IOPCEventSubscriptionMgt` (spec §5.5.1)

| Method | Opnum | Source proxy / dispatcher | Windows CCW | Tests |
|---|---|---|---|---|
| `SetFilter` | 3 | `IOPCInterfaces.cs` | `OpcAeServerCcwMethods.cs` / `OpcAeSubscriptionCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeSubscriptionCcwTests.cs`, `OpcAeEventFilterMarshalingTests.cs` |
| `GetFilter` | 4 | generated | same | same |
| `SelectReturnedAttributes` | 5 | generated as `SetReturnedAttributesAsync` | same | `OpcAeSubscriptionCcwTests.cs`, `OpcAeServerCcwArrayTests.cs` |
| `GetReturnedAttributes` | 6 | generated | same | same |
| `Refresh` | 7 | generated | same | `OpcAeSubscriptionCcwTests.cs`, `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeRefreshTests.cs` |
| `CancelRefresh` | 8 | generated | same | same |
| `GetState` | 9 | generated | same | `OpcAeSubscriptionCcwTests.cs` |
| `SetState` | 10 | generated | same | `OpcAeSubscriptionCcwTests.cs` |

`CreateEventSubscription` returns a dedicated `OpcAeSubscriptionCcw`; the managed listener path registers subscription tearoffs through `AeEventServerDispatcherInterceptor`.

### 1.6 `IOPCEventSubscriptionMgt2` (spec §5.5.2, optional)

| Method | Opnum | Source | Tests | Outcome |
|---|---|---|---|---|
| `SetKeepAlive` | 11 | `IOPCInterfaces.cs` | `OpcAeMethodOpnumTests.cs`, `tests/Opc.Classic.Ae.Tests/Dcom/IOPCEventProxyTests.cs` | managed DCOM conformant |
| `GetKeepAlive` | 12 | `IOPCInterfaces.cs` | same | managed DCOM conformant |

Windows CCW exposure was not validated before the stop request; mark as **unverified — Phase 2 deep-validation will close**. The interface is optional by §5.5.2.

### 1.7 Subscription connection point and `IOPCEventSink` (spec §5.5.3 - §5.6.1)

| Surface | Source | Tests | Outcome |
|---|---|---|---|
| `IConnectionPointContainer::FindConnectionPoint(IID_IOPCEventSink)` | `src/Opc.Classic.Hosting.Windows/Ae/OpcAeSubscriptionCcw.cs` | `tests/Opc.Classic.Hosting.Windows.Tests/Ae/OpcAeEventSinkProxyTests.cs`, `OpcAeRefreshTests.cs` | conformant |
| `IConnectionPoint::Advise` / `Unadvise` | `OpcAeSubscriptionCcw.cs` | same | conformant |
| `IConnectionPoint::EnumConnections` | `OpcAeSubscriptionCcw.cs` | `OpcAeSubscriptionCcwTests.cs` | `E_NOTIMPL` allowed by §5.5.3 |
| `IConnectionPointContainer::EnumConnectionPoints` | `OpcAeSubscriptionCcw.cs` | partial | soft gap / waiver |
| `IOPCEventSink::OnEvent` | `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs`, `src/Opc.Classic.Hosting.Windows/Ae/OpcAeEventSinkProxy.cs` | `tests/Opc.Classic.Ae.Tests/NdrOpcEventNotificationCodecTests.cs`, `OpcAeEventSinkProxyTests.cs` | conformant |

`ONEVENTSTRUCT` callback layout is covered by both the managed NDR codec and the Windows outbound sink proxy.

### 1.8 AE structures (spec §5.3.4.1.1, §5.3.4.10, §5.6.1, Appendix D)

| Structure | Source | Tests | Outcome |
|---|---|---|---|
| `OPCEVENTSERVERSTATUS` | `src/Opc.Classic.Ae/Ndr/NdrOpcEventServerStatusCodec.cs`; Windows allocation in `OpcAeServerCcwMethods.cs` | `tests/Opc.Classic.Ae.Tests/NdrOpcEventServerStatusCodecTests.cs` | conformant |
| `OPCCONDITIONSTATE` | `src/Opc.Classic.Ae/Ndr/NdrOpcConditionStateCodec.cs`; Windows allocation in `src/Opc.Classic.Hosting.Windows/Ae/OpcAeArrayMarshaler.cs` | `tests/Opc.Classic.Ae.Tests/NdrOpcConditionStateCodecTests.cs`, `OpcAeServerCcwArrayTests.cs` | conformant; native `opcae_ps.dll` waiver applies to external round-trip |
| `ONEVENTSTRUCT` | `src/Opc.Classic.Ae/Ndr/OpcEventNotificationCodec.cs`; `OpcAeEventSinkProxy.cs` native struct | `tests/Opc.Classic.Ae.Tests/NdrOpcEventNotificationCodecTests.cs`, `OpcAeEventSinkProxyTests.cs` | conformant |

### 1.9 Component category and AE HRESULTs (spec §6.2, §7, Appendix F)

| Surface | Source | Tests | Outcome |
|---|---|---|---|
| `CATID_OPCAEServer10` | `src/Opc.Classic.Core/OpcGuids.cs` | `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs` | conformant |
| `OPC_S_ALREADYACKED`, `OPC_S_INVALIDBUFFERTIME`, `OPC_S_INVALIDMAXSIZE`, `OPC_E_INVALIDBRANCHNAME`, `OPC_E_INVALIDTIME`, `OPC_E_BUSY`, `OPC_E_NOINFO` | `src/Opc.Classic.Core/OpcAeResultId.cs` | `tests/Opc.Classic.Ae.Tests/OpcAeResultIdTests.cs` | conformant |

### 1.10 `IOPCCommon` carry-over (spec §5.3.3)

OPC AE §5.3.3 imports the OPC Common `IOPCCommon` design for locale, error text, and client-name metadata. Opc.Classic has the common projection in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` and common client/server helpers in `src/Opc.Classic.Core/Dcom/`, but this pass did not find AE EventServer exposure in `src/Opc.Classic.Ae` or `src/Opc.Classic.Hosting.Windows/Ae`.

This is recorded as a hard gap for AE server-object conformance, distinct from the already-covered OPC Common document.

---

## 2 Normative-clause checklist

OPC-AE-1.10 contains 1 normative MUST/SHALL entry in the Phase 0 CSV (`opc-ae-1-10-clauses.csv`):

| § | Clause | Status | Evidence |
|---|---|---|---|
| Front matter / warranty text | "IN NO EVENT SHALL THE OPC FOUNDATION, ITS MEMBERS, OR ANY THIRD PARTY BE ..." | n/a | Legal disclaimer, not an implementer requirement. |

The actionable AE conformance requirements are interface, structure, registration, and HRESULT-shape requirements; these are covered in §1.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Native `opcae_ps.dll` round-trip limitation for `GetConditionState` / `AckCondition`

The managed encoder was corrected to emit AE's `[simple_ref]` scalar `LPWSTR` shape for `GetConditionState` and `AckCondition`, and the wire fixtures match the vendored MIDL proxy/stub layout. The remaining failure is isolated to the OPC Foundation native `opcae_ps.dll` proxy/stub path: elevated native-CCW matrix runs still observe TCP reset / connection-forcibly-closed behavior after spec-correct bytes are sent.

Status: **WAIVED external-component limitation**. The recommended operational path is the `samples-ae-managed` profile / `tcp://` direct connect, which bypasses `opcae_ps.dll`. See [`ae-wire-format.md`](ae-wire-format.md).

#### 3.1.2 Subscription `IConnectionPointContainer::EnumConnectionPoints` returns `E_NOTIMPL`

`FindConnectionPoint(IID_IOPCEventSink)` and `Advise`/`Unadvise` work, and `EnumConnections` may return `E_NOTIMPL` under §5.5.3. `EnumConnectionPoints` currently returns `E_NOTIMPL` in `OpcAeSubscriptionCcw.cs`; §5.5.3 says subscriptions must return an enumerator including `IOPCEventSink`.

Status: **WAIVED for current native-client interop**, because clients can and do use direct `FindConnectionPoint`; Phase 2 should decide whether to implement an enumerator for strict COM browsing parity.

#### 3.1.3 Optional AE 1.10 extension CCW exposure unverified

`IOPCEventServer2` and `IOPCEventSubscriptionMgt2` are generated and tested on the managed DCOM path. Windows CCW exposure was not re-validated before the stop request.

Status: **unverified — Phase 2 deep-validation will close**.

### 3.2 Hard gaps

#### 3.2.1 AE EventServer does not expose `IOPCCommon`

Spec §5.3.3 includes `IOPCCommon` on the OPCEventServer object. The AE source and Windows AE CCW evidence found in this pass does not expose `IOPCCommon` from the AE EventServer. The implementation has reusable common/DA support, but AE-specific attachment remains missing.

Suggested fix: add `IOPCCommon` to the AE host/CCW interface map and route locale, error-string, and client-name behavior through the shared common dispatcher/client helpers.

#### 3.2.2 AE EventServer lacks server-level `IConnectionPointContainer` for `IOPCShutdown`

Spec §5.3.6 requires the OPCEventServer object to support `IConnectionPointContainer` for `IOPCShutdown`; §5.6.2 defines the callback. The AE Windows CCW currently shows subscription-level connection points for `IOPCEventSink`, but no server-level shutdown connection point.

Suggested fix: add an AE EventServer connection-point-container tearoff that supports `FindConnectionPoint(IID_IOPCShutdown)`, `Advise`, and `Unadvise`, and wire it to `IAeServer.ServerShutdown`.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC AE 1.10](../CONFORMANCE.md#opc-ae-110)
- Deep wire-format audit: [`docs/conformance/ae-wire-format.md`](ae-wire-format.md)
- OPC Common carry-over review: [`docs/conformance/opc-common-1-10.md`](opc-common-1-10.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-AE-1.10.md` (OPC Alarms and Events Custom Interface 1.10 specification, Final Release).

Phase 0 inventory:

- `files/conformance/inventory/opc-ae-1-10-headings.csv` (125 entries)
- `files/conformance/inventory/opc-ae-1-10-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-ae-1-10-interfaces.csv` (11 interfaces + 39 method references)
