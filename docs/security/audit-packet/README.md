<!-- SPDX-License-Identifier: MIT -->
<!-- Last-updated: 2026-06-09T08:19:11+02:00; Commit: 3db24c2050a80a3d28c58e801516088d30aa8592 -->

# NTLMSSP audit packet

This packet is the self-contained NTLMSSP security-audit preparation set for the `rw-e4` track. It is written for an external crypto/security reviewer who is already familiar with [MS-NLMP], NTLMv2, HMAC-MD5, RC4, MD4, DCE/RPC authentication verifiers, and channel binding concepts.

The scope is the managed NTLMv2 stack used over DCE/RPC bind authentication for OPC Classic DCOM: Type1/Type2/Type3 messages, AV pairs, MIC, CBT, NTOWFv2/LMOWFv2/HMAC-MD5 response computation, session keys, signing/sealing, MD4/RC4 primitives, password-buffer zeroization, and RPC verifier wrapping.

## Quick start

1. [scope.md](scope.md) — exact in-scope, out-of-scope, and non-goal boundaries.
2. [threat-model.md](threat-model.md) — NTLM-specific threat model extract plus attacker model.
3. [inventory.md](inventory.md) — file inventory, line counts, purposes, and API surface.
4. [design.md](design.md) — NTLM Type1→Type2→Type3 architecture and RPC wire integration.
5. [known-answer-vectors.md](known-answer-vectors.md) — spec/test-vector map and fixture references.
6. [test-coverage.md](test-coverage.md) — unit, property, fuzz, fixture, integration, and CBT coverage.
7. [limitations.md](limitations.md) — documented limits and deliberate design choices.
8. [reviewer-checklist.md](reviewer-checklist.md) — concrete review checklist and open questions.

## Related parent documents

- `docs\security\THREAT_MODEL.md`
- `docs\security\CHANNEL_BINDING.md`
- `docs\security\NTLMSSP_AUDIT_GUIDE.md`
