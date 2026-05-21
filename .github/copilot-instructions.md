# Repository: opc-classic

A cross-platform, **NativeAOT-compatible .NET 10** implementation of OPC Classic (DA / AE / HDA / DX / Cpx / Batch / Commands / Security / XML-DA) with both **client** and **server** hosting, secured by **NTLMv2 + Kerberos/SPNEGO** (Kerberos arrives in Phase 3D). The project is mid-refactor — see the implementation plan in `~/.copilot/session-state/<session>/plan.md` for the active roadmap and `todos` SQL table for live status.

## Project layout

```
opc-classic/
├── src/                          production source — all `IsAotCompatible=true`
│   ├── Directory.Build.props     strict net10 + AOT + analyzer setup
│   ├── Directory.Packages.props  central package version management
│   ├── BannedSymbols.txt         banned API list (Reflection.Emit, Expression.Compile,
│                                  MethodInfo.Invoke, Type.GetType(string),
│                                  Activator.CreateInstance(Type), [ComImport],
│                                  Marshal.GetObjectForNativeVariant/ReleaseComObject, ...)
│   └── OpcClassic.Dcom/          cross-platform pure-managed MSRPC/DCOM stack
│                                  (ex-SharpInterop; legacy j-Interop port being modernized).
│                                  Currently TRANSITIONAL — see csproj header for relaxations.
│       └── Crypto/               hand-rolled MD4 + RC4 + BC-API-shaped compat shim
├── tests/
│   ├── Directory.Build.props     TUnit on Microsoft.Testing.Platform (NOT VSTest);
│                                  IsTestProject default-true (opt out with false).
│                                  Tests are NOT AOT-strict (libs_only scope).
│   ├── Directory.Packages.props  TUnit 0.13.x, CsCheck, Verify.TUnit, MS Logging.Testing
│   ├── OpcClassic.Dcom.Crypto.Tests/   ← first real TUnit project (Phase 11A complete)
│   │                                     RFC 1320 MD4 + RFC 6229 RC4 vectors, all passing
│   └── OpcClassic.Dcom.Tests/    legacy SharpInterop integration drivers (Phase 11 rewrites)
│                                  opt out of TUnit via <IsTestProject>false</IsTestProject>.
├── samples/                      sample apps and the AOT canary (Phase 16D)
├── docs/                         DocFX site + cookbook (Phase 15)
├── COM/                          PRESERVED  native C++ OPC sample servers — conformance reference
├── External/                     PRESERVED  OPC Foundation MSIs + proxy-stub merge modules
├── .github/workflows/build.yml   Linux/macOS/Windows matrix + AOT-canary gate +
│                                  Windows conformance job that builds COM/ samples
├── OpcClassic.slnx               (.NET 10 XML solution format — NOT a legacy .sln)
├── global.json                   pins .NET 10 SDK >= 10.0.100
├── LICENSE                       EPL-1.0 stub (final disposition in Phase 1E)
└── README.md
```

The legacy `DotNet/`, `Java/`, `COM.Net/`, and the three old `.sln` files were removed in Phase 1A. The `COM/` and `External/` trees are intentionally preserved as the OPC Foundation conformance reference used by the Windows CI job.

## Build / test / run

Requires **.NET 10 SDK** (10.0.100 or later). `global.json` rolls forward to the highest installed `10.0.x`.

```powershell
dotnet restore OpcClassic.slnx
dotnet build OpcClassic.slnx
dotnet run --project tests\OpcClassic.Dcom.Crypto.Tests --no-build
# Single test: TUnit's MTP runner supports filtering by name
dotnet run --project tests\OpcClassic.Dcom.Crypto.Tests --no-build -- --treenode-filter "/*/*/Md4Tests/HashData_MatchesRfc1320Vector"
```

`dotnet test OpcClassic.slnx` also works, but TUnit projects emit an `Exe` and prefer `dotnet run` — the runner is Microsoft.Testing.Platform, not VSTest. Conformance and integration tests against native COM/ servers run only on the Windows CI runner (`OPC COM.sln` builds with VS 2017 build tools; `regserver.cmd` registers the EXEs into the system COM catalog).

To verify NativeAOT-cleanliness of the libraries:

