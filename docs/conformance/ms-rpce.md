# MS-RPCE (RPC Extensions) conformance review

**Spec:** `opc-classic-docs/MS-RPCE.md` (Remote Procedure Call Protocol Extensions).

**Scope:** The DCE 1.1 / C706 RPC protocol with Microsoft extensions used by DCOM. Covers connection-oriented PDU formats (`bind`, `bind_ack`, `bind_nak`, `request`, `response`, `fault`, `alter_context`, `auth3`, `cancel`, `shutdown`, `orphaned`), presentation context negotiation, fragmentation + chunking via PFC flags, the per-PDU auth-trailer block (`sec_trailer` carrying NTLM / Kerberos / SPNEGO blobs), call-id semantics, association group identification, and the underlying transport layer (`ncacn_ip_tcp`, `ncacn_np`, `ncalrpc`).

**Implementing assemblies:** `Opc.Classic.Dcom` (PDU codecs, presentation context, auth verifier, transport).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `bind` PDU | §2.2.2.13 | ✅ `BindPdu` | ✅ | conformant |
| `bind_ack` PDU | §2.2.2.14 | ✅ `BindAcknowledgePdu` | ✅ | conformant |
| `bind_nak` PDU | §2.2.2.15 | ✅ `BindNoAcknowledgePdu`, `BindNoAcknowledgeReason` | ✅ | conformant |
| `request` PDU | §2.2.2.4 | ✅ `RequestCoPdu` | ✅ | conformant |
| `response` PDU | §2.2.2.5 | ✅ `ResponseCoPdu` | ✅ | conformant |
| `fault` PDU | §2.2.2.6 | ✅ `FaultCoPdu`, `FaultCode`, `FaultException` | ✅ | conformant |
| `alter_context` PDU | §2.2.2.16 | ✅ `AlterContextPdu` | ✅ | conformant |
| `alter_context_resp` PDU | §2.2.2.17 | ✅ `AlterContextResponsePdu` | ✅ | conformant |
| `auth3` PDU | §2.2.2.18 | ✅ `Auth3Pdu` | ✅ | conformant |
| `cancel` PDU (`CO_CANCEL`) | §2.2.2.7 | ✅ `CancelCoPdu` | ✅ | conformant |
| `shutdown` PDU | §2.2.2.10 | ✅ `ShutdownPdu` | ✅ | conformant |
| `orphaned` PDU | §2.2.2.11 | ✅ `OrphanedPdu` | ✅ | conformant |
| PDU header (`rpc_vers`, `rpc_vers_minor`, `PTYPE`, `pfc_flags`, `packed_drep`, `frag_length`, `auth_length`, `call_id`) | §2.2.2.1 | ✅ `PduCodec.cs`, `IProtocolDataUnit.cs` | ✅ `PduCodecTests`, `PduCodecFuzzTests`, `RpcPduCodecRegressionTests`, `DecodedOpcPduTests` | conformant |
| PFC flags (`PFC_FIRST_FRAG`, `PFC_LAST_FRAG`, `PFC_PENDING_CANCEL`, `PFC_SUPPORT_HEADER_SIGN`, `PFC_CONC_MPX`, `PFC_DID_NOT_EXECUTE`, `PFC_MAYBE`, `PFC_OBJECT_UUID`) | §2.2.2.3 | ✅ encoded in `PduCodec` | ✅ | conformant |
| Presentation context list (multiple syntaxes per context, alter-context flow) | §2.2.2.13 | ✅ `PresentationContext`, `PresentationSyntax`, `PresentationResult`, `PresentationResultCode`, `PresentationResultReason`, `PresentationException` | ✅ | conformant |
| Authentication verifier / `sec_trailer` | §2.2.2.11 / §2.2.2.12 | ✅ `AuthenticationVerifier`, `AuthenticationSource`, `NullAuthenticationSource` | ✅ | conformant |
| Auth-trailer types (`RPC_C_AUTHN_*`): NTLM (10) / Kerberos (16) / SPNEGO (9) / Negotiate (9) | §2.2.1.1.7 | ✅ via NTLM / Kerberos / SPNEGO stacks | ✅ | conformant |
| Protection levels (none / connect / call / pkt / pkt_integrity / pkt_privacy) | §2.2.1.1.8 | ✅ `ProtectionLevel`, `ProtectionLevelDefaultsTests` | ✅ | conformant |
| Impersonation levels | §2.2.1.1.9 | ✅ `RpcImpersonationLevel` | ✅ | conformant |
| Fragmentation + reassembly | §3.3.1.5.6 | ✅ `IFragmentable`, fragment-loop in `RpcServerConnectionProcessor` + client channel | ✅ exercised by activation + ORPC tests | conformant |
| Call multiplexing (`CONC_MPX`) | §2.2.2.13 / §3.1.1.5 | ✅ per-call-id state in `RpcServerConnectionProcessor`, `DcomCallChannel` | ✅ `DcomCallChannelTests`, `DcomCallChannelOrpcTests` | conformant |
| Server connection processor | §3.3 | ✅ `RpcServerConnectionProcessor` | ✅ `RpcServerConnectionProcessorTests` | conformant |
| Endpoint mapper `ept_map` server | MS-RPCE EPM | ✅ `EndpointMapperDispatcher` | ✅ `EndpointMapperDispatcherTests`, `ManagedDcomFullStackE2ETests` | conformant for TCP tower lookup |
| Client call channel | §3.2 | ✅ `DcomCallChannel`, `DcomCallChannelFactory` | ✅ | conformant |
| Connection establishment + tear-down | §3.3.1.5.x | ✅ `IConnection`, `Connection`, `DefaultConnection`, `IConnectionContext`, `BasicConnectionContext` | ✅ | conformant |
| Sensitive buffer pool (clear-on-return) | §5 | ✅ `SensitiveBufferPool` | ✅ exercised by all PDU codec tests | conformant |
| Transport — ncacn_ip_tcp / ncacn_np / ncalrpc | §3.1.1.1 | ✅ TCP + named pipe; ALPC deferred — see [`ms-dcom.md`](ms-dcom.md) §1.9 | ✅ | conformant (TCP + pipe) |

