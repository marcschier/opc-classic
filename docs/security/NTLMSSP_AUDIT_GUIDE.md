# NTLMSSP audit prep guide

## 1. Document scope

This guide is a focused audit-preparation addendum for the self-contained
NTLMSSP implementation in Opc.Classic.
It is intended for the external third-party audit tracked as
`rw-e4-ntlm-audit`.
The goal is to let an auditor begin review without further development-team
orientation.
This document covers:
- The current NTLMSSP code surface and adjacent Kerberos/SPNEGO surfaces.
- Cryptographic primitives used by NTLMSSP and DCE/RPC packet protection.
- Message construction and parsing for NEGOTIATE, CHALLENGE, and AUTHENTICATE.
- Test coverage that already exists, including RFC and MS-NLMP vectors.
- NTLM-specific threat-model deltas and residual risks.
- Known limitations that are intentionally deferred or deployment-managed.
- Recommended abuse tests and audit deliverables.
This document does not replace the repository-wide threat model.
Use `docs\security\THREAT_MODEL.md` as the parent STRIDE assessment.
Use `docs\security\CHANNEL_BINDING.md` for the channel-binding design that
feeds NTLMv2 and Kerberos/SPNEGO.
Component names are guideposts from the current checkout and should be
revalidated before an audit report cites exact implementation details.
The NTLM implementation is split across `src\Opc.Classic.Dcom\rpc\Auth`
for DCE/RPC authentication/session security and `src\Opc.Classic.Dcom\Common\Ntlm`
for NTLMSSP message DTOs and helpers. Kerberos companion code lives in
`src\Opc.Classic.Dcom.Kerberos`; SPNEGO code is under
`src\Opc.Classic.Dcom.Kerberos\Spnego` in this checkout.

## 2. Why a self-contained NTLMSSP

Opc.Classic targets cross-platform, NativeAOT-compatible .NET 10.
The stack cannot depend on Windows COM runtime callable wrappers or Windows-only
SSPI entry points.
`BannedSymbols` bans Reflection.Emit and runtime expression-tree
compilation.
`BannedSymbols` bans reflection-based activation and invocation
patterns that defeat trim analysis.
`BannedSymbols` bans `[ComImport]` and Windows COM Automation
helpers, because the managed DCOM stack replaces them with generated proxies,
NDR codecs, and managed MSRPC transport.
The public authentication selector is `OpcAuthMode`.
`OpcAuthMode` documents NTLMv1 as legacy and
NTLMv2 as the cross-platform default.
`OpcAuthMode` documents Kerberos/SPNEGO as the
preferred Active Directory mechanism.
`OpcConnectData` defaults connections to
`OpcAuthMode.NtlmV2` and `OpcProtectionLevel.Integrity` when callers do not opt
into another mode.
`OpcProtectionLevel` ties that default to
Microsoft DCOM hardening expectations.
Microsoft's NTLM overview says Kerberos version 5 is the preferred
authentication method for Active Directory environments, while NTLM remains
needed for workgroup/local-account and compatibility cases.
Microsoft's "The evolution of Windows authentication" guidance describes the
platform direction as reducing NTLM usage.
Opc.Classic follows that posture:
- Prefer Kerberos/SPNEGO where a realm, SPN, and service ticket are available.
- Keep NTLMv2 for cross-platform DCOM interoperability when Kerberos is not
  available.
- Disable NTLMv1 by default.
- Never introduce MD4, RC4, or DES for new designs; use them only where MS-NLMP
  requires them for compatibility.
The self-contained implementation exists because .NET does not provide a
portable NTLMSSP implementation that can be used directly for managed DCOM
packet signing/sealing on every target.
The hand-rolled portions are intentionally narrow and auditable.
The BCL is still used for primitives that exist in modern .NET, such as
HMAC-MD5, MD5 wrappers, DES, SHA-256/SHA-384, and certificate handling.

## 3. Code surface inventory

### 3.1 NTLM authentication and session-security files

| File | Purpose | Trust boundary |
| --- | --- | --- |
| `AuthenticationSource` | Pluggable server-side credential source contract and default registration API. | Server process to credential store. |
| `NullAuthenticationSource` | Fail-closed placeholder when no credential source is registered. | Server process configuration boundary. |
| `NtlmAuthentication` | Main NTLMSSP orchestrator: properties, Type1/2/3, MIC, CBT, proof verification, session key setup. | Client/server auth handshake over network. |
| `NtlmConnection` | Legacy DCE/RPC bind/rebind state machine for NTLM tokens. | DCE/RPC auth verifier boundary. |
| `NtlmConnectionContext` | Client bind/alter-context context and bind-ack validation. | Network PDU to connection state. |
| `Ntlm1` | DCE/RPC packet integrity/privacy using NTLM signing and RC4 sealing keys. | Protected RPC PDU body and verifier. |
| `NTLMKeyFactory` | Session key derivation, RC4 key wrapping, signing/sealing key derivation, SIGNATURE_BLOCK generation. | Password-derived keys to packet protection. |
| `Responses` | LM/NTLM/NTLMv2 response functions, NTOWFv1/v2, blob creation, HMAC-MD5, DES key expansion, and sensitive-buffer cleanup hooks. | Password material to wire challenge response. |

