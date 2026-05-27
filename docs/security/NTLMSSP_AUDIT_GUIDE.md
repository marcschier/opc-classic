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
Line references are inclusive and reflect the current checkout when this file
was authored.
The legacy NTLM auth subdirectory mentioned in the task is not present as
a directory in this checkout.
The namespace is `Opc.Classic.Dcom.Rpc.Auth.ntlm`, but the files live directly
under `src\Opc.Classic.Dcom\rpc\Auth\`.
The NTLM message DTOs and helpers live under
`src\Opc.Classic.Dcom\Common\Ntlm\`.

## 2. Why a self-contained NTLMSSP

Opc.Classic targets cross-platform, NativeAOT-compatible .NET 10.
The stack cannot depend on Windows COM runtime callable wrappers or Windows-only
SSPI entry points.
`src\BannedSymbols.txt:11-19` bans Reflection.Emit and runtime expression-tree
compilation.
`src\BannedSymbols.txt:21-31` bans reflection-based activation and invocation
patterns that defeat trim analysis.
`src\BannedSymbols.txt:33-37` bans `[ComImport]` and Windows COM Automation
helpers, because the managed DCOM stack replaces them with generated proxies,
NDR codecs, and managed MSRPC transport.
The public authentication selector is `OpcAuthMode`.
`src\Opc.Classic.Core\OpcAuthMode.cs:20-32` documents NTLMv1 as legacy and
NTLMv2 as the cross-platform default.
`src\Opc.Classic.Core\OpcAuthMode.cs:34-39` documents Kerberos/SPNEGO as the
preferred Active Directory mechanism.
`src\Opc.Classic.Core\OpcConnectData.cs:44-78` defaults connections to
`OpcAuthMode.NtlmV2` and `OpcProtectionLevel.Integrity` when callers do not opt
into another mode.
`src\Opc.Classic.Core\OpcProtectionLevel.cs:13-17` ties that default to
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

| File | Lines | Purpose | Trust boundary |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom\rpc\Auth\AuthenticationSource.cs` | 1-79 | Pluggable server-side credential source contract and default registration API. | Server process to credential store. |
| `src\Opc.Classic.Dcom\rpc\Auth\NullAuthenticationSource.cs` | 1-35 | Fail-closed placeholder when no credential source is registered. | Server process configuration boundary. |
| `src\Opc.Classic.Dcom\rpc\Auth\NtlmAuthentication.cs` | 1-843 | Main NTLMSSP orchestrator: properties, Type1/2/3, MIC, CBT, proof verification, session key setup. | Client/server auth handshake over network. |
| `src\Opc.Classic.Dcom\rpc\Auth\NtlmConnection.cs` | 1-127 | Legacy DCE/RPC bind/rebind state machine for NTLM tokens. | DCE/RPC auth verifier boundary. |
| `src\Opc.Classic.Dcom\rpc\Auth\NtlmConnectionContext.cs` | 1-141 | Client bind/alter-context context and bind-ack validation. | Network PDU to connection state. |
| `src\Opc.Classic.Dcom\rpc\Auth\Ntlm1.cs` | 1-184 | DCE/RPC packet integrity/privacy using NTLM signing and RC4 sealing keys. | Protected RPC PDU body and verifier. |
| `src\Opc.Classic.Dcom\rpc\Auth\NTLMKeyFactory.cs` | 1-295 | Session key derivation, RC4 key wrapping, signing/sealing key derivation, SIGNATURE_BLOCK generation. | Password-derived keys to packet protection. |
| `src\Opc.Classic.Dcom\rpc\Auth\Responses.cs` | 1-348 | LM/NTLM/NTLMv2 response functions, NTOWFv1/v2, blob creation, HMAC-MD5, DES key expansion. | Password material to wire challenge response. |

### 3.2 NTLM message DTOs and helpers

