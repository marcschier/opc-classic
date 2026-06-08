# Test projects and categories

The repository uses TUnit on Microsoft.Testing.Platform. The rc.10 baseline is 2113 passed / 12 skipped / 0 failed across 23 .NET test projects. Run the full solution with:

```powershell
dotnet test Opc.Classic.slnx
```

The default local filter for fast, environment-independent coverage is:

```powershell
dotnet test Opc.Classic.slnx --filter "Category!=NativeConformance&Category!=MatrikonConformance&Category!=CompatMatrix&Category!=Kerberos"
```

## Top-level test directories

Support folders:

- `_Fixtures` — shared fixture infrastructure; `Testcontainers\` contains `KerberosKdcFixture`.
- `_TestDoubles` — policy docs for shared hand-written doubles.

TUnit projects (23):

- `Opc.Classic.Ae.Tests`
- `Opc.Classic.Batch.Tests`
- `Opc.Classic.Commands.Tests`
- `Opc.Classic.Core.Tests`
- `Opc.Classic.Cpx.Tests`
- `Opc.Classic.Da.Tests`
- `Opc.Classic.Dcom.Crypto.Tests`
- `Opc.Classic.Dcom.Kerberos.Tests`
- `Opc.Classic.Dcom.Logging.Tests`
- `Opc.Classic.Dcom.Smb.Tests`
- `Opc.Classic.Dcom.Tests`
- `Opc.Classic.Discovery.Tests`
- `Opc.Classic.Dx.Tests`
- `Opc.Classic.Generators.Tests`
- `Opc.Classic.Hda.Tests`
- `Opc.Classic.Hosting.Tests`
- `Opc.Classic.Integration.Tests`
- `Opc.Classic.Mcp.Tests`
- `Opc.Classic.MigrationAnalyzer.Tests`
- `Opc.Classic.PropertyTests`
- `Opc.Classic.Security.Tests`
- `Opc.Classic.SnapshotTests`
- `Opc.Classic.Xml.Tests`

This list matches the current `tests\` top-level project directories.

## Environment-dependent categories

- `Kerberos` — Docker/Testcontainers-backed MIT krb5 KDC integration tests. They are skipped unless Docker is running and `OPC_CLASSIC_RUN_KDC_TESTS=1` is set.
- `NativeConformance` — tests that need the vendored OPC Foundation native sample servers built/registered.
- `MatrikonConformance` — tests that need Matrikon OPC Simulation Server installed/registered.
- `CompatMatrix` — compatibility-matrix and native-client/server orchestration tests.
- `WinRegSambaSmoke` — opt-in Samba WINREG smoke; requires `external\docker\samba` and `OPC_CLASSIC_INTEGRATION_SAMBA=1`.
- `Da.FullLifecycle` / `Da.Loopback` — loopback TCP DA integration coverage for object-IPID group dispatch, item attributes, callbacks, and namespaced browse continuation tokens (`opc-da-browse:N`).

Folder-specific details live under `tests\Opc.Classic.Integration.Tests\Native\README.md`, `tests\Opc.Classic.Integration.Tests\Matrikon\README.md`, and `tests\Opc.Classic.Integration.Tests\CompatMatrix\README.md`.