---

## 1 Surface-by-surface coverage matrix

### 1.1 PDU types (spec §2.2.2)

| PDU type | PTYPE | Source | Tests |
|---|---|---|---|
| `request` (CO) | 0 | `src/Opc.Classic.Dcom/RequestCoPdu.cs` | `tests/Opc.Classic.Dcom.Tests/PduCodecTests.cs`, `PduCodecFuzzTests.cs`, `RpcPduCodecRegressionTests.cs` |
| `response` (CO) | 2 | `ResponseCoPdu.cs` | same |
| `fault` (CO) | 3 | `FaultCoPdu.cs`, `FaultCode.cs`, `FaultException.cs` | same |
| `bind` | 11 | `BindPdu.cs` | same + `RpcServerConnectionProcessorTests.cs` |
| `bind_ack` | 12 | `BindAcknowledgePdu.cs` | same |
| `bind_nak` | 13 | `BindNoAcknowledgePdu.cs`, `BindNoAcknowledgeReason.cs` | same |
| `alter_context` | 14 | `AlterContextPdu.cs` | same |
| `alter_context_resp` | 15 | `AlterContextResponsePdu.cs` | same |
| `auth3` | 16 | `Auth3Pdu.cs` | same |
| `shutdown` | 17 | `ShutdownPdu.cs` | same |
| `co_cancel` | 18 | `CancelCoPdu.cs` | same |
| `orphaned` | 19 | `OrphanedPdu.cs` | same |

### 1.2 PDU header (spec §2.2.2.1) + PFC flags (spec §2.2.2.3)

| Header field | Source | Tests |
|---|---|---|
| `rpc_vers` (5 - per DCE 1.1) | `PduCodec.cs` (write/read) | `PduCodecTests` |
| `rpc_vers_minor` (0) | same | same |
| `PTYPE` (1 byte) | same | same |
| `pfc_flags` (1 byte, see PFC flags table) | same | same |
| `packed_drep` (4 bytes — `0x10, 0x00, 0x00, 0x00` = little-endian ASCII IEEE) | same | same |
| `frag_length` (2 bytes) | same | same |
| `auth_length` (2 bytes) | same | same |
| `call_id` (4 bytes) | same | same |

PFC flag encoding (`PFC_FIRST_FRAG = 0x01`, `PFC_LAST_FRAG = 0x02`,
`PFC_PENDING_CANCEL = 0x04`, `PFC_SUPPORT_HEADER_SIGN = 0x04` for bind,
`PFC_CONC_MPX = 0x10`, `PFC_DID_NOT_EXECUTE = 0x20`, `PFC_MAYBE = 0x40`,
`PFC_OBJECT_UUID = 0x80`) is honoured per spec.

