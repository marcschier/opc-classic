# MS-CSSP (Credential Security Support Provider) conformance review

**Spec:** `opc-classic-docs/MS-CSSP.md` (Credential Security Support
Provider Protocol).

**Scope:** CredSSP (Terminal Server credential delegation). Out of scope
for the OPC + DCOM wire stack. Opc.Classic references MS-CSSP only for
TLS server-end-point channel-bindings terminology (`tls-server-end-point:`
prefix), which is actually defined by RFC 5929 (consumed by MS-CSSP §3
implicitly via SPNEGO).

**Implementing assemblies:** `Opc.Classic.Core/Security/` (TLS channel
bindings + hash). No CredSSP protocol implementation is shipped.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| CredSSP TSRequest / TSCredentials message stack | §2.2.1 | ❌ not implemented | n/a | deferred-by-design — see §3.1 |
| TLS server-end-point channel bindings (`tls-server-end-point:` prefix + cert hash) | §1.3 narrative + RFC 5929 | ✅ `ChannelBindingsFactory` | ✅ | conformant (via RFC 5929, referenced by MS-CSSP) |
| Channel-bindings MD5 checksum for the NTLM trailer | §1.3 narrative + RFC 4121 | ✅ `ChannelBindingsHash` | ✅ | conformant |
| TLS protocol selection (SslProtocols.None / 1.2 / 1.3) | §1.3 / §1.4 | ✅ host-managed via `HttpClient` / `SslStream` | ✅ | conformant |
| Smart-card credentials (`TSSmartCardCreds`) | §2.2.1.2.2 | ❌ not implemented | n/a | deferred-by-design |
| Remote Guard credentials (`TSRemoteGuardCreds`) | §2.2.1.2.3 | ❌ not implemented | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 CredSSP message stack (spec §2.2.1)

Spec §2 defines the CredSSP wire protocol: a composite of TLS plus
SPNEGO plus the `TSRequest` ASN.1 message carrying `negoTokens` +
`authInfo` + `pubKeyAuth` + `errorCode` + `clientNonce`.

**Implementation status:** ❌ **not implemented**. Opc.Classic does
not delegate credentials via CredSSP. The OPC + DCOM chain uses NTLM /
Kerberos / SPNEGO authentication directly over the RPCE auth-trailer
mechanism per MS-RPCE §2.2.1.1; CredSSP is a different higher-level
delegation protocol used by Terminal Server / Remote Desktop. Status:
**WAIVED** (deferred-by-design) — CredSSP is not on the path for any
OPC client / server scenario.

### 1.2 TLS server-end-point channel bindings (spec §1.3 narrative + RFC 5929)

| Surface | Source | Tests |
|---|---|---|
| `ForTlsServerEndpoint(ReadOnlySpan<byte>)` (cert DER → channel bindings) | `src/Opc.Classic.Core/Security/ChannelBindingsFactory.cs` line 27 | `tests/Opc.Classic.Core.Tests/Security/ChannelBindingsTests.cs`, `tests/Opc.Classic.Dcom.Tests/ChannelBindingTlsTests.cs` |
| `ForTlsServerEndpoint(ReadOnlySpan<byte>, SslProtocols)` (TLS-1.3-aware variant) | `ChannelBindingsFactory.cs` line 34 | same |
| Hash selection (SHA-256 / SHA-384 / SHA-512 per RFC 5929 §4.1) | `ChannelBindingsFactory.cs` line 36 | same |
| TLS-1.3 endpoints use SHA-384 by default | `ChannelBindingsFactory.cs` line 25 | same |
| Application-data prefix `tls-server-end-point:` | `ChannelBindingsFactory.cs` line 20 | same |

### 1.3 Channel-bindings MD5 checksum (spec §1.3 narrative + RFC 4121 §4.1.1.2)

The channel-bindings field in the NTLM `AUTHENTICATE_MESSAGE` trailer is the MD5 hash of an `EXTENDED_BINDING` structure. This is conformant with the broader extended-protection-for-authentication scheme that MS-CSSP §1.3 narrative references.

| Surface | Source | Tests |
|---|---|---|
| `ChannelBindingsHash` (MD5 of the EXTENDED_BINDING structure) | `src/Opc.Classic.Core/Security/ChannelBindingsHash.cs` | `tests/Opc.Classic.Core.Tests/Security/ChannelBindingsTests.cs`, `tests/Opc.Classic.Dcom.Kerberos.Tests/KerberosChannelBindingChecksumTests.cs` |

