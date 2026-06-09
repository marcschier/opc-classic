<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP limitations and design choices

## Server-side listener bind auth is not implemented

Server-side NTLM bind challenge handling is **not implemented** in the managed listener. `NtlmConnectionContext.Accept` throws `RpcException` for inbound `BindPdu` and `AlterContextPdu` with the messages `Server-side NTLM bind challenge handling is not implemented.` and `Server-side NTLM alter-context challenge handling is not implemented.` (`src\Opc.Classic.Dcom\rpc\Auth\NtlmConnectionContext.cs:116-126`).

`tests\Opc.Classic.Integration.Tests\Loopback\F4Auth.cs:12-25` documents the skip reason: authenticated calls over the managed TCP listener are not yet supported; `RpcServerConnectionProcessor` rejects authenticated binds unless a dispatcher consumes RPC auth context, and protocol-level NTLMv2 handshake coverage lives in `NtlmHandshakeProtocolTests`.

Result: clients can drive authenticated binds against external servers; servers in this stack are anonymous-only at listener level today.

## Hand-rolled MD4 and RC4

MD4 and RC4 are implemented in-tree for NTLM compatibility:

- `src\Opc.Classic.Dcom\Crypto\Md4.cs`
- `src\Opc.Classic.Dcom\Crypto\Md4State.cs`
- `src\Opc.Classic.Dcom\Crypto\Rc4.cs`
- `src\Opc.Classic.Dcom\Crypto\MD4Digest.cs`
- `src\Opc.Classic.Dcom\Crypto\RC4Engine.cs`

RC4 keystream state is indirectly covered by direction/key-derivation tests in `NtlmNegotiateFlagsTests.cs`; password-derived temporary buffers are zeroized through `SensitiveBufferPool.Return` (`src\Opc.Classic.Dcom\rpc\Auth\SensitiveBufferPool.cs:23-34`) and tested in `PasswordZeroizationTests.cs`.

MD4 is deterministic and RFC-vector tested, including incremental state. Reviewers should still treat MD4 as a hand-rolled legacy primitive requiring close inspection.

## NTLMv2-only default posture

NTLMv1 is disabled by default. `NtlmAuthentication` throws when `rpc.ntlm.ntlmv2=false` unless `rpc.ntlm.allowV1=true` (`src\Opc.Classic.Dcom\rpc\Auth\NtlmAuthentication.cs:67-70`). `NtlmDefaultsTests.cs` asserts this. `Ntlm1` remains because the session-security implementation name is legacy and because explicit opt-in compatibility tests cover old flag combinations.

## Channel binding depends on caller-provided TLS evidence

`ChannelBindingsFactory` and `ChannelBindingsHash` can compute RFC 5056/RFC 2744/RFC 5929-style CBT hashes, but TLS certificate validation and endpoint trust are delegated to .NET and the hosting application. The NTLM code verifies hash equality once configured.

## SMB3 encryption status

SMB3 AES-128-CCM/GCM is adjacent to named-pipe transport, not NTLMSSP. Current docs conflict historically: `docs\security\THREAT_MODEL.md:308` and architecture docs describe SMB3 encryption support, while `src\Opc.Classic.Dcom.Smb\README.md:25-29` still says deferred. Treat SMB3 encryption as out of this NTLM audit unless the reviewer is asked to audit SMB transport separately.

## Audit track boundary

This is the auth track prepared for 1.0.0 NTLMSSP review. Kerberos and SPNEGO are tracked separately, though CBT helpers are shared and referenced here because NTLMv2 uses `MsvAvChannelBindings`.
