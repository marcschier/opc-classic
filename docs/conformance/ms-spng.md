# MS-SPNG (SPNEGO) conformance review

**Spec:** `opc-classic-docs/MS-SPNG.md` (Simple and Protected GSS-API Negotiation Mechanism).

**Scope:** RFC 4178 SPNEGO with Microsoft extensions. SPNEGO is the GSS-API negotiation layer that lets a client + server agree on Kerberos vs NTLM vs other mechs at connection time. Covers `NegTokenInit` (initial token from initiator), `NegTokenResp` (response tokens during back-and-forth negotiation), mech-list MIC (RFC 4178 §4.2.2 — proves which mech was selected), and the standard mech OIDs (`1.2.840.113554.1.2.2` Kerberos v5, `1.2.840.113554.1.2.2.3` Kerberos U2U, `1.3.6.1.4.1.311.2.2.10` NTLM, `1.3.6.1.5.5.2` SPNEGO itself).

**Implementing assemblies:** `Opc.Classic.Dcom.Kerberos/Spnego/` (encoder, decoder, token types, OID constants), `Opc.Classic.Dcom.Kerberos` (Kerberos MIC provider), and `Opc.Classic.Dcom/rpc/Auth/` (server negotiation provider and policy).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `NegTokenInit` (initiator → acceptor) | §2.2.1 (RFC 4178 §4.2.1) | ✅ `SpnegoNegTokenInit` + `SpnegoEncoder` / `SpnegoDecoder` | ✅ `SpnegoTests` | conformant |
| `NegTokenResp` (acceptor → initiator) | §2.2.2 (RFC 4178 §4.2.2) | ✅ `SpnegoNegTokenResp` + same encoder / decoder | ✅ `SpnegoNegTokenRespTests` | conformant |
| `negState` (`accept-completed`, `accept-incomplete`, `reject`, `request-mic`) | §2.2.2 (RFC 4178 §4.2.2) | ✅ `SpnegoNegState` | ✅ | conformant |
| Mech OIDs (Kerberos v5, NTLM, SPNEGO) | §1.4 (RFC 4178 §3.1) | ✅ `SpnegoOids` | ✅ | conformant |
| Mech selection and fallback policy | §3.1.5.1 | ✅ Kerberos-first; NTLM only when explicitly enabled and Kerberos is unavailable | ✅ `SpnegoServerAuthenticationProviderTests` | conformant |
| Mech-list MIC (RFC 4178 §4.2.2 + MS-SPNG §3.1.5.x) | §3.1.5.4 | ✅ `KerberosSpnegoMicProvider` + Kerberos MIC token | ✅ `KerberosSpnegoMicProviderTests` | conformant |
| Mech-list MIC mismatch handling (RFC 4178 §3) | §3.1.5.4 | ✅ rejected with `negState = reject` | ✅ same | conformant |
| `SupportedMech` field in NegTokenResp | §2.2.2 | ✅ `SpnegoNegTokenResp.SupportedMech` | ✅ | conformant |
| `MechToken` (opaque inner blob — Kerberos AP_REQ or NTLM message) | §2.2.1 / §2.2.2 | ✅ passthrough via `SpnegoMech` | ✅ | conformant |
| ASN.1 DER encoding (per RFC 4178) | §2.2 | ✅ `SpnegoEncoder` / `SpnegoDecoder` | ✅ `SpnegoFuzzTests` | conformant |
| MS-NEGOEX (Negotiate Extension) | MS-NEGOEX | ❌ not implemented | n/a | deferred-by-design |
| Managed-listener SPNEGO acceptor | §3.1.5.x | ✅ `SpnegoServerAuthenticationProvider` | ✅ unit + integration tests | conformant |

---

## 1 Surface-by-surface coverage matrix

### 1.1 SPNEGO ASN.1 token types (spec §2.2 / RFC 4178 §4.2)

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| `NegotiationToken` ASN.1 choice | RFC 4178 §4.2 | `src/Opc.Classic.Dcom.Kerberos/Spnego/SpnegoEncoder.cs`, `SpnegoDecoder.cs` | `tests/Opc.Classic.Dcom.Kerberos.Tests/SpnegoTests.cs` |
| `NegTokenInit` SEQUENCE (`mechTypes`, `reqFlags`, `mechToken`, `mechListMIC`) | RFC 4178 §4.2.1 | `SpnegoNegTokenInit.cs` | same |
| `NegTokenResp` SEQUENCE (`negState`, `supportedMech`, `responseToken`, `mechListMIC`) | RFC 4178 §4.2.2 | `SpnegoNegTokenResp.cs` | `SpnegoNegTokenRespTests.cs` |
| `NegState` ENUMERATED (`accept-completed=0`, `accept-incomplete=1`, `reject=2`, `request-mic=3`) | RFC 4178 §4.2.2 | `SpnegoNegState.cs` | covered by both above |
| GSS-API token wrapper (OID `1.3.6.1.5.5.2`) | RFC 2743 + RFC 4178 §4.2 | `SpnegoTokenBuilder.cs` | `SpnegoTests` |

### 1.2 Mech OIDs (spec §1.4 / RFC 4178 §3.1)

| Mech | OID | Source | Tests |
|---|---|---|---|
| Kerberos v5 (mandatory in DCOM) | `1.2.840.113554.1.2.2` | `src/Opc.Classic.Dcom.Kerberos/Spnego/SpnegoOids.cs` | covered by SPNEGO tests |
| Kerberos v5 User-to-User | `1.2.840.113554.1.2.2.3` | same | not implemented (see §3.1) |
| NTLM (`NTLMSSP`) | `1.3.6.1.4.1.311.2.2.10` | same | covered by SPNEGO tests |
| SPNEGO itself | `1.3.6.1.5.5.2` | same | same |
| MS-NEGOEX | `1.3.6.1.4.1.311.2.2.30` | not implemented | not implemented |

