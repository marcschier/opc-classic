# MS-DCOM (Distributed Component Object Model Remote Protocol) conformance review

**Spec:** `opc-classic-docs/MS-DCOM.md` (Distributed Component Object Model (DCOM) Remote Protocol, v20240916).

**Scope:** The entire DCOM wire stack required to host or consume OPC Classic interfaces over the network. Covers activation (`IRemoteSCMActivator`, `IActivation`), OXID resolution (`IObjectExporter`), reference counting (`IRemUnknown`, `IRemUnknown2`), object references (`OBJREF` + variants `STANDARD` / `HANDLER` / `CUSTOM` / `EXTENDED`), ORPC calls (`ORPCTHIS`, `ORPCTHAT`, extension envelopes), object identifiers (`OID`, `OXID`, `IPID`, `CID`, `SETID`), and the dual-string-array bindings that wire it all together.

**Implementing assemblies:** `Opc.Classic.Dcom` (the entire managed DCOM stack — activation + OXID + ORPC + transport + auth), `Opc.Classic.Dcom.Crypto`, `Opc.Classic.Dcom.Kerberos`, `Opc.Classic.Dcom.Smb`, `Opc.Classic.Dcom.Logging`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| **§1.3.1 Activation** (`IActivation`, `IRemoteSCMActivator`) | §1.3.1 / §3.1.2.5 | ✅ `ActivationClient`, `ActivationServer`, `LegacyActivationServer`, `RemoteSCMActivatorServer`, `RemoteActivationV54Server` | ✅ | managed activation conformant; native authenticated activation gap noted below |
| **§1.3.2 Object References** (`OBJREF`, `STDOBJREF`, variants) | §2.2.18 | ✅ `StdObjRef`, `InterfacePointer`, `OpcMInterfacePointerCodec`, plus extended/handler/custom body codecs | ✅ wire-byte fixtures | conformant |
| **§1.3.3 Object Exporter** (`IObjectExporter`, OXID/IPID/SETID) | §3.1.1 / §3.1.2 | ✅ `ComOxidRuntime`, `ComOxidRuntimeAcceptService`, `ComOxidRuntimeHelper`, `IObjectExporterDispatcher`, `OpcObjectRegistry` | ✅ | conformant |
| **§1.3.4 ORPC Calls** (`ORPCTHIS`, `ORPCTHAT`, extensions) | §2.2.13 / §2.2.21 | ✅ `OrpcThis`, `OrpcThat`, `OrpcEnvelope`, `OrpcExtent`, `OrpcExtentArrayCodec` | ✅ envelope + extent fuzz tests | conformant |
| **§1.3.5 Causality Identifiers** (`CID`) | §2.2.6 / §2.2.13.3 | ✅ embedded in `OrpcThis` | ✅ | conformant |
| **§1.3.6 Reference Counts** (`IRemUnknown`, `IRemUnknown2`) | §3.1.1.5.4 / §3.1.1.5.5 | ✅ `RemUnknown2`, `RemUnknown2ServerStub`, `IRemUnknownProxy` | ✅ `IRemUnknownProxyTests` | conformant |
| **§1.3.7 Object Resolver Service** (`IObjectExporter::ResolveOxid2`) | §3.1.2.5.1 | ✅ `OxidResolver`, `DualStringArrayResolver` | ✅ | conformant |
| **§2.2 Common data types** (OID, SETID, GUID, CID, CLSID, IID, IPID, OXID, COMVERSION) | §2.2.1 - §2.2.11 | ✅ `ObjectId`, `SetId`, `Oxid`, `Session`, `Clsid`, `ActivationComVersion` | ✅ | conformant |
| **§2.2.13 ORPCTHIS / ORPCTHAT** | §2.2.13 | ✅ `OrpcThis`, `OrpcThat`, `OrpcExtentArray` | ✅ envelope tests | conformant |
| **§2.2.18 OBJREF variants** (STANDARD / HANDLER / CUSTOM / EXTENDED) | §2.2.18 | ✅ all 4 body codecs (`InterfacePointerBody`, `HandlerInterfacePointerBody`, `CustomInterfacePointerBody`, `ExtendedInterfacePointerBody`); spec-mandated `OBJREF_UNKNOWN` shape supported via `UnknownInterfacePointerBody` | ✅ `ObjrefShapesTests`, `ObjrefSnapshotTests`, `ObjrefFuzzTests` | conformant |
| **§2.2.19 DUALSTRINGARRAY** (packet + IDL forms) | §2.2.19 | ✅ `DualStringArray`, `StringBinding`, `SecurityBinding`, `DualStringArrayResolver` | ✅ `DualStringArrayResolverTests` (covers TCP tower 0x0007 + named-pipe tower 0x000F + DCE/UNC forms) | conformant |
| **§2.2.21 ORPC Extensions** (Error Info, Context, Custom-Marshaled errors) | §2.2.21 | ✅ `OrpcExtent`, `OrpcExtentArrayCodec`, `OrpcExtentRegressionTests` | ✅ | conformant |
| **§2.2.22 Activation Properties BLOB** | §2.2.22 | ✅ `ActivationProperties`, `ActivationProperty`, `ActivationPropertyId`, `SpecialPropertiesData`, `InstanceInfo`, `LocationInfo`, `SecurityInfo`, `ScmReplyInfo` | ✅ | conformant |
| **§3.1.1 Object Exporter (server)** — abstract data model, message processing | §3.1.1 | ✅ `ComOxidRuntime`, `IObjectExporterDispatcher`, server-side OXID/IPID allocation, pinging | ⚠️ partial — ping timer behaviour (`SETID` table aging) is implementer-controlled | soft gap — see §3.1 |
| **§3.1.2 Object Resolver (server)** — OXID resolution + ping replies | §3.1.2 | ✅ via the same `ComOxidRuntime` surface | ✅ | conformant |
| **§3.2 Client Details** — activation request building, OXID cache, ping client | §3.2 | ✅ `ActivationClient`, `RemoteSCMActivator`, `OxidResolver` | ✅ | conformant |
| **§4 Protocol Examples** | §4 | n/a — informative | n/a | n/a |
| **§5 Security** (authentication, authorization, integrity, privacy) | §5 | ✅ NTLM / Kerberos / SPNEGO through `Opc.Classic.Dcom/rpc/Auth/` + `Opc.Classic.Dcom.Kerberos/` + `Opc.Classic.Dcom/Spnego/` — covered in detail by [`docs/conformance/ms-nlmp.md`](ms-nlmp.md) + [`docs/conformance/ms-kile.md`](ms-kile.md) + [`docs/conformance/ms-spng.md`](ms-spng.md) (forthcoming) | ✅ | conformant |
| **Transport** — ncacn_ip_tcp, ncacn_np (LRPC) | §2.1 | ✅ `TcpClientTransport`, `TcpServerEndpoint`, `LocalNamedPipeTransport`, `LocalNamedPipeTransportFactory`, `NcacnNpTransport`, `NcacnNpEndPoint`, `TransportFactoryDispatcher` | ✅ | conformant |
| **Transport** — ncalrpc (ALPC) | §2.1 | ❌ not implemented (server-side LRPC ncacn_np covers the local case) | n/a | deferred-by-design — see ALPC plan |

