<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP audit scope

## In scope

- NTLMSSP message exchange: Type1 NEGOTIATE, Type2 CHALLENGE, Type3 AUTHENTICATE in `src\Opc.Classic.Dcom\Common\Ntlm\Type1Message.cs`, `Type2Message.cs`, `Type3Message.cs`.
- AV-pair processing, MIC flag handling, MIC computation/verification, and channel-binding AV pairs in `src\Opc.Classic.Dcom\Common\Ntlm\NtlmAvPairs.cs`, `NtlmMic.cs`, and `src\Opc.Classic.Core\Security\ChannelBindings*.cs`.
- NTLMv2 response computation: HMAC-MD5, NTOWFv2, LMOWFv2, timestamp/nonce blob construction, and server proof verification in `src\Opc.Classic.Dcom\rpc\Auth\Responses.cs` and `NtlmAuthentication.cs`.
- Session-key derivation and key-exchange wrapping in `src\Opc.Classic.Dcom\rpc\Auth\NTLMKeyFactory.cs`.
- DCE/RPC `PKT_INTEGRITY` / `PKT_PRIVACY` signing and sealing in `src\Opc.Classic.Dcom\rpc\Auth\Ntlm1.cs`, `NtlmAuthentication.cs:155-233`, and `src\Opc.Classic.Dcom\Transport\DcomCallChannel.cs:402-445`.
- MD4 and RC4 hand-rolled primitives and compatibility wrappers in `src\Opc.Classic.Dcom\Crypto\Md4*.cs`, `Rc4.cs`, `MD4Digest.cs`, and `RC4Engine.cs`.
- Password-derived buffer zeroization in `src\Opc.Classic.Dcom\rpc\Auth\SensitiveBufferPool.cs:17-34`, covered by `tests\Opc.Classic.Dcom.Crypto.Tests\PasswordZeroizationTests.cs`.
- DCE/RPC bind-time auth verifier wrapping: `src\Opc.Classic.Dcom\rpc\Auth\NtlmConnection.cs:44-124`, `NtlmConnectionContext.cs`, `src\Opc.Classic.Dcom\Transport\PduCodec.cs`, and `DcomCallChannel.cs:342-503`.

## Out of scope

- Kerberos and SPNEGO protocol review; those are separate audit tracks if needed.
- TLS and certificate-chain validation; the CBT input depends on .NET BCL certificate/TLS APIs.
- OPC DA/AE/HDA/Batch/Commands/Security/DX/XML-DA behavior above the authentication layer.
- The managed listener's anonymous-bind path and application-level authorization policy.

## Non-goals

- Cryptographic primitive analysis is limited to RFC/spec adherence unless the primitive is hand-rolled here.
- HMAC-MD5, MD5, DES, AES, SHA-256, SHA-384, X509, and TLS behavior are delegated to the Microsoft .NET BCL.
- MD4 and RC4 are in-tree and require cryptographic review despite being legacy compatibility primitives.
- This packet prepares the external review; it is not the `rw-e4` security review deliverable.
