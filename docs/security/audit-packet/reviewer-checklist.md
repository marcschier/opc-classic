<!-- Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# Reviewer checklist

## Protocol and parser checks

- [ ] Are all NTLM security-buffer length/offset fields validated before slicing in `NtlmMessage`, `Type1Message`, `Type2Message`, and `Type3Message`? Cross-check FZ-1 coverage in `NtlmFuzzTests`.
- [ ] Does MIC computation cover exactly `NEGOTIATE_MESSAGE || CHALLENGE_MESSAGE || AUTHENTICATE_MESSAGE-with-zeroed-MIC` per [MS-NLMP]? Review `NtlmMic` and `Type3Message.ToByteArrayWithMic`.
- [ ] Does protocol-level verification reject Type3 tampering, MIC mismatch, wrong password, and CBT mismatch? Start with `NtlmHandshakeProtocolTests`.
- [ ] Does the parser reject downgrade to NTLMv1 unless `rpc.ntlm.allowV1=true` is explicitly set? Check `NtlmAuthentication` and `NtlmDefaultsTests.cs`.
- [ ] Are malformed AV pairs handled without over-read, infinite loop, or unexpected exception type? Review `NtlmAvPairs.TryGet`, `AddOrReplace`, and fuzz cases.

## Crypto and key schedule checks

- [ ] Are NTOWFv2, LMOWFv2, NT proof, session base key, and exported session key derived per [MS-NLMP] §3.3.2 and §4.2.4?
- [ ] Are session keys derived correctly for sign/seal/key-exchange/key-strength combinations in `NTLMKeyFactory`?
- [ ] Is RC4 keystream state initialized separately for client/server signing directions and never reused across incompatible directions?
- [ ] Does `Ntlm1.ProcessIncoming` / `ProcessOutgoing` increment sequence counters exactly once per accepted protected PDU?
- [ ] Are SIGNATURE_BLOCK bytes laid out and verified per [MS-NLMP] §3.4.4 and §3.4.5?
- [ ] Are all sensitive password-derived buffers cleared after use? Review `SensitiveBufferPool.Return` and `PasswordZeroizationTests.cs`.
- [ ] Are fixed-time comparisons used wherever attacker-controlled MAC/MIC bytes are compared? `NtlmMic.Verify` uses `FixedTimeEquals`; `NTLMKeyFactory.CompareSignature` currently uses `SequenceEqual`.

## Channel binding checks

- [ ] Is the channel-binding token serialized as RFC 2744 §3.11 little-endian GSS channel-bindings and hashed with MD5 as required for NTLM/CBT?
- [ ] Is `tls-server-end-point:` application data computed per RFC 5929 and incorporated as `MsvAvChannelBindings` per [MS-NLMP] §3.1.5.1.2?
- [ ] Does a configured CBT mismatch fail closed in the protocol-level NTLM verifier before session establishment?
- [ ] Is absence of TLS/CBT explicit and deployment-controlled rather than silently claiming verifier-impersonation resistance?

## DCE/RPC integration checks

- [ ] Does `DcomCallChannel.ApplyPacketProtectionCore` sign the correct PDU byte range per MS-RPCE: full PDU including verifier header, excluding `auth_value`?
- [ ] Are `frag_length`, `auth_length`, padding, auth type, auth level, and verifier start offsets validated before slicing?
- [ ] Is `PduCodec.ReadPduFrameAsync` robust against short frames and invalid fragment length?
- [ ] Does the managed listener's server-side NTLM authenticated-bind path fail closed when an `AuthenticationSource` is configured: wrong passwords rejected, anonymous/plain requests rejected before dispatch, and per-PDU integrity/privacy verified in `F4Auth.cs`?

## Open questions for reviewer comment

- [ ] Should `NTLMKeyFactory.CompareSignature` be changed from `SequenceEqual` to `CryptographicOperations.FixedTimeEquals`?
- [ ] Should NTLM Type1/2/3 token sizes have a stricter public quota than the current 64 KiB default parser bound?
- [ ] Should server challenge and client nonce generation be upgraded or injectable so tests remain deterministic without non-crypto randomness in production paths?
- [ ] Are current `NetworkCredential` string lifetimes acceptable for 1.0.0, or should a secret-provider/span-based credential path be required?
- [ ] Should high-assurance deployments expose a policy knob to disable NTLM entirely and require Kerberos/SPNEGO?
- [ ] Is the Windows-specific shim's use of `Microsoft.Win32.SafeHandles` the correct NativeAOT disposal pattern for adjacent authenticated Windows SSO paths?
