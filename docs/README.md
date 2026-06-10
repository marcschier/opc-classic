# Opc.Classic Documentation

Current rc.10 baseline: **10 sample apps** and **2113 passed / 12 skipped / 0 failed across 23 .NET test projects** with 0 build warnings / 0 build errors.

## Getting Started

- [Architecture overview](ARCHITECTURE.md) — what Opc.Classic is and how it fits together
- [Adoption guide](ADOPTION.md) — step-by-step for adopters
- [Tutorial: build your first DA client](tutorials/01-build-your-first-da-client.md)

## For Adopters

- [Cookbook recipes](cookbook/README.md) — short focused how-tos
- [Implementing OPC Security](cookbook/08-implementing-opc-security.md) — OPC-layer security sample and production guidance
- [Tutorials](tutorials/README.md) — long-form walkthroughs
- [Samples](../samples/README.md) — 10 runnable sample apps
- [Migrating from OPC Foundation .NET API](tutorials/07-migrating-from-opc-foundation-net-api.md)
- [Migration analyzer diagnostics](migration/README.md) — OCM* code-fix providers

## For Operators

- [Cross-platform deployment](tutorials/03-cross-platform-deployment.md)
- [Performance tuning](tutorials/08-performance-tuning.md)
- [Troubleshooting](tutorials/09-troubleshooting-and-diagnostics.md)
- [AOT + trimming](tutorials/10-aot-and-trimming.md)
- [Docker test fleet cookbook](test-fleet.md) — Windows-container native/managed interop workflows

## Tools / Integrations

- [Opc.Classic.Mcp](mcp/README.md) — stdio MCP server for Claude Desktop, Cursor, VS Code Copilot Chat, GitHub Copilot CLI, and other AI agents

## Interop with native OPC servers

- [Probe coverage](interop/probe-coverage.md) — MCP tool-by-tool status against Matrikon Simulation Server + OPC Foundation TestServer
- [OPCEnum DCOM authentication](interop/opcenum-auth.md) — auth-level requirements + `interop/tools/grant-opcenum-acl.ps1` AppID ACL helper
- [`IOPCDataCallback` push delivery](interop/da-callbacks.md) — server→client subscription callback architecture (Track AP)
- [TestServer activation residual](interop/testserver.md) — known SCM-activation blocker for the OPC Foundation managed TestServer
- [Wire captures](interop/wire-captures/README.md) — opt-in NDR diagnostic dumps via `OPCCLASSIC_WIRE_CAPTURE_DIR`
- [Unblocking `da.get_properties` decode](interop/unblocking-get-properties-decode.md) — step-by-step procedure to capture the live Matrikon exchange needed to close `ag-get-properties-decode`

## Security

- [Threat model](security/THREAT_MODEL.md)
- [Channel binding (CBT) on TLS](security/CHANNEL_BINDING.md)
- [NTLMSSP audit prep guide](security/NTLMSSP_AUDIT_GUIDE.md)
- [Kerberos in Active Directory](cookbook/03-kerberos-in-active-directory.md)
- [DCOM hardening explainer](cookbook/05-dcom-hardening-pkt-integrity-explainer.md)
- [Implementing OPC Security](cookbook/08-implementing-opc-security.md)

## Specification coverage

- [Architecture diagrams](architecture/diagrams.md) — 10 Mermaid diagrams
- [Generator diagnostics](generators/diagnostics.md) — OPCGEN*+ OCM* reference
- [XML-DA status](CONFORMANCE.md#opc-xml-da-101)
- [Roadmap](ROADMAP.md) — delivered rc.* work, release gates, and beyond

## For Contributors

- [Contributing guide](../CONTRIBUTING.md)
- [Release process](RELEASE_PROCESS.md)
- [Changelog](../CHANGELOG.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
