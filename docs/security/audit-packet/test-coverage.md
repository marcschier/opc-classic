<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP test coverage

## Unit tests: Opc.Classic.Dcom.Crypto tests

| File | Tests | High-value invariant |
| --- | ---: | --- |
| `Md4Tests.cs` | 5 test methods / 11 cases | RFC 1320 Appendix A.5 hashes and NT hash for `"password"` match expected bytes. |
| `Rc4Tests.cs` | 6 methods / 10 cases | RFC 6229 first 16 keystream bytes match for 40/56/64/80/128-bit keys; RC4 is self-inverse. |
| `NtlmV2ServerKeyDerivationTests.cs` | 4 | [MS-NLMP] §4.2.4.1 derives NTOWFv2, proof, session base key, and encrypted session key; wrong password throws `SecurityException`. |
| `NtlmMicTests.cs` | 6 | Type2 requests MIC, Type3 includes MIC at offset 72, tampered authenticate fails, MIC compare uses `CryptographicOperations.FixedTimeEquals`. |
| `NtlmSignatureBlockTests.cs` | 7 methods / 9 cases | SIGNATURE_BLOCK rejects tampered sequence/checksum/wrong key/replay and handles UInt32 wrap. |
| `NtlmNegotiateFlagsTests.cs` | 5 methods / 17 cases | SIGNKEY/SEALKEY/KXKEY combinations derive expected directional signing/sealing keys and protect payloads correctly. |
| `PasswordZeroizationTests.cs` | 1 method / 7 cases | Password-derived pooled buffers are all zero before return. |
| `Fixtures\Ntlm\NtlmHandshakeFixtureTests.cs` | 5 | [MS-NLMP] sample negotiate/challenge/authenticate fixture bytes marshal/unmarshal and preserve flags. |

## Additional NTLM defaults

| File | Tests | High-value invariant |
| --- | ---: | --- |
| `NtlmDefaultsTests` | 4 | `rpc.ntlm.ntlmv2` and `rpc.ntlm.ntlm2` default true; NTLMv1 without explicit opt-in throws. |

## Property tests: Opc.Classic tests

| File | Tests | High-value invariant |
| --- | ---: | --- |
| `InvariantProperties.cs` (`CryptoProperties`) | 4 crypto property tests | MD4 always returns 16 bytes and is deterministic; RC4 is self-inverse for generated keys/data and output length equals input length. |

## Fuzz tests: FZ track

| File | Cases | Allowed exception set / invariant |
| --- | ---: | --- |
| `NtlmFuzzTests` | 9 methods / 13 surfaces including corpus replay | Random and mutated Type1/Type2/Type3, AV-pair scans, and MIC verification must not escape `InvalidDataException`, `ArgumentException`, `ArgumentOutOfRangeException`, `FormatException`, or `EndOfStreamException`. |

The parent threat model records the same NTLM fuzz surface at `docs\security\THREAT_MODEL.md:238`.

## Handshake fixture tests

`NtlmHandshakeFixtureTests` has 5 tests. Highest-value invariant: `authenticate.bin` round-trips NT response, LM response, identity fields, encrypted session key, and [MS-NLMP] sample flag semantics.

## Integration tests

`NtlmHandshakeProtocolTests` has 6 `[Test, Category("EndToEnd")]` methods. Highest-value invariants:

- Type1 has expected NTLMSSP header and negotiated NTLMv2/sign/seal/key-exchange flags.
- Type2 carries challenge, target-info, and MIC-request AV flag.
- Type3 carries NTLMv2 proof/blob/session key/MIC; client and server session keys match.
- Tampered Type3 MIC and wrong passwords throw `SecurityException`.
- CBT is embedded as `MsvAvChannelBindings`; mismatched CBT is rejected.
- `CreateAuthContext` maps anonymous, NTLMv2, Kerberos, and Windows SSO modes as expected.

## Channel-binding coverage

| File | Tests | High-value invariant |
| --- | ---: | --- |
| `ChannelBindingsTests` | 4 | RFC 2744 serialized empty struct hash is stable; `tls-server-end-point:` application data prefix is exact; hash length is 16. |
| `ChannelBindingTlsTests` | 6 | Fixed SHA-256/SHA-384 certificates produce expected CBT application data; NTLM Type3 includes CBT AV pair; server verifies matching CBT; SslStream loopback extracts certificate CBT. |

## Adjacent but out-of-scope

- `KerberosChannelBindingChecksumTests` and `Rfc4757Rc4HmacTests.cs` cover Kerberos/SPNEGO-adjacent crypto, not NTLMSSP itself.
