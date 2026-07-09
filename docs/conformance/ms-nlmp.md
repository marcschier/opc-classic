# MS-NLMP (NTLM Authentication) conformance review

**Spec:** `opc-classic-docs/MS-NLMP.md` (NT LAN Manager (NTLM) Authentication Protocol).

**Scope:** The full NTLMv2 three-message authentication handshake (`NEGOTIATE_MESSAGE`, `CHALLENGE_MESSAGE`, `AUTHENTICATE_MESSAGE`), key derivation (LM v2 response, NT v2 response, session key, signing keys, sealing keys, sequence-number keys), AV-pair encoding, MIC field calculation + verification, channel-bindings hash, target-info construction, and the per-PDU `NTLMSSP_MESSAGE_SIGNATURE` block. Both client + server roles are implemented, including configured managed-listener NTLMv2 bind accept; per-spec connection-oriented mode is in scope (datagram NTLM is not implemented and is out of scope for DCOM).

**Implementing assemblies:** `Opc.Classic.Dcom` (`rpc/Auth/Ntlm*.cs` for the auth-trailer flow, `Common/Ntlm/*.cs` for the wire-message + MIC + key infrastructure), `Opc.Classic.Dcom.Crypto` (MD4, RC4, MD5, HMAC), `Opc.Classic.Core/Security/ChannelBindingsHash.cs` (EXTENDED_BINDING MD5 for the `gss_channel_bindings_struct`).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `NEGOTIATE_MESSAGE` (Type-1) | §2.2.1.1 | ✅ `NtlmMessage` | ✅ `NtlmHandshakeFixtureTests`, `NtlmHandshakeProtocolTests` | conformant |
| `CHALLENGE_MESSAGE` (Type-2) | §2.2.1.2 | ✅ `NtlmMessage` + `NtlmAvPairs` | ✅ same | conformant |
| `AUTHENTICATE_MESSAGE` (Type-3) | §2.2.1.3 | ✅ `NtlmMessage` + `NtlmAvPairs` + `NtlmMic` | ✅ same | conformant |
| NEGOTIATE_FLAGS (`NTLMSSP_NEGOTIATE_*`) | §2.2.2.5 | ✅ `NtlmFlags` | ✅ `NtlmNegotiateFlagsTests` | conformant |
| AV_PAIR (`MsvAv*`) encoding | §2.2.2.1 | ✅ `NtlmAvPairs` (incl. `MsvAvFlags`, `MsvAvTimestamp`, `MsvAvChannelBindings`, `MsvAvTargetName`) | ✅ | conformant |
| LMOWF v2 / NTOWF v2 (NTLMv2 key derivation) | §3.3.2 | ✅ `NTLMKeyFactory` | ✅ `NtlmV2ServerKeyDerivationTests` | conformant |
| NTLM v2 response (`NTLMv2_RESPONSE`, `NTLMv2_CLIENT_CHALLENGE`) | §2.2.2.7 / §2.2.2.8 | ✅ `NtlmAuthentication` | ✅ `NtlmHandshakeFixtureTests` | conformant |
| LM v2 response (`LMv2_RESPONSE`) | §2.2.2.4 | ✅ same | ✅ | conformant |
| Session base key + key exchange key | §3.4.5.1 / §3.4.5.2 | ✅ `NTLMKeyFactory` + `NtlmAuthentication` | ✅ | conformant |
| Encrypted random session key (`EncryptedRandomSessionKey`) | §3.1.5.1.2 | ✅ `NtlmAuthentication` (RC4-encrypted) | ✅ | conformant |
| Sign / seal sub-keys (`SIGNKEY` / `SEALKEY` / sequence numbers for client → server + server → client) | §3.4.5.2 / §3.4.5.3 | ✅ `NTLMKeyFactory.cs` + `NtlmConnection.cs` | ✅ `NtlmSignatureBlockTests`, `NtlmPassiveUnwrapperTests` | conformant |
| `NTLMSSP_MESSAGE_SIGNATURE` (per-PDU signing) | §2.2.2.9 | ✅ `NtlmMessageSignature` | ✅ `NtlmSignatureBlockTests` | conformant |
| MIC field (`MessageIntegrityCheck`) | §3.1.5.1.2 | ✅ `NtlmMic`, `NtlmMicProvider` | ✅ `NtlmMicTests` | conformant |
| Channel-bindings hash (`MD5(EXTENDED_BINDING)`) | §2.2.2.1 (AvPair `MsvAvChannelBindings = 0x000A`) | ✅ `ChannelBindingsHash` (in `Opc.Classic.Core/Security/`) | ✅ `ChannelBindingsTests`, `ChannelBindingTlsTests` | conformant |
| Channel-bindings token: `tls-server-end-point` prefix | RFC 5929 (referenced) | ✅ `ChannelBindingsFactory` | ✅ same | conformant |
| Server: `CHALLENGE_MESSAGE` synthesis with TargetInfo + ServerChallenge | §3.2.5.1.1 | ✅ `NtlmConnectionContext`, `ComRuntimeNTLMConnectionContext`, `ConfiguredAuthenticationSource` + `RpcServerConnectionProcessor` | ✅ `NtlmHandshakeProtocolTests`, `F4Auth` | conformant |
| Server: `AUTHENTICATE_MESSAGE` verification + reproducing client's session key | §3.2.5.1.2 | ✅ same | ✅ same, including wrong-password and anonymous-bypass rejection | conformant |
| Anonymous authentication (`NTLMSSP_ANONYMOUS` flow) | §3.2.5.1.2 | ✅ `NtlmAuthentication` (anonymous path) | ✅ `NtlmDefaultsTests` | conformant |
| Datagram NTLM (`SECPKG_CONTEXT_FLAG_ALWAYS_*`) | §3.4.5.4 | ❌ not implemented | n/a | deferred-by-design (DCOM is connection-oriented) |

