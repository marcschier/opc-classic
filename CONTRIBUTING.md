# Contributing

Thank you for helping improve **Opc.Classic**. This repository is a NativeAOT-compatible .NET 10 implementation of OPC Classic with managed DCOM/MSRPC, XML-DA, source-generated call paths, and protocol-specific assemblies for DA, AE, HDA, DX, Cpx, Batch, Commands, Security, and Discovery.

Keep contributions small, focused, and aligned with the current `Opc.Classic.*` architecture.

## Quick start

Install the .NET 10 SDK. The repository root `global.json` pins SDK `10.0.100` with `rollForward` set to `latestFeature`, so any compatible later .NET 10 feature SDK may be used.

```powershell
git clone https://github.com/marcschier/opc-classic.git
cd opc-classic
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

`Opc.Classic.slnx` is the .NET 10 XML solution format, not a legacy `.sln` file.

## Project layout

| Path | Purpose |
| --- | --- |
| `src\` | Production libraries and generators. `src\Directory.Build.props` applies .NET 10, nullable, analyzer, package, NativeAOT, and trimming settings. |
| `tests\` | TUnit and Microsoft.Testing.Platform test projects, including unit, property, snapshot, generator, logging, conformance, and integration scaffolds. |
| `samples\` | Nine runnable managed samples for DA/AE/HDA clients and servers, loopback, CTT, and AOT publishing. |
| `docs\` | Plain Markdown architecture, adoption, cookbook, tutorials, security, migration, architecture diagrams, conformance, release, and roadmap docs. |
| `external\docker\` | Windows-container test fleet for CTT, managed server, and native C server/client interop. |
| `external\redist\samples\` | OPC Foundation native C++ sample servers and test applications used as conformance references. Do not casually rewrite or relicense them. |
| `external\` | OPC Foundation redistributables, IDL, headers, CTT installers, `external\private\docs`, and native sample assets used as conformance inputs. |

The portable stack must not introduce Windows-only COM runtime dependencies such as `[ComImport]`, RCW activation, or `ole32.dll` P/Invoke.

## Code style

Every new source file in `src\` must carry the repository SPDX and copyright header:

```csharp
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
```

Follow the style enforced by `.editorconfig` and `src\Directory.Build.props`:

- Use file-scoped namespaces.
- Keep nullable reference types enabled.
- Prefer explicit, AOT-safe code over reflection or dynamic dispatch.
- Keep one type per file unless a narrow local suppression is clearer for tiny IDL stubs.
- Validate public arguments with `ArgumentNullException.ThrowIfNull`.
- Do not add broad analyzer suppressions. Fix the warning or document a narrow local suppression.

Fresh source code must compile cleanly under source-project defaults.

## NativeAOT requirements

Runtime source projects are expected to keep:

```xml
<IsAotCompatible>true</IsAotCompatible>
<IsTrimmable>true</IsTrimmable>
<EnableTrimAnalyzer>true</EnableTrimAnalyzer>
<EnableAotAnalyzer>true</EnableAotAnalyzer>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

The Banned API analyzer uses `src\BannedSymbols.txt` to reject NativeAOT-hostile patterns. Do not introduce:

- `System.Reflection.Emit`.
- `Activator.CreateInstance(Type)` or equivalent runtime type activation.
- `Expression.Compile`.
- `dynamic` dispatch for production code.
- `[ComImport]`, `ole32.dll` P/Invoke, or Windows COM runtime dependencies in cross-platform source.
- `MethodInfo.Invoke` or other reflection-dispatch paths that should be generated instead.

If a feature needs runtime-looking dispatch, prefer emitting static code from `src\Opc.Classic.Generators\`.

## Test conventions

Tests use TUnit on Microsoft.Testing.Platform. Prefer the async fluent assertion style:

```csharp
await Assert.That(actual).IsEqualTo(expected);
```

Use `[Test]` and `[Arguments]` for parameterized cases. Keep tests deterministic. When a vulnerability report includes vectors, add them under the relevant test project in `tests\`.

### Mocking and test doubles

Do not add runtime-proxy mocking frameworks such as NSubstitute, Moq, or FakeItEasy; they rely on Castle.Core, Reflection.Emit, DispatchProxy, or similar dynamic code paths that conflict with the NativeAOT direction. Shared hand-written doubles live under `tests\_TestDoubles\` when needed.

Use `FakeXxx` for reusable configurable doubles, `StubXxx` for narrow scaffolds, and `CapturingXxx` for doubles that record calls or state. For `ILogger` assertions, prefer Microsoft.Extensions.Diagnostics.Testing (`Microsoft.Extensions.Logging.Testing` namespace).

### Snapshot-first NDR tests

For NDR tests, snapshot byte arrays, DTOs, or formatted strings rather than `ref struct` values directly. `NdrReader` and `NdrWriter` cannot cross `await` boundaries. Capture values into ordinary locals inside a synchronous block, then await assertions against those locals.

```csharp
int status;
Guid itemId;

