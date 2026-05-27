# Opc.Classic

[![Build](https://github.com/marcschier/opc-classic/actions/workflows/build.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/build.yml)
[![Docker test fleet](https://github.com/marcschier/opc-classic/actions/workflows/docker-test-fleet.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/docker-test-fleet.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-1.0.0--rc.7-blue)](CHANGELOG.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A cross-platform, NativeAOT-compatible **.NET 10** implementation of OPC Classic for clients and servers. Managed DCOM/MSRPC with no Windows COM runtime interop, XML-DA over HTTP, source-generated proxies and dispatchers, and self-contained NTLMv2 / Kerberos / SPNEGO authentication.

> **Status — release candidate.** Currently tagged `1.0.0-rc.7` locally (rc.1 through rc.7 tags exist locally; none have been pushed to origin yet). The 1.0.0 FINAL tag is gated on three remaining items tracked in [`docs/release-blockers.md`](docs/release-blockers.md): OPC CTT smoke green on Windows Docker, NTLMv2 wire test against a live Windows Server, and an external third-party NTLMSSP audit. Everything except those three gates is shipping today.

## What you get

- **Cross-platform OPC Classic** — DA (1.0 / 2.05a / 3.0), AE 1.10, HDA 1.20, Batch 2.00, Commands, Complex Data, DX, Security, and XML-DA over HTTP. Runs on Windows, Linux, and macOS (.NET 10).
- **Managed DCOM/MSRPC stack** — full MS-DCOM PDU framing, NDR marshaling, OBJREF/ORPC, packet integrity + privacy, channel binding (RFC 5056/5929). No `[ComImport]`, no Windows COM runtime dependency.
- **Self-contained authentication** — NTLMv2, Kerberos (RC4-HMAC + AES128/256), SPNEGO with `mechListMIC`, channel binding tokens. Cryptography (MD4, RC4) ships in-tree with RFC test vectors; MD5 / HMAC / DES / AES come from the BCL.
- **Source-generated proxies and dispatchers** — Roslyn `IIncrementalGenerator` emits a client proxy and a server dispatcher for every OPC interface marked `[OpcInterface]`. No reflection at runtime; AOT-clean and trim-safe.
- **Windows COM-callable wrappers** — when running on Windows, SCM-activated servers are exposed through raw-vtable CCWs (also `[ComImport]`-free). Full per-method vtables for `IOPCServer`, `IOPCGroupStateMgt(2)`, `IOPCItemMgt`, `IOPCSyncIO(2)`, `IOPCAsyncIO2/3`, `IConnectionPoint(Container)` plus AE `IOPCEventServer` and HDA `IOPCHDA_Server`.
- **NativeAOT + trimming compatible** across every runtime project, enforced by `IsAotCompatible`, `EnableTrimAnalyzer`, `EnableAotAnalyzer`, and an explicit [`BannedSymbols.txt`](src/BannedSymbols.txt).
- **Validation baseline** — 0 build warnings, 0 build errors, all 17 test projects green (DA 385, AE 86, HDA 123, DCOM 123, Crypto 36, SMB 22, Integration 94, plus discovery, generators, property-based, snapshot, and more).

## Quick start

Install the prerelease NuGet packages, then consume the managed contracts from the `Opc.Classic.*` namespaces:

```powershell
dotnet add package Opc.Classic.Da --prerelease
```

```csharp
using Opc.Classic.Da;

static async Task ReadOneAsync(IDaServer server, CancellationToken cancellationToken = default)
{
    IReadOnlyList<ItemValueResult> values = await server.ReadAsync(
    [
        new Item("Random.Int1") { ClientHandle = 1 },
    ], cancellationToken);

    ItemValueResult value = values[0];
    Console.WriteLine($"{value.ItemName} = {value.Value} ({value.Quality}) @ {value.Timestamp:O}");
}
```

Walk through the [first DA client tutorial](docs/tutorials/01-build-your-first-da-client.md) for an end-to-end example, or browse the [adoption guide](docs/ADOPTION.md) for hosting + deployment patterns.

## Repository layout

| Path | What's inside |
| --- | --- |
| [`src/`](src/) | Runtime assemblies, source generators, shared build props (`Directory.Build.props`), central package versions (`Directory.Packages.props`), and the AOT/trim ban list. |
| [`tests/`](tests/) | TUnit projects on Microsoft.Testing.Platform: primitives, transports, auth, generators, codecs, hosting, discovery, integration matrices, property-based, snapshot, and crypto. |
| [`samples/`](samples/) | Nine runnable apps — three servers, three clients, a loopback demo, the OPC CTT sample server, and a NativeAOT canary. |
| [`docs/`](docs/README.md) | Documentation hub: architecture, adoption, tutorials, cookbook, migration analyzer, security, conformance, roadmap, ADRs, diagrams. |
| [`docker/`](docker/README.md) | Windows Docker test fleet — CTT image, managed-server image, C-built native server/client images, `docker-compose.test.yml`. |
| [`COM/`](COM/) | OPC Foundation native C++ sample servers, kept verbatim as Windows conformance references. |
| [`External/`](External/) | OPC Foundation IDL, headers, redistributables, and specification assets used by the source generators and CTT pipeline. |
| [`.github/`](.github/) | Build, OPC CTT, Docker test fleet, and release workflows. |

## Samples

Each sample folder ships its own README with run instructions. DA/AE/HDA/CTT sample servers bind `OPC_CLASSIC_SAMPLE_PORT` on `0.0.0.0` by default, and the DA/AE/HDA sample clients dial TCP when `OPC_CLASSIC_SERVER_HOST` + `OPC_CLASSIC_SERVER_PORT` are set (otherwise they keep the in-process fallback). See [`samples/`](samples/) for the complete list. Quick map:

| Folder | What it demonstrates |
| --- | --- |
| [`Opc.Classic.Samples.DaServer/`](samples/Opc.Classic.Samples.DaServer/README.md) | Managed DA server with browse, reads, writes, and data-change publishing. |
| [`Opc.Classic.Samples.AeServer/`](samples/Opc.Classic.Samples.AeServer/README.md) | Managed AE server with areas, sources, conditions, and event metadata. |
| [`Opc.Classic.Samples.HdaServer/`](samples/Opc.Classic.Samples.HdaServer/README.md) | Managed HDA server with historical values, aggregates, and annotations. |
| [`Opc.Classic.Samples.DaClient/`](samples/Opc.Classic.Samples.DaClient/README.md) | DA client bootstrap: browse, read, write, subscribe. |
| [`Opc.Classic.Samples.AeClient/`](samples/Opc.Classic.Samples.AeClient/README.md) | AE client subscription + event acknowledgement. |
| [`Opc.Classic.Samples.HdaClient/`](samples/Opc.Classic.Samples.HdaClient/README.md) | HDA client reads, aggregates, annotations, updates. |
| [`Opc.Classic.Samples.LoopbackDemo/`](samples/Opc.Classic.Samples.LoopbackDemo/) | In-process DA client/server loopback through the managed channel stack. |
| [`Opc.Classic.Samples.CttServer/`](samples/Opc.Classic.Samples.CttServer/README.md) | Minimal CTT-oriented managed DA server registered as `Opc.Classic.DaSample.1`. |
| [`Opc.Classic.Samples.AotCanary/`](samples/Opc.Classic.Samples.AotCanary/) | NativeAOT publish verification used in CI. |

## Documentation

Start with the [documentation hub](docs/README.md). Common entry points:

- [Architecture overview](docs/ARCHITECTURE.md) — the managed DCOM stack from the listener through dispatchers.
- [Adoption guide](docs/ADOPTION.md) — host a server, consume a client, deploy on Windows/Linux.
- [Tutorials](docs/tutorials/README.md) — step-by-step walkthroughs for DA, AE, HDA, AOT, troubleshooting.
- [Cookbook](docs/cookbook/README.md) — task-flavored recipes (Linux to Matrikon, Kerberos in AD, DCOM hardening, etc.).
- [Spec coverage](docs/spec-coverage/README.md) — what's implemented per OPC sub-spec.
- [Security](docs/security/THREAT_MODEL.md) — STRIDE threat model, channel binding, NTLMSSP audit guide.
- [Migration analyzer](docs/migration/README.md) — diagnostics for porting from the legacy .NET Framework OPC API.
- [MCP integration](docs/mcp/README.md) — using Opc.Classic from VS Code Copilot, Cursor, Claude Desktop, Copilot CLI.
- [OPC CTT conformance](docs/OPC_CTT_CONFORMANCE.md) — running the OPC Compliance Test Tool against the managed sample.
- [Docker test fleet](docker/README.md) — the multi-container Windows interop test setup.
- [Roadmap](docs/ROADMAP.md) — forward-looking gates and gaps.
- [Release blockers](docs/release-blockers.md) — what stands between the rc.* series and 1.0.0 FINAL.
- [Changelog](CHANGELOG.md) — release-by-release detail.

## Build from source

Requires the .NET 10 SDK pinned by [`global.json`](global.json):

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

Targeted test subsets:

```powershell
# DA only
dotnet test tests\Opc.Classic.Da.Tests --no-build

# Solution-wide minus Windows-only and platform-blocked categories
dotnet test Opc.Classic.slnx --filter "Category!=NativeConformance&Category!=MatrikonConformance"
```

NativeAOT publish (used by CI to verify trim + AOT cleanliness):

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

The expected baseline at every commit on `master`: **0 build warnings, 0 build errors, every test project green.**

## What's still in flight

The rc.* series ships the stack through rc.7, including Track E sample DCOM-over-IP. The three items that stand between rc.7 and `1.0.0` FINAL are all infrastructure-bound:

1. **OPC CTT smoke green** — requires a Windows host with Docker Desktop in Windows-container mode to execute the [test fleet](docker/README.md) and triage results.
2. **NTLMv2 wire test against a real Windows Server** — requires a live Active Directory lab; the [audit guide](docs/security/NTLMSSP_AUDIT_GUIDE.md) and [wire fixture replay tests](tests/Opc.Classic.Dcom.Crypto.Tests/) document the sandbox-feasible coverage available today.
3. **Third-party NTLMSSP crypto audit** — process work; the audit guide enumerates code surface, primitives, and test coverage to start.

Details and remediation paths are in [`docs/release-blockers.md`](docs/release-blockers.md).

A handful of advanced Windows-CCW features remain documented as future work (full `IEnumConnections` / `IEnumConnectionPoints` CCW infrastructure, AE `IOPCEventSubscriptionMgt` advanced filter marshaling, HDA `OPCHDA_ITEM[]` read marshaling). The cross-platform DCOM transport path covers these today; the CCW-side gaps are tracked in [`CHANGELOG.md`](CHANGELOG.md) "Known gaps" sections.

## Contributing and license

Contributions are welcome. Please read [`CONTRIBUTING.md`](CONTRIBUTING.md) before opening a pull request, and report security issues through [`SECURITY.md`](SECURITY.md).

Opc.Classic is licensed under the [MIT License](LICENSE). The `External/` and `COM/` trees retain their upstream OPC Foundation Sample Code license terms; see [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for the full attribution list.

Opc.Classic is an independent implementation. It is not endorsed by, affiliated with, or certified by the OPC Foundation. OPC, OPC Classic, OPC DA, OPC AE, OPC HDA, OPC UA and related marks are trademarks of the OPC Foundation. Use of those marks here refers only to the published specifications this codebase implements.

