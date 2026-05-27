# DCOM hardening: why packet integrity is the default

## What this covers

`Opc.Classic` defaults to `OpcProtectionLevel.Integrity`, which corresponds to `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY` for DCE/RPC/DCOM. This is the safe baseline for cross-machine OPC Classic connections.

`OpcProtectionLevel.Integrity` and default expansion are in `Opc.Classic` core connection policy. Authentication support includes self-contained NTLMv2, Kerberos, SPNEGO, and RFC 5056 / RFC 5929 channel binding.

## Why integrity is the default

Microsoft DCOM hardening for KB5004442 requires packet integrity for cross-machine activation against patched Windows servers. `OpcConnectData` expands `OpcProtectionLevel.Default` to `Integrity`.

```csharp
using System.Net;
using Opc.Classic;

var connectData = OpcConnectData.WithNtlmV2(
    OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1"),
    new NetworkCredential("opc-reader", password, "CORP"));

Console.WriteLine(connectData.ProtectionLevel); // Integrity
```

## What packet integrity adds

Every protected DCE/RPC PDU carries a per-message signature. The receiver verifies that signature before accepting the PDU, which detects tampering and downgrade attempts. Packet integrity does not encrypt values; use `OpcProtectionLevel.Privacy` when confidentiality is required.

The overhead is modest: an authentication verifier on each PDU plus the cryptographic work needed by the selected mechanism. NTLMv2 uses HMAC-MD5 session security; Kerberos/SPNEGO uses the negotiated Kerberos encryption type and checksum rules.

## Protection-level choices

| Level | Use |
| --- | --- |
| `Integrity` | Default for cross-machine DCOM. Signs PDUs and satisfies DCOM hardening. |
| `Privacy` | Signs and encrypts PDUs. Use when tag values or event payloads require confidentiality. |
| `Connect` | Compatibility exception for isolated endpoints that cannot accept integrity. |

Only downgrade isolated targets that cannot accept integrity, and document the exception.

```csharp
var compatibilityException = OpcConnectData.WithNtlmV2(
    url,
    credentials,
    OpcProtectionLevel.Connect);
```

Bind-only authentication leaves later PDUs unsigned, exposing reads, writes, callbacks, and activation flows to tampering.

## NTLMv2, Kerberos, and EPA

Use `OpcAuthMode.NtlmV2` or `OpcAuthMode.Kerberos`. Kerberos/SPNEGO is preferred in Active Directory environments. For TLS-protected endpoints, channel binding / Extended Protection for Authentication includes the `tls-server-end-point` certificate hash in the authentication exchange.

See [Kerberos in Active Directory](03-kerberos-in-active-directory.md) for SPN and channel-binding setup. For the NTLMv2 implementation audit trail, see [../security/NTLMSSP_AUDIT_GUIDE.md](../security/NTLMSSP_AUDIT_GUIDE.md).