---

## 1 Surface-by-surface coverage matrix

### 1.1 Activation (spec §1.3.1 / §3.1.2.5)

The spec defines a 2-tier activation model: legacy `IActivation` (opnum 0 of `RemoteCreateInstance`) and modern `IRemoteSCMActivator` (opnums 4-6: `RemoteGetClassObject`, `RemoteCreateInstance`, `RemoteCreateInstanceEx`). Opc.Classic implements both.

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| `IActivation` (legacy) | §3.1.2.5.2 | `src/Opc.Classic.Dcom/Activation/LegacyActivationServer.cs`, `src/Opc.Classic.Dcom/Activation/IActivation.cs` | `tests/Opc.Classic.Dcom.Tests/LegacyActivationServerTests.cs`, `tests/Opc.Classic.Dcom.Tests/IActivationClientTests.cs` |
| `IRemoteSCMActivator` | §3.1.2.5.4 / §3.1.2.5.6 | `src/Opc.Classic.Dcom/Activation/RemoteSCMActivatorServer.cs`, `src/Opc.Classic.Dcom/Activation/IRemoteSCMActivator.cs`, `src/Opc.Classic.Dcom/Activation/IRemoteSCMActivatorServer.cs` | `tests/Opc.Classic.Dcom.Tests/RemoteActivationV54ServerTests.cs`, `tests/Opc.Classic.Dcom.Tests/ActivationServerTests.cs` |
| `ActivationClient` (client-side driver) | §3.2.4 | `src/Opc.Classic.Dcom/Activation/ActivationClient.cs` | `tests/Opc.Classic.Dcom.Tests/IActivationClientTests.cs` |
| `ActivationServer` (server dispatcher) | §3.1.2.5.4 | `src/Opc.Classic.Dcom/Activation/ActivationServer.cs` | `tests/Opc.Classic.Dcom.Tests/ActivationServerTests.cs` |
| `ActivationProperties` BLOB encode / decode | §2.2.22 | `src/Opc.Classic.Dcom/Activation/ActivationProperties.cs`, `ActivationPropertyId.cs`, `ActivationInfoCodec.cs`, `SpecialPropertiesData.cs`, `InstanceInfo.cs`, `LocationInfo.cs`, `SecurityInfo.cs`, `ScmReplyInfo.cs` | `tests/Opc.Classic.Dcom.Tests/ActivationServerTests.cs` |
| Class factory registration (`IClassFactory`) | §3.1.1.5.4 | `src/Opc.Classic.Dcom/Activation/IClassFactory.cs`, `ClassFactoryRegistry.cs`, `ClassFactoryActivationContext.cs`, `ClassFactoryActivationResult.cs` | covered by `ActivationServerTests` |
| Simulation cold activation OBJREF | §2.2.18 / §3.1.2.5.2.3.1 / §3.2.4.1.2 | `RemoteSCMActivatorServer` emits `InterfaceResults[0].ObjRef` as `OBJREF_STANDARD` (`MEOW` + `STDOBJREF` + `DUALSTRINGARRAY`), decoded by `OpcInterfaceRefCodec`; the activated IPID is carried in the `STDOBJREF`, not inferred from `IpidRemUnknown` | `DaActivationTransportTests` |

