# Opc.Classic

[![Build](https://github.com/marcschier/opc-classic/actions/workflows/build.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/build.yml)
[![OPC CTT](https://github.com/marcschier/opc-classic/actions/workflows/opc-ctt.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/opc-ctt.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-0.6.0--alpha.1-blue)](CHANGELOG.md)
[![Next](https://img.shields.io/badge/next-1.0.0--rc.1-purple)](docs/ROADMAP.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Opc.Classic** is a cross-platform, NativeAOT-compatible **.NET 10** implementation of OPC Classic for client and server workloads. It provides managed DCOM/MSRPC, XML-DA over HTTP, source-generated proxies and server dispatchers, and modern authentication without relying on Windows COM runtime interop.

Current release: **0.6.0-alpha.1**. The documentation and release gates are being prepared for **1.0.0-rc.1**.

## What is included

- OPC sub-spec coverage for **DA, AE, HDA, Batch, Commands, Complex Data, DX, Security, and Discovery**, plus XML-DA support.
- Source-generated client proxies and server dispatchers (**47 dispatchers / 127 opnums**) with AOT-safe call paths.
- Self-contained NTLMv2, Kerberos packet protection for five encryption types, SPNEGO negotiation, and channel binding token support.
- Managed DCOM activation through `IRemoteSCMActivator` v5.6 and a real `OpcEnumClient`.
- NativeAOT and trimming compatibility across source projects.
- MIT licensing and centralized third-party notices.
- Validation baseline: **0 build warnings**, **0 build errors**, **1253 passed / 24 skipped / 0 failed** tests.

## Quick start

Install a prerelease package, then use the managed per-spec contracts from `Opc.Classic.*` namespaces:

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

## Documentation

Start with the [documentation hub](docs/README.md), then use the [roadmap](docs/ROADMAP.md) for forward-looking release gates and known coverage gaps.

Common entry points:

- [Architecture overview](docs/ARCHITECTURE.md)
- [Adoption guide](docs/ADOPTION.md)
- [Tutorial: build your first DA client](docs/tutorials/01-build-your-first-da-client.md)
- [Cookbook recipes](docs/cookbook/README.md)
- [Migration analyzer diagnostics](docs/migration/README.md)
- [Threat model](docs/security/THREAT_MODEL.md)
- [OPC CTT conformance](docs/OPC_CTT_CONFORMANCE.md)
- [Changelog](CHANGELOG.md)

## Sample apps

| Sample | What it demonstrates |
| --- | --- |
| [`samples/Opc.Classic.Samples.DaServer/`](samples/Opc.Classic.Samples.DaServer/) | Managed DA server with tag tree, browse support, reads, writes, and data-change publishing. |
| [`samples/Opc.Classic.Samples.AeServer/`](samples/Opc.Classic.Samples.AeServer/) | Managed AE server with areas, sources, condition events, and event-category metadata. |
| [`samples/Opc.Classic.Samples.HdaServer/`](samples/Opc.Classic.Samples.HdaServer/) | Managed HDA server with historical values, aggregates, annotations, and time-range queries. |
| [`samples/Opc.Classic.Samples.DaClient/`](samples/Opc.Classic.Samples.DaClient/) | DA client bootstrap, browse, read, write, and subscription flows. |
| [`samples/Opc.Classic.Samples.AeClient/`](samples/Opc.Classic.Samples.AeClient/) | AE client subscription and event acknowledgement flows. |
| [`samples/Opc.Classic.Samples.HdaClient/`](samples/Opc.Classic.Samples.HdaClient/) | HDA client reads, aggregates, annotations, and updates. |
| [`samples/Opc.Classic.Samples.LoopbackDemo/`](samples/Opc.Classic.Samples.LoopbackDemo/) | In-process DA client/server loopback through the managed channel stack. |
| [`samples/Opc.Classic.Samples.CttServer/`](samples/Opc.Classic.Samples.CttServer/) | Minimal CTT-oriented managed DA server registered as `Opc.Classic.DaSample.1`. |
| [`samples/Opc.Classic.Samples.AotCanary/`](samples/Opc.Classic.Samples.AotCanary/) | NativeAOT publish verification for consumer applications. |

## Build from source

Requires the .NET 10 SDK pinned by [`global.json`](global.json):

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

Publish the NativeAOT canary:

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

## Repository layout

| Path | Purpose |
| --- | --- |
| [`src/`](src/) | Runtime assemblies, generators, shared build props, AOT/trimming settings, and banned API rules. |
| [`tests/`](tests/) | TUnit projects for primitives, transports, auth, generators, codecs, hosting, discovery, property tests, snapshots, and integration loops. |
| [`samples/`](samples/) | The nine runnable sample applications listed above. |
| [`docs/`](docs/) | Plain Markdown documentation hub, tutorials, cookbook, security notes, migration diagnostics, diagrams, conformance docs, and roadmap. |
| [`COM/`](COM/) | OPC Foundation native C++ sample servers used as Windows conformance references. |
| [`External/`](External/) | OPC Foundation headers, IDL, redistributables, and specification assets used for conformance and code generation validation. |
| [`.github/`](.github/) | CI workflows and repository guidance. |

## Contributing and license

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request, and report security issues through [SECURITY.md](SECURITY.md).

Opc.Classic is licensed under the [MIT License](LICENSE). See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for dependency and reference-asset notices.
