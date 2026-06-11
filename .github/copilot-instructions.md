# Repository: opc-classic

Opc.Classic is a cross-platform, **NativeAOT-compatible .NET 10** implementation of OPC Classic. It covers DA, AE, HDA, Batch, Commands, Complex Data, DX, Security, and Discovery, with XML-DA support over HTTP. The runtime uses managed DCOM/MSRPC, source-generated client proxies and server dispatchers, and self-contained NTLMv2/Kerberos/SPNEGO authentication with channel binding support.

## Project layout

```
opc-classic/
├── src/                          production source; AOT and trim compatible
│   ├── Directory.Build.props     strict net10, analyzer, package, AOT, and trim setup
│   ├── Directory.Packages.props  central package version management
│   ├── BannedSymbols.txt         banned APIs for runtime reflection, dynamic code, and Windows COM interop
│   ├── Opc.Classic.Dcom/         managed MSRPC/DCOM stack, activation, auth, OBJREF/ORPC, and transports
│   ├── Opc.Classic.Generators/   source generators for OPC interfaces, proxies, dispatchers, and diagnostics
│   └── Opc.Classic.*             per-spec runtime assemblies
├── tests/                        TUnit projects on Microsoft.Testing.Platform
├── samples/                      10 sample apps: DA/AE/HDA servers + clients, LoopbackDemo, CttServer (additional managed DA sample), OpcSecurityServer, AotCanary
├── docs/                         plain Markdown documentation hub and topic pages
├── interop/                     OPC Foundation IDL, redistributables (`external`), and native C++ samples/test apps; spec reference markdown lives in the private `marcschier/opc-classic-docs` repo
│   └── docker/                   Windows-container managed/native interop test fleet
├── .github/workflows/            build, release, and Docker test fleet workflows
├── Opc.Classic.slnx              .NET 10 XML solution format
├── global.json                   pins .NET 10 SDK >= 10.0.100
├── LICENSE                       MIT license
└── README.md
```

`interop/` retains its upstream notices and is used for conformance validation. Project source is MIT licensed.

## Build / test / run

Requires **.NET 10 SDK** (10.0.100 or later). `global.json` rolls forward to compatible .NET 10 feature SDKs.

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

Run a targeted TUnit project:

```powershell
dotnet run --project tests\Opc.Classic.Dcom.Crypto.Tests --no-build
```

Publish the NativeAOT canary:

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

The expected baseline is 0 build warnings and 0 build errors. The current validation sweep has all 25 .NET test projects green (DA 475, AE 128, HDA 177, DCOM 123, Crypto 65, SMB 61, Integration 109, plus the remaining suites).

## NativeAOT requirements

Every runtime source project inherits:

```xml
<IsAotCompatible>true</IsAotCompatible>
<IsTrimmable>true</IsTrimmable>
<EnableTrimAnalyzer>true</EnableTrimAnalyzer>
<EnableAotAnalyzer>true</EnableAotAnalyzer>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

`BannedSymbols` rejects runtime patterns that would break AOT or cross-platform behavior:

- `System.Reflection.Emit.*`
- `Expression<T>.Compile()`
- `MethodInfo.Invoke`, `MethodBase.Invoke`
- `Activator.CreateInstance(Type)`, `Activator.CreateInstance<T>()` without proper annotations
- `Type.GetType(string)`
- `[ComImport]`
- Windows COM runtime helpers such as `Marshal.GetObjectForNativeVariant`, `GetNativeVariantForObject`, and `ReleaseComObject`

Use source generation for static dispatch tables, proxy methods, and server dispatchers.

## Conventions

- **C# style**: file-scoped namespaces, usings outside namespace declarations, `_camelCase` private fields, predefined C# type aliases, and no broad analyzer suppressions.
- **IDL names**: OPC/MS-DCOM wire identifiers keep their original casing, underscores, and reserved-word shapes where needed for spec readability.
- **License headers**: every new `src/` file carries `SPDX-License-Identifier: MIT` and the repository copyright header.
- **Crypto**: MD4 and RC4 live in `Crypto`; MD5, HMAC, DES, and AES primitives come from the BCL where available. Do not add new crypto dependencies without a security review.
- **Tests**: use TUnit, `[Test]`, `[Arguments]`, and `await Assert.That(actual).IsEqualTo(expected)`. Prefer hand-written test doubles over runtime-proxy mocking frameworks.
- **Solutions**: `Opc.Classic.slnx` is the only root solution and uses the .NET XML solution format.
- **Packages**: central package versions live in `Directory.Packages` and Directory.Packages tests.
- **Conformance**: All code must conform strictly to the vendored OPC/MS specifications.

## Quick task pointers

- **Documentation hub**: `docs\README.md`; forward-looking work belongs in `docs\ROADMAP.md`.
- **Architecture overview**: `docs\ARCHITECTURE.md`.
- **Generator diagnostics**: `docs\generators\diagnostics.md` and `Opc.Classic`.
- **Migration diagnostics**: `docs\migration\` and `Opc.Classic`.
- **NTLM/Kerberos/SPNEGO auth**: `Auth`, `Opc.Classic.Dcom`, and `Spnego`.
- **DCOM activation**: `Activation` and `RemActivation`.
- **Server dispatch path**: generated dispatchers plus `ComRuntimeEndpoint` and ComOxidRuntime*.
- **Discovery**: `OpcEnumClient` and `OpcEnumDcomInterfaces`.
- **Samples**: Opc.Classic.Samples sample.