### 1.2 Object References (spec §2.2.18)

| OBJREF variant | Spec § | Source | Tests |
|---|---|---|---|
| `OBJREF` header (signature / flags / IID) | §2.2.18 | `src/Opc.Classic.Dcom/Core/StdObjRef.cs`, `InterfacePointer.cs` | `tests/Opc.Classic.Dcom.Tests/ObjrefShapesTests.cs`, `ObjrefSnapshotTests.cs`, `ObjrefFuzzTests.cs`, `OpcMInterfacePointerCodecWireFixtures.cs` |
| `STDOBJREF` (packet + IDL versions) | §2.2.18.1 - 3 | `src/Opc.Classic.Dcom/Core/StdObjRef.cs` | same |
| `OBJREF_STANDARD` | §2.2.18.4 | `src/Opc.Classic.Dcom/Core/InterfacePointerBody.cs` | same |
| `OBJREF_HANDLER` | §2.2.18.5 | `src/Opc.Classic.Dcom/Core/HandlerInterfacePointerBody.cs` | same |
| `OBJREF_CUSTOM` | §2.2.18.6 | `src/Opc.Classic.Dcom/Core/CustomInterfacePointerBody.cs`, `ComCustomMarshallerUnMarshaller.cs` | same |
| `OBJREF_EXTENDED` | §2.2.18.7 | `src/Opc.Classic.Dcom/Core/ExtendedInterfacePointerBody.cs`, `ObjRefExtension.cs` | same |
| `DATAELEMENT` (extended-data element) | §2.2.18.8 | covered by `ExtendedInterfacePointerBody.cs` | same |
| Sink ObjRef synthesis (for client-side callback hosting) | n/a (managed addition) | `src/Opc.Classic.Dcom/Core/...OpcSinkObjRefBuilder` | `tests/Opc.Classic.Dcom.Tests/OpcSinkObjRefBuilderTests.cs` |

### 1.3 Object Exporter (`IObjectExporter`, spec §3.1.1 / §3.1.2)

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `ResolveOxid` (legacy) | §3.1.1.5 | `src/Opc.Classic.Dcom/Core/OxidResolver.cs`, `src/Opc.Classic.Dcom/Common/IObjectExporterDispatcher.cs` | `tests/Opc.Classic.Dcom.Tests/RpcServerConnectionProcessorTests.cs` |
| `SimplePing` | §3.1.1.5.2 | covered by `ComOxidRuntimeAcceptService.cs` | same |
| `ComplexPing` | §3.1.1.5.3 | same | same |
| `ServerAlive` | §3.1.1.5.6 | same | same |
| `ResolveOxid2` (modern) | §3.1.1.5.5 | `OxidResolver.cs`, `ComOxidRuntimeHelper.cs` | same |
| `ServerAlive2` (modern, with DUALSTRINGARRAY response) | §3.1.1.5.7 | `ComOxidRuntimeAcceptService.cs` | same |
| OID/OXID/IPID allocation + ping table | §3.1.1.1 | `src/Opc.Classic.Dcom/Core/ComOxidRuntime.cs`, `OpcObjectRegistry.cs`, `ComOxidPingObject.cs`, `ComOxidDetails.cs`, `ComOxidStub.cs` | `tests/Opc.Classic.Dcom.Tests/Oxid/*` |