---

## 1 Surface-by-surface coverage matrix

### 1.1 NTLMSSP messages (spec §2.2.1)

| Message | Spec § | Source | Tests |
|---|---|---|---|
| `NEGOTIATE_MESSAGE` | §2.2.1.1 | `src/Opc.Classic.Dcom/Common/Ntlm/NtlmMessage.cs` | `tests/Opc.Classic.Dcom.Tests/NtlmHandshakeFixtureTests.cs`, `NtlmHandshakeProtocolTests.cs` |
| `CHALLENGE_MESSAGE` | §2.2.1.2 | same | same |
| `AUTHENTICATE_MESSAGE` | §2.2.1.3 | same | same |
| Header `Signature` ("NTLMSSP\0") + `MessageType` | §2.2.1 | same | same |

### 1.2 NEGOTIATE_FLAGS (spec §2.2.2.5)

| Source | Tests |
|---|---|
| `src/Opc.Classic.Dcom/Common/Ntlm/NtlmFlags.cs` | `tests/Opc.Classic.Dcom.Tests/NtlmNegotiateFlagsTests.cs` |

All 26 spec-defined flags are declared with canonical names + values
(e.g. `NTLMSSP_NEGOTIATE_UNICODE = 0x00000001`,
`NTLMSSP_NEGOTIATE_OEM = 0x00000002`,
`NTLMSSP_NEGOTIATE_SIGN = 0x00000010`,
`NTLMSSP_NEGOTIATE_SEAL = 0x00000020`,
`NTLMSSP_NEGOTIATE_NTLM = 0x00000200`,
`NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY = 0x00080000`,
`NTLMSSP_NEGOTIATE_VERSION = 0x02000000`,
`NTLMSSP_NEGOTIATE_128 = 0x20000000`,
`NTLMSSP_NEGOTIATE_KEY_EXCH = 0x40000000`,
`NTLMSSP_NEGOTIATE_56 = 0x80000000`, etc.).

### 1.3 AV_PAIR (spec §2.2.2.1)

