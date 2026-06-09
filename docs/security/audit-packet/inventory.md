<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP file inventory

Line counts were measured from this checkout. Purposes use XML docs/file comments where present, otherwise the type role is summarized. Channel-binding files live under `src\Opc.Classic.Core\Security\`, not `src\Opc.Classic.Security\`.

## `src\Opc.Classic.Dcom\Common\Ntlm\`

| Path | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom\Common\Ntlm\Type1Message.cs` | 128 | NEGOTIATE message. | `Type1Message(...)`; `MessageType`; `GetDefaultFlags()`; `GetSuppliedDomain()`; `ToByteArray()` |
| `src\Opc.Classic.Dcom\Common\Ntlm\Type2Message.cs` | 193 | CHALLENGE message and target info. | `Type2Message(...)`; `GetChallenge()`; `GetTargetInformation()`; `SetTargetInformation(...)`; `ToByteArray()` |
| `src\Opc.Classic.Dcom\Common\Ntlm\Type3Message.cs` | 321 | AUTHENTICATE message, MIC, identity, responses, session key. | `MicOffset`; `GetNTResponse()`; `GetSessionKey()`; `SetMic(...)`; `ToByteArrayWithMic(...)` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMessage.cs` | 160 | Shared NTLMSSP signature, flags, bounds/encoding helpers. | `MessageType`; `SetFlags(...)`; `GetFlag(...)`; `ToByteArray()` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmAvPairs.cs` | 107 | Target-info AV_PAIR add/read/write, MIC flag, CBT ID. | `HasMicFlag(...)`; `AddMicFlag(...)`; `AddOrReplace(...)`; `TryGet(...)`; `Write(...)` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMic.cs` | 44 | HMAC-MD5 MIC compute/verify. | `MicLength`; `Compute(...)`; `Verify(...)` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMessageSignature.cs` | 71 | 16-byte SIGNATURE_BLOCK helper. | `SignatureLength`; `Sign(...)`; `Verify(...)` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmFlags.cs` | 38 | MS-NLMP negotiate flags. | `enum NtlmFlags` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmMicProvider.cs` | 43 | SPNEGO `IGssMicProvider` adapter. | `NtlmMicProvider` |
| `src\Opc.Classic.Dcom\Common\Ntlm\NtlmPasswordAuthentication.cs` | 28 | Legacy credential holder. | `NtlmPasswordAuthentication` |
| `src\Opc.Classic.Dcom\Common\Ntlm\Arrays.cs` | 10 | Compatibility array helper. | `Arrays.Equals(...)` |
| `src\Opc.Classic.Dcom\Common\Ntlm\Config.cs` | 27 | Compatibility config shim. | `GetProperty(...)`; `SetProperty(...)`; `GetBoolean(...)` |
| `src\Opc.Classic.Dcom\Common\Ntlm\Hashtable.cs` | 42 | Compatibility dictionary wrapper. | `Hashtable` |
| `src\Opc.Classic.Dcom\Common\Ntlm\SharpenCompatibilityExtensions.cs` | 96 | SharpCifs compatibility extensions. | extension helpers |
| `src\Opc.Classic.Dcom\Common\Ntlm\*.cs` compatibility exceptions/address/thread stubs | 301 | `InstantiationException`, `Iterator`, `MissingResourceException`, `NbtAddress`, `NoSuchElementException`, `PrintWriter`, `SmbAuthException`, `SmbException`, `SmbNamedPipe`, `SmbSession`, `StringTokenizer`, `Thread`, `ThreadGroup`, `UniAddress`, `UnknownHostException`, `UnsupportedEncodingException`, `Uuid`. | Constructors/properties only; no crypto logic. |

## `src\Opc.Classic.Dcom\Crypto\`

