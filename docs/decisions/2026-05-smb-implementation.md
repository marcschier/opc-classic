# ADR-2026-05 — SMB transport implementation strategy

**Status**: Accepted — Phase 1 (`src\Opc.Classic.Dcom.Smb\`) has landed; `ncacn_np` wire-up remains pending.
**Date**: 2026-05-26
**Decider**: project maintainer

## Context

The `ncacn_np` (RPC over SMB) transport is required to:

1. Implement WINREG-based server discovery (`[MS-RRP]`).
2. Talk to legacy DCOM servers that use `IActivation::RemoteActivation` over
   SMB rather than `IRemoteSCMActivator` over TCP.
3. Support DCOM-over-firewall scenarios where TCP port 135 is blocked but
   SMB (445) is open.

The legacy `src\Opc.Classic.Dcom` `ncacn_np` path still uses the stub
`Opc.Classic.Dcom\Common\Ntlm\SmbNamedPipe.cs` wrapper and is not wired to a
real SMB transport. A focused SMB2 client now exists in
`src\Opc.Classic.Dcom.Smb\`, but consumers such as `RemoteRegistryEnum` still
fall back to the TCP-based `OpcEnumClient` alternative until the wire-up lands.

## Decision

**Hand-roll a focused SMB2-only client** (option B in
`docs\architecture\smb-transport.md`) in `src\Opc.Classic.Dcom.Smb\` under
the repo's standard MIT + AOT-clean conventions.

## Rationale

| Criterion | Hand-rolled SMB2 (chosen) | SMBLibrary NuGet (rejected) |
|---|---|---|
| License | MIT (in-repo) | LGPL-3.0 — requires per-jurisdiction legal review; complicates downstream redistribution from MIT consumers |
| AOT compatibility | Designed AOT-clean from day one | Likely OK but unverified for our trimming profile |
| Surface | Tightly scoped to named-pipe primitives | Full SMB1/2/3 client + server + DFS + RDMA — large unused surface |
| Auditability | All wire-format code grounded in vendored `[MS-SMB2]` spec; easy to review per-PDU | External dependency; auditing requires shadowing the upstream commit log |
| Reuse of existing crypto | NTLMSSP Type 1/2/3 already implemented in `src\Opc.Classic.Dcom\rpc\Auth\` — fits naturally into SMB2_SESSION_SETUP security blob | Likely duplicates managed NTLM logic |

Hand-rolling is the larger upfront cost (~4-5 days) but produces a focused
asset (~4000-5000 LOC) with no third-party license entanglement and minimal
unused surface.

SMB1 is explicitly NOT supported: Windows 10 / Server 2016+ disable SMB1 by
default for security reasons; the modern Windows fleet exclusively speaks
SMB2/3. Adding SMB1 doubles surface for marginal benefit (XP / 2003
compatibility only) and is out of scope unless an adopter demonstrates a
real-world need.

## Consequences

### Positive

- Single MIT-licensed assembly, no LGPL constraints in distribution.
- Tight scope: only the named-pipe primitives needed by the OPC Classic call
  graph (no DFS, no full file-share semantics, no RDMA, no SMB1).
- AOT-clean from day one.
- Tests and wire fixtures live alongside the implementation in the same repo.

### Negative

- ~4-5 days of focused work to deliver Phase 1 (the SMB2 client itself).
- Future Windows server feature changes (e.g. mandatory encryption in Server
  2025+) will require us to follow them; with SMBLibrary we'd inherit those
  upstream.
- Crypto-heavy code (HMAC-SHA256 signing, AES-128-CCM/GCM encryption) is
  subject to the same audit / review rigor as the rest of the OPC Classic
  crypto stack (`Opc.Classic.Dcom.Kerberos`, NTLMSSP).

### Neutral

- The decision can be revisited if a future SMB-protocol library appears
  under a permissive license (MIT/Apache 2.0) with AOT support.

## Alternatives considered

| # | Alternative | Why rejected |
|---|---|---|
| A | Consume `SMBLibrary` NuGet | LGPL-3.0 conflicts with MIT-only redistribution model |
| C | Hand-roll SMB1-only | Doesn't work against default Windows 10+/Server 2016+ |
| D | Hand-roll hybrid SMB1+SMB2 | Doubles surface area for compat with already-deprecated servers |
| E | Windows-only `NamedPipeClientStream` | Defeats the cross-platform goal |
| F | P/Invoke into `cifs-utils` / native SMB client | AOT-hostile, requires native deps in the publish profile, ugly portability story |

## Open questions (deferred to implementation phases)

- **Signing and encryption**: SMB signing is implemented for HMAC-SHA256
  (SMB 2.0.2/2.1) and AES-128-CMAC (SMB 3.x) once the NTLMSSP/Kerberos
  SessionKey is available. SMB 3.x encryption (AES-128-CCM/GCM) remains
  deferred to cap-h2 before WINREG E2E tests against encryption-required servers.
- **Kerberos**: NTLMSSP is sufficient for the smoke phases (WINREG +
  IActivation). Kerberos over SMB2 (mandatory when joined to AD with NTLMv2
  restrictions) can be a follow-on after IActivation client lands; it reuses
  `src\Opc.Classic.Dcom.Kerberos\`.
- **Server-side ncacn_np**: ACCEPTING SMB-tunneled DCOM activations into our
  own managed server is out of initial scope. The IActivation server can be
  added later if adopters demand legacy-client interop.

## References

- `docs\architecture\smb-transport.md` — phased implementation plan
- `docs\architecture\activation-transports.md` — TCP vs SMB activation paths
- `src\Opc.Classic.Dcom.Smb\README.md` — current SMB2 client status
- `External\Docs\Win\[MS-SMB2].md` — protocol spec
