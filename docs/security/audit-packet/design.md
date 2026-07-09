<!-- Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP design overview

## State machine

1. Client auth context is created by `NtlmAuthentication.CreateAuthContext` from `OpcConnectData`.
2. Client creates Type1 with `NtlmAuthentication.CreateType1` (`NtlmAuthentication`) or `NtlmAuthContext.BuildInitialToken` (`NtlmAuthentication`).
3. Protocol-level verifier code can create Type2 with `AuthenticationSource.CreateChallenge` and `NtlmAuthentication.CreateType2` (`NtlmAuthentication`); the managed listener does not invoke this for inbound binds today.
4. Client processes Type2 and creates Type3 with `NtlmAuthContext.ProcessChallengeToken` (`NtlmAuthentication`) and `NtlmAuthentication.CreateType3` (`NtlmAuthentication`).
5. Type3 includes NTLMv2 response material from `Responses.GetLMv2Response` / `GetNTLMv2Response` and session-key setup through `NTLMKeyFactory`.
6. Protocol-level verification of Type3 goes through `AuthenticationSource.Authenticate` (`AuthenticationSource`) and `NtlmAuthentication.CreateSecurityWhenServerWithMic` (`NtlmAuthentication`); this path is covered by tests but not wired into listener bind handling.
7. Established calls use `NtlmConnection.OutgoingRebind` or modern `IAuthContext.SignAndSeal` / `VerifyAndUnseal` (`NtlmAuthentication`).
8. `NtlmConnectionContext.Accept` accepts client-side bind acks, but server-side `BindPdu`/`AlterContextPdu` paths throw because listener-level authenticated binds are not implemented.

## Key types

- Message DTOs: `Type1Message`, `Type2Message`, `Type3Message` in `Ntlm`.
- AV pairs: `NtlmAvPairs` (`MsvAvFlags`, `MsvAvChannelBindings`, MIC flag).
- MIC: `NtlmMic` and `Type3Message.ToByteArrayWithMic`.
- Channel binding: `ChannelBindings`, `ChannelBindingsFactory`, `ChannelBindingsHash` in `Security`.
- Session security: `NTLMKeyFactory` and `Ntlm1` in `Auth`.

## NTLMv2 with CBT sequence

```mermaid
sequenceDiagram
    participant C as Client NtlmAuthentication
    participant R as DCE/RPC bind/auth PDUs
    participant S as Protocol verifier / AuthenticationSource
    participant CBT as ChannelBindingsHash
    C->>CBT: Compute RFC 2744/RFC 5056 CBT hash
    C->>R: Type1 NEGOTIATE flags/domain/workstation
    R->>S: CreateChallenge(Type1) (protocol path; not listener-wired)
    S->>R: Type2 CHALLENGE flags/challenge/target-info + MIC flag
    R->>C: Type2 bytes
    C->>C: NTOWFv2/LMOWFv2, blob, MsvAvChannelBindings, session key
    C->>R: Type3 AUTHENTICATE LMv2/NTLMv2/session key/MIC
    R->>S: Authenticate(Type2, Type3) (protocol path; not listener-wired)
    S->>S: verify NT proof, MIC, CBT, derive session key
    C-->>S: subsequent DCE/RPC PDUs signed/sealed by Ntlm1
```

## Wire-protocol integration

`PduCodec` reads and writes complete DCE/RPC frames and preserves frame `auth_length` metadata. The client channel appends the actual DCE/RPC auth verifier in `DcomCallChannel.AttachAuthenticationVerifier`.

For packet integrity/privacy, `DcomCallChannel.ApplyPacketProtectionCore` builds the verifier header, updates `frag_length` and `auth_length`, signs the full PDU except `auth_value`, and copies the 16-byte NTLM verifier returned by `IAuthContext.SignAndSeal` (`DcomCallChannel`). Incoming frames are stripped and verified in `DcomCallChannel.StripAuthenticationVerifier` and `VerifyPacketProtection` (`DcomCallChannel`).
