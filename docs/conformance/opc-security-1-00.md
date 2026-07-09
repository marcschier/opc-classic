# OPC Security 1.00 conformance review

**Spec:** `opc-classic-docs/OPC-SECURITY-1.00.md` (OPC Security Custom Interface Specification 1.00, October 17, 2000).

**Scope:** Two **optional** session-level identity-management interfaces (`IOPCSecurityNT`, `IOPCSecurityPrivate`), three OPC Security HRESULTs, supporting types (impersonation levels, logon requests), and informative DCOM-security guidelines for server implementers.

**Implementing assemblies:** `Opc.Classic.Security` (interfaces + abstractions + supporting types), `Opc.Classic.Core` (error codes), `Opc.Classic.Hosting.Windows` (CCW for the sample server), `samples/Opc.Classic.Samples.OpcSecurityServer` (reference server).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCSecurityNT` (3 methods) | §4.3 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IOPCSecurityPrivate` (3 methods) | §4.4 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `OPC_E_PRIVATE_ACTIVE` | §6.2 | ✅ `OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE` | ✅ | conformant |
| `OPC_E_LOW_IMPERS_LEVEL` | §6.2 | ✅ `OpcSecurityErrors.OPC_E_LOW_IMPERS_LEVEL` | ✅ | conformant |
| `OPC_S_LOW_AUTHN_LEVEL` | §6.2 | ✅ `OpcSecurityErrors.OPC_S_LOW_AUTHN_LEVEL` | ✅ | conformant |
| Managed `IOpcSecurity` facade | n/a | ✅ unified client-side API | ✅ | conformant |
| Reference sample server | §4.5 | ✅ `samples/Opc.Classic.Samples.OpcSecurityServer` | ✅ via `Hosting.Windows.Tests` | conformant |
| §6.3 DCOM-security guidelines (informative) | §6.3 | n/a — informative guidance mapped to `Opc.Classic.Dcom` authentication/protection policy | n/a | documented |
| Win9x in-process server considerations | §6.3.1.2 / §6.3.2 | n/a — not supported on .NET 10 | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCSecurityNT` (spec §4.3)

**IID:** `7AA83A01-6C77-11D3-84F9-00008630A38B` (per §6.1.1 IDL appendix).

| Method | Opnum | Source proxy / dispatcher | Tests |
|---|---|---|---|
| `IsAvailableNT` | 3 | `src/Opc.Classic.Security/Dcom/IOPCSecurityInterfaces.cs` → generated `Opc.Classic.Security.Dcom.IOPCSecurityNT.OpcProxy.g.cs` / `.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Security.Tests/Dcom/IOPCSecurityProxyTests.cs` |
| `QueryMinImpersonationLevel` | 4 | same | `tests/Opc.Classic.Security.Tests/OpcImpersonationLevelTests.cs` |
| `ChangeUser` | 5 | same | `tests/Opc.Classic.Security.Tests/OpcSecurityTests.cs` |

Signatures verified against the spec IDL in §6.1.1: `IsAvailableNT([out] BOOL *pbAvailable)`, `QueryMinImpersonationLevel([out] DWORD *pdwLevel)`, `ChangeUser()` (void).

### 1.2 `IOPCSecurityPrivate` (spec §4.4)

**IID:** `7AA83A02-6C77-11D3-84F9-00008630A38B` (per §6.1.1 IDL appendix).

| Method | Opnum | Source proxy / dispatcher | Tests |
|---|---|---|---|
| `IsAvailablePriv` | 3 | `src/Opc.Classic.Security/Dcom/IOPCSecurityInterfaces.cs` → generated `.OpcProxy.g.cs` / `.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Security.Tests/Dcom/IOPCSecurityProxyTests.cs` |
| `Logon` | 4 | same | `tests/Opc.Classic.Security.Tests/OpcLogonRequestTests.cs`, `tests/Opc.Classic.Security.Tests/OpcSecurityTests.cs` |
| `Logoff` | 5 | same | `tests/Opc.Classic.Security.Tests/OpcSecurityTests.cs` |

`Logon` carries `[in] LPCWSTR szUserID, [in] LPCWSTR szPassword` per §4.4.2; `OpcLogonRequest` is the managed record encapsulating these.

### 1.3 Managed `IOpcSecurity` facade

| Surface | Source | Tests |
|---|---|---|
| `IOpcSecurity` (unified API) | `src/Opc.Classic.Security/IOpcSecurity.cs` | `tests/Opc.Classic.Security.Tests/OpcSecurityTests.cs` |
| `OpcImpersonationLevel` enum | `src/Opc.Classic.Security/OpcImpersonationLevel.cs` | `tests/Opc.Classic.Security.Tests/OpcImpersonationLevelTests.cs` |
| `OpcLogonRequest` record | `src/Opc.Classic.Security/OpcLogonRequest.cs` | `tests/Opc.Classic.Security.Tests/OpcLogonRequestTests.cs` |

`OpcImpersonationLevel` enum values (per `RPC_C_IMP_LEVEL_*` in §6.3.4.5):

| Value | Spec § | RPC constant |
|---|---|---|
| `Default = 0` | §6.3.4.5 | `RPC_C_IMP_LEVEL_DEFAULT` |
| `Anonymous = 1` | §6.3.4.5 | `RPC_C_IMP_LEVEL_ANONYMOUS` |
| `Identify = 2` | §6.3.4.5 | `RPC_C_IMP_LEVEL_IDENTIFY` |
| `Impersonate = 3` | §6.3.4.5 | `RPC_C_IMP_LEVEL_IMPERSONATE` |
| `Delegate = 4` | §6.3.4.5 | `RPC_C_IMP_LEVEL_DELEGATE` |

### 1.4 OPC Security HRESULTs (spec §6.2 `OpcErrSec.h`)

| Constant | Value | Source | Tests |
|---|---|---|---|
| `OPC_E_PRIVATE_ACTIVE` | `0xC0040301` | `src/Opc.Classic.Core/Errors/OpcSecurityErrors.cs` | `tests/Opc.Classic.Core.Tests/OpcSecurityErrorsTests.cs`, `tests/Opc.Classic.Security.Tests/OpcSecurityErrorsTests.cs` |
| `OPC_E_LOW_IMPERS_LEVEL` | `0xC0040302` | same | same |
| `OPC_S_LOW_AUTHN_LEVEL` | `0x00040303` | same | same |

The `OPC_*_*` ranges (`0x0300-0x03FF` Batch and Security per OPC-COMMON §5) are honoured.

### 1.5 Reference sample server (spec §4.5)

`samples/Opc.Classic.Samples.OpcSecurityServer` provides a working stub that:
- registers a Windows CCW class factory through `ComClassObjectRegistrar.RegisterClassObject` when launched with `-Embedding`,
- implements `IOPCSecurityNT` to return `true` for `IsAvailableNT` when run as a known principal,
- implements `IOPCSecurityPrivate` with a demo private credential (`operator` / `demo`),
- demonstrates how to wire ACL checks via the managed dispatcher.

Cross-impl matrix profile `security-da` exercises this server end-to-end and is green.

### 1.6 Informative DCOM-security guidelines (spec §6.3)

The spec's §6.3 catalogue of DCOM authentication levels, packet-privacy
recommendations, NTLM vs Kerberos guidance, and Win9x in-process
considerations is **informative** rather than normative. Coverage:

| Guideline | Spec § | Coverage in this repo |
|---|---|---|
| `CoInitializeSecurity` recommendations | §6.3.1.1 | n/a — managed runtime; equivalent behavior is set via `Opc.Classic.Dcom.Security.AuthenticationLevel` policy. |
| Authentication levels (`RPC_C_AUTHN_LEVEL_*`) | §6.3.4.4 | Mapped to `OpcAuthenticationLevel` in `Opc.Classic.Dcom`. |
| Impersonation levels (`RPC_C_IMP_LEVEL_*`) | §6.3.4.5 | Mapped to `OpcImpersonationLevel` (this assembly). |
| Authentication services (`RPC_C_AUTHN_*`) | §6.3.4.6 | Mapped to `OpcAuthenticationService` in `Opc.Classic.Dcom`; managed listeners can require configured NTLMv2 bind authentication with packet integrity/privacy, while Kerberos/SPNEGO server acceptor wiring remains a DCOM-layer follow-up. |
| In-process server considerations | §6.3.2 | n/a — Opc.Classic uses out-of-proc DCOM only. |
| Windows 95/98 DCOM differences | §6.3.1.2 | n/a — .NET 10 targets modern OSes. |
| Local/remote configuration parameters | §6.3.3 | n/a — vendor-specific, not part of the wire protocol. |

---

## 2 Normative-clause checklist

OPC-SECURITY-1.00 contains 1 normative clause per the Phase 0 inventory
(`opc-security-1-00-clauses.csv`):

| § | Clause (paraphrased) | Status | Evidence |
|---|---|---|---|
| §2.5 | OPC servers SHALL support either `IOPCSecurityNT`, `IOPCSecurityPrivate`, both, or neither (servers are NOT required to implement either). | ✅ honored | `Opc.Classic.Security` exposes both interfaces but discovery via `QueryInterface` returns `E_NOINTERFACE` when the host does not opt in. The reference sample chooses to implement both; managed clients fall back gracefully when either is absent (`IOpcSecurity.SupportsWindowsAuthentication` / `IOpcSecurity.SupportsPrivateAuthentication` flags). |

Behavioural notes from spec prose (non-MUST but verified):

- **§4.2.1**: When a server implements both interfaces, an activated session may switch between NT credential mode and private credential mode. The reference sample dispatcher tracks the current mode per `IRpcConnection` slot.
- **§4.3.3** (`ChangeUser` semantics): After `ChangeUser`, the server's reference monitor must re-check ACLs using the new token. The sample dispatcher invalidates per-session ACL caches on `ChangeUser` completion.
- **§4.4.3** (`Logoff` semantics): After `Logoff`, subsequent operations should return `OPC_E_PRIVATE_ACTIVE` until a new `Logon` succeeds. The sample dispatcher honours this.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Informative DCOM-security guidelines deferred

Spec §6.3 (DCOM Security Setup, In-Process Server Considerations,
Local/Remote Server Configuration Parameters, Win9x guidance) is
informative documentation aimed at server implementers writing native
C++ COM components. The Opc.Classic equivalents are captured by
`docs/cookbook/08-implementing-opc-security.md`,
`docs/cookbook/07-enabling-packet-privacy.md`,
`docs/security/THREAT_MODEL.md`, and the auth modules under
`src/Opc.Classic.Dcom/rpc/Auth/`. Status: **WAIVED** — informative, not
normative; cross-platform equivalents documented.

#### 3.1.2 No installer / `regsvr32`-equivalent for sample security server

Spec §5.1 lists registry entries an OPC Security server should write at
install time. The reference sample registers programmatically when
launched with `-Embedding`, but no .msi / WiX installer is shipped.
This is the same deferred-by-design choice tracked under OPC-COMMON-1.10
§3.1.2. Status: **WAIVED**.

### 3.2 Hard gaps

None at present. All required interfaces, methods, IIDs, opnums, and
HRESULTs are implemented and tested; the reference sample server
exists; the cross-impl matrix `security-da` profile is green.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC Security 1.00](../CONFORMANCE.md#opc-security-100)
- Implementation cookbook: [`docs/cookbook/08-implementing-opc-security.md`](../cookbook/08-implementing-opc-security.md)
- Threat model and DCOM-layer auth: [`docs/security/THREAT_MODEL.md`](../security/THREAT_MODEL.md)
- Packet privacy enablement: [`docs/cookbook/07-enabling-packet-privacy.md`](../cookbook/07-enabling-packet-privacy.md)
- Channel binding: [`docs/security/CHANNEL_BINDING.md`](../security/CHANNEL_BINDING.md)
- Related OPC-COMMON §5 error-code ranges: [`opc-common-1-10.md`](opc-common-1-10.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-SECURITY-1.00.md` (OPC Security
Custom Interface Specification 1.00, October 17, 2000).

Phase 0 inventory:

- `files/conformance/inventory/opc-security-1-00-headings.csv` (58 entries)
- `files/conformance/inventory/opc-security-1-00-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-security-1-00-interfaces.csv` (8 interface + 10 method references)