| AvId | Name | Source | Notes |
|---|---|---|---|
| `MsvAvEOL` (0x00) | end-of-list terminator | `NtlmAvPairs.cs` | |
| `MsvAvNbComputerName` (0x01) | server NetBIOS computer | same | UTF-16LE |
| `MsvAvNbDomainName` (0x02) | server NetBIOS domain | same | UTF-16LE |
| `MsvAvDnsComputerName` (0x03) | server DNS computer | same | UTF-16LE |
| `MsvAvDnsDomainName` (0x04) | server DNS domain | same | UTF-16LE |
| `MsvAvDnsTreeName` (0x05) | forest DNS tree | same | UTF-16LE |
| `MsvAvFlags` (0x06) | per-message flags (MIC present, target-info length, etc.) | same | |
| `MsvAvTimestamp` (0x07) | server time-of-challenge | same | FILETIME |
| `MsvAvSingleHost` (0x08) | client SingleHost structure | same | |
| `MsvAvTargetName` (0x09) | target SPN (UTF-16LE) | same | used by `MsvAvFlags` "target SPN check" bit |
| `MsvAvChannelBindings` (0x0A) | MD5 hash of `gss_channel_bindings_struct` | same + `ChannelBindingsHash` | hash is MD5(EXTENDED_BINDING) per spec §3.1.5.1.2 |

### 1.4 Key derivation (spec §3.3.2 / §3.4.5)

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| `NTOWFv2(Passwd, User, UserDom)` | §3.3.2 | `src/Opc.Classic.Dcom/rpc/Auth/NTLMKeyFactory.cs` | `NtlmV2ServerKeyDerivationTests.cs` |
| `LMOWFv2 == NTOWFv2` | §3.3.2 | same | same |
| `NTLMv2_RESPONSE = HMAC-MD5(NTOWFv2, ServerChallenge \|\| temp)` | §3.3.2 | `NtlmAuthentication.cs` | `NtlmHandshakeFixtureTests` |
| `LMv2_RESPONSE = HMAC-MD5(NTOWFv2, ServerChallenge \|\| ClientChallenge)` | §3.3.2 | same | same |
| `SessionBaseKey = HMAC-MD5(NTOWFv2, NTProofStr)` | §3.4.5.1 | `NTLMKeyFactory.cs` | `NtlmV2ServerKeyDerivationTests` |
| `KeyExchangeKey == SessionBaseKey` (NTLMv2 with NEGOTIATE_EXTENDED_SESSIONSECURITY) | §3.4.5.1 | same | same |
| `RandomSessionKey` + encryption with KeyExchangeKey via RC4 | §3.1.5.1.2 | `NtlmAuthentication.cs` + RC4 from `Opc.Classic.Dcom.Crypto` | same |
| Sign sub-key derivation (`SIGNKEY = MD5(ExportedSessionKey \|\| "session key to client-to-server signing key magic constant\0")`) | §3.4.5.2 | `NTLMKeyFactory.cs` | `NtlmSignatureBlockTests` |
| Seal sub-key derivation (analogous) | §3.4.5.3 | same | same |

### 1.5 MIC field (spec §3.1.5.1.2)

The `AUTHENTICATE_MESSAGE` carries an optional 16-byte MIC at offset
72 (immediately before the LM response data). MIC =
`HMAC-MD5(ExportedSessionKey, NEGOTIATE_MESSAGE \|\| CHALLENGE_MESSAGE \|\| AUTHENTICATE_MESSAGE)` with the MIC field zeroed during calculation.

| Surface | Source | Tests |
|---|---|---|
| MIC compute + verify | `src/Opc.Classic.Dcom/Common/Ntlm/NtlmMic.cs`, `NtlmMicProvider.cs` | `tests/Opc.Classic.Dcom.Tests/NtlmMicTests.cs` |

### 1.6 NTLMSSP_MESSAGE_SIGNATURE (per-PDU, spec §2.2.2.9)

`NTLMSSP_MESSAGE_SIGNATURE` (16 bytes: `Version=1` + `Checksum(8)` + `SeqNum(4)`) is computed per outbound PDU using `SIGNKEY` + `SealingHandle` + `SeqNum`. Per-direction sequence numbers prevent replay.

| Surface | Source | Tests |
|---|---|---|
| Signature encode / decode + verify | `src/Opc.Classic.Dcom/Common/Ntlm/NtlmMessageSignature.cs` + `NtlmConnection.cs` | `tests/Opc.Classic.Dcom.Tests/NtlmSignatureBlockTests.cs`, `NtlmPassiveUnwrapperTests.cs` |

