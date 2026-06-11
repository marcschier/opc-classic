<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP audit surface inventory

This inventory summarizes the NTLMSSP, cryptographic, RPC framing, and channel-binding components that make up the audit surface. Purposes use XML docs/file comments where present, otherwise the type role is summarized.

## `Ntlm`

| Component | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `Type1Message` | 128 | NEGOTIATE message. | `Type1Message(...)`; `MessageType`; `GetDefaultFlags()`; `GetSuppliedDomain()`; `ToByteArray()` |
| `Type2Message` | 193 | CHALLENGE message and target info. | `Type2Message(...)`; `GetChallenge()`; `GetTargetInformation()`; `SetTargetInformation(...)`; `ToByteArray()` |
| `Type3Message` | 321 | AUTHENTICATE message, MIC, identity, responses, session key. | `MicOffset`; `GetNTResponse()`; `GetSessionKey()`; `SetMic(...)`; `ToByteArrayWithMic(...)` |
| `NtlmMessage` | 160 | Shared NTLMSSP signature, flags, bounds/encoding helpers. | `MessageType`; `SetFlags(...)`; `GetFlag(...)`; `ToByteArray()` |
| `NtlmAvPairs` | 107 | Target-info AV_PAIR add/read/write, MIC flag, CBT ID. | `HasMicFlag(...)`; `AddMicFlag(...)`; `AddOrReplace(...)`; `TryGet(...)`; `Write(...)` |
| `NtlmMic` | 44 | HMAC-MD5 MIC compute/verify. | `MicLength`; `Compute(...)`; `Verify(...)` |
| `NtlmMessageSignature` | 71 | 16-byte SIGNATURE_BLOCK helper. | `SignatureLength`; `Sign(...)`; `Verify(...)` |
| `NtlmFlags` | 38 | MS-NLMP negotiate flags. | `enum NtlmFlags` |
| `NtlmMicProvider` | 43 | SPNEGO `IGssMicProvider` adapter. | `NtlmMicProvider` |
| `NtlmPasswordAuthentication` | 28 | Legacy credential holder. | `NtlmPasswordAuthentication` |
| `Arrays` | 10 | Compatibility array helper. | `Arrays.Equals(...)` |
| `Config` | 27 | Compatibility config shim. | `GetProperty(...)`; `SetProperty(...)`; `GetBoolean(...)` |
| `Hashtable` | 42 | Compatibility dictionary wrapper. | `Hashtable` |
| `SharpenCompatibilityExtensions` | 96 | jcifs-port compatibility extensions. | extension helpers |
| Ntlm compatibility helpers compatibility exceptions/address/thread stubs | 301 | `InstantiationException`, `Iterator`, `MissingResourceException`, `NbtAddress`, `NoSuchElementException`, `PrintWriter`, `SmbAuthException`, `SmbException`, `SmbNamedPipe`, `SmbSession`, `StringTokenizer`, `Thread`, `ThreadGroup`, `UniAddress`, `UnknownHostException`, `UnsupportedEncodingException`, `Uuid`. | Constructors/properties only; no crypto logic. |

## `Crypto`