### 3.2 NTLM message DTOs and helpers

| File | Purpose | Trust boundary |
| --- | --- | --- |
| `NtlmMessage` | Shared NTLMSSP signature, message type, flags, security-buffer bounds, string encoding. | Untrusted token bytes to typed messages. |
| `Type1Message` | NEGOTIATE encode/decode, supplied domain/workstation, optional version. | Client-supplied Type1 token. |
| `Type2Message` | CHALLENGE encode/decode, challenge, target, target-info AV pairs. | Server-supplied Type2 token. |
| `Type3Message` | AUTHENTICATE encode/decode, LM/NT responses, identity fields, session key, MIC. | Client-supplied Type3 token. |
| `NtlmFlags` | MS-NLMP negotiate flag constants used by the message classes. | Negotiated protocol-policy input. |
| `NtlmAvPairs` | Target-info AV_PAIR add/replace/read helpers, MIC flag, CBT AV ID. | Type2/Type3 target-info boundary. |
| `NtlmMic` | AUTHENTICATE MIC compute/verify with fixed-time comparison. | Handshake transcript integrity. |
| `NtlmMessageSignature` | NTLM SIGNATURE_BLOCK generation and verification. | SPNEGO mechListMIC and packet MIC helpers. |
| `NtlmMicProvider` | SPNEGO `IGssMicProvider` adapter for NTLMSSP signing keys. | SPNEGO mechanism-list integrity. |
| `NtlmPasswordAuthentication` | Small legacy credential holder used by NTLM call sites. | Credential object boundary. |
| `Arrays` | Java compatibility helper for filling arrays. | None; local utility. |
| `Config` | Legacy configuration lookup shim. | Local configuration boundary. |
| `Hashtable` | Java compatibility dictionary wrapper. | None; local utility. |
| `InstantiationException` | Compatibility exception type. | None. |
| `Iterator` | Compatibility iterator wrapper. | None. |
| `MissingResourceException` | Compatibility exception type. | None. |
| `NbtAddress` | NetBIOS address compatibility shim. | Name-resolution compatibility. |
| `NoSuchElementException` | Compatibility exception type. | None. |
| `PrintWriter` | Compatibility writer shim. | None. |
| `SharpenCompatibilityExtensions` | Legacy compatibility extension helpers. | Local utility. |
| `SmbAuthException` | Compatibility exception type. | Auth error propagation. |
| `SmbException` | Compatibility exception type. | Protocol error propagation. |
| `SmbNamedPipe` | Compatibility stream wrapper for named-pipe-style I/O. | Local stream boundary. |
| `SmbSession` | Compatibility session stub. | None. |
| `StringTokenizer` | Compatibility tokenizer. | Local parsing utility. |
| `Thread` | Compatibility thread wrapper. | Local threading utility. |
| `ThreadGroup` | Compatibility thread-group wrapper. | Local threading utility. |
| `UniAddress` | Compatibility address holder. | Name/address input. |
| `UnknownHostException` | Compatibility exception type. | Name-resolution error propagation. |
| `UnsupportedEncodingException` | Compatibility exception type. | Encoding error propagation. |
| `Uuid` | Compatibility UUID wrapper. | Local identifier parsing. |

### 3.3 Kerberos companion surface

| File | Purpose | Trust boundary |
| --- | --- | --- |
| `IKerberosAuthInfo` | Public Kerberos realm/SPN/user contract. | Caller configuration to Kerberos client. |
| `KerberosAuthInfo` | Immutable Kerberos auth configuration with password/keytab options. | Caller secrets/configuration boundary. |
| `IKerberosConnectionContext` | AP-REQ/AP-REP handshake abstraction. | KDC/service-ticket boundary. |
| `KerberosConnectionContext` | Kerberos.NET-backed ticket acquisition, AP-REP validation, GSS token extraction, CBT checksum injection. | Client to KDC and service. |
| `IKerberosSession` | RFC 4121 MIC/Wrap session abstraction. | Protected PDU body boundary. |
| `KerberosSession` | RFC 4121 MIC/Wrap and RC4-HMAC/AES packet protection. | Kerberos session key to network tokens. |
| `KerberosSessionKey` | Session-key metadata record. | AP exchange to packet protection. |
| `KerberosChannelBindingChecksum` | MS-KILE GSS channel-binding checksum builder. | TLS channel binding to Kerberos AP-REQ. |
| `KerberosAuthContext` | DCOM `IAuthContext` implementation for Kerberos/SPNEGO. | DCOM bind/call to Kerberos/SPNEGO. |

### 3.4 SPNEGO surface