| File | Lines | Purpose | Trust boundary |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMessage.cs` | 1-104 | Shared NTLMSSP signature, message type, flags, security-buffer bounds, string encoding. | Untrusted token bytes to typed messages. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Type1Message.cs` | 1-114 | NEGOTIATE encode/decode, supplied domain/workstation, optional version. | Client-supplied Type1 token. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Type2Message.cs` | 1-179 | CHALLENGE encode/decode, challenge, target, target-info AV pairs. | Server-supplied Type2 token. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Type3Message.cs` | 1-300 | AUTHENTICATE encode/decode, LM/NT responses, identity fields, session key, MIC. | Client-supplied Type3 token. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmFlags.cs` | 1-38 | MS-NLMP negotiate flag constants used by the message classes. | Negotiated protocol-policy input. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmAvPairs.cs` | 1-107 | Target-info AV_PAIR add/replace/read helpers, MIC flag, CBT AV ID. | Type2/Type3 target-info boundary. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMic.cs` | 1-44 | AUTHENTICATE MIC compute/verify with fixed-time comparison. | Handshake transcript integrity. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMessageSignature.cs` | 1-71 | NTLM SIGNATURE_BLOCK generation and verification. | SPNEGO mechListMIC and packet MIC helpers. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMicProvider.cs` | 1-43 | SPNEGO `IGssMicProvider` adapter for NTLMSSP signing keys. | SPNEGO mechanism-list integrity. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmPasswordAuthentication.cs` | 1-17 | Small legacy credential holder used by SharpCifs-compatible code. | Credential object boundary. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Arrays.cs` | 1-10 | Java compatibility helper for filling arrays. | None; local utility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Config.cs` | 1-27 | Legacy configuration lookup shim. | Local configuration boundary. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Hashtable.cs` | 1-42 | Java compatibility dictionary wrapper. | None; local utility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\InstantiationException.cs` | 1-10 | Compatibility exception type. | None. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Iterator.cs` | 1-13 | Compatibility iterator wrapper. | None. |
| `src\Opc.Classic.Dcom\Common\Ntlm\MissingResourceException.cs` | 1-10 | Compatibility exception type. | None. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NbtAddress.cs` | 1-15 | NetBIOS address compatibility shim. | Name-resolution compatibility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\NoSuchElementException.cs` | 1-10 | Compatibility exception type. | None. |
| `src\Opc.Classic.Dcom\Common\Ntlm\PrintWriter.cs` | 1-20 | Compatibility writer shim. | None. |
| `src\Opc.Classic.Dcom\Common\Ntlm\SharpenCompatibilityExtensions.cs` | 1-96 | Legacy SharpCifs helper extensions. | Local utility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\SmbAuthException.cs` | 1-12 | Compatibility exception type. | Auth error propagation. |
| `src\Opc.Classic.Dcom\Common\Ntlm\SmbException.cs` | 1-20 | Compatibility exception type. | Protocol error propagation. |
| `src\Opc.Classic.Dcom\Common\Ntlm\SmbNamedPipe.cs` | 1-43 | Compatibility stream wrapper for named-pipe-style I/O. | Local stream boundary. |
| `src\Opc.Classic.Dcom\Common\Ntlm\SmbSession.cs` | 1-12 | Compatibility session stub. | None. |
| `src\Opc.Classic.Dcom\Common\Ntlm\StringTokenizer.cs` | 1-21 | Compatibility tokenizer. | Local parsing utility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Thread.cs` | 1-51 | Compatibility thread wrapper. | Local threading utility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\ThreadGroup.cs` | 1-27 | Compatibility thread-group wrapper. | Local threading utility. |
| `src\Opc.Classic.Dcom\Common\Ntlm\UniAddress.cs` | 1-13 | Compatibility address holder. | Name/address input. |
| `src\Opc.Classic.Dcom\Common\Ntlm\UnknownHostException.cs` | 1-14 | Compatibility exception type. | Name-resolution error propagation. |
| `src\Opc.Classic.Dcom\Common\Ntlm\UnsupportedEncodingException.cs` | 1-10 | Compatibility exception type. | Encoding error propagation. |
| `src\Opc.Classic.Dcom\Common\Ntlm\Uuid.cs` | 1-43 | Compatibility UUID wrapper. | Local identifier parsing. |

### 3.3 Kerberos companion surface