| Component | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `Md4` | 47 | Hand-rolled MD4 one-shot API. | `HashSizeInBytes`; `BlockSizeInBytes`; `HashData(...)` |
| `Md4State` | 200 | Incremental MD4 state. | `Initialize()`; `AppendData(...)`; `GetHashAndReset(...)` |
| `MD4Digest` | 26 | BouncyCastle-shaped MD4 adapter. | `IDigest` methods |
| `Rc4` | 66 | Hand-rolled RC4 stream cipher. | `Rc4(ReadOnlySpan<byte>)`; `Process(...)`; `XorInPlace(...)` |
| `RC4Engine` | 46 | BouncyCastle-shaped RC4 adapter. | `IStreamCipher` methods |
| `DesEcbNoPaddingCipher` | 48 | DES/ECB/no-padding BCL wrapper for legacy NTLM response paths. | `IBufferedCipher` methods |
| `CipherUtilities` | 23 | Cipher factory compatibility shim. | `GetCipher(...)` |
| `DigestUtilities` | 24 | Digest factory compatibility shim. | `GetDigest(...)` |
| `MD5Digest` | 30 | BCL MD5 digest adapter. | `IDigest` methods |
| `IBufferedCipher` | 14 | Cipher interface. | `Init(...)`; `DoFinal(...)` |
| `ICipherParameters` | 9 | Marker interface. | `ICipherParameters` |
| `IDigest` | 13 | Digest interface. | digest methods |
| `IStreamCipher` | 16 | Stream-cipher interface. | `Init(...)`; `ProcessBytes(...)`; `ReturnByte(...)` |
| `KeyParameter` | 17 | Key wrapper. | `KeyParameter(byte[])`; `GetKey()` |

## `Auth`

| Component | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `NtlmAuthentication` | 859 | Main NTLM orchestrator. | `CreateAuthContext(...)`; `CreateType1()`; `CreateType2(...)`; `CreateType3(...)`; `Security`; `EstablishedSessionKey` |
| `AuthenticationSource` | 79 | Server-side credential source contract. | `DefaultInstance`; `SetDefaultInstance(...)`; `CreateChallenge(...)`; `Authenticate(...)` |
| `NtlmConnection` | 141 | Legacy RPC NTLM rebind token state machine. | `IncomingRebind(...)`; `OutgoingRebind()` |
| `NtlmConnectionContext` | 141 | Client bind/alter context state. | `Init(...)`; `Alter(...)`; `Accept(...)`; `Established` |
| `Ntlm1` | 190 | NTLM signing/sealing security context. | `VerifierLength`; `Protection`; `ProcessIncoming(...)`; `ProcessOutgoing(...)` |
| `NTLMKeyFactory` | 355 | Session/sign/seal key derivation and RC4 wrapping. | `GetNTLMv2UserSessionKey(...)`; `SecondarySessionKey`; `Generate*Key(...)`; `SigningPt1(...)`; `SigningPt2(...)` |
| `Responses` | 428 | LM/NTLM/NTLMv2 responses, NTOWF, blobs, HMAC-MD5. | `GetLMResponse(...)`; `GetNTLMResponse(...)`; `GetLMv2Response(...)`; `GetNTLMv2Response(...)`; `GetNTLM2SessionResponse(...)` |
| `SensitiveBufferPool` | 54 | Zeroizes password-derived pooled buffers before return. | `Rent(...)`; `Return(...)` |
| `NullAuthenticationSource` | 35 | Fail-closed default credential source. | `Instance`; overrides throw |
| `WindowsSsoAuthContext` | 205 | Windows SSPI context; adjacent but not NTLMv2 password path. | `IAuthContext` implementation |

## RPC framing and CBT

| Component | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `PduCodec` | 153 | DCE/RPC PDU frame read/encode/decode; carries optional auth verifier bytes in frames. | `ReadPduFrameAsync(...)`; `TryGetFragmentLength(...)`; `DecodePdu(...)`; `EncodePdu(...)` |
| `DcomCallChannel` | 669 | Client channel that attaches/verifies auth verifiers and calls `SignAndSeal`/`VerifyAndUnseal`. | `AttachAuthenticationVerifier(...)`; `ApplyPacketProtectionCore(...)`; `StripAuthenticationVerifier(...)` |
| `ChannelBindings` | 18 | RFC 5056/RFC 2744 channel-bindings struct. | `record ChannelBindings(...)` |
| `ChannelBindingsFactory` | 129 | Builds `tls-server-end-point` CBT application data. | `ForTlsServerEndpoint(...)` |
| `ChannelBindingsHash` | 79 | MD5 hash of serialized GSS channel-bindings struct. | `Compute(...)`; `ForTlsServerCert(...)` |
