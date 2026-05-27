# Opc.Classic Documentation

## Getting Started
- [Architecture overview](ARCHITECTURE.md) — what Opc.Classic is and how it fits together
- [Adoption guide](ADOPTION.md) — step-by-step for adopters
- [Migration guide](MIGRATION.md) — preview/rc to 1.0.0 adoption notes
- [Tutorial: build your first DA client](tutorials/01-build-your-first-da-client.md)

## For Adopters
- [Cookbook recipes](cookbook/README.md) — short focused how-tos
- [Tutorials](tutorials/README.md) — long-form walkthroughs
- [Migrating from OPC Foundation .NET API](tutorials/07-migrating-from-opc-foundation-net-api.md)
- [Migration analyzer diagnostics](migration/README.md) — OCM* code-fix providers

## For Operators
- [Cross-platform deployment](tutorials/03-cross-platform-deployment.md)
- [Performance tuning](tutorials/08-performance-tuning.md)
- [Troubleshooting](tutorials/09-troubleshooting-and-diagnostics.md)
- [AOT + trimming](tutorials/10-aot-and-trimming.md)
- [Docker test fleet cookbook](test-fleet.md) — Windows-container CTT and native/managed interop workflows

## Tools / Integrations
- [Opc.Classic.Mcp](mcp/README.md) — stdio MCP server for Claude Desktop, Cursor, VS Code Copilot Chat, GitHub Copilot CLI, and other AI agents

## Security
- [Threat model](security/THREAT_MODEL.md)
- [Channel binding (CBT) on TLS](security/CHANNEL_BINDING.md)
- [NTLMSSP audit prep guide](security/NTLMSSP_AUDIT_GUIDE.md)
- [Kerberos in Active Directory](cookbook/03-kerberos-in-active-directory.md)
- [DCOM hardening explainer](cookbook/05-dcom-hardening-pkt-integrity-explainer.md)

## Specification coverage
- [Architecture diagrams](diagrams/README.md) — 10 Mermaid diagrams
- [Generator diagnostics](generators/diagnostics.md) — OPCGEN* + OCM* reference
- [XML-DA status](XMLDA_STATUS.md)
- [OPC CTT conformance](OPC_CTT_CONFORMANCE.md)
- [Roadmap](ROADMAP.md) — delivered rc.* work, release gates, and beyond
- [Release blockers](release-blockers.md) — the three remaining gates before 1.0.0 FINAL

## For Contributors
- [Contributing guide](../CONTRIBUTING.md)
- [Release process](RELEASE_PROCESS.md)
- [Changelog](../CHANGELOG.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