SPNEGO code is under `src\Opc.Classic.Dcom.Kerberos\Spnego` in this checkout.
| File | Purpose | Trust boundary |
| --- | --- | --- |
| `IGssMicProvider` | Mechanism-independent MIC provider contract. | Inner mechanism to SPNEGO verifier. |
| `KerberosMicProvider` | Kerberos-backed `mechListMIC` provider. | Kerberos session to SPNEGO response. |
| `SpnegoDecoder` | DER decoder for NegTokenInit/NegTokenResp, preserving MechTypeList bytes. | Untrusted SPNEGO token to typed fields. |
| `SpnegoEncoder` | DER encoder for NegTokenInit/NegTokenResp and mechListMIC creation. | Local negotiation state to network token. |
| `SpnegoMech` | Mechanism enum. | Policy/display helper. |
| `SpnegoNegState` | RFC 4178 negotiation-state enum. | SPNEGO response policy. |
| `SpnegoNegTokenInit` | NegTokenInit record, including exact MechTypeList bytes. | Initiator token model. |
| `SpnegoNegTokenResp` | NegTokenResp record and mechListMIC verification. | Acceptor token model. |
| `SpnegoOids` | SPNEGO, Kerberos, and NTLMSSP OID constants. | Mechanism-selection policy. |
| `SpnegoTokenBuilder` | Kerberos-preferred token builder offering Kerberos then NTLMSSP. | Mechanism-list downgrade boundary. |

## 4. Cryptographic primitives in use

| Primitive | Used for | Implementation | RFC/spec reference | Test coverage |
| --- | --- | --- | --- | --- |
| MD4 | NTOWFv1 / NT hash = MD4(UTF-16LE password). | `Md4`, `Md4State`, `MD4Digest`. | RFC 1320; MS-NLMP §3.3.1. | `Md4Tests`. |
| HMAC-MD5 | NTOWFv2, LMv2/NTLMv2 proof, MIC, message signatures. | BCL `System.Security.Cryptography.HMACMD5` in `Responses`, `NtlmMic`, `NtlmMessageSignature`. | RFC 2104; MS-NLMP §3.3.2 and §3.4.4. | `NtlmV2ServerKeyDerivationTests`, `NtlmMicTests`. |
| MD5 | NTLM2-session hash and key-magic digest wrappers. | BCL via `MD5Digest`; `Responses`; `NTLMKeyFactory`. | RFC 1321; MS-NLMP session security. | Indirect through NTLMv2/key tests; no standalone MD5 test needed because BCL-backed. |
| RC4 / ARCFOUR | NTLM packet sealing and exported-session-key wrapping. | `Rc4`, `RC4Engine`, `NTLMKeyFactory`. | MS-NLMP §3.4.5; RFC 6229 vectors for validation. | `Rc4Tests`. |
| DES-ECB no padding | LM/NTLMv1 legacy response construction only. | BCL DES wrapper `DesEcbNoPaddingCipher`; `Responses`. | MS-NLMP NTLMv1/LM compatibility. | No direct NTLMv1 vector test; NTLMv1 disabled by default. |
| SHA-256 / SHA-384 | TLS `tls-server-end-point` certificate digest. | BCL via `ChannelBindingsFactory`, summarized in `CHANNEL_BINDING.md`. | RFC 5929; RFC 5056. | `ChannelBindingTlsTests`. |
| MD5 GSS channel-binding hash | RFC 2744 channel-bindings structure hash consumed by NTLM and Kerberos. | `ChannelBindingsHash.Compute`, summarized in `CHANNEL_BINDING.md:36-54`. | RFC 2744; MS-NLMP AV_PAIR `MsvAvChannelBindings`. | `ChannelBindingTlsTests`; Kerberos CBT tests under `KerberosChannelBindingChecksumTests`. |
| Random nonces/session keys | NTLMv2 client challenge and exported session key. | `NtlmAuthentication`; `NTLMKeyFactory`. | MS-NLMP nonce/session-key requirements. | Partially covered by round trips; randomness quality is a known audit focus. |
| Kerberos RC4-HMAC | Kerberos RFC 4757 per-message tokens. | `KerberosSession`. | RFC 4757. | `Rfc4757Rc4HmacTests`. |
| Kerberos AES CTS-HMAC | Kerberos RFC 4121 wrap/MIC for AES etypes. | `KerberosSession` via Kerberos.NET crypto transformers. | RFC 3962; RFC 8009; RFC 4121. | `Rfc3962AesCtsTests.cs`, `Rfc8009AesShaaTests.cs`, `Rfc4121MicTokenTests.cs`, `Rfc4121WrapTokenTests.cs`. |
The BCL/hand-rolled split is intentional.
MD4 is not available in the BCL and is required only for NTLM compatibility.
RC4 is not available in modern .NET BCL cipher APIs and is required for NTLM
sealing and RC4-HMAC Kerberos compatibility.
HMAC-MD5, MD5, DES, SHA-256, SHA-384, RSA certificate handling, and TLS
certificate extraction are delegated to the BCL where available.
Auditors should treat MD4, RC4, NTLM key derivation, MIC verification, and
session-key state as the primary cryptographic review surface.

