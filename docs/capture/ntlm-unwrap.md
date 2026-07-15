# NTLM auth-trailer unwrap (developer-only)

> ⚠️ **STOP.** This document covers a sensitive, developer-only
> feature for inspecting **your own** sign/seal-protected DCOM
> traffic. The NTLM session key is equivalent to the wire-level
> secrets that protect an authenticated DCOM connection. Treat it
> with the same care you would treat the user's password.

## What this is

`NtlmPassiveUnwrapper` is a passive (sniffer-side) decoder for the
NTLMSSP sign-and-seal auth-trailer used by sign/seal-protected
DCOM Request and Response PDUs. Given:

- A 16-byte NTLMv2 session key (established by a captured Type3
  handshake), and
- The encrypted PDU body + the 16-byte auth value from the wire,

…it (a) decrypts the body in place via RC4 + (b) verifies the body's
HMAC-MD5 signature against the auth value. On signature match it
advances the per-direction sequence counter and returns the plaintext;
on mismatch it surfaces a clean `SignatureMismatch` and leaves the
counter untouched.

The unwrapper is a **self-contained primitive** and is also integrated
into `OpcDcomDecoder`'s byte-level frame parsing: the decoder extracts the
auth trailer using `auth_length`, accounts for `auth_pad_length`, calls
`TryUnwrap`, and surfaces the result on
`DecodedOpcPdu.AuthUnwrapStatus`. It remains usable directly from offline
pcap-analysis scripts (see "Direct use" below).

The same decoder path is used by bounded `opcclassic.capture.decode_file` and
`opcclassic.capture.replay_file` processing. Sequence-aware TCP reassembly
orders segments, deduplicates retransmissions, merges overlaps, and advances
NTLM counters only for complete ordered frames.

## When (not) to use it

✅ **Use it for:**

- Debugging your own DCOM client / server lab traffic (you control
  both peers; you can extract the session key from the peer's NTLM
  state).
- Forensic analysis of traffic the customer/operator has explicitly
  authorised you to decrypt (e.g. responding to a vendor-OPC bug
  reproduced via a hand-off capture).
- Validating that a captured DCOM trace matches an expected request
  pattern after the fact.

❌ **Do NOT use it for:**

- Decrypting any traffic you do not own and have not been explicitly
  authorised to inspect. Doing so is likely unauthorised access under
  CFAA / GDPR / equivalent local law.