| File | Lines | Purpose | Trust boundary |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom.Kerberos\IKerberosAuthInfo.cs` | 1-32 | Public Kerberos realm/SPN/user contract. | Caller configuration to Kerberos client. |
| `src\Opc.Classic.Dcom.Kerberos\KerberosAuthInfo.cs` | 1-63 | Immutable Kerberos auth configuration with password/keytab options. | Caller secrets/configuration boundary. |
| `src\Opc.Classic.Dcom.Kerberos\IKerberosConnectionContext.cs` | 1-42 | AP-REQ/AP-REP handshake abstraction. | KDC/service-ticket boundary. |
| `src\Opc.Classic.Dcom.Kerberos\KerberosConnectionContext.cs` | 1-203 | Kerberos.NET-backed ticket acquisition, AP-REP validation, GSS token extraction, CBT checksum injection. | Client to KDC and service. |
| `src\Opc.Classic.Dcom.Kerberos\IKerberosSession.cs` | 1-50 | RFC 4121 MIC/Wrap session abstraction. | Protected PDU body boundary. |
| `src\Opc.Classic.Dcom.Kerberos\KerberosSession.cs` | 1-561 | RFC 4121 MIC/Wrap and RC4-HMAC/AES packet protection. | Kerberos session key to network tokens. |
| `src\Opc.Classic.Dcom.Kerberos\KerberosSessionKey.cs` | 1-20 | Session-key metadata record. | AP exchange to packet protection. |
| `src\Opc.Classic.Dcom.Kerberos\KerberosChannelBindingChecksum.cs` | 1-40 | MS-KILE GSS channel-binding checksum builder. | TLS channel binding to Kerberos AP-REQ. |
| `src\Opc.Classic.Dcom.Kerberos\KerberosAuthContext.cs` | 1-199 | DCOM `IAuthContext` implementation for Kerberos/SPNEGO. | DCOM bind/call to Kerberos/SPNEGO. |

### 3.4 SPNEGO surface

There is no separate DCOM SPNEGO directory in this checkout.
SPNEGO code is under `src\Opc.Classic.Dcom.Kerberos\Spnego\`.
| File | Lines | Purpose | Trust boundary |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\IGssMicProvider.cs` | 1-29 | Mechanism-independent MIC provider contract. | Inner mechanism to SPNEGO verifier. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\KerberosMicProvider.cs` | 1-78 | Kerberos-backed `mechListMIC` provider. | Kerberos session to SPNEGO response. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoDecoder.cs` | 1-224 | DER decoder for NegTokenInit/NegTokenResp, preserving MechTypeList bytes. | Untrusted SPNEGO token to typed fields. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoEncoder.cs` | 1-157 | DER encoder for NegTokenInit/NegTokenResp and mechListMIC creation. | Local negotiation state to network token. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoMech.cs` | 1-28 | Mechanism enum. | Policy/display helper. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoNegState.cs` | 1-32 | RFC 4178 negotiation-state enum. | SPNEGO response policy. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoNegTokenInit.cs` | 1-22 | NegTokenInit record, including exact MechTypeList bytes. | Initiator token model. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoNegTokenResp.cs` | 1-35 | NegTokenResp record and mechListMIC verification. | Acceptor token model. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoOids.cs` | 1-27 | SPNEGO, Kerberos, and NTLMSSP OID constants. | Mechanism-selection policy. |
| `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoTokenBuilder.cs` | 1-49 | Kerberos-preferred token builder offering Kerberos then NTLMSSP. | Mechanism-list downgrade boundary. |

## 4. Cryptographic primitives in use

