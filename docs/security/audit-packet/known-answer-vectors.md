<!-- Copyright (c) 2026 marcschier. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# Known-answer vectors

## Unit-test vector map

| Test class | Reference source | Where |
| --- | --- | --- |
| `Md4Tests` | RFC 1320 Appendix A.5 and NT hash example. Quote: `Test vectors come from RFC 1320 Appendix A.5 (the canonical MD4 reference).` | `Md4Tests` |
| `Rc4Tests` | RFC 6229 §2.1 RC4 vectors. Quote: `Test vectors come from RFC 6229 §2.1 — the IETF reference RC4 test vectors.` | `Rc4Tests` |
| `NtlmV2ServerKeyDerivationTests` | [MS-NLMP] §4.2.4.1 NTLMv2 sample vectors. Quote: `MS-NLMP §4.2.4.1 NTLMv2 sample vectors and server-side key derivation tests.` | `NtlmV2ServerKeyDerivationTests` |
| `NtlmMicTests` | Fixed HMAC-MD5 vector plus full client/server MIC round-trip and tamper rejection. | `NtlmMicTests` |
| `NtlmSignatureBlockTests` | [MS-NLMP] §3.4.4 SIGNATURE_BLOCK and §3.4.5 HMAC-MD5 checksum. Quote: `MS-NLMP §3.4.4 defines the 16-byte SIGNATURE_BLOCK layout, and §3.4.5 defines the HMAC-MD5 checksum over SeqNum || Message used by extended session security signing.` | `NtlmSignatureBlockTests` |
| `NtlmNegotiateFlagsTests` | [MS-NLMP] §3.4.5.1 KXKEY, §3.4.5.2 SIGNKEY, §3.4.5.3 SEALKEY, §3.4.4.2 MAC. | `NtlmNegotiateFlagsTests` |
| `PasswordZeroizationTests` | Internal invariant: password-derived pooled buffers are zeroed before return. | `PasswordZeroizationTests` |

## Handshake fixtures

Fixture binaries live under Ntlm tests:

- `negotiate.bin` — 46 bytes.
- `challenge.bin` — 104 bytes.
- `authenticate.bin` — 232 bytes.
- `NtlmHandshakeFixtureTests.cs` — 5 tests replay [MS-NLMP] §4.2.4.1 handshake bytes.

Quote: `MS-NLMP §4.2.4.1 NTLMv2 sample handshake fixture replay tests.`

The fixture test also cites [MS-NLMP] §4.2.4.3 for authenticate-message field ordering and §4.2.4.2.3 for encrypted session-key vector stability (`NtlmHandshakeFixtureTests`).

## Channel-binding vectors

- Empty GSS channel-bindings MD5: `441018525208457705BF09A8EE3C1093` in `ChannelBindingsTests`.
- TLS `tls-server-end-point` SHA-256/SHA-384 certificates in `ChannelBindingTlsTests`.
- NTLMv2 CBT AV-pair insertion and server verification in `ChannelBindingTlsTests`.

## Kerberos RFC 4757 note

RFC 4757 RC4-HMAC vectors are tested separately in `Rfc4757Rc4HmacTests`; they are adjacent crypto coverage, not part of the NTLMSSP password-auth flow.
