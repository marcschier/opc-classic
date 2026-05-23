# opc-classic

A cross-platform, NativeAOT-compatible **.NET 10** implementation of OPC Classic (DA, AE, HDA, DX, Cpx, Batch, Commands, Security, XML-DA), with both **client** and **server** hosting on Linux / macOS / Windows.

> **Status: early development.** This repository is being restructured per the implementation plan in `~/.copilot/session-state/<session-id>/plan.md`. The existing `COM/`, `COM.Net/`, `DotNet/`, `Java/`, and `External/` folders are the legacy trees being migrated or retired (Phase 1A).

## Goals

- **Cross-platform.** Talk to and host OPC DA / AE / HDA servers from Linux and macOS, not just Windows. No `[ComImport]`, no `ole32.dll` P/Invoke, no Windows-specific runtime — a pure-managed DCOM stack is the foundation.
- **NativeAOT-compatible libraries.** Consumer apps can publish with `dotnet publish -p:PublishAot=true` against any `Opc.Classic.*` package and see zero `IL2xxx` / `IL3xxx` warnings. All in-tree code is source-generated or hand-rolled — no reflection emit, no expression-tree compile, no `MethodInfo.Invoke`.
- **Full OPC Classic spec coverage** — DA 2.05a + 3.0, AE 1.x, HDA 1.x, DX, Cpx, Batch, Commands, Security, XML-DA.
- **Modern auth** — NTLMv2 + Kerberos / SPNEGO, defaulting to `PKT_INTEGRITY` for compatibility with Microsoft's mandatory DCOM hardening (KB5004442, phase-3, March 2023).
- **Both client and server.** Hosting a managed OPC server cross-platform via `Microsoft.Extensions.Hosting`.
- **Fully tested.** [TUnit](https://github.com/thomhurst/TUnit) with source-generator-integrated mocking, managed loopback integration tests, and layered conformance against the native C++ sample servers preserved under `COM/`, Matrikon OPC Simulation, and optionally the OPC Foundation Compliance Test Tool.

## Project layout (target)

| Folder | Purpose |
|---|---|
| `src/` | Production source — every assembly `IsAotCompatible=true`. |
| `tests/` | TUnit-based unit, integration, and conformance test projects. |
| `samples/` | Quickstart sample apps and the AOT canary (`samples/Opc.Classic.Samples.AotCanary`). |
| `docs/` | DocFX site, ARCHITECTURE, cookbook, migration guide. |
| `COM/` | **Preserved** native C++ OPC sample servers (conformance reference; not built by default). |
| `External/` | **Preserved** OPC Foundation Core Components — downloaded by CI for Windows conformance jobs. |
| `.github/workflows/` | Linux / macOS / Windows build matrix + AOT-canary publish gate. |

The legacy `DotNet/`, `Java/`, and `COM.Net/` folders migrate into the new layout per Phase 1A of the implementation plan.

## Building

Requires **.NET 10 SDK** (10.0.100 or later). See [`global.json`](global.json) for the exact version. From the repository root:

```bash
dotnet restore
dotnet build
dotnet test
```

To verify NativeAOT-compatibility:

```bash
dotnet publish samples/Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true
```

Zero warnings = AOT-clean. Any `IL2xxx` or `IL3xxx` warning is treated as a regression by CI.

## Architecture

A pure-managed MSRPC/DCOM stack (`Opc.Classic.Dcom`) is the foundation. On top of it sit per-spec assemblies (`Opc.Classic.Da`, `Opc.Classic.Ae`, `Opc.Classic.Hda`, …) that translate the OPC Classic interface semantics into managed APIs. A Roslyn source generator (`Opc.Classic.Generators`) emits AOT-safe call shims, NDR marshallers, and `LocalCoClass` dispatch tables — replacing every reflection / expression-tree code path that would otherwise break NativeAOT.

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) (coming as part of Phase 15A).

## License

[MIT](LICENSE). Per-file `SPDX-License-Identifier` headers are being added as code is migrated.

## Contributing

`CONTRIBUTING.md` lands with Phase 15D. For the active roadmap, see the implementation plan in the session-state folder.
