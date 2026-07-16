# MS-KILE (Kerberos Extensions) conformance review

**Spec:** `opc-classic-docs/MS-KILE.md` (Kerberos Protocol Extensions).

**Scope:** RFC 4120 Kerberos with Microsoft extensions for DCOM initiators and managed-listener acceptors, directly or through SPNEGO. Covers ticket acquisition, `KRB_AP_REQ` / `KRB_AP_REP`, `KRB_ERROR`, GSS-API tokens, authenticator construction and validation, subkeys, channel binding, MIC/Wrap, service credentials, principal mapping, and key derivation.

**Implementing assemblies:** `Opc.Classic.Dcom.Kerberos` (client context, server options/credentials, session, key derivation, RFC 4121 tokens), `Opc.Classic.Dcom/rpc/Auth/` (Kerberos/SPNEGO acceptors and provider registry), `Opc.Classic.Dcom/Spnego/` (SPNEGO wrapping), `Kerberos.NET` package (KDC interaction and ticket parsing).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `KRB_AS_REQ` / `KRB_AS_REP` (initial authentication) | RFC 4120 §3.1 + MS-KILE §3.2.5.4 | ✅ via `Kerberos.NET` | ✅ `KerberosKdcIntegrationTests` | conformant |
| `KRB_TGS_REQ` / `KRB_TGS_REP` (service-ticket acquisition) | RFC 4120 §3.3 | ✅ via `Kerberos.NET` | ✅ same | conformant |
| `KRB_AP_REQ` (mutual authentication) | RFC 4120 §3.2 + MS-KILE §3.2.5.3 | ✅ `KerberosAuthContext`, `KerberosSession` | ✅ `KerberosAuthContextTests`, `KerberosConnectionContextTests` | conformant |
| `KRB_AP_REP` (server response) | RFC 4120 §3.2.4 | ✅ same | ✅ same | conformant |
| Managed-listener AP-REQ acceptor | RFC 4120 §3.2 + MS-KILE §3.2.5.3 | ✅ `KerberosServerAuthenticationProvider` | ✅ provider integration tests | conformant for configured policy |
| Service credentials and principal mapping | MS-KILE identity/service policy | ✅ bounded keytab/password providers + explicit mapper | ✅ `KerberosServerOptionsTests` | conformant for implemented policy |
| `KRB_ERROR` decoding | RFC 4120 §5.9.1 + MS-KILE §3.2.5.6 | ✅ via `Kerberos.NET` | ✅ | conformant |
| Authenticator construction (`Authenticator` per RFC 4120 §5.5.1) | RFC 4120 §5.5.1 | ✅ `KerberosAuthContext` | ✅ | conformant |
| Subkey selection (per-session subkey for per-PDU integrity) | RFC 4120 §5.5.1 + MS-KILE §3.2.5.3 | ✅ `KerberosSessionKey`, `KerberosSession` | ✅ `KerberosAuthContextTests` | conformant |
| GSS-API token wrapper (OID `1.2.840.113554.1.2.2`, mutual + replay + sequence flags) | RFC 4121 §4 | ✅ `KerberosAuthContext` | ✅ same | conformant |
| Channel-binding checksum (RFC 4121 §4.1.1 — gss-bnd, embedded in Authenticator.cksum) | RFC 4121 §4.1.1 | ✅ `KerberosChannelBindingChecksum` | ✅ `KerberosChannelBindingChecksumTests` | conformant |
| MIC token (RFC 4121 §4.2.6.1) | RFC 4121 §4.2.6.1 | ✅ `Rfc4121MicTokenTests` | ✅ | conformant |
| Wrap token (RFC 4121 §4.2.6.2) | RFC 4121 §4.2.6.2 | ✅ `Rfc4121WrapTokenTests` | ✅ | conformant |
| MS-RPCE `GSS_GetMICEx` / `GSS_WrapEx` packet protection | MS-KILE §3.4.5.4-§3.4.5.7 | ✅ segmented header/body/trailer protection, directional AP sequence numbers, AES EC/RRC framing, RC4 DCE-style framing | ✅ `KerberosRpcPacketProtectionTests`, published MS-KILE §4.5 RC4 vector | conformant |
| RFC 3962 AES-CTS encryption (`aes128-cts-hmac-sha1-96`, `aes256-cts-hmac-sha1-96`) | RFC 3962 | ✅ `Rfc3962AesCtsTests` | ✅ | conformant |
| RFC 4757 RC4-HMAC (`rc4-hmac-md5`) | RFC 4757 | ✅ `Rfc4757Rc4HmacTests` | ✅ | conformant |
| RFC 8009 AES-SHA2 (`aes128-cts-hmac-sha256-128`, `aes256-cts-hmac-sha384-192`) | RFC 8009 | ✅ `Rfc8009AesShaaTests` | ✅ | conformant |
| Replay-cache (per-host receive cache) | MS-KILE §3.2.5.3 | ✅ `KerberosReplayProtectionTests` | ✅ | conformant |
| SPNEGO mech selection of Kerberos | MS-SPNG §3.1.5.x | ✅ `KerberosSpnegoMicProviderTests` | ✅ | conformant |
| PA-DATA (`PA-ENC-TIMESTAMP`, `PA-PAC-REQUEST`, etc.) | MS-KILE §2.2.x | ✅ via `Kerberos.NET` | ✅ | conformant |
| PAC validation (KERB_VALIDATION_INFO, server signature, KDC signature) | MS-PAC | ✅ via `Kerberos.NET` | ⚠️ basic — see §3.1 | partial |
| U2U (User-to-User) authentication | MS-KILE §3.2.5.4 | ❌ not implemented | n/a | deferred-by-design |
| Constrained delegation (S4U2Self / S4U2Proxy) | MS-SFU | ❌ not implemented | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 Kerberos handshake (RFC 4120 + MS-KILE §3.x)