## 5. Message structure walkthrough

### 5.1 NEGOTIATE / Type 1 (`MS-NLMP` §2.2.1.1)

`Type1Message` writes the `NTLMSSP\0` signature and message type 1 through
`NtlmMessage.WriteHeader`.
`NtlmMessage` validates the signature and message type for all NTLM
messages.
`Type1Message` serializes flags plus optional supplied domain and
workstation security buffers.
`Type1Message` parses flags and security buffers, rejecting messages
shorter than 32 bytes.
`NtlmAuthentication.CreateType1` in `NtlmAuthentication` builds the
client NEGOTIATE token from `DefaultFlags` and records the raw message for MIC
calculation.
`NtlmAuthentication` builds default flags, including NTLM, always
sign, Unicode/OEM, optional sign/seal/key-exchange, 56-bit/128-bit flags, and
extended session security.
`NtlmAuthentication` adjusts peer flags by local policy.
Auditor focus: verify unsupported or deprecated flags are not accidentally
honored after `AdjustFlags`.

### 5.2 CHALLENGE / Type 2 (`MS-NLMP` §2.2.1.2)

`Type2Message` serializes target name, negotiate flags, 8-byte
server challenge, 8-byte context, target-info AV_PAIR bytes, and optional
version.
`Type2Message` parses the same fields and rejects messages shorter
than 48 bytes.
`Type2Message` enforces an 8-byte server challenge.
`Type2Message` enforces an 8-byte context value.
`Type2Message` creates default target-info containing the workstation
name and an EOL pair.
`NtlmAuthentication.CreateType2` in `NtlmAuthentication` adjusts
client flags, marks target type server, uses the current server challenge, and
adds the MIC-required AV flag when key exchange and version are negotiated.
`NtlmAvPairs` defines `MsvAvFlags`, `MsvAvChannelBindings`, and the MIC
flag.
`NtlmAvPairs` detects and adds the MIC flag.
`NtlmAvPairs` adds or replaces target-info AV_PAIRs with bounds checks.
Auditor focus: challenge generation, target-info length validation, and MIC flag
policy.

### 5.3 AUTHENTICATE / Type 3 (`MS-NLMP` §2.2.1.3)

`Type3Message` serializes LM response, NT response, domain, user,
workstation, encrypted random session key, flags, optional version, and optional
MIC.
`Type3Message` parses the same fields and computes the minimum
payload offset before treating version/MIC bytes as present.
`Type3Message` enforces a 16-byte MIC.
`Type3Message` zeroes the MIC field, computes the transcript MIC,
then writes the MIC back at offset 72.
`NtlmAuthentication.CreateType3` in `NtlmAuthentication` is the main
client construction path.
For NTLMv2, `NtlmAuthentication` generates the client nonce,
applies channel bindings to target info, and calls the response builders.
For legacy fallback, `NtlmAuthentication` contains NTLM2-session and
plain NTLMv1 response code paths that require explicit opt-in.
`NtlmAuthentication` derives the exported session key, optionally
wraps it with RC4 key exchange, creates packet-protection state, and adds the
MIC if required.
`Responses` creates the NTLMv2 response from NTOWFv2, target info,
challenge, and client nonce.
`Responses` creates the NTLMv2 blob with timestamp, nonce, target
info, and terminator bytes.
`NtlmAuthentication` contains the protocol-level server-side Type3 session-security setup.
`NtlmAuthentication` validates the NTLMv2 proof using
`CryptographicOperations.FixedTimeEquals`, derives the session base key, and
unwraps the encrypted random session key when key exchange was negotiated.
Auditor focus: prove the protocol verifier checks the exact challenge it issued,
the exact target-info/CBT bytes, and the MIC over the original Type1/Type2/Type3
transcript. Also verify this remains documented as not wired into managed listener
server-side bind handling.

### 5.4 MIC and channel-binding flow

`NtlmAuthentication` stores original NEGOTIATE and CHALLENGE bytes.
`NtlmAuthentication` requires an exported session key and original
messages before computing a MIC.
`NtlmAuthentication` verifies the MIC during server-side Type3
processing.
`NtlmMic` computes `HMAC-MD5(sessionKey, negotiate || challenge ||
authenticate)`.
`NtlmMic` zeroes the AUTHENTICATE MIC bytes and compares the expected
and actual MIC in fixed time.
`NtlmAuthentication` inserts and validates `MsvAvChannelBindings`
when a 16-byte CBT hash is configured.
`CHANNEL_BINDING.md:42-54` explains the AV_PAIR encoding and no-TLS behavior.

### 5.5 Packet signing and sealing

