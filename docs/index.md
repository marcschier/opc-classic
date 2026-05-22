# OPC Classic .NET

A cross-platform, NativeAOT-compatible **.NET 10** implementation of OPC Classic
(DA, AE, HDA, DX, Cpx, Batch, Commands, Security, XML-DA) with both client and
server hosting on Linux, macOS, and Windows.

- **[Architecture](ARCHITECTURE.md)** — top-down design and per-spec coverage
- **[Cookbook](cookbook/README.md)** — focused how-to articles
- **[API reference](api/index.md)** — every public type, method, property in src/
- **[Contributing](https://github.com/marcschier/opc-classic/blob/main/CONTRIBUTING.md)**
- **[Changelog](https://github.com/marcschier/opc-classic/blob/main/CHANGELOG.md)**

## Quick start

```bash
dotnet add package OpcClassic.Core
dotnet add package OpcClassic.Da
```

(Pre-1.0 — packages are not yet published to nuget.org.)
