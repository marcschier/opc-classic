<!-- Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP limitations and design choices

## Server-side listener bind auth status

Server-side NTLMv2 bind challenge handling is implemented in the managed listener when it is configured with an `AuthenticationSource`. `RpcServerConnectionProcessor` issues the Type2 challenge, completes Type3 verification through `ConfiguredAuthenticationSource`, and then uses the established NTLM session security for per-PDU integrity/privacy.

`F4Auth` now exercises NTLMv2 authenticated managed-loopback calls, wrong-password rejection, required-auth anonymous/plain-request rejection, and privacy-mode sealing. Its remaining skipped listener-auth tests are Kerberos and SPNEGO acceptor paths.

Result: clients can drive authenticated binds against external servers, and managed servers can require NTLMv2 authenticated binds when a credential source is configured. The remaining listener-auth gap is server-side Kerberos/SPNEGO acceptor wiring.

## Hand-rolled MD4 and RC4

MD4 and RC4 are implemented in-tree for NTLM compatibility:

- `Md4`
- `Md4State`
- `Rc4`
- `MD4Digest`
- `RC4Engine`

RC4 keystream state is indirectly covered by direction/key-derivation tests in `NtlmNegotiateFlagsTests.cs`; password-derived temporary buffers are zeroized through `SensitiveBufferPool.Return` and tested in `PasswordZeroizationTests.cs`.

MD4 is deterministic and RFC-vector tested, including incremental state. Reviewers should still treat MD4 as a hand-rolled legacy primitive requiring close inspection.

## NTLMv2-only default posture

NTLMv1 is disabled by default. `NtlmAuthentication` throws when `rpc.ntlm.ntlmv2=false` unless `rpc.ntlm.allowV1=true`. `NtlmDefaultsTests.cs` asserts this. `Ntlm1` remains because the session-security implementation name is legacy and because explicit opt-in compatibility tests cover old flag combinations.

## Channel binding depends on caller-provided TLS evidence

`ChannelBindingsFactory` and `ChannelBindingsHash` can compute RFC 5056/RFC 2744/RFC 5929-style CBT hashes, but TLS certificate validation and endpoint trust are delegated to .NET and the hosting application. The NTLM protocol verifier checks hash equality once configured; `ConfiguredAuthenticationSource` passes the configured CBT hash into server-side NTLMv2 bind verification.

## SMB3 encryption status

SMB3 AES-128-CCM/GCM is adjacent to named-pipe transport, not NTLMSSP. Current docs conflict historically: `docs\security\THREAT_MODEL.md:308` and architecture docs describe SMB3 encryption support, while `Opc.Classic.Dcom.Smb` still says deferred. Treat SMB3 encryption as out of this NTLM audit unless the reviewer is asked to audit SMB transport separately.

## Audit track boundary

This is the auth track prepared for 1.0.0 NTLMSSP review. Kerberos and SPNEGO are tracked separately, though CBT helpers are shared and referenced here because NTLMv2 uses `MsvAvChannelBindings`.
