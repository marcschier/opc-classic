# Contributing

Thank you for helping improve opc-classic. This repository is an early-stage, NativeAOT-compatible .NET 10 implementation of OPC Classic, with a pure-managed DCOM/MSRPC stack and protocol-specific assemblies for DA, AE, HDA, DX, Cpx, Batch, Commands, Security, and XML-DA.

Keep contributions small, focused, and aligned with the active implementation plan.

## Quick Start

Install the .NET 10 SDK. The repository root `global.json` currently pins SDK `10.0.100` with `rollForward` set to `latestFeature`, so any compatible later .NET 10 feature SDK may be used.

```powershell
git clone https://github.com/marcschier/opc-classic.git
cd opc-classic
dotnet restore OpcClassic.slnx
dotnet build OpcClassic.slnx
dotnet test OpcClassic.slnx
```

`OpcClassic.slnx` is the .NET 10 XML solution format, not a legacy `.sln` file.

## Project Layout

The current repository layout is intentionally split between migrated source, tests, documentation, and preserved conformance assets.

| Path | Purpose |
| --- | --- |
| `src\` | Production libraries and tools. `src\Directory.Build.props` applies strict .NET 10, nullable, analyzer, and NativeAOT settings to source projects. |
| `tests\` | TUnit and Microsoft.Testing.Platform test projects. The solution currently lists test projects under `tests\`. |
| `docs\` | Documentation, architecture notes, and cookbook material for Phase 15. |
| `External\` | Preserved OPC Foundation redistributables and headers used as conformance inputs. Do not casually rewrite or relicense them. |
| `COM\` | Preserved native C++ OPC sample servers used as conformance references, especially for Windows CI. |
| `samples\` | Sample applications and the planned AOT canary. |

The old legacy trees are being migrated into this structure phase by phase. Do not reintroduce legacy solution files or Windows-only COM runtime dependencies for new cross-platform code.

## Code Style

Every new source file must carry the repository SPDX and copyright header:

```csharp
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
```

Follow the style enforced by `.editorconfig` and `src\Directory.Build.props`:

- Use file-scoped namespaces.
- Keep nullable reference types enabled; `src\Directory.Build.props` sets `<Nullable>enable</Nullable>`.
- Use `LangVersion` `latest` as inherited from `src\Directory.Build.props`.
- Prefer explicit, AOT-safe code over reflection or dynamic dispatch.
- Keep one type per file. MA0048 enforces this; suppress at file scope only for trivial sibling stubs that are clearer together.
- Satisfy CA1062 by validating public arguments with `ArgumentNullException.ThrowIfNull`.
- Do not add broad analyzer suppressions to make a warning disappear. Fix the warning or document a narrow, local suppression.

Fresh code must compile cleanly under the strict source-project defaults. Transitional code in `src\OpcClassic.Dcom\` may still carry temporary relaxations, but new code should not copy those relaxations.

## Test Conventions

Tests use TUnit on Microsoft.Testing.Platform. Prefer the async fluent assertion style:

```csharp
await Assert.That(actual).IsEqualTo(expected);
```

Use `[Test]` and `[Arguments]` for parameterized cases. See the mocking policy below before adding any test-double dependency.

### Mocking and Test Doubles

Do not add runtime-proxy mocking frameworks such as NSubstitute, Moq, or FakeItEasy; they rely on Castle.Core, Reflection.Emit, DispatchProxy, or similar dynamic code paths that conflict with the NativeAOT direction. Shared hand-written doubles live under `tests\_TestDoubles\` and follow the conventions in `tests\_TestDoubles\Testing.md`.

Use `FakeXxx` for reusable configurable doubles, `StubXxx` for narrow scaffolds, and `CapturingXxx` for doubles that record calls or state. Keep shared doubles per-interface, small, and free of production logic. For `ILogger` assertions, prefer the Microsoft fake logger APIs provided by `Microsoft.Extensions.Diagnostics.Testing` (`Microsoft.Extensions.Logging.Testing` namespace).

For NDR tests, use the snapshot-first pattern. `NdrReader` and `NdrWriter` are `ref struct` types and cannot cross `await` boundaries. Capture all values into ordinary locals inside a synchronous block, then await assertions against those locals.

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

Keep tests deterministic. When a vulnerability report includes vectors, add them under the relevant test project in `tests\`.

## Coverage

CI gates code coverage at **70% line / 60% branch** (initial thresholds; targeting 80%/70% by 1.0). Transitional code in `src\OpcClassic.Dcom\*` and source generators (`src\OpcClassic.Generators\`) are excluded from gating.

Run coverage locally for a targeted project:

```powershell
dotnet test tests\OpcClassic.Core.Tests\OpcClassic.Core.Tests.csproj --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory .\coverage-results\OpcClassic.Core.Tests
```

Generate a human-readable report:

```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:.\coverage-results\**\coverage.cobertura.xml -targetdir:.\coverage-report -reporttypes:Html
```

## Building the documentation site

Install DocFX:

```bash
dotnet tool install -g docfx
```

Build + serve locally:

```bash
docfx docfx.json --serve
```

Open http://localhost:8080

## Build Quality Gates

Source projects must maintain zero build errors and zero build warnings. `src\Directory.Build.props` sets `TreatWarningsAsErrors=true`, enables latest recommended analysis, and marks source projects as NativeAOT-compatible.

Every `src\` project is expected to keep:

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

If a feature needs runtime dispatch, prefer a source generator in `src\OpcClassic.Generators\`.

## PR Process

Before opening a pull request:

1. Keep the change small and focused.
2. Avoid unrelated cleanup, formatting churn, or drive-by refactors.
3. Use clear phase numbering when relevant, from Phase 1A through Phase 16E per the implementation plan.
4. Run `dotnet build OpcClassic.slnx` from the repository root.
5. Run targeted tests for the area you changed, and run `dotnet test OpcClassic.slnx` when practical.
6. Explain behavior changes, compatibility concerns, and any skipped tests in the PR description.

Use clear commit messages. Do not use PowerShell here-strings for commit messages in this repository. PowerShell 7+ can treat backtick-u sequences as Unicode escapes and break messages containing source snippets such as `using X;` lines. Write commit messages to a temporary file and commit with `git commit -F`, then remove the temporary file.

## Implementation Plan

The multi-phase roadmap lives outside the repository in the session workspace at `~\.copilot\session-state\<session-id>\plan.md`. That plan is the source of truth for sequencing and phase boundaries.

Phase names run from 1A through 16E. When a contribution completes or advances a planned phase, mention the phase in the commit message and PR description.

## Windows conformance jobs (Phase 14A–14D)

The CI matrix includes a `windows-conformance` job that:

1. Installs the OPC Foundation Core Components (DCOM proxy/stub registrations)
2. Builds the native C++ OPC sample servers under `COM/`
3. Registers them via `regserver.cmd`
4. Runs the managed test suite and the `Category=NativeConformance` subset against the native servers

The Phase 14C Matrikon Simulation Server test runs separately via the
`matrikon-conformance` job (Phase 14C-followup; not yet added — depends on
a workflow secret carrying the Matrikon installer URL).

The job is currently `continue-on-error` for the native-conformance step
because the native build chain is platform-specific and may not be available
on every runner image. Until Phase 14B test files land, this is a green-by-default
job that lights up automatically once the conformance tests are added.

## OPC CTT conformance (Phase 14E)

The separate `.github\workflows\opc-ctt.yml` workflow scaffolds official OPC
Foundation Compliance Test Tool runs for managed server conformance. It is
gated on OPC Foundation membership and the `OPC_CTT_INSTALLER_URL` secret; forks
are skipped and an unset secret no-ops. See `docs\OPC_CTT_CONFORMANCE.md` for
prerequisites, triggers, result artifacts, and release-gating status.

## License

This project currently uses EPL-1.0 and carries license inheritance from SharpInterop and the j-Interop C# port, with preserved OPC Foundation material under legacy conformance paths.

Every new source file must include:

```csharp
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
```

Do not remove existing third-party notices or OPC Foundation sample-code disclaimers from preserved files under `COM\` or `External\`.

## Snapshot tests (Verify.TUnit)

PDU byte-stream golden files live alongside the test source code under
`tests\<ProjectName>.Tests\Snapshots\<TestClassName>.<TestMethodName>.verified.txt`.

On first run, a test produces a `.received.txt` file — review and rename
to `.verified.txt` to accept the snapshot. Subsequent runs assert equality.
Diffs are visible in the test output and via any diff tool.

To regenerate ALL snapshots: delete the `.verified.*` files and re-run tests;
accept the new `.received.*` files.
