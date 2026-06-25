<!-- Copyright (c) 2026 marcschier. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP threat-model extract

## Source

Parent STRIDE model: `docs\security\THREAT_MODEL.md`.

This packet intentionally extracts only NTLM-relevant headings and evidence, not the whole document.

## Quoted NTLM-relevant headings

- `docs\security\THREAT_MODEL.md:114`: `### 2.3 Level 2 auth-flow DFD: NTLMv2 negotiation`
- `docs\security\THREAT_MODEL.md:174`: `## 3. STRIDE analysis per major flow`
- `docs\security\THREAT_MODEL.md:187`: `### 3.2 Flow: Authenticate`
- `docs\security\THREAT_MODEL.md:220`: `## 4. Threat-specific mitigation evidence by STRIDE category`
- `docs\security\THREAT_MODEL.md:231`: `### 4.1 Fuzz coverage of attacker-controlled parsers`

## Extracted NTLM flow

The NTLM flow in `docs\security\THREAT_MODEL.md:114-145` has credentials entering the client NTLM auth context; Type1, Type2, and Type3 tokens crossing the network in DCE/RPC bind/auth PDUs; server-side verification of NT proof, MIC, and CBT when the managed listener is configured with `ConfiguredAuthenticationSource`; and later signed/sealed request/response PDUs.

Implementation anchors listed there are:

- `Type1Message`
- `Type2Message`
- `Type3Message`
- `NtlmAuthentication`
- `NTLMKeyFactory`
- `Ntlm1`

## Attacker model

- Network MITM can replay, reorder, truncate, mutate, downgrade, or inject DCE/RPC fragments and NTLMSSP tokens.
- Malicious server can send crafted Type2 target-info AV pairs, flags, challenge bytes, and CBT expectations.
- Malicious client can send crafted Type1/Type3 fields, invalid security-buffer offsets, malformed AV pairs, MIC omissions, wrong proofs, and oversized tokens.
- Replay attacker can reuse Type3 responses, packet signatures, or sealed payloads if sequence/key handling is wrong.
- Downgrade attacker attempts NTLMv1, lower key strengths, no signing/sealing, missing MIC, or missing CBT.

## Trust boundaries defended

- Client credential boundary: `NetworkCredential` / password-derived material enters `NtlmAuthentication` and `Responses`.
- Network token boundary: untrusted bytes enter `Type1Message`, `Type2Message`, `Type3Message`, `NtlmAvPairs`, and `NtlmMic.Verify`.
- Channel-binding boundary: TLS endpoint evidence becomes `ChannelBindingsHash` and then `MsvAvChannelBindings`.
- Packet-protection boundary: DCE/RPC body bytes and auth verifier bytes enter `SignAndSeal` / `VerifyAndUnseal`.
- Server credential-store boundary: `AuthenticationSource.CreateChallenge` and `AuthenticationSource.Authenticate` bridge protocol verifier code to application credential policy; `RpcServerConnectionProcessor` invokes this boundary for inbound NTLM binds when an authentication source is configured.

## STRIDE evidence to cross-check

- Spoofing: NTLMv2 default, CBT support, but NTLM is not Kerberos-style mutual authentication.
- Tampering: parsers validate signatures, message types, lengths, security-buffer bounds, MIC, and CBT (`docs\security\THREAT_MODEL.md:192`).
- Information disclosure: privacy mode exists, but credentials are still supplied as strings.
- DoS: malformed token parsing has bounds checks but still needs token-size ceilings and rate limits (`docs\security\THREAT_MODEL.md:195`).
- Elevation of privilege: NTLMv1 downgrade is blocked by default (`docs\security\THREAT_MODEL.md:196`).

## Fuzz coverage extract

The NTLM fuzz row is `docs\security\THREAT_MODEL.md:238`: Type1/Type2/Type3, AV pairs, and MIC are fuzzed by `NtlmFuzzTests`; allowed parser exceptions are `InvalidDataException`, `ArgumentException`, `ArgumentOutOfRangeException`, `FormatException`, and `EndOfStreamException`.