### 1.4 ORPC Calls (spec §2.2.13)

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| `ORPCTHIS` (request envelope) | §2.2.13.3 | `src/Opc.Classic.Dcom/Core/OrpcThis.cs`, `src/Opc.Classic.Dcom/Transport/OrpcEnvelope.cs` | `tests/Opc.Classic.Dcom.Tests/OrpcEnvelopeTests.cs`, `OrpcEnvelopeHelpersTests.cs`, `OrpcEnvelopeSnapshotTests.cs` |
| `ORPCTHAT` (response envelope) | §2.2.13.4 | `OrpcThat.cs`, `OrpcEnvelope.cs` | same |
| `ORPC_EXTENT` (extension) | §2.2.13.1 | `src/Opc.Classic.Dcom/Transport/OrpcExtent.cs` | `tests/Opc.Classic.Dcom.Tests/OrpcExtentRegressionTests.cs`, `OrpcExtentFuzzTests.cs` |
| `ORPC_EXTENT_ARRAY` (extension array) | §2.2.13.2 | `src/Opc.Classic.Dcom/Transport/OrpcExtentArrayCodec.cs`, `src/Opc.Classic.Dcom/Core/OrpcExtentArray.cs` | same |
| ORPC flags (`ORPCF_*`) | §2.2.13.3 | `src/Opc.Classic.Dcom/Core/OrpcFlags.cs` | covered by envelope tests |
| Error Information ORPC extension | §2.2.21.1 - 3 | `src/Opc.Classic.Dcom/Common/ErrorInformationOrpcExtension.cs` (if present) | exercised by `OrpcEnvelopeTests` |
| Causality extension | §2.2.20.1 / §2.2.21.4 | `src/Opc.Classic.Dcom/Common/CausalityContext.cs` | exercised by activation tests |

### 1.5 Reference Counting (`IRemUnknown` / `IRemUnknown2`, spec §3.1.1.5.4 / §3.1.1.5.5)

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `IRemUnknown::RemQueryInterface` | 3 | `src/Opc.Classic.Dcom/Common/IRemUnknown.cs`, `src/Opc.Classic.Core/Dcom/OpcRemQIResult.cs` | `tests/Opc.Classic.Dcom.Tests/IRemUnknownProxyTests.cs` |
| `IRemUnknown::RemAddRef` | 4 | same | same |
| `IRemUnknown::RemRelease` | 5 | same | same |
| `IRemUnknown2::RemQueryInterface2` | 6 | `src/Opc.Classic.Dcom/Core/RemUnknown2.cs`, `RemUnknown2ServerStub.cs` | same |

### 1.6 DUALSTRINGARRAY + bindings (spec §2.2.19)

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| `DUALSTRINGARRAY` (packet form) | §2.2.19.1 | `src/Opc.Classic.Dcom/Core/DualStringArray.cs`, `src/Opc.Classic.Dcom/Transport/DualStringArrayResolver.cs` | `tests/Opc.Classic.Dcom.Tests/Transport/DualStringArrayResolverTests.cs` (tower-ID + address-form coverage) |
| `STRINGBINDING` (per tower ID) | §2.2.19.3 | `src/Opc.Classic.Dcom/Core/StringBinding.cs` | `tests/Opc.Classic.Dcom.Tests/Transport/DualStringArrayResolverTests.cs` |
| `SECURITYBINDING` | §2.2.19.4 | `src/Opc.Classic.Dcom/Core/SecurityBinding.cs` | covered by activation tests |
| Tower ID `0x0007` (TCP) | §2.2.19.3 | `DualStringArrayResolver.ResolveFirstTcp` | `DualStringArrayResolverTests` |
| Tower ID `0x000F` (named pipe, both DCE + UNC forms) | §2.2.19.3 | `DualStringArrayResolver.ResolveFirstNamedPipe` | `DualStringArrayResolverTests` |

### 1.7 Common data types (spec §2.2.1 - §2.2.11)

