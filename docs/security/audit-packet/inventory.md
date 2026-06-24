<!-- Copyright (c) 2026 marcschier. Licensed under the MIT License. -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP audit surface inventory

This inventory summarizes the NTLMSSP, cryptographic, RPC framing, and channel-binding components that make up the audit surface. Purposes use XML docs/file comments where present, otherwise the type role is summarized.

## `Ntlm`

| Component | Purpose | Key API |
| --- | --- | --- |
| `Type1Message` | NEGOTIATE message. | `Type1Message(...)`; `MessageType`; `GetDefaultFlags()`; `GetSuppliedDomain()`; `ToByteArray()` |
| `Type2Message` | CHALLENGE message and target info. | `Type2Message(...)`; `GetChallenge()`; `GetTargetInformation()`; `SetTargetInformation(...)`; `ToByteArray()` |
| `Type3Message` | AUTHENTICATE message, MIC, identity, responses, session key. | `MicOffset`; `GetNTResponse()`; `GetSessionKey()`; `SetMic(...)`; `ToByteArrayWithMic(...)` |
| `NtlmMessage` | Shared NTLMSSP signature, flags, bounds/encoding helpers. | `MessageType`; `SetFlags(...)`; `GetFlag(...)`; `ToByteArray()` |
| `NtlmAvPairs` | Target-info AV_PAIR add/read/write, MIC flag, CBT ID. | `HasMicFlag(...)`; `AddMicFlag(...)`; `AddOrReplace(...)`; `TryGet(...)`; `Write(...)` |
| `NtlmMic` | HMAC-MD5 MIC compute/verify. | `MicLength`; `Compute(...)`; `Verify(...)` |
| `NtlmMessageSignature` | 16-byte SIGNATURE_BLOCK helper. | `SignatureLength`; `Sign(...)`; `Verify(...)` |
| `NtlmFlags` | MS-NLMP negotiate flags. | `enum NtlmFlags` |
| `NtlmMicProvider` | SPNEGO `IGssMicProvider` adapter. | `NtlmMicProvider` |
| `NtlmPasswordAuthentication` | Legacy credential holder. | `NtlmPasswordAuthentication` |
| `Arrays` | Compatibility array helper. | `Arrays.Equals(...)` |
| `Config` | Compatibility config shim. | `GetProperty(...)`; `SetProperty(...)`; `GetBoolean(...)` |
| `Hashtable` | Compatibility dictionary wrapper. | `Hashtable` |
| `SharpenCompatibilityExtensions` | Legacy compatibility extension helpers. | extension helpers |
| Ntlm compatibility helpers compatibility exceptions/address/thread stubs | `InstantiationException`, `Iterator`, `MissingResourceException`, `NbtAddress`, `NoSuchElementException`, `PrintWriter`, `SmbAuthException`, `SmbException`, `SmbNamedPipe`, `SmbSession`, `StringTokenizer`, `Thread`, `ThreadGroup`, `UniAddress`, `UnknownHostException`, `UnsupportedEncodingException`, `Uuid`. | Constructors/properties only; no crypto logic. |

## `Crypto`

| Component | Purpose | Key API |
| --- | --- | --- |
| `Md4` | Hand-rolled MD4 one-shot API. | `HashSizeInBytes`; `BlockSizeInBytes`; `HashData(...)` |
| `Md4State` | Incremental MD4 state. | `Initialize()`; `AppendData(...)`; `GetHashAndReset(...)` |
| `MD4Digest` | BouncyCastle-shaped MD4 adapter. | `IDigest` methods |
| `Rc4` | Hand-rolled RC4 stream cipher. | `Rc4(ReadOnlySpan<byte>)`; `Process(...)`; `XorInPlace(...)` |
| `RC4Engine` | BouncyCastle-shaped RC4 adapter. | `IStreamCipher` methods |
| `DesEcbNoPaddingCipher` | DES/ECB/no-padding BCL wrapper for legacy NTLM response paths. | `IBufferedCipher` methods |
| `CipherUtilities` | Cipher factory compatibility shim. | `GetCipher(...)` |
| `DigestUtilities` | Digest factory compatibility shim. | `GetDigest(...)` |
| `MD5Digest` | BCL MD5 digest adapter. | `IDigest` methods |
| `IBufferedCipher` | Cipher interface. | `Init(...)`; `DoFinal(...)` |
| `ICipherParameters` | Marker interface. | `ICipherParameters` |
| `IDigest` | Digest interface. | digest methods |
| `IStreamCipher` | Stream-cipher interface. | `Init(...)`; `ProcessBytes(...)`; `ReturnByte(...)` |
| `KeyParameter` | Key wrapper. | `KeyParameter(byte[])`; `GetKey()` |

## `Auth`

| Component | Purpose | Key API |
| --- | --- | --- |
| `NtlmAuthentication` | Main NTLM orchestrator. | `CreateAuthContext(...)`; `CreateType1()`; `CreateType2(...)`; `CreateType3(...)`; `Security`; `EstablishedSessionKey` |
| `AuthenticationSource` | Server-side credential source contract. | `DefaultInstance`; `SetDefaultInstance(...)`; `CreateChallenge(...)`; `Authenticate(...)` |
| `NtlmConnection` | Legacy RPC NTLM rebind token state machine. | `IncomingRebind(...)`; `OutgoingRebind()` |
| `NtlmConnectionContext` | Client bind/alter context state. | `Init(...)`; `Alter(...)`; `Accept(...)`; `Established` |
| `Ntlm1` | NTLM signing/sealing security context. | `VerifierLength`; `Protection`; `ProcessIncoming(...)`; `ProcessOutgoing(...)` |
| `NTLMKeyFactory` | Session/sign/seal key derivation and RC4 wrapping. | `GetNTLMv2UserSessionKey(...)`; `SecondarySessionKey`; `Generate*Key(...)`; `SigningPt1(...)`; `SigningPt2(...)` |
| `Responses` | LM/NTLM/NTLMv2 responses, NTOWF, blobs, HMAC-MD5. | `GetLMResponse(...)`; `GetNTLMResponse(...)`; `GetLMv2Response(...)`; `GetNTLMv2Response(...)`; `GetNTLM2SessionResponse(...)` |
| `SensitiveBufferPool` | Zeroizes password-derived pooled buffers before return. | `Rent(...)`; `Return(...)` |
| `NullAuthenticationSource` | Fail-closed default credential source. | `Instance`; overrides throw |
| `WindowsSsoAuthContext` | Windows SSPI context; adjacent but not NTLMv2 password path. | `IAuthContext` implementation |

## RPC framing and CBT

| Component | Purpose | Key API |
| --- | --- | --- |
| `PduCodec` | DCE/RPC PDU frame read/encode/decode; carries optional auth verifier bytes in frames. | `ReadPduFrameAsync(...)`; `TryGetFragmentLength(...)`; `DecodePdu(...)`; `EncodePdu(...)` |
| `DcomCallChannel` | Client channel that attaches/verifies auth verifiers and calls `SignAndSeal`/`VerifyAndUnseal`. | `AttachAuthenticationVerifier(...)`; `ApplyPacketProtectionCore(...)`; `StripAuthenticationVerifier(...)` |
| `ChannelBindings` | RFC 5056/RFC 2744 channel-bindings struct. | `record ChannelBindings(...)` |
| `ChannelBindingsFactory` | Builds `tls-server-end-point` CBT application data. | `ForTlsServerEndpoint(...)` |
| `ChannelBindingsHash` | MD5 hash of serialized GSS channel-bindings struct. | `Compute(...)`; `ForTlsServerCert(...)` |