| Primitive | Used for | Implementation | RFC/spec reference | Test coverage |
| --- | --- | --- | --- | --- |
| MD4 | NTOWFv1 / NT hash = MD4(UTF-16LE password). | `src\Opc.Classic.Dcom\Crypto\Md4.cs:21-47`, `Md4State.cs:37-200`, `MD4Digest.cs:10-25`. | RFC 1320; MS-NLMP §3.3.1. | `tests\Opc.Classic.Dcom.Crypto.Tests\Md4Tests.cs:18-103`. |
| HMAC-MD5 | NTOWFv2, LMv2/NTLMv2 proof, MIC, message signatures. | BCL `System.Security.Cryptography.HMACMD5` in `Responses.cs:270-275`, `NtlmMic.cs:25-26`, `NtlmMessageSignature.cs:40-42`. | RFC 2104; MS-NLMP §3.3.2 and §3.4.4. | `NtlmV2ServerKeyDerivationTests.cs:51-109`, `NtlmMicTests.cs:24-116`. |
| MD5 | NTLM2-session hash and key-magic digest wrappers. | BCL via `MD5Digest.cs:12-29`; `Responses.cs:97-107`; `NTLMKeyFactory.cs:126-195`. | RFC 1321; MS-NLMP session security. | Indirect through NTLMv2/key tests; no standalone MD5 test needed because BCL-backed. |
| RC4 / ARCFOUR | NTLM packet sealing and exported-session-key wrapping. | `src\Opc.Classic.Dcom\Crypto\Rc4.cs:27-65`, `RC4Engine.cs:11-46`, `NTLMKeyFactory.cs:62-80`. | MS-NLMP §3.4.5; RFC 6229 vectors for validation. | `tests\Opc.Classic.Dcom.Crypto.Tests\Rc4Tests.cs:17-125`. |
| DES-ECB no padding | LM/NTLMv1 legacy response construction only. | BCL DES wrapper `DesEcbNoPaddingCipher.cs:11-48`; `Responses.cs:116-133`, `177-195`, `308-347`. | MS-NLMP NTLMv1/LM compatibility. | No direct NTLMv1 vector test; NTLMv1 disabled by default. |
| SHA-256 / SHA-384 | TLS `tls-server-end-point` certificate digest. | BCL via `ChannelBindingsFactory`, summarized in `CHANNEL_BINDING.md:24-40`. | RFC 5929; RFC 5056. | `tests\Opc.Classic.Dcom.Tests\ChannelBindingTlsTests.cs:34-59`, `105-134`. |
| MD5 GSS channel-binding hash | RFC 2744 channel-bindings structure hash consumed by NTLM and Kerberos. | `ChannelBindingsHash.Compute`, summarized in `CHANNEL_BINDING.md:36-54`. | RFC 2744; MS-NLMP AV_PAIR `MsvAvChannelBindings`. | `ChannelBindingTlsTests.cs:61-102`; Kerberos CBT tests under `tests\Opc.Classic.Dcom.Kerberos.Tests\KerberosChannelBindingChecksumTests.cs`. |
| Random nonces/session keys | NTLMv2 client challenge and exported session key. | `NtlmAuthentication.cs:304-325`, `346-347`, `842`; `NTLMKeyFactory.cs:49-54`, `258`. | MS-NLMP nonce/session-key requirements. | Partially covered by round trips; randomness quality is a known audit focus. |
| Kerberos RC4-HMAC | Kerberos RFC 4757 per-message tokens. | `KerberosSession.cs:392-439`, `469-560`. | RFC 4757. | `tests\Opc.Classic.Dcom.Kerberos.Tests\Rfc4757Rc4HmacTests.cs`. |
| Kerberos AES CTS-HMAC | Kerberos RFC 4121 wrap/MIC for AES etypes. | `KerberosSession.cs:351-388` via Kerberos.NET crypto transformers. | RFC 3962; RFC 8009; RFC 4121. | `Rfc3962AesCtsTests.cs`, `Rfc8009AesShaaTests.cs`, `Rfc4121MicTokenTests.cs`, `Rfc4121WrapTokenTests.cs`. |
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
`NtlmMessage.cs:41-60` validates the signature and message type for all NTLM
messages.
`Type1Message.cs:51-83` serializes flags plus optional supplied domain and
workstation security buffers.
`Type1Message.cs:89-113` parses flags and security buffers, rejecting messages
shorter than 32 bytes.
`NtlmAuthentication.CreateType1` in `NtlmAuthentication.cs:239-249` builds the
client NEGOTIATE token from `DefaultFlags` and records the raw message for MIC
calculation.
`NtlmAuthentication.cs:441-469` builds default flags, including NTLM, always
sign, Unicode/OEM, optional sign/seal/key-exchange, 56-bit/128-bit flags, and
extended session security.
`NtlmAuthentication.cs:472-503` adjusts peer flags by local policy.
Auditor focus: verify unsupported or deprecated flags are not accidentally
honored after `AdjustFlags`.

### 5.2 CHALLENGE / Type 2 (`MS-NLMP` §2.2.1.2)

`Type2Message.cs:106-132` serializes target name, negotiate flags, 8-byte
server challenge, 8-byte context, target-info AV_PAIR bytes, and optional
version.
`Type2Message.cs:138-163` parses the same fields and rejects messages shorter
than 48 bytes.
`Type2Message.cs:81-88` enforces an 8-byte server challenge.
`Type2Message.cs:90-97` enforces an 8-byte context value.
`Type2Message.cs:68-75` creates default target-info containing the workstation
name and an EOL pair.
`NtlmAuthentication.CreateType2` in `NtlmAuthentication.cs:257-282` adjusts
client flags, marks target type server, uses the current server challenge, and
adds the MIC-required AV flag when key exchange and version are negotiated.
`NtlmAvPairs.cs:10-13` defines `MsvAvFlags`, `MsvAvChannelBindings`, and the MIC
flag.
`NtlmAvPairs.cs:15-29` detects and adds the MIC flag.
`NtlmAvPairs.cs:31-70` adds or replaces target-info AV_PAIRs with bounds checks.
Auditor focus: challenge generation, target-info length validation, and MIC flag
policy.

### 5.3 AUTHENTICATE / Type 3 (`MS-NLMP` §2.2.1.3)

