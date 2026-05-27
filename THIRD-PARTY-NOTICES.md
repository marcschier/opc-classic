# Third-Party Notices

This project incorporates components from the projects listed below.
The licenses governing the use of these components are included below.

## Project License

Opc.Classic .NET is licensed under MIT. See [LICENSE](LICENSE).

## Production and Tool Dependencies

### Microsoft.Extensions.*
- Source: https://github.com/dotnet/runtime
- License: MIT
- Used in: `src\Opc.Classic.*` projects and `mcp\Opc.Classic.Mcp` for configuration, dependency injection, hosting, logging, and options abstractions
- Version: 10.0.5

### Kerberos.NET
- Source: https://github.com/dotnet/Kerberos.NET
- License: MIT
- Used in: `src\Opc.Classic.Dcom.Kerberos`
- Version: 4.6.146

### ModelContextProtocol
- Source: https://github.com/modelcontextprotocol/csharp-sdk
- License: MIT
- Used in: `mcp\Opc.Classic.Mcp`
- Version: 1.2.0

## Build/Generator Dependencies

### Microsoft.CodeAnalysis.CSharp / Workspaces
- Source: https://github.com/dotnet/roslyn
- License: MIT
- Used in: `src\Opc.Classic.Generators`, `src\Opc.Classic.MigrationAnalyzer`, and analyzer tests (build-time only)
- Version: 4.11.0

### Microsoft.CodeAnalysis.Analyzers
- Source: https://github.com/dotnet/roslyn-analyzers
- License: MIT
- Used in: `src\Opc.Classic.Generators` and `src\Opc.Classic.MigrationAnalyzer` (build-time analyzer dependency)
- Version: 3.11.0

### Microsoft.CodeAnalysis.BannedApiAnalyzers
- Source: https://github.com/dotnet/roslyn-analyzers
- License: MIT
- Used in: all `src\` projects through `src\Directory.Build.props` (build-time only)
- Version: 3.3.4

### Microsoft.SourceLink.GitHub
- Source: https://github.com/dotnet/sourcelink
- License: MIT
- Used in: all `src\` projects through `src\Directory.Build.props` (build-time only)
- Version: 8.0.0

### Meziantou.Analyzer
- Source: https://github.com/meziantou/Meziantou.Analyzer
- License: MIT
- Used in: all `src\` projects through `src\Directory.Build.props` (build-time only)
- Version: 2.0.197

### Microsoft.VisualStudio.Threading.Analyzers
- Source: https://github.com/microsoft/vs-threading
- License: MIT
- Used in: all `src\` projects through `src\Directory.Build.props` (build-time only)
- Version: 17.13.61

## Test Dependencies (tests/)

### TUnit (+ TUnit.Assertions, TUnit.Engine)
- Source: https://github.com/thomhurst/TUnit
- License: MIT
- Used in: test projects through `tests\Directory.Build.props`
- Version: TUnit 0.13.0; TUnit.Assertions 0.13.0; TUnit.Engine transitive via TUnit

### Testcontainers
- Source: https://github.com/testcontainers/testcontainers-dotnet
- License: MIT
- Used in: Kerberos and integration test projects
- Version: 4.0.0

### coverlet.collector / coverlet.msbuild
- Source: https://github.com/coverlet-coverage/coverlet
- License: MIT
- Used in: test projects through `tests\Directory.Build.props`
- Version: 6.0.4

### CsCheck
- Source: https://github.com/AnthonyLloyd/CsCheck
- License: Apache-2.0
- Used in: `tests\Opc.Classic.PropertyTests`
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
- Used in: `tests\Opc.Classic.MigrationAnalyzer.Tests`
- Version: 1.1.2

## Reference / Conformance Assets (NOT redistributed in built packages)

### COM/ (native C++ OPC Foundation sample servers)
- Source: OPC Foundation
- License: OPC Foundation sample license — preserved verbatim in the original files
- Status: Used only as a conformance reference; no redistribution

### External/Include/ and External/CTT/
- Source: OPC Foundation
- License: OPC Foundation specification, sample, and CTT installer terms preserved in-place
- Status: Used for IDL definitions, redistributable inputs, and conformance validation; do not redistribute outside the rights granted by the OPC Foundation
