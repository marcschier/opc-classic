<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# Known-answer vectors

## Unit-test vector map

| Test class | Reference source | Where |
| --- | --- | --- |
| `Md4Tests` | RFC 1320 Appendix A.5 and NT hash example. Quote: `Test vectors come from RFC 1320 Appendix A.5 (the canonical MD4 reference).` | `tests\Opc.Classic.Dcom.Crypto.Tests\Md4Tests.cs:5-7`, `18-27`, `36-42` |
| `Rc4Tests` | RFC 6229 §2.1 RC4 vectors. Quote: `Test vectors come from RFC 6229 §2.1 — the IETF reference RC4 test vectors.` | `tests\Opc.Classic.Dcom.Crypto.Tests\Rc4Tests.cs:5-7`, `17-26` |
| `NtlmV2ServerKeyDerivationTests` | [MS-NLMP] §4.2.4.1 NTLMv2 sample vectors. Quote: `MS-NLMP §4.2.4.1 NTLMv2 sample vectors and server-side key derivation tests.` | `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmV2ServerKeyDerivationTests.cs:5`, `51-65` |
| `NtlmMicTests` | Fixed HMAC-MD5 vector plus full client/server MIC round-trip and tamper rejection. | `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmMicTests.cs:24-116` |
| `NtlmSignatureBlockTests` | [MS-NLMP] §3.4.4 SIGNATURE_BLOCK and §3.4.5 HMAC-MD5 checksum. Quote: `MS-NLMP §3.4.4 defines the 16-byte SIGNATURE_BLOCK layout, and §3.4.5 defines the HMAC-MD5 checksum over SeqNum || Message used by extended session security signing.` | `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmSignatureBlockTests.cs:18-31` |
| `NtlmNegotiateFlagsTests` | [MS-NLMP] §3.4.5.1 KXKEY, §3.4.5.2 SIGNKEY, §3.4.5.3 SEALKEY, §3.4.4.2 MAC. | `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmNegotiateFlagsTests.cs:28-146` |
| `PasswordZeroizationTests` | Internal invariant: password-derived pooled buffers are zeroed before return. | `tests\Opc.Classic.Dcom.Crypto.Tests\PasswordZeroizationTests.cs:29-51` |

## Handshake fixtures

Fixture binaries live under `tests\Opc.Classic.Dcom.Crypto.Tests\Fixtures\Ntlm\`:

- `negotiate.bin` — 46 bytes.
- `challenge.bin` — 104 bytes.
- `authenticate.bin` — 232 bytes.
- `NtlmHandshakeFixtureTests.cs` — 5 tests replay [MS-NLMP] §4.2.4.1 handshake bytes.

Quote: `MS-NLMP §4.2.4.1 NTLMv2 sample handshake fixture replay tests.` (`tests\Opc.Classic.Dcom.Crypto.Tests\Fixtures\Ntlm\NtlmHandshakeFixtureTests.cs:5`)

The fixture test also cites [MS-NLMP] §4.2.4.3 for authenticate-message field ordering and §4.2.4.2.3 for encrypted session-key vector stability (`NtlmHandshakeFixtureTests.cs:103-129`).

## Channel-binding vectors

- Empty GSS channel-bindings MD5: `441018525208457705BF09A8EE3C1093` in `tests\Opc.Classic.Core.Tests\Security\ChannelBindingsTests.cs:18-33`.
- TLS `tls-server-end-point` SHA-256/SHA-384 certificates in `tests\Opc.Classic.Dcom.Tests\ChannelBindingTlsTests.cs:34-59`.
- NTLMv2 CBT AV-pair insertion and server verification in `tests\Opc.Classic.Dcom.Tests\ChannelBindingTlsTests.cs:61-102`.

## Kerberos RFC 4757 note

RFC 4757 RC4-HMAC vectors are tested separately in `tests\Opc.Classic.Dcom.Kerberos.Tests\Rfc4757Rc4HmacTests.cs`; they are adjacent crypto coverage, not part of the NTLMSSP password-auth flow.
