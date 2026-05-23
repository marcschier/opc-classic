# Opc.Classic

[![Build](https://github.com/marcschier/opc-classic/actions/workflows/build.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/build.yml)
[![OPC CTT](https://github.com/marcschier/opc-classic/actions/workflows/opc-ctt.yml/badge.svg)](https://github.com/marcschier/opc-classic/actions/workflows/opc-ctt.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-clean-brightgreen)](samples/Opc.Classic.Samples.AotCanary/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Opc.Classic** is a cross-platform, NativeAOT-compatible **.NET 10** implementation of OPC Classic for client and server workloads. It brings DA, AE, HDA, DX, Complex Data, Batch, Commands, Security, and XML-DA into a modern managed stack that runs on Linux, macOS, and Windows without depending on Windows DCOM automation or legacy COM runtime interop.

> **Status:** pre-1.0 alpha. The public assembly names, package IDs, and namespaces now use the `Opc.Classic.*` dotted form, and the project is licensed under MIT.

## Quick start

Install the DA package from the prerelease feed:

```powershell
dotnet add package Opc.Classic.Da --prerelease
```

Read an OPC DA item through the async managed surface once discovery/activation has provided an `IDaServer`:

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

The alpha connection bootstrap continues to settle around discovery, activation, and generated DCOM proxies; `IDaServer`, `IDaSubscription`, and the per-spec managed contracts are the consumer-facing shape.

## Why Opc.Classic?

- **Cross-platform OPC Classic stack.** The DCOM path is pure managed MSRPC/DCOM over `ncacn_ip_tcp`; XML-DA uses `HttpClient`. No Windows-only `[ComImport]`, RCW activation, or `ole32.dll` dependency is required for the portable stack.
- **Full OPC Classic family coverage.** Assemblies cover DA, AE, HDA, DX, Complex Data, Batch, Commands, Security, and XML-DA.
- **Source-generator-driven DCOM proxies.** `[OpcInterface(iid)]`, `[OpcMethod(opnum)]`, and `[GenerateOpcProxy]` emit interface IDs, opnum tables, and client proxies with no runtime reflection dispatch.
- **NativeAOT-compatible by design.** Runtime source projects are written for trimming and AOT; the canary at [`samples/Opc.Classic.Samples.AotCanary/`](samples/Opc.Classic.Samples.AotCanary/) verifies publish-time cleanliness.
- **Modern authentication.** NTLMv2 is the default, Kerberos is available through Kerberos.NET, SPNEGO token negotiation is implemented, and channel binding supports Extended Protection for Authentication.
- **Hardened DCOM defaults.** DCOM traffic defaults to packet integrity, NTLMv2, and NTLM2 session security to align with Microsoft KB5004442 hardening.
- **License-clean.** The project is MIT licensed and has removed the LGPL `SharpCifs.Std` transitional runtime dependency.
- **Deep tests.** 700+ TUnit tests span unit, property, snapshot, generator, loopback, and conformance scaffolding across the `tests/` tree, with coverage gates in CI.

## Repository layout

| Path | Purpose |
| --- | --- |
| [`src/`](src/) | Production assemblies. Shared props enforce .NET 10, analyzer, package metadata, AOT, trimming, and source-generator wiring. |
| [`tests/`](tests/) | TUnit projects for primitives, DCOM transport/auth, generators, per-spec codecs, hosting, discovery, property tests, and integration loops. |
| [`samples/`](samples/) | Runnable samples, including the AOT canary and managed DA/AE/HDA server samples. |
| [`docs/`](docs/) | Architecture notes, release/conformance docs, XML-DA status, and documentation site content. |
| [`COM/`](COM/) | Preserved OPC Foundation native C++ sample servers used as Windows conformance references. |
| [`External/`](External/) | Preserved OPC Foundation redistributables and merge modules used by conformance jobs. |
| [`.github/`](.github/) | GitHub Actions workflows for build, conformance, OPC CTT, release, and repository guidance. |

## Sample apps

| Sample | What it demonstrates |
| --- | --- |
| [`samples/Opc.Classic.Samples.AotCanary/`](samples/Opc.Classic.Samples.AotCanary/) | NativeAOT publish verification for consumer applications. CI treats IL2xxx/IL3xxx warnings as regressions. |
| [`samples/Opc.Classic.Samples.CttServer/`](samples/Opc.Classic.Samples.CttServer/) | Minimal CTT-oriented managed DA server registered as `Opc.Classic.DaSample.1`. |
| [`samples/Opc.Classic.Samples.DaServer/`](samples/Opc.Classic.Samples.DaServer/) | Full DA server sample with a tag tree, browse support, reads/writes, and data-change publishing. |
| [`samples/Opc.Classic.Samples.AeServer/`](samples/Opc.Classic.Samples.AeServer/) | AE server sample with area/source hierarchy, condition events, and event-category metadata. |
| [`samples/Opc.Classic.Samples.HdaServer/`](samples/Opc.Classic.Samples.HdaServer/) | HDA server sample with historical values, aggregates, annotations, and time-range queries. |

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

## Documentation

- [Architecture](docs/ARCHITECTURE.md) — top-down design of transports, NDR codecs, source generators, hosting, auth, discovery, tests, AOT, CI, and roadmap.
- [Adoption guide](docs/ADOPTION.md) — migration guidance for consumers adopting the alpha packages.
- [Cookbook](docs/COOKBOOK.md) — recipes for reads, writes, subscriptions, XML-DA, hosting, and conformance workflows.
- [XML-DA status](docs/XMLDA_STATUS.md) — operation coverage and serializer notes for SOAP-over-HTTP.
- [Changelog](CHANGELOG.md) — release history from `0.1.0-alpha.1` through the current alpha.

## Contributing and governance

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request, and report security issues through [SECURITY.md](SECURITY.md). Preserved OPC Foundation material under `COM/` and `External/` retains its original notices; project source is MIT licensed.

## License

Opc.Classic is licensed under the [MIT License](LICENSE).