`Ntlm1` derives client/server signing and sealing keys from the
exported session key.
`Ntlm1` decrypts/verifies inbound protected PDUs.
`Ntlm1` signs and optionally seals outbound protected PDUs.
`NTLMKeyFactory` derives signing and sealing keys using MS-NLMP magic
constants.
`NTLMKeyFactory` builds the NTLM SIGNATURE_BLOCK with sequence number
and HMAC-MD5 checksum.
`NTLMKeyFactory` currently compares packet signatures with
`SequenceEqual`; `THREAT_MODEL.md:237-240` calls out replacement with fixed-time
comparison as an open recommendation.
TODO(auditor): confirm all negotiated flag combinations that reach `Ntlm1` match
MS-NLMP §3.4.5, especially sign-only vs seal modes and key-exchange presence;
covered by `NtlmNegotiateFlagsTests.cs` for the documented combinations.

## 6. Test coverage map

| Source file or area | Has unit tests? | Has wire-fixture tests? | Has live-server tests? | Status |
| --- | --- | --- | --- | --- |
| `Crypto\Md4.cs`, `Md4State.cs`, `MD4Digest.cs` | ✅ | ✅ RFC 1320 Appendix A.5 | ❌ N/A | Good primitive vectors and incremental-state tests. |
| `Crypto\Rc4.cs`, `RC4Engine.cs` | ✅ | ✅ RFC 6229 §2.1 | ❌ N/A | Good KSA/PRGA and wrapper compatibility tests. |
| `Responses.cs` NTLMv2 derivation | ✅ partial | ✅ MS-NLMP §4.2.4.1 | ❌ | Covered by `NtlmV2ServerKeyDerivationTests.cs`; `PasswordZeroizationTests.cs` verifies password-derived pooled buffers are cleared before return. |
| `Responses.cs` LM/NTLMv1 | ❌ | ❌ | ❌ | Deferred because NTLMv1/LM are disabled by default. |
| `NTLMKeyFactory.cs` session-key derivation | ✅ partial | ✅ MS-NLMP vector | ❌ | Signing-key equality covered; random/session-key quality not directly tested. |
| `NTLMKeyFactory.cs` packet signatures | ✅ direct | ✅ MS-NLMP §3.4.4/§3.4.5 SIGNATURE_BLOCK vectors | ❌ | `NtlmSignatureBlockTests.cs` covers direct formation, mismatch rejection, replay, and wrap-boundary sequence handling. |
| `NtlmAuthentication.cs` default policy | ✅ | ❌ | ❌ | `NtlmDefaultsTests.cs` covers NTLMv2 defaults and NTLMv1 opt-in guard. |
| `NtlmAuthentication.cs` Type1/2/3 round trip | ✅ | ⚠️ partial | ❌ blocked by `rw-e1` live interop | Client/server in-memory round trips exist. |
| `NtlmAuthentication.cs` MIC | ✅ | ⚠️ synthetic vectors | ❌ | `NtlmMicTests.cs` covers compute, verify, tamper, fixed-time source check. |
| `NtlmAuthentication.cs` CBT | ✅ | ⚠️ synthetic TLS cert vectors | ❌ | `ChannelBindingTlsTests.cs` covers AV_PAIR insertion and matching protocol-verifier rejection. |
| `Type1Message.cs` | ✅ bounds | ⚠️ malformed synthetic corpus | ❌ | `DecoderBoundsFuzzTests.cs` covers truncated, bad-signature, wrong-type, oversized, out-of-range, overlapping, and version-truncated cases. |
| `Type2Message.cs` | ✅ bounds | ⚠️ malformed synthetic corpus | ❌ | `DecoderBoundsFuzzTests.cs` covers truncated, wrong-type, oversized, target, target-info, and overlap cases. |
| `Type3Message.cs` | ✅ partial | ⚠️ MS-NLMP fixture plus malformed synthetic corpus | ❌ | MIC offset, parser paths, bounded malformed inputs, and fixture replay are covered; broader structure-aware fuzzing remains recommended. |
| `NtlmAvPairs.cs` | ✅ indirect | ❌ | ❌ | Covered through MIC/CBT tests; direct invalid-length tests recommended. |
| `NtlmMessageSignature.cs` | ✅ direct | ✅ MS-NLMP §3.4.4/§3.4.5 SIGNATURE_BLOCK vectors | ❌ | Covered directly by `NtlmSignatureBlockTests.cs` plus SPNEGO NTLM MIC provider paths. |
| `NtlmConnection.cs` / `NtlmConnectionContext.cs` | ❌ | ❌ | ❌ | Needs bind/auth verifier state-machine tests. |
| `AuthenticationSource.cs` / `NullAuthenticationSource.cs` | ❌ | ❌ | ❌ | Contract is fail-closed but should get server-host tests. |
| `KerberosAuthContext.cs` | ✅ | ⚠️ synthetic | ⚠️ integration KDC | Companion coverage under Kerberos test project. |
| `KerberosSession.cs` | ✅ | ✅ RFC 4121/4757/3962/8009 | ⚠️ integration KDC | Good separate Kerberos audit input. |
| `SpnegoEncoder.cs` / `SpnegoDecoder.cs` | ✅ | ✅ known DER fragments | ❌ | `SpnegoTests.cs` and `SpnegoNegTokenRespTests.cs`. |
| `SpnegoTokenBuilder.cs` | ✅ | ✅ OID constants | ❌ | Confirms Kerberos-first, NTLMSSP-fallback mech list. |
Test files of interest:
- `Md4Tests`.
- `Rc4Tests`.
- `NtlmV2ServerKeyDerivationTests`.
- `NtlmMicTests`.
- `NtlmSignatureBlockTests`.
- `PasswordZeroizationTests`.
- `NtlmDefaultsTests`.
- `ChannelBindingTlsTests`.
- `DecoderBoundsFuzzTests`.
- `SpnegoTests`.
- `SpnegoNegTokenRespTests`.
- `KerberosChannelBindingChecksumTests`.
- `KerberosKdcIntegrationTests`.
- `NtlmHandshakeFixtureTests` plus `negotiate.bin`, `challenge.bin`, and `authenticate.bin` provide byte-exact MS-NLMP §4.2 fixture replay coverage.