The MD5 dependency is explicitly waived via a per-call
`#pragma warning disable CA5351` — MS-NLMP requires MD5 by spec, and
this is the only legitimate MD5 use in the project.

### 1.4 TLS protocol selection (spec §1.3 narrative)

Opc.Classic does not negotiate TLS itself; TLS is set up by the
caller-owned `HttpClient` (XML-DA path) or by the host operating
system's `SslStream` when wrapping the OPC DCOM transport in
TLS-via-SChannel. The `SslProtocols` enum value flows into
`ChannelBindingsFactory.ForTlsServerEndpoint` so the channel-bindings
hash matches the negotiated TLS handshake.

---

## 2 Normative-clause checklist

MS-CSSP contains 13 normative MUST/SHALL clauses per Phase 0 inventory.
Since Opc.Classic does NOT implement CredSSP itself, none of these
clauses apply to our code. Spot-checked all 13: all are about
CredSSP message construction, TSRequest field semantics, or SPNEGO
handshake within the CredSSP context — none are reachable from any
OPC + DCOM code path.

| § | Clause (paraphrased) | Status | Evidence |
|---|---|---|---|
| §2.2.1 + §2.2.1.x | TSRequest / TSCredentials / TSPasswordCreds / TSSmartCardCreds / TSRemoteGuardCreds field encoding rules | n/a — protocol not implemented | n/a |
| §3.1.5 | Sequencing rules for CredSSP handshake | n/a | n/a |
| §5.1 | Security considerations for implementors | n/a — informative | n/a |

The only practical conformance touch-point — the spec §1.3 narrative
that defines channel-bindings semantics derived from RFC 5929 + RFC
4121 — is verified through the tests cited in §1.2 + §1.3.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 CredSSP protocol not implemented

OPC + DCOM does not use CredSSP. Status: **WAIVED**
(deferred-by-design) — implementing CredSSP would only be needed for
Terminal Server / Remote Desktop interop, which is out of scope.

#### 3.1.2 Smart-card credentials + Remote Guard credentials not implemented

Both `TSSmartCardCreds` (§2.2.1.2.2) and `TSRemoteGuardCreds`
(§2.2.1.2.3) are CredSSP-specific extensions. Status: **WAIVED** (same
rationale as §3.1.1).

### 3.2 Hard gaps

| Gap | Severity | Source citation | Suggested fix |
|---|---|---|---|
| Citation `MS-CSSP §2.1.1.2` in `ChannelBindingsFactory.cs` line 23 is incorrect — `tls-server-end-point:` prefix is defined by RFC 5929 §4.1, not by MS-CSSP. MS-CSSP §1.3 narrative references the general extended-protection-for-authentication concept but does not contain a `§2.1.1.2` subsection at all. | hard (doc) | `src/Opc.Classic.Core/Security/ChannelBindingsFactory.cs` line 23 | Update comment to cite RFC 5929 §4.1 (`tls-server-end-point` channel binding registration) and link MS-CSSP §1.3 narrative + MS-NLMP §3.1.5.1.2 for the consumption side. |
| Citation `MS-NLMP/MS-CSSP require MD5` in `ChannelBindingsHash.cs` line 61 is overly broad — MS-CSSP does not require MD5; MS-NLMP §3.1.5.1.2 (CHANNEL_BINDINGS_HASH) does. | hard (doc) | `src/Opc.Classic.Core/Security/ChannelBindingsHash.cs` line 61 | Narrow the comment to cite MS-NLMP §3.1.5.1.2 only. |

---

## 4 Cross-references

- Related spec: [`docs/conformance/ms-nlmp.md`](ms-nlmp.md) — NTLM CHANNEL_BINDINGS_HASH consumer (forthcoming).
- Related spec: [`docs/conformance/ms-spng.md`](ms-spng.md) — SPNEGO outer wrapper (forthcoming).
- Tutorial: [`docs/tutorials/04-security-with-kerberos-and-channel-binding.md`](../tutorials/04-security-with-kerberos-and-channel-binding.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-CSSP.md` (Microsoft Open
Specifications MS-CSSP: Credential Security Support Provider Protocol,
v20240423).

Phase 0 inventory:

- `files/conformance/inventory/ms-cssp-headings.csv` (40 entries)
- `files/conformance/inventory/ms-cssp-clauses.csv` (13 normative entries — none applicable to OPC + DCOM)