`Type3Message.cs:169-209` serializes LM response, NT response, domain, user,
workstation, encrypted random session key, flags, optional version, and optional
MIC.
`Type3Message.cs:238-275` parses the same fields and computes the minimum
payload offset before treating version/MIC bytes as present.
`Type3Message.cs:154-161` enforces a 16-byte MIC.
`Type3Message.cs:211-227` zeroes the MIC field, computes the transcript MIC,
then writes the MIC back at offset 72.
`NtlmAuthentication.CreateType3` in `NtlmAuthentication.cs:291-415` is the main
client construction path.
For NTLMv2, `NtlmAuthentication.cs:317-329` generates the client nonce,
applies channel bindings to target info, and calls the response builders.
For legacy fallback, `NtlmAuthentication.cs:336-369` contains NTLM2-session and
plain NTLMv1 response code paths that require explicit opt-in.
`NtlmAuthentication.cs:373-415` derives the exported session key, optionally
wraps it with RC4 key exchange, creates packet-protection state, and adds the
MIC if required.
`Responses.cs:55-63` creates the NTLMv2 response from NTOWFv2, target info,
challenge, and client nonce.
`Responses.cs:227-259` creates the NTLMv2 blob with timestamp, nonce, target
info, and terminator bytes.
`NtlmAuthentication.cs:556-606` is the server-side Type3 session-security setup.
`NtlmAuthentication.cs:608-644` validates the NTLMv2 proof using
`CryptographicOperations.FixedTimeEquals`, derives the session base key, and
unwraps the encrypted random session key when key exchange was negotiated.
Auditor focus: prove the server verifies the exact challenge it issued, the
exact target-info/CBT bytes, and the MIC over the original Type1/Type2/Type3
transcript.

### 5.4 MIC and channel-binding flow

`NtlmAuthentication.cs:646-650` stores original NEGOTIATE and CHALLENGE bytes.
`NtlmAuthentication.cs:652-666` requires an exported session key and original
messages before computing a MIC.
`NtlmAuthentication.cs:668-685` verifies the MIC during server-side Type3
processing.
`NtlmMic.cs:13-27` computes `HMAC-MD5(sessionKey, negotiate || challenge ||
authenticate)`.
`NtlmMic.cs:29-43` zeroes the AUTHENTICATE MIC bytes and compares the expected
and actual MIC in fixed time.
`NtlmAuthentication.cs:701-720` inserts and validates `MsvAvChannelBindings`
when a 16-byte CBT hash is configured.
`CHANNEL_BINDING.md:42-54` explains the AV_PAIR encoding and no-TLS behavior.

### 5.5 Packet signing and sealing

`Ntlm1.cs:43-65` derives client/server signing and sealing keys from the
exported session key.
`Ntlm1.cs:77-129` decrypts/verifies inbound protected PDUs.
`Ntlm1.cs:139-174` signs and optionally seals outbound protected PDUs.
`NTLMKeyFactory.cs:126-195` derives signing and sealing keys using MS-NLMP magic
constants.
`NTLMKeyFactory.cs:207-247` builds the NTLM SIGNATURE_BLOCK with sequence number
and HMAC-MD5 checksum.
`NTLMKeyFactory.cs:256` currently compares packet signatures with
`SequenceEqual`; `THREAT_MODEL.md:237-240` calls out replacement with fixed-time
comparison as an open recommendation.
TODO(auditor): confirm all negotiated flag combinations that reach `Ntlm1` match
MS-NLMP §3.4.5, especially sign-only vs seal modes and key-exchange presence.

## 6. Test coverage map