## 7. Threat model addendum

### 7.1 NTLM relay — PARTIAL

Mitigations present:
- Packet integrity is the default for `OpcConnectData` (`OpcConnectData`).
- Privacy can be selected through `OpcProtectionLevel.Privacy`.
- NTLM channel binding is inserted and validated when callers provide CBT
  (`NtlmAuthentication`).
- MIC protects the Type1/Type2/Type3 transcript when requested
  (`NtlmAuthentication`).
Residual risk:
- NTLM is not mutual authentication in the Kerberos sense.
- Relay resistance depends on TLS endpoint validation, correct CBT input, and
  deployment policy.
- Kerberos/SPNEGO should be required for high-assurance Active Directory
  deployments.
Audit abuse case: proxy a valid client Type1/Type3 through a malicious endpoint
with and without CBT and confirm CBT mismatch fails closed.

### 7.2 NTLM downgrade / force NTLMv1 — MITIGATED BY DEFAULT

Mitigations present:
- `NtlmAuthentication` defaults `_useNtlmV2` to true and throws when
  NTLMv2 is disabled without `rpc.ntlm.allowV1=true`.
- `NtlmAuthentication` creates public `OpcConnectData` properties
  with `rpc.ntlm.ntlmv2=true` and `rpc.ntlm.allowV1=false`.
- `Ntlm1` marks NTLMv1 session-security fallback obsolete and warns it
  is cryptographically broken.
- `NtlmDefaultsTests` verifies the throw and explicit opt-in behavior.
Residual risk:
- The explicit opt-in remains for very old legacy targets.
- Audit should confirm no untrusted server flag can force `_useNtlmV2=false`.

### 7.3 Replay attacks — PARTIAL

Mitigations present:
- NTLMv2 proofs bind server challenge, timestamp, client nonce, and target-info
  blob (`Responses`, `NtlmAuthentication`).
- Packet protection uses monotonically increasing request/response counters
  (`Ntlm1`).
- Kerberos companion sessions enforce receive sequence numbers
  (`KerberosSession`).
Residual risk:
- `NtlmAuthentication` currently uses a fixed default server challenge.
- `NTLMKeyFactory` and `NtlmAuthentication` use `Random`, not
  `RandomNumberGenerator`.
- There is no NTLM replay cache for seen challenges/timestamps/client nonces.
- `THREAT_MODEL.md:237-240` already marks randomness hardening as high priority.
Audit abuse case: replay a captured Type3 against the same and different server
challenge; verify expected failure once challenge randomness is hardened.

### 7.4 Dictionary / brute-force against NTOWF — PARTIAL

Mitigations present:
- NTLMv2 is the default and avoids LM/NTLMv1 by default.
- NTOWFv2 derives `HMAC-MD5(NTOWFv1, UppercaseUser || Target)` in
  `Responses`.
- The protocol verifier checks proofs in fixed time (`NtlmAuthentication`).
Residual risk:
- Captured NTLMv2 exchanges are still offline password-guessing material.
- Password strength, lockout, account policy, and credential rotation are
  deployment controls, not library controls.
- Passwords arrive as `NetworkCredential` strings and are not zeroized.
Audit abuse case: confirm the implementation never logs passwords, NT hashes,
NTOWFv2, session keys, or decrypted payloads in default logging paths.

### 7.5 MIC bypass / Drop-the-MIC — MITIGATED FOR DEFAULT KEY-EXCHANGE PATH

Mitigations present:
- Server Type2 adds `MsvAvFlagsMic` when key exchange and version are negotiated
  (`NtlmAuthentication`).
- Client computes MIC over original NEGOTIATE, CHALLENGE, and AUTHENTICATE
  (`Type3Message`, `NtlmMic`).
- Server verifies MIC in fixed time and rejects invalid or missing MIC
  (`NtlmAuthentication`, `NtlmMic`).
- `NtlmMicTests` covers MIC insertion and tamper rejection.
Residual risk:
- MIC enforcement depends on `RequiresMic`; auditors should test opt-down flag
  combinations and malicious target-info with missing or malformed `MsvAvFlags`.