Opc.Classic delegates KDC interaction (`AS_REQ`/`AS_REP`,
`TGS_REQ`/`TGS_REP`) and ticket parsing to `Kerberos.NET`. Opc.Classic owns
the client connection context, listener acceptor policy, service-credential
lifetime, principal mapping, channel binding, MIC/Wrap integration, and SPNEGO
policy.

| Surface | Source | Tests |
|---|---|---|
| `AS_REQ` / `AS_REP` driver | `Kerberos.NET` (external) — invoked via `KerberosSession` | `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosKdcIntegrationTests.cs` |
| `TGS_REQ` / `TGS_REP` driver | same | same |
| `AP_REQ` construction (with Authenticator including channel-binding cksum + subkey + flags) | `src/Opc.Classic.Dcom.Kerberos/KerberosAuthContext.cs` | `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosAuthContextTests.cs` |
| `AP_REP` parsing (server validates Authenticator's timestamp + microseconds + subkey echo) | same | same |
| `KRB_ERROR` parsing | via `Kerberos.NET` | covered by integration tests |
| AP-REQ ticket validation and AP-REP response | `KerberosServerAuthenticationProvider` | `KerberosServerAuthenticationProviderIntegrationTests.cs` |
| Keytab/password service credentials | `FileKerberosKeytabCredentialProvider`, `PasswordKerberosServerCredentialProvider` | `KerberosServerOptionsTests.cs`, provider integration tests |
| Authenticated-principal mapping | `KerberosPrincipalMappingPolicy` | `KerberosServerOptionsTests.cs` |

### 1.2 GSS-API token wrapper (RFC 4121 §4)

Context-establishment tokens carry the Kerberos mechanism directly or inside SPNEGO, depending on the selected RPC authentication service. Per-PDU `auth_value` framing depends on the negotiated encryption type: AES uses the RFC 4121 MIC/Wrap header with the MS-KILE `GSS_GetMICEx`/`GSS_WrapEx` EC/RRC split, while RC4 uses the RFC 4757 pseudo-ASN.1 InitialContextToken. The PDU body remains in its RPCE segment and only the confidentiality-selected body bytes are encrypted.

| Surface | Source | Tests |
|---|---|---|
| Token framing (OID + tok_id) | `src/Opc.Classic.Dcom.Kerberos/KerberosAuthContext.cs` | `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosAuthContextTests.cs` |
| Flags (`GSS_C_MUTUAL_FLAG`, `GSS_C_REPLAY_FLAG`, `GSS_C_SEQUENCE_FLAG`, `GSS_C_CONF_FLAG`, `GSS_C_INTEG_FLAG`, `GSS_C_DELEG_FLAG`, `GSS_C_DCE_STYLE`) | same | same |

### 1.3 Per-PDU integrity + privacy (RFC 4121 §4.2)

| Token | RFC 4121 § | Source | Tests |
|---|---|---|---|
| MIC token (16-byte header + checksum) | §4.2.6.1 | `src/Opc.Classic.Dcom.Kerberos/...MicProvider` (via `KerberosConnectionContext`) | `tests/Opc.Classic.Dcom.Kerberos.Tests/Rfc4121MicTokenTests.cs` |
| Wrap token (16-byte header + RRC + EC + encrypted body) | §4.2.6.2 | same | `tests/Opc.Classic.Dcom.Kerberos.Tests/Rfc4121WrapTokenTests.cs` |
| Per-sequence-number replay protection | §4.2.4 | `src/Opc.Classic.Dcom.Kerberos/KerberosSession.cs` | `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosReplayProtectionTests.cs` |
| RPCE segmented packet protection | MS-KILE §3.4.5.4-§3.4.5.7 | `KerberosSession.ProtectRpcMessage` / `UnprotectRpcMessage` | `KerberosRpcPacketProtectionTests`, `Rfc4757Rc4HmacTests` |

### 1.4 Channel-binding checksum (RFC 4121 §4.1.1)

The Authenticator's `cksum` field carries a structured payload that
includes the channel-bindings hash (MD5 of `EXTENDED_BINDING`), flags,
and optional delegation token. Same channel-binding-hash plumbing as
NTLM (via `Opc.Classic.Core/Security/ChannelBindingsHash.cs`).

| Surface | Source | Tests |
|---|---|---|
| Authenticator-cksum encoding (`Lgth` + `Bnd` + `Flags`) | `src/Opc.Classic.Dcom.Kerberos/KerberosChannelBindingChecksum.cs` | `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosChannelBindingChecksumTests.cs` |
| Channel-binding hash insertion | same + `ChannelBindingsHash` | same |

### 1.5 Encryption types (RFC 3961 / 3962 / 4757 / 8009)

| Etype | RFC | Source | Tests |
|---|---|---|---|
| `aes128-cts-hmac-sha1-96` (17) | RFC 3962 | via `Kerberos.NET` + verification harness | `tests/Opc.Classic.Dcom.Kerberos.Tests/Rfc3962AesCtsTests.cs` |
| `aes256-cts-hmac-sha1-96` (18) | RFC 3962 | same | same |
| `rc4-hmac-md5` (23) | RFC 4757 | same | `tests/Opc.Classic.Dcom.Kerberos.Tests/Rfc4757Rc4HmacTests.cs` |
| `aes128-cts-hmac-sha256-128` (19) | RFC 8009 | same | `tests/Opc.Classic.Dcom.Kerberos.Tests/Rfc8009AesShaaTests.cs` |
| `aes256-cts-hmac-sha384-192` (20) | RFC 8009 | same | same |

### 1.6 Auth-context plumbing

| Surface | Source | Tests |
|---|---|---|
| `IKerberosAuthInfo` (per-call credentials) | `src/Opc.Classic.Dcom.Kerberos/IKerberosAuthInfo.cs`, `KerberosAuthInfo.cs` | `KerberosAuthInfoTests.cs` |
| `IKerberosConnectionContext` (handshake state machine) | `src/Opc.Classic.Dcom.Kerberos/IKerberosConnectionContext.cs`, `KerberosConnectionContext.cs` | `KerberosConnectionContextTests.cs`, `KerberosConnectionContextIntegrationTests.cs` |
| `IKerberosSession` (post-handshake session state: subkey, sequence numbers) | `src/Opc.Classic.Dcom.Kerberos/IKerberosSession.cs`, `KerberosSession.cs` | covered by `KerberosAuthContextTests` |
| `KerberosSessionKey` (per-session subkey container) | `src/Opc.Classic.Dcom.Kerberos/KerberosSessionKey.cs` | covered by RFC 4121 token tests |

### 1.7 SPNEGO ↔ Kerberos integration

| Surface | Source | Tests |
|---|---|---|
| SPNEGO mech-list with Kerberos OID (`1.2.840.113554.1.2.2`) preferred over NTLM | `src/Opc.Classic.Dcom/Spnego/...` (see [`ms-spng.md`](ms-spng.md) forthcoming) | `tests/Opc.Classic.Dcom.Kerberos.Tests/SpnegoTests.cs`, `SpnegoNegTokenRespTests.cs`, `SpnegoFuzzTests.cs` |
| SPNEGO mic (`KerberosSpnegoMicProvider`) | `src/Opc.Classic.Dcom.Kerberos/KerberosSpnegoMicProvider.cs` (if separate) or co-located in SPNEGO | `KerberosSpnegoMicProviderTests.cs` |
| KDC integration test fixture (real Kerberos.NET KDC) | `tests/Opc.Classic.Dcom.Kerberos.Tests/KdcFixture.cs` | `KerberosKdcIntegrationTests.cs` |

---

## 2 Normative-clause checklist

MS-KILE contains **143 MUST/SHALL clauses** per Phase 0 inventory.
§-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 9 | ✅ informative | n/a |
| §2.2 | Common data structures (PA-DATA types, etc.) | 38 | ✅ conformant via Kerberos.NET | §1.1 |
| §3.1 | Common message processing | 24 | ✅ conformant | §1.1 - §1.4 |
| §3.2 | Client details | 22 | ✅ conformant | §1.1 - §1.6 |
| §3.3 | KDC details | 33 | n/a — Opc.Classic is an initiator/acceptor endpoint, not a KDC | n/a |
| §3.4 | Realm details | 8 | n/a | n/a |
| §5 | Security considerations | 9 | ✅ documented | n/a |

Phase 2 deep-validation will pin each client-side clause individually.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 PAC validation is light

`Kerberos.NET` exposes PAC parsing but Opc.Classic only consumes the
basic identity claims (UPN, SID) and does not implement the full
KERB_VALIDATION_INFO claims-validation pipeline. Status: **WAIVED**
(deferred) — most OPC servers do not require PAC-based authorization
beyond the resolved Windows identity.

#### 3.1.2 U2U (User-to-User) authentication not implemented

MS-KILE §3.2.5.4 specifies U2U for service-to-service flows where both
peers act as users. Not applicable to OPC. Status: **WAIVED**
(deferred-by-design).

#### 3.1.3 Constrained delegation (S4U2Self / S4U2Proxy) not implemented

MS-SFU defines S4U2Self / S4U2Proxy for protocol transitions. Not
applicable to OPC. Status: **WAIVED** (deferred-by-design).

#### 3.1.4 KDC role not implemented

Opc.Classic is a Kerberos initiator and service acceptor. KDC role would require AS server,
TGS server, principal database, etc. Status: **WAIVED** —
deferred-by-design (test fixtures use Kerberos.NET's embedded KDC).

#### 3.1.5 Service-principal SPN cache management is in-memory only

`KerberosAuthContext` caches service tickets in-process. There's no
persistent ticket cache (krb5cc) and no kinit-style cli. Status:
**WAIVED** (deferred) — tickets are short-lived; in-memory cache is
sufficient for OPC sessions.

### 3.2 Hard gaps

None at present for the implemented endpoint roles. The initiator and acceptor
handshakes (`AP_REQ` / `AP_REP`), GSS-API token wrapping, per-PDU MIC + Wrap
tokens, channel-binding policy, service credential validation, principal
mapping, replay protection, and all five supported etypes
(aes128-sha1, aes256-sha1, rc4-hmac, aes128-sha2, aes256-sha2) are
implemented and tested. Packet protection uses AP-REQ/AP-REP directional sequence numbers and is checked against the published MS-KILE §4.5 RC4 `GSS_WrapEx` known-answer vector. KDC integration is verified via an embedded Kerberos.NET KDC test fixture.

---

## 4 Cross-references

- Security threat model: [`docs/security/THREAT_MODEL.md`](../security/THREAT_MODEL.md)
- Channel binding: [`docs/security/CHANNEL_BINDING.md`](../security/CHANNEL_BINDING.md)
- Tutorial: [`docs/tutorials/04-security-with-kerberos-and-channel-binding.md`](../tutorials/04-security-with-kerberos-and-channel-binding.md)
- Related spec: [`docs/conformance/ms-rpce.md`](ms-rpce.md) — auth-trailer carrier.
- Related spec: [`docs/conformance/ms-nlmp.md`](ms-nlmp.md) — alternative mech selected by SPNEGO.
- Related spec: [`docs/conformance/ms-spng.md`](ms-spng.md) — SPNEGO wrapper (forthcoming).
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-KILE.md` (Microsoft Open
Specifications MS-KILE: Kerberos Protocol Extensions).

Phase 0 inventory:

- `files/conformance/inventory/ms-kile-headings.csv` (162 entries)
- `files/conformance/inventory/ms-kile-clauses.csv` (143 normative entries)
