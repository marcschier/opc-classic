# Repository: opc-classic

OPC Classic (DA / AE / HDA / DX / Batch / Security) sample servers, clients, and APIs spanning **three implementation stacks**: native C++ COM servers, a .NET Framework 4.6.2 wrapper API, and a .NET Standard 2.0 pure-managed DCOM client (`SharpInterop`).

There is **no README, CI, or test suite** in this repo — it's a port/aggregation of OPC Foundation sample code, OPC Foundation .NET API, and a C# port of Java's j-Interop. Treat each top-level folder as an independent project.

## The four solutions / build targets

| Solution | Lang / Framework | Build with | Notes |
|---|---|---|---|
| `OPC COM.sln` | C++ COM, Win32, v141 toolset, Windows 8.1 SDK | Visual Studio 2017 or `msbuild` | Most projects are **Win32-only**; `x64` configs map back to `Win32`. Only `OpcUtils` builds true x64. |
| `OPC DotNet.sln` | C# .NET Framework **4.6.2** (legacy csproj) | Visual Studio or `msbuild` (Windows only) | `.csproj` files use the lowercase path `dotnet\...` while the folder is `DotNet/` — relies on Windows case-insensitive paths. |
| `COM.Net/SharpInterop.sln` | C# .NET Standard 2.0 (SDK-style) | `dotnet build` (cross-platform) | Only solution that builds with the modern `dotnet` CLI. |
| `Java/` | Java sources of j-Interop 2.01 / 2.08 | — | Reference source for the SharpInterop C# port. Do not modify unless intentionally re-syncing. |