| Source file or area | Has unit tests? | Has wire-fixture tests? | Has live-server tests? | Status |
| --- | --- | --- | --- | --- |
| `Crypto\Md4.cs`, `Md4State.cs`, `MD4Digest.cs` | ✅ | ✅ RFC 1320 Appendix A.5 | ❌ N/A | Good primitive vectors and incremental-state tests. |
| `Crypto\Rc4.cs`, `RC4Engine.cs` | ✅ | ✅ RFC 6229 §2.1 | ❌ N/A | Good KSA/PRGA and wrapper compatibility tests. |
| `Responses.cs` NTLMv2 derivation | ✅ partial | ✅ MS-NLMP §4.2.4.1 | ❌ | Covered by `NtlmV2ServerKeyDerivationTests.cs`. |
| `Responses.cs` LM/NTLMv1 | ❌ | ❌ | ❌ | Deferred because NTLMv1/LM are disabled by default. |
| `NTLMKeyFactory.cs` session-key derivation | ✅ partial | ✅ MS-NLMP vector | ❌ | Signing-key equality covered; random/session-key quality not directly tested. |
| `NTLMKeyFactory.cs` packet signatures | ✅ direct | ✅ MS-NLMP §3.4.4/§3.4.5 SIGNATURE_BLOCK vectors | ❌ | `NtlmSignatureBlockTests.cs` covers direct formation, mismatch rejection, replay, and wrap-boundary sequence handling. |
| `NtlmAuthentication.cs` default policy | ✅ | ❌ | ❌ | `NtlmDefaultsTests.cs` covers NTLMv2 defaults and NTLMv1 opt-in guard. |
| `NtlmAuthentication.cs` Type1/2/3 round trip | ✅ | ⚠️ partial | ❌ blocked by `rw-e1` live interop | Client/server in-memory round trips exist. |
| `NtlmAuthentication.cs` MIC | ✅ | ⚠️ synthetic vectors | ❌ | `NtlmMicTests.cs` covers compute, verify, tamper, fixed-time source check. |
| `NtlmAuthentication.cs` CBT | ✅ | ⚠️ synthetic TLS cert vectors | ❌ | `ChannelBindingTlsTests.cs` covers AV_PAIR insertion and matching server verification. |
| `Type1Message.cs` | ⚠️ indirect | ❌ | ❌ | Needs malformed security-buffer/fuzz coverage. |
| `Type2Message.cs` | ⚠️ indirect | ❌ | ❌ | Needs malformed target-info/fuzz coverage. |
| `Type3Message.cs` | ✅ partial | ⚠️ MS-NLMP fixture through Type3 construction | ❌ | MIC offset and parser paths tested; fuzz gaps remain. |
| `NtlmAvPairs.cs` | ✅ indirect | ❌ | ❌ | Covered through MIC/CBT tests; direct invalid-length tests recommended. |
| `NtlmMessageSignature.cs` | ✅ direct | ✅ MS-NLMP §3.4.4/§3.4.5 SIGNATURE_BLOCK vectors | ❌ | Covered directly by `NtlmSignatureBlockTests.cs` plus SPNEGO NTLM MIC provider paths. |
| `NtlmConnection.cs` / `NtlmConnectionContext.cs` | ❌ | ❌ | ❌ | Needs bind/auth verifier state-machine tests. |
| `AuthenticationSource.cs` / `NullAuthenticationSource.cs` | ❌ | ❌ | ❌ | Contract is fail-closed but should get server-host tests. |
| `KerberosAuthContext.cs` | ✅ | ⚠️ synthetic | ⚠️ integration KDC | Companion coverage under Kerberos test project. |
| `KerberosSession.cs` | ✅ | ✅ RFC 4121/4757/3962/8009 | ⚠️ integration KDC | Good separate Kerberos audit input. |
| `SpnegoEncoder.cs` / `SpnegoDecoder.cs` | ✅ | ✅ known DER fragments | ❌ | `SpnegoTests.cs` and `SpnegoNegTokenRespTests.cs`. |
| `SpnegoTokenBuilder.cs` | ✅ | ✅ OID constants | ❌ | Confirms Kerberos-first, NTLMSSP-fallback mech list. |
Test files of interest:
- `tests\Opc.Classic.Dcom.Crypto.Tests\Md4Tests.cs:18-103`.
- `tests\Opc.Classic.Dcom.Crypto.Tests\Rc4Tests.cs:17-125`.
- `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmV2ServerKeyDerivationTests.cs:51-109`.
- `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmMicTests.cs:24-116`.
- `tests\Opc.Classic.Dcom.Tests\Tests\NtlmDefaultsTests.cs:18-56`.
- `tests\Opc.Classic.Dcom.Tests\ChannelBindingTlsTests.cs:34-134`.
- `tests\Opc.Classic.Dcom.Kerberos.Tests\SpnegoTests.cs:15-75`.
- `tests\Opc.Classic.Dcom.Kerberos.Tests\SpnegoNegTokenRespTests.cs:22-146`.
- `tests\Opc.Classic.Dcom.Crypto.Tests\Fixtures\Ntlm\NtlmHandshakeFixtureTests.cs:44-97` plus `negotiate.bin`, `challenge.bin`, and `authenticate.bin` provide byte-exact MS-NLMP §4.2 fixture replay coverage.

## 7. Threat model addendum

### 7.1 NTLM relay — PARTIAL

Mitigations present:
- Packet integrity is the default for `OpcConnectData` (`OpcConnectData.cs:44-78`).
- Privacy can be selected through `OpcProtectionLevel.Privacy`.
- NTLM channel binding is inserted and validated when callers provide CBT
  (`NtlmAuthentication.cs:701-720`).
- MIC protects the Type1/Type2/Type3 transcript when requested
  (`NtlmAuthentication.cs:652-685`).
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
- `NtlmAuthentication.cs:37-69` defaults `_useNtlmV2` to true and throws when
  NTLMv2 is disabled without `rpc.ntlm.allowV1=true`.