| Path | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom\Crypto\Md4.cs` | 47 | Hand-rolled MD4 one-shot API. | `HashSizeInBytes`; `BlockSizeInBytes`; `HashData(...)` |
| `src\Opc.Classic.Dcom\Crypto\Md4State.cs` | 200 | Incremental MD4 state. | `Initialize()`; `AppendData(...)`; `GetHashAndReset(...)` |
| `src\Opc.Classic.Dcom\Crypto\MD4Digest.cs` | 26 | BouncyCastle-shaped MD4 adapter. | `IDigest` methods |
| `src\Opc.Classic.Dcom\Crypto\Rc4.cs` | 66 | Hand-rolled RC4 stream cipher. | `Rc4(ReadOnlySpan<byte>)`; `Process(...)`; `XorInPlace(...)` |
| `src\Opc.Classic.Dcom\Crypto\RC4Engine.cs` | 46 | BouncyCastle-shaped RC4 adapter. | `IStreamCipher` methods |
| `src\Opc.Classic.Dcom\Crypto\DesEcbNoPaddingCipher.cs` | 48 | DES/ECB/no-padding BCL wrapper for legacy NTLM response paths. | `IBufferedCipher` methods |
| `src\Opc.Classic.Dcom\Crypto\CipherUtilities.cs` | 23 | Cipher factory compatibility shim. | `GetCipher(...)` |
| `src\Opc.Classic.Dcom\Crypto\DigestUtilities.cs` | 24 | Digest factory compatibility shim. | `GetDigest(...)` |
| `src\Opc.Classic.Dcom\Crypto\MD5Digest.cs` | 30 | BCL MD5 digest adapter. | `IDigest` methods |
| `src\Opc.Classic.Dcom\Crypto\IBufferedCipher.cs` | 14 | Cipher interface. | `Init(...)`; `DoFinal(...)` |
| `src\Opc.Classic.Dcom\Crypto\ICipherParameters.cs` | 9 | Marker interface. | `ICipherParameters` |
| `src\Opc.Classic.Dcom\Crypto\IDigest.cs` | 13 | Digest interface. | digest methods |
| `src\Opc.Classic.Dcom\Crypto\IStreamCipher.cs` | 16 | Stream-cipher interface. | `Init(...)`; `ProcessBytes(...)`; `ReturnByte(...)` |
| `src\Opc.Classic.Dcom\Crypto\KeyParameter.cs` | 17 | Key wrapper. | `KeyParameter(byte[])`; `GetKey()` |

## `src\Opc.Classic.Dcom\rpc\Auth\`

| Path | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom\rpc\Auth\NtlmAuthentication.cs` | 859 | Main NTLM orchestrator. | `CreateAuthContext(...)`; `CreateType1()`; `CreateType2(...)`; `CreateType3(...)`; `Security`; `EstablishedSessionKey` |
| `src\Opc.Classic.Dcom\rpc\Auth\AuthenticationSource.cs` | 79 | Server-side credential source contract. | `DefaultInstance`; `SetDefaultInstance(...)`; `CreateChallenge(...)`; `Authenticate(...)` |
| `src\Opc.Classic.Dcom\rpc\Auth\NtlmConnection.cs` | 141 | Legacy RPC NTLM rebind token state machine. | `IncomingRebind(...)`; `OutgoingRebind()` |
| `src\Opc.Classic.Dcom\rpc\Auth\NtlmConnectionContext.cs` | 141 | Client bind/alter context state. | `Init(...)`; `Alter(...)`; `Accept(...)`; `Established` |
| `src\Opc.Classic.Dcom\rpc\Auth\Ntlm1.cs` | 190 | NTLM signing/sealing security context. | `VerifierLength`; `Protection`; `ProcessIncoming(...)`; `ProcessOutgoing(...)` |
| `src\Opc.Classic.Dcom\rpc\Auth\NTLMKeyFactory.cs` | 355 | Session/sign/seal key derivation and RC4 wrapping. | `GetNTLMv2UserSessionKey(...)`; `SecondarySessionKey`; `Generate*Key(...)`; `SigningPt1(...)`; `SigningPt2(...)` |
| `src\Opc.Classic.Dcom\rpc\Auth\Responses.cs` | 428 | LM/NTLM/NTLMv2 responses, NTOWF, blobs, HMAC-MD5. | `GetLMResponse(...)`; `GetNTLMResponse(...)`; `GetLMv2Response(...)`; `GetNTLMv2Response(...)`; `GetNTLM2SessionResponse(...)` |
| `src\Opc.Classic.Dcom\rpc\Auth\SensitiveBufferPool.cs` | 54 | Zeroizes password-derived pooled buffers before return. | `Rent(...)`; `Return(...)` |
| `src\Opc.Classic.Dcom\rpc\Auth\NullAuthenticationSource.cs` | 35 | Fail-closed default credential source. | `Instance`; overrides throw |
| `src\Opc.Classic.Dcom\rpc\Auth\WindowsSsoAuthContext.cs` | 205 | Windows SSPI context; adjacent but not NTLMv2 password path. | `IAuthContext` implementation |

## RPC framing and CBT

| Path | Lines | Purpose | Key API |
| --- | ---: | --- | --- |
| `src\Opc.Classic.Dcom\Transport\PduCodec.cs` | 153 | DCE/RPC PDU frame read/encode/decode; carries optional auth verifier bytes in frames. | `ReadPduFrameAsync(...)`; `TryGetFragmentLength(...)`; `DecodePdu(...)`; `EncodePdu(...)` |
| `src\Opc.Classic.Dcom\Transport\DcomCallChannel.cs` | 669 | Client channel that attaches/verifies auth verifiers and calls `SignAndSeal`/`VerifyAndUnseal`. | `AttachAuthenticationVerifier(...)`; `ApplyPacketProtectionCore(...)`; `StripAuthenticationVerifier(...)` |
| `src\Opc.Classic.Core\Security\ChannelBindings.cs` | 18 | RFC 5056/RFC 2744 channel-bindings struct. | `record ChannelBindings(...)` |
| `src\Opc.Classic.Core\Security\ChannelBindingsFactory.cs` | 129 | Builds `tls-server-end-point` CBT application data. | `ForTlsServerEndpoint(...)` |
| `src\Opc.Classic.Core\Security\ChannelBindingsHash.cs` | 79 | MD5 hash of serialized GSS channel-bindings struct. | `Compute(...)`; `ForTlsServerCert(...)` |
