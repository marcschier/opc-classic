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
├── samples/                      9 sample apps: 3 servers, 3 clients, LoopbackDemo, CttServer, AotCanary
├── docs/                         plain Markdown documentation hub and topic pages
├── COM/                          OPC Foundation native C++ sample servers used as conformance references
├── External/                     OPC Foundation IDL, headers, redistributables, and spec assets
├── .github/workflows/            build, CTT, release, and conformance workflows
├── Opc.Classic.slnx              .NET 10 XML solution format
├── global.json                   pins .NET 10 SDK >= 10.0.100
├── LICENSE                       MIT license
└── README.md
```

`COM/` and `External/` retain their upstream notices and are used for conformance validation. Project source is MIT licensed.

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

The expected baseline is 0 build warnings and 0 build errors. Tests currently pass with 1253 passed / 24 skipped / 0 failed.

## NativeAOT requirements

Every runtime source project inherits:

```xml
<IsAotCompatible>true</IsAotCompatible>
<IsTrimmable>true</IsTrimmable>
<EnableTrimAnalyzer>true</EnableTrimAnalyzer>
<EnableAotAnalyzer>true</EnableAotAnalyzer>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

`src/BannedSymbols.txt` rejects runtime patterns that would break AOT or cross-platform behavior:

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
- **Crypto**: MD4 and RC4 live in `src\Opc.Classic.Dcom\Crypto\`; MD5, HMAC, DES, and AES primitives come from the BCL where available. Do not add new crypto dependencies without a security review.
- **Tests**: use TUnit, `[Test]`, `[Arguments]`, and `await Assert.That(actual).IsEqualTo(expected)`. Prefer hand-written test doubles over runtime-proxy mocking frameworks.
- **Solutions**: `Opc.Classic.slnx` is the only root solution and uses the .NET XML solution format.
- **Packages**: central package versions live in `src\Directory.Packages.props` and `tests\Directory.Packages.props`.

## Quick task pointers

- **Documentation hub**: `docs\README.md`; forward-looking work belongs in `docs\ROADMAP.md`.
- **Architecture overview**: `docs\ARCHITECTURE.md`.
- **Generator diagnostics**: `docs\generators\diagnostics.md` and `src\Opc.Classic.Generators\`.
- **Migration diagnostics**: `docs\migration\` and `src\Opc.Classic.MigrationAnalyzer\`.
- **NTLM/Kerberos/SPNEGO auth**: `src\Opc.Classic.Dcom\rpc\Auth\`, `src\Opc.Classic.Dcom.Kerberos\`, and `src\Opc.Classic.Dcom\Spnego\`.
- **DCOM activation**: `src\Opc.Classic.Dcom\Activation\` and `src\Opc.Classic.Dcom\Core\RemActivation.cs`.
- **Server dispatch path**: generated dispatchers plus `src\Opc.Classic.Dcom\Transport\ComRuntimeEndpoint.cs` and `src\Opc.Classic.Dcom\Core\ComOxidRuntime*.cs`.
- **Discovery**: `src\Opc.Classic.Discovery\OpcEnumClient.cs` and `src\Opc.Classic.Discovery\OpcEnumDcomInterfaces.cs`.
- **Samples**: `samples\Opc.Classic.Samples.*`.