### 1.3 Presentation context (spec §2.2.2.13)

| Surface | Source | Tests |
|---|---|---|
| `p_cont_elem` (`p_cont_id`, `n_transfer_syn`, `abstract_syntax`, `transfer_syntaxes[]`) | `PresentationContext.cs`, `PresentationSyntax.cs` | covered by `PduCodecTests` + activation tests |
| `p_result_t` (per-context result + reason) | `PresentationResult.cs`, `PresentationResultCode.cs`, `PresentationResultReason.cs` | same |
| Bind-time multiple abstract syntaxes | per `BindPdu` handling | exercised by DCOM bind flow in `RpcServerConnectionProcessorTests` |
| Alter-context (add presentation contexts to active connection) | `AlterContextPdu.cs` | same |

The presentation-context infrastructure supports the OPC + OXID
discovery pattern: a single bind PDU contains one presentation context
per OPC interface (`IOPCServer`, `IRemUnknown`, etc.) so a single
connection can drive multiple interfaces.

### 1.4 Authentication verifier (spec §2.2.2.11 / §2.2.2.12)

| Field | Source | Tests |
|---|---|---|
| `auth_type` (1 byte: NTLM=10 / Kerberos=16 / SPNEGO=9 / Negotiate=9) | `AuthenticationVerifier.cs` | covered by NLMP + KILE + SPNG tests |
| `auth_level` (1 byte: none=1 / connect=2 / call=3 / pkt=4 / pkt_integrity=5 / pkt_privacy=6) | `ProtectionLevel.cs`, `ProtectionLevelDefaultsTests` | `ProtectionLevelDefaultsTests.cs` |
| `auth_pad_length` (1 byte) | `AuthenticationVerifier.cs` | same |
| `auth_reserved` (1 byte) | same | same |
| `auth_context_id` (4 bytes) | same | same |
| `auth_value` (security-context-specific blob) | `AuthenticationSource.cs` (NTLM / Kerberos / SPNEGO source), `NullAuthenticationSource.cs` | `NtlmDefaultsTests.cs`, plus the security-provider-specific tests |

### 1.5 Protection levels (spec §2.2.1.1.8)

| Level | Numeric | Source | Tests |
|---|---|---|---|
| `RPC_C_AUTHN_LEVEL_DEFAULT` | 0 | `ProtectionLevel.cs` | `ProtectionLevelDefaultsTests` |
| `RPC_C_AUTHN_LEVEL_NONE` | 1 | same | same |
| `RPC_C_AUTHN_LEVEL_CONNECT` | 2 | same | same |
| `RPC_C_AUTHN_LEVEL_CALL` | 3 | same | same |
| `RPC_C_AUTHN_LEVEL_PKT` | 4 | same | same |
| `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY` | 5 | same | same |
| `RPC_C_AUTHN_LEVEL_PKT_PRIVACY` | 6 | same | same |

The defaults are honoured: `PKT_INTEGRITY` (5) is the minimum for any
DCOM connection per Windows 2008+ Extended Protection policy
(`docs/security/THREAT_MODEL.md`).

### 1.6 Server connection processor (spec §3.3)

| Surface | Source | Tests |
|---|---|---|
| Bind handling (associate group ID, presentation contexts) | `src/Opc.Classic.Dcom/Transport/RpcServerConnectionProcessor.cs` | `tests/Opc.Classic.Dcom.Tests/RpcServerConnectionProcessorTests.cs` |
| Request → dispatcher routing | same | same |
| Response framing | same | same |
| Fault generation | same | same |
| Cancellation propagation (CO_CANCEL → CallId cancellation token) | same | same |
| Per-call-id state | same | same |
| Auth-trailer validation on every PDU | same + `AuthenticationVerifier.cs` | covered by NLMP + KILE tests |
| Server-side NTLM authenticated bind and per-PDU protection | same + `ConfiguredAuthenticationSource` | `F4Auth` |
| `AcceptUnauthenticated` gate for `ncacn_np` connections (named-pipe kernel impersonation) | same | covered by LRPC tests |

### 1.7 Endpoint mapper (`ept_map`)

`EndpointMapperDispatcher` implements the managed TCP endpoint-mapper path needed by DCOM activation discovery. It decodes incoming map towers, returns TCP tower bindings for registered interfaces, and returns `EPT_S_NOT_REGISTERED` when no mapped interface or endpoint is available.