```powershell
dotnet publish samples\OpcClassic.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

Zero `IL2xxx`/`IL3xxx` warnings is the contract. Any warning fails CI. (The canary sample doesn't exist yet — added in Phase 16D.)

## NativeAOT requirements (cross-cutting, libs_only scope)

Every `src/*` assembly has:

```xml
<IsAotCompatible>true</IsAotCompatible>
<IsTrimmable>true</IsTrimmable>
<EnableTrimAnalyzer>true</EnableTrimAnalyzer>
<EnableAotAnalyzer>true</EnableAotAnalyzer>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

inherited from `src/Directory.Build.props`. `src/BannedSymbols.txt` rejects, with build errors:

- `System.Reflection.Emit.*` (DynamicMethod, AssemblyBuilder, …)
- `Expression<T>.Compile()`
- `MethodInfo.Invoke`, `MethodBase.Invoke`
- `Activator.CreateInstance(Type)`, `Activator.CreateInstance<T>()` (without `[DynamicallyAccessedMembers]`)
- `Type.GetType(string)` (runtime string-to-type)
- `[ComImport]` (irrelevant cross-platform; meaningless without Windows COM runtime)
- `Marshal.GetObjectForNativeVariant`, `GetNativeVariantForObject`, `ReleaseComObject`

When you need code that *looks* like one of those patterns — e.g., "given a type, dispatch to its handler" — emit it from a source generator instead. The plan introduces `src/OpcClassic.Generators` for exactly this in Phase 4A.

**Tests and samples are NOT AOT-strict** (per the `libs_only` decision) — they may use Verify, CsCheck, etc. without breaking the rule.

The `src/OpcClassic.Dcom/OpcClassic.Dcom.csproj` is **TRANSITIONAL**: it currently overrides the strict properties to `false` because the legacy SharpInterop code under it is Java-converter style and is being modernized phase by phase. The csproj header documents which NoWarn entries are temporary and which Phase removes them. **Do not relax the rules for fresh code** — any new file should compile clean under the parent `src/Directory.Build.props` defaults.

## Conventions

- **C# style** (root `.editorconfig`): file-scoped namespaces (`namespace X;` not `namespace X { … }`), usings outside the namespace block, `_camelCase` for private fields, no `this.` qualification, predefined `int`/`string` over BCL aliases. AOT analyzer codes (IL2xxx/IL3xxx) and `CA1062`/`CA2007`/`CA1031`/`VSTHRD002`/`VSTHRD003`/`VSTHRD100`/`VSTHRD110` are errors in `src/`. The same rules are relaxed inside legacy folders (`COM/`, anywhere under the transitional OpcClassic.Dcom that opts out).
- **License headers**: every file in `src/*` carries `SPDX-License-Identifier: EPL-1.0`. The transitional SharpInterop-derived files additionally credit Vikram Roopchand 2013 (preserved). Files under `COM/` and `External/` carry the OPC Foundation sample-code disclaimer verbatim — don't strip it.
- **Crypto**: zero crypto NuGet dependencies. MD4 + RC4 are hand-rolled in `src/OpcClassic.Dcom/Crypto/`; MD5 + HMAC-MD5 + DES come from BCL via `Crypto/BcCompat.cs` (the transitional shim). New code goes against the hand-rolled / BCL primitives directly; the BcCompat shim exists only to bridge the legacy NTLM code through Phase 2.
- **Tests** use TUnit + `Assert.That(actual).IsEqualTo(expected)` async fluent assertions. `[Arguments]` for parameterized cases. Don't add NSubstitute / Moq / FakeItEasy — they use runtime IL emit. Source-generator mocking (or hand-written test doubles) only.
- **Solutions**: `OpcClassic.slnx` is the **.NET 10 XML solution** format, not the legacy `.sln`. `dotnet sln add/remove` modifies it. There is exactly one solution at the repo root.
- **NuGet**: central package versions live in `src/Directory.Packages.props` and `tests/Directory.Packages.props`. Per-project `<PackageReference Include="X" />` carries no `Version`. Two transitional packages remain in src (`Serilog`, `SharpCifs.Std`) — both scheduled for removal in Phase 2G / 2D respectively.

## Quick task pointers

- **Where does NTLM auth live?** `src/OpcClassic.Dcom/rpc/Auth/` — NTLMv1 + v2 + Session Security with hand-rolled crypto. SSO returns in Phase 3D via Kerberos.
- **Where does DCOM activation happen?** `src/OpcClassic.Dcom/Core/RemActivation.cs` (v5.4) and `RemoteSCMActivator.cs` (v5.6). Default activation flips to v5.6 in Phase 3A.
- **Where is the server-side callback path?** `src/OpcClassic.Dcom/Core/LocalCoClass.cs` + `ComOxidRuntime.cs` + `ComOxidRuntimeHelper.cs` + `Transport/ComRuntimeEndpoint.cs`. Reflection-based dispatch today; replaced by source-generated dispatch in Phase 4A.
- **Where are RC4/MD4?** `src/OpcClassic.Dcom/Crypto/Md4.cs`, `Rc4.cs`. `BcCompat.cs` is the BouncyCastle-API-shaped wrapper used by legacy callers until Phase 2C rewrites them.
- **How do I write a new test?** Add a project under `tests/`, file-scoped namespace, `using TUnit.Core;`. The Directory.Build.props auto-wires TUnit + coverlet. Use `[Test]` + `[Arguments]` + `await Assert.That(...).IsEqualTo(...)`.
- **Why does my `using ole32.dll`/`[ComImport]`/`MethodInfo.Invoke` not compile?** It's banned by `src/BannedSymbols.txt`. Use the cross-platform alternative or a source generator. The plan documents the replacement for each banned pattern.

## Status

Phase 1 (Foundations) and Phase 2E/2F (hand-rolled crypto + SSPI removal) are **done**. Phase 2A/2B (de-Javaify + idiomatic .NET) are **in progress**. The rest is in `~/.copilot/session-state/<session>/plan.md`. Query the `todos` SQL table for live status.