### 1.7 Channel-bindings token (spec §3.1.5.1.2 — AvPair 0x0A)

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| EXTENDED_BINDING structure construction | §3.1.5.1.2 + RFC 4121 | `src/Opc.Classic.Core/Security/ChannelBindingsHash.cs` | `tests/Opc.Classic.Core.Tests/Security/ChannelBindingsTests.cs` |
| `tls-server-end-point:` prefix selection | RFC 5929 §4.1 | `src/Opc.Classic.Core/Security/ChannelBindingsFactory.cs` | same |
| MD5 hash | §3.1.5.1.2 | `ChannelBindingsHash.cs` | same |
| Insertion into `AUTHENTICATE_MESSAGE`'s `MsvAvChannelBindings` av-pair | §3.1.5.1.2 | `NtlmAvPairs.cs` + `NtlmAuthentication.cs` | covered by `NtlmHandshakeProtocolTests` |
| Server-side verification | §3.2.5.1.2 | `NtlmConnectionContext.cs`; managed listener via `ConfiguredAuthenticationSource` | covered by `NtlmHandshakeProtocolTests` and `F4Auth` |

### 1.8 Negotiate-Sign / Negotiate-Seal / Negotiate-Always-Sign defaults

| Flag default | Spec § | Source | Tests |
|---|---|---|---|
| `NTLMSSP_NEGOTIATE_SIGN` set by default | §3.1.1.1 | `src/Opc.Classic.Dcom/rpc/Auth/Ntlm1.cs` + `NtlmAuthentication.cs` | `NtlmDefaultsTests.cs` |
| `NTLMSSP_NEGOTIATE_SEAL` honoured when protection level >= PKT_PRIVACY | §3.1.1.1 | same | same |
| `NTLMSSP_NEGOTIATE_KEY_EXCH` set by default | §3.1.5.1.1 | same | same |
| `NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY` mandatory | §3.1.5.1.1 | same | same |
| `NTLMSSP_NEGOTIATE_128` set by default | §3.1.5.1.1 | same | same |
| `NTLMSSP_NEGOTIATE_56` set by default | §3.1.5.1.1 | same | same |
| `NTLMSSP_NEGOTIATE_VERSION` set by default | §3.1.1.1 | same | same |

### 1.9 Connection-context plumbing

| Surface | Source | Tests |
|---|---|---|
| `ComRuntimeNTLMConnectionContext` (wires NTLM into the COM-runtime auth-context surface) | `src/Opc.Classic.Dcom/Transport/ComRuntimeNTLMConnectionContext.cs` | `tests/Opc.Classic.Dcom.Tests/NtlmHandshakeProtocolTests.cs` |
| `NtlmConnection` (per-call sequence numbers + per-direction sealing handles) | `src/Opc.Classic.Dcom/rpc/Auth/NtlmConnection.cs` | `NtlmSignatureBlockTests.cs` |
| `NtlmConnectionContext` (legacy handshake-state machine) | `src/Opc.Classic.Dcom/rpc/Auth/NtlmConnectionContext.cs` | `NtlmHandshakeProtocolTests.cs` |
| `ConfiguredAuthenticationSource` + `RpcServerConnectionProcessor` (managed-listener NTLM acceptor) | `src/Opc.Classic.Dcom/rpc/Auth/ConfiguredAuthenticationSource.cs`, `src/Opc.Classic.Dcom/Transport/RpcServerConnectionProcessor.cs` | `F4Auth.cs` |
| `Ntlm1` (NTLMv1 minimal fallback, gated for legacy peers only) | `src/Opc.Classic.Dcom/rpc/Auth/Ntlm1.cs` | covered by `NtlmDefaultsTests` |
| `NtlmAuthentication` (top-level orchestrator) | `src/Opc.Classic.Dcom/rpc/Auth/NtlmAuthentication.cs` | `NtlmHandshakeFixtureTests` |
| `NTLMKeyFactory` (NTOWFv2, sign / seal key derivation) | `src/Opc.Classic.Dcom/rpc/Auth/NTLMKeyFactory.cs` | `NtlmV2ServerKeyDerivationTests.cs` |
| Replay detection (server-side sequence number monotonicity) | `NtlmConnection.cs` + `NtlmPassiveUnwrapperTests.cs` | `NtlmPassiveUnwrapperTests` |

### 1.10 Fuzz / property tests

| Surface | Source |
|---|---|
| Message-shape fuzz | `tests/Opc.Classic.Dcom.Tests/NtlmFuzzTests.cs` |
| Wire-byte fixture replay | `tests/Opc.Classic.Dcom.Tests/NtlmHandshakeFixtureTests.cs` |