All four solutions output to a shared **`BuildOutput/`** folder at the repo root (gitignored). C++ samples land in `BuildOutput\bin\servers\<Platform>\<Configuration>\`; .NET 4.x assemblies in `BuildOutput\bin\Net40\<Platform>\<Configuration>\`.

### Registering the native COM servers
After building `OPC COM.sln`, run from the repo root to (un)register all built `.exe` servers (elevates via UAC):

```cmd
regserver.cmd            :: registers Release Win32 servers
regserver.cmd --debug    :: registers Debug Win32 servers
unregserver.cmd
```

Servers will not start until the OPC Foundation proxy/stub DLLs from `External/` (`OPC Core Components Redistributable …msi` or the `…ProxyStub MergeModule.msm`) are installed on the machine.

### Running a single SharpInterop test
The `SharpInterop.Test` project is **not** an xUnit/NUnit/MSTest project — each file under `Tests/` is a stand-alone sample driver with a `public static void RunTest(string[] args)` method that requires live DCOM credentials (`address domain username password`). `Driver.cs` is explicitly `<Compile Remove>`d. To exercise one, wire it from a small `Program.Main` or call e.g. `OPC.RunTest(args)` directly — `dotnet test` will not discover anything.

## Architecture: the big picture

### `COM/` — native OPC servers and shared libs
- `COM/Include/` — OPC Foundation interface headers + IDL (`opcda.idl`, `opcae.idl`, `opchda.idl`, `OpcEnum.idl`, …). These are the canonical interface definitions referenced by every native project.
- `COM/Shared/Utils/` (`OpcUtils.vcxproj`) — **static library** of generic COM helpers (`COpcComObject`, `COpcClassFactory`, `COpcConnectionPoint`, `COpcVariant`, `COpcXml*`, …) used by all sample servers. Headers like `COpcComObject.h` define the base templates every server class inherits from.
- `COM/Sample Server/Da/Core/` (`OpcDaServerCore.vcxproj`) — **static library** with the generic OPC DA server engine (`COpcDaServer`, `COpcDaGroup`, `COpcDaCache`, `COpcDaUpdateThread`, …). It defines `IOpcDaDevice` as the interface that concrete servers implement.
- `COM/Sample Server/Da/Device/`, `Server/`, `Server (2.05a Only)/`, `Server (3.00 Only)/`, `Wrapper/` — concrete EXEs that link `OpcDaServerCore` + `OpcUtils`. The “2.05a” and “3.00” variants demonstrate version-specific surface area; `Wrapper/` wraps an existing in-proc server as an out-of-proc one.
- `COM/Sample Server/Ae/`, `COM/Sample Server/Hda/` — equivalent layered structure for Alarms & Events and Historical Data Access.

Native servers register via the OPC Foundation macros in `OpcDefs.h`:
`OPC_DECLARE_APPLICATION(...)` / `OPC_BEGIN_CLASS_TABLE() ... OPC_END_CLASS_TABLE()` / `OPC_BEGIN_CATEGORY_TABLE()`. Don't hand-roll DllRegisterServer plumbing — extend the tables.

### `DotNet/` — layered .NET Framework 4.6.2 OPC API
The four assemblies form a strict dependency chain — when adding code, pick the right layer:

1. **`OpcComRcw`** (`DotNet/Rcw/`, namespace `OpcRcw`) — raw runtime-callable wrapper interfaces for the OPC IDLs (`DataAccess.cs`, `AlarmsAndEvents.cs`, `HistoricalDataAccess.cs`, `Commands.cs`, `Batch.cs`, `DataExchange.cs`, `Security.cs`). **No higher-level logic.**
2. **`OpcNetApi`** (`DotNet/Api/`, namespace `Opc`) — high-level managed API. Subfolders `Ae/`, `Cpx/`, `Da/`, `Dx/`, `Hda/` mirror the OPC specs. `Opc.Server`, `Opc.Factory`, `Opc.IServer` are the entry points consumers use.
3. **`OpcNetApi.Com`** (`DotNet/Ccw/`, namespace `OpcCom`) — bridges `Opc.*` to `OpcRcw.*` (`OpcCom.Da.Server`, `OpcCom.Ae.Server`, `OpcCom.Factory`, `OpcCom.Interop`). Also contains `Wrapper/` for exposing managed servers via COM.
4. **`OpcNetApi.Xml`** (`DotNet/Xml/`, namespace `OpcXml`) — XML-DA bindings.

Shared version constants live in **`DotNet/AssemblyVersionInfo.cs`** and are linked into every csproj as `<Compile Include="..\AssemblyVersionInfo.cs" />`. A second copy at `COM/Include/AssemblyVersionInfo.cs` is for native projects — keep `CurrentVersion` / `CurrentFileVersion` in sync when bumping versions.

The csproj files declare `<SignAssembly>true</SignAssembly>` but ship **no `.snk`** — either supply one via `AssemblyOriginatorKeyFile` or disable signing before building.

### `COM.Net/SharpInterop/` — pure-managed DCOM client
A C# port of Java's [j-Interop](https://j-interop.org/) (the source it was ported from is preserved under `Java/j-interop/` and `Java/2.08/`). Namespaces mirror the original Java packages:

- `SharpInterop.Core` — `Session`, `ComServer`, `ComObjectImpl`, `CallBuilder`, `InterfacePointer`, OXID resolver, remote activation. **Start here** when wiring a DCOM call: create a `Session`, build a `ComServer` with `ProgId.ValueOf(...)`, then `CreateInstance()` / `QueryInterface()`.
- `SharpInterop.Common` — `Interop` runtime config flags, `InteropException`, `IAuthInfo`.
- `SharpInterop.Automation` — IDispatch / type-library marshalling (`IDispatch`, `ITypeInfo`, `DispatchImpl`, `Variant`-related types overlap with Core).
- `SharpInterop.Transport` — `ComTransport`, `ComEndpoint`, `ComRuntimeNTLMConnectionContext` (the COM layer above generic RPC).
- `SharpInterop.Rpc` — MSRPC stack (`Stub`, `ConnectionOrientedEndpoint`, `pdu/`, `ncacn_np/`, `Auth/`). Pure protocol — no COM knowledge.
- `SharpInterop.Registry` — remote registry over SMB (used for ProgID→CLSID resolution).
- `SharpInterop.Ndr` — NDR encoder/decoder.
- `SharpInterop.Extensions` — `RectangularArrays`, `StringHelperClass`, `DictionaryEx`, `Utils` — many of these are scaffolding emitted by the Java-to-C# converter; prefer real BCL types in new code.

Dependencies are minimal and significant: **`SharpCifs.Std`** (SMB), **`Portable.BouncyCastle`** (crypto), **`Serilog`** (logging). The static `progIdVsClsidDB.properties` (shipped in both `Java/.../` and `COM.Net/SharpInterop/`) is a fallback ProgID→CLSID map used when remote registry lookup is unavailable.

## Conventions

- **C# style is enforced by `COM.Net/.editorconfig`** for the SharpInterop project: 4-space indent, **CRLF**, `csharp_new_line_before_open_brace = none` (opening brace on the same line — note this differs from the default Visual Studio behavior on many templates), expression-bodied members preferred, `var` preferred when type is apparent, `using`s **inside** the namespace block (see any file under `SharpInterop/`). The `.NET Framework` projects under `DotNet/` predate this and don't follow it — match the surrounding file's style there.
- **License headers**:
  - `COM/` files carry the OPC Foundation sample-code disclaimer (“© Copyright 2002-… The OPC Foundation … provided as-is …”). Keep it on any new file in that tree.
  - `COM.Net/SharpInterop/**` files carry the EPL v1.0 header attributing **Vikram Roopchand 2013** (the original j-Interop author). Preserve attribution on derived files.
  - `Java/` is LGPL (`lgpl.txt`); `Java/j-interopdeps/` carries the Guile license — don't relicense.
- **C++ projects**: Unicode character set, `PlatformToolset = v141`, `WindowsTargetPlatformVersion = 8.1`. Static-lib outputs go to `BuildOutput\lib\...`; EXEs to `BuildOutput\bin\servers\...`. Add new headers under `COM/Shared/Utils/` if they're generic, otherwise next to the server that uses them.
- **.NET 4.6.2 csproj** files in `DotNet/` are legacy-format (`<Project ToolsVersion="12.0">`) — when adding a `.cs` file, edit the csproj to add a `<Compile Include="…" />` entry; it will not auto-include.
- **Solution paths are case-sensitive on non-Windows**: `OPC DotNet.sln` references `dotnet\...` (lowercase) while the folder is `DotNet/`. Don't try to build that solution on Linux/macOS — only `COM.Net/SharpInterop.sln` is portable.
- **`External/`** ships binary redistributables (MSIs and MergeModules) used at install time on customer machines; it is **not** a build dependency for the source tree.