- `NtlmAuthentication.cs:129-151` creates public `OpcConnectData` properties
  with `rpc.ntlm.ntlmv2=true` and `rpc.ntlm.allowV1=false`.
- `Ntlm1.cs:14-18` marks NTLMv1 session-security fallback obsolete and warns it
  is cryptographically broken.
- `NtlmDefaultsTests.cs:37-56` verifies the throw and explicit opt-in behavior.
Residual risk:
- The explicit opt-in remains for very old legacy targets.
- Audit should confirm no untrusted server flag can force `_useNtlmV2=false`.

### 7.3 Replay attacks — PARTIAL

Mitigations present:
- NTLMv2 proofs bind server challenge, timestamp, client nonce, and target-info
  blob (`Responses.cs:227-259`, `NtlmAuthentication.cs:608-644`).
- Packet protection uses monotonically increasing request/response counters
  (`Ntlm1.cs:119`, `169`, `176-183`).
- Kerberos companion sessions enforce receive sequence numbers
  (`KerberosSession.cs:304-334`).
Residual risk:
- `NtlmAuthentication.cs:694` currently uses a fixed default server challenge.
- `NTLMKeyFactory.cs:49-54` and `NtlmAuthentication.cs:304-325` use `Random`, not
  `RandomNumberGenerator`.
- There is no NTLM replay cache for seen challenges/timestamps/client nonces.
- `THREAT_MODEL.md:237-240` already marks randomness hardening as high priority.
Audit abuse case: replay a captured Type3 against the same and different server
challenge; verify expected failure once challenge randomness is hardened.

### 7.4 Dictionary / brute-force against NTOWF — PARTIAL

Mitigations present:
- NTLMv2 is the default and avoids LM/NTLMv1 by default.
- NTOWFv2 derives `HMAC-MD5(NTOWFv1, UppercaseUser || Target)` in
  `Responses.cs:160-164`.
- The server verifies proofs in fixed time (`NtlmAuthentication.cs:629-631`).
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
  (`NtlmAuthentication.cs:270-272`, `687-692`).
- Client computes MIC over original NEGOTIATE, CHALLENGE, and AUTHENTICATE
  (`Type3Message.cs:211-227`, `NtlmMic.cs:13-27`).
- Server verifies MIC in fixed time and rejects invalid or missing MIC
  (`NtlmAuthentication.cs:668-685`, `NtlmMic.cs:29-43`).
- `NtlmMicTests.cs:56-89` covers MIC insertion and tamper rejection.
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
  16-byte CBT hash is configured (`NtlmAuthentication.cs:701-720`).
- Kerberos carries the same hash in the AP-REQ checksum
  (`KerberosChannelBindingChecksum.cs:14-39`).
- Tests cover NTLM insertion, no-TLS behavior, and server-side matching
  (`ChannelBindingTlsTests.cs:61-102`).
Residual risk:
- CBT is optional and caller-supplied; no-TLS sessions omit it.
- TLS certificate validation is delegated to the hosting application and .NET.
- The audit should include CVE-2022-21841-style channel-binding bypass attempts,
  including all-zero CBT values, absent AV pairs, duplicate AV pairs, and
  mismatched TLS endpoints.

## 8. Known limitations and deferred items

- LM authentication is not a supported production path.
  `Responses.cs:23-39` and `116-133` retain LM/NTLMv1 compatibility helpers,
  but `NtlmAuthentication.cs:67-70` blocks NTLMv1 unless explicitly opted in.
- NTLMv1 challenge-response is security-deprecated.
  It exists only behind `rpc.ntlm.allowV1=true` for legacy targets.
- NTLM SSO / Windows SSPI is intentionally unsupported.
  `NtlmAuthentication.cs:72-82` throws `PlatformNotSupportedException` for
  `rpc.ntlm.sso=true` and points callers to Kerberos/SPNEGO.
- Server-side generic NTLM bind challenge handling is incomplete.
  `NtlmConnectionContext.cs:116-125` throws for server-side bind and
  alter-context challenge handling.
- `AuthenticationSource` is not wired into the main proof-validation path.
  `NtlmAuthentication.cs:420-439` contains the old reflection-based source hook
  as commented-out code, and `AuthenticationSource.cs:38-79` is currently a
  contract for future server-host integration.
- NTLM challenge and key randomness need hardening.
  `NtlmAuthentication.cs:694` uses a fixed challenge for the current server
  helper, and `NtlmAuthentication.cs:842` / `NTLMKeyFactory.cs:258` use
  `Random`.