### 7.6 Pass-the-hash — NOT MITIGATED

Mitigations present:
- Public connection APIs accept passwords/`NetworkCredential`, not a raw NT hash.
- There is no documented pass-the-hash API surface.
Residual risk:
- NTLM as a protocol cannot prevent an attacker who has equivalent password hash
  material from authenticating elsewhere.
- Server authorization and account hygiene are deployment responsibilities.
- Credential theft in the caller process remains outside this library's control.
Audit abuse case: verify no internal API accidentally exposes NT hash or
NTOWFv2 bytes to logs, exceptions, callbacks, or application-visible state.

### 7.7 Channel-binding bypass — PARTIAL

Mitigations present:
- `CHANNEL_BINDING.md:24-40` defines TLS endpoint data and RFC 2744 hash flow.
- NTLM inserts `MsvAvChannelBindings` into the NTLMv2 target-info blob when a
  16-byte CBT hash is configured (`NtlmAuthentication`).
- Kerberos carries the same hash in the AP-REQ checksum
  (`KerberosChannelBindingChecksum`).
- Tests cover NTLM insertion, no-TLS behavior, and protocol-level verifier matching
  (`ChannelBindingTlsTests`).
Residual risk:
- CBT is optional and caller-supplied; no-TLS sessions omit it.
- TLS certificate validation is delegated to the hosting application and .NET.
- The audit should include CVE-2022-21841-style channel-binding bypass attempts,
  including all-zero CBT values, absent AV pairs, duplicate AV pairs, and
  mismatched TLS endpoints.

## 8. Known limitations and deferred items

- LM authentication is not a supported production path.
  `Responses` retains LM/NTLMv1 compatibility helpers,
  but `NtlmAuthentication` blocks NTLMv1 unless explicitly opted in.
- NTLMv1 challenge-response is security-deprecated.
  It exists only behind `rpc.ntlm.allowV1=true` for legacy targets.
- NTLM SSO / Windows SSPI is intentionally unsupported.
  `NtlmAuthentication` throws `PlatformNotSupportedException` for
  `rpc.ntlm.sso=true` and points callers to Kerberos/SPNEGO.
- Server-side NTLM bind challenge handling is incomplete in the managed listener.
  `NtlmConnectionContext` throws for server-side bind and alter-context challenge
  handling, and `RpcServerConnectionProcessor` strips auth verifier metadata and
  rejects authenticated binds unless a dispatcher explicitly consumes
  `RpcRequestContext`; it does not run NTLM challenge/Type3 proof validation.
- `AuthenticationSource` is not wired into the main proof-validation path.
  `NtlmAuthentication` contains the old reflection-based source hook
  as commented-out code, and `AuthenticationSource` is currently a
  contract for future server-host integration.
- NTLM challenge and key randomness need hardening.
  `NtlmAuthentication` uses a fixed challenge for the current server
  helper, and `NtlmAuthentication` / `NTLMKeyFactory` use
  `Random`.
- Secrets are not completely zeroized.
  Public credentials are strings/`NetworkCredential`; password-derived pooled
  buffers are tested for zeroization, but not every derived byte array has a
  documented cleanup path.
- Packet-signature comparison is not consistently fixed-time.
  `NtlmMic`, `NtlmMessageSignature`, and the NTLMv2 proof verifier
  use fixed-time comparison; `NTLMKeyFactory` still uses `SequenceEqual`.
- Token-size ceilings and fuzzing gates are not release blockers.
  `NtlmMessage.cs` bounds security buffers and `DecoderBoundsFuzzTests.cs`
  covers malformed NDR and NTLM message inputs, but aggregate authentication
  token size, AV_PAIR count, and coverage-guided PDU fuzzing should expand.
- CBT is opt-in per connection.
  If callers do not provide `OpcConnectData.ChannelBindings`, NTLM channel
  bindings are omitted or treated as no-TLS behavior.
- Audit logging is incomplete.
  `THREAT_MODEL.md:231-244` lists security audit events and OpenTelemetry as
  open recommendations.

## 9. Audit scope recommendations

### 9.1 High-priority code paths

1. `NtlmAuthentication`.
   Review Type3 construction, nonce handling, key exchange, MIC insertion, and
   CBT insertion.
2. `NtlmAuthentication`.
   Review server-side Type3 validation, NT proof verification, session-base-key
   derivation, and encrypted-session-key handling.
3. `Responses`.
   Review NTLMv2 response math, timestamp encoding, identity canonicalization,
   and BCL HMAC use.
4. `NTLMKeyFactory`.

   Review randomness, RC4 usage, signing/sealing key constants, SIGNATURE_BLOCK
   layout, and comparison behavior.
5. `Type1Message`, `Type2Message.cs`,
   `Type3Message.cs`, and `NtlmMessage.cs`.
   Review parser bounds checks, security-buffer offset handling, duplicate or
   overlapping payloads, and optional version/MIC offsets.
