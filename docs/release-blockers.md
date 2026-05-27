# Opc.Classic 1.0.0 release blockers

This document tracks the gates that must pass before tagging `1.0.0` (FINAL).
All three gates require infrastructure or process outside the dev sandbox.

## Gate 1: OPC CTT smoke green

**Owner**: CI maintainer with Windows Docker host access.
**Blocker for**: `release-100-tag`.
**Status**: Infrastructure ready (`.github\workflows\docker-test-fleet.yml`,
`docker\docker-compose.test.yml`, and rc.5 native C server/client scaffolds are
in place); awaiting Windows-container execution and triage.

### What's needed

1. Provision a Windows host with Docker Desktop in Windows-container mode.
2. Create the required `opc-test-net` l2bridge network if it does not already
   exist.
3. Build the four test-fleet images:
   `docker compose --file docker\docker-compose.test.yml --profile interactive build`.
4. Run the managed CTT smoke with `docker\run-matrix.ps1 -OnlyManaged`, or run
   the equivalent compose profile directly.
5. Review the CTT report, triage failures, iterate, and mark this gate satisfied
   only when the report is GREEN.

### Estimated effort

Variable — depends on CTT failures encountered. The managed server is expected
to pass the release-scope DA smoke first; deeper subscriptions and async I/O may
surface follow-up defects.

### See also

- `docker\README.md` — fleet overview
- `docs\test-fleet.md` — adopter cookbook
- `samples\Opc.Classic.Samples.CttServer\README.md` — CTT SUT

---

## Gate 2: Real-server NTLMv2 wire test

**Owner**: Network/security maintainer with live AD access.
**Blocker for**: `rw-e1-ntlmv2-realserver`.
**Status**: In-repo vectors cover MS-NLMP key derivation, MIC handling, managed
loopback auth, and TLS channel binding; a live AD/DCOM round-trip is still
required.

### What's needed

1. Provision Windows Server 2022 with Active Directory Domain Services and a
   test domain user.
2. Configure DCOM trust between an Opc.Classic-based client/server and the
   Windows machine.
3. Run an integration exchange against the live machine.
4. Validate NTLMv2 handshake, signing, sealing, and channel binding.
5. Document deviations or compatibility issues.

### Estimated effort

~1-2 days assuming the lab is already provisioned. The remaining gate is live
execution and any resulting triage.

### See also

- `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmV2ServerKeyDerivationTests.cs`
- `tests\Opc.Classic.Dcom.Crypto.Tests\NtlmMicTests.cs`
- `tests\Opc.Classic.Dcom.Tests\ChannelBindingTlsTests.cs`
- `docs\diagrams\04-ntlm-handshake.md`

---

## Gate 3: NTLMSSP third-party audit

**Owner**: Project owner contracting an external security firm.
**Blocker for**: `rw-e4-ntlm-audit`.
**Status**: Security context documents are ready; external NTLMSSP audit
engagement and sign-off are still pending.

### What's needed

1. Identify and engage a qualified crypto/security audit firm.
2. Provide repository access plus the threat model and channel-binding notes.
3. Run the audit window, typically 2-4 weeks.
4. Review findings, remediate, and obtain sign-off.

### Estimated effort

~4-12 weeks calendar time depending on findings and remediation.

### See also

- `docs\security\THREAT_MODEL.md` — STRIDE threat model
- `docs\security\CHANNEL_BINDING.md` — channel binding security

---

## After all three gates

When all three gates pass:

1. Update the consolidated `[1.0.0]` CHANGELOG entry's date if the tag date has
   changed.
2. Remove the "*(awaiting CTT smoke green to tag)*" suffix.
3. Confirm `docs\RELEASE_PROCESS.md` still matches the release workflow.
4. Create the annotated release tag using the repository convention:
   `git tag -a v1.0.0 -m "Opc.Classic 1.0.0"`.
5. Push to the remote only with explicit user consent.
