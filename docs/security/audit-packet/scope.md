<!-- Copyright (c) 2026 marcschier. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP audit scope

## In scope

- NTLMSSP message exchange: Type1 NEGOTIATE, Type2 CHALLENGE, Type3 AUTHENTICATE in `Type1Message`, `Type2Message.cs`, `Type3Message.cs`.
- AV-pair processing, MIC flag handling, MIC computation/verification, and channel-binding AV pairs in `NtlmAvPairs`, `NtlmMic.cs`, and ChannelBindings*.
- NTLMv2 response computation: HMAC-MD5, NTOWFv2, LMOWFv2, timestamp/nonce blob construction, and server proof verification in `Responses` and `NtlmAuthentication.cs`.
- Session-key derivation and key-exchange wrapping in `NTLMKeyFactory`.
- DCE/RPC `PKT_INTEGRITY` / `PKT_PRIVACY` signing and sealing in `Ntlm1`, `NtlmAuthentication`, and `DcomCallChannel`.
- MD4 and RC4 hand-rolled primitives and compatibility wrappers in Md4*, `Rc4.cs`, `MD4Digest.cs`, and `RC4Engine.cs`.
- Password-derived buffer zeroization in `SensitiveBufferPool`, covered by `PasswordZeroizationTests`.
- DCE/RPC bind-time auth verifier wrapping on the client/protocol path: `NtlmConnection`, `NtlmConnectionContext.cs`, `PduCodec`, and `DcomCallChannel`.

## Out of scope

- Kerberos and SPNEGO protocol review; those are separate audit tracks if needed.
- TLS and certificate-chain validation; the CBT input depends on .NET BCL certificate/TLS APIs.
- OPC DA/AE/HDA/Batch/Commands/Security/DX/XML-DA behavior above the authentication layer.
- The managed listener's anonymous-bind path, incomplete server-side NTLM bind handshake, and application-level authorization policy.

## Non-goals

- Cryptographic primitive analysis is limited to RFC/spec adherence unless the primitive is hand-rolled here.
- HMAC-MD5, MD5, DES, AES, SHA-256, SHA-384, X509, and TLS behavior are delegated to the Microsoft .NET BCL.
- MD4 and RC4 are in-tree and require cryptographic review despite being legacy compatibility primitives.
- This packet prepares the external review; it is not the `rw-e4` security review deliverable.