- Production observability. Production OPC traffic should be
  inspected at the application boundary (e.g. via the OPC client/
  server's own audit logs), NOT by extracting secrets from the auth
  layer.

## Security model

The class follows a "no leakage by default" posture:

| Concern | How the class handles it |
| --- | --- |
| Session key on the heap | Caller owns the input buffer; class copies it once during constructor, derives 4 sub-keys, then zeroes the input copy. Caller is responsible for zeroing their own copy after construction. |
| Derived sub-keys on the heap | Held as `byte[]` for the lifetime of the unwrapper. Zeroed via `CryptographicOperations.ZeroMemory` on `Dispose()`. |
| Logging | The class itself never logs the key. `CaptureStartRequest.ToString()` is overridden to print `NtlmSessionKey = REDACTED[16 bytes]` instead of the raw bytes (the auto-generated record `ToString` would have leaked it via any structured log of the request). |
| Persistence | The class never writes the key to disk. Live sessions and one-shot file tools accept it only on the call boundary; the MCP host MUST redact `ntlmSessionKeyHex` from tool-call audit logs. |
| Sequence counters | Both directions start at 0 after Type3. If the capture missed the handshake, counters drift and EVERY unwrap fails clean with `SignatureMismatch` — there is no graceful "guess the counter" fallback by design. |

## Mid-session capture: NOT supported

`NtlmPassiveUnwrapper` REQUIRES that the capture includes the NTLM
Type1/Challenge/Type3 handshake so that sequence counters start at 0
and stay in lock-step with the live peers. Mid-session capture (where
the wire connection was already authenticated before the pcap began)
is **not recoverable from passive observation alone**: the per-
direction sequence counters cannot be derived from later traffic
patterns.

Symptom of this scenario: every `TryUnwrap` call returns
`Status = SignatureMismatch` with `Reason` mentioning "counter drift"
and "Verify the supplied session key matches the captured Type3
handshake AND that the capture starts BEFORE the bind/handshake."

Workaround: restart the capture, then restart the DCOM client
connection (or trigger any operation that forces a new NTLM bind).

## Where to get the session key

The session key is a 16-byte value derived by both peers during the
NTLM Type1/Challenge/Type3 negotiation. It is NOT directly logged by
the production `Opc.Classic.Dcom.Rpc.Auth.ntlm.NtlmAuthentication`
class (deliberately — leaking it from a runtime log would be bad).
Authorised options to obtain it for your own lab traffic include:

- A custom build of `Opc.Classic.Dcom.Rpc.Auth.ntlm.NtlmAuthentication`
  that exposes `EstablishedSessionKey` for a one-off lab run.
- A custom `IAuthSessionKeyProvider` injected into the runtime that
  records the key out-of-band (developer scaffolding, not for
  production).
- Vendor / Windows-side tooling such as Wireshark's NTLM session-key
  table import (where you've populated the key yourself).

Do not extract session keys from production processes you do not own
or by other means without explicit authorisation.

## Usage (direct)

```csharp
using Opc.Classic.Dcom.Internal.Ntlm;        // NtlmFlags
using Opc.Classic.Dcom.Rpc;                   // ProtectionLevel
using Opc.Classic.Mcp.Capture;                // NtlmPassiveUnwrapper, NtlmDirection

byte[] sessionKey = LoadSessionKey();         // 16 bytes
using var unwrapper = new NtlmPassiveUnwrapper(sessionKey);
CryptographicOperations.ZeroMemory(sessionKey);

// For each captured Request / Response PDU:
//   stubBuffer = the encrypted PDU body bytes (excluding common
//                header, auth verifier header, and auth value)
//   authTrailer = the 16-byte auth value from the end of the frame
byte[] stubBuffer = ExtractEncryptedBody(rawFrame);  // your code
byte[] authTrailer = ExtractAuthValue(rawFrame);    // your code
NtlmDirection dir = IsClientToServer(rawFrame)
    ? NtlmDirection.ClientToServer
    : NtlmDirection.ServerToClient;

NtlmUnwrapResult result = unwrapper.TryUnwrap(dir, stubBuffer, authTrailer);
if (result.Succeeded)
{
    // stubBuffer now holds plaintext bytes; decode as usual.
}
else
{
    // Fail clean; do NOT pretend the plaintext is trustworthy.
    Console.Error.WriteLine($"unwrap failed: {result.Status}: {result.Reason}");
}
```

## Usage (MCP tool)

The `opcclassic.capture.start` MCP tool accepts an optional
`ntlmSessionKeyHex` parameter (a 32-character hex-encoded 16-byte
NTLMv2 session key; whitespace, `0x` prefix, and `:`/`-`/`,`/`;`
separators are stripped). The key is validated for length and hex
character set up-front; an actionable `McpException` is thrown
before the capture even starts when validation fails.

When set, the per-session `OpcDcomDecoder` (used by
`opcclassic.capture.tail`, `opcclassic.capture.get`, and
`opcclassic.capture.summarize`) automatically unwraps the
sign/seal-protected Request / Response / Fault PDUs inline. Each
decoded PDU surfaces the outcome via the new
`DecodedOpcPdu.AuthUnwrapStatus` and `DecodedOpcPdu.AuthUnwrapReason`
fields:

- `"Decrypted"` — body decrypted + signature verified (privacy mode);
  the projected PDU's stub reflects the plaintext bytes.
- `"IntegrityVerified"` — signature verified (integrity-only mode); body left as-is.
- `"SignatureMismatch"` — verifier did not match; reason explains the likely cause
  (wrong key, capture started after Type3 handshake, etc.).
- `"InvalidTrailerLength"` — auth_length field is inconsistent with frame size.
- `null` — no unwrap attempted (no key configured, PDU has no auth
  trailer, or auth scheme is SPNEGO / Kerberos rather than NTLM).

Direction detection is heuristic: the side that sends the first Bind
PDU on a flow is treated as the DCOM client; subsequent
Request / Response PDUs on the bidirectional flow are unwrapped with
the matching per-direction sub-key + counter. If the capture starts
AFTER the Bind, the unwrap returns `SignatureMismatch` with a
"Direction unknown" reason.

## Wire-level reference

The unwrapper implements the NTLMv2 sign-and-seal scheme from
MS-NLMP §3.4 with the standard modern flag set (`NtlmsspNegotiateUnicode |
NtlmsspNegotiateExtendedSessionSecurity | NtlmsspNegotiateSign |
NtlmsspNegotiateAlwaysSign | NtlmsspNegotiateSeal |
NtlmsspNegotiateKeyExch | NtlmsspNegotiate128`).

Key derivation (per direction):
- `ClientSigningKey = MD5(sessionKey || "session key to client-to-server signing key magic constant\0")`
- `ClientSealingKey = MD5(sessionKey || "session key to client-to-server sealing key magic constant\0")`
- `ServerSigningKey = MD5(sessionKey || "session key to server-to-client signing key magic constant\0")`
- `ServerSealingKey = MD5(sessionKey || "session key to server-to-client sealing key magic constant\0")`

Verifier (16 bytes):
```
0x01 0x00 0x00 0x00                           // NTLMSSP version 1, little-endian
HMAC-MD5(signingKey, seqNum_LE || plaintext)[0..7]  // 8 bytes
seqNum_LE                                     // 4 bytes
```

When `NtlmsspNegotiateExtendedSessionSecurity | NtlmsspNegotiateKeyExch`
is negotiated (default), the 8 HMAC bytes are XOR-encrypted via the
direction-specific RC4 stream (`SigningPt2`) before being written to
the verifier.

Per-direction RC4 stream state is preserved across PDUs (the stream
advances by `body.Length + 8` bytes on each `TryUnwrap` call when
the protection level is privacy; by 8 bytes for integrity-only).

## Related

- `NtlmPassiveUnwrapper` source: `NtlmPassiveUnwrapper`
- Unit tests + round-trip vs production `Ntlm1.ProcessOutgoing`:
  `NtlmPassiveUnwrapperTests`
- In-decoder integration (`OpcDcomDecoder.TryUnwrapInPlace` +
  `FlowState.KnownDirection`):
  `OpcDcomDecoder`
- Decoder integration tests (sealed-frame round-trip via
  `BuildSealedFramePerCodebase`):
  `OpcDcomDecoderTests`
- MCP tool parameter: `opcclassic.capture.start --ntlmSessionKeyHex`
  in `CaptureTools`
- Redacting `ToString()` on `CaptureStartRequest`:
  `CaptureStartRequest`

## Wire-format compatibility caveat

The in-decoder unwrap path matches the **production receiver's wire
expectation** (`DcomCallChannel.VerifyPacketProtection`): plaintext
common header on the wire, RC4-sealed body between common header
and auth verifier header, plaintext auth verifier header, 16-byte
NTLM auth value as the trailer. The HMAC covers the body only
(starting at offset 16).

This matches the typical NTLMSSP wire format used by Windows-side
DCOM peers in the field. If you encounter `SignatureMismatch` on
traffic from a peer that uses a different signing region (e.g.
covering the common header per a strict reading of MS-RPCE §13.3 or
the sender side of this codebase's
`DcomCallChannel.ApplyPacketProtectionCore`), the unwrapper today
will not recover the plaintext — investigate the peer's exact
signing-region convention and file an issue if you need a
configurable variant.