{
    var reader = new NdrReader(buffer);
    status = reader.ReadInt32();
    itemId = reader.ReadGuid();
}

await Assert.That(status).IsEqualTo(expectedStatus);
await Assert.That(itemId).IsEqualTo(expectedItemId);
```

Verify.TUnit golden files live alongside tests under `tests\<ProjectName>.Tests\Snapshots\`. Review `.received.*` files before accepting them as `.verified.*` snapshots.

## Coverage

CI gates code coverage at **70% line / 50% branch** (workspace-wide aggregate
across all `tests/**/coverage.cobertura.xml`, after the ReportGenerator
exclusion of `Opc.Classic.Dcom*`/`Opc.Classic.Generators*`/`*Tests*`). The
branch floor was lifted in step with the actual aggregate as Tracks AR + BD
raised coverage on the new sink + listener + element-VARIANT surfaces;
tighten further in proportion to future targeted coverage work. Exclusions
must be narrow and documented.

Run coverage locally for a targeted project:

```powershell
dotnet test tests\Opc.Classic.Core.Tests\Opc.Classic.Core.Tests.csproj --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory .\coverage-results\Opc.Classic.Core.Tests
```

Generate a human-readable report:

```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:.\coverage-results\**\coverage.cobertura.xml -targetdir:.\coverage-report -reporttypes:Html
```

## Sample apps

Use samples to demonstrate public APIs, not test-only shortcuts.

| Sample | Purpose |
| --- | --- |
| `samples\Opc.Classic.Samples.DaServer\` | Managed DA server. |
| `samples\Opc.Classic.Samples.AeServer\` | Managed AE server. |
| `samples\Opc.Classic.Samples.HdaServer\` | Managed HDA server. |
| `samples\Opc.Classic.Samples.DaClient\` | Managed DA client. |
| `samples\Opc.Classic.Samples.AeClient\` | Managed AE client. |
| `samples\Opc.Classic.Samples.HdaClient\` | Managed HDA client. |
| `samples\Opc.Classic.Samples.LoopbackDemo\` | In-process client/server loopback. |
| `samples\Opc.Classic.Samples.CttServer\` | CTT-oriented managed DA server. |
| `samples\Opc.Classic.Samples.OpcSecurityServer\` | Managed OPC Security reference server. |
| `samples\Opc.Classic.Samples.AotCanary\` | NativeAOT publish canary. |

Build or run a sample with the XML solution restored:

```powershell
dotnet run --project samples\Opc.Classic.Samples.CttServer\Opc.Classic.Samples.CttServer.csproj
```

Publish the AOT canary:

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

## Documentation

Documentation is plain Markdown under `docs\`. Start at `docs\README.md` and keep new pages linked from the hub or the relevant section index. Do not add documentation-site generators or generated site output.

## Build quality gates

Current rc.10 validation baseline: **0 build warnings, 0 build errors, 2113 passed / 12 skipped / 0 failed across 23 .NET test projects**.

Before opening a pull request:

1. Keep the change small and focused.
2. Avoid unrelated cleanup, formatting churn, or drive-by refactors.
3. Run `dotnet build Opc.Classic.slnx` from the repository root.
4. Run targeted tests for the area you changed, and run `dotnet test Opc.Classic.slnx` when practical.
5. Explain behavior changes, compatibility concerns, and any skipped tests in the PR description.

Use clear commit messages. Do not use PowerShell here-strings for commit messages in this repository; write commit messages to a temporary file in the repository root and commit with `git commit -F`, then remove the file.

## Windows and OPC conformance jobs

The CI matrix includes Windows conformance coverage that can:

1. install OPC Foundation Core Components,
2. build preserved native C++ OPC sample servers under `external\redist\samples\`,
3. register them via `external\redist\samples\regserver.cmd`,
4. run managed native-conformance subsets against those servers.

The `.github\workflows\opc-ctt.yml` workflow installs the vendored CTT MSIs from `external\private\ctt\`, registers `samples\Opc.Classic.Samples.CttServer`, and uploads `opc-ctt-results`. The `.github\workflows\docker-test-fleet.yml` workflow builds the Windows-container fleet under `external\docker\` and runs the managed CTT smoke when a Windows-container host is available.

## License

This project is licensed under MIT. Preserved OPC Foundation material under `external\` keeps its original notices.

Every new project source file should include:

```csharp
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
```

Do not remove third-party notices, OPC Foundation sample-code disclaimers, or entries from `THIRD-PARTY-NOTICES.md`.