### 1.8 Client call channel (spec §3.2)

| Surface | Source | Tests |
|---|---|---|
| `DcomCallChannel` (per-connection call multiplexor) | `src/Opc.Classic.Dcom/Transport/DcomCallChannel.cs` | `tests/Opc.Classic.Dcom.Tests/DcomCallChannelTests.cs`, `DcomCallChannelOrpcTests.cs` |
| `DcomCallChannelFactory` | `DcomCallChannelFactory.cs` | same |
| Per-call cancellation, request fragmentation, response reassembly | same | same |
| OXID + IPID routing for `IRemUnknown` calls | same | `IRemUnknownProxyTests` |

### 1.9 Fragmentation + reassembly (spec §3.3.1.5.6)

`PFC_FIRST_FRAG` / `PFC_LAST_FRAG` driven request + response
fragmentation is implemented in `RpcServerConnectionProcessor` (server
side) and `DcomCallChannel` (client side). Default fragment size 5840
bytes (Windows COM default), configurable via `RpcTransportQuotas`.

### 1.10 Transport (spec §3.1.1.1)

See [`ms-dcom.md`](ms-dcom.md) §1.9 — the same `IAsyncTransport` /
`IAsyncEndpoint` surface is used for both DCOM activation and ORPC
calls.

---

## 2 Normative-clause checklist

MS-RPCE contains **575 MUST/SHALL clauses** per Phase 0 inventory
(`ms-rpce-clauses.csv`). §-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 18 | ✅ informative | n/a |
| §2.1 | Transport | 42 | ✅ TCP + pipe conformant; ALPC deferred | §1.10 |
| §2.2.1 | Common data types (DREP, conformant arrays) | 86 | ✅ conformant | `PduCodec` |
| §2.2.2 | PDU formats | 215 | ✅ all 12 PDU types conformant | §1.1 - §1.5 |
| §3.1 - 3.2 | Client side | 92 | ✅ conformant | §1.8 |
| §3.3 | Server side | 98 | ✅ conformant | §1.6 |
| §5 | Security | 24 | ✅ covered by MS-NLMP / MS-KILE / MS-SPNG docs | §1.4 |

Phase 2 deep-validation will pin each clause individually.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 ncalrpc (ALPC) transport not implemented

Same as MS-DCOM §3.1.2. Status: **WAIVED**.

#### 3.1.2 Connectionless RPC PDUs (DG_*) not implemented

Spec §2.2.3 specifies the datagram (`ncadg_*`) PDU types. DCOM
universally uses connection-oriented RPC (`ncacn_*`); no
datagram-RPC path is reachable from any OPC scenario. Status:
**WAIVED** (deferred-by-design).

#### 3.1.3 RPC over HTTP (RPCH) not implemented

Spec references RPC-over-HTTP via MS-RPCH. Opc.Classic does not
proxy DCOM through HTTP. Status: **WAIVED** — would only be needed
for firewall-traversal scenarios; non-OPC use case.

### 3.2 Hard gaps

None at present. PDU codecs, presentation context, auth verifier,
protection level handling, fragmentation, and per-call multiplexing
are all conformant and exercised by the green cross-impl matrix
plus the unit/property/fuzz test fleet.

---

## 4 Cross-references

- Architecture: [`docs/architecture/activation-transports.md`](../architecture/activation-transports.md)
- NDR pointer marshalling: [`docs/architecture/ndr-pointer-marshaling.md`](../architecture/ndr-pointer-marshaling.md)
- Related spec: [`docs/conformance/ms-dcom.md`](ms-dcom.md) — DCOM activation + ORPC layered atop RPCE.
- Related spec: [`docs/conformance/ms-nlmp.md`](ms-nlmp.md) — NTLM auth-value provider.
- Related spec: [`docs/conformance/ms-kile.md`](ms-kile.md) — Kerberos auth-value provider.
- Related spec: [`docs/conformance/ms-spng.md`](ms-spng.md) — SPNEGO auth-value provider.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-RPCE.md` (Microsoft Open
Specifications MS-RPCE: Remote Procedure Call Protocol Extensions).

Phase 0 inventory:

- `files/conformance/inventory/ms-rpce-headings.csv` (440 entries)
- `files/conformance/inventory/ms-rpce-clauses.csv` (575 normative entries)