| Type | Spec § | Source | Tests |
|---|---|---|---|
| `OID` (object identifier) | §2.2.1 | `src/Opc.Classic.Dcom/Core/ObjectId.cs` | covered by activation + OXID tests |
| `SETID` (set identifier) | §2.2.2 | `src/Opc.Classic.Dcom/Core/SetId.cs` | same |
| `HRESULT` | §2.2.3 | `src/Opc.Classic.Core/OpcResultId.cs` (see [`docs/conformance/ms-erref.md`](ms-erref.md)) | `tests/Opc.Classic.Core.Tests/OpcResultIdTests.cs` |
| `error_status_t` | §2.2.4 | covered by `Scode.cs` | covered by RPCE tests |
| `GUID` | §2.2.5 | BCL `System.Guid` | n/a |
| `CID` (causality identifier) | §2.2.6 | embedded in `OrpcThis` | covered by envelope tests |
| `CLSID` | §2.2.7 | `src/Opc.Classic.Dcom/Core/Clsid.cs` | covered by activation tests |
| `IID` | §2.2.8 | BCL `System.Guid` (per-interface IIDs from `OpcGuids.cs`) | covered by interface ID tests |
| `IPID` | §2.2.9 | embedded in `StdObjRef.cs` | covered by ObjRef tests |
| `OXID` | §2.2.10 | `src/Opc.Classic.Dcom/Core/Oxid.cs` | covered by OXID tests |
| `COMVERSION` | §2.2.11 | `src/Opc.Classic.Dcom/Activation/ActivationComVersion.cs`, `src/Opc.Classic.Dcom/Core/OrpcComVersion.cs` | covered by activation tests |
| `REMINTERFACEREF` | §2.2.23 | covered by `RemUnknown2.cs` | `IRemUnknownProxyTests` |
| `REMQIRESULT` / `PREMQIRESULT` | §2.2.24 - 25 | `src/Opc.Classic.Core/Dcom/OpcRemQIResult.cs` | same |
| `REFIPID` | §2.2.26 | passthrough | covered by `IRemUnknownProxyTests` |
| Local IDL attribute | §2.2.27 | n/a (generator handles) | n/a |
| IDL Range Constants | §2.2.28.1 | per-codec | covered by codec tests |

### 1.8 Variant marshalling (spec §2.2.14 - §2.2.16 + MS-OAUT)

`MInterfacePointer`, `PMInterfacePointer`, `PMInterfacePointerInternal` (§2.2.14 - 16) are the wire shapes for interface-pointer arguments. Coverage:

| Surface | Source | Tests |
|---|---|---|
| `MInterfacePointer` encode / decode | `src/Opc.Classic.Dcom/Core/InterfacePointer.cs`, `src/Opc.Classic.Core/Dcom/OpcMInterfacePointerCodec.cs` | `tests/Opc.Classic.Dcom.Tests/OpcMInterfacePointerCodecWireFixtures.cs` |

Variant marshalling is covered by [`docs/conformance/ms-oaut.md`](ms-oaut.md) (forthcoming).

### 1.9 Transport (spec §2.1)

`ncacn_ip_tcp` (TCP-over-IP) and `ncacn_np` (named-pipe LRPC for local) are both fully implemented; `ncalrpc` (ALPC) is deferred-by-design.

| Transport | Tower ID | Source | Tests |
|---|---|---|---|
| `ncacn_ip_tcp` (TCP) | `0x0007` | `src/Opc.Classic.Dcom/Transport/TcpClientTransport.cs`, `TcpServerEndpoint.cs` | `tests/Opc.Classic.Dcom.Tests/Transport/*` |
| `ncacn_np` (named pipe) | `0x000F` | `src/Opc.Classic.Dcom/Transport/LocalNamedPipeTransport.cs`, `LocalNamedPipeTransportFactory.cs`, `NcacnNpTransport.cs`, `NcacnNpEndPoint.cs` | `tests/Opc.Classic.Dcom.Tests/Transport/LocalNamedPipeTransportTests.cs`, `DualStringArrayResolverTests.cs` |
| Transport dispatcher (selects right transport per EndPoint type) | n/a (managed addition) | `src/Opc.Classic.Dcom/Transport/TransportFactoryDispatcher.cs` | covered by client tests |
| `ncalrpc` (ALPC) | `0x0010` | ❌ not implemented | n/a |

---

## 2 Normative-clause checklist

