# Opc.Classic - "A modern classic"

[![Build](https://github.com/marcschier/opc-classic/actions/workflows/build.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/build.yml)
[![Docker test fleet](https://github.com/marcschier/opc-classic/actions/workflows/docker-test-fleet.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/docker-test-fleet.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A cross-platform, NativeAOT-compatible **.NET 10** implementation of OPC Classic for clients and servers. Managed DCOM/MSRPC with no Windows COM runtime interop, XML-DA over HTTP, source-generated proxies and dispatchers, and self-contained NTLMv2 / Kerberos / SPNEGO authentication.

> **Status — release candidate.**

## What you get

- **Cross-platform OPC Classic** — DA (1.0 / 2.05a / 3.0), AE 1.10, HDA 1.20, Batch 2.00, Commands, Complex Data, DX, Security, and XML-DA over HTTP. Runs on Windows, Linux, and macOS (.NET 10). No call to native OS code other than what .net provides.
- **Managed DCOM/MSRPC stack** — full MS-DCOM PDU framing, NDR marshaling, OBJREF/ORPC, packet integrity + privacy, channel binding (RFC 5056/5929). No `[ComImport]`, no Windows COM runtime dependency.
- **Self-contained authentication** — NTLMv2, Kerberos (RC4-HMAC + AES128/256), SPNEGO with `mechListMIC`, channel binding tokens. Cryptography (MD4, RC4) ships in-tree with RFC test vectors; MD5 / HMAC / DES / AES come from the BCL.
- **Source-generated proxies and dispatchers** — Roslyn `IIncrementalGenerator` emits a client proxy and a server dispatcher for every OPC interface marked `[OpcInterface]`. No reflection at runtime; AOT-clean and trim-safe.
- **Windows COM-callable wrappers** — when running on Windows, SCM-activated servers are exposed through raw-vtable CCWs (also `[ComImport]`-free). Full release-scope vtables cover DA server/group/item/sync/async I/O/connection-point paths, AE server/subscription array-heavy methods, and HDA server/read/update/advise/playback paths.
- **NativeAOT + trimming compatible** across every runtime project, enforced by `IsAotCompatible`, `EnableTrimAnalyzer`, `EnableAotAnalyzer`, and an explicit `BannedSymbols.txt`.
- **Validation baseline** — 0 build warnings, 0 build errors, 2758 passed / 13 skipped / 0 failed across all 25 test projects (DA 475, AE 128, HDA 177, DCOM 123, Crypto 65, Kerberos 48, SMB 61, Integration 109, MCP 118, MCP Capture 99, plus core, discovery, generators, property-based, snapshot, XML-DA, and more).

## Quick start

Install the NuGet packages, then consume the managed contracts from the `Opc.Classic.*` namespaces:

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
| `src` | Runtime assemblies, source generators, shared build props, central package versions, and the AOT/trim ban list. |
| `tests` | TUnit projects on Microsoft.Testing.Platform: primitives, transports, auth, generators, codecs, hosting, discovery, integration matrices, property-based, snapshot, and crypto. |
| [samples/](samples/README.md) | Ten runnable apps — three DA/AE/HDA servers, three clients, a loopback demo, an additional managed DA sample (CttServer), the OPC Security sample server, and a NativeAOT canary. |
| [docs/](docs/README.md) | Documentation hub: architecture, adoption, tutorials, cookbook, migration analyzer, security, conformance, roadmap, and architecture diagrams. |
| [Docker test fleet](interop/docker/README.md) | Windows Docker test fleet — managed-server image, C-built native server/client images, OPC Foundation TestServer/TestClient images, and the docker-compose. |
| [interop/](interop/README.md) | OPC Foundation conformance assets: vendored IDL headers, CMake-built native sample servers/test apps, and helper scripts. |
| `.github` | Build, Docker test fleet, and release workflows. |

## Samples

Ten runnable apps live under [samples/](samples/README.md) — three DA/AE/HDA managed servers, three clients, an in-process loopback, an additional managed DA sample (CttServer), the OPC Security sample server, and a NativeAOT canary. See the [samples README](samples/README.md) for the map, run instructions, env-var conventions, and sample-container deployment.

## Documentation

Start with the [documentation hub](docs/README.md). Common entry points:

- [Architecture overview](docs/ARCHITECTURE.md) — the managed DCOM stack from the listener through dispatchers.
- [Adoption guide](docs/ADOPTION.md) — host a server, consume a client, deploy on Windows/Linux.
- [Tutorials](docs/tutorials/README.md) — step-by-step walkthroughs for DA, AE, HDA, AOT, troubleshooting.
- [Cookbook](docs/cookbook/README.md) — task-flavored recipes (Linux to Matrikon, Kerberos in AD, DCOM hardening, [OPC Security](docs/cookbook/08-implementing-opc-security.md), etc.).
- [Spec coverage](docs/CONFORMANCE.md) — what's implemented per OPC sub-spec.
- [Security](docs/security/THREAT_MODEL.md) — STRIDE threat model, channel binding, NTLMSSP audit guide.
- [Migration analyzer](docs/migration/README.md) — diagnostics for porting from the legacy .NET Framework OPC API.
- [MCP integration](docs/mcp/README.md) — using Opc.Classic from VS Code Copilot, Cursor, Claude Desktop, Copilot CLI.
- [Docker test fleet](interop/docker/README.md) — the multi-container Windows interop test setup.
- [Roadmap](docs/ROADMAP.md) — forward-looking gates and gaps.
- [Changelog](CHANGELOG.md) — release-by-release detail.

## Build from source

Requires the .NET 10 SDK pinned by [global.json](global.json):

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

## Contributing and license

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request, and report security issues through [SECURITY.md](SECURITY.md).

Opc.Classic is licensed under the [MIT License](LICENSE). The `interop/` tree retains upstream OPC Foundation sample, redistributable, and reference-asset license terms; see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for the full attribution list.

Opc.Classic is an independent implementation. It is not endorsed by, affiliated with, or certified by the OPC Foundation. OPC, OPC Classic, OPC DA, OPC AE, OPC HDA, OPC UA and related marks are trademarks of the OPC Foundation. Use of those marks here refers only to the published specifications this codebase implements.
