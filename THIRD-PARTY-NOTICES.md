# Third-Party Notices

This project incorporates components from the projects listed below.
The licenses governing the use of these components are included below.

## Production Dependencies (src/)

### Microsoft.Extensions.Logging.Abstractions
- Source: https://github.com/dotnet/runtime
- License: MIT
- Used in: OpcClassic.Dcom
- Version: 10.0.0

### SharpCifs.Std
- Source: https://github.com/iyasai/SharpCifs.Std
- License: LGPL-2.1
- Status: TRANSITIONAL — will be replaced in Phase 2D
- Used in: OpcClassic.Dcom
- Version: 0.2.13

## Build/Generator Dependencies

### Microsoft.CodeAnalysis.CSharp
- Source: https://github.com/dotnet/roslyn
- License: MIT
- Used in: OpcClassic.Generators (build-time only, not redistributed at runtime)
- Version: 4.11.0

### Microsoft.CodeAnalysis.Analyzers
- Source: https://github.com/dotnet/roslyn-analyzers
- License: MIT
- Used in: OpcClassic.Generators (build-time analyzer dependency)
- Version: 3.11.0

### Microsoft.CodeAnalysis.BannedApiAnalyzers
- Source: https://github.com/dotnet/roslyn-analyzers
- License: MIT
- Used in: all src/ projects through src/Directory.Build.props (build-time only)
- Version: 3.3.4

### Microsoft.SourceLink.GitHub
- Source: https://github.com/dotnet/sourcelink
- License: MIT
- Used in: all src/ projects through src/Directory.Build.props (build-time only)
- Version: 8.0.0

### Meziantou.Analyzer
- Source: https://github.com/meziantou/Meziantou.Analyzer
- License: MIT
- Used in: all src/ projects through src/Directory.Build.props (build-time only)
- Version: 2.0.197

### Microsoft.VisualStudio.Threading.Analyzers
- Source: https://github.com/microsoft/vs-threading
- License: MIT
- Used in: all src/ projects through src/Directory.Build.props (build-time only)
- Version: 17.13.61

## Test Dependencies (tests/)

### TUnit (+ TUnit.Assertions, TUnit.Engine)
- Source: https://github.com/thomhurst/TUnit
- License: MIT
- Used in: test projects through tests/Directory.Build.props
- Version: TUnit 0.13.0; TUnit.Assertions 0.13.0; TUnit.Engine transitive via TUnit

### coverlet.collector / coverlet.msbuild
- Source: https://github.com/coverlet-coverage/coverlet
- License: MIT
- Used in: test projects through tests/Directory.Build.props
- Version: 6.0.4

### CsCheck
- Source: https://github.com/AnthonyLloyd/CsCheck
- License: Apache-2.0
- Used in: OpcClassic.PropertyTests
- Version: 4.4.0

### Verify.TUnit
- Source: https://github.com/VerifyTests/Verify
- License: MIT
- Used in: tests/ central package versions for snapshot tests
- Version: 28.10.0

### Microsoft.Extensions.Logging.Abstractions
- Source: https://github.com/dotnet/runtime
- License: MIT
- Used in: OpcClassic.Dcom.Logging.Tests
- Version: 10.0.0

### Microsoft.Extensions.Logging.Testing
- Source: https://github.com/dotnet/runtime
- License: MIT
- Used in: tests/ central package versions for log assertions
- Version: 10.0.0

## Reference / Conformance Assets (NOT redistributed in built packages)

### COM/ (native C++ OPC Foundation sample servers)
- Source: OPC Foundation
- License: OPC Foundation sample license — preserved verbatim in the original files
- Status: Used only as a conformance reference; no redistribution

### External/Include/ (OPC IDL/header files)
- Source: OPC Foundation
- License: OPC Foundation specification (used for IDL definitions only)

## Historical / Archived

### SharpInterop (basis for src/OpcClassic.Dcom)
- Original author: Vikram Roopchand (c) 2013
- License: EPL-1.0
- The OpcClassic.Dcom assembly preserves the EPL-1.0 lineage; project-wide license is also EPL-1.0.

### j-Interop (Java upstream of SharpInterop)
- License: LGPL-3.0
- Status: Java sources removed from working tree; git history retains them

### DotNet/ (OPC Foundation .NET Framework 4.6.2 API — design reference)
- Source: OPC Foundation
- License: OPC Foundation sample license
- Status: Removed from working tree; type designs migrated under SPDX attribution