- Secrets are not zeroized.
  Public credentials are strings/`NetworkCredential`, and derived byte arrays
  are not consistently cleared after use.
- Packet-signature comparison is not consistently fixed-time.
  `NtlmMic.cs:42`, `NtlmMessageSignature.cs:69`, and the NTLMv2 proof verifier
  use fixed-time comparison; `NTLMKeyFactory.cs:256` still uses `SequenceEqual`.
- Token-size ceilings and fuzzing gates are not release blockers.
  `NtlmMessage.cs:82-99` bounds security buffers, but aggregate authentication
  token size, AV_PAIR count, and malformed PDU fuzzing should be added.
- CBT is opt-in per connection.
  If callers do not provide `OpcConnectData.ChannelBindings`, NTLM channel
  bindings are omitted or treated as no-TLS behavior.
- Audit logging is incomplete.
  `THREAT_MODEL.md:231-244` lists security audit events and OpenTelemetry as
  open recommendations.

## 9. Audit scope recommendations

### 9.1 High-priority code paths

1. `src\Opc.Classic.Dcom\rpc\Auth\NtlmAuthentication.cs:291-415`.
   Review Type3 construction, nonce handling, key exchange, MIC insertion, and
   CBT insertion.
2. `src\Opc.Classic.Dcom\rpc\Auth\NtlmAuthentication.cs:556-644`.
   Review server-side Type3 validation, NT proof verification, session-base-key
   derivation, and encrypted-session-key handling.
3. `src\Opc.Classic.Dcom\rpc\Auth\Responses.cs:55-164` and `227-275`.
   Review NTLMv2 response math, timestamp encoding, identity canonicalization,
   and BCL HMAC use.
4. `src\Opc.Classic.Dcom\rpc\Auth\NTLMKeyFactory.cs:49-80`, `126-195`, and
   `207-256`.
   Review randomness, RC4 usage, signing/sealing key constants, SIGNATURE_BLOCK
   layout, and comparison behavior.
5. `src\Opc.Classic.Dcom\Common\Ntlm\Type1Message.cs`, `Type2Message.cs`,
   `Type3Message.cs`, and `NtlmMessage.cs`.
   Review parser bounds checks, security-buffer offset handling, duplicate or
   overlapping payloads, and optional version/MIC offsets.
6. `src\Opc.Classic.Dcom\Common\Ntlm\NtlmAvPairs.cs` and
   `NtlmAuthentication.cs:701-812`.
   Review AV_PAIR add/replace/read behavior, duplicate CBT handling, EOL
   handling, and malformed length behavior.
7. `src\Opc.Classic.Dcom\Crypto\Md4.cs`, `Md4State.cs`, `Rc4.cs`, and
   `RC4Engine.cs`.
   Review hand-rolled primitive correctness, state reuse, allocation behavior,
   and side-channel considerations.
8. `src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoEncoder.cs`,
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
- Suggested regression tests suitable for `tests\Opc.Classic.Dcom.Crypto.Tests`
  or `tests\Opc.Classic.Dcom.Tests`.

## 10. Cross-references

- `docs\security\THREAT_MODEL.md` — parent STRIDE assessment and open security
  recommendations.
- `docs\security\CHANNEL_BINDING.md` — RFC 5056/RFC 5929 CBT flow for NTLMv2 and
  Kerberos.
- `src\BannedSymbols.txt` — NativeAOT and cross-platform constraints that shape
  the implementation.
- `SECURITY.md` — responsible-disclosure policy and security-sensitive surface.
- `src\Opc.Classic.Core\OpcConnectData.cs` — public auth-mode and protection-level
  defaults.
- `src\Opc.Classic.Core\OpcProtectionLevel.cs` — DCE/RPC protection-level mapping
  and DCOM hardening note.
- `src\Opc.Classic.Dcom\rpc\Auth\NtlmAuthentication.cs` — primary NTLMSSP
  orchestrator.
- `src\Opc.Classic.Dcom\Common\Ntlm\` — NTLM message and MIC helpers.
- `src\Opc.Classic.Dcom\Crypto\` — MD4, RC4, and compatibility crypto wrappers.
- `src\Opc.Classic.Dcom.Kerberos\` — Kerberos and SPNEGO companion implementation.
- Microsoft NTLM overview — Kerberos is preferred in Active Directory, NTLM is
  retained for compatibility/workgroup scenarios.
- Microsoft "The evolution of Windows authentication" — platform direction is
  to reduce NTLM usage and improve stronger alternatives.