### 1.3 Mech-list MIC (spec §3.1.5.4 / RFC 4178 §4.2.2)

The mech-list MIC is computed over the canonical DER-encoded
`MechTypeList` from the initial `NegTokenInit`. Once both sides have
established a session, each side computes the MIC using the selected
mech's `GSS_GetMIC` primitive and exchanges it via the `mechListMIC`
field of `NegTokenResp`. Mismatch ⇒ `negState = reject`.

| Surface | Source | Tests |
|---|---|---|
| MIC computation (delegates to Kerberos `Get_MIC` per RFC 4121 §4.2.6.1) | `src/Opc.Classic.Dcom.Kerberos/KerberosSpnegoMicProvider.cs` (or equivalent in `Spnego/`) | `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosSpnegoMicProviderTests.cs` |
| MIC verification on receive | same | same |
| Per-spec ordering: MIC is the LAST exchange in the negotiation | same | covered by `SpnegoTests.cs` |

### 1.4 Mech preference order (spec §3.1.5.1)

Opc.Classic advertises Kerberos first. NTLM appears only when an NTLM provider
and `SpnegoNtlmFallbackPolicy.WhenKerberosUnavailable` are configured:

```
mechTypes = SEQUENCE {
  1.2.840.113554.1.2.2,      -- Kerberos v5
  1.3.6.1.4.1.311.2.2.10     -- NTLM
}
```

Mech selection happens when the acceptor returns a `NegTokenResp` with
`supportedMech` set. `Disabled` is Kerberos-only. Under
`WhenKerberosUnavailable`, NTLM can be selected only when Kerberos is not
available to both peers; a selected Kerberos failure is rejected rather than
silently retried as NTLM.

| Surface | Source | Tests |
|---|---|---|
| Mech list construction | `src/Opc.Classic.Dcom.Kerberos/Spnego/SpnegoTokenBuilder.cs` | `SpnegoTests.cs` |
| Fallback handling | `SpnegoServerOptions`, `SpnegoNtlmFallbackPolicy`, `SpnegoServerAuthenticationProvider` | `SpnegoServerAuthenticationProviderTests.cs` |

### 1.5 Mech-token passthrough (spec §2.2.1 / §2.2.2)

The `mechToken` field carries the opaque inner blob:
- For Kerberos: the GSS-wrapped `AP_REQ` (or `AP_REP` on the back-flow).
- For NTLM: the `NEGOTIATE_MESSAGE` / `CHALLENGE_MESSAGE` / `AUTHENTICATE_MESSAGE`.

SPNEGO does not interpret the inner blob — it just routes bytes to the
selected mech.

| Surface | Source | Tests |
|---|---|---|
| `mechToken` passthrough | `src/Opc.Classic.Dcom.Kerberos/Spnego/SpnegoMech.cs` | `SpnegoTests.cs` |

---

## 2 Normative-clause checklist

MS-SPNG contains **25 MUST/SHALL clauses** per Phase 0 inventory.
§-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 4 | ✅ informative | n/a |
| §2.2 | Message syntax | 7 | ✅ conformant | §1.1 |
| §3.1 | Common message processing | 9 | ✅ conformant | §1.3 - §1.5 |
| §5 | Security | 5 | ✅ documented | n/a |

Phase 2 deep-validation will pin each clause individually.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Kerberos v5 U2U mech not advertised in mechTypes

Kerberos User-to-User (OID `1.2.840.113554.1.2.2.3`) is not
advertised. Same rationale as MS-KILE §3.1.2. Status: **WAIVED**
(deferred-by-design).

#### 3.1.2 MS-NEGOEX not implemented

MS-NEGOEX extends SPNEGO with additional metadata exchange (mostly
for PKU2U scenarios). Status: **WAIVED** — not used by any current
DCOM deployment.

#### 3.1.3 `request-mic` handling is implicit

When the acceptor needs a MIC explicitly, it sends `negState =
request-mic`. The current implementation always computes + sends the
MIC at the appropriate point in the negotiation regardless of explicit
request, so the `request-mic` state is effectively no-op (compliant).
Status: **WAIVED** (no functional impact).

### 3.2 Hard gaps

None at present. `NegTokenInit` / `NegTokenResp` ASN.1 encoding,
mech OID handling, mech-list MIC computation + verification, and
preference-ordered mech selection all conform to RFC 4178 + MS-SPNG.
SPNEGO fuzz testing in `SpnegoFuzzTests.cs` covers malformed-token
robustness.

---

## 4 Cross-references

- Related spec: [`docs/conformance/ms-kile.md`](ms-kile.md) — Kerberos inner mech.
- Related spec: [`docs/conformance/ms-nlmp.md`](ms-nlmp.md) — NTLM inner mech.
- Related spec: [`docs/conformance/ms-rpce.md`](ms-rpce.md) — auth-trailer carrier wrapping SPNEGO tokens.
- Tutorial: [`docs/tutorials/04-security-with-kerberos-and-channel-binding.md`](../tutorials/04-security-with-kerberos-and-channel-binding.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-SPNG.md` (Microsoft Open
Specifications MS-SPNG: Simple and Protected GSS-API Negotiation
Mechanism Extension).

Phase 0 inventory:

- `files/conformance/inventory/ms-spng-headings.csv` (69 entries)
- `files/conformance/inventory/ms-spng-clauses.csv` (25 normative entries)