---

## 2 Normative-clause checklist

MS-NLMP contains **204 MUST/SHALL clauses** per Phase 0 inventory
(`ms-nlmp-clauses.csv`). §-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 12 | ✅ informative | n/a |
| §2.2.1 | Three message formats | 38 | ✅ conformant | §1.1 |
| §2.2.2 | Common structures (AV_PAIR, NEGOTIATE_FLAGS, etc.) | 51 | ✅ conformant | §1.2 - §1.3 |
| §3.1 - 3.3 | Common + client + server data models | 65 | ✅ conformant | §1.4 - §1.9 |
| §3.4 | Key derivation + signing + sealing | 31 | ✅ conformant | §1.4 - §1.6 |
| §5 | Security considerations | 7 | ✅ documented in `docs/security/THREAT_MODEL.md` | n/a |

Phase 2 deep-validation will pin each clause individually.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Datagram NTLM not implemented

Spec §3.4.5.4 specifies datagram-mode NTLM. DCOM is exclusively
connection-oriented; datagram NTLM has no consumer in the OPC stack.
Status: **WAIVED** — deferred-by-design.

#### 3.1.2 NTLMv1 only available as legacy fallback

`Ntlm1.cs` retains a minimal NTLMv1 path for interop with legacy
peers that pre-date NTLMv2. The default flow is NTLMv2-only;
NTLMv1 is gated by explicit caller opt-in. Status: **WAIVED** —
matches the OPC + Windows recommended security stance.

#### 3.1.3 LMv1 / NTLMv1 session-security paths not implemented

`NTLMSSP_REVISION_W2K3` (v1 session security with key exchange) and
the older LMv1 hash are intentionally not implemented. Status:
**WAIVED** — these are obsolete and Microsoft itself disables them
by default since Windows 7.

#### 3.1.4 Anonymous authentication has limited test surface

`NtlmDefaultsTests` covers the basic anonymous flow; comprehensive
property tests for all anonymous-mode flag combinations would
strengthen the surface. Status: **WAIVED** (deferred — backlog
item).

### 3.2 Hard gaps

None at present. NTLMv2 three-message handshake, AV-pair encoding
(including `MsvAvChannelBindings`), key derivation, MIC verification,
signing, sealing, replay detection, target-info construction, and the
configured managed-listener server acceptor conform to MS-NLMP. The
cross-implementation matrix exercises the full handshake against managed
peers and external-server profiles; passive NTLM-trailer unwrap is verified
against captured Windows native PDUs in `NtlmPassiveUnwrapperTests` and the
[`docs/capture/ntlm-unwrap.md`](../capture/ntlm-unwrap.md) playbook.

---

## 4 Cross-references

- Security threat model: [`docs/security/THREAT_MODEL.md`](../security/THREAT_MODEL.md)
- NTLM-SSP audit guide: [`docs/security/NTLMSSP_AUDIT_GUIDE.md`](../security/NTLMSSP_AUDIT_GUIDE.md)
- Channel binding: [`docs/security/CHANNEL_BINDING.md`](../security/CHANNEL_BINDING.md)
- Capture / unwrap playbook: [`docs/capture/ntlm-unwrap.md`](../capture/ntlm-unwrap.md)
- Tutorial: [`docs/tutorials/04-security-with-kerberos-and-channel-binding.md`](../tutorials/04-security-with-kerberos-and-channel-binding.md)
- Related spec: [`docs/conformance/ms-rpce.md`](ms-rpce.md) — auth-trailer carrier in connection-oriented RPCE PDUs.
- Related spec: [`docs/conformance/ms-spng.md`](ms-spng.md) — SPNEGO wraps NTLM as one mech option.
- Related spec: [`docs/conformance/ms-kile.md`](ms-kile.md) — Kerberos is the alternative selected via SPNEGO.
- Related spec: [`docs/conformance/ms-cssp.md`](ms-cssp.md) — channel-bindings hash citation.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-NLMP.md` (Microsoft Open
Specifications MS-NLMP: NT LAN Manager (NTLM) Authentication Protocol).

Phase 0 inventory:

- `files/conformance/inventory/ms-nlmp-headings.csv` (134 entries)
- `files/conformance/inventory/ms-nlmp-clauses.csv` (204 normative entries)
