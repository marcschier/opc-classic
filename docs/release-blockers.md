# Release blockers

These are the remaining gates before tagging `1.0.0` FINAL. Implemented release-candidate work is tracked in the [changelog](../CHANGELOG.md); forward-looking work is tracked in the [roadmap](ROADMAP.md).

## Current state

- Current release candidate: `1.0.0-rc.10`.
- Validation baseline: 0 build warnings / 0 build errors; 2113 passed / 12 skipped / 0 failed across 23 .NET test projects.
- The remaining gates are environment- or audit-blocked, not known missing release-scope CCW implementation stubs.

## Gates

| Gate | Status | Notes |
| --- | --- | --- |
| OPC CTT smoke on Windows Docker host | Open | Run the Docker CTT fleet and archive the green report with release artifacts. |
| NTLMv2 wire verification against live Windows Server / AD lab | Open | Requires a real Windows/AD environment outside the repository sandbox. |
| External third-party NTLMSSP crypto/security audit | Open | Schedule and record the audit outcome or explicit maintainer waiver before FINAL. |