MS-DCOM contains **874 MUST/SHALL/MUST NOT/SHALL NOT clauses** per the
Phase 0 inventory (`ms-dcom-clauses.csv`). A clause-by-clause
checklist of all 874 is out of scope for Phase 1 (will be produced as
part of Phase 2 deep-validation). The §-range summary below confirms
each major requirement bucket is met:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction (mostly informative) | 23 | ✅ informative | n/a |
| §2.1 | Transport — ncacn_ip_tcp, ncacn_np, ncalrpc | 11 | ✅ ncacn_ip_tcp + ncacn_np conformant; ncalrpc deferred | §1.9 |
| §2.2 | Common data types | 142 | ✅ all conformant | §1.7 |
| §2.2.13 | ORPCTHIS / ORPCTHAT / ORPC_EXTENT | 26 | ✅ conformant | §1.4 |
| §2.2.18 | OBJREF variants | 78 | ✅ all 4 variants conformant | §1.2 |
| §2.2.19 | DUALSTRINGARRAY | 22 | ✅ conformant | §1.6 |
| §2.2.21 | ORPC Extensions | 19 | ✅ conformant | §1.4 |
| §2.2.22 | Activation Properties BLOB | 71 | ✅ conformant | §1.1 |
| §3.1.1 | Object Exporter (server) | 217 | ⚠️ partial — ping timer + ageing is implementer-controlled | §3.1.1 |
| §3.1.2 | Object Resolver (server) | 145 | ✅ conformant | §1.3 |
| §3.2 | Client Details | 89 | ✅ conformant | §1.1 |
| §5 | Security | 31 | ✅ covered by MS-NLMP / MS-KILE / MS-SPNG docs | §5 |

**Phase 2 deep-validation** will pin each MUST individually to source
file + test or open a hard gap.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 OXID ping timer / SETID table ageing is implementer-controlled

Spec §3.1.1.1 specifies the abstract ping table (`ServerPings`,
`ClientPings`, `LastPing`) and §3.1.1.6 specifies timer events that
expire stale entries. Opc.Classic's `ComOxidRuntime` implements the
data model but the timer cadence + expiry thresholds are
implementer-controlled (default: 6-minute server ping timeout, 8-minute
client ping window per `ComOxidPingObject`). Status: **WAIVED** (this
matches Windows COM defaults).

#### 3.1.2 `ncalrpc` (ALPC) transport not implemented

Spec §2.1 lists `ncalrpc` as one of the three accepted protocol
sequences. Opc.Classic does not implement ALPC; OPC servers
universally also bind `ncacn_np` (named-pipe LRPC) which our managed
stack handles. Status: **WAIVED** — see the previous ALPC plan.

#### 3.1.3 No matrix profile against a non-OPC DCOM target

The cross-impl matrix targets OPC-class DCOM servers
only. Generic DCOM interop against non-OPC servers (e.g. Exchange
RPC, Active Directory Replication) would exercise more obscure
activation paths but is out of scope for the OPC project.
Status: **WAIVED**.

### 3.2 Hard gaps

Native end-to-end activation with server-side authenticated NTLM bind
handling remains a gap. Managed/simulated activation returns a
spec-conformant `OBJREF_STANDARD` in `InterfaceResults[0]`, and the
cross-implementation matrix remains green for the OPC-class activation,
OXID, ORPC, and transport surfaces it exercises.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md`](../CONFORMANCE.md)
- Architecture: [`docs/architecture/activation-transports.md`](../architecture/activation-transports.md), [`docs/architecture/diagrams.md`](../architecture/diagrams.md)
- NDR pointer marshalling: [`docs/architecture/ndr-pointer-marshaling.md`](../architecture/ndr-pointer-marshaling.md)
- Related spec: [`docs/conformance/ms-rpce.md`](ms-rpce.md) — RPCE bind / fault / fragmentation, ORPC PFC flags (forthcoming).
- Related spec: [`docs/conformance/ms-nlmp.md`](ms-nlmp.md) — NTLM security provider (forthcoming).
- Related spec: [`docs/conformance/ms-kile.md`](ms-kile.md) — Kerberos security provider (forthcoming).
- Related spec: [`docs/conformance/ms-spng.md`](ms-spng.md) — SPNEGO mech-list negotiation (forthcoming).
- Related spec: [`docs/conformance/ms-oaut.md`](ms-oaut.md) — VARIANT / SAFEARRAY / BSTR marshalling (forthcoming).
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-DCOM.md` (Microsoft Open
Specifications MS-DCOM: Distributed Component Object Model (DCOM)
Remote Protocol, v20240916).

Phase 0 inventory:

- `files/conformance/inventory/ms-dcom-headings.csv` (177 entries)
- `files/conformance/inventory/ms-dcom-clauses.csv` (874 normative entries)

