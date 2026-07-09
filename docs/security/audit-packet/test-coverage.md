<!-- Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP test coverage

## Unit tests: Opc.Classic.Dcom.Crypto tests

| File | Coverage | High-value invariant |
| --- | --- | --- |
| `Md4Tests.cs` | Covered | RFC 1320 Appendix A.5 hashes and NT hash for `"password"` match expected bytes. |
| `Rc4Tests.cs` | Covered | RFC 6229 first 16 keystream bytes match for 40/56/64/80/128-bit keys; RC4 is self-inverse. |
| `NtlmV2ServerKeyDerivationTests.cs` | Covered | [MS-NLMP] §4.2.4.1 derives NTOWFv2, proof, session base key, and encrypted session key; wrong password throws `SecurityException`. |
| `NtlmMicTests.cs` | Covered | Type2 requests MIC, Type3 includes MIC at offset 72, tampered authenticate fails, MIC compare uses `CryptographicOperations.FixedTimeEquals`. |
| `NtlmSignatureBlockTests.cs` | Covered | SIGNATURE_BLOCK rejects tampered sequence/checksum/wrong key/replay and handles UInt32 wrap. |
| `NtlmNegotiateFlagsTests.cs` | Covered | SIGNKEY/SEALKEY/KXKEY combinations derive expected directional signing/sealing keys and protect payloads correctly. |
| `PasswordZeroizationTests.cs` | Covered | Password-derived pooled buffers are all zero before return. |
| `Fixtures\Ntlm\NtlmHandshakeFixtureTests.cs` | Covered | [MS-NLMP] sample negotiate/challenge/authenticate fixture bytes marshal/unmarshal and preserve flags. |

## Additional NTLM defaults

| File | Coverage | High-value invariant |
| --- | --- | --- |
| `NtlmDefaultsTests` | Covered | `rpc.ntlm.ntlmv2` and `rpc.ntlm.ntlm2` default true; NTLMv1 without explicit opt-in throws. |

## Property tests: Opc.Classic tests

| File | Coverage | High-value invariant |
| --- | --- | --- |
| `InvariantProperties.cs` (`CryptoProperties`) | Covered | MD4 always returns 16 bytes and is deterministic; RC4 is self-inverse for generated keys/data and output length equals input length. |

## Fuzz tests: FZ track

| File | Coverage | Allowed exception set / invariant |
| --- | --- | --- |
| `NtlmFuzzTests` | Covered, including corpus replay | Random and mutated Type1/Type2/Type3, AV-pair scans, and MIC verification must not escape `InvalidDataException`, `ArgumentException`, `ArgumentOutOfRangeException`, `FormatException`, or `EndOfStreamException`. |

The parent threat model records the same NTLM fuzz surface at `docs\security\THREAT_MODEL.md:238`.

## Handshake fixture tests

`NtlmHandshakeFixtureTests` fixture coverage highest-value invariant: `authenticate.bin` round-trips NT response, LM response, identity fields, encrypted session key, and [MS-NLMP] sample flag semantics.

## Integration tests

`NtlmHandshakeProtocolTests` provides `[Test, Category("EndToEnd")]` protocol coverage. Highest-value invariants:

- Type1 has expected NTLMSSP header and negotiated NTLMv2/sign/seal/key-exchange flags.
- Type2 carries challenge, target-info, and MIC-request AV flag.
- Type3 carries NTLMv2 proof/blob/session key/MIC; client and server session keys match.
- Tampered Type3 MIC and wrong passwords throw `SecurityException`.
- CBT is embedded as `MsvAvChannelBindings`; mismatched CBT is rejected.
- `CreateAuthContext` maps anonymous, NTLMv2, Kerberos, and Windows SSO modes as expected.

## Channel-binding coverage

| File | Coverage | High-value invariant |
| --- | --- | --- |
| `ChannelBindingsTests` | Covered | RFC 2744 serialized empty struct hash is stable; `tls-server-end-point:` application data prefix is exact; hash length is 16. |
| `ChannelBindingTlsTests` | Covered | Fixed SHA-256/SHA-384 certificates produce expected CBT application data; NTLM Type3 includes CBT AV pair; the protocol verifier rejects mismatches; SslStream loopback extracts certificate CBT. |

## Adjacent but out-of-scope

- `KerberosChannelBindingChecksumTests` and `Rfc4757Rc4HmacTests.cs` cover Kerberos/SPNEGO-adjacent crypto, not NTLMSSP itself.
