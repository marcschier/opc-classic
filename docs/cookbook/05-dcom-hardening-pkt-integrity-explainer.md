# DCOM hardening: why PKT_INTEGRITY is the default

## What this covers

Why OpcClassic defaults to `PROTECTION_LEVEL_INTEGRITY` (`RPC_C_AUTHN_LEVEL_PKT_INTEGRITY`, level 5) for cross-machine DCOM.

## Status / availability

`OpcProtectionLevel.Integrity` and default expansion are in `src\OpcClassic.Core\OpcProtectionLevel.cs` and `OpcConnectData.cs`. Roadmap references: Phase 3B default integrity, Phase 3C NTLMv2-only policy, Phase 3F channel binding / EPA.

## Why integrity is the default

KB5004442 made DCOM hardening mandatory in March 2023. Patched Windows DCOM servers reject cross-machine activation below packet integrity, so `OpcConnectData` expands `Default` to `Integrity`.

```csharp
var connectData = OpcConnectData.WithNtlmV2(
    OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1"),
    new NetworkCredential("opc-reader", password, "CORP"));

Console.WriteLine(connectData.ProtectionLevel); // Integrity
```

## What PKT_INTEGRITY adds

Every DCE/RPC PDU carries a per-message HMAC signature. The receiver verifies it before accepting the PDU, detecting tampering and downgrade attempts. It does not encrypt values; use `OpcProtectionLevel.Privacy` for confidentiality.

Cost is small: about a 20-byte verifier per PDU plus HMAC work. NTLM commonly uses HMAC-MD5; Kerberos/SPNEGO may negotiate SHA-based keys.

## Opting out for legacy servers

Only downgrade isolated legacy targets that cannot accept integrity.

```csharp
var legacy = OpcConnectData.WithNtlmV2(url, credentials, OpcProtectionLevel.Connect);
```

Planned builder equivalent:

```csharp
var legacy = OpcConnectData.Builder
    .WithProtectionLevel(OpcProtectionLevel.Connect)
    .Build();
```

Bind-only authentication leaves later PDUs unsigned, exposing reads/writes to tampering and downgrade attacks.

## NTLMv2 and EPA

Use `OpcAuthMode.NtlmV2`. `OpcAuthMode.NtlmV1` is legacy and intended to require explicit `AllowNtlmV1` opt-in; do not enable it without a documented exception.

For TLS-protected endpoints, Phase 3F adds EPA by including the `tls-server-end-point` certificate hash in the auth exchange. See [03-kerberos-in-active-directory.md](03-kerberos-in-active-directory.md).
