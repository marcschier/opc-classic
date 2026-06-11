# Third-Party Notices

This project incorporates components from the projects listed below.
The licenses governing the use of these components are included below.

## Project License

Opc.Classic .NET is licensed under MIT. See [LICENSE](LICENSE).

## Production and Tool Dependencies

### Microsoft.Extensions.*
- Source: https://github.com/dotnet/runtime
- License: MIT
- Used in: `Opc.Classic` projects and `Opc.Classic` for configuration, dependency injection, hosting, logging, and options abstractions
- Version: 10.0.5

### Kerberos.NET
- Source: https://github.com/dotnet/Kerberos.NET
- License: MIT
- Used in: `Opc.Classic.Dcom`
- Version: 4.6.146

### ModelContextProtocol
- Source: https://github.com/modelcontextprotocol/csharp-sdk
- License: MIT
- Used in: `Opc.Classic`
- Version: 1.2.0

## Build/Generator Dependencies

### Microsoft.CodeAnalysis.CSharp / Workspaces
- Source: https://github.com/dotnet/roslyn
- License: MIT
- Used in: `Opc.Classic`, `Opc.Classic`, and analyzer tests (build-time only)
- Version: 4.11.0

### Microsoft.CodeAnalysis.Analyzers
- Source: https://github.com/dotnet/roslyn-analyzers
- License: MIT
- Used in: `Opc.Classic` and `Opc.Classic` (build-time analyzer dependency)
- Version: 3.11.0

### Microsoft.CodeAnalysis.BannedApiAnalyzers
- Source: https://github.com/dotnet/roslyn-analyzers
- License: MIT
- Used in: all `src\` projects through `Directory.Build` (build-time only)
- Version: 3.3.4

### Microsoft.SourceLink.GitHub
- Source: https://github.com/dotnet/sourcelink
- License: MIT
- Used in: all `src\` projects through `Directory.Build` (build-time only)
- Version: 8.0.0

### Meziantou.Analyzer
- Source: https://github.com/meziantou/Meziantou.Analyzer
- License: MIT
- Used in: all `src\` projects through `Directory.Build` (build-time only)
- Version: 2.0.197

### Microsoft.VisualStudio.Threading.Analyzers
- Source: https://github.com/microsoft/vs-threading
- License: MIT
- Used in: all `src\` projects through `Directory.Build` (build-time only)
- Version: 17.13.61

## Test Dependencies (tests/)

### TUnit (+ TUnit.Assertions, TUnit.Engine)
- Source: https://github.com/thomhurst/TUnit
- License: MIT
- Used in: test projects through Directory.Build tests
- Version: TUnit 0.13.0; TUnit.Assertions 0.13.0; TUnit.Engine transitive via TUnit

### Testcontainers
- Source: https://github.com/testcontainers/testcontainers-dotnet
- License: MIT
- Used in: Kerberos and integration test projects
- Version: 4.0.0

### coverlet.collector / coverlet.msbuild
- Source: https://github.com/coverlet-coverage/coverlet
- License: MIT
- Used in: test projects through Directory.Build tests
- Version: 6.0.4

### CsCheck
- Source: https://github.com/AnthonyLloyd/CsCheck
- License: Apache-2.0
- Used in: Opc.Classic tests
- Version: 4.4.0

### Verify.TUnit
- Source: https://github.com/VerifyTests/Verify
- License: MIT
- Used in: snapshot tests
- Version: 28.10.0

### Microsoft.Extensions test helpers
- Source: https://github.com/dotnet/runtime and https://github.com/dotnet/extensions
- License: MIT
- Used in: integration, MCP, DA, discovery, and logging tests
- Version: Microsoft.Extensions.* 10.0.5; Microsoft.Extensions.Diagnostics.Testing 10.1.0

### Microsoft.CodeAnalysis Analyzer/CodeFix Testing
- Source: https://github.com/dotnet/roslyn-sdk
- License: MIT
- Used in: Opc.Classic.MigrationAnalyzer tests
- Version: 1.1.2

## Reference / Conformance Assets (NOT redistributed in built packages)

### interop/samples/ (native C++ OPC Foundation sample servers)
- Source: OPC Foundation
- License: OPC Foundation sample license (older Sample Server SDK) — preserved verbatim in the original files
- Status: Used only as a conformance reference; no redistribution

### interop/ (vendored OPC-Classic-CoreComponents sources)
- Source: OPC Foundation [OPC-Classic-CoreComponents](https://github.com/OPCFoundation/OPC-Classic-CoreComponents) repository
- License: OPC Foundation MIT License 1.00 (per-source-file headers); `LICENSE.md` at the vendor root is the umbrella OPC Foundation specification license
- Status: Vendored so the native C++ DA 2.05a TestServer + proxy/stub DLLs can be built without an external clone. Not redistributed in the published Opc.Classic NuGet packages.

### interop/ (OPC Foundation reference + redistributable assets)
- Source: OPC Foundation
- License: OPC Foundation specification, sample, and SDK terms preserved in-place
- Subfolders:
  - `inc` — OPC IDL/headers used by the Docker C-server and C-client builds
  - `interop/` — OPC COM Core Components readme PDF and vendored CoreComponents sources
  - `samples` — OPC Foundation native C++ sample servers used by conformance validation
  - `interop/` — pruned/restructured vendoring of the OPC-Classic-CoreComponents repository (MIT-licensed); see entry above
- Status: Used for IDL definitions, redistributable inputs, conformance validation, and developer reference; do not redistribute outside the rights granted by the OPC Foundation

### Spec reference markdown (extracted to private companion repo)
- Source: OPC Foundation specification PDFs + Microsoft Open Specifications
- Location: [`marcschier/opc-classic-docs`](https://github.com/marcschier/opc-classic-docs) (private)
- License: OPC Foundation specification distribution terms + Microsoft Open Specifications Promise (see `NOTICES.md` in that repo)
- Status: Internal reference only. The mirrors were extracted from `docs` of this repo in commit `(see Changelog [Unreleased] Removed)` to keep this repo lean.