6. `NtlmAvPairs` and
   `NtlmAuthentication`.
   Review AV_PAIR add/replace/read behavior, duplicate CBT handling, EOL
   handling, and malformed length behavior.
7. `Md4`, `Md4State.cs`, `Rc4.cs`, and
   `RC4Engine.cs`.
   Review hand-rolled primitive correctness, state reuse, allocation behavior,
   and side-channel considerations.
8. `SpnegoEncoder`,
   `SpnegoDecoder.cs`, and `SpnegoTokenBuilder.cs`.
   Review mechanism-list preservation, Kerberos-first ordering, NTLMSSP fallback,
   and `mechListMIC` verification.

### 9.2 Abuse cases to exercise

- Force server flags that omit sign, seal, key exchange, version, target-info, or
  extended session security.
- Send Type1/2/3 messages with short headers, bad signatures, wrong message
  types, out-of-range security buffers, overlapping payloads, and huge lengths.
- Duplicate AV_PAIRs, malformed EOL, invalid AV lengths, missing `MsvAvFlags`,
  missing CBT, all-zero CBT, wrong CBT length, and mismatched CBT hash.
- Drop the MIC, zero the MIC, move the MIC, or tamper with domain/user/workstation
  after MIC generation.
- Replay Type3 against a new server challenge and against the same challenge.
- Reorder signed/sealed packets and replay old NTLM SIGNATURE_BLOCK values.
- Use low-entropy passwords and confirm captured traces permit only expected
  offline guessing, not online bypass.
- Attempt `rpc.ntlm.sso=true`, `rpc.ntlm.ntlmv2=false`, and
  `rpc.ntlm.allowV1=false` combinations.
- Attempt `rpc.ntlm.allowV1=true` and verify the risk is explicit and visible.
- Exercise privacy mode with payload lengths 0, 1, 15, 16, 17, max-fragment - 16,
  and fragmented DCE/RPC PDUs.
- Exercise SPNEGO `NegTokenResp` with tampered `mechListMIC`, tampered mechanism
  list, unsupported selected mechanism, and nested initial-context wrappers.
- Capture a real Windows Server NTLMv2 DCOM handshake and replay altered
  variations against parser and proof-verifier tests.

### 9.3 Suggested test corpus additions

- MS-NLMP official examples for Type1/Type2/Type3 byte-exact messages.
- Windows Server 2019/2022 DCOM captures with integrity and privacy.
- Workgroup/local-account NTLMv2 capture where Kerberos cannot be used.
- TLS-wrapped DCOM capture with RFC 5929 CBT and a deliberate wrong endpoint.
- Malformed Type3 corpus generated by a structure-aware fuzzer.
- SIGNATURE_BLOCK known-answer tests for sign-only and seal modes.
- RC4 state-continuity tests over multiple RPC PDUs in each direction.
- Regression test proving packet-signature verification uses fixed-time compare
  after `NTLMKeyFactory.CompareSignature` is hardened.

### 9.4 Expected audit deliverables

The audit report should include:
- Executive summary with risk rating for NTLMSSP fallback in production.
- Finding list with severity, exploitability, affected line ranges, and PoC
  traces or unit tests where safe.
- Confirmation or rejection of each threat status in section 7.
- Protocol-conformance notes against MS-NLMP §2.2.1, §2.2.2, §3.3, and §3.4.5.
- Cryptographic primitive review for MD4, RC4, HMAC-MD5 use, DES compatibility,
  and randomness.
- Interop notes from at least one real Windows Server handshake if possible.
- Recommended fixes ordered by security impact and compatibility risk.
- Suggested regression tests suitable for Opc.Classic.Dcom.Crypto tests
  or Opc.Classic.Dcom tests.

## 10. Cross-references

- `docs\security\THREAT_MODEL.md` — parent STRIDE assessment and open security
  recommendations.
- `docs\security\CHANNEL_BINDING.md` — RFC 5056/RFC 5929 CBT flow for NTLMv2 and
  Kerberos.
- `BannedSymbols` — NativeAOT and cross-platform constraints that shape
  the implementation.
- `SECURITY.md` — responsible-disclosure policy and security-sensitive surface.
- `OpcConnectData` — public auth-mode and protection-level
  defaults.
- `OpcProtectionLevel` — DCE/RPC protection-level mapping
  and DCOM hardening note.
- `NtlmAuthentication` — primary NTLMSSP
  orchestrator.
- `Ntlm` — NTLM message and MIC helpers.
- `Crypto` — MD4, RC4, and compatibility crypto wrappers.
- `Opc.Classic.Dcom.Kerberos` and its `Spnego` subdirectory — Kerberos and SPNEGO companion implementation.
- Microsoft NTLM overview — Kerberos is preferred in Active Directory, NTLM is
  retained for compatibility/workgroup scenarios.
- Microsoft "The evolution of Windows authentication" — platform direction is
  to reduce NTLM usage and improve stronger alternatives.
