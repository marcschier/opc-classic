# Opc.Classic conformance

This document consolidates the OPC Classic specification coverage reviews, XML-DA client status, and cross-cutting conformance themes for `Opc.Classic` so implementers can assess protocol coverage, remaining gaps, validation evidence, and release gates from one place.

## Contents

- [Spec coverage overview](#spec-coverage-overview)
- [OPC AE 1.10](#opc-ae-110)
- [OPC Batch 2.00](#opc-batch-200)
- [OPC Common 1.10](#opc-common-110)
- [OPC Complex Data 1.00](#opc-complex-data-100)
- [OPC DA 2.05a](#opc-da-205a)
- [OPC DA 3.00](#opc-da-300)
- [OPC DX 1.00](#opc-dx-100)
- [OPC HDA 1.20](#opc-hda-120)
- [OPC Security 1.00](#opc-security-100)
- [OPC XML-DA 1.01](#opc-xml-da-101)
- [Cross-cutting themes](#cross-cutting-themes)

## Spec coverage overview

The former per-spec review set compared each OPC specification's protocol surface against the `Opc.Classic.*` implementation. Each review:

- Reads the full vendored OPC/MS spec markdown set
- Cross-references interfaces, methods, structs, error codes, and behavioral requirements against `src/Opc.Classic.*`
- Separates cross-platform DCOM/source-generated coverage from Windows CCW/native-hosting coverage
- Lists source and test file references that exist in the current tree

| Spec | Doc | Current implementation coverage | Remaining status notes |
| --- | --- | --- | --- |
| [OPC AE 1.10](#opc-ae-110) | Alarms & Events | DCOM declarations, proxies, and dispatchers cover the AE interfaces; CCW now covers subscription, area browser, event sink delivery, and the 14 array-heavy query/translate/condition/returned-attribute methods. | Remaining items are server-policy/conformance concerns, including native-client stress coverage and connection-point enumeration stubs. |
| [OPC Batch 2.00](#opc-batch-200) | Batch | 4/4 interfaces and 11/11 methods projected; batch summary/filter codecs, Batch error constants, and `OpcBatchPropertyId` metadata for IDs 400-478 are present. | Server namespace/property population semantics remain implementation work for Batch servers. |
| [OPC Common 1.10](#opc-common-110) | Common (locale, shutdown, server-list) | `IOPCCommon`, `IOPCShutdown`, `IOPCServerList(2)`, `IOPCEnumGUID`, `OpcStringFilter`, and `IDaServer.SetClientNameAsync` are covered. | All convenience helpers shipped; no open gaps. |
| [OPC Complex Data 1.00](#opc-complex-data-100) | Complex Data | Interface projections, CPX property IDs/HRESULTs, OPCBinary/XMLSchema parsers, XML serialization, OPCBinary encode/decode including BitString, `OpcCpxAddressSpace`, and `OpcCpxItemProperties` are implemented. | Type-conversion and data-filter execution remain server-specific runtime work. |
| [OPC DA 2.05a](#opc-da-205a) | DA (V20 back-compat + modern DCOM) | V20 remains a minimal compatibility shim; the modern DCOM surface covers DA 2.05a including `IOPCServer`, `IOPCCommon`, group/item management, sync/async I/O, browsing, properties, callbacks, connection points, and full lifecycle loopback coverage. | Remaining caveats are mostly V20-scope and native interop hardening, not missing modern DCOM methods. |
| [OPC DA 3.00](#opc-da-300) | DA (flagship) | DA 3.0 DCOM projections and default hosting helpers cover browse, item I/O, group keep-alive, sync/async VQT and max-age I/O, deadband, sampling, callbacks, item enumeration, continuation points, and `vEUInfo` loopback paths. | Remaining DA work is native interop edge hardening and optional custom deadband/sampling policy samples. |
| [OPC DX 1.00](#opc-dx-100) | Data eXchange | `IOPCConfiguration` has a complete hand-written client proxy backed by DX structure codecs, status records, enums, namespace helpers, and error constants. | DX server runtime/DA bridge, persistence, and live data-transfer state machine are not implemented. |
| [OPC HDA 1.20](#opc-hda-120) | Historical Data Access | 56/56 methods and 5/5 codecs are declared; CCW now covers browser, sync/async read, sync/async update, playback, annotation insert, and raw/processed advise paths. | Remaining items are server-policy concerns such as aggregate semantics, relative time parsing, persistence, and connection-point enumeration stubs. |
| [OPC Security 1.00](#opc-security-100) | Security | 6/6 methods across `IOPCSecurityNT` and `IOPCSecurityPrivate` are projected and tested. | Reference sample server (`samples/Opc.Classic.Samples.OpcSecurityServer`) ships in this release. |
| [OPC XML-DA 1.01](#opc-xml-da-101) | XML-DA (SOAP transport) | Client supports all 8 operations, SOAP 1.1, scalar/extended scalar values, array values, base64Binary, quality, errors, and polled subscriptions. | XML-DA server hosting and SOAP 1.2 are not implemented. |

### Current validation baseline

The solution baseline is 0 build warnings / 0 build errors and 2758 passed / 13 skipped / 0 failed across 25 .NET test projects.

### Read order

1. **[OPC DA 3.00](#opc-da-300)** — flagship spec and broadest runtime surface
2. **[OPC DA 2.05a](#opc-da-205a)** — modern DA 2.x coverage plus V20 compatibility scope
3. **[OPC HDA 1.20](#opc-hda-120)** — full DCOM declarations and broad CCW coverage; server semantics remain
4. **[OPC AE 1.10](#opc-ae-110)** — DCOM complete, CCW broad; native interop hardening remains
5. **[OPC Common 1.10](#opc-common-110)** — shared locale, shutdown, discovery, and convenience helpers
6. **[OPC Batch 2.00](#opc-batch-200)** — projections and property-ID helpers complete; server semantics remain
7. **[OPC Complex Data 1.00](#opc-complex-data-100)** — codecs/types and DA hosting helpers complete; conversion/filter runtime remains
8. **[OPC DX 1.00](#opc-dx-100)** — configuration client complete; runtime not implemented
9. **[OPC Security 1.00](#opc-security-100)** — optional security API coverage
10. **[OPC XML-DA 1.01](#opc-xml-da-101)** — XML/SOAP client coverage

## OPC AE 1.10

_Generated by review against the vendored `OPC-AE-1.10.md` spec and `src/Opc.Classic.Ae/`._

### Summary

- **Interfaces**: 6/6 AE interfaces plus `IEnumString` projected.
- **DCOM/source-generated path**: ✅ complete declarations, correct opnums, generated client proxies, and generated server dispatchers for `IOPCEventServer`, `IOPCEventServer2`, `IOPCEventSubscriptionMgt`, `IOPCEventSubscriptionMgt2`, `IOPCEventAreaBrowser`, and `IOPCEventSink`.
- **Windows CCW path**: ✅ vtables are present; real bodies cover `IOPCEventServer::GetStatus`, `QueryAvailableFilters`, `CreateEventSubscription`, `CreateAreaBrowser`, the 14 array-heavy query/translate/condition/returned-attribute methods, full `IOPCEventSubscriptionMgt::SetFilter/GetFilter` and returned-attribute/state marshaling, and `IOPCEventSink::OnEvent` delivery through the subscription connection point. Remaining explicit `E_NOTIMPL` responses are limited to connection-point enumeration stubs.
- **Structs/codecs**: ✅ `OPCEVENTSERVERSTATUS`, `OPCCONDITIONSTATE`, and `ONEVENTSTRUCT` are covered by runtime types/codecs and tests.

Opnums 3-24 are represented in `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs:23-180`, and subscription/browser/callback interfaces are declared in `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs:182-306`.

### Documented waiver: condition-state round-trip via the native `opcae_ps.dll` stub

> The wire-format root cause was extracted from `interop/inc/opc_ae_p.c`
> ([ae-wire-format.md](conformance/ae-wire-format.md)), validated by
> byte-level capture of the managed encoder's output (fixtures at
> `tests/Opc.Classic.Ae.Tests/Wire/Dr3233/Fixtures/`), and fixed by
> applying `[OpcRefString]` to the 4 simple_ref scalar LPWSTR
> parameters in `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs`. With the
> fix in place the captured wire bytes match the MIDL spec EXACTLY for
> both the `GetConditionState` request+response and the `AckCondition`
> request — including the OPCCONDITIONSTATE struct body byte layout,
> the 4 FILETIMEs at offsets 24/32/40/48, and the deferred-pointer
> pre-order traversal. An operator-gated `samples-ae` matrix re-run on
> a Windows host with admin elevation confirmed the 2 AE tools still
> fail through the native `opcae_ps.dll` path even with spec-compliant
> wire bytes — the client receives `SocketException (10054): An
> existing connection was forcibly closed by the remote host` after
> dispatching the spec-correct request bytes, proving the residual
> failure is in the vendor proxy/stub itself, not in the managed
> encoder. The EXPECTED_FAIL waivers therefore stay **permanent** on
> the `samples-ae` native-CCW profile, and the `samples-ae-managed`
> alternative path (which bypasses `opcae_ps.dll` via `tcp://` direct
> connect) is the recommended operational path.

Two cross-implementation interop-matrix tools — `opcclassic.ae.get_condition_state`
and `opcclassic.ae.ack_condition` — are marked `EXPECTED_FAIL` in
`tools/probe_matrix.py` (`_ae_matrix`). They therefore count as MATCH and the
`samples-ae` profile reads 104/0/0/0; this is a **documented external-component
limitation, not a defect in the managed stack** — proven conclusively by the
elevated matrix re-run: with byte-for-byte spec-correct wire bytes,
`opcae_ps.dll` still forcibly closes the connection rather than processing
the response (for `GetConditionState`) or the request (for `AckCondition`).

- **Wire-format root cause (proven by wire tracing + server-side CCW instrumentation):**
  the `samples-ae` profile registers a native CCW (`OpcAeServerCcw`) via
  `CoRegisterClassObject`, so the matrix client reaches it through the OS RPC
  runtime and the **OPC Foundation `opcae_ps.dll` MIDL proxy/stub**. Per
  the vendored `opc_ae_p.c` proxy/stub source, that stub marks the AE `LPWSTR`
  params as `[simple ref]`; tagging them `[OpcRefString]` makes
  `GetConditionState`'s request decode succeed (the managed CCW logs
  `ENTER → decoded → RETURN S_OK` and the matrix briefly reaches 103/1), but
  the `OPCCONDITIONSTATE` response round-trip then crashes `opcae_ps.dll`, and
  `AckCondition`'s `[in]` unmarshal is rejected by the stub before the CCW is
  entered.
- **Wire-format fix:** byte-level analysis of `opc_ae_p.c` showed the managed
  encoder was emitting outer 4-byte referent IDs before the simple_ref scalar
  LPWSTR params in `GetConditionState` (szSource, szConditionName) AND
  `AckCondition` (szAcknowledgerID, szComment). Fixtures captured the
  before/after wire bytes in `tests/Opc.Classic.Ae.Tests/Wire/Dr3233/Fixtures/`;
  the encoder applies `[OpcRefString]` to all 4 params; the wire bytes match
  the MIDL spec byte-by-byte. The `[OpcDeferredElements]` attribute on
  `pszSource`/`pszConditionName` correctly emits the within-parameter pile
  layout per DCE C706 §14.3.12.3; the FILETIMEs in OPCCONDITIONSTATE land at
  offsets 24/32/40/48 (multiples of 8 by struct layout coincidence, not a
  hidden alignment-override rule on FILETIME itself). The in-process AE
  round-trip tests (`AeEndToEndDispatchTests`,
  `AeManagedClientOverTransportTests`) all remain green.
- **Elevated matrix re-run finding:** running the `samples-ae` matrix
  profile elevated against a HKLM-registered samples-ae server
  confirmed `opcae_ps.dll` still forcibly closes the connection mid-call
  for both AE methods, despite the spec-compliant request wire bytes.
  The transcript shows `System.IO.IOException: Unable to {read,write}
  data from the transport connection: An existing connection was forcibly
  closed by the remote host` with inner `SocketException (10054)` for both
  `opcclassic.ae.get_condition_state` (read-side, response phase) and
  `opcclassic.ae.ack_condition` (write-side, request phase). The
  EXPECTED_FAIL waivers therefore remain **permanent** on the native-CCW
  path; downstream consumers needing these AE methods should use the
  `samples-ae-managed` profile / `tcp://` AE connect scheme, which bypasses
  `opcae_ps.dll` entirely.
- **Secondary regressions fixed during the elevated run:**
  `opcclassic.ae.disconnect` threw `ObjectDisposedException`
  because the server-side socket disposal raced ahead of the managed-side
  graceful close (defensively caught in `AeClientState.DisposeAsync`),
  and `opcclassic.capture.tail` failed with "Capture session not found"
  because the probe driver's auto-probe fallback injected the OPC session
  id instead of the capture session id (curated `ProbeSpec` alongside
  `capture.get`/`capture.summarize` handles this).

### Alternative path: `samples-ae-managed` profile

For the operator who needs AE conformance without the `opcae_ps.dll`
limitation, the `samples-ae-managed` matrix profile bypasses
`opcae_ps.dll` entirely via the managed TCP listener and counts these
2 tools as real-PASS. Use this profile when the operational goal is
"AE conformance proven"; use `samples-ae` when the goal is
specifically "native-CCW + `opcae_ps.dll` conformance".

### Closed AE coverage items and remaining gaps

#### Closed high-priority gaps

##### 1. Windows CCW `CreateEventSubscription` now returns a subscription CCW

**Spec**: `IOPCEventServer::CreateEventSubscription` (opnum 4) returns an `IOPCEventSubscriptionMgt` interface pointer.

**Cross-platform DCOM status**: ✅ declared and generated (`src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs:33-45`).
**Windows CCW status**: ✅ implemented for dispatchers that create an `IOPCEventSubscriptionMgt`; the returned `OpcAeSubscriptionCcw` covers filter, returned-attribute, refresh, cancel-refresh, and state methods.

**Impact**: Native Windows clients using the CCW can create and manage AE subscriptions for the implemented subscription-management surface.

---

##### 2. Windows CCW `CreateAreaBrowser` returns an area-browser CCW

**Spec**: `IOPCEventServer::CreateAreaBrowser` (opnum 18) returns an `IOPCEventAreaBrowser` interface pointer.

**Cross-platform DCOM status**: ✅ declared (`src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs:131-136`).
**Windows CCW status**: ✅ implemented for servers/dispatchers that provide area browsing; unsupported servers still return `E_NOTIMPL` (`src/Opc.Classic.Ae/Hosting/Windows/OpcAeServerCcwMethods.cs:203-237`, `src/Opc.Classic.Ae/Hosting/Windows/OpcAeAreaBrowserCcw.cs`).

---

##### 3. Windows CCW subscription filter marshaling and event callbacks are implemented

**Spec**: `IOPCEventSubscriptionMgt::SetFilter/GetFilter` and `IOPCEventSink::OnEvent` with `ONEVENTSTRUCT[]` callback delivery.

**Windows CCW status**: ✅ implemented. `src/Opc.Classic.Ae/Hosting/Windows/OpcAeSubscriptionCcw.cs` marshals category arrays, severity bounds, and BSTR area/source lists; `src/Opc.Classic.Ae/Hosting/Windows/OpcAeEventSinkProxy.cs` marshals `ONEVENTSTRUCT[]` including BSTRs, FILETIME, BOOL, and VARIANT event attributes; `src/Opc.Classic.Ae/Hosting/IOpcAeEventSinkRegistration.cs` plus `src/Opc.Classic.Ae/Hosting/OpcAeServerDispatcher.cs` register sinks and fan out normal/refresh notifications.

---

##### 4. Windows CCW array-heavy AE query and condition methods are implemented

**Spec**: `IOPCEventServer` opnums 6-17 and `IOPCEventSubscriptionMgt` opnums 5-6 require correlated native `LPWSTR`, `DWORD`, `VARTYPE`, `FILETIME`, `VARIANT`, `CLSID`, and `HRESULT` arrays.

**Cross-platform DCOM status**: ✅ declared/generated in `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs:51-129`, `188-247`.
**Windows CCW status**: ✅ implemented in `src/Opc.Classic.Ae/Hosting/Windows/OpcAeServerCcwMethods.cs` with shared array marshaling helpers in `OpcAeArrayMarshaler.cs`; covered by `tests/Opc.Classic.Ae.Tests/Hosting/Windows/OpcAeServerCcwArrayTests.cs:1-1028`.

**Remaining explicit `E_NOTIMPL` list**: `IOPCEventSubscriptionMgt` connection-point enumeration methods (`EnumConnections`, `EnumConnectionPoints`) remain stubs in `src/Opc.Classic.Ae/Hosting/Windows/OpcAeSubscriptionCcw.cs`.

---

### Coverage Gaps (where implementation is complete)

Current test coverage (`tests/Opc.Classic.Ae.Tests/`):

- ✅ Interface IDs and method opnums: `tests/Opc.Classic.Ae.Tests/OpcAeMethodOpnumTests.cs:1-105`
- ✅ Deferred/multi-out DCOM round trips: `tests/Opc.Classic.Ae.Tests/Dcom/IOPCAeDeferredMethodRoundTripTests.cs:1-573`
- ✅ Windows CCW method behavior for implemented methods, array-heavy AE server marshaling, and remaining `E_NOTIMPL` placeholders: `tests/Opc.Classic.Ae.Tests/Hosting/Windows/OpcAeServerCcwMethodsTests.cs:1-405`, `tests/Opc.Classic.Ae.Tests/Hosting/Windows/OpcAeServerCcwArrayTests.cs:1-1028`, and `tests/Opc.Classic.Ae.Tests/Hosting/Windows/OpcAeSubscriptionCcwTests.cs:1-704`
- ✅ Codec tests for AE condition/event/status structures: `tests/Opc.Classic.Ae.Tests/NdrOpcConditionStateCodecTests.cs:1-150`, `tests/Opc.Classic.Ae.Tests/NdrOpcEventNotificationCodecTests.cs:1-166`, and `tests/Opc.Classic.Ae.Tests/NdrOpcEventServerStatusCodecTests.cs:1-120`.

#### Integration tests recommended

1. End-to-end subscription lifecycle against an external native COM client.
2. Broader native interoperability coverage for `IOPCEventSink::OnEvent` against external COM clients.
3. Native interoperability coverage for category, condition, acknowledgement, and enable-state queries against external COM clients.
4. Area-browser navigation against an external native COM client.

### Conclusion

The AE DCOM projection is substantially more complete than older external reviews indicated: declarations and opnums are current, generated proxies/dispatchers cover the managed cross-platform path, and the Windows CCW covers the core server, subscription, browser, callback, and array-heavy AE surfaces. Remaining risk is concentrated in external native-client interoperability validation.

## OPC Batch 2.00

**Spec**: OPC Batch Custom Interface Specification Version 2.0 (July 19, 2001)
**Implementation**: `src/Opc.Classic.Batch/`
**Review target**: `1.0.0-rc.10`

---

### Summary

**Interfaces**: 4/4 projected
**Methods**: 11/11 declared/projected
**Structs**: 2/2 codecs registered (`OPCBATCHSUMMARY`, `OPCBATCHSUMMARYFILTER`)
**Error constants**: Batch-specific `OPCB_E_NOT_MEANINGFUL` is present

**Overall compliance**: **Projection complete; server semantics remain implementation-specific**. `CreateEnumerator`, `CreateFilteredEnumerator`, `IOPCEnumerationSets::*`, and `IEnumOPCBatchSummary::Clone` are projected.

---

### Implementation Status

| Interface | Methods | Status | Source |
| --- | --- :| --- | --- |
| `IOPCBatchServer` | 2/2 | ✅ `GetDelimiter`, `CreateEnumerator` | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:20-31` |
| `IOPCBatchServer2` | 1/1 | ✅ `CreateFilteredEnumerator` | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:33-40` |
| `IEnumOPCBatchSummary` | 5/5 | ✅ `Next`, `Skip`, `Reset`, `Clone`, `Count` | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:42-65` |
| `IOPCEnumerationSets` | 3/3 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Batch/Dcom/IOPCBatchInterfaces.cs:67-86` |

Interface-pointer methods use hand-written proxy/dispatcher paths where needed:

- `IOPCBatchServerClientProxy` and dispatcher: `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs:19-184`
- `IOPCBatchServer2ClientProxy` and dispatcher: `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs:50-223`
- `IEnumOPCBatchSummaryClientProxy` and dispatcher: `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs:76-311`

---

### Structures and Error Codes

| Feature | Status | Source / Test |
| --- | --- | --- |
| `OPCBATCHSUMMARY` | ✅ codec | `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryCodec.cs`; `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryCodecTests.cs:1-116` |
| `OPCBATCHSUMMARYFILTER` | ✅ codec | `src/Opc.Classic.Batch/Ndr/NdrOpcBatchSummaryFilterCodec.cs`; `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryFilterCodecTests.cs:1-137` |
| Batch HRESULTs | ✅ constants/tests | `src/Opc.Classic.Batch/OpcBatchErrors.cs`; `tests/Opc.Classic.Batch.Tests/OpcBatchErrorsTests.cs:1-21` |
| Batch property IDs | ✅ constants/metadata/tests | `src/Opc.Classic.Batch/OpcBatchPropertyId.cs:1-454`; `tests/Opc.Classic.Batch.Tests/OpcBatchPropertyIdTests.cs:1-199` |

---

### Remaining Implementation Work

#### 1. Batch namespace models

The library projects Batch interfaces and codecs but does not provide a complete vendor-neutral Batch server namespace. A compliant server still needs to expose and maintain the standard Batch namespace models such as `OPCBPhysicalModel`, `OPCBMasterRecipeModel`, `OPCBBatchModel`, `OPCBBatchArchiveModel`, and `OPCBBatchIDList`.

**Status**: server-implementation responsibility.
**Priority**: Medium for a sample/reference Batch server; not a DCOM projection gap.

#### 2. Batch DA namespace integration

`OpcBatchPropertyId` now provides the spec-defined Batch DA property IDs (400-478), descriptions, and expected VARTYPE metadata. Reference namespace population remains server-specific.

**Priority**: Low/Medium convenience for server authors.

#### 3. Enumeration-set data source

`IOPCEnumerationSets` is now projected, but a server implementation must still supply localized enumeration set names and values for standard and vendor-defined sets.

---

### Test Coverage

| Test File | Scope |
| --- | --- |
| `tests/Opc.Classic.Batch.Tests/Dcom/IOPCBatchProxyTests.cs:1-286` | Batch proxy and dispatcher payloads, including interface-reference returns |
| `tests/Opc.Classic.Batch.Tests/DcomInterfaceIdTests.cs:1-52` | IIDs |
| `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryCodecTests.cs:1-116` | Summary codec round trips |
| `tests/Opc.Classic.Batch.Tests/NdrOpcBatchSummaryFilterCodecTests.cs:1-137` | Filter codec round trips |
| `tests/Opc.Classic.Batch.Tests/OpcBatchErrorsTests.cs:1-21` | Batch HRESULT constants |
| `tests/Opc.Classic.Batch.Tests/OpcBatchPropertyIdTests.cs:1-199` | Batch DA property ID constants and metadata |

---

### Compliance Checklist (§3.5)

| Requirement | Current status | Notes |
| --- | --- | --- |
| OPC Data Access dependency | ✅ via DA runtime | Batch project depends on the DA surface for item/property semantics. |
| `IOPCBrowseServerAddressSpace` | ✅ in DA runtime | Batch namespace population remains server-specific. |
| `IOPCBatchServer` | ✅ projected | Server must implement delimiter/enumerator behavior. |
| `IOPCBatchServer2` | ✅ projected | Server must implement filter semantics. |
| `IEnumOPCBatchSummary` | ✅ projected | Clone is projected. |
| `IOPCEnumerationSets` | ✅ projected | Server must provide enumeration-set content. |
| Batch property IDs | ✅ helper | `OpcBatchPropertyId` covers IDs 400-478 with descriptions and VARTYPE metadata. |
| Batch namespace population | ⚠️ server-specific | Suitable for a future sample/reference server. |

---

### Conclusion

`Opc.Classic.Batch` now provides complete Batch DCOM projection coverage for the spec interfaces and the two required structures. The remaining work is not missing wire declarations; it is server-side semantics: maintaining the Batch namespace, property metadata, enumeration-set catalogs, and real batch summary data.

## OPC Common 1.10

**Spec version**: OPC Common Definitions and Interfaces 1.10 (December 13, 2002)
**Spec file**: vendored `OPC-COMMON-1.10.md` spec
**Reviewed assemblies**: `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Discovery`, `Opc.Classic.Dcom`

---

### Executive Summary

OPC Common 1.10 defines shared infrastructure used by DA, AE, HDA, DX, Batch, Commands, Security, and XML-DA:

- `IOPCCommon` for locale, error text, and client-name metadata
- `IOPCShutdown` for server-to-client shutdown notifications
- `IOPCServerList` / `IOPCServerList2` and `IOPCEnumGUID` for OPCEnum discovery
- Component-category GUIDs and standard OPC HRESULT codes
- The Appendix B string-filter function

**Coverage**: ✅ **High**. The DCOM interfaces are projected, the discovery client uses real hand-written proxy paths for OPCEnum enumeration, and the public convenience helpers (`OpcStringFilter`, `IDaServer.SetClientNameAsync`) have shipped. There are no open convenience gaps.

---

### 1. Interfaces

#### 1.1 IOPCCommon (§7)

**Spec**: 5 methods — `SetLocaleID`, `GetLocaleID`, `QueryAvailableLocaleIDs`, `GetErrorString`, `SetClientName`

**Implementation status**: ✅ **5/5 DCOM methods declared with source-generated proxy and dispatcher**

| Method | Status | Source |
| --- | --- | --- |
| `SetLocaleID` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `GetLocaleID` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `QueryAvailableLocaleIDs` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `GetErrorString` | ✅ | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `SetClientName` | ✅ DCOM + high-level `IDaServer.SetClientNameAsync` convenience member | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:116-120`, `src/Opc.Classic.Da/IDaServer.cs:1-108` |

`IDaServer` exposes the common high-value locale/error members (`SetLocaleAsync`, `LocaleId`, `GetSupportedLocalesAsync`, `GetErrorTextAsync`) plus optional diagnostic metadata through `SetClientNameAsync` (`src/Opc.Classic.Da/IDaServer.cs:1-108`).

---

#### 1.2 IOPCShutdown (§6)

**Spec**: 1 method — `ShutdownRequest(szReason)`

**Implementation status**: ✅ **1/1 DCOM method declared with source-generated proxy and dispatcher**

- Source: `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:729-740`
- Managed DA facade: `IDaServer.ServerShutdown` remains the high-level event pattern for clients.

---

#### 1.3 IOPCServerList / IOPCServerList2 (§9)

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

#### 1.4 IOPCEnumGUID (§9.6)

**Spec**: `Next`, `Skip`, `Reset`, `Clone`

**Implementation status**: ✅ **4/4 supported in the OPCEnum discovery proxy/dispatcher; generated DA projection declares `Next`, `Skip`, and `Reset`**

| Method | Status | Source |
| --- | --- | --- |
| `Next` | ✅ | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:127-145` |
| `Skip` | ✅ | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:147-160` |
| `Reset` | ✅ | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:162-170` |
| `Clone` | ✅ hand-written interface-ref path | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs:172-180`, `258-270` |

The generated declaration in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:799-824` intentionally omits `Clone` because it returns an enumerator interface pointer; the Discovery proxy covers that pattern for real OPCEnum usage.

---

### 2. Component Categories (CATIDs)

**Implementation status**: ✅ **All standard CATIDs defined**

`OpcGuids` contains DA, AE, HDA, DX, Batch, Commands, Security, and XML-DA category IDs and category arrays used by discovery (`src/Opc.Classic.Core/OpcGuids.cs:1-415`). Tests verify canonical values and duplicate safety (`tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs:1-154`).

---

### 3. Error Codes (HRESULTs)

**Implementation status**: ✅ **Common OPC HRESULTs are represented by `OpcResultId`**

`OpcResultId` defines the standard OPC facility and shared DA/Common result IDs, plus helper properties such as `IsFailure`, `IsSuccess`, `Facility`, and `CodePart` (`src/Opc.Classic.Core/OpcResultId.cs:1-136`). Spec-specific errors live in their owning assemblies, for example HDA, CPX, Batch, DX, Security, and XML-DA.

---

### 4. Gap Summary & Recommendations

| # | Feature | Status | Priority | Recommendation |
| --- | --- | --- | --- | --- |
| 1 | `SetClientName` high-level convenience API | ✅ Complete | Low | `IDaServer.SetClientNameAsync` forwards to the DA `IOPCCommon::SetClientName` path when the adapter supports it; the default implementation is a no-op. |
| 2 | Appendix B string filter utility | ✅ `Opc.Classic.OpcStringFilter.MatchPattern(...)` in `src/Opc.Classic.Core/OpcStringFilter.cs:1-161` | Medium | Use the existing public helper for server implementers. |
| 3 | Property ID range enforcement | Informational | Low | Keep as documentation; runtime enforcement is not required by OPC Common. |

---

### 5. Test Coverage

| Test File | Scope |
| --- | --- |
| `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs:1-154` | IID/CLSID/CATID registry and category arrays |
| `tests/Opc.Classic.Core.Tests/OpcStringFilterTests.cs:1-58` | OPC Common Appendix B string-filter helper coverage |
| `tests/Opc.Classic.Discovery.Tests/OpcEnumClientTests.cs:1-325` | OPCEnum enumeration, category merging, and error mapping |
| `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs:1-244` | Server dispatcher coverage that includes common DA hosting paths |
| `tests/Opc.Classic.Da.Tests/SetClientNameTests.cs:1-343` | `IDaServer.SetClientNameAsync` loopback, default no-op, update, and Windows CCW forwarding coverage |

---

### 6. Conclusion

OPC Common coverage is high for the wire-visible functionality required by modern OPC Classic clients. The current DCOM/discovery implementation projects `IOPCCommon` and `IOPCEnumGUID::Skip/Reset/Clone`.

## OPC Complex Data 1.00

**Spec**: OPC Complex Data Specification Version 1.00 (December 10, 2003)
**Implementation**: `src/Opc.Classic.Cpx/`
**Review target**: `1.0.0-rc.10`

---

### Summary

**Interfaces**: 3/3 extension interfaces declared (100%)
**Methods**: 11/11 declared (100%)
**Core types**: implemented
**XML/OPCBinary support**: implemented for dictionary parsing, XML complex value serialization, and OPCBinary encode/decode
**DA Properties**: IDs 600-609 defined
**Error codes**: `OPCCPX_*` constants defined

**Overall compliance**: **Projection, codec infrastructure, DA server CPX namespace/property helpers, and BitString support are complete; conversion/filter execution remains server-specific**. The codecs, property constants, namespace helpers, and CPX HRESULTs are in place.

---

### Implementation Status

#### 1. DCOM Interface Projection

| Interface | Methods | Status | Source |
| --- | --- :| --- | --- |
| `IOPCComplexDataItem` | 4/4 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs:20-41` |
| `IOPCComplexDataItem2` | 3/3 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs:43-60` |
| `IOPCTypeLibrary` | 3/3 | ✅ generated proxy + dispatcher | `src/Opc.Classic.Cpx/Dcom/IOPCCpxInterfaces.cs:62-79` |

These are vendor/industry extension interfaces around the CPX property model; the spec itself primarily standardizes DA properties, type dictionaries, namespace conventions, conversion, and filtering.

#### 2. CPX Property and Error Constants

| Feature | Status | Source |
| --- | --- | --- |
| Property IDs 600-609 | ✅ | `src/Opc.Classic.Cpx/OpcComplexDataProperty.cs:8-42` |
| CPX HRESULT constants | ✅ | `src/Opc.Classic.Cpx/OpcComplexDataResult.cs:11-48` |

#### 3. Type Systems and Codecs

| Feature | Status | Source |
| --- | --- | --- |
| OPCBinary dictionary parser | ✅ | `src/Opc.Classic.Cpx/OpcBinaryDictionaryParser.cs:15-80` |
| OPCBinary decoder | ✅ | `src/Opc.Classic.Cpx/OpcBinaryDecoder.cs:12-60` |
| OPCBinary encoder | ✅ | `src/Opc.Classic.Cpx/OpcBinaryEncoder.cs` |
| XML Schema parser | ✅ | `src/Opc.Classic.Cpx/XmlSchemaParser.cs:15-60` |
| XML complex value serializer | ✅ | `src/Opc.Classic.Cpx/XmlComplexValueSerializer.cs` |
| CPX namespace helper | ✅ | `src/Opc.Classic.Cpx/CpxNamespaceBuilder.cs:11-80` |

---

### Remaining Gaps in Implementation

#### HIGH

##### 1. DA server runtime integration — closed

The CPX assembly now includes managed DA hosting helpers that wire registered dictionaries and complex items into a server's browse namespace and item-property provider.

Implemented helpers:

1. `OpcCpxAddressSpace` exposes `/CPX/{TypeSystem}/{Dictionary}/{TypeID}` alongside an existing `IOpcAddressSpace`.
2. `OpcCpxItemProperties` publishes properties 600-609 for registered complex DA items and dictionary/type namespace items.
3. `AddOpcCpxAddressSpace` registers the address-space and property-provider decorators for managed DA hosts.

Remaining semantic behavior such as timestamp, quality, deadband, type-conversion execution, and data-filter execution is server-specific.

**Source helpers**: `src/Opc.Classic.Cpx/Hosting/OpcCpxAddressSpace.cs:1-261`, `src/Opc.Classic.Cpx/Hosting/OpcCpxItemProperties.cs:1-308`, `src/Opc.Classic.Cpx/Hosting/ServiceCollectionExtensions.cs:1-106`.

---

##### 2. Type conversion and data filter execution

The spec's §7 type-conversion and §8 data-filter behavior require server-side state and policy. Helpers exist for paths and the core data representation, but the runtime does not provide a generic conversion/filter engine.

**Status**: helper infrastructure present; semantic execution is server-specific.
**Priority**: High for a CPX sample server, lower for client-side dictionary/value decoding.

---

#### LOW

##### 3. BitString support — closed

`TypeKind.BitString` and the OPCBinary encoder/decoder now handle bit lengths that do not align to byte boundaries, including consecutive bit fields and required padding before non-BitString fields.

##### 4. End-to-end DA/CPX samples

No sample server/client currently demonstrates property-based type discovery, CPX namespace browsing, conversion branches, and data filters.

---

### Test Coverage Assessment

| Test File | Scope |
| --- | --- |
| `tests/Opc.Classic.Cpx.Tests/CpxCodecTests.cs:1-216` | OPCBinary/XML codec behavior |
| `tests/Opc.Classic.Cpx.Tests/CpxTypesTests.cs:1-190` | Type model, properties, namespace helpers, errors |
| `tests/Opc.Classic.Cpx.Tests/OpcBinaryBitStringTests.cs:1-105` | BitString encoder/decoder byte-boundary and padding behavior |
| `tests/Opc.Classic.Cpx.Tests/OpcCpxAddressSpaceTests.cs:1-95` | `OpcCpxAddressSpace` browse namespace integration |
| `tests/Opc.Classic.Cpx.Tests/OpcCpxItemPropertiesTests.cs:1-136` | `OpcCpxItemProperties` property IDs 600-609 publishing |
| `tests/Opc.Classic.Cpx.Tests/Dcom/IOPCCpxProxyTests.cs:1-66` | DCOM proxy/interface projection |

Missing tests are now mostly conversion/filter execution and end-to-end sample workflows rather than core codec/property/hosting-helper tests.

---

### Compliance Summary

| Feature | Spec § | Status | Priority |
| --- | --- | --- | --- |
| DCOM extension interfaces | vendor extension | ✅ 3/3 declared | N/A |
| Managed type model | §6 | ✅ Complete | N/A |
| XMLSchema parser | §5 | ✅ Implemented | N/A |
| OPCBinary parser/codec | §6 | ✅ Implemented | N/A |
| DA property constants | §3.3 | ✅ Implemented | N/A |
| CPX namespace helpers | §3.4 | ✅ Implemented | N/A |
| Type conversions | §7 | ⚠️ Server-specific runtime work | High |
| Data filters | §8 | ⚠️ Server-specific runtime work | High |
| Error codes | §9 | ✅ Implemented | N/A |
| BitString support | §6.2.4.2.1 | ✅ Implemented | N/A |

---

### Conclusion

CPX is not codec- or hosting-helper-blocked. Core CPX parsing, encoding/decoding, BitString support, property IDs, HRESULTs, namespace helpers, and managed DA integration helpers are present and tested. Remaining work is server-specific conversion/filter execution and sample workflows.

## OPC DA 2.05a

**Spec**: OPC Data Access Custom Interface Specification 2.05a
**Implementation**: `src/Opc.Classic.Da/V20/IOPCV20Interfaces.cs` (minimal V2 back-compat layer), `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` (modern DA surface)
**Review target**: `1.0.0-rc.10`

---

### Summary

#### V20 Back-Compat Layer

- **Status**: intentionally minimal.
- **Purpose**: compatibility shims for older clients/servers.
- **Guidance**: new code should use the modern `Opc.Classic.Da.Dcom` surface.

#### Modern DCOM Surface

- **Status**: ✅ full DA 2.05a projection coverage, plus DA 3.0 extensions used for shared implementation.
- **Cross-platform path**: source-generated proxies and server dispatchers cover the DA 2.05a interfaces in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:30-797`.
- **Windows CCW path**: full vtables with real bodies for server/group lifecycle, item management, sync/async I/O, callbacks, connection points, and item-attribute enumeration.
- **Interface coverage**: `IOPCServer`, `IOPCCommon`, `IOPCGroupStateMgt`, `IOPCItemMgt`, `IOPCSyncIO`, `IOPCAsyncIO2`, `IOPCDataCallback`, `IConnectionPoint`, `IOPCBrowseServerAddressSpace`, and `IOPCItemProperties` are all projected and exercised in the modern surface.

---

### Gap Analysis by Namespace

#### 1. V20 Back-Compat Layer (`Opc.Classic.Da.V20.Dcom`)

The V20 namespace remains deliberately narrow. Missing V20 declarations are not treated as modern DA 2.05a gaps because the current supported surface is `Opc.Classic.Da.Dcom`.

**Recommendation**: keep V20 documentation clear: use it only for legacy compatibility and use modern DCOM for full DA 2.05a/3.0 coverage.

---

#### 2. Modern DCOM Surface (`Opc.Classic.Da.Dcom`)

| Spec interface | Cross-platform status | Windows CCW status | Source |
| --- | --- | --- | --- |
| `IOPCServer` | ✅ 6/6 methods declared; `AddGroup`, `GetGroupByName`, `CreateGroupEnumerator` return interface refs | ✅ full vtable, real `AddGroup`, `GetErrorString`, `GetStatus`, `GetGroupByName`, `RemoveGroup`, etc. | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:30-84`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaServerCcw.cs:1-556` |
| `IOPCCommon` | ✅ 5/5 generated proxy + dispatcher | routed through hosting where implemented | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `IOPCGroupStateMgt` | ✅ 4/4 | ✅ full group CCW; `CloneGroup` copies items | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:414-452`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcw.cs:1-512` |
| `IOPCItemMgt` | ✅ 7/7 including `AddItems`, `ValidateItems`, `CreateEnumerator` | ✅ full vtable; `OPCITEMDEF[]`/`OPCITEMRESULT[]` marshaling and item enumerator CCW | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:354-412`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcw.cs:1-512` |
| `IOPCSyncIO` | ✅ 2/2 | ✅ `Read`/`Write` real bodies with VARIANT marshaling | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:473-494` |
| `IOPCAsyncIO2` | ✅ 6/6 | ✅ `Read`, `Write`, `Refresh2`, `Cancel2`, `SetEnable`, `GetEnable` real bodies | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:539-590` |
| `IOPCBrowseServerAddressSpace` | ✅ 5/5, backed by address-space abstractions | usable through default browse services | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:159-199`; `src/Opc.Classic.Da/Hosting/DefaultBrowseServerAddressSpace.cs:1-167` |
| `IOPCItemProperties` | ✅ 3/3 | default properties include canonical IDs 1-8 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:201-242`; `src/Opc.Classic.Da/Hosting/DefaultItemProperties.cs:1-106` |
| `IConnectionPointContainer` / `IConnectionPoint` | ✅ dispatcher coverage; `Advise`/`Unadvise` wired | ✅ group CCW participates in callback fan-out and exposes `IEnumConnections` / `IEnumConnectionPoints` CCWs | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:681-727`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumConnectionsCcw.cs`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumConnectionPointsCcw.cs` |
| `IOPCDataCallback` | ✅ 4/4 outbound callback projection | ✅ `OpcDataCallbackProxy` marshals `OnDataChange`, `OnReadComplete`, `OnWriteComplete`, `OnCancelComplete` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:742-797`; `src/Opc.Classic.Da/Hosting/Windows/OpcDataCallbackProxy.cs:1-395` |
| `IEnumOPCItemAttributes` | ✅ dispatcher + stateful enumerator | ✅ `Next`, `Skip`, `Reset`, `Clone`, including `vEUInfo` VARIANT marshaling | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:651-679`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumOpcItemAttributesCcw.cs:1-153` |

`IOpcDaServer` also includes `ResolveGroupAsync` and `ResolveGroupByNameAsync` helper defaults for server/group lookup (`src/Opc.Classic.Da/Hosting/IOpcDaServer.cs:1-131`).

---

### Structure Coverage

DA 2.05a structures and VARIANT-heavy payloads are implemented through the shared DA/NDR code and Windows CCW paths:

- `OPCITEMSTATE` / `OpcItemState`
- `OPCITEMDEF` / `OpcItemDef`
- `OPCITEMRESULT` / `OpcItemResult`
- `OPCITEMATTRIBUTES` / `OpcItemAttributes`
- `OPCSERVERSTATUS` / `OpcServerStatus`
- `OPC_QUALITY` / `OpcQuality`

Current Windows CCW item and I/O tests exercise the marshaling paths.

---

### Test Coverage

| Test File | Scope |
| --- | --- |
| `tests/Opc.Classic.Da.Tests/V20/IOPCV20InterfaceIdTests.cs:1-44` | V20 IID compatibility |
| `tests/Opc.Classic.Da.Tests/Dcom/IOPCServerProxyTests.cs:1-103` | `IOPCServer` proxy calls |
| `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs:1-244` | Server dispatcher routing |
| `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs:1-200` | Browse/property defaults |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaServerCcwTests.cs:1-560` | Windows server CCW |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaGroupCcwTests.cs:1-2411` | Windows group CCW item/I/O/state/callback behavior, including connection enumerator CCWs |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDataCallbackProxyTests.cs:1-781` | Outbound `IOPCDataCallback` VARIANT marshaling |
| `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcEnumOpcItemAttributesCcwTests.cs:1-247` | Item attribute enumerator CCW |
| `tests/Opc.Classic.Integration.Tests/Da/DaFullLifecycleTests.cs:1-1120` | End-to-end group lifecycle, sync read/write, async callback, and remove-group loopback |
| `tests/Opc.Classic.Integration.Tests/Da/DaBrowseContinuationPointTests.cs:1-363` | DA browse continuation-point integration coverage |
| `tests/Opc.Classic.Integration.Tests/Da/DaEnumOpcItemAttributesVeuInfoTests.cs:1-404` | `vEUInfo` and object-IPID item-attribute enumerator integration coverage |

---

### Recommendations

1. Keep V20 docs explicit about its intentionally minimal scope.
2. Continue adding native interop coverage for Windows CCW edge cases.
3. ✅ End-to-end DA 2.x client/server scenarios now combine group creation, item addition, sync reads, async callbacks, and group removal in `tests/Opc.Classic.Integration.Tests/Da/DaFullLifecycleTests.cs:1-1120`.
4. Add public convenience wrappers only where adoption feedback shows raw DCOM projections are too low-level.

---

### Conclusion

The modern DA DCOM surface provides full DA 2.05a coverage of `IOPCCommon`, `IOPCShutdown`, group/item management, synchronous reads, connection points, and item enumerators. Remaining caveats are framed around V20 compatibility scope and integration hardening.

## OPC DA 3.00

**Spec**: OPC Data Access Custom Interface Specification Version 3.0 (March 4, 2003)
**Implementation**: `src/Opc.Classic.Da/`
**Review target**: `1.0.0-rc.10`

---

### Summary

**Interfaces**: DA 3.0 server, group, item, browse, I/O, callback, and connection-point surfaces are projected.
**Overall compliance**: ✅ **Modern DCOM projection complete with substantial Windows CCW support**.

The DA 3.0 implementation projects `AddGroup`, `AddItems`, `SetState`, `ReadMaxAge`, keep-alive, `CreateEnumerator`, and callback/connection point methods.

---

### Implementation Status by Area

#### OPCServer interfaces

| Interface | Status | Source |
| --- | --- | --- |
| `IOPCServer` | ✅ all 6 methods declared with interface-ref returns where required; Windows CCW has real method bodies | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:30-84`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaServerCcw.cs:1-556` |
| `IOPCCommon` | ✅ all 5 methods generated | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:86-120` |
| `IOPCBrowse` | ✅ DA 3.0 unified browse (`GetProperties`, `Browse`) | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:123-157`; `src/Opc.Classic.Da/Hosting/DefaultBrowse.cs:1-123` |
| `IOPCBrowseServerAddressSpace` | ✅ DA 2.x browse compatibility with `IEnumString` interface refs | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:159-199`; `src/Opc.Classic.Da/Hosting/DefaultBrowseServerAddressSpace.cs:1-167` |
| `IOPCItemProperties` | ✅ property query/read/lookup; default canonical property IDs 1-8 | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:201-242`; `src/Opc.Classic.Da/Hosting/DefaultItemProperties.cs:1-106` |
| `IOPCItemIO` | ✅ stateless DA 3.0 read/write VQT | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:327-352` |

#### OPCGroup interfaces

| Interface | Status | Source |
| --- | --- | --- |
| `IOPCItemMgt` | ✅ `AddItems`, `ValidateItems`, `RemoveItems`, active/client/datatype setters, `CreateEnumerator`; Windows CCW includes OPCITEMDEF/OPCITEMRESULT marshaling | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:354-412`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcw.cs:1-512` |
| `IOPCGroupStateMgt` | ✅ `GetState`, `SetState`, `SetName`, `CloneGroup`; Windows CCW clone copies items | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:414-452`; `src/Opc.Classic.Da/Hosting/OpcDaGroup.cs:1-1225` |
| `IOPCGroupStateMgt2` | ✅ `SetKeepAlive`, `GetKeepAlive` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:454-471` |
| `IOPCSyncIO` | ✅ DA 2.x sync `Read`/`Write`; Windows CCW has real VARIANT bodies | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:473-494` |
| `IOPCSyncIO2` | ✅ `Read`, `Write`, `ReadMaxAge`, `WriteVQT` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:496-537` |
| `IOPCAsyncIO2` | ✅ `Read`, `Write`, `Refresh2`, `Cancel2`, `SetEnable`, `GetEnable` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:539-590` |
| `IOPCAsyncIO3` | ✅ DA 3.0 async max-age/VQT extensions; Windows CCW `WriteVQT` marshals OPCITEMVQT and fires `OnWriteComplete` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:592-649`; `src/Opc.Classic.Da/Hosting/Windows/OpcDaGroupCcwAsyncIoMethods.cs:254-295` |
| `IOPCItemDeadbandMgt` | ✅ 3/3; projection and server-policy hooks present | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:244-272`; `src/Opc.Classic.Da/Hosting/DefaultItemDeadbandMgt.cs:1-54` |
| `IOPCItemSamplingMgt` | ✅ 5/5; projection and server-policy hooks present | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:274-325`; `src/Opc.Classic.Da/Hosting/DefaultItemSamplingMgt.cs:1-83` |
| `IEnumOPCItemAttributes` | ✅ dispatcher and Windows CCW `Next`/`Skip`/`Reset`/`Clone` | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:651-679`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumOpcItemAttributesCcw.cs:1-153` |
| `IConnectionPointContainer` / `IConnectionPoint` | ✅ callback connection routing; `Advise`/`Unadvise`, `IEnumConnections`, and `IEnumConnectionPoints` CCWs wired | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:681-727`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumConnectionsCcw.cs`; `src/Opc.Classic.Da/Hosting/Windows/OpcEnumConnectionPointsCcw.cs` |
| `IOPCDataCallback` | ✅ callback proxy/dispatcher; Windows CCW outbound proxy handles VARIANT arrays | `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:742-797`; `src/Opc.Classic.Da/Hosting/Windows/OpcDataCallbackProxy.cs:1-395` |

---

### Current Gaps and Deferred Work

#### MEDIUM

1. **Policy-specific implementations for deadband/sampling**
   The DA 3.0 deadband/sampling interfaces and dispatch paths are present. Custom server policies and sample behavior beyond the default helper remain optional implementation work.

2. **Windows CCW/native interop breadth**
   DA has broad CCW coverage, but continued interop testing is still needed for uncommon edge cases such as public groups, alternate access paths, and complex `vEUInfo` values.

3. **Higher-level convenience APIs**
   Some low-level DCOM methods are projected directly rather than wrapped by higher-level abstractions. This is intentional to preserve spec fidelity.

---

### Coverage Gaps (Integration Tests Recommended)

Current tests cover the major areas:

- `tests/Opc.Classic.Da.Tests/Dcom/IOPCServerProxyTests.cs:1-103`
- `tests/Opc.Classic.Da.Tests/Hosting/OpcDaServerDispatcherTests.cs:1-244`
- `tests/Opc.Classic.Da.Tests/BrowseAndPropertyTests.cs:1-200`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaServerCcwTests.cs:1-560`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDaGroupCcwTests.cs:1-2411` (including `IEnumConnections` / `IEnumConnectionPoints` CCW coverage)
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcDataCallbackProxyTests.cs:1-781`
- `tests/Opc.Classic.Da.Tests/Hosting/Windows/OpcEnumOpcItemAttributesCcwTests.cs:1-247`
- `tests/Opc.Classic.Integration.Tests/Da/DaFullLifecycleTests.cs:1-1120`
- `tests/Opc.Classic.Integration.Tests/Da/DaBrowseContinuationPointTests.cs:1-363`
- `tests/Opc.Classic.Integration.Tests/Da/DaEnumOpcItemAttributesVeuInfoTests.cs:1-404`

Recommended next tests:

1. ✅ Full DA group lifecycle: `AddGroup` → `AddItems` → sync read/write → async callback → `RemoveGroup` (`tests/Opc.Classic.Integration.Tests/Da/DaFullLifecycleTests.cs:1-1120`).
2. ✅ DA 3.0 `IOPCBrowse` continuation-point scenarios against hierarchical and flat namespaces (`tests/Opc.Classic.Integration.Tests/Da/DaBrowseContinuationPointTests.cs:1-363`).
3. Item deadband/sampling custom policy implementations beyond the default helper behavior.
4. ✅ Managed DCOM loopback tests for `vEUInfo` engineering-unit arrays and uncommon VARIANT types (`tests/Opc.Classic.Integration.Tests/Da/DaEnumOpcItemAttributesVeuInfoTests.cs:1-404`).

---

### Compliance Checklist (DA 3.0 vs Implementation)

| Interface | DA 3.0 Requirement | Current Status |
| --- | --- | --- |
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

### Conclusion

The DA 3.0 coverage document should be read as a current implementation-status document, not as the older gap list. The flagship DA surface now has full DCOM declarations and broad Windows CCW support for practical DA server/client workflows. Remaining work is targeted conformance hardening and policy/sample coverage.

## OPC DX 1.00

**Specification**: OPC Data eXchange Specification Version 1.0 (March 5, 2003)
**Implementation**: `Opc.Classic.Dx` managed assembly
**Review target**: `1.0.0-rc.10`

---

### Executive Summary

`Opc.Classic.Dx` provides a complete configuration-client projection for `IOPCConfiguration` backed by DX structure codecs, status records, spec-aligned enums, namespace helpers, and DX error constants.

The implementation is still intentionally **configuration-focused**. It does not implement the DX server runtime/DA bridge that performs live data transfer between source and target servers.

#### Coverage Summary

| Category | Specified | Implemented | Coverage | Notes |
| --- | --- :| --- :| --- :| --- |
| `IOPCConfiguration` methods | 12 | 12 | 100% | Hand-written client proxy covers source-server and connection operations |
| DX structure codecs | 16 registry entries | 16 | 100% | `NdrOpcDxCodecRegistry` lists registered codecs |
| Status records | 4 | 4 | 100% | Server, connection, source-server, and quality records |
| Enumerations | spec-aligned enums | present | high | Server type/state, connection state, connect status, quality/limit, masks |
| Error codes | DX HRESULT constants | present | high | `OpcDxError` constants |
| Runtime model | full DX server behavior | not implemented | 0% | DA bridge, persistence, and transfer state machine remain future work |

---

### 1. Interface Coverage

#### 1.1 IOPCConfiguration (IID `C130D281-F4AA-4779-8846-C2C4CB444F2A`)

| Method | Opnum | Status | Source |
| --- | --- :| --- | --- |
| `QuerySourceServers` | 3 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:22-24` |
| `AddSourceServers` | 4 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:26-28` |
| `ModifySourceServers` | 5 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:30-32` |
| `DeleteSourceServers` | 6 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:34-36` |
| `CopyDefaultServerAttributes` | 7 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:38-40` |
| `QueryDXConnections` | 8 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:42-47` |
| `AddDXConnections` | 9 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:49-51` |
| `UpdateDXConnections` | 10 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:53-55` |
| `ModifyDXConnections` | 11 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:57-59` |
| `DeleteDXConnections` | 12 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:61-63` |
| `CopyDefaultDXConnectionAttributes` | 13 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:65-67` |
| `ResetConfiguration` | 14 | ✅ | `src/Opc.Classic.Dx/Dcom/IOPCDxInterfaces.cs:69-71` |

The hand-written `IOPCConfigurationClientProxy` implements payload encode/decode for these methods (`src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs:18-234`).

---

### 2. Data Structures and Codecs

| Structure / codec area | Status | Source |
| --- | --- | --- |
| `DxConnection`, `DxSourceServer`, `DxGeneralResponse`, `DxItemIdentifier` | ✅ records | `src/Opc.Classic.Dx/*.cs` |
| DX status records (`DxServerStatus`, `DxConnectionStatus`, `DxSourceServerStatus`, `DxQuality`) | ✅ | `src/Opc.Classic.Dx/DxStatusRecords.cs:12-70` |
| Codec registry | ✅ 16 entries | `src/Opc.Classic.Dx/Ndr/NdrOpcDxCodecs.cs:15-38` |
| Connection/source/general-response codecs | ✅ | `src/Opc.Classic.Dx/Ndr/NdrOpcDxCodecs.cs:40-647` |

---

### 3. Enumeration and Error Coverage

| Feature | Status | Source |
| --- | --- | --- |
| DX server type/state, connection state, connect status, quality/limit, masks | ✅ | `src/Opc.Classic.Dx/DxEnums.cs:14-177` |
| Legacy/custom connection and override state helpers | ✅ | `src/Opc.Classic.Dx/ConnectionState.cs`, `src/Opc.Classic.Dx/OverrideState.cs` |
| DX HRESULT constants | ✅ | `src/Opc.Classic.Dx/OpcDxErrors.cs:13-14` and following constants |
| DX namespace constants/helpers | ✅ | `src/Opc.Classic.Dx/DxNamespace.cs` |

---

### 4. Managed `IDxServer` Interface

`IDxServer` remains an async-first managed abstraction over DX configuration concepts (`src/Opc.Classic.Dx/IDxServer.cs:13-63`). It intentionally simplifies add/modify into add-or-update operations and exposes connection/source-server configuration rather than implementing the DX runtime transfer loop.

---

### 5. DX Database / Runtime Model Coverage

**Status**: ❌ **not implemented as a generic server runtime**

The following spec behavior still requires a DX server implementation:

- Browseable `DX/` root, `ServerStatus`, `DXConnectionsRoot`, and `SourceServers` DA branches
- Source-server connection/reconnect/disconnect lifecycle
- Subscription and queueing from source DA servers
- Data conversion and target update truth-table execution
- Override/substitute value behavior
- Persistence and dirty-flag management

This is a runtime/server product feature, not a client proxy/codec gap.

---

### 6. Test Coverage

| Test File | Purpose |
| --- | --- |
| `tests/Opc.Classic.Dx.Tests/DcomInterfaceIdTests.cs:1-29` | IIDs |
| `tests/Opc.Classic.Dx.Tests/DxTypesTests.cs:1-88` | Record/enumeration construction and helpers |
| `tests/Opc.Classic.Dx.Tests/Dcom/IOPCDxProxyTests.cs:1-160` | `IOPCConfiguration` proxy and codec behavior |

Recommended additions: integration tests against a DX server or managed test shim, and state-machine tests if a server runtime is added.

---

### 7. Known Gaps and Deferred Work

#### 7.1 DX Runtime Server Implementation

Implement §6 (OPC DX Runtime Model) as a future server/runtime feature: DA branch exposure, source access, transfer loop, target writes, connection state machine, and persistence.

#### 7.2 XML-DA Mapping

Appendix A XML-DA mapping remains lower priority than DCOM configuration support.

---

### 8. Conclusion

DX is not codec-blocked. The library is ready for DX configuration-client scenarios that need to query, add, modify, delete, update, copy defaults, and reset source servers and DX connections. It is not a DX server runtime.

## OPC HDA 1.20

**Spec**: OPC Historical Data Access Specification Version 1.20 (January 30, 2004)
**Implementation**: `src/Opc.Classic.Hda/`
**Review target**: `1.0.0-rc.10`

---

### Summary

**Interfaces**: 10 projected interfaces (9 HDA service interfaces plus `IOPCHDA_DataCallback`)
**Methods**: 56/56 declared
**Structs**: 5/5 codecs registered
**Aggregates**: 27 standard aggregate IDs declared
**Quality flags**: HDA-specific flags defined
**Error constants**: HDA Appendix C constants present

**Overall compliance**: **Full DCOM declaration/proxy/dispatcher coverage; Windows CCW now covers browser creation, HDA sync/async reads, sync/async update, playback, annotation insert, and async advise**.

Earlier claims that HDA was fully production-ready through all paths are too broad. The DCOM projection is complete, and the Windows CCW now has native `OPCHDA_ITEM[]`/`OPCHDA_ATTRIBUTE[]`/`OPCHDA_MODIFIEDITEM[]`/`OPCHDA_ANNOTATION[]` marshaling for history reads, annotation inserts, raw/processed advise updates, update operations, and playback callbacks. Remaining concerns are connection-point enumeration stubs plus server-policy semantics such as aggregate calculations, relative time parsing, and persistence.

---

### Implementation Status

#### Interface Coverage

All HDA service and callback interfaces are declared with opnums matching the spec in `src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs:21-309`.

| Interface | Methods | Cross-platform DCOM status | Windows CCW status |
| --- | --- :| --- | --- |
| `IOPCHDA_Server` | 6/6 | ✅ generated proxy + dispatcher | ✅ real `GetItemAttributes`, `GetAggregates`, `GetHistorianStatus`, `ValidateItemIDs`, `GetItemHandles`, `ReleaseItemHandles`, and `CreateBrowse` |
| `IOPCHDA_Browser` | 4/4 | ✅ generated proxy declaration | ✅ `CreateBrowse` returns a raw-vtable browser CCW with `GetEnum`, `ChangeBrowsePosition`, `GetItemID`, and `GetBranchPosition` |
| `IOPCHDA_SyncRead` | 5/5 | ✅ generated proxy + dispatcher | ✅ `ReadRaw`, `ReadProcessed`, `ReadAtTime`, `ReadModified`, and `ReadAttribute` marshal native HDA arrays |
| `IOPCHDA_SyncUpdate` | 6/6 | ✅ generated proxy + dispatcher | ✅ raw-vtable CCW for capabilities, insert/replace/insert-replace, and delete raw/at-time |
| `IOPCHDA_SyncAnnotations` | 3/3 | ✅ generated proxy + dispatcher | ✅ `Read` marshals native annotation arrays; `Insert` marshals annotation input and returns per-item HRESULTs; query capabilities wired when implemented by the server |
| `IOPCHDA_AsyncRead` | 8/8 | ✅ generated proxy + dispatcher | ✅ read and advise methods return cancel IDs/errors and fire `IOPCHDA_DataCallback` |
| `IOPCHDA_AsyncUpdate` | 7/7 | ✅ generated proxy + dispatcher | ✅ raw-vtable CCW returns cancel IDs/errors and fires `OnUpdateComplete` |
| `IOPCHDA_AsyncAnnotations` | 4/4 | ✅ generated proxy + dispatcher | ✅ `Read`/`Insert` return cancel IDs/errors and fire `OnReadAnnotations`/`OnInsertAnnotations` |
| `IOPCHDA_Playback` | 3/3 | ✅ generated proxy + dispatcher | ✅ raw-vtable CCW streams `OnPlayback` updates and supports cancel |
| `IOPCHDA_DataCallback` | 9/9 | ✅ callback projection | ✅ `Advise`/`Unadvise`/`FindConnectionPoint`; ⚠️ `EnumConnections`/`EnumConnectionPoints` stubs |

**Key CCW sources**:

- Real `IOPCHDA_Server` methods and `CreateBrowse`: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaServerCcwMethods.cs`
- Sync read and annotation-read CCW bodies: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaServerCcwMethods.cs`
- Sync/async update and playback CCW bodies: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaSyncUpdateCcw.cs`, `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaAsyncUpdateCcw.cs`, and `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaPlaybackCcw.cs`
- Async read/update/playback callback bridge: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaCallbackProxy.cs`
- Native HDA array/VARIANT marshaling: `src/Opc.Classic.Hda/Hosting/Windows/OpcHdaItemMarshaler.cs`

---

### Data Structure Coverage

| Structure | Status |
| --- | --- |
| `OPCHDA_TIME` | ✅ codec |
| `OPCHDA_ITEM` | ✅ codec |
| `OPCHDA_MODIFIEDITEM` | ✅ codec |
| `OPCHDA_ANNOTATION` | ✅ codec |
| `OPCHDA_ATTRIBUTE` | ✅ codec |

These codecs support the generated DCOM path. The Windows CCW now also has native memory allocation and OAUT `VARIANT` array marshaling for read results.

---

### Aggregate, Quality, and Error Coverage

- Standard aggregate IDs are represented by `OpcHdaAggregateId` / aggregate support types.
- HDA quality flags are represented in `OpcHdaQuality`.
- HDA Appendix C error constants are present in `src/Opc.Classic.Hda/OpcHdaErrors.cs`.

HDA error constants are present in the implementation.

---

### Gaps in Implementation

#### HIGH

##### 1. Server-side aggregate/update/annotation semantics

The interfaces and codecs are present, but aggregate calculations, update policies, annotation storage, relative time parsing, and playback behavior are server-specific. A sample/reference HDA server would make those semantics testable.

#### MEDIUM

##### 2. Extended conformance scenarios

Run targeted interop coverage against the Windows CCWs, especially callback ordering, cancellation races, and server-specific update policies.

---

### Test Coverage

| Test File | Scope |
| --- | --- |
| `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaServerCcwMethodsTests.cs:1-524` | Windows CCW server metadata/status/handle methods and validation paths |
| `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaBrowserCcwTests.cs:1-457` | Native browser CCW cursor and item-ID behavior |
| `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaServerCcwReadTests.cs:1-719` | Sync/async read and annotation-read native HDA array marshaling |
| `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaServerCcwAnnotationAdviseTests.cs:1-730` | Windows CCW annotation insert and async raw/processed advise callbacks |
| `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaSyncUpdateCcwTests.cs:1-350`, `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaAsyncUpdateCcwTests.cs:1-323`, `tests/Opc.Classic.Hda.Tests/Hosting/Windows/OpcHdaPlaybackCcwTests.cs:1-242` | Native CCW update/playback methods, callbacks, and cancellation |
| `tests/Opc.Classic.Hda.Tests/Dcom/HdaMissingMethodProxyRoundTripTests.cs:1-455` | DCOM proxy round trips for HDA methods |
| `tests/Opc.Classic.Hda.Tests/Hosting/OpcHdaServerDispatcherTests.cs:1-110` | Server dispatcher routing |

Recommended additions:

1. Aggregate calculation, annotation persistence, update, playback, and relative time parser tests for any sample/reference HDA server.
2. Targeted callback ordering and cancellation stress tests.

---

### Compliance Checklist (§3 Compliance)

| Requirement | Current status | Notes |
| --- | --- | --- |
| `IOPCHDA_Server` | ✅ DCOM, ✅ CCW real bodies | Includes browser creation |
| `IOPCHDA_SyncRead` | ✅ DCOM, ✅ CCW real bodies | Native HDA item/attribute/modified marshaling covered |
| Async callback pattern | ✅ DCOM projection, ✅ CCW read/advise/annotation/update/playback callbacks | Callback ordering and stress coverage remain follow-up interop tasks |
| `IOPCHDA_Browser` or DA browse | ✅ DCOM declaration, ✅ native browser CCW | Browser cursor methods covered |
| Optional update/annotation/playback interfaces | ✅ DCOM declarations, ✅ update/annotation/playback CCWs | Server-specific semantics pending |

---

### Conclusion

HDA should be described as declaration- and codec-complete for the managed DCOM path, with Windows CCWs covering read, update, annotation, advise, and playback surfaces. Remaining work is server-specific behavior and interop stress coverage, not missing HDA interface declarations.

## OPC Security 1.00

**Spec**: OPC Security Custom Interface Version 1.0 (October 17, 2000)
**Implementation**: `Opc.Classic.Security` namespace + `OpcSecurityErrors` in `Opc.Classic.Core`
**Analysis date**: 2026-01-XX

---

### Executive Summary

**Coverage**: **100%** of required interfaces and methods
**Test coverage**: 6 test classes, 17 unit tests
**Gaps**: 0 blocking, 0 minor (server-side reference sample shipped ✅)

OPC Security 1.00 defines two **optional** interfaces for managing client identity changes within a single OPC server connection:

1. **`IOPCSecurityNT`** (Windows-integrated authentication) – 3 methods
2. **`IOPCSecurityPrivate`** (server-private credentials) – 3 methods

Both interfaces are **fully declared** in `IOPCSecurityInterfaces.cs` with correct IIDs, opnums, and async signatures. The managed abstraction `IOpcSecurity` provides a unified API surface. All 6 wire methods are implemented by source-generated client proxies.

---

### Specification Overview

#### Scope (OPC Security 1.00)

| Area | Spec Coverage |
| --- | --- |
| **IOPCSecurityNT** | 3 methods: `IsAvailableNT`, `QueryMinImpersonationLevel`, `ChangeUser` |
| **IOPCSecurityPrivate** | 3 methods: `IsAvailablePriv`, `Logon`, `Logoff` |
| **Error codes** | 3 HRESULTs: `OPC_E_PRIVATE_ACTIVE`, `OPC_E_LOW_IMPERS_LEVEL`, `OPC_S_LOW_AUTHN_LEVEL` |
| **DCOM guidelines** | Authentication levels, impersonation levels, proxy blanket setup, Win95/98/2000 notes |
| **Security model** | Principals, Access Certificates, ACLs, Reference Monitor, Channels (conceptual framework) |

**Out of scope** (per spec 1.5.1):
- Which server objects are secured (vendor-specific)
- Server-side ACL implementation (vendor-specific)
- OLE Automation interface (separate spec)

---

### Implementation Coverage

#### 1. Core Interface Declarations

**File**: `src/Opc.Classic.Security/Dcom/IOPCSecurityInterfaces.cs`

```csharp
[OpcInterface("7AA83A01-6C77-11D3-84F9-00008630A38B")]
public partial interface IOPCSecurityNT
{
    [OpcMethod(3)] Task<bool> IsAvailableNTAsync(...);
    [OpcMethod(4)] Task<int> QueryMinImpersonationLevelAsync(...);
    [OpcMethod(5)] Task ChangeUserAsync(...);
}

[OpcInterface("7AA83A02-6C77-11D3-84F9-00008630A38B")]
public partial interface IOPCSecurityPrivate
{
    [OpcMethod(3)] Task<bool> IsAvailablePrivAsync(...);
    [OpcMethod(4)] Task LogonAsync(string userId, string password, ...);
    [OpcMethod(5)] Task LogoffAsync(...);
}
```

✅ **Status**: **Complete**
- IIDs match spec IDL exactly
- Opnums match spec (3, 4, 5 for both interfaces)
- Async-first API (returns `Task`/`Task<T>`)
- `[GenerateOpcProxy]` and `[OpcGenerateServerDispatch]` attributes present

---

#### 2. Managed API Abstraction

**File**: `src/Opc.Classic.Security/IOpcSecurity.cs`

Unified interface for client-side usage:

```csharp
public interface IOpcSecurity
{
    bool SupportsWindowsAuthentication { get; }  // maps to IOPCSecurityNT presence
    bool SupportsPrivateAuthentication { get; }  // maps to IOPCSecurityPrivate presence

    Task<bool> LoginAsCurrentUserAsync(...);     // wraps ChangeUser
    Task<bool> LoginPrivateAsync(...);           // wraps Logon
    Task LogoutAsync(...);                       // wraps Logoff

    bool IsAuthenticated { get; }
    string CurrentIdentity { get; }
}
```

✅ **Status**: **Complete**
- Higher-level abstraction over raw DCOM interfaces
- Maps both authentication models to unified API
- Client code doesn't need to query for two interfaces separately

---

#### 3. Supporting Types

| Type | File | Spec Reference | Status |
| --- | --- | --- | --- |
| `OpcImpersonationLevel` | `OpcImpersonationLevel.cs` | Section 4.3.2 (QueryMinImpersonationLevel return values) | ✅ Complete |
| `OpcLogonRequest` | `OpcLogonRequest.cs` | Section 4.4.2 (Logon parameters) | ✅ Complete |
| `OpcSecurityErrors` | `src/Opc.Classic.Core/Errors/OpcSecurityErrors.cs` | Section 6.2 (`OpcErrSec.h` HRESULTs) | ✅ Complete |

**`OpcImpersonationLevel`** enum:
```csharp
public enum OpcImpersonationLevel
{
    Default = 0,      // RPC_C_IMP_LEVEL_DEFAULT
    Anonymous = 1,    // RPC_C_IMP_LEVEL_ANONYMOUS
    Identify = 2,     // RPC_C_IMP_LEVEL_IDENTIFY
    Impersonate = 3,  // RPC_C_IMP_LEVEL_IMPERSONATE
    Delegate = 4,     // RPC_C_IMP_LEVEL_DELEGATE
}
```

✅ Maps to DCOM impersonation levels (spec section 6.3.4.5)

**`OpcLogonRequest`** record:
```csharp
public sealed record OpcLogonRequest(string UserId, string Password);
```

✅ Encapsulates `IOPCSecurityPrivate::Logon` parameters (spec section 4.4.2)

---

#### 4. Client-Side Proxies (Source-Generated)

Generated by `Opc.Classic.Generators` from `[OpcInterface]` + `[OpcMethod]` attributes:

- **`IOPCSecurityNTClientProxy`** – marshals 3 methods to ORPC calls
- **`IOPCSecurityPrivateClientProxy`** – marshals 3 methods to ORPC calls

✅ **Status**: **Complete** (validated by `tests/Opc.Classic.Security.Tests/Dcom/IOPCSecurityProxyTests.cs:1-72`)

Example test:
```csharp
[Test]
public async Task SecurityNT_IsAvailable_decodes_boolean()
{
    var proxy = new IOPCSecurityNTClientProxy(channel);
    bool available = await proxy.IsAvailableNTAsync(...);
    // Verifies: IID, opnum 3, response deserialization
}
```

---

#### 5. Server-Side Dispatchers (Source-Generated)

`[OpcGenerateServerDispatch]` attribute is present on both interfaces. This triggers generation of:

- **Dispatch tables** for opnum → method routing
- **Unmarshal/marshal logic** for request/response payloads

✅ **Gap closed**: `samples/Opc.Classic.Samples.OpcSecurityServer/Program.cs:1-269` now publishes `IOPCSecurityNT` and `IOPCSecurityPrivate` with a documented stub implementation. This remains **optional** — OPC Security is rarely implemented by servers, and most deployments rely on DCOM-layer authentication instead.

**Impact**: None for client usage; server implementers now have a reference for wiring `IOPCSecurityNT` / `IOPCSecurityPrivate` to actual ACL logic.

---

#### 6. Test Coverage

| Test Class | Focus | Coverage |
| --- | --- | --- |
| `tests/Opc.Classic.Security.Tests/OpcSecurityTests.cs:1-101` (`IOpcSecurityContractTests`) | `IOpcSecurity` behavior (login/logout state machine) | ✅ 6 tests |
| `tests/Opc.Classic.Security.Tests/Dcom/IOPCSecurityProxyTests.cs:1-72` | Client proxy marshaling (opnum, IID, payload encoding) | ✅ 2 tests |
| `tests/Opc.Classic.Security.Tests/OpcImpersonationLevelTests.cs:1-27` | Enum values | ✅ 1 test covering all 5 values |
| `tests/Opc.Classic.Security.Tests/OpcLogonRequestTests.cs:1-58` | Record validation | ✅ 4 tests |
| `tests/Opc.Classic.Security.Tests/DcomInterfaceIdTests.cs:1-57` | IID correctness | ✅ 3 tests |
| `tests/Opc.Classic.Security.Tests/OpcSecurityErrorsTests.cs:1-29` | HRESULT constants | ✅ 1 test |

✅ **Total**: 17 unit tests covering:
- Interface presence detection
- Authentication state transitions
- DCOM wire-level encoding/decoding
- Enum/type correctness
- OPC Security HRESULT constants

---

### Gap Analysis

#### ✅ **Status**: OPC Security Error Code Constants

**Spec reference**: Section 6.2 (OpcErrSec.h)

The spec defines 3 HRESULTs:
```c
#define OPC_E_PRIVATE_ACTIVE     0xC0040301L  // Private logon already active
#define OPC_E_LOW_IMPERS_LEVEL   0xC0040302L  // Server requires higher impersonation
#define OPC_S_LOW_AUTHN_LEVEL    0x00040303L  // Server expected higher packet privacy
```

**Implementation**: `src/Opc.Classic.Core/Errors/OpcSecurityErrors.cs` defines all three constants with the spec values.

**Tests**: `tests/Opc.Classic.Security.Tests/OpcSecurityErrorsTests.cs:1-29` asserts the numeric values match `OpcErrSec.h`.

✅ **No gap** — Client and server code can use named constants for these OPC Security HRESULTs.

---

#### ✅ **Gap 1 closed**: Server-Side Implementation Scaffold

**Spec reference**: Sections 4.3.3 (ChangeUser), 4.4.2 (Logon), 4.5.1 (NT Credential Approach)

The spec provides **guidelines** for server implementers:
- How to call `CoImpersonateClient()` and `CoQueryClientBlanket()`
- How to cache NT Access Tokens
- How to perform `AccessCheck()` against private ACLs

**Closed in codebase**: `samples/Opc.Classic.Samples.OpcSecurityServer/Program.cs:1-269` demonstrates OPC Security server-side wiring.

**Impact**: **None for clients**. Server authors can start from the sample and replace the stub with production ACL logic.

**Shipped sample**:
- Stub implementation of `IOPCSecurityNT`, `IOPCSecurityPrivate`, and `IOpcSecurity`
- Demo private credential (`operator` / `demo`) and current-user identity capture
- Cookbook guidance in `docs/cookbook/08-implementing-opc-security.md`

**Note**: This is **optional per spec** — most OPC servers do not implement OPC Security.

---

### Cross-Platform Considerations

**Spec assumption**: Windows NT 4.0 SP5+ with DCOM (section 1.5.3)

**Opc.Classic.Security handling**:
- **Client-side**: Works on all platforms (Linux, macOS, Windows) — the client-side proxies marshal calls over DCOM transport (which is already cross-platform in `Opc.Classic.Dcom`).
- **Server-side** (note in `IOPCSecurityInterfaces.cs`):
  ```csharp
  // Cross-platform note:
  //   IOPCSecurityNT historically depends on Windows SSPI to enumerate the
  //   caller's authenticated identity. The Opc.Classic.Dcom.Kerberos stack
  //   replaces SSPI with Kerberos.NET-based credential
  //   acquisition. The cross-platform server-side implementation of
  //   IsAvailableNT returns true iff a configured Kerberos KDC is reachable
  //   from the server.
  ```

✅ **No gap** — cross-platform path is documented, Kerberos-based auth is in `Opc.Classic.Dcom.Kerberos` (out of scope for this review per instructions).

---

### Method-by-Method Coverage

#### `IOPCSecurityNT` (IID `7AA83A01-6C77-11D3-84F9-00008630A38B`)

| Method | Opnum | Spec Section | Implementation | Tests | Notes |
| --- | --- | --- | --- | --- | --- |
| `IsAvailableNT` | 3 | 4.3.1 | ✅ `IOPCSecurityInterfaces.cs:34` | ✅ `IOPCSecurityProxyTests.cs:22` | Returns bool |
| `QueryMinImpersonationLevel` | 4 | 4.3.2 | ✅ `IOPCSecurityInterfaces.cs:38` | ✅ Indirect via enum tests | Returns DWORD (mapped to `int`) |
| `ChangeUser` | 5 | 4.3.3 | ✅ `IOPCSecurityInterfaces.cs:42` | ✅ `IOpcSecurityContractTests.cs:57` | void return (signals credential change) |

✅ **Complete** — All 3 methods declared with correct signatures.

---

#### `IOPCSecurityPrivate` (IID `7AA83A02-6C77-11D3-84F9-00008630A38B`)

| Method | Opnum | Spec Section | Implementation | Tests | Notes |
| --- | --- | --- | --- | --- | --- |
| `IsAvailablePriv` | 3 | 4.4.1 | ✅ `IOPCSecurityInterfaces.cs:53` | ✅ Indirect via contract tests | Returns bool |
| `Logon` | 4 | 4.4.2 | ✅ `IOPCSecurityInterfaces.cs:57` | ✅ `IOPCSecurityProxyTests.cs:43` | Takes `userId`, `password` (strings) |
| `Logoff` | 5 | 4.4.3 | ✅ `IOPCSecurityInterfaces.cs:61` | ✅ `IOpcSecurityContractTests.cs:85` | void return (clears private credentials) |

✅ **Complete** — All 3 methods declared with correct signatures.

---

### Spec Guidelines vs. Implementation

The spec includes extensive **guidelines** (section 6.3) that are **not part of the wire protocol**:

| Guideline | Spec Section | Relevance to Opc.Classic |
| --- | --- | --- |
| DCOM Security Setup | 6.3.1 | Handled by `Opc.Classic.Dcom` RPC stack (out of scope) |
| `CoInitializeSecurity()` recommendations | 6.3.1.1 | N/A (managed runtime, no direct COM) |
| Windows 95/98 DCOM differences | 6.3.1.2 | N/A (not supported; .NET 10 targets modern OSes) |
| In-Process Server Considerations | 6.3.2 | N/A (Opc.Classic uses out-of-proc DCOM only) |
| Server Configuration Parameters | 6.3.3 | N/A (vendor-specific, not part of wire protocol) |
| Windows 2000 Kerberos notes | 6.3.4 | Covered by `Opc.Classic.Dcom.Kerberos` |
| Impersonation Levels | 6.3.4.5 | ✅ Mapped to `OpcImpersonationLevel` enum |

✅ **No gaps** — Guidelines are informational for server implementers, not wire-protocol requirements.

---

### Conformance Summary

| Category | Required | Implemented | Gap |
| --- | --- | --- | --- |
| **IOPCSecurityNT interface** | Optional (spec 2.5) | ✅ Yes | None |
| **IOPCSecurityPrivate interface** | Optional (spec 2.5) | ✅ Yes | None |
| **Method signatures** | 6 methods total | ✅ 6/6 | None |
| **IID correctness** | 2 GUIDs | ✅ 2/2 | None |
| **Opnum correctness** | 3-5 for each interface | ✅ 6/6 | None |
| **Error codes** | 3 HRESULTs | ✅ 3/3 | None |
| **Client proxy generation** | Implicit | ✅ Yes | None |
| **Server dispatch generation** | Implicit | ✅ Yes (sample server shipped) | None |

**Overall**: **100% wire-protocol coverage**, 0 blocking gaps.

---

### Recommendations

#### 1. Reference Server Sample Shipped ✅

**Sample**: `samples/Opc.Classic.Samples.OpcSecurityServer/Program.cs:1-269`

The server demonstrates:
- How to implement `IOPCSecurityNT` + `IOPCSecurityPrivate`
- Stub ACL checks for demo-only Windows/private logon flows
- How to publish the generated security dispatchers with the managed DCOM listener

**Benefit**: Documents server-side wiring for future implementers. Not required for client usage.

---

#### 2. Document DCOM-Layer vs. OPC-Layer Security

**Related docs**: `docs/security/THREAT_MODEL.md`

Clarify distinction:
- **DCOM Security** (handled by `Opc.Classic.Dcom`, always present): Connection-level authentication (NTLM/Kerberos), packet integrity, packet privacy.
- **OPC Security** (this spec, optional): Session-level identity switching without re-activating server.

**Benefit**: Avoids confusion between DCOM auth (mandatory) and OPC Security (rare, optional).

---

### Related Work

| Area | Location | Status |
| --- | --- | --- |
| DCOM authentication (NTLM/Kerberos) | `src/Opc.Classic.Dcom/rpc/Auth/` | ✅ Implemented |
| SPNEGO negotiation | `src/Opc.Classic.Dcom.Kerberos/Spnego/` | ✅ Implemented |
| Channel Binding Token (CBT) | `src/Opc.Classic.Dcom/Crypto/` | ✅ Implemented |
| Packet integrity / privacy | `src/Opc.Classic.Dcom/rpc/Auth/` | ✅ Implemented (NTLM sign/seal, Kerberos sign/seal) |

These are **out of scope** for OPC Security 1.00 (which is a higher-layer, session-level API).

---

### Conclusion

**OPC Security 1.00 coverage: 100%** of required interfaces and methods.

The `Opc.Classic.Security` implementation is **complete** for client-side usage:
- ✅ Both interfaces declared with correct IIDs and opnums
- ✅ Source-generated client proxies tested and working
- ✅ Unified `IOpcSecurity` API for ease of use
- ✅ Supporting types (`OpcImpersonationLevel`, `OpcLogonRequest`, `OpcSecurityErrors`) present

**Reference sample**:
- `samples/Opc.Classic.Samples.OpcSecurityServer/Program.cs:1-269` demonstrates server-side wiring and documents the stub ACL caveat.

**Recommendation**: Mark as **COMPLETE**. No remaining sample-level gap affects client or server implementers.

---

**Generated**: 2026-01-XX
**Reviewer**: GitHub Copilot CLI (code-review agent)
**Next review**: When production ACL semantics or additional OPC Security conformance tests are added.

## OPC XML-DA 1.01

**Specification**: OPC XML-DA 1.01 (October 2003)
**Implementation**: `src/Opc.Classic.Xml/` (managed C# client)
**Tests**: `tests/Opc.Classic.Xml.Tests/`
**Analysis Date**: 2025-01-24
**Target Release**: 1.0.0-rc.10 (client-only, scalar + array values)


---

### Executive Summary

The `Opc.Classic.Xml` library provides a **complete client implementation** for all 8 OPC XML-DA 1.01 operations with scalar, extended scalar, XML-DA array, base64Binary, typed error-code, and quality-bit support. It is an AOT- and trim-compatible SOAP 1.1/HTTP client surface built around caller-owned `HttpClient` instances; XML-DA server hosting remains out of scope for the current package.

#### Coverage overview

- **Operations**: 8/8 implemented (100%)
- **Scalar Data Types**: Base + extended XML Schema scalars implemented
- **Array Data Types**: 10/10 requested XML-DA array/binary types implemented (100%)
- **Value/quality handling**: `XmlDaValue`, `OpcQuality`, and `XmlDaQualityCompat` cover scalar, array, base64Binary, raw unknown values, quality kind, substatus, limit, and vendor-extension bits
- **Error Codes**: SOAP faults and per-item `ResultID` values are mapped through `XmlDaErrorCode` / `XmlDaErrorCodes`
- **Server Hosting**: Not implemented (pending)
- **SOAP Transport**: 1.1 only (1.2 not implemented)

#### Major gaps

1. **Server hosting** — No ASP.NET/ASP.NET Core server implementation (pending)
2. **SOAP 1.2 bindings** — Only SOAP 1.1 supported
3. **Third-party interop runs** — No integration tests against representative XML-DA servers yet

---

### Supported transport model

- SOAP 1.1 envelopes over HTTP with `text/xml; charset=utf-8` content.
- Per-operation `SOAPAction` headers from the XML-DA namespace.
- Caller-controlled endpoint URI, authentication, proxy, TLS, timeout, and retry policy through `HttpClient`.
- Streaming XML read/write through `System.Xml.XmlReader` and `System.Xml.XmlWriter`.
- DTD processing and external XML resolution disabled in the SOAP reader.


---

### Operations coverage

All 8 XML-DA 1.01 operations are fully implemented in `HttpXmlDaClient.cs`. The table combines the production client surface from the former status page with the spec/test evidence from the gap analysis.

| Operation | Spec Section | Client API | Implementation | Tests | Coverage |
| --- | --- | --- | --- | --- | --- |
| `GetStatus` | 3.2 | `GetStatusAsync` | `HttpXmlDaClient.GetStatusAsync` (lines 45-59) | `tests/Opc.Classic.Xml.Tests/GetStatusSerializerTests.cs:1-272` | Server state, vendor info, product/version info, supported locale IDs, supported interface versions, status info, start time, current time, and last update time. |
| `Read` | 3.3 | `ReadAsync` | `HttpXmlDaClient.ReadAsync` (lines 61-87) | `tests/Opc.Classic.Xml.Tests/ReadSerializerTests.cs:1-298` | Item names, client item handles, max age, per-item value, quality, timestamp, and result ID. |
| `Write` | 3.4 | `WriteAsync` | `HttpXmlDaClient.WriteAsync` (lines 89-113) | `tests/Opc.Classic.Xml.Tests/WriteSerializerTests.cs:1-164` | Item values, client item handles, per-item result IDs, optional error text, scalar, extended scalar, array, and base64Binary values. |
| `Subscribe` | 3.5 | `SubscribeAsync` | `HttpXmlDaClient.SubscribeAsync` (lines 115-147) | `tests/Opc.Classic.Xml.Tests/SubscribeSerializerTests.cs:1-155` | Server subscription handle, requested and revised sampling rate, ping rate, buffering flag, deadband, initial values, and per-item results. |
| `SubscriptionPolledRefresh` | 3.6 | `SubscriptionPolledRefreshAsync` | `HttpXmlDaClient.SubscriptionPolledRefreshAsync` (lines 149-176) | `tests/Opc.Classic.Xml.Tests/SubscriptionPolledRefreshSerializerTests.cs:1-151` | One or more subscription handles, hold time, wait time, changed/all item mode, data-buffer overflow flag, invalid handles, and per-subscription item lists. |
| `SubscriptionCancel` | 3.7 | `SubscriptionCancelAsync` | `HttpXmlDaClient.SubscriptionCancelAsync` (lines 178-198) | `tests/Opc.Classic.Xml.Tests/SubscriptionCancelSerializerTests.cs:1-118` | Cancellation by server subscription handle and echoed client request handle. |
| `Browse` | 3.8 | `BrowseAsync` | `HttpXmlDaClient.BrowseAsync` (lines 200-214) | `tests/Opc.Classic.Xml.Tests/BrowseSerializerTests.cs:1-167` | Root/branch browsing, branch/item/all filters, max elements, continuation point, element name filter, item path, item name, leaf flag, child flag, properties, and vendor filters. |
| `GetProperties` | 3.9 | `GetPropertiesAsync` | `HttpXmlDaClient.GetPropertiesAsync` (lines 216-226) | `tests/Opc.Classic.Xml.Tests/GetPropertiesSerializerTests.cs:1-187` | Item properties, selected or all property names, optional property values, descriptions, and per-item/per-property result IDs. |

#### Operation-specific notes

##### GetStatus
- Fully implements `Status` response with `StatusInfo`, `SupportedLocaleIDs`, `SupportedInterfaceVersions`, `VendorInfo`, `ProductVersion`, `ServerState`, `StartTime`
- Correctly handles optional fields per spec

##### Read
- Supports `ItemList` with hierarchical `ItemPath`/`ReqType` at list and item levels
- Implements `RequestOptions` (ReturnErrorText, ReturnDiagnosticInfo, ReturnItemTime, ReturnItemPath, ReturnItemName, LocaleID, ClientRequestHandle, RequestDeadline)
- Handles `RItemList` response with quality, timestamp, error codes

##### Write
- Implements all write features including hierarchical `ItemPath`/`ReqType`
- Correctly serializes `XmlDaValue` as xsi:type discriminated content
- Supports scalar, extended scalar, array, and base64Binary values (see Data Types section)

##### Subscribe
- Implements polled-subscription model per spec
- Supports `SubscriptionPingRate`, `ReturnValuesOnReply`, `SubscriptionDeadline`, `EnableBuffering`, `HoldTime`, `WaitTime`
- Correctly returns `ServerSubHandle` for subsequent polling

##### SubscriptionPolledRefresh
- Implements `HoldTime`, `WaitTime`, `ReturnAllItems` parameters
- Correctly parses `RItemList` with invalidated items

##### SubscriptionCancel
- Implements `ServerSubHandle` and `ClientRequestHandle` parameters
- Correctly handles `SOAP Fault` responses for invalid handles

##### Browse
- Implements `PropertyNames`, `BrowseFilter`, `ElementNameFilter`, `VendorFilter`, `ReturnAllProperties`, `ReturnPropertyValues`, `ReturnErrorText`, `ContinuationPoint`
- Supports hierarchical browsing with `ItemPath`/`ItemName`

##### GetProperties
- Implements `ItemID`, `ItemPath`, `PropertyNames`, `ReturnAllProperties`, `ReturnPropertyValues`, `ReturnErrorText`
- Correctly handles property IDs (standard and vendor-specific)

---

### Data types coverage

`XmlDaValue` supports the XML Schema scalar types used by common XML-DA servers: `string`, signed and unsigned integer widths, `float`, `double`, `decimal`, `boolean`, `dateTime`, `time`, `date`, `duration`, `QName`, and `base64Binary`. It also supports XML-DA array carriers for byte, short, int, long, float, double, string, boolean, and dateTime values. Unknown value types preserve raw text for diagnostics. `OpcQuality` maps the DA packed quality bits exposed by XML-DA item values.

#### 2.1 Scalar Types (Spec Section 2.7.1)

All 9 scalar types from the spec are implemented in `XmlDaValueType.cs` and `XmlDaValue.cs`.

| xsi:type | Spec Section | `XmlDaValueType` Enum | `XmlDaValue` Methods | Status |
| --- | --- | --- | --- | --- |
| **xsd:string** | 2.7.1 | `String` (line 21) | `CreateString` (line 23), `GetString` (line 65) | ✅ Complete |
| **xsd:byte** | 2.7.1 | `Int8` (line 24) | `CreateInt8` (line 26), `GetInt8` (line 68) | ✅ Complete |
| **xsd:unsignedByte** | 2.7.1 | `UInt8` (line 27) | `CreateUInt8` (line 29), `GetUInt8` (line 71) | ✅ Complete |
| **xsd:short** | 2.7.1 | `Int16` (line 30) | `CreateInt16` (line 32), `GetInt16` (line 74) | ✅ Complete |
| **xsd:unsignedShort** | 2.7.1 | `UInt16` (line 33) | `CreateUInt16` (line 35), `GetUInt16` (line 77) | ✅ Complete |
| **xsd:int** | 2.7.1 | `Int32` (line 36) | `CreateInt32` (line 38), `GetInt32` (line 80) | ✅ Complete |
| **xsd:unsignedInt** | 2.7.1 | `UInt32` (line 39) | `CreateUInt32` (line 41), `GetUInt32` (line 83) | ✅ Complete |
| **xsd:long** | 2.7.1 | `Int64` (line 42) | `CreateInt64` (line 44), `GetInt64` (line 86) | ✅ Complete |
| **xsd:unsignedLong** | 2.7.1 | `UInt64` (line 45) | `CreateUInt64` (line 47), `GetUInt64` (line 89) | ✅ Complete |
| **xsd:float** | 2.7.1 | `Single` (line 48) | `CreateSingle` (line 50), `GetSingle` (line 92) | ✅ Complete |
| **xsd:double** | 2.7.1 | `Double` (line 51) | `CreateDouble` (line 53), `GetDouble` (line 95) | ✅ Complete |
| **xsd:boolean** | 2.7.1 | `Boolean` (line 54) | `CreateBoolean` (line 56), `GetBoolean` (line 98) | ✅ Complete |
| **xsd:dateTime** | 2.7.1 | `DateTime` (line 57) | `CreateDateTime` (line 59), `GetDateTime` (line 101) | ✅ Complete |

**Test Coverage**: All scalar types are tested in `tests/Opc.Classic.Xml.Tests/ReadSerializerTests.cs:1-298` and `tests/Opc.Classic.Xml.Tests/WriteSerializerTests.cs:1-164`

#### 2.2 Array Types (Spec Section 2.7.2)

Implemented for the 10 requested array/binary types commonly used by XML-DA clients:

| xsi:type | Spec Section | Implementation Status |
| --- | --- | --- |
| **ArrayOfByte** | 2.7.2 | ✅ Implemented |
| **ArrayOfShort** | 2.7.2 | ✅ Implemented |
| **ArrayOfInt** | 2.7.2 | ✅ Implemented |
| **ArrayOfLong** | 2.7.2 | ✅ Implemented |
| **ArrayOfFloat** | 2.7.2 | ✅ Implemented |
| **ArrayOfDouble** | 2.7.2 | ✅ Implemented |
| **ArrayOfBoolean / ArrayOfBool** | 2.7.2 | ✅ Implemented |
| **ArrayOfString** | 2.7.2 | ✅ Implemented |
| **ArrayOfDateTime** | 2.7.2 | ✅ Implemented |
| **base64Binary** | 2.7.2 | ✅ Implemented |

**Test Coverage**: Round-trip tests cover all 10 array/binary types in `tests/Opc.Classic.Xml.Tests/XmlDaValueSerializerTests.cs:1-215`.

#### 2.3 Extended Types

| xsi:type | Spec Section | Implementation Status |
| --- | --- | --- |
| **xsd:decimal** | 2.7.1 | ✅ Implemented |
| **xsd:time** | 2.7.1 | ✅ Implemented |
| **xsd:date** | 2.7.1 | ✅ Implemented |
| **xsd:duration** | 2.7.1 | ✅ Implemented |
| **xsd:QName** | 2.7.1 | ✅ Implemented |

**Test Coverage**: Round-trip tests cover all 5 extended scalar types in `tests/Opc.Classic.Xml.Tests/XmlDaValueSerializerTests.cs:1-215`.

#### 2.4 Enumerations (Spec Section 2.7.3)

The spec describes a methodology for server-specific enumerations via `dataType` property (ID 1). Implementation does **not** have explicit support for enumerations, but they can be handled as:
- Strings (if server returns enumeration labels)
- Integers (if server returns enumeration ordinals)

---

### Error and quality codes

The spec defines standard success/error result codes. Implementation maps SOAP fault codes and per-item `ResultID` values to the typed `XmlDaErrorCode` enum while preserving the original QName text.

#### Result codes


| Enum name | OPC result ID string | Spec section | Success vs fault | Description |
| --- | --- | --- | --- | --- |
| `Unknown` | Vendor-specific, malformed, or empty `Parse(...)` input | Implementation sentinel | Unknown (`IsSuccess()` false) | Unknown result code; `ToResultId` returns an empty string. |
| `Ok` | `S_OK` | 3.1.9 | Success (`IsSuccess()` true) | Operation succeeded; also used for omitted per-item `ResultID`. |
| `Clamp` | `S_CLAMP` | 3.1.9 | Success (`IsSuccess()` true) | Value was clamped to a valid range. |
| `DataQueueOverflow` | `S_DATAQUEUEOVERFLOW` | 3.1.9 | Success (`IsSuccess()` true) | Subscription data queue overflowed. |
| `UnsupportedRate` | `S_UNSUPPORTEDRATE` | 3.1.9 | Success (`IsSuccess()` true) | Requested subscription sampling rate was not supported; use the revised rate. |
| `AccessDenied` | `E_ACCESS_DENIED` | 3.1.9 | Fault (`IsSuccess()` false) | Caller lacks permission for the operation or item. |
| `Busy` | `E_BUSY` | 3.1.9 | Fault (`IsSuccess()` false) | Server is busy. |
| `Fail` | `E_FAIL` | 3.1.9 | Fault (`IsSuccess()` false) | Unspecified server failure. |
| `InvalidContinuationPoint` | `E_INVALIDCONTINUATIONPOINT` | 3.1.9 | Fault (`IsSuccess()` false) | Browse continuation point is invalid. |
| `InvalidFilter` | `E_INVALIDFILTER` | 3.1.9 | Fault (`IsSuccess()` false) | Browse filter is invalid. |
| `InvalidHoldTime` | `E_INVALIDHOLDTIME` | 3.1.9 | Fault (`IsSuccess()` false) | Subscription hold time is invalid. |
| `InvalidItemId` | `E_INVALIDITEMID` | 3.1.9 | Fault (`IsSuccess()` false) | Item identifier is syntactically invalid. |
| `InvalidItemName` | `E_INVALIDITEMNAME` | 3.1.9 | Fault (`IsSuccess()` false) | Item name is syntactically invalid. |
| `InvalidItemPath` | `E_INVALIDITEMPATH` | 3.1.9 | Fault (`IsSuccess()` false) | Item path is syntactically invalid. |
| `InvalidPid` | `E_INVALIDPID` | 3.1.9 | Fault (`IsSuccess()` false) | Property ID is invalid. |
| `NoSubscription` | `E_NOSUBSCRIPTION` | 3.1.9 | Fault (`IsSuccess()` false) | Subscription handle is unknown or no longer active. |
| `NotSupported` | `E_NOTSUPPORTED` | 3.1.9 | Fault (`IsSuccess()` false) | Operation or requested feature is not supported. |
| `OutOfMemory` | `E_OUTOFMEMORY` | 3.1.9 | Fault (`IsSuccess()` false) | Server could not allocate memory. |
| `Range` | `E_RANGE` | 3.1.9 | Fault (`IsSuccess()` false) | Value is outside the accepted range. |
| `BadType` | `E_BADTYPE` | 3.1.9 | Fault (`IsSuccess()` false) | Value type conversion or requested type is unsupported. |
| `ReadOnly` | `E_READONLY` | 3.1.9 | Fault (`IsSuccess()` false) | Item cannot be written. |
| `ServerState` | `E_SERVERSTATE` | 3.1.9 | Fault (`IsSuccess()` false) | Server is not in an operational state for the request. |
| `TimedOut` | `E_TIMEDOUT` | 3.1.9 | Fault (`IsSuccess()` false) | Operation timed out. |
| `UnknownItemId` | `E_UNKNOWNITEMID` | 3.1.9 | Fault (`IsSuccess()` false) | Item identifier is not known to the server. |
| `UnknownItemName` | `E_UNKNOWNITEMNAME` | 3.1.9 | Fault (`IsSuccess()` false) | Item name is not known to the server. |
| `UnknownItemPath` | `E_UNKNOWNITEMPATH` | 3.1.9 | Fault (`IsSuccess()` false) | Item path is not known to the server. |
| `WriteOnly` | `E_WRITEONLY` | 3.1.9 | Fault (`IsSuccess()` false) | Item cannot be read. |
| `BadRights` | `E_BADRIGHTS` | Legacy compatibility | Fault (`IsSuccess()` false) | Legacy server result for insufficient item rights. |

#### Quality bit mapping

`OpcQuality` packs the DA quality word as quality kind, substatus, limit, and vendor-extension fields. XML-DA readers map `QualityField` to the top-level quality kind and `LimitField` to the limit field; `XmlDaQualityCompat` preserves the low XML-DA wire byte and drops the high vendor-extension byte when writing XML-DA quality bytes.

| Bit name | Value | Meaning | Related XML-DA quality string |
| --- | --- | --- | --- |
| `OpcQuality.QualityMask` | `0x0003` (bits 0-1) | Selects the top-level `OpcQualityKind`. | `QualityField` values below. |
| `OpcQualityKind.Bad` / `OpcQuality.Bad` | `0` (`0x0000`) | Value is not useful. | `bad` |
| `OpcQualityKind.Uncertain` / `OpcQuality.Uncertain` | `1` (`0x0001`) | Value is not known to be correct. | `uncertain` |
| `OpcQualityKind.Reserved` | `2` (`0x0002`) | Reserved by OPC DA; should not appear. | None; unrecognized `QualityField` strings decode as `Bad`. |
| `OpcQualityKind.Good` / `OpcQuality.Good` | `3` (`0x0003`) | Value is current and reliable. | `good`, `goodNonSpecific` |
| `OpcQuality.SubstatusMask` | `0x003C` (bits 2-5) | Four-bit `Substatus` value, 0 through 15. | Not separately decoded by the XML-DA readers. |
| Substatus bit 0 | `0x0004` | Adds 1 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| Substatus bit 1 | `0x0008` | Adds 2 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| Substatus bit 2 | `0x0010` | Adds 4 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| Substatus bit 3 | `0x0020` | Adds 8 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| `OpcQuality.LimitMask` | `0x00C0` (bits 6-7) | Selects the `OpcQualityLimit`. | `LimitField` values below. |
| `OpcQualityLimit.NotLimited` | `0` (`0x0000`) | Value is not limited. | `none` |
| `OpcQualityLimit.Low` | `1` (`0x0040`) | Value has been pegged to the low limit. | `low` |
| `OpcQualityLimit.High` | `2` (`0x0080`) | Value has been pegged to the high limit. | `high` |
| `OpcQualityLimit.Constant` | `3` (`0x00C0`) | Value is constant and cannot move. | `constant` |
| `OpcQuality.VendorMask` | `0xFF00` (bits 8-15) | Vendor-specific extension byte, exposed as `VendorExtension`. | No XML-DA quality string; `XmlDaQualityCompat.ToWireByte` drops this high byte. |

Inspect quality bits directly from any XML-DA item result:

```csharp
using Opc.Classic;
using Opc.Classic.Xml;

static void PrintQuality(XmlDaItemValueResult item)
{
    OpcQuality quality = item.Quality;
    byte xmlDaWireByte = XmlDaQualityCompat.ToWireByte(quality);

    Console.WriteLine(
        $"raw=0x{quality.RawValue:X4} wire=0x{xmlDaWireByte:X2} " +
        $"kind={quality.Quality} substatus={quality.Substatus} " +
        $"limit={quality.Limit} vendor=0x{quality.VendorExtension:X2}");

    if (quality.Quality == OpcQualityKind.Bad)
    {
        Console.WriteLine($"{item.ItemName}: bad quality");
    }

    if (quality.Limit != OpcQualityLimit.NotLimited)
    {
        Console.WriteLine($"{item.ItemName}: limited at {quality.Limit}");
    }
}
```


**Implementation status**: `XmlDaQualityCompat.cs` implements bidirectional quality-byte mapping. `tests/Opc.Classic.Xml.Tests/XmlDaQualityCompatTests.cs:1-38` verifies the mapping, and the implementation correctly packs/unpacks quality, limit, and vendor bits per spec Table 3.1.8-1. Quality codes match OPC DA 2.05a quality codes for backwards compatibility.

---

### 5. Transport Compliance (Spec Section 2.6)

#### 5.1 SOAP 1.1 (Implemented)

| Requirement | Spec Section | Implementation | Status |
| --- | --- | --- | --- |
| **SOAP 1.1 envelope** | 2.6.1 | `SoapEnvelope.Serialize` (Serialization/SoapEnvelope.cs) | ✅ Complete |
| **HTTP POST** | 2.6.1 | `HttpXmlDaClient.PostAsync` (lines 228-249) | ✅ Complete |
| **Content-Type: text/xml; charset=utf-8** | 2.6.1 | `HttpXmlDaClient.PostAsync` (line 236) | ✅ Complete |
| **SOAPAction header** | 2.6.1 | `XmlDaConstants` (lines 29-44) + `PostAsync` (line 235) | ✅ Complete |
| **Namespace: http://opcfoundation.org/webservices/XMLDA/1.0/** | 2.6.1 | `XmlDaConstants.Namespace` (line 20) | ✅ Complete |
| **DTD/external entity resolution disabled** | 2.6.3 | `SoapEnvelope.Deserialize` XmlReaderSettings (Security) | ✅ Complete |

**Test Coverage**: `tests/Opc.Classic.Xml.Tests/SoapEnvelopeTests.cs:1-222`, `tests/Opc.Classic.Xml.Tests/HttpXmlDaClientTests.cs:1-290`

#### 5.2 SOAP 1.2 (Not Implemented)

| Requirement | Spec Section | Implementation Status |
| --- | --- | --- |
| **SOAP 1.2 envelope** | 2.6.2 | ❌ Not implemented |
| **Content-Type: application/soap+xml** | 2.6.2 | ❌ Not implemented |

**Impact**: Cannot connect to servers that **only** support SOAP 1.2 (rare)

**Recommendation**: Low priority; SOAP 1.1 is universally supported

---

### 6. Subscription Model (Spec Section 2.5)

**Fully Implemented** with polled-pull semantics:

| Feature | Spec Section | Implementation | Status |
| --- | --- | --- | --- |
| **Subscribe** | 3.5 | `HttpXmlDaClient.SubscribeAsync` | ✅ Complete |
| **SubscriptionPolledRefresh** | 3.6 | `HttpXmlDaClient.SubscriptionPolledRefreshAsync` | ✅ Complete |
| **SubscriptionCancel** | 3.7 | `HttpXmlDaClient.SubscriptionCancelAsync` | ✅ Complete |
| **ServerSubHandle** | 2.5 | Returned by Subscribe, used in Refresh/Cancel | ✅ Complete |
| **PingRate** | 2.5 | Specified in Subscribe | ✅ Complete |
| **HoldTime** | 2.5 | Specified in SubscriptionPolledRefresh | ✅ Complete |
| **WaitTime** | 2.5 | Specified in SubscriptionPolledRefresh | ✅ Complete |
| **EnableBuffering** | 2.5 | Specified in Subscribe | ✅ Complete |
| **ReturnValuesOnReply** | 2.5 | Specified in Subscribe | ✅ Complete |
| **InvalidateOnException** | 2.5 | Handled in SubscriptionPolledRefresh response | ✅ Complete |

**Notes**:
- XML-DA subscriptions are **polled** (client-pull), not COM-style **pushed** (server-callback)
- Implementation correctly handles `HoldTime` = 0 for immediate return vs. blocking until data available
- Buffering is server-controlled; client specifies preference via `EnableBuffering`

---

### 7. Property IDs (Spec Section 2.8)

The spec defines 111 standard property IDs. Implementation **does not** have an enum for these; they are handled as integers.

#### 7.1 Standard Properties (Sample)

| ID | Name | Description | Implementation Status |
| --- | --- | --- | --- |
| 1 | dataType | Canonical data type | ✅ Can be queried via GetProperties |
| 2 | value | Current value | ✅ Can be queried via GetProperties |
| 3 | quality | Current quality | ✅ Can be queried via GetProperties |
| 4 | timestamp | Current timestamp | ✅ Can be queried via GetProperties |
| 5 | accessRights | Read/write access | ✅ Can be queried via GetProperties |
| 6 | scanRate | Server scan rate | ✅ Can be queried via GetProperties |
| ... | ... | ... | ... |
| 109 | minimumValue | Min value for range | ✅ Can be queried via GetProperties |
| 110 | maximumValue | Max value for range | ✅ Can be queried via GetProperties |
| 111 | valuePrecision | Decimal precision | ✅ Can be queried via GetProperties |

**Status**: All 111 standard properties **can be queried**; implementation does not have type-safe accessors or enums.

**Recommendation for 2.0.0**: Create `XmlDaPropertyId` enum for common properties


### 8. Server Hosting (Spec Section 2.6.4)

**NOT IMPLEMENTED**. The implementation provides client-side operations only.

#### 8.1 Missing Server Components

| Component | Description | Status |
| --- | --- | --- |
| **SOAP message handlers** | Parse incoming SOAP requests | ❌ Not implemented |
| **Operation handlers** | Implement 8 XML-DA operations | ❌ Not implemented |
| **OPC DA bridge** | Bridge XML-DA to OPC DA COM servers | ❌ Not implemented |
| **Subscription manager** | Manage polled subscriptions | ❌ Not implemented |
| **ASP.NET Core middleware** | Host SOAP endpoints | ❌ Not implemented |

**Impact**: Cannot host an XML-DA server in .NET; only client connections to existing servers are supported.

**Roadmap**: Evaluate XML-DA server hosting for a future major release.


---

### Not supported

- Hosting an XML-DA server endpoint.
- SOAP 1.2 bindings.
- BSTR variants and vendor-specific value carriers beyond raw-text preservation.
- Generated SOAP proxy types or reflection-based XML serialization.
- Built-in WS-Security policy; use the supplied `HttpClient` for authentication and transport security.


---

### 9. Test Coverage Assessment

#### 9.1 Unit Tests

15 test files provide comprehensive coverage of serialization/deserialization:

| Test File | Lines | Coverage Focus |
| --- | --- | --- |
| `tests/Opc.Classic.Xml.Tests/GetStatusSerializerTests.cs:1-272` | 272 | GetStatus request/response |
| `tests/Opc.Classic.Xml.Tests/ReadSerializerTests.cs:1-298` | 298 | Read request/response, scalar types |
| `tests/Opc.Classic.Xml.Tests/WriteSerializerTests.cs:1-164` | 164 | Write request/response, scalar types |
| `tests/Opc.Classic.Xml.Tests/BrowseSerializerTests.cs:1-167` | 167 | Browse request/response |
| `tests/Opc.Classic.Xml.Tests/GetPropertiesSerializerTests.cs:1-187` | 187 | GetProperties request/response |
| `tests/Opc.Classic.Xml.Tests/SubscribeSerializerTests.cs:1-155` | 155 | Subscribe request/response |
| `tests/Opc.Classic.Xml.Tests/SubscriptionPolledRefreshSerializerTests.cs:1-151` | 151 | SubscriptionPolledRefresh request/response |
| `tests/Opc.Classic.Xml.Tests/SubscriptionCancelSerializerTests.cs:1-118` | 118 | SubscriptionCancel request/response |
| `tests/Opc.Classic.Xml.Tests/XmlDaQualityCompatTests.cs:1-38` | 38 | Quality bit mapping |
| `tests/Opc.Classic.Xml.Tests/XmlDaConstantsTests.cs:1-50` | 50 | Namespace/SOAPAction constants |
| `tests/Opc.Classic.Xml.Tests/XmlDaErrorCodesTests.cs:1-64` | 64 | XML-DA success/error code mapping |
| `tests/Opc.Classic.Xml.Tests/XmlDaServerStateTests.cs:1-35` | 35 | ServerState enum |
| `tests/Opc.Classic.Xml.Tests/HttpXmlDaClientTests.cs:1-290` | 290 | HTTP client integration |
| `tests/Opc.Classic.Xml.Tests/SoapEnvelopeTests.cs:1-222` | 222 | SOAP envelope serialization |
| `tests/Opc.Classic.Xml.Tests/XmlDaValueSerializerTests.cs:1-215` | 215 | Array, base64Binary, and extended scalar value round-trips |

**Total**: 2426 lines of test code

#### 9.2 Integration Tests

**NOT FOUND**. No tests against third-party XML-DA servers (e.g., Softing, Kepware, Matrikon).

**Recommendation before 1.0 GA / post-1.0 hardening**: Add integration tests against a public XML-DA server or docker-based test server

---

#### Verification

`tests\Opc.Classic.Xml.Tests` covers the HTTP client path and per-operation serializers. XML-DA server hosting remains out of scope for the current package. The project participates in the repository build and test gates; the repository-wide validation baseline is summarized in [Spec coverage overview](#spec-coverage-overview).

---

### 10. Gap Analysis & Recommendations

#### 10.1 Critical Gaps (Blocking Production Use)

| Gap | Impact | Recommendation |
| --- | --- | --- |
| **No server hosting** | Cannot expose .NET applications as XML-DA servers | **2.0.0**: Implement ASP.NET Core server |

#### 10.2 Non-Critical Gaps

| Gap | Impact | Recommendation |
| --- | --- | --- |
| **No SOAP 1.2** | Cannot connect to SOAP 1.2-only servers (rare) | **2.0.0**: Consider if user demand exists |
| **Generic property ID handling** | No type-safe property accessors | **2.0.0**: Create `XmlDaPropertyId` enum |
| **No integration tests** | Limited real-world validation | **1.0 GA / post-1.0**: Add integration tests |

#### 10.3 Coverage Recommendations by Release

##### 1.0.0-rc.10 (Client-Only)
- ✅ All 8 operations, scalar values, extended scalar values, array values, base64Binary, type-safe error codes, and SOAP 1.1 complete
- ⚠️ **Add integration tests** — Validate against real XML-DA servers

##### 2.0.0 (Full Feature Set)
- 🎯 **Implement server hosting** — ASP.NET Core middleware + subscription manager (pending)
- 🔍 **Consider SOAP 1.2** — If user demand exists
- 🔍 **Consider type-safe property accessors** — `XmlDaPropertyId` enum

---

### 11. Spec Alignment Summary

#### 11.1 Compliance Score

| Category | Score | Notes |
| --- | --- | --- |
| **Operations** | 100% (8/8) | All operations implemented |
| **Scalar Types** | 100% | Base + extended XML Schema scalar types implemented |
| **Array Types** | 100% (10/10 requested) | ArrayOfByte/Short/Int/Long/Float/Double/String/Boolean/DateTime + base64Binary |
| **Quality Codes** | 100% | Full bit-packing compliance |
| **Error Codes** | 100% | Typed `XmlDaErrorCode` mapping for faults and ResultID values |
| **Transport** | 90% | SOAP 1.1 complete, SOAP 1.2 missing |
| **Subscription** | 100% | Polled-pull model fully implemented |
| **Properties** | 100% | All 111 properties queryable |
| **Server Hosting** | 0% | Client-only |

**Overall**: ~85% spec compliance (client-side, scalar + array values; server hosting pending)

#### 11.2 Deviations from Spec

1. **No server hosting** — Pending ASP.NET Core server implementation
2. **No SOAP 1.2** — Acceptable; SOAP 1.1 universally supported

#### 11.3 Spec Ambiguities / Open Questions

None identified. Spec is clear and implementation follows closely.


### Roadmap

- complete XML-DA interop runs against representative third-party servers when access is available.
- evaluate XML-DA server hosting;
- add vendor-specific value carriers where interop demand justifies them;
- consider optional SOAP security helpers layered on top of `HttpClient`.

---

### References

- **Specification**: vendored `OPC-XMLDA-1.01.md` spec (4914 lines)
- **Implementation**: `src/Opc.Classic.Xml/` (HttpXmlDaClient.cs, IXmlDaClient.cs, XmlDaValue.cs, XmlDaValueType.cs, XmlDaQualityCompat.cs, Serialization/)
- **Tests**: `tests/Opc.Classic.Xml.Tests/` (15 test files, 2426 lines)
- **Status**: this section

---

**Analysis Completed**: 2025-01-24
**Next Review**: After server hosting implementation

## Cross-cutting themes

### Generated and hand-written projections now coexist

Most OPC Classic DCOM interfaces use `[GenerateOpcProxy]` and `[OpcGenerateServerDispatch]`. A few interface-pointer-heavy surfaces still use hand-written proxies or dispatchers, for example Batch enumerators and DX configuration calls with compound structures.

### Cross-platform vs Windows CCW coverage differs

The cross-platform managed DCOM path usually has broader interface coverage than Windows CCW native hosting. DA has the most complete CCW surface; AE covers subscription, browser, event sink delivery, and the array-heavy server methods, with only connection-point enumeration stubs remaining. HDA CCWs cover browser, read, update, annotation, advise, and playback paths; remaining HDA concerns are server semantics, connection-point enumeration stubs, and conformance stress coverage.

### Runtime semantics are server-specific

Several specs define server behavior beyond wire projection: Batch namespace models, CPX type-conversion/data filters, DX runtime transfer state, HDA aggregate calculations, and XML-DA server hosting. The docs mark those separately from client proxy/dispatcher coverage.

### Error constants and codecs have moved forward

Earlier reviews flagged missing CPX, DX, HDA, Batch, XML-DA array, and DA VARIANT/OPCITEM codecs. Those are now implemented where noted in each document; do not carry forward old “codec-blocked” caveats without checking the source.

### Current validation baseline

See [Spec coverage overview](#spec-coverage-overview) for the repository-wide baseline.

